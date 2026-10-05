// Uma pedra de muralha caída, sozinha, para o Obstacle_Low de Troia (0,6 × 0,8 un) — 0 gerações:
//
//   node Tools/build-troy-obstacle.js
//
// Recorta a PRIMEIRA PEDRA INTEIRA do troy_fallen_block, de junta a junta (colunas 12 a 42, medidas pelo perfil de pixels
// escuros: as juntas estão em 11-12, 42-43, 81-83 e 114-115), então as duas bordas já vêm com o contorno do próprio
// asset — mesma fiada, paleta e densidade das plataformas. 31 × 35 px = 0,72 × 0,82 un.
//
// (A primeira versão espelhava a ponta do bloco, e o espelho duplicava o contorno da junta num traço preto no meio.)
const fs = require('fs'), path = require('path');
const png = require('./png.js');
const { textureMeta, ALIGN } = require('./unity-import.js');

const ROOT = path.join(__dirname, '..');
const FONTE = 'Assets/Art/Environments/Troy/Gameplay/troy_fallen_block.png';
const SAIDA = 'Assets/Art/Environments/Troy/Gameplay/troy_fallen_stone.png';
const X0 = 12, X1 = 42;

const src = png.read(path.join(ROOT, FONTE));
const out = png.crop(src, X0, 0, X1 - X0 + 1, src.height);
png.write(path.join(ROOT, SAIDA), out);
fs.writeFileSync(path.join(ROOT, SAIDA + '.meta'), textureMeta(SAIDA, out, { align: ALIGN.bottom }));
console.log(`${SAIDA}: ${out.width}x${out.height} px = ${(out.width / 42.857143).toFixed(2)} x ${(out.height / 42.857143).toFixed(2)} un`);
