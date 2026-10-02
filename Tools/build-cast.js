// Elenco da Fase 01 em pixel art, no mesmo pipeline do master do Odisseu (create_character v3).
//
//   node Tools/build-cast.js create  [nome...]   dispara a criação (2 gerações cada a 64 px)
//   node Tools/build-cast.js fetch   [nome...]   espera e baixa a rotação east
//   node Tools/build-cast.js animate [nome...]   dispara o Idle em east (1 geração cada)
//   node Tools/build-cast.js fetchanim [nome...] espera e baixa os quadros do Idle
//   node Tools/build-cast.js status
//
// O estado (ids do PixelLab) fica em Docs/Characters/cast.json. Cada passo só age em quem
// ainda não passou por ele, então rodar de novo nunca cobra duas vezes o mesmo personagem.
//
// Por que v3 e não `pro` com style_character_id apontando para o master: o master FOI feito
// em v3, então v3 é literalmente o mesmo pipeline — e o `pro` exige canvas pelo menos do
// tamanho do conteúdo do master (60 px), o que impede a criança menor que o adulto.
// A escala sai do `size`: v3 devolve o conteúdo em ~0,94× o tamanho pedido (64 -> 60 no
// Odisseu). Medir sempre depois; nunca assumir.
const fs = require('fs'), path = require('path');
const px = require('./pixellab.js');

const ROOT = path.join(__dirname, '..');
const DIR = path.join(ROOT, 'Docs/Characters');
const ESTADO = path.join(DIR, 'cast.json');
const sleep = ms => new Promise(r => setTimeout(r, ms));

// Direção de arte comum, na ordem em que o modelo pesa: estilo, câmera, época.
const BASE = 'side view, 16-bit pixel art game character, Bronze Age Mycenaean Greek, Mediterranean island of Ithaca';

// O Idle segue a Receita A do master: âncora no quadro 0 + trava pesada de orientação.
const IDLE = 'standing still and breathing calmly, chest rising and falling gently, arms relaxed, ' +
  'in strict side profile facing right, head stays turned right in profile, shoulders and hips ' +
  'stay square to the right, no turning toward the viewer, feet stay planted';

const ELENCO = {
  // --- nomeados ---
  Penelope: {
    // 64 deu 62 px de conteúdo: mais alta que o Odisseu. Alvo ~0,95 dele. O v3 exige múltiplo de 4.
    size: 60,
    desc: 'Penelope, queen of Ithaca, adult woman in her late twenties, slender upright posture, ' +
      'long dark reddish-brown hair gathered in a low knot with a thin bronze band, ' +
      'ankle-length off-white linen peplos pinned at the shoulders with small bronze pins, ' +
      'deep sea-blue border on the hem, sea-blue woollen mantle draped over one shoulder and arm, ' +
      'leather sandals, serene warm expression, hands empty',
  },
  Telemachus_Child: {
    // Na Fase 01 ele é criança ("Pai, você vai lutar contra monstros?"). O alvo é ~0,70 do
    // Odisseu: 42 px contra 60.
    size: 44,
    desc: 'Telemachus, a seven year old Greek boy, son of the king, in strict side profile facing right, ' +
      'small sturdy child body, short tousled dark reddish-brown hair like his father, ' +
      'simple small dot eyes and a small nose, plain determined little face, ' +
      'knee-length off-white linen tunic with a crimson border and a thin leather belt, ' +
      'simple leather sandals, hands empty',
    // A frase 'in strict side profile facing right' fez o v3 desenhar a vista SOUTH já de
    // perfil, e as rotações giraram 90 graus: a 'east' saiu de costas. A south é o perfil.
    perfil: 'south',
    // 1a tentativa (Telemachus_Child_r1) saiu com rosto de anime, olhos grandes e rubor,
    // lendo como menina e em 3/4. Nomear o rosto simples e o perfil corrigiu.
  },
  // --- Micenas: o arauto é a presença de Agamenon na fase ---
  Herald_Mycenae: {
    size: 64,
    desc: 'herald of king Agamemnon from Mycenae, tall formal adult man, neatly trimmed black beard, ' +
      'long deep indigo-blue tunic to the shins with a gold meander border, ' +
      'short indigo cloak fastened with a gold disc brooch, thin gold circlet on black hair, ' +
      'holding a tall wooden herald staff with a bronze tip upright in his front hand, ' +
      'the staff stays attached to his hand, proud stern expression',
  },
  // --- sistema modular: um master de soldado, variações por rampa ---
  Soldier_Ithaca: {
    size: 64,
    desc: 'Ithacan foot soldier, adult man, bronze Greek helmet with cheek guards and no crest, ' +
      'layered leather and linen cuirass over a short off-white tunic, leather pteruges skirt, ' +
      'bronze greaves, leather sandals, short dark beard, ' +
      'holding a tall wooden spear with a bronze head upright at his side, the spear stays attached to his hand',
  },
  Trainer_Veteran: {
    size: 64,
    desc: 'veteran Ithacan drill instructor, muscular man in his fifties, short grey beard, ' +
      'bronze Greek helmet with a tall crimson horsehair crest, bronze muscle cuirass over a sea-blue short tunic, ' +
      'bronze greaves, holding a plain wooden practice staff upright in his hand, the staff stays attached to his hand, ' +
      'stern but fair expression',
  },
  // --- biblioteca civil: o que faltava (nenhuma mulher nem comerciante entre os figurantes) ---
  Villager_Woman: {
    size: 60,
    desc: 'Ithacan village woman, adult, sturdy build, ankle-length plain undyed linen dress, ' +
      'olive-green shawl over the shoulders, dark brown hair covered by a pale sand-colored headscarf, ' +
      'bare forearms, leather sandals, friendly expression, hands empty',
  },
  Villager_Merchant: {
    size: 64,
    desc: 'Ithacan market trader, middle-aged plump man, short curly black beard, ' +
      'knee-length sand-colored tunic with a brown woollen mantle over one shoulder, ' +
      'leather coin pouch on a rope belt, leather sandals, cheerful expression, hands empty',
  },
  // Os quatro arquétipos que existiam em modo standard (Figures/ithaca_villager_*), refeitos em
  // v3: ao lado do master eles liam chapados e cabeçudos. Mesmas descrições, para manter o papel.
  Villager_Fisherman: {
    size: 64,
    desc: 'Ithacan village fisherman, lean weathered adult man, plain undyed short linen tunic, bare arms, ' +
      'short dark beard, rope belt, bare feet, hands empty',
  },
  Villager_Farmer: {
    size: 64,
    desc: 'Ithacan village farmer, sturdy adult man, coarse brown belted knee-length tunic, ' +
      'wide straw sun hat, short brown beard, leather sandals, hands empty',
  },
  Villager_Sailor: {
    size: 64,
    desc: 'Ithacan sailor, wiry young adult man, short sleeveless faded sea-blue tunic, rope belt, ' +
      'bare feet, wind-burnt face, short black hair, clean-shaven, hands empty',
  },
  Villager_Elder: {
    // Encurvado: o alvo é ficar abaixo do Odisseu, não acima como o antigo (63 px).
    size: 60,
    desc: 'old Ithacan villager, long grey beard, bald crown with grey hair at the sides, ' +
      'long plain dark blue robe, slightly stooped, leaning on a wooden walking staff held in his front hand, ' +
      'the staff stays attached to his hand',
  },
};

