# Unity Integration Specification — Ilha dos Ciclopes

> Como os assets entram no projeto **sem quebrar o gameplay já validado**.
> Cena alvo: `Assets/Scenes/Levels/Level_04_Ciclopes.unity`.

---

## 1. Princípio de migração: substituir, não reconstruir

A fase já existe e funciona: geometria, colisões, boss telegrafado, checkpoint, diálogos,
pause, save. **Nada disso deve ser refeito.**

O art pass é uma **troca de camada visual**:

| O que muda | O que **não** muda |
|---|---|
| `SpriteRenderer.sprite` dos blocos → tiles | Posições e tamanhos dos colliders |
| Retângulos placeholder → Tilemap | `LevelManager`, `LevelIntro`, `LevelGoal` |
| Sprites do Player/inimigos | `PlayerController`, `PlayerCombat`, físicas |
| Fundos chapados → parallax de 3 camadas | `CameraFollow` e seus bounds |
| Clipes de áudio | `AudioManager`, `SceneAudio` |

**Por que isso importa:** o balanceamento das distâncias de pulo e as posições de ataque do
boss já estão ajustados ao layout atual. Reconstruir a geometria invalidaria esse ajuste.

---

## 2. Compatibilidade de escala — verificação crítica

O placeholder atual usa **PPU 8**; a arte nova usa **PPU 100**. Isso **não** quebra nada,
porque toda a geometria do jogo está em **unidades de mundo**, não em pixels.

| Objeto | Hoje | Depois |
|---|---|---|
| Bloco de chão | Sprite 8×8 com `localScale (16, 1)` | Tilemap com 16 tiles de 1 un |
| Odisseu | Retângulos com escala | Sprite 192×192 @ PPU 100, pivot na base |
| Colisor do Player | `CapsuleCollider2D (0,6 × 1,4)` | **Idêntico** |

**Ponto de atenção:** os blocos placeholder usam `localScale` para esticar um sprite de
8×8. Ao trocar por tiles reais, o `localScale` volta para `(1,1,1)` e a extensão vem da
quantidade de tiles. Isso **muda a hierarquia**, então precisa ser feito com cuidado — ver
§3.

---

## 3. Estrutura de Tilemap

Substitui os `GameObject` de chão soltos por Tilemaps organizados por função.

```
Grid  (Cell Size 1, 1, 0  — casa com tile de 1 unidade)
├── Tilemap_Background      Order -10   sem collider
├── Tilemap_Ground          Order   0   TilemapCollider2D + CompositeCollider2D  Layer: Ground
├── Tilemap_Platform        Order   0   TilemapCollider2D                        Layer: Ground
└── Tilemap_Decoration      Order   5   sem collider
```

### Configuração dos colliders

`Tilemap_Ground`:
- `TilemapCollider2D` + `CompositeCollider2D` (com `Rigidbody2D` em `Static`)
- Marcar `Used By Composite` no TilemapCollider2D
- **Motivo:** o Composite funde centenas de colliders de tile num punhado de polígonos.
  Sem isso, o `Physics2D.OverlapCircle` do `GroundCheck` teria que testar contra cada tile
  individualmente — desperdício em WebGL.

**Layer obrigatório: `Ground` (8).** O `PlayerController.groundLayer` já aponta para lá; um
tile em layer errado torna o chão intangível para a detecção de pulo.

---

## 4. Prefabs de personagem

### PF_Odysseus — atualização do `Player.prefab` existente

**Não crie um prefab novo.** Modifique o existente, para preservar todas as referências nas
16 cenas.

Hierarquia atual e o que muda:

```
Player                          ← mantém TODOS os componentes
├── Visual                      ← mantém (o flip usa o localScale dele)
│   ├── Body    [SpriteRenderer]   ← SUBSTITUIR por sprite animado
│   ├── Head    [SpriteRenderer]   ← REMOVER (agora é parte do sprite único)
│   ├── Sword   [SpriteRenderer]   ← REMOVER
│   └── AttackPoint                ← MANTER (PlayerCombat depende dele)
└── GroundCheck                 ← MANTER (posição exata importa)
```

**Passos:**
1. Em `Visual/Body`: trocar o sprite e adicionar `Animator` com `AC_Odysseus`
2. Remover `Head` e `Sword` (o sprite novo já contém tudo)
3. **Não mexer** em `AttackPoint` nem `GroundCheck` — posições calibradas
4. **Não mexer** no `CapsuleCollider2D`

**Cuidado com o `TransformationEffect` (Fase 7):** ele referencia `Visual/Body` como
`bodyRenderer` e `Visual/Sword` como `swordVisual`. Remover `Sword` deixa essa referência
nula. Como o `TransformationEffect` já trata null (`if (swordVisual != null)`), não quebra —
mas o efeito de "esconder a espada" deixa de funcionar. **Registrado como pendência** no
checklist: quando a Fase 7 receber arte, a espada precisa virar um objeto separado de novo,
ou o efeito precisa mudar para tint.

### PF_Cyclops

O Polifemo atual é montado por script na cena, com `BossController` + `HealthSystem` +
partes visuais. Vira prefab:

```
PF_Cyclops
├── Visual  [SpriteRenderer + Animator(AC_Cyclops)]
├── (sem Collider2D no corpo — intencional, ver abaixo)
└── AttackPoints (3 filhos, marcadores)
```

**Sem collider no corpo é deliberado** e já é assim na implementação: o boss fica numa
saliência acima da passagem e nunca pode bloquear fisicamente o jogador (o que travaria a
fase). O dano vem dos `attackPoints` do `BossController`.

### PF_Beast

