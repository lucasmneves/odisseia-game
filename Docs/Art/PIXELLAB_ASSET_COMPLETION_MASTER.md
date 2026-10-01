# PixelLab Asset Completion — master

2026-10-01 · **Estado: PLANO AGUARDANDO APROVAÇÃO. Nenhuma geração foi feita nesta etapa.**

## 1. Saldo

| | |
|---|---|
| Saldo inicial (`get_balance`) | **865 usadas / 1135 restantes** de 2000 |
| Saldo atual | igual — 0 gerações nesta etapa |
| Reinício do ciclo | **2026-10-02**. O `get_balance` diz que o total do ciclo "refills on generations_reset": o que sobrar hoje não passa para o próximo ciclo |

## 2. Método

`node Tools/art-audit.js` (novo, só leitura) → `Docs/Art/_audit/`:
- `inventario.tsv` — todos os **391 PNGs** de `Assets/`: tamanho, nº de cores, % semi-alpha, fator de ampliação
  (mesma regra do `scale-probe`), PPU e modo do `.meta`, e **quem usa** (GUID em cena/prefab, caminho em `Resources.Load`);
- `placeholders.tsv` — todo SpriteRenderer/Image de cena ou prefab com `PlaceholderSquare` ou sprite embutido do Unity,
  com caminho, ativo/ligado e cor;
- `resumo.txt` — por pasta, mais o código que desenha com o `PlaceholderSprite`.

Fontes cruzadas: `CHARACTER_ART_MASTER.md`, `FINAL_CHARACTER_ART_POLISH.md` (auditoria do elenco nas 16 cenas),
`QA/FULL_CAMPAIGN_PLAYTEST.md`, `ESTADO_ATUAL.md`, os masters de cada cenário, `Environment/Fase16/`, os scripts de
`VfxBurst`, HUD, mapa e Ending, e o plano anterior de sprint (branch `claude/stoic-wright-i8dr1o`), cujos números conferem.

**Limite:** a leitura é do YAML, sem Unity. Os vestidores salvam as cenas, então o YAML reflete o que eles desligaram;
objetos criados em runtime (efeitos, HUD) foram auditados pelo código.

## 3. Regras herdadas (valem para tudo que for gerado)

- **42,857143 px/un**, densidade nativa 1×, escala 1 no Transform, filtro Point, sem compressão, pivô na base.
- Contorno preto de cor única, alpha duro, sombreamento de forma (D-011), vista `side`.
- Paleta da fase; cor de facção (D-014: carmesim só da casa de Odisseu).
- Prompt com enquadramento positivo, sem "vazio/margem", sem nomear o que não deve aparecer (lição do machado da 16).
- Só estados que o código toca (D-019). O que exigir código fica marcado **[exige código]** e só é gerado se você aprovar.

## 4. Inventário e classificação

A definitivo · B bom · C polimento · D placeholder · E ausente

### 4.1 Personagens, inimigos, bosses
| Grupo | Arquivos | Estados | Classe | Nota |
|---|---|---|---|---|
| Odisseu master | `CHR_Odysseus` (98 quadros) | 16 estados | **A** | falta escudo no ar (QA-14) — **E [exige código]** |
| NPCs v3 (24 folhas) | `Resources/.../NPCs/` | Idle | **A** | nenhum outro estado é acionado |
| Inimigos (6 facções) | Troiano, Cícone, Lobo, Sombra, Pretendente, Soldado de treino | Idle, Run, Hit, Death | **A** | **não há ataque** — o `EnemyAnimator` só tem esses 4 estados: **E [exige código]** |
| Bosses | Polifemo, Lestrigão, Cila | Idle, Telegraph, Attack | **A** | — |
| Gado do Sol | master | estático | **A** | — |
| `CHR_Enemy_Basic` | pintado, 100 px/un | — | B | só o default do prefab; nenhuma cena desenha |
| NPCs pintados antigos | `Art/Odisseia/Characters/NPCs` | — | — | só em `Level_13_Feacios`, fora da campanha |

