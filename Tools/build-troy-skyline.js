// Silhueta distante de Troia, por código.
//
//   node Tools/build-troy-skyline.js
//
// A seção 5 do briefing pede menos detalhe em elemento distante — que é justamente onde o
// código ganha e o modelo generativo insiste em colocar detalhe. Em Ítaca isso já tinha
// custado: ilha e oceano foram mandados para o `pro` e voltaram como cenas de vila.
//
// Silhueta em DOIS tons chapados, sem textura: é o que faz o fundo recuar. A muralha real, com
// suas 16 cores, compete com o primeiro plano quando aparece ao longe.
const path = require('path'), fs = require('fs');
const p = require('./png.js');
const { loadPalette } = require('./ramp-map.js');

const ROOT = path.join(__dirname, '..');
const ENV = path.join(ROOT, 'Docs/Environment_Troy');
const ramps = loadPalette(path.join(ENV, 'Palette/TROY_PALETTE.gpl'));

const UN = 42.857143;
const W = 512, H = 176;                     // 11,95 x 4,11 un
const CORPO = ramps['Pedra sombra'][2];
const TOPO = ramps['Pedra sombra'][1];      // uma fiada mais clara no coroamento

const img = p.blank(W, H);
const preenche = (x0, y0, w, h, c) => {
  for (let y = y0; y < y0 + h && y < H; y++)
    for (let x = x0; x < x0 + w && x < W; x++) {
      if (x < 0 || y < 0) continue;
      const o = (y * W + x) * 4;
      img.data[o] = c[0]; img.data[o + 1] = c[1]; img.data[o + 2] = c[2]; img.data[o + 3] = 255;
    }
};

// Semente fixa: a silhueta tem de sair igual a cada reconstrução.
let s = 20260904;
const rnd = () => ((s = (s * 1103515245 + 12345) & 0x7fffffff) / 0x7fffffff);

// Cortina de muralha correndo a faixa inteira, com o topo irregular.
const BASE = H - 8;
for (let x = 0; x < W; x += 4) {
  const alt = 54 + Math.round(rnd() * 10);
  preenche(x, BASE - alt, 4, alt, CORPO);
  preenche(x, BASE - alt, 4, 3, TOPO);
}

// Torres, mais altas que a cortina, espaçadas de forma irregular.
// A largura é múltipla de 4 para o padrão de ameias fechar em qualquer posição.
let x = 12;
while (x < W - 24) {
  const larg = 20 + Math.round(rnd() * 3) * 4;
  const alt = 86 + Math.round(rnd() * 34);
  preenche(x, BASE - alt, larg, alt, CORPO);
  preenche(x, BASE - alt, larg, 4, TOPO);
  // Ameias: cheio e vazio alternados de 4 px.
  for (let m = 0; m < larg; m += 8) preenche(x + m + 4, BASE - alt - 5, 4, 5, TOPO);
  x += larg + 40 + Math.round(rnd() * 60);
}

// Base plana: a silhueta assenta numa linha, senão parece flutuar sobre o horizonte.
preenche(0, BASE, W, 8, CORPO);

fs.mkdirSync(path.join(ENV, 'Layers'), { recursive: true });
const saida = path.join(ENV, 'Layers/troy_bg_city.png');
p.write(saida, img);
console.log(`troy_bg_city  ${W}x${H}px = ${(W / UN).toFixed(2)} x ${(H / UN).toFixed(2)} un, 2 cores`);
