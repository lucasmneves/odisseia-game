# Environment Bible — Ilha dos Ciclopes

> Deriva de `00_ArtBible.md`. Grade de tile = **1,0 unidade = 100 × 100 px** (Art Bible §5).

---

## 1. Paleta mestra da fase

Esta é a paleta **fechada** da Ilha dos Ciclopes. Nenhum asset da fase pode usar cor fora
daqui (as paletas de personagem do doc `01` fazem parte dela).

### Cores principais — a ilha

| Nome | Hex | Onde aparece |
|---|---|---|
| Pedra calcária clara | `#C9B99B` | Rocha exposta, ruínas, colunas |
| Pedra média | `#9A8B72` | Corpo de rochedos e plataformas |
| Pedra sombra | `#6B6052` | Faces em sombra |
| Terra seca | `#A8794E` | Solo, trilhas |
| Areia | `#E0C9A0` | Praia, chão claro |
| Verde-oliva | `#6E7F4A` | Vegetação mediterrânea |
| Verde-oliva escuro | `#48562F` | Sombra de vegetação, arbustos densos |

### Cores secundárias — atmosfera

| Nome | Hex | Uso |
|---|---|---|
| Mar (raso) | `#4FA3B8` | Água próxima da praia |
| Mar (fundo) | `#2A6E86` | Horizonte marítimo |
| Céu (alto) | `#7FC4DC` | Topo do céu |
| Céu (horizonte) | `#F0D9A8` | Quente, sol baixo |
| Névoa | `#B9C4C9` | Camadas de midground |

### Cores de destaque — leitura de gameplay

| Nome | Hex | Significado |
|---|---|---|
| **Borda pisável** | `#E8DCC0` | Topo de toda plataforma colidível — **regra absoluta** |
| **Interativo/coletável** | `#FFD54A` | Só objetos que o jogador pode pegar/usar |
| **Perigo** | `#D95A3C` | Espinhos, zonas letais, sinais de ameaça |
| **Âmbar do Ciclope** | `#FFB020` | Exclusivo do boss (doc `01`) |

### Cores de sombra — profundidade

| Camada | Hex | Opacidade |
|---|---|---|
| Sombra projetada (chão) | `#3A3227` | 35% |
| Escurecimento de midground | `#54606B` | 25% sobreposto |
| Escurecimento de background | `#7A8894` | 45% sobreposto |
| Interior de caverna | `#241F1D` | Base |

### Verificação de contraste

| Par | Diferença de valor | OK? |
|---|---|---|
| Odisseu (terracota) vs pedra média | ~28% | Sim |
| Odisseu vs vegetação | ~32% | Sim |
| Fera (marrom escuro) vs pedra clara | ~40% | Sim |
| Fera vs terra seca | ~22% | **Atenção** — nunca posicione feras sobre terra seca sem uma borda clara atrás |
| Ciclope (cinza-oliva) vs caverna escura | ~30% | Sim |
| Borda pisável vs corpo do bloco | 1 tom acima | Sim |

**Regra derivada:** a fera não pode ser posicionada sobre solo de terra seca (`#A8794E`)
sem uma silhueta de fundo mais clara. Está registrado aqui para o level design respeitar.

---

## 2. Tileset — ENV_CyclopsIsland

Grade **100 × 100 px** por tile. Todo tile é reutilizável e encaixa em qualquer vizinho da
mesma categoria.

### Categorias e conteúdo

| Categoria | Tiles | Descrição | Colisão |
|---|---|---|---|
| **Ground** | 9 (conjunto 3×3) | Solo principal: cantos, bordas, centro. Terra com topo de grama/pedra | Sim |
| **Platform** | 3 | Plataforma flutuante estreita: esquerda, meio, direita. Altura 0,5 un | Sim (só topo) |
| **Cliff** | 4 | Paredes verticais de penhasco: topo, corpo, base, canto | Sim |
| **Rock** | 5 | Blocos rochosos soltos, 1×1 e 2×1, formatos variados | Sim |
| **Cave** | 6 | Interior de caverna: parede, teto, estalactite, entrada, chão escuro | Sim (parede/chão) |
| **Grass** | 4 | Coberturas de grama para o topo do solo — variações de tufo | Não |
| **Sand** | 4 | Praia: centro, borda, transição para água | Sim |
| **Stone** | 4 | Alvenaria trabalhada (piso construído, degraus) | Sim |
| **AncientRuins** | 6 | Blocos de ruína: base de coluna, arquitrave, parede quebrada, degrau | Sim |
| **Decoration** | 8 | Rachaduras, musgo, marcas, manchas — sobrepostos, sem colisão | Não |
| **Collision** | — | **Não é arte.** Ver nota abaixo |

