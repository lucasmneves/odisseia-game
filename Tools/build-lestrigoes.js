// Quantiza as fontes de Lestrigoes para a paleta oficial e as leva aos grupos finais.
//
//   node Tools/build-lestrigoes.js   (rode build-lestrigoes-cave.js antes)
//
// A fase tem TRES rampas de pedra — Calcario ao sol, Pedra cinza a meia luz, Rocha profunda no
// interior — e a escolha de qual usar e por ASSET, nao por classificador: o mesmo cinza que e
// falesia iluminada la fora e parede de caverna la dentro, e nenhum limiar de cor separa os
// dois. Quem separa e o lugar onde o asset vai ser usado, que so o plano abaixo sabe.
const path = require('path'), fs = require('fs');
const p = require('./png.js');
const { loadPalette, remap, remapParaRampa } = require('./ramp-map.js');

const ROOT = path.join(__dirname, '..');
const ENV = path.join(ROOT, 'Docs/Environment_Lestrigoes');
const ramps = loadPalette(path.join(ENV, 'Palette/LESTRIGOES_PALETTE.gpl'));
const UN = 42.857143;

const lum = (r, g, b) => 0.2126 * r + 0.7152 * g + 0.0722 * b;
const sat = (r, g, b) => { const mx = Math.max(r, g, b); return mx ? (mx - Math.min(r, g, b)) / mx : 0; };
const hue = (r, g, b) => {
  const mx = Math.max(r, g, b), mn = Math.min(r, g, b), d = mx - mn;
  if (!d) return 0;
  let h = mx === r ? (((g - b) / d) + 6) % 6 : mx === g ? ((b - r) / d) + 2 : ((r - g) / d) + 4;
  return h * 60;
};

const materialLestrigoes = (corteMadeira = 0.40, { temFogo = false } = {}) => (r, g, b) => {
  const L = lum(r, g, b), S = sat(r, g, b), H = hue(r, g, b);
  if (L < 26 || (L < 42 && S < 0.30)) return 'Contorno';

  if (temFogo && S >= 0.70 && H >= 12 && H <= 55 && L >= 120) return 'Fogo';
  if (H >= 60 && H < 160 && S >= 0.20) return 'Terra';          // musgo e arbusto
  if (H >= 160 && H <= 250 && S >= 0.22) return 'Mar profundo';
  if (S >= corteMadeira) return 'Madeira';
  // Sem saturacao a fase inteira e pedra fria, e a decisao e SO por luminancia. Nao ha corte
  // de matiz aqui de proposito: em Lestrigoes o quente e o frio sao o mesmo material sob luz
  // diferente, e separa-los por matiz partiria uma falesia ao meio.
  return L >= 190 ? 'Ceu frio' : L >= 140 ? 'Pedra fria' : L >= 88 ? 'Falesia' : 'Pedra clara';
};

function espelhar(img) {
  const W = img.width, H = img.height, P = 2 * W - 2;
  const out = p.blank(P, H);
  for (let y = 0; y < H; y++) {
    for (let x = 0; x < P; x++) {
      const sx = x < W ? x : P - x;
      const o = (y * W + sx) * 4, q = (y * P + x) * 4;
      out.data[q] = img.data[o]; out.data[q + 1] = img.data[o + 1];
      out.data[q + 2] = img.data[o + 2]; out.data[q + 3] = img.data[o + 3];
    }
  }
  return out;
}

/** Remove a faixa chapada que o modelo poe no topo de um corte de terreno. */
function cortarCeu(img) {
  const cor = [img.data[0], img.data[1], img.data[2]];
  const chapada = y => {
    for (let x = 0; x < img.width; x++) {
      const o = (y * img.width + x) * 4;
      if (Math.abs(img.data[o] - cor[0]) + Math.abs(img.data[o + 1] - cor[1]) +
          Math.abs(img.data[o + 2] - cor[2]) > 24) return false;
    }
    return true;
  };
  let y = 0;
  while (y < img.height && chapada(y)) y++;
  return y === 0 ? img : p.crop(img, 0, y, img.width, img.height - y);
}

const PLANO = [
  ['Gameplay', '_fonte_tiles_quay', 'tiles_quay', { classe: materialLestrigoes(0.44), cortarCeu: true, espelho: true }],
  ['Gameplay', '_fonte_tiles_rock', 'tiles_rock', { rampa: 'Falesia', cortarCeu: true, espelho: true }],
  ['Gameplay', '_fonte_tiles_planks', 'tiles_planks', { classe: materialLestrigoes(0.34), cortarCeu: true, espelho: true }],
  ['Gameplay', '_fonte_stone_stair', 'stone_stair', { classe: materialLestrigoes(0.44) }],

  ['Midground', '_fonte_cliff_wall', 'cliff_wall', { rampa: 'Falesia', espelho: true }],
  ['Midground', '_fonte_giant_wall', 'giant_wall', { rampa: 'Pedra clara', espelho: true }],

  ['City', '_fonte_giant_house', 'giant_house', { classe: materialLestrigoes(0.42) }],
  ['City', '_fonte_watchtower', 'watchtower', { classe: materialLestrigoes(0.42) }],

  ['Harbor', '_fonte_mooring', 'mooring', { classe: materialLestrigoes(0.36) }],
  ['Harbor', '_fonte_giant_crates', 'giant_crates', { classe: materialLestrigoes(0.36) }],
  ['Harbor', '_cut_broken_ship', 'broken_ship', { classe: materialLestrigoes(0.36) }],

  ['Props', '_fonte_coastal_scrub', 'coastal_scrub', { classe: materialLestrigoes(0.34) }],
];

const cores = [];
for (const k of Object.keys(ramps)) for (const c of ramps[k]) cores.push(c);
const distancia = img => {
  let soma = 0, n = 0;
  for (let i = 0; i < img.data.length; i += 4) {
    if (img.data[i + 3] < 8) continue;
    let m = Infinity;
    for (const c of cores) {
      const d = (img.data[i] - c[0]) ** 2 + (img.data[i + 1] - c[1]) ** 2 + (img.data[i + 2] - c[2]) ** 2;
      if (d < m) m = d;
    }
    soma += Math.sqrt(m); n++;
  }
  return n ? soma / n : 0;
};

console.log('asset'.padEnd(18) + 'grupo'.padEnd(12) + 'tamanho     unidades      antes -> depois');
for (const [grupo, fonte, nome, est] of PLANO) {
  const entrada = est.deCave
    ? path.join(ENV, 'Cave', fonte + '.png')
    : path.join(ENV, '_fontes', fonte + '.png');
  if (!fs.existsSync(entrada)) { console.log('  FALTA ' + fonte); continue; }

  let img = p.read(entrada);
  const antes = distancia(img);
  if (est.cortarCeu) img = cortarCeu(img);
  if (est.alturaMax) img = p.crop(img, 0, 0, img.width, est.alturaMax);
  img = est.rampa ? remapParaRampa(img, ramps, est.rampa) : remap(img, ramps, { classify: est.classe });
  if (est.espelho) img = espelhar(img);

  const dir = path.join(ENV, grupo);
  fs.mkdirSync(dir, { recursive: true });
  p.write(path.join(dir, 'lestrigoes_' + nome + '.png'), img);

  console.log(nome.padEnd(18) + grupo.padEnd(12) +
    `${img.width}x${img.height}`.padEnd(12) +
    `${(img.width / UN).toFixed(2)}x${(img.height / UN).toFixed(2)}`.padEnd(14) +
    `${antes.toFixed(1)} -> ${distancia(img).toFixed(1)}`);
}
