// Extrai a paleta de MundoDosMortos dos pixels reais dos conceitos aprovados.
//
//   node Tools/extract-palette-cicones.js [--dry]
//
// Mesmo metodo de Troia: os passos sao ESCOLHIDOS do inventario medido dos conceitos, nunca
// amostrados por retangulo. Amostrar coordenada num conceito de 672x384 acerta o material
// errado com facilidade — em Troia pegou ceu no estandarte e verde no bronze. Todo hex abaixo
// aparece de fato nos conceitos, e o script confere que cada rampa desce em luminancia antes
// de gravar.
//
// QUATRO rampas sao COMPARTILHADAS de proposito, e e isso que faz as fases parecerem o mesmo
// jogo em vez de tres jogos com paletas diferentes (secao 2 do briefing):
//   Contorno e Bronze vem do personagem; Ceu vem de Itaca; sao os materiais que atravessam
//   todas as fases. Terra, pedra, madeira e vegetacao sao proprias de MundoDosMortos.
const fs = require('fs'), path = require('path');

const ROOT = path.join(__dirname, '..');
const SAIDA = path.join(ROOT, 'Docs/Environment_MundoDosMortos/Palette');
const seco = process.argv.includes('--dry');

const RAMPAS = {
  // --- compartilhadas ---
  'Contorno':      ['2b2e27', '1d221d', '141415', '050507'],
  'Bronze':        ['f5cd5c', 'bb7e2c', '9d611f', '733e16'],
  'Fogo':          ['fcd84e', 'e3a14d', 'f1681a', 'a7311d'],

  // --- proprias do Mundo dos Mortos ---
  // A fase e escura, e por isso a paleta foi montada com MATIZ de proposito. Lestrigoes
  // provou o oposto: oito rampas de cinza viraram uma massa unica e mataram a leitura do
  // gameplay. Aqui ha quatro familias distintas — pedra fria azulada, agua verde-morta,
  // marmore palido e fogo quente — e sao elas que separam os planos numa fase sem sol.
  'Pedra fria':    ['879ba5', '6c7f8b', '536674', '455868'],
  'Pedra funda':   ['4d5459', '395053', '354a4d', '151615'],
  'Marmore palido':['d9e8ea', 'bdced3', 'acc0c8', '798d99'],
  'Agua morta':    ['456553', '27482a', '1d351e', '111f0f'],
  'Alma':          ['e4e8d4', 'bac2ae', '9eb3ae', '70827e'],
  'Madeira morta': ['c4c0ad', 'a69d85', '7d6e57', '4e493d'],
  'Terra':         ['b59166', '715946', '604733', '35251a'],
};


const ORIGENS = {
  'Contorno': 'paleta do personagem — compartilhada',
  'Bronze': 'paleta do personagem — compartilhada',
  'Fogo': 'paleta de Troia — compartilhada',
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

const gpl = ['GIMP Palette', 'Name: Odisseia - MundoDosMortos Environment', 'Columns: 4', '#'];
const hexTxt = [];
for (const [nome, passos] of Object.entries(RAMPAS)) {
  passos.forEach((h, i) => {
    const [r, g, b] = rgb(h);
    gpl.push(`${String(r).padStart(3)} ${String(g).padStart(3)} ${String(b).padStart(3)}\t${nome} ${PASSOS[i]}`);
    hexTxt.push(h.toUpperCase());
  });
}
fs.writeFileSync(path.join(SAIDA, 'MORTOS_PALETTE.gpl'), gpl.join('\n') + '\n');
fs.writeFileSync(path.join(SAIDA, 'MORTOS_PALETTE.hex'), hexTxt.join('\n') + '\n');

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
p.write(path.join(SAIDA, 'MORTOS_PALETTE.png'), img);
console.log(`gravado em ${path.relative(ROOT, SAIDA)}`);
