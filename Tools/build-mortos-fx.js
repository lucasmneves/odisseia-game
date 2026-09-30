// Rio, caverna de fundo, nevoa e almas do Mundo dos Mortos, por codigo.
//
//   node Tools/build-mortos-fx.js
//
// O rio gerado voltou como uma faixa fina entre duas bandas pretas, com a agua reduzida a uma
// linha verde. Agua plana com ondulacao horizontal e o mesmo caso do oceano de Cytera: forma
// repetitiva e geometrica, onde o codigo entrega emenda exata e cor da rampa de graca.
//
// Nevoa e almas ladrilham e deslizam com `ScrollingLayer`, o componente criado em Eolo. Nao ha
// sistema de particulas no projeto e o briefing permite a solucao mais simples: duas faixas
// custam dois transforms por quadro, contra dezenas de GameObjects.
const path = require('path'), fs = require('fs');
const p = require('./png.js');
const { loadPalette } = require('./ramp-map.js');

const ROOT = path.join(__dirname, '..');
const ENV = path.join(ROOT, 'Docs/Environment_MundoDosMortos');
const ramps = loadPalette(path.join(ENV, 'Palette/MORTOS_PALETTE.gpl'));
const FRIA = ramps['Pedra fria'], FUNDA = ramps['Pedra funda'], MARM = ramps['Marmore palido'];
const AGUA = ramps['Agua morta'], ALMA = ramps['Alma'], FOGO = ramps['Fogo'];
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
  p.write(path.join(dir, 'mortos_' + nome + '.png'), img);
  console.log(nome.padEnd(16) + String(img.width + 'x' + img.height).padEnd(10) +
    (img.width / UN).toFixed(2) + 'x' + (img.height / UN).toFixed(2) + ' un   ' + nota);
};

const L = 512;

// ---------- rio dos mortos ----------
// Ondulacoes por senos de periodo INTEIRO na largura, para a emenda fechar ao ladrilhar. Os
// reflexos de tocha sao poucos e quentes: sao eles que dizem que ha fogo fora do quadro, e
// sao a unica cor quente numa fase fria.
{
  const H = 128;
  const img = p.blank(L, H);
  const rnd = aleatorio(9091);
  for (let y = 0; y < H; y++) {
    const s = 0.4 + 2.4 * (y / (H - 1));   // clareia no topo, escurece no fundo
    for (let x = 0; x < L; x++) px(img, x, y, dither(AGUA, s, x, y));
  }
  for (let k = 0; k < 34; k++) {
    const yBase = 3 + Math.floor(rnd() * (H - 10));
    const ciclos = 1 + Math.floor(rnd() * 3);
    const amp = 1.5 + rnd() * 3;
    const comp = 40 + Math.floor(rnd() * 110);
    const x0 = Math.floor(rnd() * L);
    const claro = rnd() > 0.6;
    for (let d = 0; d < comp; d++) {
      const x = (x0 + d) % L;
      const y = yBase + Math.round(Math.sin((x / L) * Math.PI * 2 * ciclos) * amp);
      px(img, x, y, claro ? ALMA[2] : AGUA[0]);
    }
  }
  for (let k = 0; k < 7; k++) {
    const x0 = Math.floor(rnd() * L), yb = 6 + Math.floor(rnd() * (H - 20));
    for (let d = 0; d < 5 + rnd() * 9; d++) {
      px(img, (x0 + d) % L, yb + Math.round(Math.sin(d * 0.9) * 2), FOGO[2]);
    }
  }
  gravar('River', 'river_band', img, 'ladrilha nos dois eixos; deslocar em X anima o rio');
}

// ---------- caverna ao fundo ----------
// Silhueta em dois tons: estalagmites embaixo, teto irregular em cima, vazio no meio. E o
// vazio que faz a caverna ler como profunda em vez de parede.
{
  const H = 256;
  const img = p.blank(L, H);
  const rnd = aleatorio(313);
  for (let x = 0; x < L; x++) {
    const t = x / L * Math.PI * 2;
    const teto = Math.round(34 + 18 * Math.sin(t * 1.7) + 9 * Math.sin(t * 4.3 + 1));
    const chao = Math.round(46 + 22 * Math.sin(t * 2.1 + 2) + 11 * Math.sin(t * 5.7));
    for (let y = 0; y < teto; y++) px(img, x, y, y < teto - 5 ? FUNDA[3] : FUNDA[1]);
    for (let y = H - chao; y < H; y++) px(img, x, y, y > H - chao + 5 ? FUNDA[3] : FUNDA[1]);
  }
  // Estalagmites e estalactites soltas, para o vao nao virar duas linhas paralelas.
  for (let i = 0; i < 26; i++) {
    const x = Math.floor(rnd() * L), alt = 20 + Math.floor(rnd() * 60), cima = rnd() > 0.5;
    for (let d = 0; d < alt; d++) {
      const w = Math.max(0, Math.round(5 * (1 - d / alt)));
      for (let k = -w; k <= w; k++) {
        px(img, (x + k + L) % L, cima ? 34 + d : H - 46 - d, FUNDA[2]);
      }
    }
  }
  gravar('Background', 'cavern_far', img, 'silhueta de dois tons; o vazio no meio da profundidade');
}

// ---------- nevoa ----------
{
  const H = 128;
  const img = p.blank(L, H);
  const rnd = aleatorio(55);
  for (let i = 0; i < 130; i++) {
    const cx = rnd() * L, cy = H * (0.35 + rnd() * 0.65), rx = 18 + rnd() * 54, ry = 5 + rnd() * 13;
    for (let y = -ry; y <= ry; y++) {
      for (let x = -rx; x <= rx; x++) {
        if ((x * x) / (rx * rx) + (y * y) / (ry * ry) > 1) continue;
        // Alpha baixo e somado por sobreposicao: e a soma de manchas fracas que da nevoa, e
        // nao uma mancha forte, que leria como fumaca.
        px(img, (cx + x + L) % L, cy + y, MARM[1], 26);
      }
    }
  }
  gravar('VFX', 'mist', img, 'manchas fracas somadas; nunca esconde o jogador');
}

// ---------- almas ----------
// Pontinhos palidos com halo. Nao ha figura fantasmagorica desenhada de proposito: o briefing
// proibe gerar personagens, e luz flutuante le como alma sem desenhar ninguem.
{
  const T = 256;
  const img = p.blank(T, T);
  const rnd = aleatorio(777);
  for (let i = 0; i < 22; i++) {
    const cx = rnd() * T, cy = rnd() * T, r = 3 + rnd() * 4;
    for (let y = -r; y <= r; y++) {
      for (let x = -r; x <= r; x++) {
        const d = Math.sqrt(x * x + y * y);
        if (d > r) continue;
        const a = Math.round(150 * (1 - d / r) * (1 - d / r));
        if (a > 5) px(img, (cx + x + T) % T, (cy + y + T) % T, ALMA[0], a);
      }
    }
    px(img, cx, cy, ALMA[0], 235);
  }
  gravar('Souls', 'soul_lights', img, 'luzes flutuantes; nenhuma figura desenhada');
}
