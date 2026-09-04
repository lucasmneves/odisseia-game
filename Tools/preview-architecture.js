// Prancha do Grupo 3: todas as construções na mesma linha de chão, com o Odisseu para escala.
// Serve para julgar coerência entre peças, que é o que uma imagem isolada não mostra.
const path = require('path');
const C = require('./compose.js');
const p = C.p;

const out = process.argv[2] || path.join(C.ENV, 'Architecture/_conjunto.png');
const A = path.join(C.ENV, 'Architecture');

const sheet = p.read(path.join(C.ROOT, 'Assets/Resources/Odisseia/Characters/CHR_Odysseus.png'));
const idle = p.crop(sheet, 0, 0, 84, 84);
const ib = p.bounds(idle);
const hero = p.crop(idle, ib.x0, ib.y0, ib.w, ib.h);

const wallTile = p.read(path.join(A, 'ithaca_wall_low_01.png'));
const muro = p.blank(wallTile.width * 3, wallTile.height);
for (let i = 0; i < 3; i++) p.blit(muro, wallTile, i * wallTile.width, 0);

const pieces = [
  p.read(path.join(A, 'ithaca_palace_01.png')),
  p.read(path.join(A, 'ithaca_house_odysseus_01.png')),
  p.read(path.join(A, 'ithaca_house_small_01.png')),
  p.read(path.join(A, 'ithaca_column_01.png')),
  muro,
  hero,
];

const GAP = 18, PAD = 20;
const W = PAD * 2 + pieces.reduce((s, x) => s + x.width, 0) + GAP * (pieces.length - 1);
const H = PAD * 2 + Math.max(...pieces.map(x => x.height)) + 14;
const img = p.blank(W, H);
const sky = C.ramps['Ceu'][1], soil = C.ramps['Terra / caminho'][3];
for (let i = 0; i < W * H; i++) {
  const o = i * 4;
  img.data[o] = sky[0]; img.data[o + 1] = sky[1]; img.data[o + 2] = sky[2]; img.data[o + 3] = 255;
}
const ground = H - PAD;
C.fillRect(img, 0, ground, W, H - ground, soil);
C.fillRect(img, 0, ground, W, 1, C.ramps['Contorno'][3]);

let x = PAD;
for (const piece of pieces) { p.blit(img, piece, x, ground - piece.height); x += piece.width + GAP; }
p.write(out, img);
console.log(`${path.relative(C.ROOT, out)}  ${W}x${H}`);
for (const [n, piece] of Object.entries({
  'palácio': pieces[0], 'casa de Odisseu': pieces[1], 'casa pequena': pieces[2],
  'coluna': pieces[3], 'muro (3 ladrilhos)': pieces[4], 'Odisseu': hero,
})) console.log(`  ${n.padEnd(20)} ${String(piece.width).padStart(3)}x${String(piece.height).padStart(3)}px = ${C.un(piece.width)} x ${C.un(piece.height)} un`);