// ------------------------------------------------------------------ Fase 02 — Troia
// Extensões do master global (CHARACTER_ART_MASTER.md, seção 5): cada facção tem uma família de
// cor reservada. Micenas = índigo + ouro. Troia = bronze, terracota, areia e azul-ardósia —
// tirados das rampas do cenário de Troia (Bronze, Vermelho, Lona), para os soldados
// pertencerem às muralhas. Carmesim fica só com a casa de Odisseu.
const BASE_TROIA = 'side view, 16-bit pixel art game character, Bronze Age Mycenaean era, Trojan War, sun-bleached Aegean coast';

// Receita B do master (movimento amplo): sem âncora, descrição anatômica, apego do objeto à mão.
const APEGO = 'spear and shield stay attached to his hands in every frame';
const ANIM_TROIANO = {
  Idle: { desc: IDLE, frames: 6, ancora: true },
  Run: { frames: 8, ancora: false, desc: 'running forward fast, side view facing right, front knee lifted to hip height, ' +
    'back heel kicking up behind, body leaning slightly forward, spear held level in the front hand and shield on the back arm, ' + APEGO },
  Hit: { frames: 6, ancora: false, desc: 'recoiling from a blow to the chest, side view facing right, torso snaps backward ' +
    'and head jerks back, one foot steps back to catch balance, then returns to a guarded stance, ' + APEGO },
  Death: { frames: 8, ancora: false, desc: 'struck down, side view facing right, knees buckle and the body falls backward, ' +
    'landing flat on his back on the ground and lying still, spear and shield dropping beside him' },
  // PXL-012 (Asset Completion): o EnemyAnimator passou a tocar Attack no evento EnemyController.Attacked.
  Attack: { frames: 6, ancora: false, desc: 'thrusting the spear forward hard at chest height, side view facing right, ' +
    'front foot stepping in and body lunging forward, then pulling the spear back to a guarded stance, ' + APEGO },
};

Object.assign(ELENCO, {
  Trojan_Soldier: {
    // Inimigo (EnemyBasic). O colisor tem 1,2 un: 60 -> ~57 px = 1,33 un, perto dele sem
    // sair da faixa de adulto (0,95 do Odisseu).
    fase: '02', base: BASE_TROIA, size: 60, anims: ANIM_TROIANO,
    desc: 'Trojan foot soldier defending the walled city of Troy, lean sturdy adult man, ' +
      'bronze Phrygian-style helmet whose top curves forward into a rounded point, no horsehair crest, ' +
      'bronze scale armour vest over a knee-length sand-colored linen tunic with a terracotta border, ' +
      'slate-blue sash at the waist, bronze greaves, leather sandals, short black curly beard, ' +
      'round shield faced with terracotta leather and a sand-colored spiral motif on his back arm, ' +
      'short spear in his front hand, ' + APEGO,
  },
  Greek_Soldier_Mycenae: {
    // Figura de fundo no acampamento: só Idle.
    fase: '02', base: BASE_TROIA, size: 64,
    desc: 'Mycenaean Greek hoplite of the army of Agamemnon, adult man, bronze Greek helmet with a tall deep indigo ' +
      'horsehair crest running front to back, bronze muscle cuirass with gold trim over a deep indigo short tunic, ' +
      'large round bronze aspis shield with a gold rim on his back arm, tall spear held upright in his front hand, ' +
      'bronze greaves, leather sandals, short dark beard, ' + APEGO,
  },
});

// ------------------------------------------------------------------ Fase 03 — Cicones
// Os Cicones eram trácios. A identidade vem daí, não de uma paleta nova sobre um hoplita:
// gorro de pele de raposa (alopekis), escudo em meia-lua (pelta) e manto longo estampado (zeira).
// Cada um é um elemento de silhueta que nem o grego (crista alta) nem o troiano (capacete frígio)
// têm. Cor: rampas Oliveira, Terra seca, Madeira e Bronze do cenário dos Cicones — sem carmesim,
// sem índigo, sem o areia dominante do troiano.
const BASE_CICONES = 'side view, 16-bit pixel art game character, Bronze Age, Thracian coast of the northern Aegean';