### 4.2 Cenário (15 fases vestidas + Fase 16)
| | Classe | Nota |
|---|---|---|
| 263 PNGs de `Art/Environments` | **A/B** | todos em densidade 1× (os 9 "ampliados" do detector são degradês e cores chapadas) |
| Fase 16 (11 assets) | **A** | auditados, sem defeito de qualidade; vestidor não executado — **não regenerar** |
| Troia: 7 peças de geometria (`Wall_Troia`, `Platform_Bridge`, `Gauntlet_1..3`, `Obstacle_Low`, mastro/vela) | **D** | N-08; muralha e barricada já existem como arte |
| Éolo: entablamento | **C** | esticado 2,0× (N-06) |
| Fogo (tochas, braseiros, fogueiras, lareira) | **C** | estático em 01, 02, 14, 15, 16 |
| Primeiro plano nas fases 04, 06, 14, 15 | **E** | as outras 11 têm |
| 24 PNGs sem uso | — | kits Wang de onde as faixas foram extraídas e figurantes antigos de Ítaca; `troy_dust_cloud_01` é reaproveitável como poeira |

### 4.3 Objetos de gameplay (aparecem em todas as fases)
| Objeto | Arte atual | Classe | Caminho |
|---|---|---|---|
| Coletável | moeda de louros, bonita, mas pintada a **100 px/un** (8 quadros) | **C** | reamostrar por código (D-050), **0 gerações** |
| Checkpoint | pilar cinza com runa "Y", genérico e não grego, **131 px/un** | **C/D** | altar grego novo |
| Pilha de flechas (`ArrowPickup`) | **quadrado** | **D** | aljava nova |
| Flecha | `odysseus_arrow`, 1× | **A** | — |
| `LevelGoal` | brilho chapado justificado; os vestidores o substituem pela arte de cada fase | B | — |

### 4.4 Efeitos
`VfxBurst` espalha quadradinhos do `PlaceholderSprite` em **11 pontos do código**: golpe (`PlayerCombat`), dano e morte
(`DamageFeedback` ×2), bloqueio (`PlayerShield`), disparo (`PlayerBow`), impacto da flecha (`Arrow`), coleta, checkpoint,
pilha de flechas, ponto de interação e alvo de treino. **D.** Poeira de salto/aterrissagem: **E** (não existe nem no código).

### 4.5 UI e mapa
| | Classe | Nota |
|---|---|---|
| HUD | **E** | só texto (QA-15): vida, coletáveis, flechas, vidas, XP; indicadores de fome e lótus também texto |
| Controles touch | B | retângulo arredondado + texto, funcional |
| Menu principal | C | ilustração pintada 1536×1024, outra linguagem |
| `LevelSelect` | C | `mapa.png` pintado 1674×940 |
| **Mapa-múndi** | **D/E** | **sem fundo nenhum**; nós, trilha e marcador do jogador são quadrados; não há emblemas (QA-16) |
| **Ending** | **D** | Odisseu, Penélope e Telêmaco são **caixas coloridas**; chão e céu chapados |

## 5. Plano de geração

Custo real: `pixen` = 1 geração por candidato; `animate_image` = ~1 por 64×64×8 quadros; v3 64 px = 1 por estado;
`pro` = 20–40 por chamada (16 candidatos ≤85 px, 64 ≤42 px).

**Receita de efeito:** quadro-chave `pixen` → `animate_image` (2 gerações por candidato). Fogo: recortar a chama que já
existe no asset e animar (1 por candidato, 0 de quadro-chave).

