# Troy Visual Pack — Fase 02 (Troia)

2026-10-02 · **Estado: TROY-01 aprovado (`master_a`); TROY-02 pronto para revisão.** Sem integração no Unity.

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

## TROY-01 — Master Background · 3 gerações (1153 → 1156) · **APROVADO: `master_a`**

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

## TROY-02 — Cidade, céu e montanhas · 6 gerações (rodada: 9 de 50)

> Saldo: o ciclo da conta PixelLab renovou durante o grupo — `get_balance` passou a **6 usadas / 1994 restantes** (as 6
> são deste grupo). O teto da rodada continua o combinado: 50 gerações, 9 usadas.

**Método:** recortar a cidade do próprio master não dá — 20 cores do céu/fumaça são as mesmas da sombra da muralha, e
o recorte por cor vazaria. Cada camada foi gerada **separada, com fundo transparente e o mesmo texto do master**, e depois
**trazida para as cores do `master_a`** (quantização / faixas de luminância). Assim a paleta é idêntica por construção.
Muralha, torre e portão de gameplay (`Architecture/`) já eram bons — **reutilizados**, não regenerados.

| Arquivo (`Assets/Art/Environments/Troy/Background/`) | px · un | Pivô | Origem | Custo |
|---|---|---|---|---|
| `troy_bg_master` | 672×384 · 15,7×9,0 | centro | o master aprovado — fundo de tela cheia (carregamento, cutscene) e referência | 0 |
| `troy_bg_sky_war` | 380×240 · 8,9×5,6 — **ladrilho** | base | **do master**: faixa x 0–189, y 0–79 (o único céu sem fumaça), espelhada; abaixo, a cor dominante da última linha. 7 cores, todas do master | 0 |
| `troy_bg_mountains` | 1536×108 · 35,8×2,5 — **ladrilho** | base | `mountains_b` (pixen; xadrez pintado removido) → 5 faixas de luminância mapeadas para os 5 azuis das montanhas limpas do master; espelhada | 2 (a + b) |
| `troy_bg_city_distant` | 768×229 · 17,9×5,3 | base | `city_c` (pixen; fundo chapado recortado): muralha na MESMA altura da do master (~75 px), templos à esquerda, palácio no alto à direita, fogos nos telhados; quantizada para as 67 cores do master (30 usadas) | 3 (a, b, c) |

**Prova:** tela de jogo recomposta (762×428 = câmera em 1×) com céu → montanhas → cidade → planície do master: as camadas
casam com o master em luz, paleta e escala. Prancha: `Docs/Art/PixelLab/_troy02_city.png`.

**Descartados:** `sky_a` (laranja chapado, nuvens em disco voador — o céu do master é melhor), `mountains_a` (veio com céu
pintado de mesmo tom das montanhas; o recorte comeu a serra), `city_a` (menor, dois portões, restos de xadrez),
`city_b` (a mais bonita, mas a muralha é ~40% maior que a do master — fica como alternativa para uma camada mais
próxima). Defeitos meus corrigidos antes de fechar: o recorte do céu pegava telhados (blocos repetidos) e o 1º
mapeamento das montanhas puxava o verde das árvores do master e virava ruído.

**Integração (depois):** parallax sugerido — céu 0,95 (quase parado, ladrilhado), montanhas 0,85 (ladrilhadas), cidade
0,7 (peça única: Troia é um marco, desliza devagar e fica para trás do portão da Área 4). A fumaça sobre a cidade vem do
TROY-07, por cima desta camada. Animação: nenhuma PixelLab; fogos dos telhados podem piscar por código.

## TROY-02 — Integração no Unity (2026-10-02) · 0 gerações

`TroySceneDresser.Run` (idempotente: 2ª e 3ª rodadas dão a cena idêntica linha a linha, só os fileIDs mudam).

| Camada | Asset | Fator | Ordem | Base com o jogador no chão |
|---|---|---|---|---|
| Cobertura | `Sky_Fill` `#ca8f67` (1ª linha do `sky_war`) | mundo | −60 | — |
| Céu | `troy_bg_sky_war`, ladrilhado | 0,95 | −50 | y −1,6 (topo na borda da tela) |
| Planície | `BG_Troy_Plain` `#b28b54` (planície do master), filho das montanhas | 0,85 | −49 | abaixo das montanhas |
| Montanhas | `troy_bg_mountains`, ladrilhado | 0,85 | −48 | y −2,1 (pé escondido no chão) |
| Cidade | `troy_bg_city_distant`, **peça única** | 0,70 | −46 | y −2,66 (como na prova de composição) |

- **Bug corrigido (apresentação):** o vestidor supunha a câmera começando em x=−33; ela está em x=0 na cena e o
  `CameraFollow` não salta, desliza. O `ParallaxLayer` mede a partir da posição da cena, então o céu antigo ficava 33 un
  fora do lugar e o início da fase mostrava só o bloco azul. Agora posição e percurso são lidos da câmera e dos limites
  do `CameraFollow`; a cobertura lateral considera tela até 20:9 (meia largura 12 un).
- **Cidade como marco:** entra pela direita no campo de batalha (câmera ≈ −10), fica inteira em quadro na aproximação da
  Área 4 (câmera 50) e termina atrás da muralha de gameplay.
- **Planície presa às montanhas:** com a câmera alta (salto) e nos abismos, o vão abaixo das camadas mostra a planície
  do master em vez de céu.
- `troy_bg_sky` e `troy_bg_city` antigos ficaram sem uso (não apagados).
- Capturas antes/depois: `Docs/Art/PixelLab/TR-02/_integracao/` (y −1 e y 2); **0 pixel magenta** nas 9.
- `PlaceholderProbe` Troia: 9 → **8** visíveis — restam só as 8 geometrias de gameplay (N-08): `Wall_Troia`,
  `Obstacle_Low`, `Platform_Bridge`, `Gauntlet_1..3`, `LevelGoal/Mast|Sail`. Colisores da cena: 14, idênticos.
