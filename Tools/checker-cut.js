// Remove o xadrez de transparência que o PixelLab DESENHA como conteúdo.
//
//   node Tools/checker-cut.js <entrada.png> <saida.png> [tolerância]
//
// ## Por que isto é um módulo à parte do `cutout.js`
//
// A armadilha já estava registrada no projeto ("vazio é uma palavra que o modelo desenha"),
// mas ela reapareceu duas vezes seguidas — na parede colunar de Cila e Caribdis e no templo e
// na árvore de Gado do Sol — e por um motivo que vale isolar: **o visualizador de PNG mostra
// alpha real como xadrez cinza, exatamente igual a um xadrez pintado.** Olhar não distingue os
// dois. A única prova é contar pixels opacos: o canto superior esquerdo da parede tinha 576 de
// 576 opacos, em dois cinzas alternados de ~7px de passo.
//
// E o `cutout.js` genérico NÃO resolve este caso. Ele é flood-fill de borda por tolerância de
// cor, e os cinzas do xadrez (#6e6d6c, #9e9e9c, #a0a19e) medem o mesmo valor das faces
// iluminadas do basalto: com tolerância 8 o preenchimento atravessou o contorno e removeu 50,7%
// da imagem, devolvendo um esqueleto. **Cor não separa fundo de objeto quando o fundo é cinza
// e o objeto é pedra cinza.**
//
// O que separa é a ESTRUTURA: o xadrez é um punhado de cores exatas que ocupam a primeira
// linha INTEIRA da imagem. Então as referências saem só da linha 0, a tolerância cai para perto
// do casamento exato, e a semeadura vem do topo e das laterais. Assim o preenchimento só anda
// por xadrez e para no contorno escuro do objeto.
const p = require('./png.js');

/**
 * @param tol tolerância por canal. 6 é o valor medido: separa os tons do xadrez das faces
 *            claras da rocha, que ficam a mais de 6 de distância em pelo menos um canal.
 * @param cercado liga a terceira passada, para objetos vazados (navio, cordame) — ver lá.
 * @returns `{ img, removido }` — `removido` é a fração do quadro que virou alpha.
 */
