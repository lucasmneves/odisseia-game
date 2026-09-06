// Encaixa um estado novo do Odisseu no formato da folha mestra.
//
//   node Tools/fit-character-state.js <pasta_origem> <pasta_destino>
//
// O `animate_character` em modo template devolve os frames no canvas nativo do personagem
// (64×64), não no canvas 84×84 dos estados já integrados, e com a cor levemente fora da
// paleta oficial. Este script faz as três correções que a folha exige:
//
// 1. **Paleta** — vizinho mais próximo entre as 28 cores oficiais. É seguro AQUI porque a
//    deriva medida é pequena (distância média ~12); a mesma operação estragou os aldeões,
//    cuja túnica estava a 122-154 de qualquer entrada e caiu toda no mostarda. Medir a
//    distância antes de quantizar é parte do procedimento, não zelo extra.
//
// 2. **Linha dos pés em y=71** — é de onde sai o pivô {0,5; 0,142857} da folha. O
//    deslocamento é o MESMO para todos os frames do ciclo, calculado pelo frame de apoio
//    (o de pé mais baixo). Alinhar cada frame pelo próprio pé colaria o personagem no chão
//    e apagaria justamente a fase aérea da corrida.
//
// 3. **Centro da cabeça em x=42** — a cabeça é o ponto que menos balança. Usar o retângulo
//    do sprite inteiro enviesaria para trás, porque a capa esvoaça atrás do corpo e estica
//    o retângulo de um lado só. Também aqui o deslocamento é único para o ciclo, para o
//    corpo não parar de avançar dentro da passada.
const fs = require('fs'), path = require('path');
const p = require('./png.js');
const { loadPalette } = require('./ramp-map.js');

const ORIG = process.argv[2], DEST = process.argv[3];
const CEL = 84, PE = 71, CABECA_X = 42;

const ramps = loadPalette(path.join(__dirname, '..', 'Docs/CharacterMaster_Odysseus/Palette/ODYSSEUS_PALETTE.gpl'));
const CORES = [];
for (const k of Object.keys(ramps)) for (const c of ramps[k]) CORES.push(c);

const arqs = fs.readdirSync(ORIG).filter(n => n.endsWith('.png')).sort();
const imgs = arqs.map(n => p.read(path.join(ORIG, n)));

const opaco = (im, x, y) => im.data[(y * im.width + x) * 4 + 3] > 8;

function metricas(im) {
  let topo = 1e9, pe = -1;
  for (let y = 0; y < im.height; y++) for (let x = 0; x < im.width; x++) {
    if (!opaco(im, x, y)) continue;
    if (y < topo) topo = y;
    if (y > pe) pe = y;
  }
  let soma = 0, n = 0;
  for (let y = topo; y < topo + 12 && y < im.height; y++)
    for (let x = 0; x < im.width; x++) if (opaco(im, x, y)) { soma += x; n++; }
  return { topo, pe, cabecaX: soma / n };
}

const m = imgs.map(metricas);
const dy = PE - Math.max(...m.map(v => v.pe));
const dx = Math.round(CABECA_X - m.reduce((a, v) => a + v.cabecaX, 0) / m.length);

fs.mkdirSync(DEST, { recursive: true });
let trocados = 0, total = 0, pior = 0;

imgs.forEach((im, k) => {
  const out = p.blank(CEL, CEL);
  for (let y = 0; y < im.height; y++) {
    for (let x = 0; x < im.width; x++) {
      const o = (y * im.width + x) * 4;
      if (im.data[o + 3] < 8) continue;
      const X = x + dx, Y = y + dy;
      if (X < 0 || Y < 0 || X >= CEL || Y >= CEL) {
        throw new Error(`frame ${k}: pixel sai da celula em (${X},${Y}) — folga do canvas insuficiente`);
      }

      const r = im.data[o], g = im.data[o + 1], b = im.data[o + 2];
      let melhor = null, dist = Infinity;
      for (const c of CORES) {
        const d = (r - c[0]) ** 2 + (g - c[1]) ** 2 + (b - c[2]) ** 2;
        if (d < dist) { dist = d; melhor = c; }
      }
      dist = Math.sqrt(dist);
      total++; if (dist > 0.5) trocados++; if (dist > pior) pior = dist;

      const q = (Y * CEL + X) * 4;
      out.data[q] = melhor[0]; out.data[q + 1] = melhor[1]; out.data[q + 2] = melhor[2];
      out.data[q + 3] = 255;   // alpha duro: a folha não tem um único pixel semitransparente
    }
  }
  p.write(path.join(DEST, `CHR_Odysseus_Run_${String(k).padStart(2, '0')}.png`), out);
});

const fin = fs.readdirSync(DEST).filter(n => n.endsWith('.png')).sort()
  .map(n => metricas(p.read(path.join(DEST, n))));
console.log(`${imgs.length} frames ${imgs[0].width}px -> ${CEL}px, deslocamento (${dx}, ${dy})`);
console.log(`  pes em y: ${fin.map(v => v.pe).join(' ')}  (apoio deve bater ${PE})`);
console.log(`  topo em y: ${fin.map(v => v.topo).join(' ')}`);
console.log(`  cabeca em x: ${fin.map(v => v.cabecaX.toFixed(1)).join(' ')}  (alvo ${CABECA_X})`);
console.log(`  paleta: ${trocados} de ${total} pixels trocados, maior correcao ${pior.toFixed(0)}`);
