// Pré-visualização da Fase 16 montada FORA do Unity, a partir das mesmas posições do
// FinalSceneDresser.cs.
//
//   node Tools/preview-final.js
//
// ## Por que existe, e o que ela NÃO é
//
// A sessão que vestiu a 16 não tinha Unity: nem batchmode, nem captura real. Esta montagem é
// para julgar COMPOSIÇÃO — escala dos props contra o Odisseu, leitura da luz, o primeiro plano
// longe do caminho — antes da captura de verdade (`FinalSceneDresser.Shots`).
// Não é prova de import: cor de renderer, ordem e parallax estão replicados à mão, e o modo
// Tiled do SpriteRenderer foi aproximado (o ladrilho começa na borda esquerda do retângulo). Se o
// dresser mudar, este arquivo precisa mudar junto.
//
// Câmera: tamanho 6 (o CameraFollow desta cena) e y=0 — o alvo fica em y=−1 e o limite inferior
// (−6 + 6) trava a câmera em 0. Parallax pela fórmula do ParallaxLayer: posição inicial mais
// deslocamento da câmera × fator, com a câmera começando em x=0.
const path = require('path'), fs = require('fs');
const p = require('./png.js');

const ROOT = path.join(__dirname, '..');
const A = (rel) => p.read(path.join(ROOT, 'Assets/Art/Environments', rel));
const PPU = 42.857143;
const TOPO = -2, CAM_MIN = -16, CAM_MAX = 40, MEIO = (CAM_MIN + CAM_MAX) / 2, ALCANCE = 50, SALAO = 0;

const cor = (hex, a = 1) => [parseInt(hex.slice(1, 3), 16) / 255, parseInt(hex.slice(3, 5), 16) / 255,
  parseInt(hex.slice(5, 7), 16) / 255, a];
const BRANCO = [1, 1, 1, 1];

// ---- arte
const quadrado = (() => { const q = p.blank(1, 1); q.data.fill(255); return q; })();
const odisseu = p.crop(p.read(path.join(ROOT, 'Assets/Resources/Odisseia/Characters/CHR_Odysseus.png')), 0, 0, 84, 84);

/**
 * Cada item: { img, x, y, sx, sy, rot, tint, ordem, piv: [px,py] normalizado,
 *              tiled: {w,h} em un | undefined, parallax: fator | undefined }
 */
