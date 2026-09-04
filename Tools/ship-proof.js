// Prova do navio: o Odisseu em pé no convés, na linha publicada pelo build-ship,
// mais uma cópia no chão ao lado para comparar. É o requisito do ShipDeparture.
const path = require('path'), fs = require('fs');
const C = require('./compose.js');
const p = C.p;

const shipFile = process.argv[2] || path.join(C.ENV, 'Ships/ithaca_ship_01.png');
const out = process.argv[3] || path.join(C.ENV, 'Ships/_prova_escala.png');
const ship = p.read(shipFile);
const meta = JSON.parse(fs.readFileSync(shipFile.replace(/[.]png$/, '.json'), 'utf8'));

const sheet = p.read(path.join(C.ROOT, 'Assets/Resources/Odisseia/Characters/CHR_Odysseus.png'));
const idle = p.crop(sheet, 0, 0, 84, 84);
const ib = p.bounds(idle);
const hero = p.crop(idle, ib.x0, ib.y0, ib.w, ib.h);

const PAD = 24, W = ship.width + hero.width + PAD * 3, H = ship.height + PAD * 2 + 20;
const img = p.blank(W, H);
const sky = C.ramps['Ceu'][1], sea = C.ramps['Agua'][2];
for (let i = 0; i < W * H; i++) {
  const o = i * 4;
  img.data[o] = sky[0]; img.data[o + 1] = sky[1]; img.data[o + 2] = sky[2]; img.data[o + 3] = 255;
}
// Linha d'água na altura em que a quilha mergulha, para o navio não parecer boiando no ar.
const waterY = PAD + ship.height - 14;
C.fillRect(img, 0, waterY, W, H - waterY, sea);
C.fillRect(img, 0, waterY, W, 1, C.ramps['Agua'][1]);

p.blit(img, ship, PAD, PAD);
p.blit(img, hero, PAD + meta.deck.x - Math.round(hero.width / 2), PAD + meta.deck.y - hero.height);
p.blit(img, hero, PAD + ship.width + PAD, waterY - hero.height);
p.write(out, img);

console.log(`navio    ${ship.width}x${ship.height}px = ${C.un(ship.width)} x ${C.un(ship.height)} un`);
console.log(`Odisseu  ${hero.width}x${hero.height}px = ${C.un(hero.height)} un de altura`);
console.log(`convés   (${meta.deck.x}, ${meta.deck.y}) — silhueta apoiada na linha da borda`);
console.log(`razão    navio ${(ship.width / hero.height).toFixed(1)}x a altura do herói`);
