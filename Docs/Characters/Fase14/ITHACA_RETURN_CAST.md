# Fase 14 — Ítaca Return: elenco

**Extensão do `Docs/Characters/CHARACTER_ART_MASTER.md` (v1), que não foi alterado.** 2026-09-30.
Contém o **EUMAEUS CHARACTER MASTER v1** e o **TELEMACHUS (ADULT) CHARACTER MASTER v1**. O **SUITOR CHARACTER
MASTER v1** está documentado em `Fase15/FINAL_CAST.md` e é usado aqui.

- Rotações: `Fase14/_rot_eumeu.png`, `_rot_telemaco.png` · tiras: `_tira_eumeu.png`, `_tira_telemaco.png`
- Capturas: `Fase14/_capturas/` (`antes_000|008|028`, `odisseu_inicio`, `odisseu_e_eumeu`, `odisseu_e_telemaco`,
  `odisseu_e_pretendente`, `fim_da_fase`)
- Ids e prompts: `Docs/Characters/cast.json` (`Eumaeus`, `Telemachus_Adult`)

## 1. CastProbe

`CastProbe.Run -probeScene Assets/Scenes/Levels/Level_14_Itaca_Return.unity -probeArt NPC,Suitor,Pretend,Penelope,Telem,Figure,CHR_`

| CHARACTER | ROLE | FACTION | PREFAB | SPRITE (antes) | ANIMATOR | USED STATES | ACTION | PRIORITY |
|---|---|---|---|---|---|---|---|---|
| Odisseu | jogador | Casa de Odisseu | Player | `CHR_Odysseus` 42,857 | `PlayerAnimator` | os do master | reutilizado | — |
| Eumeu | NPC (`NPCDialogue`, x 0) | Ítaca | nenhum | `CHR_NPC_Eumaeus` **pintado, 123 px/un** | nenhum | Idle | **master novo** | 1 |
| Telêmaco (adulto) | NPC (`NPCDialogue`, x 28) | Casa de Odisseu | nenhum | `CHR_NPC_Telemachus` **pintado, 140 px/un** | nenhum | Idle | **master novo** | 1 |
| Pretendentes ×2 | inimigo (`EnemyBasic`, x 8 e 16) | Pretendentes | `EnemyBasic` | `CHR_Enemy_Basic` **carmesim, 100 px/un, escala 0,82** | `EnemyAnimator` | Idle, Run, Hit, Death | **override** (SUITOR MASTER) | 1 |

Falas: Odisseu ("Ítaca. Depois de vinte anos…"), Eumeu ("Um mendigo, hein? Sente-se, estranho…"), Telêmaco, e o
reconhecimento (narrador + Telêmaco: "É hora. Vamos reaver o que é seu.").

**Disfarce de mendigo:** `DisguiseEffect.startDisguised` está ligado nesta cena, mas o disfarce só troca a
**camada de física** (os inimigos não o detectam). Não há sprite de mendigo e nenhum código trocaria a folha:
**não gerado** (seria estado novo no `PlayerAnimator`, mudança de sistema). Registrado como oportunidade.

Animator: 0. Nenhuma cutscene além das `DialogueSequence`.

## 2. EUMAEUS CHARACTER MASTER v1

| | |
|---|---|
| Folha | `Assets/Resources/Odisseia/Characters/NPCs/CHR_Eumaeus.png` (6 quadros, célula 30 × 57) |
| PixelLab | `d397a0b9-91a6-4e53-8c5c-4f483c0d9ca3` · v3 · size 60 · rotação `east` |
| Altura | 57 px = **1,33 un** = 0,95 do Odisseu (faixa de idoso, D-012) |
| Leitura | porqueiro fiel: cabelo e barba grisalhos, túnica de lã marrom com cinto de corda, **capa de pele de cabra**, cajado de pastor. Terra e couro, sem carmesim |
| Idle | pingue-pongue 0..3 (depois do 3 ele se inclina para a frente) |