function cena() {
  const L = [];
  const add = (o) => L.push({ sx: 1, sy: 1, rot: 0, tint: BRANCO, piv: [0.5, 0], ...o });

  // céu
  add({ nome: 'Sky_Fill', img: quadrado, x: MEIO, y: 16 - 18, sx: 200 * PPU, sy: 36 * PPU, tint: cor('#b0c5d1'), ordem: -60 });
  add({ nome: 'Ground_Fill', img: quadrado, x: MEIO, y: -17.95 - 16.05, sx: 200 * PPU, sy: 32.1 * PPU, tint: cor('#3e3a33'), ordem: -12 });
  const camada = (nome, rel, f, ordem, baseY, tint = BRANCO) => {
    const img = A(rel);
    const curso = CAM_MAX - CAM_MIN;
    add({ nome, img, x: MEIO * (1 - f), y: baseY, parallax: f, ordem, tint,
      tiled: { w: curso * (1 - f) + 44, h: img.height / PPU } });
  };
  camada('BG_Dawn', 'Final/Background/final_bg_dawn.png', 0.92, -50, 0.3);
  camada('MG_Palace', 'Final/Midground/final_mg_palace_band.png', 0.55, -45, -2.2, [0.94, 0.94, 0.98, 1]);

  const faixa = (nome, rel, cx, topoY, largura, ordem, tint = BRANCO, alturaFixa = 0) => {
    const img = A(rel);
    const h = alturaFixa || img.height / PPU;
    add({ nome, img, x: cx, y: topoY - h, ordem, tint, tiled: { w: largura, h } });
  };
  faixa('Hall_Wall', 'Pretendentes/GreatHall/pret_hall_wall_band.png', (SALAO + ALCANCE) / 2, TOPO + 7.47,
    ALCANCE - SALAO, -30, [1, 0.97, 0.92, 1]);

  // chão: Floor_1 −16..0 (pátio), Floor_2 0..40 (salão)
  const sup = 16 / PPU;
  for (const [e, d, salao] of [[CAM_MIN - 12, 0, false], [0, ALCANCE, true]]) {
    faixa('GroundTop', 'ItacaReturn/Gameplay/ithaca_stone_ground_top.png', (e + d) / 2, TOPO + sup, d - e, -10,
      salao ? [0.96, 0.92, 0.86, 1] : BRANCO);
    const k = salao ? 0.52 : 0.62;
    faixa('GroundBody', 'ItacaReturn/Gameplay/ithaca_stone_ground_body.png', (e + d) / 2, TOPO + sup - 32 / PPU,
      d - e, -11, [k, k * 0.97, k * 0.94, 1], 2.4);
  }

  const prop = (nome, rel, x, y, ordem, s = 1, extra = {}) => add({ nome, img: A(rel), x, y, sx: s, sy: s, ordem, ...extra });
  // pátio
  for (const x of [-14.6, -2.4]) prop('Brazier', 'ItacaReturn/Props/itaca_ret_brazier.png', x, TOPO, -6, 0.55);
  prop('Bow_Odysseus', 'Final/Props/final_bow_odysseus.png', -4.4, TOPO, -4, 1, { rot: -10 });
  prop('Arrows', 'Ithaca/Arsenal/ithaca_arrows_bundle_01.png', -5.3, TOPO, -3, 0.7);
  for (const dx of [-0.9, 0.9]) prop('Door_Column', 'Ithaca/Architecture/ithaca_column_01.png', SALAO + dx, TOPO - 0.05, -28, 1.15);
  // prova
  for (const x of [10, 18, 26, 34]) prop('Axe', 'Final/Props/final_axe_post.png', x, TOPO, -3);
  // salão
  prop('Hearth_Base', 'Final/Props/final_hearth_base.png', 22, TOPO, -6);
  prop('Hearth_Fire', 'Final/Props/final_hearth_fire.png', 22, TOPO, -5);
  prop('Loom', 'Final/Props/final_loom_penelope.png', 30, TOPO, -6);
  for (const x of [3, 39.8]) prop('Hall_Brazier', 'ItacaReturn/Props/itaca_ret_brazier.png', x, TOPO, -7, 0.55);
  prop('Throne', 'Pretendentes/GreatHall/pret_throne.png', 42, TOPO, -8, 0.62);
  // luz
  for (const x of [6, 14, 22, 30, 38]) prop('Dawn_Shaft', 'Final/VFX/final_dawn_shaft.png', x, TOPO, -25);
  {
    const img = A('Final/VFX/final_dawn_gradient.png');
    const x0 = CAM_MIN - 12, ate = ALCANCE + 6;
    add({ nome: 'DawnVeil', img, x: (x0 + ate) / 2, y: -16, sx: (ate - x0) / (img.width / PPU), sy: 40 / (img.height / PPU), ordem: 20 });
  }
  // primeiro plano
  const contraLuz = [0.42, 0.40, 0.46, 1];
  prop('FG_Stones', 'Final/Foreground/final_fg_stones.png', -15.4, TOPO - 0.25, 30, 1, { tint: contraLuz });
  prop('FG_Column', 'Final/Foreground/final_fg_column.png', -17.6, TOPO - 0.25, 30, 1, { tint: contraLuz });
  prop('FG_Column', 'Final/Foreground/final_fg_column.png', 43.4, TOPO - 0.25, 30, 1, { tint: contraLuz });
  return L;
}