### P0
| ID | Fase | Asset | Motivo | Gerações | Integração futura |
|---|---|---|---|---|---|
| PXL-001 | todas | Impacto de golpe (faísca) | placeholder | 3 cand × 2 = **6** | troca no `VfxBurst` + spawn de quadros |
| PXL-002 | todas | Arco do golpe (slash) | placeholder | **6** | idem |
| PXL-003 | todas | Faísca de bloqueio no escudo | placeholder | **6** | idem |
| PXL-004 | todas | Morte (nuvem de poeira) | placeholder | **6** | idem |
| PXL-005 | todas | Poeira de salto e aterrissagem | ausente | anima o `troy_dust_cloud_01` existente: **3** | [exige código] spawn no pouso |
| PXL-006 | todas | Impacto da flecha (lascas) | placeholder | 2 × 2 = **4** | `Arrow` |
| PXL-007 | todas | Brilho de coleta | placeholder | **4** | `Collectible` |
| PXL-008 | todas | Pilha de flechas (aljava) | **quadrado em gameplay** | **3** | prefab `ArrowPickup` |
| PXL-009 | Ending | Odisseu e Penélope no reencontro (par, uma imagem) | Ending é caixas | **4** | Ending; Telêmaco adulto e fundo da 16 reaproveitados |
| PXL-010 | 02 | Ponte de tábuas | placeholder | **2** | `Platform_Bridge` |
| PXL-011 | 02 | Bloco caído (plataforma) | placeholder | **2** | `Gauntlet_1..3`, variação por espelho |
| | | | | **P0 = 46** | |

### P0 — exige código (só com sua aprovação, por causa da D-019)
| ID | Asset | Motivo | Gerações |
|---|---|---|---|
| PXL-012 | Ataque das 6 facções inimigas | o inimigo bate sem animação | 6 × 2 = **12** |
| PXL-013 | Odisseu JumpShield e FallShield | defesa no ar sem visual (QA-14) | 2 × 2 = **4** |
| | | | **16** |

### P1
| ID | Fase | Asset | Motivo | Gerações |
|---|---|---|---|---|
| PXL-014 | todas | Checkpoint grego (altar de pedra; aceso = mesmo altar + chama da PXL-015) | fora da linguagem, 3× densidade | **3** |
| PXL-015 | 01, 02, 14, 15, 16 | Fogo em loop: tocha, braseiro, fogueira, lareira, tocha de Troia, chama do altar | fogo estático | 6 × 2 = **12** (`SpriteAnimator` já existe) |
| PXL-016 | mapa | Fundo do mar Egeu | sem fundo | **3** |
| PXL-017 | mapa | Navio de Odisseu (marcador) | quadrado | **3** |
| PXL-018 | mapa | **16 emblemas** (miolo gerado; moldura de medalhão por código, igual nos 16 — é o que faz o conjunto) | ausentes | 16 × 3 = **48** |
| PXL-019 | HUD | 8 ícones: vida, flecha, vidas (elmo), XP (louro), fome (pão), lótus, cera (Sereias), escudo. A moeda reaproveita o coletável | HUD só texto | 8 × 3 = **24** |
| PXL-020 | menu | Arte-chave do menu em pixel art | ilustração pintada | **3** |
| | | | | **P1 = 96** |

### P2
| ID | Fase | Asset | Gerações |
|---|---|---|---|
| PXL-021 | 04, 06, 14, 15 | Primeiro plano, 2 peças por fase | 8 × 2 = **16** |
| PXL-022 | 06 | Entablamento de Éolo nativo | **3** |
| | | | **P2 = 19** |

### Sem PixelLab (0 gerações)
Coletável reamostrado na densidade nativa · fagulhas do `VfxBurst` desenhadas por código (o burst atual troca de sprite
sem mudar código) · mastro, vela, muralha e obstáculo de Troia por reuso (`build-ship.js`, `troy_wall_section_01`,
`troy_barricade_01`) · fundo do Ending com os assets da 16 · `LevelSelect` com o fundo novo do mapa · moldura dos
emblemas, trilha do mapa e painel do HUD por código · ícones touch reaproveitando os do HUD.

## 6. Orçamento

