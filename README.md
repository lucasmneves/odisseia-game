# Odisseia

Platformer 2D baseado na *Odisseia* de Homero, feito em Unity com alvo Web (WebGL).

---

## 1. Visão do jogo

Você é **Odisseu**, voltando de Troia para Ítaca. A campanha tem **16 fases jogáveis**, cada uma inspirada num episódio do mito e construída em torno de uma ideia de jogo diferente — fuga, esquiva, resistência, decisão, exploração, stealth — em vez de repetir a mesma fórmula dezesseis vezes.

**Tom**: colorido, amigável e legível. Todo o visual é placeholder geométrico (retângulos coloridos), de propósito: a prioridade foi ritmo e clareza de gameplay, não arte definitiva. Não há violência gráfica em nenhum momento — inimigos derrotados simplesmente somem, e o desfecho é emocionalmente positivo.

**Duração estimada**: ~50-70 minutos para a campanha completa (3-7 min por fase, por dimensionamento — ver *Troubleshooting*).

---

## 2. Controles

| Ação | Teclas | Onde funciona |
|---|---|---|
| Mover | `A` / `D` ou `←` / `→` | Fases |
| Pular | `Espaço` | Fases |
| Atacar | `J` | Fases |
| Interagir | `E` | NPCs, gado sagrado (Fase 11), saco dos ventos (Fase 5) |
| Pausar | `Esc` | Fases |
| Avançar diálogo | `Espaço` / `J` | Durante cutscenes |
| Pular diálogo | `Esc` | Durante cutscenes |
| Navegar menus | Mouse | Menus |

Definidos em `Assets/ScriptableObjects/PlayerControls.inputactions` (Unity Input System), em dois action maps: `Player` (Move/Jump/Attack/Interact/Pause) e `Dialogue` (Advance/Skip). O mapa `Dialogue` fica ativo só durante falas, e o `Player` é desligado nesses momentos — por isso avançar o diálogo não faz Odisseu pular junto.

---

## 3. Arquitetura

Composição sobre herança, componentes pequenos e um punhado de estáticos para estado global. Nenhum framework externo.

### Camadas

```
Core/       Estado e dados que não dependem de cena
            GameManager, CampaignManager, HealthSystem, LevelDefinition,
            GameAssets, AudioLibrary

Systems/    Serviços reutilizáveis, agnósticos de fase
            SceneLoader, SaveSystem, CameraFollow, CheckpointManager,
            CollectibleCounter, AudioManager, DamageFeedback, VfxBurst,
            ParallaxLayer, MovingPlatform, DecisionFlags

Player/     Componentes de Odisseu (um por capacidade)
            PlayerController, PlayerCombat, PlayerRespawn, PlayerInputLock,
            + efeitos por fase: LotusEffect, WindBagAbility, TransformationEffect,
              SirenResistance, HungerMeter, DisguiseEffect

Enemies/    EnemyController (patrulha/perseguição), BossController (telegrafado)

Levels/     Peças que compõem uma fase
            LevelManager, LevelIntro, LevelGoal, KillZone, TutorialTrigger,
            DialogueTrigger, NPCDialogue + hazards/zonas específicos por fase

UI/         Telas e HUD
            HUD, DialogueSequence, PauseMenu, DeathOverlay, ScreenFader,
            MainMenuController, LevelSelectController, LevelCompleteMenu,
            EndingController, SettingsPanel, UITheme + indicadores
```

### Decisões que valem explicar

- **`HealthSystem` é genérico**: Player, inimigos e bosses usam o mesmo componente. Quem quiser reagir (respawn, feedback, morte) assina os eventos `Damaged`/`Died`. É por isso que `DamageFeedback` funciona igual em todos sem nenhum `if`.
- **Estáticos para estado de sessão**: `CheckpointManager`, `CollectibleCounter` e `DecisionFlags` são estáticos porque são globais por natureza e precisam sobreviver a trocas de cena sem um objeto carregado junto. `CampaignManager` é MonoBehaviour porque precisa referenciar assets no Inspector.
- **`GameAssets` via Resources**: catálogo único (sprite + biblioteca de áudio) carregado uma vez. Sem ele, cada script de feedback precisaria de referências serializadas em 21 cenas.
- **Disfarce por layer de física** (Fase 14): mudar o layer de Odisseu faz os inimigos deixarem de detectá-lo, sem tocar em `EnemyController`.
- **Fases são dados, não código**: `LevelDefinition` (ScriptableObject) descreve cada fase; `CampaignManager` só lê a lista. Adicionar uma fase 17 não exige mudar nenhum sistema.
- **Degradação graciosa**: todo acesso a singleton usa `?.`. Abrir qualquer fase direto no Editor (sem passar pelo Boot) funciona — só não salva progresso.

