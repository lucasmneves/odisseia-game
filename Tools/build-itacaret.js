// Leva as fontes de Ítaca Return à paleta OFICIAL DE ÍTACA e corrige o que a geração errou.
//
//   node Tools/build-itacaret.js
//
// ## Esta fase NÃO tem paleta própria, e essa é a decisão central dela
//
// Todas as onze fases anteriores ganharam uma paleta nova. Esta usa
// `Docs/Environment_Ithaca/Palette/ITHACA_PALETTE.gpl` — a mesma da fase 01 — sem trocar um
// hex.
//
// O briefing pede que o jogador reconheça Ítaca imediatamente e perceba que algo mudou. Cor é a
// coisa que o olho reconhece antes da forma: uma paleta nova, por mais parecida que fosse,
// diria "outro lugar" antes de qualquer prédio aparecer na tela. Então a cor fica idêntica e a
// MUDANÇA vem de outras três coisas:
//
//   1. a LUZ — fundo de fim de tarde e um degradê âmbar por cima, no lugar do céu azul de
//      meio-dia da fase 01;
//   2. o ESTADO dos objetos — casa tomada de hera, cerca quebrada, barco apodrecendo na praia;
//   3. o que PASSOU A EXISTIR — a barraca de feira e o braseiro, que a fase 01 não tem.
//
// ## O braseiro não é quantizado, e a exceção já existia
//
// A paleta de Ítaca não tem rampa de FOGO. Isso está registrado desde a fase 01, onde a tocha
// foi um dos quatro assets que ficaram fora da paleta pelo mesmo motivo. Quantizar uma chama
// numa paleta sem laranja não a aproxima: joga o fogo na rampa errada e destrói o objeto.
const path = require('path'), fs = require('fs');
const p = require('./png.js');
const { cutout } = require('./cutout.js');
const { removerXadrez, tirarPoeira, temXadrezPintado } = require('./checker-cut.js');
const { loadPalette, remap } = require('./ramp-map.js');
const { mirrorDouble } = require('./make-tileable.js');
const { check } = require('./seam-test.js');

const ROOT = path.join(__dirname, '..');
const ENV = path.join(ROOT, 'Docs/Environment_ItacaReturn');
const FONTES = path.join(ENV, '_fontes');
// A paleta vem da pasta da FASE 01, e não de uma cópia local. Cópia divergiria.
const ramps = loadPalette(path.join(ROOT, 'Docs/Environment_Ithaca/Palette/ITHACA_PALETTE.gpl'));
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
 * Classificador sobre as nove rampas de Ítaca.
 *
 * Os dois ramos ambíguos são declarados por asset, pela razão que as fases 12 e 13 já mediram
 * duas vezes: casca, terra, pano e calcário ocupam a mesma região de matiz e luminância, e
 * nenhum limiar os separa. Quem sabe a diferença é quem chamou.
 */
const materialItaca = ({ quenteClaro = 'Terra / caminho', quenteEscuro = 'Madeira',
  cinzaVaiPara = 'Pedra fria', temAgua = false } = {}) => (r, g, b) => {
  const L = lum(r, g, b), S = sat(r, g, b), H = hue(r, g, b);

  if (L < 34) { return 'Contorno'; }
  if (temAgua && S >= 0.15 && H >= 175 && H < 255) { return 'Agua'; }
  // Verde-azulado é folha viva; verde-amarelado dessaturado é oliveira e mato seco. É a mesma
  // separação por matiz que Circe provou e que Calipso repetiu.
  if (H >= 90 && H < 175 && S >= 0.14) { return 'Folhagem'; }
  if (H >= 50 && H < 90 && S >= 0.14) { return 'Oliva seca'; }
  if (H >= 175 && H < 260) { return 'Ceu'; }
  if (S < 0.14) { return cinzaVaiPara; }
  return L > 140 ? quenteClaro : quenteEscuro;
};

function gravar(destino, img, nota) {
  fs.mkdirSync(path.dirname(destino), { recursive: true });
  p.write(destino, img);
  console.log(`  ${path.relative(ENV, destino).padEnd(44)} ${img.width}x${img.height}px = ` +
    `${(img.width / UN).toFixed(2)} x ${(img.height / UN).toFixed(2)} un${nota ? '   ' + nota : ''}`);
}
const fonte = (n) => p.read(path.join(FONTES, n + '.png'));
const apertar = (img) => { const b = p.bounds(img); return b ? p.crop(img, b.x0, b.y0, b.w, b.h) : img; };

