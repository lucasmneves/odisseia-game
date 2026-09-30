// Variações de figurante por troca de cor — o "NPC design system" na prática: um arquétipo
// gerado vira vários personagens sem gastar geração. Lido por build-cast-sheets.js.
//
// A regra que decide ONDE dá para variar é medida, não escolhida: a troca gira o matiz de uma
// faixa e só funciona se essa faixa for exclusiva da roupa. Medido nos arquétipos da Fase 01:
//   pele, cabelo castanho, linho cru, couro e túnica marrom caem todos em 0–30 graus
//   -> Lavrador, Pescador e Soldado NÃO têm variação por cor: pintaria o rosto junto.
//   túnica do Marinheiro 165–215 graus e manto do Ancião 210–240 graus são exclusivos
//   -> esses dois variam.
// O contorno (luminância < 0,1) e os tons neutros (saturação < 0,12) nunca mudam, e a
// luminância de cada pixel é preservada: a rampa de sombra da roupa continua a mesma.

function hsl(r, g, b) {
  r /= 255; g /= 255; b /= 255;
  const mx = Math.max(r, g, b), mn = Math.min(r, g, b), l = (mx + mn) / 2;
  let h = 0, s = 0;
  if (mx !== mn) {
    const d = mx - mn;
    s = l > 0.5 ? d / (2 - mx - mn) : d / (mx + mn);
    h = mx === r ? (g - b) / d + (g < b ? 6 : 0) : mx === g ? (b - r) / d + 2 : (r - g) / d + 4;
    h *= 60;
  }
  return [h, s, l];
}

function rgb(h, s, l) {
  const c = (1 - Math.abs(2 * l - 1)) * s, x = c * (1 - Math.abs((h / 60) % 2 - 1)), m = l - c / 2;
  const [r, g, b] = h < 60 ? [c, x, 0] : h < 120 ? [x, c, 0] : h < 180 ? [0, c, x]
    : h < 240 ? [0, x, c] : h < 300 ? [x, 0, c] : [c, 0, x];
  return [r, g, b].map(v => Math.round((v + m) * 255));
}

// Gira o matiz da faixa [de0, de1] para `para`, multiplicando a saturação por `sat`.
function trocar(de0, de1, para, sat) {
  return img => {
    const out = { width: img.width, height: img.height, data: Buffer.from(img.data) };
    for (let i = 0; i < out.data.length; i += 4) {
      if (!out.data[i + 3]) continue;
      const [h, s, l] = hsl(out.data[i], out.data[i + 1], out.data[i + 2]);
      if (l < 0.1 || s < 0.12 || h < de0 || h > de1) continue;
      const [r, g, b] = rgb(para, Math.min(1, s * sat), l);
      out.data[i] = r; out.data[i + 1] = g; out.data[i + 2] = b;
    }
    return out;
  };
}

module.exports = [
  { de: 'Villager_Sailor', nome: 'Villager_Sailor_Ochre', nota: 'túnica azul -> ocre desbotado (remador)',
    aplicar: trocar(160, 220, 28, 0.9) },
  { de: 'Villager_Sailor', nome: 'Villager_Sailor_Olive', nota: 'túnica azul -> verde-oliva (o mais novo da tripulação)',
    aplicar: trocar(160, 220, 75, 0.8) },
  { de: 'Villager_Elder', nome: 'Villager_Elder_Brown', nota: 'manto azul -> castanho (artesão)',
    aplicar: trocar(200, 250, 25, 0.75) },
  // Fase 10: penas e cabelo turquesa/verde-mar (≈150–200°) são exclusivos — a pele fica intacta.
  { de: 'Siren', nome: 'Siren_Violet', nota: 'penas e cabelo turquesa -> violeta suave',
    aplicar: trocar(140, 210, 268, 0.8) },
  { de: 'Siren', nome: 'Siren_Sea', nota: 'penas e cabelo turquesa -> azul-mar',
    aplicar: trocar(140, 210, 205, 1.0) },
];
