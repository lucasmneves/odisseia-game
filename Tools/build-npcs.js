// Gera os figurantes da fase como pixel art, no mesmo padrão do master do Odisseu.
//
//   node Tools/build-npcs.js [--dry]
//
// Por que não `create_image_pixflux`: testado, custa o mesmo 1 geração e não alcança o
// acabamento do master — perde a vista lateral e sai chapado, sem rosto legível.
// `create_character` em modo `standard` custa 1 geração, devolve um personagem com rig e
// quatro rotações, e a de leste é a que o projeto usa (o master é todo `east`, espelhado
// por código).
//
// NÃO quantiza na paleta do personagem, e isso foi medido: as túnicas de linho cru dos
// figurantes ficam a 122 e 154 da entrada mais próxima. A paleta do Odisseu não tem branco
// nem off-white — ele veste bronze, couro e manto vermelho —, então quantizar empurra o linho
// para a rampa de PELE e os aldeões saem inteiros de mostarda. Foi o que aconteceu na
// primeira passada.
//
// É a mesma regra da quantização de cenário: antes de quantizar, conferir se a paleta cobre
// os materiais do asset. Eles já saem como pixel art limpa, com contorno preto e 17 a 21
// cores, na mesma família; o que falta na paleta é uma rampa de linho.
const fs = require('fs'), path = require('path');
const px = require('./pixellab.js');
const p = require('./png.js');

const ROOT = path.join(__dirname, '..');
const SAIDA = path.join(ROOT, 'Docs/Environment_Ithaca/Figures');
const PALETA = path.join(ROOT, 'Docs/CharacterMaster_Odysseus/Palette/ODYSSEUS_PALETTE.png');
const UN = 42.857143;
const seco = process.argv.includes('--dry');

// Cinco arquétipos cobrem os dezenove figurantes da fase. Repetir arquétipo com variação de
// cor é o que os jogos fazem; gerar dezenove pessoas distintas custaria dezenove gerações
// para uma diferença que ninguém lê num figurante de fundo.
const ARQUETIPOS = [
  { nome: 'villager_fisherman', desc: 'Bronze Age Greek village fisherman, plain undyed short tunic, bare arms, short dark beard, weathered and lean' },
  { nome: 'villager_farmer', desc: 'Bronze Age Greek village farmer, coarse brown belted tunic, straw sun hat, sturdy build, holding nothing' },
  { nome: 'villager_sailor', desc: 'Bronze Age Greek sailor, short sleeveless tunic, bare feet, rope belt, wind-burnt face, wiry build' },
  { nome: 'villager_soldier', desc: 'Bronze Age Greek foot soldier at rest, bronze helmet, leather cuirass over a short tunic, greaves, standing at ease' },
  { nome: 'villager_elder', desc: 'old Bronze Age Greek villager, long grey beard, long plain robe, leaning slightly on a walking staff, stooped' },
];

const sleep = ms => new Promise(r => setTimeout(r, ms));

