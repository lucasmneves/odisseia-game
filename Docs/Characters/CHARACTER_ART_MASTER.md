# CHARACTER ART MASTER — Odisseia

**Fase 01 (Itaca Prologue) como referência de personagens. Aprovado em 2026-09-29.**

Este documento é a régua para todo personagem futuro: principais, NPCs, soldados, inimigos,
chefes. O que está aqui foi **medido** nos sprites que estão no jogo, não escolhido.

- Master do protagonista (detalhe completo, 16 estados): `Docs/CharacterMaster_Odysseus/ODYSSEUS_MASTER_REFERENCE.md`
- Prancha de escala e cor: `Docs/Characters/CAST_LINEUP.png`
- Teste de silhueta: `Docs/Characters/CAST_SILHOUETTE.png`
- Capturas na fase: `Docs/Characters/Fase01/_capturas/`
- Ids do PixelLab e prompts exatos: `Docs/Characters/cast.json`

---

## 1. Auditoria da Fase 01 — o que havia

| Personagem | Papel | Asset anterior | Problema medido | Decisão |
|---|---|---|---|---|
| Odisseu | Player | `CHR_Odysseus` (16 estados, 99 quadros) | nenhum | **Mantido** — já é o master |
| Penélope | NPC, 2 cenas | `CHR_NPC_Penelope` pintada, 6.927 cores, 129 px/un | borrava; mostrada a **1,7 un, mais alta que o Odisseu** (1,4) | **Recriada** |
| Telêmaco | NPC, 2 cenas | `CHR_NPC_Telemachus` pintado, 140 px/un | borrava; adolescente numa fase em que ele é criança | **Recriado** como criança |
| Arauto de Micenas | NPC | figurante `soldier` genérico | igual aos soldados de Ítaca; o mensageiro de Agamenon não se distinguia | **Recriado** |
| Instrutor do treino | Figura | arte do **Alcínoo** (rei feácio), 1,8 un | personagem de outra fase, manto azul, gigante | **Recriado** |
| Villager_1 / _2 | Figuras | arte do **Bardo** e do **Eumeu**, pintadas | desfaziam-se em buracos na densidade do jogo | **Substituídos** por Mulher e Mercador |
| Soldados (10) | Figuras e recrutas | `ithaca_villager_soldier`, modo `standard` | cabeçudo, chapado ao lado do master | **Recriado** (Soldier master) |
| Recrutas civis (8) | NPCs | 4 arquétipos `standard` | idem | **Recriados** em v3 + variações |
| Marinheiros do porto (3) | Figuras | `standard` | idem | **Recriados** + variações |
| Agamenon | — | **não aparece na fase** | — | **Não criado**; o arauto é a presença dele |

Nenhum NPC tinha animação: todos eram sprite parado.

## 2. Resultado

| Folha (`Assets/Resources/Odisseia/Characters/NPCs/`) | Altura | × Odisseu | Idle | Usado por |
|---|---|---|---|---|
| `CHR_Penelope` | 58 px = 1,35 un | 0,97 | 6 quadros | NPC_Penelope, NPC_Penelope_Port |
| `CHR_Telemachus_Child` | 42 px = 0,98 un | 0,70 | 6 | NPC_Telemaco, NPC_Telemaco_Port |
| `CHR_Herald_Mycenae` | 62 px = 1,45 un | 1,03 | 6 | NPC_Herald |
| `CHR_Trainer_Veteran` | 62 px | 1,03 | 6 | NPC_Trainer |
| `CHR_Soldier_Ithaca` | 62 px | 1,03 | 6 | Soldier_1/2, Soldier_Drill_0..5, Recruit_Eurylochus, Recruit_Watchman |
| `CHR_Villager_Woman` | 58 px | 0,97 | 6 | Villager_1 |
| `CHR_Villager_Merchant` | 62 px | 1,03 | 6 | Villager_2, Recruit_Blacksmith, NPC_Blacksmith |
| `CHR_Villager_Fisherman` | 62 px | 1,03 | 6 | Recruit_Fisherman |
| `CHR_Villager_Farmer` | 62 px | 1,03 | 6 | Recruit_Farmer, Recruit_Shepherd |
| `CHR_Villager_Sailor` | 62 px | 1,03 | 6 | Recruit_Sailor, Sailor_1 |
| `CHR_Villager_Sailor_Ochre` | 62 px | 1,03 | 6 | Recruit_Rower, Sailor_2 *(variação por cor)* |
| `CHR_Villager_Sailor_Olive` | 62 px | 1,03 | 6 | Recruit_Elpenor, Sailor_3 *(variação por cor)* |
| `CHR_Villager_Elder` | 56 px = 1,31 un | 0,93 | 4 | biblioteca (sem uso na 01) |
| `CHR_Villager_Elder_Brown` | 56 px | 0,93 | 4 | Recruit_Carpenter *(variação por cor)* |