Substitui `EnemyBasic.prefab` **apenas nesta fase** — as outras 15 fases continuam usando o
`EnemyBasic` genérico. Criar variante:

```
PF_Beast  (variante de EnemyBasic.prefab)
└── Visual  [SpriteRenderer + Animator(AC_Beast)]
```

Mantém `EnemyController`, `HealthSystem`, `DamageFeedback` e o `BoxCollider2D`.

---

## 5. Animator Controllers

### AC_Odysseus

| Estado | Clipe | Transição |
|---|---|---|
| `Idle` | `ANIM_Odysseus_Idle` | Default |
| `Run` | `ANIM_Odysseus_Run` | `Speed > 0.1` |
| `Jump` | `ANIM_Odysseus_Jump` | `IsGrounded == false` |
| `Attack` | `ANIM_Odysseus_Attack` | Trigger `Attack` |
| `Damage` | `ANIM_Odysseus_Damage` | Trigger `Damage` |
| `Death` | `ANIM_Odysseus_Death` | Trigger `Death` |

**Parâmetros:** `Speed` (float), `IsGrounded` (bool), `VelocityY` (float), `Attack`
(trigger), `Damage` (trigger), `Death` (trigger).

**Código necessário — não existe hoje.** O `PlayerController` atual não fala com nenhum
Animator (o placeholder não anima). Será preciso um `PlayerAnimatorBridge` que leia
`IsGrounded`, a velocidade do `Rigidbody2D` e os eventos do `HealthSystem`, e alimente os
parâmetros. **Isso é código, fora do escopo desta etapa de arte** — está registrado no
checklist.

### AC_Cyclops

Estados: `Idle`, `Walk`, `AttackPrepare`, `Attack`, `HeavyAttack`, `Damage`, `Death`.

O `BossController` já dispara os eventos `AttackTelegraphed` e `AttackExecuted` — a ponte
de animação pode assiná-los diretamente, sem alterar o `BossController`.

---

## 6. Parallax

O `ParallaxLayer.cs` já existe. Montagem na cena:

```
BackgroundRoot
├── BG_Layer_Sky         [SpriteRenderer Order -30]  ParallaxLayer(factor 0.10)
├── BG_Layer_Midground   [SpriteRenderer Order -20]  ParallaxLayer(factor 0.35)
└── BG_Layer_Foreground  [SpriteRenderer Order  10]  ParallaxLayer(factor 0.75)
```

**Para ladrilhar horizontalmente:** `SpriteRenderer.drawMode = Tiled` + textura com
`Wrap Mode: Repeat`, com o tamanho em X cobrindo a extensão da fase (~72 un) mais folga.

**Foreground em Order 10** — passa na frente do jogador (Order 0), que é o efeito desejado.

---

## 7. Sprite Atlases

Criar em `Assets/Odisseia/Art/Phase01_CyclopsIsland/`:

| Atlas | Conteúdo | Max Size |
|---|---|---|
| `ATL_Odysseus` | 6 sheets | 2048 |
| `ATL_Cyclops` | 8 sheets | 2048 |
| `ATL_Beast` | 5 sheets | 1024 |
| `ATL_Environment` | 53 tiles + 10 props | 1024 |
| `ATL_VFX` | ~39 peças | 1024 |

Configuração de todos:
- `Allow Rotation`: **Off** (rotação em sprite 2D causa artefato de borda)
- `Tight Packing`: **On** (economia de ~45%, ver `03` §5)
- `Padding`: 4
- Compressão: Normal + Crunch 50

**Backgrounds ficam fora de atlas** — são grandes demais e usados um por vez.

---

## 8. Ordem de renderização (Sorting Order)

| Order | Camada |
|---|---|
| -30 | BG_Layer_Sky |
| -20 | BG_Layer_Midground |
| -10 | Tilemap_Background |
| 0 | Tilemap_Ground / Platform, personagens, props |
| 5 | Tilemap_Decoration |
| 10 | BG_Layer_Foreground |
| 20 | VFX (o `VfxBurst` já usa `sortingOrder = 20`) |
| 100+ | UI (Canvas Screen Space Overlay) |

---

## 9. Ordem de integração recomendada

Integre e teste **um bloco por vez** — assim, se algo quebrar, a causa é óbvia:

1. **Tileset** → montar o Tilemap, verificar colisão e detecção de chão
2. **Backgrounds** → parallax, verificar bounds da câmera
3. **Props** → decorar, verificar colisões
4. **Odisseu** → prefab + Animator + ponte de código
5. **Fera** → variante de inimigo
6. **Ciclope** → prefab de boss + sincronia com telegrafia
7. **VFX** → substituir sprites do `VfxBurst`
8. **Áudio** → trocar referências no `AudioLibrary`

**Após cada bloco:** rodar a cena e verificar que o jogo continua jogável. Após o bloco 1,
especificamente, confirmar que **o pulo ainda alcança todas as plataformas** — é a
regressão mais provável.

---

## 10. Critérios de aceite do Vertical Slice

O slice está pronto quando:

- [ ] A fase é jogável do início ao fim sem placeholder visível
- [ ] Odisseu, fera e ciclope são distinguíveis em silhueta
- [ ] Todas as plataformas alcançáveis continuam alcançáveis
- [ ] Boss telegrafa visualmente durante os 0,9 s e a esquiva é possível
- [ ] Parallax se move em 3 velocidades distintas
- [ ] VFX disparam nos frames corretos
- [ ] Música troca ao entrar na arena do boss
- [ ] Teste de 25% em escala de cinza: gameplay legível
- [ ] Build WebGL gerado, **abaixo de 15 MB**
- [ ] Sem erro no console do navegador
