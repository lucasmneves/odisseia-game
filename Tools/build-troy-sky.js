// Céu e faixa de calor de Troia, por código.
//
//   node Tools/build-troy-sky.js
//
// Banda plana e repetitiva vai de procedural, não de geração: a regra do pipeline de Ítaca.
// Sai com emenda indistinguível do interior, cores exatamente da rampa e custo zero — contra
// 40 gerações e emenda de 45 a 78% quando o céu de Ítaca foi tentado pelo `pro`.
//
// A transição usa dithering ordenado Bayer 4x4 entre passos da rampa. Um degradê liso pediria
// dezenas de tons intermediários, que é o oposto de uma paleta fechada.
const path = require('path');
const p = require('./png.js');
const { loadPalette } = require('./ramp-map.js');

const ROOT = path.join(__dirname, '..');
const ENV = path.join(ROOT, 'Docs/Environment_Troy');
const ramps = loadPalette(path.join(ENV, 'Palette/TROY_PALETTE.gpl'));

const BAYER = [[0, 8, 2, 10], [12, 4, 14, 6], [3, 11, 1, 9], [15, 7, 13, 5]];

// A fração é comparada com o limiar SEM somar degrau quando ela é zero. O erro inverso cria
// uma linha dura em cada junção da rampa — foi cometido no céu de Ítaca e é visível a olho nu.
function dither(rampa, s, x, y) {
  const i = Math.min(rampa.length - 1, Math.floor(s));
  const frac = s - i;
  const th = (BAYER[y & 3][x & 3] + 0.5) / 16;
  return rampa[Math.min(rampa.length - 1, i + (frac > th ? 1 : 0))];
}

// A largura é múltipla de 4, o período do Bayer. Assim a emenda ao ladrilhar fica idêntica a
// qualquer fronteira interna, e o ladrilhamento é exato em vez de aproximado.
function banda(largura, altura, rampa, deCima, paraBaixo) {
  const img = p.blank(largura, altura);
  for (let y = 0; y < altura; y++) {
    const t = altura === 1 ? 0 : y / (altura - 1);
    const s = deCima + (paraBaixo - deCima) * t;
    for (let x = 0; x < largura; x++) {
      const c = dither(rampa, s, x, y);
      const o = (y * largura + x) * 4;
      img.data[o] = c[0]; img.data[o + 1] = c[1]; img.data[o + 2] = c[2]; img.data[o + 3] = 255;
    }
  }
  return img;
}

const UN = 42.857143;
const saidas = [
  // Céu: do passo Profunda no topo ao Highlight no horizonte, como em Ítaca.
  ['troy_bg_sky', banda(512, 256, ramps['Ceu'], 3, 0)],
  // Faixa de calor sobre o horizonte seco: fumaça clareando para o alto. É o que dá a leitura
  // de "sítio de guerra" ao fundo sem custar um asset gerado.
  ['troy_bg_haze', banda(512, 96, ramps['Fumaca'], 3, 1)],
];

const dir = path.join(ENV, 'Layers');
require('fs').mkdirSync(dir, { recursive: true });
for (const [nome, img] of saidas) {
  p.write(path.join(dir, nome + '.png'), img);
  console.log(`${nome}  ${img.width}x${img.height}px = ${(img.width / UN).toFixed(2)} x ${(img.height / UN).toFixed(2)} un`);
}
