# PixelLab Visual Polish Pass 02 — High Impact Game Feel

2026-10-01 · **Estado: CONCLUÍDO — 12 grupos, 50 assets, 166 gerações (987 → 1153; restam 847). Sem integração no Unity; sem commit.**

## Saldo
Início: **987 usadas / 1013 restantes** (`get_balance`). `backblaze.pixellab.ai` segue bloqueado: as 8 animações de personagem
pagas na rodada anterior continuam no servidor (ver `PIXELLAB_ASSET_COMPLETION_MASTER.md`).

## Auditoria — o que já existe e entra como base (0 gerações)

| Área | Já existe | Situação |
|---|---|---|
| Combate v1 | `fx_hit`, `fx_slash`, `fx_block`, `fx_death`, `fx_arrow_impact`, `fx_collect`, `fx_jump_dust` | bons; faltam lança, golpe pesado, ricochete, faíscas metálicas soltas, poeira de corrida/pouso |
| Inimigos | 6 facções com Idle/Run/Hit/Death; **Attack das 6 já pago** (download pendente) | completo quando baixar. Pedidos do briefing que contrariam os masters (Cícone com machado) não entram: a arma é a do master |
| Bosses | Polifemo, Lestrigão, Cila: Idle/Telegraph/Attack | completo |
| Fogo | 5 loops (tocha, braseiro, fogueira de assar, fogueira de Troia, lareira) + altar | **estáticos ainda:** `ciclopes_giant_fire`, `ciclopes_torch_wall`, `eolo_brazier_native`, `ithaca_torch_01`, `mortos_brazier_tall` |
| Água | ondas/espuma/superfície por fase (Citera, Sereias, Calipso, Gado, Cila, Ciclopes), cascata de Calipso | **todas estáticas** (rolagem por código); não há splash nem impacto na água |
| Citera | céu de tempestade, nuvem, relâmpago, chuva, spray, ondas média/grande, anéis de espuma | camadas prontas; **relâmpago, spray e onda grande não animam** |
| Circe | névoa, raios de luz, partículas (pontos), flores luminosas | **não há magia de transformação, círculo, fumaça mágica** |
| Mundo dos Mortos | luzes de almas (pontos), névoa, braseiro alto | **não há alma errante animada nem fogo espectral** |
| Cila/Caríbdis | mar agitado, funil, `FX_Charybdis` girado por código | redemoinho resolvido; **falta o splash do golpe da Cila** |
| Ending | par do reencontro, Telêmaco adulto, salão e amanhecer da 16 | **falta o cenário próprio** e a atmosfera (partículas douradas) |
| Mapa | Egeu, navio, 16 emblemas + bloqueados | **faltam estados CURRENT/COMPLETED e anel de seleção** |
| HUD | 9 ícones | faltam moldura/barra, objetivo, pausa; lótus pequena; moeda com degradê |
| Transições | — | **ausentes** (só fade) |

## Plano

Custo: `pixen` 1 por candidato · `animate_image` ≈1 por 64×64×8 · `pro` 20–30 por chamada (64 candidatos a ≤42 px,
16 a ≤85 px). **O `pro` entra onde ele rende muito: partículas pequenas** (uma chamada dá dezenas de variações para o
sorteio de partículas) e os dois assets de maior vitrine (círculo mágico, cenário do Ending).

### P0 — Game feel
| ID | Asset | Base existente | Método | Gerações |
|---|---|---|---|---|
| PP-01 | Estocada de lança (rastro reto + ponta) | — | pixen + anim, 2 cand | 4 |
| PP-02 | Golpe pesado (arco largo, 2º golpe do combo) | `fx_slash` | pixen + anim, 2 cand | 4 |
| PP-03 | Ricochete de flecha (faísca + lasca) | `fx_arrow_impact` | pixen + anim | 2 |
| PP-04 | Partículas de combate: faíscas metálicas, lascas, gota estilizada mínima | — | **pro 32 px** ×3 famílias | ~75 |
| PP-05 | Poeira de corrida, de pouso e de queda forte | `troy_dust_cloud_01` | anim da base, 3 | 3–6 |
| PP-06 | Partículas de movimento/ambiente: poeira, folha, areia/detrito, gota/espuma | — | **pro 32 px** ×4 famílias | ~100 |
| PP-07 | Fogo: animar os 5 estáticos + chama pequena (vela/lamparina) | 5 sprites existentes | anim do recorte (receita da rodada 1) + pixen | ~8 |
| PP-08 | Água: splash pequeno, impacto grande na água, espuma animada (3 fases), onda grande quebrando (Citera), cascata | ondas e espumas existentes | pixen + anim / anim da base | ~16 |
| | | | **P0** | **~215** |

