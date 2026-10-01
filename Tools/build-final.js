// Leva as fontes da Fase 16 — Final à paleta da casa de Ítaca e monta os derivados.
//
//   node Tools/build-final.js
//
// ## A mesma casa, outra hora
//
// A 16 se passa no MESMO palácio das fases 01, 14 e 15. Por isso a paleta é a da 15 (as nove
// rampas de Ítaca lidas da fase 01, mais Bronze, Fogo, Vinho e Noite), sem rampa nova: o que muda
// na 16 é a LUZ — amanhecer em vez de noite —, e luz se resolve na cena (cor do renderer e véu),
// não na paleta. O classificador é o da 15, copiado e não importado porque o build-pretendentes
// executa a reconstrução inteira ao ser carregado.
//
// ## O que é quantizado e o que não é
//
// - Props, primeiro plano e a faixa de arquitetura: quantizados. São objetos do palácio, e a pedra
//   e o bronze deles precisam ser os mesmos da 15.
// - O fundo do amanhecer NÃO: é céu, e degradê de céu espremido em quatro passos vira faixa — a
//   regra do projeto desde Ítaca (seção 7 do ESTADO_ATUAL).
//
// ## A lareira sai em duas peças
//
// O briefing pede que o fogo possa ser animado por código depois. A chama vira um sprite próprio
// (`final_hearth_fire`) no MESMO canvas da base, então as duas peças compartilham pivô e se
// sobrepõem exatas com a mesma posição; a base guarda as brasas no lugar da chama, para não
// abrir um buraco quando o fogo baixar.
const path = require('path'), fs = require('fs');
const p = require('./png.js');
const { cutout } = require('./cutout.js');
const { removerXadrez, tirarPoeira, temXadrezPintado } = require('./checker-cut.js');
const { loadPalette, remap } = require('./ramp-map.js');
const { mirrorDouble } = require('./make-tileable.js');
const { check } = require('./seam-test.js');

const ROOT = path.join(__dirname, '..');
const ENV = path.join(ROOT, 'Docs/Environment/Fase16');
const FONTES = path.join(ENV, '_fontes');
const ramps = loadPalette(path.join(ROOT, 'Docs/Environment_Pretendentes/Palette/PRETENDENTES_PALETTE.gpl'));
const UN = 42.857143;

const lum = (r, g, b) => 0.2126 * r + 0.7152 * g + 0.0722 * b;
const sat = (r, g, b) => { const mx = Math.max(r, g, b); return mx ? (mx - Math.min(r, g, b)) / mx : 0; };
const hue = (r, g, b) => {
  const mx = Math.max(r, g, b), mn = Math.min(r, g, b), d = mx - mn;
  if (!d) return 0;
  const h = mx === r ? (((g - b) / d) + 6) % 6 : mx === g ? ((b - r) / d) + 2 : ((r - g) / d) + 4;
  return h * 60;
};

/**
 * O classificador da fase 15 (build-pretendentes.js), com duas opções a mais:
 *
 * - `temVinho` (padrão ligado, como na 15). Desligado no machado e no arco: a madeira escura deles
 *   é marrom-avermelhada a S 0,55–0,7, e com Vinho ligado o poste e o arco saíam cor de estandarte.
 * - `fogoMinS` (padrão 0,50, o da 15). Na lareira a pedra do aro, iluminada pela chama, mede
 *   S 0,40–0,55; a chama mede 0,70 ou mais. Com 0,50 metade do aro virava fogo — sobe para 0,68.
 */
const material = ({ temFogo = false, temBronze = false, temVinho = true, quenteClaro = 'Terra / caminho',
  quenteEscuro = 'Madeira', cinzaVaiPara = 'Pedra fria', fogoMinL = 115, fogoMinS = 0.50 } = {}) => (r, g, b) => {
  const L = lum(r, g, b), S = sat(r, g, b), H = hue(r, g, b);
  if (L < 30) { return 'Contorno'; }
  // A chama vem ANTES do vinho quando a lareira pede: a ponta vermelha do fogo mede H < 14, e
  // com a ordem da 15 ela virava cor de vinho.
  if (temFogo && S >= fogoMinS && (H >= 340 || H < 58) && L > fogoMinL) { return 'Fogo'; }
  if (temVinho && S >= 0.52 && (H >= 340 || H < 14)) { return 'Vinho'; }
  if (temBronze && S >= 0.38 && H >= 28 && H < 58 && L > 60 && L < 215) { return 'Bronze'; }
  if (H >= 190 && H < 260 && S >= 0.12) { return 'Noite'; }
  if (H >= 90 && H < 170 && S >= 0.18) { return 'Folhagem'; }
  if (H >= 58 && H < 90 && S >= 0.18) { return 'Oliva seca'; }
  if (S < 0.14) { return cinzaVaiPara; }
  return L > 140 ? quenteClaro : quenteEscuro;
};

