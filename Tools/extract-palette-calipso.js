// Extrai a paleta de Calipso dos pixels reais dos conceitos.
//
//   node Tools/extract-palette-calipso.js [--dry]
//
// Mesmo metodo das fases anteriores: os passos sao ESCOLHIDOS do inventario medido, nunca
// amostrados por coordenada.
//
// ## O que separa Calipso de Gado do Sol, que e o risco real desta paleta
//
// As duas sao fases bonitas de ilha mediterranea com verde, agua e pedra clara — e sem uma
// decisao explicita Calipso sairia como uma segunda Gado do Sol. O que as separa aqui:
//
//   Gado do Sol   ceu AZUL de meio-dia, pasto amarelo-esverdeado, calcario dourado claro
//   Calipso       luz de FIM DE TARDE, floresta azul-esverdeada funda, rocha OCRE saturada
//
// A rampa `Rocha ocre` mede S 0,68 a 0,74 — a cor mais saturada das duas paletas — e Gado do
// Sol nao tem nada parecido. E a assinatura da ilha, e tambem a peca que carrega a melancolia:
// ocre e a cor do sol baixo batendo na falesia.
//
// NAO ha rampa de ceu. O ceu de Calipso e um degrade creme-dourado, e degrade nao sobrevive a
// quatro passos — vira faixas. Ele vive nas camadas de fundo, que por regra do projeto nao sao
// quantizadas, e a cor de cobertura sai MEDIDA da primeira linha do asset.
const fs = require('fs'), path = require('path');

const ROOT = path.join(__dirname, '..');
const SAIDA = path.join(ROOT, 'Docs/Environment_Calipso/Palette');
const seco = process.argv.includes('--dry');

const RAMPAS = {
  // --- compartilhadas: sao elas que fazem as fases parecerem o mesmo jogo ---
  'Contorno':        ['2b2e27', '1d221d', '141415', '050507'],
  'Bronze':          ['f5cd5c', 'bb7e2c', '9d611f', '733e16'],

  // --- proprias de Calipso ---
  // O branco da paleta mora em `Agua rasa`, e nao em `Areia`, e isso e uma correcao medida.
  //
  // Na primeira versao o passo mais claro de `Areia` era #f7f1ec — quase branco puro — e o
  // resultado foi um tileset de praia ESTOURADO: a areia saiu branca, sem cor, e ao lado do
  // personagem leria como neve. Areia de praia ao sol de fim de tarde e DOURADA, nao branca.
  //
  // E o branco faz falta em outro lugar: a espuma da cachoeira, sem ele, caiu em `Marmore` e
  // a queda d'agua saiu cinza-pedra — uma cachoeira que nao e branca deixa de ler como agua.
  // Espuma E o passo mais claro da agua, entao o branco pertence a rampa da agua.
  'Areia':           ['fad6a8', 'e4cdb4', 'cfb69c', 'aa998a'],
  'Agua rasa':       ['f7f1ec', '81b0a9', '6ca6a7', '528590'],
  'Agua funda':      ['456c7e', '3a5872', '253633', '0d1112'],

  // DOIS verdes separados por FAMILIA DE MATIZ, nao so por valor: a copa iluminada e
  // amarelo-esverdeada (matiz ~80) e a mata funda e azul-esverdeada (~150). E a licao de Circe,
  // e aqui ela importa o dobro porque a floresta e a maior massa da fase.
  'Folhagem clara':  ['7f9d3a', '668b35', '4d7733', '2b512a'],
  'Folhagem funda':  ['397950', '23533c', '102923', '0b221e'],

  // A assinatura da ilha — ver o cabecalho.
  'Rocha ocre':      ['eeaf49', 'ee943f', 'a55935', '603628'],

  // Marmore e CINZA FRIO de proposito, contra a areia que e creme quente. Sem essa oposicao o
  // palacio de Calipso encostaria na praia em matiz e os dois planos colariam.
  'Marmore':         ['b3a594', '887d78', '757071', '555258'],

  // A jangada. Ela e o assunto narrativo da fase — o desejo de partir —, e por isso tem rampa
  // propria em vez de sair de um passo escuro de outra: madeira crua tem de ler como material
  // NOVO num lugar de marmore e folhagem.
  'Madeira':         ['9f6a4f', '805849', '704b43', '5d403e'],
};

// NAO existe rampa de "pedra molhada" aqui, e a ausencia e deliberada.
//
// O inventario tem cinzas de baixa saturacao no risco d'agua (675b56, 5e5454, 52494e, 413d43),
// e eles caberiam numa rampa. Mas mediriam L 0,37 a 0,24 — uma faixa estreita de cinza, que e
// exatamente a forma da rampa que achatou Lestrigoes. A pedra molhada sai do passo escuro de
// "Areia e calcario"; e o mesmo material sob agua, nao um material novo.

