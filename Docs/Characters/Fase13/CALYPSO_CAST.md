# Fase 13 — Calipso: elenco

**Extensão do `Docs/Characters/CHARACTER_ART_MASTER.md` (v1), que não foi alterado.** 2026-09-30.
Contém o **CALYPSO CHARACTER MASTER v1**.

- Master e rotações: `Fase13/_master_rotacoes.png` · tira do Idle: `_tira_idle.png`
- Capturas: `Fase13/_capturas/` (`calipso_idle`, `calipso_e_odisseu`, `despedida_pico_idle`,
  `odisseu_no_gatilho_da_jangada`, `ogigia_-08|10|20|36`)
- Id e prompts: `Docs/Characters/cast.json` (`Calypso`)

## 1. CastProbe

`CastProbe.Run -probeScene Assets/Scenes/Levels/Level_13_Calipso.unity -probeArt Calypso,Calipso,NPC,Figure`

| Entidade | Implementação encontrada | Classificação |
|---|---|---|
| **Calipso** | **nenhuma figura, nenhuma fala.** Só é citada pelo Odisseu | personagem narrativa **sem corpo** |
| Odisseu | Player, master v1; nenhum estado especial (sem Talk, Sit, Interact ou cutscene) | reutilizado |
| `DialogueTrigger_Grove` (x 9..11) | Odisseu: "Calipso teceu junto ao tear, cantando. Por um instante, quase esqueci Penélope." (passado) | gatilho sem figura |
| `DialogueTrigger_Raft` (x 27..29) | Odisseu: "Hermes trouxe a ordem de Zeus: Calipso deve me deixar partir. **Ela chora, mas obedece.**" | gatilho sem figura |
| Intro / outro | Odisseu ("Sete anos. Calipso me oferece imortalidade…") / narrador | — |
| Hermes | só citado | não entra |
| NPCs, tripulação, inimigos, criaturas, animais | **nenhum** | — |
| Animator / Controllers / Clips | **0** | — |
| Efeitos | nenhum efeito mágico ligado à Calipso. `DuskVeil` (degradê do pôr do sol) e as `ScrollingLayer` do mar são cenário | não duplicados no sprite |

**Por que ela entra:** a fala da jangada, no presente, põe a Calipso na despedida — o mesmo critério que pôs
Anticleia (D-036), Circe e Éolo em cena (D-023, D-027). Hermes não entra: só é citado.

| CHARACTER | ROLE | FACTION | PREFAB | SPRITE | ANIMATOR | USED STATES | CURRENT ASSET | ACTION | PRIORITY |
|---|---|---|---|---|---|---|---|---|---|
| Calipso | figura narrativa | Calipso (própria) | nenhum | `CHR_Calypso` | `SpriteAnimator` | **Idle** | nenhum | **master novo** | 1 |
| Odisseu | jogador | Casa de Odisseu | Player | `CHR_Odysseus` | `PlayerAnimator` | os do master | master v1 | reutilizado | — |

**Estados:** só **Idle**. Nenhum código aciona Talk, Interact, Cast, Walk, Run, Hit ou Death nela, e o sistema
de diálogo não mostra retrato. A regra do Run no quadro 3 não se aplica.

## 2. CALYPSO CHARACTER MASTER v1

| | |
|---|---|
| Folha | `Assets/Resources/Odisseia/Characters/NPCs/CHR_Calypso.png` (10 quadros, célula 34 × 60) |
| PixelLab | `ad432a38-9a93-4974-be76-58f274724e9b` · v3 · size 64 · **perfil = rotação `south-west`** (olha para a esquerda) |
| Altura | 60 px = **1,40 un**, igual ao Odisseu e à Circe (imortal, figura de presença) |
| PPU / pivô | 42,857 · BottomCenter, pés na última linha |
| Silhueta | **véu longo caindo da cabeça até o meio das costas** — ninguém mais no elenco tem. Cabelo solto até a cintura, peplos reto até os tornozelos, mãos juntas e vazias |
| Paleta | peplos **branco**, **véu e barra verde-jade** (rampa `Folhagem funda` do cenário), **cinto e brincos dourados**, cabelo **mel**. Sem carmesim |

### Distinta da Circe (e das Sereias)

| | Circe | Calipso |
|---|---|---|
| Cor dominante | **violeta** | **branco + verde-jade** |
| Cabeça | cabelo solto com **coroa de louros** | **véu** sobre o cabelo |
| Mãos | **taça dourada erguida** | **juntas, vazias** |
| Postura | serena, sorriso de quem sabe | cabeça que **baixa**; triste |
| Magia | vapor na taça | nenhuma |

