# Fase 16 — Final · Environment Art

`Level_16_Final` · 2026-10-01 · PixelLab: **20 gerações** (845 → 865 de 2000)

A última fase se passa no mesmo palácio das fases 01, 14 e 15, na manhã seguinte à retomada.
A tese da arte: **a mesma casa, outra hora.** A estrutura vem das fases anteriores; o que é novo
é só o que a 16 tem e as outras não — a prova do arco, a lareira, o tear de Penélope e a luz do
amanhecer.

> **Estado da integração:** os assets estão processados, na paleta, com densidade conferida e
> importados em `Assets/Art/Environments/Final/` com `.meta` escrito. O vestidor
> `FinalSceneDresser` está pronto, mas **não foi executado**: a sessão que fez este trabalho não
> tinha Unity. A cena `Level_16_Final.unity` está intocada. CampaignValidation, WebGL Build e as
> capturas reais ficam para a primeira execução local (comandos na seção 9).

---

## 1. A cena antes

`CastProbe` não roda sem Unity; a leitura foi feita direto no YAML da cena (que não enxerga
instâncias de prefab, por isso Player, LevelGoal e Checkpoint vieram dos `propertyPath`):

| Objeto | Posição / tamanho | Observação |
|---|---|---|
| `Floor_1` | x −16..0, topo y=−2 | placeholder colorido |
| `Floor_2` | x 0..40, topo y=−2 | placeholder colorido |
| `Sky_Background` | 70 × 6 un em (12, −4) | placeholder colorido |
| `AxeRing_10/18/26/34` | x=10, 18, 26, 34; y=−1,1 | quadrado cinza 0,15 × 1,6; trigger 0,45 × 4,8 |
| `Player` | x=−12 | — |
| `Checkpoint` | x=2 | — |
| `LevelGoal` | x=38 | revelado pelo `BowChallenge` |
| Câmera | `CameraFollow` tamanho 6, limites x −18..44, y −6..8 | com o alvo em y=−1 a câmera trava em y=0 |

Personagens: só o Odisseu (já registrado em `Docs/Characters/FINAL_CHARACTER_ART_POLISH.md`).

## 2. Escala — medida, não assumida

| | |
|---|---|
| PPU | **42,857143** (Odisseu 60 px = 1,4 un) — o mesmo de todo o projeto |
| Densidade | **nativa 1×** em todos os 11 assets novos (`Tools/scale-probe.js`) |
| Assets de Ítaca de referência | trono 178×221 px e braseiro 125×190 px, ambos 1× no arquivo, exibidos a 0,62 e 0,55 pela fase 15 |

Os assets novos entram em **escala 1** no Transform. Os reusados mantêm a escala que já tinham
na 15 (trono 0,62, braseiro 0,55, coluna 1,15, flechas 0,7), para a 16 ser idêntica à 15 nesses
objetos — a pendência de densidade deles é anterior e não foi tocada.

| Asset novo | px | un | × Odisseu |
|---|---|---|---|
| Machado no poste | 57 × 79 | 1,33 × 1,84 | 1,31 — vão do machado a 1,33 un do chão, altura da cabeça |
| Arco de Odisseu | 26 × 87 | 0,61 × 2,03 | 1,45 — o arco que nenhum pretendente armou |
| Lareira (base e chama) | 103 × 82 | 2,40 × 1,91 | — |
| Tear de Penélope | 91 × 97 | 2,12 × 2,26 | 1,6 — tear de pesos, de pé |
| Pedras (primeiro plano) | 154 × 60 | 3,59 × 1,40 | — |
| Coluna quebrada (primeiro plano) | 53 × 140 | 1,24 × 3,27 | — |
| Fundo do amanhecer | 1108 × 288 | 25,85 × 6,72 | período do ladrilho |
| Fachada do palácio (midground) | 1534 × 232 | 35,79 × 5,41 | período do ladrilho |

## 3. Assets reutilizados (0 gerações)

| Asset | Origem | Uso na 16 |
|---|---|---|
| `pret_hall_wall_band` | fase 15 | parede do salão, de x=0 até onde a câmera vê, tom de manhã `(1, 0,97, 0,92)` |
| `pret_throne` | fase 15 | trono, em x = objetivo + 4 (42), sem as lanças dos pretendentes |
| `itaca_ret_brazier` | fase 14 | 4 braseiros (−14,6; −2,4; 3; 39,8) |
| `ithaca_stone_ground_top/body` | fase 14 (do tileset da 01) | piso de laje dos dois `Floor_*` |
| `ithaca_column_01` | fase 01 | as duas colunas da porta do salão (x ±0,9) |
| `ithaca_arrows_bundle_01` | fase 01 | feixe de flechas ao lado do arco |

