# Fase 12 — Gado do Sol: elenco

**Extensão do `Docs/Characters/CHARACTER_ART_MASTER.md` (v1), que não foi alterado.** 2026-09-30.
Contém o **SUN CATTLE MASTER v1**. O gado é **ANIMAL / ENVIRONMENT interativo**: sprite estático, sem estado de jogo.

- Master e rotações: `Fase12/_master_rotacoes.png` · variações de pelagem: `_variacoes.png`
- Capturas: `Fase12/_capturas/` (`antes_004|008|013`, `odisseu_e_boi`, `boi_2`, `rebanho`, `boi_comido`, `ilha_-08|04|16|28`)
- Id e prompt: `Docs/Characters/cast.json` (`Sun_Cattle`)

## 1. CastProbe

`CastProbe.Run -probeScene Assets/Scenes/Levels/Level_12_GadoDoSol.unity -probeArt Cattle,Herd`

**O CastProbe não via o gado.** Ele só listava figura com papel (jogador, inimigo, chefe, NPC) ou em pasta de
personagem, e o gado mora em `Art/Environments/GadoDoSol/SacredCattle/`: a primeira rodada devolveu 5 linhas,
nenhuma delas um boi. Ganhou o argumento `-probeArt` (casa nome do objeto ou caminho da textura; imprime caixa,
ordem e cor) e uma seção **ZONA** (gatilho de `Odisseia.Levels`: colisor, estado do renderer próprio e que
arte pedida cai sobre ele).

| Entidade | Implementação encontrada | Classificação |
|---|---|---|
| **Gado interativo ×2** | `SacredCattle` (x = 4 e 13): `SacredCattleZone` + gatilho 1,0 × 0,9 un. Interact = fome cheia, **20 de dano**, flag `AteSacredCattle`, e **alpha 0,3 no SpriteRenderer do próprio gatilho**. Renderer do gatilho **desligado** (placeholder); a arte era outro objeto, `GadoDoSolScenery/Cattle_0/1` | **ANIMAL / ENVIRONMENT interativo** |
| **Rebanho de ambiente ×5** | `GadoDoSolScenery/Herd_-3, 7, 9, 16, 17`: props, ordem −2, tinta de distância | **ENVIRONMENT** |
| Odisseu | Player, master v1; nenhum estado especial (comer é Interact sem animação) | reutilizado |
| Tripulação | **não existe** na cena (só citada na fala) | — |
| Personagens narrativos | nenhuma figura. Falas: intro do Odisseu ("Circe avisou: não toquem no gado sagrado de Hélio…") e outro do narrador | — |
| Inimigos, chefes, NPCs | nenhum | — |
| Animator / Controllers / Clips | **0** | — |
| Outros | `RationPickup` ×2, `Collectible` ×3, `TimeGatedActivator` (portão de 25 s no `LevelGoal`) | não são personagem |

| CHARACTER | ROLE | FACTION | PREFAB | SPRITE (antes) | ANIMATOR | USED STATES | ACTION | PRIORITY |
|---|---|---|---|---|---|---|---|---|
| Gado do Sol (interativo) | interação | Gado do Sol | nenhum (objeto da cena) | `gado_cattle_idle/grazing`, **escala 0,55** | nenhum | nenhum (estático) | **master novo + override** | 1 |
| Rebanho | cenário | Gado do Sol | nenhum | idle/grazing/calf, **escala 0,38–0,50** | nenhum | nenhum | override com o master | 1 |
| Odisseu | jogador | Casa de Odisseu | Player | `CHR_Odysseus` | `PlayerAnimator` | os 16 do master | reutilizado | — |

## 2. O que estava errado

1. **Densidade.** O gado antigo era prop gerado grande (138 × 94 px = 3,22 un) e **encolhido no Transform** a
   0,38–0,55: pixels até 2,6× menores que os do Odisseu, o defeito de "arte encolhida" já registrado no projeto.
2. **A consequência invisível.** O `SacredCattleZone` esmaece o renderer **do próprio gatilho** — que o dresser de
   cenário tinha desligado, pondo a arte num objeto à parte. **Quem comia o boi o via continuar inteiro na tela.**
   Nenhum erro no console: o script funcionava, só mexia no renderer errado.

## 3. SUN CATTLE MASTER v1

| | |
|---|---|
| Arquivo | `Assets/Art/Environments/GadoDoSol/SacredCattle/sun_cattle_idle.png` (75 × 64 px) |
| PixelLab | `26c5d746-9d2d-47e5-bc7f-12a7b96df0e6` · **`pro`**, quadrúpede (gabarito **`horse`**), **`style_character_id` = master do Odisseu**, size 80 |
| Perfil | rotação **`east`** (desta vez sem deslocamento de 45°) |
| Escala | **1,75 × 1,49 un** com chifres; **cernelha 48 px = 1,12 un**, abaixo do Odisseu (1,40) |
| PPU / pivô | 42,857 · (0,5; 0), os cascos próximos na última linha do recorte |
| Paleta | creme-branco com sombra ouro-ocre (a rampa `Gado sagrado` da fase), chifres marfim, cascos escuros, **coleira dourada fina** (já existia no gado antigo: mantida). 33 cores. Sem carmesim, sem efeito mágico |
| Silhueta | cabeça, **chifres em lira**, dorso reto, **quatro patas** separadas, cauda com tufo |