**Total: 53 tiles.** Cabe em **um atlas 1024 × 1024** (que comporta 100 tiles de 100 px).

### Nota sobre a categoria "Collision"

Não é um asset de arte. No Unity, a colisão vem do `TilemapCollider2D` no Tilemap
colidível, ou de `BoxCollider2D` em objetos individuais. O que existe é uma **separação de
Tilemaps**:

- `Tilemap_Ground` → tem `TilemapCollider2D` + `CompositeCollider2D`
- `Tilemap_Decoration` → sem collider
- `Tilemap_Background` → sem collider

Detalhado no documento `07`.

### Regras de construção do tileset

1. **Bordas encaixam sem costura.** Um tile de centro ladrilhado em qualquer direção não
   pode mostrar linha de emenda.
2. **Topo colidível sempre tem a borda `#E8DCC0`** com 4–6 px de espessura.
3. **Variação sem quebrar tiling:** produza 2–3 variantes do tile de centro para evitar
   repetição óbvia; todas devem ser intercambiáveis.
4. **Luz consistente:** superior-esquerda em todos os tiles, sem exceção.

---

## 3. Backgrounds — estrutura de parallax

O projeto já tem `ParallaxLayer.cs` implementado (fator configurável 0–1). Estas camadas
alimentam diretamente esse componente.

### Estrutura em 3 camadas

| Camada | Fator de parallax | Saturação | Conteúdo |
|---|---|---|---|
| **Background** | 0,10 | 20–35% | Céu, sol, mar distante, montanhas no horizonte |
| **Midground** | 0,35 | 40–55% | Formações rochosas, vegetação, silhuetas de ruínas |
| **Foreground** | 0,75 | Variável, escurecida | Rochas e folhagem passando na frente da câmera |

**Fator 0,10 no background:** quase estático, simula distância infinita.
**Fator 0,75 no foreground:** passa mais rápido que o jogador — sensação de proximidade.

### Dimensões

Área visível = 21,3 × 12 unidades = 2133 × 1200 px @ PPU 100.

| Camada | Resolução | Motivo |
|---|---|---|
| Background | **2048 × 1152** | Cobre a tela; ladrilha horizontalmente sem costura |
| Midground | **2048 × 1152** | Idem, com transparência |
| Foreground | **2048 × 1152** | Idem, majoritariamente transparente |

Todas as camadas precisam **ladrilhar horizontalmente sem costura** (a borda direita
encaixa na esquerda) — a fase tem ~70 unidades de extensão e a câmera percorre tudo.

---

### BG_CyclopsIsland_01 — Ilha (área externa)

Aberto, convidativo, épico. É o primeiro contato do jogador com a direção de arte.

| Camada | Conteúdo |
|---|---|
| **Background** | Céu gradiente `#7FC4DC` → `#F0D9A8` no horizonte. Sol baixo à esquerda (concorda com a luz). Mar `#2A6E86` ocupando o terço inferior. Ilhas distantes esmaecidas em névoa `#B9C4C9` |
| **Midground** | Encosta rochosa `#9A8B72` subindo pela direita. Aglomerados de oliveiras `#6E7F4A`. Uma coluna grega solitária quebrada na crista — promessa narrativa |
| **Foreground** | Rochas escuras `#6B6052` nos cantos inferiores, folhagem de oliveira entrando pelo topo. Escurecido 25% |

---

### BG_CyclopsIsland_02 — Região do Ciclope

Fechado, opressivo, ameaçador. Mesma paleta, tratamento oposto — é assim que se comunica
"território do boss" sem trocar a identidade visual.

