// Detecta se um PNG de "pixel art" foi na verdade renderizado com pixels ampliados (2x, 3x…).
// Vale conferir sempre: um asset ampliado passa no teste de escala em unidades e mesmo assim
// destoa do personagem, porque a densidade de pixels fica pela metade.
//
// O sinal é o histograma de sequências de linhas idênticas. Ampliação 2x produz quase só
// sequências de 2. Não use o MDC: uma única sequência ímpar zera o resultado.
const p = require('./png.js');
const UN = 42.857143;

function runHistogram(img, axis) {
  const n = axis === 'y' ? img.height : img.width;
  const m = axis === 'y' ? img.width : img.height;
  const line = (i) => {
    const out = new Array(m);
    for (let j = 0; j < m; j++) {
      const [x, y] = axis === 'y' ? [j, i] : [i, j];
      out[j] = img.data.readUInt32BE((y * img.width + x) * 4);
    }
    return out.join(',');
  };
  const runs = [];
  let run = 1, prev = line(0);
  for (let i = 1; i < n; i++) {
    const cur = line(i);
    if (cur === prev) run++; else { runs.push(run); run = 1; }
    prev = cur;
  }
  runs.push(run);
  const hist = {};
  for (const r of runs) hist[r] = (hist[r] || 0) + 1;
  // Fator provável: se >70% das linhas estão em sequências de tamanho k>1, é ampliação k.
  let best = 1;
  for (const [k, c] of Object.entries(hist)) if (+k > 1 && (+k * c) / n > 0.7) best = +k;
  return { hist, factor: best };
}

for (const f of process.argv.slice(2)) {
  const img = p.read(f);
  const b = p.bounds(img);
  const y = runHistogram(img, 'y'), x = runHistogram(img, 'x');
  const k = Math.max(x.factor, y.factor);
  console.log(`${f}`);
  console.log(`  canvas ${img.width}x${img.height}   conteúdo ${b.w}x${b.h} = ${(b.w / UN).toFixed(2)} x ${(b.h / UN).toFixed(2)} un`);
  console.log(`  sequências Y ${JSON.stringify(y.hist)}`);
  console.log(`  sequências X ${JSON.stringify(x.hist)}`);
  console.log(`  densidade: ${k === 1 ? 'nativa (1x) — bate com o personagem' : `AMPLIADA ${k}x — arte real de ${b.w / k}x${b.h / k}, não usar`}`);
}
