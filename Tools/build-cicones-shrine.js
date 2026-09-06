// Altar costeiro de Cícones — o landmark da fase (§4.8), desenhado por código.
//
//   node Tools/build-cicones-shrine.js
//
// Por que não gerar: duas gerações foram gastas e as duas falharam do mesmo jeito. A primeira
// virou uma lareira dentro de uma caverna; a segunda saiu em perspectiva isométrica com fundo
// marrom opaco, ignorando `no_background`. A causa é conhecida e está registrada em
// `pixellab-armadilhas-de-prompt`: descrição com estrutura ("plataforma de degraus, postes
// sustentando uma viga") é lida como CENA, e cena traz chão, parede e perspectiva junto.
//
// Objeto pequeno e geométrico é onde o código ganha — já foi assim para a coluna e o portão de
// Ítaca e para a flecha. Aqui garante de graça as três coisas que o modelo errou: perfil reto,
// fundo transparente e paleta exata.
const path = require('path'), fs = require('fs');
const p = require('./png.js');
const { loadPalette } = require('./ramp-map.js');

const ROOT = path.join(__dirname, '..');
const ramps = loadPalette(path.join(ROOT, 'Docs/Environment_Cicones/Palette/CICONES_PALETTE.gpl'));
const CLARA = ramps['Pedra clara'];
const CINZA = ramps['Pedra cinza'];
const BRONZE = ramps['Bronze'];
const FOGO = ramps['Fogo'];
const MUSGO = ramps['Oliveira'];
const OUT = ramps['Contorno'][3];

const UN = 42.857143;
const W = 216, H = 176;          // 5,04 x 4,11 un — cabe na tela sem dominá-la
const img = p.blank(W, H);

const px = (x, y, c) => {
  x = Math.round(x); y = Math.round(y);
  if (x < 0 || y < 0 || x >= W || y >= H) return;
  const o = (y * W + x) * 4;
  img.data[o] = c[0]; img.data[o + 1] = c[1]; img.data[o + 2] = c[2]; img.data[o + 3] = 255;
};

/**
 * Bloco de pedra com sombreamento de FORMA: clara na fiada de cima, base no miolo, sombra na
 * de baixo. Não é luz vinda de um canto — é a regra medida no master do personagem, e é ela
 * que mantém o cenário coerente com quem anda na frente dele.
 */
function bloco(x0, y0, w, h, rampa) {
  for (let y = 0; y < h; y++) {
    const t = h <= 2 ? 1 : y / (h - 1);
    const c = t < 0.18 ? rampa[0] : t < 0.62 ? rampa[1] : t < 0.88 ? rampa[2] : rampa[3];
    for (let x = 0; x < w; x++) px(x0 + x, y0 + y, c);
  }
}

/** Juntas da alvenaria: sem elas o bloco vira uma barra chapada no tamanho do jogo. */
function juntas(x0, y0, w, h, passo, rampa) {
  for (let y = y0 + passo; y < y0 + h; y += passo) {
    for (let x = x0; x < x0 + w; x++) px(x, y, rampa[3]);
    // Junta vertical alternada por fiada, para não formar uma coluna contínua.
    const desloc = ((y - y0) / passo) % 2 ? Math.floor(passo * 0.9) : 0;
    for (let x = x0 + desloc; x < x0 + w; x += passo * 2) {
      for (let d = 0; d < passo && y - d > y0; d++) px(x, y - d, rampa[3]);
    }
  }
}

// ---- três degraus, o mais largo embaixo ----
const DEG = [[0, 158, 216, 18], [16, 142, 184, 16], [32, 128, 152, 14]];
for (const [x, y, w, h] of DEG) { bloco(x, y, w, h, CLARA); juntas(x, y, w, h, 7, CLARA); }

// ---- quatro postes de topo plano, sem capitel: é o que separa isto de um templo romano ----
const TOPO_PLAT = 128;
const POSTE_H = 74, POSTE_W = 20;
const POSTES = [40, 78, 118, 156];
for (const x of POSTES) {
  bloco(x, TOPO_PLAT - POSTE_H, POSTE_W, POSTE_H, CLARA);
  juntas(x, TOPO_PLAT - POSTE_H, POSTE_W, POSTE_H, 12, CLARA);
  // Ábaco: uma laje lisa e um pouco mais larga no topo do poste.
  bloco(x - 3, TOPO_PLAT - POSTE_H - 7, POSTE_W + 6, 7, CLARA);
}

// ---- viga reta atravessando os postes; nunca arco ----
const VIGA_Y = TOPO_PLAT - POSTE_H - 20;
bloco(34, VIGA_Y, 148, 13, CLARA);
juntas(34, VIGA_Y, 148, 13, 13, CLARA);
// Coroamento na MESMA rampa do corpo. Numa rampa fria ele lia como uma laje solta pousada
// em cima, porque a diferença de matiz separa mais que a de luminância.
bloco(30, VIGA_Y - 6, 156, 6, CLARA);
for (let x = 30; x < 186; x++) px(x, VIGA_Y - 6, CLARA[0]);

