// Tiles de BORDA do chão das Sereias.
//
// Na primeira montagem a faixa de areia entrou com a altura nativa, 3,73 un, e tomou o terço
// inferior do quadro como uma laje única e chapada — enquanto as lajes de rocha tomavam o terço
// superior. O mar, que é o assunto da fase, ficou espremido numa fresta no meio.
//
// Encurtar pelo `size` do renderer não serve, e a razão já está registrada em Mundo dos Mortos:
// com o pivô na base o Tiled desenha de baixo para cima, e reduzir a altura guarda a BASE do
// tile. Aqui a base é areia molhada escura; o que interessa é o TOPO, a superfície clara da
// praia. Recorte é trabalho de pipeline, não de parâmetro de renderer.
//
//   node Tools/build-mortos-edge-tiles.js
const path = require('path');
const p = require('./png.js');

const PXPORUN = 42.857143;
const RAIZ = path.join(__dirname, '..', 'Assets/Art/Environments/Sereias/Gameplay');
const ALTURA_UN = 2.2;

const TILES = ['sereias_tiles_sand', 'sereias_tiles_rock'];

function main() {
  const alvo = Math.round(ALTURA_UN * PXPORUN);

  for (const nome of TILES) {
    const im = p.read(path.join(RAIZ, `${nome}.png`));
    if (im.height < alvo) {
      console.error(`ERRO: ${nome} tem ${im.height}px, menos que os ${alvo}px pedidos`);
      process.exit(1);
    }

    const out = p.blank(im.width, alvo);
    for (let y = 0; y < alvo; y++) {
      for (let x = 0; x < im.width; x++) {
        const o = (y * im.width + x) * 4;
        const q = (y * im.width + x) * 4;
        for (let c = 0; c < 4; c++) { out.data[q + c] = im.data[o + c]; }
      }
    }

    // A última fiada é escurecida para a face morrer na água em vez de terminar num corte reto.
    // Duas linhas bastam: mais que isso volta a ser o degradê que se quis evitar.
    const escuras = 3;
    for (let y = alvo - escuras; y < alvo; y++) {
      const k = 0.45 + 0.35 * ((alvo - y) / escuras);
      for (let x = 0; x < im.width; x++) {
        const q = (y * im.width + x) * 4;
        for (let c = 0; c < 3; c++) { out.data[q + c] = Math.round(out.data[q + c] * k); }
      }
    }

    p.write(path.join(RAIZ, `${nome}_edge.png`), out);

    let soma = 0, n = 0;
    for (let i = 0; i < out.data.length; i += 4) {
      if (out.data[i + 3] < 128) { continue; }
      soma += (0.299 * out.data[i] + 0.587 * out.data[i + 1] + 0.114 * out.data[i + 2]) / 255;
      n++;
    }
    console.log(`${nome}_edge  ${im.width}x${alvo}  (${ALTURA_UN} un)  L media ${(soma / n).toFixed(3)}`);
  }
}

main();
