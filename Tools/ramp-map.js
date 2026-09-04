// Remapeia a arte crua do PixelLab para a paleta oficial de Ítaca, por rampa.
//
// Por que não quantizar pelo vizinho mais próximo: as rampas de Ítaca são poucas e
// espaçadas, então duas cores vizinhas do original caem na mesma entrada e o objeto perde
// o contraste interno. Aqui cada material é classificado primeiro, e as cores dele são
// distribuídas ao longo dos 4 passos da rampa correspondente por luminância relativa —
// o degradê sobrevive.
const p = require('./png.js');
const fs = require('fs');

function loadPalette(gpl) {
  const ramps = {};
  for (const line of fs.readFileSync(gpl, 'utf8').split('\n')) {
    const m = line.match(/^\s*(\d+)\s+(\d+)\s+(\d+)\s+(.*?)\s*$/);
    if (!m) continue;
    const step = m[4].replace(/\s+(Highlight|Base|Sombra|Profunda)$/, '');
    (ramps[step] ||= []).push([+m[1], +m[2], +m[3]]);
  }
  return ramps; // cada rampa em ordem: Highlight, Base, Sombra, Profunda
}

const lum = (r, g, b) => 0.2126 * r + 0.7152 * g + 0.0722 * b;
const sat = (r, g, b) => { const mx = Math.max(r, g, b); return mx ? (mx - Math.min(r, g, b)) / mx : 0; };

// Classificação por material. Os limiares foram medidos nas cores reais da casa v3:
// a madeira mais apagada tem sat 0,42 e a pedra mais quente tem 0,31 — 0,36 separa com folga.
function material(r, g, b) {
  // Contorno é o quase-preto (< 25) mais o escuro DESSATURADO. Mandar todo lum < 40 para
  // Contorno funcionava em parede clara, mas num casco de madeira escura engolia o marrom
  // legítimo — a metade de baixo do casco virava contorno e o objeto perdia a profundidade.
  const L = lum(r, g, b);
  if (L < 25 || (L < 40 && sat(r, g, b) < 0.35)) return 'Contorno';
  // Só conta como vidro o que é francamente frio. Sem a margem, um cinza neutro como
  // #8c8c87 (g == r) entra aqui e sai verde-oliva.
  if (g - r >= 6 || b - r >= 6) return 'vidro';
  if (sat(r, g, b) >= 0.36) return 'Madeira';
  return 'Terra / caminho';                       // calcário claro e quente
}

// Classificador de CENÁRIO: vegetação, props, arsenal e porto. Separado do de arquitetura de
// propósito — o de arquitetura foi calibrado nas cores da casa e do navio e está verificado
// em 12 assets; alargá-lo para cobrir verde regrediria o vão da janela, que é um teal escuro
// a 0,205 de saturação, bem no meio da faixa onde folhagem e vidro se confundem.
function materialDeCenario(r, g, b) {
  const L = lum(r, g, b), S = sat(r, g, b);
  if (L < 25 || (L < 40 && S < 0.35)) return 'Contorno';

  if (g - r >= 6 || b - r >= 6) {
    if (b > g) return 'Pedra fria';                    // azulado: metal, sombra fria
    return S >= 0.22 ? 'Folhagem' : 'Oliva seca';      // verde vivo contra verde seco
  }

  if (S >= 0.42) {
    // Quente e saturado ainda é duas coisas: marrom de madeira e ocre de bronze, palha e
    // terracota. Mandar tudo para Madeira apagou o escudo e a ânfora na primeira passada.
    // O que separa é quanto o verde acompanha o vermelho: no amarelo ele acompanha quase
    // tudo, no marrom fica bem abaixo.
    const ocre = (g - b) / Math.max(1, r - b);
    return ocre >= 0.65 ? 'Oliva seca' : 'Madeira';
  }

  if (S >= 0.20) return 'Terra / caminho';             // areia, calcário, terracota clara
  return 'Pedra';                                      // cinza quente
}

