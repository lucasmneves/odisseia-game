// Extrai a paleta de Troia dos pixels reais dos conceitos aprovados.
//
//   node Tools/extract-palette.js [--dry]
//
// Onze rampas de quatro passos, 44 cores. O metodo esta explicado junto de RAMPAS abaixo;
// em resumo: os passos sao escolhidos do inventario medido dos conceitos, e o script confere
// que cada rampa desce em luminancia antes de gravar.
const fs = require('fs'), path = require('path');
const p = require('./png.js');


const ROOT = path.join(__dirname, '..');
const C = path.join(ROOT, 'Docs/Environment_Troy/Concepts');
const SAIDA = path.join(ROOT, 'Docs/Environment_Troy/Palette');
const seco = process.argv.includes('--dry');

// Os passos sao ESCOLHIDOS do inventario medido dos tres conceitos, nao amostrados por
// retangulo. A primeira versao amostrava regioes e errou feio: pegou ceu no estandarte, verde
// no bronze e passos repetidos, porque coordenada chutada num conceito de 672x384 acerta o
// material errado com facilidade. Todo hex abaixo aparece de fato nos conceitos — a grade
// ordenada por matiz e luminancia esta registrada no TROY_MASTER_REFERENCE.
//
// Duas rampas sao COMPARTILHADAS de proposito:
//   Contorno vem da paleta do personagem (#050507 ja e comum ao Odisseu e a Itaca);
//   Ceu vem de Itaca, porque o ceu do Mediterraneo e o mesmo e o azul dos conceitos de Troia
//   era um preenchimento chapado incidental, sem faixa suficiente para virar rampa.
const RAMPAS = {
  'Contorno':     ['2b2e27', '1d221d', '141415', '050507'],
  'Ceu':          ['ceeefe', '94d0f3', '74b5ed', '6481a0'],
  'Pedra clara':  ['f2e8c4', 'e3d6b7', 'd2c2a0', 'c8b690'],
  'Pedra sombra': ['b39c78', '9c8a72', '836c52', '6f5a45'],
  'Terra seca':   ['d7b97a', 'b0844c', '8e6845', '623d25'],
  'Madeira':      ['8f6942', '653f2f', '4d3329', '2e1e1c'],
  'Lona':         ['ece0ba', 'd5bb8d', 'b1966d', '7e6648'],
  'Bronze':       ['f9d688', 'bf8e52', '885e34', '5f4431'],
  'Vermelho':     ['a4462c', '953029', '5c2b24', '381f1a'],
  'Fogo':         ['fcd84e', 'e3a14d', 'f1681a', 'a7311d'],
  'Fumaca':       ['e6e8e2', 'a09486', '887c74', '725f4a'],
};

const ORIGENS = {
  'Contorno': 'paleta do personagem — compartilhada',
  'Ceu': 'paleta de Itaca — compartilhada',
  'Pedra clara': 'B_Muralha — alvenaria ciclopica iluminada',
  'Pedra sombra': 'B_Muralha — face sombreada da muralha',
  'Terra seca': 'C_Batalha — chao batido',
  'Madeira': 'B_Muralha — porta de pranchas e vergas',
  'Lona': 'A_Acampamento — tendas de linho cru',
  'Bronze': 'A_Acampamento — escudos e caldeirao',
  'Vermelho': 'A e B — estandartes',
  'Fogo': 'A e C — fogueiras',
  'Fumaca': 'C_Batalha — coluna de fumaca',
};

const paraRgb = (h) => [parseInt(h.slice(0, 2), 16), parseInt(h.slice(2, 4), 16), parseInt(h.slice(4, 6), 16)];
const ramps = {};
for (const [nome, hexes] of Object.entries(RAMPAS)) ramps[nome] = hexes.map(paraRgb);
const origens = ORIGENS;

const lum = (c) => 0.2126 * c[0] + 0.7152 * c[1] + 0.0722 * c[2];

// Confere que cada rampa desce em luminancia. Passo fora de ordem quebra o remapeamento, que
// distribui as cores do material pelos passos justamente por luminancia relativa.
for (const [nome, r] of Object.entries(ramps)) {
  for (let i = 1; i < r.length; i++) {
    if (lum(r[i]) >= lum(r[i - 1])) {
      console.error(`rampa "${nome}": passo ${i} nao e mais escuro que o anterior`);
      process.exit(1);
    }
  }
}

const hex = (c) => c.map(v => v.toString(16).padStart(2, '0')).join('');
const PASSOS = ['Highlight', 'Base', 'Sombra', 'Profunda'];

console.log('rampa            Highlight  Base     Sombra   Profunda   origem');
for (const [nome, r] of Object.entries(ramps))
  console.log(`  ${nome.padEnd(14)} ${r.map(c => '#' + hex(c)).join(' ')}   ${origens[nome]}`);

const unicas = new Set();
for (const r of Object.values(ramps)) for (const c of r) unicas.add(hex(c));
console.log(`\n${Object.keys(ramps).length} rampas, ${unicas.size} cores únicas`);

if (seco) { console.log('(simulação, nada gravado)'); process.exit(0); }

fs.mkdirSync(SAIDA, { recursive: true });
const NL = String.fromCharCode(10);

fs.writeFileSync(path.join(SAIDA, 'TROY_PALETTE.hex'),
  Object.values(ramps).flat().map(c => hex(c).toUpperCase()).join(NL) + NL);

const gpl = ['GIMP Palette', 'Name: Odisseia - Troy Environment', 'Columns: 4', '#'];
for (const [nome, r] of Object.entries(ramps))
  r.forEach((c, i) => gpl.push(`${String(c[0]).padStart(3)} ${String(c[1]).padStart(3)} ${String(c[2]).padStart(3)}\t${nome} ${PASSOS[i]}`));
fs.writeFileSync(path.join(SAIDA, 'TROY_PALETTE.gpl'), gpl.join(NL) + NL);

// Amostra visual: uma linha por rampa, quatro quadrados por linha.
const CEL = 32, linhas = Object.keys(ramps).length;
const img = p.blank(CEL * 4, CEL * linhas);
Object.values(ramps).forEach((r, li) => r.forEach((c, ci) => {
  for (let y = 0; y < CEL; y++) for (let x = 0; x < CEL; x++) {
    const o = ((li * CEL + y) * CEL * 4 + ci * CEL + x) * 4;
    img.data[o] = c[0]; img.data[o + 1] = c[1]; img.data[o + 2] = c[2]; img.data[o + 3] = 255;
  }
}));
p.write(path.join(SAIDA, 'TROY_PALETTE.png'), img);
console.log(`gravado em ${path.relative(ROOT, SAIDA)}`);
