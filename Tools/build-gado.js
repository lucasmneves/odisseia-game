// Leva as fontes de Gado do Sol à paleta oficial e corrige o que a geração errou.
//
//   node Tools/build-gado.js
//
// ## O classificador desta fase pode cortar por MATIZ, e é a primeira em muito tempo que pode
//
// Cila e Caribdis, Lestrigões e Mundo dos Mortos são fases dessaturadas, em que quase tudo é
// cinza e o material tinha de ser decidido por luminância. Aqui não: céu azul, pasto
// verde-amarelado, folhagem verde-azulada, calcário dourado, terra marrom e gado branco são
// SEIS famílias de matiz distintas, e o olho — e o classificador — separam sozinhos. É a
// mesma condição que Sereias teve, e pelo mesmo motivo dispensa o tint de perspectiva aérea
// que Lestrigões cobrou.
//
// ## Três correções, e todas por medida
//
// 1. O TEMPLO e a ÁRVORE GRANDE voltaram com o xadrez de transparência PINTADO — 0,0% de
//    pixels transparentes num asset pedido com `no_background: true`. É a terceira e a quarta
//    ocorrência da mesma armadilha nesta rodada, e por isso ela virou `Tools/checker-cut.js`.
// 2. O MAR voltou escuro demais: L 0,18 no topo e 0,29 de média, o que lê como mar de noite
//    numa fase cujo assunto é o sol. É corrigido por fator, não por nova geração.
// 3. O OLIVAL tem uma mancha alaranjada de pôr do sol na ponta direita, que ao ladrilhar
//    apareceria como um clarão repetido. Sai por recorte.
const path = require('path'), fs = require('fs');
const p = require('./png.js');
const { cutout } = require('./cutout.js');
const { removerXadrez, tirarPoeira, temXadrezPintado } = require('./checker-cut.js');
const { loadPalette, remap, remapParaRampa } = require('./ramp-map.js');
// Espelhar e a saida para faixa que nao ladrilha, e as tres faixas desta fase nao ladrilham.
const { mirrorDouble } = require('./make-tileable.js');

const ROOT = path.join(__dirname, '..');
const ENV = path.join(ROOT, 'Docs/Environment_GadoDoSol');
const FONTES = path.join(ENV, '_fontes');
const ramps = loadPalette(path.join(ENV, 'Palette/GADO_PALETTE.gpl'));
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
 * Classificador de Gado do Sol, por MATIZ primeiro.
 *
 * A ordem dos ramos importa e não é arbitrária:
 *
 * - `Gado sagrado` vem ANTES de tudo quando ligado, porque o pelo do boi é creme quase branco
 *   com dourado no dorso — sem o ramo próprio, o creme cairia em `Calcario dourado` e o boi
 *   sairia da mesma cor do templo, perdendo justamente o que o marca como sagrado.
 * - `Pasto` e `Folhagem` se separam em matiz 105: abaixo disso é verde-amarelado (pasto),
 *   acima é verde-azulado (oliveira, cipreste). Medido nos conceitos — o pasto do Conceito A
 *   mede matiz 78 a 88 e a folhagem 95 a 145.
 * - `Terra` só é alcançada por matiz laranja com saturação alta; o calcário dourado é laranja
 *   também, mas MUITO mais claro, então o corte entre os dois é de luminância dentro do ramo
 *   quente.
 */
