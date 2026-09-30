# CIRCE_ENVIRONMENT_MASTER — Fase 08_Circe

**Conceitos A (mundo) + B (templo) + C (ruínas) — aprovados em 2026-09-06.**
Paleta: `Palette/CIRCE_PALETTE.gpl` · 12 rampas, 48 cores

> Floresta encantada. É a fase com mais matiz do jogo, e por isso a que separou os planos com
> menos esforço — o oposto exato de Lestrigões.

---

## 1. Identidade aprovada

Os três conceitos saíram bons e em perfil correto, e **os três foram aceitos** porque
correspondem a partes diferentes da fase, que a geometria já define:

| | Papel |
|---|---|
| **A — Floresta** | direção do mundo: troncos antigos, copa em camadas, luz filtrada, samambaias, névoa |
| **C — Jardim em ruínas** | a transição: colunas tombadas, muro quebrado, bacia d'água, estátua, flores |
| **B — Templo** | o domínio de Circe: colunas, verga reta, hera e raízes subindo, braseiro |

`Floor_1` é a praia, `Floor_2` a mata e as ruínas, `Floor_3` o domínio — e a fronteira sai da
`TransformationZone_Big`, que o level design já pôs onde a fase muda de caráter.

---

## 2. Três verdes, separados por luminância

`Folha ao sol` · `Folhagem` · `Sombra verde`. A divisão é por **luminância, não por matiz**:
numa floresta o mesmo verde aparece iluminado na copa, médio no meio e quase preto na sombra, e
separá-los por matiz partiria uma mesma folha ao meio.

É essa divisão em três que faz os planos se separarem sozinhos — exatamente o que faltou em
Lestrigões, onde oito das onze rampas eram cinza.

A **rampa de terracota entrou depois**: sem ela as ânforas caíam nos marrons frios de `Casca` e
`Tronco escuro` e liam **arroxeadas** contra o verde quente da mata. Reusar a ânfora de Cícones
trocou um problema de cor por outro — o lado sombreado dela usa os azuis frios daquela paleta.
Uma rampa quente própria resolveu os dois.

Compartilhadas: **Contorno** e **Bronze** (personagem), **Fogo** (Troia).

---

## 3. Não há céu

O que se vê acima é **luz filtrada pela copa**, dourada e abafada. Pôr o azul das outras fases
aqui abriria um buraco de céu no meio da mata. A camada de fundo clareia **para baixo**, na
direção do horizonte, que é por onde a luz entra numa floresta fechada.

A cobertura acima de tudo é `#8d734e`, o passo escuro da luz dourada — e não um azul.

---

## 4. A copa que virou mata de fundo

A copa gerada veio com fundo escuro ocupando **93%** da imagem, e a guarda do `cutout` recusou
o recorte com razão: as folhas eram 7%, sparse demais para ler como overlay. Em vez de forçar,
ela virou a **banda de mata do meio de campo**, opaca — que era justamente o que faltava para
fechar o fundo da floresta.

Defeito local vira reaproveitamento, não geração nova.

---

## 5. Magia sutil, sem sistema de partículas

Três camadas de alpha baixo, e não neon:

| Camada | Fator | Movimento |
|---|---|---|
| Feixes de luz | 0,72 | **parado** — luz que atravessa a copa não anda |
| Névoa | 0,30 | desliza a −0,55 un/s |
| Poeira de luz | 0,18 | desliza a −0,9 un/s |

As duas que se movem usam `ScrollingLayer`, o componente criado em Eolo, que responde ao TEMPO
— o parallax responde à câmera e sozinho deixaria tudo parado quando o jogador para.

A névoa é **veladura, não massa**: quantizada para a rampa de mármore ela subiu até o branco
puro e, opaca, lia como neve amontoada ao pé das árvores. Alpha 0,28 resolve.

---

## 6. Escala dos props

A escala vinda da geração é sempre grande demais para prop de cenário. Medido nesta fase:

- flores luminosas: 4,48 un nativas → **0,26** (moita de 1,2 un, abaixo do ombro do Odisseu)
- estátuas: 6,72 un → **0,45** (3 un, o dobro do jogador, que é o que uma estátua votiva tem)
- árvores de fundo: **0,85** e recuadas em valor

Flores maiores que o personagem deixam de ser magia sutil, que é o que o briefing pede.

---

## 7. Assets

**17 gerações.** Catorze assets gerados, mais céu, mata de fundo, feixes, partículas e rasteira
por código (6 camadas, custo zero).

**Reuso:** nenhum de outra fase no resultado final — a tentativa de reusar as ânforas de Cícones
foi revertida por incompatibilidade de paleta, e a lição está registrada em §2.

**Circe não é gerada.** O vão da porta do templo fica livre: é onde ela, o Odisseu e o diálogo
entram depois.