30 figuras da fase trocadas, todas com Idle animado. **38 gerações** (616 → 654 de 2000).

---

## 3. Escala — regra

A régua é o Odisseu: **60 px de corpo = 1,4 un**, porque o `BoxCollider2D` do Player é 1,4 e o
level design inteiro assume isso. Daí **42,857143 px por unidade**, sem exceção.

| Categoria | Alvo × Odisseu | Medido na 01 |
|---|---|---|
| Adulto | 0,95 – 1,05 | 0,97 (mulheres) · 1,03 (homens) |
| Soldado / guarda | 1,00 – 1,10 | 1,03 |
| Idoso encurvado | 0,90 – 0,95 | 0,93 |
| Criança (~7 anos) | 0,65 – 0,75 | 0,70 |
| Chefe / gigante | livre — a escala é narrativa | — |

**Nunca escalar pelo Transform.** Personagem é importado a 42,857 e a cena usa a altura nativa
(`Npc()`/`FiguraDoElenco()` fazem isso). Escala no Transform é o que deixava a Penélope a 1,7 un:
a arte pintada a 129 px/un era esticada até uma altura digitada à mão.

### Como acertar a altura no PixelLab (v3)

O `size` é o canvas pedido e o conteúdo sai em **~0,94 – 0,97 dele**, e precisa ser
**múltiplo de 4** (58 e 62 são recusados — erro de validação, não cobrado):

| `size` | Conteúdo obtido |
|---|---|
| 64 | 60 – 62 px (adulto masculino) |
| 60 | 56 – 58 px (mulher, idoso) |
| 44 | 41 – 42 px (criança) |

**Medir sempre depois.** A primeira Penélope a 64 saiu com 62 px, mais alta que o Odisseu, e foi
refeita a 60 (2 gerações). Pixel art não se reduz: um sprite alto demais é regerado, nunca encolhido.

## 4. Pixel density, contorno e forma

- **42,857 px/un**, filtro Point, compressão desligada, `alphaIsTransparency`, pivô **BottomCenter**
  com os pés na última linha da célula.
- **Contorno preto de cor única** (`single color black outline`), alpha duro, zero pixel semitransparente
  (medido nos 77 quadros gerados).
- **Sombreamento de forma**, sem luz direcional — a regra do master (seção 4 do master do Odisseu).
- 40 a 65 cores por personagem, na família do master (51). Pintura com milhares de cores é outro
  pipeline e não entra no jogo.
- Vista **`side`**, olhando para a direita. O código espelha (`flipX`) para a esquerda.
- Proporção ≈ 5,5 cabeças no adulto; a criança tem cabeça proporcionalmente maior, sem virar chibi.

## 5. Paleta de personagem — cor carrega significado

A paleta do Odisseu (28 cores, 7 rampas, `Docs/CharacterMaster_Odysseus/Palette/`) é a base.
Os NPCs não são quantizados nela: ela não tem off-white, e quantizar joga o linho na rampa de
pele (medido em `build-npcs.js`). As cores de roupa seguem estas famílias:

| Família | Uso | Por quê |
|---|---|---|
| **Carmesim** | só a casa de Odisseu: capa dele, barra da túnica do Telêmaco, crista do instrutor | liga o filho ao pai sem copiar o pai |
| **Índigo + ouro** | só Micenas (o arauto) | "vem de fora e manda": nenhum ilhéu veste isso |
| Off-white de linho, areia | povo de Ítaca, Penélope | a cor-base da ilha |
| Azul-mar | Penélope (manto), marinheiro, instrutor | o mar de Ítaca |
| Verde-oliva, marrom, ocre | trabalhadores | terra e oliveira |
| Bronze | armadura, broches, pontas de lança | igual ao do master |
| Neon, saturação alta | **proibido** | |

