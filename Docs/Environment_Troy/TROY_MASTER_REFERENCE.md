# TROY_ENVIRONMENT_MASTER — Fase 02_Troia

**Conceito B (Muralha) aprovado em 2026-09-04.** Etapa 2 de 3 concluída.

Conceitos em `Concepts/`; os três da primeira leva, descartados, em `Concepts/_descartados/`.

---

## 1. Identidade aprovada

**A muralha é o master.** É o clímax visual da fase — a Área 4 do briefing chama a
aproximação das muralhas de "a grande revelação visual" —, define o material dominante
(alvenaria ciclópica de calcário claro) e tem a paleta mais limpa dos três conceitos: 25 cores
contra 46 e 51.

Os outros dois conceitos **não foram descartados**. Acampamento e Batalha são as Áreas 2 e 3 da
mesma fase, não identidades concorrentes; servem de referência para essas regiões.

### Ítaca contra Troia

| | Ítaca | Troia |
|---|---|---|
| Material dominante | calcário quente irregular, madeira | **calcário ciclópico**, blocos enormes |
| Terreno | grama, terra, pedra, madeira | **terra seca e rachada** |
| Cor de acento | nenhuma — tudo é terra e folhagem | **vermelho, bronze, fogo** |
| Escala | casa de 3 un, palácio de 7 | **muralha acima da tela** |
| Vegetação | oliveira, cipreste, moita | rala e seca; a fase é sítio de guerra |

---

## 2. Paleta oficial

**Onze rampas de quatro passos, 44 cores.** Arquivos em
`Palette/TROY_PALETTE.{hex,gpl,png}`, gerados por `node Tools/extract-palette.js`.

| Rampa | Origem | Highlight | Base | Sombra | Profunda |
|---|---|---|---|---|---|
| Contorno | **personagem** | `#2b2e27` | `#1d221d` | `#141415` | `#050507` |
| Céu | **Ítaca** | `#ceeefe` | `#94d0f3` | `#74b5ed` | `#6481a0` |
| Pedra clara | B | `#f2e8c4` | `#e3d6b7` | `#d2c2a0` | `#c8b690` |
| Pedra sombra | B | `#b39c78` | `#9c8a72` | `#836c52` | `#6f5a45` |
| Terra seca | C | `#d7b97a` | `#b0844c` | `#8e6845` | `#623d25` |
| Madeira | B | `#8f6942` | `#653f2f` | `#4d3329` | `#2e1e1c` |
| Lona | A | `#ece0ba` | `#d5bb8d` | `#b1966d` | `#7e6648` |
| Bronze | A | `#f9d688` | `#bf8e52` | `#885e34` | `#5f4431` |
| Vermelho | A e B | `#a4462c` | `#953029` | `#5c2b24` | `#381f1a` |
| Fogo | A e C | `#fcd84e` | `#e3a14d` | `#f1681a` | `#a7311d` |
| Fumaça | C | `#e6e8e2` | `#a09486` | `#887c74` | `#725f4a` |

### Duas rampas são compartilhadas, de propósito

**Contorno** vem da paleta do personagem. O `#050507` já é comum ao Odisseu e a Ítaca; dar a
Troia um preto próprio separaria as duas fases na única cor que aparece em absolutamente todo
sprite.

**Céu** vem de Ítaca. O céu do Mediterrâneo é o mesmo, e o azul dos conceitos de Troia era um
preenchimento chapado incidental — quatro tons quase idênticos (luminância 199 a 220), sem
faixa para virar rampa de degradê.

As outras nove saem dos pixels reais dos conceitos.

### O que Troia tem e Ítaca não

**Bronze, Vermelho e Fogo.** A paleta de Ítaca não cobre nenhum dos três, e isso já tinha
custado: a tocha de Ítaca ficou fora da quantização porque a chama `#fb7507` estava a **116**
da entrada mais próxima, sem rampa de fogo para receber. Em Troia, fogo é elemento de primeira
classe do briefing.

### Método de extração, e o erro do caminho

A primeira versão do extrator amostrava **retângulos** de cada conceito — e errou feio: pegou
céu no estandarte, verde no bronze e passos repetidos em quatro rampas. Coordenada chutada num
conceito de 672×384 acerta o material errado com facilidade.

