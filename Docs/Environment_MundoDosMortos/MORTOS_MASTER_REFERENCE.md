# MORTOS_ENVIRONMENT_MASTER — Fase 09_MundoDosMortos

**Conceito A — Caverna com rio — aprovado em 2026-09-06.**
Paleta: `Palette/MORTOS_PALETTE.gpl` · 10 rampas, 40 cores

> Subterrâneo, sem céu. A fase que mais dependeu de MEDIR em vez de olhar: quase todo defeito
> aqui era invisível na captura e óbvio no histograma.

---

## 1. Identidade aprovada

| | Resultado |
|---|---|
| **A — Caverna com rio** | **Aprovado.** Arcada de pedra alta, teto de estalactites, água parada correndo abaixo do caminho. Estabelece "subterrâneo" antes de qualquer objeto. |
| B — Ruínas | Aproveitado como vocabulário, não como direção: colunas, muro e estátua entraram no trecho final. |
| C — Vale | Recusado: tinha horizonte e luz de céu, e não existe céu aqui. |

O percurso vai de x=−16 a x=38 e muda de material uma vez: chão de caverna até x=20, laje de
ruína depois — que é onde ficam o `DialogueTrigger_Mother` e o objetivo.

---

## 2. As três camadas pintadas que a cena já trazia

A cena vinha com `BG_Fase03_Underworld_Far/Mid/Near`, de um pipeline anterior. Elas **foram
desligadas**, e o briefing pede para não substituir assets existentes sem necessidade — então a
necessidade está medida, não opinada:

| | Cores únicas |
|---|---|
| `BG_Fase03_Underworld_Far` | 128.014 |
| `BG_Fase03_Underworld_Mid` | 73.862 |
| `BG_Fase03_Underworld_Near` | 71.130 |
| *(referência)* sprite do Odisseu | **51** |

São ilustrações suavizadas, não pixel art na densidade do projeto — e ainda eram **vermelho-lava**,
contra a iluminação azulada/arroxeada que o briefing pede para esta fase.

Elas continuam na cena, apenas com o renderer desligado, e voltam mudando uma linha em
`DesligarPlaceholders()`.

---

## 3. Escuro sem virar Lestrigões

O risco declarado da fase era ficar escura demais para ser jogada. A solução **não** foi clarear:
foi dar à paleta quatro famílias de matiz — pedra fria azulada, água verde-morta, mármore pálido,
fogo quente — e pôr as fontes de luz nos pontos narrativos.

A hierarquia certa é **a chama ser o mais claro do quadro**, e na primeira montagem ela não era:

| | L média antes | depois |
|---|---|---|
| laje de ruína | 0,436 | 0,32 |
| chão de caverna | 0,358 | 0,31 |
| árvore morta | 0,507 | ~0,31 |

A árvore lia como **coberta de neve** — branco puro contra pedra escura vira inverno, não galho
seco. `Apagar()` puxa para o azul-pedra em vez de dessaturar para cinza, que foi exatamente o
erro que achatou Lestrigões.

---

## 4. O rio, e por que ele lia como grama

Três defeitos independentes, um de cada vez:

1. **Sem especular.** Medida, a banda era 38,6% + 36,1% + 13,1% de verdes chapados e **1,5% de
   brilho**. Uma superfície verde plana é um gramado. `Tools/fix-mortos-river.js` acrescenta
   riscos horizontais longos por composição sobre o asset gerado — riscos verticais leriam como
   chuva, e pontos como cascalho. Área clara: **1,5% → 14%**.
2. **Ordem errada.** Estava em −18, atrás do barranco (−12), e só aparecia nas frestas entre
   trechos de chão — daí "manchas de grama soltas". Foi para −11, entre barranco e chão.
3. **Coberto pela face do chão.** O tile de chão tem 3,73 un e engolia a janela onde a água
   deveria aparecer. Ver §5.

O rio gerado pelo PixelLab foi **descartado** (`_rio_gerado_descartado.png`): a banda por código
ladrilha sem emenda e responde a `ScrollingLayer`. Geometria é trabalho de código; o orgânico é
que vale gerar.

---

## 5. Encurtar faixa pelo `size` guarda a BASE do tile

A armadilha mais cara da fase, e ela é do Unity, não da arte.

Com o pivô na base, `SpriteDrawMode.Tiled` desenha de baixo para cima: reduzir `size.y` **mantém
a base do tile e joga fora o topo**. O chão de caverna é desenhado para se dissolver no escuro,
e sua base mede L 0,17 — então a face sob o jogador saiu **preta** no trecho de caverna e normal
no de ruína (base L 0,26). Foi essa diferença, e não a troca de material, que produziu o corte
vertical duro em x=20.

A correção é recorte de pipeline, não parâmetro de renderer: `Tools/build-mortos-edge-tiles.js`
gera `*_edge.png` cortados **do topo** a 1,8 un. Os dois materiais passam a medir L 0,52, e a
junção deixa de ser uma linha — coberta ainda por um monte de pedra, como nas fases anteriores.

**Regra que fica:** encurtar uma faixa Tiled é recortar o tile, e é preciso escolher qual ponta
sobrevive.

---

## 6. Atmosfera

| Camada | Fator | Movimento |
|---|---|---|
| Névoa de fundo | 0,50 | desliza a −0,35 un/s |
| Almas | 0,34 | desliza a −0,50 un/s |
| Névoa da frente | 0,12 | desliza a −0,85 un/s, alpha 0,45 |

As três usam `ScrollingLayer` (criado em Eolo), que responde ao TEMPO — o parallax responde à
câmera e deixaria as almas congeladas sempre que o jogador parasse, que é o oposto do que uma
fase de almas precisa. A névoa é veladura: o briefing proíbe que ela esconda jogador, inimigo ou
plataforma.

---

## 7. O que o `PortalGlow` ensinou

Ele é um quadrado branco de 3×3 — e foi o **único placeholder da cena que não podia ser apenas
desligado**, porque é a pista de gameplay que diz onde a fase acaba. Apagá-lo tiraria informação
do jogador, não sujeira da tela.

A saída é desenhada por cima, na medida dele: portão de pedra atrás, luz das almas na frente. O
objeto do level design não é tocado — nem sprite, nem escala — como em todo o resto da cena.

---

## 8. Assets

**16 gerações** (3 conceitos + 8 + 5), 18 assets no disco. Rio, preenchimentos de rocha, água
funda e os dois tiles de borda são código ou recorte, a custo zero de geração.

**Reuso:** nenhum de outra fase. As rampas `Contorno`, `Bronze` (personagem) e `Fogo` (Troia)
são compartilhadas.

**Não é gerado:** as sombras, Tirésias e a mãe de Odisseu. O briefing proíbe gerar personagens, e
o espaço em volta dos dois altares fica livre para eles e para o diálogo entrarem depois.

---

## 9. Honestidade sobre o resultado

Três fraquezas conhecidas:

- **O brilho da saída (`Exit_Glow`) quase não aparece** contra o portão. Funciona como reforço,
  não como chamada — quem guia o jogador ali é o portão de pedra.
- **O teto de estalactites repete visivelmente.** É uma banda de 11,9 un ladrilhada por 60 un.
- **A ponta direita em x=38** ainda mostra uma quina do barranco, parcialmente coberta pelo monte
  de pedra. Só é vista se a câmera chegar ao extremo do nível.

Nenhuma delas atrapalha a leitura do gameplay, que é a prioridade declarada do briefing.