const materialGado = ({ temGado = false, temAgua = false, temCeu = false,
  quenteClaro = 'Terra' } = {}) => (r, g, b) => {
  const L = lum(r, g, b), S = sat(r, g, b), H = hue(r, g, b);

  if (L < 34) { return 'Contorno'; }
  if (temGado && S < 0.45 && L > 150) { return 'Gado sagrado'; }
  if (temAgua && H >= 175 && H < 260) { return 'Mar'; }
  if (S < 0.14) { return 'Muro seco'; }
  if (H >= 60 && H < 105) { return 'Pasto'; }
  if (H >= 105 && H < 175) { return 'Folhagem'; }
  // O ramo de CÉU é opt-in, e a razão é um erro medido: os menires voltaram AZUIS.
  //
  // A pedra deles é cinza-esverdeada com líquen, o que dá matiz ~185 e saturação logo acima de
  // 0,14 — não cinza o bastante para cair em `Muro seco`, e bem dentro da faixa de matiz do
  // céu. Saíram pintados de azul-celeste, lendo como cascatas de água no meio do pasto.
  //
  // A correção não é mexer no limiar: qualquer limiar que separe pedra com líquen de céu vai
  // errar o outro lado. É lembrar que NENHUM sprite solto desta fase contém céu — céu só existe
  // nas camadas de fundo, e essas não são quantizadas. Um ramo que nunca deveria ser alcançado
  // não pode ficar ligado por padrão.
  if (temCeu && H >= 175 && H < 270) { return 'Ceu'; }
  // Faixa quente e clara. O destino dela é DECLARADO POR ASSET, e essa é a correção mais
  // importante deste classificador.
  //
  // A primeira versão mandava tudo que fosse quente e claro para `Calcario dourado`, na ideia
  // de que "quente e claro = pedra de templo". O resultado: a casca pálida da oliveira, a terra
  // do tileset de grama e as pedras do muro seco saíram TODAS douradas — três materiais
  // diferentes pintados da cor do templo, porque nenhum deles é cinza o bastante para cair em
  // `Muro seco` e todos são quentes.
  //
  // O erro de fundo é querer que um limiar decida uma coisa que o limiar não sabe: casca,
  // terra e mármore ocupam a MESMA região de matiz, saturação e luminância. Quem sabe a
  // diferença é quem chamou. Por isso `quenteClaro` é obrigatório na prática e o padrão é
  // `Terra` — o material mais comum e o mais inofensivo se estiver errado.
  return L > 140 ? quenteClaro : 'Terra';
};

function gravar(destino, img, nota) {
  fs.mkdirSync(path.dirname(destino), { recursive: true });
  p.write(destino, img);
  console.log(`  ${path.relative(ENV, destino).padEnd(44)} ${img.width}x${img.height}px = ` +
    `${(img.width / UN).toFixed(2)} x ${(img.height / UN).toFixed(2)} un${nota ? '   ' + nota : ''}`);
}
const fonte = (n) => p.read(path.join(FONTES, n + '.png'));

/** Multiplica os canais preservando as razões — o mesmo recurso do basalto, ao contrário. */
function ajustarBrilho(img, fator) {
  const out = p.blank(img.width, img.height);
  img.data.copy(out.data);
  for (let i = 0; i < img.width * img.height; i++) {
    const o = i * 4;
    if (out.data[o + 3] === 0) { continue; }
    for (let c = 0; c < 3; c++) { out.data[o + c] = Math.min(255, Math.round(out.data[o + c] * fator)); }
  }
  return out;
}

/** Recorta e devolve só o conteúdo, para o sprite não carregar margem vazia. */
function apertar(img) {
  const b = p.bounds(img);
  return b ? p.crop(img, b.x0, b.y0, b.w, b.h) : img;
}

/**
 * Limpa um sprite solto: tira o xadrez pintado quando houver, senão tira o fundo chapado, e
 * aperta. Cobra em voz alta qual dos dois caminhos usou — é a única forma de saber, sem abrir
 * a imagem, se o modelo respeitou `no_background` naquela geração.
 */
