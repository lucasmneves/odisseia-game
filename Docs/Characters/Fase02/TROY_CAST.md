# Fase 02 — Troia: elenco

**Extensão do `Docs/Characters/CHARACTER_ART_MASTER.md` (v1), que não foi alterado.** Tudo aqui
segue as regras de lá: 42,857 px/un, escala nativa, vista `side`, contorno preto, v3, pingue-pongue.
Aprovado em 2026-09-29.

- Prancha de facções (cor + silhueta): `Docs/Characters/CAST_FACTIONS.png`
- Tiras das animações: `Fase02/_tiras.png`
- Capturas na fase: `Fase02/_capturas/`
- Ids e prompts: `Docs/Characters/cast.json` (`Trojan_Soldier`, `Greek_Soldier_Mycenae`)

## 1. Auditoria — quem está na fase

Levantado pelo `CastProbe` (novo, serve a qualquer fase), que varre a cena inteira, inclusive
inativos e instâncias de prefab:

```
Unity.exe -batchmode -nographics -quit -projectPath . -executeMethod CastProbe.Run -probeScene <cena>
```

| Personagem | Papel | Facção | Asset anterior | Decisão |
|---|---|---|---|---|
| Odisseu | Player | Casa de Odisseu | `CHR_Odysseus` (master v1) | **Reutilizado**, sem variação |
| 3× `EnemyBasic` (x = −8, 20, 47) | Inimigo | Troia | `CHR_Enemy_Basic`: pintado, ~8.000 cores, 100 px/un encolhido a 0,82, **capa e crista carmesim** | **Recriado** como Trojan Soldier Master |
| Soldados gregos | — | Micenas | **não existiam** — acampamento só com props | **Novos**, 3 figuras de fundo |
| Agamenon | citado no intro | Micenas | não aparece em cena | não criado (D-015) |
| Personagens narrativos | — | — | nenhum: intro e outro são falas do Odisseu e do narrador | — |
| NPCs civis | — | — | nenhum | — |

O inimigo antigo feria a D-014: vestia o carmesim reservado à casa de Odisseu.

## 2. Facções presentes

| Facção | Cor de identidade | Silhueta exclusiva | Quem |
|---|---|---|---|
| Casa de Odisseu | **carmesim** | capa aberta + espada | Odisseu |
| Micenas / gregos | **índigo + ouro** | **crista alta de crina**, escudo aspis grande | soldados do acampamento (e o arauto, na 01) |
| Troia | **bronze, areia, terracota, azul-ardósia** | **capacete frígio curvado para a frente, sem crista**; corpo compacto | inimigos |

A paleta troiana sai das rampas do **cenário** de Troia (Bronze, Lona, Vermelho de
`Environment_Troy/Palette/TROY_PALETTE`): o troiano pertence às muralhas que defende. O vermelho de
Troia é terracota (`#953029`, puxado para o laranja); o carmesim do Odisseu é `#720e18`, puxado para
o vinho — e mesmo assim a terracota fica só em detalhe (borda da túnica, face do escudo).

**Ponto fraco honesto:** a terracota saiu discreta; o troiano lê mais como *areia + bronze* do que
como vermelho. A separação das facções não depende dela (o índigo e a crista separam o grego; a
ausência de crista e a túnica clara separam o troiano), então não gastei geração para reforçá-la.

## 3. Trojan Soldier Master

| | |
|---|---|
| Folha | `Assets/Resources/Odisseia/Enemies/CHR_Trojan_Soldier.png` |
| PixelLab | `c416f3a0-7b39-4dd0-86ad-969c7dc29075` · v3 · size 60 |
| Altura | 59 px = **1,38 un** (0,98 do Odisseu) |
| Estados | **Idle** 6 `[0,1,2,3,2,1]` · **Run** 5 `[3..7]` · **Hit** 6 · **Death** 8 |
| Por que só esses | são os únicos que o `EnemyAnimator` aciona. A folha antiga tinha Attack, AttackThrow e Jump, que nenhum código toca |

`size` 60 e não 64: o colisor do inimigo é 1,2 un, e 64 daria ~1,49 un, com a cabeça bem acima da
caixa de acerto. 60 fica perto da caixa sem sair da faixa de adulto.

**Run 3–7:** os quadros 0–2 do ciclo gerado são a arrancada a partir da pose parada (Receita B sem
âncora parte da referência). Em loop, eles fariam o soldado "recomeçar a correr" a cada volta.

