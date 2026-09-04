# ITHACA_ENVIRONMENT_MASTER — Fase 01_Itaca_Prologue

**Conceito A (Rural) aprovado em 2026-09-02.** Etapa 2 de 3 concluída.
Review visual: `ithaca-concept.html` (publicado como Artifact)

---

## 1. Identidade aprovada

**Conceito A — Rural.** Terraços de oliveiras, muros de pedra seca, casas baixas de telhado
plano, caminho de terra, montanhas verdes rochosas.

Venceu na única medida objetiva que o briefing estabelece — pertencer ao mesmo universo visual
do personagem: **75% dos pixels dentro da família da paleta do Odisseu**, contra 48% (B) e
50% (C).

| Conceito | job_id | Afinidade |
|---|---|---|
| **A — Rural (aprovado)** | `7db2cc11-a43a-4116-b3f7-837fb03dff34` | **75%** |
| B — Costeira | `e277e809-8866-4ead-8605-49607d05ee0d` | 48% |
| C — Nobre | `47e57111-99a6-4f96-86cb-e52b61ca3eec` | 50% |

B e C não foram descartados: o porto é um trecho distinto do percurso e vai usar o vocabulário
do B (cais, barco, redes, ânforas).

---

## 2. Paleta oficial do cenário

Nove rampas de quatro passos, **35 cores únicas**, extraídas dos pixels reais — não escolhidas
à mão. Arquivos em `Palette/ITHACA_PALETTE.{hex,gpl,png}`.

| Rampa | Origem | Highlight | Base | Sombra | Profunda |
|---|---|---|---|---|---|
| Contorno | A | `#2b2e27` | `#1d221d` | `#141415` | `#050507` |
| Céu | A | `#ceeefe` | `#94d0f3` | `#74b5ed` | `#6481a0` |
| Água | **B**, dessaturada | `#d0ebf2` | `#6eadb6` | `#577ea2` | `#406b84` |
| Folhagem | A | `#7f8b5a` | `#6b7958` | `#525734` | `#333d2e` |
| Oliva seca | A | `#b4a05b` | `#998c64` | `#6f6842` | `#5d5734` |
| Terra / caminho | A | `#f5d5a7` | `#cbb492` | `#968268` | `#6b5d4e` |
| Pedra | A | `#846f5e` | `#6b5d4e` | `#57473d` | `#3e3a33` |
| Pedra fria | B | `#a0bcc4` | `#8e8a6f` | `#6f665f` | `#433c3b` |
| Madeira | tileset wood | `#977a42` | `#7d5c4f` | `#64433a` | `#382e2a` |

**A rampa de Água vem do Conceito B de propósito.** O A quase não tem mar — 4,4% da imagem,
quatro tons agrupados entre luminância 77 e 85. Isso não é uma rampa. Fabricar uma a partir
daí produziria um mar chapado; o B tem 13 tons de água num espectro de 97 a 228.

Ela foi **dessaturada em 0,62** depois de montada a composição do Grupo 1 — ver seção 7.

**Pedra fria e Madeira foram acrescentadas no Grupo 2** (seção 8): o Conceito A não tem nem
madeira nem pedra cinza, e sem essas rampas a quantização dos tilesets jogava a madeira na
Folhagem e os capstones na Água.

Cruzamentos intencionais:
- `#6b5d4e` é o passo profundo de *Terra* e o passo base de *Pedra* — caminho e muro se tocam
  no mesmo tom, o que é correto num material contíguo.
- `#050507` é **compartilhado com a paleta do personagem** (o contorno é o mesmo).

O método de extração: agrupar as 59 cores da imagem por família de matiz, e dentro de cada
família escolher 4 passos espalhados em luminância com intervalo mínimo, preferindo os tons
mais frequentes. Sem o intervalo mínimo, famílias pequenas devolvem quatro tons quase
idênticos e a "rampa" não é uma rampa — foi o que aconteceu na primeira tentativa.

---

## 3. Escala — herdada do personagem, não negociável

| | |
|---|---|
| Corpo do Odisseu | **60 px = 1,4 unidades** |
| Pixels por unidade | **42,857143** |
| Pivô do personagem | {0,5 ; 0,142857} — linha dos pés |

Referências derivadas:

| Elemento | Altura alvo | Em pixels |
|---|---|---|
| Porta | **acima de 1,4 un** | > 60 px |
| Barril / ânfora | ≈ 0,5 un | ≈ 21 px |
| Muro baixo de pedra seca | ≈ 0,8 un | ≈ 34 px |
| Casa de um andar | ≈ 3 un | ≈ 129 px |
| Oliveira | ≈ 3–4 un | 129–171 px |
| Navio (comprimento) | ≈ 8–12 un | 343–514 px |

A seção 25 do briefing alerta contra porta menor que o personagem. A regra acima é o que
impede isso.

---

## 4. Iluminação — corrigindo o briefing

A seção 23 pede luz do canto superior esquerdo. **O personagem master não tem luz direcional**
— medido na Fase 1 dele: correlações de luminância fracas (|r| < 0,27) e componente vertical
trocando de sinal entre as vistas. O que o PixelLab aplica é sombreamento de forma.

**A regra oficial do cenário é sombreamento de forma**, igual à do personagem. Manter "luz
superior esquerda" na spec faria o cenário divergir do master, que é justamente o que a
seção 2 quer evitar.

---

## 5. Pipeline de geração — qual ferramenta para quê

Testado em 2026-09-02 gerando a mesma faixa de montanha distante com as duas ferramentas.

| | Custo | `no_background` | Resultado |
|---|---|---|---|
| `create_image_pixen` | 1 | **ignorado** | Cena opaca com céu embutido — inútil como camada, taparia tudo atrás |
| `create_image_pro` | 40 | **respeitado** | Transparente acima da crista, banda de baixo contraste — camada correta |

**Regra:** camadas e cenas usam `pro`; sprites soltos (barril, árvore, pedra) usam `pixen`,
que é o caso de uso declarado dele ("True for sprites and items, False for scenes").

Sempre passar o Conceito A como `style_image_base64` com
`style_copy: [color_palette, outline, shading]`.

### O corte é parte do processo

O `pro` respeitou a transparência mas **misturou uma faixa de vegetação e muro na base** — 
conteúdo de midground dentro de uma camada de background, apesar de o prompt pedir "no trees,
no buildings". O modelo ignora exclusões parcialmente.

Solução: cortar por perfil de luminância. A banda de montanha distante é de baixo contraste; a
vegetação traz verdes escuros e o muro traz areia clara. Varrer de cima para baixo e cortar na
primeira linha com mais de 5% de pixels claros (lum > 190) ou 12% de escuros (lum < 60) isola a
camada. Na faixa de teste: corte na linha 136 de 192.

Custo zero, e o resultado é a camada limpa em `Layers/ithaca_bg_mountains_far.png`
(512 × 136, 44% opaco, topo 100% transparente).

### Emenda horizontal

A faixa de teste tem **17% de diferença entre a coluna 0 e a última** — não é seamless.
Para camadas distantes isso passa se a faixa for larga o bastante ou espelhada; para tilesets
de terreno vai precisar do `create_sidescroller_tileset` (2–3 gerações), que é feito para isso.

---

## 6. Orçamento

Plano **Tier 1: Pixel Apprentice** — 2000 por ciclo, renova em **2026-10-02**.

Gasto: **232 de 2000.**