Object.assign(ELENCO, {
  Cicones_Warrior: {
    // Inimigo (EnemyBasic), mesmo colisor de 1,2 un do troiano: size 60.
    // Estados = os que o EnemyAnimator aciona (CastProbe da Fase 03), com as receitas do troiano.
    fase: '03', base: BASE_CICONES, size: 60, anims: ANIM_TROIANO,
    desc: 'Ciconian warrior, a Thracian coastal tribesman defending his town, lean wiry adult man, ' +
      'tall pointed fox-fur cap with ear flaps, long dark braided beard, ' +
      'ankle-length olive-green woollen cloak with an earth-brown geometric zigzag border, ' +
      'leather jerkin over a short brown tunic, soft leather boots, bronze arm rings, ' +
      'crescent-shaped wicker shield covered in brown hide on his back arm, short spear in his front hand, ' + APEGO,
  },
});

// ------------------------------------------------------------------ Fase 05 — Ciclopes
// Polifemo é chefe estacionário (BossController): não anda, não morre (999 de vida, sem colisor).
// O código só produz dois momentos: o aviso (AttackTelegraphed, 0,9 s) e o golpe (AttackExecuted).
// Por isso: Idle + UMA animação "Slam" (erguer, segurar, golpear) que a folha divide em Telegraph e
// Attack — o golpe começa exatamente onde o aviso termina, e custa uma geração em vez de duas.
// Altura: a mesma do placeholder que ele substitui, 4,6 un = ~197 px; v3 devolve ~0,95 do size.
const BASE_CICLOPES = 'side view, 16-bit pixel art game character, Greek mythology, wild rocky island of the Cyclopes';
const APEGO_CLAVA = 'the club stays attached to his hands in every frame';

Object.assign(ELENCO, {
  Polyphemus: {
    fase: '05', base: BASE_CICLOPES, size: 208,
    // Rotação east com o olho único pintado por Tools/cyclops-eye.js (o v3 insistiu em dois olhos).
    inicio: 'Docs/Characters/Fase05/Polyphemus/Polyphemus_east.png',
    anims: {
      Idle: { frames: 6, ancora: true, desc: IDLE + ', ' + APEGO_CLAVA },
      // 8, não 12: com o gigante o canvas chega a 256 px, e ali o v3 aceita no máximo 8 quadros.
      Slam: { frames: 8, ancora: false, desc: 'slowly raising the huge club high over his head with both hands, ' +
        'body leaning back to wind up, holding it there for a moment, then slamming it straight down onto the ground ' +
        'in front of him with all his weight, body bending forward as the club head strikes the ground, ' +
        'side view facing right, ' + APEGO_CLAVA },
    },
    // Um olho só se descreve pelo que É, não pelo que falta: negação no prompt deixa o modelo
    // escolher o substituto (lição das fases 13/14).
    // 1a tentativa (Polyphemus_r1): 'a single huge round eye set in the centre of his forehead' deu
    // DOIS olhos normais + um terceiro pequeno na testa — o modelo acrescentou o olho em vez de
    // substituir. Descrever o ROSTO inteiro com um olho só, no lugar dos dois, e a clava longe do rosto
    // (na vista east ela o cobria).
    desc: 'Polyphemus the cyclops of Greek myth, a colossal hulking shepherd giant, his face has only one eye: ' +
      'one huge round cyclops eye in the middle of his face where two eyes would be, directly above the nose, ' +
      'under one heavy unibrow, wild dark curly hair and a thick unkempt beard, ' +
      'broad hunched shoulders, huge muscular arms and oversized hands, barrel chest, thick heavy legs, ' +
      'weathered tanned skin, a rough off-white sheep fleece wrapped around his waist and slung over one shoulder, ' +
      'rope belt, bare feet, holding a huge knotted olive-wood club low at his side with its head resting on the ground, ' +
      'the club kept away from his face, ' + APEGO_CLAVA + ', ' +
      'menacing brutish but slightly comical expression',
  },
});

// ------------------------------------------------------------------ Fase 06 — Éolo
// O CastProbe não achou o Éolo na cena — mas o palácio foi montado com o vão central RESERVADO para
// ele ("é onde Éolo, o Odisseu e o diálogo entram depois", EoloSceneDresser). Ele entra como figura:
// no alto dos degraus, diante da porta, junto ao navio de saída. Nenhum código aciona estado de
// figura além do padrão, então: só Idle.
// Um pouco maior que o Odisseu (figura divina; o briefing permite): size 68 -> ~65 px = 1,08×.
// Cor: céu e vento — azul-céu claro, branco, creme, ouro discreto. NÃO o índigo de Micenas, NÃO carmesim.
// A direção do olhar fica FORA da descrição (na Fase 01 isso girou as rotações do v3).
// Sem lugar no texto-base: 'floating island of Aeolia among the clouds' (Aeolus_r1) foi DESENHADO —
// ilha de grama sob os pés e nuvens em volta. Palavra de cenário em prompt de personagem vira conteúdo.
const BASE_EOLO = 'side view, 16-bit pixel art game character, Greek mythology';

Object.assign(ELENCO, {
  Aeolus: {
    fase: '06', base: BASE_EOLO, size: 68,
    anims: { Idle: { frames: 6, ancora: true, desc: IDLE + ', the staff stays attached to his hand' } },
    desc: 'Aeolus, keeper of the winds and king of Aeolia in Greek myth, tall dignified mature man, ' +
      'long silver-white hair and a long silver beard streaming sideways as if blown by the wind, ' +
      'ankle-length pale sky-blue chiton, a white woollen himation mantle billowing out behind him, ' +
      'a thin pale gold circlet, a bronze arm band, leather sandals, ' +
      'holding a slender white wooden staff topped with a small spiral in his front hand, ' +
      'the staff stays attached to his hand, calm powerful expression',
  },
});

