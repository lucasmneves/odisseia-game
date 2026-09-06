// Camadas e efeitos de Cytera, por codigo.
//
//   node Tools/build-cytera-fx.js
//
// O paragrafo 11 do briefing pede que o mar seja dividido em elementos reutilizaveis e
// animaveis, e proibe "um unico sprite contendo toda a tempestade". Quase tudo que ele lista
// e faixa repetitiva ou forma geometrica — ceu, superficie do mar, chuva, espuma, relampago,
// redemoinho — e e exatamente a classe em que o codigo ganha do modelo generativo: emenda
// exata, cor exata da rampa, custo zero, e a garantia de que LADRILHA, que e o que permite
// animar por deslocamento no Unity em vez de por folha de sprites.
//
// O que ficou para geracao foi o organico: onda, rocha, nuvem, destroco, mastro.
const path = require('path'), fs = require('fs');
const p = require('./png.js');
const { loadPalette } = require('./ramp-map.js');

const ROOT = path.join(__dirname, '..');
const ENV = path.join(ROOT, 'Docs/Environment_Cytera');
const ramps = loadPalette(path.join(ENV, 'Palette/CYTERA_PALETTE.gpl'));
const UN = 42.857143;

const ESPUMA = ramps['Espuma'], CEU = ramps['Ceu tempestade'], NUVEM = ramps['Nuvem escura'];
const MAR = ramps['Mar'], FUNDO = ramps['Mar profundo'], ROCHA = ramps['Rocha molhada'];
const OUT = ramps['Contorno'][3];

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

const grupos = {};
function gravar(grupo, nome, img, nota) {
  const dir = path.join(ENV, grupo);
  fs.mkdirSync(dir, { recursive: true });
  p.write(path.join(dir, `cytera_${nome}.png`), img);
  (grupos[grupo] ||= []).push(
    `${nome.padEnd(20)} ${String(img.width + 'x' + img.height).padEnd(10)} ` +
    `${(img.width / UN).toFixed(2)}x${(img.height / UN).toFixed(2)} un   ${nota}`);
}

const L = 512;   // multiplo de 4, o periodo do Bayer: a emenda ao ladrilhar fica identica ao interior

// ---------- ceu de tempestade ----------
// Escurece PARA CIMA, ao contrario do ceu de Cicones. E a inversao do gradiente que faz a
// mesma engine de fundo ler como tempestade em vez de meio-dia.
{
  const H = 512;
  const img = p.blank(L, H);
  for (let y = 0; y < H; y++) {
    const t = y / (H - 1);
    for (let x = 0; x < L; x++) {
      // Duas rampas emendadas: nuvem escura em cima, ceu de tempestade embaixo. Uma rampa so
      // nao cobre a faixa de valor que uma tempestade precisa.
      const c = t < 0.55
        ? dither(NUVEM, 3 * (1 - t / 0.55) * 0.85, x, y)
        : dither(CEU, 2.6 * (1 - (t - 0.55) / 0.45), x, y);
      px(img, x, y, c);
    }
  }
  gravar('Background', 'sky_storm', img, 'escurece para cima; ladrilha na horizontal');
}

// ---------- superficie do mar ----------
// Faixa que ladrilha nos dois eixos, com cristas horizontais. Deslocar esta faixa em X no
// Unity ja da a leitura de mar em movimento, sem folha de animacao.
{
  const H = 128;
  const img = p.blank(L, H);
  const rnd = aleatorio(4041);
  for (let y = 0; y < H; y++) {
    const s = 0.2 + 2.6 * (y / (H - 1));    // clareia no topo, escurece no fundo
    for (let x = 0; x < L; x++) px(img, x, y, dither(MAR, s, x, y));
  }
  // Cristas: senos de periodo inteiro na largura, para a emenda fechar.
  for (let k = 0; k < 26; k++) {
    const yBase = 4 + Math.floor(rnd() * (H - 14));
    const ciclos = 1 + Math.floor(rnd() * 3);
    const amp = 2 + rnd() * 4;
    const comp = 30 + Math.floor(rnd() * 90);
    const x0 = Math.floor(rnd() * L);
    const claro = rnd() > 0.45;
    for (let d = 0; d < comp; d++) {
      const x = (x0 + d) % L;
      const y = yBase + Math.round(Math.sin((x / L) * Math.PI * 2 * ciclos) * amp);
      px(img, x, y, claro ? ESPUMA[2] : MAR[0]);
      if (claro) px(img, x, y + 1, ESPUMA[3]);
    }
  }
  gravar('Ocean', 'ocean_surface', img, 'ladrilha nos dois eixos; deslocar em X anima o mar');
}

