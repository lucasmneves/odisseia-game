// Muro baixo de pedra seca — alvo do briefing: ≈ 0,8 un de altura.
// Tudo recortado da parede da casa v3; nenhuma geração nova.
const path = require('path');
const C = require('./compose.js');
const p = C.p;

const H = 34;            // 0,79 un
const TILE = 58;         // período do espelho de uma faixa de 30px: 2*30 - 2
const FACE_OFFSET = 30;  // fiada com a melhor irregularidade das candidatas medidas
const out = process.argv[2] || path.join(C.ENV, 'Architecture/ithaca_wall_low_01.png');

const CAP = 4;           // coroamento
const img = p.blank(TILE, H);
p.blit(img, C.mirrorTile(C.region('paredeLimpa'), TILE, H - CAP - 1, FACE_OFFSET), 0, CAP + 1);

// Coroamento em Pedra fria: é como o briefing marca superfície pisável (seção 36).
// Sem ele o muro lê como parede cortada, não como muro.
C.fillRect(img, 0, 1, TILE, 1, C.ramps['Contorno'][3]);
C.fillRect(img, 0, 2, TILE, 1, C.ramps['Pedra fria'][0]);
C.fillRect(img, 0, 3, TILE, 1, C.ramps['Pedra fria'][1]);
C.fillRect(img, 0, 4, TILE, 1, C.ramps['Pedra fria'][2]);
// Juntas do coroamento. O passo 11 não divide 46, então a emenda do espelho não cai numa junta.
for (let x = 5; x < TILE; x += 11) C.fillRect(img, x, 2, 1, 2, C.ramps['Pedra fria'][2]);
C.fillRect(img, 0, H - 1, TILE, 1, C.ramps['Contorno'][3]);

p.write(out, img);
console.log(`${path.relative(C.ROOT, out)}  ${img.width}x${img.height}px = ${C.un(img.width)} x ${C.un(img.height)} un  (alvo 0,8 de altura)`);