| Bloco | Gerações |
|---|---|
| P0 | 46 |
| P0 [exige código] | 16 |
| P1 | 96 |
| P2 | 19 |
| **Base (pixen / v3 / animate)** | **177** |
| **Nível qualidade** (`pro` com imagem de referência — Odisseu, cenário da fase — só onde os candidatos da base reprovarem): âncora dos emblemas, âncora dos ícones, quadros-chave de efeito, checkpoint, reencontro, aljava | **até ~270** |
| **Teto planejado** | **~450** |
| Reserva | ≥ 685 |

Fica abaixo da meta de 700–900 de propósito: a auditoria não achou lacuna real que justifique mais. Se você quiser
usar o período antes do reinício de amanhã, o lugar certo para gastar mais é o nível qualidade (mais chamadas `pro`
nos assets de maior visibilidade: emblemas, HUD, efeitos de combate, Ending), não mais variações.

**Regra de parada por asset:** o primeiro candidato que passar em densidade, paleta, contorno e leitura no tamanho de
jogo encerra o asset.

## 7. Organização (quando gerar)

- Escolhidos: `Assets/Art/Effects/`, `Assets/Art/Items/`, `Assets/Art/UI/HUD/`, `Assets/Art/Map/`,
  `Assets/Art/Ending/`, `Assets/Art/Environments/<Fase>/`, folhas de personagem em `Resources/Odisseia/...` — com `.meta`
  escrito, **sem referência em cena ou prefab**.
- Fontes, candidatos e descartes: `Docs/Art/PixelLab/<ID>/` (`_candidatos/`, `_descartados/`) mais o lote `.json` com o prompt.
- Nada de Unity, cena, prefab, script do jogo, commit ou PR nesta etapa.

## 8. Pendências e próximos passos

1. **Sua aprovação do plano** — e se as duas linhas [exige código] entram.
2. Gerar na ordem P0 → P1 → P2 → nível qualidade, conferindo o saldo a cada lote.
3. Documentar candidatos, escolhas, descartes e prompts aqui; Notion (🎨 Art → PixelLab Asset Completion, 🤖 AI →
   PixelLab, 🧠 Decisions) quando houver resultado a registrar.
4. A integração no Unity é uma etapa à parte: código de spawn de efeitos, HUD com ícones, mapa, Ending.

---

# EXECUÇÃO

Aprovado em 2026-10-01: plano base (177), PXL-012 e PXL-013, nível qualidade **seletivo** (`pro` só com ganho visual
real). Reserva mínima: **500**. Regra: um candidato por vez; o próximo só se o anterior for insuficiente.

Início: 2026-10-01 03:10 UTC · saldo **865 usadas / 1135 restantes**. Livro-razão de jobs: `Docs/Art/PixelLab/_ledger.tsv`
(`Tools/pxl.js`).

| Grupo | Saldo depois (usadas / restantes) | Consumo do grupo |
|---|---|---|
| 1 · Efeitos (PXL-001…007) | 880 / 1120 | 15–16 (16 jobs: 10 quadros-chave + 6 animações) |
| 2 · Aljava, reencontro, Troia (PXL-008…011) | 908 / 1092 | 27 (aljava 1, reencontro `pro` 25, ponte 1; bloco caído 0) |
| 3 · Ataques das 6 facções (PXL-012) | 914 / 1086 | 6 |
| 3 · Escudo no ar (PXL-013) | — | ~4 (2 animações; cobradas junto do grupo seguinte) |
| 4 · Checkpoint e fogo (PXL-014/015) | 922 / 1078 | 6 (altar 1, 5 chamas) |
| 4 · Mapa e emblemas (PXL-016…018) | 943* / 1057 | 24 + ~23 do `pro` da sereia (descartado), cobrado depois |
| 5 · HUD, menu, primeiro plano, Éolo (PXL-019…022) | 987 / 1013 | 19 |
| **Total da etapa** | **987 / 1013** | **122** |

\* As chamadas `pro` são cobradas com atraso: o saldo só as mostra alguns minutos depois. O total (122) vem do
`get_balance`; a divisão por grupo, do livro-razão e dos saldos intermediários. **Reserva atual: 1013 (mínimo pedido: 500).**

