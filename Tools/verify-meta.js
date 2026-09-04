// Espelho, em Node, do que IthacaEnvironmentProbe confere dentro do Unity.
//
// Não substitui a probe — só o Unity sabe o que de fato importou. Serve para pegar erro de
// geração de .meta antes do Editor ver, o que importa porque o Editor precisa estar fechado
// para rodar batchmode e nem sempre está.
//
//   node Tools/verify-meta.js
const fs = require('fs'), path = require('path');
const p = require('./png.js');

const ROOT = path.join(__dirname, '..');
const R = path.join(ROOT, 'Assets/Art/Environments/Ithaca');
const PPU = '42.857143';
const TILE = 32;

// Precisam de wrap Repeat: as camadas de parallax e o muro, que repetem lateralmente.
const REPETEM = new Set([
  'Background/ithaca_bg_clouds', 'Background/ithaca_bg_island_far',
  'Background/ithaca_bg_mountains_far', 'Background/ithaca_bg_ocean',
  'Background/ithaca_bg_sky', 'Architecture/ithaca_wall_low_01',
]);

let falhas = 0, metas = 0, tiles = 0;
const guids = new Map();

// Lê "  chave: valor" em qualquer indentação, sem regex — evita a classe inteira de erro de
// escape que já custou uma rodada aqui.
function campo(texto, chave) {
  for (const linha of texto.split('\n')) {
    const corte = linha.indexOf(': ');
    if (corte < 0) continue;
    if (linha.slice(0, corte).trim() === chave) return linha.slice(corte + 2).trim();
  }
  return null;
}

function reprovar(rel, msg) {
  console.log('  FALHA ' + rel + ' - ' + msg);
  falhas++;
}

function conferirTiles(rel, meta, img) {
  const cols = img.width / TILE;
  const linhas = meta.split('\n');
  let vistos = 0;
  for (let i = 0; i < linhas.length; i++) {
    const nome = campo(linhas[i], 'name');
    if (!nome || !nome.startsWith('ithaca_tiles_')) continue;
    // O bloco rect vem logo abaixo: serializedVersion, x, y, width, height.
    const x = +campo(linhas[i + 3], 'x'), y = +campo(linhas[i + 4], 'y');
    const w = +campo(linhas[i + 5], 'width'), h = +campo(linhas[i + 6], 'height');
    const idx = +nome.slice(-2);
    const ex = (idx % cols) * TILE, ey = img.height - (Math.floor(idx / cols) + 1) * TILE;
    if (w !== TILE || h !== TILE) reprovar(rel, nome + ' mede ' + w + 'x' + h);
    else if (x !== ex || y !== ey) reprovar(rel, nome + ' rect (' + x + ',' + y + ') esperado (' + ex + ',' + ey + ')');
    else vistos++;
  }
  if (vistos !== 16) reprovar(rel, vistos + ' tiles válidos, esperado 16');
  else tiles += vistos;
}

for (const grupo of fs.readdirSync(R)) {
  const dir = path.join(R, grupo);
  if (!fs.statSync(dir).isDirectory()) continue;
  for (const f of fs.readdirSync(dir).sort()) {
    if (!f.endsWith('.png.meta')) continue;
    const rel = grupo + '/' + f.replace('.png.meta', '');
    const meta = fs.readFileSync(path.join(dir, f), 'utf8');
    const terreno = grupo === 'Terrain';
    metas++;

    const g = campo(meta, 'guid');
    if (guids.has(g)) reprovar(rel, 'GUID repetido com ' + guids.get(g));
    else guids.set(g, rel);

    const esperado = {
      spritePixelsToUnits: PPU,
      filterMode: '0',                                  // Point
      textureType: '8',                                 // Sprite
      alphaIsTransparency: '1',
      enableMipMap: '0',
      alignment: terreno ? '0' : '7',                   // Center para tile, BottomCenter no resto
      wrapU: REPETEM.has(rel) ? '0' : '1',              // 0 = Repeat
      spriteMode: terreno ? '2' : '1',                  // Multiple para os tilesets
      // FullRect onde o drawMode Tiled é usado: parallax, muro e os tilesets de terreno.
      spriteMeshType: (REPETEM.has(rel) || terreno) ? '0' : '1',
    };
    for (const [k, v] of Object.entries(esperado)) {
      const lido = campo(meta, k);
      if (lido !== v) reprovar(rel, k + ' = ' + lido + ', esperado ' + v);
    }
    if (meta.split('\n').some(l => l.trim().startsWith('textureCompression:') && l.trim() !== 'textureCompression: 0')) {
      reprovar(rel, 'compressão ligada em alguma plataforma');
    }

    if (terreno) conferirTiles(rel, meta, p.read(path.join(dir, f.replace('.meta', ''))));
  }
}

console.log(metas + ' metas, ' + tiles + ' tiles, ' + guids.size + ' GUIDs únicos, ' + falhas + ' falhas');
process.exit(falhas ? 1 : 0);
