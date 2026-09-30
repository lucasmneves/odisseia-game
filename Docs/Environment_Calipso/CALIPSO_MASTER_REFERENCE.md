# CALIPSO_ENVIRONMENT_MASTER — Fase 13_Calipso

**Conceito A — Praia e floresta — aprovado em 2026-09-12.**
Paleta: `Palette/CALIPSO_PALETTE.gpl` · 10 rampas, 40 cores

> A ilha mais bonita do jogo, e uma prisão. A melancolia é de COMPOSIÇÃO, não de cor.

---

## 1. Identidade aprovada

| | Cores | Faixas de valor | Famílias de matiz | Veredito |
|---|---|---|---|---|
| **A — Praia** | 59 | 9 de 10 | 5 | **Direção aprovada** |
| B — Cachoeira | 92 | 10 de 10 | 6 | Vocabulário: falésia ocre, queda d'água, colunata |
| C — Isolada | 48 | 7 de 10 | 4 | Recusado — veio **isométrico** |

O Conceito B mede melhor em tudo e mesmo assim não venceu, e a razão vale registrar: ele é uma
**cena em perspectiva**, com o templo de frente e a lagoa recuando. O Conceito A é a única das
três em elevação lateral estrita, com linha de chão contínua — que é a projeção que um
sidescroller usa. Medida alta não salva projeção errada.

O Conceito C saiu isométrico, que é a armadilha já registrada do `pixen` com construções.

---

## 2. A melancolia não está na paleta

Não há uma cor triste aqui. O que carrega "paraíso que virou prisão" são três decisões de
arranjo, todas no vestidor:

1. **O mar está sempre visível.** Do primeiro ao último metro, e é a única camada que nunca
   muda. Numa fase de floresta densa isso é escolha: o horizonte aberto atrás de tudo lembra, a
   cada tela, que existe um lugar para onde ir e que ele não está ao alcance.
2. **Os objetos de gente vêm sempre sozinhos.** Um banco, uma fonte, uma coluna quebrada — um de
   cada, nunca dois juntos, nunca com nada em cima. Dois bancos lado a lado viram mobiliário; um
   banco sozinho vira ausência.
3. **A luz baixa conforme se anda.** O degradê âmbar do último terço é o sol se pondo sobre a
   saída — oposto exato do véu de tempestade de Gado do Sol, mesma ferramenta e cor contrária.

---

## 3. Dez rampas, e o que a separa de Gado do Sol

`Contorno` · `Bronze` (compartilhadas) · `Areia` · `Agua rasa` · `Agua funda` ·
`Folhagem clara` · `Folhagem funda` · `Rocha ocre` · `Marmore` · `Madeira`

As duas fases são ilhas mediterrâneas bonitas com verde, água e pedra clara, e sem uma decisão
explícita esta sairia como uma segunda Gado do Sol. O que as separa:

| | Gado do Sol | Calipso |
|---|---|---|
| Luz | céu AZUL de meio-dia | fim de tarde creme-dourado |
| Verde | pasto amarelo-esverdeado | floresta azul-esverdeada funda |
| Pedra | calcário dourado claro | **arenito OCRE saturado** (S 0,68–0,74) |

`Rocha ocre` é a cor mais saturada das duas paletas e Gado do Sol não tem nada parecido. É a
assinatura da ilha e também a peça que carrega a melancolia — ocre é a cor do sol baixo batendo
na falésia.

**Não há rampa de céu**, e a ausência é decidida: o céu é um degradê creme-dourado, e degradê não
sobrevive a quatro passos. Ele vive nas camadas de fundo, que por regra do projeto não são
quantizadas.

---

## 4. A falésia só saiu certa quando parei de NEGAR alvenaria

A primeira tentativa usou a formulação já registrada no projeto — *"not built by anyone, no
bricks, no masonry"* — e voltou em fiadas de blocos retangulares, como em Cila e Caribdis.

A segunda funcionou com o oposto: **descrever positivamente a forma natural e insistir na
propriedade que a alvenaria não tem — curva.**

> *"worn into soft rounded scallops and shallow scooped hollows, every edge CURVED and bulging,
> no flat surface anywhere and no straight edge anywhere"*

É a mesma razão pela qual `cila_wall_columnar` saiu perfeito de primeira: aquele prompt nomeava
*basalto colunar hexagonal*, uma estrutura natural específica. **Negar um conceito deixa o modelo
escolher o que pôr no lugar; nomear a alternativa não deixa.**

