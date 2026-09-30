// Olho único do Polifemo, pintado por código sobre os quadros gerados pelo v3.
//
//   node Tools/cyclops-eye.js rotacao <original.png> <saida.png>       o quadro parado (master)
//   node Tools/cyclops-eye.js quadros <Estado> [<Estado>...]           todos os quadros da animação
//
// Por que existe: duas gerações do v3 (14 gerações) desenharam DOIS olhos, por mais que o prompt
// descrevesse um só — na primeira ele até acrescentou um terceiro na testa. E animar a partir do
// quadro editado não basta: o v3 usa o quadro inicial como referência de POSE, não de anatomia, e
// redesenhou o olho humano pequeno nos outros 14 quadros. De perfil qualquer rosto mostra um olho
// só; o que faz um ciclope é esse olho ser GRANDE e estar no MEIO da testa. É defeito local
// (~12x12 px), então é caso de recorte por código, não de geração (memória "compor em vez de gerar").
//
// Nos quadros de animação o olho humano às vezes some (ele pisca, aperta), então não dá para
// procurá-lo. O que se procura é a CABEÇA: casamento de padrão da cabeça da rotação original (sem a
// região do olho) em cada quadro, por translação. O olho vai para o mesmo ponto da cabeça.
//
// Cores: todas do próprio sprite — nenhuma entra na paleta.
const fs = require('fs'), path = require('path');
const p = require('./png.js');
const ROOT = path.join(__dirname, '..');
const DIR = path.join(ROOT, 'Docs/Characters/Fase05/Polyphemus');
const ORIGINAL = path.join(DIR, 'Polyphemus_east_v3_original.png');

const hex = h => [parseInt(h.slice(1, 3), 16), parseInt(h.slice(3, 5), 16), parseInt(h.slice(5, 7), 16), 255];
const C = {
  contorno: hex('#090405'), sombra: hex('#9e5f3e'), pele: hex('#c68754'), esclera: [232, 225, 187, 255],
  cabelo: hex('#2c1b17'), cabeloClaro: hex('#3c2618'), iris: hex('#93563a'), pupila: hex('#000000'),
};
// Na rotação original (208x208): olho humano em (116..119, 37..39); olho novo centrado em (120,34).
const OLHO = { x: 120, y: 34 };
// Centro do olho humano na rotação original, e o deslocamento dele até o centro do olho novo.
const OLHO_HUMANO = { x: 117.5, y: 38.5 };
const DESLOCAMENTO = { x: OLHO.x - OLHO_HUMANO.x, y: OLHO.y - OLHO_HUMANO.y };

// Esclera do olho humano (a do sprite é ~(232,225,187)) num raio de 8 px do ponto previsto.
function acharOlhoHumano(im, px, py) {
  let sx = 0, sy = 0, n = 0;
  for (let y = Math.round(py) - 8; y <= Math.round(py) + 8; y++) for (let x = Math.round(px) - 8; x <= Math.round(px) + 8; x++) {
    if (x < 0 || y < 0 || x >= im.width || y >= im.height) continue;
    const i = (y * im.width + x) * 4;
    if (im.data[i + 3] && im.data[i] > 200 && im.data[i + 1] > 185 && im.data[i + 2] > 140) { sx += x; sy += y; n++; }
  }
  // O olho humano tem 4 a 8 px de esclera; mais que 12 é o olho único já pintado (quadro 0 do Idle,
  // que é o próprio quadro inicial editado) — ancorar nele deslocaria o olho a cada rodada.
  return n >= 2 && n <= 12 ? { x: sx / n, y: sy / n, n } : null;
}

// Janela da cabeça usada no casamento (cabelo, testa, nariz, barba), sem a região do olho.
const CABECA = { x0: 96, y0: 12, x1: 136, y1: 56 };
const regiaoDoOlho = (x, y) => x >= OLHO.x - 7 && x <= OLHO.x + 7 && y >= OLHO.y - 7 && y <= OLHO.y + 7;

