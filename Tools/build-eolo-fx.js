// Ceu, nuvens e vento de Eolo, por codigo.
//
//   node Tools/build-eolo-fx.js
//
// O briefing pede folhas voando, poeira e linhas de vento, e permite uma implementacao simples
// se nao houver sistema de particulas — nao ha. A escolha aqui e FAIXA QUE LADRILHA em vez de
// particula: uma camada com ScrollingLayer custa um transform por quadro e nenhum GameObject
// por particula, contra dezenas de objetos animados de um sistema de particulas. E o que o
// paragrafo de performance pede para WebGL e mobile.
const path = require('path'), fs = require('fs');
const p = require('./png.js');
const { loadPalette } = require('./ramp-map.js');

const ROOT = path.join(__dirname, '..');
const ENV = path.join(ROOT, 'Docs/Environment_Eolo');
const ramps = loadPalette(path.join(ENV, 'Palette/EOLO_PALETTE.gpl'));
const CEU = ramps['Ceu de altitude'], FUNDO = ramps['Ceu profundo'];
const NUVEM = ramps['Nuvem'], GRAMA = ramps['Grama'];
const UN = 42.857143;

const BAYER = [[0, 8, 2, 10], [12, 4, 14, 6], [3, 11, 1, 9], [15, 7, 13, 5]];
function dither(rampa, s, x, y) {
  const i = Math.min(rampa.length - 1, Math.floor(s));
  const frac = s - i;
  const th = (BAYER[y & 3][x & 3] + 0.5) / 16;
  return rampa[Math.min(rampa.length - 1, i + (frac > th ? 1 : 0))];
}

const px = (img, x, y, c, a = 255) => {
  x = Math.round(x); y = Math.round(y);
  if (x < 0 || y < 0 || x >= img.width || y >= img.height) return;
  const o = (y * img.width + x) * 4;
  img.data[o] = c[0]; img.data[o + 1] = c[1]; img.data[o + 2] = c[2]; img.data[o + 3] = a;
};

function aleatorio(semente) {
  let s = semente;
  return () => ((s = (s * 1103515245 + 12345) & 0x7fffffff) / 0x7fffffff);
}

const gravar = (grupo, nome, img, nota) => {
  const dir = path.join(ENV, grupo);
  fs.mkdirSync(dir, { recursive: true });
  p.write(path.join(dir, 'eolo_' + nome + '.png'), img);
  console.log(nome.padEnd(16) + String(img.width + 'x' + img.height).padEnd(10) +
    (img.width / UN).toFixed(2) + 'x' + (img.height / UN).toFixed(2) + ' un   ' + nota);
};

const L = 512;

// ---------- ceu de altitude ----------
// Escurece PARA CIMA. E a inversao do ceu de Cicones, e e o que faz a mesma engine de fundo ler
// como "alto" em vez de "meio-dia ao nivel do mar": em altitude o zenite e mais profundo.
{
  const H = 512;
  const img = p.blank(L, H);
  for (let y = 0; y < H; y++) {
    const t = y / (H - 1);
    for (let x = 0; x < L; x++) {
      const c = t < 0.5
        ? dither(FUNDO, 2.6 * (1 - t / 0.5), x, y)
        : dither(CEU, 2.6 * (1 - (t - 0.5) / 0.5), x, y);
      px(img, x, y, c);
    }
  }
  gravar('Background', 'sky_high', img, 'escurece para cima: e assim que altitude le');
}

// ---------- bancos de nuvem ----------
// Silhueta em tres tons e SEM contorno: nuvem com contorno preto vira balao de quadrinhos. O
// que a faz ler como nuvem e a borda de cima em bolhas contra a de baixo reta.
for (const cfg of [['clouds_far', 128, 1.0, 31], ['clouds_near', 176, 1.6, 97]]) {
  const nome = cfg[0], H = cfg[1], escala = cfg[2], semente = cfg[3];
  const img = p.blank(L, H);
  const rnd = aleatorio(semente);
  const BASE = H - 6;
  for (let k = 0; k < 46; k++) {
    const cx = rnd() * L, r = (12 + rnd() * 26) * escala;
    const cy = BASE - rnd() * (H * 0.5);
    for (let y = -r; y <= r; y++) {
      for (let x = -r; x <= r; x++) {
        if (x * x + y * y > r * r) continue;
        const yy = cy + y;
        if (yy > BASE) continue;
        // Tres tons por altura dentro da bolha: o topo pega luz, a barriga fica na sombra.
        const t = (y + r) / (2 * r);
        px(img, (cx + x + L) % L, yy, t < 0.35 ? NUVEM[0] : t < 0.72 ? NUVEM[1] : NUVEM[2]);
      }
    }
  }
  for (let x = 0; x < L; x++) for (let y = BASE; y < H; y++) px(img, x, y, NUVEM[2]);
  gravar('Background', nome, img, 'silhueta em tres tons, sem contorno');
}

// ---------- linhas de vento ----------
// Ladrilha nos dois eixos e desliza com ScrollingLayer. Arcos longos e finos, nao riscas retas:
// vento se le pela CURVA, e risca reta le como chuva — que e a fase 04.
{
  const T = 256;
  const img = p.blank(T, T);
  const rnd = aleatorio(555);
  for (let i = 0; i < 26; i++) {
    const y0 = rnd() * T, comp = 60 + rnd() * 150, x0 = rnd() * T;
    const amp = 4 + rnd() * 10, freq = 0.02 + rnd() * 0.03;
    const forte = rnd() > 0.6;
    for (let d = 0; d < comp; d++) {
      const x = (x0 + d) % T;
      const y = (y0 + Math.sin((x0 + d) * freq) * amp + T) % T;
      // A linha some nas pontas: terminada num corte seco ela le como risco de caneta.
      const borda = Math.min(d, comp - d) / (comp * 0.35);
      const a = Math.round((forte ? 150 : 90) * Math.min(1, borda));
      if (a > 6) px(img, x, y, NUVEM[0], a);
    }
  }
  gravar('Wind', 'wind_lines', img, 'arcos que ladrilham; deslizam com ScrollingLayer');
}

// ---------- folhas e poeira ----------
{
  const T = 256;
  const img = p.blank(T, T);
  const rnd = aleatorio(808);
  for (let i = 0; i < 70; i++) {
    const x = Math.floor(rnd() * T), y = Math.floor(rnd() * T);
    if (rnd() > 0.45) {
      // Folha: tres pixels na diagonal, o minimo que le como algo girando no ar.
      const c = rnd() > 0.5 ? GRAMA[0] : GRAMA[1];
      px(img, x, y, c, 210); px(img, x + 1, y, c, 210); px(img, x + 1, y + 1, c, 170);
    } else {
      px(img, x, y, NUVEM[1], 130);
    }
  }
  gravar('Wind', 'wind_motes', img, 'folhas e poeira; densidade baixa de proposito');
}
