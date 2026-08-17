# Animation Specification — Ilha dos Ciclopes

> **Princípio:** poses fortes e legíveis a 8–12 fps valem mais que interpolação suave a
> 24 fps. Em sprite de 140 px, o olho lê a silhueta, não a fluidez — e o custo de textura
> cai pela metade (`03_SpriteSpecification.md` §5).

---

## 1. Sincronia com o gameplay implementado

Estes números **não são estéticos** — vêm do código já em produção. Animação que não
respeitá-los vai parecer dessincronizada do controle.

| Parâmetro do jogo | Valor | Fonte | Impacto na animação |
|---|---|---|---|
| Cooldown de ataque do jogador | **0,4 s** | `PlayerCombat.cooldown` | O ataque **precisa** caber em 0,4 s |
| Duração do pulo | ~0,82 s | Calculado de `jumpForce`/`gravityScale` | Subida ~0,41 s, queda ~0,41 s |
| Velocidade máxima | 6 un/s | `PlayerController.maxSpeed` | Cadência da corrida (ver §2) |
| Telegrafia do boss | **0,9 s** | `BossController.telegraphDuration` | A preparação precisa durar exatos 0,9 s |
| Recuperação do boss | **0,5 s** | `BossController.recoveryDuration` | Janela de contra-ataque legível |
| Intervalo entre ataques do boss | 3,5 s | `BossController.attackInterval` | Ciclo idle entre golpes |
| Flash de dano | 0,1 s | `DamageFeedback.flashDuration` | A animação de dano deve durar mais que isso |

### Cadência de corrida — cálculo

A 6 un/s com passada de ~0,7 un, o personagem dá **~8,6 passos por segundo**. Uma corrida
de 8 frames a 12 fps completa o ciclo em 0,67 s, o que dá ~3 passos/s — **lento demais**,
os pés patinariam.

**Correção:** ciclo de corrida de **8 frames a 16 fps** = 0,5 s por ciclo com 2 passos por
ciclo = 4 passos/s. Ainda abaixo do ideal teórico, mas visualmente correto — passada mais
longa e heroica compensa. **Validar em movimento e ajustar o fps se houver patinação.**

---

## 2. ODISSEU — 6 animações, 38 frames

| Animação | Frames | FPS | Duração | Loop | Notas |
|---|---|---|---|---|---|
| `Idle` | **6** | 8 | 0,75 s | Sim | Respiração sutil, manto oscilando, olhar à frente |
| `Run` | **8** | 16 | 0,50 s | Sim | Ciclo de 2 passos. Manto voando atrás |
| `Jump` | **6** | — | — | **Não** | Ver divisão abaixo |
| `Attack` | **7** | 18 | **0,39 s** | Não | Cabe no cooldown de 0,4 s |
| `Damage` | **4** | 12 | 0,33 s | Não | Recuo, mais longo que o flash de 0,1 s |
| `Death` | **7** | 10 | 0,70 s | Não | Última pose **mantida** |

### Divisão do pulo (6 frames, controlados por estado, não por tempo)

| Sub-estado | Frames | Quando |
|---|---|---|
| Impulso | 1–2 | No frame do input, ~0,1 s |
| Subida | 3 | Mantido enquanto `velocity.y > 0` |
| Ápice | 4 | Mantido quando `|velocity.y| < 1` |
| Queda | 5 | Mantido enquanto `velocity.y < 0` |
| Pouso | 6 | Ao tocar o chão, ~0,1 s |

O pulo **não roda em fps fixo** — o Animator troca de frame conforme a velocidade
vertical. Isso mantém a animação colada à física em qualquer altura de pulo.

### Detalhamento do ataque (o mais crítico)

7 frames a 18 fps, 0,39 s:

| Frame | Fase | Descrição |
|---|---|---|
| 1–2 | Antecipação | Espada recua, peso no pé de trás. ~0,11 s |
| **3** | **Golpe** | **Frame de contato — o `OverlapCircle` do `PlayerCombat` dispara aqui.** Espada estendida, borrão de movimento |
| 4–5 | Acompanhamento | Arco completo, corpo rotacionado |
| 6–7 | Retorno | Volta à guarda |

**O VFX de impacto e o som saem no frame 3** — sincronia exata (ver `08_VFXSpecification.md`).

---

## 3. FERA — 5 animações, 22 frames

Deliberadamente enxuto: inimigo simples, muitas instâncias na tela, animação barata.

| Animação | Frames | FPS | Duração | Loop | Notas |
|---|---|---|---|---|---|
| `Idle` | **4** | 6 | 0,67 s | Sim | Respiração, cabeça balançando |
| `Run` | **6** | 14 | 0,43 s | Sim | Galope quadrúpede |
| `Attack` | **5** | 14 | 0,36 s | Não | Investida com a cabeça. Contato no frame 3 |
| `Damage` | **3** | 12 | 0,25 s | Não | |
| `Death` | **4** | 10 | 0,40 s | Não | Tomba de lado, pose final mantida |