function gravar(rel, img, nota) {
  const destino = path.join(ENV, rel);
  fs.mkdirSync(path.dirname(destino), { recursive: true });
  p.write(destino, img);
  console.log(`  ${rel.padEnd(42)} ${img.width}x${img.height}px = ` +
    `${(img.width / UN).toFixed(2)} x ${(img.height / UN).toFixed(2)} un${nota ? '   ' + nota : ''}`);
}
const fonte = (n) => p.read(path.join(FONTES, n + '.png'));
const apertar = (img) => { const b = p.bounds(img); return b ? p.crop(img, b.x0, b.y0, b.w, b.h) : img; };

function limparSprite(nome) {
  let img = fonte(nome);
  if (temXadrezPintado(img)) {
    const r = removerXadrez(img, 6);
    const d = tirarPoeira(r.img, 20);
    console.log(`    ${nome}: xadrez PINTADO — ${(r.removido * 100).toFixed(1)}% virou alpha`);
    img = d.img;
  } else {
    const r = cutout(img, 18);
    if (r.removed / (img.width * img.height) > 0.02) {
      console.log(`    ${nome}: fundo chapado — ${(100 * r.removed / (img.width * img.height)).toFixed(1)}% removido`);
      img = r.img;
    }
  }
  // Alpha duro: o master pede zero pixel semitransparente.
  for (let i = 3; i < img.data.length; i += 4) { img.data[i] = img.data[i] > 127 ? 255 : 0; }
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

/** Corta o céu de uma faixa por flood-fill a partir do TOPO (mesma receita da 15). */
function cortarCeuDoTopo(img, tol = 24) {
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

console.log('Fase 16 — Final: paleta da casa de Ítaca, luz de amanhecer\n');

// 1. Props narrativos.
const PROPS = [
  // O fio das lâminas é cinza claro: vai para `Terra / caminho`, e não para `Pedra fria`, cujo
  // passo claro é azul (#a0bcc4) — o mesmo engano que azulou as paredes da 15 na primeira passada.
  ['final_axe_post_d', 'Props/final_axe_post.png',
    { temBronze: true, temVinho: false, quenteClaro: 'Terra / caminho', quenteEscuro: 'Madeira', cinzaVaiPara: 'Terra / caminho' }],
  ['final_bow_odysseus_c', 'Props/final_bow_odysseus.png',
    { temBronze: true, temVinho: false, quenteClaro: 'Terra / caminho', quenteEscuro: 'Madeira', cinzaVaiPara: 'Madeira' }],
  // O tear de pesos (`_b`): o de Ítaca, com os pesos de argila presos na base dos fios. O `_a` tem
  // pano e postes certos mas sem pesos, e o `_c` tem moldura fechada embaixo, que é tear de
  // pedal, não grego.
  ['final_loom_penelope_b', 'Props/final_loom_penelope.png',
    { quenteClaro: 'Terra / caminho', quenteEscuro: 'Madeira', cinzaVaiPara: 'Terra / caminho' }],
  ['final_fg_stones_a', 'Foreground/final_fg_stones.png',
    { quenteClaro: 'Terra / caminho', quenteEscuro: 'Madeira', cinzaVaiPara: 'Terra / caminho' }],
  ['final_fg_column_a', 'Foreground/final_fg_column.png',
    { quenteClaro: 'Terra / caminho', quenteEscuro: 'Pedra', cinzaVaiPara: 'Terra / caminho' }],
];
for (const [nome, destino, opts] of PROPS) {
  gravar(destino, remap(limparSprite(nome), ramps, { classify: material(opts) }));
}

// 2. A lareira, em base + chama.
{
  const opts = { temFogo: true, fogoMinL: 100, fogoMinS: 0.68, quenteClaro: 'Terra / caminho', quenteEscuro: 'Pedra',
    cinzaVaiPara: 'Terra / caminho' };
  const limpo = limparSprite('final_hearth_a');
  // A lareira não tem planta: o que mede Oliva ou Folhagem é o miolo amarelo da chama entre as
  // toras, e verde no meio do fogo lê como erro.
  const classe = (r, g, b) => {
    const m = material(opts)(r, g, b);
    return m === 'Oliva seca' || m === 'Folhagem' ? 'Fogo' : m;
  };
  const q = remap(limpo, ramps, { classify: classe });
  const { width: w, height: h } = q;

  // A chama é o conjunto de pixels `Fogo` da FONTE (antes da quantização, onde a classificação
  // é feita) mais o contorno escuro que os cerca por cima. Mede-se na fonte porque depois de
  // quantizada a chama e o barro dos passos baixos de `Fogo` são a mesma cor.
  const ehFogo = new Uint8Array(w * h);
  for (let i = 0; i < w * h; i++) {
    const o = i * 4;
    if (limpo.data[o + 3] && classe(limpo.data[o], limpo.data[o + 1], limpo.data[o + 2]) === 'Fogo') { ehFogo[i] = 1; }
  }
  // O aro de pedra: a primeira linha em que a pedra (não-fogo, opaca) cobre metade da largura.
  let topoDoAro = 0;
  for (let y = 0; y < h; y++) {
    let n = 0;
    for (let x = 0; x < w; x++) { const i = y * w + x; if (q.data[i * 4 + 3] && !ehFogo[i]) { n++; } }
    if (n >= w * 0.5) { topoDoAro = y; break; }
  }
  // Acima do aro só existe chama e ar: a borda vermelha escura da chama (S alto, L baixo) não
  // passa no limiar de claridade de `Fogo`, mas ali não pode ser outra coisa.
  for (let y = 0; y < topoDoAro; y++) for (let x = 0; x < w; x++) {
    const i = y * w + x, o = i * 4;
    const [r, g, b] = [limpo.data[o], limpo.data[o + 1], limpo.data[o + 2]];
    const H = hue(r, g, b);
    if (limpo.data[o + 3] && sat(r, g, b) >= 0.68 && (H >= 340 || H < 58)) { ehFogo[i] = 1; }
  }
  // Só conta como chama o fogo LIGADO ao que sobe acima do aro. A pedra do aro tem pontos de
  // reflexo que medem como chama (S 0,7, claros) e, soltos, viravam uma poeira laranja na camada
  // do fogo — que se mexeria sozinha quando a chama for animada.
  {
    // Dentro do poço o miolo da chama é amarelo-claro (H até 70, S 0,55+) e cairia em Oliva; ligado
    // à chama de cima, ele é chama. O critério frouxo só vale na inundação a partir dela.
    const pareceChama = (k) => {
      const o = k * 4;
      if (!limpo.data[o + 3]) { return false; }
      const [r, g, b] = [limpo.data[o], limpo.data[o + 1], limpo.data[o + 2]];
      const H = hue(r, g, b);
      return sat(r, g, b) >= 0.58 && (H >= 340 || H < 70) && lum(r, g, b) > 70;
    };
    const ligado = new Uint8Array(w * h), pilha = [];
    for (let y = 0; y < topoDoAro; y++) for (let x = 0; x < w; x++) {
      const i = y * w + x;
      if (ehFogo[i]) { ligado[i] = 1; pilha.push(i); }
    }
    while (pilha.length) {
      const k = pilha.pop(), x = k % w, y = (k - x) / w;
      for (let dy = -1; dy <= 1; dy++) for (let dx = -1; dx <= 1; dx++) {
        const nx = x + dx, ny = y + dy;
        if (nx < 0 || ny < 0 || nx >= w || ny >= h) { continue; }
        const nk = ny * w + nx;
        if (ligado[nk] || !(ehFogo[nk] || pareceChama(nk))) { continue; }
        ligado[nk] = 1; pilha.push(nk);
      }
    }
    for (let i = 0; i < w * h; i++) { ehFogo[i] = ligado[i]; }
  }
  // Contorno da chama: pixel de Contorno acima do aro encostado num pixel de fogo.
  const chama = new Uint8Array(w * h);
  for (let y = 0; y < h; y++) for (let x = 0; x < w; x++) {
    const i = y * w + x;
    if (ehFogo[i]) { chama[i] = 1; continue; }
    if (y >= topoDoAro || !q.data[i * 4 + 3]) { continue; }
    const vizinhoFogo = [[1, 0], [-1, 0], [0, 1], [0, -1]].some(([dx, dy]) => {
      const nx = x + dx, ny = y + dy;
      return nx >= 0 && ny >= 0 && nx < w && ny < h && ehFogo[ny * w + nx];
    });
    if (vizinhoFogo) { chama[i] = 1; }
  }
  const base = p.blank(w, h), fogo = p.blank(w, h);
  q.data.copy(base.data);
  const brasa = ramps['Fogo'][3];
  let nChama = 0, nBrasa = 0;
  for (let y = 0; y < h; y++) for (let x = 0; x < w; x++) {
    const i = y * w + x, o = i * 4;
    if (!chama[i]) { continue; }
    nChama++;
    q.data.copy(fogo.data, o, o, o + 4);
    // Acima do aro a chama sai da base (ali só há ar); dentro do aro fica brasa, para o poço
    // não abrir quando o fogo baixar.
    if (y < topoDoAro) { base.data[o + 3] = 0; }
    else { base.data[o] = brasa[0]; base.data[o + 1] = brasa[1]; base.data[o + 2] = brasa[2]; nBrasa++; }
  }
  gravar('Props/final_hearth_base.png', base, `aro na linha ${topoDoAro}; ${nBrasa} px de brasa no poço`);
  gravar('Props/final_hearth_fire.png', fogo, `${nChama} px de chama, mesmo canvas da base`);
}

// 3. Fundo do amanhecer — não quantizado (é céu), espelhado para ladrilhar.
//
//    O espelho ingênuo duplica o SOL: com o período de 35,8 un e parallax 0,92, os dois sóis caíam
//    a 9,8 un um do outro, dentro da mesma tela de 21 un. Apagar o segundo por pintura deixou
//    listras. A saída foi geométrica: recortar a fonte ATÉ A COLUNA DO SOL (x=554, o pixel mais
//    claro da imagem) e espelhar em volta dela. O sol cai na dobra e existe uma vez por período —
//    agora de 25,9 un, maior que a tela — e o halo, que é concêntrico, espelha sem emenda.
{
  const SOL_X = 554;
  const recorte = p.crop(fonte('final_bg_dawn_b'), 0, 0, SOL_X + 1, 288);
  const bg = espelharSePreciso(recorte, 'bg_dawn');
  gravar('Background/final_bg_dawn.png', bg, 'céu: fora da quantização, por regra; sol na dobra do espelho');
}

// 4. Midground: o palácio visto de fora, céu cortado, quantizado como arquitetura.
{
  const pedra = { temFogo: false, temBronze: false, quenteClaro: 'Terra / caminho',
    quenteEscuro: 'Pedra', cinzaVaiPara: 'Terra / caminho' };
  const { img, removido } = cortarCeuDoTopo(fonte('final_mg_palace_a'));
  const q = remap(apertar(img), ramps, { classify: material(pedra) });
  gravar('Midground/final_mg_palace_band.png', espelharSePreciso(q, 'mg_palace'),
    `céu cortado (${(removido * 100).toFixed(0)}%), quantizada`);
}

// 5. Luz do amanhecer, procedural (0 gerações).
//
//    a) véu: o inverso do véu de fogo da 15. Lá a luz esquentava em direção ao trono porque o
//       fogo era dos pretendentes; aqui ela CLAREIA em direção ao fim, onde Penélope espera —
//       ouro pálido de manhã, alpha 0 a 0,16.
//    b) facho: um raio de sol oblíquo entrando pelo alto, alpha máximo 0,32 no centro (0,22 mal aparecia contra a parede clara, medido na prévia), para os
//       vãos da parede do salão reusada. Sprite de 48x256 com o degradê no próprio pixel.
{
  const LARGURA = 256, ALTURA = 8;
  const veu = p.blank(LARGURA, ALTURA);
  const [cr, cg, cb] = ramps['Bronze'][0];               // Bronze Highlight, #f5cd5c
  for (let x = 0; x < LARGURA; x++) {
    const t = x / (LARGURA - 1);
    const a = Math.round(255 * 0.16 * Math.pow(t, 1.3));
    for (let y = 0; y < ALTURA; y++) {
      const o = (y * LARGURA + x) * 4;
      veu.data[o] = cr; veu.data[o + 1] = cg; veu.data[o + 2] = cb; veu.data[o + 3] = a;
    }
  }
  gravar('VFX/final_dawn_gradient.png', veu, 'degradê de alpha por coluna, 0 a 0,16');

  const W = 48, H = 256, facho = p.blank(W, H);
  const [fr, fg, fb] = ramps['Terra / caminho'][0];    // #f5d5a7
  for (let y = 0; y < H; y++) {
    // Inclinado: o centro anda 24 px da base ao topo (o sol vem da esquerda, baixo).
    const centro = W * 0.25 + (W * 0.5) * (1 - y / (H - 1));
    const vertical = Math.min(1, y / (H * 0.35)) * Math.min(1, (H - y) / (H * 0.2));
    for (let x = 0; x < W; x++) {
      const d = Math.abs(x - centro) / (W * 0.25);
      if (d >= 1) { continue; }
      const o = (y * W + x) * 4;
      facho.data[o] = fr; facho.data[o + 1] = fg; facho.data[o + 2] = fb;
      facho.data[o + 3] = Math.round(255 * 0.32 * (1 - d * d) * vertical);
    }
  }
  gravar('VFX/final_dawn_shaft.png', facho, 'raio de sol, alpha até 0,32');
}

console.log('\npronto.');
