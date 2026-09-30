// Quantiza as fontes das Sereias para a paleta oficial e as leva aos grupos finais.
//
//   node Tools/build-sereias.js
//
// A fase tem SEIS familias de matiz proprias, e por isso o classificador aqui pode cortar por
// MATIZ com seguranca — o oposto de Lestrigoes e de Mortos, onde quase tudo era cinza e a
// decisao tinha de ser so por luminancia.
//
// Duas excecoes conscientes, e as duas seguem regra ja registrada no projeto:
//
//   1. As camadas de FUNDO nao sao quantizadas. O ceu do conceito A e um degrade dourado com
//      sol baixo, e espremer isso em 36 cores o transforma em faixas. Itaca ja abriu essa
//      excecao para os fundos gerados, e o motivo e o mesmo.
//   2. Assets de UM material so vao por `remapParaRampa`, nao pelo classificador. Rampa unica
//      num asset multi-material achata tudo (foi o que aconteceu com os pedregulhos de
//      Ciclopes), e classificador num asset de material unico inventa material que nao existe.
const path = require('path'), fs = require('fs');
const p = require('./png.js');
const { loadPalette, remap, remapParaRampa } = require('./ramp-map.js');

const ROOT = path.join(__dirname, '..');
const ENV = path.join(ROOT, 'Docs/Environment_Sereias');
const ramps = loadPalette(path.join(ENV, 'Palette/SEREIAS_PALETTE.gpl'));
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
 * O corte agua/espuma NAO e por saturacao: medidas, a espuma media `afdeca` (S 0,21) e a agua
 * clara `9dd5c5` (S 0,26) sao quase iguais, e qualquer limiar ali erra metade dos pixels. O que
 * separa os dois de verdade e a LUMINANCIA — espuma e agua que virou branco.
 */
const materialSereias = ({ temMadeira = true, temAgua = false } = {}) => (r, g, b) => {
  const L = lum(r, g, b), S = sat(r, g, b), H = hue(r, g, b);
  if (L < 24 || (L < 46 && S < 0.20)) return 'Contorno';

  // `temAgua` e FALSO por padrao, e isso e o ponto. Nenhum asset construido desta fase contem
  // agua: o mar e a espuma sao faixas proprias, e o fundo nao e quantizado. Com o ramo ligado,
  // as juntas cinza-esverdeadas do tile de rocha caiam em "Agua rasa" e o chao saia com
  // argamassa TURQUESA entre as pedras.
  if (temAgua && H >= 145 && H < 200) return L >= 205 ? 'Espuma' : L >= 128 ? 'Agua rasa' : 'Agua funda';
  if (H >= 200 && H < 255 && S >= 0.15) return 'Ceu';
  if (H >= 50 && H < 145 && S >= 0.20) return 'Vegetacao';
  if (S < 0.10 && L >= 200) return 'Espuma';
  // Madeira e areia se sobrepoem em matiz — foi o erro das anforas de Circe, que caiam nos
  // marrons frios e liam arroxeadas. Aqui a separacao e por matiz E saturacao juntos: a
  // madeira do naufragio e mais vermelha (H < 36) e mais saturada que o calcario.
  if (temMadeira && H < 36 && S >= 0.30 && L < 205) return 'Madeira naufragio';
  return 'Areia e calcario';
};

/** Faixa que ladrilha lateralmente: espelhar com periodo 2W-2 evita repetir a coluna da borda. */
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

/**
 * Torna transparente o fundo PRETO chapado que o modelo pintou atras do penhasco.
 *
 * O `no_background` estava desligado nesse asset de proposito (faixa de terreno), e o modelo
 * respondeu pintando o vazio de preto em vez de continuar a rocha — a mesma armadilha ja
 * registrada: pedir "vazio" faz ele DESENHAR o vazio.
 *
 * A varredura entra pelas bordas e so anda por pixels escuros vizinhos. Um limiar global
 * apagaria tambem as fendas escuras DENTRO da pedra, que sao o que da relevo a faixa.
 */
function fundoPretoParaAlpha(img, limiar = 40) {
  return fundoChapadoParaAlpha(img, i =>
    img.data[i] <= limiar && img.data[i + 1] <= limiar && img.data[i + 2] <= limiar);
}

/**
 * Recorta o CÉU chapado de uma camada que vai por cima de outra.
 *
 * Só uma camada de fundo pode carregar céu. `bg_sea` e `bg_islets` vieram as duas com o seu, e
 * empilhadas produziram três barras horizontais de cor diferente atravessando o quadro — cada
 * uma com uma emenda dura, exatamente o defeito que a fase estava tentando não ter. Sem o céu,
 * os ilhéus deixam de ser uma faixa e passam a ser o que deviam: silhuetas sobre o mar.
 *
 * A tolerância é generosa porque o céu gerado tem nuvens: elas fazem parte do que sai fora.
 */
