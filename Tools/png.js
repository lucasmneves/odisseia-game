// Leitura e escrita de PNG sem dependências — só zlib do Node.
// Suporta os formatos que o PixelLab devolve: 8 bits, RGB / RGBA / paletizado (+tRNS), sem entrelaçamento.
// Toda a análise de arte do projeto passa por aqui; não há Python nesta máquina.
const fs = require('fs');
const zlib = require('zlib');

const CHANNELS = { 0: 1, 2: 3, 3: 1, 4: 2, 6: 4 };

function paeth(a, b, c) {
  const p = a + b - c, pa = Math.abs(p - a), pb = Math.abs(p - b), pc = Math.abs(p - c);
  return pa <= pb && pa <= pc ? a : pb <= pc ? b : c;
}

// Devolve { width, height, data } com data em RGBA de 8 bits, 4 bytes por pixel.
function read(file) {
  const buf = fs.readFileSync(file);
  if (buf.readUInt32BE(0) !== 0x89504e47) throw new Error(`não é PNG: ${file}`);

  let width = 0, height = 0, depth = 0, colorType = 0, interlace = 0;
  let palette = null, alpha = null;
  const idat = [];

  for (let off = 8; off < buf.length;) {
    const len = buf.readUInt32BE(off);
    const type = buf.toString('ascii', off + 4, off + 8);
    const body = buf.subarray(off + 8, off + 8 + len);
    if (type === 'IHDR') {
      width = body.readUInt32BE(0); height = body.readUInt32BE(4);
      depth = body[8]; colorType = body[9]; interlace = body[12];
    } else if (type === 'PLTE') palette = body;
    else if (type === 'tRNS') alpha = body;
    else if (type === 'IDAT') idat.push(body);
    else if (type === 'IEND') break;
    off += 12 + len;
  }
  if (depth !== 8) throw new Error(`profundidade ${depth} não suportada`);
  if (interlace) throw new Error('PNG entrelaçado não suportado');

  const ch = CHANNELS[colorType];
  const raw = zlib.inflateSync(Buffer.concat(idat));
  const stride = width * ch;
  const lines = Buffer.alloc(height * stride);

  // Desfaz os filtros por linha (spec PNG §9).
  for (let y = 0; y < height; y++) {
    const filter = raw[y * (stride + 1)];
    const src = raw.subarray(y * (stride + 1) + 1, y * (stride + 1) + 1 + stride);
    const cur = lines.subarray(y * stride, (y + 1) * stride);
    const prev = y ? lines.subarray((y - 1) * stride, y * stride) : null;
    for (let i = 0; i < stride; i++) {
      const a = i >= ch ? cur[i - ch] : 0;
      const b = prev ? prev[i] : 0;
      const c = prev && i >= ch ? prev[i - ch] : 0;
      let v = src[i];
      if (filter === 1) v += a;
      else if (filter === 2) v += b;
      else if (filter === 3) v += (a + b) >> 1;
      else if (filter === 4) v += paeth(a, b, c);
      cur[i] = v & 0xff;
    }
  }

  const data = Buffer.alloc(width * height * 4);
  for (let p = 0; p < width * height; p++) {
    const s = p * ch, d = p * 4;
    if (colorType === 6) { data[d] = lines[s]; data[d+1] = lines[s+1]; data[d+2] = lines[s+2]; data[d+3] = lines[s+3]; }
    else if (colorType === 2) { data[d] = lines[s]; data[d+1] = lines[s+1]; data[d+2] = lines[s+2]; data[d+3] = 255; }
    else if (colorType === 3) {
      const i = lines[s];
      data[d] = palette[i*3]; data[d+1] = palette[i*3+1]; data[d+2] = palette[i*3+2];
      data[d+3] = alpha && i < alpha.length ? alpha[i] : 255;
    }
    else if (colorType === 0) { data[d] = data[d+1] = data[d+2] = lines[s]; data[d+3] = 255; }
    else if (colorType === 4) { data[d] = data[d+1] = data[d+2] = lines[s]; data[d+3] = lines[s+1]; }
  }
  return { width, height, data };
}

