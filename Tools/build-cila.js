// Leva as fontes de Cila e Caribdis à paleta oficial e COMPÕE por código o que a geração
// errou três vezes seguidas.
//
//   node Tools/build-cila.js
//
// ## Por que há composição aqui, e não mais geração
//
// A falésia com boca de caverna e o pináculo de rocha voltaram como ALVENARIA em três
// tentativas — blocos cortados, arco de aduelas, degraus empilhados —, inclusive com a
// formulação que corrigiu o mesmo defeito em Sereias ("not built by anyone, no bricks, no
// masonry"). O que aquela formulação não cobre é que as palavras "boca de caverna", "arco" e
// "prateleira de rocha" JÁ SÃO substantivos de arquitetura: o modelo lê estrutura e desenha
// estrutura, e negar depois não desfaz.
//
// A saída é a mesma da casa de Ítaca (ESTADO_ATUAL §4): quando a geração falha sempre no mesmo
// eixo, parar de gerar e construir. `cila_wall_columnar` voltou PERFEITO — basalto colunar de
// verdade, densidade nativa, transparência respeitada —, então a falésia da caverna e o
// pináculo são RECORTES e ESCAVAÇÕES desse mesmo asset. Mesma geologia, custo zero, e sem o
// risco de um quarto lote voltar em blocos.
const path = require('path'), fs = require('fs');
const p = require('./png.js');
const { cutout } = require('./cutout.js');
const { loadPalette, remap, remapParaRampa } = require('./ramp-map.js');
// O removedor de xadrez virou modulo proprio: a mesma armadilha reapareceu no templo e na
// arvore de Gado do Sol, e duplicar a heuristica em dois pipelines faria as duas divergirem.
const { removerXadrez } = require('./checker-cut.js');
const { mirrorDouble } = require('./make-tileable.js');

const ROOT = path.join(__dirname, '..');
const ENV = path.join(ROOT, 'Docs/Environment_CilaCaribdis');
const FONTES = path.join(ENV, '_fontes');
const ramps = loadPalette(path.join(ENV, 'Palette/CILA_PALETTE.gpl'));
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
 * Classificador da fase. Cila e Caribdis é quase toda fria, então cortar por matiz só é seguro
 * para separar ÁGUA (teal) de ROCHA (cinza) — e isso funciona porque a paleta reservou o teal
 * exclusivamente para água. O resto decide por luminância, que é a regra de Lestrigões e de
 * Mundo dos Mortos, fases igualmente dessaturadas.
 *
 * `temMadeira` é o único ramo quente: só os destroços e o navio o ligam.
 */
const materialCila = ({ temMadeira = false, temAgua = false } = {}) => (r, g, b) => {
  const L = lum(r, g, b), S = sat(r, g, b), H = hue(r, g, b);

  if (temMadeira && S >= 0.28 && H >= 12 && H < 55) return 'Madeira do navio';
  if (temAgua && S >= 0.22 && H >= 140 && H < 210) return L > 150 ? 'Espuma' : 'Agua funda';
  if (L > 200 && S < 0.20) return 'Espuma';
  if (L < 30) return 'Contorno';
  return 'Rocha jogavel';
};

function gravar(destino, img, nota) {
  fs.mkdirSync(path.dirname(destino), { recursive: true });
  p.write(destino, img);
  console.log(`  ${path.relative(ENV, destino).padEnd(42)} ${img.width}x${img.height}px = ` +
    `${(img.width / UN).toFixed(2)} x ${(img.height / UN).toFixed(2)} un${nota ? '   ' + nota : ''}`);
}
const fonte = (n) => p.read(path.join(FONTES, n + '.png'));

/**
 * Escurece por FATOR MULTIPLICATIVO, preservando as razões entre os tons e portanto todo o
 * contraste interno do asset.
 *
 * Existe porque quantizar a parede colunar na rampa `Basalto` a destruiu duas vezes. Uma rampa
 * de quatro passos distribui as cores por luminância RELATIVA dentro do asset, então um asset
 * de 256x512 com sombreado rico é sempre espremido nos quatro degraus — e numa rampa escura o
 * resultado não é uma parede escura, é uma mancha preta sem colunas.
 *
 * A medida que resolveu: a parede crua já tem L médio 0,278 contra 0,432 do tileset jogável —
 * ela JÁ era mais escura que o chão, e a quantização não estava corrigindo nada, só apagando.
 * O fator 0,72 leva a parede a L 0,20 e abre a distância para 0,23, que é o que separa o plano
 * de jogo do plano de fundo sem perder um pixel de detalhe.
 *
 * É a mesma exceção que Ítaca e Sereias já abriram para as camadas de fundo, pelo mesmo motivo:
 * espremer um degradê em poucas cores o transforma em faixas.
 */
