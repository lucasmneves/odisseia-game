// Auditoria global de arte, fora do Unity — só LÊ arquivos, não altera nada.
//
//   node Tools/art-audit.js            → Docs/Art/_audit/{inventario.tsv, placeholders.tsv, resumo.txt}
//
// Cruza três fontes:
//   1. todo PNG de Assets/ (tamanho, cores, densidade, PPU e modo do .meta);
//   2. quem usa cada PNG — pelo GUID em cenas, prefabs e assets, e pelo caminho em Resources.Load no código;
//   3. todo SpriteRenderer/Image de cena ou prefab que desenha o PlaceholderSquare (ou sprite embutido do
//      Unity), com o nome do objeto, se está ativo e se o renderer está ligado.
//
// Limites: lê YAML, então NÃO resolve override de prefab por instância (um renderer desligado pelo vestidor
// numa instância aparece pelo valor do prefab) nem objetos criados em runtime. O PlaceholderProbe no Unity
// continua sendo a fonte de verdade sobre placeholder visível; esta auditoria é o mapa de onde olhar.
const fs = require('fs'), path = require('path');
const p = require('./png.js');

const ROOT = path.join(__dirname, '..');
const OUT = path.join(ROOT, 'Docs/Art/_audit');
const PLACEHOLDER_GUID = '1b72e2c6984fc0442aea0da3df0fafcb';
const UNITY_BUILTIN = '0000000000000000f000000000000000';

function andar(dir, filtro, out = []) {
  for (const e of fs.readdirSync(dir, { withFileTypes: true })) {
    if (e.name.startsWith('.') || e.name.endsWith('~')) continue;
    const f = path.join(dir, e.name);
    if (e.isDirectory()) andar(f, filtro, out);
    else if (filtro(e.name)) out.push(f);
  }
  return out;
}
const rel = (f) => path.relative(ROOT, f).split(path.sep).join('/');

// ---------------------------------------------------------------- densidade (mesma regra do scale-probe)
function fatorDeAmpliacao(img) {
  const fator = (eixo) => {
    const n = eixo === 'y' ? img.height : img.width, m = eixo === 'y' ? img.width : img.height;
    const linha = (i) => {
      let h = 0;
      for (let j = 0; j < m; j++) {
        const [x, y] = eixo === 'y' ? [j, i] : [i, j];
        h = (h * 31 + img.data.readUInt32BE((y * img.width + x) * 4)) >>> 0;
      }
      return h;
    };
    const runs = []; let run = 1, prev = linha(0);
    for (let i = 1; i < n; i++) { const c = linha(i); if (c === prev) run++; else { runs.push(run); run = 1; } prev = c; }
    runs.push(run);
    const hist = {}; for (const r of runs) hist[r] = (hist[r] || 0) + 1;
    let best = 1;
    for (const [k, c] of Object.entries(hist)) if (+k > 1 && (+k * c) / n > 0.7) best = +k;
    return best;
  };
  return Math.max(fator('x'), fator('y'));
}

function analisarPng(f) {
  const img = p.read(f);
  const cores = new Set(); let opacos = 0, semi = 0;
  for (let i = 0; i < img.width * img.height; i++) {
    const a = img.data[i * 4 + 3];
    if (!a) continue;
    opacos++; if (a < 255) semi++;
    if (cores.size < 5000) cores.add(img.data.readUInt32BE(i * 4) >>> 8);
  }
  const grande = img.width * img.height > 4e6;
  return { w: img.width, h: img.height, cores: cores.size >= 5000 ? '5000+' : cores.size,
    semi: opacos ? +(semi / opacos * 100).toFixed(1) : 0, fator: grande ? '?' : fatorDeAmpliacao(img) };
}