## 4. Assets novos

Fonte e derivados em `Docs/Environment/Fase16/`; no Unity em `Assets/Art/Environments/Final/`.

| Arquivo | Grupo | Origem |
|---|---|---|
| `final_axe_post.png` | Props | `final_axe_post_d` |
| `final_bow_odysseus.png` | Props | `final_bow_odysseus_c` |
| `final_hearth_base.png` + `final_hearth_fire.png` | Props | `final_hearth_a`, separado em duas peças |
| `final_loom_penelope.png` | Props | `final_loom_penelope_b` |
| `final_bg_dawn.png` | Background | `final_bg_dawn_b`, recortado até o sol e espelhado |
| `final_mg_palace_band.png` | Midground | `final_mg_palace_a`, céu cortado, espelhado |
| `final_fg_stones.png` | Foreground | `final_fg_stones_a` |
| `final_fg_column.png` | Foreground | `final_fg_column_a` |
| `final_dawn_shaft.png` | VFX | procedural (0 gerações) |
| `final_dawn_gradient.png` | VFX | procedural (0 gerações) |

`node Tools/build-final.js` reconstrói todos os derivados a partir de `_fontes/`.

## 5. Gerações — 20 no total

Todas `create_image_pixen` (1 geração cada). Nenhum `pro`, nenhuma geração de 4 candidatos.

| Asset | Gerações | Escolhido | Descartados e por quê |
|---|---|---|---|
| Machado | **4** | `d` | `a`, `b` com uma **flecha atravessada** (o prompt dizia "so an arrow could pass through it" e o modelo desenhou a flecha); `c` virou um anel, não um machado. O `d` foi um prompt novo sem a palavra "arrow" |
| Arco | 3 | `c` | `a` e `b` sem corda |
| Lareira | 2 | `a` | `b` com fogo pequeno e menos acolhedor |
| Tear | 3 | `b` | `a` sem os pesos de argila; `c` com moldura fechada embaixo, que é tear de pedal e não grego |
| Fundo | 2 | `b` | `a` com uma árvore grande no centro, que repetiria a cada ladrilho |
| Midground | 2 | `a` | `b` com montanhas pintadas atrás da fachada, impossíveis de separar do céu |
| Pedras (FG) | 2 | `a` | `b` veio isométrico |
| Coluna (FG) | 2 | `a` | `b` veio inteira, não quebrada |

Créditos: saldo do PixelLab **845 → 865** usadas (`get_balance`), dentro do teto de ~25.

**Um disparo do machado `d` falhou na rede** e não foi registrado pelo servidor (a lista de jobs
mostrou 7 no lote B, não 8) — não cobrado. Duas respostas caíram no polling depois de o job ter
sido cobrado (`stones_a` e o primeiro `get_image` errado); foram recuperadas por `get_image` com o
`job_id` de `list_jobs`, sem gerar de novo.

### Prompts

Sufixo comum: *"Retro 16-bit pixel art, crisp pixels, flat readable shapes, no anti-aliasing."*
Sprites soltos: `view: side`, `outline: single color black outline`, `detail: medium detail`,
`no_background: true` e o enquadramento positivo *"Isolated game asset cut out on a fully
transparent background, only the X"* (regra da seção 3 do `ESTADO_ATUAL`). Os prompts completos
estão em `_lote_a.json`, `_lote_b.json` e `_lote_c.json`.

| Asset | Canvas | Prompt (resumo) |
|---|---|---|
| Machado `d` | 64×96 | *single heavy ancient Greek double-headed bronze axe head, a labrys with two curved blades, with a round empty hole in the middle where the handle socket is, mounted upright on a short plain wooden post in a small square stone base, you can see straight through the open round hole* |
| Arco | 64×96 | *the great bow of Odysseus, an elegant ancient Greek recurved bow of dark polished wood and horn with gently curled bronze-capped tips and a thin taut string, standing upright … a precious old heirloom, simple and heroic* |
| Lareira | 128×96 | *the round central hearth of an ancient Greek palace hall, a low raised circular platform of pale cream limestone blocks with a painted red and dark blue meander band around its rim, a fire of crossed oak logs … warm and welcoming* |
| Tear | 96×112 | *the upright loom of Penelope, an ancient Greek warp-weighted loom of pale oak … half-woven cloth of cream and deep red with a simple meander border … a row of small round clay loom weights* |
| Fundo | 768×288 | *a wide calm dawn landscape of the Greek island of Ithaca … soft clear pale blue sky fading to warm peach and pale gold near the horizon, the sun just rising … calm sea … Peaceful, hopeful early morning light, the end of a long journey* |
| Midground | 768×256 | *a long continuous row of the palace of Odysseus on Ithaca seen from directly in front at early morning, strict flat side profile … portico of thick plain fluted columns … slim dark cypress trees and olive trees … plain pale morning sky above the rooftops* |
| Pedras | 160×64 | *a low foreground clump of weathered pale limestone blocks and a fallen fluted column drum half-buried in earth, wild acanthus leaves, tall dry grass and a small flowering caper bush* |
| Coluna | 64×160 | *a broken fluted column of pale cream limestone standing upright, snapped off at the top, wrapped in dark green ivy, a young olive branch growing from its base* |