function pintarOlho(im, cx, cy) {
  const put = (x, y, c) => {
    if (x < 0 || y < 0 || x >= im.width || y >= im.height) return;
    const i = (y * im.width + x) * 4; im.data[i] = c[0]; im.data[i + 1] = c[1]; im.data[i + 2] = c[2]; im.data[i + 3] = c[3];
  };
  const opaco = (x, y) => im.data[(y * im.width + x) * 4 + 3] > 0;
  // 1. Apaga o olho humano: bochecha em pele, sombra sob o olho novo na primeira linha.
  for (let y = cy + 3; y <= cy + 5; y++) for (let x = cx - 5; x <= cx; x++) if (opaco(x, y)) put(x, y, y === cy + 3 ? C.sombra : C.pele);
  // 2. Olho: elipse rx 4,5 x ry 3,4 — contorno no anel, esclera dentro.
  const rx = 4.5, ry = 3.4;
  for (let y = cy - 4; y <= cy + 4; y++) for (let x = cx - 5; x <= cx + 5; x++) {
    const d = ((x - cx) / rx) ** 2 + ((y - cy) / ry) ** 2;
    if (d <= 1) put(x, y, d > 0.62 ? C.contorno : C.esclera);
  }
  // 3. Íris puxada para a frente, com 1 px de esclera antes do contorno (encostada, a pupila preta
  //    se fundia ao contorno preto e a íris lia como um entalhe na borda).
  for (let y = cy - 1; y <= cy + 2; y++) for (let x = cx; x <= cx + 2; x++) put(x, y, C.iris);
  for (const [dx, dy] of [[1, 0], [2, 0], [1, 1], [2, 1]]) put(cx + dx, cy + dy, C.pupila);
  put(cx, cy - 1, C.esclera);
  // 4. Monocelha pesada, uma só.
  for (let x = cx - 5; x <= cx + 5; x++) { put(x, cy - 5, C.cabelo); put(x, cy - 4, C.cabeloClaro); }
  for (let x = cx - 3; x <= cx + 3; x++) put(x, cy - 6, C.cabelo);
}

// Translação (dx, dy) que leva a cabeça da rotação original ao quadro: menor soma de diferenças.
function acharCabeca(ref, im) {
  let melhor = { erro: Infinity };
  const baseX = (im.width - ref.width) >> 1, baseY = (im.height - ref.height) >> 1;
  for (let dy = baseY - 40; dy <= baseY + 40; dy++) for (let dx = baseX - 60; dx <= baseX + 60; dx++) {
    let erro = 0, n = 0;
    for (let y = CABECA.y0; y <= CABECA.y1 && erro < melhor.erro; y += 1) for (let x = CABECA.x0; x <= CABECA.x1; x += 1) {
      if (regiaoDoOlho(x, y)) continue;
      const qx = x + dx, qy = y + dy;
      const a = (y * ref.width + x) * 4;
      if (qx < 0 || qy < 0 || qx >= im.width || qy >= im.height) { erro += 3 * 255; n++; continue; }
      const b = (qy * im.width + qx) * 4;
      if (!ref.data[a + 3] && !im.data[b + 3]) continue;
      if (!ref.data[a + 3] || !im.data[b + 3]) { erro += 3 * 255; n++; continue; }
      erro += Math.abs(ref.data[a] - im.data[b]) + Math.abs(ref.data[a + 1] - im.data[b + 1]) + Math.abs(ref.data[a + 2] - im.data[b + 2]); n++;
    }
    if (erro < melhor.erro) melhor = { erro, dx, dy, n };
  }
  melhor.medio = melhor.erro / Math.max(1, melhor.n);
  return melhor;
}

const [modo, ...args] = process.argv.slice(2);
if (modo === 'rotacao') {
  const im = p.read(args[0]); pintarOlho(im, OLHO.x, OLHO.y); p.write(args[1], im);
  console.log(`olho em (${OLHO.x},${OLHO.y})`);
} else if (modo === 'quadros') {
  const ref = p.read(ORIGINAL);
  for (const estado of args) {
    const pasta = path.join(DIR, `${estado}_east`), guarda = path.join(DIR, `${estado}_east_v3_original`);
    if (!fs.existsSync(guarda)) fs.renameSync(pasta, guarda);   // o original do v3 nunca é sobrescrito
    fs.mkdirSync(pasta, { recursive: true });
    for (const f of fs.readdirSync(guarda).filter(f => f.endsWith('.png')).sort()) {
      const im = p.read(path.join(guarda, f));
      const m = acharCabeca(ref, im);
      // A cabeça INCLINA no golpe (ele franze e se curva), e translação sozinha erra o ponto. Quando
      // o olho humano está aberto, a posição real dele manda: esclera clara perto do ponto previsto.
      const humano = acharOlhoHumano(im, OLHO_HUMANO.x + m.dx, OLHO_HUMANO.y + m.dy);
      const cx = humano ? Math.round(humano.x + DESLOCAMENTO.x) : OLHO.x + m.dx;
      const cy = humano ? Math.round(humano.y + DESLOCAMENTO.y) : OLHO.y + m.dy;
      pintarOlho(im, cx, cy);
      p.write(path.join(pasta, f), im);
      console.log(`${f}: cabeça (${m.dx},${m.dy}) erro ${m.medio.toFixed(1)}; ` +
        (humano ? `olho humano em (${humano.x.toFixed(1)},${humano.y.toFixed(1)})` : 'olho fechado, pela cabeça') +
        ` -> olho único em (${cx},${cy})`);
    }
  }
} else {
  console.error('uso: rotacao <in> <out> | quadros <Estado>...'); process.exit(1);
}
