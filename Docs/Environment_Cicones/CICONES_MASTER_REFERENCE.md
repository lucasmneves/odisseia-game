# CICONES_ENVIRONMENT_MASTER — Fase 03_Cicones

**Conceito A — Vila costeira — aprovado em 2026-09-05.**
Ferramenta: PixelLab MCP (`create_image_pixen`) + código · Paleta: `Palette/CICONES_PALETTE.gpl`

> Vila mediterrânea habitada, fértil e pequena. É o contraponto de Troia: onde Troia era
> monumental e militar, Cícones é doméstica e agrícola — até deixar de ser.

---

## 1. Identidade aprovada

Três conceitos gerados (§6, Etapa 1). **A — Vila** venceu.

| | Resultado |
|---|---|
| **A — Vila mediterrânea** | **Aprovado.** Alvenaria de pedra irregular com reboco de barro, telhado plano de argila sobre vigas redondas aparentes, verga reta de madeira nas portas, pithoi, poço, oliveira, carroça. |
| B — Costa rural | Recusado: saiu em **vista de cima**. Os campos aparecem pela superfície, não pelo perfil. É a falha recorrente do modelo, a mesma da primeira leva de Troia. |
| C — Conflito | Recusado como direção: os telhados saíram **inclinados e de telha**, que é a arquitetura que o §18 proíbe. Serve como referência dos elementos de combate, não da arquitetura. |

Os três estão em `Concepts/`, os prompts em `_lote_conceitos.json`.

### A regra de arquitetura, herdada de Troia

Exclusão no prompt não segura; enquadramento positivo segura. Para Cícones o vocabulário é:

- alvenaria de **pedra irregular** com reboco de barro — não fiada regular de tijolo
- **telhado plano** de argila compactada sobre vigas redondas de madeira **aparentes**
- portas de **verga reta** — nunca arco
- janelas: aberturas **quadradas pequenas**, sem moldura
- muros de campo de **pedra seca**, baixos
- **pithoi** de terracota apoiados no chão do lado de fora

E a fórmula de enquadramento que já vinha funcionando:

> *"seen from directly beside it, strict flat side profile like a technical elevation drawing"*

Sem ela, `view: side` sozinho deixa o modelo cair em vista de cima — aconteceu no conceito B.

---

## 2. Paleta oficial

**14 rampas, 56 cores.** Gerada por `Tools/extract-palette-cicones.js`, que recusa gravar se
alguma rampa não descer em luminância ou se uma cor aparecer em duas rampas.

Os passos são **escolhidos do inventário medido** dos conceitos, nunca amostrados por
coordenada — amostrar retângulo num conceito de 672×384 acerta o material errado com
facilidade, e já custou em Troia.

| Rampa | Highlight | Base | Sombra | Profunda | |
|---|---|---|---|---|---|
| Contorno | `#2b2e27` | `#1d221d` | `#141415` | `#050507` | *compartilhada — personagem* |
| Céu | `#ceeefe` | `#94d0f3` | `#74b5ed` | `#6481a0` | *compartilhada — Ítaca* |
| Bronze | `#f5cd5c` | `#bb7e2c` | `#9d611f` | `#733e16` | *compartilhada — personagem* |
| Mar raso | `#a4dfed` | `#7ec4de` | `#598a91` | `#386774` | |
| Areia | `#d8cba7` | `#caad73` | `#b2955f` | `#917649` | |
| Terra seca | `#cfb379` | `#c29f51` | `#a58651` | `#685030` | |
| Pedra clara | `#d2ceb2` | `#ada587` | `#968e72` | `#756a4f` | |
| Pedra cinza | `#a6a098` | `#8f8e8d` | `#73767b` | `#5f626a` | |
| Madeira | `#8d7045` | `#735633` | `#573e24` | `#3e2819` | |
| Terracota | `#b76a35` | `#a65227` | `#874b2c` | `#602b17` | |
| Oliveira | `#8f9c74` | `#787e57` | `#55613d` | `#31402d` | |
| Grama seca | `#b7995a` | `#827a3d` | `#5f5435` | `#4b412d` | |
| Fogo | `#fcd84e` | `#e3a14d` | `#f1681a` | `#a7311d` | *compartilhada — Troia* |
| Fumaça | `#e6e8e2` | `#a09486` | `#887c74` | `#725f4a` | *compartilhada — Troia* |

### Quatro rampas compartilhadas, de propósito

É isso que faz as fases parecerem **o mesmo jogo** (§2 do briefing) em vez de três jogos com
paletas diferentes. Contorno e Bronze vêm do personagem, Céu vem de Ítaca, Fogo e Fumaça vêm
de Troia — são os materiais que atravessam todas as fases. Terra, pedra, madeira e vegetação
são próprias de Cícones.

### O que separa Cícones de Troia, na cor

Troia é **poeira e calcário pálido** sob sol duro: pedra clara indo a `#f2e8c4`, sem verde
nenhum. Cícones tem **duas rampas de vegetação** (Oliveira e Grama seca) e um **mar turquesa
raso** que Troia não tem. A pedra de Cícones é mais fria e acinzentada (`Pedra cinza`), porque
é alvenaria de campo, não calcário monumental esquadrado.

---

## 3. Escala — herdada, não negociável

