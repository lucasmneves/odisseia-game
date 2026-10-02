// Monta o ASSET PACK do PixelLab Asset Completion a partir dos candidatos escolhidos.
//
//   node Tools/build-pack.js [grupo]     (sem argumento: todos)
//
// Cada entrada diz de onde vêm os quadros (Docs/Art/PixelLab/<ID>/_candidatos), o que fazer com eles e para onde vão
// em Assets/Art/. A saída é PNG + .meta escritos pelas mesmas funções do unity-import (GUID derivado do caminho,
// 42,857 px/un, Point, sem compressão) — **nenhuma cena ou prefab referencia estes arquivos ainda**: a integração é
// etapa separada.
//
// Regras aplicadas a todo quadro: alpha duro (≥128 opaco), recorte pelo retângulo COMUM a todos os quadros do mesmo
// efeito (para o pivô não pular entre quadros) e folha horizontal com 1 px de folga entre células.
const fs = require('fs'), path = require('path');
const p = require('./png.js');
const { textureMeta, ensureFolder, ALIGN } = require('./unity-import.js');
const { moda } = require('./downscale-native.js');

const ROOT = path.join(__dirname, '..');
const PX = path.join(ROOT, 'Docs/Art/PixelLab');
const UN = 42.857143;
const cand = (id, nome) => p.read(path.join(PX, id, '_candidatos', nome + '.png'));
const prep = (nome) => p.read(path.join(PX, '_prep', nome + '.png'));

function alphaDuro(img) {
  const o = p.blank(img.width, img.height); img.data.copy(o.data);
  for (let i = 3; i < o.data.length; i += 4) o.data[i] = o.data[i] >= 128 ? 255 : 0;
  return o;
}
function espelhar(img) {
  const o = p.blank(img.width, img.height);
  for (let y = 0; y < img.height; y++) for (let x = 0; x < img.width; x++) {
    const s = (y * img.width + x) * 4; img.data.copy(o.data, (y * img.width + img.width - 1 - x) * 4, s, s + 4);
  }
  return o;
}
/** Dissolve por dithering ordenado: some `t` (0..1) dos pixels, sempre os mesmos primeiro. */
function dissolver(img, t) {
  const B = [[0, 8, 2, 10], [12, 4, 14, 6], [3, 11, 1, 9], [15, 7, 13, 5]];
  const o = p.blank(img.width, img.height); img.data.copy(o.data);
  for (let y = 0; y < img.height; y++) for (let x = 0; x < img.width; x++)
    if ((B[y & 3][x & 3] + 0.5) / 16 < t) o.data[(y * img.width + x) * 4 + 3] = 0;
  return o;
}

function folha(rel, quadros, { align = ALIGN.center, canvasInteiro = false } = {}) {
  quadros = quadros.map(alphaDuro);
  const W = Math.max(...quadros.map(q => q.width)), H = Math.max(...quadros.map(q => q.height));
  // centraliza quadros de tamanhos diferentes (a poeira reamostrada, por exemplo) num canvas comum
  const norm = quadros.map(q => {
    if (q.width === W && q.height === H) return q;
    const o = p.blank(W, H), dx = (W - q.width) >> 1, dy = H - q.height;
    for (let y = 0; y < q.height; y++) q.data.copy(o.data, ((y + dy) * W + dx) * 4, y * q.width * 4, (y + 1) * q.width * 4);
    return o;
  });
  // retângulo comum
  // canvasInteiro: substitutos de sprites que já estão em cena (fogo animado) e camadas que se sobrepõem a
  // outra peça (a chama da lareira sobre a base) precisam do MESMO canvas da original — recortar mudaria o pivô.
  let x0 = W, y0 = H, x1 = -1, y1 = -1;
  if (canvasInteiro) { x0 = 0; y0 = 0; x1 = W - 1; y1 = H - 1; }
  else for (const q of norm) { const b = p.bounds(q); if (!b) continue;
    x0 = Math.min(x0, b.x0); y0 = Math.min(y0, b.y0); x1 = Math.max(x1, b.x1); y1 = Math.max(y1, b.y1); }
  const cw = x1 - x0 + 1, ch = y1 - y0 + 1, gap = 1;
  const out = p.blank(norm.length * (cw + gap) - gap, ch);
  norm.forEach((q, n) => {
    for (let y = 0; y < ch; y++) q.data.copy(out.data, (y * out.width + n * (cw + gap)) * 4,
      ((y0 + y) * W + x0) * 4, ((y0 + y) * W + x1 + 1) * 4);
  });
  const destino = path.join(ROOT, rel);
  ensureFolder(path.dirname(destino), path.dirname(rel));
  p.write(destino, out);
  const base = path.basename(rel, '.png');
  const slice = norm.map((_, n) => ({ name: `${base}_${String(n).padStart(2, '0')}`, x: n * (cw + gap), y: 0, w: cw, h: ch }));
  fs.writeFileSync(destino + '.meta', textureMeta(rel, out, { align, slice: norm.length > 1 ? slice : null }));
  console.log(`  ${rel.padEnd(52)} ${norm.length} quadro(s) de ${cw}x${ch} px = ${(cw / UN).toFixed(2)} x ${(ch / UN).toFixed(2)} un`);
  return out;
}

const GRUPOS = {
  efeitos() {
    const q = (id, nome, idx) => idx.map(i => cand(id, `${nome}_${String(i).padStart(2, '0')}`));
    folha('Assets/Art/Effects/fx_hit.png', q('PXL-001', 'hit_a_anim', [0, 1, 2, 3]));
    folha('Assets/Art/Effects/fx_slash.png', q('PXL-002', 'slash_anim', [0, 1, 2, 3]));
    folha('Assets/Art/Effects/fx_block.png', q('PXL-003', 'block_anim', [0, 1, 2, 3]));
    // A nuvem do PixelLab quase não se desfaz: dois quadros de dissolução por dithering fecham o efeito.
    const morte = q('PXL-004', 'death_anim', [0, 1, 2, 3]);
    folha('Assets/Art/Effects/fx_death.png', [...morte, dissolver(morte[3], 0.45), dissolver(morte[3], 0.8)]);
    // O quarto quadro do impacto de flecha virou silhueta preta: fora.
    folha('Assets/Art/Effects/fx_arrow_impact.png', q('PXL-006', 'arrowhit_anim', [0, 1, 2]));
    folha('Assets/Art/Effects/fx_collect.png', q('PXL-007', 'collect_anim', [0, 1, 2, 3]));
    // Poeira de salto: a nuvem de Troia tem 2,7 un; reamostrada pela metade (moda) fica do tamanho dos pés.
    folha('Assets/Art/Effects/fx_jump_dust.png', q('PXL-005', 'dust_anim', [0, 1, 2, 3]).map(f => moda(f, 0.5)),
      { align: ALIGN.bottom });
  },
};

/** Contorno de 1 px na cor do contorno do projeto (#050507) em volta de tudo que é opaco. */
function contornar(img) {
  const o = p.blank(img.width + 2, img.height + 2);
  for (let y = 0; y < img.height; y++) img.data.copy(o.data, ((y + 1) * o.width + 1) * 4, y * img.width * 4, (y + 1) * img.width * 4);
  const op = (x, y) => x >= 0 && y >= 0 && x < o.width && y < o.height && o.data[(y * o.width + x) * 4 + 3] > 0;
  const borda = [];
  for (let y = 0; y < o.height; y++) for (let x = 0; x < o.width; x++)
    if (!op(x, y) && (op(x + 1, y) || op(x - 1, y) || op(x, y + 1) || op(x, y - 1))) borda.push((y * o.width + x) * 4);
  for (const i of borda) { o.data[i] = 5; o.data[i + 1] = 5; o.data[i + 2] = 7; o.data[i + 3] = 255; }
  return o;
}

GRUPOS.props = function () {
  folha('Assets/Art/Items/item_arrow_quiver.png', [cand('PXL-008', 'quiver_a')], { align: ALIGN.bottom });
  // Reencontro: o candidato 0 do pro em duas escalas. A nativa (78 px de altura) serve a um Ending com câmera
  // mais próxima; a de 60 px, reamostrada por moda, fica na régua do Odisseu (60 px = 1,4 un) e casa com o
  // Telêmaco adulto na mesma cena.
  const par = cand('PXL-009', 'reunion_pro_00');
  folha('Assets/Art/Ending/ending_reunion_close.png', [par], { align: ALIGN.bottom });
  const b = p.bounds(par);
  folha('Assets/Art/Ending/ending_reunion.png', [moda(p.crop(par, b.x0, b.y0, b.w, b.h), 60 / b.h)], { align: ALIGN.bottom });
};

GRUPOS.troia = function () {
  // Ponte: 7 un = 300 px. Tampas de 10 px das duas pontas da fonte + miolo espelhado (ladrilha sem emenda).
  const fonte = alphaDuro(cand('PXL-010', 'bridge_a')), b = p.bounds(fonte);
  const viga = p.crop(fonte, b.x0, b.y0, b.w, b.h);
  const TAMPA = 10, ALVO = 300;
  const miolo = p.crop(viga, TAMPA, 0, viga.width - 2 * TAMPA, viga.height);
  const ponte = p.blank(ALVO, viga.height);
  const copiar = (src, sx, dx, w) => { for (let y = 0; y < src.height; y++) src.data.copy(ponte.data, (y * ALVO + dx) * 4, (y * src.width + sx) * 4, (y * src.width + sx + w) * 4); };
  copiar(viga, 0, 0, TAMPA);
  for (let x = TAMPA, k = 0; x < ALVO - TAMPA; k++) {
    const w = Math.min(miolo.width, ALVO - TAMPA - x);
    const seg = k % 2 ? espelhar(miolo) : miolo;
    copiar(seg, k % 2 ? miolo.width - w : 0, x, w);
    x += w;
  }
  copiar(viga, viga.width - TAMPA, ALVO - TAMPA, TAMPA);
  folha('Assets/Art/Environments/Troy/Gameplay/troy_plank_bridge.png', [ponte], { align: ALIGN.center });
  // Bloco caído: uma fiada da própria muralha (juntas medidas em y 110 e 144), lado esquerdo, fora da torre.
  const muro = p.read(path.join(ROOT, 'Assets/Art/Environments/Troy/Architecture/troy_wall_section_01.png'));
  folha('Assets/Art/Environments/Troy/Gameplay/troy_fallen_block.png', [contornar(p.crop(muro, 0, 111, 126, 33))], { align: ALIGN.center });
};

