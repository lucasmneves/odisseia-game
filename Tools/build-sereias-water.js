// Faixa de mar das Sereias, construída por código.
//
//   node Tools/build-sereias-water.js
//
// A geração veio com riscos VERTICAIS ondulados — lê como cachoeira, não como mar parado. E o
// briefing desta fase depende de o mar parecer calmo e convidativo: é a calmaria que esconde o
// perigo. Riscos horizontais são o que faz uma superfície ler como água parada; foi assim que o
// rio de Mundo dos Mortos deixou de parecer grama.
//
// Faixa que ladrilha lateralmente é geometria, e geometria é trabalho de código. O que vale
// gerar é o orgânico — a pedra, a madeira, a vegetação.
const path = require('path');
const p = require('./png.js');
const { loadPalette } = require('./ramp-map.js');

const ROOT = path.join(__dirname, '..');
const ENV = path.join(ROOT, 'Docs/Environment_Sereias');
const ramps = loadPalette(path.join(ENV, 'Palette/SEREIAS_PALETTE.gpl'));
const UN = 42.857143;

const LARGURA = 512;
const ALTURA = 128;

function aleatorio(semente) {
  let s = semente >>> 0;
  return () => { s = (s * 1664525 + 1013904223) >>> 0; return s / 4294967296; };
}

function main() {
  const rasa = ramps['Agua rasa'], funda = ramps['Agua funda'], espuma = ramps['Espuma'];
  const img = p.blank(LARGURA, ALTURA);
  const rnd = aleatorio(0x5e2ea1);

  const por = (x, y) => (y * LARGURA + x) * 4;
  const pintar = (x, y, cor) => {
    const o = por(((x % LARGURA) + LARGURA) % LARGURA, y);
    img.data[o] = cor[0]; img.data[o + 1] = cor[1]; img.data[o + 2] = cor[2]; img.data[o + 3] = 255;
  };

  // Fundo: quatro degraus de profundidade, do turquesa claro no topo ao teal fundo na base.
  // Degraus e não degradê — 36 cores não comportam gradiente, e degrau é o vocabulário do
  // pixel art de 16 bits.
  const degraus = [rasa[0], rasa[1], rasa[2], rasa[3], funda[0], funda[1], funda[2]];
  for (let y = 0; y < ALTURA; y++) {
    const k = Math.min(degraus.length - 1, Math.floor((y / ALTURA) ** 0.8 * degraus.length));
    for (let x = 0; x < LARGURA; x++) { pintar(x, y, degraus[k]); }
  }

  // Reflexos: riscos horizontais longos, mais densos e mais claros perto da superfície. O
  // comprimento é o que faz a leitura — um risco curto vira cascalho, um vertical vira chuva.
  let riscos = 0;
  for (let y = 1; y < ALTURA - 1; y += 2) {
    const profundidade = y / ALTURA;
    const densidade = 0.55 * (1 - profundidade) ** 1.4;
    let x = Math.floor(rnd() * 60);
    while (x < LARGURA) {
      if (rnd() < densidade) {
        const comp = 14 + Math.floor(rnd() * 70);
        const brilhante = rnd() < 0.3 && profundidade < 0.4;
        const cor = brilhante ? espuma[0] : profundidade < 0.5 ? espuma[1] : rasa[0];
        for (let i = 0; i < comp; i++) { pintar(x + i, y, cor); }
        riscos++;
        x += comp + 10 + Math.floor(rnd() * 60);
      } else {
        x += 14;
      }
    }
  }

  const saida = path.join(ENV, 'Ocean', 'sereias_water_surface.png');
  require('fs').mkdirSync(path.dirname(saida), { recursive: true });

  // Espelha para ladrilhar sem emenda. Período 2W−2, e não 2W: com 2W a coluna da borda
  // apareceria duplicada e o olho pega a repetição.
  const P = 2 * LARGURA - 2;
  const largo = p.blank(P, ALTURA);
  for (let y = 0; y < ALTURA; y++) {
    for (let x = 0; x < P; x++) {
      const sx = x < LARGURA ? x : P - x;
      const o = por(sx, y), q = (y * P + x) * 4;
      for (let c = 0; c < 4; c++) { largo.data[q + c] = img.data[o + c]; }
    }
  }
  p.write(saida, largo);

  // As cores usadas têm de sair todas da paleta — a faixa é construída, não gerada, e nada aqui
  // pode introduzir cor nova.
  const permitidas = new Set();
  for (const k of Object.keys(ramps)) { for (const c of ramps[k]) { permitidas.add(c.join(',')); } }
  const usadas = new Set();
  for (let i = 0; i < largo.data.length; i += 4) {
    usadas.add([largo.data[i], largo.data[i + 1], largo.data[i + 2]].join(','));
  }
  const forasteiras = [...usadas].filter(c => !permitidas.has(c));
  console.log(`mar: ${P}x${ALTURA} (${(P / UN).toFixed(2)}x${(ALTURA / UN).toFixed(2)} un), ` +
    `${riscos} riscos, ${usadas.size} cores`);
  if (forasteiras.length) {
    console.error('ERRO: cores fora da paleta: ' + forasteiras.join(' | '));
    process.exit(1);
  }
}

main();
