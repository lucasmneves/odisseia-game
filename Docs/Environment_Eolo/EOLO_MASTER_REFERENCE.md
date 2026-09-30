# EOLO_ENVIRONMENT_MASTER — Fase 06_Eolo

**Conceitos C (mundo) + B (palácio) — aprovados em 2026-09-06.**
Paleta: `Palette/EOLO_PALETTE.gpl` · 12 rampas, 48 cores

> Ilha dos ventos. É a fase mais clara e mais alta do jogo, e a única em que o cenário se move
> sozinho.

---

## 1. Identidade aprovada

| | Resultado |
|---|---|
| A — Ilha elevada | Recusado: saiu em **três-quartos**, e a balaustrada veio renascentista. |
| **B — Palácio** | **Aprovado para a arquitetura.** Perfil reto, colunas, entablamento reto sem arco, degraus, braseiros, estandartes. |
| **C — Arquipélago** | **Aprovado como direção do mundo.** Ilhas flutuantes com topo de grama e base esfarrapada, pontes de corda, linhas de vento. |

Os dois foram aceitos porque **correspondem a partes diferentes da fase**, que a geometria já
define: `Floor_1` a `Floor_3b` são a travessia (ilhas, vão e ponte) e `Floor_4` é o terraço do
palácio. Não é indecisão — é a fase tendo duas metades.

---

## 2. Duas rampas de céu

`Céu de altitude` e `Céu profundo`, e não uma só: a fase é sobre altura, e altura se lê pelo
céu ficando **mais profundo em cima** e mais pálido perto do horizonte.

O degradê de Eolo escurece **para cima** — o inverso do de Cícones. É essa inversão que faz a
mesma engine de fundo ler como altitude em vez de meio-dia ao nível do mar, e é o mesmo truque
que Cytera usa para ler como tempestade.

Compartilhadas de propósito: **Contorno** e **Bronze** (personagem), **Fogo** (Troia).

---

## 3. As ilhas flutuam pela base

Cada `Floor_*` recebe duas faixas: a de chão (grama e rocha) e, abaixo dela, a de **rocha
esfarrapada pendurada**. É essa segunda que diz que aquilo está no ar — sem ela, uma faixa de
grama sobre o azul lê como terreno comum com o fundo errado.

A **plataforma móvel** que atravessa o vão de 7,5 un ganha uma ilha pequena inteira, filha do
objeto que se move, e não uma faixa: faixa esticada num objeto em movimento denunciaria a
costura andando.

---

## 4. O vento se move — sem sistema de partículas

O projeto não tem um, e o briefing pede a implementação mais simples que funcione. A escolha
foi **faixa que ladrilha**, com um componente novo:

`Odisseia.Systems.ScrollingLayer` desliza uma faixa **no tempo**.

**Não é um segundo `ParallaxLayer`.** Os dois movem a camada, mas por motivos opostos: o
parallax responde à **câmera** e é o que cria profundidade; este responde ao **tempo** e é o
que cria movimento próprio. Uma camada pode carregar os dois, e as de vento carregam — o
componente aplica o deslocamento como delta sobre o que o parallax deixou, e é isso que permite
a convivência.

Custo: um `transform.position` por quadro e **nenhum GameObject por partícula**.

Duas camadas com velocidades diferentes (−3,5 e −6,5 un/s): uma só leria como textura colada
na tela; duas leem como ar com volume entre a câmera e o cenário.

As linhas de vento são **arcos**, não riscas retas — risca reta lê como chuva, que é a fase 04.

---

## 5. O palácio

Montado de peças modulares — degraus, coluna, entablamento, porta de bronze, braseiro, estátua,
estandarte — a **0,52 de escala**. Em tamanho nativo a coluna sozinha (7,47 un) ocupava três
quartos da altura da tela, o entablamento ficava acima do quadro e a fachada nunca aparecia
inteira. A 0,52 o conjunto fecha em 6,4 un e cabe com folga.

**O vão central fica vazio de propósito:** é onde Éolo, o Odisseu e o diálogo entram depois,
sem nada na frente. A porta de bronze marca o eixo.

**O entablamento não espelha.** Espelhar duplica o ornamento inteiro — cornija, friso e tudo —
e a viga aparece como duas peças iguais lado a lado em vez de atravessar a fachada. Espelhar só
serve para textura sem começo nem fim, como pedra e terra.

Os **estandartes** são o que mostra o vento no palácio, onde não há vegetação para inclinar.

---

## 6. Assets

**16 gerações.** Treze assets gerados, mais céu, nuvens e vento por código (cinco camadas,
custo zero).

| Grupo | Assets |
|---|---|
| Gameplay | tileset de ilha, base esfarrapada, ilha pequena, ponte de corda |
| Palace | coluna, entablamento, degraus, porta de bronze, braseiro, estátua |
| Props | cipreste, estandarte, odre dos ventos |
| Background | céu de altitude, nuvens distantes, nuvens próximas — **por código** |
| Wind | linhas de vento, folhas e poeira — **por código** |

### Reuso

O **altar de Cícones** vira a ruína sobre o `Rubble`, e o **mastro de Cytera** vira o marcador
de saída — a saída de Eolo é o embarque, e o navio já tinha sido desenhado uma vez. Duas
gerações economizadas.