// ---------------------------------------------------------------- fogo e checkpoint
const ehChama = (r, g, b) => { const mx = Math.max(r, g, b), mn = Math.min(r, g, b), S = mx ? (mx - mn) / mx : 0;
  if (S < 0.45 || mx < 90) return false; const d = mx - mn; const H = mx === r ? (((g - b) / d) + 6) % 6 * 60 : 999; return H < 60 || H > 340; };

/**
 * Devolve a chama animada ao sprite original: dentro do retângulo, o pixel que era CHAMA na fonte passa a vir do
 * quadro animado (ou some, se o quadro não tem chama ali); o que não era chama (suporte, toras, carne) fica, a não
 * ser que uma língua de fogo do quadro passe por cima. O suporte nunca se mexe.
 */
function recompor(fonte, ret, quadro) {
  const o = p.blank(fonte.width, fonte.height); fonte.data.copy(o.data);
  for (let y = 0; y < ret.h; y++) for (let x = 0; x < ret.w; x++) {
    const so = ((ret.y0 + y) * fonte.width + ret.x0 + x) * 4, qo = (y * quadro.width + x) * 4;
    const eraChama = fonte.data[so + 3] && ehChama(fonte.data[so], fonte.data[so + 1], fonte.data[so + 2]);
    const temChama = quadro.data[qo + 3] >= 128 && ehChama(quadro.data[qo], quadro.data[qo + 1], quadro.data[qo + 2]);
    if (temChama || (eraChama && quadro.data[qo + 3] >= 128)) quadro.data.copy(o.data, so, qo, qo + 4);
    else if (eraChama) o.data[so + 3] = 0;
  }
  return o;
}