## 6. Processamento (`Tools/build-final.js`)

- **Paleta:** a da fase 15 (`PRETENDENTES_PALETTE`: as 9 rampas de Ítaca + Bronze, Fogo, Vinho,
  Noite), sem rampa nova. A 16 muda a **luz**, não a paleta. Props, primeiro plano e a fachada:
  0 cores fora da paleta, 0 pixels semitransparentes. O fundo fica **fora** da quantização — é céu,
  e degradê espremido em quatro passos vira faixa (regra do projeto desde Ítaca).
- **Classificador:** o da 15, com duas opções novas medidas:
  - `temVinho: false` no machado e no arco — a madeira deles mede S 0,55–0,7 e saía cor de estandarte;
  - `fogoMinS: 0,68` na lareira — a pedra do aro iluminada mede S 0,40–0,55, a chama 0,70+; com o
    0,50 da 15 metade do aro virava fogo. Na lareira, a chama é testada antes do vinho (a ponta
    vermelha mede H < 14) e Oliva/Folhagem viram Fogo (o miolo amarelo entre as toras).
- **Lareira em duas peças, no mesmo canvas:** a chama é o fogo **ligado** ao que sobe acima do aro
  (inundação a partir da chama de cima; pontos de reflexo soltos no aro ficam na base). Acima do
  aro a chama sai da base; dentro do poço fica brasa (Fogo Profunda), para não abrir buraco quando
  a chama for animada. Mesmo canvas → mesmo pivô → as duas peças se sobrepõem exatas na mesma
  posição. **A chama pode ser animada por código depois** (escala, cintilação, troca de sprite)
  sem tocar na pedra.
- **Fundo:** o espelho ingênuo duplicava o sol (dois sóis a 9,8 un, dentro da mesma tela de 21 un).
  Apagar o segundo por pintura deixou listras. A solução foi geométrica: recortar a fonte até a
  coluna do sol (x=554, o pixel mais claro) e espelhar em volta dela — o sol cai na dobra e
  aparece uma vez por período de 25,9 un. Emenda 252,6 → 9,5 (p90 interna 13,3).
- **Midground:** céu cortado por flood-fill do topo (27%), quantizado como arquitetura,
  espelhado (emenda 174,9 → 61,9, p90 interna 91,1).
- **Luz procedural:** facho de sol oblíquo 48×256 (alpha até 0,32 — 0,22 mal aparecia na prévia)
  e véu de ouro pálido 256×8 (alpha 0 → 0,16 por coluna), o inverso do véu de fogo da 15.

## 7. Import

`FASE_SRC=Docs/Environment/Fase16 FASE_DST=Assets/Art/Environments/Final node Tools/unity-import.js`
(grupo `GRUPOS_FINAL` novo no importador). 11 texturas, `.meta` escritos, todos conferidos:
PPU 42,857143, filtro Point, compressão desligada, `alphaIsTransparency`, pivô **BottomCenter**,
GUID derivado do caminho e único no projeto. Faixas que repetem (fundo, fachada, véu): wrap
Repeat e malha FullRect; o resto Clamp e Tight.

## 8. Integração — `FinalSceneDresser`

`Assets/Scripts/Editor/FinalSceneDresser.cs`, no molde do `PretendentesSceneDresser`. Cria tudo sob
`FinalScenery` e só **desliga o SpriteRenderer** de `Floor_*`, `Sky_Background` e `AxeRing_*`.
Nenhum colisor, trigger, script, posição de gameplay, câmera, inimigo ou objetivo é alterado.

### Sorting

