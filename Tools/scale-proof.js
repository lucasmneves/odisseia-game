// Prova visual da seção 25 do briefing: o Odisseu tem de passar pela porta.
// Acha a porta sozinho (maior bloco de Madeira da paleta) e encaixa o Idle_00 no vão,
// alinhado pela linha dos pés, com uma cópia ao lado para comparar a silhueta.
//
//   node Tools/scale-proof.js <casa.png> <saida.png>
const path = require('path'), p = require('./png.js');
const { loadPalette } = require('./ramp-map.js');
const ROOT = path.join(__dirname, '..'), UN = 42.857143, CELL = 84;

const ramps = loadPalette(path.join(ROOT, 'Docs/Environment_Ithaca/Palette/ITHACA_PALETTE.gpl'));
const WOOD = new Set(ramps['Madeira'].map(c => (c[0] << 16) | (c[1] << 8) | c[2]));

// Maior componente conexa de madeira = o painel da porta. As ombreiras ficam de fora
// por serem tiras estreitas separadas pelo contorno, que é o que queremos: o vão útil.
function biggestWoodBlock(img) {
  const key = (o) => (img.data[o] << 16) | (img.data[o + 1] << 8) | img.data[o + 2];
  const seen = new Uint8Array(img.width * img.height);
  const achadas = [];
  for (let i = 0; i < seen.length; i++) {
    if (seen[i] || img.data[i * 4 + 3] <= 8 || !WOOD.has(key(i * 4))) continue;
    const stack = [i]; seen[i] = 1;
    const c = { n: 0, x0: 1e9, y0: 1e9, x1: -1, y1: -1 };
    while (stack.length) {
      const k = stack.pop(), x = k % img.width, y = (k - x) / img.width;
      c.n++;
      if (x < c.x0) c.x0 = x; if (x > c.x1) c.x1 = x;
      if (y < c.y0) c.y0 = y; if (y > c.y1) c.y1 = y;
      for (const [dx, dy] of [[1, 0], [-1, 0], [0, 1], [0, -1]]) {
        const nx = x + dx, ny = y + dy;
        if (nx < 0 || ny < 0 || nx >= img.width || ny >= img.height) continue;
        const nk = ny * img.width + nx;
        if (seen[nk] || img.data[nk * 4 + 3] <= 8 || !WOOD.has(key(nk * 4))) continue;
        seen[nk] = 1; stack.push(nk);
      }
    }
    achadas.push(c);
  }

  // Porta DUPLA é duas componentes, não uma: as folhas ficam separadas pela linha central
  // escura. Pegar só a maior mede meia porta — no palácio isso reprovava o vão por 1 px,
  // com o Odisseu cabendo folgado nas duas folhas juntas.
  //
  // Juntar exige as duas condições: mesma faixa vertical (a verga e a soleira também são
  // madeira e ficam logo acima e abaixo) e encostadas horizontalmente.
  achadas.sort((a, b) => b.n - a.n);
  const maior = achadas[0];
  if (!maior) {
    return null;
  }

  const alturaDe = (c) => c.y1 - c.y0 + 1;
  const larguraDe = (c) => c.x1 - c.x0 + 1;
  let x0 = maior.x0, x1 = maior.x1, y0 = maior.y0, y1 = maior.y1;
  for (const c of achadas.slice(1)) {
    const mesmaFaixa = Math.abs(c.y0 - maior.y0) <= 4 && Math.abs(alturaDe(c) - alturaDe(maior)) <= 4;
    const encostada = c.x0 - x1 <= 20 && x0 - c.x1 <= 20;
    // Folha gêmea tem largura comparável à outra. Sem esta condição as OMBREIRAS da casa
    // entram junto — são madeira, da mesma altura e encostadas — e o vão medido passa a
    // incluir a moldura em vez do buraco por onde o jogador passa.
    // 0,30 separa com folga os dois casos medidos: no armazém as bandas de ferro deixam as
    // folhas em 17 e 7 px (0,41), e na casa as ombreiras ficam em 5 e 6 px contra 39 (0,13).
    const folhaGemea = larguraDe(c) >= larguraDe(maior) * 0.3;
    if (!mesmaFaixa || !encostada || !folhaGemea) continue;
    x0 = Math.min(x0, c.x0); x1 = Math.max(x1, c.x1);
    y0 = Math.min(y0, c.y0); y1 = Math.max(y1, c.y1);
  }

  return { n: maior.n, x0, y0, x1, y1 };
}

module.exports = { acharPorta: biggestWoodBlock };

if (require.main !== module) {
  return;
}

const house = p.read(process.argv[2]);
const sheet = p.read(path.join(ROOT, 'Assets/Resources/Odisseia/Characters/CHR_Odysseus.png'));
const idle = p.crop(sheet, 0, 0, CELL, CELL);            // linha 0 = Idle, coluna 0 = frame 00
const ib = p.bounds(idle);
const hero = p.crop(idle, ib.x0, ib.y0, ib.w, ib.h);

const door = biggestWoodBlock(house);
const dw = door.x1 - door.x0 + 1, dh = door.y1 - door.y0 + 1;
const hb = p.bounds(house);

const SKY = ramps['Ceu'][1];
const pad = 24, W = hb.w + hero.width + pad * 3, H = hb.h + pad * 2;
const out = p.blank(W, H);
for (let i = 0; i < W * H; i++) {
  const o = i * 4;
  out.data[o] = SKY[0]; out.data[o + 1] = SKY[1]; out.data[o + 2] = SKY[2]; out.data[o + 3] = 255;
}
p.blit(out, house, pad, pad);
p.blit(out, hero, pad + door.x0 + Math.round((dw - hero.width) / 2), pad + door.y1 + 1 - hero.height);
p.blit(out, hero, pad + hb.w + pad, pad + hb.h - hero.height);   // mesma linha de chão
p.write(process.argv[3], out);

const verdict = (dw > hero.width && dh > hero.height) ? 'PASSA' : 'REPROVA';
console.log(`Odisseu Idle_00  ${hero.width}x${hero.height}px = ${(hero.height / UN).toFixed(2)} un`);
console.log(`vão da porta     ${dw}x${dh}px = ${(dh / UN).toFixed(2)} un`);
console.log(`folga            ${dw - hero.width}px lateral, ${dh - hero.height}px de altura  ->  ${verdict}`);
