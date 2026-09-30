// Prancha de validação do elenco: escala, densidade, paleta e silhueta lado a lado.
//
//   node Tools/cast-lineup.js
//
// Substitui a "cena de preview" do briefing: uma cena no Unity viraria um arquivo a manter e
// correria o risco de entrar no build. A prancha lê as MESMAS folhas que o jogo carrega
// (Resources/.../NPCs), na densidade do jogo (1 px de arte = 1 px aqui, ampliado por inteiro),
// então mede a mesma coisa sem ser dependência de nada.
//
// Saída em Docs/Characters/:
//   CAST_LINEUP.png     cores, com a linha de 1,4 un (altura do Odisseu) tracejada
//   CAST_SILHOUETTE.png preto chapado, ampliado 3x e no tamanho real — o teste de "reconhecível
//                       só pela silhueta" do briefing
const fs = require('fs'), path = require('path');
const p = require('./png.js');
const ROOT = path.join(__dirname, '..');
const NPC = 'Assets/Resources/Odisseia/Characters/NPCs/';

const ELENCO = [
  ['Odisseu', 'Assets/Resources/Odisseia/Characters/CHR_Odysseus.png', [0, 0, 84, 84]],
  ['Penelope', NPC + 'CHR_Penelope.png'],
  ['Telemaco', NPC + 'CHR_Telemachus_Child.png'],
  ['Arauto (Agamenon)', NPC + 'CHR_Herald_Mycenae.png'],
  ['Instrutor', NPC + 'CHR_Trainer_Veteran.png'],
  ['Soldado', NPC + 'CHR_Soldier_Ithaca.png'],
  ['Mulher', NPC + 'CHR_Villager_Woman.png'],
  ['Mercador', NPC + 'CHR_Villager_Merchant.png'],
  ['Pescador', NPC + 'CHR_Villager_Fisherman.png'],
  ['Lavrador', NPC + 'CHR_Villager_Farmer.png'],
  ['Marinheiro', NPC + 'CHR_Villager_Sailor.png'],
  ['Marinheiro ocre', NPC + 'CHR_Villager_Sailor_Ochre.png'],
  ['Marinheiro oliva', NPC + 'CHR_Villager_Sailor_Olive.png'],
  ['Anciao', NPC + 'CHR_Villager_Elder.png'],
  ['Artesao', NPC + 'CHR_Villager_Elder_Brown.png'],
];

// Primeiro quadro, recortado pelo rect do Idle_00 no .meta — o mesmo recorte que o Unity usa.
// (Achar a célula por coluna vazia falha: lança e cajado deixam vão entre o objeto e o corpo.)
function primeiro(arq, janela) {
  const im = p.read(path.join(ROOT, arq));
  if (!janela) {
    const meta = fs.readFileSync(path.join(ROOT, arq + '.meta'), 'utf8');
    const m = meta.match(/name: \S+_Idle_00\s+rect:\s+serializedVersion: 2\s+x: (\d+)\s+y: (\d+)\s+width: (\d+)\s+height: (\d+)/);
    if (!m) throw new Error('sem Idle_00 em ' + arq);
    const [x, y, w, h] = m.slice(1).map(Number);
    janela = [x, im.height - y - h, w, h];   // rect do Unity tem origem embaixo
  }
  const q = p.crop(im, ...janela);
  const b = p.bounds(q, 0);
  return p.crop(q, b.x0, b.y0, b.w, b.h);
}

function prancha(ims, Z, cor, silhueta) {
  const GAP = 8, H = Math.max(...ims.map(i => i.height)) + 6;
  const W = ims.reduce((a, i) => a + i.width + GAP, GAP);
  const b = p.blank(W, H);
  for (let i = 0; i < b.data.length; i += 4) { b.data[i] = cor[0]; b.data[i + 1] = cor[1]; b.data[i + 2] = cor[2]; b.data[i + 3] = 255; }
  let x = GAP;
  for (const im0 of ims) {
    const im = silhueta ? { ...im0, data: Buffer.from(im0.data).fill(0).map((v, k) => (k % 4 === 3 ? im0.data[k] : 0)) } : im0;
    p.blit(b, im, x, H - 3 - im.height); x += im.width + GAP;
  }
  if (!silhueta) for (let xx = 0; xx < W; xx++) if (xx % 4 < 2) { const i = ((H - 3 - 60) * W + xx) * 4; b.data[i] = 220; b.data[i + 1] = 30; b.data[i + 2] = 30; }
  const o = p.blank(W * Z, H * Z);
  for (let y = 0; y < H * Z; y++) for (let xx = 0; xx < W * Z; xx++) {
    const s = ((y / Z | 0) * W + (xx / Z | 0)) * 4; b.data.copy(o.data, (y * W * Z + xx) * 4, s, s + 4);
  }
  return o;
}