---

## 4. Estrutura de diretórios

```
Assets/
├── Art/Player/          PlaceholderSquare.png — o único sprite do jogo inteiro
├── Audio/Generated/     12 clipes .wav gerados proceduralmente (ver "Áudio")
├── Materials/
├── Prefabs/             Player, EnemyBasic, Checkpoint, Collectible, LevelGoal
├── Resources/           GameAssets.asset (catálogo carregado em runtime)
├── Scenes/
│   ├── Boot/            Boot.unity
│   ├── Menu/            MainMenu, LevelSelect, LevelComplete, Ending
│   └── Levels/          Level_01_Troia .. Level_16_Final
├── Scripts/             Core, Systems, Player, Enemies, Levels, UI, Editor
├── ScriptableObjects/
│   ├── PlayerControls.inputactions
│   ├── AudioLibrary.asset
│   └── Levels/          16 LevelDefinition.asset
└── Editor/

.github/workflows/       build-web.yml (CI, ver "Deploy")
Builds/WebGL/            saída do build (fora do controle de versão)
```

---

## 5. Fases

Fluxo comum a todas: **início → diálogo de abertura → gameplay → checkpoint → objetivo → diálogo de encerramento → transição**. Todas reaproveitam `LevelIntro` / `LevelGoal` / `LevelManager` / `DialogueSequence` / `HUD` / `CameraFollow`.

| # | Fase | Ideia central |
|---|---|---|
| 1 | Troia | Tutorial: mover, pular, atacar. Poços, combate, desafio de plataformas |
| 2 | Cícones | Sem mecânica nova — pequenas arenas de combate e obstáculos |
| 3 | Lotófagos | **Lótus**: zonas que acumulam sonolência, reduzem velocidade e travam o controle no limite |
| 4 | Ciclopes | **Polifemo** (mini-boss): ataques telegrafados; a saída é esquivar e correr, não matar |
| 5 | Éolo | **Vento**: correntes que empurram, plataforma móvel, saco dos ventos (`E`) para desobstruir |
| 6 | Lestrígones | **Fuga**: ameaça que persegue por trás + gigantes arremessando pedras |
| 7 | Circe | **Transformação**: zonas mágicas desativam o combate temporariamente; erva de moly cura |
| 8 | Mundo dos Mortos | Atmosfera e narrativa: ruínas, névoa, falas no meio da fase |
| 9 | Sereias | **Resistência sonora**: barra que drena na zona de influência; o mastro dá imunidade |
| 10 | Cila e Caríbdis | **Sobrevivência**: ondas, redemoinho e Cila como ameaças ambientais, sem combate |
| 11 | Gado do Sol | **Recurso + tempo + decisão**: fome drena; comer o gado sagrado resolve, mas cobra o preço |
| 12 | Calipso | Fase tranquila: exploração e narrativa, sem inimigos |
| 13 | Feácios | **NPCs**: cidade e palácio, diálogos por interação |
| 14 | Ítaca | **Disfarce**: guardas não detectam Odisseu disfarçado; stealth leve e reencontros |
| 15 | Pretendentes | **Combate em grupo**: o confronto no salão, com Penélope e Telêmaco |
| 16 | Arco de Odisseu | **Clímax**: atravessar as argolas na ordem para revelar o objetivo → tela final |

Progressão: concluir uma fase desbloqueia a próxima (`CampaignManager`), salva coletáveis e pontuação. **Continue** no menu retoma a fase desbloqueada mais avançada. A Fase 16 é a única que carrega `Ending` em vez de `LevelComplete`.

---

## 6. Polish (esta etapa)

