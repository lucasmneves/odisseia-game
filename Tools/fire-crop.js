// PXL-015: recorta a REGIÃO DA CHAMA de cada fonte de fogo existente, para animar só ela.
//   node Tools/fire-crop.js → Docs/Art/PixelLab/PXL-015/_ref/<nome>.png + _ref/recortes.json (posição na fonte)
const fs = require('fs'), path = require('path'), p = require('./png.js');
const ROOT = path.join(__dirname, '..'), OUT = path.join(ROOT, 'Docs/Art/PixelLab/PXL-015/_ref');
const FONTES = {
  torch_stand: 'Assets/Art/Environments/Pretendentes/VFX/pret_torch_stand.png',
  brazier: 'Assets/Art/Environments/ItacaReturn/Props/itaca_ret_brazier.png',
  cooking_fire: 'Assets/Art/Environments/Pretendentes/Courtyard/pret_cooking_fire.png',
  campfire_troy: 'Assets/Art/Environments/Troy/Camp/troy_campfire_01.png',
  hearth: null,                      // base + chama da Fase 16 compostas
};
function abrir(nome) {
  if (nome !== 'hearth') return p.read(path.join(ROOT, FONTES[nome]));
  const b = p.read(path.join(ROOT, 'Assets/Art/Environments/Final/Props/final_hearth_base.png'));
  const f = p.read(path.join(ROOT, 'Assets/Art/Environments/Final/Props/final_hearth_fire.png'));
  for (let i = 0; i < b.width * b.height; i++) if (f.data[i * 4 + 3]) f.data.copy(b.data, i * 4, i * 4, i * 4 + 4);
  return b;
}
const quente = (r, g, b) => { const mx = Math.max(r, g, b), mn = Math.min(r, g, b), S = mx ? (mx - mn) / mx : 0;
  if (S < 0.55 || mx < 150) return false; const d = mx - mn; const H = mx === r ? (((g - b) / d) + 6) % 6 * 60 : mx === g ? ((b - r) / d + 2) * 60 : 999; return H < 58 || H > 345; };
// Retângulos MEDIDOS à mão em cada fonte (x, y, w, h). A detecção automática por cor não serve: o suporte de
// bronze do braseiro, a carne da fogueira e o caldeirão de Troia também são quentes e saturados.
const RET = {
  torch_stand: [0, 0, 37, 36],
  brazier: [40, 0, 46, 62],
  cooking_fire: [62, 84, 68, 46],
  campfire_troy: [16, 50, 34, 28],
  hearth: [30, 0, 42, 50],
};
const rec = {};
for (const nome of Object.keys(FONTES)) {
  const img = abrir(nome);
  const [x0, y0, w, h] = RET[nome];
  const W = w + (w & 1), H = h + (h & 1);
  const canvas = p.blank(W, H);
  for (let y = 0; y < h; y++) img.data.copy(canvas.data, (y * W) * 4, ((y0 + y) * img.width + x0) * 4, ((y0 + y) * img.width + x0 + w) * 4);
  p.write(path.join(OUT, nome + '.png'), canvas);
  rec[nome] = { fonte: FONTES[nome] || 'Final/Props/final_hearth_base+fire', x0, y0, w, h };
  console.log(nome.padEnd(14), `${W}x${H} em (${x0},${y0})`);
}
fs.writeFileSync(path.join(OUT, 'recortes.json'), JSON.stringify(rec, null, 2));
