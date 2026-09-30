// Camadas de fundo de Ciclopes, por codigo.
//
//   node Tools/build-ciclopes-bg.js
//
// Banda plana e repetitiva vai de procedural, nao de geracao — a regra que vem de Itaca e se
// confirmou em Troia. Sai com emenda indistinguivel do interior, cores exatamente da rampa e
// custo zero, contra as 40 geracoes que o ceu de Itaca custou pelo `pro` para voltar com
// emenda de 45 a 78%.
//
// A secao 5 do briefing pede menos detalhe no que esta longe, e e exatamente ali que o modelo
// generativo insiste em colocar detalhe. Silhueta em DOIS tons chapados e o que faz o fundo
// recuar; a vila real, com suas dezenas de cores, competiria com o primeiro plano.
const path = require('path'), fs = require('fs');
const p = require('./png.js');
const { loadPalette } = require('./ramp-map.js');

const ROOT = path.join(__dirname, '..');
const ENV = path.join(ROOT, 'Docs/Environment_Ciclopes');
const ramps = loadPalette(path.join(ENV, 'Palette/CICLOPES_PALETTE.gpl'));
const SAIDA = path.join(ENV, 'Background');
const UN = 42.857143;

const BAYER = [[0, 8, 2, 10], [12, 4, 14, 6], [3, 11, 1, 9], [15, 7, 13, 5]];

// A fracao e comparada ao limiar SEM somar degrau quando ela e zero. O erro inverso cria uma
// linha dura em cada juncao da rampa — foi cometido no ceu de Itaca e e visivel a olho nu.
function dither(rampa, s, x, y) {
  const i = Math.min(rampa.length - 1, Math.floor(s));
  const frac = s - i;
  const th = (BAYER[y & 3][x & 3] + 0.5) / 16;
  return rampa[Math.min(rampa.length - 1, i + (frac > th ? 1 : 0))];
}

const px = (img, x, y, c) => {
  if (x < 0 || y < 0 || x >= img.width || y >= img.height) return;
  const o = (y * img.width + x) * 4;
  img.data[o] = c[0]; img.data[o + 1] = c[1]; img.data[o + 2] = c[2]; img.data[o + 3] = 255;
};

/** Semente fixa: a silhueta tem de sair identica a cada reconstrucao. */
function aleatorio(semente) {
  let s = semente;
  return () => ((s = (s * 1103515245 + 12345) & 0x7fffffff) / 0x7fffffff);
}

// Largura multipla de 4, o periodo do Bayer, para a emenda ao ladrilhar ficar identica a
// qualquer fronteira interna.
const L = 512;

// ---------- 1. ceu ----------
// Alto o bastante para cobrir a fase inteira com a camera em qualquer altura: a fase tem 19 un
// e a camera ve 10, entao 12 un de ceu sobram por cima do horizonte em qualquer enquadramento.
{
  const H = 512;
  const img = p.blank(L, H);
  for (let y = 0; y < H; y++) {
    const t = y / (H - 1);
    // Do topo para o horizonte o ceu CLAREIA — e o oposto do gradiente ingenuo, e e o que se
    // ve no Mediterraneo ao meio-dia. A rampa esta ordenada do claro para o escuro, entao a
    // posicao anda de tras para frente.
    const s = 2.4 * (1 - t);
    for (let x = 0; x < L; x++) px(img, x, y, dither(ramps['Ceu'], s, x, y));
  }
  fs.mkdirSync(SAIDA, { recursive: true });
  p.write(path.join(SAIDA, 'ciclopes_bg_sky.png'), img);
  console.log(`ciclopes_bg_sky        ${L}x${H} = ${(L / UN).toFixed(2)} x ${(H / UN).toFixed(2)} un`);
}

// ---------- 2. montanhas distantes ----------
// Dois tons chapados, sem textura. Perfil por soma de senos: cumes irregulares sem parecer
// dentes de serra, que e como sai quando se sorteia altura por coluna.
{
  const H = 160;
  const img = p.blank(L, H);
  const CORPO = ramps['Rocha profunda'][0], TOPO = ramps['Pedra cinza'][3];
  for (let x = 0; x < L; x++) {
    const t = x / L * Math.PI * 2;
    const alt = Math.round(70 + 30 * Math.sin(t * 1.5) + 18 * Math.sin(t * 3.7 + 1.2) + 9 * Math.sin(t * 7.1 + 0.4));
    for (let y = H - alt; y < H; y++) px(img, x, y, y < H - alt + 6 ? TOPO : CORPO);
  }
  p.write(path.join(SAIDA, 'ciclopes_bg_mountains.png'), img);
  console.log(`ciclopes_bg_mountains  ${L}x${H} = ${(L / UN).toFixed(2)} x ${(H / UN).toFixed(2)} un, 2 cores`);
}

