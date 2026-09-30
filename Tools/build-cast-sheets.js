// Monta as folhas do elenco da Fase 01 a partir dos quadros baixados por build-cast.js e grava
// o .meta junto, para o Unity importar certo de primeira (inclusive com o Editor aberto).
//
//   node Tools/build-cast-sheets.js
//
// Saída: Assets/Resources/Odisseia/Characters/NPCs/CHR_<Nome>.png, fatiada em
// CHR_<Nome>_Idle_NN — o formato que o SpriteAnimator lê (<Folha>_<Estado>_<NN>). Fica em
// Resources porque é de lá que o SpriteAnimator carrega; o Idle_00 é também o sprite parado.
//
// Regras, as mesmas do master do Odisseu:
// - 42,857143 px por unidade: a densidade do projeto inteiro. Nada de escala no Transform.
// - Pivô BottomCenter com os pés na última linha da célula: a cena apoia a figura pelo
//   bounds.min.y, então qualquer folga embaixo dos pés vira personagem flutuando.
// - A MESMA janela de recorte em todos os quadros de um personagem: recortar cada quadro pelo
//   próprio contorno faz o corpo tremer de lado a lado no ciclo.
// - Quadros escolhidos, não todos. O Idle volta em pingue-pongue (0..pico..1): respiração é
//   simétrica, e isso também tira do ciclo os quadros em que o v3 gira o rosto para a câmera
//   no fim da animação (Lavrador, Ancião) ou solta o objeto da mão (Treinador).
const fs = require('fs'), path = require('path');
const p = require('./png.js');
const { textureMeta, ensureFolder } = require('./unity-import.js');

const ROOT = path.join(__dirname, '..');
const DOCS = path.join(ROOT, 'Docs/Characters');
const NPCS = 'Assets/Resources/Odisseia/Characters/NPCs';
const GUTTER = 1;