function limparSprite(nome, { peçaUnica = false } = {}) {
  let img = fonte(nome);
  if (temXadrezPintado(img)) {
    const r = removerXadrez(img);
    const d = tirarPoeira(r.img, peçaUnica ? 2000 : 20);
    console.log(`    ${nome}: xadrez PINTADO — ${(r.removido * 100).toFixed(1)}% virou alpha` +
      (d.ilhas ? `, ${d.ilhas} ilhas de poeira removidas` : ''));
    img = d.img;
  } else {
    const r = cutout(img, 18);
    if (r.removed / (img.width * img.height) > 0.02) {
      console.log(`    ${nome}: fundo chapado — ${(100 * r.removed / (img.width * img.height)).toFixed(1)}% removido`);
      img = r.img;
    }
  }
  return apertar(img);
}

function espelharSePreciso(img, nome) {
  const antes = check(img);
  if (antes.ok) {
    console.log(`    ${nome}: ladrilha nativamente (emenda ${antes.seam.toFixed(1)}, p90 ${antes.p90.toFixed(1)})`);
    return img;
  }
  const out = mirrorDouble(img);
  const depois = check(out);
  console.log(`    ${nome}: emenda ${antes.seam.toFixed(1)} -> ${depois.seam.toFixed(1)} ` +
    `(p90 interna ${depois.p90.toFixed(1)}) — espelhada`);
  return out;
}

/**
 * Corta o céu de uma faixa por flood-fill a partir do TOPO.
 *
 * Existe porque SÓ UMA camada de fundo pode carregar céu — duas empilhadas produzem faixas
 * horizontais com emendas duras atravessando a tela, medido em Sereias. Aqui quem carrega o céu
 * é `itaca_ret_bg_dusk`, e a faixa de vila veio com um céu quase preto no topo que, empilhado,
 * poria uma noite no meio de um fim de tarde.
 */
function cortarCeuDoTopo(img, tol = 34) {
  const { width: w, height: h } = img;
  const out = p.blank(w, h);
  img.data.copy(out.data);
  const at = (i) => [img.data[i * 4], img.data[i * 4 + 1], img.data[i * 4 + 2]];
  const refs = [];
  for (let x = 0; x < w; x += 8) { refs.push(at(x)); }
  const near = (i) => {
    const c = at(i);
    return refs.some(r => Math.abs(c[0] - r[0]) <= tol && Math.abs(c[1] - r[1]) <= tol &&
      Math.abs(c[2] - r[2]) <= tol);
  };
  const bg = new Uint8Array(w * h), pilha = [];
  for (let x = 0; x < w; x++) { if (near(x)) { bg[x] = 1; pilha.push(x); } }
  while (pilha.length) {
    const k = pilha.pop(), x = k % w, y = (k - x) / w;
    for (const [dx, dy] of [[1, 0], [-1, 0], [0, 1], [0, -1]]) {
      const nx = x + dx, ny = y + dy;
      if (nx < 0 || ny < 0 || nx >= w || ny >= h) { continue; }
      const nk = ny * w + nx;
      if (bg[nk] || !near(nk)) { continue; }
      bg[nk] = 1; pilha.push(nk);
    }
  }
  let n = 0;
  for (let i = 0; i < w * h; i++) { if (bg[i]) { out.data[i * 4 + 3] = 0; n++; } }
  return { img: out, removido: n / (w * h) };
}

// ---------------------------------------------------------------- construção

console.log('Ítaca Return — quantização na paleta da FASE 01\n');

// 1. O fundo de fim de tarde. NÃO quantizado: é a peça que carrega a mudança de hora, e um
//    degradê de pôr do sol espremido em quatro passos vira faixas.
gravar(path.join(ENV, 'Background/itaca_ret_bg_dusk.png'),
  espelharSePreciso(fonte('itaca_ret_bg_dusk'), 'bg_dusk'), 'sem quantizar — é fundo');

// 2. A faixa de vila, com o céu cortado.
{
  const { img, removido } = cortarCeuDoTopo(fonte('itaca_ret_village_band'));
  const b = p.bounds(img);
  gravar(path.join(ENV, 'Midground/itaca_ret_village_band.png'),
    espelharSePreciso(p.crop(img, b.x0, b.y0, b.w, b.h), 'village_band'),
    `céu cortado (${(removido * 100).toFixed(0)}% do quadro)`);
}