| Item | Custo |
|---|---|
| 3 conceitos (`pro`, 688×384) | 120 |
| Teste de faixa (`pixen` + `pro`) | 41 |
| Camada de montanha distante (aproveitada do teste) | — |

Projeção do Grupo 1 (5 camadas de background em `pro`): ~200. Os grupos de props em `pixen`
saem por ~1 cada.

---

## 7. Grupo 1 — Background (concluído)

Cinco camadas, todas 512 px de largura, prontas para parallax. Em `Layers/`.

| Camada | Tamanho | Método | Opaco | Ladrilha |
|---|---|---|---|---|
| `ithaca_bg_sky` | 512 × 256 | procedural | 100% | sim |
| `ithaca_bg_clouds` | 512 × 128 | `pro` (40 ger.) | 26% | quase |
| `ithaca_bg_island_far` | 512 × 96 | procedural | 50% | sim |
| `ithaca_bg_ocean` | 512 × 128 | procedural | 100% | sim |
| `ithaca_bg_mountains_far` | 512 × 136 | `pro` + corte | 44% | não (17%) |

Composição de referência em `Layers/_composicao_demo.png`.

### Duas das três gerações falharam — e por quê

Mandei céu (não), nuvens, ilha distante e oceano para o `pro`. **Só as nuvens saíram.**
A ilha e o oceano voltaram como cenas de vila rural — casas, muros de pedra, oliveiras.

A causa é o `style_image`. Passar o Conceito A amarra a paleta, mas **vaza conteúdo**: o A
*é* uma vila rural, e pedir "ilha distante sem construções, sem árvores" contra essa
referência perde a disputa. As exclusões no prompt não seguram.

Rejeitadas, preservadas em `_testes/`.

### Onde procedural ganha de geração

Refiz ilha e oceano por código, junto com o céu. Para **banda plana e de baixo detalhe** o
método procedural é melhor em todos os critérios que importam:

| | Gerado | Procedural |
|---|---|---|
| Emenda ao ladrilhar | 45–78% de diferença | **indistinguível do interior** |
| Cores | 51–60, fora da paleta | **exatamente as da rampa** |
| Custo | 40 gerações | **zero** |
| Controle | prompt e torcida | determinístico |

A seção 5 do briefing pede que elementos distantes tenham menos detalhe — que é justamente
onde o código ganha e o modelo generativo insiste em colocar detalhe.

**Regra atualizada do pipeline:**

- **Banda plana e repetitiva** (céu, oceano, silhueta distante) → **procedural**
- **Forma orgânica com silhueta própria** (nuvem, montanha, árvore, casa) → **`pro`**
- **Sprite solto com fundo transparente** (barril, ânfora, pedra) → **`pixen`**

Detalhe técnico do céu e do oceano: dithering ordenado Bayer 4×4 entre os passos da rampa.
É o que faz a transição parecer gradiente sem sair da paleta — um gradiente liso precisaria
de dezenas de tons intermediários, exatamente o que a seção 24 manda evitar. Com a largura
múltipla de 4 (o período do Bayer), a emenda horizontal fica idêntica a qualquer fronteira
interna, então ladrilha perfeito.

### Correção de saturação na rampa de Água

Medindo a saturação média de cada camada montada, o oceano saía com **57%** contra 13–39%
de todo o resto. Ele destoava na composição — e a seção 24 pede explicitamente para evitar
excesso de saturação.

A causa: a rampa de Água veio do Conceito B, que media só 48% de afinidade de paleta.

**A rampa foi dessaturada em 0,62 em direção à própria luminância**, o que levou o oceano a
39% — alinhado com os 39% do céu.

| | Antes | Depois |
|---|---|---|
| Água | `#c4f0fb` `#52b8c6` `#4383bd` `#2c719a` | `#d0ebf2` `#6eadb6` `#577ea2` `#406b84` |

Os arquivos de paleta em `Palette/` já refletem a rampa corrigida.

### Erros cometidos no caminho

- **Dithering com erro de índice.** A primeira versão do céu subia um degrau da rampa mesmo
  com fração zero, criando uma linha dura em cada junção. Visível a olho nu.
- **Dois testes de emenda mal formulados.** Comparei a coluna 0 com a última diretamente, e
  linhas verticais vizinhas — mas em dithering ordenado vizinhos *devem* diferir. O teste
  correto compara a emenda ao ladrilhar contra uma fronteira interna equivalente.

---

## 8. Grupo 2 — Terrain (concluído)

Quatro tilesets Wang de 16 tiles cada, 32 × 32 px, fundo transparente. Em `Terrain/`.

| Kit | tileset_id | Cores | Saturação | Afinidade |
|---|---|---|---|---|
| `ithaca_tiles_stone` | `29898a80-e23c-4c44-a7cc-7d5bca29b847` | 16 | 38% | 95% |
| `ithaca_tiles_earth` | `46b54952-dcbf-4126-8a24-ce08a7a67a3d` | 8 | 32% | 95% |
| `ithaca_tiles_grass` | `52c1faaf-7c24-488a-a60a-65f9c9b1f871` | 15 | 32% | 100% |
| `ithaca_tiles_wood` | `8e7bc7be-6e92-4d21-8918-374df7c0a2c5` | 8 | 34% | 99% |

Referência de saturação: o Conceito A tem 36%. Prova de escala em `Terrain/_prova_escala.png`.

### Escolha do tamanho de tile

**32 px.** Com o Odisseu em 60 px de corpo isso dá **1,88 tiles de altura**, a proporção
padrão de platformer. Mantém 42,857 px por unidade, então a densidade de pixel continua igual
à do personagem — o tile vale 0,747 unidade de mundo.

16 px daria 3,75 tiles por personagem: granularidade fina demais para o nível de detalhe da arte.

### O tileset é Wang por cantos

Cada tile declara `NW/NE/SW/SE` como `upper` (ar) ou `lower` (terreno sólido). Uma plataforma
horizontal usa:

| Papel | Tile | Cantos sólidos |
|---|---|---|
| Ponta esquerda | `wang_14` | SE |
| Topo do meio | `wang_12` | SW + SE |
| Ponta direita | `wang_13` | SW |
| Miolo | `wang_0` | todos |

**A posição na folha é o índice do tile no array do JSON, em ordem row-major** — *não* o
`original_position`, que aponta para uma grade de origem maior (col até 5, row até 6) e leva
ao tile errado. Conferido cruzando com os pixels: `wang_0` cai no índice 6 = célula (2,1), que
é o único tile 100% opaco da folha.

Consequência para o level design: nos tiles de topo os cantos de cima são `upper`, então
**a superfície pisável fica na metade do tile, não no topo dele**. O colisor precisa seguir isso.

### A quantização expôs dois buracos na paleta

Os tilesets saíram com 44–56 cores próprias. Quantizei na paleta oficial para o terreno não
divergir do resto — e o primeiro resultado piorou duas coisas:

- a **madeira virou verde-oliva**, porque não havia marrom de madeira e o vizinho mais próximo
  caiu na rampa Folhagem;
- os **capstones viraram azuis**, porque não havia cinza neutro e eles caíram na rampa Água.

A causa é de origem: a paleta foi extraída do Conceito A, que **não tem madeira nem pedra
cinza**. Duas rampas foram acrescentadas:

| Rampa | Origem | Passos |
|---|---|---|
| Madeira | tileset wood, dessaturado 0,65 | `#977a42` `#7d5c4f` `#64433a` `#382e2a` |
| Pedra fria | Conceito B, tons neutros | `#a0bcc4` `#8e8a6f` `#6f665f` `#433c3b` |

A paleta foi de 27 para **35 cores**. Depois disso a quantização corrigiu o que estava errado
sem quebrar o que estava certo: o `grass` saiu do rosado para tan quente e o `wood` voltou a
ser marrom.

Originais sem quantização preservados em `Terrain/_originais_sem_paleta/`.

### Dois pontos de julgamento

- **A pedra saiu em fiadas regulares de tijolo**, não no muro de pedra seca irregular do
  Conceito A. É limpa e utilizável, mas é um desvio da identidade aprovada.
- **Os capstones ficaram cinza-azulados.** Destoam do calor da paleta, mas dão leitura clara
  de onde dá para pisar — o que a seção 36 coloca como prioridade. Mantidos de propósito.

Refazer qualquer um dos dois custa ~3 gerações.

### O encadeamento não aconteceu

A ferramenta aceita `base_tile_id` para conectar tilesets visualmente. Meu regex não casou o
formato `base_tile_id (for chaining):` da resposta, então os quatro saíram independentes.
Na prática a quantização na paleta comum resolveu a coerência de cor, mas o vocabulário de
borda de cada kit ainda é próprio. Se as transições entre materiais ficarem visíveis na fase,
refazer encadeado a partir de `e21c7fa9-745d-4258-a13c-aec3625f70c7` (base do `stone`).

---

## 9. Grupo 3 — Architecture: a casa resolvida por cirurgia, não por geração

A casa pequena consumiu **60 gerações** e não fechou. Cada correção de um eixo quebrava o
outro:

| | Escala | Identidade |
|---|---|---|
| v1 (canvas 128) | casa 1,61 un, porta 0,79 un ❌ | telhado plano, calcário quente ✓ |
| v2 (canvas 128, "preencha o quadro") | casa 1,87 un, porta 1,12 un ❌ | telhado plano, calcário quente ✓ |
| v3 (canvas 192) | casa 4,08 un, porta 1,98 un ✓ | telha inclinada, pedra cinza ❌ |

A v1 e a v2 reprovam na seção 25 do briefing: o Odisseu não passa pela porta. A v3 passa,
mas telhado de telha é a leitura romana que a seção 9 do briefing proíbe.

### O que destravou

Medir em vez de gerar. Duas medidas mudaram o diagnóstico:

1. **A v3 é pixel art nativa 1×.** A suspeita era que o canvas maior tivesse vindo ampliado
   2×, o que daria escala certa com metade da densidade do personagem. O histograma de
   sequências de linhas idênticas desmente: a v3 tem densidade nativa, igual à do Odisseu.
2. **O erro da v3 é local.** O telhado ocupa as linhas 2–55, com o contorno inferior em 56–57.
   Parede, porta e janela — tudo que passou no teste — vivem abaixo da linha 58 e não têm
   nada de errado.

Ou seja: a v3 não é uma casa errada, é uma casa certa com o chapéu errado.

**A casa final é a v3 recortada na linha 58, com uma laje plana redesenhada por código**, na
estratigrafia medida da v2 (contorno 2 / luz 2 / corpo 5 / sombra 1 / contorno 2 px) e balanço
proporcional à parede — 3px sobre 108 na v2, 5px sobre 176 aqui.

| | Resultado |
|---|---|
| Casa | 186 × 131 px = **4,34 × 3,06 un** (alvo de altura ~3,0) |
| Vão da porta | 39 × 82 px = **1,91 un** |
| Folga do Odisseu | 6 px lateral, 22 px de altura — **passa** |
| Densidade | nativa 1× |
| Cores | 13, **todas da paleta oficial** |
| Custo | **zero gerações** |

Prova visual em `Architecture/_prova_escala.png`, gerada por ferramenta e não à mão.

### Remapeamento por rampa, não quantização por vizinho mais próximo

A v3 nunca tinha sido quantizada — as 36 cores dela não estavam na paleta. Quantizar pelo
vizinho mais próximo colapsaria tons vizinhos na mesma entrada e achataria a parede, porque
as rampas de Ítaca são poucas e espaçadas.

`Tools/ramp-map.js` faz outra coisa: **classifica o material primeiro**, e só então distribui
as cores daquele material pelos 4 passos da rampa dele, por luminância relativa. O degradê
interno sobrevive. 36 cores → 13, com a parede ainda legível como pedra.

Os limiares foram medidos nas cores reais, não chutados:

- **Madeira vs. pedra por saturação.** A madeira mais apagada da v3 tem sat 0,42 e a pedra
  mais quente tem 0,31. O corte em **0,36** separa com folga.
- **Vidro precisa de margem de frieza.** A primeira regra era `g >= r`, e o cinza neutro
  `#8c8c87` (g == r) entrou como vidro e saiu verde-oliva. A regra correta é `g - r >= 6`.
- **Calcário claro vai para a rampa Terra / caminho**, não para Pedra. A rampa Pedra da
  paleta é marrom escura — serve para sombra e argamassa, não para parede clara.
- **O vão da janela usa só a metade escura de Pedra fria.** Lê como interior escuro, que é o
  honesto para Ítaca rural. `GLASS=Agua` produz a variante envidraçada, se a fase pedir.

### O Grupo 3 completo — tudo por composição

Resolvida a casa pequena, as outras três peças saíram do mesmo material, sem geração nenhuma.

| Asset | Tamanho | Em unidades | Origem |
|---|---|---|---|
| `ithaca_house_small_01` | 186 × 131 | 4,34 × 3,06 | v3 recortada + laje plana |
| `ithaca_house_odysseus_01` | 314 × 187 | 7,33 × 4,36 | parede + porta + janelas da v3 + pórtico |
| `ithaca_column_01` | 28 × 107 | 0,65 × 2,50 | desenhada por código |
| `ithaca_wall_low_01` | 58 × 34 | 1,35 × 0,79 | espelho da parede da v3, ladrilhável |

As quatro passam em paleta (só cores oficiais), densidade (nativa 1×) e escala. As duas casas
usam **exatamente o mesmo conjunto de 13 cores** — os dois caminhos de remapeamento foram
conferidos e concordam.

`node Tools/build-architecture.js` reconstrói o grupo inteiro e roda todas as verificações.
Os PNGs em `Architecture/` são **derivados**: podem ser apagados e refeitos. As fontes são
`_fonte_v3_telha_inclinada.png` e `_v2_telhado_plano_escala_curta.png`.

### As duas fachadas largas — 1 geração cada, e por que precisaram de geração

O megaron do Ato 2 e o armazém do Ato 5 são os únicos prédios que **não** saíram por
composição. Têm 17,8 un de largura e o maior material do banco (o corpo da casa v3) tem 4,1:
compor daria uma casa esticada. Faltava silhueta própria — colunata, friso e cornija no
palácio; cornija e portões duplos com bandas de ferro no armazém.

Custou **1 geração** no `pixen`. O `pro` acima de 170 px devolve um candidato só por 20–40, e
768 px de largura cairia nesse caso. O `pixen` vai até 768 por lado — que é justamente o teto
que define a largura da fachada.

