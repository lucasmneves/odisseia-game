# Fase 03 — Cicones: elenco

**Extensão do `Docs/Characters/CHARACTER_ART_MASTER.md` (v1), que não foi alterado.** Segue também o
padrão de `Fase02/TROY_CAST.md` (inimigo por facção, override na cena, Run a partir do quadro 3).
Aprovado em 2026-09-29.

- Tiras: `Fase03/_tiras.png` · Capturas: `Fase03/_capturas/` · Facções: `Docs/Characters/CAST_FACTIONS.png`
- Id e prompt: `Docs/Characters/cast.json` (`Cicones_Warrior`)

## 1. CastProbe

```
Unity.exe -batchmode -nographics -quit -projectPath . -executeMethod CastProbe.Run
          -probeScene Assets/Scenes/Levels/Level_03_Cicones.unity
```

O probe foi estendido nesta fase: agora também conta **Animator** na cena, lista os **overrides de
cada instância de inimigo** (é o único jeito de dois `EnemyBasic` diferirem em gameplay) e os
**scripts do jogo** presentes, para achar papel de personagem que não usa `EnemyController`.

Resultado:

| Item | Achado |
|---|---|
| Personagens com figura | Odisseu (Player) e 3 `EnemyBasic` (x = −10, 8, 22) |
| Arte dos inimigos | `CHR_Enemy_Basic`, pintado, 100 px/un, escala 0,82, **capa e crista carmesim** |
| Animator / AnimatorController | **0** — o projeto usa `SpriteAnimator` (folha em `Resources`, estado pelo nome do sprite) |
| Variantes de inimigo | **nenhuma**: as três instâncias não têm override, são idênticas ao prefab |
| Estados acionados | `EnemyAnimator`: **Idle, Run, Hit, Death** (constantes do código; Attack, AttackThrow e Jump da folha antiga nunca rodam) |
| NPCs | nenhum (`NPCDialogue` ausente) |
| Personagens narrativos | nenhum em cena — as duas falas (intro/outro) são do Odisseu |
| Gregos | nenhum em cena — o Greek Soldier Master **não** é usado aqui |
| Scripts de fase | nenhum com figura de personagem (há `LotusEffect`, `DisguiseEffect`, `HungerMeter` etc. do jogador, sem arte própria) |

## 2. Inventário

| Personagem | Papel | Facção | Prefab | Sprite antes | Animator | Estados usados | Decisão | Prioridade |
|---|---|---|---|---|---|---|---|---|
| Odisseu | Player | Casa de Odisseu | `Player` | `CHR_Odysseus` (master v1) | `SpriteAnimator` | 16 | **reutilizado** | — |
| Guerreiro cicone ×3 | Inimigo | Cicones | `EnemyBasic` | `CHR_Enemy_Basic` (carmesim) | `SpriteAnimator` | Idle, Run, Hit, Death | **override na cena** com master novo | 1 |

## 3. Facções presentes

| Facção | Cor | Silhueta exclusiva |
|---|---|---|
| Casa de Odisseu | carmesim | capa curta aberta + espada à frente |
| **Cicones** | **oliva, terra, madeira, bronze** (rampas Oliveira, Terra seca, Madeira e Bronze do cenário dos Cicones) | **gorro de pele pontudo, manto longo até a canela**, escudo grande, lança na diagonal |

Os Cicones eram **trácios**; a identidade sai daí: gorro de pele de raposa (alopekis), manto de lã
estampado (zeira), braceletes de bronze. Nenhum desses elementos existe no grego (crista alta) nem no
troiano (capacete frígio) — a separação vem de forma, não só de cor.

## 4. Cicones Warrior Master

| | |
|---|---|
| Folha | `Assets/Resources/Odisseia/Enemies/CHR_Cicones_Warrior.png` |
| PixelLab | `f4640c38-d2a2-4e6f-9a1d-353a7b84a946` · v3 · size 60 |
| Altura | 58 px = **1,35 un** (0,97 do Odisseu) — perto do colisor de 1,2 un, como o troiano |
| Estados | **Idle** 6 `[0,1,2,3,2,1]` · **Run** 5 `[3..7]` · **Hit** 6 · **Death** 8 |