// ------------------------------------------------------------------ Fase 07 — Lestrigões
// Três em jogo, um master: dois arremessadores (BossController, como o Polifemo) e um perseguidor
// (PursuerHazard, que o dresser de cenário deixou INVISÍVEL). Arma = pedra: "eles despedaçam meus
// navios com pedras do tamanho de casas", e o chefe marca pontos de impacto no chão.
// Diferença do Polifemo pela FORMA e pela AÇÃO: dois olhos, cabeça raspada com coque (a do ciclope é
// juba), couro escuro com bronze (o dele é velo branco), pedra (a dele é clava). Povo guerreiro, não pastor.
// Cor quente contra o cenário frio (basalto, água escura): pele ocre-avermelhada, couro, bronze.
// Altura = a dos placeholders, 4,25 un = ~182 px -> size 192.
const BASE_LESTRIGOES = 'side view, 16-bit pixel art game character, Greek mythology';
const APEGO_PEDRA = 'the boulder stays held in both hands';

Object.assign(ELENCO, {
  Lestrigon_Warrior: {
    fase: '07', base: BASE_LESTRIGOES, size: 192,
    anims: {
      Idle: { frames: 6, ancora: true, desc: IDLE + ', ' + APEGO_PEDRA },
      // Uma geração, dois estados (como o Slam do Polifemo): erguer a pedra (aviso) e arremessar (golpe).
      Throw: { frames: 8, ancora: false, desc: 'lifting the huge boulder high over his head with both arms, ' +
        'leaning back and holding it there for a moment, then hurling it forward and down with all his strength, ' +
        'arms swinging through, body lunging forward, side view facing right' },
      // Charge: a corrida do perseguidor, de MÃOS VAZIAS. O 'Run' com a pedra (abaixo, mantido como
      // registro) fez a pedra sumir nos quadros 1–4 e voltar em 5–7 — pisca no loop. O perseguidor não
      // arremessa, só corre: investida sem pedra é coerente em todos os quadros. A folha mapeia Charge -> Run.
      Charge: { frames: 8, ancora: false, desc: 'charging forward heavily with long thundering strides, empty-handed, ' +
        'both fists clenched and arms pumping, side view facing right, front knee lifted high, back leg pushing off, ' +
        'torso leaning forward' },
      // O perseguidor anda sozinho: Run é o estado padrão dele. Loop do quadro 3 em diante.
      Run: { frames: 8, ancora: false, desc: 'running forward heavily with long thundering strides, side view facing right, ' +
        'front knee lifted high, back leg pushing off, torso leaning forward, arms pumping, ' + APEGO_PEDRA + ' against his chest' },
    },
    desc: 'Laestrygonian giant warrior from the man-eating giant people of Greek myth, a towering broad-shouldered giant, ' +
      'ruddy ochre-brown skin, two fierce eyes under a heavy brow, shaved head with a single black topknot, ' +
      'a long black braided beard, thick muscular arms and legs, a dark leather kilt studded with bronze, ' +
      'a wide leather belt, a bronze torque around his neck and bronze arm rings, bare feet, ' +
      'holding a large rough grey boulder in both hands in front of his belly, ' + APEGO_PEDRA + ', ' +
      'menacing brutish expression',
  },
});

// ------------------------------------------------------------------ Fase 08 — Circe
// Circe não está na cena, mas o templo foi montado com o vão da porta RESERVADO para ela (CirceSceneDresser),
// como o palácio de Éolo: figura de fundo, só Idle (nenhum código aciona Cast/Talk/Transform nela).
// Paleta: violeta, verde-oliva, creme, ouro discreto — magia + natureza. Sem carmesim, sem índigo.
// Objeto: a taça (kylix) do filtro — o que a define no mito; o cajado já é do Éolo.
// Inimigo: os EnemyBasic carmesim viram LOBOS ENCANTADOS (a casa de Circe era cercada de lobos e leões
// mansos por feitiço). Lobo e não leão: o colisor é 0,6 x 1,2 un, e o corpo do lobo é mais curto.
const BASE_CIRCE = 'side view, 16-bit pixel art game character, Greek mythology';
const ANIM_LOBO = {
  Idle: { desc: 'standing alert and breathing, head low, tail slowly swaying, side view facing right, all four paws stay planted', frames: 6, ancora: true },
  Run: { frames: 8, ancora: false, desc: 'running fast in a full gallop, side view facing right, legs stretching forward and back, ' +
    'body low and long, ears back' },
  Hit: { frames: 6, ancora: false, desc: 'flinching back from a blow, side view facing right, head jerking back, ' +
    'body recoiling and crouching, then recovering to a snarling stance' },
  Death: { frames: 8, ancora: false, desc: 'collapsing from a fatal blow, side view facing right, legs giving way, ' +
    'body falling onto its side on the ground and lying still' },
  Attack: { frames: 6, ancora: false, desc: 'lunging forward and snapping its jaws at chest height, side view facing right, ' +
    'front paws pushing off, head thrusting forward with the mouth open, then pulling back to a crouched snarl, back paws stay planted' },
};

Object.assign(ELENCO, {
  Circe: {
    fase: '08', base: BASE_CIRCE, size: 64,
    // As rotações vieram 45° deslocadas: a 'east' sai de costas em 3/4; a 'south-east' é o perfil à direita.
    perfil: 'south-east',
    anims: { Idle: { frames: 6, ancora: true, desc: IDLE + ', the cup stays in her hand' } },
    desc: 'Circe the enchantress of Greek myth, tall graceful adult woman, long dark wavy hair falling to her waist ' +
      'crowned with a thin golden laurel wreath, ankle-length deep violet peplos with an olive-green woven border, ' +
      'a sheer pale green shawl draped over her arms, golden armbands and a golden snake bracelet, bare feet, ' +
      'holding a small golden kylix cup raised in one hand, the cup stays in her hand, serene knowing smile',
  },
  Circe_Wolf: {
    // v3 recusou quadrúpede. Standard (Circe_Wolf_std, 1 geração) saiu chapado, 18 cores, preto-carvão.
    // Pro com style_character_id = master do Odisseu: o recurso da API para casar estilo.
    fase: '08', base: BASE_CIRCE, size: 68, quadrupede: 'dog', modo: 'pro', estilo: '908b7f60-0624-43bc-a0cd-89ad16416400', anims: ANIM_LOBO,
    desc: 'enchanted grey wolf from the island of Circe in Greek myth, lean powerful wolf, grey-brown fur with a ' +
      'darker back, glowing pale green eyes, a thin golden collar with a small violet charm, snarling',
  },
});

