// Torna uma camada de parallax ladrilhável por espelho, com período 2W-2.
//
// Por que isso importa aqui: a camada de montanhas foi gerada pelo `pro` e tem emenda de
// 120,7 contra p90 interna de 14,2 — não ladrilha. Sem ladrilhar, a única saída é uma imagem
// única com fator de parallax alto (perto de 1), que é o que o PrologueSceneBuilder fazia com
// as telas pintadas antigas. E fator alto significa parallax quase nulo justamente na camada
// mais PRÓXIMA, que é onde a profundidade deveria aparecer mais.
//
// Espelhando, a camada passa a ladrilhar e o fator pode descer para onde a profundidade
// realmente se vê. O custo é simetria, que numa silhueta distante e de baixo detalhe — que é
// o que a seção 5 do briefing pede — não se lê.
//
//   node Tools/make-tileable.js <entrada.png> <saida.png>
const p = require('./png.js');
const { check } = require('./seam-test.js');

function mirrorDouble(img) {
  // Período 2W-2, não 2W: o espelho ingênuo repete a coluna da borda e deixa linha dupla.
  const W = img.width, period = 2 * W - 2;
  const out = p.blank(period, img.height);
  for (let y = 0; y < img.height; y++)
    for (let x = 0; x < period; x++) {
      const sx = x < W ? x : period - x;
      const so = (y * W + sx) * 4;
      img.data.copy(out.data, (y * period + x) * 4, so, so + 4);
    }
  return out;
}

if (require.main === module) {
  const [inF, outF] = process.argv.slice(2);
  const src = p.read(inF);
  const antes = check(src);
  const dst = mirrorDouble(src);
  const depois = check(dst);
  p.write(outF, dst);
  const UN = 42.857143;
  console.log(`${inF}  ${src.width}x${src.height}  emenda ${antes.seam.toFixed(1)} (p90 interna ${antes.p90.toFixed(1)}) -> ${antes.ok ? 'ladrilha' : 'EMENDA VISÍVEL'}`);
  console.log(`${outF}  ${dst.width}x${dst.height} = ${(dst.width / UN).toFixed(2)} un de período`);
  console.log(`  emenda ${depois.seam.toFixed(1)} (p90 interna ${depois.p90.toFixed(1)}) -> ${depois.ok ? 'LADRILHA' : 'AINDA COM EMENDA'}`);
}
module.exports = { mirrorDouble };
