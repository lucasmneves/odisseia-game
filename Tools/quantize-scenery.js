// Traz os Grupos 4 a 8 e as camadas de fundo geradas para a paleta oficial.
//
// Eles foram gerados com o Conceito A como style_image, o que já os deixa na família certa
// (afinidade 100%, distância média de 15 a 37 ao vizinho mais próximo) — mas nenhuma cor deles
// bate exatamente com uma entrada da paleta, e a paleta fechada de 35 cores é a regra.
//
// Os originais vão para `_originais_sem_paleta/` de cada grupo, como já foi feito com os
// tilesets. Rodar de novo é seguro: a fonte é sempre o original preservado.
//
//   node Tools/quantize-scenery.js [--dry]
const fs = require('fs'), path = require('path');
const p = require('./png.js');
const { loadPalette, remap, materialDeCenario } = require('./ramp-map.js');

const ROOT = path.join(__dirname, '..');
const ENV = path.join(ROOT, 'Docs/Environment_Ithaca');
const ramps = loadPalette(path.join(ENV, 'Palette/ITHACA_PALETTE.gpl'));
const seco = process.argv.includes('--dry');

// Só o que nunca foi quantizado. Terrain, Architecture e Ships já estão na paleta e não
// entram aqui — o classificador de cenário não é o que calibrei para eles.
// As camadas de fundo (Layers) NÃO entram, e isso foi medido, não suposto: o classificador
// aqui é calibrado para props e vegetação, e os materiais do fundo são outros. Branco de
// nuvem cai em Pedra, que é marrom, e verde dessaturado de ilha distante cai em Oliva seca,
// que é amarela — as nuvens saíram marrons e a ilha amarela na tela. Céu e oceano já estão
// na paleta por construção (foram feitos por código), e as outras três têm 100% de afinidade.
// Quantizá-las exigiria um classificador com Céu, Água e Folhagem, calibrado à parte.
const ALVOS = [
  ['Nature', null], ['Props', null], ['Training', null], ['Arsenal', null], ['Port', null],
];

const oficiais = new Set();
const cores = [];
for (const ramp of Object.values(ramps)) {
  for (const c of ramp) {
    oficiais.add((c[0] << 16) | (c[1] << 8) | c[2]);
    cores.push(c);
  }
}

// A regra do projeto — "antes de quantizar, conferir se a paleta cobre os materiais do asset"
// — vira medida aqui em vez de julgamento: a maior distância de uma cor RELEVANTE (>= 1% dos
// pixels) até a entrada mais próxima da paleta. Material que a paleta não tem aparece como
// distância alta, e quantizar destrói o objeto em vez de aproximá-lo.
//
// O limiar de 90 saiu dos dados: a tocha mede 116 (a chama #fb7507 não tem vizinho na
// paleta, que não tem rampa de fogo), o segundo pior mede 77 e todo o resto fica em 63 ou
// menos. Não é um número redondo escolhido a esmo — é o vão entre os dois grupos.
const LIMIAR_DE_COBERTURA = 90;

function piorCorDescoberta(img) {
  const hist = new Map();
  let total = 0;
  for (let i = 0; i < img.width * img.height; i++) {
    const o = i * 4;
    if (img.data[o + 3] <= 8) continue;
    total++;
    const k = (img.data[o] << 16) | (img.data[o + 1] << 8) | img.data[o + 2];
    hist.set(k, (hist.get(k) || 0) + 1);
  }

  let pior = 0, qual = 0;
  for (const [k, n] of hist) {
    if (n / total < 0.01) continue;
    const c = [(k >> 16) & 255, (k >> 8) & 255, k & 255];
    let melhor = Infinity;
    for (const q of cores) {
      const v = Math.hypot(c[0] - q[0], c[1] - q[1], c[2] - q[2]);
      if (v < melhor) melhor = v;
    }
    if (melhor > pior) { pior = melhor; qual = k; }
  }
  return { pior, qual };
}

let feitos = 0;
for (const [grupo, filtro] of ALVOS) {
  const dir = path.join(ENV, grupo);
  const backup = path.join(dir, '_originais_sem_paleta');
  if (!seco) fs.mkdirSync(backup, { recursive: true });

  for (const f of fs.readdirSync(dir).sort()) {
    if (!f.endsWith('.png') || f.startsWith('_')) continue;
    if (filtro && !filtro.includes(f)) continue;

    // A fonte é sempre o original preservado, para requantizar não empilhar perdas.
    const guardado = path.join(backup, f);
    const origem = fs.existsSync(guardado) ? guardado : path.join(dir, f);
    const img = p.read(origem);

    const cobertura = piorCorDescoberta(img);
    if (cobertura.pior > LIMIAR_DE_COBERTURA) {
      const hex = '#' + cobertura.qual.toString(16).padStart(6, '0');
      console.log(`${(grupo + '/' + f).padEnd(46)} PULADO — ${hex} a ${cobertura.pior.toFixed(0)} da paleta`);
      // Se já tinha sido quantizado numa passada anterior, devolve o original.
      if (!seco && fs.existsSync(guardado)) fs.copyFileSync(guardado, path.join(dir, f));
      continue;
    }

    let antes = 0, total = 0;
    for (let i = 0; i < img.width * img.height; i++) {
      const o = i * 4;
      if (img.data[o + 3] <= 8) continue;
      total++;
      if (oficiais.has((img.data[o] << 16) | (img.data[o + 1] << 8) | img.data[o + 2])) antes++;
    }

    const saida = remap(img, ramps, { classify: materialDeCenario });
    const cores = new Set();
    for (let i = 0; i < saida.width * saida.height; i++) {
      const o = i * 4;
      if (saida.data[o + 3] > 8) cores.add((saida.data[o] << 16) | (saida.data[o + 1] << 8) | saida.data[o + 2]);
    }

    console.log(`${(grupo + '/' + f).padEnd(46)} ${((100 * antes) / total).toFixed(0).padStart(3)}% na paleta -> 100%, ${cores.size} cores`);

    if (!seco) {
      if (!fs.existsSync(guardado)) fs.copyFileSync(path.join(dir, f), guardado);
      p.write(path.join(dir, f), saida);
    }
    feitos++;
  }
}
console.log(`\n${feitos} assets${seco ? ' (simulação, nada gravado)' : ' quantizados; originais em <grupo>/_originais_sem_paleta/'}`);
