// QA do asset pack do PixelLab Asset Completion — só lê, não altera nada.
//
//   node Tools/pack-audit.js     → Docs/Art/_audit/pack_qa.tsv + resumo no console
//
// Para cada PNG novo do pack (lista abaixo, montada pelo build-pack.js):
//   .meta   — PPU 42,857143 · filtro Point · sem compressão · alphaIsTransparency · pivô · fatias de tamanho igual
//   imagem  — dimensões · % de pixels semitransparentes (deve ser 0) · cantos opacos (fundo que sobrou) · xadrez pintado
//             · pixels soltos (componentes de 1–2 px) · densidade (fator de ampliação, mesma regra do scale-probe)
//             · paleta: distância média de cada cor à paleta de Ítaca/Pretendentes
//   nome    — snake_case minúsculo
// Imagens que SÃO o quadro inteiro (mapa, menu) dispensam o teste de canto opaco.
const fs = require('fs'), path = require('path');
const p = require('./png.js');
const { loadPalette } = require('./ramp-map.js');
const { temXadrezPintado } = require('./checker-cut.js');

const ROOT = path.join(__dirname, '..');
const PAL = Object.values(loadPalette(path.join(ROOT, 'Docs/Environment_Pretendentes/Palette/PRETENDENTES_PALETTE.gpl'))).flat();
const QUADRO_INTEIRO = /map_aegean|menu_bg_/;

function listar() {
  const raizes = ['Assets/Art/Effects', 'Assets/Art/Items', 'Assets/Art/Ending', 'Assets/Art/Map', 'Assets/Art/UI'];
  const avulsos = [
    'Assets/Art/Environments/Troy/Gameplay', 'Assets/Art/Environments/Cytera/Foreground', 'Assets/Art/Environments/Eolo/Foreground',
    'Assets/Art/Environments/ItacaReturn/Foreground', 'Assets/Art/Environments/Pretendentes/Foreground',
  ];
  const arquivos = [
    'Assets/Art/Environments/Eolo/Palace/eolo_entablature_native.png',
    'Assets/Art/Environments/Final/Props/final_hearth_fire_anim.png',
    'Assets/Art/Environments/ItacaReturn/Props/itaca_ret_brazier_anim.png',
    'Assets/Art/Environments/Pretendentes/Courtyard/pret_cooking_fire_anim.png',
    'Assets/Art/Environments/Pretendentes/VFX/pret_torch_stand_anim.png',
    'Assets/Art/Environments/Troy/Camp/troy_campfire_01_anim.png',
  ];
  const andar = (d) => fs.existsSync(path.join(ROOT, d)) ? fs.readdirSync(path.join(ROOT, d), { withFileTypes: true }).flatMap(e =>
    e.isDirectory() ? andar(d + '/' + e.name) : e.name.endsWith('.png') ? [d + '/' + e.name] : []) : [];
  return [...raizes.flatMap(andar), ...avulsos.flatMap(andar), ...arquivos];
}

function meta(rel) {
  const f = path.join(ROOT, rel + '.meta');
  if (!fs.existsSync(f)) return { falta: true };
  const t = fs.readFileSync(f, 'utf8');
  const fatias = [...t.matchAll(/^\s+width: (\d+)\s*\n\s+height: (\d+)/gm)].map(m => m[1] + 'x' + m[2]);
  return {
    ppu: +(t.match(/spritePixelsToUnits: ([\d.]+)/) || [])[1], point: /filterMode: 0/.test(t),
    semCompressao: !/textureCompression: [1-9]/.test(t), alpha: /alphaIsTransparency: 1/.test(t),
    pivo: /alignment: 7/.test(t) ? 'base' : /alignment: 0/.test(t) ? 'centro' : '?',
    fatias: fatias.length, fatiasIguais: new Set(fatias).size <= 1, celula: fatias[0] || null,
  };
}

function fator(img) {
  const um = (eixo) => {
    const n = eixo === 'y' ? img.height : img.width, m = eixo === 'y' ? img.width : img.height;
    const linha = (i) => { let h = 0; for (let j = 0; j < m; j++) { const [x, y] = eixo === 'y' ? [j, i] : [i, j];
      h = (h * 31 + img.data.readUInt32BE((y * img.width + x) * 4)) >>> 0; } return h; };
    const runs = []; let r = 1, prev = linha(0);
    for (let i = 1; i < n; i++) { const c = linha(i); if (c === prev) r++; else { runs.push(r); r = 1; } prev = c; } runs.push(r);
    const hist = {}; for (const x of runs) hist[x] = (hist[x] || 0) + 1;
    let k = 1; for (const [a, c] of Object.entries(hist)) if (+a > 1 && (+a * c) / n > 0.7) k = +a; return k;
  };
  return Math.max(um('x'), um('y'));
}

