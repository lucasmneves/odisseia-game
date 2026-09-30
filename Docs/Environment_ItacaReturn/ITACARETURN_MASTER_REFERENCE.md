# ITACARETURN_ENVIRONMENT_MASTER — Fase 14_Itaca_Return

**Conceito C — Dourado — aprovado como LUZ em 2026-09-12.**
Paleta: **`Docs/Environment_Ithaca/Palette/ITHACA_PALETTE.gpl` — a da FASE 01, sem trocar um hex.**

> A única fase da campanha sem paleta própria. Isso não é economia: é o requisito.

---

## 1. A decisão central: não há paleta nova

Todas as doze fases anteriores ganharam uma paleta. Esta usa a da fase 01, idêntica.

O briefing pede que o jogador reconheça Ítaca imediatamente. **Cor é a coisa que o olho
reconhece antes da forma** — uma paleta nova, por mais parecida que fosse, diria "outro lugar"
antes de qualquer prédio aparecer na tela. Então a cor fica igual e a mudança vem de outro
lugar.

Pelo mesmo motivo, **40 dos 56 objetos de cenário são arte da fase 01 carregada direto de
`Assets/Art/Environments/Ithaca/`**, sem cópia e sem variação: o palácio, a casa de Odisseu, a
casa pequena, o armazém, o portão, a coluna, o muro, a oliveira, o cipreste, as pedras, o navio,
o poste de cais, a rede, a âncora, a ânfora, o barril, a carroça, o banco, a cerca, a silhueta
de montanhas e os dois tilesets de chão.

**Reconhecimento não se produz com arte parecida. Produz-se com a mesma arte.**

---

## 2. Os conceitos, e por que o mais bonito venceu só em parte

| | Cores | Faixas de valor | Famílias de matiz | Veredito |
|---|---|---|---|---|
| A — Amanhecer | 52 | 9 de 10 | **6, equilibradas** | Aprovado como referência de EQUILÍBRIO |
| B — Caminho | 67 | 8 de 10 | 6, mas **laranja 60%** | Vocabulário: trilha, muros secos, cabras |
| **C — Dourado** | 67 | 9 de 10 | 5, **laranja 61%** | **Aprovado como LUZ** |

B e C são ~60% uma só família de matiz — a condição que achatou Lestrigões. O Conceito A é o
único equilibrado, e é também o mais próximo da paleta da fase 01.

A resolução não foi escolher um: **C dá a LUZ, e a paleta continua sendo a da fase 01** — que
já traz o azul, o verde e o cinza que faltam ao C. O dourado entra como camada de iluminação
sobre assets reusados, não como repintura. É o mesmo padrão da fase 11, em que o conceito
vencedor tinha a identidade certa e a estrutura errada.

E havia um segundo argumento, narrativo: a fase 01 é meio-dia azul. Se a 14 fosse **amanhecer**,
as duas ficariam frias e a diferença seria sutil. Fim de tarde → anoitecer é imediatamente
legível **e** aponta para a fase 15, que é o confronto.

---

## 3. O que mudou, então

Três coisas, e as três são deliberadas:

1. **A LUZ.** Fundo de fim de tarde, mais um degradê violeta-azulado de anoitecer no último
   terço. Mesmo lugar, outra hora — a diferença que o olho lê primeiro e a que não exige nenhum
   asset novo.
2. **O ESTADO.** Casa tomada de hera, cerca quebrada, barco apodrecendo na praia, mato crescido.
   **Os prédios reusados ficam INTACTOS de propósito**: o contraste entre o que resistiu e o que
   não resistiu é o que conta a passagem do tempo. Se tudo estivesse arruinado, seria outra
   cidade.
3. **O QUE PASSOU A EXISTIR.** A barraca de feira e os braseiros acesos não têm equivalente na
   fase 01. A feira diz que a vila seguiu viva sem ele; os braseiros, que alguém está esperando
   alguma coisa esta noite.

Os pares são montados lado a lado de propósito: navio intacto **e** casco apodrecido no porto;
cerca inteira **e** cerca quebrada na vila; casas em pé dos dois lados da casa tomada de hera.

---

## 4. Comparação direta com a fase 01

| | 01 Itaca Prologue | 14 Itaca Return |
|---|---|---|
| Paleta | ITHACA_PALETTE | **a mesma** |
| Céu | azul de meio-dia | fim de tarde dourado → anoitecer |
| Navio | vela ABERTA (`ithaca_ship_01`) | vela RECOLHIDA (`ithaca_ship_01_furled`) — o mesmo navio, parado |
| Palácio | em cena | **em cena, o mesmo arquivo** |
| Estado | tudo cuidado | um terço tomado de mato |
| Fogo | tocha | dois braseiros acesos na porta de casa |
| Extensão | 351 un | 90 un |

A vela é o detalhe mais barato e o mais eloquente: a fase 01 já tinha os dois sprites. Na 01 ele
parte; aqui ele está parado. Variação com custo zero.

---

## 5. O braseiro não é quantizado, e a exceção já existia

A paleta de Ítaca **não tem rampa de fogo**. Está registrado desde a fase 01, onde a tocha foi
um dos quatro assets que ficaram fora da paleta pelo mesmo motivo. Quantizar uma chama numa
paleta sem laranja não a aproxima: joga o fogo na rampa errada e destrói o objeto.

---

## 6. Erros de montagem que a captura pegou

