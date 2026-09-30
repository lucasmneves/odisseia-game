# Fase 08 — Circe: elenco

**Extensão do `Docs/Characters/CHARACTER_ART_MASTER.md` (v1), que não foi alterado.** Aprovado em 2026-09-29.
Contém o **CIRCE CHARACTER MASTER v1** e o **lobo encantado** (inimigo da fase).

- Circe: `Fase08/_tira_circe.png`, rotações em `Circe/_rotacoes.png` · lobo: `_lobo_pro_rotacoes_e_std.png`, `_tiras_lobo.png`
- Descartado: `_circe_e_lobo_std_descartado.png` (o lobo `standard`, à direita)
- Facções: `Docs/Characters/CAST_FACTIONS.png` · capturas: `Fase08/_capturas/`
- Ids e prompts: `Docs/Characters/cast.json` (`Circe`, `Circe_Wolf`, descartado `Circe_Wolf_std`)

## 1. CastProbe

| Item | Achado |
|---|---|
| Circe | **não está na cena** — mas o `CirceSceneDresser` deixou o vão da porta do templo **reservado** para ela ("é onde Circe, o Odisseu e o diálogo entram depois") |
| Inimigos | **2 `EnemyBasic`** (x = 4 e 12) com o **`CHR_Enemy_Basic` carmesim** — override só da cor (branca), sem variação de comportamento |
| Transformação | `TransformationZone` ×2 + `TransformationEffect` no jogador: **tinge o Odisseu de rosa, esconde a espada e desliga o combate** ("um porco não empunha espada") até a erva *moly* (`MolyHerbPickup`) curar. **Não troca de sprite — não há animal no código** |
| Animais | nenhum além do inimigo |
| NPCs, tripulação | nenhum |
| Falas | Intro (Odisseu): "Fumaça e cantos vêm daquela casa na floresta… e meus homens não voltaram." · Outro (narrador): "Circe cede diante da espada e da erva de Hermes." |
| Animator | 0 |

## 2. Inventário

| Personagem | Papel | Facção | Antes | Estados | Decisão |
|---|---|---|---|---|---|
| Odisseu | Player | Casa de Odisseu | master v1 | 16 (+ tinta da transformação, por código) | **reutilizado** |
| Circe | Figura no templo | Circe | nenhum | **Idle** (nenhum código aciona Cast, Talk ou Transform nela) | **novo — CIRCE CHARACTER MASTER v1** |
| Lobo encantado ×2 | Inimigo (`EnemyBasic`) | Circe | espartano carmesim pintado | **Idle, Run, Hit, Death** (os do `EnemyAnimator`) | **novo, por override na cena** |
| Porco (Odisseu transformado) | — | — | tinta rosa no corpo do Odisseu | — | **não gerado**: o código não troca de sprite; um porco exigiria estado novo no `PlayerAnimator` (mudança de sistema). Registrado como oportunidade |

## 3. O inimigo carmesim — facção correta

Era o `CHR_Enemy_Basic`: lanceiro de capa e crista **carmesim** (a cor reservada à casa de Odisseu).
Na ilha de Circe, **a facção hostil não é humana**: no mito a casa dela é cercada de **lobos e leões
encantados**, e os homens transformados em porcos são vítimas, não agressores.

**Lobo, não leão:** o colisor do `EnemyBasic` é 0,6 × 1,2 un (estreito e alto). O lobo é mais curto que
o leão, então o descompasso entre desenho e caixa de acerto é menor; a altura bate quase exato (1,21 un).

Override na cena por `EnemyFactionOverride.Aplicar` (folha, escala 1, ordem 2). **Prefab `EnemyBasic`
intacto** (arquivo de 20/08).

## 4. CIRCE CHARACTER MASTER v1

| | |
|---|---|
| Folha | `Assets/Resources/Odisseia/Characters/NPCs/CHR_Circe.png` |
| PixelLab | `112d8983-7ddc-47bc-b7ac-5387b73896a5` · v3 · size 64 · **perfil = rotação `south-east`** |
| Altura | 60 px = **1,40 un** (igual ao Odisseu — "alta", figura de poder) |
| Paleta | **violeta** (peplo), **verde-oliva** (barra), **verde-claro** (xale), **ouro** (coroa de louros, braceletes, serpente, taça), cabelo preto. Sem carmesim; o violeta não é o índigo de Micenas |
| Silhueta | esguia, **cabelo longo até a cintura**, **taça erguida** à frente — distinta de toda humana anterior (Penélope: coque e manto; mulher de Ítaca: lenço) |
| Objeto | a **taça (kylix)** do filtro — o que a define no mito. Não é objeto de gameplay; é identidade (o cajado já é do Éolo) |
| Idle | **7 quadros, ciclo inteiro** a 5 FPS: a taça **solta um fio de vapor** nos quadros 2, 4 e 6 — a magia dela, sem efeito extra. Em pingue-pongue o vapor piscaria |
| Em cena | diante da porta escura do templo (x = 31,2), no chão, olhando para a esquerda; ordem 0 |

