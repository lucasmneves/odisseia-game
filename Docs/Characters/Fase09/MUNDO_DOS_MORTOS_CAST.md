# Fase 09 — Mundo dos Mortos: elenco

**Extensão do `Docs/Characters/CHARACTER_ART_MASTER.md` (v1), que não foi alterado.** Aprovado em 2026-09-29.
Contém três masters: **SHADE WARRIOR** (inimigo), **SPIRIT NPC MASTER** (`Shade`) e **ANTICLEIA**.

- Masters: `Fase09/_masters.png` · tiras: `_tiras.png` · correção de leitura: `_sombra_guerreira_legivel.png`
- Capturas: `Fase09/_capturas/` (`anticleia_com_odisseu`, `sombras_com_odisseu`, `sombra_guerreira_com_odisseu`, `cena_-012|016`)
- Ids e prompts: `Docs/Characters/cast.json` (`Shade_Warrior`, `Shade`, `Anticleia`)

## 1. CastProbe

| Item | Achado |
|---|---|
| Personagens com figura | só o Odisseu |
| Inimigos | **2 `EnemyBasic`** (x = 6 e 14) com o **`CHR_Enemy_Basic` carmesim**, e um override de cena **já existente de alpha 0,55** (o tratamento espectral é da Unity, pela cor do renderer) |
| Gatilhos de diálogo sem figura | **`DialogueTrigger_Shades`** (x = −10) e **`DialogueTrigger_Mother`** (x = 16), com um altar cada |
| Animator | 0 |

Falas (falante: Odisseu, exceto o desfecho):

| Onde | Fala | O que diz sobre quem está em cena |
|---|---|---|
| Intro | "Circe me mandou aqui… para ouvir a profecia de Tirésias." | — |
| Shades | "Sombras sem nome se aproximam do sangue que derramei." | **as sombras estão presentes** |
| Mother | "Ali… minha mãe. Ela morreu de saudade, esperando meu retorno." | **Anticleia está presente** (ele aponta para ela) |
| Outro (narrador) | "Tirésias falou…" | Tirésias **no passado** — **não está em cena** |

O `MundoDosMortosSceneDresser` pôs os altares nos gatilhos e deixou "o espaço em volta livre para Odisseu,
as sombras e o diálogo entrarem depois" — o mesmo padrão de Éolo e Circe (D-027, D-032).

## 2. Inventário

| Personagem | Papel | Facção | Antes | Estados | Decisão |
|---|---|---|---|---|---|
| Odisseu | Player | Casa de Odisseu | master v1 | 16 — nenhum estado especial do submundo é acionado | **reutilizado** |
| Sombra guerreira ×2 | Inimigo (`EnemyBasic`) | Mortos | lanceiro carmesim pintado, alpha 0,55 | **Idle, Run (3–7), Hit, Death** | **novo, por override** |
| Sombras sem nome ×3 | Figuras no altar | Mortos | nenhuma | **Idle** | **novo — SPIRIT NPC MASTER + 3 variações por cor** |
| Anticleia | Figura no altar | Mortos (casa de Odisseu) | nenhuma | **Idle** | **nova** |
| Tirésias, Elpenor, Aquiles, Agamenon | — | — | — | — | **não gerados**: não estão na cena (Tirésias só é citado no desfecho) |

## 3. Identidade do submundo

**Submundo grego, não inferno medieval.** Paleta dessaturada: cinza-azulado, branco pálido, prata apagada,
violeta pálido. Sem sangue, sem caveira, sem vermelho, sem carmesim.

**A transparência é da Unity, não do sprite.** Os sprites são opacos e simples; o espectral vem da cor
do `SpriteRenderer` (tom frio + alpha), que a cena já usava nos inimigos. Nada de efeito desenhado.

| Figura | Tom × alpha |
|---|---|
| Sombra guerreira (inimigo) | ciano pálido, **0,85** (ver §6) |
| Sombras sem nome | azul 0,50 · violeta 0,45 · verde 0,50 — a variação da multidão é **só por cor** |
| Anticleia | quase branco, **0,72** — mais presente que as sombras anônimas: é a mãe |

## 4. Masters

Base de prompt: `side view, 16-bit pixel art game character, Greek mythology` (sem lugar — lição da Fase 06).