// ------------------------------------------------------------------ Fase 09 — Mundo dos Mortos
// Em cena (falas + altares reservados pelo MundoDosMortosSceneDresser): as sombras junto ao sangue
// (DialogueTrigger_Shades) e a mãe, Anticleia (DialogueTrigger_Mother). Tirésias só é citado no desfecho.
// Inimigo: os EnemyBasic carmesim viram SOMBRAS DE GUERREIROS (a cena já os deixa com alpha 0,55).
// Paleta do submundo: cinza-azulado, branco pálido, violeta apagado — dessaturado, sem sangue, sem carmesim.
// A transparência é da Unity (cor do renderer): o sprite fica opaco e simples, sem efeito desenhado.
const BASE_MORTOS = 'side view, 16-bit pixel art game character, Greek mythology';

Object.assign(ELENCO, {
  Shade_Warrior: {
    fase: '09', base: BASE_MORTOS, size: 60, anims: ANIM_TROIANO,
    desc: 'ghostly shade of a dead Greek warrior from the underworld of Greek myth, pale grey-blue skin, ' +
      'a faded ragged tunic and a cracked bronze helmet all washed out in grey-blue and pale white tones, ' +
      'a small battered round shield on his back arm, a broken spear in his front hand, pale glowing eyes, ' +
      'sorrowful stern expression, ' + APEGO,
  },
  Shade: {
    fase: '09', base: BASE_MORTOS, size: 60,
    desc: 'ghostly shade of a nameless dead person from the underworld of Greek myth, a thin hooded figure ' +
      'wrapped in a long faded grey-blue burial shroud, pale grey-violet skin, pale glowing eyes, ' +
      'arms held close to the body, sorrowful longing expression',
  },
  Anticleia: {
    fase: '09', base: BASE_MORTOS, size: 60,
    desc: 'Anticleia, the ghost of the mother of Odysseus in Greek myth, elderly noble Greek woman, ' +
      'grey hair in a low bun under a pale translucent-looking veil, a long faded pale blue-grey peplos and mantle, ' +
      'pale grey skin, hands clasped in front of her, gentle sorrowful loving expression',
  },
});

// ------------------------------------------------------------------ Fase 10 — Sereias
// Nenhuma sereia na cena; o SereiasSceneDresser deixou "o vão do santuário e o espaço sobre as agulhas
// de rocha livres para elas". A direção do cenário manda: "o perigo não pode avisar — a armadilha É a
// beleza". Sereia ANTIGA (mulher-ave), não de cauda de peixe; bonita, não monstruosa. Sem instrumento: o
// código tem o canto (SirenZone, véu Siren_Song) e nenhuma lira — o canto fica na pose e no Idle.
// Paleta: turquesa, creme, branco, ouro (as rampas do cenário são quentes e turquesa). Sem carmesim.
const BASE_SEREIAS = 'side view, 16-bit pixel art game character, Greek mythology';

Object.assign(ELENCO, {
  Siren: {
    fase: '10', base: BASE_SEREIAS, size: 64,
    anims: { Idle: { frames: 6, ancora: true, desc: 'singing softly with her mouth open, head gently tilting, ' +
      'wings slowly opening a little and folding back, chest rising with each breath, hands open at her chest, ' +
      'in strict side profile facing right, head stays turned right in profile, no turning toward the viewer, feet stay planted' } },
    desc: 'a siren of Greek myth, beautiful young woman with large white and turquoise feathered bird wings ' +
      'folded behind her shoulders, long flowing sea-green hair with small white shells in it, ' +
      'soft turquoise and cream feathers covering her body from the waist down like a feathered dress, ' +
      'slender bird legs ending in small golden talons, a thin gold circlet, pale skin, ' +
      'gentle alluring smile, both hands open at her chest as if singing',
  },
});

// ------------------------------------------------------------------ Fase 11 — Cila e Caríbdis
// Cila = chefe (BossController, placeholder "Giant" que o dresser de cenário deixou como marcador), dentro
// da caverna escavada na falésia, ACIMA do estreito: golpeia 3 pontos do convés. Caríbdis = ENVIRONMENT
// (FX_Charybdis, folha polar de 8 quadros) — não ganha master.
// A Cila clássica: torso de mulher e, da cintura para baixo, pescoços de serpente-marinha com cabeças
// que mordem. O v3 só faz humanoide, e o torso é o que dá a ele um esqueleto.
// Paleta: azul-profundo, violeta, cinza-ardósia, acento de coral e osso — NÃO o turquesa das Sereias.
// Altura = a do marcador, 4,25 un -> size 192.
const BASE_CILA = 'side view, 16-bit pixel art game character, Greek mythology';

Object.assign(ELENCO, {
  Scylla: {
    fase: '11', base: BASE_CILA, size: 192,
    anims: {
      Idle: { frames: 6, ancora: true, desc: 'swaying slowly and menacingly, serpent necks coiling and uncoiling, ' +
        'serpent heads snapping their jaws, hair drifting, in strict side profile facing right, no turning toward the viewer' },
      // Uma geração, dois estados (como o Slam do Polifemo e o Throw do Lestrigão).
      Strike: { frames: 8, ancora: false, desc: 'rearing back with all serpent necks drawn up high, jaws open, holding for a moment, ' +
        'then lunging forward and down, every serpent head darting down to snatch prey below, side view facing right' },
    },
    desc: 'Scylla the sea monster of Greek myth, the upper body of a fierce woman with long wild dark violet-blue hair ' +
      'and slate-grey skin, from her waist downward a writhing mass of six long serpent necks each ending in a ' +
      'snapping sea-serpent head with pale bone teeth, deep blue and violet scales, coral-pink fins along the necks, ' +
      'clawed hands, menacing expression',
  },
});