### P1 — Momentos importantes
| ID | Asset | Base | Método | Gerações |
|---|---|---|---|---|
| PP-09 | Citera: relâmpago animado (clarão + 2 raios), spray animado | `cytera_lightning`, `cytera_spray` | anim da base + 1 raio novo | ~4 |
| PP-10 | Circe: **círculo mágico grego** (meandro + símbolos de Hécate) | — | **pro 84 px**, 16 cand | ~25 |
| PP-11 | Circe: brilho de transformação (verde-dourado), fumaça mágica | — | pixen + anim, 2 cand cada | ~8 |
| PP-12 | Mortos: fogo espectral (braseiro existente recolorido por código + anim), alma errante animada | `mortos_brazier_tall` | anim + pixen/anim | ~6 |
| PP-13 | Cila: splash do golpe no convés/mar | — | pixen + anim, 2 cand | 4 |
| PP-14 | Ending: **cenário do quarto com a cama de oliveira** (o sinal do reconhecimento na Odisseia) | par, Telêmaco, salão 16 | pixen 604×340, até 3 cand | 3 |
| PP-15 | Ending: partículas douradas/folhas ao vento | — | (sai das famílias PP-06) | 0 |
| | | | **P1** | **~50** |

### P2 — Apresentação
| ID | Asset | Método | Gerações |
|---|---|---|---|
| PP-16 | Mapa: anel de seleção animado (CURRENT), coroa de louros (COMPLETED); AVAILABLE/LOCKED por código | pixen + anim | ~5 |
| PP-17 | Mapa: ondinhas animadas sobre o mar, trilha pontilhada por código | anim + código | ~2 |
| PP-18 | HUD: moldura de meandro e barra de vida/energia | pixen, 2 cand | ~4 |
| PP-19 | HUD: objetivo (pergaminho), pausa; refazer lótus maior. Moeda quantizada e coração por código | pixen | ~4 |
| PP-20 | Transições: faixa de meandro que varre a tela, explosão de louros (vitória), vinheta de morte por código, emblema da fase no centro (já existe) | pixen + anim | ~6 |
| | | **P2** | **~21** |

### Total
| | Gerações |
|---|---|
| P0 | ~215 |
| P1 | ~50 |
| P2 | ~21 |
| **Estimativa** | **~285** |
| Margem para candidatos extras onde o 1º reprovar (~15%) | ~45 |
| **Teto planejado** | **~330** |
| Reserva ao fim | **≥ 680** |

Fica abaixo dos 400–600 da meta de propósito: a auditoria mostrou que a maior parte do cenário e dos personagens já está
pronta. **Onde vale gastar mais, se você quiser:** mais chamadas `pro` nas partículas (são as que aparecem o tempo todo) e
no círculo de Circe e no cenário do Ending.

### Não entra (decisão)
- **Regenerar inimigos ou trocar armas** (Cícone com machado, Pretendente com arco): contraria os masters aprovados; os
  ataques das 6 facções já estão pagos.
- **Background inteiro de Citera**: as camadas já existem; só o FX anima.
- **Emblemas**: aprovados; os estados do mapa saem por código + 2 peças novas.

## Organização (quando gerar)
Escolhidos em `Assets/Art/Effects/{Combat,Particles,Water,Fire,Magic,Underworld,Transitions}/`, `Assets/Art/Map/`,
`Assets/Art/UI/HUD/`, `Assets/Art/Ending/`; candidatos e descartes em `Docs/Art/PixelLab/PP-XX/`. Mesmo executor
(`Tools/pxl.js`, livro-razão) e empacotador (`Tools/build-pack.js`).

---

# EXECUÇÃO

Aprovado: plano base (~330) com autorização para ir a ~500–600 onde houver ganho real; reserva ≥ ~400. Início: **987 / 1013**.

## Grupo 1 — Game feel / combate · 94 gerações (987 → 1081; restam 919)

**Achado de custo:** neste plano o `create_image_pro` devolve **4 candidatos por chamada** (não 64/16) a **~22 gerações**.
Ele só compensa quando o `pixen` falha ou gera formas genéricas — e foi o caso aqui.

| Arquivo (`Assets/Art/Effects/Combat/`) | Quadros | Origem | Por que este |
|---|---|---|---|
| `fx_hit_v2` | 4 | `pro` 32 px, candidato 1 de 4 + `animate_image` | cores chapadas e contorno nítido; o `fx_hit` v1 tinha degradê suave que destoava |
| `fx_slash_v2` | 4 | `pro` 40 px, candidato 3 de 4 (fragmento solto cortado) + anim | rastros de velocidade; mais vivo que o v1 |
| `fx_thrust` | 4 | `pro` 64×32, candidato 0 + anim | estocada de lança: rastro reto branco com linhas douradas |
| `fx_heavy_slash` | 4 | `pro` 64 px, candidato 0; quadros 0–1 da anim + 2 de dissolução | os quadros 2–3 da anim fechavam um anel completo |
| `fx_ricochet` | 3 | **composição**: clarão `hit_pro_00` + rastro em zigue-zague por código | 0 gerações |

Mantidos da rodada 1 (bons): `fx_block`, `fx_death`, `fx_arrow_impact`, `fx_collect`, `fx_jump_dust`. Os v1 de golpe e arco
continuam no projeto até a integração escolher.