// 3. Sprites soltos, todos na paleta da fase 01.
const SOLTOS = [
  // Calcário claro das casas, madeira nas portas, hera na `Folhagem`.
  ['itaca_ret_house_overgrown', 'Variations/itaca_ret_house_overgrown.png',
    { quenteClaro: 'Terra / caminho', quenteEscuro: 'Madeira', cinzaVaiPara: 'Pedra', peçaUnica: true }],

  // A barraca: pano claro e madeira. `cinzaVaiPara: 'Pedra'` porque o cinza aqui é cerâmica de
  // jarro, não pedra fria de muro.
  ['itaca_ret_market_stall', 'Village/itaca_ret_market_stall.png',
    { quenteClaro: 'Terra / caminho', quenteEscuro: 'Madeira', cinzaVaiPara: 'Pedra' }],

  // Madeira envelhecida: o cinza dela é madeira prateada pelo tempo, não pedra — por isso
  // `cinzaVaiPara: 'Madeira'`. É o mesmo erro que em Calipso pintou um tronco de mármore.
  ['itaca_ret_boat_derelict', 'Harbor/itaca_ret_boat_derelict.png',
    { quenteClaro: 'Terra / caminho', quenteEscuro: 'Madeira', cinzaVaiPara: 'Madeira' }],
  ['itaca_ret_fence_broken', 'Variations/itaca_ret_fence_broken.png',
    { quenteClaro: 'Oliva seca', quenteEscuro: 'Madeira', cinzaVaiPara: 'Madeira' }],

  ['itaca_ret_overgrowth', 'Vegetation/itaca_ret_overgrowth.png',
    { quenteClaro: 'Oliva seca', quenteEscuro: 'Folhagem', cinzaVaiPara: 'Oliva seca' }],
];
for (const [nome, destino, opts] of SOLTOS) {
  const limpo = limparSprite(nome, { peçaUnica: !!opts.peçaUnica });
  gravar(path.join(ENV, destino), remap(limpo, ramps, { classify: materialItaca(opts) }));
}

// 4. O braseiro, SEM quantizar — ver o cabeçalho. A paleta de Ítaca não tem rampa de fogo, e a
//    fase 01 já abriu esta exceção com a tocha.
gravar(path.join(ENV, 'Props/itaca_ret_brazier.png'), limparSprite('itaca_ret_brazier'),
  'sem quantizar — a paleta de Ítaca não tem rampa de fogo');

// 5. O degradê do anoitecer.
//
//    Terceira fase seguida a usar a mesma ferramenta, e a terceira cor: azul frio em Gado do
//    Sol (punição), âmbar em Calipso (partida), e aqui um violeta-azulado de anoitecer —
//    porque o que esta fase prepara não é consequência nem despedida, é o CONFRONTO da fase 15.
//    A noite chegando é o que liga uma coisa na outra.
{
  const LARGURA = 256, ALTURA = 8;
  const grad = p.blank(LARGURA, ALTURA);
  const [cr, cg, cb] = [0x3a, 0x36, 0x5e];
  for (let x = 0; x < LARGURA; x++) {
    // Rampa em 45% da largura, e nao 70%.
    //
    // O veu precisa de DUAS coisas ao mesmo tempo: chegar ao teto onde o jogador chega, e se
    // estender bem alem do fim da fase para a borda direita dele nao aparecer em quadro. Com
    // rampa de 70% nao da para ter as duas — ou o veu termina cedo e a borda aparece, ou ele se
    // estende e a rampa fica fraca justamente na porta de casa (media 0,19, invisivel).
    // Encurtando a rampa e alongando o plato, as duas coisas cabem: teto em x≈38, que e a porta,
    // e o sprite seguindo ate 56.
    const t = Math.min(1, (x / (LARGURA - 1)) / 0.45);
    const a = Math.round(255 * 0.34 * Math.pow(t, 1.2));
    for (let y = 0; y < ALTURA; y++) {
      const o = (y * LARGURA + x) * 4;
      grad.data[o] = cr; grad.data[o + 1] = cg; grad.data[o + 2] = cb; grad.data[o + 3] = a;
    }
  }
  gravar(path.join(ENV, 'VFX/itaca_ret_dusk_gradient.png'), grad,
    'degradê de alpha por coluna, 0 a 0,34');
}

console.log('\npronto.');