// ------------------------------------------------------------------ Fase 12 — Gado do Sol
// O gado NÃO é personagem animado: são sprites estáticos de cenário, dois deles sobre os gatilhos
// SacredCattleZone (comer = fome cheia, 20 de dano, flag). Nenhum código aciona estado. O master existe
// porque o gado antigo era prop gerado em tamanho grande e encolhido a 0,38–0,55 no Transform (densidade
// quase 2× a do Odisseu).
// Base sem lugar: "ilha" no texto-base é desenhada sob o personagem (Fase 06).
const BASE_GADO = 'side view, 16-bit pixel art game character, Bronze Age Mycenaean Greek';

Object.assign(ELENCO, {
  Sun_Cattle: {
    // v3 recusa quadrúpede; o gabarito `horse` é o de casco e perna longa. Estilo casado com o Odisseu,
    // como o lobo da Circe. size 80: alvo ~76 px de comprimento (1,8 un) e cernelha abaixo do herói.
    fase: '12', base: BASE_GADO, size: 80, quadrupede: 'horse', modo: 'pro', estilo: '908b7f60-0624-43bc-a0cd-89ad16416400',
    // Sem animação: o gado é estático e nenhum código aciona estado. Uma pose 'pastando' chegou a ser
    // planejada e foi cortada por economia (2026-09-30). anims vazio = o 'animate' não dispara nada.
    anims: {},
    desc: 'sacred cattle of the sun god in Greek myth, a calm sturdy ox with a robust body and a deep chest, ' +
      'four sturdy legs with dark hooves, cream-white coat with soft warm golden-ochre shading, ' +
      'long pale ivory horns curving upward with golden tips, gentle dark eyes, a tail ending in a tuft, ' +
      'a thin golden collar, peaceful friendly look',
  },
});

// ------------------------------------------------------------------ Fase 13 — Calipso
// Calipso não tem figura nem fala na cena: é citada pelo Odisseu, e a fala da jangada ("Ela chora, mas
// obedece") a põe presente na despedida. Entra como figura de fundo junto à jangada, só com Idle (D-023).
// Separada da Circe (violeta, louros, taça): peplos branco, VÉU verde-jade sobre o cabelo (o véu é dela no
// canto 5), cinto dourado, mãos juntas e vazias. O jade vem da rampa Folhagem funda do cenário — não é o
// turquesa das Sereias. Base sem lugar e sem direção do olhar (lições das Fases 01 e 06).
const BASE_CALIPSO = 'side view, 16-bit pixel art game character, Greek mythology';

Object.assign(ELENCO, {
  Calypso: {
    fase: '13', base: BASE_CALIPSO, size: 64,
    // As rotações vieram 45° deslocadas: a 'east' é 3/4; a 'south-west' é o perfil verdadeiro, olhando para a
    // ESQUERDA. O Idle é pedido nessa direção e a cena espelha (flipX) quando ela precisa olhar para a direita.
    perfil: 'south-west',
    anims: {
      // A melancolia vai no Idle: nenhum código aciona Talk, Interact ou Cast nela.
      Idle: { frames: 6, ancora: true, desc: 'standing still and breathing softly, head slightly lowered in sorrow, ' +
        'veil and long hair stirring gently in a soft sea breeze, hands stay clasped in front of her, ' +
        'in strict side profile facing left, head stays turned left in profile, no turning toward the viewer, feet stay planted' },
    },
    desc: 'Calypso the immortal nymph of Greek myth, tall graceful adult woman with a serene melancholy face, ' +
      'long loose honey-brown hair falling past her waist, a sheer jade-green veil draped over the back of her head ' +
      'and down over her shoulders, ankle-length shimmering white peplos with a thin jade-green woven border, ' +
      'a slim golden belt at the waist, small golden earrings, bare feet, hands clasped gently in front of her, ' +
      'hands empty, calm sad gentle expression',
  },
});

// ------------------------------------------------------------------ Fases 14 e 15 — Ítaca Return e Pretendentes
// CastProbe: Eumeu (14) e Telêmaco (14 e 15) são NPCDialogue com a arte PINTADA (123–140 px/un); Penélope (15)
// reusa a v3 da Fase 01. Os EnemyBasic das duas fases são os pretendentes ("Um a um, os pretendentes recuam"):
// um só master. A paleta da 15 tem rampa Vinho (141,43,47) — vermelho colado ao carmesim da casa, NÃO usado.
// Pretendentes = açafrão e bronze (rampas Fogo/Bronze), guirlanda de videira, sem armadura, espada curta.
const BASE_ITACA = 'side view, 16-bit pixel art game character, Bronze Age Mycenaean Greek';
const APEGO_ESPADA = 'the short sword stays attached to his hand in every frame';
const ANIM_PRETENDENTE = {
  Idle: { desc: IDLE + ', the sword stays in his hand', frames: 6, ancora: true },
  Run: { frames: 8, ancora: false, desc: 'running forward fast, side view facing right, front knee lifted to hip height, ' +
    'back heel kicking up behind, body leaning slightly forward, short sword held forward in the front hand, ' + APEGO_ESPADA },
  Hit: { frames: 6, ancora: false, desc: 'recoiling from a blow to the chest, side view facing right, torso snaps backward ' +
    'and head jerks back, one foot steps back to catch balance, then returns to a guarded stance, ' + APEGO_ESPADA },
  Death: { frames: 8, ancora: false, desc: 'struck down, side view facing right, knees buckle and the body falls backward, ' +
    'landing flat on his back on the ground and lying still, the sword dropping beside him' },
  Attack: { frames: 6, ancora: false, desc: 'swinging the short sword in a wide arc from high above the shoulder down in front of him, ' +
    'side view facing right, front foot stepping in, then recovering to a guarded stance, ' + APEGO_ESPADA },
};

