// SUN CATTLE MASTER v1 -> sprites estáticos da Fase 12 (Docs/Characters/Fase12/SUN_CATTLE_CAST.md).
//
//   node Tools/build-sun-cattle.js
//
// O gado não tem estado de jogo e não ganhou animação: uma pose só, a rotação de perfil (east) do master,
// recortada no conteúdo com os cascos próximos na última linha — o pivô (0,5; 0) do .meta cai no chão.
// A pose "pastando" e o bezerro foram cortados por economia (decisão de 2026-09-30): o rebanho varia por
// pelagem e espelhamento. A variação de pelagem é troca por RAMPA, não por matiz: o creme mora em 35–50°,
// a mesma faixa do chifre, então girar matiz pintaria tudo junto (ver cast-variants.js).
const fs = require('fs'), path = require('path'), crypto = require('crypto');
const png = require('./png.js');

const ROOT = path.join(__dirname, '..');
const SRC = path.join(ROOT, 'Docs/Characters/Fase12/Sun_Cattle');
const OUT = path.join(ROOT, 'Assets/Art/Environments/GadoDoSol/SacredCattle');
const MOLDE = path.join(OUT, 'gado_cattle_idle.png.meta');


function recortar(img) {
  const b = png.bounds(img, 8);
  return png.crop(img, b.x0, b.y0, b.w, b.h);
}

// GUID derivado do caminho: re-rodar não quebra referência de cena.
function meta(arquivoPng) {
  const rel = path.relative(ROOT, arquivoPng).replace(/\\/g, '/');
  const guid = crypto.createHash('md5').update(rel).digest('hex');
  const m = fs.readFileSync(MOLDE, 'utf8').replace(/^guid: \w+$/m, 'guid: ' + guid);
  fs.writeFileSync(arquivoPng + '.meta', m);
}

function gravar(nome, img) {
  const f = path.join(OUT, nome + '.png');
  png.write(f, img);
  meta(f);
  console.log(`${nome}: ${img.width} x ${img.height} px = ${(img.width / 42.857).toFixed(2)} x ${(img.height / 42.857).toFixed(2)} un`);
}

// Pelagem ocre: só os pixels que são pelo (claros, quentes, pouco saturados) descem um degrau
// para o ouro-ocre, mantendo a luminância relativa. Chifre, casco, olho e contorno ficam.
function ocre(img, eDoPelo) {
  const out = png.blank(img.width, img.height);
  img.data.copy(out.data);
  for (let i = 0; i < out.data.length; i += 4) {
    if (out.data[i + 3] < 8) continue;
    const [r, g, b] = [out.data[i], out.data[i + 1], out.data[i + 2]];
    if (!eDoPelo(r, g, b, (i / 4) % img.width, Math.floor(i / 4 / img.width))) continue;
    out.data[i] = Math.round(r * 0.93);
    out.data[i + 1] = Math.round(g * 0.78);
    out.data[i + 2] = Math.round(b * 0.58);
  }
  return out;
}

module.exports = { recortar, gravar, ocre };

if (require.main === module) {
  const img = recortar(png.read(path.join(SRC, 'rot_east.png')));
  gravar('sun_cattle_idle', img);
  // Os chifres ficam marfim: tudo acima da cernelha (linha 15 do recorte) é chifre e não troca de pelagem.
  const cernelha = 15;
  gravar('sun_cattle_idle_ochre', ocre(img, (r, g, b, x, y) => {
    const mx = Math.max(r, g, b), mn = Math.min(r, g, b), l = (mx + mn) / 510;
    // Saturação HSV: a HSL de um creme quase branco passa de 0,9 e deixaria o pelo claro de fora.
    const sat = mx === 0 ? 0 : (mx - mn) / mx;
    return y >= cernelha && l >= 0.45 && sat < 0.5 && r >= g && g >= b;
  }));
}