## Resultado por asset

Todos em densidade nativa 1×, 42,857 px/un, alpha duro, com `.meta` (Point, sem compressão, GUID derivado do caminho).
**Nenhuma cena ou prefab referencia estes arquivos** — a integração é etapa separada. Fontes, candidatos e descartes em
`Docs/Art/PixelLab/<ID>/_candidatos/`; montagem em `node Tools/build-pack.js [grupo]`; prancha em
`Docs/Art/PixelLab/_prancha_pack.png` e `PXL-018/_prancha_emblemas.png`.

### Efeitos — `Assets/Art/Effects/`
Receita: quadro-chave `pixen` → `animate_image` (que devolveu 3 quadros novos por chamada, não 6–8).

| Arquivo | Quadros | Tamanho | Escolhido | Descartado / ajuste |
|---|---|---|---|---|
| `fx_hit` | 4 | 0,61 un | `hit_a` (estrela de 8 raios) | — |
| `fx_slash` | 4 | 0,96 × 1,03 un | `slash_a` | a cauda fina fechava um círculo: cortada e espelhada por código (`Tools/fx-prep.js`) |
| `fx_block` | 4 | 0,68 un | `block_b` recolorido | `block_a` desenhou um escudo e uma lâmina dentro do efeito; `block_b` veio vermelho de fogo → rampa Bronze por código |
| `fx_death` | 6 | 1,0 un | `death_a` + 2 quadros de dissolução por dithering | a nuvem do PixelLab quase não se desfazia |
| `fx_arrow_impact` | 3 | 0,70 un | `arrowhit_c` | `_a` pintou chão e xadrez; `_b` fino demais; 4º quadro da animação virou silhueta preta |
| `fx_collect` | 4 | 0,58 un | `collect_a` | — |
| `fx_jump_dust` | 4 | 1,33 × 0,35 un | `troy_dust_cloud_01` (já existia) animado | reamostrado 0,5× por moda (2,7 un era largo demais) |

### Itens, Ending, Troia
| Arquivo | Origem | Nota |
|---|---|---|
| `Items/item_arrow_quiver` | `quiver_a` (1 candidato) | o `ithaca_arrows_bundle_01` existente não lia como item (marrom sobre marrom) |
| `Items/item_checkpoint_altar` | altar `altar_a` + chama da tocha animada | 7 quadros: 0 = apagado, 1–6 = aceso (pingue-pongue). Mesma célula nos dois estados. Vista 3/4 (o `pixen` ignora `side` em construção), coerente com a lareira da 16 |
| `Items/item_collectible_coin` | `PROP_Collectible` reamostrado por moda (100 → 42,857 px/un) | 0 gerações |
| `Ending/ending_reunion` (60 px) e `_close` (78 px) | `pro` com Odisseu e Penélope de referência, candidato 0 de 4 | os 4 eram bons; 1 e 3 com lágrimas que leem como defeito, 2 menos claro. A de 60 px está na régua do Odisseu |
| `Environments/Troy/Gameplay/troy_plank_bridge` | `bridge_a` | 7 un = 300 px, tampas + miolo espelhado |
| `Environments/Troy/Gameplay/troy_fallen_block` | **composição** de uma fiada da `troy_wall_section_01` | 0 gerações |

### Personagens (PXL-012 e PXL-013) — **gerados, download pendente**
- **Código (aprovado):** `EnemyController` ganhou o evento `Attacked` (disparado junto do dano, sem mudar dano, alcance nem
  cooldown); `EnemyAnimator` toca `Attack` só se a folha da facção tiver esse estado. `PlayerAnimator` toca
  `ShieldJump`/`ShieldFall` no ar com `IsBlocking`; sem esses estados na folha, cai em `Jump`/`Fall` como antes.