async function criar(a) {
  const r = await px.call('create_character', {
    description: a.desc,
    name: 'ITHACA_' + a.nome,
    body_type: 'humanoid',
    mode: 'standard',
    n_directions: 4,
    // 55, não 64: pedindo 64 os figurantes saíram com 70 px de conteúdo — 1,63 un contra os
    // 1,40 un do Odisseu, ou seja, uma cabeça mais altos que o herói. O conteúdo sai cerca de
    // 1,09x o tamanho pedido.
    size: 55,
    view: 'side',
    outline: 'single color black outline',
    detail: 'medium detail',
  });
  const id = (px.textOf(r).match(/[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/i) || [])[0];
  if (!id) throw new Error('sem character_id: ' + px.textOf(r).slice(0, 200));
  return id;
}

// A rotação vem por URL sem autenticação; get_character só devolve a primeira inline.
async function baixarLeste(id) {
  for (let i = 0; i < 60; i++) {
    await sleep(8000);
    const g = await px.call('get_character', { character_id: id });
    const txt = px.textOf(g);
    if (/status: failed|error/i.test(txt)) throw new Error(txt.slice(0, 200));
    const m = txt.match(/east:\s*(https:\/\/\S+)/);
    if (m && /status: completed/.test(txt)) {
      const res = await fetch(m[1]);
      if (!res.ok) throw new Error('download HTTP ' + res.status);
      return Buffer.from(await res.arrayBuffer());
    }
  }
  throw new Error('tempo esgotado');
}

// Paleta do personagem, para o vizinho mais próximo.
function carregarPaleta() {
  const im = p.read(PALETA);
  const s = new Set();
  for (let i = 0; i < im.width * im.height; i++) {
    const o = i * 4;
    if (im.data[o + 3] <= 8) continue;
    s.add((im.data[o] << 16) | (im.data[o + 1] << 8) | im.data[o + 2]);
  }
  return [...s].map(k => [(k >> 16) & 255, (k >> 8) & 255, k & 255]);
}

// Distância da cor RELEVANTE (>= 2% dos pixels) mais distante da paleta. Serve de aviso: se
// cair para perto de 60, vale quantizar; nos 120-154 medidos, não.
function piorCorDescoberta(img, cores) {
  const h = new Map();
  let tot = 0;
  for (let i = 0; i < img.width * img.height; i++) {
    const o = i * 4;
    if (img.data[o + 3] <= 8) continue;
    tot++;
    const k = (img.data[o] << 16) | (img.data[o + 1] << 8) | img.data[o + 2];
    h.set(k, (h.get(k) || 0) + 1);
  }
  let pior = 0;
  for (const [k, n] of h) {
    if (n / tot < 0.02) continue;
    const c = [(k >> 16) & 255, (k >> 8) & 255, k & 255];
    let melhor = Infinity;
    for (const q of cores) {
      const d = Math.hypot(c[0] - q[0], c[1] - q[1], c[2] - q[2]);
      if (d < melhor) melhor = d;
    }
    if (melhor > pior) pior = melhor;
  }
  return pior;
}

function quantizar(img, cores) {
  const out = p.blank(img.width, img.height);
  img.data.copy(out.data);
  const cache = new Map();
  for (let i = 0; i < img.width * img.height; i++) {
    const o = i * 4;
    if (img.data[o + 3] <= 8) continue;
    const k = (img.data[o] << 16) | (img.data[o + 1] << 8) | img.data[o + 2];
    let melhor = cache.get(k);
    if (!melhor) {
      const c = [(k >> 16) & 255, (k >> 8) & 255, k & 255];
      let d = Infinity;
      for (const q of cores) {
        const v = (c[0] - q[0]) ** 2 + (c[1] - q[1]) ** 2 + (c[2] - q[2]) ** 2;
        if (v < d) { d = v; melhor = q; }
      }
      cache.set(k, melhor);
    }
    out.data[o] = melhor[0]; out.data[o + 1] = melhor[1]; out.data[o + 2] = melhor[2];
  }
  return out;
}

async function main() {
  const cores = carregarPaleta();
  fs.mkdirSync(SAIDA, { recursive: true });
  console.log(`paleta do personagem: ${cores.length} cores`);

  const ids = [];
  for (const a of ARQUETIPOS) {
    if (seco) { console.log(`(simulação) criaria ${a.nome}`); continue; }
    ids.push({ a, id: await criar(a) });
    console.log(`${a.nome}: ${ids[ids.length - 1].id}`);
  }
  if (seco) return;

  for (const { a, id } of ids) {
    const bruto = await baixarLeste(id);
    const tmp = path.join(SAIDA, '_' + a.nome + '_bruto.png');
    fs.writeFileSync(tmp, bruto);
    const img = p.read(tmp);
    const b = p.bounds(img);
    const final = p.crop(img, b.x0, b.y0, b.w, b.h);
    const cobertura = piorCorDescoberta(final, cores);
    const destino = path.join(SAIDA, 'ithaca_' + a.nome + '.png');
    p.write(destino, final);

    const usadas = new Set();
    for (let i = 0; i < final.width * final.height; i++) {
      const o = i * 4;
      if (final.data[o + 3] > 8) usadas.add((final.data[o] << 16) | (final.data[o + 1] << 8) | final.data[o + 2]);
    }
    console.log(`  ${a.nome.padEnd(20)} ${final.width}x${final.height}px = ${(final.height / UN).toFixed(2)} un, ` +
      `${usadas.size} cores, pior cor a ${cobertura.toFixed(0)} da paleta do personagem`);
  }
}

main().catch(e => { console.error('ERRO:', e.message); process.exit(1); });
