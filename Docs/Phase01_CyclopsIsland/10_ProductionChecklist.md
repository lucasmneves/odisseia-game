# Production Checklist — Ilha dos Ciclopes (Vertical Slice 01)

**Legenda de status:**

| Status | Significado |
|---|---|
| `TODO` | Não iniciado |
| `IN_PROGRESS` | Em produção |
| `READY` | Arte finalizada e aprovada nos 3 portões |
| `INTEGRATED` | Dentro do Unity, funcionando na cena |

Todos os itens estão em `TODO` — esta etapa produziu **especificação**, não os arquivos.

---

## Tabela mestra de assets

### Personagens

| Asset | Tipo | Qtd | Prioridade | Prompt necessário | Status |
|---|---|---:|---|---|---|
| Odisseu — concept sheet | Concept | 1 | **P0 — bloqueia tudo** | `09` §3.1 | TODO |
| Odisseu — sprite base | Sprite | 1 | **P0 — asset canônico** | `09` §3.2 | TODO |
| `CHR_Odysseus_Idle` | Animação | 6 fr | P0 | `09` §4 | TODO |
| `CHR_Odysseus_Run` | Animação | 8 fr | P0 | `09` §4 | TODO |
| `CHR_Odysseus_Jump` | Animação | 6 fr | P0 | `09` §4 | TODO |
| `CHR_Odysseus_Attack` | Animação | 7 fr | P0 | `09` §4 | TODO |
| `CHR_Odysseus_Damage` | Animação | 4 fr | P1 | `09` §4 | TODO |
| `CHR_Odysseus_Death` | Animação | 7 fr | P1 | `09` §4 | TODO |
| Ciclope — concept sheet | Concept | 1 | P1 | `09` §3.3 | TODO |
| `CHR_Cyclops_Idle` | Animação | 6 fr | P1 | `09` §4 | TODO |
| `CHR_Cyclops_Walk` | Animação | 8 fr | P1 | `09` §4 | TODO |
| `CHR_Cyclops_AttackPrepare` | Animação | 5 fr | **P1 — crítico p/ gameplay** | `09` §4 | TODO |
| `CHR_Cyclops_Attack` | Animação | 6 fr | P1 | `09` §4 | TODO |
| `CHR_Cyclops_Damage` | Animação | 3 fr | P2 | `09` §4 | TODO |
| `CHR_Cyclops_Death` | Animação | 9 fr | P2 | `09` §4 | TODO |
| `CHR_Cyclops_HeavyAttack` | Animação | 8 fr | **P3 — sem gameplay** | `09` §4 | TODO |
| `CHR_Cyclops_SpecialAttack` | Animação | 7 fr | **P3 — sem gameplay** | `09` §4 | TODO |
| Fera — sprite base | Sprite | 1 | P2 | `09` §3.4 | TODO |
| `CHR_Beast_Idle` | Animação | 4 fr | P2 | `09` §4 | TODO |
| `CHR_Beast_Run` | Animação | 6 fr | P2 | `09` §4 | TODO |
| `CHR_Beast_Attack` | Animação | 5 fr | P2 | `09` §4 | TODO |
| `CHR_Beast_Damage` | Animação | 3 fr | P3 | `09` §4 | TODO |
| `CHR_Beast_Death` | Animação | 4 fr | P3 | `09` §4 | TODO |

### Ambiente

| Asset | Tipo | Qtd | Prioridade | Prompt necessário | Status |
|---|---|---:|---|---|---|
| `ENV_..._Ground` | Tileset | 9 | **P0** | `09` §5 | TODO |
| `ENV_..._Platform` | Tileset | 3 | **P0** | `09` §5 | TODO |
| `ENV_..._Cliff` | Tileset | 4 | P1 | `09` §5 | TODO |
| `ENV_..._Rock` | Tileset | 5 | P1 | `09` §5 | TODO |
| `ENV_..._Cave` | Tileset | 6 | P1 | `09` §5 | TODO |
| `ENV_..._Grass` | Tileset | 4 | P2 | `09` §5 | TODO |
| `ENV_..._Sand` | Tileset | 4 | P2 | `09` §5 | TODO |
| `ENV_..._Stone` | Tileset | 4 | P2 | `09` §5 | TODO |
| `ENV_..._AncientRuins` | Tileset | 6 | P2 | `09` §5 | TODO |
| `ENV_..._Decoration` | Tileset | 8 | P3 | `09` §5 | TODO |

