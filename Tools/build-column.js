// Coluna dórica de pedra — 2,5 un. Desenhada por código: é objeto geométrico, e o modelo
// não controla escala nem simetria com confiabilidade.
//
// Sombreamento de FORMA, não luz direcional (seção 4 de ITHACA_ENVIRONMENT.md): a coluna é
// um cilindro, então clareia no eixo e escurece nas bordas, sem lado iluminado.
const path = require('path');
const C = require('./compose.js');
const p = C.p;

const out = process.argv[2] || path.join(C.ENV, 'Architecture/ithaca_column_01.png');
const R = C.ramps['Terra / caminho'];        // calcário quente, a mesma rampa das paredes
const OUT = C.ramps['Contorno'][3];

const H = 107;                               // 2,50 un
const ABACUS = 26, SHAFT_BOT = 20, SHAFT_TOP = 17, FLUTES = 5;
const W = ABACUS + 2;
const img = p.blank(W, H);
const cx = W / 2;

// Índice da rampa pela distância ao eixo do cilindro.
const shade = (t) => { const d = Math.abs(t - 0.5) * 2; return d < 0.34 ? 0 : d < 0.68 ? 1 : d < 0.9 ? 2 : 3; };

// -- capitel: ábaco (laje quadrada) + equino (a curva que recebe a carga) --
const capTop = 0;
C.fillRect(img, Math.round(cx - ABACUS / 2), capTop + 1, ABACUS, 5, R[1]);
C.fillRect(img, Math.round(cx - ABACUS / 2), capTop + 1, ABACUS, 2, R[0]);
for (let i = 0; i < 6; i++) {                            // equino: alarga de baixo para cima
  const w = Math.round(SHAFT_TOP + (ABACUS - SHAFT_TOP) * (i / 5));
  const y = capTop + 11 - i;
  for (let x = 0; x < w; x++) {
    const px = Math.round(cx - w / 2) + x;
    const c = R[shade(x / (w - 1))];
    C.fillRect(img, px, y, 1, 1, c);
  }
}
const shaftTop = capTop + 13, shaftBot = H - 8;

// -- fuste: estreita para cima, com 5 caneluras --
for (let y = shaftTop; y < shaftBot; y++) {
  const t = (y - shaftTop) / (shaftBot - shaftTop - 1);
  const w = Math.round(SHAFT_TOP + (SHAFT_BOT - SHAFT_TOP) * t);
  const x0 = Math.round(cx - w / 2);
  for (let x = 0; x < w; x++) {
    let idx = shade(x / (w - 1));
    // Canelura: um passo mais escuro na borda de cada estria.
    const flute = (x / w) * FLUTES;
    if (Math.abs(flute - Math.round(flute)) < 0.11 && x > 0 && x < w - 1) idx = Math.min(3, idx + 1);
    C.fillRect(img, x0 + x, y, 1, 1, R[idx]);
  }
}
// Colarinho: sulco que separa fuste e capitel — sem ele os dois viram um bloco só.
C.fillRect(img, Math.round(cx - SHAFT_TOP / 2), shaftTop, SHAFT_TOP, 1, R[3]);

// -- base: plinto de duas fiadas --
C.fillRect(img, Math.round(cx - (SHAFT_BOT + 2) / 2), shaftBot, SHAFT_BOT + 2, 3, R[1]);
C.fillRect(img, Math.round(cx - (SHAFT_BOT + 5) / 2), shaftBot + 3, SHAFT_BOT + 5, 4, R[1]);
C.fillRect(img, Math.round(cx - (SHAFT_BOT + 5) / 2), shaftBot + 3, SHAFT_BOT + 5, 1, R[0]);
C.fillRect(img, Math.round(cx - (SHAFT_BOT + 5) / 2), shaftBot + 6, SHAFT_BOT + 5, 1, R[2]);

const final = C.trim(C.outline(img));
p.write(out, final);
console.log(`${path.relative(C.ROOT, out)}  ${final.width}x${final.height}px = ${C.un(final.width)} x ${C.un(final.height)} un`);