- **Ataques:** Trojan_Soldier, Cicones_Warrior, Shade_Warrior e Soldier_Ithaca (estocada de lança), Circe_Wolf (bote com
  mordida), Suitor (golpe de espada em arco) — `Tools/build-cast.js animate`, 6 frames, Receita B. Receitas em `build-cast.js`.
- **Escudo no ar:** `animate_character` no **personagem do master** (`908b7f60…`) partindo do quadro `ShieldHold_00`
  (8 quadros subindo, 4 caindo). Antes, a composição por código (escudo colado nos quadros de Jump/Fall,
  `Tools/shield-air.js`) foi tentada e **descartada**: cobria o rosto e fragmentava o escudo nos quadros encolhidos.
- **Bloqueio:** o PixelLab só serve quadros de animação de personagem por URL em `backblaze.pixellab.ai`, que a política de
  rede deste ambiente nega. As 8 animações estão prontas no servidor; baixar com `node Tools/build-cast.js fetchanim
  <nomes>` (ataques) e `get_character 908b7f60…` (escudo) quando o host for liberado — **sem gerar de novo**.

### Fogo — chama animada recomposta sobre o sprite original
`animate_image` só no recorte da chama (retângulos medidos à mão: a detecção por cor pegava suporte, carne e caldeirão);
o suporte volta do original pixel a pixel (`recompor`), então nunca se mexe. 6 quadros em pingue-pongue, **canvas igual
ao da original** (mesmo pivô — troca direta no `SpriteRenderer` + `SpriteAnimator`).

`pret_torch_stand_anim` · `itaca_ret_brazier_anim` · `pret_cooking_fire_anim` · `troy_campfire_01_anim` ·
`final_hearth_fire_anim` (camada de fogo no canvas da base da lareira da 16).

### Mapa — `Assets/Art/Map/`
| Arquivo | Nota |
|---|---|
| `map_aegean` | `map_a` (604×340; o `pixen` recusa 768×432 — erro de validação, não cobrado). Mar escuro e frio levado à rampa Água por código |
| `map_ship_marker` | `ship_a`, flâmula carmesim da casa de Odisseu |
| `Emblems/emblem_01..16` + `_locked` | moldura de medalhão **por código** (contorno, aro Bronze com pontos a cada 30°, disco Noite); símbolo gerado com o mesmo modelo de prompt para os 16 |

Emblemas: 13 aprovados no 1º candidato. Refeitos: **07** (a pedra na mão lia como ovo → rocha voando), **16** (arco,
flecha e machados amontoados → arco tenso com flecha), **10** (`_b` virou águia; `pro` com a Sereia do jogo de
referência deu retratos cortados com costuras — descartado; ficou o 1º candidato, mulher com asas).

### HUD — `Assets/Art/UI/HUD/`
`icon_health` (coração) · `icon_arrows` · `icon_lives` (perfil do herói) · `icon_xp` (louro) · `icon_hunger` (pão) ·
`icon_lotus` · `icon_wax` (cera, Sereias) · `icon_shield` · `icon_coin` (do coletável). Todos no 1º candidato.

### Menu — `Assets/Art/UI/MainMenu/`
`menu_bg_ithaca` (vista por colunata ao pôr do sol, galeras partindo, céu livre para o título) e
`menu_bg_ithaca_odysseus` (o mesmo com o **Odisseu do master** colado no parapeito). A arte pintada antiga tinha o título
dentro da imagem e um Odisseu de capa azul que não é o do jogo.

### Primeiro plano e Éolo — `Assets/Art/Environments/<Fase>/Foreground/`
04 `cytera_fg_rope_barrel`, `cytera_fg_rigging` · 06 `eolo_fg_cloud`, `eolo_fg_balustrade` · 14 `itaca_ret_fg_weeds`,
`itaca_ret_fg_branch` · 15 `pret_fg_spilled_amphora`, `pret_fg_banner_edge`. Limpezas por código: maior componente
conectado, nuvem sem os traços de esboço, cordame cortado acima da onda. Refeito: `fg04_rope` (veio com mar pintado atrás).
06 `Palace/eolo_entablature_native` — 2× os pixels da antiga, para entrar em escala 1 (N-06).