function removerXadrez(img, tol = 6, { cercado = false } = {}) {
  const { width: w, height: h } = img;
  const at = (i) => [img.data[i * 4], img.data[i * 4 + 1], img.data[i * 4 + 2]];

  const contagem = new Map();
  for (let x = 0; x < w; x++) { const k = at(x).join(); contagem.set(k, (contagem.get(k) || 0) + 1); }
  // 5% da primeira linha: pega os tons do xadrez e descarta ruído solto.
  const refs = [...contagem.entries()].filter(([, n]) => n / w >= 0.05)
    .map(([k]) => k.split(',').map(Number));
  if (refs.length === 0) { return { img, removido: 0 }; }

  const near = (i) => {
    const c = at(i);
    return refs.some(r => Math.abs(c[0] - r[0]) <= tol && Math.abs(c[1] - r[1]) <= tol &&
      Math.abs(c[2] - r[2]) <= tol);
  };

  const bg = new Uint8Array(w * h), pilha = [];
  for (let x = 0; x < w; x++) { if (near(x)) { bg[x] = 1; pilha.push(x); } }
  for (let y = 0; y < h; y++) {
    for (const x of [0, w - 1]) {
      const k = y * w + x;
      if (!bg[k] && near(k)) { bg[k] = 1; pilha.push(k); }
    }
  }
  while (pilha.length) {
    const k = pilha.pop(), x = k % w, y = (k - x) / w;
    for (const [dx, dy] of [[1, 0], [-1, 0], [0, 1], [0, -1]]) {
      const nx = x + dx, ny = y + dy;
      if (nx < 0 || ny < 0 || nx >= w || ny >= h) { continue; }
      const nk = ny * w + nx;
      if (bg[nk] || !near(nk)) { continue; }
      bg[nk] = 1; pilha.push(nk);
    }
  }

  // Segunda passada: ilhas de xadrez CERCADAS pelo objeto.
  //
  // Onde o xadrez encosta no contorno, sobram manchas que a borda não alcança — no templo eram
  // 67 pontinhos claros presos entre as colunas, e no tamanho da cena eles lêem como sujeira.
  // O critério combina DUAS condições, e nenhuma das duas sozinha serviria: a ilha tem de ser
  // pequena (senão apagaria detalhe legítimo) E ter a cor exata do xadrez (senão apagaria
  // detalhe pequeno que é do objeto, como uma folha solta ou um brilho).
  const ilha = new Uint8Array(w * h);
  let cercadas = 0;
  for (let i = 0; i < w * h; i++) {
    if (bg[i] || ilha[i] || img.data[i * 4 + 3] <= 8) { continue; }
    const grupo = [i];
    ilha[i] = 1;
    let doXadrez = 0;
    for (let k = 0; k < grupo.length && grupo.length <= LIMITE_DE_ILHA; k++) {
      const idx = grupo[k], x = idx % w, y = (idx - x) / w;
      if (near(idx)) { doXadrez++; }
      for (const [dx, dy] of [[1, 0], [-1, 0], [0, 1], [0, -1]]) {
        const nx = x + dx, ny = y + dy;
        if (nx < 0 || ny < 0 || nx >= w || ny >= h) { continue; }
        const nk = ny * w + nx;
        if (ilha[nk] || bg[nk] || img.data[nk * 4 + 3] <= 8) { continue; }
        ilha[nk] = 1; grupo.push(nk);
      }
    }
    // Maioria dos pixels na cor do xadrez, e pequena o bastante para não ser o objeto.
    if (grupo.length <= LIMITE_DE_ILHA && doXadrez * 2 >= grupo.length) {
      for (const idx of grupo) { bg[idx] = 1; }
      cercadas++;
    }
  }

  // Terceira passada, OPT-IN: xadrez cercado por objeto VAZADO.
  //
  // A segunda passada agrupa por conectividade com qualquer pixel opaco. Num navio isso falha:
  // o xadrez preso entre a vela, o mastro e o cordame encosta nas cordas, vira parte do mesmo
  // componente do navio, e nunca é tratado como ilha — sobraram manchas quadriculadas atrás da
  // vela do navio dos pretendentes, visíveis na cena.
  //
  // Esta passada anda SÓ por pixels que são ao mesmo tempo da cor do xadrez e cinza-neutros
  // (saturação < 0,06), e remove regiões com pelo menos 24 px. As duas condições protegem o
  // objeto: cor exata do xadrez em pixel colorido não entra, e cinza legítimo do objeto raramente
  // forma uma região de 24 px na cor exata de referência. É opt-in porque num asset de pedra
  // cinza — a parede de Cila — essa proteção não vale.
  if (cercado) {
    const neutro = (i) => {
      const r = img.data[i * 4], g = img.data[i * 4 + 1], b = img.data[i * 4 + 2];
      const mx = Math.max(r, g, b);
      return mx === 0 || (mx - Math.min(r, g, b)) / mx < 0.06;
    };
    const visto = new Uint8Array(w * h);
    for (let i = 0; i < w * h; i++) {
      if (bg[i] || visto[i] || img.data[i * 4 + 3] <= 8 || !near(i) || !neutro(i)) { continue; }
      const grupo = [i];
      visto[i] = 1;
      for (let k = 0; k < grupo.length; k++) {
        const idx = grupo[k], x = idx % w, y = (idx - x) / w;
        for (const [dx, dy] of [[1, 0], [-1, 0], [0, 1], [0, -1]]) {
          const nx = x + dx, ny = y + dy;
          if (nx < 0 || ny < 0 || nx >= w || ny >= h) { continue; }
          const nk = ny * w + nx;
          if (visto[nk] || bg[nk] || img.data[nk * 4 + 3] <= 8 || !near(nk) || !neutro(nk)) { continue; }
          visto[nk] = 1; grupo.push(nk);
        }
      }
      if (grupo.length >= 24) {
        for (const idx of grupo) { bg[idx] = 1; }
        cercadas++;
      }
    }
  }

  const out = p.blank(w, h);
  img.data.copy(out.data);
  let n = 0;
  for (let i = 0; i < w * h; i++) { if (bg[i]) { out.data[i * 4 + 3] = 0; n++; } }
  return { img: out, removido: n / (w * h), cercadas };
}