const ims = ELENCO.map(([, arq, j]) => primeiro(arq, j));
p.write(path.join(ROOT, 'Docs/Characters/CAST_LINEUP.png'), prancha(ims, 4, [0x8f, 0xb8, 0xc8], false));
// Silhueta: os quatro principais do briefing + soldado e civil, em 3x e em 1x empilhados.
const principais = [0, 1, 2, 3, 5, 6].map(i => ims[i]);
const g = prancha(principais, 3, [0xe8, 0xe4, 0xd8], true), s = prancha(principais, 1, [0xe8, 0xe4, 0xd8], true);
const both = p.blank(g.width, g.height + s.height + 8);
for (let i = 0; i < both.data.length; i += 4) { both.data[i] = 0xe8; both.data[i + 1] = 0xe4; both.data[i + 2] = 0xd8; both.data[i + 3] = 255; }
p.blit(both, g, 0, 0); p.blit(both, s, 0, g.height + 8);
p.write(path.join(ROOT, 'Docs/Characters/CAST_SILHOUETTE.png'), both);
console.log(ELENCO.map(([n], i) => `${n.padEnd(18)} ${ims[i].width}x${ims[i].height} px = ${(ims[i].height / 42.857143).toFixed(2)} un = ${(ims[i].height / 60).toFixed(2)}x Odisseu`).join('\n'));

// Teste de facção (Fases 02 e 03): cor e silhueta de quem precisa ser distinguido em combate.
// Casa de Odisseu = carmesim; Micenas = índigo + ouro; Troia = bronze, terracota, areia,
// azul-ardósia; Cicones = oliva, terra, madeira, bronze. A linha de baixo é a mesma fila em preto, em tamanho real.
const ENEMIES = 'Assets/Resources/Odisseia/Enemies/';
const faccoes = [
  ['Odisseu', 'Assets/Resources/Odisseia/Characters/CHR_Odysseus.png', [0, 0, 84, 84]],
  ['Grego (Micenas)', NPC + 'CHR_Greek_Soldier_Mycenae.png'],
  ['Arauto (Micenas)', NPC + 'CHR_Herald_Mycenae.png'],
  ['Troiano', ENEMIES + 'CHR_Trojan_Soldier.png'],
  ['Cicone', ENEMIES + 'CHR_Cicones_Warrior.png'],
  ['Circe', NPC + 'CHR_Circe.png'],
  ['Lobo (Circe)', ENEMIES + 'CHR_Circe_Wolf.png'],
  ['Civil (Itaca)', NPC + 'CHR_Villager_Woman.png'],
].map(([, arq, j]) => primeiro(arq, j));
const cor = prancha(faccoes, 4, [0xd8, 0xc8, 0xa0], false);   // fundo areia: o chão de Troia
const sil = prancha(faccoes, 4, [0xe8, 0xe4, 0xd8], true), sil1 = prancha(faccoes, 1, [0xe8, 0xe4, 0xd8], true);
const f = p.blank(cor.width, cor.height + sil.height + sil1.height + 16);
for (let i = 0; i < f.data.length; i += 4) { f.data[i] = 0xe8; f.data[i + 1] = 0xe4; f.data[i + 2] = 0xd8; f.data[i + 3] = 255; }
p.blit(f, cor, 0, 0); p.blit(f, sil, 0, cor.height + 8); p.blit(f, sil1, 0, cor.height + sil.height + 16);
p.write(path.join(ROOT, 'Docs/Characters/CAST_FACTIONS.png'), f);
console.log('prancha de facções: Docs/Characters/CAST_FACTIONS.png');

// Gigantes (Fase 07): humano × ciclope × lestrigão, em cor e em silhueta. O teste é distinguir os
// três sem detalhe pequeno — o olho único do ciclope NÃO vale como critério aqui, porque some em preto.
const gigantes = [
  ['Odisseu', 'Assets/Resources/Odisseia/Characters/CHR_Odysseus.png', [0, 0, 84, 84]],
  ['Polifemo', ENEMIES + 'CHR_Polyphemus.png'],
  ['Lestrigao', ENEMIES + 'CHR_Lestrigon_Warrior.png'],
].map(([, arq, j]) => primeiro(arq, j));
const gc = prancha(gigantes, 2, [0xb8, 0xbc, 0xb4], false);
const gs = prancha(gigantes, 2, [0xe8, 0xe4, 0xd8], true);
const g2 = p.blank(gc.width, gc.height + gs.height + 8);
for (let i = 0; i < g2.data.length; i += 4) { g2.data[i] = 0xe8; g2.data[i + 1] = 0xe4; g2.data[i + 2] = 0xd8; g2.data[i + 3] = 255; }
p.blit(g2, gc, 0, 0); p.blit(g2, gs, 0, gc.height + 8);
p.write(path.join(ROOT, 'Docs/Characters/CAST_GIANTS.png'), g2);
console.log('prancha de gigantes: Docs/Characters/CAST_GIANTS.png');