// ---------- agua profunda ----------
{
  const H = 96;
  const img = p.blank(L, H);
  for (let y = 0; y < H; y++) {
    const s = 0.6 + 2.4 * (y / (H - 1));
    for (let x = 0; x < L; x++) px(img, x, y, dither(FUNDO, s, x, y));
  }
  gravar('Ocean', 'deep_water', img, 'abaixo da superficie; sem detalhe, para nao competir');
}

// ---------- linha de espuma ----------
{
  const H = 24;
  const img = p.blank(L, H);
  const rnd = aleatorio(77);
  for (let x = 0; x < L; x++) {
    const alt = 6 + Math.round(4 * Math.sin(x / L * Math.PI * 2 * 3) + 3 * Math.sin(x / L * Math.PI * 2 * 7));
    for (let y = 0; y < alt; y++) {
      px(img, x, H - 1 - y, y > alt - 3 ? ESPUMA[0] : y > alt - 6 ? ESPUMA[1] : ESPUMA[2]);
    }
    if (rnd() > 0.86) px(img, x, H - alt - 2 - Math.floor(rnd() * 3), ESPUMA[0]);
  }
  gravar('Ocean', 'foam_line', img, 'crista de espuma que ladrilha; vai no topo da superficie');
}

// ---------- chuva ----------
// Inclinacao 1:1 de proposito: uma risca que entra em x sai em x+altura, e com altura igual a
// largura isso e exatamente UM ladrilho — a chuva emenda em qualquer direcao. Inclinacao
// "bonita" quebra a emenda e aparece uma costura diagonal a cada repeticao.
{
  const T = 256;
  const img = p.blank(T, T);
  const rnd = aleatorio(313);
  for (let i = 0; i < 62; i++) {
    const x0 = Math.floor(rnd() * T), y0 = Math.floor(rnd() * T);
    const comp = 10 + Math.floor(rnd() * 22);
    const forte = rnd() > 0.75;
    // Alpha baixo de propósito: a chuva vai em DUAS camadas de parallax, então a densidade
    // vista soma. A primeira versão tinha 170 riscas a alpha 190 e apagou o personagem.
    for (let d = 0; d < comp; d++) {
      px(img, (x0 + d) % T, (y0 + d) % T, forte ? ESPUMA[1] : ESPUMA[3], forte ? 120 : 70);
    }
  }
  gravar('Weather', 'rain', img, 'inclinacao 1:1 para ladrilhar nos dois eixos');
}

// ---------- relampago ----------
// Nucleo branco dentro de um halo mais escuro: e o halo que faz o raio ler como luz e nao
// como um risco de giz. Ramifica duas vezes, senao vira um zigue-zague de desenho animado.
{
  const W = 128, H = 320;
  const img = p.blank(W, H);
  const rnd = aleatorio(1717);

  function ramo(x, y, alturaRestante, largura) {
    while (y < H - 1 && alturaRestante > 0) {
      const passo = 6 + Math.floor(rnd() * 10);
      const desvio = Math.round((rnd() - 0.5) * 18);
      for (let d = 0; d < passo && y + d < H; d++) {
        const cx = x + Math.round(desvio * d / passo);
        for (let w = -largura; w <= largura; w++) px(img, cx + w, y + d, CEU[0], 170);
        for (let w = -Math.max(0, largura - 1); w <= Math.max(0, largura - 1); w++) {
          px(img, cx + w, y + d, ESPUMA[0]);
        }
      }
      x += desvio; y += passo; alturaRestante -= passo;
      if (largura > 0 && rnd() > 0.72) ramo(x, y, alturaRestante * 0.45, largura - 1);
    }
  }
  ramo(W / 2, 0, H, 2);
  gravar('Weather', 'lightning', img, 'nucleo branco em halo; duas ramificacoes');
}