/** Renderiza a cena com a câmera em (cx, cy), meia-altura `tam`, num quadro de w×h unidades. */
function render(L, cx, cy, wUn, hUn, odisseuX) {
  const W = Math.round(wUn * PPU), H = Math.round(hUn * PPU);
  const buf = new Float32Array(W * H * 3);
  for (let i = 0; i < W * H; i++) { buf[i * 3] = 1; buf[i * 3 + 1] = 0; buf[i * 3 + 2] = 1; }   // magenta = buraco
  const esq = cx - wUn / 2, topo = cy + hUn / 2;
  const itens = [...L];
  if (odisseuX !== undefined) itens.push({ img: odisseu, x: odisseuX, y: TOPO, sx: 1, sy: 1, rot: 0, tint: BRANCO, piv: [0.5, 0.142857], ordem: 2 });
  itens.sort((a, b) => a.ordem - b.ordem);

  for (const it of itens) {
    const { img } = it;
    const x = it.parallax !== undefined ? it.x + cx * it.parallax : it.x;
    const y = it.y;
    const tw = it.tiled ? it.tiled.w : img.width / PPU * it.sx;
    const th = it.tiled ? it.tiled.h : img.height / PPU * it.sy;
    const ox = x - it.piv[0] * tw, oy = y - it.piv[1] * th;           // canto inferior esquerdo, em un
    const rad = (it.rot || 0) * Math.PI / 180, cos = Math.cos(rad), sin = Math.sin(rad);
    const R = Math.hypot(tw, th);
    const px0 = Math.max(0, Math.floor((x - R - esq) * PPU)), px1 = Math.min(W, Math.ceil((x + R - esq) * PPU));
    const py0 = Math.max(0, Math.floor((topo - (y + R)) * PPU)), py1 = Math.min(H, Math.ceil((topo - (y - R)) * PPU));
    for (let py = py0; py < py1; py++) for (let px = px0; px < px1; px++) {
      // ponto do mundo -> espaço local do sprite (desfaz a rotação em torno do pivô)
      const wx = esq + (px + 0.5) / PPU - x, wy = topo - (py + 0.5) / PPU - y;
      const lx = cos * wx + sin * wy + (x - ox), ly = -sin * wx + cos * wy + (y - oy);
      if (lx < 0 || ly < 0 || lx >= tw || ly >= th) continue;
      let sxp, syp;
      if (it.tiled) {
        sxp = Math.floor(lx * PPU) % img.width;
        syp = img.height - 1 - (Math.floor(ly * PPU) % img.height);
      } else {
        sxp = Math.floor(lx / tw * img.width);
        syp = img.height - 1 - Math.floor(ly / th * img.height);
      }
      const o = (syp * img.width + sxp) * 4;
      const a = img.data[o + 3] / 255 * it.tint[3];
      if (!a) continue;
      const d = (py * W + px) * 3;
      for (let c = 0; c < 3; c++) buf[d + c] = img.data[o + c] / 255 * it.tint[c] * a + buf[d + c] * (1 - a);
    }
  }
  const out = p.blank(W, H);
  let magenta = 0;
  for (let i = 0; i < W * H; i++) {
    const r = Math.round(buf[i * 3] * 255), g = Math.round(buf[i * 3 + 1] * 255), b = Math.round(buf[i * 3 + 2] * 255);
    if (r === 255 && g === 0 && b === 255) magenta++;
    out.data[i * 4] = r; out.data[i * 4 + 1] = g; out.data[i * 4 + 2] = b; out.data[i * 4 + 3] = 255;
  }
  return { img: out, magenta };
}

function ampliar(img, k) {
  const out = p.blank(img.width * k, img.height * k);
  for (let y = 0; y < out.height; y++) for (let x = 0; x < out.width; x++) {
    const s = (Math.floor(y / k) * img.width + Math.floor(x / k)) * 4;
    img.data.copy(out.data, (y * out.width + x) * 4, s, s + 4);
  }
  return out;
}

if (require.main === module) {
  const L = cena();
  const pasta = path.join(ROOT, 'Docs/Environment/Fase16/_previa');
  fs.mkdirSync(pasta, { recursive: true });
  const meiaLargura = 6 * 16 / 9;
  const travar = (x) => Math.min(44 - meiaLargura, Math.max(-18 + meiaLargura, x));
  const quadros = [
    ['01_inicio', -12, -12], ['02_arco', -4.4, -6.4], ['03_machados', 14, 12.6],
    ['04_lareira', 22, 19.6], ['05_tear', 30, 28.2], ['06_fim', 38, 36.6],
  ];
  for (const [nome, alvo, ox] of quadros) {
    const cx = travar(alvo);
    const { img, magenta } = render(L, cx, 0, meiaLargura * 2, 12, ox);
    p.write(path.join(pasta, nome + '.png'), ampliar(img, 2));
    console.log(`  ${nome.padEnd(12)} câmera x=${cx.toFixed(2)}  magenta ${magenta} px`);
  }
  const { img, magenta } = render(L, 13, 1, 62, 14, -12);
  p.write(path.join(pasta, '07_composicao.png'), img);
  console.log(`  07_composicao  x −18..44  magenta ${magenta} px`);
}
module.exports = { cena, render };
