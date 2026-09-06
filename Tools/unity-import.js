// Leva o cenário de Ítaca para o Unity: copia os PNGs e escreve os .meta já com as
// configurações certas.
//
// Por que escrever o .meta em vez de importar e ajustar depois: o Editor gera um .meta com
// os defaults dele (compressão ligada, filtro bilinear, 100 px por unidade) no instante em
// que vê o arquivo. Com o .meta pronto ao lado do PNG, ele importa certo de primeira — e
// isto funciona com o Editor aberto, que é o caso aqui (batchmode exige ele fechado).
//
// GUIDs são derivados do caminho, então re-rodar não quebra referência nenhuma de cena.
// É a mesma regra que já vale para a folha do personagem: trocar GUID deixa o sprite
// missing no Inspector.
//
//   node Tools/unity-import.js
const fs = require('fs'), path = require('path'), crypto = require('crypto');
const p = require('./png.js');

const ROOT = path.join(__dirname, '..');
const SRC = path.join(ROOT, process.env.FASE_SRC || 'Docs/Environment_Ithaca');
const DST = path.join(ROOT, process.env.FASE_DST || 'Assets/Art/Environments/Ithaca');
const PPU = 42.857143;
const NL = String.fromCharCode(10);

// Grupo de origem -> pasta de destino. Layers vira Background, o nome que a estrutura usa.
const GRUPOS_ITACA = [
  ['Layers', 'Background'], ['Terrain', 'Terrain'], ['Architecture', 'Architecture'],
  ['Nature', 'Nature'], ['Props', 'Props'], ['Training', 'Training'],
  ['Arsenal', 'Arsenal'], ['Port', 'Port'], ['Ships', 'Ships'], ['Figures', 'Figures'],
];
const GRUPOS_TROIA = [
  ['Layers', 'Background'], ['Terrain', 'Terrain'], ['Architecture', 'Architecture'],
  ['Camp', 'Camp'], ['Effects', 'Effects'],
];
const GRUPOS_CICONES = [
  ['Background', 'Background'], ['Terrain', 'Terrain'], ['Architecture', 'Architecture'],
  ['Nature', 'Nature'], ['Props', 'Props'],
];
const GRUPOS_CYTERA = [
  ['Background', 'Background'], ['Ocean', 'Ocean'], ['Rocks', 'Rocks'],
  ['Ship', 'Ship'], ['Effects', 'Effects'], ['Weather', 'Weather'],
];
const FASE = process.env.FASE_SRC || '';
const GROUPS = FASE.includes('Troy') ? GRUPOS_TROIA
  : FASE.includes('Cicones') ? GRUPOS_CICONES
  : FASE.includes('Cytera') ? GRUPOS_CYTERA
  : GRUPOS_ITACA;

// Pasta de destino dentro de Assets, derivada de DST. Antes estava escrita a mao como
// ".../Ithaca/" em todo lugar, inclusive quando a fase importada era Troia: os GUIDs saiam de
// um caminho que nao existia. Funcionava por serem deterministicos, mas dois arquivos de mesmo
// nome em fases diferentes colidiriam no mesmo GUID.
const PASTA_UNITY = DST.slice(DST.indexOf('Assets')).split(/[\/]/).join('/');

// Tileset que e uma FAIXA que repete lateralmente, nao um kit de pecas. Cicones nao usa
// Tilemap (decisao registrada na secao 8 do master): o terreno e SpriteRenderer em modo Tiled,
// entao a faixa entra como UM sprite com wrap Repeat, e nao fatiada em celulas de 32.
// Em Cytera quase tudo que ladrilha e o proprio efeito: mar, chuva, espuma e o deck do navio
// sao faixas que o Unity repete e desloca para animar, sem folha de sprites.
const FAIXAS_CYTERA = [
  'cytera_ocean_surface.png', 'cytera_deep_water.png', 'cytera_foam_line.png',
  'cytera_rain.png', 'cytera_deck_planks.png', 'cytera_sky_storm.png',
];
const ehFaixaQueRepete = nome => nome.includes('_tiles_') || FAIXAS_CYTERA.includes(nome);