// ---------- redemoinho: tres aneis ----------
// Um anel por objeto ja existente na cena (Redemoinho_Anel_1..3). Girar cada um em velocidade
// diferente no Unity da a leitura de vortice sem nenhuma folha de animacao.
[[192, 5, 'ring_1'], [140, 4, 'ring_2'], [92, 3, 'ring_3']].forEach(([D, esp, nome], k) => {
  const img = p.blank(D, Math.round(D * 0.55));
  const cx = D / 2, cy = img.height / 2;
  const rnd = aleatorio(900 + k);
  const rx = D / 2 - esp - 2, ry = img.height / 2 - esp - 2;
  for (let a = 0; a < 1440; a++) {
    const t = a / 1440 * Math.PI * 2;
    // Espessura variavel: um anel de espessura constante le como aro desenhado, nao como
    // espuma arrastada pela correnteza.
    const e = esp * (0.45 + 0.55 * Math.abs(Math.sin(t * 2 + k)));
    for (let d = -e; d <= e; d++) {
      const c = Math.abs(d) < e * 0.4 ? ESPUMA[0] : Math.abs(d) < e * 0.75 ? ESPUMA[1] : ESPUMA[2];
      px(img, cx + Math.cos(t) * (rx + d), cy + Math.sin(t) * (ry + d * 0.55), c);
    }
    if (rnd() > 0.985) {
      px(img, cx + Math.cos(t) * (rx + esp + 2), cy + Math.sin(t) * (ry + esp + 1), ESPUMA[1]);
    }
  }
  gravar('Effects', nome, img, 'anel do redemoinho; girar no Unity');
});

// ---------- borrifo ----------
{
  const W = 160, H = 96;
  const img = p.blank(W, H);
  const rnd = aleatorio(55);
  for (let i = 0; i < 260; i++) {
    // Densidade cai com a altura: espuma jogada para cima rareia, e e essa queda que faz o
    // borrifo ler como respingo em vez de nuvem.
    const t = Math.pow(rnd(), 1.8);
    const y = Math.round((1 - t) * (H - 1));
    const x = Math.round(W / 2 + (rnd() - 0.5) * W * (0.35 + t * 0.65));
    const r = t > 0.55 ? 2 : 1;
    const c = t > 0.5 ? ESPUMA[0] : ESPUMA[2];
    for (let dy = 0; dy < r; dy++) for (let dx = 0; dx < r; dx++) px(img, x + dx, y + dy, c);
  }
  gravar('Effects', 'spray', img, 'respingo de crista; densidade cai com a altura');
}

// ---------- deck do navio ----------
// Faixa que repete, e nao um objeto: o deck de popa tem 14 unidades e o de proa 10, e um
// sprite unico de deck teria de ser esticado. A geracao tentou e devolveu um barril.
{
  const W = 256, H = 96;
  const img = p.blank(W, H);
  const MAD = ramps['Madeira'];
  const rnd = aleatorio(88);
  // Tabuas horizontais com junta vertical deslocada por fiada.
  const ALT = 12;
  for (let y = 0; y < H; y++) {
    const fiada = Math.floor(y / ALT);
    const dentro = y % ALT;
    // So os dois passos escuros da rampa, mais o contorno na junta: madeira encharcada.
    const c = dentro === 0 ? OUT : dentro < 3 ? MAD[1] : dentro < ALT - 2 ? MAD[2] : MAD[3];
    for (let x = 0; x < W; x++) px(img, x, y, c);
    // Juntas verticais: passo multiplo da largura para a emenda fechar.
    const desloc = (fiada % 2) * 32;
    for (let x = desloc; x < W; x += 64) for (let d = 0; d < ALT; d++) px(img, x, y, MAD[3]);
  }
  // Veio e pregos.
  for (let i = 0; i < 200; i++) {
    const x = Math.floor(rnd() * W), y = Math.floor(rnd() * H);
    px(img, x, y, rnd() > 0.5 ? MAD[3] : MAD[1]);
  }
  for (let x = 8; x < W; x += 64) for (let y = 6; y < H; y += ALT * 2) px(img, x, y, ROCHA[2]);
  gravar('Ship', 'deck_planks', img, 'ladrilha na horizontal; cobre popa e proa');
}

for (const [g, linhas] of Object.entries(grupos)) {
  console.log('== ' + g);
  for (const l of linhas) console.log('   ' + l);
}
