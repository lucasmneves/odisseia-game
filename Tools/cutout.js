// Remove fundo chapado por flood-fill a partir das bordas.
//
// Existe por uma armadilha confirmada do PixelLab: `create_image_pixen` ignora
// `no_background` quando o prompt tem qualquer enquadramento de cena — devolve o objeto certo
// sobre um fundo opaco. Como o fundo sai chapado, recortar por código é exato e de graça.
//
// Flood-fill a partir da borda, e não "remover todo pixel dessa cor": o que está cercado pelo
// objeto (o interior de uma argola de remo, por exemplo) fica preservado mesmo se a cor bater.
//
//   node Tools/cutout.js <entrada.png> <saida.png> [tolerância]
const p = require('./png.js');

// enclosed: limpa também as manchas da cor de fundo que ficaram CERCADAS pelo objeto — o
// vazado da espiral de uma roda de proa, por exemplo. O flood-fill de borda não as alcança,
// e deixá-las opacas põe um borrão da cor do fundo dentro da silhueta. Só vale quando a cor
// de fundo não é uma cor legítima do objeto, que é o caso aqui (verde-acinzentado num casco
// de madeira).
function cutout(img, tol = 14, { enclosed = false } = {}) {
  const { width: w, height: h } = img;
  const at = (i) => [img.data[i * 4], img.data[i * 4 + 1], img.data[i * 4 + 2]];
  const seeds = [];
  for (let x = 0; x < w; x++) { seeds.push(x); seeds.push((h - 1) * w + x); }
  for (let y = 0; y < h; y++) { seeds.push(y * w); seeds.push(y * w + w - 1); }

  // Referência: TODAS as cores de borda com peso, não só a mais comum.
  //
  // O PixelLab às vezes devolve o fundo como xadrez de transparência desenhado — dois tons
  // alternados. Com uma referência só, o outro tom sobra como quadradinhos opacos espalhados
  // pela imagem, que depois a quantização transforma em manchas claras. No armazém os tons
  // iam de (90,90,91) a (108,111,112): fora da tolerância um do outro.
  const tally = new Map();
  for (const s of seeds) { const k = at(s).join(); tally.set(k, (tally.get(k) || 0) + 1); }
  const refs = [...tally.entries()]
    .filter(([, n]) => n / seeds.length >= 0.02)
    .map(([k]) => k.split(',').map(Number));
  const ref = refs[0];

  const near = (i) => {
    const c = at(i);
    return refs.some(r =>
      Math.abs(c[0] - r[0]) <= tol && Math.abs(c[1] - r[1]) <= tol && Math.abs(c[2] - r[2]) <= tol);
  };

  const bg = new Uint8Array(w * h);
  const stack = seeds.filter(s => near(s));
  for (const s of stack) bg[s] = 1;
  while (stack.length) {
    const k = stack.pop(), x = k % w, y = (k - x) / w;
    for (const [dx, dy] of [[1, 0], [-1, 0], [0, 1], [0, -1]]) {
      const nx = x + dx, ny = y + dy;
      if (nx < 0 || ny < 0 || nx >= w || ny >= h) continue;
      const nk = ny * w + nx;
      if (bg[nk] || !near(nk)) continue;
      bg[nk] = 1; stack.push(nk);
    }
  }

  let trapped = 0;
  if (enclosed) for (let i = 0; i < w * h; i++) if (!bg[i] && near(i)) { bg[i] = 1; trapped++; }

  let removed = 0;
  for (let i = 0; i < w * h; i++) if (bg[i]) removed++;

  // Guarda pelo SINTOMA, não pela causa. Quando o objeto sangra até as bordas, as cores dele
  // entram na lista de referência, o flood-fill escapa para dentro e come a imagem — foi o que
  // aconteceu com uma seção de muralha que preenchia o quadro, sem erro nenhum no caminho.
  //
  // Tentei detectar pela borda e pelo miolo, e as duas medidas reprovavam o casco do navio,
  // que é um recorte legítimo com 75% de fundo. O que separa os casos é quanto sobra: a
  // muralha perdia 92% e o casco perde 75%. O limiar fica em 85%, e a mensagem diz o que fazer
  // — asset que sangra até a borda é textura, não recorte.
  const fatia = removed / (w * h);
  if (fatia > 0.85) {
    throw new Error(`o recorte removeria ${(100 * fatia).toFixed(0)}% da imagem — o objeto ` +
      'provavelmente sangra até as bordas; trate-o como textura, sem cutout');
  }

  const out = p.blank(w, h);
  img.data.copy(out.data);
  for (let i = 0; i < w * h; i++) if (bg[i]) out.data[i * 4 + 3] = 0;
  return { img: out, removed, trapped, ref };
}

if (require.main === module) {
  const args = process.argv.slice(2);
  const enclosed = args.includes('--enclosed');
  const [inF, outF, tolArg] = args.filter(a => a !== '--enclosed');
  const src = p.read(inF);
  const { img, removed, trapped, ref } = cutout(src, tolArg ? +tolArg : undefined, { enclosed });
  const b = p.bounds(img);
  p.write(outF, b ? p.crop(img, b.x0, b.y0, b.w, b.h) : img);
  const UN = 42.857143;
  console.log(`fundo #${ref.map(v => v.toString(16).padStart(2, '0')).join('')} — ${(100 * removed / (src.width * src.height)).toFixed(1)}% removido${trapped ? `, ${trapped}px cercados` : ''}`);
  console.log(`${outF}  ${b.w}x${b.h}px = ${(b.w / UN).toFixed(2)} x ${(b.h / UN).toFixed(2)} un`);
}
module.exports = { cutout };
