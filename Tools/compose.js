// Banco de materiais de Ítaca: recorta da casa v3 e produz superfícies do tamanho pedido.
// Toda arquitetura nova sai daqui — nenhuma geração nova é necessária enquanto o material
// existente cobrir o objeto. Ver seção 9 de ITHACA_ENVIRONMENT.md.
const path = require('path'), p = require('./png.js');
const { loadPalette, remap } = require('./ramp-map.js');

const ROOT = path.join(__dirname, '..');
const ENV = path.join(ROOT, 'Docs/Environment_Ithaca');
const UN = 42.857143;
const ramps = loadPalette(path.join(ENV, 'Palette/ITHACA_PALETTE.gpl'));

// Coordenadas medidas por Tools/map-materials.js sobre o corpo da v3 (linhas 58..176 da fonte).
const BODY = { top: 58, bottom: 176 };
const REGIONS = {
  // Faixa mais larga sem porta, janela nem sobra de contorno — medida por map-materials.js.
  // Contorno importa: uma sobra dele vira listra preta repetida a cada período do espelho.
  paredeLimpa: { x: 10, y: 14, w: 30, h: 86 },
  porta:       { x: 42, y: 24, w: 58, h: 93 },    // verga + ombreiras + painel
  janela:      { x: 131, y: 40, w: 27, h: 32 },   // moldura + vidro
};

// O corpo é remapeado na paleta UMA vez, aqui. Assim tudo que sai do banco já está na
// paleta e a composição nunca precisa remapear de novo — o que importa porque o remapeamento
// distribui por luminância relativa à imagem inteira: rodá-lo sobre um recorte daria outro
// resultado que sobre o corpo todo, e as peças deixariam de casar entre si.
let _body = null;
function body() {
  if (!_body) {
    const src = p.read(path.join(ENV, 'Architecture/_fonte_v3_telha_inclinada.png'));
    _body = remap(p.crop(src, 0, BODY.top, src.width, BODY.bottom - BODY.top + 1), ramps);
  }
  return _body;
}
const region = (name) => { const r = REGIONS[name]; return p.crop(body(), r.x, r.y, r.w, r.h); };

// Ladrilha por espelhamento com período 2W-2. O espelho ingênuo (período 2W) repete a coluna
// da borda e deixa uma linha dupla visível; descontar as duas colunas de dobra elimina isso e
// a emenda passa a ser uma reflexão exata, sem descontinuidade possível.
function mirrorTile(strip, w, h, offsetY = 0, offsetX = 0) {
  const W = strip.width, period = 2 * W - 2;
  const out = p.blank(w, h);
  for (let y = 0; y < h; y++) {
    const sy = ((y + offsetY) % strip.height + strip.height) % strip.height;
    for (let x = 0; x < w; x++) {
      const m = ((x + offsetX) % period + period) % period;
      const sx = m < W ? m : period - m;
      const so = (sy * W + sx) * 4;
      strip.data.copy(out.data, (y * w + x) * 4, so, so + 4);
    }
  }
  return out;
}

// Laje de telhado plano — a estratigrafia medida da v2, esticada para qualquer largura.
function lajePlana(w, h = 12) {
  const [OUT, LIGHT, BASE, SHADE] = [
    ramps['Contorno'][3], ramps['Terra / caminho'][0], ramps['Terra / caminho'][1], ramps['Terra / caminho'][2],
  ];
  const bands = [[0, 2, OUT], [2, 4, LIGHT], [4, h - 3, BASE], [h - 3, h - 2, SHADE], [h - 2, h, OUT]];
  const out = p.blank(w, h);
  for (const [y0, y1, c] of bands)
    for (let y = y0; y < y1; y++)
      for (let x = 0; x < w; x++) {
        if ((x === 0 || x === w - 1) && (y === 0 || y === h - 1)) continue;  // canto chanfrado
        const o = (y * w + x) * 4;
        out.data[o] = c[0]; out.data[o + 1] = c[1]; out.data[o + 2] = c[2]; out.data[o + 3] = 255;
      }
  return out;
}

// Contorno de 1px em volta da silhueta, na cor de contorno profunda da paleta.
function outline(img) {
  const c = ramps['Contorno'][3];
  const out = p.blank(img.width, img.height);
  img.data.copy(out.data);
  const solid = (x, y) => x >= 0 && y >= 0 && x < img.width && y < img.height && img.data[(y * img.width + x) * 4 + 3] > 8;
  for (let y = 0; y < img.height; y++)
    for (let x = 0; x < img.width; x++) {
      if (solid(x, y)) continue;
      if (!(solid(x - 1, y) || solid(x + 1, y) || solid(x, y - 1) || solid(x, y + 1))) continue;
      const o = (y * img.width + x) * 4;
      out.data[o] = c[0]; out.data[o + 1] = c[1]; out.data[o + 2] = c[2]; out.data[o + 3] = 255;
    }
  return out;
}

function fillRect(img, x0, y0, w, h, c) {
  // Arredonda na entrada. Coordenada fracionária (um W/2 com largura ímpar, por exemplo)
  // vira índice não-inteiro no Buffer, que não grava o pixel e ainda deixa cor lixo pela
  // metade — apareceu como verde-limão no meio de um portão de madeira.
  x0 = Math.round(x0); y0 = Math.round(y0); w = Math.round(w); h = Math.round(h);
  for (let y = y0; y < y0 + h; y++)
    for (let x = x0; x < x0 + w; x++) {
      if (x < 0 || y < 0 || x >= img.width || y >= img.height) continue;
      const o = (y * img.width + x) * 4;
      img.data[o] = c[0]; img.data[o + 1] = c[1]; img.data[o + 2] = c[2]; img.data[o + 3] = 255;
    }
}

const trim = (img) => { const b = p.bounds(img); return p.crop(img, b.x0, b.y0, b.w, b.h); };
const un = (px) => (px / UN).toFixed(2);

module.exports = { ramps, ENV, ROOT, UN, body, region, REGIONS, mirrorTile, lajePlana, outline, fillRect, trim, un, p };