| | `ithaca_palace_01` | `ithaca_warehouse_01` |
|---|---|---|
| Tamanho | 762 × 300 = **17,78 × 7,00 un** | 739 × 142 = **17,24 × 3,31 un** |
| Vão da porta | 65 × 134 px, folga de 32 px lateral | 25 × 79 px — decorativa |
| Cores | 12, da paleta | 12, da paleta |
| Custo | 1 geração | 1 geração |

A altura do palácio bate **exatamente** com a do bloco que ele substitui na cena.

**A porta do armazém não precisa passar no teste da seção 25.** Ele é pano de fundo: nenhum
portão entra nele e as bancadas de preparação ficam à frente da fachada. O teste vale para o
que o jogador atravessa — casa, casa de Odisseu e palácio, que passam.

**O fundo veio opaco de novo** — o `no_background` do `pixen` cai em qualquer prompt com
enquadramento, como já estava registrado. Saiu por `Tools/cutout.js`.

### O corte entre madeira e pedra é propriedade do asset

Quantizar com o limiar padrão de 0,36 estragou a fachada: metade da parede virou madeira e o
calcário saiu mosqueado de malva. Medindo os dois blocos separadamente, a porta fica entre
**0,60 e 0,69** de saturação e o calcário entre **0,356 e 0,43** — o corte aqui é **0,50**.

No armazém é 0,38 (porta em 0,50–0,53, parede em 0,16–0,26) e na casa v3 é 0,36. O limiar
virou argumento do `build-facade.js`, com o padrão preservando os assets já verificados.
**Medir nos dois blocos antes de escolher é parte do trabalho**, não detalhe.

### O recorte precisa de todos os tons de borda

O xadrez de transparência que o `pixen` desenha tem **dois** tons alternados. No armazém iam
de (90,90,91) a (108,111,112) — fora da tolerância um do outro. Com uma referência só, o
segundo tom sobrava como quadradinhos opacos espalhados pela imagem, que a quantização
transformava em manchas claras. O `cutout.js` agora usa todas as cores de borda com peso
acima de 2%, o que resolve xadrez de dois tons por construção.

### Porta dupla é duas componentes

O `scale-proof` achava a porta pela maior mancha de madeira, e no palácio isso media **uma
folha**: 32 px de vão contra 33 px de Odisseu — reprovava por 1 px um vão em que ele cabe
folgado. Agora junta folhas gêmeas: mesma faixa vertical, encostadas, e de largura
comparável.

A condição de largura importa. Sem ela as **ombreiras** da casa entram junto — são madeira, da
mesma altura e encostadas — e o vão medido passa a incluir a moldura em vez do buraco. Com ela,
a casa volta a medir 39 px (o vão real) e o palácio mede 65 px (as duas folhas).

O `build-palace.js` tinha uma cópia dessa busca e foi ela que reportou o falso 32 px. As duas
agora usam a mesma função — duas implementações do mesmo teste divergem.

### Mobília de jogo: portões e pontos de interação

O que restava de retângulo colorido não era cenário, e por isso foi tratado à parte: os cinco
portões de progressão e os pontos de interação são objetos que o jogador **usa**.

**Portão** (`ithaca_gate_01`, 2,40 × 4,99 un) e **batente** (`ithaca_gatepost_01`,
0,65 × 5,16 un), os dois desenhados por código como a coluna — o banco só tem portas de casa,
de 2,17 un, baixas demais para o vão de 5 un que o portão bloqueia.

**O desenho tem a mesma altura que o colisor, de propósito.** Desenhar um portão baixo e
deixar o colisor alto criaria parede invisível, e o README é explícito sobre isso: parede muda
é indistinguível de bug. O que se vê é o que bloqueia.

O batente é um arquivo separado do portão porque **ele permanece quando o portão abre** — a
folha some e os batentes ficam, para o lugar continuar legível como passagem. A folha desenhada
entra no `lockedVisuals` junto do bloco; sem isso o colisor sairia e o desenho ficaria, com o
jogador atravessando um portão fechado.

**Pontos de interação** ganharam o prop que representam: espada, escudo e feixe de flechas no
arsenal, cesto e rolo de corda no porto. Três não têm asset próprio e receberam o mais honesto
disponível — a lança pelos remos (é o cabo de madeira longo mais próximo), o rolo de corda pelo
velame, e o banco pela tripulação, que são pessoas.

**A altura passada ao `Figura` é a NATIVA do sprite.** Ele escala a arte para a altura pedida,
o que é certo para NPC (proporção própria) e errado para cenário: esticar quebraria os 42,857
px por unidade. Passando a altura nativa, o fator de escala dá exatamente 1.

### O que eu tinha declarado errado

Afirmei que a fase não tinha mais retângulo colorido. **Tinha:** 15 visíveis e mais 10 que
apareciam durante os atos. A afirmação veio de olhar capturas — e uma captura mostra 18 das
349 unidades da fase por vez, sem nenhum objeto inativo.

O pior dos casos era autoinfligido: **os bonecos e alvos do treino já tinham arte no pacote**
(`Training/ithaca_training_dummy_01`, `ithaca_training_target_01`) e eu nunca liguei. Eles
nascem inativos e só aparecem no Ato 4, então nenhuma captura minha os pegou.

`Assets/Scripts/Editor/PlaceholderProbe.cs` existe por causa disso:

```
Unity.exe -batchmode -nographics -quit -projectPath . -executeMethod PlaceholderProbe.Run
```

Varre a cena inteira, **inclusive objetos inativos**, e separa três casos: visível agora,
inativo (que um ato pode ligar depois), e desenho desligado de propósito — os colisores de
chão e as paredes de limite continuam com o sprite atribuído e o renderer desligado.

Sai com código 1 quando sobra pendência, então encadeia com as outras verificações.

### Preenchimento chapado não é placeholder

Quatro objetos usam o quadrado legitimamente, como retângulo de cor: o fundo de cobertura do
céu, a água profunda sob a faixa de oceano, e o `LevelGoal` — que fica a x=420, **50 unidades
além do alcance máximo da câmera**. A conta: o navio anda 16 un na partida (4 un/s por 4 s),
de 340 a 356, e a câmera trava em 360.

A probe não consegue tirar isso do sprite: um retângulo de cor é a ferramenta certa para um
fundo de cobertura e a errada para uma casa. Ficam numa lista com o motivo escrito de cada um,
que é revisável — um número solto na saída não seria.

### Duas correções que a varredura provocou

- **O `Bedrock` aparece.** Eu tinha presumido que o bloco de terra sob o chão nunca entrava em
  quadro; com a câmera três unidades mais baixa, entra. Agora é o tile cheio de terra
  ladrilhado, e o preenchimento de terreno passou de 5 para 7 fiadas.
- **O mar repetia na vertical.** Estiquei o `size.y` da faixa de oceano para 8 unidades e ela
  ladrilhou, deixando uma listra no meio da água — exatamente o erro que eu tinha evitado no
  céu. Voltou à altura nativa, com água profunda chapada por baixo.

A silhueta do convés virou o **próprio Odisseu parado**, que é quem embarca; vinha como
retângulo dourado. O sprite vem por nome da folha, não pelo primeiro do arquivo — numa folha
de 99 quadros o primeiro é quase certamente o errado.

O tear de Penélope foi o único que precisou de arte nova: 1 geração.

### Coordenada fracionária vira lixo, não erro

