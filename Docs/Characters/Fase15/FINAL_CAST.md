# Fase 15 — Pretendentes (o final da jornada): elenco

**Extensão do `Docs/Characters/CHARACTER_ART_MASTER.md` (v1), que não foi alterado.** 2026-09-30.
Contém o **SUITOR CHARACTER MASTER v1**. Reusa a **Penélope v3 da Fase 01** e o **Telêmaco adulto da Fase 14**.

**Qual cena é esta.** O briefing chama de "Fase 15 — Final" o retorno com Penélope e Telêmaco. No projeto isso é
`Level_15_Pretendentes`. A campanha ainda tem `Level_16_Final`, que não foi auditada nesta etapa (as três falas
dela são do Odisseu). Decisão do usuário: fazer a 14, depois a 15 com este briefing.

- Rotações: `Fase15/_rot_pretendente.png` · tiras: `_tira_idle.png`, `_tira_run.png`, `_tira_hit.png`, `_tira_death.png`
- Capturas: `Fase15/_capturas/` (`antes_002|016|026`, `odisseu_inicio`, `odisseu_e_penelope`, `odisseu_e_telemaco`,
  `odisseu_e_pretendente`, `fim_da_fase`)
- Id e prompts: `Docs/Characters/cast.json` (`Suitor`)

## 1. CastProbe

`CastProbe.Run -probeScene Assets/Scenes/Levels/Level_15_Pretendentes.unity -probeArt NPC,Suitor,Pretend,Penelope,Telem,Figure,CHR_`

| CHARACTER | ROLE | FACTION | PREFAB | SPRITE (antes) | ANIMATOR | USED STATES | ACTION | PRIORITY |
|---|---|---|---|---|---|---|---|---|
| Odisseu | jogador | Casa de Odisseu | Player | `CHR_Odysseus` 42,857 | `PlayerAnimator` | os do master | reutilizado | 1 |
| Penélope | NPC (`NPCDialogue`, x 2) | Casa de Odisseu | nenhum | `CHR_NPC_Penelope` **pintada, 129 px/un** | nenhum | Idle | **reuso do master da Fase 01** | 2 |
| Telêmaco (adulto) | NPC (`NPCDialogue`, x 16) | Casa de Odisseu | nenhum | `CHR_NPC_Telemachus` **pintado, 140 px/un** | nenhum | Idle | **reuso do master da Fase 14** | 3 |
| Pretendentes ×4 | inimigo (`EnemyBasic`, x 24, 28, 32, 36) | Pretendentes | `EnemyBasic` | `CHR_Enemy_Basic` **carmesim, 100 px/un, escala 0,82** | `EnemyAnimator` | **Idle, Run, Hit, Death** | **SUITOR MASTER novo, override** | 1 |

