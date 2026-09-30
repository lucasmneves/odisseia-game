# FINAL CHARACTER ART POLISH — auditoria global

2026-09-30. Extensão do `CHARACTER_ART_MASTER.md` (v1), **que não foi alterado**: nenhuma regra global mudou — uma
regra que estava ESCRITA errado nos docs das fases 14 e 15 foi corrigida (ver §3).

- Auditoria bruta: `Docs/Characters/_audit_global.txt` (antes) e `_audit_global_depois.txt` (depois)
- Capturas antes/depois: `Docs/Characters/_final_polish/` (`P01..P08_*_antes|depois.png`)
- Correções de cena: `Assets/Scripts/Editor/FinalPolishDresser.cs` (`Dress`, `Shots -polishShots antes|depois`)

## 1. Auditoria (Etapa 1)

`Assets/Scripts/Editor/GlobalCastAudit.cs` abre as **16 cenas** numa rodada (a campanha tem 16: o briefing fala em 15
porque junta Ítaca Return e Pretendentes). Para cada figura de personagem: folha, px/un, escala, ordem, sorting layer,
estado; e três achados automáticos: cenário na faixa de ordem do jogador, personagem parado empatado com o jogador,
primeiro plano sobre personagem parado. Também o `EnemyBasic` com a folha carmesim antiga.

**Resultado de consistência:**
- **Odisseu:** `CHR_Odysseus`, 42,857 px/un, escala 1, nas 16 cenas (e o `Odysseus_OnDeck` da 01). Nenhuma versão alternativa.
- **Todo o elenco** das 16 cenas a 42,857 px/un e escala 1 — exceto os dois parceiros de treino da 01 (P-08), agora corrigidos.
- **Nenhuma arte pintada** (`CHR_NPC_*`) restante em cena.
- **Animator/AnimatorController:** 0 em todas as cenas (o projeto usa `SpriteAnimator`).
- **Sorting layer:** todos em `Default`; a ordem é só `Order in Layer`.
- **`Level_16_Final`:** só o Odisseu — nenhum personagem a auditar.

**Facções (carmesim).** Medida a fração de pixels carmesim (matiz 340–10°, saturação > 0,45) de cada folha de personagem:
só o `CHR_Enemy_Basic` (15%) violava a regra, e ele não é mais usado em cena nenhuma. Os demais índices altos são da
casa de Odisseu (Odisseu 11%, Instrutor 11%, Telêmaco adulto 6,5% — barra da túnica) ou pele sombreada que cai na mesma
faixa de matiz (Lestrigão 6,7%, pele ocre-avermelhada). Micenas segue índigo + ouro (arauto, soldado grego).

## 2. Pendências consolidadas e classificação (Etapas 2 e 4)

A — Unity · B — Scene Override · C — editar asset existente · D — PixelLab · E — não corrigir