> Eumaeus the old swineherd of Ithaca, loyal kind elderly man, weathered tanned face, short grey beard and thinning
> grey hair, knee-length coarse brown woollen tunic with a rope belt, a rough goatskin cape over his shoulders,
> leather sandals, holding a plain wooden shepherd staff upright in his front hand, the staff stays attached to his
> hand, warm humble expression

## 3. TELEMACHUS (ADULT) CHARACTER MASTER v1

Na Fase 01 ele é criança (`CHR_Telemachus_Child`); a D-013 já previa o adulto aqui. Serve às fases **14 e 15**.

| | |
|---|---|
| Folha | `Assets/Resources/Odisseia/Characters/NPCs/CHR_Telemachus_Adult.png` (6 quadros, célula 31 × 63) |
| PixelLab | `4537dcee-8133-42a4-aaab-cb3dfcc620f6` · v3 · size 64 · rotação `east` |
| Altura | 63 px com a ponta da lança = **1,47 un**; corpo ≈ 1,40, igual ao pai |
| Leitura | cabelo castanho-avermelhado e barba curta **do pai**, linho cru com **barra carmesim fina** (a casa de Odisseu, só na barra, como na criança), **manto azul-mar de viagem**, lança |
| Idle | pingue-pongue 0..3 |

> Telemachus son of Odysseus, a young man of twenty, lean and upright, short tousled dark reddish-brown hair and a
> short young beard like his father, knee-length off-white linen tunic with a thin crimson border and a leather belt,
> a sea-blue travelling cloak pinned at one shoulder, leather sandals, holding a tall spear upright in his front hand,
> the spear stays attached to his hand, determined earnest expression

Base dos dois: `side view, 16-bit pixel art game character, Bronze Age Mycenaean Greek` (sem lugar, sem olhar).

## 4. Integração — SCENE OVERRIDE

`Assets/Scripts/Editor/IthacaReturnCastDresser.cs` → `Dress14` / `Poses14` (roda **depois** do
`ItacaReturnSceneDresser`):
- **NPCs:** sprite do `Body` = master, escala 1, `flipX` (olham para a esquerda, de onde o jogador chega), **ordem −1** (atrás do jogador, que está em 0),
  `SpriteAnimator` no Idle, pés em **y = −2,00**. `NPCDialogue` e colisor intocados.
- **Pretendentes:** `EnemyFactionOverride.Aplicar` (folha, escala 1, ordem 2). **Prefab `EnemyBasic` intacto.**
- **Primeiro plano sobre NPC:** o dresser de cenário pôs `FG_Grass_1` e `FG_Grass_3` (ordem 12) exatamente em x = 0
  e 28, **enterrando Eumeu e Telêmaco até o joelho**. O NPC não pode sair (o colisor de diálogo está inteiro sob a
  grama), então a grama anda o mínimo até liberá-lo: −2,53 e −2,31 un.
- **Ordem de desenho:** o `Body` do jogador está na ordem 0 (a primeira versão deste documento dizia 2 — corrigido no Final
  Polish). O dresser lista o cenário grande por cima dele: os braseiros junto ao objetivo (ordem 3), profundidade intencional.

## 5. Validação

| Teste | Resultado |
|---|---|
| CastProbe | Eumeu, Telêmaco e pretendentes a 42,857 px/un, escala 1 |
| Contato com o chão | NPCs y −2,00; pretendentes na base do prefab |
| `PlaceholderProbe` | ver `Fase15/FINAL_CAST.md` §7 |
| `CampaignValidation` / WebGL | ver `Fase15/FINAL_CAST.md` §7 |

**Não testado:** play mode, navegador, mobile, gamepad.

## 6. Custo

Eumeu 2 + Idle 1, Telêmaco 2 + Idle 1 = **6 gerações**. O pretendente está na conta da Fase 15.

## 7. Pendências

- **Disfarce de mendigo sem visual** (só camada de física). Sprite próprio exigiria estado no `PlayerAnimator`.
- **Contorno azul brilhante** nas trepadeiras de uma casa da vila (captura `odisseu_e_pretendente`) — ENVIRONMENT ART.
- Se o `ItacaReturnSceneDresser` rodar de novo, ele recoloca a grama sobre os NPCs: rodar o `Dress14` depois.