// Determinístico: o mesmo caminho sempre dá o mesmo GUID.
const guid = (key) => crypto.createHash('md5').update('odisseia:' + key).digest('hex');
// GRUPOS pode ser sobrescrito por fase: Troia tem Camp e Effects, Ítaca tem Ships e Figures.
const spriteId = (key) => crypto.createHash('md5').update('sprite:' + key).digest('hex');
// internalID é int32 com sinal; manter positivo e diferente de zero.
const internalId = (key) => (crypto.createHash('md5').update('iid:' + key).digest().readUInt32BE(0) & 0x7fffffff) || 1;

const ALIGN = { center: 0, bottom: 7 };

// O Unity escreve um spriteID no .meta de textura de sprite único, e reescrever o arquivo do
// zero o zera — o que não quebra nada (a cena referencia por fileID 21300000 mais o GUID da
// textura), mas suja o diff de todos os .meta a cada import. Preservar o valor existente
// mantém o histórico legível.
function spriteIdExistente(caminhoDoMeta) {
  if (!fs.existsSync(caminhoDoMeta)) return '';
  for (const linha of fs.readFileSync(caminhoDoMeta, 'utf8').split(String.fromCharCode(10))) {
    if (linha.startsWith('    spriteID: ')) return linha.slice('    spriteID: '.length).trim();
  }
  return '';
}