| ID | Fase | Elemento | Tipo | Problema | Solução | Classe | PixelLab | Prioridade | Status |
|---|---|---|---|---|---|---|---|---|---|
| P-01 | 11 | Cila | SILHOUETTE | pés humanoides abaixo das serpentes: "pairando" | **poleiro de basalto** (`cila_crag_stack` da fase) à frente da parte de baixo (ordem −19), topo nas coxas: ela passa a estar SOBRE o rochedo | B | não | ALTA | **corrigido** (melhorado) |
| P-02 | 11 | Caríbdis | ENTITY | folha a 0,42 no Transform (densidade 2,4×) | folha **renderizada na densidade nativa** (`build-cila-fx.js`, moda) → escala 1,012, mesmo tamanho | C | não | MÉDIA | **corrigido** |
| P-03 | 06 | Palácio de Éolo | ENVIRONMENT | peças a 0,52–0,70 no Transform (granuladas) | 6 texturas **reamostradas** (`downscale-native.js`, moda) em escala 1, mesmas caixas | C + B | não | MÉDIA | **corrigido** (exceto entablamento, N-06) |
| P-04 | 08 | Porco da Circe | SISTEMA | tinta rosa no Odisseu | nenhuma: não há asset de porco e o `TransformationEffect` só tinge. Overlay/sprite exigiria estado no `PlayerAnimator` ou código novo no efeito | E | — | MÉDIA | **mantido** |
| P-05 | 12 | Rebanho | COMPOSIÇÃO | leitura de fila | recomposto em **pares sobrepostos + bois soltos**, lados alternados, os de trás 0,1 un mais altos e uma ordem atrás; bois interativos intocados | B | não | MÉDIA | **corrigido** |
| P-06 | 12 | Árvore grande | ASSET | xadrez pintado | o xadrez estava **quantizado** em Muro seco (219,204,184 / 140,127,116) — por isso o filtro de cinza não o via. Removidos os componentes ≥ 16 px dessas duas cores (3.329 px) | C | não | MÉDIA | **corrigido** |
| P-07 | **13** (não 14) | Jangada | SORTING | Odisseu some atrás dela no gatilho | jangada ordem 2 → **−1** | B | não | ALTA | **corrigido** |
| P-08 | 01 | Parceiros de treino (`EnemyBasic`) | FACÇÃO | lanceiro carmesim pintado, 100 px/un, 0,82 | **soldado de Ítaca** (`Soldier_Ithaca`, já existente) + Run/Hit/Death → `CHR_Ithaca_Sparring`, por override | D + B | **sim (3)** | ALTA | **corrigido** |
| P-09 | 15 | `EnemyBasic` | FACÇÃO | carmesim | já resolvido na etapa da Fase 15 (pretendentes açafrão) — confirmado pela auditoria | — | — | ALTA | **já resolvido** |
| N-01 | 14, 15 | docs | PROCESSO | docs e dresser supunham o jogador na ordem 2 | medido: **0** (o 2 do prefab é o `ShieldVisual`). Dresser e docs corrigidos | A | não | ALTA | **corrigido** |
| N-02 | 02, 04, 08, 09, 13, 14, 15 | NPCs/figuras | SORTING | empatados com o jogador na ordem 0 | ordem **−1** | B | não | MÉDIA | **corrigido** (exceto 2 gregos da fogueira em Troia: −1 os empataria com o terreno) |
| N-03 | 12 | arbustos de 1º plano | SORTING | cobriam os cascos dos bois interativos | deslocados o mínimo (+1,59 / −1,82 un) | B | não | BAIXA | **corrigido** |
| N-04 | 05 | rocha de 1º plano | SORTING | cobre 0,15 un dos pés do Polifemo | lê como "atrás da pedra" | E | — | BAIXA | mantido |
| N-05 | várias | ~170 props | SORTING | cenário na ordem 0–9 por cima do jogador | profundidade intencional (ondas de Citera, destroços de Cila, rochas/barricadas de Troia, portões da 01, braseiros da 14); nenhum num ponto de PARADA além da jangada | E | — | BAIXA | mantido |
| N-06 | 06 | entablamento de Éolo | ENVIRONMENT | **esticado 2,0×** (o oposto do resto do palácio) | ampliar não cria detalhe — só um asset novo de ~512 px resolveria | (D) | não feito | BAIXA | **pendente** |
| N-07 | todas | `ShieldVisual` do jogador | CHARACTER / UI | **placeholder** (quadrado azul, 8 px/un) ligado pelo `PlayerShield` enquanto o Odisseu defende | **o master já desenha o escudo** (Shield/ShieldHold); o placeholder o cobria. Renderer do placeholder desligado no prefab — ver `Shield/SHIELD_ART.md` | A | não | MÉDIA | **corrigido** (defesa no ar sem visual) |
| N-08 | 02 | geometria de Troia | ENVIRONMENT | `PlaceholderProbe` acusa 10 placeholders visíveis (muralha, ponte, plataformas, mastro/vela do objetivo, céu) | pré-existente (a cena não os mudou nesta etapa) | — | — | MÉDIA | **pendente — novo** |
| N-09 | 01 | Hit do parceiro | ANIMAÇÃO | lê mais investida que recuo | aceito | E | — | BAIXA | mantido |
| N-10 | 14 | disfarce, trepadeiras | — | disfarce sem visual; contorno azul nas trepadeiras | registrados na Fase 14 | E | — | BAIXA | mantido |

## 3. Correções