function write(file, img) {
  const { width, height, data } = img;
  const raw = Buffer.alloc(height * (width * 4 + 1));
  for (let y = 0; y < height; y++) {
    raw[y * (width * 4 + 1)] = 0; // filtro None: o zlib comprime bem o suficiente para pixel art
    data.copy(raw, y * (width * 4 + 1) + 1, y * width * 4, (y + 1) * width * 4);
  }
  const chunk = (type, body) => {
    const out = Buffer.alloc(12 + body.length);
    out.writeUInt32BE(body.length, 0);
    out.write(type, 4, 'ascii');
    body.copy(out, 8);
    out.writeInt32BE(crc(out.subarray(4, 8 + body.length)), 8 + body.length);
    return out;
  };
  const ihdr = Buffer.alloc(13);
  ihdr.writeUInt32BE(width, 0); ihdr.writeUInt32BE(height, 4);
  ihdr[8] = 8; ihdr[9] = 6;
  fs.writeFileSync(file, Buffer.concat([
    Buffer.from([0x89, 0x50, 0x4e, 0x47, 0x0d, 0x0a, 0x1a, 0x0a]),
    chunk('IHDR', ihdr),
    chunk('IDAT', zlib.deflateSync(raw, { level: 9 })),
    chunk('IEND', Buffer.alloc(0)),
  ]));
}

let table = null;
function crc(buf) {
  if (!table) {
    table = new Int32Array(256);
    for (let n = 0; n < 256; n++) {
      let c = n;
      for (let k = 0; k < 8; k++) c = c & 1 ? 0xedb88320 ^ (c >>> 1) : c >>> 1;
      table[n] = c;
    }
  }
  let c = -1;
  for (let i = 0; i < buf.length; i++) c = table[(c ^ buf[i]) & 0xff] ^ (c >>> 8);
  return c ^ -1;
}

const blank = (width, height) => ({ width, height, data: Buffer.alloc(width * height * 4) });
const at = (img, x, y) => img.data.readUInt32BE((y * img.width + x) * 4);

// Caixa dos pixels com alfa acima do limiar. Devolve null se a imagem estiver vazia.
function bounds(img, threshold = 8) {
  let x0 = img.width, y0 = img.height, x1 = -1, y1 = -1;
  for (let y = 0; y < img.height; y++)
    for (let x = 0; x < img.width; x++)
      if (img.data[(y * img.width + x) * 4 + 3] > threshold) {
        if (x < x0) x0 = x; if (x > x1) x1 = x;
        if (y < y0) y0 = y; if (y > y1) y1 = y;
      }
  return x1 < 0 ? null : { x0, y0, x1, y1, w: x1 - x0 + 1, h: y1 - y0 + 1 };
}

function crop(img, x0, y0, w, h) {
  const out = blank(w, h);
  for (let y = 0; y < h; y++)
    for (let x = 0; x < w; x++) {
      const sx = x0 + x, sy = y0 + y;
      if (sx < 0 || sy < 0 || sx >= img.width || sy >= img.height) continue;
      img.data.copy(out.data, (y * w + x) * 4, (sy * img.width + sx) * 4, (sy * img.width + sx) * 4 + 4);
    }
  return out;
}

// Cola src sobre dst em (x,y), respeitando o alfa como máscara binária — pixel art não faz blend.
function blit(dst, src, x, y) {
  for (let sy = 0; sy < src.height; sy++)
    for (let sx = 0; sx < src.width; sx++) {
      if (src.data[(sy * src.width + sx) * 4 + 3] <= 8) continue;
      const dx = x + sx, dy = y + sy;
      if (dx < 0 || dy < 0 || dx >= dst.width || dy >= dst.height) continue;
      src.data.copy(dst.data, (dy * dst.width + dx) * 4, (sy * src.width + sx) * 4, (sy * src.width + sx) * 4 + 4);
    }
}

module.exports = { read, write, blank, at, bounds, crop, blit };