function textureMeta(unityPath, img, opts) {
  const align = opts.align === undefined ? ALIGN.bottom : opts.align;
  const repeat = !!opts.repeat, fullRect = !!opts.fullRect, slice = opts.slice || null;
  const wrap = repeat ? 0 : 1;                       // 0 = Repeat, 1 = Clamp
  const meshType = fullRect ? 0 : 1;                 // 0 = FullRect, 1 = Tight
  const spriteMode = slice ? 2 : 1;                  // 1 = Single, 2 = Multiple
  const pivotY = align === ALIGN.bottom ? '0' : '0.5';

  let table = '  internalIDToNameTable: []';
  let sprites = '    sprites: []';
  let names = '    nameFileIdTable: {}';

  if (slice) {
    const rows = [], entries = [], nameRows = [];
    for (const s of slice) {
      const key = unityPath + '#' + s.name;
      const iid = internalId(key);
      rows.push('  - first:' + NL + '      213: ' + iid + NL + '    second: ' + s.name);
      entries.push([
        '    - serializedVersion: 2',
        '      name: ' + s.name,
        '      rect:',
        '        serializedVersion: 2',
        '        x: ' + s.x, '        y: ' + s.y,
        '        width: ' + s.w, '        height: ' + s.h,
        '      alignment: ' + align,
        '      pivot: {x: 0.5, y: ' + pivotY + '}',
        '      border: {x: 0, y: 0, z: 0, w: 0}',
        '      customData: ', '      outline: []', '      physicsShape: []',
        '      tessellationDetail: 0', '      bones: []',
        '      spriteID: ' + spriteId(key),
        '      internalID: ' + iid,
        '      vertices: []', '      indices: ', '      edges: []', '      weights: []',
      ].join(NL));
      nameRows.push('      ' + s.name + ': ' + iid);
    }
    table = '  internalIDToNameTable:' + NL + rows.join(NL);
    sprites = '    sprites:' + NL + entries.join(NL);
    names = '    nameFileIdTable:' + NL + nameRows.join(NL);
  }

  const platform = (target) => [
    '  - serializedVersion: 4',
    '    buildTarget: ' + target,
    '    maxTextureSize: 2048', '    resizeAlgorithm: 0', '    textureFormat: -1',
    '    textureCompression: 0', '    compressionQuality: 50', '    crunchedCompression: 0',
    '    allowsAlphaSplitting: 0', '    overridden: 0', '    ignorePlatformSupport: 0',
    '    androidETC2FallbackOverride: 0', '    forceMaximumCompressionQuality_BC6H_BC7: 0',
  ].join(NL);

  return [
    'fileFormatVersion: 2',
    'guid: ' + guid(unityPath),
    'TextureImporter:',
    table,
    '  externalObjects: {}',
    '  serializedVersion: 13',
    '  mipmaps:',
    '    mipMapMode: 0', '    enableMipMap: 0', '    sRGBTexture: 1', '    linearTexture: 0',
    '    fadeOut: 0', '    borderMipMap: 0', '    mipMapsPreserveCoverage: 0',
    '    alphaTestReferenceValue: 0.5', '    mipMapFadeDistanceStart: 1', '    mipMapFadeDistanceEnd: 3',
    '  bumpmap:',
    '    convertToNormalMap: 0', '    externalNormalMap: 0', '    heightScale: 0.25',
    '    normalMapFilter: 0', '    flipGreenChannel: 0',
    '  isReadable: 0', '  streamingMipmaps: 0', '  streamingMipmapsPriority: 0', '  vTOnly: 0',
    '  ignoreMipmapLimit: 0', '  grayScaleToAlpha: 0', '  generateCubemap: 6',
    '  cubemapConvolution: 0', '  seamlessCubemap: 0', '  textureFormat: 1', '  maxTextureSize: 2048',
    '  textureSettings:',
    '    serializedVersion: 2',
    '    filterMode: 0',                              // Point — obrigatório em pixel art
    '    aniso: 1', '    mipBias: 0',
    '    wrapU: ' + wrap, '    wrapV: ' + wrap, '    wrapW: ' + wrap,
    '  nPOTScale: 0', '  lightmap: 0', '  compressionQuality: 50',
    '  spriteMode: ' + spriteMode,
    '  spriteExtrude: 1',
    '  spriteMeshType: ' + meshType,
    '  alignment: ' + align,
    '  spritePivot: {x: 0.5, y: ' + pivotY + '}',
    '  spritePixelsToUnits: ' + PPU,
    '  spriteBorder: {x: 0, y: 0, z: 0, w: 0}',
    '  spriteGenerateFallbackPhysicsShape: 0',
    '  alphaUsage: 1', '  alphaIsTransparency: 1', '  spriteTessellationDetail: -1',
    '  textureType: 8',                               // Sprite (2D and UI)
    '  textureShape: 1', '  singleChannelComponent: 0',
    '  flipbookRows: 1', '  flipbookColumns: 1',
    '  maxTextureSizeSet: 0', '  compressionQualitySet: 0', '  textureFormatSet: 0',
    '  ignorePngGamma: 0', '  applyGammaDecoding: 0', '  swizzle: 50462976', '  cookieLightType: 0',
    '  platformSettings:',
    platform('DefaultTexturePlatform'), platform('Standalone'), platform('WebGL'),
    '  spriteSheet:',
    '    serializedVersion: 2',
    sprites,
    '    outline: []', '    customData: ', '    physicsShape: []', '    bones: []',
    '    spriteID: ' + (opts.spriteId || ''), '    internalID: 0', '    vertices: []', '    indices: ',
    '    edges: []', '    weights: []', '    secondaryTextures: []',
    '    spriteCustomMetadata:', '      entries: []',
    names,
    '  mipmapLimitGroupName: ', '  pSDRemoveMatte: 0',
    '  userData: ', '  assetBundleName: ', '  assetBundleVariant: ', '',
  ].join(NL);
}

const folderMeta = (unityPath) => [
  'fileFormatVersion: 2', 'guid: ' + guid(unityPath), 'folderAsset: yes', 'DefaultImporter:',
  '  externalObjects: {}', '  userData: ', '  assetBundleName: ', '  assetBundleVariant: ', '',
].join(NL);

const textMeta = (unityPath) => [
  'fileFormatVersion: 2', 'guid: ' + guid(unityPath), 'TextScriptImporter:',
  '  externalObjects: {}', '  userData: ', '  assetBundleName: ', '  assetBundleVariant: ', '',
].join(NL);