Object.assign(ELENCO, {
  Eumaeus: {
    // Idoso: 0,90–0,95 do Odisseu (D-012). O porqueiro fiel — rústico, gentil, cajado.
    fase: '14', base: BASE_ITACA, size: 60,
    anims: { Idle: { frames: 6, ancora: true, desc: IDLE + ', the staff stays in his hand' } },
    desc: 'Eumaeus the old swineherd of Ithaca, loyal kind elderly man, weathered tanned face, short grey beard and ' +
      'thinning grey hair, knee-length coarse brown woollen tunic with a rope belt, a rough goatskin cape over his shoulders, ' +
      'leather sandals, holding a plain wooden shepherd staff upright in his front hand, the staff stays attached to his hand, ' +
      'warm humble expression',
  },
  Telemachus_Adult: {
    // Adulto jovem, ~1,0 do Odisseu. Mesma família de cor da criança da Fase 01 (linho cru, barra carmesim:
    // a casa de Odisseu — só na barra, nunca dominante) e o cabelo castanho-avermelhado do pai.
    fase: '14', base: BASE_ITACA, size: 64,
    anims: { Idle: { frames: 6, ancora: true, desc: IDLE + ', the spear stays in his hand' } },
    desc: 'Telemachus son of Odysseus, a young man of twenty, lean and upright, short tousled dark reddish-brown hair ' +
      'and a short young beard like his father, knee-length off-white linen tunic with a thin crimson border and a leather belt, ' +
      'a sea-blue travelling cloak pinned at one shoulder, leather sandals, holding a tall spear upright in his front hand, ' +
      'the spear stays attached to his hand, determined earnest expression',
  },
  Suitor: {
    // Inimigo (EnemyBasic, colisor 1,2 un): 60 como o troiano. Nobres de festa, não soldados.
    fase: '15', base: BASE_ITACA, size: 60, anims: ANIM_PRETENDENTE,
    desc: 'arrogant young Greek nobleman, one of the suitors of Penelope, well-fed and smug, oiled black curly hair crowned ' +
      'with a garland of green vine leaves, fine knee-length saffron-yellow chiton with a bronze-colored meander border, ' +
      'a cream cloak thrown back over one shoulder, gold arm rings, no armour and no helmet, leather sandals, ' +
      'holding a short bronze sword in his front hand, the short sword stays attached to his hand, sneering expression',
  },
});

// ------------------------------------------------------------------ Final Polish — P-08
// Os parceiros de treino da Fase 01 (EnemyBasic) ainda usavam o lanceiro CARMESIM pintado. A facção certa é
// o soldado de Ítaca da própria fase: o Soldier_Ithaca ganha os 3 estados do EnemyAnimator que faltavam
// (Receita B, a de Troia, só com a lança). Nenhum master novo.
const APEGO_LANCA = 'the spear stays attached to his hand in every frame';
ELENCO.Soldier_Ithaca.anims = {
  Idle: { desc: IDLE, frames: 6, ancora: true },
  Run: { frames: 8, ancora: false, desc: 'running forward fast, side view facing right, front knee lifted to hip height, ' +
    'back heel kicking up behind, body leaning slightly forward, spear held level in the front hand, ' + APEGO_LANCA },
  Hit: { frames: 6, ancora: false, desc: 'recoiling from a blow to the chest, side view facing right, torso snaps backward ' +
    'and head jerks back, one foot steps back to catch balance, then returns to a guarded stance, ' + APEGO_LANCA },
  Death: { frames: 8, ancora: false, desc: 'struck down, side view facing right, knees buckle and the body falls backward, ' +
    'landing flat on his back on the ground and lying still, the spear dropping beside him' },
  Attack: { frames: 6, ancora: false, desc: 'thrusting the spear forward hard at chest height, side view facing right, ' +
    'front foot stepping in and body lunging forward, then pulling the spear back to a guarded stance, ' + APEGO_LANCA },
};

const faseDe = n => ELENCO[n].fase || '01';
const pasta = n => path.join(DIR, 'Fase' + faseDe(n), n);
const promptDe = n => `${ELENCO[n].desc}, ${ELENCO[n].base || BASE}`;
// Estados a animar: a Fase 01 só tinha Idle, com a Receita A.
const animsDe = n => ELENCO[n].anims || { Idle: { desc: IDLE, frames: 6, ancora: true } };