### Unity / Scene Override (`FinalPolishDresser.Dress`, idempotente, depois dos dressers de cada fase)
- **01:** `EnemyFactionOverride.Aplicar("Odisseia/Enemies/CHR_Ithaca_Sparring")` nos 2 parceiros (x 182 e 234). Prefab intacto.
- **02, 04, 08, 09, 13:** figuras paradas 0 → −1 (checando empate com cenário na −1).
- **06:** 13 renderers do palácio trocados pela versão nativa, escala 1 (degraus em Tiled com o `size` convertido).
- **11:** Caríbdis na folha nativa (escala pela fórmula do marcador); poleiro `FinalPolish_Cila/Scylla_Perch`.
- **12:** rebanho recomposto; arbustos afastados dos bois interativos.
- **13:** jangada −1; Calipso −1.
- **14, 15:** `IthacaReturnCastDresser` corrigido (ordem do jogador 0; NPCs −1) e reaplicado.

### Assets editados por código (sem PixelLab)
| Asset | Ferramenta | Antes → depois |
|---|---|---|
| `Resources/Odisseia/Environments/FX_Charybdis.png` | `Tools/build-cila-fx.js` (`ESCALA_NATIVA = 0,42`) | quadros 512×256 → **215×108** (backup em `Docs/Environment_CilaCaribdis/_FX_Charybdis_0.42_antes.png`) |
| `eolo_column/_bronze_door/_steps/_brazier/_statue/_banner` `_native.png` | `Tools/downscale-native.js` (novo) | 96×320→50×166, 160×256→83×133, 510×128→265×67, 128×256→79×159, 128×288→77×173, 256×128→179×90 |
| `GadoDoSol/Fields/gado_shade_tree_big.png` | remoção por cor exata | −3.329 px de xadrez (backup em `Docs/Environment_GadoDoSol/_gado_shade_tree_big_com_xadrez.png`) |

**Filtro de moda** (`downscale-native.js`): cada pixel recebe a cor mais frequente do bloco de origem — nunca inventa
cor, então paleta e contorno seguem pixel art. Com o filtro Point, o Unity já descartava pixels a cada quadro; fazer uma
vez fixa quais ficam e acaba com o tremor quando a câmera anda.

### O que NÃO funcionou (registrado)
**P-01 por edição de cor** (`Tools/scylla-no-feet.js`): apagar os pés por saturação (pele ≤ 0,16, serpentes ≥ 0,21) deixou
as sombras dos pés (mais escuras que o limiar) e abriu buracos no ventre lavanda das serpentes. **Revertido** — os quadros
originais foram restaurados e a folha nunca foi remontada. O caminho que funcionou foi composição.

## 4. PixelLab

| | |
|---|---|
| Item | P-08 — único classificado como D |
| Personagem | `Soldier_Ithaca` existente (`cast.json`), **sem master novo** |
| Gerado | Run 8, Hit 6, Death 8 (v3, Receita B — a de Troia, só com a lança) |
| Créditos | antes **842** usadas / 1158 restantes → depois **845** / 1155 |
| Gerações | **3** · descartes **0** |
| Folha | `Resources/Odisseia/Enemies/CHR_Ithaca_Sparring.png` (25 quadros; Run 3–7), via `build-cast-sheets.js` com a nova opção `origem` |

Prompts: *running forward fast … spear held level in the front hand, the spear stays attached to his hand in every frame* ·
*recoiling from a blow to the chest …* · *struck down … landing flat on his back … the spear dropping beside him*.

Nenhuma outra geração. P-01, P-02, P-03, P-05, P-06 e P-07 foram resolvidos sem PixelLab.

## 5. Validação

| Teste | Resultado |
|---|---|
| `GlobalCastAudit` (depois) | 0 `CHR_Enemy_Basic`; personagens todos a 42,857 px/un e escala 1 (só o `ShieldVisual` acusa, N-07); empates restantes: os 2 gregos da fogueira (Troia) e a rocha do Polifemo |
| `PlaceholderProbe` nas 11 cenas alteradas | **10 OK**; Troia falha com 10 placeholders de geometria pré-existentes (N-08) |
| `CampaignValidation` | passa (exit 0, 0 erros de compilação) |
| Build WebGL | **Build Finished, Result: Success** (exit 0) |
| Capturas | 8 pares antes/depois em `_final_polish/` |

**Não testado:** play mode (combate com os parceiros, defesa com o escudo, diálogos), navegador, mobile, gamepad.

## 6. Ferramentas novas
`GlobalCastAudit` (16 cenas), `ScaleListProbe`, `SortingNearProbe` (os dois exigem alvo e saem com erro sem ele),
`Tools/downscale-native.js`, `FinalPolishDresser`. `CastProbe` ganhou `-probeArt` e a seção ZONA na Fase 12.