**Descartados:** `thrust_a` (lia como flecha), `thrust_b` (desenhou a lança — duplicaria a arma do inimigo), `heavy_a`
(foice e gravetos), `heavy_b` (anéis de fumaça confusos), `ricochet_a` (virou chama), `hit_pro_02/03`, `slash_pro_00..02`,
`thrust_pro_01..03`, `heavy_pro_01..03` (candidatos piores do mesmo conjunto).

Prompts: os do `pro` em `_ledger.tsv` (job) e nos comandos desta seção — modelo *"a single [efeito] for a 2D side-scrolling
action game set in ancient Greece: [forma e cor], nothing else, clean crisp shapes … Retro 16-bit pixel art, crisp hard
pixels, no anti-aliasing, no glow blur"*.

## Grupo 2 — Partículas · 35 gerações (1081 → 1116; restam 884)

Método: cada família pediu **uma fileira de 4–6 partículas separadas** num canvas pequeno; `pecas()` (em `build-pack.js`)
recorta por componente conectado e `celulas()` centraliza tudo numa célula comum — 1 geração rende várias variações para o
código sortear. Movimento (rotação, velocidade, gravidade, fade) fica para o código; só pouso e queda animam no PixelLab.

| Arquivo (`Assets/Art/Effects/Particles/`) | Variações | Origem | Custo |
|---|---|---|---|
| `Combat/ptc_spark` | 10 | 6 manchas quentes `sparks_a` (pixen) + 4 lascas de faísca desenhadas por código | 1 |
| `Debris/ptc_chip` | 6 | `chips_a` (pixen): madeira, pedra, bronze | 1 |
| `Debris/ptc_debris_tint` | 10 | as lascas em cinza claro + 4 cacos por código — **tingíveis** | 0 |
| `Dust/ptc_dust_puff` | 4 | `rundust_b` (pixen) — corrida, frenagem, mudança de direção | 2 |
| `Dust/fx_land_dust` | 5 quadros | `pro` candidato 0 + anim + dissolução — pouso | ~23 |
| `Dust/fx_fall_dust` | 5 quadros | `pro` candidato 2 (rachadura no chão) + anim + dissolução — queda pesada, morte | 1 |
| `Dust/ptc_skid_dust` | 1 | `pro` candidato 3 (rasteira) — derrapagem | 0 |
| `Water/ptc_drop` | 5 | `drops_a` (pixen): gota, gotinha, spray | 1 |
| `Water/ptc_foam` | 18 | `foam_b` (pixen) | 2 |
| `Nature/ptc_leaf` | 5 | `leaves_a` (pixen): oliveira prateada e verde, folha seca | 1 |
| `Environment/ptc_sand` | 6 | grãos de 1–3 px por código nas cores Terra/caminho | 0 |

**`pro`: 1 chamada (~22)**, que rendeu 3 assets (pouso, queda, derrapagem) — os 4 candidatos eram bons.

**Descartados:** `sparks_b` (cometas de fogo — lia como chama), `rundust_a` (pedras contornadas), `land_a` (base cortada
reta), `land_b` (xadrez pintado, formas pobres), `fall_a` (árvore/brócolis), `fall_b` (ouriço cinza), `foam_a` (seis
"peixinhos" iguais), `land_pro_01` (redundante).

**Compartilhamento:** splash e espuma de Caríbdis/Citera saem de `ptc_drop` + `ptc_foam`; debris de qualquer material sai de
`ptc_debris_tint` com cor; impacto de lança/espada/escudo/ricochete compartilham `ptc_spark`; a poeira de queda serve
morte e objeto caindo.

## Grupo 3 — Água · 8 gerações (1116 → 1124; restam 876)

Auditoria: as faixas de espuma (512–1022 px), superfícies e a cascata de Calipso **ladrilham** — animar um recorte quebraria
o ladrilho; o movimento delas é rolagem/oscilação por código. Spray de Citera e anéis de espuma: o código espalha
`ptc_drop`/`ptc_foam` e escala os anéis existentes. Nada disso foi regenerado.

| Arquivo | Quadros | Origem | Custo |
|---|---|---|---|
| `Effects/Water/fx_splash_large` | 5 | `splash_l_b` (pixen, coroa aberta teal/branco) + anim + dissolução — queda no mar, golpe da Cila na água | 2 |
| `Effects/Water/fx_splash_small` | 5 | o grande reamostrado 50% por moda — pé na água rasa, objeto caindo | 0 |
| `Effects/Water/fx_ripple` | 4 | elipses na rampa Água, por código | 0 |
| `Environments/Cytera/Ocean/cytera_wave_large_anim` | 6 (pingue-pongue) | recorte da crista (176×112) animado e recomposto sobre a onda original; mesmo canvas | ~3 |

**Descartados:** `splash_s_a` (jato de chafariz), `splash_l_a` (gêiser em cogumelo, azul saturado demais), `ripple_a`
(contorno preto — prato), `splash_s_b` (espinhos escuros, lia como broto). `pro`: não usado.

