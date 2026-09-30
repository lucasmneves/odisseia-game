// Extrai a paleta de Sereias dos pixels reais dos conceitos aprovados.
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
//   todas as fases. Terra, pedra, madeira e vegetacao sao proprias de Sereias.
const fs = require('fs'), path = require('path');

const ROOT = path.join(__dirname, '..');
const SAIDA = path.join(ROOT, 'Docs/Environment_Sereias/Palette');
const seco = process.argv.includes('--dry');

const RAMPAS = {
  // --- compartilhadas ---
  'Contorno':      ['2b2e27', '1d221d', '141415', '050507'],
  'Bronze':        ['f5cd5c', 'bb7e2c', '9d611f', '733e16'],
  'Ceu':           ['ceeefe', '94d0f3', '74b5ed', '6481a0'],

  // --- proprias das Sereias ---
  // NOVE rampas, e nao onze. Lestrigoes tinha onze com oito cinzas e virou uma massa unica; a
  // licao nao foi "use menos", foi "separe por FAMILIA DE MATIZ". Aqui as seis proprias sao
  // creme, marrom, turquesa claro, teal fundo, branco-esverdeado e verde-oliva — seis familias
  // que o olho separa sozinho, e por isso a fase nao precisa de tint de perspectiva aerea para
  // os planos se descolarem.
  //
  // Todos os hex abaixo foram MEDIDOS nos conceitos A e B, que juntos usam 79 cores.
  'Areia e calcario': ['f8ebcb', 'd1c1a1', 'ae957f', '816f63'],
  'Agua rasa':        ['9dd5c5', '7ac0b4', '60aea4', '4e9993'],
  'Agua funda':       ['4d837d', '40747a', '3b666f', '162d2a'],
  'Espuma':           ['f9fcfb', 'afdeca', '8dc0a9', '719e8a'],
  'Vegetacao':        ['c5d4b7', '98ab4e', '5a794e', '415c42'],
  'Madeira naufragio':['dbb98c', '9c816e', '8a6861', '664642'],
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
  'Ceu': 'paleta de Itaca — compartilhada',
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

const gpl = ['GIMP Palette', 'Name: Odisseia - Sereias Environment', 'Columns: 4', '#'];
const hexTxt = [];
for (const [nome, passos] of Object.entries(RAMPAS)) {
  passos.forEach((h, i) => {
    const [r, g, b] = rgb(h);
    gpl.push(`${String(r).padStart(3)} ${String(g).padStart(3)} ${String(b).padStart(3)}\t${nome} ${PASSOS[i]}`);
    hexTxt.push(h.toUpperCase());
  });
}
fs.writeFileSync(path.join(SAIDA, 'SEREIAS_PALETTE.gpl'), gpl.join('\n') + '\n');
fs.writeFileSync(path.join(SAIDA, 'SEREIAS_PALETTE.hex'), hexTxt.join('\n') + '\n');

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
p.write(path.join(SAIDA, 'SEREIAS_PALETTE.png'), img);
console.log(`gravado em ${path.relative(ROOT, SAIDA)}`);
