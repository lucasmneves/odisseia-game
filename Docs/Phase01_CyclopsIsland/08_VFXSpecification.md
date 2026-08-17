# VFX Specification — Ilha dos Ciclopes

> Canvas padrão **128 × 128 px**, pivot central. Mesmo tratamento de contorno e paleta dos
> sprites (`00_ArtBible.md` §2) — VFX que parece de outro jogo quebra a consistência mais
> rápido que qualquer outro asset.

---

## 1. Como os VFX funcionam neste projeto

O jogo **não usa `ParticleSystem`**. O sistema implementado é o `VfxBurst.cs`: instancia
alguns `SpriteRenderer` que voam para fora com gravidade e fade, e se autodestroem.

**Motivo (registrado na etapa de polish):** para WebGL, meia dúzia de sprites é mais barata
e previsível que instanciar sistemas de partículas a cada golpe.

**Consequência para a arte:** cada VFX é um conjunto de **sprites individuais pequenos**
(estilhaços, faíscas, tufos), não uma folha de animação de explosão. O movimento vem do
código; a arte fornece as "peças".

Parâmetros disponíveis por chamada: `count` (máx. 12), `speed`, `duration`, `size`,
`gravity`, `color`.

---

## 2. Catálogo de VFX

| # | Nome | Tipo | Peças | Quando dispara | Parâmetros sugeridos |
|---|---|---|---|---|---|
| 1 | `VFX_DustRun` | Tufo | 3 variantes | A cada passo na corrida | count 3, speed 1,5, dur 0,3, gravity 2 |
| 2 | `VFX_DustLand` | Tufo | 3 variantes | Ao pousar de um pulo | count 6, speed 3, dur 0,35, gravity 4 |
| 3 | `VFX_SwordImpact` | Estilhaço + arco | 4 peças | Frame 3 do ataque, ao acertar | count 5, speed 4, dur 0,25 |
| 4 | `VFX_HitReceived` | Faísca | 4 peças | `HealthSystem.Damaged` no jogador | count 5, speed 2,5, dur 0,3 |
| 5 | `VFX_StoneParticles` | Fragmento de pedra | 5 variantes | Impacto do Ciclope no chão | count 10, speed 5, dur 0,5, gravity 8 |
| 6 | `VFX_Fire` | Chama | 4 frames (loop) | Tocha (`PROP_Torch`) | Loop contínuo, 8 fps |
| 7 | `VFX_Smoke` | Fumaça | 3 variantes | Acompanha fogo e impactos pesados | count 4, speed 1, dur 0,8, gravity -1 |
| 8 | `VFX_CyclopsSlam` | Onda de choque | 5 frames | Golpe do Ciclope | Anel expandindo, 16 fps |
| 9 | `VFX_Collect` | Brilho | 4 peças | Ao coletar item | count 8, speed 2,5, dur 0,35 |
| 10 | `VFX_Landing_Ring` | Anel de poeira | 4 frames | Pouso de altura grande | Opcional, 20 fps |

**Partículas mágicas:** o briefing marca "caso necessárias". **Decisão: fora do slice.**
A Ilha dos Ciclopes não tem elemento mágico — Polifemo é força bruta, não feitiçaria.
Introduzir brilho mágico aqui poluiria a linguagem visual que reserva brilho para
**interativos** (`02` §1). Fica para as fases de Circe (7) e Éolo (5).

---

## 3. Paletas de VFX

VFX usa a paleta da fase, com uma exceção deliberada (branco de impacto).

| Efeito | Cores |
|---|---|
| Poeira | `#C9B99B` → `#E0C9A0` (pedra clara/areia), alfa 70% → 0% |
| Impacto de espada | `#FFFFFF` núcleo → `#F5D98A` (bronze) borda |
| Dano recebido | `#D95A3C` (perigo) → `#FFD54A` faísca |
| Partícula de pedra | `#9A8B72`, `#6B6052` (pedra média/sombra) |
| Fogo | `#FFD54A` → `#E8752F` → `#B33A1E` (do centro para fora) |
| Fumaça | `#B9C4C9` (névoa), alfa 50% → 0% |
| Onda do Ciclope | `#C9B99B` com toque de `#FFB020` (âmbar do boss) |
| Coleta | `#FFD54A` (interativo) + `#FFFFFF` |

**Branco puro só em impacto**, no frame de contato, por 1–2 frames. É o único lugar do jogo
onde branco puro é permitido — o que o torna um sinal forte de "acertou".

---

## 4. Sincronia com animação — a regra que mais importa

| VFX | Frame exato | Por quê |
|---|---|---|
| `VFX_SwordImpact` | Ataque frame **3** | É o frame de contato onde o `OverlapCircle` roda |
| `VFX_DustRun` | Corrida frames **2 e 6** | Os dois frames de contato do pé |
| `VFX_DustLand` | Pulo frame **6** | Frame de pouso |
| `VFX_StoneParticles` | Ciclope Attack frame **4** | Cajado toca o chão |
| `VFX_CyclopsSlam` | Ciclope Attack frame **4** | Mesmo frame — onda sai do ponto de impacto |
| `VFX_HitReceived` | Damage frame **1** | Imediato, junto com o flash |

**VFX atrasado em relação ao golpe é a causa nº 1 de combate que "não parece responder".**
Se houver dúvida, adiante 1 frame em vez de atrasar.

---

## 5. Especificação de arte das peças

Cada "peça" é um sprite pequeno e simples:

| Tipo | Tamanho (px) | Forma |
|---|---|---|
| Estilhaço/fragmento | 12–24 | Triângulo ou losango irregular, cor sólida + contorno |
| Faísca | 8–16 | Losango alongado, gradiente do centro |
| Tufo de poeira | 24–40 | Círculo irregular macio, sem contorno duro |
| Chama (frame) | 48 × 64 | Silhueta de chama, 3 tons |
| Anel de choque | 128 × 32 | Elipse achatada, contorno claro, interior transparente |

**Sem contorno duro em poeira e fumaça** — são as únicas exceções à regra de contorno do
Art Bible. Contorno em fumaça a faria parecer sólida.

---

## 6. Orçamento

| Conjunto | Peças/frames | Canvas | Pixels |
|---|---|---|---|
| Poeira (run/land/ring) | 10 | 128² | 0,16 M |
| Impacto (espada/dano) | 8 | 128² | 0,13 M |
| Pedra | 5 | 128² | 0,08 M |
| Fogo + fumaça | 7 | 128² | 0,11 M |
| Onda do Ciclope | 5 | 128² | 0,08 M |
| Coleta | 4 | 128² | 0,07 M |
| **Total** | **~39** | | **~0,63 M px** |

Cabe folgado em **um atlas 1024²** (`ATL_VFX`), ~0,2 MB comprimido.
