// Monta a paleta de Pretendentes: a paleta de ÍTACA inteira, mais o que os pretendentes trouxeram.
//
//   node Tools/extract-palette-pretendentes.js [--dry]
//
// ## Estender, não substituir
//
// A fase 14 usa a paleta da fase 01 sem trocar um hex, porque o jogador precisa reconhecer a casa.
// A fase 15 é a MESMA casa — mas o briefing pede identidade própria: luz artificial, fogo,
// excesso. As duas exigências só cabem juntas de um jeito: **todos os 36 hex de Ítaca continuam
// aqui, lidos do arquivo da fase 01**, e a paleta ganha rampas que Ítaca nunca teve.
//
// E as rampas novas não são escolha de gosto — são literalmente o que os pretendentes trouxeram:
//
//   Fogo    — Ítaca não tinha rampa de fogo; a tocha da fase 01 e o braseiro da 14 ficaram fora
//             da paleta por isso. Aqui o fogo é a luz da fase inteira.
//   Vinho   — estandartes vermelhos e vinho derramado no chão. Ítaca não tem vermelho nenhum.
//   Noite   — o azul frio do ambiente. É a correção medida do Conceito B (ver abaixo).
//   Bronze  — escudos, elmos, braseiros e taças. Rampa compartilhada do personagem, que as fases
//             02 a 13 já usam e Ítaca não carregava.
//
// ## Por que existe uma rampa de NOITE
//
// O Conceito B (o banquete) mediu 88% dos pixels em famílias quentes — laranja 63%, vermelho 25%.
// Um salão iluminado só por fogo, com pedra quente e vinho vermelho, colapsa numa massa única:
// é a falha de Lestrigões, em laranja. O Conceito C mediu a separação que falta — cinza 44%,
// laranja 25%, AZUL 25% — e é o contraste frio-quente dele que mantém o gameplay legível numa
// cena escura. A rampa `Noite` existe para as sombras do salão serem azuis, e só o fogo, o vinho
// e a comida serem quentes.
const fs = require('fs'), path = require('path');
const { loadPalette } = require('./ramp-map.js');

const ROOT = path.join(__dirname, '..');
const SAIDA = path.join(ROOT, 'Docs/Environment_Pretendentes/Palette');
const seco = process.argv.includes('--dry');

const hex = (c) => c.map(v => v.toString(16).padStart(2, '0')).join('');
const itaca = loadPalette(path.join(ROOT, 'Docs/Environment_Ithaca/Palette/ITHACA_PALETTE.gpl'));

const RAMPAS = {};
for (const [nome, passos] of Object.entries(itaca)) { RAMPAS[nome] = passos.map(hex); }

// Todos os hex abaixo foram MEDIDOS nos três conceitos desta fase.
Object.assign(RAMPAS, {
  'Bronze': ['f5cd5c', 'bb7e2c', '9d611f', '733e16'],   // compartilhada do personagem
  'Fogo':   ['e7b871', 'f4842f', 'ac714a', '6d3125'],
  'Vinho':  ['bc4d47', '8d2b2f', '761c28', '511120'],
  'Noite':  ['4b5866', '2b3d4d', '182331', '0f1118'],
});

const NOVAS = new Set(['Bronze', 'Fogo', 'Vinho', 'Noite']);
const PASSOS = ['Highlight', 'Base', 'Sombra', 'Profunda'];
const rgb = h => [parseInt(h.slice(0, 2), 16), parseInt(h.slice(2, 4), 16), parseInt(h.slice(4, 6), 16)];
const lum = h => { const [r, g, b] = rgb(h); return (0.2126 * r + 0.7152 * g + 0.0722 * b) / 255; };

let erros = 0;
for (const [nome, passos] of Object.entries(RAMPAS)) {
  for (let i = 1; i < passos.length; i++) {
    if (lum(passos[i]) >= lum(passos[i - 1])) {
      console.error(`${nome}: passo ${i} (#${passos[i]}) não é mais escuro que #${passos[i - 1]}`);
      erros++;
    }
  }
}
// Cor repetida entre rampas confunde o classificador — mas a checagem cobra só as rampas NOVAS.
//
// A paleta da fase 01 já traz uma repetição: #6b5d4e é o passo Profunda de `Terra / caminho` e o
// passo Base de `Pedra`. Ela é anterior à regra (que só nasceu em Lestrigões) e as fases 01 e 14
// funcionam com ela. Corrigir aqui significaria alterar a paleta da fase 01, e a continuidade das
// três fases de Ítaca depende de ela ficar intacta. Fica registrada, não corrigida; o que esta
// checagem garante é que nada do que os pretendentes trouxeram colide com Ítaca ou entre si.
const vistos = new Map();
for (const [nome, passos] of Object.entries(RAMPAS)) {
  for (const h of passos) {
    const anterior = vistos.get(h);
    if (anterior && (NOVAS.has(nome) || NOVAS.has(anterior))) {
      console.error(`#${h} aparece em "${anterior}" e em "${nome}"`); erros++;
    } else if (anterior) {
      console.log(`  (herdado da fase 01: #${h} em "${anterior}" e em "${nome}")`);
    }
    if (!anterior) { vistos.set(h, nome); }
  }
}
if (erros) { console.error(`${erros} problema(s) — nada gravado`); process.exit(1); }

const total = Object.keys(RAMPAS).length;
console.log(`${total} rampas, ${total * 4} cores — ${total - NOVAS.size} de Ítaca intactas, ${NOVAS.size} novas`);
for (const [nome, passos] of Object.entries(RAMPAS)) {
  console.log(`  ${(NOVAS.has(nome) ? '+ ' : '  ') + nome.padEnd(17)} ${passos.map(h => '#' + h).join(' ')}`);
}
if (seco) { process.exit(0); }

fs.mkdirSync(SAIDA, { recursive: true });
const gpl = ['GIMP Palette', 'Name: Odisseia - Pretendentes Environment', 'Columns: 4', '#'];
const hexTxt = [];
for (const [nome, passos] of Object.entries(RAMPAS)) {
  passos.forEach((h, i) => {
    const [r, g, b] = rgb(h);
    gpl.push(`${String(r).padStart(3)} ${String(g).padStart(3)} ${String(b).padStart(3)}\t${nome} ${PASSOS[i]}`);
    hexTxt.push(h.toUpperCase());
  });
}
fs.writeFileSync(path.join(SAIDA, 'PRETENDENTES_PALETTE.gpl'), gpl.join('\n') + '\n');
fs.writeFileSync(path.join(SAIDA, 'PRETENDENTES_PALETTE.hex'), hexTxt.join('\n') + '\n');
console.log(`gravado em ${path.relative(ROOT, SAIDA)}`);
