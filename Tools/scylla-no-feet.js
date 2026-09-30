// P-01 do FINAL_CHARACTER_ART_POLISH: tira os PÉS da Cila, sem gerar nada.
//
//   node Tools/scylla-no-feet.js            edita os quadros em Docs/Characters/Fase11/Scylla/{Idle,Strike}_east
//                                           (os originais ficam em <estado>_east_com_pes, só na primeira vez)
//   node Tools/build-cast-sheets.js         remonta CHR_Scylla
//
// O sprite é humanoide (o v3 só faz humanoide) e os pés cinza-ardósia apareciam abaixo das serpentes: ela lia
// "pairando" na boca da caverna. Sem pés, é torso de mulher + serpentes da cintura para baixo — a Cila do mito.
//
// Separação por COR, medida: pele/pés têm saturação <= 0,16; escamas e ventre das serpentes, >= 0,21. O corte
// só vale na faixa de baixo de cada quadro (os últimos FAIXA px do conteúdo), para não tocar rosto, braços e
// ombros, que usam a mesma pele. Depois, o contorno que ficou solto (sem vizinho colorido) sai também.
const fs = require('fs'), path = require('path');
const png = require('./png.js');

const DIR = path.join(__dirname, '..', 'Docs/Characters/Fase11/Scylla');
const FAIXA = 44;

function hsl(r, g, b) {
  r /= 255; g /= 255; b /= 255;
  const mx = Math.max(r, g, b), mn = Math.min(r, g, b), l = (mx + mn) / 2, d = mx - mn;
  if (!d) return [0, 0, l];
  return [0, l > 0.5 ? d / (2 - mx - mn) : d / (mx + mn), l];
}

function semPes(im) {
  const W = im.width, b = png.bounds(im), topo = b.y1 - FAIXA;
  const px = (x, y) => (y * W + x) * 4;
  const cor = (x, y) => { const i = px(x, y); return im.data[i + 3] < 8 ? null : hsl(im.data[i], im.data[i + 1], im.data[i + 2]); };
  let pele = 0, contorno = 0;
  for (let y = topo; y <= b.y1; y++) for (let x = b.x0; x <= b.x1; x++) {
    const c = cor(x, y);
    if (c && c[1] <= 0.18 && c[2] >= 0.25 && c[2] <= 0.85) { im.data[px(x, y) + 3] = 0; pele++; }
  }
  // Contorno solto: escuro, sem nenhum vizinho (8-conexo) que não seja escuro. Duas passadas.
  for (let k = 0; k < 2; k++) {
    const apagar = [];
    for (let y = topo; y <= b.y1; y++) for (let x = b.x0; x <= b.x1; x++) {
      const c = cor(x, y);
      if (!c || c[2] >= 0.14) continue;
      let colorido = false;
      for (let dy = -1; dy <= 1 && !colorido; dy++) for (let dx = -1; dx <= 1; dx++) {
        const v = (dx || dy) && x + dx >= 0 && y + dy >= 0 && x + dx < W && y + dy < im.height ? cor(x + dx, y + dy) : null;
        if (v && v[2] >= 0.14) { colorido = true; break; }
      }
      if (!colorido) apagar.push(px(x, y));
    }
    for (const i of apagar) { im.data[i + 3] = 0; contorno++; }
  }
  return { pele, contorno };
}

for (const estado of ['Idle_east', 'Strike_east']) {
  const dir = path.join(DIR, estado), bkp = dir.replace(/_east$/, '_east_com_pes');
  if (!fs.existsSync(bkp)) fs.cpSync(dir, bkp, { recursive: true });
  for (const f of fs.readdirSync(bkp).filter(f => f.endsWith('.png')).sort()) {
    const im = png.read(path.join(bkp, f));
    const r = semPes(im);
    png.write(path.join(dir, f), im);
    console.log(`${estado}/${f}: pele ${r.pele}, contorno ${r.contorno}, conteúdo agora ${JSON.stringify(png.bounds(im))}`);
  }
}
