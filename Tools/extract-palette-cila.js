// Extrai a paleta de Cila e Caribdis dos pixels reais dos conceitos.
//
//   node Tools/extract-palette-cila.js [--dry]
//
// Mesmo metodo das fases anteriores: os passos sao ESCOLHIDOS do inventario medido, nunca
// amostrados por coordenada.
//
// ESTA PALETA CORRIGE UM DEFEITO MEDIDO NO CONCEITO APROVADO. O Conceito A venceu como
// direcao, mas mediu 5 de 10 faixas de valor ocupadas, com 27% dos pixels num unico quase-preto
// (#090909). Isso e a falha de Lestrigoes de cabeca para baixo: la uma massa cinza unica, aqui
// uma massa PRETA unica — e o briefing proibe a fase completamente preta, porque o gameplay
// precisa continuar visivel.
//
// A correcao e a rampa "Rocha jogavel", cujos quatro passos vem do Conceito B (que mediu 9 de
// 10 faixas de valor). O Conceito B foi RECUSADO como direcao — veio como alvenaria de blocos,
// o mesmo defeito ja documentado em Sereias — mas e ele que tem os tons medios de rocha que o
// A nao tem. A rocha do plano de jogo sobe ate L 0,56 enquanto a falesia de fundo fica abaixo
// de L 0,15: e essa distancia de valor que separa onde se pisa do que e cenario.
//
// A outra decisao e o OURO ser a unica familia quente. Numa fase de basalto preto, mar teal e
// ceu violeta, tudo e frio; o relampago e a madeira do navio sao o unico calor, e por isso o
// navio — que e plano de jogo — se destaca do fundo por MATIZ, nao so por valor.
const fs = require('fs'), path = require('path');

const ROOT = path.join(__dirname, '..');
const SAIDA = path.join(ROOT, 'Docs/Environment_CilaCaribdis/Palette');
const seco = process.argv.includes('--dry');

const RAMPAS = {
  // --- compartilhadas: sao elas que fazem as fases parecerem o mesmo jogo ---
  'Contorno':      ['2b2e27', '1d221d', '141415', '050507'],
  'Bronze':        ['f5cd5c', 'bb7e2c', '9d611f', '733e16'],

  // --- proprias de Cila e Caribdis ---
  // Basalto e a falesia de FUNDO: quase preta de proposito, e o que fecha o quadro e cria a
  // claustrofobia. Ela nunca e pisada.
  'Basalto':          ['353742', '2d2e36', '1d1e22', '090909'],

  // Rocha jogavel e a rampa que salva a fase — ver o cabecalho. Medida no Conceito B.
  'Rocha jogavel':    ['89918c', '606a66', '445351', '263135'],

  'Agua funda':       ['3a666b', '204a49', '133732', '081e1d'],
  'Agua agitada':     ['5a968e', '3f7f74', '2a6a5d', '0f4339'],
  'Espuma':           ['fdfdf9', 'dfebee', 'b8cedd', '8fb0bd'],
  'Ceu de tempestade':['b8bcd5', '9a9bbc', '736e8b', '4a435c'],
  'Relampago':        ['fdfae2', 'fadf9d', 'c78f53', 'ab703a'],
  'Madeira do navio': ['a68d7b', '7d5e3b', '6b5236', '4f3123'],
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
  'Basalto': 'Conceito A — a falesia de fundo',
  'Rocha jogavel': 'Conceito B — os tons medios que o A nao tem',
  'Agua funda': 'Conceito A',
  'Agua agitada': 'Conceito B — o teal do redemoinho',
  'Espuma': 'Conceitos A e B',
  'Ceu de tempestade': 'Conceito A — a unica familia violeta',
  'Relampago': 'Conceitos A e B — a unica familia quente de luz',
  'Madeira do navio': 'Conceitos A e B — o calor do plano de jogo',
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

const gpl = ['GIMP Palette', 'Name: Odisseia - CilaCaribdis Environment', 'Columns: 4', '#'];
const hexTxt = [];
for (const [nome, passos] of Object.entries(RAMPAS)) {
  passos.forEach((h, i) => {
    const [r, g, b] = rgb(h);
    gpl.push(`${String(r).padStart(3)} ${String(g).padStart(3)} ${String(b).padStart(3)}\t${nome} ${PASSOS[i]}`);
    hexTxt.push(h.toUpperCase());
  });
}
fs.writeFileSync(path.join(SAIDA, 'CILA_PALETTE.gpl'), gpl.join('\n') + '\n');
fs.writeFileSync(path.join(SAIDA, 'CILA_PALETTE.hex'), hexTxt.join('\n') + '\n');

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
p.write(path.join(SAIDA, 'CILA_PALETTE.png'), img);
console.log(`gravado em ${path.relative(ROOT, SAIDA)}`);
