// Transforma as fontes geradas de Cicones nos assets finais.
//
//   node Tools/build-cicones.js
//
// Duas coisas acontecem aqui, e a ordem importa:
//
// 1. QUANTIZACAO para a paleta oficial. As fontes voltam do PixelLab a 10-23 de distancia
//    media da paleta, com 36-67 cores cada; sem isto cada asset traz o proprio verde e o
//    proprio marrom e a fase deixa de parecer o mesmo jogo.
//
// 2. TILING EM ESPELHO dos tilesets, com periodo 2W-2 e nao 2W — 2W duplicaria a coluna da
//    borda e criaria uma listra visivel a cada repeticao.
//
// Cada asset diz a propria estrategia. Asset de UM material vai por `remapParaRampa`, que
// recebe a rampa pronta: classificar um kit de terreno so cria chance de errar, e ja errou em
// Troia (o kit de terra caiu na rampa de Madeira e o chao saiu marrom-vinho).
const path = require('path'), fs = require('fs');
const p = require('./png.js');
const { loadPalette, remap, remapParaRampa } = require('./ramp-map.js');

const ROOT = path.join(__dirname, '..');
const ENV = path.join(ROOT, 'Docs/Environment_Cicones');
const ramps = loadPalette(path.join(ENV, 'Palette/CICONES_PALETTE.gpl'));
const UN = 42.857143;

const lum = (r, g, b) => 0.2126 * r + 0.7152 * g + 0.0722 * b;
const sat = (r, g, b) => { const mx = Math.max(r, g, b); return mx ? (mx - Math.min(r, g, b)) / mx : 0; };
const hue = (r, g, b) => {
  const mx = Math.max(r, g, b), mn = Math.min(r, g, b), d = mx - mn;
  if (!d) return 0;
  let h = mx === r ? (((g - b) / d) + 6) % 6 : mx === g ? ((b - r) / d) + 2 : ((r - g) / d) + 4;
  return h * 60;
};

/**
 * Classificador de Cicones.
 *
 * A ordem dos testes e a propria regra: o que e RARO e inconfundivel sai primeiro (contorno,
 * agua, vegetacao), e o que e ambiguo fica por ultimo decidido por luminancia. Inverter isso
 * faz a pedra clara comer a areia, porque as duas sao claras e pouco saturadas.
 *
 * `corteMadeira` e propriedade do ASSET, nao do pipeline — a madeira de uma carroca gasta e a
 * de uma porta nova nao medem a mesma saturacao. Foi assim em Itaca e em Troia.
 */
const materialCicones = (corteMadeira = 0.42,
  { temTerracota = false, temAgua = false, corteFrio = -4 } = {}) =>
  (r, g, b) => {
    const L = lum(r, g, b), S = sat(r, g, b), H = hue(r, g, b);
    if (L < 26 || (L < 42 && S < 0.30)) return 'Contorno';

    if (temAgua && H >= 160 && H <= 250 && S >= 0.20) return 'Mar raso';
    // Verde e a assinatura de Cicones e nao aparece em Troia: separa folha de palha pelo matiz,
    // porque as duas medem luminancia parecida.
    if (H >= 75 && H < 160 && S >= 0.15) return 'Oliveira';
    if (H >= 55 && H < 75 && S >= 0.20) return 'Grama seca';
    if (temTerracota && H >= 8 && H < 30 && S >= 0.55) return 'Terracota';
    if (H >= 30 && H <= 58 && S >= 0.50 && L >= 150) return 'Bronze';
    if (S >= corteMadeira) return 'Madeira';
    // Frio sem saturacao e argamassa e sombra de alvenaria; quente e areia ou terra.
    //
    // `corteFrio` e propriedade do ASSET. Com o padrao -4 a casa grande inteira caiu em
    // Pedra cinza e virou uma construcao de pedra fria no meio de uma vila mediterranea
    // ensolarada: o reboco dela e quase neutro, e "quase neutro" cai do lado frio de um corte
    // frouxo. A casa pequena, com o mesmo corte, saiu certa — o limiar nao e do pipeline.
    if (b >= r + corteFrio) return 'Pedra cinza';
    return L >= 172 ? 'Pedra clara' : L >= 118 ? 'Terra seca' : 'Madeira';
  };