/**
 * Teto de tamanho para uma ilha ser considerada resto de xadrez. 400 px é maior que qualquer
 * mancha que o recorte deixa e menor que qualquer peça desenhada destes assets — o degrau mais
 * fino do templo tem mais de 1200.
 */
const LIMITE_DE_ILHA = 400;

/**
 * Remove ilhas opacas minúsculas soltas no vazio.
 *
 * Existe porque o `removerXadrez` deixa restos: onde o xadrez encosta no contorno do objeto,
 * alguns quadradinhos ficam CERCADOS por pixels que não casam a referência, e o flood-fill de
 * borda não os alcança. No templo isso apareceu como uma poeira de pontinhos claros ao redor
 * das colunas — invisível na miniatura e evidente quando o sprite entra na cena.
 *
 * O limiar é conservador de propósito: 20 pixels é menor que qualquer detalhe desenhado nestes
 * assets (a menor folha da oliveira tem 40 e poucos), e ilhas legítimas maiores que isso
 * sobrevivem. Um limiar generoso aqui apagaria detalhe de verdade.
 */
function tirarPoeira(img, minimo = 20) {
  const { width: w, height: h } = img;
  const visto = new Uint8Array(w * h);
  const out = p.blank(w, h);
  img.data.copy(out.data);
  let ilhas = 0;

  for (let i = 0; i < w * h; i++) {
    if (visto[i] || img.data[i * 4 + 3] <= 8) { continue; }
    const grupo = [i];
    visto[i] = 1;
    for (let k = 0; k < grupo.length; k++) {
      const idx = grupo[k], x = idx % w, y = (idx - x) / w;
      for (const [dx, dy] of [[1, 0], [-1, 0], [0, 1], [0, -1]]) {
        const nx = x + dx, ny = y + dy;
        if (nx < 0 || ny < 0 || nx >= w || ny >= h) { continue; }
        const nk = ny * w + nx;
        if (visto[nk] || img.data[nk * 4 + 3] <= 8) { continue; }
        visto[nk] = 1; grupo.push(nk);
      }
    }
    if (grupo.length < minimo) {
      for (const idx of grupo) { out.data[idx * 4 + 3] = 0; }
      ilhas++;
    }
  }
  return { img: out, ilhas };
}

/**
 * Diz se a imagem tem xadrez PINTADO, para o pipeline poder cobrar em vez de confiar no olho.
 * O sinal é o canto: um sprite com transparência de verdade tem canto transparente.
 */
function temXadrezPintado(img, lado = 24) {
  let opacos = 0;
  for (let y = 0; y < Math.min(lado, img.height); y++) {
    for (let x = 0; x < Math.min(lado, img.width); x++) {
      if (img.data[(y * img.width + x) * 4 + 3] > 128) { opacos++; }
    }
  }
  return opacos === Math.min(lado, img.height) * Math.min(lado, img.width);
}

if (require.main === module) {
  const [inF, outF, tolArg] = process.argv.slice(2);
  const src = p.read(inF);
  const { img, removido } = removerXadrez(src, tolArg ? +tolArg : undefined);
  p.write(outF, img);
  console.log(`${outF}  ${(removido * 100).toFixed(1)}% do quadro virou alpha`);
}

module.exports = { removerXadrez, tirarPoeira, temXadrezPintado };
