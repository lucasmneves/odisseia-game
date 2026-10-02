// Ajustes de quadro-chave de efeito ANTES de animar, sem gerar de novo.
//   node Tools/fx-prep.js  → Docs/Art/PixelLab/_prep/
//
// - slash: a fonte é uma meia-lua cuja cauda fina dá a volta e fecha um círculo. Ficam só as colunas da meia-lua
//   (x ≤ 25 de 48, medido: a cauda tem 1–2 px de espessura daí em diante) e o resultado é espelhado — o Odisseu
//   olha para a direita, e o golpe varre à frente dele.
// - block: o formato (clarão + leque de faíscas) está certo, mas o vermelho lia como fogo. Todo pixel quente
//   saturado vai para a rampa Bronze da paleta de Ítaca/Pretendentes, por luminância; branco e contorno ficam.
const path = require('path'), p = require('./png.js');
const { loadPalette } = require('./ramp-map.js');
const ROOT = path.join(__dirname, '..'), B = path.join(ROOT, 'Docs/Art/PixelLab');
const bronze = loadPalette(path.join(ROOT, 'Docs/Environment_Pretendentes/Palette/PRETENDENTES_PALETTE.gpl'))['Bronze'];

{
  const src = p.read(path.join(B, 'PXL-002/_candidatos/slash_a.png'));
  const out = p.blank(src.width, src.height);
  for (let y = 0; y < src.height; y++) for (let x = 0; x <= 25; x++) {
    const s = (y * src.width + x) * 4, d = (y * src.width + (src.width - 1 - x)) * 4;
    src.data.copy(out.data, d, s, s + 4);
  }
  p.write(path.join(B, '_prep/slash_key.png'), out);
}
{
  const src = p.read(path.join(B, 'PXL-003/_candidatos/block_b.png'));
  const lum = (r, g, b) => 0.2126 * r + 0.7152 * g + 0.0722 * b;
  let n = 0;
  for (let i = 0; i < src.width * src.height; i++) {
    const o = i * 4; if (!src.data[o + 3]) continue;
    const [r, g, b] = [src.data[o], src.data[o + 1], src.data[o + 2]];
    const mx = Math.max(r, g, b), mn = Math.min(r, g, b), S = mx ? (mx - mn) / mx : 0;
    if (S < 0.35 || r < g) continue;                         // branco, cinza e contorno ficam
    const L = lum(r, g, b), c = bronze[L > 170 ? 0 : L > 110 ? 1 : L > 60 ? 2 : 3];
    src.data[o] = c[0]; src.data[o + 1] = c[1]; src.data[o + 2] = c[2]; n++;
  }
  p.write(path.join(B, '_prep/block_key.png'), src);
  console.log(`block: ${n} px para a rampa Bronze`);
}