function lerMeta(f) {
  const m = f + '.meta';
  if (!fs.existsSync(m)) return {};
  const t = fs.readFileSync(m, 'utf8');
  const v = (re) => (t.match(re) || [])[1];
  return { guid: v(/^guid: (\w+)/m), ppu: v(/spritePixelsToUnits: ([\d.]+)/), modo: v(/spriteMode: (\d)/),
    filtro: v(/filterMode: (\d)/), sprites: (t.match(/^\s+- serializedVersion: 2\n\s+name: /gm) || []).length };
}

// ---------------------------------------------------------------- uso
const yamls = andar(path.join(ROOT, 'Assets'), n => /\.(unity|prefab|asset|controller|anim)$/.test(n));
const codigo = andar(path.join(ROOT, 'Assets/Scripts'), n => n.endsWith('.cs'));
const textoYaml = new Map(yamls.map(f => [f, fs.readFileSync(f, 'utf8')]));
const textoCodigo = codigo.map(f => [f, fs.readFileSync(f, 'utf8')]);

function usos(guid, arquivoPng) {
  const onde = [];
  if (guid) for (const [f, t] of textoYaml) if (t.includes(guid)) onde.push(path.basename(f).replace(/\.(unity|prefab|asset)$/, ''));
  // Resources.Load por caminho: procurar o nome sem extensão no código.
  const nome = path.basename(arquivoPng, '.png');
  if (arquivoPng.includes('/Resources/')) {
    const caminho = arquivoPng.split('/Resources/')[1].replace(/\.png$/, '');
    for (const [f, t] of textoCodigo) if (t.includes(caminho) || t.includes(`"${nome}"`)) onde.push('cs:' + path.basename(f, '.cs'));
  } else {
    for (const [f, t] of textoCodigo) if (t.includes(nome + '.png')) onde.push('cs:' + path.basename(f, '.cs'));
  }
  return [...new Set(onde)];
}