**O palácio tomava o quadro inteiro.** Em escala 0,90 ele mede 16 × 6,3 unidades contra os
17,8 × 10 que a câmera mostra: lia como uma parede, sem céu nem mar em volta. O reencontro
funciona melhor com ele inteiro em quadro do que com uma coluna dele em close. Em 0,62 sobra
céu por cima e ele ainda é de longe o maior prédio do jogo.

**Espelhar uma faixa cria um eixo de simetria, e o olho pega.** A faixa de vila espelhada
produziu, no eixo, um prédio branco perfeitamente simétrico em forma de borboleta — bem ao lado
da porta de casa. O eixo é calculável (a faixa começa em −3, período 31,31 → eixo em x≈44) e a
câmera desta cena enxerga até 53, então ele **está** em quadro. Duas peças da fase 01 postas ali
mascaram, mais barato que regerar.

**Folga do chão tem de cobrir o alcance da CÂMERA, não o fim do chão.** Com folga de 8 sobrava
um buraco magenta no canto inferior direito do último quadro, porque a câmera vai a x=44 e
enxerga até 52,9. Passou para 12.

**Um degradê precisa de duas coisas incompatíveis, e a rampa resolve.** O véu tem de chegar ao
teto onde o jogador chega **e** se estender além do fim da fase para a borda direita não
aparecer. Com rampa de 70% não cabem as duas: ou termina cedo e a borda aparece, ou se estende e
o alpha na porta de casa fica em 0,19 — invisível. Rampa de 45% e platô de 55% resolvem: teto em
x≈38, que é a porta, e o sprite seguindo até 56.

**O barranco de laje precisa ser mais escuro que o de grama.** A folha de pedra da fase 01 é
clara; no mesmo tom da grama ela lia como muro de tijolo aparente ocupando o terço de baixo da
tela.

---

## 7. Assets NOVOS (seis, mais o degradê)

| Arquivo | Por que existe |
|---|---|
| `itaca_ret_bg_dusk` | a mudança de hora — a peça que carrega tudo |
| `itaca_ret_village_band` | a vila ao fundo; a fase 01 tem prédios soltos, não faixa |
| `itaca_ret_house_overgrown` | tempo |
| `itaca_ret_fence_broken` | tempo, em par com a cerca inteira |
| `itaca_ret_boat_derelict` | tempo, em par com o navio intacto |
| `itaca_ret_market_stall` | a vila seguiu viva sem ele |
| `itaca_ret_brazier` | alguém está esperando esta noite |
| `itaca_ret_overgrowth` | abandono — sempre ao pé de algo construído |
| `itaca_ret_dusk_gradient` | construído por código |

---

## 8. Verificação

```
Unity.exe -batchmode -quit -nographics -projectPath . -executeMethod PlaceholderProbe.Run \
          -probeScene Assets/Scenes/Levels/Level_14_Itaca_Return.unity
```

| Medida | Valor |
|---|---|
| Sprites na cena | 77 |
| Camadas de parallax | 3 |
| Objetos no cenário | 62 — **40 reusados da fase 01, 16 novos** |
| Extensão | 90 un (−35 a 55) |
| Placeholders pendentes | **0** |
| Gerações gastas | **11** (3 conceitos + 8 assets) |

---

## 9. Revisão de 2026-09-12 — segundo briefing (fases 14 e 15)

A fase já estava construída e verificada. O segundo briefing foi tratado como **revisão**, não
reconstrução: os três conceitos e a decisão de paleta continuam valendo, e só entrou o que o novo
texto pede e a primeira montagem não tinha.

**Acrescentado:**

| O quê | Por quê | De onde |
|---|---|---|
| Navio dos pretendentes atracado atrás da galera de Odisseu | "novos barcos" no porto — e o primeiro sinal da invasão chega pelo mar | kit da fase 15 |
| Dois estandartes do javali no palácio | "área controlada pelos pretendentes" antes da transição para a 15 | kit da fase 15 |
| Barricada encostada no muro e tocha dos pretendentes | idem | kit da fase 15 |

Os sinais de ocupação são **os mesmos sprites** que a fase 15 usa. A 14 não mostra a ocupação —
ela a anuncia, e o jogador reencontra exatamente estes objetos, multiplicados, na fase seguinte.
Todos em ordem negativa: a barricada é cenário, não obstáculo.

**Corrigido — defeito que passou na primeira montagem:** vazamento **magenta** na linha do chão.
Os tilesets de Ítaca têm falhas na fileira de superfície, e o bloco de terra terminava em y=−3,
então a faixa entre −3 e −2 ficava descoberta atrás delas. **Medido na captura: até 104 pixels
por quadro nesta fase, todos na linha y=378** (y=−2,0 no mundo). O bloco passou a subir até −1,9.
Recontado depois: zero em todas as capturas. Calipso não tinha o defeito porque os tiles dela têm
a superfície fechada — e foi a comparação entre as duas que apontou a causa.

**Não regerado:** os três conceitos do novo briefing (o C agora pede "céu parcialmente nublado").
Regerar para chegar à mesma decisão numa fase já construída gastaria gerações sem mudar a
direção; a tensão do fim da fase já vem do véu de anoitecer e, agora, dos sinais de ocupação.

| Medida | Antes | Depois |
|---|---|---|
| Objetos no cenário | 62 | 67 |
| Reusados da fase 01 | 40 | 40 |
| Sprites na cena | 77 | 82 |
| Pixels magenta por quadro | até 104 | **0** |