O portão saiu com verde-limão no meio da madeira. A causa: `W / 2` numa largura ímpar dá
coordenada fracionária, e índice não-inteiro no Buffer não grava o pixel — deixa cor pela
metade. Não levanta exceção nem aparece em nenhuma checagem numérica; só o `palette-check`
acusou, como três cores intrusas. O `fillRect` do `compose.js` agora arredonda na entrada,
o que fecha a classe inteira.

### O último lote: cinco objetos que faltavam

Bigorna, forja, torre de vigia, lavoura e o casco escorado da praia. Cinco sprites soltos,
**5 gerações** num lote só de `pixen` — `Tools/pixellab-batch.js` dispara em paralelo, porque
sequencialmente cinco assets levariam cinco minutos de espera contra pouco mais de um.

| Asset | Tamanho | Grupo |
|---|---|---|
| `ithaca_anvil_01` | 1,49 × 1,12 un | Props |
| `ithaca_forge_01` | 1,61 × 1,56 un | Props |
| `ithaca_watchtower_01` | 1,59 × 4,92 un | Architecture |
| `ithaca_field_01` | 4,06 × 0,68 un | Nature |
| `ithaca_boat_hull_01` | 5,06 × 2,05 un | Port |

**Os cinco respeitaram `no_background`.** É a confirmação da regra pelo lado positivo: o que
derruba a transparência é linguagem de enquadramento, e estes prompts não têm nenhuma — só
descrevem o objeto e terminam em "isolated game asset cut out on a fully transparent
background, only the X".

O casco da praia resolve um erro anterior: eu tinha posto ali o navio de Odisseu, de 9,12 un,
que dominava a zona e contava a história errada — a expedição pronta antes de ser preparada.
Um barco de pesca escorado de 5,06 un é o que a cena pede.

### A checagem de cobertura pegou um caso que o prompt causou

A lavoura veio com cevada em `#fad632`, a **97** da entrada mais próxima da paleta — a rampa
Oliva seca é bem mais apagada. O quantizador pulou, como pula a tocha.

Mas a causa aqui é outra: a tocha tem fogo, que a paleta não cobre por decisão; a lavoura só
veio saturada demais porque **o prompt não pediu o tom**. Regerar dizendo "muted dull olive
and dusty tan tones, sun-bleached and desaturated like sun-dried straw" resolveu por 1
geração, e o asset passou a quantizar normalmente.

**A distinção importa:** quando a checagem pula um asset, a pergunta seguinte é se falta rampa
na paleta ou se falta instrução no prompt. Só o primeiro caso é para aceitar.

### Ladrilhar textura irregular por espelho

O muro e a parede da casa de Odisseu vêm de uma faixa de 30 px espelhada. O detalhe que
importa: o **período é `2W-2`, não `2W`**. O espelho ingênuo repete a coluna da borda e deixa
uma linha dupla visível; descontar as duas colunas de dobra faz a emenda virar uma reflexão
exata, sem descontinuidade possível — emenda medida em **0,0** contra mediana interna de 8,8.

**A faixa de origem precisa estar limpa de contorno, não só de porta e janela.** A primeira
versão pegou `x102..125` e as duas primeiras colunas carregavam a sobra do contorno da verga
da porta — 20 pixels de `#050507` que o espelho transformou numa listra preta repetida a cada
período, visível na parede inteira. `Tools/map-materials.js` hoje testa Contorno junto com
madeira e vidro, e apontou uma faixa melhor: `x10..39`, 30 px de largura.

Corolário: a varredura de "coluna limpa" tem de ser feita **na faixa útil**, não na altura
toda — o contorno de topo e de base da parede atravessa todas as colunas e reprovaria todas.

### Escolher a fiada, não só a faixa

Recortar 29 px quaisquer da parede dá um resultado chapado. Comparando cinco fiadas
candidatas lado a lado ampliadas, só uma tinha a irregularidade que faz ler como pedra seca;
as outras saíam como parapeito liso, com bandas cinza ou com triângulos escuros. **Vale gerar
a prancha de comparação antes de escolher.**

Na casa de Odisseu a parede usa duas bandas com fiadas e defasagens diferentes: a de baixo é
mais escura, como parede que envelhece do chão para cima. Defasar também em **X** importa —
com o mesmo offset horizontal os eixos dos espelhos se alinham entre as bandas e aparece um
ritmo vertical que denuncia o ladrilhamento.

### Coluna: geometria vai de código

A coluna é dórica — ábaco, equino, fuste com 5 caneluras que estreita para cima, plinto de
duas fiadas. Desenhada por código porque é objeto geométrico, e é justamente em escala e
simetria que o modelo falha.

O sombreamento segue a **regra de forma** da seção 4, não luz direcional: o fuste é um
cilindro, então clareia no eixo e escurece nas bordas, sem lado iluminado. As caneluras são um
passo a mais de sombra na borda de cada estria, por cima desse gradiente.

---

## 10. Grupo 9 — Ships: 3 gerações, e só o casco

O navio era o único asset do cenário que não tinha como sair de material existente — a
silhueta de um casco não aparece em lugar nenhum do que já foi feito. Ainda assim custou
**3 gerações**, não as 20–40 de uma chamada `pro`.

### Por que não foi de `pro`

O `create_image_pro` devolve **um conjunto de candidatos por chamada**: 64 abaixo de 42 px,
16 até 85, 4 até 170, e **1 só acima disso**. O custo é por chamada, não por candidato. Um
navio precisa de 343–514 px de comprimento, ou seja: 20–40 gerações para **um** candidato,
sem escolha — exatamente a situação que queimou 60 gerações na casa.

`create_image_pixen` custa **1 geração** e aceita canvas de até 768 px por lado. Errar com
ele é barato, e foi o que permitiu iterar.

### As três tentativas

| | O que pedi | O que veio |
|---|---|---|
| 1 | casco em 400×160 | silhueta ótima, transparência correta, **as duas pontas cortadas** |
| 2 | "com margem vazia nas pontas" em 512×176 | vaso completo, mas o modelo **desenhou o xadrez de transparência** |
| 3 | "desenhado pequeno o bastante para as duas pontas caberem" | vaso completo, fundo **chapado opaco** |

A terceira serviu: o fundo chapado sai por código.

### Duas armadilhas novas do `pixen`

- **Pedir "margem vazia" faz o modelo pintar o xadrez de transparência.** Ele renderiza a
  convenção visual de vazio como conteúdo — 100% do canvas opaco, em dois cinzas alternados
  de 4 px. O enquadramento positivo continua valendo, mas **"vazio" é uma palavra que ele
  desenha**. A formulação que funciona descreve o *sujeito*, não o espaço: "desenhado pequeno
  o bastante para as duas pontas estarem completas e visíveis".
- **`no_background` cai junto com qualquer linguagem de enquadramento.** A primeira tentativa,
  sem instrução de composição, respeitou a transparência; as duas seguintes, com ela, não.
  Isso amplia a armadilha já conhecida ("`pixen` ignora `no_background` em cenas"): não é só
  cena — **é qualquer prompt que faça o modelo pensar em quadro**.

Nos dois casos o fundo sai chapado, então `Tools/cutout.js` resolve de graça: flood-fill a
partir da borda, com tolerância de cor. Flood-fill e não "apagar toda cor igual", para o que
está cercado pelo objeto sobreviver. O vazado da espiral das rodas de proa e o interior das
argolas de remo **precisam** da segunda passada (`--enclosed`): são cercados, o flood-fill de
borda não os alcança, e deixá-los opacos põe 385 px da cor do fundo dentro da silhueta.

