// Confere se um asset usa SÓ cores da paleta oficial, e lista as intrusas.
// Um asset fora da paleta passa despercebido isolado e denuncia na composição.
const path = require('path'), p = require('./png.js');
const { loadPalette } = require('./ramp-map.js');

const ramps = loadPalette(path.join(__dirname, '..', 'Docs/Environment_Ithaca/Palette/ITHACA_PALETTE.gpl'));
const official = new Map();
for (const [name, ramp] of Object.entries(ramps))
  ramp.forEach((c, i) => official.set((c[0] << 16) | (c[1] << 8) | c[2], `${name} ${['Highlight','Base','Sombra','Profunda'][i]}`));

let bad = 0;
for (const f of process.argv.slice(2)) {
  const img = p.read(f);
  const seen = new Map(), intruders = new Map();
  for (let i = 0; i < img.width * img.height; i++) {
    const o = i * 4;
    if (img.data[o + 3] <= 8) continue;
    const k = (img.data[o] << 16) | (img.data[o + 1] << 8) | img.data[o + 2];
    (official.has(k) ? seen : intruders).set(k, (( official.has(k) ? seen : intruders).get(k) || 0) + 1);
  }
  const ok = intruders.size === 0;
  if (!ok) bad++;
  console.log(`${ok ? 'OK  ' : 'FORA'} ${path.basename(f).padEnd(32)} ${seen.size} cores da paleta${ok ? '' : `, ${intruders.size} intrusas`}`);
  for (const [k, n] of [...intruders].sort((a, b) => b[1] - a[1]).slice(0, 8))
    console.log(`       #${k.toString(16).padStart(6, '0')}  ${n}px`);
}
process.exit(bad ? 1 : 0);