GRUPOS.fogo = function () {
  const R = JSON.parse(fs.readFileSync(path.join(PX, 'PXL-015/_ref/recortes.json'), 'utf8'));
  const fonteDe = (n) => {
    if (n !== 'hearth') return p.read(path.join(ROOT, 'Assets/Art/Environments', R[n].fonte.replace(/^Assets\/Art\/Environments\//, '')));
    return null;
  };
  const PINGUE = [0, 1, 2, 3, 2, 1];
  const destinos = {
    torch_stand: 'Assets/Art/Environments/Pretendentes/VFX/pret_torch_stand_anim.png',
    brazier: 'Assets/Art/Environments/ItacaReturn/Props/itaca_ret_brazier_anim.png',
    cooking_fire: 'Assets/Art/Environments/Pretendentes/Courtyard/pret_cooking_fire_anim.png',
    campfire_troy: 'Assets/Art/Environments/Troy/Camp/troy_campfire_01_anim.png',
  };
  for (const [n, destino] of Object.entries(destinos)) {
    const fonte = fonteDe(n);
    const quadros = PINGUE.map(i => recompor(fonte, R[n], alphaDuro(cand('PXL-015', `${n}_anim_${String(i).padStart(2, '0')}`))));
    folha(destino, quadros, { align: ALIGN.bottom, canvasInteiro: true });
  }
  // Lareira da 16: só a CHAMA anima — a base já é peça separada. Cada quadro vira a camada de fogo no canvas
  // da base (103x82), no mesmo lugar da chama estática.
  const base = p.read(path.join(ROOT, 'Assets/Art/Environments/Final/Props/final_hearth_base.png'));
  folha('Assets/Art/Environments/Final/Props/final_hearth_fire_anim.png', PINGUE.map(i => {
    const q = alphaDuro(cand('PXL-015', `hearth_anim_${String(i).padStart(2, '0')}`)), o = p.blank(base.width, base.height);
    for (let y = 0; y < R.hearth.h; y++) for (let x = 0; x < R.hearth.w; x++) {
      const qo = (y * q.width + x) * 4, so = ((R.hearth.y0 + y) * base.width + R.hearth.x0 + x) * 4;
      if (q.data[qo + 3] && ehChama(q.data[qo], q.data[qo + 1], q.data[qo + 2])) q.data.copy(o.data, so, qo, qo + 4);
    }
    return o;
  }), { align: ALIGN.bottom, canvasInteiro: true });

  // Checkpoint: altar apagado (PXL-014) e aceso = mesmo altar + a chama da tocha (PXL-015) sobre a taça.
  const altar = alphaDuro(cand('PXL-014', 'altar_a')), ab = p.bounds(altar);
  const apagado = p.crop(altar, ab.x0, ab.y0, ab.w, ab.h);
  // taça: linha mais alta com bronze — a chama assenta 3 px abaixo da borda da taça, centrada no altar
  let topoTaca = 0; for (let y = 0; y < apagado.height && !topoTaca; y++) for (let x = 0; x < apagado.width; x++) {
    const o = (y * apagado.width + x) * 4; const [r, g, b] = [apagado.data[o], apagado.data[o + 1], apagado.data[o + 2]];
    if (apagado.data[o + 3] && r > 150 && r > b + 60 && g > 80) { topoTaca = y; break; } }
  const ACIMA = 30;
  const acesos = PINGUE.map(i => {
    const q = alphaDuro(cand('PXL-015', `torch_stand_anim_${String(i).padStart(2, '0')}`));
    const o = p.blank(apagado.width, apagado.height + ACIMA);
    for (let y = 0; y < apagado.height; y++) apagado.data.copy(o.data, ((y + ACIMA) * o.width) * 4, y * apagado.width * 4, (y + 1) * apagado.width * 4);
    // só a chama da tocha (as 26 linhas de cima do recorte; o resto é a taça da tocha)
    const dx = Math.round((apagado.width - q.width) / 2), base = ACIMA + topoTaca + 3;
    for (let y = 0; y < 26; y++) for (let x = 0; x < q.width; x++) {
      const qo = (y * q.width + x) * 4; if (!q.data[qo + 3]) continue;
      const ty = base - 26 + y, tx = dx + x; if (tx < 0 || tx >= o.width || ty < 0) continue;
      q.data.copy(o.data, (ty * o.width + tx) * 4, qo, qo + 4);
    }
    return o;
  });
  const apagadoAlto = p.blank(apagado.width, apagado.height + ACIMA);
  for (let y = 0; y < apagado.height; y++) apagado.data.copy(apagadoAlto.data, ((y + ACIMA) * apagado.width) * 4, y * apagado.width * 4, (y + 1) * apagado.width * 4);
  // mesma célula nos dois estados: o pivô (base) é o mesmo quando o checkpoint acende
  folha('Assets/Art/Items/item_checkpoint_altar.png', [apagadoAlto, ...acesos], { align: ALIGN.bottom });
};

// ---------------------------------------------------------------- mapa e emblemas
const PAL = require('./ramp-map.js').loadPalette(path.join(ROOT, 'Docs/Environment_Pretendentes/Palette/PRETENDENTES_PALETTE.gpl'));

GRUPOS.mapa = function () {
  // O mar veio escuro e frio (teal profundo) para um Egeu ensolarado. Todo pixel de água (matiz 150–215°) vai para
  // a rampa Água da paleta de Ítaca, por luminância; costa, ilhas e montanhas ficam como vieram.
  const m = p.read(path.join(PX, 'PXL-016/_candidatos/map_a.png'));
  const agua = PAL['Água'] || PAL['Agua'];
  for (let i = 0; i < m.width * m.height; i++) {
    const o = i * 4; const [r, g, b] = [m.data[o], m.data[o + 1], m.data[o + 2]];
    const mx = Math.max(r, g, b), mn = Math.min(r, g, b), d = mx - mn; if (!d) continue;
    const H = mx === r ? (((g - b) / d) + 6) % 6 * 60 : mx === g ? ((b - r) / d + 2) * 60 : ((r - g) / d + 4) * 60;
    if (H < 150 || H > 215) continue;
    const L = 0.2126 * r + 0.7152 * g + 0.0722 * b;
    const c = agua[L > 150 ? 0 : L > 95 ? 1 : L > 60 ? 2 : 3];
    m.data[o] = c[0]; m.data[o + 1] = c[1]; m.data[o + 2] = c[2];
  }
  folha('Assets/Art/Map/map_aegean.png', [m], { align: ALIGN.center, canvasInteiro: true });
  folha('Assets/Art/Map/map_ship_marker.png', [cand('PXL-017', 'ship_a')], { align: ALIGN.bottom });
};

/** Medalhão comum dos 16 emblemas, desenhado aqui — é o que faz deles um conjunto. */
function medalhao(simbolo, { bloqueado = false } = {}) {
  const S = 56, c = (S - 1) / 2, o = p.blank(S, S);
  const bronze = PAL['Bronze'], noite = PAL['Noite'], cont = [5, 5, 7];
  const pinta = (x, y, col) => { const i = (y * S + x) * 4; o.data[i] = col[0]; o.data[i + 1] = col[1]; o.data[i + 2] = col[2]; o.data[i + 3] = 255; };
  for (let y = 0; y < S; y++) for (let x = 0; x < S; x++) {
    const d = Math.hypot(x - c, y - c);
    if (d > 27.5) continue;
    if (d > 26.3) pinta(x, y, cont);
    else if (d > 22.6) {
      // aro de bronze: luz em cima à esquerda (forma, não direção de cena), pontos a cada 30°
      const ang = Math.atan2(y - c, x - c), ponto = Math.abs(((ang * 180 / Math.PI) + 360) % 30 - 15) < 3.2 && d > 23.6 && d < 25.4;
      pinta(x, y, ponto ? bronze[0] : (y < c - 4 ? bronze[1] : y > c + 8 ? bronze[3] : bronze[2]));
    } else if (d > 21.6) pinta(x, y, cont);
    else pinta(x, y, d > 18 ? noite[1] : noite[2]);
  }
  const s = alphaDuro(simbolo), b = p.bounds(s);
  const sx = Math.round(c - b.w / 2 + 0.5) - b.x0, sy = Math.round(c - b.h / 2 + 0.5) - b.y0;
  for (let y = b.y0; y <= b.y1; y++) for (let x = b.x0; x <= b.x1; x++) {
    const i = (y * s.width + x) * 4; if (!s.data[i + 3]) continue;
    const tx = x + sx, ty = y + sy; if (tx < 0 || ty < 0 || tx >= S || ty >= S) continue;
    s.data.copy(o.data, (ty * S + tx) * 4, i, i + 4);
  }
  if (bloqueado) for (let i = 0; i < S * S; i++) {
    const k = i * 4; if (!o.data[k + 3]) continue;
    const L = Math.round((0.2126 * o.data[k] + 0.7152 * o.data[k + 1] + 0.0722 * o.data[k + 2]) * 0.55 + 18);
    o.data[k] = L; o.data[k + 1] = L; o.data[k + 2] = Math.min(255, L + 6);
  }
  return o;
}

GRUPOS.emblemas = function () {
  const ESCOLHA = {
    '01': 'e01_itaca', '02': 'e02_troia', '03': 'e03_cicones', '04': 'e04_citera', '05': 'e05_ciclopes',
    '06': 'e06_eolo', '07': 'e07_lestrigoes_b', '08': 'e08_circe', '09': 'e09_mortos', '10': 'e10_sereias',
    '11': 'e11_cila', '12': 'e12_gado', '13': 'e13_calipso', '14': 'e14_itaca_retorno', '15': 'e15_pretendentes',
    '16': 'e16_final_b',
  };
  for (const [n, fonte] of Object.entries(ESCOLHA)) {
    const s = cand('PXL-018', fonte);
    folha(`Assets/Art/Map/Emblems/emblem_${n}.png`, [medalhao(s)], { align: ALIGN.center, canvasInteiro: true });
    folha(`Assets/Art/Map/Emblems/emblem_${n}_locked.png`, [medalhao(s, { bloqueado: true })], { align: ALIGN.center, canvasInteiro: true });
  }
  // prancha de conferência do conjunto (não vai para o jogo)
  const tiles = Object.keys(ESCOLHA).sort().map(n => p.read(path.join(ROOT, `Assets/Art/Map/Emblems/emblem_${n}.png`)));
  const prancha = p.blank(8 * 60, 2 * 60);
  tiles.forEach((t, k) => { const ox = (k % 8) * 60 + 2, oy = Math.floor(k / 8) * 60 + 2;
    for (let y = 0; y < t.height; y++) t.data.copy(prancha.data, ((oy + y) * prancha.width + ox) * 4, y * t.width * 4, (y + 1) * t.width * 4); });
  p.write(path.join(PX, 'PXL-018/_prancha_emblemas.png'), prancha);
};

GRUPOS.hud = function () {
  for (const n of ['health', 'arrows', 'lives', 'xp', 'hunger', 'lotus', 'wax', 'shield'])
    folha(`Assets/Art/UI/HUD/icon_${n}.png`, [cand('PXL-019', 'h_' + n)], { align: ALIGN.center });
  // Coletável: a moeda pintada a 100 px/un (8 quadros de 89x93) reamostrada por moda para 42,857 px/un — D-050.
  const col = p.read(path.join(ROOT, 'Assets/Resources/Odisseia/Items/PROP_Collectible.png'));
  const k = 42.857143 / 100, qw = Math.floor(col.width / 8);
  const quadros = [...Array(8).keys()].map(i => moda(p.crop(col, i * qw, 0, qw, col.height), k));
  folha('Assets/Art/Items/item_collectible_coin.png', quadros, { align: ALIGN.center });
  folha('Assets/Art/UI/HUD/icon_coin.png', [quadros[0]], { align: ALIGN.center });
};

GRUPOS.menu = function () {
  const m = p.read(path.join(PX, 'PXL-020/_candidatos/menu_a.png'));
  folha('Assets/Art/UI/MainMenu/menu_bg_ithaca.png', [m], { align: ALIGN.center, canvasInteiro: true });
  // O Odisseu do MASTER (Idle_00), na densidade da paisagem, de pé no parapeito da esquerda e olhando o mar.
  // Pés na linha do topo do parapeito (y 324, medido na imagem).
  const folhaOd = p.read(path.join(ROOT, 'Assets/Resources/Odisseia/Characters/CHR_Odysseus.png'));
  const od = p.crop(folhaOd, 0, 0, 84, 84), b = p.bounds(od);
  const com = p.blank(m.width, m.height); m.data.copy(com.data);
  const PES_Y = 324, CENTRO_X = 150;
  for (let y = b.y0; y <= b.y1; y++) for (let x = b.x0; x <= b.x1; x++) {
    const i = (y * 84 + x) * 4; if (!od.data[i + 3]) continue;
    const tx = CENTRO_X - Math.round(b.w / 2) + (x - b.x0), ty = PES_Y - (b.y1 - y);
    od.data.copy(com.data, (ty * m.width + tx) * 4, i, i + 4);
  }
  folha('Assets/Art/UI/MainMenu/menu_bg_ithaca_odysseus.png', [com], { align: ALIGN.center, canvasInteiro: true });
};

/** Fica só o maior componente conectado (8-vizinhança): tira pedra solta, traço de esboço e poeira. */
function maiorComponente(img) {
  const { width: W, height: H } = img, rot = new Int32Array(W * H).fill(-1); let melhor = -1, tam = 0, k = 0;
  for (let s = 0; s < W * H; s++) {
    if (rot[s] >= 0 || !img.data[s * 4 + 3]) continue;
    let n = 0; const pilha = [s]; rot[s] = k;
    while (pilha.length) { const i = pilha.pop(); n++; const x = i % W, y = (i - x) / W;
      for (let dy = -1; dy <= 1; dy++) for (let dx = -1; dx <= 1; dx++) { const nx = x + dx, ny = y + dy;
        if (nx < 0 || ny < 0 || nx >= W || ny >= H) continue; const j = ny * W + nx;
        if (rot[j] < 0 && img.data[j * 4 + 3]) { rot[j] = k; pilha.push(j); } } }
    if (n > tam) { tam = n; melhor = k; } k++;
  }
  const o = p.blank(W, H);
  for (let i = 0; i < W * H; i++) if (rot[i] === melhor) img.data.copy(o.data, i * 4, i * 4, i * 4 + 4);
  return o;
}
/** Remove o fundo pintado que o pixen às vezes deixa (borda chapada), como o cutout das fases anteriores. */
const semFundo = (img) => { if (!img.data[3] && !img.data[(img.width * img.height - 1) * 4 + 3]) return img;
  const { cutout } = require('./cutout.js'); const r = cutout(img, 18); return r.removed > img.width * img.height * 0.02 ? r.img : img; };

/**
 * A nuvem da 06 veio com traços de esboço e hachurado cinza encostando nela. Fica o que é nuvem (claro, ou azulado)
 * e o contorno escuro que toca nuvem; linha solta e hachurado (cinza neutro sem nuvem ao lado) saem.
 */
function soNuvem(img) {
  const { width: W, height: H } = img, o = p.blank(W, H);
  const lum = (i) => 0.2126 * img.data[i] + 0.7152 * img.data[i + 1] + 0.0722 * img.data[i + 2];
  const nuvem = (x, y) => { if (x < 0 || y < 0 || x >= W || y >= H) return false; const i = (y * W + x) * 4;
    return img.data[i + 3] && (lum(i) > 165 || img.data[i + 2] > img.data[i] + 25); };
  for (let y = 0; y < H; y++) for (let x = 0; x < W; x++) {
    const i = (y * W + x) * 4; if (!img.data[i + 3]) continue;
    let perto = nuvem(x, y);
    for (let dy = -1; dy <= 1 && !perto; dy++) for (let dx = -1; dx <= 1 && !perto; dx++) perto = nuvem(x + dx, y + dy) && lum(i) < 90;
    if (perto) img.data.copy(o.data, i, i, i + 4);
  }
  return o;
}

GRUPOS.primeiroPlano = function () {
  const c = (n) => semFundo(alphaDuro(cand('PXL-021', n)));
  // cordame da 04: a onda que veio embaixo sai — fica só o que está acima da linha da água (y < 88 de 128)
  const cord = c('fg04_rigging'); const corte = p.blank(cord.width, cord.height);
  cord.data.copy(corte.data, 0, 0, 88 * cord.width * 4);
  const pecas = {
    'Cytera/Foreground/cytera_fg_rope_barrel.png': c('fg04_rope_b'),
    'Cytera/Foreground/cytera_fg_rigging.png': maiorComponente(corte),
    'Eolo/Foreground/eolo_fg_cloud.png': maiorComponente(soNuvem(c('fg06_cloud'))),
    'Eolo/Foreground/eolo_fg_balustrade.png': maiorComponente(c('fg06_rail')),
    'ItacaReturn/Foreground/itaca_ret_fg_weeds.png': c('fg14_weeds'),
    'ItacaReturn/Foreground/itaca_ret_fg_branch.png': maiorComponente(c('fg14_branch')),
    'Pretendentes/Foreground/pret_fg_spilled_amphora.png': c('fg15_spill'),
    'Pretendentes/Foreground/pret_fg_banner_edge.png': c('fg15_banner'),
  };
  for (const [rel, img] of Object.entries(pecas)) folha('Assets/Art/Environments/' + rel, [img], { align: ALIGN.bottom });
};

GRUPOS.eolo = function () {
  // Versão nativa do entablamento: 2× os pixels da antiga, para o vestidor usar escala 1 em vez de esticar 2×.
  folha('Assets/Art/Environments/Eolo/Palace/eolo_entablature_native.png', [semFundo(alphaDuro(cand('PXL-022', 'entab_a')))], { align: ALIGN.bottom });
};

// ================================================================ VISUAL POLISH PASS 02
const pp = (id, nome) => cand(id, nome);
const quadrosDe = (id, nome, idx) => idx.map(i => pp(id, `${nome}_${String(i).padStart(2, '0')}`));

GRUPOS.combate2 = function () {
  // Impacto e arco v2 (pro): substituem fx_hit/fx_slash da rodada 1, que ficam no projeto até a integração escolher.
  folha('Assets/Art/Effects/Combat/fx_hit_v2.png', quadrosDe('PP-01', 'hit_v2_anim', [0, 1, 2, 3]));
  folha('Assets/Art/Effects/Combat/fx_slash_v2.png', quadrosDe('PP-02', 'slash_v2_anim', [0, 1, 2, 3]));
  folha('Assets/Art/Effects/Combat/fx_thrust.png', quadrosDe('PP-01', 'thrust_anim', [0, 1, 2, 3]));
  // Golpe pesado: os quadros 2–3 da animação fecham num anel completo, que não lê como golpe. Ficam 0–1 + dissolução.
  const pesado = quadrosDe('PP-02', 'heavy_anim', [0, 1]);
  folha('Assets/Art/Effects/Combat/fx_heavy_slash.png', [...pesado, dissolver(pesado[1], 0.45), dissolver(pesado[1], 0.8)]);
  // Ricochete por composição: o clarão hit_pro_00 (o candidato sem lascas) + um rastro em zigue-zague que sobe para trás.
  const flash = alphaDuro(pp('PP-01', 'hit_pro_00'));
  const W = 40, H = 32, ox = 18, oy = 6;
  const bronze = PAL['Bronze'];
  const quadro = (k) => {
    const o = p.blank(W, H);
    const f = k === 0 ? flash : dissolver(flash, k === 1 ? 0.35 : 0.75);
    for (let y = 0; y < f.height; y++) for (let x = 0; x < f.width; x++) {
      const s = (y * f.width + x) * 4; if (!f.data[s + 3]) continue;
      const tx = ox + x - 8, ty = oy + y; if (tx >= 0 && ty >= 0 && tx < W && ty < H) f.data.copy(o.data, (ty * W + tx) * 4, s, s + 4);
    }
    // rastro: segmentos que sobem para a esquerda, mais longos a cada quadro
    const pts = [[25, 20], [18, 12], [14, 15], [8, 6], [3, 9]].slice(0, 2 + k);
    for (let i = 0; i + 1 < pts.length; i++) {
      const [x0, y0] = pts[i], [x1, y1] = pts[i + 1], n = Math.max(Math.abs(x1 - x0), Math.abs(y1 - y0));
      for (let t = 0; t <= n; t++) {
        const x = Math.round(x0 + (x1 - x0) * t / n), y = Math.round(y0 + (y1 - y0) * t / n), c = i === pts.length - 2 ? bronze[1] : [250, 240, 214];
        for (const [dx, dy] of [[0, 0], [1, 0], [0, 1]]) { const d = ((y + dy) * W + x + dx) * 4; o.data[d] = c[0]; o.data[d + 1] = c[1]; o.data[d + 2] = c[2]; o.data[d + 3] = 255; }
      }
    }
    return o;
  };
  folha('Assets/Art/Effects/Combat/fx_ricochet.png', [0, 1, 2].map(quadro));
};

// ---------------------------------------------------------------- Grupo 2 — partículas
/**
 * Separa uma fileira de partículas em peças: componentes conectados com tolerância de `folga` px (faíscas e espuma
 * têm pixels quase encostados que são a MESMA partícula). Descarta poeira menor que `minimo` px.
 */
function pecas(img, { folga = 1, minimo = 3 } = {}) {
  const { width: W, height: H } = img, rot = new Int32Array(W * H).fill(-1), out = [];
  let k = 0;
  for (let s = 0; s < W * H; s++) {
    if (rot[s] >= 0 || img.data[s * 4 + 3] < 128) continue;
    const pilha = [s], px = []; rot[s] = k;
    while (pilha.length) {
      const i = pilha.pop(); px.push(i); const x = i % W, y = (i - x) / W;
      for (let dy = -1 - folga; dy <= 1 + folga; dy++) for (let dx = -1 - folga; dx <= 1 + folga; dx++) {
        const nx = x + dx, ny = y + dy; if (nx < 0 || ny < 0 || nx >= W || ny >= H) continue;
        const j = ny * W + nx; if (rot[j] < 0 && img.data[j * 4 + 3] >= 128) { rot[j] = k; pilha.push(j); }
      }
    }
    k++;
    if (px.length < minimo) continue;
    let x0 = W, y0 = H, x1 = 0, y1 = 0;
    for (const i of px) { const x = i % W, y = (i - x) / W; x0 = Math.min(x0, x); y0 = Math.min(y0, y); x1 = Math.max(x1, x); y1 = Math.max(y1, y); }
    const o = p.blank(x1 - x0 + 1, y1 - y0 + 1);
    for (const i of px) { const x = i % W, y = (i - x) / W; img.data.copy(o.data, ((y - y0) * o.width + x - x0) * 4, i * 4, i * 4 + 4); }
    out.push({ img: o, x: x0 });
  }
  return out.sort((a, b) => a.x - b.x).map(c => c.img);
}
/** Várias peças de tamanhos diferentes → células iguais, cada peça centralizada (o pivô de partícula é o centro). */
function celulas(lista) {
  const W = Math.max(...lista.map(i => i.width)), H = Math.max(...lista.map(i => i.height));
  return lista.map(i => { const o = p.blank(W, H), dx = (W - i.width) >> 1, dy = (H - i.height) >> 1;
    for (let y = 0; y < i.height; y++) i.data.copy(o.data, ((y + dy) * W + dx) * 4, y * i.width * 4, (y + 1) * i.width * 4); return o; });
}
/** Folha de partículas: células iguais SEM recorte comum (o recorte cortaria o centro de cada peça). */
const folhaPart = (rel, lista) => folha(rel, celulas(lista.map(alphaDuro)), { align: ALIGN.center, canvasInteiro: true });
/** Partícula desenhada à mão em pixels: lista de [x, y, cor]. */
function desenho(w, h, pts) { const o = p.blank(w, h); for (const [x, y, c] of pts) { const d = (y * w + x) * 4;
  o.data[d] = c[0]; o.data[d + 1] = c[1]; o.data[d + 2] = c[2]; o.data[d + 3] = 255; } return o; }

GRUPOS.particulas = function () {
  const R = 'Assets/Art/Effects/Particles/';
  const BR = PAL['Bronze'], BRANCO = [255, 246, 220];
  // Faíscas: as 6 manchas quentes do pixen + 4 lascas de faísca diagonais desenhadas aqui (o pixen as fazia de cometa).
  const lascas = [
    [[0, 3, BRANCO], [1, 2, BRANCO], [2, 1, BR[0]], [3, 0, BR[1]]],
    [[0, 2, BRANCO], [1, 2, BRANCO], [2, 1, BR[0]], [3, 1, BR[1]], [4, 0, BR[2]]],
    [[0, 0, BRANCO], [1, 1, BR[0]], [2, 1, BR[1]], [3, 2, BR[2]]],
    [[0, 4, BR[0]], [1, 3, BRANCO], [1, 2, BRANCO], [2, 1, BR[0]], [3, 0, BR[2]]],
  ].map(pts => desenho(5, 5, pts));
  folhaPart(R + 'Combat/ptc_spark.png', [...pecas(cand('PP-04', 'sparks_a')), ...lascas]);
  // Lascas físicas (madeira, pedra, bronze) e o debris TINGÍVEL: as mesmas peças em cinza claro, para o código colorir.
  const chips = pecas(cand('PP-04', 'chips_a'), { minimo: 2 });
  folhaPart(R + 'Debris/ptc_chip.png', chips);
  const cinza = (img) => { const o = p.blank(img.width, img.height); img.data.copy(o.data);
    for (let i = 0; i < o.width * o.height; i++) { const d = i * 4; if (!o.data[d + 3]) continue;
      const L = Math.round(0.2126 * o.data[d] + 0.7152 * o.data[d + 1] + 0.0722 * o.data[d + 2]);
      const v = Math.min(255, 140 + Math.round(L * 0.45)); o.data[d] = v; o.data[d + 1] = v; o.data[d + 2] = v; } return o; };
  const cacos = [[[0, 0], [1, 0], [0, 1]], [[0, 0], [1, 0], [1, 1], [2, 1]], [[1, 0], [0, 1], [1, 1], [2, 1], [1, 2]], [[0, 0], [1, 1]]]
    .map(pts => desenho(3, 3, pts.map(([x, y]) => [x, y, [205, 205, 205]])));
  folhaPart(R + 'Debris/ptc_debris_tint.png', [...chips.map(cinza), ...cacos]);
  // Poeira: lufadas de corrida, pouso e queda animados (pro), derrapagem estática.
  folhaPart(R + 'Dust/ptc_dust_puff.png', pecas(cand('PP-05', 'rundust_b')));
  const pouso = quadrosDe('PP-05', 'land_anim', [0, 1, 2, 3]);
  folha(R + 'Dust/fx_land_dust.png', [...pouso, dissolver(pouso[3], 0.6)], { align: ALIGN.bottom });
  const queda = quadrosDe('PP-05', 'fall_anim', [0, 1, 2, 3]);
  folha(R + 'Dust/fx_fall_dust.png', [...queda, dissolver(queda[3], 0.6)], { align: ALIGN.bottom });
  folha(R + 'Dust/ptc_skid_dust.png', [cand('PP-05', 'land_pro_03')], { align: ALIGN.bottom });
  // Água e natureza.
  folhaPart(R + 'Water/ptc_drop.png', pecas(cand('PP-06', 'drops_a'), { folga: 0, minimo: 1 }));
  folhaPart(R + 'Water/ptc_foam.png', pecas(cand('PP-06', 'foam_b')));
  folhaPart(R + 'Nature/ptc_leaf.png', pecas(cand('PP-07', 'leaves_a')));
  // Areia: grãos de 1–3 px nas três cores de Terra/caminho — menor que qualquer geração faria com nitidez.
  const T = PAL['Terra / caminho'];
  const graos = [[[0, 0, T[0]]], [[0, 0, T[1]], [1, 0, T[0]]], [[0, 0, T[1]], [0, 1, T[2]]], [[0, 0, T[0]], [1, 0, T[1]], [0, 1, T[1]], [1, 1, T[2]]],
    [[1, 0, T[0]], [0, 1, T[1]], [1, 1, T[1]], [2, 1, T[2]]], [[0, 0, T[2]]]].map(pts => desenho(3, 2, pts));
  folhaPart(R + 'Environment/ptc_sand.png', graos);
};

// ---------------------------------------------------------------- Grupo 3 — água
GRUPOS.agua = function () {
  const R = 'Assets/Art/Effects/Water/';
  const grande = quadrosDe('PP-08', 'splash_l_anim', [0, 1, 2, 3]);
  folha(R + 'fx_splash_large.png', [...grande, dissolver(grande[3], 0.6)], { align: ALIGN.bottom });
  // Splash pequeno = o grande reamostrado a 50% por moda: mesma família, 0 gerações (o pixen fazia jato de chafariz).
  folha(R + 'fx_splash_small.png', [...grande, dissolver(grande[3], 0.6)].map(q => moda(alphaDuro(q), 0.5)), { align: ALIGN.bottom });
  // Ondinha: duas elipses na rampa Água que se abrem e somem — código desenha mais nítido que uma geração.
  const A = PAL['Agua'], W = 32, H = 10;
  const anel = (t) => { const o = p.blank(W, H);
    const pinta = (rx, ry, c, falha) => { for (let a = 0; a < 360; a += 2) { const x = Math.round(W / 2 + rx * Math.cos(a * Math.PI / 180)), y = Math.round(H / 2 + ry * Math.sin(a * Math.PI / 180));
      if (x < 0 || y < 0 || x >= W || y >= H || (falha && (a / 2) % 5 === 0)) continue; const d = (y * W + x) * 4; o.data[d] = c[0]; o.data[d + 1] = c[1]; o.data[d + 2] = c[2]; o.data[d + 3] = 255; } };
    pinta(5 + t * 3.5, 1.5 + t * 0.9, t < 2 ? [236, 246, 248] : A[0], t >= 3);
    if (t < 3) pinta(2 + t * 2, 0.8 + t * 0.5, A[1], t >= 2);
    return o; };
  folha(R + 'fx_ripple.png', [0, 1, 2, 3].map(anel), { align: ALIGN.center, canvasInteiro: true });
  // Crista da onda grande de Citera: a região animada volta sobre a onda original, mesmo canvas (troca direta).
  const onda = p.read(path.join(ROOT, 'Assets/Art/Environments/Cytera/Ocean/cytera_wave_large.png'));
  const rc = JSON.parse(fs.readFileSync(path.join(PX, 'PP-08/_ref/wave_crest.json'), 'utf8'));
  const crista = quadrosDe('PP-08', 'crest_anim', [0, 1, 2, 3]).map(alphaDuro);
  folha('Assets/Art/Environments/Cytera/Ocean/cytera_wave_large_anim.png', [0, 1, 2, 3, 2, 1].map(i => {
    const o = p.blank(onda.width, onda.height); onda.data.copy(o.data);
    for (let y = 0; y < rc.h; y++) for (let x = 0; x < rc.w; x++) {
      const d = ((rc.y0 + y) * onda.width + rc.x0 + x) * 4, s = (y * crista[i].width + x) * 4;
      crista[i].data.copy(o.data, d, s, s + 4);
    }
    return o;
  }), { align: ALIGN.bottom, canvasInteiro: true });
};

// ---------------------------------------------------------------- Grupo 4 — tempestade de Citera
GRUPOS.tempestade = function () {
  const R = 'Assets/Art/Effects/Storm/', W = 'Assets/Art/Environments/Cytera/Weather/';
  // Aviso do raio (fase warn do StormHazard): faixa de arcos do pixen sem a linha escura da base, animada em loop.
  folha(R + 'fx_storm_warn_crackle.png', quadrosDe('PP-09', 'warn_anim', [0, 1, 2, 3]), { align: ALIGN.bottom, canvasInteiro: true });
  // Impacto do raio no chão: só os raios claros do strike_b (a fumaça cinza com brasas vermelhas pesava demais).
  // Quadros: núcleo → explosão inteira → duas dissoluções.
  const raios = alphaDuro(prep9('strike_rays'));
  const nucleo = (() => { const o = p.blank(raios.width, raios.height); raios.data.copy(o.data);
    for (let y = 0; y < o.height; y++) for (let x = 0; x < o.width; x++)
      if (Math.hypot(x - o.width / 2, (y - o.height) * 1.3) > 16) o.data[(y * o.width + x) * 4 + 3] = 0; return o; })();
  folha(R + 'fx_storm_strike.png', [nucleo, raios, dissolver(raios, 0.45), dissolver(raios, 0.8)], { align: ALIGN.bottom, canvasInteiro: true });
  // Raio: o de Fase 04 é bom (ramificado); daqui saem o espelhado e uma sequência de golpe com o mesmo canvas
  // (cheio → só o núcleo branco → só o rastro cinza → dissolvido) para a fase strike não ser um sprite parado.
  const raio = p.read(path.join(ROOT, W + 'cytera_lightning.png'));
  const soCor = (img, manter) => { const o = p.blank(img.width, img.height); img.data.copy(o.data);
    for (let i = 0; i < o.width * o.height; i++) { const d = i * 4; if (o.data[d + 3] && !manter(o.data[d])) o.data[d + 3] = 0; } return o; };
  const rastro = soCor(raio, r => r < 200);
  folha(W + 'cytera_lightning_b.png', [espelhar(raio)], { align: ALIGN.bottom, canvasInteiro: true });
  folha(W + 'cytera_lightning_strike.png', [raio, soCor(raio, r => r >= 200), rastro, dissolver(rastro, 0.6)], { align: ALIGN.bottom, canvasInteiro: true });
  // Nuvem iluminada por baixo pelo relâmpago: a mesma nuvem clareada por baixo e puxada para azul-branco (troca por 1–2 quadros).
  const nuvem = p.read(path.join(ROOT, 'Assets/Art/Environments/Cytera/Background/cytera_storm_cloud.png'));
  const lit = p.blank(nuvem.width, nuvem.height); nuvem.data.copy(lit.data);
  for (let i = 0; i < lit.width * lit.height; i++) { const d = i * 4; if (!lit.data[d + 3]) continue;
    // o contorno fica; o resto clareia mais embaixo (a luz vem do raio, abaixo da nuvem)
    if (lit.data[d] + lit.data[d + 1] + lit.data[d + 2] < 90) continue;
    const t = 0.18 + 0.32 * Math.floor(i / lit.width) / lit.height, alvo = [214, 226, 240];
    for (let c = 0; c < 3; c++) lit.data[d + c] = Math.round(lit.data[d + c] + (alvo[c] - lit.data[d + c]) * t); }
  folha('Assets/Art/Environments/Cytera/Background/cytera_storm_cloud_lit.png', [lit], { align: ALIGN.bottom, canvasInteiro: true });
};
const prep9 = (nome) => p.read(path.join(PX, 'PP-09/_prep', nome + '.png'));

// ---------------------------------------------------------------- Grupo 5 — Circe
GRUPOS.circe = function () {
  const R = 'Assets/Art/Effects/Magic/';
  const lum = (d, i) => 0.2126 * d[i] + 0.7152 * d[i + 1] + 0.0722 * d[i + 2];
  const mapear = (img, fn) => { const o = p.blank(img.width, img.height); img.data.copy(o.data);
    for (let i = 0; i < o.width * o.height; i++) { const d = i * 4; if (o.data[d + 3]) fn(o.data, d); } return o; };
  // Círculo de Circe (frente): meandro + luas + roda de Hécate do pixen circle_b, sem o fundo escuro (pixels com
  // luminância < 70). Pulso por código: base → clareado → base → escurecido.
  const circ = alphaDuro(p.read(path.join(PX, 'PP-10/_prep/circle_clean.png')));
  const tom = (t) => mapear(circ, (o, d) => { for (let c = 0; c < 3; c++)
    o[d + c] = Math.round(t > 0 ? o[d + c] + (255 - o[d + c]) * t : o[d + c] * (1 + t)); });
  folha(R + 'fx_circe_circle.png', [circ, tom(0.3), circ, tom(-0.22)], { align: ALIGN.center, canvasInteiro: true });
  // O mesmo círculo no chão (TransformationZone): desenhado por código — projetar o círculo achatado borrava o
  // meandro, e a elipse do pixen (ground_a) vinha cheia de ruído. Anel de luas gira e fagulhas sobem a cada quadro.
  const MENTA = [[206, 255, 233], [169, 241, 204], [110, 200, 160]], OURO = [[254, 239, 144], [220, 186, 96], [182, 142, 67]];
  const chao = (k) => {
    const W = 128, H = 34, o = p.blank(W, H), cx = 63.5, cy = 17, rad = (a) => a * Math.PI / 180;
    const px = (x, y, c) => { x = Math.round(x); y = Math.round(y); if (x < 0 || y < 0 || x >= W || y >= H) return;
      const d = (y * W + x) * 4; o.data[d] = c[0]; o.data[d + 1] = c[1]; o.data[d + 2] = c[2]; o.data[d + 3] = 255; };
    const elip = (rx, ry, c, passo = 0.5, filtro) => { for (let a = 0; a < 360; a += passo)
      if (!filtro || filtro(a)) px(cx + rx * Math.cos(rad(a)), cy + ry * Math.sin(rad(a)), c); };
    // dois anéis menta (alternam de tom: pulso) e, entre eles, ganchos radiais alternados — o meandro achatado
    const anel = MENTA[k % 2];
    elip(62, 15.5, anel); elip(56, 13.4, anel);
    for (let a = 0; a < 360; a += 7.5) {
      const impar = (a / 7.5) % 2, [r0, r1] = impar ? [57, 60] : [58, 61.5], t = rad(a);
      for (let r = r0; r <= r1; r += 0.5) px(cx + r * Math.cos(t), cy + r * Math.sin(t) * 15.5 / 62, MENTA[1]);
      if (impar) px(cx + 60 * Math.cos(t + 0.05), cy + 60 * Math.sin(t + 0.05) * 15.5 / 62, MENTA[1]);
    }
    // anel de luas: traços dourados que giram 7,5° por quadro
    const lua = (a) => ((a + k * 7.5) % 30) < 14;
    elip(40, 9.8, OURO[0], 0.5, lua); elip(41, 10.3, OURO[2], 0.5, lua);
    elip(36, 8.6, MENTA[2], 1);
    // centro: três pequenos anéis — a roda de Hécate achatada
    for (const [dx, dy] of [[-7, 1.5], [7, 1.5], [0, -2]])
      for (let a = 0; a < 360; a += 10) px(cx + dx + 3 * Math.cos(rad(a)), cy + dy + 1.3 * Math.sin(rad(a)), OURO[0]);
    // fagulhas que sobem
    for (const [x, f] of [[14, 0], [26, 3], [40, 1], [52, 6], [64, 2], [76, 5], [90, 4], [102, 7], [114, 1]]) {
      const y = cy - 4 - ((k * 3 + f * 2) % 10); if (y <= 0) continue;
      px(x, y, (f + k) % 2 ? MENTA[0] : OURO[0]); if ((f + k) % 3 === 0) px(x, y - 1, MENTA[0]);
    }
    return o;
  };
  folha(R + 'fx_circe_circle_ground.png', [0, 1, 2, 3].map(chao), { align: ALIGN.bottom, canvasInteiro: true });
  // Transformação (porco): o puff do pixen animado + duas dissoluções.
  const transf = quadrosDe('PP-11', 'transform_anim', [0, 1, 2, 3]).map(alphaDuro);
  folha(R + 'fx_circe_transform.png', [...transf, dissolver(transf[3], 0.45), dissolver(transf[3], 0.8)], { align: ALIGN.center, canvasInteiro: true });
  // Cura pela moly: a mesma explosão ao contrário (fecha em vez de abrir), com o lilás trocado por dourado — lê como
  // o oposto da transformação sem gerar nada.
  const dourar = (img) => mapear(img, (o, d) => { const r = o[d], g = o[d + 1], b = o[d + 2];
    if (b > r + 10 && b > g) { const L = lum(o, d), c = L > 150 ? OURO[0] : L > 95 ? OURO[1] : OURO[2]; o[d] = c[0]; o[d + 1] = c[1]; o[d + 2] = c[2]; } });
  folha(R + 'fx_circe_cure.png', [dissolver(transf[3], 0.6), ...[3, 2, 1, 0].map(i => transf[i]), dissolver(transf[0], 0.6)].map(dourar),
    { align: ALIGN.center, canvasInteiro: true });
  // Fumaça mágica em loop: as fitas menta/lilás do pixen animado, sem o xadrez cinza que veio pintado entre elas.
  const fitas = (img) => mapear(img, (o, d) => { const mx = Math.max(o[d], o[d + 1], o[d + 2]), mn = Math.min(o[d], o[d + 1], o[d + 2]);
    if (!((mx - mn > 40 && mx > 110) || mx > 200)) o[d + 3] = 0; });
  folha(R + 'fx_circe_smoke.png', quadrosDe('PP-11', 'smoke_anim', [0, 1, 2, 3]).map(alphaDuro).map(fitas), { align: ALIGN.bottom, canvasInteiro: true });
};

// ---------------------------------------------------------------- Grupo 6 — Mundo dos Mortos
GRUPOS.mortos = function () {
  // Braseiro de fogo espectral: a chama quente animada pela receita de fogo (recorte da chama → anim → recompor) e,
  // depois, tudo que é quente vai para uma rampa ciano-gélida por luminância (no vaso, abaixo da linha 90, dois
  // degraus mais escura). Mesmo canvas e pivô do braseiro original: troca direta na cena.
  const ref = JSON.parse(fs.readFileSync(path.join(PX, 'PP-12/_ref/brazier_flame.json'), 'utf8'));
  const fonte = p.read(path.join(ROOT, ref.fonte));
  const GELO = [[22, 64, 78], [38, 120, 134], [84, 192, 198], [168, 238, 230], [236, 255, 250]];
  const espectral = (img) => { const o = p.blank(img.width, img.height); img.data.copy(o.data);
    for (let y = 0; y < o.height; y++) for (let x = 0; x < o.width; x++) { const d = (y * o.width + x) * 4;
      if (!o.data[d + 3] || !ehChama(o.data[d], o.data[d + 1], o.data[d + 2])) continue;
      const L = 0.2126 * o.data[d] + 0.7152 * o.data[d + 1] + 0.0722 * o.data[d + 2];
      // abaixo da taça os realces do vaso viram reflexo frio, dois degraus mais escuros (não brilham como a chama)
      const k = Math.min(4, Math.max(0, Math.floor((L - 40) / 45))) - (y >= 90 ? 2 : 0), c = GELO[Math.max(0, k)]; o.data[d] = c[0]; o.data[d + 1] = c[1]; o.data[d + 2] = c[2]; }
    return o; };
  folha('Assets/Art/Environments/MundoDosMortos/Props/mortos_brazier_tall_spectral_anim.png',
    [0, 1, 2, 3, 2, 1].map(i => espectral(recompor(fonte, ref, alphaDuro(cand('PP-12', `brazier_anim_${String(i).padStart(2, '0')}`))))),
    { align: ALIGN.bottom, canvasInteiro: true });
  // Alma errante: a sombra encapuzada do pixen (soul_a) flutuando em pingue-pongue; a mesma reamostrada a 50% para as
  // almas do fundo; e o "sumir" (dissolução) para quando ela atravessa a parede ou o jogador chega perto.
  const R = 'Assets/Art/Environments/MundoDosMortos/Souls/';
  const alma = [0, 1, 2, 3, 2, 1].map(i => alphaDuro(cand('PP-12', `soul_anim_${String(i).padStart(2, '0')}`)));
  folha(R + 'mortos_soul_wander.png', alma, { align: ALIGN.bottom, canvasInteiro: true });
  folha(R + 'mortos_soul_wander_small.png', alma.map(q => moda(q, 0.5)), { align: ALIGN.bottom, canvasInteiro: true });
  folha(R + 'mortos_soul_vanish.png', [alma[0], ...[0.3, 0.55, 0.8].map(t => dissolver(alma[0], t))], { align: ALIGN.bottom, canvasInteiro: true });
};

// ---------------------------------------------------------------- Grupo 7 — Cila e Caríbdis
/** Quadro `n` de uma folha já montada por `folha` (células de `cw`×`ch` com 1 px de folga). */
function quadroDaFolha(rel, n, cw, ch) { return p.crop(p.read(path.join(ROOT, rel)), n * (cw + 1), 0, cw, ch); }
/** Cola `src` sobre `dst` com o centro da base em (cx, base). */
function colarBase(dst, src, cx, base) {
  const ox = Math.round(cx - src.width / 2), oy = base - src.height;
  for (let y = 0; y < src.height; y++) for (let x = 0; x < src.width; x++) {
    const s = (y * src.width + x) * 4, tx = ox + x, ty = oy + y;
    if (src.data[s + 3] && tx >= 0 && ty >= 0 && tx < dst.width && ty < dst.height) src.data.copy(dst.data, (ty * dst.width + tx) * 4, s, s + 4);
  }
}
GRUPOS.cila = function () {
  // Maré mortal (TidalHazard, 2 na Fase 11): o placeholder é desligado pelo dresser e nada a desenhava — o jogador
  // morria para uma caixa invisível. `surge_a` (pixen) saiu nas cores e no traço do cila_sea_churn; anim em loop.
  // 128 px = 2,99 un: cobre o colisor de 3 × 1 un; o pivô é a base (o colisor sobe e desce, a arte vai junto).
  folha('Assets/Art/Environments/CilaCaribdis/Ocean/cila_tidal_surge_anim.png',
    [0, 1, 2, 3].map(i => alphaDuro(cand('PP-13', `surge_anim_${String(i).padStart(2, '0')}`))), { align: ALIGN.bottom, canvasInteiro: true });
  // Golpe da Cila (BossController: AttackExecuted em cada attackPoint): splash grande (G3) + poeira de queda com
  // rachadura (G2) na mesma base — a cabeça bate no convés molhado. 0 gerações. Os dois vêm recoloridos por
  // luminância: a água para as cores do cila_sea_churn (verde-garrafa + espuma), a poeira bege para basalto.
  const porLuz = (img, rampa) => { const o = p.blank(img.width, img.height); img.data.copy(o.data);
    for (let i = 0; i < o.width * o.height; i++) { const d = i * 4; if (!o.data[d + 3]) continue;
      const L = 0.2126 * o.data[d] + 0.7152 * o.data[d + 1] + 0.0722 * o.data[d + 2];
      const c = rampa.find(([lim]) => L < lim)[1]; o.data[d] = c[0]; o.data[d + 1] = c[1]; o.data[d + 2] = c[2]; }
    return o; };
  const MAR = [[60, [40, 59, 57]], [110, [57, 96, 82]], [170, [111, 154, 136]], [215, [196, 203, 194]], [256, [243, 243, 237]]];
  const BASALTO = [[70, [30, 34, 35]], [120, [62, 68, 70]], [170, [98, 104, 104]], [256, [146, 150, 146]]];
  const W = 72, H = 64;
  folha('Assets/Art/Effects/Boss/fx_scylla_strike.png', [0, 1, 2, 3, 4].map(k => {
    const o = p.blank(W, H);
    colarBase(o, porLuz(quadroDaFolha('Assets/Art/Effects/Water/fx_splash_large.png', k, 63, 58), MAR), W / 2, H);
    colarBase(o, porLuz(quadroDaFolha('Assets/Art/Effects/Particles/Dust/fx_fall_dust.png', k, 59, 24), BASALTO), W / 2, H);
    return o;
  }), { align: ALIGN.bottom, canvasInteiro: true });
  // Aviso (AttackTelegraphed, 0,7 s): a sombra da cabeça cresce no chão — elipse escura com miolo pontilhado
  // (alpha duro não faz sombra translúcida) e duas gotas caindo da boca da serpente.
  const SOMBRA = [10, 26, 28], GOTA = PAL['Agua'][0];
  folha('Assets/Art/Effects/Boss/fx_scylla_telegraph.png', [0, 1, 2, 3].map(k => {
    const w = 60, h = 40, o = p.blank(w, h), rx = 10 + k * 5.5, ry = 2.5 + k * 1.1, cx = w / 2 - 0.5, cy = h - 5;
    const pinta = (x, y, c) => { const d = (y * w + x) * 4; o.data[d] = c[0]; o.data[d + 1] = c[1]; o.data[d + 2] = c[2]; o.data[d + 3] = 255; };
    for (let y = 0; y < h; y++) for (let x = 0; x < w; x++) {
      const e = ((x - cx) / rx) ** 2 + ((y - cy) / ry) ** 2;
      if (e <= 1 && (e > 0.6 || (x + y) % 2 === 0)) pinta(x, y, SOMBRA);
    }
    for (const [gx, fase] of [[cx - 4, 0], [cx + 5, 2]]) { const gy = Math.round(4 + ((k + fase) % 4) * 7);
      for (const dy of [0, 1, 2]) pinta(Math.round(gx), gy + dy, dy === 2 ? [236, 246, 248] : GOTA); }
    return o;
  }), { align: ALIGN.bottom, canvasInteiro: true });
};

// ---------------------------------------------------------------- Grupo 8 — fogo
GRUPOS.fogo2 = function () {
  // Os quatro fogos que ficaram parados na rodada 1: mesma receita (recorte da chama → anim → recompor no sprite
  // original, pingue-pongue). Mesmo canvas e pivô de cada original — troca direta. O braseiro dos Mortos não está
  // aqui: virou espectral no Grupo 6.
  const R = JSON.parse(fs.readFileSync(path.join(PX, 'PP-07/_ref/recortes.json'), 'utf8'));
  const PINGUE = [0, 1, 2, 3, 2, 1];
  for (const [n, ret] of Object.entries(R)) {
    const fonte = p.read(path.join(ROOT, ret.fonte));
    // Fogueira gigante dos Ciclopes: a anim também redesenhou toras e pedras (laranja = "quente" para o filtro).
    // `soChama`: pixel opaco do original que não é chama nunca muda; `protege`: retângulos (no recorte) intactos.
    const guarda = (q) => { if (!ret.soChama && !ret.protege) return q;
      for (let y = 0; y < ret.h; y++) for (let x = 0; x < ret.w; x++) {
        const d = ((ret.y0 + y) * fonte.width + ret.x0 + x) * 4;
        const protegido = (ret.protege || []).some(([px, py, pw, ph]) => x >= px && x < px + pw && y >= py && y < py + ph);
        const solido = ret.soChama && fonte.data[d + 3] && !ehChama(fonte.data[d], fonte.data[d + 1], fonte.data[d + 2]);
        if (protegido || solido) fonte.data.copy(q.data, d, d, d + 4);
      }
      return q; };
    folha(ret.fonte.replace(/\.png$/, '_anim.png'),
      PINGUE.map(i => guarda(recompor(fonte, ret, alphaDuro(cand('PP-07', `${n}_anim_${String(i).padStart(2, '0')}`))))),
      { align: ALIGN.bottom, canvasInteiro: true });
  }
  // Chama pequena: lamparina grega de terracota (lychnos, pixen) — Ítaca, o quarto do Ending, o templo de Circe.
  // A argila laranja também é "quente": só a faixa da chama (acima da linha 14) acompanha a animação.
  const lamp = alphaDuro(cand('PP-07', 'lamp_a'));
  folha('Assets/Art/Environments/Shared/Props/shared_oil_lamp_anim.png', PINGUE.map(i => {
    const q = alphaDuro(cand('PP-07', `lamp_anim_${String(i).padStart(2, '0')}`)), o = p.blank(lamp.width, lamp.height); lamp.data.copy(o.data);
    for (let y = 0; y < 14; y++) for (let x = 0; x < o.width; x++) { const d = (y * o.width + x) * 4; q.data.copy(o.data, d, d, d + 4); }
    return o;
  }), { align: ALIGN.bottom });
};

// ---------------------------------------------------------------- Grupo 9 — mapa
GRUPOS.mapa2 = function () {
  const R = 'Assets/Art/Map/States/', BR = PAL['Bronze'], AG = PAL['Agua'], VI = PAL['Vinho'], CONT = [5, 5, 7];
  const tela = (w, h) => { const o = p.blank(w, h); o.px = (x, y, c) => { x = Math.round(x); y = Math.round(y);
    if (x < 0 || y < 0 || x >= w || y >= h) return; const d = (y * w + x) * 4; o.data[d] = c[0]; o.data[d + 1] = c[1]; o.data[d + 2] = c[2]; o.data[d + 3] = 255; }; return o; };
  // COMPLETED: coroa de louros (pixen laurel_a) — vai ATRÁS do medalhão (raio 27,5): as folhas aparecem em volta do
  // aro. A dourada (laurel_b) se confundia com o aro de bronze.
  folha(R + 'map_state_completed_wreath.png', [alphaDuro(cand('PP-16', 'laurel_a'))], { align: ALIGN.center, canvasInteiro: true });
  // CURRENT: anel de seleção em volta do medalhão (fora do raio 27,5, não cobre nada): aro dourado com contorno e
  // quatro contas claras que giram 22,5° por quadro (4 quadros = 90° = volta completa, já que as contas se repetem a
  // cada 90°) + um segundo aro que pulsa.
  folha(R + 'map_state_current_ring.png', [0, 1, 2, 3].map(k => {
    const S = 72, c = (S - 1) / 2, o = tela(S, S);
    for (let y = 0; y < S; y++) for (let x = 0; x < S; x++) {
      const d = Math.hypot(x - c, y - c), a = (Math.atan2(y - c, x - c) * 180 / Math.PI + 360 + 45) % 90;
      if (d > 28.4 && d <= 29.4) o.px(x, y, CONT);
      else if (d > 29.4 && d <= 31.6) {
        const conta = Math.abs(a - (k * 22.5 + 11.25)) < 7;
        o.px(x, y, conta ? [255, 246, 220] : y < c ? BR[0] : BR[1]);
      } else if (d > 31.6 && d <= 32.6) o.px(x, y, CONT);
      else if (k % 2 === 0 && d > 34 && d <= 35 && Math.round(a) % 12 < 6) o.px(x, y, BR[0]);
    }
    return o;
  }), { align: ALIGN.center, canvasInteiro: true });
  // Brilhos do mar: um "~" que nasce, abre e some (o código espalha vários com fases diferentes sobre map_aegean).
  folha('Assets/Art/Map/map_wave_glint.png', [0, 1, 2, 3].map(k => {
    const o = tela(11, 4), larg = [3, 7, 9, 5][k], cor = k === 2 ? AG[1] : AG[0];
    for (let i = 0; i < larg; i++) { const x = 5 - (larg >> 1) + i; o.px(x, 1 + (i % 4 === 1 || i % 4 === 2 ? 0 : 1), cor); }
    return o;
  }), { align: ALIGN.center, canvasInteiro: true });
  // Trilha do navio: ponto já navegado (vinho, contornado) e ponto por navegar (areia clara, sem contorno).
  const ponto = (cor, borda) => { const o = tela(5, 5);
    for (let y = 0; y < 5; y++) for (let x = 0; x < 5; x++) { const d = Math.hypot(x - 2, y - 2);
      if (d <= 1.2) o.px(x, y, cor); else if (borda && d <= 2.3) o.px(x, y, CONT); } return o; };
  folha('Assets/Art/Map/map_trail_dot.png', [ponto(VI[0], true), ponto([236, 222, 186], false)], { align: ALIGN.center, canvasInteiro: true });
};

// ---------------------------------------------------------------- Grupo 10 — transições
GRUPOS.transicoes = function () {
  const R = 'Assets/Art/UI/Transitions/', BR = PAL['Bronze'], NO = PAL['Noite'], CONT = [5, 5, 7];
  const tela = (w, h) => { const o = p.blank(w, h); o.px = (x, y, c) => { x = Math.round(x); y = Math.round(y);
    if (x < 0 || y < 0 || x >= w || y >= h) return; const d = (y * w + x) * 4; o.data[d] = c[0]; o.data[d + 1] = c[1]; o.data[d + 2] = c[2]; o.data[d + 3] = 255; }; return o; };
  // Faixa de meandro (a "grega"): ladrilho horizontal de 15 px — o código desenha a chave exata e contínua, coisa
  // que geração não garante. Fundo Noite, linhas de bronze com luz em cima, filetes de borda. A transição de cena
  // são duas faixas que se fecham sobre a tela (integração); a faixa também serve de moldura de painel.
  // Chave clássica numa grade de unidades de 2×2 px (linha 2 px, vão 2 px), período 6 unidades = 12 px:
  // a base é contínua e o traço vertical da esquerda emenda no ladrilho seguinte.
  const CHAVE = ['11111.', '1...1.', '1.111.', '1.....', '111111'];
  const U = 2, W = 6 * U, H = 5 * U + 8;
  const meandro = tela(W, H);
  for (let y = 0; y < H; y++) for (let x = 0; x < W; x++) meandro.px(x, y, NO[2]);
  for (let x = 0; x < W; x++) { meandro.px(x, 0, CONT); meandro.px(x, 1, BR[0]); meandro.px(x, 2, BR[2]);
    meandro.px(x, H - 3, BR[1]); meandro.px(x, H - 2, BR[3]); meandro.px(x, H - 1, CONT); }
  CHAVE.forEach((linha, r) => [...linha].forEach((v, c) => { if (v !== '1') return;
    for (let dy = 0; dy < U; dy++) for (let dx = 0; dx < U; dx++) {
      // luz em cima: o pixel de cima de um traço horizontal (sem linha acima) é o tom claro
      const topo = dy === 0 && !(r > 0 && CHAVE[r - 1][c] === '1');
      meandro.px(c * U + dx, 4 + r * U + dy, topo ? BR[0] : BR[1]);
    } }));
  folha(R + 'ui_meander_band_tile.png', [meandro], { align: ALIGN.center, canvasInteiro: true });

  // Vitória (LevelCompleteMenu): a coroa DOURADA (laurel_b, que no mapa perdia para a verde) cresce de 50% a 100%
  // enquanto oito faíscas (ptc_spark, G2) se abrem em leque e se dissolvem; o último quadro segura a coroa.
  const coroa = alphaDuro(cand('PP-16', 'laurel_b')), S = 112, c = S / 2;
  const fai = [0, 1, 2, 3, 4, 5, 6, 7].map(n => quadroDaFolha('Assets/Art/Effects/Particles/Combat/ptc_spark.png', n % 10, 7, 7));
  const quadroV = (escala, raio, sumir) => {
    const o = p.blank(S, S), cr = escala === 1 ? coroa : moda(coroa, escala);
    colarBase(o, cr, c, Math.round(c + cr.height / 2));
    if (raio) fai.forEach((f, n) => { const a = n * Math.PI / 4 + Math.PI / 8;
      colarBase(o, sumir ? dissolver(f, sumir) : f, c + raio * Math.cos(a), Math.round(c + raio * Math.sin(a) + 3)); });
    return o; };
  folha(R + 'fx_victory_laurel.png', [quadroV(0.5, 0), quadroV(0.75, 30), quadroV(1, 44), quadroV(1, 50, 0.5), quadroV(1, 0)],
    { align: ALIGN.center, canvasInteiro: true });

  // Morte (DeathOverlay / GameOverScreen): vinheta 9-slice — borda de 16 px em pontilhado ordenado (alpha duro),
  // densa na beira e rala para dentro, vinho-escuro; o miolo é vazio e estica.
  const B = [[0, 8, 2, 10], [12, 4, 14, 6], [3, 11, 1, 9], [15, 7, 13, 5]], VI = PAL['Vinho'][3], V = 48, borda = 16;
  const vinheta = p.blank(V, V);
  for (let y = 0; y < V; y++) for (let x = 0; x < V; x++) {
    const dist = Math.min(x, y, V - 1 - x, V - 1 - y); if (dist >= borda) continue;
    if ((B[y & 3][x & 3] + 0.5) / 16 < 1 - dist / borda) { const d = (y * V + x) * 4;
      vinheta.data[d] = VI[0]; vinheta.data[d + 1] = VI[1]; vinheta.data[d + 2] = VI[2]; vinheta.data[d + 3] = 255; }
  }
  folha(R + 'ui_death_vignette_9slice.png', [vinheta], { align: ALIGN.center, canvasInteiro: true });
  const meta = path.join(ROOT, R + 'ui_death_vignette_9slice.png.meta');
  fs.writeFileSync(meta, fs.readFileSync(meta, 'utf8').replace(/spriteBorder: \{x: 0, y: 0, z: 0, w: 0\}/, `spriteBorder: {x: ${borda}, y: ${borda}, z: ${borda}, w: ${borda}}`));
};

// ---------------------------------------------------------------- Grupo 11 — Ending
GRUPOS.ending = function () {
  // O quarto de Odisseu e Penélope: a cama construída em volta do tronco VIVO de uma oliveira (o sinal do
  // reconhecimento, Odisseia XXIII), janelas para o mar de Ítaca ao amanhecer, friso de meandro e os teares.
  // `bedroom_b` (pixen 604×340) com UMA correção: a lamparina de vidro com chaminé (anacrônica) sai — a parede é
  // refeita copiando a mesma fileira de pedras 26 px à esquerda (juntas alinhadas), com o brilho corrigido linha a
  // linha pela parede real em volta e quantizada para as cores da própria imagem — e entra a lychnos do Grupo 8.
  // A chama animada (shared_oil_lamp_anim) pode ir por cima no mesmo ponto: centro x 223, base y 246.
  const b = cand('PP-14', 'bedroom_b'), src = p.blank(b.width, b.height); b.data.copy(src.data);
  const X0 = 213, X1 = 234, Y0 = 197, Y1 = 236, DX = 26, CX = 223, BASE = 246;
  const lum = (x, y) => { const d = (y * b.width + x) * 4; return 0.2126 * src.data[d] + 0.7152 * src.data[d + 1] + 0.0722 * src.data[d + 2]; };
  const media = (xs, y, off) => xs.reduce((a, x) => a + lum(x - off, y), 0) / xs.length;
  const cores = [...new Set([...Array(b.width * b.height).keys()].filter(i => { const x = i % b.width, y = (i / b.width) | 0;
    return !(x >= X0 && x <= X1 && y >= Y0 && y <= Y1); }).map(i => src.data.readUInt32BE(i * 4) >>> 8))].map(k => [k >> 16, (k >> 8) & 255, k & 255]);
  for (let y = Y0; y <= Y1; y++) {
    const esq = [X0 - 3, X0 - 2, X0 - 1], dir = [X1 + 1, X1 + 2, X1 + 3];
    const gE = media(esq, y, 0) / Math.max(1, media(esq, y, DX)), gD = media(dir, y, 0) / Math.max(1, media(dir, y, DX));
    for (let x = X0; x <= X1; x++) {
      const t = (x - X0) / (X1 - X0), g = gE * (1 - t) + gD * t, s = (y * b.width + x - DX) * 4, d = (y * b.width + x) * 4;
      const alvo = [0, 1, 2].map(c => Math.min(255, src.data[s + c] * g));
      let melhor = cores[0], bd = 1e9;
      for (const c of cores) { const e = (c[0] - alvo[0]) ** 2 + (c[1] - alvo[1]) ** 2 + (c[2] - alvo[2]) ** 2; if (e < bd) { bd = e; melhor = c; } }
      b.data[d] = melhor[0]; b.data[d + 1] = melhor[1]; b.data[d + 2] = melhor[2]; b.data[d + 3] = 255;
    }
  }
  const lamp = alphaDuro(cand('PP-07', 'lamp_a')), lb = p.bounds(lamp), lc = p.crop(lamp, lb.x0, lb.y0, lb.w, lb.h);
  colarBase(b, lc, CX, BASE);
  folha('Assets/Art/Ending/ending_bedroom_olive.png', [b], { align: ALIGN.center, canvasInteiro: true });
};

// ================================================================ TROY VISUAL PACK (Docs/Art/TROY_VISUAL_PACK.md)
const MASTER_TROIA = () => p.read(path.join(PX, 'TR-01/_candidatos/master_a.png'));
/** Cores (RGB) de uma imagem, opcionalmente só as que passam no filtro. */
function coresDe(img, filtro = () => true) {
  const s = new Map();
  for (let i = 0; i < img.width * img.height; i++) { const d = i * 4; if (img.data[d + 3] < 128) continue;
    const k = img.data.readUInt32BE(d) >>> 8, c = [k >> 16, (k >> 8) & 255, k & 255]; if (filtro(c)) s.set(k, c); }
  return [...s.values()];
}
/** Cada pixel opaco vai para a cor mais próxima (RGB) da paleta dada. */
function quantizar(img, pal) {
  const o = p.blank(img.width, img.height); img.data.copy(o.data); const memo = new Map();
  for (let i = 0; i < o.width * o.height; i++) { const d = i * 4; if (o.data[d + 3] < 128) { o.data[d + 3] = 0; continue; }
    const k = o.data.readUInt32BE(d) >>> 8; let c = memo.get(k);
    if (!c) { let bd = 1e9; for (const q of pal) { const e = (q[0] - (k >> 16)) ** 2 + (q[1] - ((k >> 8) & 255)) ** 2 + (q[2] - (k & 255)) ** 2; if (e < bd) { bd = e; c = q; } } memo.set(k, c); }
    o.data[d] = c[0]; o.data[d + 1] = c[1]; o.data[d + 2] = c[2]; o.data[d + 3] = 255; }
  return o;
}
/** Mapeia as cores por POSIÇÃO de luminância: a i-ésima mais escura da origem vira a correspondente da rampa alvo. */
function porRampa(img, alvo) {
  const lum = (c) => 0.2126 * c[0] + 0.7152 * c[1] + 0.0722 * c[2];
  const orig = coresDe(img).sort((a, b) => lum(a) - lum(b)), dest = [...alvo].sort((a, b) => lum(a) - lum(b));
  const mapa = new Map(orig.map((c, i) => [c.join(), dest[Math.min(dest.length - 1, Math.floor(i * dest.length / orig.length))]]));
  const o = p.blank(img.width, img.height); img.data.copy(o.data);
  for (let i = 0; i < o.width * o.height; i++) { const d = i * 4; if (o.data[d + 3] < 128) continue;
    const c = mapa.get([o.data[d], o.data[d + 1], o.data[d + 2]].join()); o.data[d] = c[0]; o.data[d + 1] = c[1]; o.data[d + 2] = c[2]; }
  return o;
}
/** Junta a imagem com o próprio espelho: ladrilho horizontal sem emenda (céu, montanhas). */
function ladrilhoEspelhado(img) {
  const o = p.blank(img.width * 2, img.height), m = espelhar(img);
  for (let y = 0; y < img.height; y++) { img.data.copy(o.data, y * o.width * 4, y * img.width * 4, (y + 1) * img.width * 4);
    m.data.copy(o.data, (y * o.width + img.width) * 4, y * img.width * 4, (y + 1) * img.width * 4); }
  return o;
}

// ---------------------------------------------------------------- TROY-01 / TROY-02 — fundo, céu, montanhas, cidade
GRUPOS.troia02 = function () {
  const R = 'Assets/Art/Environments/Troy/Background/', M = MASTER_TROIA();
  // Master aprovado (TROY-01): a referência de luz, paleta e escala de todo o pack; também serve de fundo de tela cheia
  // (cutscene, tela de carregamento) — 672×384 ≈ 1× nativo.
  folha(R + 'troy_bg_master.png', [M], { align: ALIGN.center, canvasInteiro: true });
  // Céu: a faixa esquerda do master (x 0–189, y 0–79) é o único trecho sem fumaça, cidade nem montanha — nuvens em
  // faixas horizontais, cores exatas do master. Espelhada vira ladrilho sem emenda; abaixo da linha 79 (atrás das
  // montanhas) a cor dominante da última linha, até 240 px.
  const CEU_W = 190, CEU_H = 80, ALTO = 240, ceu = p.blank(CEU_W, ALTO);
  for (let y = 0; y < CEU_H; y++) M.data.copy(ceu.data, y * CEU_W * 4, y * M.width * 4, (y * M.width + CEU_W) * 4);
  // abaixo: a cor DOMINANTE da última linha (repetir a linha pixel a pixel esticava qualquer detalhe em coluna)
  const ult = new Map(); for (let x = 0; x < CEU_W; x++) { const k = M.data.readUInt32BE(((CEU_H - 1) * M.width + x) * 4); ult.set(k, (ult.get(k) || 0) + 1); }
  const dom = [...ult].sort((a, b) => b[1] - a[1])[0][0];
  for (let y = CEU_H; y < ALTO; y++) for (let x = 0; x < CEU_W; x++) ceu.data.writeUInt32BE(dom >>> 0, (y * CEU_W + x) * 4);
  folha(R + 'troy_bg_sky_war.png', [ladrilhoEspelhado(ceu)], { align: ALIGN.bottom, canvasInteiro: true });
  // Montanhas: mountains_b (pixen, xadrez pintado removido) — o resto de xadrez na encosta esquerda vira a cor da
  // serra de trás; depois as cores vão, por luminância, para os azuis-ardósia das montanhas do master. Ladrilho espelhado.
  const mt = alphaDuro(p.read(path.join(PX, 'TR-02/_prep/mountains_b.png')));
  const fundoSerra = (() => { const n = new Map(); for (let y = 0; y < 60; y++) for (let x = 60; x < 200; x++) { const d = (y * mt.width + x) * 4;
    if (mt.data[d + 3]) { const k = mt.data.readUInt32BE(d) >>> 8; n.set(k, (n.get(k) || 0) + 1); } } return [...n].sort((a, b) => b[1] - a[1])[0][0]; })();
  const lumK = (k) => 0.2126 * (k >> 16) + 0.7152 * ((k >> 8) & 255) + 0.0722 * (k & 255);
  for (let y = 0; y < mt.height; y++) for (let x = 0; x < 45; x++) { const d = (y * mt.width + x) * 4; if (!mt.data[d + 3]) continue;
    const k = mt.data.readUInt32BE(d) >>> 8; if (k !== fundoSerra && lumK(k) > lumK(fundoSerra) - 12) mt.data.writeUInt32BE(((fundoSerra << 8) | 255) >>> 0, d); }
  // Faixas de luminância da mountains_b (serra da frente ~88, faces iluminadas ~104–112, serra de trás ~184, neve 256)
  // → os cinco azuis das montanhas limpas do master (x 0–109, y 96–139). Faixa a faixa, e não cor a cor: o pontilhado
  // sutil do pixen viraria ruído se cada cinza ganhasse um azul diferente.
  const AZUL = [[0x44, 0x60, 0x72], [0x4a, 0x67, 0x78], [0x60, 0x7f, 0x8f], [0x85, 0x9c, 0xa4], [0xb3, 0xcd, 0xd2]];
  const faixa = (L) => L < 93 ? 0 : L < 100 ? 1 : L < 140 ? 2 : L < 215 ? 3 : 4;
  for (let i = 0; i < mt.width * mt.height; i++) { const d = i * 4; if (!mt.data[d + 3]) continue;
    const c = AZUL[faixa(0.2126 * mt.data[d] + 0.7152 * mt.data[d + 1] + 0.0722 * mt.data[d + 2])];
    mt.data[d] = c[0]; mt.data[d + 1] = c[1]; mt.data[d + 2] = c[2]; }
  const b = p.bounds(mt);
  folha(R + 'troy_bg_mountains.png', [ladrilhoEspelhado(p.crop(mt, 0, b.y0, mt.width, b.h))], { align: ALIGN.bottom, canvasInteiro: true });
  // Cidade distante: city_c (pixen, fundo chapado recortado) — a muralha tem a mesma altura da do master (~75 px), os
  // templos à esquerda e o palácio à direita repetem a composição dele; quantizada para as 67 cores do master.
  folha(R + 'troy_bg_city_distant.png', [quantizar(alphaDuro(p.read(path.join(PX, 'TR-02/_prep/city_c.png'))), coresDe(M))], { align: ALIGN.bottom });
};

const pedido = process.argv[2];
for (const [nome, fn] of Object.entries(GRUPOS)) {
  if (pedido && pedido !== nome) continue;
  console.log(nome);
  fn();
}
module.exports = { folha, alphaDuro, espelhar, dissolver, cand, prep };
