// Leva as fontes de Calipso à paleta oficial e corrige o que a geração errou.
//
//   node Tools/build-calipso.js
//
// ## A falésia só saiu certa quando parei de NEGAR alvenaria e passei a NOMEAR a geologia
//
// A primeira tentativa usou a formulação que já está registrada no projeto — "not built by
// anyone, no bricks, no masonry" — e voltou em fiadas de blocos retangulares, como em Cila e
// Caribdis. A segunda funcionou com o oposto: descrever POSITIVAMENTE a forma natural, e
// insistir na propriedade que a alvenaria não tem — **curva**. "worn into soft rounded scallops
// and shallow scooped hollows, every edge CURVED and bulging, no flat surface anywhere and no
// straight edge anywhere".
//
// É a mesma razão pela qual `cila_wall_columnar` tinha saído perfeito de primeira: aquele
// prompt nomeava "basalto colunar hexagonal", uma estrutura natural específica. Negar um
// conceito deixa o modelo escolher o que pôr no lugar; nomear a alternativa não deixa.
//
// ## O xadrez pintado continua aparecendo
//
// Palácio e falésia voltaram com o xadrez de transparência DESENHADO, apesar de
// `no_background: true`. É a quinta e a sexta ocorrência; `Tools/checker-cut.js` cobra e corta.
const path = require('path'), fs = require('fs');
const p = require('./png.js');
const { cutout } = require('./cutout.js');
const { removerXadrez, tirarPoeira, temXadrezPintado } = require('./checker-cut.js');
const { loadPalette, remap, remapParaRampa } = require('./ramp-map.js');
const { mirrorDouble } = require('./make-tileable.js');

const ROOT = path.join(__dirname, '..');
const ENV = path.join(ROOT, 'Docs/Environment_Calipso');
const FONTES = path.join(ENV, '_fontes');
const ramps = loadPalette(path.join(ENV, 'Palette/CALIPSO_PALETTE.gpl'));
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
 * Classificador de Calipso.
 *
 * A fase tem oito famílias próprias e poderia cortar quase tudo por matiz, mas duas escolhas
 * aqui vêm de erro medido em Gado do Sol e estão feitas de propósito:
 *
 * - **O ramo de água é opt-in.** Em Gado do Sol o ramo de céu ficou ligado por padrão e pintou
 *   os menires de azul, porque pedra com líquen mede matiz ~185. Aqui a água ocupa a mesma
 *   faixa, e a `Marmore` do palácio é cinza-azulada: só quem tem água de fato liga o ramo.
 * - **O destino do quente-claro é declarado por asset.** Areia, mármore iluminado e arenito
 *   ocre ocupam a mesma região de matiz e luminância — nenhum limiar os separa, e quem sabe a
 *   diferença é quem chamou.
 */
const materialCalipso = ({ temAgua = false, quenteClaro = 'Areia', verdeFundo = false,
  cinzaVaiPara = 'Marmore' } = {}) => (r, g, b) => {
  const L = lum(r, g, b), S = sat(r, g, b), H = hue(r, g, b);

  if (L < 32) { return 'Contorno'; }
  // Espuma antes de tudo: branco quase puro num asset que tem agua e agua batendo, nao pedra.
  // Sem este ramo a queda d'agua caiu em `Marmore` e a cachoeira saiu cinza.
  if (temAgua && L > 200 && S < 0.20) { return 'Agua rasa'; }
  if (temAgua && S >= 0.18 && H >= 160 && H < 260) { return L > 150 ? 'Agua rasa' : 'Agua funda'; }
  // Verde: a copa iluminada é amarelo-esverdeada (~80) e a mata funda é azul-esverdeada (~150).
  if (H >= 60 && H < 115) { return 'Folhagem clara'; }
  if (H >= 115 && H < 200 && S >= 0.18) { return verdeFundo ? 'Folhagem funda' : 'Folhagem clara'; }
  // Cinza de baixa saturacao NAO e sempre pedra, e presumir que era custou uma arvore.
  //
  // A casca da arvore antiga e palida e quase acromatica, entao ela caiu em `Marmore` e o
  // tronco saiu de MARMORE BRANCO. E o mesmo erro que em Gado do Sol pintou os menires de
  // azul: um ramo que resolve um material esta resolvendo tambem todos os outros que por
  // acaso caem na mesma medida. Quem sabe se o cinza daquele asset e pedra ou casca e quem
  // chamou, entao o destino e declarado por asset.
  if (S < 0.16) { return cinzaVaiPara; }
  if (H >= 200) { return 'Marmore'; }
  return L > 130 ? quenteClaro : 'Madeira';
};

