// Casa de Odisseu — a construção principal da fase. Mesmo vocabulário da casa pequena
// (calcário, telhado plano, porta de prancha), em escala de casa de chefe: pórtico de duas
// colunas e arquitrave sobre a entrada.
//
// Composta inteiramente de material já existente: parede e porta recortadas da v3, colunas
// do ithaca_column_01, laje pela receita da v2. Zero gerações.
const path = require('path');
const C = require('./compose.js');
const p = C.p;

const out = process.argv[2] || path.join(C.ENV, 'Architecture/ithaca_house_odysseus_01.png');
const R = C.ramps['Terra / caminho'], OUT = C.ramps['Contorno'][3];

const WALL_W = 300, WALL_H = 165, ROOF_H = 14, PLINTH_H = 8, OVERHANG = 7;
const H = ROOF_H + WALL_H + PLINTH_H;
const W = WALL_W + OVERHANG * 2;
const X0 = OVERHANG;                       // origem da parede dentro do canvas
const wallTop = ROOF_H, wallBot = ROOF_H + WALL_H - 1, ground = H - 1;

const img = p.blank(W, H);

// -- parede: espelho da faixa limpa. Duas bandas com offsets diferentes quebram a leitura
//    de repetição vertical que uma banda única produziria numa parede desta altura.
const strip = C.region('paredeLimpa');
const bandH = Math.ceil(WALL_H / 2);
// Offsets diferentes em Y (fiadas diferentes) e em X (o eixo do espelho não se alinha entre
// as bandas, senão aparece um ritmo vertical que denuncia o ladrilhamento).
// A banda de baixo usa uma fiada mais escura da fonte: a parede envelhece do chão para cima,
// e a diferença de tom disfarça a repetição melhor do que duas bandas iguais.
p.blit(img, C.mirrorTile(strip, WALL_W, bandH, 30, 0), X0, wallTop);
p.blit(img, C.mirrorTile(strip, WALL_W, WALL_H - bandH, 54, 29), X0, wallTop + bandH);
// Fiada de separação entre as bandas, para a junção virar detalhe de obra em vez de emenda.
C.fillRect(img, X0, wallTop + bandH - 1, WALL_W, 1, R[2]);
C.fillRect(img, X0, wallTop + bandH, WALL_W, 1, R[0]);

// -- porta ao centro, apoiada no plinto --
const door = C.region('porta');
const doorX = X0 + Math.round((WALL_W - door.width) / 2);
const doorY = wallBot - door.height + 1;
p.blit(img, door, doorX, doorY);

// -- janelas, fora do pórtico --
const win = C.region('janela');
const winY = wallTop + 46;
p.blit(img, win, X0 + 30, winY);
p.blit(img, win, X0 + WALL_W - 30 - win.width, winY);

// -- pórtico: arquitrave apoiada em duas colunas, à frente da parede --
const col = p.read(path.join(C.ENV, 'Architecture/ithaca_column_01.png'));
const colY = ground - PLINTH_H + 1 - col.height + PLINTH_H;   // base assenta no topo do plinto
const colLX = doorX - col.width - 6, colRX = doorX + door.width + 6;
const archY = colY - 11, archX = colLX - 4, archW = (colRX + col.width + 4) - archX;
C.fillRect(img, archX, archY, archW, 11, R[1]);
C.fillRect(img, archX, archY, archW, 2, R[0]);
C.fillRect(img, archX, archY + 9, archW, 1, R[2]);
C.fillRect(img, archX, archY - 1, archW, 1, OUT);
C.fillRect(img, archX, archY + 10, archW, 1, OUT);
p.blit(img, col, colLX, colY);
p.blit(img, col, colRX, colY);

// -- plinto: duas fiadas, mais largas que a parede, como a casa assentada no terreno --
C.fillRect(img, X0 - 3, wallBot + 1, WALL_W + 6, 1, OUT);
C.fillRect(img, X0 - 3, wallBot + 2, WALL_W + 6, 2, R[0]);
C.fillRect(img, X0 - 3, wallBot + 4, WALL_W + 6, 3, R[1]);
C.fillRect(img, X0 - 5, wallBot + 7, WALL_W + 10, 1, R[2]);
C.fillRect(img, X0 - 5, ground, WALL_W + 10, 1, OUT);

// -- laje de telhado plano --
p.blit(img, C.lajePlana(W, ROOF_H), 0, 0);
// Contorno lateral da parede, que a laje não cobre.
C.fillRect(img, X0 - 1, wallTop, 1, WALL_H, OUT);
C.fillRect(img, X0 + WALL_W, wallTop, 1, WALL_H, OUT);

const final = C.trim(img);
p.write(out, final);
console.log(`${path.relative(C.ROOT, out)}  ${final.width}x${final.height}px = ${C.un(final.width)} x ${C.un(final.height)} un`);