## Grupo 4 — Tempestade de Citera · 6 gerações (1124 → 1130; restam 870)

Auditoria: céu, nuvem, chuva, spray, ondas e a crista animada (G3) já dão a camada atmosférica — nada regenerado. O raio é
**hazard de gameplay** (`StormHazard`: rest → warn → strike) e precisava de leitura nas três fases. O raio da Fase 04
(ramificado, branco + rastro cinza) é melhor que qualquer candidato novo; as variações saem dele por código. O clarão de
céu inteiro é tinta de câmera/`SpriteRenderer` na integração (sem asset: alpha duro não faz véu).

| Arquivo | Quadros | Origem | Custo |
|---|---|---|---|
| `Effects/Storm/fx_storm_warn_crackle` | 4 (loop) | `warn_b` (pixen, faixa de arcos) sem a linha escura da base + anim — fase **warn**, no chão sob o raio | 2 |
| `Effects/Storm/fx_storm_strike` | 4 | só os raios claros de `strike_b` (fumaça e brasas removidas por cor): núcleo → explosão → 2 dissoluções — impacto no chão | 1 |
| `Environments/Cytera/Weather/cytera_lightning_strike` | 4 | raio da Fase 04: cheio → núcleo branco → rastro cinza → dissolvido; mesmo canvas/pivô (troca direta) — fase **strike** | 0 |
| `Environments/Cytera/Weather/cytera_lightning_b` | 1 | raio espelhado (dois raios seguidos não ficam iguais) | 0 |
| `Environments/Cytera/Background/cytera_storm_cloud_lit` | 1 | a nuvem clareada por baixo (contorno mantido) — troca por 1–2 quadros no strike | 0 |

**Descartados:** `bolts_a` (zigue-zague uniforme de desenho animado; o raio existente é melhor), `warn_a` (desenhou uma
tábua de convés), `strike_a` (a fumaça virou pedras). `pro`: não usado.

**Para revisão:** `fx_storm_strike` quadros 2–3 (dissolução em xadrez no centro — ok em movimento, conferir em jogo);
`cytera_storm_cloud_lit` (ficou acinzentada clara; se faltar força, tingir por código na integração).
Prancha: `Docs/Art/PixelLab/_g4_tempestade.png`.

## Grupo 5 — Circe · 7 gerações (1130 → 1137; restam 863)

Auditoria: névoa, feixes de luz, partículas, flores luminosas, templo e ruínas já existem — nada regenerado. O que faltava é
o que a mecânica pede: `TransformationZone` (área que transforma) não tem marca visual e `TransformationEffect` só tinge
o corpo; a erva de moly cura sem efeito. O `pro` previsto (~25) não foi preciso: o 2º pixen do círculo acertou.

| Arquivo (`Assets/Art/Effects/Magic/`) | Quadros | Origem | Custo |
|---|---|---|---|
| `fx_circe_circle` | 4 (pulso) | `circle_b` (pixen): meandro grego, anel de luas, **roda de Hécate** (espiral tripla); fundo escuro removido; pulso por código — cutscene, parede do templo, Circe conjurando | 1 |
| `fx_circe_circle_ground` | 4 (loop) | desenhado por código na mesma paleta: elipse com meandro, luas girando, roda de Hécate, fagulhas subindo — marca a `TransformationZone` no chão | 0 |
| `fx_circe_transform` | 6 | `transform_a` (pixen, puff menta/lilás com estrelas douradas) + anim + 2 dissoluções — Odisseu vira porco | 2 |
| `fx_circe_cure` | 6 | a transformação **ao contrário** com o lilás trocado por dourado — cura pela moly | 0 |
| `fx_circe_smoke` | 4 (loop) | `smoke_a` (pixen, fitas menta/lilás) + anim, xadrez cinza pintado entre as fitas removido por cor — caldeirão/mesa de alquimia, porta do templo | 2 |

**Descartados:** `circle_a` (pentagrama — anacrônico, não é grego; as luas douradas eram boas), `ground_a` (elipse ruidosa
com preenchimento escuro; a projeção do círculo achatado também borrava o meandro — por isso o desenho por código).
`pro`: não usado (economia de ~25 sobre o plano).

**Para revisão:** `fx_circe_transform` tem centro escuro (vazio do puff) e 348 px soltos (as estrelinhas — intencional);
`fx_circe_circle` quadro 2 clareia bastante (ver se o pulso não pisca demais em jogo).
Prancha: `Docs/Art/PixelLab/_g5_circe.png`.

## Grupo 6 — Mundo dos Mortos · 4 gerações (1137 → 1141; restam 859)

Auditoria: caverna, paredes, estalactites, rio, ruínas, portão, altar, névoa e luzes de almas (pontos) já existem — nada
regenerado. Faltavam o fogo espectral (o braseiro alto queimava laranja, como em Ítaca) e uma alma com corpo — as
sombras do Hades são o centro do canto XI.

