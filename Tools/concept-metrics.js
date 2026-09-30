// Mede um conceito antes de aprová-lo: inventário de cores, faixa de luminância e famílias
// de matiz.
//
//   node Tools/concept-metrics.js <a.png> [b.png ...]
//
// Por que existe: a lição de Lestrigões é que uma paleta monocromática destrói a leitura do
// gameplay, e isso não se vê a olho num conceito bonito. O que separa os planos é ter faixa
// de VALOR e de MATIZ suficientes — então o conceito é medido nessas duas coisas antes de
// virar direção da fase.
const path = require('path');
const { read } = require('./png.js');

const lum = (r, g, b) => (0.2126 * r + 0.7152 * g + 0.0722 * b) / 255;

function matiz(r, g, b) {
  const max = Math.max(r, g, b), min = Math.min(r, g, b), d = max - min;
  if (d === 0) return -1;                       // acromático
  let h;
  if (max === r) h = ((g - b) / d) % 6;
  else if (max === g) h = (b - r) / d + 2;
  else h = (r - g) / d + 4;
  return (h * 60 + 360) % 360;
}
const sat = (r, g, b) => { const max = Math.max(r, g, b), min = Math.min(r, g, b); return max === 0 ? 0 : (max - min) / max; };

const FAMILIAS = [
  ['vermelho', 345, 15], ['laranja', 15, 45], ['ouro', 45, 70], ['verde', 70, 165],
  ['ciano/teal', 165, 200], ['azul', 200, 255], ['violeta', 255, 300], ['magenta', 300, 345],
];
function familia(h) {
  if (h < 0) return 'cinza';
  for (const [nome, a, b] of FAMILIAS) {
    if (a > b ? (h >= a || h < b) : (h >= a && h < b)) return nome;
  }
  return 'cinza';
}

for (const arquivo of process.argv.slice(2)) {
  const img = read(arquivo);
  const contagem = new Map(), porFamilia = new Map(), hist = new Array(10).fill(0);
  let total = 0;
  for (let i = 0; i < img.width * img.height; i++) {
    const o = i * 4;
    if (img.data[o + 3] < 128) continue;
    const r = img.data[o], g = img.data[o + 1], b = img.data[o + 2];
    const hex = [r, g, b].map(v => v.toString(16).padStart(2, '0')).join('');
    contagem.set(hex, (contagem.get(hex) || 0) + 1);
    const L = lum(r, g, b);
    hist[Math.min(9, Math.floor(L * 10))]++;
    // Cinza de baixa saturação não conta como família de matiz: é o que fez Lestrigões
    // parecer ter cores e não ter.
    const f = sat(r, g, b) < 0.18 ? 'cinza' : familia(matiz(r, g, b));
    porFamilia.set(f, (porFamilia.get(f) || 0) + 1);
    total++;
  }

  const fam = [...porFamilia.entries()].sort((a, b) => b[1] - a[1]);
  // Famílias que ocupam pelo menos 4% são as que o olho realmente usa para separar planos.
  const uteis = fam.filter(([, n]) => n / total >= 0.04);
  const ocupadas = hist.filter(n => n / total >= 0.03).length;

  console.log(`\n=== ${path.basename(arquivo)} ===`);
  console.log(`  cores distintas .......... ${contagem.size}`);
  console.log(`  faixas de valor ocupadas . ${ocupadas} de 10   ${hist.map(n => Math.min(9, Math.round(n / total * 40))).join('')}`);
  console.log(`  famílias de matiz úteis .. ${uteis.length}   ${uteis.map(([f, n]) => `${f} ${(n / total * 100).toFixed(0)}%`).join(' · ')}`);
  console.log(`  top 12 cores: ${[...contagem.entries()].sort((a, b) => b[1] - a[1]).slice(0, 12)
    .map(([h, n]) => `#${h} ${(n / total * 100).toFixed(1)}%`).join('  ')}`);
}
