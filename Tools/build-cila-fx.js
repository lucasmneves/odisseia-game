// Constrói a folha de quadros de Caribdis e a põe em Resources com o .meta pronto.
//
//   node Tools/build-cila-fx.js
//
// ## Por que a animação é feita aqui e não gerada
//
// Caribdis é o maior elemento visual da fase e um redemoinho parado seria a fase inteira
// falhando no que ela promete. Mas gerar um ciclo de animação para um asset de 512x256 sairia
// caro e voltaria com quadros que não casam entre si.
//
// O redemoinho tem uma propriedade que a maioria dos assets não tem: ele é RADIALMENTE
// SIMÉTRICO. Então girar não precisa de quadros novos — precisa de reamostrar o mesmo quadro
// em coordenadas polares, deslocando o ângulo. Oito quadros saem do único asset gerado, com
// custo zero e emenda perfeita no ciclo (o oitavo volta exatamente ao primeiro, porque o
// deslocamento total é 2π).
//
// A amostragem é por VIZINHO MAIS PRÓXIMO, e isso não é economia: interpolar reamostraria a
// pixel art com meios-tons e produziria exatamente o anti-aliasing que a direção de arte
// proíbe. O custo é um pouco de serrilha nos anéis, que em movimento não se vê.
//
// A reprodução usa o `SpriteAnimator` que já existe — nenhum sistema novo, nenhum
// AnimatorController, nenhum shader. É o que a seção de performance do briefing pede.
const path = require('path'), fs = require('fs');
const p = require('./png.js');
const { textureMeta, ensureFolder, ALIGN } = require('./unity-import.js');

const ROOT = path.join(__dirname, '..');
const ENV = path.join(ROOT, 'Docs/Environment_CilaCaribdis');
const RES = path.join(ROOT, 'Assets/Resources/Odisseia/Environments');
const UNITY = 'Assets/Resources/Odisseia/Environments/FX_Charybdis.png';

const QUADROS = 8;
// Densidade nativa (P-02 do FINAL_CHARACTER_ART_POLISH): a cena mostrava a folha a 0,42 no Transform —
// pixels a 0,42 do tamanho dos do resto do jogo. Agora cada quadro é girado no tamanho do asset gerado e
// REAMOSTRADO por moda (Tools/downscale-native.js) para 0,42: a folha já sai no tamanho de tela, e a escala que o
// CilaCaribdisSceneDresser calcula pela largura do marcador cai para ~1. Gameplay (TidalHazard/KillZone) intocado.
const ESCALA_NATIVA = 0.42;
const { moda } = require('./downscale-native.js');
const COLUNAS = 4;

const funil = p.read(path.join(ENV, 'Charybdis/cila_charybdis_funnel.png'));
const W = funil.width, H = funil.height;
const cx = W / 2, cy = H / 2, rx = W / 2, ry = H / 2;

/**
 * Gira o funil por `fase` radianos em coordenadas ELÍPTICAS.
 *
 * Girar em coordenadas circulares num asset de 512x256 achataria os anéis — o redemoinho é uma
 * elipse vista de cima em ângulo raso, e a rotação tem de acontecer no plano dele, não no
 * plano da tela.
 */
function girar(fase) {
  const out = p.blank(W, H);
  for (let y = 0; y < H; y++) {
    for (let x = 0; x < W; x++) {
      const u = (x - cx) / rx, v = (y - cy) / ry;
      const r = Math.hypot(u, v), ang = Math.atan2(v, u) + fase;
      const sx = Math.round(cx + r * Math.cos(ang) * rx);
      const sy = Math.round(cy + r * Math.sin(ang) * ry);
      const o = (y * W + x) * 4;
      if (sx < 0 || sy < 0 || sx >= W || sy >= H) { continue; }
      const so = (sy * W + sx) * 4;
      out.data[o] = funil.data[so]; out.data[o + 1] = funil.data[so + 1];
      out.data[o + 2] = funil.data[so + 2]; out.data[o + 3] = funil.data[so + 3];
    }
  }
  return out;
}

const QW = Math.round(W * ESCALA_NATIVA), QH = Math.round(H * ESCALA_NATIVA);
const linhas = Math.ceil(QUADROS / COLUNAS);
const folha = p.blank(QW * COLUNAS, QH * linhas);
const fatias = [];
for (let i = 0; i < QUADROS; i++) {
  const col = i % COLUNAS, lin = Math.floor(i / COLUNAS);
  p.blit(folha, moda(girar((i / QUADROS) * Math.PI * 2), ESCALA_NATIVA), col * QW, lin * QH);
  fatias.push({
    // O nome tem de seguir <Folha>_<Estado>_<NN>: é assim que o SpriteAnimator descobre os
    // clipes por Resources.LoadAll, sem nada serializado que um refatiamento possa quebrar.
    name: `FX_Charybdis_Spin_${String(i).padStart(2, '0')}`,
    // O rect do Unity tem origem embaixo à esquerda; a folha é montada de cima para baixo.
    x: col * QW, y: folha.height - (lin + 1) * QH, w: QW, h: QH,
  });
}

ensureFolder(path.join(ROOT, 'Assets/Resources'), 'Assets/Resources');
ensureFolder(path.join(ROOT, 'Assets/Resources/Odisseia'), 'Assets/Resources/Odisseia');
ensureFolder(RES, 'Assets/Resources/Odisseia/Environments');
p.write(path.join(RES, 'FX_Charybdis.png'), folha);
fs.writeFileSync(path.join(RES, 'FX_Charybdis.png.meta'),
  // Pivô no CENTRO: o redemoinho gira em torno do próprio eixo, e um pivô na base o faria
  // pivotar pela borda de baixo. É o oposto do padrão do projeto, e é deliberado.
  textureMeta(UNITY, folha, { align: ALIGN.center, slice: fatias, fullRect: true }));

console.log(`FX_Charybdis.png  ${folha.width}x${folha.height}  ${QUADROS} quadros de ${QW}x${QH}`);
console.log(`  estado "Spin", ${fatias[0].name} .. ${fatias[QUADROS - 1].name}`);
