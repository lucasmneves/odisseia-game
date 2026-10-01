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

const pedido = process.argv[2];
for (const [nome, fn] of Object.entries(GRUPOS)) {
  if (pedido && pedido !== nome) continue;
  console.log(nome);
  fn();
}
module.exports = { folha, alphaDuro, espelhar, dissolver, cand, prep };