Prompt (base `side view, 16-bit pixel art game character, Greek mythology`):

> Circe the enchantress of Greek myth, tall graceful adult woman, long dark wavy hair falling to her waist
> crowned with a thin golden laurel wreath, ankle-length deep violet peplos with an olive-green woven
> border, a sheer pale green shawl draped over her arms, golden armbands and a golden snake bracelet, bare
> feet, holding a small golden kylix cup raised in one hand, the cup stays in her hand, serene knowing smile

**Rotação:** as oito vistas vieram **45° deslocadas** — a `east` mostra as costas em 3/4, com o cabelo
cobrindo o rosto e sem a taça; a `south-east` é o perfil à direita. Resolvido pelo mecanismo `perfil`
(Fase 01), sem nova geração; o Idle foi animado na `south-east`.

## 5. Lobo encantado

| | |
|---|---|
| Folha | `Assets/Resources/Odisseia/Enemies/CHR_Circe_Wolf.png` (25 quadros) |
| PixelLab | `4f805faa-08e5-43a0-8438-f2a2c2b4e186` · **`pro`**, quadrúpede (gabarito `dog`), **`style_character_id` = master do Odisseu** |
| Tamanho | 52 × 65 px = **1,21 un de altura × 1,52 de comprimento** |
| Leitura | pelagem cinza-parda, **olhos verde-claros brilhantes**, **coleira dourada com amuleto violeta** — fera encantada da Circe, com as cores dela |
| Estados | Idle 6 · **Run 5 (quadros 3–7)** · Hit 6 · Death 8 |

Prompt:

> enchanted grey wolf from the island of Circe in Greek myth, lean powerful wolf, grey-brown fur with a
> darker back, glowing pale green eyes, a thin golden collar with a small violet charm, snarling

| Tentativa | Resultado |
|---|---|
| v3 | **recusado pela API**: "v3 mode does not support quadruped body type" (erro de validação, não cobrado) |
| `standard` (1 geração) | **descartado**: sombreamento chapado, 18 cores, o cinza saiu preto-carvão — lia como pantera, abaixo do padrão |
| **`pro` + estilo do Odisseu (20)** | **aprovado**: 35 cores, sombreamento de forma, no nível dos personagens v3 |

Animações v3 sobre o personagem `pro` funcionaram (1–2 gerações cada). Run: quadros 0–1 parado, galope
de 2 a 7 → loop **3–7**. A altura (1,21 un) quase coincide com o colisor (1,2 un).

## 6. Integração

`Assets/Scripts/Editor/CirceCastDresser.cs`:
- `Dress` — lobos por `EnemyFactionOverride`; Circe diante do templo (posição da `Temple`, chão y = −2).
- `Poses` — Circe com o Odisseu, e um lobo com o Odisseu; sem salvar a cena.

Intocados: `TransformationZone`, `TransformationEffect`, `MolyHerbPickup`, `EnemyController`, prefab `EnemyBasic`.

## 7. Validação

| Teste | Resultado |
|---|---|
| CastProbe | lobos e Circe a 42,857 px/un, escala 1 |
| `PlaceholderProbe` (Circe) | nada visível, nada inativo |
| `CampaignValidation` | passa |
| Compilação | 0 erros |
| Build WebGL | ver relatório |
| Capturas | Circe diante da porta escura do templo, legível; lobo cinza contra a floresta verde; os dois no mesmo chão do Odisseu |

**Não testado:** play mode (transformação, combate com os lobos), navegador, mobile, gamepad.

## 8. Custo

**30 gerações** (741 → 771 de 2000): Circe 2 + Idle 1 · lobo `standard` 1 (**descartado**) + `pro` 20 + 4 animações 6.

## 9. Pendências

- **Porco da transformação**: hoje é tinta rosa sobre o Odisseu. Um sprite de porco pediria estado novo
  no `PlayerAnimator` e troca de folha no `TransformationEffect` — decisão de sistema, fora desta etapa.
- Cenário da Fase 06 (palácio de Éolo a 0,52): ENVIRONMENT ART, não tocado.
- `EnemyBasic` carmesim ainda nas fases 01, 09, 14 e 15.