**Death:** pedi queda de costas; ele cai de bruços. Lê como morte, termina deitado e parado — aceito.

**Folha multi-estado:** uma linha por estado, **uma janela de recorte para o personagem inteiro**
(se cada estado fosse recortado pelo próprio contorno, o corpo pularia na troca Idle → Run) e a base
da célula na linha dos pés do Idle — o 1 px que a queda desce abaixo disso é cortado, para nunca
afundar no chão.

## 4. Greek Soldier Master (Micenas)

| | |
|---|---|
| Folha | `Assets/Resources/Odisseia/Characters/NPCs/CHR_Greek_Soldier_Mycenae.png` |
| PixelLab | `6855cafb-1ea7-47a7-9c5e-a2e6ee2e1dd3` · v3 · size 64 |
| Altura | 64 px = **1,49 un** (1,07 do Odisseu — faixa de soldado) |
| Estados | Idle 6 |
| Em cena | `TroyCast/Greek_Soldier_Fire` (−22,6), `Greek_Soldier_Fire_2` (−19,5, olhando de volta), `Greek_Sentry_Front` (−15,5) |

Figuras de fundo: sem colisor, sem script de gameplay, ordem de desenho 0 (atrás do jogador e dos
props de primeiro plano). Uma quarta, no estandarte (−28,6), foi retirada: caía em cima do ponto
onde o Odisseu nasce.

## 5. Variações — o que NÃO foi gerado, e por quê

O briefing lista lanceiro, espadachim, arqueiro, comandante e pesado para as duas facções.
**Nenhuma aparece no gameplay de Troia**: os três inimigos são o mesmo `EnemyBasic` (mesma vida,
mesmo comportamento), e não há arqueiro inimigo, chefe ou comandante. Gerar variação sem uso é o que
o master proíbe. O sistema para quando aparecerem:

- **Variação de papel** (arma, capacete, escudo) → uma geração v3 nova a partir do prompt do master,
  trocando só o equipamento. Cada uma custa 2 + animações.
- **Variação de cor** → `Tools/cast-variants.js`, de graça. Medido: o índigo grego (225–240°, 25% do
  sprite) é exclusivo da roupa e varia livre. No troiano, túnica, escamas e escudo moram na faixa da
  pele (0–45°, 89% do sprite) e **não** variam; só a faixa azul-ardósia (195–215°, 4%) varia.
- **Patente** → um elemento de silhueta a mais (crista transversal, capa curta índigo), não uma cor nova.

## 6. Integração no Unity

`Assets/Scripts/Editor/TroyCastDresser.cs` (`TroyCastDresser.Dress`), separado do
`TroySceneDresser` para um não refazer o trabalho do outro:

- **Os 3 inimigos são trocados por override nas instâncias desta cena.** O prefab `EnemyBasic`
  é usado em 7 fases e ficou intacto (arquivo de 20/08); nenhuma outra cena referencia o troiano.
- Override: `SpriteAnimator.resourcePath`, sprite inicial, **escala do Body 0,82 → 1** e
  **ordem de desenho 0 → 2** (na ordem 0 o inimigo sumia atrás da fogueira, barricadas e entulho,
  que são 1).
- Intocados: colisor (0,6 × 1,2), vida, patrulha, dano, FPS e loop por estado (definidos no prefab:
  Idle 8, Run 12, Hit 16 sem loop, Death 12 sem loop).

## 7. Problemas encontrados

| Problema | Resolução |
|---|---|
| Inimigo pintado, carmesim, 100 px/un | recriado (Trojan Soldier Master) |
| Inimigo desenhado atrás de props de ordem 1 | ordem 2 nas instâncias de Troia |
| Sentinela grega em cima do ponto de nascimento | retirada |
| O troiano de x = 20 nasce atrás de `FG_Rocks_2` (primeiro plano deliberado, ordem 3) | **mantido** — é profundidade de propósito e cobre o jogador também; ele sai de trás na patrulha |
| `PlaceholderProbe` falha em Troia (9 visíveis: plataformas, muralha de limite, mastro do `LevelGoal`) | **fora do escopo** — é cenário, pendência anterior já registrada no `ESTADO_ATUAL` |
| Os outros 6 estágios com `EnemyBasic` continuam com o inimigo carmesim | pendência: cada fase ganha a sua facção (Cícones, Circe…) |

## 8. Custo

**9 gerações** (654 → 663 de 2000): 2 masters × 2 + 5 animações. Nenhuma descartada.