const ORIGENS = {
  'Contorno': 'paleta do personagem — compartilhada',
  'Bronze': 'paleta do personagem — compartilhada',
  'Areia': 'Conceito A — a praia',
  'Agua rasa': 'Conceito A — mar muito mais calmo que Cytera e Cila',
  'Agua funda': 'Conceito A',
  'Folhagem clara': 'Conceito B — a floresta abundante que o A nao tem',
  'Folhagem funda': 'Conceitos A e B',
  'Rocha ocre': 'Conceito B — a falesia da cachoeira',
  'Marmore': 'Conceito A — a morada de pedra',
  'Madeira': 'Conceito A — a jangada',
};

const PASSOS = ['Highlight', 'Base', 'Sombra', 'Profunda'];
const rgb = h => [parseInt(h.slice(0, 2), 16), parseInt(h.slice(2, 4), 16), parseInt(h.slice(4, 6), 16)];
const lum = h => { const [r, g, b] = rgb(h); return (0.2126 * r + 0.7152 * g + 0.0722 * b) / 255; };

// A rampa tem de DESCER. Um passo fora de ordem nao gera erro em lugar nenhum — so faz o
// classificador de material distribuir os tons ao contrario e o asset sair com a luz invertida.
let erros = 0;
for (const [nome, passos] of Object.entries(RAMPAS)) {
  if (passos.length !== 4) { console.error(`${nome}: ${passos.length} passos, esperado 4`); erros++; }
  for (let i = 1; i < passos.length; i++) {
    if (lum(passos[i]) >= lum(passos[i - 1])) {
      console.error(`${nome}: passo ${i} (#${passos[i]}, lum ${lum(passos[i]).toFixed(2)}) nao e mais escuro que ` +
        `#${passos[i - 1]} (lum ${lum(passos[i - 1]).toFixed(2)})`);
      erros++;
    }
  }
}

// Cor repetida entre rampas diferentes confunde o classificador: ele nao saberia a que
// material o pixel pertence.
const vistos = new Map();
for (const [nome, passos] of Object.entries(RAMPAS)) {
  for (const h of passos) {
    if (vistos.has(h)) { console.error(`#${h} aparece em "${vistos.get(h)}" e em "${nome}"`); erros++; }
    vistos.set(h, nome);
  }
}
if (erros) { console.error(`${erros} problema(s) — nada gravado`); process.exit(1); }

const total = Object.keys(RAMPAS).length;
console.log(`${total} rampas, ${total * 4} cores — todas descem em luminancia, nenhuma repetida`);
for (const [nome, passos] of Object.entries(RAMPAS)) {
  console.log(`  ${nome.padEnd(14)} ${passos.map(h => '#' + h).join(' ')}` +
    (ORIGENS[nome] ? `   (${ORIGENS[nome]})` : ''));
}
if (seco) { console.log('--dry: nada gravado'); process.exit(0); }

fs.mkdirSync(SAIDA, { recursive: true });

const gpl = ['GIMP Palette', 'Name: Odisseia - Calipso Environment', 'Columns: 4', '#'];
const hexTxt = [];
for (const [nome, passos] of Object.entries(RAMPAS)) {
  passos.forEach((h, i) => {
    const [r, g, b] = rgb(h);
    gpl.push(`${String(r).padStart(3)} ${String(g).padStart(3)} ${String(b).padStart(3)}\t${nome} ${PASSOS[i]}`);
    hexTxt.push(h.toUpperCase());
  });
}
fs.writeFileSync(path.join(SAIDA, 'CALIPSO_PALETTE.gpl'), gpl.join('\n') + '\n');
fs.writeFileSync(path.join(SAIDA, 'CALIPSO_PALETTE.hex'), hexTxt.join('\n') + '\n');

// Amostra visual: uma faixa por rampa, para conferir a olho que a escada de tons e regular.
const p = require('./png.js');
const CEL = 24, nomes = Object.keys(RAMPAS);
const img = p.blank(CEL * 4, CEL * nomes.length);
nomes.forEach((nome, y) => RAMPAS[nome].forEach((h, x) => {
  const [r, g, b] = rgb(h);
  // Preenchimento a mao: `fillRect` vive em compose.js junto do banco de materiais de Itaca,
  // e arrastar aquele modulo inteiro para pintar catorze faixas nao se paga.
  for (let py = y * CEL; py < (y + 1) * CEL; py++) {
    for (let px = x * CEL; px < (x + 1) * CEL; px++) {
      const o = (py * img.width + px) * 4;
      img.data[o] = r; img.data[o + 1] = g; img.data[o + 2] = b; img.data[o + 3] = 255;
    }
  }
}));
p.write(path.join(SAIDA, 'CALIPSO_PALETTE.png'), img);
console.log(`gravado em ${path.relative(ROOT, SAIDA)}`);