// ---- altar entre os postes, com o topo enegrecido de fuligem ----
const ALT_X = 84, ALT_W = 48, ALT_H = 30, ALT_Y = TOPO_PLAT - ALT_H;
bloco(ALT_X, ALT_Y, ALT_W, ALT_H, CINZA);
juntas(ALT_X, ALT_Y, ALT_W, ALT_H, 10, CINZA);
bloco(ALT_X - 4, ALT_Y - 6, ALT_W + 8, 6, CLARA);
for (let x = ALT_X - 2; x < ALT_X + ALT_W + 2; x++) { px(x, ALT_Y - 6, OUT); px(x, ALT_Y - 5, OUT); }

// ---- chama ----
// Duas camadas, não um cone chapado: a primeira versão era um triângulo sólido e lia como um
// cone de trânsito. Fogo se reconhece pelo NÚCLEO claro dentro de um envelope escuro, e pela
// silhueta que estreita rápido e balança, não pela cor sozinha.
const CH_X = ALT_X + ALT_W / 2, CH_BASE = ALT_Y - 6, CH_ALT = 26;
const ondula = t => Math.round(Math.sin(t * 5.2) * 2.4 + Math.sin(t * 11) * 1.1);
for (let i = 0; i < CH_ALT; i++) {
  const t = i / (CH_ALT - 1);
  const dx = ondula(t);
  // Envelope: largo na base, some em ponta fina no topo.
  const fora = Math.max(0, Math.round(6.5 * Math.pow(1 - t, 0.75) - t * 1.2));
  for (let d = -fora; d <= fora; d++) {
    px(CH_X + dx + d, CH_BASE - i, t < 0.45 ? FOGO[3] : t < 0.8 ? FOGO[2] : FOGO[1]);
  }
  // Núcleo: sempre mais estreito que o envelope, e sobe menos alto.
  const dentro = Math.max(0, Math.round(fora - 1.8 - t * 2.2));
  for (let d = -dentro; d <= dentro; d++) {
    px(CH_X + dx + d, CH_BASE - i, t < 0.55 ? FOGO[1] : FOGO[0]);
  }
}

// ---- trípode de bronze ao lado ----
const TRI_X = 168, TRI_Y = TOPO_PLAT - 26;
for (let i = 0; i < 18; i++) {
  px(TRI_X - 7 + Math.round(i * 0.38), TRI_Y + 8 + i, BRONZE[2]);
  px(TRI_X + 7 - Math.round(i * 0.38), TRI_Y + 8 + i, BRONZE[2]);
  px(TRI_X, TRI_Y + 8 + i, BRONZE[3]);
}
bloco(TRI_X - 11, TRI_Y, 23, 9, BRONZE);
for (let x = TRI_X - 9; x <= TRI_X + 9; x++) px(x, TRI_Y, BRONZE[0]);

// ---- líquen: manchas fixas, não aleatórias, para o asset sair igual a cada reconstrução ----
let s = 20260905;
const rnd = () => ((s = (s * 1103515245 + 12345) & 0x7fffffff) / 0x7fffffff);
for (let i = 0; i < 90; i++) {
  const x = Math.floor(rnd() * W), y = Math.floor(rnd() * H);
  const o = (y * W + x) * 4;
  if (img.data[o + 3] < 8) continue;
  const claro = img.data[o] > 150;
  if (!claro) continue;
  px(x, y, MUSGO[2]); px(x + 1, y, MUSGO[2]);
  if (rnd() > 0.5) px(x, y + 1, MUSGO[3]);
}

// ---- contorno só na silhueta ----
const solido = (x, y) => x >= 0 && y >= 0 && x < W && y < H && img.data[(y * W + x) * 4 + 3] > 8;
const borda = [];
for (let y = 0; y < H; y++)
  for (let x = 0; x < W; x++)
    if (!solido(x, y) && (solido(x - 1, y) || solido(x + 1, y) || solido(x, y - 1) || solido(x, y + 1)))
      borda.push([x, y]);
for (const [x, y] of borda) px(x, y, OUT);

const b = p.bounds(img);
const final = p.crop(img, b.x0, b.y0, b.w, b.h);
const saida = path.join(ROOT, 'Docs/Environment_Cicones/_fontes/_fonte_shrine_landmark.png');
p.write(saida, final);
console.log(`shrine ${final.width}x${final.height}px = ${(final.width / UN).toFixed(2)} x ${(final.height / UN).toFixed(2)} un`);
