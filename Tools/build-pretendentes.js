// Leva as fontes de Pretendentes à paleta da fase (Ítaca + Fogo, Vinho, Noite, Bronze).
//
//   node Tools/build-pretendentes.js
//
// ## As faixas de arquitetura SÃO quantizadas, ao contrário dos fundos das outras fases
//
// A regra do projeto é não quantizar camada de fundo, porque degradê de céu espremido em quatro
// passos vira faixa. As duas faixas desta fase (parede do salão e muro do pátio) não são céu: são
// ARQUITETURA, e a pedra delas precisa ser a pedra da fase 01 — é o mesmo palácio. Gerada, a
// pedra veio cinza; quantizada nas rampas de Ítaca, ela volta a ser o calcário que o jogador viu
// nas fases 01 e 14.
//
// O preço é o brilho das tochas virar degraus em vez de degradê. Nesta fase isso é o idioma
// certo: poça de luz em degraus é exatamente como um jogo 16-bit desenha fogo numa parede.
//
// O céu do muro do pátio é CORTADO antes da quantização: ele vem do fundo reusado da fase 14.
const path = require('path'), fs = require('fs');
const p = require('./png.js');
const { cutout } = require('./cutout.js');
const { removerXadrez, tirarPoeira, temXadrezPintado } = require('./checker-cut.js');
const { loadPalette, remap } = require('./ramp-map.js');
const { mirrorDouble } = require('./make-tileable.js');
const { check } = require('./seam-test.js');

const ROOT = path.join(__dirname, '..');
const ENV = path.join(ROOT, 'Docs/Environment_Pretendentes');
const FONTES = path.join(ENV, '_fontes');
const ramps = loadPalette(path.join(ENV, 'Palette/PRETENDENTES_PALETTE.gpl'));
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
 * Classificador de Pretendentes, sobre treze rampas.
 *
 * Fogo e Bronze são OPT-IN. As duas ocupam a mesma faixa de matiz (laranja-dourado) e de
 * saturação, e só quem chama sabe se aquele laranja é chama ou metal — é a mesma lição que as
 * fases 12, 13 e 14 mediram com casca, terra e calcário. Um braseiro liga as duas; uma mesa de
 * banquete liga só o bronze das taças; uma ânfora não liga nenhuma.
 *
 * Vinho é LIGADO por padrão, e é a exceção consciente: nesta fase não existe vermelho que não
 * seja vinho ou pano dos pretendentes. Azul também: nenhum sprite solto tem céu, então todo azul
 * é sombra noturna.
 */