function soltos(img) {
  const { width: W, height: H } = img, vis = new Uint8Array(W * H); let n = 0;
  for (let s = 0; s < W * H; s++) {
    if (vis[s] || !img.data[s * 4 + 3]) continue;
    let tam = 0; const pilha = [s]; vis[s] = 1;
    while (pilha.length) { const i = pilha.pop(); tam++; const x = i % W, y = (i - x) / W;
      for (let dy = -1; dy <= 1; dy++) for (let dx = -1; dx <= 1; dx++) { const nx = x + dx, ny = y + dy;
        if (nx < 0 || ny < 0 || nx >= W || ny >= H) continue; const j = ny * W + nx;
        if (!vis[j] && img.data[j * 4 + 3]) { vis[j] = 1; pilha.push(j); } } }
    if (tam <= 2) n++;
  }
  return n;
}

function paleta(img) {
  const cores = new Map();
  for (let i = 0; i < img.width * img.height; i++) { const o = i * 4; if (!img.data[o + 3]) continue;
    const k = img.data.readUInt32BE(o) >>> 8; cores.set(k, (cores.get(k) || 0) + 1); }
  let soma = 0, peso = 0, exatas = 0;
  for (const [k, n] of cores) {
    const r = k >> 16, g = (k >> 8) & 255, b = k & 255;
    let d = 1e9; for (const c of PAL) d = Math.min(d, Math.hypot(r - c[0], g - c[1], b - c[2]));
    soma += d * n; peso += n; if (d === 0) exatas += n;
  }
  return { cores: cores.size, dist: peso ? soma / peso : 0, exata: peso ? exatas / peso : 0 };
}

const linhas = [['arquivo', 'px', 'célula', 'quadros', 'ppu', 'point', 'compr', 'pivô', 'semi%', 'cantos', 'xadrez', 'soltos', 'fator', 'cores', 'dist_paleta', 'nome', 'veredito'].join('\t')];
const problemas = [];
for (const rel of listar()) {
  const img = p.read(path.join(ROOT, rel)), m = meta(rel);
  let opacos = 0, semi = 0;
  for (let i = 3; i < img.data.length; i += 4) if (img.data[i]) { opacos++; if (img.data[i] < 255) semi++; }
  const canto = (x, y) => img.data[(y * img.width + x) * 4 + 3] > 0;
  const cantosOpacos = [canto(0, 0), canto(img.width - 1, 0), canto(0, img.height - 1), canto(img.width - 1, img.height - 1)].filter(Boolean).length;
  const inteiro = QUADRO_INTEIRO.test(rel);
  const xadrez = !inteiro && temXadrezPintado(img);
  const s = soltos(img), f = fator(img), pal = paleta(img);
  const nome = /^[a-z0-9_]+\.png$/.test(path.basename(rel));
  const falhas = [];
  if (m.falta) falhas.push('sem .meta');
  if (!m.falta && Math.abs(m.ppu - 42.857143) > 1e-4) falhas.push('PPU');
  if (!m.falta && (!m.point || !m.semCompressao || !m.alpha)) falhas.push('import');
  if (!m.falta && !m.fatiasIguais) falhas.push('fatias desiguais');
  if (semi) falhas.push(`${semi} px semi-alpha`);
  if (!inteiro && cantosOpacos >= 3) falhas.push('fundo nos cantos');
  if (xadrez) falhas.push('xadrez pintado');
  if (f !== 1 && !/gradient/.test(rel)) falhas.push(`ampliado ${f}x`);
  if (!nome) falhas.push('nome');
  const aviso = [];
  if (s > 6) aviso.push(`${s} px soltos`);
  if (pal.dist > 40) aviso.push(`longe da paleta (${pal.dist.toFixed(0)})`);
  const veredito = falhas.length ? 'FALHA: ' + falhas.join(', ') : aviso.length ? 'ok, ver: ' + aviso.join(', ') : 'ok';
  if (falhas.length) problemas.push(rel + ' — ' + falhas.join(', '));
  linhas.push([rel, `${img.width}x${img.height}`, m.celula || '-', m.fatias || 1, m.ppu, m.point, m.semCompressao, m.pivo,
    (opacos ? semi / opacos * 100 : 0).toFixed(1), cantosOpacos, xadrez, s, f, pal.cores, pal.dist.toFixed(1), nome, veredito].join('\t'));
}
fs.mkdirSync(path.join(ROOT, 'Docs/Art/_audit'), { recursive: true });
fs.writeFileSync(path.join(ROOT, 'Docs/Art/_audit/pack_qa.tsv'), linhas.join('\n') + '\n');
for (const l of linhas.slice(1)) { const c = l.split('\t'); console.log(`${c[16].padEnd(44).slice(0, 44)} ${c[0].replace('Assets/Art/', '')}  ${c[1]} cel ${c[2]} q${c[3]} pivô ${c[7]} cores ${c[13]} dist ${c[14]}`); }
console.log(`\n${linhas.length - 1} arquivos, ${problemas.length} com falha`);
for (const x of problemas) console.log('  ' + x);