Regra para fases futuras: **uma família de cor por facção**, reservada. Os troianos, os cícones,
os feácios ganham a sua; o carmesim continua sendo só da casa de Odisseu.

## 6. Silhueta

Teste em `CAST_SILHOUETTE.png`, preto chapado em 3× e em 1×. Em tamanho real se distinguem:

- **Odisseu** — o mais largo: capa aberta atrás e espada à frente
- **Penélope** — coluna estreita e reta, sem nenhum objeto
- **Telêmaco** — 0,7 da altura, cabeça grande
- **Arauto** — manto largo até os pés + cajado mais alto que a cabeça
- **Soldado** — estreito, com a lança; **Instrutor** — o único com crista

Regra: **cada personagem nomeado precisa de UM elemento de silhueta que nenhum outro tem**
(capa, cajado alto, crista, altura). Cor não conta — o teste é em preto.

## 7. NPC Design System

Poucos arquétipos gerados, muitos personagens na tela.

**Corpo / cabeça:** arquétipos por idade e ofício — adulto magro (pescador, marinheiro), adulto
robusto (lavrador, mercador), mulher, idoso encurvado, criança. Cada um é **um** `create_character` v3.

**Cabelo / acessórios:** vêm do arquétipo (chapéu de palha do lavrador, lenço da mulher, cajado
do idoso). Mudar isso exige outra geração.

**Roupa e cor:** variam **por código**, de graça, com `Tools/cast-variants.js`: gira o matiz de uma
faixa, preservando a luminância de cada pixel (a sombra da roupa continua a mesma) e nunca tocando
contorno nem tons neutros.

**Onde a variação por cor funciona é medido, não escolhido.** Pele, cabelo castanho, linho, couro
e túnica marrom caem todos em 0–30° de matiz. Recolorir o lavrador ou o pescador pintaria o rosto.
Os azuis (marinheiro 165–215°, manto do idoso 210–240°) são exclusivos da roupa — por isso **gere
os arquétipos com a roupa numa cor que a pele não tem** (azul, verde, índigo), e as variações saem de graça.

**Soldados:** um Soldier master uniformizado. Soldados *devem* parecer iguais; a variação que
importa é de patente (o instrutor tem crista e cuirassa de bronze).

## 8. Animação dos NPCs

- NPCs usam o **mesmo `SpriteAnimator` do Odisseu** (`Resources`, sprites `<Folha>_<Estado>_<NN>`),
  sem Animator nem AnimationClip. Nenhum código de runtime novo.
- **Idle**: `animate_character` v3, direção única, Receita A do master (`keep_first_frame: true`,
  trava de orientação), 1 geração.
- **Pingue-pongue**: dos 7 quadros gerados usam-se `0,1,2,3,2,1`. Respiração é simétrica, a
  emenda do loop some, e os quadros do fim — onde o v3 costuma **girar o rosto para a câmera**
  (lavrador 4–6, ancião 3–6) ou **soltar o objeto da mão** (instrutor 5–6) — ficam de fora.
  Conferir a tira (`Fase01/_idle_tiras_*.png`) antes de escolher o pico.
- **FPS 5,5 – 6,7**, variando com a posição x: sem isso os seis soldados do treino respiram em
  uníssono.
- **Quem conversa olha para a esquerda** (de onde o jogador chega). Figuras de fundo olham para
  onde a cena pede (os soldados do treino olham o instrutor).
- **Sem Walk:** nenhum NPC da fase se move; o `NPCDialogue` é estático. Gerar caminhada seria gasto
  sem uso.
- **Sem expressões/retratos:** o sistema de diálogo não exibe retrato. Se um dia exibir,
  `create_portrait_character` (20 gerações) converte o sprite em busto preservando a identidade.

## 9. Processo de geração (PixelLab)

`Tools/build-cast.js` guarda o estado em `cast.json` e cada passo só age em quem ainda não passou
por ele — rodar de novo nunca cobra duas vezes.

```
node Tools/build-cast.js create    [nome...]   # 2 gerações cada
node Tools/build-cast.js fetch     [nome...]   # baixa a rotação de gameplay
node Tools/build-cast.js animate   [nome...]   # Idle, 1 geração cada
node Tools/build-cast.js fetchanim [nome...]
node Tools/build-cast-sheets.js                # folhas + .meta no Unity
node Tools/cast-lineup.js                      # pranchas de validação
```