---

## 5. Três erros de classificação, todos do mesmo tipo

| Asset | Saiu | Causa | Correção |
|---|---|---|---|
| Cachoeira | queda d'água CINZA-PEDRA | branco caiu em `Marmore` | ramo de espuma antes de tudo quando o asset tem água |
| Árvore antiga | tronco de MÁRMORE BRANCO | casca pálida é acromática e caiu em `Marmore` | `cinzaVaiPara` declarado por asset |
| Tileset de praia | areia BRANCA, estourada | o passo mais claro de `Areia` era #f7f1ec, quase branco puro | o branco mudou para `Agua rasa`, onde ele é espuma; `Areia` ficou dourada |

Os três são a mesma coisa: **um ramo que resolve um material está resolvendo também todos os
outros que por acaso caem na mesma medida.** É a terceira fase seguida a bater nisso — Gado do
Sol pintou menires de azul, Ítaca Return teria pintado madeira prateada de pedra. A regra que
sobrou: **o destino dos ramos ambíguos é declarado por asset, não afinado no limiar.**

---

## 6. Erros de montagem que a captura pegou

**O mar tinha desaparecido por completo.** Com o fundo assente em baseY −4,4, a linha do
horizonte caía em y≈−0,4 — abaixo das copas — e a faixa de mata cobria o oceano inteiro: a
primeira montagem não tinha um pixel de mar em tela nenhuma. Numa fase cujo assunto é estar
preso numa ilha, isso não é detalhe de fundo. Subido para −1,2, o horizonte vai a y≈2,8 e sobra
mar aberto por cima das árvores.

**Camada de parallax não tem borda fixa em x.** A tentativa de recortar a mata pela esquerda
para liberar a praia não funciona: uma camada de parallax se desloca em relação ao mundo a cada
quadro, então encurtá-la só muda onde a borda passeia. A mata virou `Faixa` presa ao mundo — o
custo é perder o parallax dela, e o mar atrás continua com o seu.

**Espuma é onde a água encontra a areia, e em mais nenhum lugar.** Em faixa única de ponta a
ponta ela corria por dentro da floresta, com ondas quebrando ao pé da fonte. São duas faixas,
uma por praia. E elas ficam em ordem −30, ATRÁS do chão: em −8 desenhavam por cima da areia.

**Âmbar a 0,42 sobre mar azul não esquenta, EMBARRA.** As duas cores somam num marrom de lama.
O teto do degradê caiu para 0,26.

**Faixa espelhada tem pontas visíveis.** Uma árvore larga em cada junta as mascara, de graça.

---

## 7. Assets

| Grupo | Arquivos |
|---|---|
| Background | `calipso_bg_sea_horizon` (espelhado, não quantizado) |
| Midground | `calipso_forest_band` (espelhado) |
| Ocean | `calipso_foam_line` (espelhado) |
| Gameplay | `calipso_tiles_sand`, `calipso_tiles_grass` + as quatro peças de chão |
| Waterfall | `calipso_cliff_waterfall` |
| Palace | `calipso_palace` |
| Garden | `calipso_fountain`, `calipso_garden_flowers` |
| Forest | `calipso_tree_ancient`, `calipso_cypress` |
| Beach | `calipso_shore_rocks` |
| Foreground | `calipso_ferns` |
| Props | `calipso_bench_ruin` |
| Special | `calipso_raft` |
| VFX | `calipso_dusk_gradient` (construído por código) |

**Quatro dos catorze voltaram com o xadrez de transparência PINTADO** (palácio, falésia, árvore,
jangada). `Tools/checker-cut.js` cobra e corta automaticamente — foi escrito na rodada anterior
exatamente por isso.

---

## 8. Verificação

```
Unity.exe -batchmode -quit -nographics -projectPath . -executeMethod PlaceholderProbe.Run \
          -probeScene Assets/Scenes/Levels/Level_13_Calipso.unity
```

| Medida | Valor |
|---|---|
| Sprites na cena | 54 |
| Camadas de parallax | 2 |
| Objetos no cenário | 42 |
| Extensão | 60 un (−16 a 44) |
| Placeholders pendentes | **0** |
| Gerações gastas | **17** (3 conceitos + 8 + 6 correções, mais 2 kits de tileset) |