### Visual
- **Transições**: fade-out/fade-in entre todas as cenas (`ScreenFader`, singleton automático — nenhuma cena precisa configurar).
- **Partículas**: `VfxBurst` — estilhaços de sprite com gravidade e fade, em ataque, acerto, dano, morte, coleta e checkpoint. Feito com SpriteRenderers em vez de `ParticleSystem` de propósito: para WebGL, meia dúzia de sprites é mais barata e previsível que instanciar sistemas de partículas.
- **Feedback de dano**: `DamageFeedback` (genérico, um componente para Player/inimigos/bosses) pisca o sprite em branco, solta partículas, sacode a câmera e toca o som.
- **Background**: `ParallaxLayer` nos fundos de todas as fases — profundidade com um sprite por camada, custo desprezível.
- **UI**: `UITheme` centraliza paleta e tamanhos; todos os botões receberam estados normal/hover/pressed/disabled consistentes entre menu, gameplay, pause, game over, vitória e seleção de fases.

### Gameplay
- **Screen shake leve**: `CameraFollow.Shake()` — 0.1s no acerto, um pouco mais na morte. A posição do follow é mantida separada da final, senão o shake entraria no `SmoothDamp` do frame seguinte e a câmera brigaria consigo mesma. Um shake mais forte substitui um mais fraco em vez de somar, para golpes em sequência não acumularem tremor.
- **Pause** (`Esc`): tela com Continuar / Reiniciar fase / Menu principal. Usa a ação `Pause`, que existia desde a primeira etapa sem nada ligado a ela.
- **Feedback de morte**: aviso curto na tela antes do respawn. A regra de respawn não mudou — só ganhou a comunicação que faltava.

### Áudio
Organizado em `AudioLibrary` (música por contexto + efeitos por evento) e tocado por `AudioManager` (persistente, dois AudioSources). Cada cena tem um `SceneAudio` dizendo qual faixa toca — é assim que "música por fase" fica declarativa.

| Categoria | Clipes |
|---|---|
| Música | menu, fase calma, fase tensa, vitória |
| Combate | ataque, acerto, dano no jogador, morte |
| Progressão | coleta, checkpoint, pulo |
| UI | clique |

**Todos os 12 clipes são gerados proceduralmente** por script (ondas senoidais/quadradas/serra + envelopes, escritos como WAV). Nenhum asset de terceiros, portanto **zero questão de licenciamento** — e arquivos pequenos: 3-24 KB por efeito.

---

## 7. Performance Web

Medições reais do projeto atual:

| Métrica | Valor | Observação |
|---|---|---|
| GameObjects por cena | 48-75 (média 49) | Confortável para WebGL 2D |
| Total no projeto | 1.035 em 21 cenas | — |
| Texturas | **1** (`PlaceholderSquare.png`, 8×8 px) | Todo o jogo usa o mesmo sprite recolorido |
| Draw calls | Mínimos por construção | Um sprite + um material ⇒ batching quase total |
| Áudio (fonte) | ~1,2 MB WAV | Comprimido em Vorbis no build |
| Partículas | Sprites simples, ≤12 por burst, auto-destrutivos | Sem `ParticleSystem` |
| Física | Só 2D, sem malhas, sem joints | — |
| **Build final** | **5,8 MB** (wasm 4,3 + dados 1,3 + loader 0,2) | Menor que o build de 10 fases da etapa anterior (9,9 MB), mesmo com 6 fases a mais, áudio e polish |

Configurações aplicadas automaticamente por `BuildScript.ApplyWebGLSettings()` (para o build da CI ser idêntico ao local):
- Compressão **Brotli** + `dataCaching`
- `exceptionSupport = None` (wasm menor e mais rápido)
- IL2CPP com `OptimizeSize` + stripping **High**
- `stripUnusedMeshComponents`, sem símbolos de debug

---

## 8. Execução local

**Requisito**: Unity **6000.5.8f1** com o módulo *WebGL Build Support*.

1. Abra o projeto pelo Unity Hub.
2. Abra `Assets/Scenes/Boot/Boot.unity`.
3. Pressione **Play**. O Boot encaminha para o menu principal.

Para testar uma fase isolada, abra a cena dela e dê Play — funciona normalmente (só não salva progresso, por não passar pelo `CampaignManager` do Boot).

---

## 9. Build Web

### Pelo Editor
`File > Build Settings > WebGL > Build`, saída em `Builds/WebGL`.

### Por linha de comando (CLI)