Falas: Odisseu ("O salão do meu próprio palácio, tomado por homens…"), Penélope ("Vinte anos esperando… Se você é
mesmo Odisseu... prove."), Telêmaco ("Estou ao seu lado, pai. Sempre estive.") e o narrador ("Um a um, os
pretendentes recuam diante da justiça de Odisseu. O grande salão, finalmente, silencia.").

**Estados do Odisseu:** nenhum específico. Não há Victory, Celebrate, Hug, Sit nem pose final no código — o
`LevelGoal` (x 42, diante do trono) leva ao **WorldMap**; não existe transição para créditos nesta cena.
**Nada gerado para o Odisseu.**

**Penélope e Telêmaco:** só `NPCDialogue` parado — Idle. Nenhum Surprised, Happy, Hug ou Talk é acionado.

Animator: 0. Cutscene: nenhuma além das `DialogueSequence`. Animais e entidades: nenhum.

## 2. SUITOR CHARACTER MASTER v1

| | |
|---|---|
| Folha | `Assets/Resources/Odisseia/Enemies/CHR_Suitor.png` (25 quadros, célula 67 × 59) |
| PixelLab | `6aa164f4-223a-4ca6-9b62-453681c6e1b3` · v3 · size 60 · rotação `east` |
| Altura | 59 px = **1,38 un** (colisor do `EnemyBasic`: 1,2) |
| Silhueta | **guirlanda de folhas de videira** no cabelo preto encaracolado, **sem armadura e sem elmo** (o Odisseu trancou o arsenal), manto creme jogado para trás, **espada curta** |
| Paleta | **açafrão e bronze** (rampas `Fogo` e `Bronze` da fase), manto creme, braceletes de ouro |
| Estados | Idle 6 `[0,1,2,3,2,1]` · **Run 5 (quadros 3–7)** · Hit 6 · Death 8 |

**Não é vinho.** A paleta da fase tem a rampa `Vinho` (141, 43, 47) — "o que os pretendentes trouxeram". É um
vermelho colado ao carmesim da casa de Odisseu (D-014): um pretendente vermelho leria como homem da casa. O açafrão
é o luxo de festa, separa-se do carmesim, do índigo de Micenas, do oliva dos Cicones e do terracota de Troia, e lê
forte sobre a pedra fria do salão.

Prompt (base `side view, 16-bit pixel art game character, Bronze Age Mycenaean Greek`):

> arrogant young Greek nobleman, one of the suitors of Penelope, well-fed and smug, oiled black curly hair crowned
> with a garland of green vine leaves, fine knee-length saffron-yellow chiton with a bronze-colored meander border,
> a cream cloak thrown back over one shoulder, gold arm rings, no armour and no helmet, leather sandals, holding a
> short bronze sword in his front hand, the short sword stays attached to his hand, sneering expression

Animações (Receita A no Idle, Receita B no resto — as de Troia, com espada no lugar de lança e escudo):
- **Run:** running forward fast, side view facing right, front knee lifted to hip height, back heel kicking up
  behind, body leaning slightly forward, short sword held forward in the front hand, …
- **Hit:** recoiling from a blow to the chest, … then returns to a guarded stance, …
- **Death:** struck down, … knees buckle and the body falls backward, landing flat on his back … the sword dropping beside him

**Avaliação:** o Run saiu mais trote que corrida, mas lê como deslocamento; loop 3–7 pela regra da corrida. O Death
caiu **de bruços**, como em Troia e nos Cicones — aceito. Sem sangue.

## 3. Penélope — reuso

`CHR_Penelope` (Fase 01, v3, 58 px = 1,35 un, 0,97 do Odisseu, Idle 6). Linho cru e azul-mar — **a paleta do final
que o briefing pede** (creme, branco, azul), sem carmesim. **0 gerações.**

## 4. Identidade visual do final

Os três personagens do lar leem em **creme, linho, azul-mar e terra** (Penélope, Telêmaco, Eumeu); o carmesim da casa
fica restrito à capa do Odisseu e à barra fina da túnica do filho. A única massa de cor forte no salão é o açafrão
dos pretendentes — os intrusos, que são exatamente o que a fase remove. Não houve mudança de cenário: o salão
noturno com estandartes cor de vinho é do `PretendentesSceneDresser` e ficou como estava.

## 5. Integração — SCENE OVERRIDE, SORTING

`IthacaReturnCastDresser.Dress15` / `Poses15` (mesmo dresser da Fase 14):
- Penélope e Telêmaco: sprite do `Body`, escala 1, `flipX`, **ordem −1** (corrigido no Final Polish; era 0, empatada com o jogador), `SpriteAnimator` Idle, pés em **y = −2,00**.
- Pretendentes: `EnemyFactionOverride.Aplicar` — folha, escala 1, **ordem 2**. **Prefab `EnemyBasic` intacto.**
- Primeiro plano sobre NPC: nenhum nesta cena.

**Sorting.** Todos os renderers usam a sorting layer `Default`; a ordem é pela `Order in Layer`:

| Plano | Ordem |
|---|---|
| Fundo / parede do salão | ≤ −18 |
| NPCs (Penélope, Telêmaco) | **−1** |
| Jogador (`Body` do prefab) | **0** |
| Pretendentes | **2** |
| Primeiro plano | ≥ 12 |

**Correção (Final Polish, 2026-09-30):** a primeira versão deste documento dizia que o jogador estava na ordem 2 e
punha os NPCs em 0. Medido pelo `GlobalCastAudit` nas 16 cenas, o `Body` do jogador está na **ordem 0** (o 2 do
prefab é o `ShieldVisual`) — os NPCs estavam EMPATADOS com ele. Foram para −1. O dresser agora lista o cenário grande
que se desenha por cima do jogador (ordem 0–9); nesta cena, nenhum além do checkpoint. A jangada da Fase 13 foi corrigida
no Final Polish (ordem 2 → −1).

## 6. Ferramenta nova

`Assets/Scripts/Editor/SortingNearProbe.cs` — lista quem é desenhado em volta de um x (ordem, layer, caixa). Foi o
que achou a grama sobre os NPCs da 14. Exige `-probeScene` e `-probeX` e sai com erro sem eles (sem default).

## 7. Validação (fases 14 e 15)

| Teste | Resultado |
|---|---|
| CastProbe | Eumeu 1,33, Telêmaco 1,47 (com a lança), Penélope 1,35, pretendentes 1,38 un — todos a 42,857 px/un, escala 1 |
| `PlaceholderProbe` | OK nas duas — nada visível, nada inativo |
| `CampaignValidation` | passa (exit 0, 0 erros de compilação) |
| Build WebGL | **Build Finished, Result: Success** (exit 0) |
| Capturas | Odisseu no início; com Penélope; com Telêmaco; com um pretendente; o fim da fase diante do trono |

**Não testado:** play mode (diálogos, combate, `LevelGoal` → WorldMap), navegador, mobile, gamepad. Não há tela de
créditos nesta cena; a `Level_16_Final` não foi auditada.

## 8. Custo

Pretendente 2 + Idle 1 + Run 1 + Hit 1 + Death 1 = **6 gerações**. Com a Fase 14 (6): **12** (830 → 842 de 2000).
Nenhum descarte. O Hit estourou o tempo de download uma vez e foi baixado de novo sem custo.

## 9. Pendências

- **`Level_16_Final` não auditada** (Character Art da última cena da campanha).
- **`EnemyBasic` carmesim** continua só na Fase 01.
- Globais: Cila "pairando"; Caríbdis 0,42; palácio de Éolo 0,52; porco da Circe como tinta rosa; rebanho em fila e
  xadrez pintado na árvore (Fase 12); **Odisseu atrás da jangada (Fase 13)**; disfarce de mendigo sem visual e
  contorno azul nas trepadeiras (Fase 14).
