// PXL-013 por composição: o escudo da pose ShieldHold_00 colado nos quadros de Jump e Fall do master.
//
//   node Tools/shield-air.js → Docs/Art/PixelLab/PXL-013/_candidatos/{ShieldJump,ShieldFall}_NN.png (células de 84 px)
//
// Âncora por quadro: topo da cabeça e frente do rosto (o pixel opaco mais à direita nas 12 linhas abaixo do
// topo). Na guarda, o escudo começa 1 px à frente do rosto e 10 px abaixo do topo da cabeça; a mesma relação é
// repetida em cada quadro do salto, então o escudo acompanha o corpo quando ele sobe, encolhe e cai.
const fs = require('fs'), path = require('path'), p = require('./png.js');
const ROOT = path.join(__dirname, '..');
const FOLHA = path.join(ROOT, 'Assets/Resources/Odisseia/Characters/CHR_Odysseus.png');
const META = fs.readFileSync(FOLHA + '.meta', 'utf8');
const sheet = p.read(FOLHA);
const OUT = path.join(ROOT, 'Docs/Art/PixelLab/PXL-013/_candidatos');
fs.mkdirSync(OUT, { recursive: true });

function celula(nome) {
  const m = META.match(new RegExp('name: CHR_Odysseus_' + nome + '\\s*\\n\\s+rect:\\s*\\n\\s+serializedVersion: 2\\s*\\n\\s+x: (\\d+)\\s*\\n\\s+y: (\\d+)\\s*\\n\\s+width: (\\d+)\\s*\\n\\s+height: (\\d+)'));
  if (!m) return null;
  const [x, y, w, h] = m.slice(1).map(Number);
  return p.crop(sheet, x, sheet.height - y - h, w, h);
}
const op = (img, x, y) => img.data[(y * img.width + x) * 4 + 3] > 0;
function ancora(img) {
  let topo = -1;
  for (let y = 0; y < img.height && topo < 0; y++) for (let x = 0; x < img.width; x++) if (op(img, x, y)) { topo = y; break; }
  let frente = 0;
  for (let y = topo; y < topo + 12; y++) for (let x = 0; x < img.width; x++) if (op(img, x, y)) frente = Math.max(frente, x);
  return { topo, frente };
}

// O escudo na guarda: disco à frente do corpo. Fica tudo o que está à frente do rosto.
const guarda = celula('ShieldHold_00'), ag = ancora(guarda);
const x0 = ag.frente + 1;
const escudo = [];
for (let y = 0; y < guarda.height; y++) for (let x = x0; x < guarda.width; x++)
  if (op(guarda, x, y)) escudo.push({ dx: x - ag.frente, dy: y - ag.topo, c: guarda.data.readUInt32BE((y * guarda.width + x) * 4) });
// e a borda esquerda do disco, que fica sobre o peito (colunas até 4 px atrás do rosto, só cores de bronze/contorno)
const bronze = (c) => { const r = c >>> 24, g = (c >>> 16) & 255, b = (c >>> 8) & 255; return (r > g && g > b && r - b > 60) || r + g + b < 60; };
for (let y = 0; y < guarda.height; y++) for (let x = ag.frente - 4; x < x0; x++) {
  const c = guarda.data.readUInt32BE((y * guarda.width + x) * 4);
  if (op(guarda, x, y) && bronze(c) && y > ag.topo + 9) escudo.push({ dx: x - ag.frente, dy: y - ag.topo, c });
}

for (const [estado, novo] of [['Jump', 'ShieldJump'], ['Fall', 'ShieldFall']]) {
  for (let i = 0; i < 16; i++) {
    const q = celula(`${estado}_${String(i).padStart(2, '0')}`);
    if (!q) break;
    const a = ancora(q);
    for (const s of escudo) {
      const x = a.frente + s.dx, y = a.topo + s.dy;
      if (x >= 0 && y >= 0 && x < q.width && y < q.height) q.data.writeUInt32BE(s.c >>> 0, (y * q.width + x) * 4);
    }
    p.write(path.join(OUT, `${novo}_${String(i).padStart(2, '0')}.png`), q);
  }
}
console.log(`escudo: ${escudo.length} px colados`);