function gravar(destino, img, nota) {
  fs.mkdirSync(path.dirname(destino), { recursive: true });
  p.write(destino, img);
  console.log(`  ${path.relative(ENV, destino).padEnd(44)} ${img.width}x${img.height}px = ` +
    `${(img.width / UN).toFixed(2)} x ${(img.height / UN).toFixed(2)} un${nota ? '   ' + nota : ''}`);
}
const fonte = (n) => p.read(path.join(FONTES, n + '.png'));
const apertar = (img) => { const b = p.bounds(img); return b ? p.crop(img, b.x0, b.y0, b.w, b.h) : img; };

/**
 * Limpa um sprite solto e diz em voz alta por qual caminho foi. É a única forma de saber, sem
 * abrir a imagem, se o modelo respeitou `no_background` naquela geração — e a diferença não se
 * vê no visualizador, que desenha alpha real igualzinho a xadrez pintado.
 */
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

/** Espelha e diz se a emenda melhorou, para a decisão ficar registrada e não presumida. */
const { check } = require('./seam-test.js');
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

// ---------------------------------------------------------------- construção

console.log('Calipso — quantização e correções\n');

// 1. Fundos: NÃO quantizados, pela exceção que Ítaca e Sereias já abriram. O céu de Calipso é
//    um degradê creme-dourado de fim de tarde, e espremê-lo em quatro passos o vira faixas.
gravar(path.join(ENV, 'Background/calipso_bg_sea_horizon.png'),
  espelharSePreciso(fonte('calipso_bg_sea_horizon'), 'bg_sea_horizon'), 'sem quantizar — é fundo');

gravar(path.join(ENV, 'Midground/calipso_forest_band.png'),
  espelharSePreciso(fonte('calipso_bg_forest_band'), 'forest_band'), 'sem quantizar — é fundo');

gravar(path.join(ENV, 'Ocean/calipso_foam_line.png'),
  espelharSePreciso(fonte('calipso_foam_line'), 'foam_line'));

// 2. Sprites soltos.
const SOLTOS = [
  // Arquitetura: o quente-claro é mármore iluminado, não areia.
  ['calipso_palace', 'Palace/calipso_palace.png', { quenteClaro: 'Marmore', peçaUnica: true }],
  ['calipso_fountain', 'Garden/calipso_fountain.png', { quenteClaro: 'Marmore', temAgua: true }],
  ['calipso_bench_ruin', 'Props/calipso_bench_ruin.png', { quenteClaro: 'Marmore' }],

  // A falésia é o único asset em que o quente-claro é ARENITO. Mandá-lo para `Areia` faria a
  // rocha ler como um banco de areia vertical.
  ['calipso_cliff_waterfall', 'Waterfall/calipso_cliff_waterfall.png',
    { quenteClaro: 'Rocha ocre', temAgua: true, peçaUnica: true }],

  // Vegetação: `verdeFundo` liga a mata azul-esverdeada, que é o que dá volume às copas.
  // `cinzaVaiPara: 'Madeira'` — o cinza destes assets e CASCA, nao pedra.
  ['calipso_tree_ancient', 'Forest/calipso_tree_ancient.png', { verdeFundo: true, peçaUnica: true, cinzaVaiPara: 'Madeira', quenteClaro: 'Madeira' }],
  ['calipso_cypress', 'Forest/calipso_cypress.png', { verdeFundo: true, peçaUnica: true, cinzaVaiPara: 'Madeira', quenteClaro: 'Madeira' }],
  ['calipso_ferns_foreground', 'Foreground/calipso_ferns.png', { verdeFundo: true, cinzaVaiPara: 'Madeira' }],
  ['calipso_garden_flowers', 'Garden/calipso_garden_flowers.png', {}],

  ['calipso_shore_rocks', 'Beach/calipso_shore_rocks.png', { quenteClaro: 'Marmore' }],

  // A jangada. Quente-claro vai para `Madeira` — é madeira RECÉM-CORTADA, e o ponto dela é
  // parecer material novo num lugar de mármore e folhagem.
  ['calipso_raft', 'Special/calipso_raft.png', { quenteClaro: 'Madeira', cinzaVaiPara: 'Areia' }],
];
for (const [nome, destino, opts] of SOLTOS) {
  const limpo = limparSprite(nome, { peçaUnica: !!opts.peçaUnica });
  gravar(path.join(ENV, destino), remap(limpo, ramps, { classify: materialCalipso(opts) }));
}

