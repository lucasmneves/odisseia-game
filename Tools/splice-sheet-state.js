// Troca UM estado dentro da folha mestra do Odisseu, sem remontar a folha inteira.
//
//   node Tools/splice-sheet-state.js <Estado> <pasta_com_os_frames_84px>
//
// Remontar a folha do zero significa reescrever o `.meta` inteiro — e é lá que moram o GUID
// do asset e o `internalID` de cada sprite, que são o que o `Player.prefab` referencia. Um
// erro ali aparece como sprite *missing* no Inspector, não como erro de compilação. Este
// script troca só as células da linha do estado e mexe no `.meta` apenas nas entradas dos
// frames que deixaram de existir; os outros 90+ sprites saem byte a byte idênticos.
//
// A linha do estado e o tamanho da célula são LIDOS do `.meta`, não constantes daqui: um
// probe com a geometria copiada à mão mente no dia em que a folha muda.
const fs = require('fs'), path = require('path');
const p = require('./png.js');

const ESTADO = process.argv[2], ORIGEM = process.argv[3];
const ROOT = path.join(__dirname, '..');
const FOLHA = path.join(ROOT, 'Assets/Resources/Odisseia/Characters/CHR_Odysseus.png');
const META = FOLHA + '.meta';

const meta = fs.readFileSync(META, 'utf8');
const linhas = meta.split('\n');

// --- onde ficam os frames deste estado, segundo o próprio .meta ---
const atuais = [];
for (let i = 0; i < linhas.length; i++) {
  const m = linhas[i].match(new RegExp('^\\s+name: (CHR_Odysseus_' + ESTADO + '_(\\d+))\\s*$'));
  if (!m) continue;
  const bloco = linhas.slice(i, i + 8).join('\n');
  atuais.push({
    nome: m[1],
    indice: +m[2],
    x: +bloco.match(/\n\s+x: (\d+)/)[1],
    y: +bloco.match(/\n\s+y: (\d+)/)[1],
    w: +bloco.match(/\n\s+width: (\d+)/)[1],
    h: +bloco.match(/\n\s+height: (\d+)/)[1],
  });
}
if (!atuais.length) throw new Error(`nenhum sprite chamado CHR_Odysseus_${ESTADO}_NN no .meta`);

const CEL = atuais[0].w;
const yUnity = atuais[0].y;
if (atuais.some(a => a.y !== yUnity || a.w !== CEL || a.h !== CEL)) {
  throw new Error(`os frames de ${ESTADO} não estão numa linha uniforme — este script não serve`);
}

const folha = p.read(FOLHA);
// O rect do Unity conta de baixo para cima; o PNG, de cima para baixo.
const yTopo = folha.height - yUnity - CEL;

const arqs = fs.readdirSync(ORIGEM).filter(n => n.endsWith('.png')).sort();
const novos = arqs.map(n => p.read(path.join(ORIGEM, n)));
if (novos.some(im => im.width !== CEL || im.height !== CEL)) {
  throw new Error(`os frames de origem precisam ser ${CEL}x${CEL} — use Tools/fit-character-state.js antes`);
}
if (novos.length > atuais.length) {
  throw new Error(`${novos.length} frames não cabem na linha de ${atuais.length} células`);
}

// --- pixels: limpa a linha inteira e escreve os frames novos ---
for (let y = yTopo; y < yTopo + CEL; y++) {
  for (let x = 0; x < folha.width; x++) {
    const o = (y * folha.width + x) * 4;
    folha.data[o] = 0; folha.data[o + 1] = 0; folha.data[o + 2] = 0; folha.data[o + 3] = 0;
  }
}
novos.forEach((im, k) => {
  const x0 = atuais[k].x;
  for (let y = 0; y < CEL; y++) {
    for (let x = 0; x < CEL; x++) {
      const s = (y * CEL + x) * 4, d = ((yTopo + y) * folha.width + x0 + x) * 4;
      folha.data[d] = im.data[s]; folha.data[d + 1] = im.data[s + 1];
      folha.data[d + 2] = im.data[s + 2]; folha.data[d + 3] = im.data[s + 3];
    }
  }
});
p.write(FOLHA, folha);

// --- .meta: some com as entradas dos frames que não existem mais ---
const mortos = atuais.slice(novos.length).map(a => a.nome);
let texto = meta;
for (const nome of mortos) {
  // 1. internalIDToNameTable: três linhas, "- first:" / "213: <id>" / "second: <nome>"
  texto = texto.replace(new RegExp('  - first:\\n      213: \\d+\\n    second: ' + nome + '\\n'), '');
  // 2. nameFileIdTable: uma linha
  texto = texto.replace(new RegExp('      ' + nome + ': \\d+\\n'), '');
  // 3. o sprite em si: do "- serializedVersion" que abre o bloco até o próximo
  const i = texto.indexOf('      name: ' + nome + '\n');
  if (i < 0) throw new Error('sprite ' + nome + ' não encontrado para remoção');
  const ini = texto.lastIndexOf('\n    - serializedVersion:', i) + 1;
  const prox = texto.indexOf('\n    - serializedVersion:', i);
  const fim = prox < 0 ? texto.indexOf('\n    outline:', i) + 1 : prox + 1;
  texto = texto.slice(0, ini) + texto.slice(fim);
}
fs.writeFileSync(META, texto);

console.log(`${ESTADO}: linha em y=${yTopo} (PNG), ${atuais.length} celulas de ${CEL}px`);
console.log(`  escritos ${novos.length} frames: ${arqs.join(', ')}`);
console.log(mortos.length ? `  removidos do .meta: ${mortos.join(', ')}` : '  nenhum sprite removido');
