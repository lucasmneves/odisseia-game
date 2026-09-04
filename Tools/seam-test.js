// Teste de emenda correto: compara a fronteira que aparece ao ladrilhar contra uma
// fronteira interna equivalente da mesma imagem.
//
// Comparar a coluna 0 com a última direto dá falso positivo em arte com dithering ou
// textura irregular, porque colunas vizinhas *devem* diferir. O que importa é se a emenda
// difere MAIS do que uma junção qualquer do interior. Erro cometido duas vezes no projeto.
const p = require('./png.js');

const diff = (img, xa, xb) => {
  let s = 0, n = 0;
  for (let y = 0; y < img.height; y++) {
    const a = (y * img.width + xa) * 4, b = (y * img.width + xb) * 4;
    if (img.data[a + 3] <= 8 && img.data[b + 3] <= 8) continue;
    s += Math.abs(img.data[a] - img.data[b]) + Math.abs(img.data[a + 1] - img.data[b + 1]) + Math.abs(img.data[a + 2] - img.data[b + 2]);
    n++;
  }
  return n ? s / n : 0;
};

function check(img) {
  const seam = diff(img, img.width - 1, 0);           // o que o olho vê ao ladrilhar
  const internal = [];
  for (let x = 1; x < img.width - 1; x++) internal.push(diff(img, x - 1, x));
  internal.sort((a, b) => a - b);
  const median = internal[internal.length >> 1];
  const p90 = internal[Math.floor(internal.length * 0.9)];
  return { seam, median, p90, ok: seam <= p90 };
}

if (require.main === module) {
  for (const f of process.argv.slice(2)) {
    const r = check(p.read(f));
    console.log(`${f}`);
    console.log(`  emenda ${r.seam.toFixed(1)} | mediana interna ${r.median.toFixed(1)} | p90 interna ${r.p90.toFixed(1)}`);
    console.log(`  ${r.ok ? 'LADRILHA — a emenda não se destaca das junções internas' : 'EMENDA VISÍVEL'}`);
  }
}
module.exports = { check, diff };