// pico = último quadro aproveitável do Idle gerado. Medido olhando a tira de cada um
// (Docs/Characters/Fase01/_idle_tiras_a.png e _b.png): depois do pico o personagem vira ou deforma.
// Personagem com mais de um estado declara `estados` com a lista de quadros de cada um.
const ELENCO = {
  Penelope: { pico: 3 },
  Telemachus_Child: { pico: 3 },
  Herald_Mycenae: { pico: 3 },
  Soldier_Ithaca: { pico: 3 },
  Trainer_Veteran: { pico: 3 },   // 5 e 6: o cajado se solta da mão
  Villager_Woman: { pico: 3 },
  Villager_Merchant: { pico: 3 },
  Villager_Fisherman: { pico: 3 },
  Villager_Farmer: { pico: 3 },   // 4 a 6: o rosto gira para a câmera
  Villager_Sailor: { pico: 3 },
  Villager_Elder: { pico: 2 },    // 3 a 6: o rosto gira para a câmera

  // Fase 02 — Troia (tiras em Docs/Characters/Fase02/_tiras.png).
  Greek_Soldier_Mycenae: { fase: '02', pico: 3 },
  // Inimigo: só os estados que o EnemyAnimator aciona (Idle, Run, Hit, Death). A folha antiga
  // tinha Attack, AttackThrow e Jump, que nenhum código toca.
  Trojan_Soldier: {
    fase: '02', destino: 'Assets/Resources/Odisseia/Enemies',
    estados: {
      Idle: [0, 1, 2, 3, 2, 1],
      Run: [3, 4, 5, 6, 7],          // 0–2 são a arrancada a partir da pose parada; o ciclo é 3–7
      Hit: [0, 1, 2, 3, 4, 5],
      Death: [0, 1, 2, 3, 4, 5, 6, 7],
    },
  },

  // Fase 03 — Cicones (tiras em Docs/Characters/Fase03/_tiras.png). Mesmos estados e mesma regra
  // do troiano: o loop do Run começa no quadro 3, depois da arrancada.
  Cicones_Warrior: {
    fase: '03', destino: 'Assets/Resources/Odisseia/Enemies',
    estados: {
      Idle: [0, 1, 2, 3, 2, 1],
      Run: [3, 4, 5, 6, 7],
      Hit: [0, 1, 2, 3, 4, 5],
      Death: [0, 1, 2, 3, 4, 5, 6, 7],
    },
  },

  // Fase 05 — Ciclopes. Chefe estacionário: só o que o BossController produz (aviso e golpe).
  // Quadros com o olho único pintado por Tools/cyclops-eye.js (o v3 desenhou dois olhos).
  Polyphemus: {
    fase: '05', destino: 'Assets/Resources/Odisseia/Enemies', centroNoCorpo: true,
    estados: {
      Idle: [0, 1, 2, 3, 2, 1],
      Telegraph: { de: 'Slam', quadros: [0, 1, 2, 3, 4, 5] },   // erguer e segurar a clava (aviso de 0,9 s)
      Attack: { de: 'Slam', quadros: [6, 7] },                 // o golpe no chão
    },
  },

  // Fase 06 — Éolo. Figura no palácio: só Idle. Quadros 3–6: em 0–2 o cajado cruza o rosto.
  Aeolus: { fase: '06', estados: { Idle: [3, 4, 5, 6, 5, 4] } },

  // Fase 07 — Lestrigões. Uma folha para os três: arremessadores (Idle, Telegraph, Attack) e perseguidor (Run).
  // Run vem do 'Charge', loop 3–7: o 'Run' original fazia a pedra sumir nos quadros 1–4 (descartado).
  Lestrigon_Warrior: {
    fase: '07', destino: 'Assets/Resources/Odisseia/Enemies', centroNoCorpo: true,
    estados: {
      Idle: [0, 1, 2, 3, 2, 1],
      Telegraph: { de: 'Throw', quadros: [0, 1, 2, 3, 4, 5] },   // ergue a pedra e a segura acima do ombro
      Attack: { de: 'Throw', quadros: [6, 7] },                 // traz para a frente e arremessa
      Run: { de: 'Charge', quadros: [3, 4, 5, 6, 7] },
    },
  },

  // Fase 08 — Circe. Figura no templo: Idle com o ciclo INTEIRO — a taça solta vapor nos quadros
  // 2, 4 e 6; em pingue-pongue o vapor piscaria.
  Circe: { fase: '08', estados: { Idle: [0, 1, 2, 3, 4, 5, 6] } },
  // Lobo encantado: o inimigo da fase (EnemyBasic por override). Run 3–7 (0–1 parado, 2 arrancada).
  Circe_Wolf: {
    fase: '08', destino: 'Assets/Resources/Odisseia/Enemies', centroNoCorpo: true,
    estados: {
      Idle: [0, 1, 2, 3, 2, 1],
      Run: [3, 4, 5, 6, 7],
      Hit: [0, 1, 2, 3, 4, 5],
      Death: [0, 1, 2, 3, 4, 5, 6, 7],
    },
  },

  // Fase 09 — Mundo dos Mortos. Sombra guerreira = o inimigo (override); Run 3–7.
  Shade_Warrior: {
    fase: '09', destino: 'Assets/Resources/Odisseia/Enemies', centroNoCorpo: true,
    estados: {
      Idle: [0, 1, 2, 3, 2, 1],
      Run: [3, 4, 5, 6, 7],
      Hit: [0, 1, 2, 3, 4, 5],
      Death: [0, 1, 2, 3, 4, 5, 6, 7],
    },
  },
  // Sombra anônima (a multidão junto ao sangue) e Anticleia: figuras, só Idle.
  Shade: { fase: '09', pico: 3 },
  Anticleia: { fase: '09', pico: 3 },

  // Fase 10 — Sereias. Figuras (santuário e agulhas de rocha): Idle que canta; variações por matiz.
  Siren: { fase: '10', pico: 3 },

  // Fase 11 — Cila. Chefe: Idle + Strike dividido (0–3 recua, 4–7 as cabeças mergulham).
  Scylla: {
    fase: '11', destino: 'Assets/Resources/Odisseia/Enemies', centroNoCorpo: true, pesDoPrimeiro: true,
    estados: {
      Idle: [0, 1, 2, 3, 2, 1],
      Telegraph: { de: 'Strike', quadros: [0, 1, 2, 3] },
      Attack: { de: 'Strike', quadros: [4, 5, 6, 7] },
    },
  },

  // Fase 13 — Calipso. Figura junto à jangada, só Idle: a cabeça desce e o véu se ergue na brisa até o 5;
  // o 6 já volta. Pingue-pongue 0..5..1. A folha olha para a ESQUERDA (perfil = rotação south-west).
  Calypso: { fase: '13', pico: 5 },

  // Final Polish P-08 — parceiro de treino da Fase 01 (EnemyBasic por override): o soldado de Ítaca. Run 3–7.
  Ithaca_Sparring: {
    fase: '01', origem: 'Soldier_Ithaca', destino: 'Assets/Resources/Odisseia/Enemies', centroNoCorpo: true,
    estados: { Idle: [0, 1, 2, 3, 2, 1], Run: [3, 4, 5, 6, 7], Hit: [0, 1, 2, 3, 4, 5], Death: [0, 1, 2, 3, 4, 5, 6, 7] },
  },

  // Fases 14 e 15 — Ítaca Return e Pretendentes. Eumeu e Telêmaco adulto: NPCDialogue, só Idle (depois do 3 o
  // Eumeu se inclina para a frente). Pretendente = o inimigo das duas fases (EnemyBasic por override); Run 3–7.
  Eumaeus: { fase: '14', pico: 3 },
  Telemachus_Adult: { fase: '14', pico: 3 },
  Suitor: {
    fase: '15', destino: 'Assets/Resources/Odisseia/Enemies', centroNoCorpo: true,
    estados: {
      Idle: [0, 1, 2, 3, 2, 1],
      Run: [3, 4, 5, 6, 7],
      Hit: [0, 1, 2, 3, 4, 5],
      Death: [0, 1, 2, 3, 4, 5, 6, 7],
    },
  },
};