Depois: `PrologueSceneBuilder.Build`, e a bateria `PrologueProbe`, `PlaceholderProbe`,
`OdysseusSheetProbe`, `CampaignValidation` e a captura `PrologueScreenshot.Capture`.

### Estrutura do prompt

```
<quem é, idade, porte>, <cabelo e rosto>, <roupa: peça, comprimento, material, cor>,
<acessórios>, <objeto na mão + "stays attached to his hand">, <expressão>,
side view, 16-bit pixel art game character, Bronze Age Mycenaean Greek, Mediterranean island of Ithaca
```

Parâmetros: `mode: v3`, `view: side`, `outline: single color black outline`, `detail: medium detail`.
Prompts exatos de cada personagem em `cast.json`.

### O que aprendi nesta rodada

- **Não escreva a direção do olhar na descrição.** "in strict side profile facing right" fez o v3
  desenhar a vista *south* já de perfil, e as oito rotações giraram 90°: a *east* saiu **de costas**.
  A saída foi usar a south como sprite (`perfil: 'south'` no elenco) — mas o Idle já disparado na
  east foi 1 geração perdida. Conferir a rotação antes de animar.
- **Rosto de criança vira anime** se não for nomeado. "young boy, larger head" deu olhos enormes e
  rubor, lendo como menina. "simple small dot eyes and a small nose, plain determined little face" corrigiu.
- **Objeto na mão precisa da cláusula de apego** ("the staff stays attached to his hand") — a mesma
  regra do master. Mesmo com ela, o instrutor solta o cajado no fim do ciclo; por isso o pingue-pongue.
- **Limite de 8 jobs simultâneos** vale para criação + animação somadas. O excedente volta com erro,
  sem cobrança.
- `create_character` em **`pro` com `style_character_id`** (o master) seria o caminho de maior
  fidelidade de estilo, mas custa 20–40 e exige canvas ≥ conteúdo do master (60 px) — o que impede a
  criança. Como o master foi feito em **v3**, v3 já é o mesmo pipeline.

## 10. Convenção de nomes e pastas

| O quê | Onde | Nome |
|---|---|---|
| Folha de NPC (jogo) | `Assets/Resources/Odisseia/Characters/NPCs/` | `CHR_<Nome>.png`, sprites `CHR_<Nome>_<Estado>_<NN>` |
| Folha do Odisseu | `Assets/Resources/Odisseia/Characters/` | `CHR_Odysseus.png` |
| Fonte do PixelLab | `Docs/Characters/Fase<NN>/<Nome>/` | `<Nome>_east.png`, `Idle_east/<Nome>_Idle_<NN>.png` |
| Estado e prompts | `Docs/Characters/cast.json` | chave = `<Nome>` |
| Nome no PixelLab | — | `ODISSEIA_F<NN>_<Nome>` |

`<Nome>` em inglês, `PascalCase` com `_` separando papel e variação: `Villager_Sailor_Ochre`,
`Telemachus_Child`. Identificadores ASCII (acento é frágil no `JsonUtility` e no Unity).

A pasta sugerida `Assets/Art/Characters/<Nome>/{Idle,Walk,...}` **não foi criada**: o projeto já tem
a convenção acima, e o `SpriteAnimator` carrega de `Resources`.

## 11. O que ficou de fora, e por quê

- **Odisseu não foi regerado.** O master de 16 estados foi aprovado, está integrado e tem gatilho
  em todos os estados. Regerar trocaria o personagem que o jogador já conhece por outro.
- **A arte pintada (`CHR_NPC_*`) não foi apagada:** as fases 14 (Telêmaco, Eumeu) e 15 (Penélope,
  Telêmaco) e a cena fora da campanha dos Feácios ainda a usam. Trocar lá é outra tarefa — e o
  Telêmaco dessas fases é adulto, então precisa de um personagem novo, não deste.
- **Os figurantes `standard` antigos** (`Assets/Art/Environments/Ithaca/Figures/ithaca_villager_*`)
  ficaram sem uso na 01 e não são usados em nenhuma outra cena. Mantidos para não apagar arte sem pedido.
- **Agamenon** não entra na Fase 01. Quando entrar (Troia), a regra é: índigo e ouro de Micenas,
  mais largo que o Odisseu, com um elemento de silhueta próprio (manto real + cetro).