const materialPret = ({ temFogo = false, temBronze = false, quenteClaro = 'Terra / caminho',
  quenteEscuro = 'Madeira', cinzaVaiPara = 'Pedra fria', fogoMinL = 115 } = {}) => (r, g, b) => {
  const L = lum(r, g, b), S = sat(r, g, b), H = hue(r, g, b);

  if (L < 30) { return 'Contorno'; }
  // Vinho a partir de S 0,52, e nao 0,35. Com 0,35 as portas de madeira do muro do patio, que
  // sao marrom-avermelhado, sairam VERMELHO-VINHO: o palacio inteiro com portas pintadas da cor
  // dos estandartes. Pano tingido e vinho medem S 0,62 a 0,79; madeira velha fica abaixo de 0,5.
  if (S >= 0.52 && (H >= 340 || H < 14)) { return 'Vinho'; }
  // `fogoMinL` separa CHAMA de PEDRA ILUMINADA POR CHAMA. Numa faixa de arquitetura a luz da tocha
  // pinta a face das colunas de laranja saturado, e com o limiar baixo as colunas inteiras viravam
  // cor de fogo. A chama de verdade e a coisa mais clara do quadro; a pedra que ela ilumina, nao.
  if (temFogo && S >= 0.50 && H >= 14 && H < 58 && L > fogoMinL) { return 'Fogo'; }
  if (temBronze && S >= 0.38 && H >= 28 && H < 58 && L > 60 && L < 215) { return 'Bronze'; }
  if (H >= 190 && H < 260 && S >= 0.12) { return 'Noite'; }
  if (H >= 90 && H < 170 && S >= 0.18) { return 'Folhagem'; }
  if (H >= 58 && H < 90 && S >= 0.18) { return 'Oliva seca'; }
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

function limparSprite(nome, { peçaUnica = false, vazado = false } = {}) {
  let img = fonte(nome);
  if (temXadrezPintado(img)) {
    // `vazado`: objeto com cordame e vãos, em que o xadrez fica preso por dentro — ver a terceira
    // passada de Tools/checker-cut.js. Foi o navio dos pretendentes que precisou.
    const r = removerXadrez(img, 6, { cercado: vazado });
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

/** Corta o céu de uma faixa por flood-fill a partir do TOPO. */
function cortarCeuDoTopo(img, tol = 30) {
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

console.log('Pretendentes — quantização na paleta de Ítaca estendida\n');

// 1. Faixas de arquitetura, quantizadas — ver o cabeçalho.
{
  // cinzaVaiPara 'Terra / caminho', e nao 'Pedra fria'. A pedra gerada veio cinza, e com o destino
  // padrao ela caiu numa rampa cujo passo claro e #a0bcc4 — AZUL. As paredes do palacio sairam
  // como azulejo azul-claro, que e exatamente o contrario do que esta quantizacao existe para
  // fazer: devolver o calcario creme da fase 01.
  const pedra = { temFogo: true, fogoMinL: 200, quenteClaro: 'Terra / caminho',
    quenteEscuro: 'Pedra', cinzaVaiPara: 'Terra / caminho' };

  // O salão é INTERIOR: não tem céu, e o topo irregular escuro é teto, não fundo.
  gravar(path.join(ENV, 'GreatHall/pret_hall_wall_band.png'),
    remap(espelharSePreciso(fonte('pret_hall_wall_band'), 'hall_wall_band'), ramps,
      { classify: materialPret(pedra) }), 'quantizada — é arquitetura');

  const { img, removido } = cortarCeuDoTopo(fonte('pret_courtyard_wall_band'));
  gravar(path.join(ENV, 'Courtyard/pret_courtyard_wall_band.png'),
    remap(espelharSePreciso(apertar(img), 'courtyard_wall_band'), ramps,
      { classify: materialPret(pedra) }),
    `céu cortado (${(removido * 100).toFixed(0)}%), quantizada`);
}

// 2. Sprites soltos.
const SOLTOS = [
  ['pret_banquet_table', 'Banquet/pret_banquet_table.png',
    { temBronze: true, quenteClaro: 'Terra / caminho', quenteEscuro: 'Madeira', cinzaVaiPara: 'Madeira' }],
  ['pret_spilled_feast', 'Banquet/pret_spilled_feast.png',
    // Argila cozida e fogo são a mesma família de cor, e a paleta não a duplica: as ânforas
    // saem dos passos baixos da rampa `Fogo`, que são terracota.
    { temBronze: true, quenteClaro: 'Terra / caminho', quenteEscuro: 'Fogo', cinzaVaiPara: 'Madeira' }],
  ['pret_amphora_group', 'Banquet/pret_amphora_group.png',
    { quenteClaro: 'Fogo', quenteEscuro: 'Fogo', cinzaVaiPara: 'Pedra' }],
  ['pret_barricade', 'Invasion/pret_barricade.png',
    { temBronze: true, quenteClaro: 'Terra / caminho', quenteEscuro: 'Madeira', cinzaVaiPara: 'Madeira' }],
  ['pret_banner', 'Invasion/pret_banner.png',
    { temBronze: true, quenteEscuro: 'Madeira', cinzaVaiPara: 'Madeira' }],
  ['pret_suitor_ship', 'Invasion/pret_suitor_ship.png',
    { temBronze: true, quenteClaro: 'Terra / caminho', quenteEscuro: 'Madeira', cinzaVaiPara: 'Madeira', peçaUnica: true, vazado: true }],
  ['pret_bench_toppled', 'Banquet/pret_bench_toppled.png',
    { quenteClaro: 'Terra / caminho', quenteEscuro: 'Madeira', cinzaVaiPara: 'Madeira' }],
  ['pret_throne', 'GreatHall/pret_throne.png',
    { temBronze: true, quenteClaro: 'Terra / caminho', quenteEscuro: 'Pedra', cinzaVaiPara: 'Pedra fria', peçaUnica: true }],
  ['pret_cooking_fire', 'Courtyard/pret_cooking_fire.png',
    { temFogo: true, quenteClaro: 'Fogo', quenteEscuro: 'Madeira', cinzaVaiPara: 'Pedra fria' }],
  ['pret_torch_stand', 'VFX/pret_torch_stand.png',
    { temFogo: true, temBronze: true, quenteEscuro: 'Madeira', cinzaVaiPara: 'Pedra fria' }],
];
for (const [nome, destino, opts] of SOLTOS) {
  const limpo = limparSprite(nome, { peçaUnica: !!opts.peçaUnica, vazado: !!opts.vazado });
  gravar(path.join(ENV, destino), remap(limpo, ramps, { classify: materialPret(opts) }));
}

// 3. O véu da noite no salão.
//
//    Quarta fase a usar a ferramenta, e a primeira em que ele NÃO escurece: a luz desta fase é
//    fogo, e quanto mais fundo no domínio dos pretendentes, mais quente e mais forte. O véu é
//    laranja de braseiro a partir da entrada do salão — o oposto do anoitecer frio que fecha a
//    fase 14, e é essa inversão que marca a passagem de luz natural para luz artificial.
{
  const LARGURA = 256, ALTURA = 8;
  const grad = p.blank(LARGURA, ALTURA);
  const [cr, cg, cb] = [0xf4, 0x84, 0x2f];              // Fogo Base
  for (let x = 0; x < LARGURA; x++) {
    const t = Math.min(1, (x / (LARGURA - 1)) / 0.45);
    const a = Math.round(255 * 0.18 * Math.pow(t, 1.2));
    for (let y = 0; y < ALTURA; y++) {
      const o = (y * LARGURA + x) * 4;
      grad.data[o] = cr; grad.data[o + 1] = cg; grad.data[o + 2] = cb; grad.data[o + 3] = a;
    }
  }
  gravar(path.join(ENV, 'VFX/pret_firelight_gradient.png'), grad, 'degradê de alpha por coluna, 0 a 0,18');
}

console.log('\npronto.');