const pingPong = pico => [...Array(pico + 1).keys(), ...Array.from({ length: pico - 1 }, (_, i) => pico - 1 - i)];
// `origem`: a folha sai dos quadros de OUTRO personagem (mesmos quadros, outro papel — o parceiro de treino é
// o soldado de Ítaca como inimigo). Sem copiar arquivo nenhum.
const fonte = nome => path.join(DOCS, 'Fase' + (ELENCO[nome].fase || '01'), ELENCO[nome].origem || nome);
const estadosDe = cfg => cfg.estados || { Idle: pingPong(cfg.pico) };

function quadros(nome, estado) {
  const dir = path.join(fonte(nome), estado + '_east');
  return fs.readdirSync(dir).filter(f => f.endsWith('.png')).sort().map(f => p.read(path.join(dir, f)));
}

// Uma linha por estado. A janela de recorte é UMA só para o personagem inteiro (união de todos
// os quadros usados, que saem do mesmo canvas do PixelLab): recortar por estado faria o corpo
// pular de lugar na troca de Idle para Run. A base da célula é a linha dos pés do Idle, que
// precisa ser constante; os outros estados podem subir (corrida) e o que passar dela (1 px da
// queda na morte) é cortado, para o personagem nunca afundar no chão.
function montar(nome, cfg, variante) {
  const estados = estadosDe(cfg);
  // Um estado pode vir de uma animação com outro nome: { de: 'Slam', quadros: [...] }. É o caso do
  // Polifemo, cujo aviso e golpe saem de UMA geração dividida em duas.
  const linhas = Object.entries(estados).map(([estado, def]) => {
    const origem = Array.isArray(def) ? estado : def.de;
    const ordem = Array.isArray(def) ? def : def.quadros;
    const todos = quadros(nome, origem);
    return { estado, ordem, usados: ordem.map(i => todos[i]) };
  });

  const idle = linhas.find(l => l.estado === 'Idle');
  // pesDoPrimeiro: criatura sem pés (a Cila apoia em caudas de serpente, que oscilam 1 px). A base passa a
  // ser a do quadro 0 e o que descer dela é cortado; para quem tem pés, a exigência de linha fixa continua.
  const pesY = new Set(idle.usados.map(im => p.bounds(im, 0).y1));
  if (cfg.pesDoPrimeiro) { pesY.clear(); pesY.add(p.bounds(idle.usados[0], 0).y1); }
  if (pesY.size !== 1) throw new Error(`${nome}: linha dos pés do Idle varia entre quadros (${[...pesY]})`);
  const pes = [...pesY][0];

  let x0 = 1e9, y0 = 1e9, x1 = -1;
  for (const l of linhas) for (const im of l.usados) {
    const b = p.bounds(im, 0);
    x0 = Math.min(x0, b.x0); y0 = Math.min(y0, b.y0); x1 = Math.max(x1, b.x1);
  }
  // Janela simétrica em torno do CORPO (centro do Idle_00): o pivô BottomCenter fica no corpo, e
  // espelhar não o faz pular de lado. Necessário quando a arma estica a janela para um lado só
  // (a clava do Polifemo na preparação do golpe desloca o centro em ~0,6 un).
  if (cfg.centroNoCorpo) {
    const b = p.bounds(idle.usados[0], 0);
    const centro = Math.round((b.x0 + b.x1) / 2);
    const meia = Math.max(centro - x0, x1 - centro);
    x0 = centro - meia; x1 = centro + meia;
  }

  const cw = x1 - x0 + 1, ch = pes - y0 + 1;
  const maior = Math.max(...linhas.map(l => l.usados.length));
  const passoX = cw + GUTTER * 2, passoY = ch + GUTTER * 2;
  const folha = p.blank(maior * passoX, linhas.length * passoY);
  const fatias = [];
  const base = 'CHR_' + (variante ? variante.nome : nome);
  linhas.forEach((l, r) => l.usados.forEach((im, k) => {
    let q = p.crop(im, x0, y0, cw, ch);
    if (variante) q = variante.aplicar(q);
    const x = k * passoX + GUTTER, y = r * passoY + GUTTER;
    p.blit(folha, q, x, y);
    // rect do Unity: origem embaixo à esquerda.
    fatias.push({ name: `${base}_${l.estado}_${String(k).padStart(2, '0')}`, x, y: folha.height - y - ch, w: cw, h: ch });
  }));
  return { base, folha, fatias, cw, ch, resumo: linhas.map(l => `${l.estado}[${l.ordem}]`).join(' ') };
}