| Arquivo (`Assets/Art/Environments/MundoDosMortos/`) | Quadros | Origem | Custo |
|---|---|---|---|
| `Props/mortos_brazier_tall_spectral_anim` | 6 (pingue-pongue) | o braseiro existente: recorte da chama (84×64) animado, recomposto (receita do fogo da rodada 1) e **tudo que era quente recolorido para uma rampa ciano-gélida**; o vaso vira bronze azinhavrado. Mesmo canvas/pivô do original — troca direta. **Substitui a animação quente prevista para este braseiro no Grupo 8** | 1 |
| `Souls/mortos_soul_wander` | 6 (pingue-pongue) | `soul_a` (pixen): sombra encapuzada com cauda de fumaça + anim — 1,49 un, altura do Odisseu | 2 |
| `Souls/mortos_soul_wander_small` | 6 | a mesma a 50% (moda) — almas do fundo | 0 |
| `Souls/mortos_soul_vanish` | 4 | dissolução da alma — atravessa a parede / some quando o jogador chega | 0 |

**Descartados:** `soul_b` (fantasminha redondo de desenho animado com carinha triste — fora do tom).
**Para revisão:** o vaso azinhavrado do braseiro espectral (alternativa: só a taça e a chama frias, vaso marrom — 1 linha
no `build-pack`); a alma pequena a 50% perde as mãos (lê como silhueta, o que no fundo é o desejado).
Prancha: `Docs/Art/PixelLab/_g6_mortos.png`.

## Grupo 7 — Cila e Caríbdis · 2 gerações (1141 → 1143; restam 857)

Auditoria: falésia e caverna, paredes colunares, mar revolto (`cila_sea_churn`), destroços, funil e `FX_Charybdis` (girado
por código) já existem — o redemoinho está resolvido. **Achado de gameplay:** as duas `TidalHazard` da Fase 11 (colisores
3 × 1 un que sobem e descem e matam ao toque) têm o `SpriteRenderer` placeholder **desligado pelo
`CilaCaribdisSceneDresser`** e nenhuma arte as segue — em jogo, o jogador morre para uma caixa invisível. Esse virou o item
principal do grupo. Na Cila, `BossController` dispara `AttackTelegraphed` (0,7 s) e `AttackExecuted` em cada `attackPoint`.

| Arquivo | Quadros | Origem | Custo |
|---|---|---|---|
| `Environments/CilaCaribdis/Ocean/cila_tidal_surge_anim` | 4 (loop) | `surge_a` (pixen): onda com crista de espuma e pontas afinando, nas cores e nos riscos brancos do `cila_sea_churn` + anim. 2,99 un = largura do colisor; pivô na base — **filho da `TidalHazard` na integração** | 2 |
| `Effects/Boss/fx_scylla_strike` | 5 | composição: `fx_splash_large` (G3) + `fx_fall_dust` (G2), recoloridos por luminância para o verde-garrafa/espuma de Cila e para basalto — a cabeça bate no convés molhado | 0 |
| `Effects/Boss/fx_scylla_telegraph` | 4 | por código: a sombra da cabeça cresce no chão (elipse escura com miolo pontilhado) + gotas caindo — no `attackPoint` durante os 0,7 s | 0 |

**Descartados:** recorte da crista do `cila_sea_churn` com máscara arredondada (as bordas cortadas pareciam um bloco; o pixen
venceu de primeira). `pro`: não usado.
**Para revisão:** a onda curva para a direita — na segunda maré, `flipX` evita duas iguais; a sombra do aviso é pontilhada
(ver se lê sobre o convés de madeira).
Prancha: `Docs/Art/PixelLab/_g7_cila.png`.

## Grupo 8 — Fogo · 6 gerações (1143 → 1149; restam 851)

Auditoria: tocha, braseiro, fogueira de assar, fogueira de Troia e lareira já animam (rodada 1, PXL-015). Ficaram parados
quatro fogos — o quinto da lista, o braseiro dos Mortos, virou espectral no Grupo 6. Mesma receita: recorte da chama →
`animate_image` → `recompor` só nos pixels de chama, pingue-pongue de 6 quadros, **mesmo canvas e pivô do original**
(troca direta: `<nome>_anim.png` ao lado do estático). Retângulos em `Docs/Art/PixelLab/PP-07/_ref/recortes.json`.

| Arquivo | Recorte da chama | Custo |
|---|---|---|
| `Environments/Ciclopes/Special/ciclopes_giant_fire_anim` | 100×146 — a anim redesenhou toras e pedras laranja (contam como "quentes"); duas proteções: pixel opaco não-chama do original nunca muda + bloco das pedras da direita intacto | 1 |
| `Environments/Ciclopes/Props/ciclopes_torch_wall_anim` | 32×36 | 1 |
| `Environments/Eolo/Palace/eolo_brazier_native_anim` | 60×48 — as línguas marrom-escuras da chama não são "quentes" e ficam paradas; anima o miolo laranja (lê como brasa viva) | 1 |
| `Environments/Ithaca/Props/ithaca_torch_01_anim` | 20×22 | 1 |
| `Environments/Shared/Props/shared_oil_lamp_anim` | **novo**: lamparina grega de terracota (lychnos, `lamp_a` pixen) + anim só na faixa da chama — Ítaca, quarto do Ending, templo de Circe | 2 |