function cortarCeuParaAlpha(img, tolerancia = 46) {
  const ref = [img.data[0], img.data[1], img.data[2]];
  return fundoChapadoParaAlpha(img, i =>
    Math.abs(img.data[i] - ref[0]) + Math.abs(img.data[i + 1] - ref[1]) +
    Math.abs(img.data[i + 2] - ref[2]) <= tolerancia * 3);
}

/** Varredura a partir das bordas: só apaga o que está conectado ao lado de fora. */
function fundoChapadoParaAlpha(img, ehFundo) {
  const W = img.width, H = img.height;
  const escuro = ehFundo;
  const visto = new Uint8Array(W * H);
  const fila = [];
  for (let x = 0; x < W; x++) { fila.push([x, 0], [x, H - 1]); }
  for (let y = 0; y < H; y++) { fila.push([0, y], [W - 1, y]); }

  while (fila.length) {
    const [x, y] = fila.pop();
    if (x < 0 || y < 0 || x >= W || y >= H) { continue; }
    const c = y * W + x;
    if (visto[c]) { continue; }
    const i = c * 4;
    if (!escuro(i)) { continue; }
    visto[c] = 1;
    img.data[i + 3] = 0;
    fila.push([x + 1, y], [x - 1, y], [x, y + 1], [x, y - 1]);
  }
  return img;
}

/**
 * Corta a MARGEM ESCURA que o gerador desenha acima de um corte de terreno.
 *
 * `cortarCeu` não serve para isso e a razão é medida: a margem do tile de areia não é chapada,
 * é escura *com salpicos* — a linha 0 tem #816f63 e #ae957f misturados, e a diferença entre os
 * dois (111) passa longe da tolerância de 24. Ela ficava, e no jogo virava uma barra escura de
 * 1,07 un atravessando a tela logo abaixo dos pés do jogador.
 *
 * O que separa margem de superfície é a LUMINÂNCIA da linha, não a uniformidade dela: 0,45 na
 * margem contra 0,85 na areia iluminada. O limiar de 0,62 cai no meio desse vão nos dois tiles.
 */
