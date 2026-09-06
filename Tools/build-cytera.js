// Quantiza as fontes geradas de Cytera para a paleta oficial e as leva aos grupos finais.
//
//   node Tools/build-cytera.js   (rode build-cytera-fx.js antes, para as camadas por codigo)
//
// Cytera e quase toda de UM material por asset — onda e agua, rocha e rocha, nuvem e nuvem —
// entao a maioria vai por `remapParaRampa`, que recebe a rampa pronta. Classificar quando nao
// ha o que classificar so cria chance de errar, e ja errou em Troia.
const path = require('path'), fs = require('fs');
const p = require('./png.js');
const { loadPalette, remap, remapParaRampa } = require('./ramp-map.js');

const ROOT = path.join(__dirname, '..');
const ENV = path.join(ROOT, 'Docs/Environment_Cytera');
const ramps = loadPalette(path.join(ENV, 'Palette/CYTERA_PALETTE.gpl'));
const UN = 42.857143;

const lum = (r, g, b) => 0.2126 * r + 0.7152 * g + 0.0722 * b;
const sat = (r, g, b) => { const mx = Math.max(r, g, b); return mx ? (mx - Math.min(r, g, b)) / mx : 0; };

/**
 * Classificador para os assets do NAVIO, os unicos de Cytera com mais de um material.
 * Madeira molhada e lona clara nao se distinguem por matiz — as duas sao quentes e
 * dessaturadas — mas se distinguem por luminancia com folga: a lona mede acima de 150 e a
 * madeira abaixo de 110 nos assets medidos.
 */
const materialNavio = (r, g, b) => {
  const L = lum(r, g, b), S = sat(r, g, b);
  if (L < 30) return 'Contorno';
  if (S < 0.14) return L >= 150 ? 'Espuma' : 'Rocha molhada';   // corda, metal, respingo
  return L >= 130 ? 'Lona' : 'Madeira';
};

const PLANO = [
  ['Ocean', '_cut_wave_large', 'wave_large', { rampa: 'Mar' }],
  ['Ocean', '_fonte_wave_medium', 'wave_medium', { rampa: 'Mar' }],
  ['Background', '_fonte_storm_cloud', 'storm_cloud', { rampa: 'Nuvem escura' }],
  ['Rocks', '_fonte_sea_rock', 'rock_tall', { rampa: 'Rocha molhada' }],
  ['Rocks', '_fonte_sea_rock_wide', 'rock_wide', { rampa: 'Rocha molhada' }],
  ['Ship', '_cut_ship_mast', 'mast_sail', { classe: materialNavio }],
  ['Ship', '_fonte_wreckage', 'wreckage', { classe: materialNavio }],
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

console.log('asset'.padEnd(16) + 'grupo'.padEnd(13) + 'tamanho     unidades      antes -> depois');
for (const [grupo, fonte, nome, est] of PLANO) {
  const entrada = path.join(ENV, '_fontes', fonte + '.png');
  if (!fs.existsSync(entrada)) { console.log('  FALTA ' + fonte); continue; }

  let img = p.read(entrada);
  const antes = distancia(img);
  img = est.rampa ? remapParaRampa(img, ramps, est.rampa) : remap(img, ramps, { classify: est.classe });

  const dir = path.join(ENV, grupo);
  fs.mkdirSync(dir, { recursive: true });
  p.write(path.join(dir, 'cytera_' + nome + '.png'), img);

  console.log(nome.padEnd(16) + grupo.padEnd(13) +
    `${img.width}x${img.height}`.padEnd(12) +
    `${(img.width / UN).toFixed(2)}x${(img.height / UN).toFixed(2)}`.padEnd(14) +
    `${antes.toFixed(1)} -> ${distancia(img).toFixed(1)}`);
}
