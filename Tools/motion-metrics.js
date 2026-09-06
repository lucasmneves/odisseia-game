// Quanto um ciclo de animação realmente se mexe, em número.
//
//   node Tools/motion-metrics.js <pasta_do_estado> [mais pastas...]
//
// "Movimenta pouco" é uma discussão até virar medida. Três números, e o que faz eles
// funcionarem é a paleta: as rampas separam o que a silhueta sozinha confunde.
//
//   braços  — extensão horizontal dos pixels de PELE na faixa do tronco. A pele acha o
//             antebraço e o punho; usar a silhueta inteira mediria a capa, que esvoaça
//             sozinha e daria movimento de braço onde não há.
//   pernas  — extensão horizontal na faixa das canelas. Ali não há capa nem braço.
//   energia — diferença média por pixel entre quadros vizinhos, no ciclo fechado. Pega o
//             movimento que não é nem braço nem perna: tronco, cabeça, inclinação.
//
// De cada um interessa a AMPLITUDE ao longo do ciclo (máximo − mínimo), não o valor num
// quadro: uma pose larga parada o ciclo inteiro não é movimento.
const fs = require('fs'), path = require('path');
const p = require('./png.js');
const { loadPalette } = require('./ramp-map.js');

const ramps = loadPalette(path.join(__dirname, '..', 'Docs/CharacterMaster_Odysseus/Palette/ODYSSEUS_PALETTE.gpl'));
const PELE = ramps['Pele'];

// Faixas do corpo, medidas no canvas de 84: cabeça começa em y≈12, pés em y=71.
const TRONCO = [26, 52];   // ombro até o quadril — onde o braço se move
const CANELA = [56, 72];   // abaixo do joelho — nem capa nem braço chegam aqui

const ehPele = (r, g, b) => PELE.some(c => (r - c[0]) ** 2 + (g - c[1]) ** 2 + (b - c[2]) ** 2 < 400);

function extensao(im, [y0, y1], sofPele) {
  let esq = 1e9, dir = -1;
  for (let y = y0; y < y1 && y < im.height; y++) {
    for (let x = 0; x < im.width; x++) {
      const o = (y * im.width + x) * 4;
      if (im.data[o + 3] < 8) continue;
      if (sofPele && !ehPele(im.data[o], im.data[o + 1], im.data[o + 2])) continue;
      if (x < esq) esq = x;
      if (x > dir) dir = x;
    }
  }
  return dir < 0 ? 0 : dir - esq + 1;
}

function medir(dir) {
  const arqs = fs.readdirSync(dir).filter(n => n.endsWith('.png')).sort();
  const ims = arqs.map(n => p.read(path.join(dir, n)));
  const bracos = ims.map(im => extensao(im, TRONCO, true));
  const pernas = ims.map(im => extensao(im, CANELA, false));

  // Ciclo FECHADO: a emenda do último para o primeiro conta como qualquer outro par, senão
  // um ciclo que dá um salto feio na volta passaria por suave.
  let energia = 0;
  for (let k = 0; k < ims.length; k++) {
    const a = ims[k], b = ims[(k + 1) % ims.length];
    let soma = 0, n = 0;
    for (let i = 0; i < a.data.length; i += 4) {
      const va = a.data[i + 3] > 8, vb = b.data[i + 3] > 8;
      if (!va && !vb) continue;
      n++;
      if (va !== vb) { soma += 255; continue; }
      soma += (Math.abs(a.data[i] - b.data[i]) + Math.abs(a.data[i + 1] - b.data[i + 1])
             + Math.abs(a.data[i + 2] - b.data[i + 2])) / 3;
    }
    energia += soma / n;
  }
  energia /= ims.length;

  // Deslocamento do centro de massa da PELE do tronco: o punho sobe ao queixo e desce ao
  // quadril, ou seja o braço da corrida se move na diagonal. Só a extensão horizontal
  // perderia metade disso.
  const centros = ims.map(im => {
    let sx = 0, sy = 0, n = 0;
    for (let y = TRONCO[0]; y < TRONCO[1]; y++) for (let x = 0; x < im.width; x++) {
      const o = (y * im.width + x) * 4;
      if (im.data[o + 3] < 8) continue;
      if (!ehPele(im.data[o], im.data[o + 1], im.data[o + 2])) continue;
      sx += x; sy += y; n++;
    }
    return n ? [sx / n, sy / n] : null;
  }).filter(Boolean);
  let curso = 0;
  for (const a of centros) for (const b of centros)
    curso = Math.max(curso, Math.hypot(a[0] - b[0], a[1] - b[1]));

  const amp = v => Math.max(...v) - Math.min(...v);
  return {
    nome: path.basename(dir), quadros: ims.length,
    bracos: amp(bracos), bracosMax: Math.max(...bracos),
    pernas: amp(pernas), pernasMax: Math.max(...pernas), curso,
    energia,
  };
}

const linhas = process.argv.slice(2).map(medir);
console.log('estado'.padEnd(34) + 'qd  bracos(amp/max)  curso  pernas(amp/max)  energia');
for (const l of linhas) {
  console.log(
    l.nome.padEnd(34) +
    String(l.quadros).padStart(2) + '   ' +
    (l.bracos + ' / ' + l.bracosMax).padEnd(16) +
    l.curso.toFixed(1).padEnd(7) +
    (l.pernas + ' / ' + l.pernasMax).padEnd(17) +
    l.energia.toFixed(1));
}