Por que `pro` + `horse`: o v3 recusa quadrúpede (Fase 08); o gabarito `horse` é o de casco e perna longa; o estilo
do Odisseu põe o boi no mesmo nível de acabamento do elenco.

Prompt (base `side view, 16-bit pixel art game character, Bronze Age Mycenaean Greek` — **sem palavra de lugar**,
porque "ilha" no texto-base é desenhada sob o personagem, Fase 06):

> sacred cattle of the sun god in Greek myth, a calm sturdy ox with a robust body and a deep chest, four sturdy
> legs with dark hooves, cream-white coat with soft warm golden-ochre shading, long pale ivory horns curving
> upward with golden tips, gentle dark eyes, a tail ending in a tuft, a thin golden collar, peaceful friendly look

**Variação controlada:** `sun_cattle_idle_ochre.png`, pelagem ocre por código (`Tools/build-sun-cattle.js`). Troca
por rampa, não por matiz: o creme mora na mesma faixa do chifre. Só o pelo abaixo da cernelha e com **saturação
HSV** < 0,5 escurece; chifre, coleira, casco e contorno ficam. (A primeira versão filtrava por saturação **HSL**,
que num creme quase branco passa de 0,9 — o pelo claro ficava de fora e a variação mal aparecia.)

**Estados:** nenhum. O gado é estático e nenhum código aciona estado — Idle, Walk, Run, Hit, Death e Eat não foram
gerados. A regra do Run no quadro 3 não se aplica.

## 4. Integração — SCENE OVERRIDE

`Assets/Scripts/Editor/SunCattleCastDresser.cs` (roda **depois** do `GadoDoSolSceneDresser`, que recria os
`Cattle_i` e desliga o renderer do gatilho):

- **Gatilhos:** a arte passa para o **renderer do próprio `SacredCattle`**, ligado, ordem 1; o `Cattle_i` separado
  sai. O gatilho desce para o chão (y = −2), escala 1, e o `BoxCollider2D` é recalculado em coordenadas locais para
  cobrir **o mesmo retângulo do mundo** — o dresser confere e sai com erro se ele se mover. Resultado:
  x 3,50..4,50 / 12,50..13,50, y −1,95..−1,05, antes e depois. O primeiro boi olha para quem chega (esquerda).
- **Rebanho:** mesmo master, escala 1, creme/ocre alternando e espelhado; os dois **bezerros viraram bois
  adultos**; `Herd_17` a ordem −3 porque encosta no `Herd_16`. Tinta de distância e ordem −2 preservadas.
- `Poses`: fotos sem salvar a cena, incluindo o boi comido (alpha 0,3).

**Intocados:** `SacredCattleZone.cs`, `GadoDoSolSceneDresser.cs`, nenhum prefab, `PlayerAnimator`. Os PNGs antigos
(`gado_cattle_*`, `gado_calf`) ficam no projeto, sem referência nesta cena depois do override.

**Mudança de comportamento visível (e só ela):** o boi comido agora esmaece, como o script sempre pretendeu.

## 5. Validação

| Teste | Resultado |
|---|---|
| CastProbe | 7 bois a 42,857 px/un, **escala 1,00**; interativos x 3,13..4,88 e 12,13..13,88, **y −2,00..−0,51**; rebanho y −2,05 |
| Contato com o chão | pivô nos cascos; topo do chão y = −2; na captura os cascos pousam na linha da grama, junto aos pés do Odisseu |
| Colisor | idêntico antes/depois (conferido pelo dresser) |
| `PlaceholderProbe` | OK — nada visível, nada inativo |
| `CampaignValidation` | passa (exit 0, 0 erros de compilação) |
| Build WebGL | **Build Finished, Result: Success** (exit 0) |

**Não testado:** play mode (a interação comer → dano → esmaecer foi simulada na captura, não jogada), navegador,
mobile, gamepad.

## 6. Custo

**21 gerações** (806 → 827 de 2000): master `pro` 20 + **1 descartada** — a animação "pastando" foi disparada e a
tarefa foi interrompida antes do download; a fase fechou sem ela (decisão do usuário). Quadros não baixados.

## 7. Pendências

- **Rebanho em fila:** sem bezerro e sem pose pastando, sete bois do mesmo tamanho na mesma linha leem um pouco como
  desfile. Um bezerro (`pro` com estilo = este master, ~20) ou a pose pastando (já paga, só baixar) resolveriam.
- **Xadrez pintado na árvore grande** (`gado_shade_tree_big`, x ≈ 2) — ENVIRONMENT ART, visível em todas as capturas.
- Cila "pairando" (CHARACTER ART / SILHOUETTE); Caríbdis a 0,42 (ENTITY ART); palácio de Éolo a 0,52 (ENVIRONMENT
  ART); porco da Circe como tinta rosa; `EnemyBasic` carmesim nas fases 01, 14 e 15.