**Descartados:** nenhum candidato; a 1ª montagem da fogueira gigante (pedra da direita sumia no quadro 3) foi corrigida
pelas proteções acima. **Para revisão:** braseiro de Éolo (se o marrom parado incomodar, recolorir as línguas para a rampa
Fogo e reanimar — ~1 geração). Prancha: `Docs/Art/PixelLab/_g8_fogo.png`.

## Grupo 9 — Mapa · 2 gerações (1149 → 1151; restam 849)

Auditoria: `map_aegean` (604×340), navio, 16 emblemas + 16 bloqueados já existem — nada regenerado. Os medalhões são
desenhados por código (círculo de raio 27,5 em 56 px), então o anel de seleção também é por código, no círculo exato.
**AVAILABLE** = o emblema normal e **LOCKED** = `emblem_NN_locked` (já existem); faltavam CURRENT e COMPLETED.

| Arquivo (`Assets/Art/Map/`) | Quadros | Origem | Custo |
|---|---|---|---|
| `States/map_state_completed_wreath` | 1 | `laurel_a` (pixen): louro verde-oliva com fita vermelha, 80 px — **vai ATRÁS do medalhão** (as folhas aparecem em volta do aro) | 1 |
| `States/map_state_current_ring` | 4 (loop) | código: aro dourado contornado fora do raio do medalhão, 4 contas claras girando (loop de 90°) + aro externo pulsando | 0 |
| `map_wave_glint` | 4 | código: "~" na rampa Água que nasce, abre e some — o código espalha vários com fases diferentes | 0 |
| `map_trail_dot` | 2 | código: ponto navegado (vinho, contornado) e por navegar (areia clara) | 0 |

**Descartados:** `laurel_b` (louro dourado — bonito, mas se confunde com o aro de bronze; fica em `_candidatos`).
**Para revisão:** o anel gira em 4 quadros (passo de 22,5°) — a 8 FPS dá meia volta por segundo; o navio (`map_ship_marker`)
balança por código. Prancha (prova sobre o mapa real, 2 quadros): `Docs/Art/PixelLab/_g9_mapa.png`.

## Grupo 10 — Transições · 0 gerações (restam 849)

Auditoria: `SceneLoader` → `ScreenFader` (fade) → `LoadingScreen`; morte = painel de texto (`DeathOverlay`); vitória =
`LevelCompleteMenu`; o emblema da fase para o centro da tela de carregamento já existe (G-anterior). Tudo deste grupo saiu
por **código e composição** — o meandro precisa de geometria exata e ladrilhável, e a coroa dourada já estava paga.

| Arquivo (`Assets/Art/UI/Transitions/`) | Quadros | Origem | Custo |
|---|---|---|---|
| `ui_meander_band_tile` | 1 (ladrilho 12×18) | código: chave grega clássica em grade de 2 px, contínua entre ladrilhos, bronze sobre Noite com filetes — duas faixas que se fecham sobre a tela na troca de cena; serve também de moldura de painel | 0 |
| `fx_victory_laurel` | 5 | composição: `laurel_b` (G9) crescendo 50 → 75 → 100% + 8 faíscas `ptc_spark` (G2) em leque que se dissolvem; o último quadro segura a coroa | 0 |
| `ui_death_vignette_9slice` | 1 (48×48, borda 16) | código: borda em pontilhado ordenado vinho-escuro, densa na beira e rala para dentro; `spriteBorder` 16 no .meta para `Image` sliced | 0 |

**Não feito (decisão):** clarão/escurecimento de tela inteira — é cor de `Image`/`ScreenFader`, não sprite.
**Para revisão:** as faíscas da vitória são pequenas diante da coroa de 2,6 un (dá para dobrar a escala na integração).
Prancha (simulação sobre o fundo do menu): `Docs/Art/PixelLab/_g10_transicoes.png`.

## Grupo 11 — Ending · 2 gerações (1151 → 1153; restam 847)

Auditoria: `Ending.unity` + `EndingController` (Jogar novamente / Menu) e o casal do reencontro (`ending_reunion` 38×60,
`ending_reunion_close` 49×78) já existem — faltava **o lugar**. Escolhido o sinal do reconhecimento na Odisseia (XXIII):
o quarto com a cama que Odisseu construiu em volta do tronco vivo de uma oliveira.

| Arquivo | Origem | Custo |
|---|---|---|
| `Assets/Art/Ending/ending_bedroom_olive` (604×340, mesmo formato do mapa/menu) | `bedroom_b` (pixen): cama em volta da oliveira ao centro, galhos pelo teto, duas janelas para o mar ao amanhecer, friso de meandro, **teares** (a teia de Penélope), chão livre à esquerda para o casal. Correção: a lamparina de vidro com chaminé (anacrônica) foi apagada — parede refeita pela mesma fileira de pedras ao lado, com brilho corrigido linha a linha e quantizada nas cores da imagem — e trocada pela lychnos do Grupo 8 (centro x 223, base y 246: a chama animada pode ir por cima) | 2 |
| atmosfera (folhas, poeira dourada) | **reuso**: `ptc_leaf` (G2), `circe_motes` recolorido na integração, feixes de luz já pintados no fundo | 0 |

