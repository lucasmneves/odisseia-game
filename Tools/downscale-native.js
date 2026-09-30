// Reamostra um asset ENCOLHIDO NO TRANSFORM para a densidade nativa do projeto (42,857 px/un),
// para ele poder ir à cena em escala 1 com o mesmo tamanho de tela.
//
//   node Tools/downscale-native.js <entrada.png> <fator> [saida.png]
//
// Saída padrão: <entrada>_native.png, com .meta copiado do original (mesmas configurações — PPU, filtro,
// pivô, modo de malha) e GUID derivado do caminho, para re-rodar não quebrar referência.
//
// Por que: um sprite a 0,52 na cena desenha pixels de 0,52 do tamanho dos do resto do jogo. Com filtro Point,
// o Unity já descarta pixels na hora de desenhar — mas a cada quadro, na posição da câmera, e por isso o
// detalhe TREME quando a câmera anda. Reamostrar uma vez, aqui, fixa quais pixels ficam.
//
// Filtro de MODA: cada pixel de saída recebe a cor mais frequente do bloco de origem que ele cobre (a
// transparência conta como cor). Nunca inventa cor — só usa as que o asset já tem —, então a paleta e o
// contorno continuam pixel art. Média e bilinear criariam meios-tons.
const fs = require('fs'), path = require('path'), crypto = require('crypto');
const png = require('./png.js');

const ROOT = path.join(__dirname, '..');

function moda(src, k) {
  const W = Math.max(1, Math.round(src.width * k)), H = Math.max(1, Math.round(src.height * k));
  const out = png.blank(W, H);
  const fx = src.width / W, fy = src.height / H;
  for (let y = 0; y < H; y++) for (let x = 0; x < W; x++) {
    const x0 = Math.floor(x * fx), x1 = Math.max(x0 + 1, Math.floor((x + 1) * fx));
    const y0 = Math.floor(y * fy), y1 = Math.max(y0 + 1, Math.floor((y + 1) * fy));
    const conta = new Map();
    let melhor = 0, cor = 0;
    for (let sy = y0; sy < y1; sy++) for (let sx = x0; sx < x1; sx++) {
      const i = (sy * src.width + sx) * 4;
      const c = src.data[i + 3] < 128 ? 0 : src.data.readUInt32BE(i) | 0xff;
      const n = (conta.get(c) || 0) + 1;
      conta.set(c, n);
      if (n > melhor) { melhor = n; cor = c; }
    }
    out.data.writeUInt32BE(cor >>> 0, (y * W + x) * 4);
  }
  return out;
}

function meta(original, saida) {
  const rel = path.relative(ROOT, saida).replace(/\\/g, '/');
  const guid = crypto.createHash('md5').update(rel).digest('hex');
  const m = fs.readFileSync(original + '.meta', 'utf8').replace(/^guid: \w+$/m, 'guid: ' + guid);
  fs.writeFileSync(saida + '.meta', m);
}

module.exports = { moda };

if (require.main === module) {
  const [entrada, fator, saidaArg] = process.argv.slice(2);
  if (!entrada || !fator) { console.error('uso: node Tools/downscale-native.js <entrada.png> <fator> [saida.png]'); process.exit(1); }
  const saida = saidaArg || entrada.replace(/\.png$/, '_native.png');
  const src = png.read(entrada);
  const out = moda(src, parseFloat(fator));
  png.write(saida, out);
  meta(entrada, saida);
  console.log(`${path.basename(saida)}: ${src.width}x${src.height} -> ${out.width}x${out.height} (fator ${fator})`);
}