### O resto do navio é código

Só o casco foi gerado. Mastro, verga, vela, cordame e remos são desenhados — geometria, que é
onde o modelo falha e o código acerta.

| | |
|---|---|
| `ithaca_ship_01` | vela içada — a partida |
| `ithaca_ship_01_furled` | vela enrolada na verga — atracado no cais |

São **dois estados do mesmo navio**, no mesmo canvas e com a mesma linha de convés, então dá
para trocar um pelo outro no lugar sem reposicionar nada.

391 × 257 px = **9,12 × 6,00 un**, dentro do alvo de 8–12 un de comprimento. 13 cores, todas
da paleta. Proa à direita.

**A linha do convés é publicada em `ithaca_ship_01.json`.** O `ShipDeparture` troca Odisseu por
uma silhueta no convés antes de zarpar, e o recorte final desloca as coordenadas — adivinhar
a linha dá erro de pixel. Prova em `Ships/_prova_escala.png`, com o Odisseu em pé no convés.

### Ordem de desenho importa mais do que parece

O mastro tem de ser desenhado **antes** da vela: o pano corre à frente do mastro, e desenhá-lo
por cima parte a vela ao meio com um risco escuro. A verga vem **depois** da vela, porque é
nela que o pano é amarrado. Os estais vêm antes de tudo.

A vela usa o mesmo **dithering Bayer 4×4** do céu e do oceano, e pelo mesmo motivo: faz a
barriga parecer degradê sem sair da paleta. O expoente da curva importa — com `d²` quase todo
o pano cai na zona de transição e o dithering lê como chiado; com `d³` o centro fica em cor
chapada e só a beirada degrada.

Remo de 1 px some contra o céu neste tamanho. O cabo tem 3 px e a pá alarga no meio.

---

## 11. Import no Unity

`node Tools/unity-import.js` copia os 46 PNGs (e os 6 JSON) de `Docs/Environment_Ithaca/`
para `Assets/Art/Environments/Ithaca/` **e escreve os `.meta` junto**.

### Por que escrever o `.meta` em vez de importar e corrigir depois

O Editor gera um `.meta` com os defaults dele no instante em que vê o arquivo — 100 px por
unidade, filtro bilinear, compressão ligada. Com o `.meta` pronto ao lado do PNG, ele importa
certo de primeira. E isso funciona **com o Editor aberto**, que é a diferença que importa:
batchmode exige o Editor fechado, e fechar o Editor de outra pessoa não é uma opção.

### Configurações, e por que cada uma

| | | |
|---|---|---|
| `spritePixelsToUnits` | **42,857143** | herdado do personagem; a 100 a casa mediria 1,3 un e o Odisseu não passaria pela porta |
| `filterMode` | Point | bilinear borra pixel art inteira, e é o default |
| `textureCompression` | Uncompressed | compressão inventa cor e quebra a paleta fechada |
| `alphaIsTransparency` | ligado | senão sai halo escuro na borda de cada sprite |
| mipmaps | desligados | somem com o detalhe de longe |
| pivô | BottomCenter | o objeto assenta na linha do chão, como as figuras que o `PrologueProbe.ConferirApoio()` cobra |
| pivô (Terrain) | Center | é o que o Tilemap espera |
| wrap | Repeat só no parallax e no muro | são os que repetem lateralmente; malha FullRect junto, que o modo Tiled do SpriteRenderer exige |

**GUIDs são derivados do caminho**, então re-rodar o import não quebra referência de cena
nenhuma. É a mesma regra que já vale para a folha do personagem: trocar GUID deixa o sprite
missing no Inspector.

### Os tilesets entram fatiados

Cada kit de terreno vira 16 sprites `ithaca_tiles_<kit>_00..15`, de 32 px. O índice no nome é
a posição **row-major a partir do topo**, que é a ordem do array no JSON do PixelLab — não a
do campo `original_position`, que aponta para outra grade. O rect do Unity tem origem embaixo
à esquerda, então o y é invertido na conversão; é justamente essa conversão que a probe
confere, tile a tile.

### Verificação

```
node Tools/verify-meta.js
```

confere os 46 `.meta` fora do Unity: escala, filtro, compressão, pivô, wrap, GUID único, e o
rect de cada um dos 64 tiles. Serve com o Editor aberto.

```
Unity.exe -batchmode -nographics -quit -projectPath . -executeMethod IthacaEnvironmentProbe.Run
```

confere o que **de fato importou**, que é a única fonte de verdade — exige o Editor fechado.
Com o Editor aberto, o mesmo está em `Odisseia > Conferir arte de Itaca`.

**Rodada em 2026-09-03, saiu limpa:** 46 texturas, 4 tilesets fatiados com os índices
row-major corretos, 0 erro de compilação. Log em `Logs/itaca.log`.

A probe existe porque erro de import não vira erro em lugar nenhum: a textura carrega,
renderiza e roda, só fica com o tamanho errado e borrada. Isso é indistinguível de "a arte é
assim mesmo".

---

## 12. Quantização dos Grupos 4 a 8

Os Grupos 4 a 8 (Nature, Props, Training, Arsenal, Port) nunca tinham sido quantizados: 24 a
65 cores cada, **zero** batendo exatamente com a paleta. Não era grosseiro — afinidade de 100%
e distância média de 15 a 37 ao vizinho, porque foram gerados com o Conceito A como
`style_image`. Mas a paleta fechada de 35 cores é a regra da seção 2, e eles conviviam com 12
assets que estavam exatamente nela. Em cena isso aparecia: a oliveira saía prateada ao lado
das casas.

`node Tools/quantize-scenery.js` resolve. **32 assets quantizados**, originais preservados em
`<grupo>/_originais_sem_paleta/` — a fonte é sempre o original, então requantizar não empilha
perdas.

### Um classificador separado, e por quê

O `materialDeCenario` do `ramp-map.js` é distinto do classificador de arquitetura. Alargar o
de arquitetura para cobrir verde regrediria o vão da janela da casa, que é um teal escuro a
0,205 de saturação — bem no meio da faixa onde folhagem e vidro se confundem. Dois
classificadores calibrados em materiais diferentes custam menos que um que erra nos dois.

Regras, todas medidas nas cores reais:

- **Frio com `g >= b`**: saturação ≥ 0,22 vai para **Folhagem**, abaixo para **Oliva seca**.
  É o que separa verde vivo de verde seco.
- **Frio com `b > g`**: **Pedra fria** — metal e sombra fria.
- **Quente e saturado (≥ 0,42)** ainda é duas coisas. O que separa é quanto o verde acompanha
  o vermelho: `(g-b)/(r-b) >= 0,65` é ocre (bronze, palha, terracota) e vai para **Oliva
  seca**; abaixo é marrom de madeira. **Sem essa divisão o escudo e a ânfora saíram marrons
  chapados na primeira passada** — o ocre é o que dava a leitura de metal.

### A regra de cobertura virou checagem

O projeto já sabia que é preciso conferir se a paleta cobre os materiais do asset antes de
quantizar. Agora isso é medida: a maior distância de uma cor **relevante** (≥ 1% dos pixels)
até a entrada mais próxima da paleta.

**`ithaca_torch_01` é pulado**, e o número decide: a chama `#fb7507` mede **116** de distância,
o segundo pior mede 77 e todo o resto fica em 63 ou menos. A paleta não tem rampa de fogo, e
quantizar apagava a chama. O limiar de 90 é o vão entre os dois grupos, não um número redondo.

