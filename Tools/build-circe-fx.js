// Ceu, mata de fundo e VFX de Circe, por codigo.
//
//   node Tools/build-circe-fx.js
//
// Camada distante e efeito repetitivo vao de procedural: emenda exata, cor da rampa e custo
// zero. O que ficou para geracao foi o organico — arvore, samambaia, ruina, templo.
//
// Os VFX aqui ladrilham e deslizam com `ScrollingLayer`, o componente criado em Eolo. Nao ha
// sistema de particulas no projeto e o briefing permite a implementacao mais simples que
// funcione: duas faixas custam dois transforms por quadro, contra dezenas de GameObjects.
const path = require('path'), fs = require('fs');
const p = require('./png.js');
const { loadPalette } = require('./ramp-map.js');

const ROOT = path.join(__dirname, '..');
const ENV = path.join(ROOT, 'Docs/Environment_Circe');
const ramps = loadPalette(path.join(ENV, 'Palette/CIRCE_PALETTE.gpl'));
const SOL = ramps['Folha ao sol'], FOLHA = ramps['Folhagem'], SOMBRA = ramps['Sombra verde'];
const OURO = ramps['Luz dourada'], MARMORE = ramps['Marmore'], TRONCO = ramps['Tronco escuro'];
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
  p.write(path.join(dir, 'circe_' + nome + '.png'), img);
  console.log(nome.padEnd(16) + String(img.width + 'x' + img.height).padEnd(10) +
    (img.width / UN).toFixed(2) + 'x' + (img.height / UN).toFixed(2) + ' un   ' + nota);
};

const L = 512;

// ---------- luz que atravessa a copa ----------
// Nao e ceu azul: numa floresta fechada o que se ve la em cima e LUZ FILTRADA, dourada e
// abafada. Usar o azul de Cicones aqui abriria um buraco de ceu no meio da mata.
{
  const H = 384;
  const img = p.blank(L, H);
  for (let y = 0; y < H; y++) {
    const t = y / (H - 1);
    // Clareia para BAIXO: a luz entra pelo fundo da mata, na direcao do horizonte.
    for (let x = 0; x < L; x++) px(img, x, y, dither(OURO, 2.6 * (1 - t), x, y));
  }
  gravar('Background', 'canopy_light', img, 'luz filtrada, nao ceu: clareia para baixo');
}

// ---------- troncos ao fundo ----------
// Silhueta em dois tons, sem folha nenhuma. E a repeticao vertical de troncos que le como
// "mata funda"; desenhar copa aqui competiria com as arvores do plano de jogo.
for (const cfg of [['forest_far', 224, 3, 0.55, 41], ['forest_mid', 288, 5, 1.0, 83]]) {
  const nome = cfg[0], H = cfg[1], passo = cfg[2], escala = cfg[3], semente = cfg[4];
  const img = p.blank(L, H);
  const rnd = aleatorio(semente);
  const rampa = nome === 'forest_far' ? SOMBRA : TRONCO;

  let x = 0;
  while (x < L) {
    const larg = Math.round((7 + rnd() * 13) * escala) + passo;
    const alt = Math.round(H * (0.55 + rnd() * 0.45));
    for (let dx = 0; dx < larg && x + dx < L; dx++) {
      // Cilindro: a borda escurece. Sem isso a mata vira um pente de barras chapadas.
      const t = dx / Math.max(1, larg - 1);
      const c = t < 0.18 || t > 0.82 ? rampa[3] : t < 0.34 ? rampa[2] : rampa[1];
      for (let y = H - alt; y < H; y++) px(img, x + dx, y, c);
    }
    x += larg + Math.round((3 + rnd() * 9) * escala);
  }
  // Copa fechando o topo, para a mata nao terminar numa linha reta de topos de tronco.
  for (let i = 0; i < 90; i++) {
    const cx = rnd() * L, cy = rnd() * H * 0.3, r = 10 + rnd() * 22 * escala;
    for (let y = -r; y <= r; y++) for (let dx = -r; dx <= r; dx++) {
      if (dx * dx + y * y > r * r) continue;
      px(img, (cx + dx + L) % L, cy + y, rampa[nome === 'forest_far' ? 1 : 2]);
    }
  }
  gravar('Background', nome, img, 'troncos em silhueta de dois tons, sem folha');
}

// ---------- feixes de luz ----------
// Faixas inclinadas de alpha baixo que ladrilham. E o efeito que mais diz "floresta encantada"
// e o mais barato: um sprite, uma camada, nenhuma luz dinamica.
{
  const T = 256;
  const img = p.blank(T, T);
  const rnd = aleatorio(1234);
  for (let i = 0; i < 7; i++) {
    const x0 = rnd() * T, larg = 10 + rnd() * 26;
    for (let y = 0; y < T; y++) {
      // Inclinacao 1:2 com envoltoria em cosseno: o feixe some nas bordas em vez de terminar
      // num corte reto, que leria como fita adesiva.
      const cx = (x0 + y * 0.5) % T;
      for (let d = -larg; d <= larg; d++) {
        const env = Math.cos((d / larg) * Math.PI * 0.5);
        const a = Math.round(46 * env * env * (0.55 + 0.45 * (1 - y / T)));
        if (a > 4) px(img, (cx + d + T) % T, y, OURO[0], a);
      }
    }
  }
  gravar('VFX', 'light_shafts', img, 'inclinacao 1:2 que ladrilha; alpha baixo');
}

// ---------- particulas ----------
// Poeira de luz e petalas. Densidade baixa de proposito: o briefing pede magia SUTIL, e
// particula demais vira neon.
{
  const T = 256;
  const img = p.blank(T, T);
  const rnd = aleatorio(99);
  for (let i = 0; i < 54; i++) {
    const x = Math.floor(rnd() * T), y = Math.floor(rnd() * T);
    if (rnd() > 0.6) {
      px(img, x, y, MARMORE[0], 210);
      px(img, x + 1, y, MARMORE[1], 150);
    } else {
      px(img, x, y, SOL[0], 130);
    }
  }
  gravar('VFX', 'motes', img, 'poeira de luz; densidade baixa de proposito');
}

// ---------- rasteira de samambaia para o primeiro plano ----------
// Faixa que ladrilha, escura, para passar na frente da camera sem esconder ninguem: e a copa
// do mato, nao o mato inteiro.
{
  const H = 96;
  const img = p.blank(L, H);
  const rnd = aleatorio(707);
  for (let i = 0; i < 150; i++) {
    const x0 = rnd() * L, alt = 26 + rnd() * 60, curva = (rnd() - 0.5) * 26;
    const c = rnd() > 0.55 ? SOMBRA[0] : SOMBRA[1];
    for (let d = 0; d < alt; d++) {
      const t = d / alt;
      px(img, (x0 + curva * t * t + L) % L, H - 1 - d, c);
      if (d % 6 === 0) {
        // Foliolos: dois pixels para os lados a cada seis, que e o minimo que le como fronde.
        const w = Math.round(5 * (1 - t));
        for (let k = -w; k <= w; k++) px(img, (x0 + curva * t * t + k + L) % L, H - 1 - d, c);
      }
    }
  }
  gravar('VFX', 'undergrowth', img, 'rasteira de primeiro plano; ladrilha na horizontal');
}