## Prompts
Modelos comuns (o asset entra no meio):
- **Sprite:** *"…, drawn small enough that [it] is complete. Isolated game asset cut out on a fully transparent background,
  only the X. Retro 16-bit pixel art, crisp pixels, flat readable shapes, no anti-aliasing."* + `view: side`, contorno
  preto de cor única.
- **Emblema:** *"a single bold emblem symbol for a level badge in a 2D adventure game set in ancient Greece: [símbolo], one
  simple centered silhouette shape, flat colors in warm gold, bronze and cream…"* (40×40).
- **Ícone de HUD:** *"a single small HUD icon … [objeto], one simple centered shape with flat colors, two or three shading
  tones…"* (32×32).
- **Fogo:** *"the flames flicker and dance in a seamless loop … everything that is not fire stays perfectly still"*.
- **Ataque (lança):** *"thrusting the spear forward hard at chest height, side view facing right, front foot stepping in…,
  spear and shield stay attached to his hands in every frame"*.
Os textos completos de cada candidato estão nos comandos registrados nesta seção e no histórico da sessão; o
`_ledger.tsv` guarda o job de cada disparo.

## Decisões desta etapa
- **Palavra vira objeto, de novo** (machado da 16, escudo do bloqueio, chão do impacto de flecha): efeito se descreve por
  forma e cor, nunca pelo que ele atinge.
- **Composição antes de geração** quando o material existe: bloco caído de Troia, altar aceso, moeda, poeira de salto.
- **`pro` só com referência**: no reencontro deu o ganho que justificou 25 gerações (mesmo Odisseu, mesma Penélope); na
  sereia, não — registrado como descarte.
- **Fogo anima só a chama**: suporte recomposto do original, canvas idêntico — troca direta, sem mexer em posição de cena.

## Pendências
1. Liberar `backblaze.pixellab.ai` e baixar PXL-012/013 (8 animações já pagas). Depois: `build-cast-sheets.js` com o
   estado `Attack` nas 6 folhas e `splice-sheet-state.js` para `ShieldJump`/`ShieldFall` na folha do Odisseu.
2. Integração (etapa separada): `VfxBurst`/spawn de efeitos com folhas, HUD com ícones, mapa e emblemas no
   `WorldMap`, Ending com o par e o Telêmaco, checkpoint e coletável nos prefabs, fogo com `SpriteAnimator`,
   Troia (ponte, blocos), primeiro plano nas 4 fases, entablamento nativo de Éolo.
3. Nada disso foi aberto no Unity, compilado ou testado nesta etapa (sem Unity no ambiente).

---

# RECUPERAÇÃO E QA DO PACK (2026-10-01)

Saldo no início e no fim: **987 usadas / 1013 restantes — 0 gerações nesta etapa.**

## Recuperação das animações pagas — bloqueada
`backblaze.pixellab.ai` segue negado pela política de rede do ambiente (o proxy de saída responde **403 no CONNECT**).
A sessão não altera o Network Access; nada foi contornado. As 8 animações foram localizadas com `get_character` (leitura,
sem custo) e registradas, com character_id, grupo e URLs dos quadros, em
`Docs/Art/PixelLab/_pendentes/animacoes_pagas.json`:

| Personagem | Estado | character_id | Grupo | Quadros |
|---|---|---|---|---|
| Trojan_Soldier | Attack | c416f3a0-7b39-4dd0-86ad-969c7dc29075 | c2a43e69-afdb-489f-88cf-2c9c4c44041b | 6 |
| Cicones_Warrior | Attack | f4640c38-d2a2-4e6f-9a1d-353a7b84a946 | c38ed570-204f-4bea-89fb-633ac639d6f5 | 6 |
| Circe_Wolf | Attack | 4f805faa-08e5-43a0-8438-f2a2c2b4e186 | eb2a3cb4-1f47-49b4-9888-8e1d39f6dc25 | 6 |
| Shade_Warrior | Attack | 3d75c50c-fcc9-4a7b-ab67-99f3396781b5 | 7f6454cc-75b1-42cf-879c-2235cd0f7035 | 6 |
| Suitor | Attack | 6aa164f4-223a-4ca6-9b62-453681c6e1b3 | f6918b1d-3814-454a-87e8-d6db82dff007 | 6 |
| Soldier_Ithaca | Attack | 7a5a09a9-5ab2-4f93-95a6-397d542ef223 | e04adba7-630a-4752-9d89-9487aeb07b7e | 6 |
| Odisseu (master) | ShieldJump | 908b7f60-0624-43bc-a0cd-89ad16416400 | 7960de31-e216-4d8b-9c82-5d807554fb81 | 8 |
| Odisseu (master) | ShieldFall | 908b7f60-0624-43bc-a0cd-89ad16416400 | 16c0e03c-def8-4c36-884c-c5053a4766ba | 4 |

**Pagas e pendentes de download.** Com o host liberado: `node Tools/fetch-paid-anims.js` (relê os URLs na hora — os
gravados têm carimbo `?t=` — e baixa nos caminhos do `build-cast`; 0 gerações). Os itens da seção 4 do briefing
(dimensões, escala, pivô, fundo e artefatos dos quadros) só podem ser conferidos depois do download.

## QA técnico — `node Tools/pack-audit.js` → `Docs/Art/_audit/pack_qa.tsv`
**73 arquivos, 0 falhas**: todos com `.meta`, 42,857143 px/un, Point, sem compressão, `alphaIsTransparency`; 0 pixel
semitransparente; nenhum canto opaco (fundo) fora de mapa e menu; nenhum xadrez pintado; densidade nativa 1×; fatias de
mesmo tamanho em toda folha; nomes em snake_case.

Avisos analisados:
| Aviso | Arquivo | Leitura |
|---|---|---|
| pixels soltos | `fx_death` (223), fogos (32–63) | dithering intencional da dissolução; fagulhas do fogo. Esperado |
| longe da paleta (48–49), 3.871 cores | `item_collectible_coin`, `icon_coin` | **a moeda é a arte pintada antiga reamostrada e ainda carrega degradê de pintura.** Correção por código possível (quantizar nas rampas Bronze/Fogo), 0 gerações — não aplicada sem aprovação |
| longe da paleta (42–44) | `icon_health`, `icon_lotus` | vermelho e rosa de significado, que a paleta de Ítaca não tem. Aceitável |

## QA visual
| Grupo | Veredito | Nota |
|---|---|---|
| Efeitos (7) | aprovados | mesma linguagem: contorno seletivo, branco/ouro/bronze, 0,6–1 un. Morte e poeira de salto em tons de terra |
| Ending | **aprovado** | montado no salão da Fase 16 com o Odisseu do jogo como régua e o Telêmaco adulto: mesma altura, mesma densidade — lê como cena final (`_qa/ending_cena.png`) |
| Mapa, navio | aprovados | Egeu na rampa Água; navio com flâmula carmesim |
| 16 emblemas + bloqueados | aprovados | um conjunto; bloqueado legível em cinza. O 10 (sereia) é o mais fraco do conjunto, mas lê como figura alada |
| HUD | 7 aprovados, 2 ressalvas | **lótus** some em 1× (conteúdo de 17×12 px); **coração** com brilho que lembra boca |
| Menu | aprovado | céu livre para o título abaixo do friso; centro e base são mar e penhasco, sem nada sob os botões; o Odisseu é o do master |
| Fogo, altar | aprovados | só a chama se mexe; altar com o mesmo pivô apagado e aceso |
| Troia, primeiro plano, Éolo | aprovados | — |
| Fase 16 | aprovada | auditada na etapa da fase; a chama da lareira agora tem loop |