| Ordem | O quê |
|---|---|
| −60 | `Sky_Fill` (#b0c5d1, cor medida do topo do fundo) |
| −50 | `BG_Dawn` — parallax 0,92, base y=0,3 (horizonte ≈3,5, acima dos frontões) |
| −45 | `MG_Palace` — parallax 0,55, base y=−2,2, tom `(0,94, 0,94, 0,98)` |
| −30 | parede do salão (15), de x=0 em diante |
| −28 | colunas da porta (01) |
| −25 | fachos de sol (x=6, 14, 22, 30, 38) |
| −12 / −11 / −10 | terra, corpo e topo do piso |
| −8 | trono |
| −7 / −6 | braseiros, base da lareira, tear |
| −5 | chama da lareira |
| −4 / −3 | arco, flechas, **machados** |
| 2 | Odisseu (sem mudança) |
| 20 | véu do amanhecer |
| 30 | primeiro plano, em contraluz `(0,42, 0,40, 0,46)` |

### Composição

| Região | x | Conteúdo |
|---|---|---|
| Pátio ao amanhecer | −18 … 0 | fundo + fachada do palácio; braseiros; **arco** em −4,4 (inclinado 10°) com o feixe de flechas |
| Porta do salão | 0 | colunas da 01 sobre a junta pátio/parede |
| **Machados** | 10, 18, 26, 34 | um sobre cada `AxeRing`, posição **lida do anel** (se o level design mover, o machado acompanha) |
| **Lareira** | 22 | centro do mégaron, entre os anéis 18 e 26 |
| **Tear** | 30 | entre os anéis 26 e 34 |
| Fim | 38 … 44 | objetivo (38), braseiro (39,8), trono (42) |
| Primeiro plano | −15,4 · −17,6 · 43,4 | só nas pontas — nada entre o início (−12) e o objetivo |

Decisões:
- **Sem teto escuro no salão** (a 15 tinha): acima da parede aparece o céu da manhã.
- **O midground só aparece no pátio**: dentro do salão a parede da 15 passa à frente. O
  amanhecer dentro do salão é contado pelos fachos e pelo véu, que clareia em direção ao fim.
- O arco ficou em −4,4, e não em −6,4: na prévia ele sumia contra um cipreste escuro do midground.

## 9. Validação

| Verificação | Resultado |
|---|---|
| Paleta (props, FG, midground) | 0 cores fora, 0 semi-alpha |
| Densidade (`scale-probe`) | 11/11 nativas 1× |
| Emenda das faixas (`seam-test`) | fundo 9,5 ≤ p90 13,3 · fachada 61,9 ≤ p90 91,1 |
| `.meta` (11) | 0 falhas, GUIDs únicos |
| Prévia fora do Unity (`Tools/preview-final.js`) | 7 quadros, **0 px magenta** (buraco) em todos |
| `PlaceholderProbe` | `FinalScenery/Sky_Fill` e `Ground_Fill` justificados com cor medida |
| **CampaignValidation** | **não executado** — sem Unity na sessão |
| **WebGL Build** | **não executado** — sem Unity na sessão |
| Capturas reais | **não feitas** — `FinalSceneDresser.Shots` pronto |

As imagens em `_previa/` são uma **montagem fora do Unity** com as mesmas posições do vestidor
(câmera tamanho 6 em y=0, parallax pela fórmula do `ParallaxLayer`). Servem para julgar
composição e escala; não provam o import nem o modo Tiled do SpriteRenderer, que foi aproximado.

Para fechar, com o Editor fechado:

```
Unity.exe -batchmode -quit -nographics -projectPath . -executeMethod FinalSceneDresser.Run -logFile Logs/final.log
Unity.exe -batchmode -quit -nographics -projectPath . -executeMethod PlaceholderProbe.Run -probeScene Assets/Scenes/Levels/Level_16_Final.unity
Unity.exe -batchmode -quit -nographics -projectPath . -executeMethod CampaignValidation.Run
Unity.exe -batchmode -quit -projectPath . -executeMethod FinalSceneDresser.Shots
```

`Shots` grava em `_capturas/`: início, arco, machados, lareira, tear, fim e a composição larga.
Depois, o WebGL Build pelo fluxo de sempre, e contar `#ff00ff` nas capturas (a câmera de captura
limpa em magenta de propósito).

## 10. Pendências

- Rodar o vestidor, as probes, as capturas e o WebGL Build (seção 9).
- A chama da lareira está separada para animação, mas **não anima**: o projeto não tem componente
  de cintilação, e criar um sistema estava fora do escopo.
- Os braseiros e o trono reusados continuam encolhidos pelo Transform (0,55 / 0,62), herdado da 15.