// 3. Tilesets.
//
//    A areia é material único e vai de rampa única. A folha da FLORESTA tem dois materiais —
//    musgo verde em cima e terra embaixo — e por isso vai pelo classificador, com `verdeFundo`
//    ligado: o chão de floresta densa é escuro e frio, e a rampa `Folhagem funda` é exatamente
//    isso. A paleta não tem rampa de terra marrom, e a ausência é decidida: sob dossel fechado
//    a terra não lê marrom, lê verde-preta.
{
  const areia = p.read(path.join(ENV, 'Gameplay/calipso_tiles_sand.png'));
  gravar(path.join(ENV, 'Gameplay/calipso_tiles_sand.png'),
    remapParaRampa(areia, ramps, 'Areia'), 'rampa Areia');

  const mato = p.read(path.join(ENV, 'Gameplay/calipso_tiles_grass.png'));
  gravar(path.join(ENV, 'Gameplay/calipso_tiles_grass.png'),
    // quenteClaro Madeira: a terra sob o dossel e MARRON, e no padrao Areia ela saia creme,
    // lendo como praia dentro da floresta.
    remap(mato, ramps, { classify: materialCalipso({ verdeFundo: true, quenteClaro: 'Madeira', cinzaVaiPara: 'Madeira' }) }),
    'classificador — musgo e terra são dois materiais');
}

// 4. O degradê do fim de tarde.
//
//    Mesma ferramenta do véu de tempestade de Gado do Sol, cor OPOSTA — e a oposição é a
//    decisão. Lá o último terço fecha em azul-acinzentado frio porque a consequência é punição;
//    aqui ele esquenta em âmbar porque a consequência é PARTIDA, e o que a fase precisa dizer no
//    fim não é ameaça, é o sol baixando sobre uma ilha da qual se vai embora.
//
//    Sobe até 70% da largura e fica no teto, para o véu poder passar do fim da fase sem que a
//    borda direita dele apareça dentro do quadro — foi o que aconteceu na primeira montagem de
//    Gado do Sol. O teto é 0,42 e não 0,60: âmbar escurece menos que azul de tempestade, e a
//    fase não pode terminar difícil de ler — nem embarrada, que foi o primeiro resultado.
{
  const LARGURA = 256, ALTURA = 8;
  const grad = p.blank(LARGURA, ALTURA);
  const [cr, cg, cb] = [0xd8, 0x8b, 0x3a];             // âmbar de sol baixo
  for (let x = 0; x < LARGURA; x++) {
    const t = Math.min(1, (x / (LARGURA - 1)) / 0.70);
    // Teto 0,26 e nao 0,42. Medido na cena: ambar a 0,42 sobre o azul profundo do mar nao
    // esquenta, EMBARRA — as duas cores somam num marrom de lama e o oceano, que e a coisa
    // que esta fase precisa manter bonita ate o ultimo quadro, virava barro. Luz quente sobre
    // agua fria pede mao leve.
    const a = Math.round(255 * 0.26 * Math.pow(t, 1.2));
    for (let y = 0; y < ALTURA; y++) {
      const o = (y * LARGURA + x) * 4;
      grad.data[o] = cr; grad.data[o + 1] = cg; grad.data[o + 2] = cb; grad.data[o + 3] = a;
    }
  }
  gravar(path.join(ENV, 'VFX/calipso_dusk_gradient.png'), grad, 'degradê de alpha por coluna, 0 a 0,26');
}

console.log('\npronto.');