function remap(img, ramps, opts = {}) {
  const classificar = opts.classify || material;
  const hex = (o) => (img.data[o] << 16) | (img.data[o + 1] << 8) | img.data[o + 2];
  // 1) Junta as cores únicas por material.
  const groups = {};
  for (let i = 0; i < img.width * img.height; i++) {
    const o = i * 4;
    if (img.data[o + 3] <= 8) continue;
    const [r, g, b] = [img.data[o], img.data[o + 1], img.data[o + 2]];
    const m = classificar(r, g, b);
    ((groups[m] ||= new Map())).set(hex(o), { r, g, b, n: ((groups[m].get(hex(o)) || {}).n || 0) + 1 });
  }
  // 2) Espalha cada grupo pelos passos da rampa, por luminância relativa dentro do grupo.
  const table = new Map();
  const report = [];
  for (const [m, colors] of Object.entries(groups)) {
    const rampName = m === 'vidro' ? (opts.glassRamp || 'Pedra fria') : m;
    // 'vidro' é a única classe que não nomeia uma rampa; as demais nomeiam.
    const ramp = ramps[rampName];
    if (!ramp) throw new Error(`paleta não cobre o material "${m}"`);
    // O vidro usa só a metade escura da rampa: um vão pequeno lê como interior escuro.
    const [lo, hi] = m === 'vidro' ? [2, 3] : [0, 3];
    const ls = [...colors.values()].map(c => lum(c.r, c.g, c.b));
    const min = Math.min(...ls), max = Math.max(...ls);
    for (const [key, c] of colors) {
      const t = max === min ? 1 : (lum(c.r, c.g, c.b) - min) / (max - min);
      const idx = lo + Math.round((1 - t) * (hi - lo));
      table.set(key, ramp[idx]);
      report.push({ m, from: '#' + key.toString(16).padStart(6, '0'), to: '#' + ramp[idx].map(v => v.toString(16).padStart(2, '0')).join(''), n: c.n });
    }
  }
  // 3) Aplica.
  const out = p.blank(img.width, img.height);
  img.data.copy(out.data);
  for (let i = 0; i < img.width * img.height; i++) {
    const o = i * 4;
    if (img.data[o + 3] <= 8) continue;
    const t = table.get(hex(o));
    out.data[o] = t[0]; out.data[o + 1] = t[1]; out.data[o + 2] = t[2]; out.data[o + 3] = 255;
  }
  if (opts.verbose) {
    report.sort((a, b) => a.m.localeCompare(b.m) || b.n - a.n);
    for (const r of report) console.log(`  ${r.m.padEnd(16)} ${r.from} -> ${r.to}  (${r.n}px)`);
    console.log(`  cores: ${table.size} -> ${new Set([...table.values()].map(v => v.join())).size}`);
  }
  return out;
}

// O corte entre madeira e pedra é PROPRIEDADE DO ASSET, não do pipeline. Na casa v3 a
// madeira mais apagada tem sat 0,42 e a pedra mais quente 0,31, então 0,36 separa. Na fachada
// do palácio a madeira da porta vai de 0,60 a 0,69 e o calcário de 0,356 a 0,43 — com 0,36
// metade da parede virava madeira e a pedra saía mosqueada de malva. Medir nos dois blocos
// (porta e parede) antes de escolher é parte do trabalho, não detalhe.
const materialArquitetura = (corteMadeira = 0.36) => (r, g, b) => {
  const L = lum(r, g, b);
  if (L < 25 || (L < 40 && sat(r, g, b) < 0.35)) return 'Contorno';
  if (g - r >= 6 || b - r >= 6) return 'vidro';
  if (sat(r, g, b) >= corteMadeira) return 'Madeira';
  return 'Terra / caminho';
};

module.exports = { loadPalette, remap, material, materialDeCenario, materialArquitetura };
