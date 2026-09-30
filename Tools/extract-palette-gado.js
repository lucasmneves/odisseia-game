// Extrai a paleta de Gado do Sol dos pixels reais dos conceitos.
//
//   node Tools/extract-palette-gado.js [--dry]
//
// Mesmo metodo das fases anteriores: os passos sao ESCOLHIDOS do inventario medido, nunca
// amostrados por coordenada.
//
// ESTA FASE E O CONTRARIO EXATO DE CILA E CARIBDIS, e a paleta e onde isso se decide. La foram
// 10 rampas de basalto, teal e violeta, com L medio baixo e uma unica familia quente. Aqui sao
// 10 rampas de ceu, pasto, calcario dourado e gado branco, com quase tudo acima de L 0,40. As
// duas fases so continuam parecendo o mesmo jogo porque compartilham Contorno e Bronze — que
// vem da paleta do personagem e atravessam a campanha inteira.
//
// UMA DECISAO QUE PARECE OMISSAO: nao existe rampa de "ouro solar". O sol, os detalhes
// dourados do gado sagrado e o fogo do altar usam BRONZE, a rampa compartilhada do personagem.
// Criar uma segunda rampa dourada colocaria dois amarelos quase iguais na mesma paleta, e a
// regra medida em Lestrigoes e que cor repetida entre rampas nao e detalhe: e o classificador
// deixando de saber a que material o pixel pertence. Reusar Bronze resolve o problema e ainda
// amarra a fase ao resto do jogo.
const fs = require('fs'), path = require('path');

const ROOT = path.join(__dirname, '..');
const SAIDA = path.join(ROOT, 'Docs/Environment_GadoDoSol/Palette');
const seco = process.argv.includes('--dry');

const RAMPAS = {
  // --- compartilhadas: sao elas que fazem as fases parecerem o mesmo jogo ---
  'Contorno':      ['2b2e27', '1d221d', '141415', '050507'],
  // Bronze faz DOIS papeis aqui: o metal do personagem e todo o ouro solar da fase.
  'Bronze':        ['f5cd5c', 'bb7e2c', '9d611f', '733e16'],

  // --- proprias de Gado do Sol ---
  'Ceu':               ['b0d3e4', '85bfdf', '6cabd6', '4f92c7'],
  'Mar':               ['3977ab', '23598f', '102b2b', '040608'],

  // Pasto e Folhagem sao dois verdes separados por FAMILIA DE MATIZ, nao so por valor: o pasto
  // e verde-amarelado (matiz ~75) e a folhagem e verde-azulado (~140). E a licao de Circe —
  // tres verdes separados por luminancia sustentaram os planos sozinhos — aplicada com matiz
  // tambem, porque aqui os dois verdes ocupam quase a mesma faixa de valor.
  'Pasto':             ['94b048', '80a03f', '618c35', '476a2f'],
  'Folhagem':          ['5d8236', '37583d', '295032', '26412e'],

  'Calcario dourado':  ['f9d98f', 'edca84', 'd7ae68', 'bc8f53'],
  'Terra':             ['a37042', '8d5f40', '69453a', '4d3137'],

  // O gado tem rampa PROPRIA, e essa e a decisao de identidade da fase. Eles nao sao "brancos":
  // sao o unico material do jogo que vai de branco quase puro ao ouro num degrade so, e e isso
  // que os marca como sagrados sem precisar de brilho, particula ou shader.
  'Gado sagrado':      ['fef9e2', 'f9e6bd', 'e9cfa0', 'c99e5d'],

  'Muro seco':         ['dbccb8', '8c7f74', '715b4a', '3e312c'],
};

// NAO existe rampa de "pedra molhada" aqui, e a ausencia e deliberada.
//
// O inventario tem cinzas de baixa saturacao no risco d'agua (675b56, 5e5454, 52494e, 413d43),
// e eles caberiam numa rampa. Mas mediriam L 0,37 a 0,24 — uma faixa estreita de cinza, que e
// exatamente a forma da rampa que achatou Lestrigoes. A pedra molhada sai do passo escuro de
// "Areia e calcario"; e o mesmo material sob agua, nao um material novo.

const ORIGENS = {
  'Contorno': 'paleta do personagem — compartilhada',
  'Bronze': 'paleta do personagem — compartilhada; aqui e tambem o ouro solar',
  'Ceu': 'Conceito A',
  'Mar': 'Conceito A — a fase comeca na praia',
  'Pasto': 'Conceito A — o verde que o Conceito C nao tem em quantidade',
  'Folhagem': 'Conceitos A e B — oliveira e cipreste',
  'Calcario dourado': 'Conceito B — o templo mais bem desenhado dos tres',
  'Terra': 'Conceito B — o caminho de terra',
  'Gado sagrado': 'Conceito A — os animais',
  'Muro seco': 'Conceito A — o muro de pedra sem argamassa',
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

const gpl = ['GIMP Palette', 'Name: Odisseia - GadoDoSol Environment', 'Columns: 4', '#'];
const hexTxt = [];
for (const [nome, passos] of Object.entries(RAMPAS)) {
  passos.forEach((h, i) => {
    const [r, g, b] = rgb(h);
    gpl.push(`${String(r).padStart(3)} ${String(g).padStart(3)} ${String(b).padStart(3)}\t${nome} ${PASSOS[i]}`);
    hexTxt.push(h.toUpperCase());
  });
}
fs.writeFileSync(path.join(SAIDA, 'GADO_PALETTE.gpl'), gpl.join('\n') + '\n');
fs.writeFileSync(path.join(SAIDA, 'GADO_PALETTE.hex'), hexTxt.join('\n') + '\n');

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
p.write(path.join(SAIDA, 'GADO_PALETTE.png'), img);
console.log(`gravado em ${path.relative(ROOT, SAIDA)}`);