Prompt (base `side view, 16-bit pixel art game character, Bronze Age, Thracian coast of the northern Aegean`):

> Ciconian warrior, a Thracian coastal tribesman defending his town, lean wiry adult man, tall pointed
> fox-fur cap with ear flaps, long dark braided beard, ankle-length olive-green woollen cloak with an
> earth-brown geometric zigzag border, leather jerkin over a short brown tunic, soft leather boots, bronze
> arm rings, crescent-shaped wicker shield covered in brown hide on his back arm, short spear in his
> front hand, spear and shield stay attached to his hands in every frame

Animações: as mesmas receitas do troiano (`ANIM_TROIANO` em `Tools/build-cast.js`).

**Run:** quadros 0–2 são pose parada → arrancada; o loop usa **3–7**, a regra da Fase 02.

### O que o modelo não obedeceu
- **O escudo saiu redondo**, não em meia-lua. Não regerei: a silhueta já se separa pelo gorro e pelo manto.
- **Manto com capuz e pele** beira o "patrulheiro" de fantasia medieval. Braceletes de bronze, lança
  curta e manto trácio seguram a época; aceito, com registro.
- **Death** cai de bruços (pedido: de costas) — mesma coisa do troiano, lê bem.

## 5. Variações e NPCs

**Nenhuma gerada.** O CastProbe mostra três instâncias idênticas ao prefab, sem arqueiro, pesado ou
comandante, e nenhum NPC. Variação de cor, quando precisar: o oliva do manto (60–75° de matiz, 29% do sprite) está
fora da faixa da pele e varia de graça com `Tools/cast-variants.js`.

## 6. Integração (override na cena)

- `Assets/Scripts/Editor/CiconesCastDresser.cs` → `CiconesCastDresser.Dress`.
- O override ficou num helper comum, `EnemyFactionOverride.Aplicar(resources, tag)`, agora usado
  também pelo `TroyCastDresser` (refatorado; Troia verificada de novo pelo CastProbe: mesmos 3
  troianos e 3 gregos, 42,857 px/un, escala 1).
- Override por instância: folha do `SpriteAnimator`, sprite inicial, escala do Body 0,82 → 1, ordem 0 → 2.
- **Prefab `EnemyBasic` intacto** (arquivo de 20/08). Colisor, vida, patrulha, dano, FPS e loop por
  estado continuam os do prefab.

## 7. Problemas e riscos

| | |
|---|---|
| Inimigo carmesim (fere D-014) | substituído por override |
| **Camuflagem:** o oliva do cicone é da família da vegetação do cenário | risco de leitura, medido nas capturas: em x = 8, na frente da carroça, o contraste cai e quem segura é o contorno preto. Não regerei — é a paleta que o briefing e o cenário pedem. Se o playtest mostrar inimigo sumindo, a correção barata é clarear o manto por código |
| **Oliva também é o xale da mulher de Ítaca** | as duas nunca estão na mesma fase; o oliva **não** vira cor reservada como o carmesim (D-020) |
| Silhueta Odisseu × Cicone é o par mais próximo (ambos com manto atrás) | separados pelo comprimento do manto, escudo e lança; aceito |

## 8. Testes

- CastProbe em Troia e Cicones: todos os inimigos a 42,857 px/un, escala 1.
- `PlaceholderProbe` nos Cicones: **nada visível, nada inativo**.
- `CampaignValidation`: passa. Zero erro de compilação.
- Build WebGL: ver relatório.
- Capturas em x = −10, 8, 22.
- **Não testado:** play mode, navegador, mobile, gamepad — as animações só rodam em runtime.

## 9. Custo

**6 gerações** (663 → 669 de 2000): 1 master × 2 + 4 animações. Nenhuma descartada.