### As camadas de fundo ficaram de fora — depois de tentar

Quantizei `clouds`, `island_far` e a fonte de `mountains_far` e **reverti ao ver o
resultado**: branco de nuvem cai em Pedra, que é marrom, e verde dessaturado de ilha distante
cai em Oliva seca, que é amarela. As nuvens saíram marrons e a ilha amarela na tela.

O classificador de cenário é calibrado para props e vegetação; céu, nuvem e terra distante são
outros materiais. Céu e oceano já estão na paleta por construção (foram feitos por código), e
as outras três têm 100% de afinidade. Quantizá-las exigiria um terceiro classificador, com
Céu, Água e Folhagem, calibrado à parte.

**Estado:** **42 dos 46** assets exatamente na paleta. Fora dela ficam `ithaca_torch_01`
(fogo, sem rampa) e as **três camadas de fundo geradas** — `clouds`, `island_far` e
`mountains_far` —, deliberadamente não quantizadas pelo motivo acima.

---

## 13. A arte na cena — parallax, terreno e props

O `PrologueSceneBuilder` monta a Fase 1 do zero, e agora usa a arte de Ítaca quando ela está
importada. **Sem a arte ele volta sozinho às silhuetas chapadas**, então o builder continua
rodando num clone que não tenha rodado `Tools/unity-import.js`.

Capturas de referência: `_fase_002.png`, `_fase_126.png`, `_fase_195.png`, `_fase_338.png`.

### Ver é a única verificação que vale para cenário

`Assets/Scripts/Editor/PrologueScreenshot.cs` renderiza a fase para PNG sem abrir o Editor:

```
Unity.exe -batchmode -quit -projectPath . -executeMethod PrologueScreenshot.Capture \
          -shotXs 2,126,195,338 -shotOut Docs/Environment_Ithaca/_fase.png
```

**Sem `-nographics`** — batchmode sem contexto gráfico devolve imagem preta, e é o erro
natural porque todas as outras chamadas do projeto usam essa flag.

Duas coisas que a ferramenta precisou aprender:

- **O parallax tem de ser simulado.** O `ParallaxLayer` só roda em runtime; em modo de edição
  as camadas ficam na posição de autoria. Sem simular, a foto no começo da fase sai certa (a
  diferença é zero ali) e mais adiante o fundo some — e a conclusão natural, errada, é que a
  camada não cobre a fase. A captura aplica o deslocamento à mão e desfaz depois.
- **Fundo magenta na câmera.** Buraco de cobertura fica gritante em vez de passar por céu.

### As cinco camadas

| Camada | Fator | Ordem |
|---|---|---|
| `BG_Sky` | 1,00 | -50 |
| `BG_Clouds` | 0,90 | -48 |
| `BG_Island` | 0,82 | -46 |
| `BG_Ocean` | 0,74 | -44 |
| `BG_Mountains` | 0,62 | -42 |

Fator **alto é longe**: a camada acompanha a câmera e quase não desliza na tela.

**O que destravou o parallax forte.** As telas pintadas antigas não eram contínuas nas bordas,
o que obrigava a fatores de 0,93–0,97 com uma imagem única cobrindo a fase — parallax quase
nulo, e o próprio comentário do código lamentava isso. As camadas de Ítaca ladrilham, então
vão em `SpriteDrawMode.Tiled`: um renderer por camada, largura calculada pelo deslize, e o
fator livre para descer até onde a profundidade aparece.

A de montanhas não ladrilhava (emenda 120,7 contra p90 interna de 14,2). Foi resolvida por
**espelho de período 2W-2** (`Tools/make-tileable.js`), que levou a emenda a 6,6 — a mesma
técnica do muro. A fonte não-ladrilhável ficou em `_fonte_mountains_far_nao_ladrilha.png`.

`drawMode Tiled` exige **malha FullRect** no import: com Tight o recorte é descartado e a
camada sai esticada em vez de repetida.

### O terreno veste o colisor, não o substitui

A geometria de colisão da fase é ajustada e conferida pelo `PrologueProbe` — chão contínuo até
o cais, nenhum buraco antes de o pulo ser ensinado. Trocá-la por um Tilemap arriscaria a
garantia de que a fase é terminável. Então os blocos continuam lá como colisores, com o
**desenho desligado**, e o tileset é desenhado por cima.

**O tile de topo é posicionado pelo CENTRO na linha do chão.** Medido nos pixels: no tile de
topo plano (índice 3) os cantos de cima do Wang são `upper`, e o desenho sólido começa na
metade do tile. Alinhar pelo topo deixaria a superfície desenhada meio tile abaixo da
superfície do colisor, com o personagem parecendo afundado.

Dois renderers por trecho em vez de um objeto por tile: a fase tem 349 unidades de chão, o que
daria mais de dois mil GameObjects. Os kits acompanham o percurso — grama na vila e no
recrutamento, terra no treino e no arsenal, pedra no porto, madeira no cais.

### Props: o que a paisagem pintada revelou

Placeholder que funcionava contra fundo chapado passa a gritar contra paisagem pintada. Foi o
que o README já previa, e apareceu em três lugares:

- **`Training_Fence`** era uma barra marrom de 84 unidades a 2,6 de altura. Contra o céu liso
  passava por cerca; contra a paisagem virou um traço solto no ar. Agora é o muro baixo de
  pedra seca ladrilhado na linha do chão.
- **`Boundary_Left` e `Boundary_Right`** são paredes de física de 12 unidades. Passavam por
  barranco e viraram lajes marrons na frente do mar. O colisor continua; só o desenho saiu.
- **O navio foi para o cais, não para a praia do recrutamento.** O único navio do pacote tem
  9,12 un e é o de Odisseu; pôr ele na zona de recrutamento dominava a cena e contava a
  história errada, com o navio pronto antes de a expedição ser preparada.

**A silhueta do convés usa a linha publicada em `ithaca_ship_01.json`**, não um palpite:
257 px de altura menos 190 px até a borda, a 42,857 px por unidade. Adivinhar põe a silhueta
flutuando ou enterrada, e nada no jogo acusa.

A casa **não é escalada** para caber nos parâmetros `largura`/`altura` do placeholder: a porta
dela foi dimensionada para o Odisseu passar, e escalar desfaria justamente essa medida.

### Verificação

`PrologueProbe.Run` continua passando: chão contínuo de -8,0 a 341,0, **39 figuras apoiadas no
chão com desvio máximo de 0,00**, portões, alcance e cadeia de atos íntegros. Era o risco real
de mexer no builder, e é a checagem que prova que não quebrou.

---

## 14. Estado e próxima etapa

**Feito:** conceito escolhido, paleta oficial de 35 cores em 9 rampas, escala travada, regra de
iluminação definida, pipeline de ferramenta em quatro vias, e **os nove grupos de asset**.
Architecture na seção 9, Ships na seção 10.

| Grupo | Estado | Onde |
|---|---|---|
| 1 Background | completo — 5 camadas de parallax | `Layers/` |
| 2 Terrain | completo — 4 tilesets Wang de 32 px | `Terrain/` |
| 3 Architecture | **completo** — 2 casas, coluna e muro ladrilhável | `Architecture/` |
| 4 Nature | 9 sprites | `Nature/` |
| 5 Props | 10 sprites | `Props/` |
| 6 Training | 4 sprites | `Training/` |
| 7 Arsenal | 4 sprites | `Arsenal/` |
| 8 Port | 4 sprites | `Port/` |
| 9 Ships | **completo** — navio em dois estados | `Ships/` |

