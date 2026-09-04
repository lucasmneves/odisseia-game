// Portão de progressão — a folha de madeira que fecha a passagem entre os atos.
//
// Desenhado por código, como a coluna: é geometria, e o que existe no banco de arte são
// portas de casa (2,17 un) baixas demais para o vão de 5 un que o portão bloqueia.
//
// O tamanho é ditado pelo colisor, não pelo gosto: a folha bloqueia 5 unidades porque o pulo
// alcança 2,45 e o portão não pode ser transponível. Desenhar uma folha baixa e deixar o
// colisor alto criaria parede invisível — e o README é explícito: parede muda é
// indistinguível de bug. O que se vê é o que bloqueia.
const path = require('path');
const C = require('./compose.js');
const p = C.p;

const out = process.argv[2] || path.join(C.ENV, 'Architecture/ithaca_gate_01.png');
const M = C.ramps['Madeira'], PF = C.ramps['Pedra fria'], OUT = C.ramps['Contorno'][3];

const W = 103, H = 214;                 // 2,40 x 4,99 un
const img = p.blank(W, H);

// -- pranchas verticais, com sombreamento de forma: cada prancha abaula no meio --
const PRANCHA = 12;
for (let x = 1; x < W - 1; x++) {
  const dentro = (x - 1) % PRANCHA;
  // Junta entre pranchas: uma coluna escura, e a seguinte já volta ao tom cheio.
  let idx;
  if (dentro === 0) idx = 3;
  else {
    const t = dentro / (PRANCHA - 1);
    const d = Math.abs(t - 0.5) * 2;
    idx = d < 0.45 ? 0 : d < 0.8 ? 1 : 2;
  }
  for (let y = 2; y < H - 2; y++) C.fillRect(img, x, y, 1, 1, M[idx]);
}

// -- costura central: são duas folhas, não uma tábua só --
C.fillRect(img, Math.round(W / 2) - 1, 2, 2, H - 4, M[3]);

// -- bandas de ferro, em Pedra fria (é a rampa neutra fria da paleta; não há rampa de metal) --
for (const y of [Math.round(H * 0.22), Math.round(H * 0.62)]) {
  C.fillRect(img, 1, y, W - 2, 9, PF[1]);
  C.fillRect(img, 1, y, W - 2, 2, PF[0]);
  C.fillRect(img, 1, y + 7, W - 2, 2, PF[2]);
  // Rebites, que é o que faz a banda ler como ferro e não como faixa pintada.
  for (let x = 7; x < W - 5; x += 14) C.fillRect(img, x, y + 3, 2, 3, PF[3]);
}

// -- argolas de puxar, uma em cada folha --
for (const cx of [Math.round(W / 2) - 14, Math.round(W / 2) + 13]) {
  const cy = Math.round(H * 0.44);
  for (let a = 0; a < 24; a++) {
    const ang = (a / 24) * Math.PI * 2;
    C.fillRect(img, Math.round(cx + Math.cos(ang) * 5), Math.round(cy + Math.sin(ang) * 5), 2, 2, PF[2]);
  }
}

// -- contorno --
C.fillRect(img, 0, 0, W, 2, OUT);
C.fillRect(img, 0, H - 2, W, 2, OUT);
C.fillRect(img, 0, 0, 1, H, OUT);
C.fillRect(img, W - 1, 0, 1, H, OUT);

p.write(out, C.trim(img));
const final = p.read(out);
console.log(`${path.relative(C.ROOT, out)}  ${final.width}x${final.height}px = ${C.un(final.width)} x ${C.un(final.height)} un`);

// -- batente, num arquivo à parte --
//
// Separado porque ele PERMANECE quando o portão abre: a folha some e os batentes ficam, para
// o lugar continuar legível como passagem em vez de virar um vão qualquer no meio do caminho.
// Por isso também tem a altura da folha, e não a da coluna dórica (2,5 un), que ao lado de um
// portão de 5 un lia como poste caído.
const postOut = process.argv[3] || path.join(C.ENV, 'Architecture/ithaca_gatepost_01.png');
const T = C.ramps['Terra / caminho'];
const PW = 28, PH = 222;
const post = p.blank(PW, PH);

// Fiadas de silhar, com uma junta deslocada a cada fiada.
const FIADA = 16;
for (let y = 6; y < PH - 2; y++) {
  const fiada = Math.floor((y - 6) / FIADA);
  for (let x = 1; x < PW - 1; x++) {
    const t = (x - 1) / (PW - 3);
    const d = Math.abs(t - 0.5) * 2;                 // sombreamento de forma, como na coluna
    let idx = d < 0.4 ? 0 : d < 0.75 ? 1 : 2;
    if ((y - 6) % FIADA === 0) idx = 3;              // junta horizontal
    const junta = (fiada % 2 === 0 ? 9 : 18);
    if (x === junta) idx = 3;                        // junta vertical, alternada por fiada
    C.fillRect(post, x, y, 1, 1, T[idx]);
  }
}

// Capitel: uma laje que avança sobre o fuste, o que dá leitura de batente e não de parede.
C.fillRect(post, 0, 2, PW, 4, T[1]);
C.fillRect(post, 0, 2, PW, 1, T[0]);
C.fillRect(post, 0, 5, PW, 1, T[2]);
C.fillRect(post, 0, 1, PW, 1, OUT);
C.fillRect(post, 0, 6, PW, 1, OUT);
C.fillRect(post, 1, PH - 1, PW - 2, 1, OUT);
C.fillRect(post, 0, 6, 1, PH - 6, OUT);
C.fillRect(post, PW - 1, 6, 1, PH - 6, OUT);

p.write(postOut, C.trim(post));
const fp = p.read(postOut);
console.log(`${path.relative(C.ROOT, postOut)}  ${fp.width}x${fp.height}px = ${C.un(fp.width)} x ${C.un(fp.height)} un`);
