// Extrai de uma folha Wang as duas peças que o vestidor usa como FAIXA.
//
//   node Tools/build-ground-tiles.js <pasta-do-ambiente> <nome-da-folha>
//   node Tools/build-ground-tiles.js Docs/Environment_CilaCaribdis cila_tiles_rock
//
// Por que não usar a folha inteira como faixa que repete: a folha Wang é um KIT de 16 peças
// com transparência entre elas. Posta num `SpriteDrawMode.Tiled`, ela repete o kit — buracos e
// tudo. O que repete bem é UMA peça.
//
// As duas peças necessárias, lidas pelos cantos no JSON:
//   `wang_0`  — os quatro cantos `lower` (rocha sólida): o corpo do terreno.
//   `wang_12` — cantos de cima `upper` (ar) e de baixo `lower`: a superfície.
//
// A POSIÇÃO NA FOLHA É O ÍNDICE DO ARRAY, em ordem row-major. O campo `original_position` do
// JSON aponta para outra grade e leva ao tile errado — é armadilha já registrada no projeto.
//
// E a medida que o vestidor precisa: num tile de topo os cantos de cima são `upper`, então a
// linha pisável NÃO é o topo do tile. Este script MEDE onde ela está e grava o número, para o
// vestidor alinhar a arte ao colisor em vez de chutar meio tile.
const path = require('path'), fs = require('fs');
const p = require('./png.js');

const ROOT = path.join(__dirname, '..');
const [pastaArg, folhaArg] = process.argv.slice(2);
if (!pastaArg || !folhaArg) {
  console.error('uso: node Tools/build-ground-tiles.js <pasta-do-ambiente> <nome-da-folha>');
  process.exit(2);
}
const ENV = path.join(ROOT, pastaArg);
// O nome de saida PRESERVA o material. A primeira versao cortava tudo depois de "_tiles_", e
// entao 'gado_tiles_grass' e 'gado_tiles_stone' geravam os dois o mesmo 'gado_ground_top.png':
// a segunda folha sobrescrevia a primeira em silencio, e o pasto ficava com o chao do templo.
const BASE = folhaArg.replace('_tiles_', '_');
const folha = p.read(path.join(ENV, `Gameplay/${folhaArg}.png`));
const dados = JSON.parse(fs.readFileSync(path.join(ENV, `Gameplay/${folhaArg}.json`), 'utf8'));
const tiles = dados.tileset_data.tiles;
const COLS = dados.layout.grid_size.width;
const LADO = folha.width / COLS;

function porCantos(nw, ne, sw, se) {
  const i = tiles.findIndex(t => t.corners.NW === nw && t.corners.NE === ne &&
    t.corners.SW === sw && t.corners.SE === se);
  if (i < 0) { throw new Error(`nenhum tile com cantos ${nw}/${ne}/${sw}/${se}`); }
  return { i, col: i % COLS, row: Math.floor(i / COLS), nome: tiles[i].name };
}

/** Primeira linha com pixel opaco — é onde a superfície do tile começa de verdade. */
function primeiraLinhaOpaca(img) {
  for (let y = 0; y < img.height; y++) {
    for (let x = 0; x < img.width; x++) {
      if (img.data[(y * img.width + x) * 4 + 3] > 8) { return y; }
    }
  }
  return img.height;
}

const saida = {};
for (const [chave, cantos, arquivo] of [
  ['corpo', ['lower', 'lower', 'lower', 'lower'], BASE + '_ground_body.png'],
  ['topo', ['upper', 'upper', 'lower', 'lower'], BASE + '_ground_top.png'],
]) {
  const t = porCantos(...cantos);
  const img = p.crop(folha, t.col * LADO, t.row * LADO, LADO, LADO);
  p.write(path.join(ENV, 'Gameplay/' + arquivo), img);
  const linha = primeiraLinhaOpaca(img);
  saida[chave] = { tile: t.nome, indice: t.i, celula: `${t.col},${t.row}`, primeiraLinhaOpaca: linha };
  console.log(`  ${arquivo.padEnd(24)} ${t.nome} (índice ${t.i}, célula ${t.col},${t.row})   ` +
    `primeira linha opaca: ${linha} de ${LADO}px`);
}

// O corpo tem de ser sólido de ponta a ponta: se a primeira linha opaca dele não for 0, o tile
// escolhido não é o de miolo e a faixa vai ter uma fresta transparente no topo de cada célula.
if (saida.corpo.primeiraLinhaOpaca !== 0) {
  console.error(`\nERRO: o tile de corpo começa na linha ${saida.corpo.primeiraLinhaOpaca}, ` +
    'não em 0 — não é um tile sólido');
  process.exit(1);
}
console.log(`\nA superfície pisável do tile de topo está a ${saida.topo.primeiraLinhaOpaca}px do ` +
  `topo do tile (${(saida.topo.primeiraLinhaOpaca / 42.857143).toFixed(3)} un). ` +
  'O vestidor sobe a faixa de topo por esse tanto para a arte encostar no colisor.');
fs.writeFileSync(path.join(ENV, `Gameplay/${BASE}_ground_medidas.json`),
  JSON.stringify(saida, null, 2) + '\n');