```bash
Unity.exe -batchmode -quit -projectPath . -executeMethod BuildScript.BuildWebGL
```

Sai com código `0` em sucesso e `1` em falha. A saída vai para `Builds/WebGL/` (ignorada pelo Git).

### Rodar o build no navegador

WebGL **não abre por `file://`** — precisa de um servidor HTTP. O mais simples:

```bash
cd Builds/WebGL && python -m http.server 8080
```

Depois acesse `http://localhost:8080`.

---

## 10. Deploy (GitHub Actions)

`.github/workflows/build-web.yml` implementa **checkout → Unity → Build Web → Deploy** (GitHub Pages), com cache da pasta `Library` para builds subsequentes não reimportarem o projeto inteiro.

**Este repositório não contém nenhum secret real.** Para o workflow funcionar, configure em *Settings > Secrets and variables > Actions*:

| Secret | Para que serve | Como obter |
|---|---|---|
| `UNITY_LICENSE` | Conteúdo do arquivo `.ulf` de licença Personal | Gerado pelo fluxo de ativação do [game-ci](https://game.ci/docs/github/activation) |
| `UNITY_EMAIL` | E-mail da conta Unity | Sua conta Unity |
| `UNITY_PASSWORD` | Senha da conta Unity | Sua conta Unity |

Para licença **Pro**, troque `UNITY_LICENSE` por `UNITY_SERIAL`.

Também é preciso habilitar *Settings > Pages > Source: GitHub Actions*. Sem os secrets, o job falha na ativação de licença — comportamento esperado e explícito.

---

## 11. Troubleshooting

| Sintoma | Causa provável | Solução |
|---|---|---|
| Tela preta ao abrir o build | Aberto via `file://` | Sirva por HTTP (ver seção 9) |
| Build carrega mas não roda; erro de `Content-Encoding` no console | Servidor não envia header do Brotli | Configure o servidor, ou troque para `WebGLCompressionFormat.Disabled` em `BuildScript` |
| Botões não respondem | Cena sem `EventSystem` + `InputSystemUIInputModule` | O projeto usa só o Input System novo; o módulo legado não capta cliques |
| Nada é renderizado na cena | Câmera com `z = 0`, no mesmo plano dos sprites | Câmera 2D deve ficar em `z = -10` |
| Progresso não salva | Fase aberta direto, sem passar pelo Boot | Comece por `Boot.unity` — o `CampaignManager` vive lá |
| `Another Unity instance is running` no build CLI | Editor aberto no mesmo projeto | Feche o Editor antes de rodar o build por linha de comando |
| Áudio mudo no navegador | Política de autoplay do browser | Clique na página uma vez; o áudio começa após a primeira interação |
| Fase parece longa/curta demais | Ritmo nunca foi medido em playtest | Ver nota abaixo |

**Nota honesta sobre balanceamento**: as durações (3-7 min) e as dificuldades foram dimensionadas pela matemática de pulo/velocidade, **não medidas em playtest real**. Os números que mais provavelmente vão precisar de ajuste depois de você jogar: velocidade do perseguidor (Fase 6), dreno da resistência às sereias (Fase 9), drenagem da fome (Fase 11) e a dificuldade do combate em grupo (Fase 15).

---

## 12. Estado atual

- ✅ Compila sem erros
- ✅ 16 fases jogáveis + menus + tela final (21 cenas em Build Settings)
- ✅ Save/load validado por teste automatizado (round-trip campo a campo)
- ✅ Validação estrutural das 21 cenas: **0 problemas**
- ✅ Build WebGL gerado pela CLI: **5,8 MB**
- ✅ **Build aberto e verificado num navegador real**: engine inicializada, contexto WebGL 2.0 criado, física/Input System/áudio ativos, todos os assets em HTTP 200, barra de carregamento concluída, **zero erros no console**
- ⏳ **Não** passou por playtest humano — balanceamento é estimativa, e a jogabilidade em si (sensação de controle, dificuldade) não foi verificada visualmente

### Próximos passos sugeridos

- Playtest completo para calibrar dificuldade e ritmo
- Arte definitiva e tilemaps (hoje tudo é placeholder geométrico)
- Música e efeitos definitivos (hoje procedurais)
- Diálogos condicionais usando `DecisionFlags` (a decisão da Fase 11 já é registrada, mas nada a consome ainda)