| Master | PixelLab (v3, size 60) | Altura | Prompt |
|---|---|---|---|
| **Shade Warrior** | `3d75c50c-fcc9-4a7b-ab67-99f3396781b5` | 1,38 un (0,98×) | ghostly shade of a dead Greek warrior from the underworld of Greek myth, pale grey-blue skin, a faded ragged tunic and a cracked bronze helmet all washed out in grey-blue and pale white tones, a small battered round shield on his back arm, a broken spear in his front hand, pale glowing eyes, sorrowful stern expression, spear and shield stay attached to his hands in every frame |
| **Shade** (Spirit NPC) | `129989a4-cd80-430b-b76a-f615ec1754dc` | 1,33 un (0,95×) | ghostly shade of a nameless dead person from the underworld of Greek myth, a thin hooded figure wrapped in a long faded grey-blue burial shroud, pale grey-violet skin, pale glowing eyes, arms held close to the body, sorrowful longing expression |
| **Anticleia** | `414939de-a76e-4bf5-bf62-e68568a3c2ee` | 1,35 un (0,97×) | Anticleia, the ghost of the mother of Odysseus in Greek myth, elderly noble Greek woman, grey hair in a low bun under a pale translucent-looking veil, a long faded pale blue-grey peplos and mantle, pale grey skin, hands clasped in front of her, gentle sorrowful loving expression |

- **Sombra guerreira:** silhueta de hoplita (capacete de visor escuro, escudo, lança) toda em cinza e prata —
  distinta do grego (índigo e crista), do troiano (areia) e do cicone (oliva). Animações = as do troiano;
  Run com loop **3–7**.
- **Sombra:** mortalha com capuz, sem foice — morto do submundo, não ceifador.
- **Anticleia:** véu, peplo cinza-azulado, mãos juntas. A pele saiu mais quente que a das sombras, o que a
  faz parecer mais "presente" — coerente com ser a mãe; mantido.

## 5. Integração

`Assets/Scripts/Editor/UnderworldCastDresser.cs`:
- `Dress` — inimigos por `EnemyFactionOverride` (prefab `EnemyBasic` intacto) + cor espectral; sombras e
  Anticleia posicionadas a partir do **altar e dos braseiros lidos da cena**, pés no chão (y = −2), sem
  colisor, com a guarda de 1,5 un do ponto de nascimento.
- `Poses` — Odisseu diante da mãe, das sombras e de uma sombra guerreira; sem salvar a cena.

## 6. Problemas encontrados

| Problema | Situação |
|---|---|
| Inimigo carmesim | **corrigido** — sombra guerreira |
| **Inimigo invisível**: o alpha 0,55 da cena foi pensado para o lanceiro vermelho. Com a arte cinza, a sombra ficou **mais escura que a parede atrás** (luminância 65,9 contra 73,0, medida na captura) | **corrigido** — tom ciano pálido e alpha 0,85 nesta cena; continua translúcida e volta a ler (`_sombra_guerreira_legivel.png`) |
| Terceira sombra cairia a 0,7 un do nascimento do Odisseu (x = −12) | **corrigido** — a guarda recusou; ela foi para o fim da fila, do outro lado do braseiro |
| Sombras e mãe ausentes com falas que as apontam | **corrigido** — figuras nos altares |

## 7. Validação

| Teste | Resultado |
|---|---|
| CastProbe | inimigos e 4 figuras a 42,857 px/un, escala 1 |
| `PlaceholderProbe` | nada visível, nada inativo |
| `CampaignValidation` | passa |
| Compilação | 0 erros |
| Build WebGL | ver relatório |
| Capturas | Anticleia no altar com o Odisseu; três sombras translúcidas voltadas para o sangue; sombra guerreira legível diante do Odisseu |

**Não testado:** play mode, combate, navegador, mobile, gamepad.

## 8. Custo

**12 gerações** (771 → 783 de 2000): 3 masters × 2 + 6 animações. Nenhum descarte.

## 9. Pendências (não tocadas)

- Palácio de Éolo a 0,52 — ENVIRONMENT ART / SCENARIO.
- Porco da Circe — representação por tinta; exigiria mudança no sistema de animação do jogador.
- `EnemyBasic` carmesim ainda nas fases 01, 14 e 15.
