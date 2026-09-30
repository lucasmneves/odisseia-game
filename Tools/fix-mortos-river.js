// Água sem reflexo lê como grama.
//
// Medida, a banda do rio era 38,6% #1d351e + 36,1% #27482a + 13,1% #111f0f e apenas 1,5% de
// highlight. Uma massa verde chapada — e foi exatamente assim que ela apareceu na captura, um
// campo de grama no fundo do mundo dos mortos.
//
// A correção é composição sobre o asset que já existe, não uma geração nova: o defeito é local
// (falta o especular) e o corpo da água está certo. Riscos horizontais claros são o único sinal
// que faz uma superfície ler como água — verticais leem como chuva, e pontos como cascalho.
//
//   node Tools/fix-mortos-river.js
const path = require('path');
const p = require('./png.js');

const RAIZ = path.join(__dirname, '..', 'Assets/Art/Environments/MundoDosMortos/River');
const ORIGEM = path.join(RAIZ, 'mortos_river_band.png');
// O tool escreve por cima do asset. Sem uma cópia intocada ele leria o próprio resultado na
// segunda execução e empilharia riscos sobre riscos até a água virar mingau claro.
const INTOCADO = path.join(__dirname, '..', 'Docs/Environment_MundoDosMortos/_origem/mortos_river_band.raw.png');

// Da rampa "Agua morta" da paleta da fase. O especular NÃO é branco: no subterrâneo a água
// reflete a pedra pálida e o fogo dos braseiros, nunca um céu.
const BRILHO = [0x45, 0x65, 0x53];
const CRISTA = [0x9e, 0xb3, 0xae];

// Ruído determinístico: a mesma banda tem de sair igual toda vez que o pipeline roda, senão a
// arte muda sozinha entre execuções e nenhuma captura serve de referência.
function aleatorio(semente) {
  let s = semente >>> 0;
  return () => {
    s = (s * 1664525 + 1013904223) >>> 0;
    return s / 4294967296;
  };
}

function main() {
  const fs = require('fs');
  if (!fs.existsSync(INTOCADO)) { fs.copyFileSync(ORIGEM, INTOCADO); }
  const im = p.read(INTOCADO);
  const rnd = aleatorio(0x0d15ea5e);
  const largura = im.width;
  const altura = im.height;

  const por = (x, y) => (y * largura + x) * 4;
  // O x dá a volta: um risco cortado na borda direita deixa a coluna 0 e a última diferentes, e
  // o Tiled mostra isso como uma emenda vertical a cada 12 un. Envolver torna a banda contínua
  // por construção, em vez de depender de sorte no sorteio.
  const pintar = (x, y, cor, forca) => {
    if (y < 0 || y >= altura) { return; }
    const o = por(((x % largura) + largura) % largura, y);
    if (im.data[o + 3] < 128) { return; }   // fora da água não se pinta reflexo
    for (let c = 0; c < 3; c++) {
      im.data[o + c] = Math.round(im.data[o + c] * (1 - forca) + cor[c] * forca);
    }
  };

  // Riscos em bandas horizontais. Mais perto do topo (superfície) eles são mais frequentes e
  // mais claros; no fundo a água some no escuro. É o que dá volume sem desenhar perspectiva.
  let riscos = 0;
  for (let y = 2; y < altura - 2; y += 2) {
    const profundidade = y / altura;              // 0 = superfície, 1 = fundo
    const densidade = 0.5 * (1 - profundidade) ** 1.6;
    let x = Math.floor(rnd() * 40);
    while (x < largura) {
      if (rnd() < densidade) {
        // Riscos LONGOS e finos: é a razão largura/altura que faz ler como água parada.
        const comp = 10 + Math.floor(rnd() * 46);
        const crista = rnd() < 0.22 && profundidade < 0.45;
        const cor = crista ? CRISTA : BRILHO;
        const forca = (crista ? 0.85 : 0.55) * (1 - profundidade * 0.7);
        for (let i = 0; i < comp; i++) {
          // Extremidades mais fracas: um risco de força constante lê como traço desenhado.
          const borda = Math.min(i, comp - 1 - i) / Math.max(1, comp * 0.25);
          pintar(x + i, y, cor, forca * Math.min(1, borda));
        }
        riscos++;
        x += comp + 8 + Math.floor(rnd() * 50);
      } else {
        x += 12;
      }
    }
  }

  // Trava na paleta. Misturar cor com alpha gera tons intermediários — a primeira versão saiu
  // com 453 cores únicas numa fase cujo sprite mais complexo tem algumas dezenas. Pixel art com
  // gradiente deixa de ser pixel art, e o briefing pede o contrário.
  //
  // As cores permitidas são as que a banda ORIGINAL já usava: o tool acrescenta reflexo, não
  // vocabulário novo.
  const permitidas = [];
  {
    const vistas = new Map();
    const raw = p.read(INTOCADO);
    for (let i = 0; i < raw.data.length; i += 4) {
      if (raw.data[i + 3] < 128) { continue; }
      const k = (raw.data[i] << 16) | (raw.data[i + 1] << 8) | raw.data[i + 2];
      vistas.set(k, (vistas.get(k) || 0) + 1);
    }
    // Cores com presença desprezível são sujeira da geração, não passo de rampa.
    const total = [...vistas.values()].reduce((a, b) => a + b, 0);
    for (const [k, v] of vistas) {
      if (v / total >= 0.005) { permitidas.push([(k >> 16) & 255, (k >> 8) & 255, k & 255]); }
    }
  }
  for (let i = 0; i < im.data.length; i += 4) {
    if (im.data[i + 3] < 128) { continue; }
    let melhor = permitidas[0], dist = Infinity;
    for (const c of permitidas) {
      const d = (im.data[i] - c[0]) ** 2 + (im.data[i + 1] - c[1]) ** 2 + (im.data[i + 2] - c[2]) ** 2;
      if (d < dist) { dist = d; melhor = c; }
    }
    im.data[i] = melhor[0]; im.data[i + 1] = melhor[1]; im.data[i + 2] = melhor[2];
  }
  console.log(`paleta travada em ${permitidas.length} cores`);

  p.write(ORIGEM, im);

  // A banda é repetida lado a lado (Tiled): se a coluna 0 e a última não casarem, aparece uma
  // emenda vertical a cada 12 un.
  //
  // O limiar é COMPARATIVO, não absoluto: o asset original já vem com costura de 41 (média
  // 10,3), então um teto fixo reprovaria a arte que veio da geração e não o que este tool fez.
  // O que cabe verificar aqui é só uma coisa — que os riscos não pioraram a emenda.
  const costura = (img) => {
    let pior = 0;
    for (let y = 0; y < altura; y++) {
      const a = (y * largura) * 4, b = (y * largura + largura - 1) * 4;
      for (let c = 0; c < 3; c++) { pior = Math.max(pior, Math.abs(img.data[a + c] - img.data[b + c])); }
    }
    return pior;
  };
  const antes = costura(p.read(INTOCADO));
  const depois = costura(im);
  console.log(`rio: ${riscos} riscos; costura ${antes} -> ${depois}`);
  if (depois > antes) {
    console.error('ERRO: os riscos pioraram a emenda do tiling');
    process.exit(1);
  }
}

main();