---

## 4. CICLOPE — 8 animações, 46 frames

O briefing pede 8 animações. O Ciclope é o gargalo de textura (`03` §5), então cada frame
precisa se justificar.

| Animação | Frames | FPS | Duração | Loop | Notas |
|---|---|---|---|---|---|
| `Idle` | **6** | 6 | 1,00 s | Sim | Respiração pesada, olho varrendo o ambiente |
| `Walk` | **8** | 8 | 1,00 s | Sim | Passos lentos e pesados. **Screen shake no contato do pé** |
| `AttackPrepare` | **5** | — | **0,90 s** | Não | **Trava exata com `telegraphDuration`.** Ver abaixo |
| `Attack` | **6** | 16 | 0,38 s | Não | Golpe descendente de cajado |
| `HeavyAttack` | **8** | 12 | 0,67 s | Não | Pancada de dois braços no chão, área maior |
| `SpecialAttack` | **7** | 12 | 0,58 s | Não | Arremesso de pedra — ver nota de escopo |
| `Damage` | **3** | 10 | 0,30 s | Não | Recua levemente. **Não interrompe o ataque** |
| `Death` | **9** | 8 | 1,13 s | Não | Queda longa em estágios, pose final mantida |

### AttackPrepare — os 0,9 s exatos

Esta é a animação mais importante do boss: é o que dá ao jogador a chance de esquivar.
5 frames distribuídos em 0,90 s (fps variável, com sustentação):

| Frame | Tempo | Pose |
|---|---|---|
| 1 | 0,00–0,15 s | Reage — olho localiza o alvo |
| 2 | 0,15–0,35 s | Inclina para trás, cajado começa a subir |
| 3 | 0,35–0,60 s | Cajado no alto, **olho brilha em `#FFE08A`** |
| 4 | 0,60–0,80 s | **Sustentação** — pico da tensão, quase imóvel |
| 5 | 0,80–0,90 s | Início do movimento descendente |

O frame 4 sustentado é o que torna a esquiva legível — o olho tem tempo de registrar
"vai cair ali". Sem essa pausa, 0,9 s passam rápido demais.

### Nota de escopo — `SpecialAttack`

O `BossController` implementado tem **um** padrão de ataque (pontos fixos telegrafados).
`HeavyAttack` e `SpecialAttack` são pedidos pelo briefing de arte, mas **não têm gameplay
correspondente hoje**.

**Decisão:** produzir a arte das três (o briefing pede), e registrar que `HeavyAttack` e
`SpecialAttack` ficam **prontas mas não conectadas** até que alguém implemente variação de
ataque no boss. Está marcado como tal no `10_ProductionChecklist.md`.

Alternativa se o orçamento de textura apertar: cortar `SpecialAttack` (7 frames × 512² =
1,8 M pixels) — é a economia mais fácil, já que não há gameplay usando.

---

## 5. Resumo e naming das animações

| Personagem | Animações | Frames | Sheets |
|---|---|---|---|
| Odisseu | 6 | 38 | 6 |
| Fera | 5 | 22 | 5 |
| Ciclope | 8 | 46 | 8 |
| **Total** | **19** | **106** | **19** |

Nomes conforme `06_NamingConvention.md`:

```
CHR_Odysseus_Idle.png       CHR_Cyclops_Idle.png
CHR_Odysseus_Run.png        CHR_Cyclops_Walk.png
CHR_Odysseus_Jump.png       CHR_Cyclops_AttackPrepare.png
CHR_Odysseus_Attack.png     CHR_Cyclops_Attack.png
CHR_Odysseus_Damage.png     CHR_Cyclops_HeavyAttack.png
CHR_Odysseus_Death.png      CHR_Cyclops_SpecialAttack.png
                            CHR_Cyclops_Damage.png
CHR_Beast_Idle.png          CHR_Cyclops_Death.png
CHR_Beast_Run.png
CHR_Beast_Attack.png
CHR_Beast_Damage.png
CHR_Beast_Death.png
```

---

## 6. Regras de animação (todas obrigatórias)

1. **Pivot travado.** Idêntico em 100% dos frames. Pivot flutuante = personagem tremendo.
2. **Volume constante.** Squash & stretch é permitido, mas a massa aparente não muda.
3. **Contato no frame de dano.** O frame onde a arma acerta é o frame onde o código aplica
   dano. Divergência aqui é a causa nº 1 de combate que "não parece responder".
4. **Poses finais mantidas.** Morte termina no chão e fica — nada de sumir no último frame.
5. **Sem espelhamento.** Tudo voltado à direita (`03` §3).
6. **Legibilidade a 25%.** Cada frame passa no portão do Art Bible §6.
7. **Silhueta em movimento.** Rode a animação inteira em preto sólido — a ação ainda tem
   que ser identificável.