function cortarTopoEscuro(img, limiar = 0.62) {
  const mediaDaLinha = y => {
    let s = 0;
    for (let x = 0; x < img.width; x++) {
      const o = (y * img.width + x) * 4;
      s += (0.2126 * img.data[o] + 0.7152 * img.data[o + 1] + 0.0722 * img.data[o + 2]) / 255;
    }
    return s / img.width;
  };
  let y = 0;
  while (y < img.height && mediaDaLinha(y) < limiar) { y++; }
  // Guarda: se o corte comer mais de um terço do tile, a premissa está errada e é melhor
  // devolver a arte intacta do que entregar um tile mutilado sem ninguém perceber.
  if (y === 0 || y > img.height / 3) { return img; }
  return p.crop(img, 0, y, img.width, img.height - y);
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

/**
 * Corta o frontao triangular do santuario.
 *
 * O prompt pedia "uma verga reta, sem telhado triangular" e o modelo desenhou um fronton
 * classico assim mesmo — o mesmo tipo de teimosia das ameias medievais na torre de Lestrigoes.
 * A regra do projeto vale de novo: defeito LOCAL se corrige com recorte, nao com nova geracao.
 *
 * O corte e medido, nao chutado: a silhueta cresce de 9 px a 188 px enquanto o triangulo desce,
 * e estabiliza em 168 px quando comeca o corpo reto. A linha e onde a largura para de crescer.
 */
function cortarFrontao(img) {
  const largura = y => {
    let n = 0;
    for (let x = 0; x < img.width; x++) { if (img.data[(y * img.width + x) * 4 + 3] > 128) n++; }
    return n;
  };
  let pico = 0, yPico = 0;
  for (let y = 0; y < Math.floor(img.height * 0.4); y++) {
    const w = largura(y);
    if (w > pico) { pico = w; yPico = y; }
  }
  if (pico === 0) { return img; }
  // Alguns pixels abaixo do pico: a cimalha ainda pertence ao telhado.
  const corte = Math.min(img.height - 1, yPico + 4);
  return p.crop(img, 0, corte, img.width, img.height - corte);
}

const PLANO = [
  // grupo, fonte, nome, estrategia
  // Os 66 px do topo saem fora: medida, essa faixa e uma nuvem MARROM-ESCURA (L 0,36) que le
  // como tempestade — o oposto exato do dia claro e convidativo que a fase inteira depende de
  // ter. O ceu util comeca em #e6debf, na linha 66.
  ['Background', 'sereias_bg_sea', 'bg_sea', { cru: true, cortarTopoPx: 66 }],
  // Sem ceu: ele e recortado para alpha e a camada passa a se sobrepor ao bg_sea em vez de
  // empilhar mais uma faixa. Ver cortarCeuParaAlpha().
  ['Background', 'sereias_bg_islets', 'bg_islets', { cru: true, ceuParaAlpha: true }],

  // Nao e uma parede de penhasco: e uma FAIXA DE LAJES sobrepostas, que e o que o modelo
  // desenhou de fato. Com o fundo preto virando alpha ela serve de meio de campo com o mar
  // aparecendo por tras, que e melhor do que a parede opaca que eu tinha pedido.
  ['Midground', 'sereias_mid_cliff', 'rock_ledges', { classe: materialSereias({ temMadeira: false }), fundoPreto: true, espelho: true }],

  ['Gameplay', 'sereias_tiles_sand', 'tiles_sand', { rampa: 'Areia e calcario', cortarCeu: true, cortarMargem: true, espelho: true }],
  ['Gameplay', 'sereias_tiles_rock', 'tiles_rock', { classe: materialSereias({ temMadeira: false }), cortarCeu: true, cortarMargem: true, espelho: true }],

  // water_surface NAO esta aqui: a geracao veio com riscos VERTICAIS, que leem como cachoeira
  // e nao como mar parado. A faixa e construida por codigo em build-sereias-water.js.
  ['Ocean', 'sereias_foam_line', 'foam_line', { rampa: 'Espuma', espelho: true }],

  ['Beach', 'sereias_shore_stones', 'shore_stones', { classe: materialSereias({ temMadeira: false }) }],
  ['Beach', 'sereias_sea_grass', 'sea_grass', { classe: materialSereias({ temMadeira: false }) }],

  ['Rocks', 'sereias_rock_spire', 'rock_spire', { classe: materialSereias({ temMadeira: false }) }],
  ['Rocks', 'sereias_boulders', 'boulders', { classe: materialSereias({ temMadeira: false }) }],

  ['Ruins', 'sereias_shrine', 'shrine', { classe: materialSereias({ temMadeira: false }), frontao: true }],
  ['Ruins', 'sereias_fallen_column', 'fallen_column', { classe: materialSereias({ temMadeira: false }) }],

  ['Shipwrecks', 'sereias_wreck_hull', 'wreck_hull', { classe: materialSereias() }],
  ['Shipwrecks', 'sereias_wreck_ribs', 'wreck_ribs', { classe: materialSereias() }],
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

console.log('asset'.padEnd(16) + 'grupo'.padEnd(12) + 'tamanho'.padEnd(12) + 'unidades'.padEnd(14) + 'antes -> depois');
let faltando = 0;
for (const [grupo, fonte, nome, est] of PLANO) {
  const entrada = path.join(ENV, '_fontes', fonte + '.png');
  if (!fs.existsSync(entrada)) { console.log('  FALTA ' + fonte); faltando++; continue; }

  let img = p.read(entrada);
  const antes = distancia(img);
  if (est.frontao) img = cortarFrontao(img);
  if (est.cortarTopoPx) img = p.crop(img, 0, est.cortarTopoPx, img.width, img.height - est.cortarTopoPx);
  if (est.fundoPreto) img = fundoPretoParaAlpha(img);
  if (est.ceuParaAlpha) img = cortarCeuParaAlpha(img);
  if (est.cortarCeu) img = cortarCeu(img);
  if (!est.cru) {
    img = est.rampa ? remapParaRampa(img, ramps, est.rampa) : remap(img, ramps, { classify: est.classe });
    // De novo DEPOIS da quantização, e é aqui que ele funciona de verdade.
    //
    // Antes do remap a faixa de céu do gerador tem ruído e a tolerância de 24 não a reconhece
    // como chapada; depois, ela vira um único hex e o corte é exato. Sem esta segunda passada o
    // tile de areia saía com 1,05 un de #816f63 sólido colado no topo, e no jogo isso era uma
    // barra escura atravessando a tela logo abaixo dos pés do jogador.
    if (est.cortarCeu) img = cortarCeu(img);
    if (est.cortarMargem) img = cortarTopoEscuro(img);
  }
  if (est.espelho) img = espelhar(img);

  const dir = path.join(ENV, grupo);
  fs.mkdirSync(dir, { recursive: true });
  p.write(path.join(dir, 'sereias_' + nome + '.png'), img);

  console.log(nome.padEnd(16) + grupo.padEnd(12) +
    `${img.width}x${img.height}`.padEnd(12) +
    `${(img.width / UN).toFixed(2)}x${(img.height / UN).toFixed(2)}`.padEnd(14) +
    (est.cru ? 'CRU (fundo, fora da paleta)' : `${antes.toFixed(1)} -> ${distancia(img).toFixed(1)}`));
}
if (faltando) { process.exit(1); }
