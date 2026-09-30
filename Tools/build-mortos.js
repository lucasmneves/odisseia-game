// Quantiza as fontes de Mortos para a paleta oficial e as leva aos grupos finais.
//
//   node Tools/build-mortos.js   (rode build-mortos-cave.js antes)
//
// A fase tem TRES rampas de pedra — Calcario ao sol, Pedra cinza a meia luz, Rocha profunda no
// interior — e a escolha de qual usar e por ASSET, nao por classificador: o mesmo cinza que e
// falesia iluminada la fora e parede de caverna la dentro, e nenhum limiar de cor separa os
// dois. Quem separa e o lugar onde o asset vai ser usado, que so o plano abaixo sabe.
const path = require('path'), fs = require('fs');
const p = require('./png.js');
const { loadPalette, remap, remapParaRampa } = require('./ramp-map.js');

const ROOT = path.join(__dirname, '..');
const ENV = path.join(ROOT, 'Docs/Environment_MundoDosMortos');
const ramps = loadPalette(path.join(ENV, 'Palette/MORTOS_PALETTE.gpl'));
const UN = 42.857143;

const lum = (r, g, b) => 0.2126 * r + 0.7152 * g + 0.0722 * b;
const sat = (r, g, b) => { const mx = Math.max(r, g, b); return mx ? (mx - Math.min(r, g, b)) / mx : 0; };
const hue = (r, g, b) => {
  const mx = Math.max(r, g, b), mn = Math.min(r, g, b), d = mx - mn;
  if (!d) return 0;
  let h = mx === r ? (((g - b) / d) + 6) % 6 : mx === g ? ((b - r) / d) + 2 : ((r - g) / d) + 4;
  return h * 60;
};

const materialMortos = (corteMadeira = 0.40, { temFogo = false } = {}) => (r, g, b) => {
  const L = lum(r, g, b), S = sat(r, g, b), H = hue(r, g, b);
  if (L < 22 || (L < 38 && S < 0.26)) return 'Contorno';

  if (temFogo && S >= 0.62 && H >= 8 && H <= 58 && L >= 100) return 'Fogo';
  if (H >= 30 && H <= 58 && S >= 0.45 && L >= 130) return 'Bronze';
  // Agua morta e a unica coisa VERDE da fase: o corte de matiz nela e seguro e e o que impede
  // o rio de virar mais uma pedra cinza.
  if (H >= 80 && H < 165 && S >= 0.25) return 'Agua morta';
  if (S >= corteMadeira && H >= 10 && H < 45) return 'Terra';
  // Sem saturacao, a decisao e so por luminancia, e sao QUATRO degraus: marmore, osso, pedra
  // fria e pedra funda. Numa fase sem sol e o valor que separa os planos.
  return L >= 185 ? 'Marmore palido'
    : L >= 140 ? 'Madeira morta'
    : L >= 80 ? 'Pedra fria'
    : 'Pedra funda';
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
  ['Gameplay', '_fonte_tiles_cavefloor', 'tiles_cavefloor', { classe: materialMortos(0.44), cortarCeu: true, espelho: true }],
  ['Gameplay', '_fonte_tiles_ruinfloor', 'tiles_ruinfloor', { classe: materialMortos(0.44), cortarCeu: true, espelho: true }],

  ['Midground', '_fonte_cavern_wall', 'cavern_wall', { rampa: 'Pedra funda', espelho: true }],
  ['Midground', '_fonte_stalactites', 'stalactites', { rampa: 'Pedra fria', espelho: true }],

  ['Ruins', '_fonte_broken_column', 'broken_column', { classe: materialMortos(0.44) }],
  ['Ruins', '_fonte_ruined_wall', 'ruined_wall', { classe: materialMortos(0.44) }],
  ['Ruins', '_fonte_broken_statue', 'broken_statue', { classe: materialMortos(0.44) }],
  ['Ruins', '_cut_stone_gate', 'stone_gate', { classe: materialMortos(0.44) }],

  ['Special', '_fonte_altar', 'altar', { classe: materialMortos(0.42, { temFogo: true }) }],
  ['Props', '_fonte_brazier_tall', 'brazier_tall', { classe: materialMortos(0.40, { temFogo: true }) }],
  ['Props', '_fonte_dead_tree', 'dead_tree', { classe: materialMortos(0.36) }],
  ['Props', '_fonte_shore_rocks', 'shore_rocks', { classe: materialMortos(0.44) }],
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
  p.write(path.join(dir, 'mortos_' + nome + '.png'), img);

  console.log(nome.padEnd(18) + grupo.padEnd(12) +
    `${img.width}x${img.height}`.padEnd(12) +
    `${(img.width / UN).toFixed(2)}x${(img.height / UN).toFixed(2)}`.padEnd(14) +
    `${antes.toFixed(1)} -> ${distancia(img).toFixed(1)}`);
}
