// Flecha do arco do Odisseu, desenhada por código.
//
//   node Tools/build-arrow.js
//
// Por que não gerar: na escala do jogo a flecha tem cerca de 30 px de comprimento, e nesse
// tamanho o `pixen` devolve uma mancha ilegível — testado, saiu parecendo um haltere. Ampliar
// e reduzir depois quebraria a densidade de 42,857 px por unidade que todo o resto respeita.
// Objeto pequeno e geométrico é onde o código ganha, como já foi para a coluna e o portão.
//
// A paleta é a do PERSONAGEM, não a de uma fase: a flecha é equipamento do Odisseu e viaja
// com ele por todas as fases.
const path = require('path'), fs = require('fs');
const p = require('./png.js');
const { loadPalette } = require('./ramp-map.js');

const ROOT = path.join(__dirname, '..');
const PALETA = path.join(ROOT, 'Docs/CharacterMaster_Odysseus/Palette/ODYSSEUS_PALETTE.gpl');
const SAIDA = path.join(ROOT, 'Docs/CharacterMaster_Odysseus/Combat/odysseus_arrow.png');
const UN = 42.857143;

const ramps = loadPalette(PALETA);
const MADEIRA = ramps['Tunica (linho)'];     // haste: o linho da túnica é a rampa de madeira clara
const BRONZE = ramps['Bronze / Dourado'];    // ponta
const PENA = ramps['Pele'];                  // empenação, em tom claro e quente
const OUT = ramps['Contorno'][1];

// 30 x 9 px = 0,70 x 0,21 un. A flecha aponta para a DIREITA, como todo o resto do projeto:
// o master do Odisseu é todo `east` e o código espelha quando ele vira.
const W = 30, H = 9;
const img = p.blank(W, H);

const px = (x, y, c) => {
  if (x < 0 || y < 0 || x >= W || y >= H) return;
  const o = (y * W + x) * 4;
  img.data[o] = c[0]; img.data[o + 1] = c[1]; img.data[o + 2] = c[2]; img.data[o + 3] = 255;
};
const linha = (x0, x1, y, c) => { for (let x = x0; x <= x1; x++) px(x, y, c); };

const EIXO = 4;

// -- haste: três linhas, clara em cima e escura embaixo, que é o sombreamento de forma de um
//    cilindro visto de lado --
linha(4, 23, EIXO - 1, MADEIRA[0]);
linha(4, 23, EIXO, MADEIRA[1]);
linha(4, 23, EIXO + 1, MADEIRA[2]);

// -- ponta de bronze: um triângulo em folha, com a barbela marcada --
for (let i = 0; i < 6; i++) {
  const meia = Math.max(0, 3 - Math.round(i * 0.6));
  for (let d = -meia; d <= meia; d++) {
    px(24 + i, EIXO + d, d < 0 ? BRONZE[0] : d === 0 ? BRONZE[1] : BRONZE[2]);
  }
}
px(23, EIXO - 2, BRONZE[2]);
px(23, EIXO + 2, BRONZE[2]);

// -- empenação: três penas curtas inclinadas na cauda --
for (let i = 0; i < 5; i++) {
  const alt = 3 - Math.round(i * 0.4);
  for (let d = 1; d <= alt; d++) {
    px(1 + i, EIXO - d, d === alt ? PENA[2] : PENA[0]);
    px(1 + i, EIXO + d, d === alt ? PENA[3] : PENA[1]);
  }
}

// -- contorno: só onde há silhueta, para a flecha não virar uma barra preta neste tamanho --
const solido = (x, y) => x >= 0 && y >= 0 && x < W && y < H && img.data[(y * W + x) * 4 + 3] > 8;
const contorno = [];
for (let y = 0; y < H; y++)
  for (let x = 0; x < W; x++)
    if (!solido(x, y) && (solido(x - 1, y) || solido(x + 1, y) || solido(x, y - 1) || solido(x, y + 1)))
      contorno.push([x, y]);
for (const [x, y] of contorno) px(x, y, OUT);

const final = (() => { const b = p.bounds(img); return p.crop(img, b.x0, b.y0, b.w, b.h); })();
fs.mkdirSync(path.dirname(SAIDA), { recursive: true });
p.write(SAIDA, final);
console.log(`${path.relative(ROOT, SAIDA)}  ${final.width}x${final.height}px = ` +
  `${(final.width / UN).toFixed(2)} x ${(final.height / UN).toFixed(2)} un`);