// Fatia 4x4 de 32px dos tilesets Wang. O rect do Unity tem origem embaixo à esquerda, e o
// índice documentado do tileset é row-major a partir do TOPO — por isso o y é invertido.
// Manter esse índice no nome é o que preserva a correspondência com o array do JSON; o campo
// `original_position` do JSON aponta para outra grade e leva ao tile errado.
function wangSlice(base, img, tile) {
  tile = tile || 32;
  const cols = img.width / tile, rows = img.height / tile, out = [];
  for (let r = 0; r < rows; r++)
    for (let c = 0; c < cols; c++)
      out.push({
        name: base + '_' + String(r * cols + c).padStart(2, '0'),
        x: c * tile, y: img.height - (r + 1) * tile, w: tile, h: tile,
      });
  return out;
}

function ensureFolder(dir, unityPath) {
  fs.mkdirSync(dir, { recursive: true });
  const meta = dir + '.meta';
  if (!fs.existsSync(meta)) fs.writeFileSync(meta, folderMeta(unityPath));
}

ensureFolder(path.join(ROOT, 'Assets/Art/Environments'), 'Assets/Art/Environments');
ensureFolder(DST, PASTA_UNITY);

let copied = 0, sliced = 0, texts = 0;
for (const [src, dst] of GROUPS) {
  const srcDir = path.join(SRC, src), dstDir = path.join(DST, dst);
  ensureFolder(dstDir, PASTA_UNITY + '/' + dst);
  for (const f of fs.readdirSync(srcDir).sort()) {
    if (f.startsWith('_')) continue;                               // fontes, provas, descartes
    const from = path.join(srcDir, f), to = path.join(dstDir, f);
    const unityPath = PASTA_UNITY + '/' + dst + '/' + f;
    if (f.endsWith('.png')) {
      fs.copyFileSync(from, to);
      const img = p.read(from);
      const faixa = ehFaixaQueRepete(f);
      const isTileset = dst === 'Terrain' && !faixa;
      const isBackdrop = dst === 'Background';
      const tiles = isTileset ? wangSlice(path.basename(f, '.png'), img) : null;
      // Tile de terreno usa pivô central, que é o que o Tilemap espera. Todo o resto apoia
      // na base, para o objeto assentar na linha do chão do mesmo jeito que o personagem.
      // Camadas de parallax e o muro repetem lateralmente: wrap Repeat e malha FullRect,
      // que é o que o modo de desenho Tiled do SpriteRenderer exige.
      const tiling = isBackdrop || faixa || dst === 'Ocean' || f === 'ithaca_wall_low_01.png'
        || f === 'troy_wall_section_01.png' || f === 'cicones_drystone_wall.png';
      fs.writeFileSync(to + '.meta', textureMeta(unityPath, img, {
        spriteId: spriteIdExistente(to + '.meta'),
        align: isTileset ? ALIGN.center : ALIGN.bottom,   // faixa que repete cai no bottom
        repeat: tiling,
        // Malha FullRect é requisito do drawMode Tiled do SpriteRenderer, que é como o
        // terreno e o parallax são desenhados — um renderer por trecho em vez de milhares
        // de GameObjects. Os tiles precisam dela mesmo sem wrap Repeat: o Tiled gera a
        // geometria repetida, mas descarta o recorte se a malha for Tight.
        fullRect: tiling || isTileset,
        slice: tiles,
      }));
      copied++;
      if (tiles) sliced += tiles.length;
    } else if (f.endsWith('.json')) {
      fs.copyFileSync(from, to);
      fs.writeFileSync(to + '.meta', textMeta(unityPath));
      texts++;
    }
  }
}
console.log(copied + ' texturas copiadas (' + sliced + ' sprites fatiados dos tilesets), ' + texts + ' JSON como TextAsset');
console.log('destino: ' + path.relative(ROOT, DST));