**Descartados:** `bedroom_a` (também bom — cama com tronco à direita, chão aberto, tapete — mas sem o mar, sem os teares e
com duas lamparinas de vidro anacrônicas; fica em `_candidatos` como alternativa).
**Para revisão:** o casal `ending_reunion_close` (78 px) fica pequeno diante da cama (~140 px) — a tela final pode usar a
câmera mais próxima ou o par em escala 1,5× por moda; leve brilho residual na parede acima da lamparina (lê como a luz dela).
Prancha (candidato original × final com o casal): `Docs/Art/PixelLab/_g11_ending.png`.

---

# FECHAMENTO — Grupo 12 · auditoria final · 0 gerações

## Saldo
| | Usadas | Restantes |
|---|---|---|
| Início do pass | 987 | 1013 |
| **Fim do pass** | **1153** | **847** |
| **Consumo do pass** | **166** | — |

Teto planejado ~330 / autorizado até ~500–600: o pass fechou com **166** — metade do plano base. Reserva final **847**
(meta era ≥ ~400). 65 jobs disparados, **65 entregues, 0 falhas, 0 recuperações** (`_ledger.tsv`).

## Gerações por grupo
| # | Grupo | Gerações | Ferramentas | Assets |
|---|---|---|---|---|
| 1 | Combate | 94 | `pro` ×4 (~88) + pixen + anim | 5 |
| 2 | Partículas | 35 | `pro` ×1 (~22) + pixen | 11 |
| 3 | Água | 8 | pixen + anim | 4 |
| 4 | Tempestade de Citera | 6 | pixen + anim | 5 |
| 5 | Circe | 7 | pixen + anim (o `pro` previsto não foi preciso) | 5 |
| 6 | Mundo dos Mortos | 4 | pixen + anim | 4 |
| 7 | Cila e Caríbdis | 2 | pixen + anim | 3 |
| 8 | Fogo | 6 | anim ×5 + pixen | 5 |
| 9 | Mapa | 2 | pixen | 4 |
| 10 | Transições | 0 | código + composição | 3 |
| 11 | Ending | 2 | pixen | 1 |
| 12 | Auditoria | 0 | — | — |
| | **Total** | **166** | 5 chamadas `pro` (~110), 34 pixen, 26 anim | **50** |

O `pro` (4 candidatos por ~22 gerações) só valeu onde o pixen não chegava (golpes do Grupo 1, poeira do Grupo 2); dali em
diante, pixen primeiro resolveu tudo — 9 dos 11 grupos ficaram em ≤ 8 gerações.

## Auditoria final
- **Reprodutível:** os 11 grupos (`node Tools/build-pack.js <grupo>`: combate2, particulas, agua, tempestade, circe, mortos,
  cila, fogo2, mapa2, transicoes, ending) reconstruídos do zero → os **50 PNG + .meta saem byte a byte idênticos**.
- **QA** (`node Tools/pack-audit.js`): **123 arquivos, 0 falhas**; os 50 do pass estão todos cobertos. Avisos restantes são
  de propósito: pixels soltos = faíscas, gotas e dissoluções; "longe da paleta" = eletricidade, magia e fogo espectral.
  O auditor ganhou: os arquivos novos fora de `Effects/Items/Map/UI/Ending` e a exceção de canto opaco para ladrilhos,
  9-slices e o cenário do Ending.
- **Importação:** 42,857 px/un, Point, sem compressão, alpha duro, fatias de tamanho igual; pivô na base para tudo que pousa
  (poeira, splash, fogo, onda, alma, raio) e no centro para o resto. Substitutos de sprites em cena (`*_anim` de fogo,
  onda de Citera, raio, braseiro espectral, nuvem iluminada) mantêm **o canvas e o pivô do original** — troca direta.
- **Nada fora de arte:** nenhuma cena, prefab, script ou `Resources` alterado; só `Assets/Art/`, `Docs/` e `Tools/`.
  Nenhum asset de rodadas anteriores foi sobrescrito.

## Criados (50) — por pasta
- `Effects/Combat/` fx_hit_v2, fx_slash_v2, fx_thrust, fx_heavy_slash, fx_ricochet
- `Effects/Particles/` Combat/ptc_spark, Debris/ptc_chip, Debris/ptc_debris_tint, Dust/ptc_dust_puff, Dust/fx_land_dust,
  Dust/fx_fall_dust, Dust/ptc_skid_dust, Water/ptc_drop, Water/ptc_foam, Nature/ptc_leaf, Environment/ptc_sand
