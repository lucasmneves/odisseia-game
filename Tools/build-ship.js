// Navio de Ítaca — o único asset do cenário que precisou de geração, e mesmo assim só o casco.
// Mastro, verga, vela, cordame e remos são desenhados por código: são geometria, e é em
// escala e simetria que o modelo falha.
//
// O casco veio de create_image_pixen (1 geração), recortado do fundo chapado por Tools/cutout.js
// e remapeado na paleta. Requisito de gameplay (README, ShipDeparture): vista lateral, convés
// onde a silhueta do jogador se apoia, e o navio zarpa como um objeto só.
const path = require('path'), fs = require('fs');
const C = require('./compose.js');
const { remap } = require('./ramp-map.js');
const p = C.p;

// --furled: vela enrolada na verga, para o navio atracado no cais durante o ato do porto.
// A partida usa a vela içada. São dois estados do mesmo navio, não dois navios.
const FURLED = process.argv.includes('--furled');
const args = process.argv.slice(2).filter(a => a !== '--furled');
const out = args[0] || path.join(C.ENV, FURLED ? 'Ships/ithaca_ship_01_furled.png' : 'Ships/ithaca_ship_01.png');
const M = C.ramps['Madeira'], T = C.ramps['Terra / caminho'], OUT = C.ramps['Contorno'][3];

const hull = remap(p.read(path.join(C.ENV, 'Ships/_hull_v3_cut.png')), C.ramps);
const DECK = 88;                            // linha da borda no casco, medida
const RINGS = [116, 155, 194, 233, 273];    // centros das argolas de remo
const RING_Y = 91;

const MAST_LEN = 190, MAST_X = 195, TOP = 8, BOTTOM = 22;   // BOTTOM: espaço para as pás dos remos
const HULL_Y = MAST_LEN + TOP - DECK;
const W = hull.width, H = HULL_Y + hull.height + BOTTOM;
const img = p.blank(W, H);
const mastTop = TOP, mastBase = HULL_Y + DECK;

// Bayer 4x4 — o mesmo dithering ordenado usado no céu e no oceano. Faz a barriga da vela
// parecer degradê sem sair da paleta; sem ele o sombreamento sai em blocos verticais duros.
const BAYER = [[0, 8, 2, 10], [12, 4, 14, 6], [3, 11, 1, 9], [15, 7, 13, 5]];
// s é o índice contínuo na rampa. Comparar a fração com o limiar SEM somar degrau quando
// ela é zero — o erro inverso cria uma linha dura em cada junção (já cometido no céu).
function dither(ramp, s, x, y) {
  const i = Math.min(ramp.length - 1, Math.floor(s));
  const frac = s - i;
  const th = (BAYER[y & 3][x & 3] + 0.5) / 16;
  return ramp[Math.min(ramp.length - 1, i + (frac > th ? 1 : 0))];
}
const vline = (x, y0, y1, c) => C.fillRect(img, x, y0, 1, y1 - y0 + 1, c);
function rope(x0, y0, x1, y1, c) {
  const n = Math.max(Math.abs(x1 - x0), Math.abs(y1 - y0));
  for (let i = 0; i <= n; i++)
    C.fillRect(img, Math.round(x0 + (x1 - x0) * i / n), Math.round(y0 + (y1 - y0) * i / n), 1, 1, c);
}

// -- 1. cordame: estais da ponta do mastro para proa e popa (ficam atrás de tudo) --
rope(MAST_X, mastTop + 2, 40, HULL_Y + 96, M[2]);
rope(MAST_X, mastTop + 2, 352, HULL_Y + 96, M[2]);

// -- 2. mastro, ANTES da vela: a vela corre à frente do mastro, então o pau só aparece
//       acima da verga e abaixo da bainha. Desenhá-lo por cima parte a vela ao meio. --
for (let x = -2; x <= 2; x++) vline(MAST_X + x, mastTop, mastBase, M[Math.abs(x) === 0 ? 0 : Math.abs(x) === 1 ? 1 : 2]);
vline(MAST_X - 3, mastTop, mastBase, OUT);
vline(MAST_X + 3, mastTop, mastBase, OUT);