| | |
|---|---|
| Densidade | **42,857143 px por unidade** |
| Origem | corpo do Odisseu = 60 px = 1,40 un |
| Câmera | ortográfica tamanho 5 → **10 un de altura**, ~17,8 un de largura |
| Viewport | 762 × 428 px de arte visível |
| Fase | **90 un de largura × 19 un de altura** (medido por `LevelExtentProbe`) |

Um asset de 672 px cobre 15,7 un — quase a tela inteira. Nada de cenário deve passar disso
sem ser uma camada de parallax que se repete.

---

## 4. Iluminação

Sol **superior esquerdo**, como pede o §12 — mas isso vale para a *composição* (de que lado
caem as sombras projetadas no chão), não para o sombreamento dentro do sprite.

Dentro do sprite vale a regra do projeto: **sombreamento de forma, sem luz direcional
dominante**. Foi medido no master do Odisseu (correlação de luminância |r| < 0,27) e é o que
mantém os assets coerentes entre si. Insistir em luz direcional dentro do sprite faria todo
asset de Cícones divergir do personagem que anda na frente dele.

---

## 5. O que é REUSADO de Troia — e por quê

O §18 proíbe duplicar o que existe. A Área 05 (conflito) de Cícones pede exatamente o kit que
Troia já produziu, e regerar seria pagar de novo por assets equivalentes:

| Asset de Troia | Uso em Cícones |
|---|---|
| `troy_barricade_01` | barricada da área de conflito |
| `troy_broken_shield_01` | escudo caído |
| `troy_spear_cluster_01` | lanças fincadas |
| `troy_campfire_01` | fogueira (vila e conflito) |
| `troy_banner_pole_01` | estandarte |
| `troy_supply_pile_01` | suprimentos no desembarque |
| `troy_smoke_column_01` | fumaça |
| `troy_dust_cloud_01` | poeira |
| `troy_rubble_pile_01` | entulho de estrutura danificada |
| `troy_foreground_rocks_01` | pedras de primeiro plano |
| `troy_dry_bush_01` | arbusto seco |
| `troy_tiles_wood` | tábua e madeira estrutural |

**Doze assets, doze gerações economizadas.** Fogo, Fumaça e Bronze são rampas compartilhadas,
então esses saem intactos. Os que usam terra, pedra ou madeira passam por `remapParaRampa`
para a rampa equivalente de Cícones — a mesma operação que Troia usou nos próprios tilesets.

---

## 6. Assets próprios de Cícones

Nada aqui é uma imagem única grande: é kit modular (§3 do briefing), montado no Unity.

| Grupo | Assets |
|---|---|
| **Terrain** | tileset de areia, tileset de terra com grama, tileset de rocha |
| **Architecture** | casa pequena, casa grande, muro de pedra seca, **templo/altar costeiro (landmark)** |
| **Nature** | oliveira, arbusto costeiro, tufo de capim |
| **Props** | pithoi e cestos, poço, carroça, cerca de madeira, barco de pesca |
| **Foreground** | vegetação de primeiro plano |
| **Background** | céu, montanhas distantes, colinas, mar distante, vila distante — **por código, custo zero** |

O fundo sai por código pelo mesmo motivo de Troia: elemento distante pede *menos* detalhe, que
é justamente onde o modelo generativo insiste em colocar detalhe. Silhueta em dois tons chapados
é o que faz o fundo recuar.

---

## 7. Parallax (§8)

Cinco camadas, como Ítaca e Troia. Hoje a cena tem **uma**.

| Camada | Fator | Conteúdo |
|---|---|---|
| FAR | 0,05 | céu |
| BACKGROUND | 0,20 | montanhas, mar distante |
| MID-BACK | 0,40 | colinas, vila distante |
| MIDGROUND | 0,65 | árvores e casas fora do plano de jogo |
| FOREGROUND | 1,25 | vegetação e pedras na frente da câmera |

`ParallaxLayer` já existe e é o sistema oficial. **Adicionar o componente faz parte de criar a
camada** — esquecer isso não gera erro: o fundo simplesmente some quando a câmera se afasta do
início da fase, e a captura em modo de edição mente porque o componente só roda em runtime.

---

## 8. Terreno sem Tilemap — decisão

O §10 pede "compatível com Unity Tilemap". **O projeto não usa Tilemap em lugar nenhum**: o
cenário de Ítaca e Troia é `SpriteRenderer` em modo `Tiled`, e há um comentário no
`PrologueSceneBuilder` registrando que trocar por Tilemap arriscaria a garantia de que a fase
é terminável.

Introduzir Tilemap só em Cícones criaria um segundo sistema de cenário convivendo com o das
fases 1 e 2 — o que o §18 proíbe explicitamente. **Decidido com o usuário em 2026-09-05:
manter `SpriteRenderer` em modo `Tiled`.**

Consequência para a arte: o tileset é uma faixa que **repete em espelho**, com período
`2W − 2` (não `2W`, que duplicaria a coluna da borda). Não precisa de peças de canto nem de
regras de vizinhança — os desníveis são resolvidos por blocos sobrepostos, como em Ítaca.

O modo `Tiled` exige malha **FullRect** no import.

---

## 9. Ordenação e camadas

O projeto tem **uma única Sorting Layer** (`Default`). Profundidade é `sortingOrder` inteiro.
Layers de física: Ground 8, Player 9, Enemy 10, Prop 11, Disguised 12.

---

## 10. Custo

| | |
|---|---|
| Conceitos | 3 gerações |
| Assets próprios | ~16 gerações |
| Reuso de Troia | 12 gerações **economizadas** |
| Fundo por código | 0 |