function escurecer(img, fator) {
  const out = p.blank(img.width, img.height);
  img.data.copy(out.data);
  for (let i = 0; i < img.width * img.height; i++) {
    const o = i * 4;
    if (out.data[o + 3] === 0) { continue; }
    out.data[o] = Math.round(out.data[o] * fator);
    out.data[o + 1] = Math.round(out.data[o + 1] * fator);
    out.data[o + 2] = Math.round(out.data[o + 2] * fator);
  }
  return out;
}
const FATOR_PAREDE = 0.72;

// ---------------------------------------------------------------- escavação

/**
 * Ruído por soma de senos. Não é aleatório: a mesma entrada dá sempre a mesma saída, então o
 * asset é reproduzível e o script pode rodar de novo sem mudar a arte.
 */
function rugosidade(t) {
  return Math.sin(t * 3.1) * 0.55 + Math.sin(t * 7.7 + 1.3) * 0.28 + Math.sin(t * 17.3 + 0.7) * 0.17;
}

/**
 * Escava um vão irregular na rocha, em DOIS PASSOS SECOS.
 *
 * A primeira versão usava um anel de transição com degradê, na ideia de que a borda suave
 * leria como profundidade. Leu como borrão: em pixel art um degradê de dezenas de tons sobre
 * uma silhueta é exatamente o "blur" que a direção de arte do projeto proíbe, e a caverna saiu
 * como uma mancha de fuligem colada na parede, sem forma.
 *
 * O que dá profundidade em pixel art é o CONTRASTE entre dois valores chapados com uma borda
 * irregular, não a interpolação entre eles. Então: miolo em `Basalto Profunda`, aro fino de um
 * passo acima, e o contorno ondulando com o ângulo — a irregularidade fica visível justamente
 * porque a borda é dura.
 */