O que funciona é o inverso: levantar o **inventário de cores** dos três conceitos ordenado por
matiz e luminância, olhar a grade, e escolher os quatro passos de cada material. Todo hex da
tabela aparece de fato nos conceitos. O script confere que cada rampa desce em luminância antes
de gravar — passo fora de ordem quebra o remapeamento, que distribui as cores do material pelos
passos justamente por luminância relativa.

---

## 3. Escala — herdada, não negociável

Igual a Ítaca, porque vem do personagem:

| | |
|---|---|
| Corpo do Odisseu | **60 px = 1,4 unidades** |
| Pixels por unidade | **42,857143** |
| Câmera | ortográfica tamanho 5 — **10 un de altura**, ~17,8 de largura |

### Referências derivadas para Troia

| Elemento | Altura alvo | Em pixels |
|---|---|---|
| Muralha | **10–14 un** | 429–600 px |
| Torre | 14–18 un | 600–771 px |
| Portão (vão) | 4–6 un | 171–257 px |
| Tenda | ≈ 2,5 un | ≈ 107 px |
| Barricada | ≈ 1,2 un | ≈ 51 px |
| Fogueira | ≈ 0,8 un | ≈ 34 px |
| Escudo apoiado | ≈ 0,9 un | ≈ 39 px |

**A muralha é mais alta que a tela de propósito.** A câmera mostra 10 un; uma muralha de 12 un
não cabe em quadro, e é isso que faz o jogador sentir que Troia é grande. É o oposto da casa de
Ítaca, que tinha de caber inteira.

O vão do portão segue a regra da seção 25 do briefing de Ítaca: **acima de 1,4 un**, com folga
verificável por `Tools/scale-proof.js`.

---

## 4. Iluminação — sombreamento de forma

A seção 12 do briefing de Troia pede luz solar do canto superior esquerdo. **A regra do projeto
é outra, e por medida.**

O master do Odisseu não tem luz direcional: correlações de luminância fracas (|r| < 0,27) e
componente vertical trocando de sinal entre as vistas. O que o PixelLab aplica é sombreamento
de forma, e foi essa a regra adotada em Ítaca.

Aplicar luz direcional em Troia faria a fase divergir do personagem — o oposto da seção 14 do
próprio briefing, que exige consistência com o Odisseus Master.

**Fogo é exceção local:** uma fogueira pode clarear o que está imediatamente ao redor dela, no
próprio sprite. Não há iluminação dinâmica.

---

## 5. Arquitetura — o que faz Troia não virar castelo

A primeira leva de conceitos saiu **medieval europeia** nos três: arco ogival, ameias e telhado
de telha inclinado. A causa não é a ferramenta — é que "muralha fortificada com torres e
portão" tem castelo europeu como resposta padrão do modelo, e **exclusão no prompt não
segura**, o que já estava registrado em Ítaca.

O que segura é enquadramento **positivo**, nomeando o que Micenas tem:

- alvenaria **ciclópica**: blocos enormes e irregulares, não fiadas regulares de tijolo
- base da muralha **em talude**, alargando para baixo; a parte de cima vertical
- torres de **topo plano** com parapeito liso de pedra esquadrada
- portal de **verga reta**: uma viga de madeira maciça atravessando o vão, nunca arco
- portas de **pranchas verticais** com bandas de ferro

E a fórmula de enquadramento que já vinha funcionando em Ítaca para prédio:

> *"seen from directly in front, strict flat side profile like a technical elevation drawing"*

Sem ela, `view: side` sozinho não segura: dois dos três conceitos da primeira leva saíram em
vista de cima.

---

## 6. Ferramentas e custo

Todo o pipeline de Ítaca vale aqui — `Tools/` é compartilhado. O que muda é a paleta passada
aos scripts.

**Gasto até aqui: 9 gerações** (3 conceitos descartados + 1 calibração + 2 conceitos + 3 da
primeira leva). Estimativa para a fase inteira: 50–70.

### Uma métrica que engana

Ítaca escolheu o conceito por afinidade de paleta contra o personagem. Em Troia essa métrica
**inverte**: os conceitos descartados medem 89 a 100% e os aprovados 72 a 76%.