**Todos os nove grupos estão completos e importados no Unity** — 46 texturas em
`Assets/Art/Environments/Ithaca/`, com os `.meta` gerados (seção 11). Os 4 tilesets entraram
fatiados em 64 sprites de 32 px.

**A fase do prólogo já usa a arte** — parallax, terreno e a primeira leva de props (seção 13).

**A Fase 1 não tem mais retângulo colorido.** Cenário, mobília de jogo e figurantes, todos
feitos. Resta a decisão da seção 15: os seis NPCs nomeados continuam em arte pintada a 129 px
por unidade, que borra no tamanho de jogo.

Gasto total: **352 gerações** de 2000.

Gasto total do cenário: **332 gerações** de 2000. Os Grupos 3 e 9 juntos custaram 3.

Estrutura de destino, quando a produção for para o Unity:

```
Assets/Art/Environments/Ithaca/
├── Background/   Midground/   Terrain/     Architecture/
├── Nature/       Props/       Training/    Arsenal/
├── Port/         Ships/
```

A cópia é feita por `node Tools/unity-import.js` e pode ser refeita a qualquer momento — os
GUIDs são derivados do caminho, então nenhuma referência de cena quebra.

---

## 15. Figurantes — pixel art, e um achado sobre os NPCs nomeados

Os dezenove figurantes da fase (dez recrutas, seis soldados de treino, três marinheiros) eram
retângulos coloridos. Agora são **cinco arquétipos** — pescador, lavrador, marinheiro, soldado
e ancião — repartidos entre eles: gerar dezenove pessoas distintas custaria dezenove gerações
por uma diferença que ninguém lê num figurante de fundo.

**5 gerações**, uma por arquétipo, com `create_character` em modo `standard`. Cada personagem
volta com rig e quatro rotações; a de leste é a que o projeto usa, porque o master do Odisseu
é todo `east` e espelhado por código.

### Por que não o `pixflux`

Testado, custa a mesma 1 geração, aceita **paleta forçada** e img2img — e ainda assim não
alcança o acabamento do master: perdeu a vista lateral e saiu chapado, sem rosto legível.
O `create_character` custa o mesmo e devolve personagem com silhueta, contorno e rosto.

### Os NPCs pintados não são pixel art — e isso se vê

`CHR_NPC_Penelope` e os outros cinco nomeados têm **6.000 a 8.500 cores** cada e importam a
**129 px por unidade**, contra os 42,857 de todo o resto. Em unidades de mundo o tamanho está
certo (1,30 un contra 1,40 do Odisseu), mas a densidade é **três vezes maior**: reduzidos para
o tamanho de jogo, eles borram — sem contorno, sem borda definida, o detalhe virando papa ao
lado de um Odisseu de silhueta limpa.

Não foram tocados. São seis personagens nomeados, arte aprovada, e trocá-los é decisão de
identidade do jogo, não de coerência de cenário. Fica registrado com a medida.

### O tamanho pedido não é o tamanho entregue

Pedindo `size: 64` os figurantes saíram com 70 px de conteúdo — 1,63 un contra os 1,40 un do
Odisseu, uma cabeça mais altos que o herói. O conteúdo sai cerca de **1,09× o tamanho pedido**;
com `size: 55` eles caem em 1,38–1,47 un, que é a faixa certa.

### Quantizar na paleta do personagem estragaria — e o número diz por quê

A primeira passada quantizou os figurantes na paleta do Odisseu por vizinho mais próximo, e
eles saíram inteiros de mostarda. A causa medida: as túnicas de linho cru ficam a **122 e 154**
da entrada mais próxima. A paleta do Odisseu **não tem branco nem off-white** — ele veste
bronze, couro e manto vermelho —, então o linho foi empurrado para a rampa de PELE.

É a mesma regra que já valia para o cenário, aplicada tarde: antes de quantizar, conferir se a
paleta cobre os materiais do asset. Os figurantes ficam nas cores próprias, que já são pixel
art limpa com contorno preto e 16 a 32 cores, na mesma família.

**A altura passada ao `Figura` é a nativa quando o sprite já está a 42,857 px por unidade.**
A arte pintada precisa da escala; a nova não pode ser tocada. O `EhPixelArt` decide pelo
`pixelsPerUnit` do sprite, não por convenção de nome.

---

## 16. Ferramentas de arte do projeto

Em `Tools/`, sem dependências — só o `zlib` do Node. Não há Python nesta máquina.

| Arquivo | O que faz |
|---|---|
| `png.js` | lê e escreve PNG (RGB / RGBA / paletizado), recorta, cola e mede caixa de conteúdo |
| `scale-probe.js` | mede tamanho em unidades e **detecta arte ampliada** por histograma de linhas repetidas |
| `ramp-map.js` | remapeia arte crua para a paleta por rampa, preservando o degradê interno |
| `compose.js` | banco de materiais: recorta parede, porta e janela da v3 e ladrilha por espelho |
| `map-materials.js` | mede onde estão porta, janela e as faixas de parede limpa |
| `palette-check.js` | acusa qualquer cor fora da paleta oficial |
| `seam-test.js` | teste de emenda contra fronteira interna equivalente |
| `build-house.js` `build-column.js` `build-wall.js` `build-house-odysseus.js` | as quatro peças |
| `build-architecture.js` | reconstrói o Grupo 3 inteiro e roda todas as verificações |
| `pixellab.js` | cliente JSON-RPC do MCP do PixelLab, que não carrega no escopo desta pasta |
| `pixellab-image.js` | dispara uma ferramenta de imagem, espera o job e salva os PNGs |
| `cutout.js` | recorta fundo chapado por flood-fill de borda (`--enclosed` para o cercado) |
| `build-ship.js` `build-ships.js` `ship-proof.js` | o Grupo 9 |
| `unity-import.js` | copia para `Assets/` e escreve os `.meta` com as configurações certas |
| `verify-meta.js` | confere os `.meta` fora do Unity, com o Editor aberto |
| `make-tileable.js` | torna uma camada ladrilhável por espelho de período 2W-2 |
| `quantize-scenery.js` | traz os Grupos 4 a 8 para a paleta, pulando o que ela não cobre |
| `pixellab-batch.js` | dispara vários jobs em paralelo (limite de 8 simultâneos) |
| `build-facade.js` | fachada larga: recorte + quantização, com o corte madeira/pedra por argumento |
| `build-gate.js` | folha do portão de progressão e o batente, desenhados por código |
| `build-npcs.js` | gera os arquétipos de figurante e baixa a rotação leste de cada um |
| `scale-proof.js` | acha a porta sozinho e gera a prova da seção 25 com o Odisseu no vão |
| `preview-architecture.js` | prancha do conjunto na mesma linha de chão, com o Odisseu |

O `scale-probe` existe por um motivo específico: um asset ampliado 2× **passa** no teste de
escala em unidades e mesmo assim destoa do personagem, porque a densidade de pixels fica pela
metade. É um erro invisível na medida óbvia. Rodar em todo asset novo antes de aceitar.

Não use MDC para detectar ampliação — uma única sequência ímpar zera o resultado. O sinal
honesto é o histograma.