// ---------------------------------------------------------------- placeholders em cena/prefab
function placeholdersEm(f, t) {
  const docs = t.split(/\n--- /);
  const porId = new Map();
  for (const d of docs) { const m = d.match(/^!u!(\d+) &(-?\d+)/); if (m) porId.set(m[2], { k: m[1], d }); }
  const nomeDe = (go) => { const x = porId.get(go); const m = x && x.d.match(/m_Name: (.*)/); return m ? m[1].trim() : '?'; };
  const ativo = (go) => { const x = porId.get(go); return x ? !/m_IsActive: 0/.test(x.d) : true; };
  const paiDe = new Map();
  for (const [id, { k, d }] of porId) {
    if (k !== '4' && k !== '224') continue;
    const go = (d.match(/m_GameObject: \{fileID: (-?\d+)/) || [])[1];
    const pai = (d.match(/m_Father: \{fileID: (-?\d+)/) || [])[1];
    paiDe.set(id, { go, pai });
  }
  const transformDoGo = new Map([...paiDe].map(([id, v]) => [v.go, id]));
  const caminho = (go) => {
    const partes = []; let tr = transformDoGo.get(go), n = 0;
    while (tr && tr !== '0' && n++ < 12) { const v = paiDe.get(tr); if (!v) break; partes.unshift(nomeDe(v.go)); tr = v.pai; }
    return partes.join('/') || nomeDe(go);
  };
  const achados = [];
  for (const [, { k, d }] of porId) {
    if (k !== '212' && k !== '114') continue;               // SpriteRenderer ou Image (MonoBehaviour)
    const sprite = (d.match(/m_Sprite: \{fileID: (\d+), guid: (\w+)/) || []);
    if (!sprite[2]) continue;
    const tipo = sprite[2] === PLACEHOLDER_GUID ? 'PlaceholderSquare'
      : sprite[2] === UNITY_BUILTIN ? `builtin:${sprite[1]}` : null;
    if (!tipo) continue;
    const go = (d.match(/m_GameObject: \{fileID: (-?\d+)/) || [])[1];
    const ligado = !/m_Enabled: 0/.test(d);
    achados.push({ arquivo: path.basename(f), objeto: caminho(go), tipo, comp: k === '212' ? 'SpriteRenderer' : 'Image',
      ativo: ativo(go), ligado, cor: ((d.match(/m_Color: \{r: ([\d.]+), g: ([\d.]+), b: ([\d.]+)/) || []).slice(1).join(',')) });
  }
  return achados;
}

// ---------------------------------------------------------------- execução
fs.mkdirSync(OUT, { recursive: true });
const pngs = andar(path.join(ROOT, 'Assets'), n => n.endsWith('.png'));
const linhas = [['arquivo', 'w', 'h', 'cores', 'semi%', 'fator', 'ppu', 'modo', 'sprites', 'filtro', 'usos'].join('\t')];
const inventario = [];
for (const f of pngs) {
  const r = rel(f);
  const a = analisarPng(f), m = lerMeta(f), u = usos(m.guid, r);
  inventario.push({ r, ...a, ...m, usos: u });
  linhas.push([r, a.w, a.h, a.cores, a.semi, a.fator, m.ppu, m.modo, m.sprites, m.filtro, u.join(',')].join('\t'));
}
fs.writeFileSync(path.join(OUT, 'inventario.tsv'), linhas.join('\n') + '\n');

const ph = [];
for (const [f, t] of textoYaml) if (/\.(unity|prefab)$/.test(f)) ph.push(...placeholdersEm(f, t));
fs.writeFileSync(path.join(OUT, 'placeholders.tsv'),
  ['arquivo\tobjeto\ttipo\tcomp\tativo\tligado\tcor', ...ph.map(x =>
    [x.arquivo, x.objeto, x.tipo, x.comp, x.ativo, x.ligado, x.cor].join('\t'))].join('\n') + '\n');

const cs = textoCodigo.filter(([, t]) => /PlaceholderSprite/.test(t) && !/class GameAssets/.test(t))
  .map(([f, t]) => `${path.basename(f)} (${(t.match(/VfxBurst\.Spawn/g) || []).length} VfxBurst)`);

// resumo
const porPasta = {};
for (const i of inventario) {
  const pasta = i.r.split('/').slice(0, 4).join('/');
  const k = (porPasta[pasta] ||= { n: 0, semUso: 0, ampliado: 0, ppuFora: 0, pintado: 0 });
  k.n++;
  if (!i.usos.length) k.semUso++;
  if (i.fator !== 1 && i.fator !== '?') k.ampliado++;
  if (i.ppu && Math.abs(+i.ppu - 42.857143) > 0.01) k.ppuFora++;
  if (i.cores === '5000+' || i.cores > 1500) k.pintado++;
}
const res = [];
res.push(`AUDITORIA DE ARTE — ${new Date().toISOString().slice(0, 10)}`);
res.push(`${inventario.length} PNGs, ${yamls.length} arquivos YAML, ${codigo.length} scripts\n`);
res.push('pasta | PNGs | sem uso | ampliado | PPU≠42,857 | >1500 cores (pintado)');
for (const [k, v] of Object.entries(porPasta).sort()) res.push(`${k} | ${v.n} | ${v.semUso} | ${v.ampliado} | ${v.ppuFora} | ${v.pintado}`);
res.push('\nPLACEHOLDERS EM CENA/PREFAB (valor do YAML; vestidores podem desligar por override)');
const porArq = {};
for (const x of ph) { const k = (porArq[x.arquivo] ||= { total: 0, visivel: 0 }); k.total++; if (x.ativo && x.ligado) k.visivel++; }
for (const [k, v] of Object.entries(porArq).sort()) res.push(`${k}: ${v.total} (${v.visivel} ativos e ligados no YAML)`);
res.push('\nCÓDIGO QUE DESENHA COM O PlaceholderSprite');
for (const c of cs) res.push('  ' + c);
fs.writeFileSync(path.join(OUT, 'resumo.txt'), res.join('\n') + '\n');
console.log(res.join('\n'));
