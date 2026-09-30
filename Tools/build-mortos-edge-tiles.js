// Tiles de BORDA do chão do Mundo dos Mortos.
//
// O chão da fase é um passadiço sobre o rio, então a faixa de chão precisa ter uma face curta
// (~1,8 un) e acabar numa linha d'água — não descer 3,7 un até sumir no escuro.
//
// Encurtar a faixa pelo `size` do SpriteRenderer não serve: com o pivô na base, o Tiled desenha
// a partir de baixo, e reduzir a altura guarda a BASE do tile. Medido, a base do chão de caverna
// tem luminância 0,17 — é a parte desenhada para se dissolver no escuro. O resultado foi uma
// face preta sob o jogador, enquanto o trecho de ruína (base 0,26) aparecia normal. Foi essa
// diferença que criou o corte vertical duro em x=20, na junção entre os dois materiais.
//
// O que se quer é o TOPO do tile: a superfície e a primeira fiada de pedra. Isso é um recorte, e
// recorte é trabalho de pipeline, não de parâmetro de renderer.
//
//   node Tools/build-mortos-edge-tiles.js
const path = require('path');
const p = require('./png.js');

const PXPORUN = 42.857143;
const RAIZ = path.join(__dirname, '..', 'Assets/Art/Environments/MundoDosMortos/Gameplay');
const ALTURA_UN = 1.8;

const TILES = ['mortos_tiles_cavefloor', 'mortos_tiles_ruinfloor'];

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