function limparSprite(nome, { peçaUnica = false } = {}) {
  let img = fonte(nome);
  if (temXadrezPintado(img)) {
    const r = removerXadrez(img);
    // A poeira que sobra não é cosmética: são quadradinhos do xadrez CERCADOS pelo contorno do
    // objeto, que o flood-fill de borda não alcança. No templo eles apareceram como sujeira
    // clara em volta das colunas — invisível na miniatura, evidente na cena.
    //
    // `peçaUnica` diz que o asset é UM corpo conectado — um templo, uma árvore. Nesse caso
    // qualquer ilha solta é resto, por definição, e o limiar pode ser generoso sem risco. Num
    // asset com partes legitimamente separadas (menires, um monte de tábuas) o limiar tem de
    // ficar baixo, senão o recorte come uma peça inteira.
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

// ---------------------------------------------------------------- construção

console.log('Gado do Sol — quantização e correções\n');

// 1. Fundos: NÃO quantizados, pela exceção que Ítaca e Sereias já abriram — espremer um
//    degradê de céu em 40 cores o transforma em faixas.
// As colinas sao ESPELHADAS. Medida: emenda 68,2 contra p90 interna de 11,3 — a faixa nao
// ladrilha, e ao repetir deixava um corte vertical duro atravessando o ceu. Espelhada, o
// periodo vai a 31,4 un e a emenda passa a ser, por construcao, invisivel.
//
// E ha um ganho que nao era o objetivo: com 31,4 un de periodo, UMA volta ja cobre as 23,8 un
// que esta camada precisa (58 de curso x 0,10 de parallax + uma tela). O templo da colina, que
// e o elemento mais reconhecivel do fundo, deixa de aparecer repetido na mesma tela.
gravar(path.join(ENV, 'Background/gado_bg_hills.png'), mirrorDouble(fonte('gado_bg_hills')),
  'sem quantizar — é fundo; espelhada para ladrilhar');

// 2. O mar. Fator 1,55: medido, a faixa tem L médio 0,294 e topo 0,176 — mar de noite. O alvo
//    é ~0,46, que é onde o mar do Conceito A vive num dia de sol. Multiplicar preserva o
//    degradê de profundidade (fundo escuro ao longe, claro na beira), que está CERTO; o que
//    estava errado era só o nível.
{
  const claro = ajustarBrilho(fonte('gado_bg_sea'), 1.55);
  let s = 0, n = 0;
  for (let i = 0; i < claro.width * claro.height; i++) {
    const o = i * 4;
    s += lum(claro.data[o], claro.data[o + 1], claro.data[o + 2]) / 255; n++;
  }
  gravar(path.join(ENV, 'Ocean/gado_sea_calm.png'), mirrorDouble(claro),
    `L médio agora ${(s / n).toFixed(2)}, espelhada para ladrilhar`);
}

// 3. O olival. A ponta direita tem uma mancha de pôr do sol que, ao ladrilhar, viraria um
//    clarão repetido a cada 15,7 unidades. Cortada, a faixa fica homogênea e emenda.
{
  const grove = fonte('gado_mid_grove');
  const corte = Math.round(grove.width * 0.78);
  gravar(path.join(ENV, 'Midground/gado_grove_band.png'),
    mirrorDouble(p.crop(grove, 0, 0, corte, grove.height)),
    'ponta cortada (clarão de pôr do sol) e espelhada para ladrilhar');
}

// 4. Sprites soltos. Cada um declara quais materiais tem — dar `temGado` a uma árvore faria a
//    copa clara virar pelo de boi, e dar `temAgua` a um templo faria a sombra fria virar mar.
const SOLTOS = [
  // Só o que é ARQUITETURA DE TEMPLO manda o quente-claro para o calcário dourado.
  ['gado_temple', 'Temple/gado_temple.png', { quenteClaro: 'Calcario dourado' }],
  ['gado_ruined_column', 'Temple/gado_ruined_column.png', { quenteClaro: 'Calcario dourado' }],
  ['gado_altar', 'SacredArea/gado_altar.png', { quenteClaro: 'Calcario dourado' }],

  // Muro e menires são pedra de campo, não de templo: mesma família quente, outra rampa. Se
  // fossem para o calcário, o muro do pasto brilharia como o templo e a fase perderia a
  // hierarquia que faz o templo ser o assunto do último terço.
  ['gado_standing_stones', 'SacredArea/gado_standing_stones.png', { quenteClaro: 'Muro seco' }],
  ['gado_drystone_wall', 'Fields/gado_drystone_wall.png', { quenteClaro: 'Muro seco' }],

  // Vegetação: o quente-claro é CASCA, e casca é terra. Deixá-lo no padrão dourado fez o
  // tronco da oliveira sair da cor do templo.
  ['gado_olive_tree', 'Fields/gado_olive_tree.png', {}],
  ['gado_cypress', 'Fields/gado_cypress.png', {}],
  ['gado_shade_tree_big', 'Fields/gado_shade_tree_big.png', {}],
  ['gado_shrubs_flowers', 'Pasture/gado_shrubs_flowers.png', {}],

  ['gado_cattle_idle', 'SacredCattle/gado_cattle_idle.png', { temGado: true }],
  ['gado_cattle_grazing', 'SacredCattle/gado_cattle_grazing.png', { temGado: true }],
  ['gado_calf', 'SacredCattle/gado_calf.png', { temGado: true }],
];
for (const [nome, destino, opts] of SOLTOS) {
  // Templo e arvores sao corpos unicos; menires e arbustos tem pecas soltas de verdade.
  const umSoCorpo = /temple|shade_tree|olive_tree|cypress|column/.test(nome);
  const limpo = limparSprite(nome, { peçaUnica: umSoCorpo });
  gravar(path.join(ENV, destino), remap(limpo, ramps, { classify: materialGado(opts) }));
}

// 5. Tilesets — e os dois seguem regras DIFERENTES, de propósito.
//
//    A regra do projeto é "rampa única para asset de material único, classificador para asset
//    multi-material" (registrada em build-sereias). A folha de pedra é calcário puro e vai de
//    rampa única. A folha de GRAMA tem dois materiais reais — grama verde em cima e terra
//    embaixo —, e passá-la por rampa única pintaria a grama de marrom. Ela vai pelo
//    classificador, que é justamente o que sabe separar os dois por matiz.
//
//    A terra da folha vem ROSADA do gerador; o ramo quente do classificador a devolve para o
//    marrom da rampa `Terra`.
{
  const grama = p.read(path.join(ENV, 'Gameplay/gado_tiles_grass.png'));
  gravar(path.join(ENV, 'Gameplay/gado_tiles_grass.png'),
    remap(grama, ramps, { classify: materialGado({}) }),
    'classificador — grama e terra são dois materiais');

  const pedra = p.read(path.join(ENV, 'Gameplay/gado_tiles_stone.png'));
  gravar(path.join(ENV, 'Gameplay/gado_tiles_stone.png'),
    remapParaRampa(pedra, ramps, 'Calcario dourado'), 'rampa Calcario dourado');
}

// 6. O degradê da tempestade que chega.
//
//    A primeira montagem fez o escurecimento com QUATRO retângulos de alpha crescente, na ideia
//    de que a sobreposição os fundiria. Não fundiu: cada retângulo tem alpha uniforme, então
//    toda fronteira entre dois vira um degrau visível, e o último terço da fase ficou com
//    quatro painéis translúcidos de borda reta atravessando a tela. Lia como interface, não
//    como clima.
//
//    Um degradê de verdade não sai de blocos empilhados — sai de UMA imagem com o alpha
//    variando por coluna. Custa um SpriteRenderer em vez de quatro, não tem emenda nenhuma, e o
//    Unity a estica sem custo.
//
//    #46506b é o azul-acinzentado frio da tempestade, e não cinza puro: cinza sobre verde dá
//    verde sujo, enquanto o azul frio lê como a LUZ mudando de temperatura.
{
  const LARGURA = 256, ALTURA = 8;
  const grad = p.blank(LARGURA, ALTURA);
  const [cr, cg, cb] = [0x46, 0x50, 0x6b];
  for (let x = 0; x < LARGURA; x++) {
    // Expoente 1,2 e teto 0,60 — e os dois números foram CORRIGIDOS depois de medir na cena.
    //
    // A primeira versão usava t² com teto 0,52, na ideia de que uma tempestade fecha devagar e
    // depois rápido. O problema não era a curva: era que a metade escura dela caía FORA da área
    // jogável. Com o degradê estendido 12 unidades além do fim da fase, no ponto onde o jogador
    // embarca (x=36) o alpha efetivo era 0,057 — invisível. A fase terminava tão ensolarada
    // quanto começou, e a consequência que o briefing pede não acontecia.
    //
    // Com o degradê terminando NO fim da fase e expoente 1,2, o mesmo x=36 mede 0,37 e o
    // embarque acontece sob céu fechado.
    // O degradê sobe até 70% da largura e depois FICA NO TETO.
    //
    // Sem o platô, o véu tinha de terminar exatamente onde a fase termina para o trecho escuro
    // cair na área jogável — e aí a borda direita dele aparecia dentro do quadro como um corte
    // vertical duro entre o escuro e o claro. Com o platô, o véu pode passar bem do fim da fase
    // sem clarear de novo: a rampa acontece toda dentro do percurso e o resto é céu fechado.
    const t = Math.min(1, (x / (LARGURA - 1)) / 0.70);
    const a = Math.round(255 * 0.60 * Math.pow(t, 1.2));
    for (let y = 0; y < ALTURA; y++) {
      const o = (y * LARGURA + x) * 4;
      grad.data[o] = cr; grad.data[o + 1] = cg; grad.data[o + 2] = cb; grad.data[o + 3] = a;
    }
  }
  gravar(path.join(ENV, 'VFX/gado_storm_gradient.png'), grad,
    'degradê de alpha por coluna, 0 a 0,60');
}

console.log('\npronto.');
