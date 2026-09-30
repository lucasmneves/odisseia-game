// Pilar natural de caverna, COMPOSTO a partir da textura de rocha gerada.
//
//   node Tools/build-ciclopes-cave.js
//
// Duas tentativas foram descartadas antes desta, e as duas ensinam a mesma coisa por lados
// opostos:
//
// 1. GERAR "pilar de rocha" e "entrada de caverna" devolveu ALVENARIA CORTADA COM ARCO nas
//    duas — uma coluna estriada e um portal de blocos, sobre fundo opaco. O prior do modelo
//    para essas palavras e arquitetura construida, que e justamente o que o briefing proibe.
//
// 2. PINTAR a rocha por codigo, como foi feito para o altar de Cicones, saiu pior ainda: ruido
//    por soma de senos vira camuflagem, nao pedra. Altar e deck sao GEOMETRICOS — degraus,
//    postes, tabuas. Rocha e organica, e organico e onde a geracao ganha.
//
// O que resolve e COMPOR: a silhueta vem do codigo (perfil de ampulheta, que e o que separa um
// pilar natural de uma coluna) e a textura vem do tileset de parede de caverna que ja foi
// gerado. E a mesma regra que o projeto ja registrou em Itaca — defeito local pede recorte e
// composicao, nao geracao nova.
//
// A BOCA DA CAVERNA nao virou sprite nenhum. Ela e montada na cena a partir da faixa de parede,
// de um bloco escuro atras e de pedregulhos nas laterais: mais modular, custo zero, e sem
// nenhuma silhueta chapada para o modelo errar.
const path = require('path'), fs = require('fs');
const p = require('./png.js');
const { loadPalette } = require('./ramp-map.js');

const ROOT = path.join(__dirname, '..');
const ENV = path.join(ROOT, 'Docs/Environment_Ciclopes');
const ramps = loadPalette(path.join(ENV, 'Palette/CICLOPES_PALETTE.gpl'));
const CALC = ramps['Calcario'], FUNDA = ramps['Rocha profunda'];
const UN = 42.857143;

const textura = p.read(path.join(ENV, '_fontes/_fonte_tiles_cavewall.png'));

const W = 108, H = 320;
const img = p.blank(W, H);

for (let y = 0; y < H; y++) {
  const t = y / (H - 1);
  // Ampulheta: grosso nas duas pontas, fino na cintura. Estalactite encontrando estalagmite.
  const cintura = 0.40 + 0.60 * Math.pow(Math.abs(t - 0.5) * 2, 1.5);
  const m = 40 * cintura + Math.sin(t * 13) * 3.5 + Math.sin(t * 29) * 1.8;

  for (let x = 0; x < W; x++) {
    const d = Math.abs(x - W / 2);
    if (d > m) continue;

    // Amostra a textura em coordenadas proprias, para o pilar nao repetir o mesmo trecho.
    const sx = (x * 3 + Math.round(y * 0.7)) % textura.width;
    const sy = (y * 2) % textura.height;
    const o = (sy * textura.width + sx) * 4;
    let c = [textura.data[o], textura.data[o + 1], textura.data[o + 2]];

    // Volume: as bordas do cilindro escurecem. Sem isso a textura chapada le como recorte.
    const lateral = d / m;
    if (lateral > 0.80) c = FUNDA[1];
    else if (lateral > 0.58) c = [Math.round(c[0] * 0.62), Math.round(c[1] * 0.62), Math.round(c[2] * 0.62)];
    else if (lateral < 0.22) c = [Math.min(255, Math.round(c[0] * 1.28)), Math.min(255, Math.round(c[1] * 1.28)), Math.min(255, Math.round(c[2] * 1.28))];

    const q = (y * W + x) * 4;
    img.data[q] = c[0]; img.data[q + 1] = c[1]; img.data[q + 2] = c[2]; img.data[q + 3] = 255;
  }
}

// Escorrimento de calcita: riscos verticais claros, a marca visivel de pilar natural.
let s = 4242;
const rnd = () => ((s = (s * 1103515245 + 12345) & 0x7fffffff) / 0x7fffffff);
for (let i = 0; i < 30; i++) {
  const x = Math.round(W / 2 + (rnd() - 0.5) * 44);
  const y0 = Math.floor(rnd() * H * 0.75), comp = 14 + Math.floor(rnd() * 60);
  for (let y = y0; y < y0 + comp && y < H; y++) {
    const q = (y * W + x) * 4;
    if (img.data[q + 3] < 8) continue;
    img.data[q] = CALC[2][0]; img.data[q + 1] = CALC[2][1]; img.data[q + 2] = CALC[2][2];
  }
}

const dir = path.join(ENV, 'Cave');
fs.mkdirSync(dir, { recursive: true });
const b = p.bounds(img);
const final = p.crop(img, b.x0, b.y0, b.w, b.h);
p.write(path.join(dir, 'ciclopes_rock_pillar.png'), final);
console.log(`rock_pillar  ${final.width}x${final.height}px = ` +
  `${(final.width / UN).toFixed(2)} x ${(final.height / UN).toFixed(2)} un  ` +
  `(silhueta por codigo, textura da parede gerada)`);