Das Sereias (turquesa, penas, asas) ela se separa pela forma — véu e peplos, sem asa — e pelo verde, que é jade
e não turquesa.

Prompt (base `side view, 16-bit pixel art game character, Greek mythology` — sem lugar e sem direção do olhar):

> Calypso the immortal nymph of Greek myth, tall graceful adult woman with a serene melancholy face, long loose
> honey-brown hair falling past her waist, a sheer jade-green veil draped over the back of her head and down over
> her shoulders, ankle-length shimmering white peplos with a thin jade-green woven border, a slim golden belt at
> the waist, small golden earrings, bare feet, hands clasped gently in front of her, hands empty, calm sad gentle
> expression

Idle (6 quadros, âncora; pedido na direção `south-west`):

> standing still and breathing softly, head slightly lowered in sorrow, veil and long hair stirring gently in a
> soft sea breeze, hands stay clasped in front of her, in strict side profile facing left, head stays turned left
> in profile, no turning toward the viewer, feet stay planted

**A melancolia está no Idle:** a cabeça desce devagar e o véu se ergue na brisa até o quadro 5; o 6 já volta.
Folha em pingue-pongue `0,1,2,3,4,5,4,3,2,1` a 4 FPS. Rosto, cabelo, roupa e acessórios estáveis em todos os quadros.

**Rotações deslocadas 45°** (como a Circe): a `east` veio em 3/4. A `south-west` é o perfil verdadeiro; a folha
olha para a esquerda e a cena espelha.

## 3. Integração — SCENE OVERRIDE

`Assets/Scripts/Editor/CalypsoCastDresser.cs`:
- `Dress` — raiz `CalypsoCast` → `Calypso/Body` com `SpriteRenderer` (Idle_00, **flipX**: olha para a jangada e
  para o Odisseu) e `SpriteAnimator` (Idle, 4 FPS). **x = 23,07** (2,8 un à esquerda da jangada), pés em
  **y = −2,00**, ordem 0 (na frente do palácio, −18; atrás da jangada e do jogador, 2). Sem colisor, sem fala
  nova, sem script.
- `Poses` — fotos sem salvar a cena.

**Posição corrigida uma vez:** a 1,0 un da jangada, a samambaia de primeiro plano `FG_Ferns_3` (x 23,6..26,4,
ordem 12) tapava os pés dela, e figura sem pés à vista não lê apoiada no chão. Recuou para 2,8 un.

**Intocados:** `DialogueTrigger`, `DialogueSequence`, `CalipsoSceneDresser`, nenhum prefab, `PlayerAnimator`.

## 4. Validação

| Teste | Resultado |
|---|---|
| CastProbe | Calipso a 42,857 px/un, escala 1, x 22,68..23,47, y −2,00..−0,60 |
| `PlaceholderProbe` | OK — nada visível, nada inativo |
| `CampaignValidation` | passa (exit 0, 0 erros de compilação) |
| Build WebGL | **Build Finished, Result: Success** (exit 0) |
| Capturas | Calipso em Idle diante do palácio; Calipso e Odisseu frente a frente, no mesmo chão e na mesma altura; o pico do Idle; a ilha |

**Não testado:** play mode (os dois diálogos), navegador, mobile, gamepad.

## 5. Custo

**3 gerações** (827 → 830 de 2000): master 2 + Idle 1. Nenhum descarte.

## 6. Pendências

- **O Odisseu some atrás da jangada no gatilho da despedida.** A jangada (`CalipsoScenery/Raft`) está na ordem 2,
  a mesma do jogador; no gatilho (x 27..29) o empate o esconde (captura `odisseu_no_gatilho_da_jangada`).
  ENVIRONMENT ART / SORTING — não tocado.
- **Props encolhidos no Transform** (palácio 0,72, jangada 0,62, fonte 0,55, árvores 0,55–0,80…) — ENVIRONMENT
  ART, mesmo padrão de densidade das outras fases.
- Globais: Cila "pairando" (CHARACTER ART / SILHOUETTE); Caríbdis 0,42 (ENTITY ART); palácio de Éolo 0,52
  (ENVIRONMENT ART); porco da Circe como tinta rosa; `EnemyBasic` carmesim nas fases 01, 14 e 15; rebanho em fila
  e xadrez pintado na árvore (Fase 12).