### Backgrounds

| Asset | Tipo | Qtd | Prioridade | Prompt necessário | Status |
|---|---|---:|---|---|---|
| `BG_CyclopsIsland_01_Background` | Background | 1 | P1 | `09` §6 | TODO |
| `BG_CyclopsIsland_01_Midground` | Background | 1 | P1 | `09` §6 | TODO |
| `BG_CyclopsIsland_01_Foreground` | Background | 1 | P2 | `09` §6 | TODO |
| `BG_CyclopsIsland_02_Background` | Background | 1 | P1 | `09` §6 | TODO |
| `BG_CyclopsIsland_02_Midground` | Background | 1 | P1 | `09` §6 | TODO |
| `BG_CyclopsIsland_02_Foreground` | Background | 1 | P2 | `09` §6 | TODO |

### Props

| Asset | Tipo | Qtd | Prioridade | Prompt necessário | Status |
|---|---|---:|---|---|---|
| `PROP_GreekColumn_Broken` | Prop | 1 | P1 | `09` §7 | TODO |
| `PROP_Rock_Large` | Prop | 1 | P1 | `09` §7 | TODO |
| `PROP_OliveTree` | Prop | 1 | P1 | `09` §7 | TODO |
| `PROP_Bush` | Prop | 1 | P2 | `09` §7 | TODO |
| `PROP_Torch` | Prop | 1 | P2 | `09` §7 | TODO |
| `PROP_GreekVase` | Prop | 1 | P3 | `09` §7 | TODO |
| `PROP_WoodenCrate` | Prop | 1 | P2 | `09` §7 | TODO |
| `PROP_StoneStructure` | Prop | 1 | P2 | `09` §7 | TODO |
| `PROP_Altar` | Prop | 1 | P3 | `09` §7 | TODO |
| `PROP_CyclopsBasket` | Prop | 1 | P2 | `09` §7 | TODO |

### VFX

| Asset | Tipo | Qtd | Prioridade | Prompt necessário | Status |
|---|---|---:|---|---|---|
| `VFX_SwordImpact` | VFX | 4 peças | **P0** | `09` §8 | TODO |
| `VFX_HitReceived` | VFX | 4 peças | **P0** | `09` §8 | TODO |
| `VFX_DustLand` | VFX | 3 peças | P1 | `09` §8 | TODO |
| `VFX_DustRun` | VFX | 3 peças | P1 | `09` §8 | TODO |
| `VFX_StoneParticles` | VFX | 5 peças | P1 | `09` §8 | TODO |
| `VFX_CyclopsSlam` | VFX | 5 fr | P1 | `09` §8 | TODO |
| `VFX_Collect` | VFX | 4 peças | P2 | `09` §8 | TODO |
| `VFX_Fire` | VFX | 4 fr | P2 | `09` §8 | TODO |
| `VFX_Smoke` | VFX | 3 peças | P3 | `09` §8 | TODO |

### Áudio

| Asset | Tipo | Qtd | Prioridade | Prompt necessário | Status |
|---|---|---:|---|---|---|
| `MUS_CyclopsIsland_Main` | Música | 1 | P1 | `09` §9 | TODO |
| `MUS_CyclopsIsland_Boss` | Música | 1 | P1 | `09` §9 | TODO |
| SFX do Player | SFX | 7 | **P0** | `05` §3 | TODO |
| SFX de inimigo | SFX | 4 | P2 | `05` §3 | TODO |
| SFX do Ciclope | SFX | 7 | P1 | `05` §3 | TODO |
| Ambiente | SFX loop | 7 | P2 | `05` §3 | TODO |

