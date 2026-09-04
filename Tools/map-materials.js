// Mapeia as regiões aproveitáveis do corpo da v3: onde é parede limpa, onde está a porta
// e onde está a janela. Serve para recortar material sem chutar coordenadas.
const path = require('path'), p = require('./png.js');
const { loadPalette } = require('./ramp-map.js');
const ROOT = path.join(__dirname, '..'), ENV = path.join(ROOT, 'Docs/Environment_Ithaca');

const src = p.read(path.join(ENV, 'Architecture/_fonte_v3_telha_inclinada.png'));
const body = p.crop(src, 0, 58, src.width, 176 - 58 + 1);
const bb = p.bounds(body);
console.log(`corpo ${body.width}x${body.height}, conteúdo x[${bb.x0}..${bb.x1}] y[${bb.y0}..${bb.y1}]`);

// Componentes conexas por classe de material, para achar porta e janela sem número mágico.
const { material } = require('./ramp-map.js');
function components(img, want) {
  const seen = new Uint8Array(img.width * img.height), out = [];
  const ok = (k) => {
    const o = k * 4;
    return img.data[o + 3] > 8 && material(img.data[o], img.data[o + 1], img.data[o + 2]) === want;
  };
  for (let i = 0; i < seen.length; i++) {
    if (seen[i] || !ok(i)) continue;
    const st = [i]; seen[i] = 1;
    const c = { n: 0, x0: 1e9, y0: 1e9, x1: -1, y1: -1 };
    while (st.length) {
      const k = st.pop(), x = k % img.width, y = (k - x) / img.width;
      c.n++;
      if (x < c.x0) c.x0 = x; if (x > c.x1) c.x1 = x;
      if (y < c.y0) c.y0 = y; if (y > c.y1) c.y1 = y;
      for (const [dx, dy] of [[1, 0], [-1, 0], [0, 1], [0, -1]]) {
        const nx = x + dx, ny = y + dy;
        if (nx < 0 || ny < 0 || nx >= img.width || ny >= img.height) continue;
        const nk = ny * img.width + nx;
        if (!seen[nk] && ok(nk)) { seen[nk] = 1; st.push(nk); }
      }
    }
    out.push(c);
  }
  return out.sort((a, b) => b.n - a.n);
}

for (const want of ['Madeira', 'vidro']) {
  console.log(`\n${want}:`);
  for (const c of components(body, want).slice(0, 5))
    console.log(`  ${String(c.x1-c.x0+1).padStart(3)}x${String(c.y1-c.y0+1).padStart(3)} em (${c.x0},${c.y0})  ${c.n}px`);
}

// Colunas totalmente de parede (sem madeira nem vidro em nenhuma linha) — material limpo.
const clean = [];
// Varre só a faixa útil (fora do contorno de topo e de base da parede, que atravessa
// todas as colunas e faria nenhuma coluna passar).
const [SY0, SY1] = [14, 99];
for (let x = bb.x0; x <= bb.x1; x++) {
  let pure = true;
  for (let y = SY0; y <= SY1 && pure; y++) {
    const o = (y * body.width + x) * 4;
    if (body.data[o + 3] <= 8) continue;
    const m = material(body.data[o], body.data[o + 1], body.data[o + 2]);
    // Contorno conta: uma sobra de contorno na faixa vira listra preta ao espelhar.
    if (m === 'Madeira' || m === 'vidro' || m === 'Contorno') pure = false;
  }
  clean.push(pure);
}
const runs = [];
let s = -1;
for (let i = 0; i <= clean.length; i++) {
  if (clean[i] && s < 0) s = i;
  else if (!clean[i] && s >= 0) { runs.push([bb.x0 + s, bb.x0 + i - 1]); s = -1; }
}
console.log('\ncolunas de parede pura (x inicial..final, largura):');
for (const [a, b] of runs) console.log(`  ${a}..${b}  (${b - a + 1}px)`);