// -- 3. vela --
const SAIL_W = 196, SAIL_H = FURLED ? 16 : 140, SAIL_Y = TOP + 14;
const sailX = MAST_X - Math.round(SAIL_W / 2);
if (FURLED) {
  // Rolo do pano amarrado sob a verga: barriga no meio, afinando nas pontas.
  for (let x = 0; x < SAIL_W; x++) {
    const t = x / (SAIL_W - 1);
    const th = Math.round(SAIL_H * (0.55 + 0.45 * Math.sin(Math.PI * t)));
    for (let y = 0; y < th; y++) {
      const d = Math.abs(y / Math.max(1, th - 1) - 0.4) * 2;
      C.fillRect(img, sailX + x, SAIL_Y + y, 1, 1, dither(T, Math.min(3, d * d * 2.6), x, y));
    }
    C.fillRect(img, sailX + x, SAIL_Y + th, 1, 1, OUT);
  }
  // Voltas do cabo que prendem o rolo.
  for (let x = 22; x < SAIL_W; x += 34) C.fillRect(img, sailX + x, SAIL_Y - 1, 2, SAIL_H + 2, M[3]);
} else {
  for (let y = 0; y < SAIL_H; y++)
    for (let x = 0; x < SAIL_W; x++) {
      const d = Math.abs(x / (SAIL_W - 1) - 0.5) * 2;      // 0 no eixo, 1 nas bordas
      // Expoente 3, não 2: com d*d quase toda a vela cai na zona de transição e o dithering
      // lê como chiado. Com d^3 o centro fica em cor chapada e só a beirada degrada.
      let s = d * d * d * 2.6;
      if (y > SAIL_H - 8) s += 0.8;                         // sombra na bainha inferior
      C.fillRect(img, sailX + x, SAIL_Y + y, 1, 1, dither(T, Math.min(3, s), x, y));
    }
  // Costuras do pano.
  for (let x = 28; x < SAIL_W; x += 28) vline(sailX + x, SAIL_Y + 1, SAIL_Y + SAIL_H - 2, T[2]);
  // Faixa decorativa — único ornamento; a seção 5 do briefing pede pouco detalhe longe.
  C.fillRect(img, sailX, SAIL_Y + 44, SAIL_W, 6, M[0]);
  C.fillRect(img, sailX, SAIL_Y + 44, SAIL_W, 1, M[3]);
  C.fillRect(img, sailX, SAIL_Y + 49, SAIL_W, 1, M[3]);
}
// Contorno da vela.
C.fillRect(img, sailX - 1, SAIL_Y - 1, SAIL_W + 2, 1, OUT);
if (!FURLED) {
  C.fillRect(img, sailX - 1, SAIL_Y + SAIL_H, SAIL_W + 2, 1, OUT);
  vline(sailX - 1, SAIL_Y - 1, SAIL_Y + SAIL_H, OUT);
  vline(sailX + SAIL_W, SAIL_Y - 1, SAIL_Y + SAIL_H, OUT);
}

// -- 4. verga, por cima da testa da vela (é nela que o pano é amarrado) --
C.fillRect(img, sailX - 12, SAIL_Y - 6, SAIL_W + 24, 6, M[1]);
C.fillRect(img, sailX - 12, SAIL_Y - 6, SAIL_W + 24, 1, M[0]);
C.fillRect(img, sailX - 12, SAIL_Y - 1, SAIL_W + 24, 1, M[3]);
C.fillRect(img, sailX - 13, SAIL_Y - 7, SAIL_W + 26, 1, OUT);

// -- 5. casco, por cima do cordame e do pé do mastro --
p.blit(img, hull, 0, HULL_Y);

// -- 6. remos: saem das argolas para baixo e para a popa (a proa é a direita) --
for (const rx of RINGS) {
  const x0 = rx, y0 = HULL_Y + RING_Y, ang = 0.72, len = 78;
  const x1 = Math.round(x0 - len * Math.cos(ang)), y1 = Math.round(y0 + len * Math.sin(ang));
  // Cabo de 3px: um risco de 1px some contra o céu neste tamanho.
  rope(x0, y0 - 1, x1, y1 - 1, M[1]);
  rope(x0, y0, x1, y1, M[2]);
  rope(x0, y0 + 1, x1, y1 + 1, M[3]);
  for (let i = 0; i < 16; i++) {                     // pá
    const t = i / 15, ww = 3 + Math.round(3 * Math.sin(Math.PI * t));
    const xx = Math.round(x1 - 16 * Math.cos(ang) * t), yy = Math.round(y1 + 16 * Math.sin(ang) * t);
    C.fillRect(img, xx - ww, yy, ww * 2, 2, M[1]);
    C.fillRect(img, xx - ww, yy, ww * 2, 1, M[0]);
  }
}

const b = p.bounds(img);
const final = p.crop(img, b.x0, b.y0, b.w, b.h);
p.write(out, final);

// A linha do convés é o que o ShipDeparture precisa: é nela que a silhueta do jogador se
// apoia. Publicada aqui porque o recorte desloca as coordenadas e adivinhar dá erro de pixel.
const deck = { x: MAST_X - b.x0, y: HULL_Y + DECK - b.y0 };
fs.writeFileSync(out.replace(/[.]png$/, '.json'), JSON.stringify({
  size: [final.width, final.height],
  unidades: [+C.un(final.width), +C.un(final.height)],
  deck: deck,
  proa: 'direita',
  nota: 'deck.y e a linha da borda; a silhueta do jogador apoia os pes nela. deck.x e o pe do mastro.',
}, null, 2) + String.fromCharCode(10));

console.log(`${path.relative(C.ROOT, out)}  ${final.width}x${final.height}px = ${C.un(final.width)} x ${C.un(final.height)} un  (alvo 8-12 un de comprimento)`);
console.log(`  convés em (${deck.x}, ${deck.y}); proa à direita`);