/**
 * Remove a faixa chapada que o modelo desenha no topo de um tileset: ele entende "corte de
 * terreno" como paisagem e coloca um ceu em cima. A faixa se reconhece por ser uma cor unica
 * repetida em linhas inteiras — nao por um numero de linhas chutado, que mudaria a cada asset.
 */
function cortarCeu(img) {
  const cor = [img.data[0], img.data[1], img.data[2]];
  const linhaChapada = y => {
    for (let x = 0; x < img.width; x++) {
      const o = (y * img.width + x) * 4;
      if (Math.abs(img.data[o] - cor[0]) + Math.abs(img.data[o + 1] - cor[1]) +
          Math.abs(img.data[o + 2] - cor[2]) > 24) return false;
    }
    return true;
  };
  let y = 0;
  while (y < img.height && linhaChapada(y)) y++;
  return y === 0 ? img : p.crop(img, 0, y, img.width, img.height - y);
}

/** Faixa que emenda consigo mesma: periodo 2W-2, sem repetir a coluna da borda. */
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

// grupo, nome final, estrategia. `rampa` = material unico; `classe` = classificador.
const PLANO = [
  ['Terrain', 'tiles_sand', 'tiles_sand', { rampa: 'Areia', espelho: true }],
  ['Terrain', 'tiles_rock', 'tiles_rock', { rampa: 'Pedra cinza', cortarCeu: true, espelho: true }],
  ['Terrain', 'tiles_earth_grass', 'tiles_earth_grass', { classe: materialCicones(0.46), espelho: true }],

  ['Architecture', 'house_small', 'house_small', { classe: materialCicones(0.46) }],
  ['Architecture', 'house_large', 'house_large', { classe: materialCicones(0.46, { corteFrio: 6 }) }],
  ['Architecture', 'drystone_wall', 'drystone_wall', { classe: materialCicones(0.44) }],
  ['Architecture', 'shrine_landmark', 'shrine_landmark', { jaNaPaleta: true }],

  ['Nature', 'olive_tree', 'olive_tree', { classe: materialCicones(0.40) }],
  ['Nature', 'scrub_bush', 'scrub_bush', { classe: materialCicones(0.40) }],
  // O tufo veio plantado num bloco de areia retangular, como se fosse um vaso. A cobertura
  // por linha salta de 65% para 100% em y=78: e ali que o bloco comeca.
  ['Nature', 'grass_tuft', 'grass_tuft', { classe: materialCicones(0.40), alturaMax: 78 }],

  ['Props', 'pithoi_baskets', 'pithoi_baskets', { classe: materialCicones(0.42, { temTerracota: true }) }],
  ['Props', 'well', 'well', { classe: materialCicones(0.44, { temAgua: true }) }],
  ['Props', 'handcart', 'handcart', { classe: materialCicones(0.40) }],
  ['Props', 'wooden_fence', 'wooden_fence', { classe: materialCicones(0.38) }],
  ['Props', 'fishing_boat', 'fishing_boat', { classe: materialCicones(0.40) }],
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

console.log('asset'.padEnd(22) + 'grupo'.padEnd(14) + 'tamanho     unidades      antes -> depois');
for (const [grupo, fonte, nome, est] of PLANO) {
  const entrada = path.join(ENV, '_fontes', `_fonte_${fonte}.png`);
  if (!fs.existsSync(entrada)) { console.log(`  FALTA ${fonte}`); continue; }

  let img = p.read(entrada);
  const antes = distancia(img);

  if (est.cortarCeu) img = cortarCeu(img);
  if (est.alturaMax) img = p.crop(img, 0, 0, img.width, est.alturaMax);
  if (est.rampa) img = remapParaRampa(img, ramps, est.rampa);
  else if (est.classe) img = remap(img, ramps, { classify: est.classe });
  if (est.espelho) img = espelhar(img);

  const dir = path.join(ENV, grupo);
  fs.mkdirSync(dir, { recursive: true });
  p.write(path.join(dir, `cicones_${nome}.png`), img);

  console.log(nome.padEnd(22) + grupo.padEnd(14) +
    `${img.width}x${img.height}`.padEnd(12) +
    `${(img.width / UN).toFixed(2)}x${(img.height / UN).toFixed(2)}`.padEnd(14) +
    `${antes.toFixed(1)} -> ${distancia(img).toFixed(1)}`);
}