function escavarCaverna(img, cx, cy, rx, ry) {
  const out = p.blank(img.width, img.height);
  img.data.copy(out.data);
  const MIOLO = [0x09, 0x09, 0x09];                      // Basalto Profunda
  const ARO = [0x1d, 0x1e, 0x22];                        // Basalto Sombra
  for (let y = 0; y < img.height; y++) {
    for (let x = 0; x < img.width; x++) {
      const o = (y * img.width + x) * 4;
      if (out.data[o + 3] === 0) { continue; }
      const dx = (x - cx) / rx, dy = (y - cy) / ry;
      const ang = Math.atan2(dy, dx);
      // A borda ondula com o ângulo: raio efetivo entre 0,86 e 1,14 do nominal.
      const limite = 1 + rugosidade(ang * 2.4) * 0.14;
      const d = Math.hypot(dx, dy);
      const cor = d < limite * 0.90 ? MIOLO : d < limite ? ARO : null;
      if (cor === null) { continue; }
      out.data[o] = cor[0]; out.data[o + 1] = cor[1]; out.data[o + 2] = cor[2]; out.data[o + 3] = 255;
    }
  }
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

console.log('Cila e Caribdis — quantização e composição\n');

// O xadrez sai ANTES de tudo: o pináculo e a falésia da caverna são recortes desta mesma
// imagem, então limpar aqui limpa os três de uma vez.
const parede = (() => {
  const { img, removido } = removerXadrez(fonte('cila_wall_columnar'));
  console.log(`  xadrez pintado removido da parede: ${(removido * 100).toFixed(1)}% do quadro\n`);
  return img;
})();

// 1. A parede colunar, o asset de identidade da fase. Rampa única: é um material só, e o
//    classificador inventaria material que não existe (regra registrada em build-sereias).
gravar(path.join(ENV, 'Midground/cila_wall_columnar.png'),
  escurecer(parede, FATOR_PAREDE), 'escurecida por fator, sem quantizar');

// 2. O pináculo: uma FATIA da mesma parede. A geração devolveu uma chaminé de tijolos duas
//    vezes; a fatia tem a geologia certa de graça.
{
  const b = p.bounds(parede);
  // A fatia sai do lado direito, onde as colunas são mais altas e o topo mais irregular — no
  // lado esquerdo a parede desce em degrau e leria como rampa, não como pináculo.
  const largura = 96;
  const x0 = b.x0 + b.w - largura - 12;
  gravar(path.join(ENV, 'Rocks/cila_crag_stack.png'),
    escurecer(p.crop(parede, x0, b.y0, largura, b.h), FATOR_PAREDE), 'fatia da parede');
}

// 3. A falésia de Cila: a mesma parede com um vão escavado. O vão fica na METADE DE CIMA
//    porque é de lá que a criatura desce — a geometria da cena põe a cabeça do marcador em
//    y=4,6 e o chão em y=−2, ou seja o vão tem de estar acima da linha de jogo.
{
  const cliff = escavarCaverna(parede, parede.width * 0.52, parede.height * 0.30,
    parede.width * 0.34, parede.height * 0.15);
  gravar(path.join(ENV, 'Scylla/cila_cave_cliff.png'),
    escurecer(cliff, FATOR_PAREDE), 'parede escavada');
}

// 4. Caribdis. Veio opaca apesar de `no_background` — a armadilha já documentada: "visto de
//    cima num ângulo raso" é linguagem de enquadramento.
//
//    E aqui o `cutout` NÃO serve, o que é uma exceção que vale registrar: ele é flood-fill de
//    borda por COR, e neste asset a cor do fundo (preto) é a mesma cor da garganta do funil e
//    dos anéis fundos. O flood-fill vazou para dentro pelos anéis escuros e comeu 64% da
//    imagem — o redemoinho saiu rendado.
//
//    O que separa fundo de objeto aqui não é cor, é GEOMETRIA: o redemoinho é uma elipse e o
//    fundo é o que sobra fora dela. Então a máscara é elíptica, com a mesma borda irregular
//    das outras peças, e só o preto que está FORA de 80% do raio é removido — o de dentro é a
//    garganta, que é o assunto do asset.
{
  const src = fonte('cila_charybdis_funnel');
  const out = p.blank(src.width, src.height);
  src.data.copy(out.data);
  const cx = src.width / 2, cy = src.height / 2;
  const rx = src.width / 2, ry = src.height / 2;
  let removidos = 0;
  for (let y = 0; y < src.height; y++) {
    for (let x = 0; x < src.width; x++) {
      const o = (y * src.width + x) * 4;
      const dx = (x - cx) / rx, dy = (y - cy) / ry;
      const d = Math.hypot(dx, dy);
      const limite = 1 + rugosidade(Math.atan2(dy, dx) * 3.1) * 0.05;
      const L = lum(out.data[o], out.data[o + 1], out.data[o + 2]) / 255;
      if (d > limite || (d > limite * 0.80 && L < 0.12)) { out.data[o + 3] = 0; removidos++; }
    }
  }
  const b = p.bounds(out);
  gravar(path.join(ENV, 'Charybdis/cila_charybdis_funnel.png'),
    remap(p.crop(out, b.x0, b.y0, b.w, b.h), ramps, { classify: materialCila({ temAgua: true }) }),
    `máscara elíptica, ${(100 * removidos / (src.width * src.height)).toFixed(0)}% removido`);
}

// 5. Fundos: NÃO quantizados. Mesma exceção de Ítaca e das Sereias — espremer um degradê de
//    tempestade em 40 cores o transforma em faixas.
gravar(path.join(ENV, 'Background/cila_bg_walls_far.png'), fonte('cila_bg_walls_far'),
  'sem quantizar — é fundo');

// 6. A faixa de mar. O céu dela é cortado para alpha porque SÓ UMA camada de fundo pode
//    carregar céu (lição das Sereias: duas empilhadas produzem emendas horizontais duras
//    atravessando a tela). Aqui quem carrega o céu é `cila_bg_walls_far`.
{
  const { img, removido } = cortarCeuDoTopo(fonte('cila_bg_sea_churn'), 34);
  const b = p.bounds(img);
  // Espelhada: medida, a faixa tem emenda 228,4 contra p90 interna de 79,4 — de longe a pior
  // das cinco faixas das duas fases. Ao repetir, ela deixava um corte vertical no meio do mar.
  // (`cila_bg_walls_far` é a única que ladrilha nativamente, com emenda 33,2 contra p90 54,7, e
  // por isso não é espelhada.)
  gravar(path.join(ENV, 'Ocean/cila_sea_churn.png'),
    mirrorDouble(p.crop(img, b.x0, b.y0, b.w, b.h)),
    `céu cortado (${(removido * 100).toFixed(0)}% do quadro) e espelhada para ladrilhar`);
}

// 7. Destroços — os únicos assets quentes da fase, e por isso os únicos com `temMadeira`.
//
//    Sem `enclosed`: as costelas do naufrágio são um objeto VAZADO de propósito — o que está
//    entre elas é fundo legítimo alcançável da borda, e a segunda passada só teria como
//    alcançar os pixels escuros de dentro da própria madeira molhada, comendo o sombreado.
for (const [nome, destino] of [
  ['cila_wreck_ribs', 'Shipwrecks/cila_wreck_ribs.png'],
  ['cila_debris_timbers', 'Props/cila_debris_timbers.png'],
]) {
  const { img } = cutout(fonte(nome), 20);
  const b = p.bounds(img);
  gravar(path.join(ENV, destino),
    remap(p.crop(img, b.x0, b.y0, b.w, b.h), ramps, { classify: materialCila({ temMadeira: true }) }));
}

// 8. O tileset jogável. Rampa "Rocha jogavel" e não o classificador: a folha voltou verde-menta
//    saturada, e o classificador leria aquele verde como vegetação que a fase não tem. A rampa
//    única força os quatro passos de rocha de valor MÉDIO — que é a correção medida no
//    cabeçalho da paleta e o motivo de o chão se separar da falésia de fundo.
{
  const folha = p.read(path.join(ENV, 'Gameplay/cila_tiles_rock.png'));
  gravar(path.join(ENV, 'Gameplay/cila_tiles_rock.png'),
    remapParaRampa(folha, ramps, 'Rocha jogavel'), 'rampa Rocha jogavel');
}

console.log('\npronto.');