function gravar({ base, folha, fatias }, destino) {
  const pastaUnity = destino || NPCS;
  const unityPath = `${pastaUnity}/${base}.png`;
  ensureFolder(path.join(ROOT, pastaUnity), pastaUnity);
  p.write(path.join(ROOT, unityPath), folha);
  fs.writeFileSync(path.join(ROOT, unityPath + '.meta'), textureMeta(unityPath, folha, { slice: fatias }));
  return unityPath;
}

if (require.main === module) {
  // Resources/Odisseia/Characters já existe (é a pasta do Odisseu); garantir só NPCs.
  const variantes = fs.existsSync(path.join(__dirname, 'cast-variants.js')) ? require('./cast-variants.js') : [];
  const relatorio = [];
  for (const [nome, cfg] of Object.entries(ELENCO)) {
    const r = montar(nome, cfg);
    const up = gravar(r, cfg.destino);
    fs.copyFileSync(path.join(ROOT, up), path.join(fonte(nome), `${r.base}.png`));
    relatorio.push(`${r.base.padEnd(28)} ${r.fatias.length} quadros ${r.resumo} célula ${r.cw}x${r.ch} = ${(r.ch / 42.857143).toFixed(2)} un`);
  }
  for (const v of variantes) {
    const r = montar(v.de, ELENCO[v.de], v);
    gravar(r, ELENCO[v.de].destino);
    relatorio.push(`${r.base.padEnd(28)} variante de ${v.de}: ${v.nota}`);
  }
  console.log(relatorio.join('\n'));
}

module.exports = { ELENCO, pingPong };
