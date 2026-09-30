# Fase 10 — Sereias: elenco

**Extensão do `Docs/Characters/CHARACTER_ART_MASTER.md` (v1), que não foi alterado.** Aprovado em 2026-09-29.
Contém o **SIREN CHARACTER MASTER v1**.

- Master e rotações: `Fase10/_master_rotacoes.png` · variações: `_variacoes.png` · Idle: `_tira_idle.png`
- Capturas: `Fase10/_capturas/` (`antes_005`, `sereia_com_odisseu`, `ilha_-001|005|011`)
- Id e prompt: `Docs/Characters/cast.json` (`Siren`)

## 1. CastProbe

| Item | Achado |
|---|---|
| Personagens com figura | **só o Odisseu** — nenhuma sereia |
| Mecânica do canto | `SirenZone` (a área do encantamento, x −4 a 14, com `WindZone` que empurra), `SirenResistance` + `SirenResistanceIndicator` no jogador, `MastAnchor` ("amarrem-me ao mastro") |
| Inimigos, NPCs, tripulação | nenhum → **nenhum inimigo carmesim** |
| Instrumentos | **nenhum** no código |
| Animator | 0 |
| Estados especiais do Odisseu | nenhum acionado (a resistência é barra na HUD e empurrão físico, não animação) |
| Falas | Intro (Odisseu): "Circe avisou: as sereias cantam a verdade que mais desejamos ouvir… Amarrem-me ao mastro." · Outro: narrador |

O `SereiasSceneDresser` deixou **"o vão do santuário e o espaço sobre as agulhas de rocha livres para
elas entrarem depois"**, e fixou a direção da fase: **"o perigo não pode avisar — a armadilha É a beleza"**
(céu creme, nenhuma cor de alerta, paleta quente e turquesa).

## 2. Inventário

| Personagem | Papel | Facção | Antes | Estados | Decisão |
|---|---|---|---|---|---|
| Odisseu | Player | Casa de Odisseu | master v1 | 16 | **reutilizado** |
| Sereia ×3 | Figuras (santuário e agulhas) | Sereias | nenhuma | **Idle que canta** (nenhum código aciona Sing/Fly/Attack nelas) | **novo — SIREN CHARACTER MASTER v1 + 2 variações por matiz** |

## 3. Interpretação: a sereia antiga, bonita

**Mulher-ave, não cauda de peixe** — a sereia da Odisseia e da cerâmica grega. E **bonita, não monstro**:
é a direção que o cenário já tinha fixado ("a armadilha é a beleza"), e é o que o briefing pede (sem terror).
As garras são pequenas e douradas; o que domina a silhueta são as **asas**.

| | |
|---|---|
| Folha | `Assets/Resources/Odisseia/Characters/NPCs/CHR_Siren.png` (+ `CHR_Siren_Violet`, `CHR_Siren_Sea`) |
| PixelLab | `cb43f7a7-eb2c-4639-922e-bb9585cba84f` · v3 · size 64 · perfil = `east` |
| Altura | 60 px = **1,40 un** (1,0× o Odisseu) |
| Paleta | **turquesa e verde-mar** (asas, penas, cabelo), **creme e branco** (penas), **ouro** (diadema, garras), pele clara. Sem carmesim, sem preto dominante — casa com as rampas quentes e turquesa do cenário |
| Silhueta | **asas de ave** fechadas atrás dos ombros — elemento que nenhum outro personagem do jogo tem |
| Canto | **no Idle**: boca aberta cantando, cabeça inclinando, asas abrindo e fechando de leve, mãos abertas no peito. Pingue-pongue 0–3 a ~5 FPS. O véu do canto (`Siren_Song`) já é efeito de cenário e não foi duplicado no sprite |
| Instrumento | **nenhum** — não existe no código |

Prompt (base `side view, 16-bit pixel art game character, Greek mythology`):

> a siren of Greek myth, beautiful young woman with large white and turquoise feathered bird wings folded
> behind her shoulders, long flowing sea-green hair with small white shells in it, soft turquoise and cream
> feathers covering her body from the waist down like a feathered dress, slender bird legs ending in small
> golden talons, a thin gold circlet, pale skin, gentle alluring smile, both hands open at her chest as if singing

Idle:

> singing softly with her mouth open, head gently tilting, wings slowly opening a little and folding back,
> chest rising with each breath, hands open at her chest, in strict side profile facing right, head stays
> turned right in profile, no turning toward the viewer, feet stay planted

**Risco registrado:** na vista frontal os olhos grandes puxam para o anime; na vista de jogo (perfil, 1,4 un)
isso praticamente some.

## 4. Variações — por matiz, custo zero

Penas e cabelo turquesa/verde-mar ocupam ~150–210° de matiz (43% do sprite, medido), **longe da pele** (0–45°):
`Tools/cast-variants.js` troca só essa faixa, preservando a luminância.

| Folha | Troca | Onde |
|---|---|---|
| `CHR_Siren` | — | porta do santuário (x = 3,5) |
| `CHR_Siren_Violet` | turquesa → violeta suave | ao pé da agulha esquerda (x = −2,4) |
| `CHR_Siren_Sea` | turquesa → azul-mar | ao pé da agulha direita (x = 11,4) |

## 5. Integração

`Assets/Scripts/Editor/SirenCastDresser.cs`:
- `Dress` — três sereias pelas posições do santuário e das agulhas lidas da cena; pés no chão (y = −2);
  olhando para a esquerda (cantam para quem chega); ordem −14 (na frente do santuário e das agulhas, atrás
  do véu do canto e do jogador); sem colisor, sem script; guarda de 1,5 un do nascimento.
- `Poses` — Odisseu diante da sereia do santuário; sem salvar.

**Nas agulhas, não em cima delas:** a ponta de cada agulha é fina demais para apoiar uma figura — ela
pareceria flutuar. As duas ficam no chão, ao pé de cada uma, nas bordas da ilha.

Intocados: `SirenZone`, `SirenResistance`, `MastAnchor`, `WindZone`, o véu `Siren_Song`, nenhum prefab.

## 6. Validação

| Teste | Resultado |
|---|---|
| CastProbe | 3 sereias a 42,857 px/un, escala 1, 1,42 un |
| `PlaceholderProbe` | nada visível, nada inativo |
| `CampaignValidation` | passa |
| Compilação | 0 erros |
| Build WebGL | ver relatório |
| Capturas | sereia clara na porta escura do santuário diante do Odisseu; as três ao longo da ilha, voltadas para ele |

**Legibilidade:** a sereia azul-mar é a de menor contraste contra o céu creme; o contorno preto segura.
**Não testado:** play mode (resistência ao canto, mastro), navegador, mobile, gamepad.

## 7. Custo

**3 gerações** (783 → 786 de 2000): master 2 + Idle 1. Nenhum descarte. Variações a custo zero.

## 8. Pendências (não tocadas)

- Palácio de Éolo a 0,52 — ENVIRONMENT ART / SCENARIO.
- Porco da Circe — tinta rosa; exigiria mudança no sistema de animação do jogador.
- `EnemyBasic` carmesim nas fases 01, 14 e 15.