- `Effects/Water/` fx_splash_large, fx_splash_small, fx_ripple
- `Effects/Storm/` fx_storm_warn_crackle, fx_storm_strike
- `Effects/Magic/` fx_circe_circle, fx_circe_circle_ground, fx_circe_transform, fx_circe_cure, fx_circe_smoke
- `Effects/Boss/` fx_scylla_strike, fx_scylla_telegraph
- `Environments/` Cytera/Ocean/cytera_wave_large_anim, Cytera/Weather/cytera_lightning_b, cytera_lightning_strike,
  Cytera/Background/cytera_storm_cloud_lit, MundoDosMortos/Props/mortos_brazier_tall_spectral_anim,
  MundoDosMortos/Souls/mortos_soul_wander, mortos_soul_wander_small, mortos_soul_vanish,
  CilaCaribdis/Ocean/cila_tidal_surge_anim, Ciclopes/Special/ciclopes_giant_fire_anim, Ciclopes/Props/ciclopes_torch_wall_anim,
  Eolo/Palace/eolo_brazier_native_anim, Ithaca/Props/ithaca_torch_01_anim, Shared/Props/shared_oil_lamp_anim
- `Map/` States/map_state_completed_wreath, States/map_state_current_ring, map_wave_glint, map_trail_dot
- `UI/Transitions/` ui_meander_band_tile, fx_victory_laurel, ui_death_vignette_9slice
- `Ending/` ending_bedroom_olive

## Reutilizados (0 gerações) — o que mais rendeu
Raio, nuvem, onda grande e braseiro existentes (variações, animação recomposta, recolor); splash e poeira (G2/G3) →
golpe da Cila; transformação ao contrário → cura pela moly; splash grande reamostrado → splash pequeno; coroa dourada
descartada no mapa → vitória; faíscas → vitória; lychnos (G8) → quarto do Ending; o mar de Cila como referência de cor da
maré. Por código: meandro, círculo de Circe no chão, anel do mapa, ondinha, trilha, vinheta, aviso da Cila, grãos de areia,
lascas de faísca, debris tingível.

## Candidatos e descartes
137 candidatos/quadros em `Docs/Art/PixelLab/PP-*/_candidatos/`; os descartados ficam lá (motivo de cada um na seção do
grupo). Em resumo: objetos que o prompt inventou (tábua no aviso do raio, pedras na fumaça, árvore na poeira, pentagrama
no círculo de Circe), tom errado (fantasminha de desenho animado, raio de cartum, louro dourado sobre aro de bronze) e
ruído (elipse do chão, recorte mascarado da onda). Alternativas boas guardadas: `laurel_b`, `bedroom_a`.

## Prompts
Grupos 4–11, literais: `Docs/Art/PixelLab/_prompts_pass02.json` (25 entradas: pixen e ações de `animate_image`). Grupos 1–3:
modelo descrito na seção do Grupo 1 e job de cada um no `_ledger.tsv` (o texto literal não sobreviveu à compactação da
sessão). Lições que valem para o próximo pass: descrever efeito por **forma e cor**, nunca pelo nome do objeto; pedir
"fileira de partículas separadas" e cortar por componente; para fogo, animar só o recorte da chama e recompor (protegendo
pixels sólidos não-chama).

## Pendências
1. **Integração (etapa separada, nada feito aqui):** em ordem de impacto —
   **(a) as 2 `TidalHazard` da Fase 11 estão invisíveis em jogo** → `cila_tidal_surge_anim` como filho de cada uma;
   (b) StormHazard: warn → `fx_storm_warn_crackle`, strike → `cytera_lightning_strike` + `fx_storm_strike` + troca da nuvem;
   (c) `BossController` da Cila: `AttackTelegraphed` → telegraph, `AttackExecuted` → strike;
   (d) `TransformationZone` → `fx_circe_circle_ground`; `TransformationEffect` → transform / cure;
   (e) trocar os 5 fogos e o braseiro dos Mortos pelos `_anim`; (f) estados do mapa no `LevelSelectController`;
   (g) `ScreenFader`/`DeathOverlay`/`LevelCompleteMenu` → meandro, vinheta, louros; (h) `EndingController` → quarto + casal;
   (i) combate e partículas nos pontos de impacto/pouso/corrida.
2. **HUD (PP-18/PP-19 do plano)** — não estava na ordem aprovada dos 12 grupos; fica para um próximo pass (~8 gerações).
3. **8 animações de personagem já pagas** (rodada anterior): continuam no servidor; `backblaze.pixellab.ai` segue bloqueado
   pela política de rede do ambiente. Baixar com `node Tools/fetch-paid-anims.js` quando liberado — 0 gerações.
4. Pontos de revisão por grupo (escala do casal no Ending, vaso azinhavrado, faíscas da vitória, velocidade do anel do mapa,
   marrom parado do braseiro de Éolo) — listados em cada seção.

Prancha geral: `Docs/Art/PixelLab/_polish_pass_02.png` (grupos 1–11 numerados, 1:1); pranchas por grupo `_gN_*.png`.