| Camada | Conteúdo |
|---|---|
| **Background** | Céu escurecido `#54606B`, sol encoberto. Paredes de montanha imensas fechando o enquadramento. Boca de caverna escura `#241F1D` ao fundo |
| **Midground** | Pedregulhos gigantes (2–4 un cada — escala do ciclope). Ruínas em pior estado. **Névoa densa** `#B9C4C9` em faixas horizontais. Ossos de rebanho espalhados (sem sangue, sem violência gráfica) |
| **Foreground** | Estalactites descendo do topo, rochas escuras enquadrando as laterais. Escurecido 40% — o "túnel" de enquadramento aumenta a claustrofobia |

**Contraste dramático entre os dois BGs:** BG_01 tem céu claro e horizonte aberto; BG_02
tem teto de rocha e horizonte fechado. A transição entre eles é o momento em que o jogador
sente que entrou no território do boss.

---

## 4. Props — 10 unidades

Cada prop tem função visual, tamanho, camada e colisão definidos.

| # | Nome | Tamanho (un) | Camada | Colisão | Função visual |
|---|---|---|---|---|---|
| 1 | `PROP_GreekColumn_Broken` | 0,8 × 2,4 | Midground/Play | **Sim** (base) | Âncora temática — Grécia antiga. Serve de plataforma baixa |
| 2 | `PROP_Rock_Large` | 1,6 × 1,4 | Play | **Sim** | Obstáculo/cobertura. Quebra a horizontal do chão |
| 3 | `PROP_OliveTree` | 2,2 × 2,8 | Midground | Não | Vegetação mediterrânea, dá verticalidade |
| 4 | `PROP_Bush` | 0,9 × 0,6 | Play/Deco | Não | Preenchimento de base, esconde emendas de tile |
| 5 | `PROP_Torch` | 0,3 × 0,9 | Play | Não | **Fonte de luz** — justifica iluminação em caverna. Anima (fogo) |
| 6 | `PROP_GreekVase` | 0,5 × 0,7 | Play | Não | Detalhe de civilização. Cor terracota liga a Odisseu |
| 7 | `PROP_WoodenCrate` | 0,7 × 0,7 | Play | **Sim** | Plataforma modular, empilhável |
| 8 | `PROP_StoneStructure` | 2,0 × 1,2 | Play | **Sim** | Plataforma construída, degrau de ruína |
| 9 | `PROP_Altar` | 1,4 × 1,0 | Play | **Sim** | Ponto focal narrativo. Marca checkpoint/objetivo |
| 10 | `PROP_CyclopsBasket` | 1,2 × 1,0 | Play | Não | **Objeto do Ciclope** — cesto de pastor gigante. Estabelece o território dele |

### Orientação e regras

- Todos os props: **pivot na base** (`Bottom` ou `Bottom-Center`) — assentam no chão sem
  cálculo manual.
- Todos recebem **sombra projetada elíptica** (Art Bible §2, pilar 11).
- Props sem colisão **nunca** usam a borda `#E8DCC0` — ela significa "pisável".
- `PROP_CyclopsBasket` tem escala de gigante: comparado a Odisseu (1,4 un), um cesto de
  1,2 un é do tamanho do torso dele. É intencional — comunica a escala do dono.

---

## 5. Layout da fase — uso do tileset

O gameplay já existe em `Level_04_Ciclopes.unity` (~72 unidades de extensão). O art pass
substitui os retângulos placeholder pelos tiles, respeitando as métricas do Art Bible §4:

| Trecho | Ambiente | Tiles predominantes |
|---|---|---|
| Entrada (x -18 a -4) | Praia/costa externa | Sand, Ground, Grass + BG_01 |
| Subida (x -4 a 14) | Encosta rochosa | Ground, Cliff, Rock, AncientRuins |
| Passagem estreita (x 14 a 22) | Boca de caverna | Cave, Rock — **transição BG_01 → BG_02** |
| Arena do boss (x 22 a 42) | Caverna do Ciclope | Cave, Rock + BG_02 |
| Saída (x 42 a 54) | Fenda iluminada | Cave → Ground, luz entrando |

**Restrições de level design (Art Bible §4) que valem aqui:** vãos ≤ 4,0 un, degraus
≤ 2,0 un, corredores ≥ 1,8 un de altura. A arena do boss precisa de espaço lateral
suficiente para esquivar dos três pontos de ataque já implementados.
