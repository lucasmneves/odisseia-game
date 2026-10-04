// Moeda do coletável no TAMANHO DE TELA de antes, na densidade nativa (42,857 px/un), para o prefab ir a escala 1.
//
//   node Tools/build-coin-native.js
//
// O `item_collectible_coin` do Asset Completion foi reamostrado de 100 para 42,857 px/un mantendo o tamanho "em escala 1"
// da arte pintada (0,86 un) — mas o prefab a desenhava a 0,35 (0,33 un). Em escala 1 a moeda teria metade da altura do
// Odisseu; encolher no Transform reabriria o problema de densidade (D-050). Aqui cada quadro é reduzido à metade com o
// filtro de MODA do downscale-native (nunca inventa cor): 34×37 → 17×19 px = 0,40 × 0,44 un.
//
// Fonte: Assets/Art/Items/item_collectible_coin.png (intacta). Saída em Resources, porque o SpriteAnimator carrega de lá:
// Assets/Resources/Odisseia/Items/item_collectible_coin.png, com .meta fatiado (8 quadros, pivô no centro, como a moeda
// antiga) e GUID derivado do caminho — re-rodar não quebra referência.
const fs = require('fs'), path = require('path');
const png = require('./png.js');
const { moda } = require('./downscale-native.js');
const { textureMeta, ALIGN } = require('./unity-import.js');

const ROOT = path.join(__dirname, '..');
const FONTE = 'Assets/Art/Items/item_collectible_coin.png';
const SAIDA = 'Assets/Resources/Odisseia/Items/item_collectible_coin.png';
const FATOR = 0.5;
const QUADROS = 8, LARGURA = 34, PASSO = 35, ALTURA = 37;

const src = png.read(path.join(ROOT, FONTE));
const quadros = [];
for (let i = 0; i < QUADROS; i++) {
  quadros.push(moda(png.crop(src, i * PASSO, 0, LARGURA, ALTURA), FATOR));
}

const w = quadros[0].width, h = quadros[0].height, gap = 1;
const folha = png.blank(QUADROS * w + (QUADROS - 1) * gap, h);
quadros.forEach((q, i) => png.blit(folha, q, i * (w + gap), 0));
png.write(path.join(ROOT, SAIDA), folha);

const slice = quadros.map((_, i) => ({
  name: 'item_collectible_coin_' + String(i).padStart(2, '0'),
  x: i * (w + gap), y: 0, w, h,
}));
fs.writeFileSync(path.join(ROOT, SAIDA + '.meta'), textureMeta(SAIDA, folha, { align: ALIGN.center, slice }));

console.log(`${SAIDA}: ${QUADROS} quadros de ${w}x${h} px = ${(w / 42.857143).toFixed(2)} x ${(h / 42.857143).toFixed(2)} un`);