function ler() { return fs.existsSync(ESTADO) ? JSON.parse(fs.readFileSync(ESTADO, 'utf8')) : {}; }
function gravar(e) { fs.mkdirSync(DIR, { recursive: true }); fs.writeFileSync(ESTADO, JSON.stringify(e, null, 2)); }
const UUID = /[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/i;

async function criar(nome) {
  const r = await px.call('create_character', {
    description: promptDe(nome),
    name: `ODISSEIA_F${faseDe(nome)}_${nome}`,
    // Quadrúpede (o lobo da Circe) exige gabarito: bear, cat, dog, horse, lion.
    body_type: ELENCO[nome].quadrupede ? 'quadruped' : 'humanoid',
    ...(ELENCO[nome].quadrupede ? { template: ELENCO[nome].quadrupede } : {}),
    // O v3 não faz quadrúpede ('only produces humanoid characters'): o lobo vai de standard ou pro.
    mode: ELENCO[nome].modo || 'v3',
    size: ELENCO[nome].size,
    ...(ELENCO[nome].estilo ? { style_character_id: ELENCO[nome].estilo } : {}),
    view: 'side',
    outline: 'single color black outline',
    detail: 'medium detail',
  });
  const txt = px.textOf(r);
  const id = (txt.match(UUID) || [])[0];
  if (!id) throw new Error(`${nome}: sem character_id: ${txt.slice(0, 300)}`);
  return id;
}

async function baixar(url, arquivo) {
  const res = await fetch(url);
  if (!res.ok) throw new Error('download HTTP ' + res.status);
  fs.mkdirSync(path.dirname(arquivo), { recursive: true });
  fs.writeFileSync(arquivo, Buffer.from(await res.arrayBuffer()));
}

async function esperarRotacao(nome, id) {
  for (let i = 0; i < 90; i++) {
    const txt = px.textOf(await px.call('get_character', { character_id: id }));
    if (/status: failed/i.test(txt)) throw new Error(`${nome}: ${txt.slice(0, 300)}`);
    if (/status: completed/.test(txt)) {
      // _east.png é sempre o sprite de GAMEPLAY (perfil olhando para a direita), venha ele
      // da rotação east ou de outra — ver `perfil` no elenco.
      const perfil = ELENCO[nome].perfil || 'east';
      const url = d => (txt.match(new RegExp('\\b' + d + ':\\s*(https:\\/\\/\\S+)')) || [])[1];
      if (!url(perfil)) throw new Error(`${nome}: sem rotação ${perfil}`);
      const arq = path.join(pasta(nome), `${nome}_east.png`);
      await baixar(url(perfil), arq);
      if (perfil !== 'south' && url('south')) await baixar(url('south'), path.join(pasta(nome), `${nome}_south.png`));
      return arq;
    }
    await sleep(10000);
  }
  throw new Error(`${nome}: tempo esgotado`);
}

// Nome da animação no PixelLab. Quando o perfil não é a rotação east, o nome muda para não
// casar com uma animação de mesmo nome feita na direção errada.
const nomeAnim = (nome, estado) => (ELENCO[nome].perfil || 'east') === 'east' ? estado : estado + '_' + ELENCO[nome].perfil;

async function animar(nome, id, estado) {
  const a = animsDe(nome)[estado];
  const r = await px.call('animate_character', {
    character_id: id,
    action_description: a.desc,
    animation_name: nomeAnim(nome, estado),
    mode: 'v3',
    directions: [ELENCO[nome].perfil || 'east'],
    frame_count: a.frames,
    keep_first_frame: a.ancora,
    // Quadro inicial editado por código (o olho único do Polifemo): sem isto o v3 anima a partir da
    // rotação que ele mesmo gerou — a de dois olhos.
    ...(ELENCO[nome].inicio ? { custom_start_frame_base64: fs.readFileSync(path.join(ROOT, ELENCO[nome].inicio)).toString('base64') } : {}),
  });
  const txt = px.textOf(r);
  if (/^error/im.test(txt)) throw new Error(txt.slice(0, 200));
  return txt;
}

async function esperarAnimacao(nome, id, estado) {
  for (let i = 0; i < 90; i++) {
    const txt = px.textOf(await px.call('get_character', { character_id: id }));
    // Bloco "Idle — 1 dir (east), 7f ..." seguido da linha "east: url, url, ..."
    const dir = ELENCO[nome].perfil || 'east';
    const bloco = txt.split('\n');
    const k = bloco.findIndex(l => new RegExp('^\\s*' + nomeAnim(nome, estado) + ' — .*\\(' + dir + '\\)').test(l));
    if (k >= 0) {
      const linha = bloco.slice(k + 1).find(l => new RegExp('^\\s*' + dir + ':').test(l));
      const urls = linha ? linha.match(/https:\/\/[^,\s]+/g) : null;
      if (urls && urls.length) {
        for (let f = 0; f < urls.length; f++) {
          await baixar(urls[f], path.join(pasta(nome), `${estado}_east`, `${nome}_${estado}_${String(f).padStart(2, '0')}.png`));
        }
        return urls.length;
      }
    }
    await sleep(10000);
  }
  throw new Error(`${nome}: ${estado} não chegou`);
}

// Registro por estado. A Fase 01 gravava idleJob/idleFrames; lidos aqui como o estado Idle.
function registro(e, n) {
  e[n].anims = e[n].anims || {};
  if (e[n].idleJob && !e[n].anims.Idle) e[n].anims.Idle = { job: e[n].idleJob, frames: e[n].idleFrames };
  return e[n].anims;
}

(async () => {
  const [cmd, ...alvo] = process.argv.slice(2);
  const nomes = alvo.length ? alvo : Object.keys(ELENCO);
  const e = ler();
  for (const n of nomes) if (!ELENCO[n]) throw new Error('personagem desconhecido: ' + n);

  if (cmd === 'status') { console.log(JSON.stringify(e, null, 2)); return; }

  // O limite do PixelLab é de 8 jobs simultâneos, somando criação e animação.
  const tarefas = [];
  for (const n of nomes) {
    e[n] = e[n] || {};
    if (cmd === 'create' && !e[n].id) tarefas.push(async () => {
      e[n].id = await criar(n); e[n].size = ELENCO[n].size; e[n].prompt = promptDe(n);
      console.log(`[cast] ${n} criado: ${e[n].id}`);
    });
    if (cmd === 'fetch' && e[n].id && !e[n].east) tarefas.push(async () => {
      e[n].east = path.relative(ROOT, await esperarRotacao(n, e[n].id)).replace(/\\/g, '/');
      console.log(`[cast] ${n} rotação: ${e[n].east}`);
    });
    const reg = registro(e, n);
    for (const estado of Object.keys(animsDe(n))) {
      if (cmd === 'animate' && e[n].east && !reg[estado]) tarefas.push(async () => {
        const job = await animar(n, e[n].id, estado);
        reg[estado] = { job: job.slice(0, 300), prompt: animsDe(n)[estado].desc };
        console.log(`[cast] ${n} ${estado} disparado`);
      });
      if (cmd === 'fetchanim' && reg[estado] && !reg[estado].frames) tarefas.push(async () => {
        reg[estado].frames = await esperarAnimacao(n, e[n].id, estado);
        console.log(`[cast] ${n} ${estado}: ${reg[estado].frames} quadros`);
      });
    }
  }
  await Promise.all(tarefas.map(t => t().catch(err => console.error('[cast] FALHOU: ' + err.message))));
  gravar(e);
})();