// ---------- 3. colinas de oliveira ----------
// Mais claras e mais baixas que as montanhas: e a diferenca de valor entre as duas camadas que
// cria profundidade, nao o parallax sozinho.
{
  const H = 128;
  const img = p.blank(L, H);
  const CORPO = ramps['Oliveira'][3], TOPO = ramps['Oliveira'][2];
  for (let x = 0; x < L; x++) {
    const t = x / L * Math.PI * 2;
    const alt = Math.round(46 + 20 * Math.sin(t * 2.3 + 0.8) + 11 * Math.sin(t * 5.1) + 5 * Math.sin(t * 9.3 + 2));
    for (let y = H - alt; y < H; y++) px(img, x, y, y < H - alt + 5 ? TOPO : CORPO);
  }
  p.write(path.join(SAIDA, 'ciclopes_bg_hills.png'), img);
  console.log(`ciclopes_bg_hills      ${L}x${H} = ${(L / UN).toFixed(2)} x ${(H / UN).toFixed(2)} un, 2 cores`);
}

// ---------- 4. mar distante ----------
// Faixa horizontal com algumas linhas de brilho. Sem onda desenhada: a esta distancia o mar e
// uma superficie, e desenhar onda nele faria competir com o mar de gameplay da fase 04.
{
  const H = 96;
  const img = p.blank(L, H);
  const M = ramps['Mar raso'];
  const rnd = aleatorio(20260905);
  for (let y = 0; y < H; y++) {
    const s = 1.1 + 1.6 * (1 - y / (H - 1));   // clareia em direcao ao horizonte
    for (let x = 0; x < L; x++) px(img, x, y, dither(M, s, x, y));
  }
  for (let i = 0; i < 90; i++) {
    const y = Math.floor(rnd() * H), x0 = Math.floor(rnd() * L), larg = 4 + Math.floor(rnd() * 14);
    for (let x = x0; x < x0 + larg; x++) px(img, x % L, y, M[0]);
  }
  p.write(path.join(SAIDA, 'ciclopes_bg_sea.png'), img);
  console.log(`ciclopes_bg_sea        ${L}x${H} = ${(L / UN).toFixed(2)} x ${(H / UN).toFixed(2)} un`);
}

// ---------- 5. falesia proxima ----------
// Ciclopes e ilha DESABITADA: nao ha vila distante nenhuma. No lugar dela entra uma falesia
// recortada, que e o que o briefing pede em BACKGROUND NEAR e o que anuncia a caverna antes
// de o jogador chegar nela. Perfil em patamares, e nao em cume, porque calcario se rompe em
// degraus horizontais — e sao esses degraus que a diferenciam das montanhas la atras.
{
  const H = 176;
  const img = p.blank(L, H);
  const CORPO = ramps['Pedra cinza'][2], TOPO = ramps['Pedra cinza'][1], SOMBRA = ramps['Rocha profunda'][1];
  const rnd = aleatorio(919);
  const BASE = H - 4;
  let x = 0;
  while (x < L) {
    const larg = 26 + Math.floor(rnd() * 44);
    const alt = 58 + Math.floor(rnd() * 74);
    for (let dx = 0; dx < larg && x + dx < L; dx++) {
      for (let y = BASE - alt; y < BASE; y++) {
        // Fiadas horizontais mais escuras a cada 14 px: e a estratificacao que faz a massa
        // ler como calcario e nao como um bloco de cor.
        const estrato = (y - (BASE - alt)) % 14 < 2;
        px(img, x + dx, y, y < BASE - alt + 5 ? TOPO : estrato ? SOMBRA : CORPO);
      }
    }
    x += larg;
  }
  for (let dx = 0; dx < L; dx++) for (let y = BASE; y < H; y++) px(img, dx, y, SOMBRA);
  p.write(path.join(SAIDA, 'ciclopes_bg_cliffs.png'), img);
  console.log(`ciclopes_bg_cliffs     ${L}x${H} = ${(L / UN).toFixed(2)} x ${(H / UN).toFixed(2)} un, 3 cores`);
}
