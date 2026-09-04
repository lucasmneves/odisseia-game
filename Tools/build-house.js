// Monta a casa pequena de Ítaca sem gastar geração:
//   corpo da v3 (escala e densidade corretas) + laje de telhado plano redesenhada.
//
// Por que assim: a v1/v2 tinham o telhado plano certo mas o Odisseu não passava pela porta;
// a v3 acertou a escala e a densidade e errou só o telhado, que saiu de telha inclinada —
// a leitura romana que a seção 9 do briefing proíbe. Recortar a v3 abaixo do telhado e
// redesenhar a laje aproveita tudo que passou e refaz só o que reprovou.
//
//   GLASS=Agua node Tools/build-house.js [saída]   → janela envidraçada em vez de vão escuro
const path = require('path');
const p = require('./png.js');
const { loadPalette, remap } = require('./ramp-map.js');

const ROOT = path.join(__dirname, '..');
const ENV = path.join(ROOT, 'Docs/Environment_Ithaca');
const out = process.argv[2] || path.join(ENV, 'Architecture/ithaca_house_small_01.png');

const ramps = loadPalette(path.join(ENV, 'Palette/ITHACA_PALETTE.gpl'));
const src = p.read(path.join(ENV, 'Architecture/_fonte_v3_telha_inclinada.png'));

// A linha 58 é onde o contorno inferior do telhado termina e a parede começa.
// As linhas 58-65 já trazem a sombra do beiral, que serve igual para uma laje plana.
const BODY_TOP = 58, BODY_BOTTOM = 176;
const body = p.crop(src, 0, BODY_TOP, src.width, BODY_BOTTOM - BODY_TOP + 1);
const bb = p.bounds(body);

// Laje: mesma estratigrafia medida na v2 (contorno 2 / luz 2 / corpo 5 / sombra 1 / contorno 2),
// com o balanço proporcional à parede — 3px sobre 108 na v2, ~5px sobre 176 aqui.
const OVERHANG = 5, SLAB_H = 12;
const slabX0 = bb.x0 - OVERHANG, slabW = bb.w + OVERHANG * 2;
const [OUT, LIGHT, BASE, SHADE] = [
  ramps['Contorno'][3], ramps['Terra / caminho'][0], ramps['Terra / caminho'][1], ramps['Terra / caminho'][2],
];
const bands = [[0, 2, OUT], [2, 4, LIGHT], [4, 9, BASE], [9, 10, SHADE], [10, 12, OUT]];

const W = src.width, H = SLAB_H + body.height;
const house = p.blank(W, H);
for (const [y0, y1, c] of bands)
  for (let y = y0; y < y1; y++)
    for (let x = slabX0; x < slabX0 + slabW; x++) {
      // Os cantos externos do contorno são cortados em 1px para a laje não ficar num bloco duro.
      const edge = (x === slabX0 || x === slabX0 + slabW - 1);
      if (edge && (y === 0 || y === SLAB_H - 1)) continue;
      const o = (y * W + x) * 4;
      house.data[o] = c[0]; house.data[o + 1] = c[1]; house.data[o + 2] = c[2]; house.data[o + 3] = 255;
    }
p.blit(house, body, 0, SLAB_H);

console.log('remapeamento na paleta oficial:');
const final = remap(house, ramps, { verbose: true, glassRamp: process.env.GLASS });

// Recorta no conteúdo: a laje avança 5px além da parede, então a caixa útil não é a do canvas.
const fb = p.bounds(final);
p.write(out, p.crop(final, fb.x0, fb.y0, fb.w, fb.h));

const UN = 42.857143;
console.log(`\n${path.relative(ROOT, out)}`);
console.log(`  casa  ${fb.w}x${fb.h}px = ${(fb.w / UN).toFixed(2)} x ${(fb.h / UN).toFixed(2)} un   (alvo de altura ~3,0)`);