---

## Resumo quantitativo

| Categoria | Itens | Frames/peças |
|---|---|---|
| Personagens | 23 entradas | 106 frames |
| Tileset | 10 categorias | 53 tiles |
| Backgrounds | 6 camadas | 6 imagens |
| Props | 10 | 10 |
| VFX | 9 conjuntos | ~39 peças |
| Áudio | 6 entradas | 27 arquivos |
| **Total** | **64 entradas** | **~241 arquivos** |

---

## Tarefas de CÓDIGO identificadas (fora do escopo desta etapa)

Estas surgiram da análise de integração e **precisam existir antes de o slice funcionar**:

| # | Tarefa | Motivo | Prioridade |
|---|---|---|---|
| 1 | `PlayerAnimatorBridge` | O `PlayerController` não fala com nenhum Animator hoje — o placeholder não animava | **P0** |
| 2 | `CyclopsAnimatorBridge` | Ligar `BossController.AttackTelegraphed`/`AttackExecuted` aos estados | **P0** |
| 3 | `EnemyAnimatorBridge` | Idem para `EnemyController` | P1 |
| 4 | Estender `AudioLibrary` | Não tem campo para música de boss, SFX do Ciclope, nem ambiente | P1 |
| 5 | Trocar música na arena do boss | `SceneAudio` é por cena; a troca precisa ser por trigger | P1 |
| 6 | Sombra projetada em runtime | Ou é desenhada no sprite (mais barato) ou vira um `SpriteRenderer` filho | P2 |
| 7 | Corrigir `TransformationEffect` | Ele referencia `Visual/Sword`, que o novo prefab remove (`07` §4) | P2 |
| 8 | `VfxBurst` com sprites reais | Hoje usa o quadrado placeholder; precisa aceitar as peças de VFX | P1 |

---

## Ordem de execução sugerida

### Sprint 1 — Fundação (destrava todo o resto)
1. Odisseu concept sheet → **aprovar**
2. Odisseu sprite base → **aprovar nos 3 portões**
3. Tileset Ground + Platform
4. SFX do player
5. Código: `PlayerAnimatorBridge`

> **Portão de decisão:** com Odisseu e o chão dentro do Unity, dá para julgar a direção de
> arte de verdade, em movimento. Se algo estiver errado, corrija **aqui** — antes de
> produzir 200 arquivos no estilo errado.

### Sprint 2 — Odisseu completo
6. As 6 animações
7. VFX de impacto e poeira
8. Backgrounds 01 (3 camadas)

### Sprint 3 — Mundo
9. Restante do tileset
10. Props P1/P2
11. Música principal

### Sprint 4 — Boss
12. Ciclope concept → aprovar escala lado a lado com Odisseu
13. Animações P1 do Ciclope
14. Background 02, SFX do Ciclope, música de boss
15. Código: `CyclopsAnimatorBridge`

### Sprint 5 — Fechamento
16. Fera completa
17. Assets P3 restantes
18. Integração final + validação (`07` §10)

---

## Riscos registrados

| Risco | Impacto | Mitigação |
|---|---|---|
| Orçamento de textura do Ciclope estourar | Build > 15 MB | Cortar `SpecialAttack` (1,8 M px, sem gameplay). Reduzir BGs a 1600×900 |
| Assets gerados em sessões diferentes divergirem | Perda de consistência | Sempre anexar o sprite canônico de Odisseu como referência |
| Pulo não alcançar mais as plataformas após o tilemap | Fase injogável | Testar após o Sprint 1; métricas em `00` §4 |
| Gerador produzir algo parecido com franquia existente | Problema legal | Cláusula de originalidade em todo prompt + revisão humana |
| Estimativas de tamanho estarem erradas | Retrabalho | **São projeções, não medições** — meça o build real após o Sprint 2 |
