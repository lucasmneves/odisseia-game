# Troy Visual Pack — Fase 02 (Troia)

2026-10-01 · **Estado: TROY-01 (master) em seleção.** Sem integração no Unity, sem commit.

Saldo no início: **1153 usadas / 847 restantes**. Primeira rodada autorizada: até **50 gerações**.
Executor: `Tools/pxl.js` (livro-razão `Docs/Art/PixelLab/_ledger.tsv`), IDs `TR-01…TR-08`.
Densidade: **42,857 px/un** (Odisseu 60 px = 1,4 un; câmera ortográfica 5 → 10 un = 428 px de altura, ~762 px de largura).
Paleta: `Docs/Environment_Troy/Palette/TROY_PALETTE.*` (11 rampas × 4: Contorno, Céu, Pedra clara, Pedra sombra, Terra seca,
Madeira, Lona, Bronze, Vermelho, Fogo, Fumaça). Identidade: `Docs/Environment_Troy/TROY_MASTER_REFERENCE.md` (Conceito B — a
muralha é o master).

## Auditoria (0 gerações)

### Inventário — `Assets/Art/Environments/Troy/` (26 arquivos)
| Camada | Asset | px | Veredito |
|---|---|---|---|
| Céu | `Background/troy_bg_sky` | 512×256 | **fraco** — degradê pontilhado azul genérico, sem nuvem nem fumaça |
| Cidade distante | `Background/troy_bg_city` | 512×176 | **fraco** — silhueta chapada de UMA cor (marrom), torres retangulares; não lê como Troia |
| Névoa | `Background/troy_bg_haze` | 512×96 | ok como véu; reaproveitável |
| Arquitetura | `Architecture/troy_wall_section_01` | 510×509 | **bom** — calcário ciclópico, contrafortes; reaproveitar |
| | `Architecture/troy_tower_01` | 192×574 | **bom** — reaproveitar |
| | `Architecture/troy_gate_01` | 318×437 | **bom** — reaproveitar |
| Acampamento | `Camp/` barricada, estandarte, escudo quebrado, fogueira (+anim), feixe de lanças, suprimentos, 2 tendas | — | **bons** — midground pronto em boa parte |
| Efeitos | `Effects/` arbusto seco, nuvem de poeira, rochas de 1º plano, monte de entulho, escada de cerco, coluna de fumaça | — | **bons**; a coluna de fumaça é pequena (56×212) e clara demais para fumaça de incêndio |
| Gameplay | `Gameplay/troy_plank_bridge` (300×17), `troy_fallen_block` (128×35) | — | **bons** (Asset Completion) |
| Terreno | `Terrain/troy_tiles_{earth,stone,wood}` | 128×128 | ok |
| Conceitos | `Docs/Environment_Troy/Concepts/` A Acampamento, **B Muralha (aprovado)**, C Batalha — 672×384 | — | referência de estilo |

**Não existe hoje:** campo de batalha, soldados ao fundo, cidade com templos/telhados, fumaça escura de incêndio, fogo
distante, céu com atmosfera de guerra. O Greek Soldier Master (3 figuras de fundo no acampamento) e o Trojan Soldier são
**personagens de gameplay em 1×** — grandes demais para o campo distante; servem só de referência de cor/equipamento.

### Placeholders de geometria em `Level_02_Troia.unity` (10 visíveis)
| Objeto | Tamanho (un) | Resolve com o que já existe? |
|---|---|---|
| `Platform_Bridge` | 7 × 0,5 | **sim** — `troy_plank_bridge` (7 × 0,4 un) |
| `Gauntlet_1/2/3` | 3 × 0,5 cada | **sim** — recorte de `troy_plank_bridge` ou `troy_tiles_wood` |
| `Obstacle_Low` | 0,6 × 0,8 | **sim** — recorte de `troy_rubble_pile_01` / `troy_fallen_block` |
| `Wall_Troia` (limite) | 1,5 × 6 | **sim** — fatia de `troy_tower_01` / `troy_wall_section_01` |
| `Sea_Background` | 60 × 6 | **parcial** — Troia não tem mar; o de Ítaca/Citera pode ser recolorido (decidir na integração) |
| `Mast` / `Sail` (`LevelGoal`) | 0,04×2 / 0,25×0,7 | **sim** — o navio do Grupo 9 de Ítaca |
| `Sky_Fill` | 140 × 30 | **será o céu do master** (TROY-01) |

Os 7 de geometria de gameplay (ponte, 3 gauntlets, obstáculo, muralha de limite, mar) **não precisam de geração** — é
integração. O que este pack resolve é o fundo, que hoje é o ponto mais fraco da fase.

## TROY-01 — Master Background · 3 gerações (1153 → 1156) · AGUARDANDO APROVAÇÃO

Modelo: `create_image_pixen`, 672×384 (máximo 16:9 da ferramenta; ≈ 1× nativo: 384 px = 9 un, a câmera mostra 10 un).
O `create_image_pro` em 688×384 devolve **um** candidato por 20–40 gerações — reservado para o caso de nenhum pixen servir.
Prompt base (os três): panorama lateral da Guerra de Troia — montanhas ao fundo; a cidade numa colina com muralhas
ciclópicas de calcário creme, torres, portão, templos de colunas e telhados vermelhos, estandartes; planície seca com
pequenos grupos de soldados (gregos de escudo redondo de bronze à esquerda, troianos de crista vermelha à direita,
arqueiros, poeira, fogueiras); primeiro plano de terra rachada, lanças quebradas, escudo caído, pedras; perspectiva
aérea. Variações de luz: (a) fim de tarde dourado, (b) dia claro com fumaça negra, (c) céu velado de fumaça e poeira.

| Candidato | Fator | Cores | Dist. paleta Troia | Leitura |
|---|---|---|---|---|
| **`master_a`** — fim de tarde | 1× | 67 | **15,7** | **escolhido**: a mais próxima da paleta oficial; luz quente da esquerda coerente; camadas legíveis (céu → montanhas → cidade → planície → 1º plano); dois lados distinguíveis; fumaça e fogo na cidade e no campo |
| `master_b` — dia claro | 1× | 66 | 19,0 | faixa preta no topo (artefato), raios de sol de desenho animado, fumaça negra chapada, poucos soldados |
| `master_c` — céu de fumaça | 1× | 73 | 19,0 | o mais dinâmico (formações maiores, fogo forte), mas muralha em perspectiva angulada — atrapalha o recorte em camadas laterais; alternativa |

Prova de escala (prancha): Odisseu 60 px no primeiro plano × soldados distantes ~20 px → a planície lê como distância
média, a muralha como fundo. Prancha: `Docs/Art/PixelLab/_troy01_master.png`.

**Ressalvas do `master_a`:** as ameias dão um ar de castelo (a mesma linguagem do Conceito B aprovado); os soldados estão
em grupos estáticos — os duelos, quedas e arqueiros em ação vêm do TROY-04 como sprites próprios. O céu quente substitui o
`troy_bg_sky` azul (o master passa a ditar a luz da fase).