Os descartados pontuam mais porque são escuros e dessaturados, e a paleta do Odisseu é de couro
e bronze escuros. O conceito vencedor de Ítaca mediu 75% — exatamente a faixa dos aprovados de
Troia. **72–76% é a zona certa**, e otimizar esse número levaria à paleta escura que a seção 11
do briefing proíbe.

---

## 7. Etapa 3 em andamento

### Pronto

| Grupo | Assets | Custo |
|---|---|---|
| Tileset | `troy_tiles_{earth,stone,wood}` — 16 tiles Wang de 32 px cada | 9 |
| Arquitetura | `troy_wall_section_01` (11,90 × 11,88 un, ladrilha), `troy_tower_01` (4,48 × 13,39), `troy_gate_01` (7,42 × 10,20) | 3 |
| Parallax | `troy_bg_sky`, `troy_bg_haze`, `troy_bg_city` — os três **por código** | **0** |
| Acampamento e militares | tenda grande e pequena, fogueira, suprimentos, estandarte, barricada, feixe de lanças, escudo quebrado | 8 |

O parallax saiu de graça porque banda plana e silhueta distante vão de procedural — a regra do
pipeline de Ítaca, onde o céu tentado pelo `pro` custou 40 gerações e voltou com emenda de 45 a
78%. Os três ladrilham e usam só cores da paleta. A silhueta da cidade tem **duas cores
chapadas**, de propósito: a muralha real, com 16 cores, competiria com o primeiro plano.

Montagem de referência em `Architecture/_conjunto_muralha.png`.

**O encadeamento dos tilesets funcionou.** Em Ítaca os quatro kits saíram independentes porque
o regex da sessão não casou o formato `base_tile_id (for chaining):` da resposta. Aqui os três
kits saem do mesmo `base_tile_id`, e `Tools/build-tilesets.js` diz na saída se pegou o id do
JSON ou do texto.

### Quatro classificadores, e por quê

Troia tem dois: `materialTroia` para arquitetura e `materialAcampamento` para o acampamento.
Somados aos dois de Ítaca, são quatro no projeto — e não é excesso.

Calcário e lona **não se distinguem por número**: o calcário do portão mede saturação 0,20 com
luminância 192, e a lona da tenda 0,15–0,21 com 186–227. O que resolve é o grupo: num asset de
muralha não há lona, num de acampamento não há calcário.

**Fogo virou opcional por asset** (`temFogo`). Alguns pixels de brilho da madeira e dos sacos
passavam do corte de saturação, caíam num grupo Fogo de meia dúzia de pixels, e o remapeamento
os espalhava pela rampa inteira — lascas laranja vivas em cima de lança e saco. Fogo é material
raro e concentrado; tratá-lo como opcional custa um booleano.

### Quatro armadilhas novas, todas com correção na ferramenta

- **`get_sidescroller_tileset` não devolve a folha inline.** Manda `download_png` e
  `download_metadata` por URL, ao contrário das ferramentas de imagem. Esperar imagem inline
  trava o polling até o tempo esgotar — foi o que aconteceu na primeira rodada.
- **O `cutout` come o asset quando ele sangra até as bordas.** A seção de muralha preenche o
  quadro, então as cores dela entraram na lista de referência de fundo e o flood-fill removeu
  92% da imagem, deixando fragmentos. Sem erro nenhum no caminho. O `cutout.js` agora recusa
  acima de 85% de remoção e manda tratar o asset como textura.
  Tentei detectar pela borda e pelo miolo antes; as duas medidas reprovavam o casco do navio de
  Ítaca, que é recorte legítimo com 75% de fundo.
- **A argamassa da muralha saiu vermelha.** O ramo de Vermelho do classificador pegava matiz
  até 25, e a argamassa fica em 23–24 com saturação 0,38. O estandarte de verdade está em 353.
  A faixa fechou em 330–12 com saturação 0,35.

### Falta

Parallax, acampamento, props militares, efeitos, foreground — e a montagem na cena.

Nada foi copiado para `Assets/` ainda. O destino é
`Assets/Art/Environments/Troy/`, ao lado de `Ithaca/` e sem misturar.
