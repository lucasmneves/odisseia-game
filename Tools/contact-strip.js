// Tira de contato de um estado de animação, ampliada, para olhar o ciclo inteiro de uma vez.
//
//   node Tools/contact-strip.js <pasta_do_estado> <saida.png> [zoom]
//
// Um ciclo de corrida se julga pela POSE de cada quadro e pela progressão entre eles, e isso
// não dá para ver abrindo um PNG de 84 px por vez. O fundo é chapado de propósito: sobre
// transparência o contorno preto do sprite some contra o xadrez do visualizador.
const fs = require('fs'), path = require('path');
const p = require('./png.js');

const dir = process.argv[2], saida = process.argv[3], zoom = +(process.argv[4] || 4);
const arqs = fs.readdirSync(dir).filter(n => n.endsWith('.png')).sort();
const imgs = arqs.map(n => p.read(path.join(dir, n)));
const W = imgs[0].width, H = imgs[0].height;

const out = p.blank(W * imgs.length * zoom, H * zoom);
const FUNDO = [120, 170, 205];
for (let i = 0; i < out.data.length; i += 4) {
  out.data[i] = FUNDO[0]; out.data[i + 1] = FUNDO[1]; out.data[i + 2] = FUNDO[2]; out.data[i + 3] = 255;
}

imgs.forEach((im, k) => {
  for (let y = 0; y < H; y++) {
    for (let x = 0; x < W; x++) {
      const o = (y * W + x) * 4;
      if (im.data[o + 3] < 8) continue;
      // Vizinho mais próximo à mão: qualquer reamostragem suave borraria o pixel art e
      // esconderia justamente o que se quer julgar.
      for (let dy = 0; dy < zoom; dy++) {
        for (let dx = 0; dx < zoom; dx++) {
          const X = (k * W + x) * zoom + dx, Y = y * zoom + dy;
          const q = (Y * out.width + X) * 4;
          out.data[q] = im.data[o]; out.data[q + 1] = im.data[o + 1];
          out.data[q + 2] = im.data[o + 2]; out.data[q + 3] = 255;
        }
      }
    }
  }
});

p.write(saida, out);
console.log(`${arqs.length} frames de ${W}x${H} -> ${saida} (${out.width}x${out.height})`);
