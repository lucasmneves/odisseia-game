# Odisseia

Platformer 2D baseado na *Odisseia* de Homero, feito em Unity com alvo Web (WebGL).

---

## 1. Visão do jogo

Você é **Odisseu**: a campanha começa em Ítaca, na convocação para a guerra, e só termina quando ele volta para casa. A campanha tem **16 fases jogáveis**, cada uma inspirada num episódio do mito e construída em torno de uma ideia de jogo diferente — fuga, esquiva, resistência, decisão, exploração, stealth — em vez de repetir a mesma fórmula dezesseis vezes.

**Tom**: colorido, amigável e legível. Todo o visual é placeholder geométrico (retângulos coloridos), de propósito: a prioridade foi ritmo e clareza de gameplay, não arte definitiva. Não há violência gráfica em nenhum momento — inimigos derrotados simplesmente somem, e o desfecho é emocionalmente positivo.

**Duração estimada**: ~50-70 minutos para a campanha completa (3-7 min por fase, por dimensionamento — ver *Troubleshooting*).

---

## 2. Controles

### Desktop

| Ação | Teclas | Onde funciona |
|---|---|---|
| Mover | `A` / `D` ou `←` / `→` | Fases |
| Pular | `Espaço` | Fases |
| Atacar (espada) | `Z` | Fases |
| **Defender (escudo)** | **`X`** (segurar) | Fases |
| **Atirar (arco)** | **`C`** | Fases |
| Interagir | `E` | NPCs, gado sagrado (Fase 11), saco dos ventos (Fase 5) |
| Pausar | `Esc` | Fases |
| Avançar diálogo | `Espaço` / `Z` | Durante cutscenes |
| Pular diálogo | `Esc` | Durante cutscenes |
| Navegar menus | Mouse | Menus |

### Mobile (Web)

Botões virtuais criados automaticamente ao detectar um dispositivo touch:

| Botão | Ação |
|---|---|
| `◄` / `►` | Mover (segurar) |
| `JUMP` | Pular |
| `ATK` | Espada |
| `DEF` | Escudo (segurar) |
| `BOW` | Arco |
| `USE` | Interagir (conversar, examinar) |

### Controle (gamepad)

Xbox (One / Series), DualShock 4, DualSense e qualquer gamepad que o Input System reconheça. O jogo inteiro é jogável só no controle, e teclado e toque continuam iguais.

| Ação | Teclado | Xbox | PlayStation |
|---|---|---|---|
| Mover | `A`/`D`, setas | Analógico esq., D-Pad | Analógico esq., D-Pad |
| Pular | `Espaço` | `A` | `✕` |
| Espada | `Z` | `X` | `□` |
| Escudo | `X` (segurar) | `LT` (segurar) | `L2` (segurar) |
| Arco | `C` | `RT` | `R2` |
| Interagir | `E` | `Y` | `△` |
| Pausar | `Esc` | `Menu` | `Options` |
| Avançar diálogo | `Espaço`/`Z` | `A` | `✕` |
| Pular diálogo / voltar | `Esc` | `B` | `✕`→`B` |

**Um caminho por ação, não um por console.** As ações apontam para `<Gamepad>/...`, que é a camada abstrata do Input System: `buttonSouth` é `A` no Xbox e `✕` no PlayStation, `buttonWest` é `X` e `□`. Não existe `XboxPlayerController` nem `PS5PlayerController` — existe o mesmo `PlayerController` de sempre, lendo as mesmas `InputAction`. O gamepad entrou como mais um caminho de binding, não como um segundo caminho de input.

**O analógico tem deadzone própria.** Ligar direto em `leftStick/x` pula o processador de deadzone do stick, então a deadzone vem no binding (`axisDeadzone(min=0.15, max=0.95)`). Sem isso, o desgaste normal de um controle usado faz Odisseu andar sozinho. O D-Pad entra como um composto 1DAxis separado, digital, igual às setas do teclado.

**A UI mostra o botão do dispositivo em uso, não do que está conectado.** `InputDeviceTracker` guarda o ÚLTIMO dispositivo usado — quem joga de teclado com um controle plugado na mesa continua vendo teclas até encostar no controle, e volta a ver teclas ao tocar numa tecla. As dicas em tela se reescrevem na troca (`InputDeviceTracker.Changed`), então pegar o controle no meio do treino muda "Pressione Z" para "Pressione X" na hora. Isso é só apresentação: nenhum comando passa pelo tracker.

**Xbox ou PlayStation, resolvido uma vez por controle.** No Editor e em builds nativos o Input System entrega `XInputController` ou `DualShockGamepad` e a resposta é direta. No WebGL não: o navegador entrega um gamepad genérico pela Gamepad API, e o que sobra é o texto de identificação — daí a busca por nome e pelos códigos de fabricante (`045e` Microsoft, `054c` Sony), em todos os campos da descrição, porque o texto útil aparece em campos diferentes conforme o navegador.

A resposta é **guardada por dispositivo**, e `Generic` significa "não sei", não "mudou": havendo já uma resposta concreta, uma leitura genérica não a substitui. Isso não é detalhe — recalcular a família a cada frame fazia o rótulo piscar entre `Y` e `Y/△` no meio de uma conversa, porque bastava um frame de leitura ambígua. Trocar conhecimento por ignorância era a causa da inconsistência. Não identificou nada, a UI mostra as duas grafias (`A/✕`, `X/□`) em vez de arriscar a errada; o jogo nunca deixa de funcionar por não reconhecer o modelo.

**Vibração é acessório.** `HapticFeedback` vibra ao levar dano e ao aparar com o escudo — não ao atacar, que sai a cada 0,4 s e viraria chocalho. Sem controle, com a opção desligada (Configurações → Vibração do controle) ou num navegador sem suporte, as chamadas não fazem nada e o jogo segue igual. `PlayerHaptics` é um ouvinte dos eventos que o combate já dispara: nenhuma linha de espada, escudo, arco ou dano mudou para isso existir.

**Mobile não muda.** Os botões virtuais continuam decididos por `MobilePlatformDetector`, que olha plataforma e dispositivo — não gamepad. Conectar um controle no celular não some com os botões, e no desktop eles continuam ocultos.

**No navegador, o controle só aparece depois de um gesto.** A Gamepad API não expõe nada até a página receber uma interação e um botão do controle ser apertado — é regra do navegador, não do jogo. Na prática: clique no canvas uma vez e aperte qualquer botão do controle.

### Remapeamento de teclas

Todas as teclas acima são configuráveis em **Menu principal → Opções → Controles** ou, durante o jogo, em **Pause → Controles**. Clique na tecla de uma ação e pressione a nova; `Esc` cancela a captura.

- **Persistência**: os overrides ficam em PlayerPrefs sob a chave `Odisseia.Bindings`, **separada do save da campanha**. "Novo Jogo" apaga o progresso mas preserva os controles.
- **Conflitos são recusados**: escolher uma tecla já usada por outra ação desfaz a troca e explica quem já usa aquela tecla, em vez de deixar duas ações no mesmo botão.
- **Restaurar padrões** devolve tudo ao original.
- **`Esc` (Pausar) não é remapeável**, de propósito: a mesma tecla também pula diálogo no action map `Dialogue`, e remapear só um dos dois deixaria os dois fora de sincronia.
- Os **botões de toque acompanham o remapeamento** — cada `OnScreenButton` é reapontado para a tecla atual da ação, então o mobile nunca fica mandando a tecla antiga.

Implementado sobre os *binding overrides* do próprio Input System (`KeyRebindService`); não existe camada de input paralela — as ações continuam as mesmas, só o caminho do controle muda.

Definidos em `Assets/ScriptableObjects/PlayerControls.inputactions` (Unity Input System), em dois action maps: `Player` (Move/Jump/Attack/Shield/Bow/Interact/Pause) e `Dialogue` (Advance/Skip). O mapa `Dialogue` fica ativo só durante falas, e o `Player` é desligado nesses momentos — por isso avançar o diálogo não faz Odisseu pular junto.

**Teclado, controle e toque alimentam a mesma camada de input.** Cada botão virtual é um `OnScreenButton` do próprio Input System apontando para o mesmo control path de teclado da ação (`DEF` → `<Keyboard>/k`). Não existe lógica de gameplay duplicada entre as duas plataformas — `PlayerCombat`, `PlayerShield` e `PlayerBow` só conhecem a `InputAction`.

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
            ParallaxLayer, MovingPlatform, DecisionFlags, ControlHints,
            InputDeviceTracker, HapticFeedback

Player/     Componentes de Odisseu (um por capacidade)
            PlayerController, PlayerCombat, PlayerShield, PlayerBow,
            PlayerRespawn, PlayerInputLock, PlayerAnimator, PlayerHaptics,
            + efeitos por fase: LotusEffect, WindBagAbility, TransformationEffect,
              SirenResistance, HungerMeter, DisguiseEffect

Combat/     Projéteis
            Arrow

Enemies/    EnemyController (patrulha/perseguição), BossController (telegrafado)

Levels/     Peças que compõem uma fase
            LevelManager, LevelIntro, LevelGoal, KillZone, TutorialTrigger,
            DialogueTrigger, NPCDialogue, LevelObjective, InteractPoint,
            TrainingCourse, TrainingTarget, ShipDeparture,
            ProgressionGate, LevelBounds
            + hazards/zonas específicos por fase

UI/         Telas e HUD
            HUD, ObjectiveBanner, DialogueSequence, PauseMenu, DeathOverlay, ScreenFader,
            MainMenuController, LevelSelectController, LevelCompleteMenu,
            EndingController, SettingsPanel, UITheme + indicadores
```

### Combate

Odisseu tem três formas de combate que coexistem, sem "modo" e sem troca de equipamento — dá para alternar livremente entre `J`, `K` e `L` no mesmo segundo.

| | Componente | Comportamento | Valores iniciais |
|---|---|---|---|
| ⚔ **Espada** | `PlayerCombat` | Área circular no `AttackPoint`, com cooldown | dano 20, cooldown 0,4 s |
| 🛡 **Escudo** | `PlayerShield` | Reduz dano no arco frontal enquanto o botão está pressionado | 80% de redução, 120° de cobertura |
| 🏹 **Arco** | `PlayerBow` + `Arrow` | Dispara uma flecha física; munição limitada | dano 35, velocidade 15, 10 flechas, cooldown 0,5 s |

**Como o escudo sabe de onde vem o golpe.** `HealthSystem.TakeDamage(int)` sozinho não diz nada sobre direção. Em vez de criar um sistema de dano paralelo, o golpe agora pode carregar um `DamageInfo` (origem + se é bloqueável), e o `HealthSystem` procura um `IDamageMitigator` no próprio GameObject antes de aplicar o dano:

```
Inimigo/Boss ── TakeDamage(dano, DamageInfo) ──> HealthSystem
                                                     │
                                            IDamageMitigator?
                                                     │
                                               PlayerShield
                                          (frontal? bloqueável?)
```

Quem não tem escudo — todos os inimigos — não tem mitigador e se comporta exatamente como antes. A sobrecarga `TakeDamage(int)` continua existindo e representa **dano ambiental**: poço, afogamento, canto das sereias. Sem origem não há direção, então esse dano nunca é bloqueável — que é o comportamento desejado e evita ter que alterar `KillZone`, `TidalHazard` e companhia.

**Ataques bloqueáveis.** `EnemyController` e `BossController` têm um campo `attackBlockable` (ligado por padrão). Desligá-lo é o caminho pronto para um ataque especial que fura a defesa — a estrutura existe, nenhum ataque não bloqueável foi criado ainda.

**Munição.** Odisseu começa com 10 flechas de um máximo de 10. Cada disparo gasta uma; sem munição o disparo não sai e o contador da HUD pisca em vermelho. O prefab `ArrowPickup` repõe 5 flechas e **não é consumido se a aljava já estiver cheia**, para o jogador não desperdiçar o item.

**Regras de compatibilidade.** Defendendo, não se ataca nem se atira (o escudo ocupa as mãos). Soltar `K` devolve o controle imediatamente. O escudo se desliga sozinho no `OnDisable`, então cutscene, diálogo ou morte nunca deixam Odisseu travado em defesa.

Preparado para receber depois, sem reestruturação: stamina, durabilidade do escudo, parry, shield bash, trajetória balística, carregamento do arco e tipos de flecha.

### Vidas, experiência e fim de jogo

Odisseu começa a jornada com **3 vidas**.

| Evento | O que acontece |
|---|---|
| Morre com vidas restantes | Perde uma vida e volta ao último checkpoint (regra de respawn inalterada) |
| Morre sem vidas | **Fim da jornada** — tela de fim de jogo e volta ao menu principal |
| Derrota um inimigo | **+25 XP**; a cada **125 XP** (5 inimigos) ganha uma vida |

**O progresso da campanha é preservado no fim de jogo.** As fases já conquistadas continuam desbloqueadas no Seletor de Fases — perder a jornada custa a jornada, não o que já foi conquistado.

**Uma vida é consumida em um lugar só.** `PlayerRespawn` é o único ponto que chama `LivesCounter.LoseLife()`; se dois lugares descontassem, uma morte custaria duas.

**`DeathOverlay` escuta `LivesCounter.LifeLost`, não `HealthSystem.Died`.** Na última morte não existe checkpoint para onde voltar, e o aviso "retornando ao checkpoint" não deve competir com a tela de fim de jogo. Como o evento só dispara quando ainda sobra vida, os dois casos nunca se cruzam — e o comportamento não depende da ordem em que os componentes recebem o `Died`.

**As vidas não são zeradas por fase.** `LevelIntro` reseta checkpoint e coletáveis a cada fase; as vidas ficam de fora de propósito, porque uma jornada atravessa as 16. Quem reinicia a contagem é o `MainMenuController`, no carregamento do menu — isso cobre Novo Jogo, Continuar, Seletor de Fases e o retorno após um fim de jogo num lugar só.

**XP não vem de boss.** `BossController` é um perigo ambiental telegrafado, sem `HealthSystem` — não morre, então não premia. A experiência vem dos inimigos comuns (`EnemyController`), onde o combate é a mecânica.

Valores em `EnemyController.experienceReward`, `ExperienceCounter.ExperiencePerLife` e `LivesCounter.DefaultStartingLives`.

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
│   ├── Levels/          Level_01_Itaca_Prologue .. Level_16_Final
│   └── _ForaDaCampanha/ Lotófagos e Feácios (fora da campanha)
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
| 1 | Ítaca — O Chamado | **Prólogo, tutorial completo e preparação da expedição**: 6 atos, da convocação de Agamenon à partida para Troia (ver abaixo) |
| 2 | Troia | A guerra e o cavalo de madeira; ao fim, começa a viagem de volta |
| 3 | Cícones | Sem mecânica nova — pequenas arenas de combate e obstáculos |
| 4 | Cítera | **Tempestade no mar**: ondas que sobem e descem, vento, redemoinho e raios telegrafados |
| 5 | Ciclopes | **Polifemo** (mini-boss): ataques telegrafados; a saída é esquivar e correr, não matar |
| 6 | Éolo | **Vento**: correntes que empurram, plataforma móvel, saco dos ventos (`E`) para desobstruir |
| 7 | Lestrígones | **Fuga**: ameaça que persegue por trás + gigantes arremessando pedras |
| 8 | Circe | **Transformação**: zonas mágicas desativam o combate temporariamente; erva de moly cura |
| 9 | Mundo dos Mortos | Atmosfera e narrativa: ruínas, névoa, falas no meio da fase |
| 10 | Sereias | **Resistência sonora**: barra que drena na zona de influência; o mastro dá imunidade |
| 11 | Cila e Caríbdis | **Sobrevivência**: ondas, redemoinho e Cila como ameaças ambientais, sem combate |
| 12 | Gado do Sol | **Recurso + tempo + decisão**: fome drena; comer o gado sagrado resolve, mas cobra o preço |
| 13 | Calipso | Fase tranquila: exploração e narrativa, sem inimigos |
| 14 | Ítaca — O Retorno | **Disfarce**: guardas não detectam Odisseu disfarçado; stealth leve e reencontros |
| 15 | Pretendentes | **Combate em grupo**: o confronto no salão, com Penélope e Telêmaco |
| 16 | Final | **Clímax**: atravessar as argolas na ordem para revelar o objetivo → tela final |

Progressão: concluir uma fase desbloqueia a próxima (`CampaignManager`), salva coletáveis e pontuação. A Fase 16 é a única que carrega `Ending`; as demais voltam ao **mapa da jornada**.

A ordem acima é a **ordem oficial da campanha** e não deve ser alterada sem instrução explícita. Lotófagos e Feácios ficaram **fora** da campanha: as cenas continuam no repositório, em `Assets/Scenes/_ForaDaCampanha/`, mas não estão no Build Settings nem na lista do `CampaignManager`.

### Fase 1 — Ítaca, o prólogo

A primeira fase é a mais longa da campanha (10–15 min na primeira vez) porque acumula quatro papéis: prólogo narrativo, apresentação dos personagens, tutorial completo de combate e preparação da expedição. Ela vai da convocação de Agamenon até os navios deixarem o porto.

```
Exploração de Ítaca
        ↓
Arauto de Agamenon  →  conversa com Mentor
        ↓
Penélope  →  Telêmaco  →  "Eu voltarei para Ítaca"
        ↓
Recrutamento (10 homens de Ítaca)
        ↓
Treinamento ── espada ─ pulo ─ escudo ─ arco ─ desafio final
        ↓
Arsenal (armas e equipamentos)  →  Porto (suprimentos, remos, velas, tripulação)
        ↓
Última conversa com Penélope  →  despedida de Telêmaco
        ↓
Embarque  →  partida  →  WorldMap (Troia desbloqueada)
```

**Os atos são dados, não código.** Cada ato é um `LevelObjective` na cena: texto da faixa de objetivo, quantas contribuições exige, o que liga ao começar, o que desliga ao terminar, a fala de encerramento e qual é o próximo. Os atos formam uma lista encadeada — não existe um "director" com o roteiro escrito em C#. Quem contribui (NPC, ponto de interação, o curso de treino) chama `Report`, e o objetivo ignora relatos repetidos, então conversar duas vezes com o mesmo pescador não recruta dois homens.

**Os portões existem para o roteiro não sair da ordem.** Cinco portões fechados (salão, recrutamento, treino, arsenal, porto) são desligados ao fim do ato anterior. Sem eles o jogador podia falar com Penélope antes de o arauto chegar, e a promessa aconteceria antes da convocação. Uma vez abertos, ficam abertos: dá para voltar atrás a qualquer momento.

**O tutorial é o próprio jogo se reportando.** O `TrainingCourse` não reimplementa combate: ele escuta `PlayerCombat.Attacked`, `PlayerShield.Blocked`, `PlayerBow.Fired` e a ação `Jump`, e os alvos (`TrainingTarget`) avisam quando levam dano pelo `HealthSystem` de sempre. Por isso o tutorial não tem como divergir do jogo.

- **Uma etapa por vez**, e não dá para pular sem executar — mas a etapa não tem tempo limite, aceita qualquer ordem de golpes e sobrevive a morrer e voltar do checkpoint.
- **As instruções mudam com a plataforma.** `ControlHints` monta a dica com a tecla atual (já considerando remapeamento) no desktop e com o rótulo do botão de toque no mobile. Cada dica tem uma variante `.mobile` na tabela de idiomas, para "Pressione Z" virar "Toque em ATK" e não só trocar a letra.
- **As etapas seguem o terreno, e quem precisa do jogador vai até ele.** A etapa do escudo não fecha ao levantar o escudo — fecha ao BLOQUEAR um golpe. Isso exige alguém batendo, e por isso o parceiro de treino fica logo depois dos bonecos e enxerga 30 unidades em vez das 4 do inimigo comum: ele procura o rei, não o contrário. Sem isso, quem terminava os pulos alguns metros antes segurava o escudo num campo vazio, com o soldado fora da tela, e a etapa não fechava nunca. `PrologueProbe` reprova parceiro de defesa com alcance curto, ou que desista antes do alcance em que enxerga.
- **Odisseu começa o prólogo sem disfarce, e isso é verificado.** A instância do Player desta cena vinha com `startDisguised` ligado e layer 12 — mecânica da Fase 14, herdada de uma cópia daquela cena (as duas se passam em Ítaca). O disfarce funciona exatamente como anunciado: tira o jogador da layer que os `EnemyController` procuram. Com ele ligado, **nenhum inimigo do prólogo enxergava Odisseu** — perseguiam alguém que, para a física, não estava lá. Nada aparecia como erro: o combate simplesmente não acontecia, e a etapa do escudo, que só fecha ao bloquear um golpe, era impossível por construção. `PrologueSceneBuilder` normaliza o Player ao montar, e `PrologueProbe.ConferirJogador()` reprova jogador disfarçado ou qualquer inimigo cuja máscara não inclua a layer dele.
- **Golpe que passa pelo escudo erguido é explicado, não ignorado.** A etapa só conta o golpe APARADO, e aparar exige estar de frente — o escudo cobre 120°. Como o parceiro troca de lado enquanto persegue, parte dos golpes chega pelas costas: o jogador segura o botão, apanha, e o contador não anda. Sem explicação, isso é indistinguível de bug, e foi assim que pareceu. O curso compara o frame do último bloqueio com o do dano recebido; se levou dano com o escudo erguido e não aparou, avisa que veio pelas costas. O parceiro também trota mais rápido (3,5) e insiste mais (0,6 s entre golpes) — ele existe para bater, não para fazer esperar.
- **O parceiro é desligado quando a etapa dele acaba**, senão continua perseguindo Odisseu durante a etapa do arco.
- **A aljava se enche sozinha nas etapas do arco.** Sem isso, dez flechas erradas deixariam a etapa de acertar alvos impossível de cumprir. Fora do treino a munição continua sendo recurso.

**Nenhum buraco antes de o pulo ser ensinado.** O chão é contínuo do começo ao cais, e as plataformas do treino ficam *acima* de chão sólido — quem erra o pulo cai de volta no campo e tenta de novo. Isso não é generosidade: o pulo só é apresentado no Ato 4, e o caminho do recrutamento vem antes. Um vão de 3,5 unidades ali (o alcance máximo é 4,89, com corrida) cobrava uma habilidade que a fase ainda não tinha ensinado, e três quedas encerravam a jornada no menu principal — a fase ficava impossível de terminar sem nada parecer quebrado. `PrologueProbe.ConferirChao()` agora percorre os colisores de piso e reprova qualquer buraco antes do treino.

**O fundo desta fase é chapado, e isso é escolha.** As telas pintadas em `Art/Odisseia/Backgrounds/` são arte final; o resto do prólogo ainda é retângulo colorido. Juntas na mesma tela, as duas coisas fazem o cenário parecer quebrado — paisagem em detalhe atrás, caixote bege liso na frente. Enquanto a fase for placeholder, o fundo também é: céu chapado, faixa de mar no horizonte e duas camadas de morros feitas de blocos soltos. Blocos soltos em vez de uma imagem repetida resolvem por construção o problema que existia antes — as imagens de fundo não são contínuas nas bordas, e cada repetição virava uma emenda vertical no meio da paisagem. Sem borda para casar, o parallax pode voltar a ser forte (0,55 e 0,35), que é o que dá profundidade de verdade. Para trazer a arte pintada de volta, `PrologueSceneBuilder.UsarFundoPintado = true` — as três camadas já estão posicionadas e sem emenda.
**A caixa de dica tem dono.** Ela é uma só e vários componentes escrevem nela — NPCs, pontos de interação, portões, o curso de treino. Sem dono, qualquer um apaga a mensagem de qualquer outro, e áreas de gatilho que se encostam viram um piscar: o jogador entra no alcance da Penélope (a dica dela aparece) e meio metro depois sai da área de aviso do portão do salão, que apaga a dica **dela**. Andando na borda entre as duas, pisca sem parar. `TutorialPrompt.Hide(dono)` só apaga para quem escreveu; `Show`/`ShowPersistent` assumem a caixa. Isso resolve a classe inteira, não as três sobreposições que existem hoje (`Gate_Hall`×`NPC_Penelope`, `Gate_Arsenal`×`NPC_Blacksmith`, `Recruit_Elpenor`×`Gate_Training`).
**A dica de interação fica na tela enquanto o jogador estiver perto.** Ela piscava por dois segundos e sumia, mesmo com Odisseu parado ao lado de Penélope. Quem passasse correndo nunca via que ali dava para conversar — e como Penélope fica antes do portão seguinte, o jogador batia numa parede com o objetivo mandando falar com quem tinha ficado para trás. A fase estava certa e parecia travada.

**O portão pergunta ao objetivo; o objetivo não conhece portão nenhum.** `ProgressionGate` recebe um `LevelObjective` no Inspector e reavalia a condição em três momentos: ao iniciar, quando o objetivo avisa que fechou, e **quando o jogador encosta nele**. O terceiro é o que garante a recuperação — antes, quem abria a barreira era o objetivo (uma lista de objetos para desligar), e um único aviso perdido deixava o portão trancado com o ato já concluído. Isso é softlock: a fase segue "correta" e o jogador fica entre uma parede e um objetivo que não existe mais. Agora chegar no portão o destrava. A raiz do portão nunca é desligada, só o colisor e a folha da porta — desligar tudo tiraria de cena justamente o componente que sabe reavaliar. O portão também explica o que falta e para onde ir, porque parede muda é indistinguível de bug.

**A fase tem limites de mundo, não de tela.** `LevelBounds` guarda a área jogável em unidades de mundo e cuida de três camadas que não se substituem: paredes nas pontas (o jogador simplesmente para), enquadramento da câmera (ela não mostra o lado de fora) e uma rede de segurança que devolve ao último checkpoint quem escapar mesmo assim. Nada disso deriva de `Screen.width`: o jogo roda em navegador de desktop e de celular, e um limite em pixels daria um tamanho de fase diferente para cada aparelho. A rede de segurança é a última linha, não a primeira — se ela disparar em jogo normal, o buraco está nas paredes.

**A garantia contra softlock é uma checagem, não uma promessa.** `PrologueProbe.ConferirAlcance()` calcula, para cada ato, até onde o jogador consegue andar enquanto ele está aberto (o primeiro portão que ainda não abriu) e reprova se algum alvo daquele ato ficou do outro lado. Um objetivo cujo alvo está atrás da barreira que só abre quando ele fechar é um beco sem saída perfeito, e é invisível no Inspector.
**Figura em pé é apoiada pelo `bounds` do sprite, não por "meia altura acima do chão".** A arte de personagem do projeto é importada com pivô **BottomCenter** e o quadrado placeholder com pivô **central**. A conta fixa acertava o quadrado e deixava todo NPC pintado flutuando exatamente meia altura no ar. Usar `sprite.bounds.min.y` faz a mesma linha servir para qualquer pivô, inclusive o que a arte final escolher. As áreas de gatilho ganharam `offset` pelo mesmo motivo: com a raiz onde o pivô manda, `y = 0` deixou de ser o meio do personagem. `PrologueProbe.ConferirApoio()` reprova qualquer figura fora da linha do chão — é uma falha que nenhuma checagem de roteiro pega, porque a fase continua jogável; ela só fica errada de olhar.
**A partida.** `ShipDeparture` troca Odisseu por uma silhueta no convés antes de o navio zarpar — um Rigidbody dinâmico travado em cima de um objeto que se move por transform briga com a física. Quem encerra a fase continua sendo o `LevelGoal`, chamado no fim da travessia: conclusão, save e volta ao mapa da jornada seguem num lugar só.

**Penélope se despede no porto, não no palácio.** A última conversa acontece no cais porque é de lá que ela assiste à partida — e porque voltar ao palácio seria refazer 300 unidades de fase a pé.

**A cena é montada por script.** `Odisseia > Montar prologo de Itaca` (ou `-executeMethod PrologueSceneBuilder.Build`) reconstrói a fase inteira do zero, preservando o que é infraestrutura (HUD, pause, morte, câmera, Player, LevelGoal). `Odisseia > Conferir prologo de Itaca` (`PrologueProbe.Run`) confere depois que nenhum ato ficou impossível de fechar — ele conta os reportadores realmente ligados a cada objetivo e compara com o que ele exige, além de verificar que toda fala tem texto nos dois idiomas.


### Mapa da jornada (`WorldMap`)

Uma camada de progressão no estilo do mapa de mundo do Super Mario World. Odisseu caminha por uma trilha linear entre as 16 fases.

```
Menu ──► WorldMap ──► Fase ──► WorldMap ──► próxima fase desbloqueada
```

| Componente | Papel |
|---|---|
| `WorldMapPath` | A polilinha do caminho. Pontos editáveis no Inspector; alguns marcados como parada de fase |
| `LevelNode` | Uma fase no mapa. Só dados + estado; não conhece cor nem sprite |
| `LevelNodeView` | Aparência por estado (`Locked` / `Available` / `Current` / `Completed`) |
| `WorldMapPlayerController` | Caminhada presa ao caminho |
| `WorldMapManager` | Monta o mapa do save, posiciona Odisseu, trata a entrada nas fases |
| `WorldMapUI` | Título, painel da fase, aviso de bloqueio e anúncio de desbloqueio |

**Movimento por distância, não por posição.** Odisseu guarda um `float` de quanto já andou na trilha, e a posição sai de `path.Evaluate(distância)`. Isso resolve de graça dois requisitos: não sair do caminho (não existe eixo livre para sair) e não atravessar uma fase bloqueada (basta limitar a distância máxima à do último nó desbloqueado). Nenhum collider envolvido.

**Ao concluir uma fase, Odisseu anda sozinho até a próxima.** Ele chega ao mapa em cima do nó que acabou de vencer, o anúncio de desbloqueio aparece, e então `WorldMapManager.TravelToUnlocked` o leva caminhando até a parada recém-liberada. Sem isso ele fica parado no nó já concluído e quem apertar "entrar" cai de volta na **mesma fase** — o jogo parece não ter avançado. Durante a caminhada o `WorldMapPlayerController` fica com `InputSuspended`, para jogador e mapa não disputarem a mesma distância no mesmo frame.

**O mapa não guarda progresso.** Quem sabe o que está desbloqueado é o `CampaignManager`, apoiado no `SaveSystem`. `WorldMapManager` só traduz isso em estado de nó e limite de caminhada, num método só — mapa e save não têm como discordar.

**`WorldMapSession` é recado, não save.** Guarda em memória qual fase o jogador acabou de jogar, para o mapa saber onde colocar Odisseu na volta. Derivar isso do save daria sempre a fase concluída de maior ordem, o que colocaria Odisseu no lugar errado ao rejogar uma fase antiga.

**Nós e pontos intermediários são coisas diferentes.** O caminho tem 46 pontos: 16 paradas de fase e 30 intermediários. Os intermediários existem para o mapa poder ganhar curvas, ilhas e desvios depois, sem mexer no controlador.

**Placeholders, por enquanto.** Nós e trilha são quadrados coloridos vindos de `Assets/Prefabs/WorldMap/`. Cor, escala e marca de concluído são campos serializados no `LevelNodeView` — trocar por arte definitiva é trocar o prefab, sem tocar em lógica.

No mobile, o mapa mostra só as setas e o botão **JOGAR**; espada, escudo, arco e pulo ficam escondidos, porque pertencem ao gameplay. É o mesmo `MobileControlsRoot`, alternando o conjunto visível por contexto de cena.

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
| Fundos de parallax | 9 PNGs, ~4 MB de origem | Passaram a entrar no build quando os `.meta` foram corrigidos (ver *Troubleshooting*). São a maior parte do crescimento do arquivo de dados — o primeiro lugar a olhar se o tamanho incomodar |

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

Três workflows, para dois caminhos diferentes.

| Workflow | Precisa de licença Unity? | Quando usar |
|---|---|---|
| `activation.yml` | Não | **Uma vez**, para obter o arquivo de licença |
| `build-web.yml` | **Sim** | Caminho padrão: build automático a cada push na `main` |
| `deploy-pages-manual.yml` | Não | Publicar agora, com um build feito localmente |

### Pré-requisito comum

Habilite **Settings → Pages → Source: GitHub Actions**. Sem isso, o deploy falha mesmo com o build funcionando.

### Caminho A — build automático (recomendado)

Exige configurar a licença Unity uma única vez:

1. **Actions → "Acquire Unity Activation File" → Run workflow**
2. Baixe o artefato `.alf` gerado
3. Envie em [license.unity3d.com/manual](https://license.unity3d.com/manual), escolhendo *Unity Personal Edition*
4. Baixe o `.ulf` que o site devolve
5. Abra o `.ulf` num editor de texto e copie **todo** o conteúdo
6. **Settings → Secrets and variables → Actions → New repository secret**

| Secret | Valor |
|---|---|
| `UNITY_LICENSE` | Conteúdo completo do arquivo `.ulf` |
| `UNITY_EMAIL` | E-mail da sua conta Unity |
| `UNITY_PASSWORD` | Senha da sua conta Unity |

Para licença **Pro**, use `UNITY_SERIAL` no lugar de `UNITY_LICENSE`.

> ⚠️ O `.ulf` é **sensível** — nunca o comite no repositório. O `.alf` não é sensível (é só um pedido de ativação).

### Caminho B — publicar agora, sem licença

```bash
Unity.exe -batchmode -quit -projectPath . -executeMethod BuildScript.BuildWebGL
```

```bash
git add -f Builds/WebGL && git commit -m "chore: build web" && git push
```

Depois: **Actions → "Deploy Pages (build manual)" → Run workflow**.

O `-f` é necessário porque o `.gitignore` ignora `Builds/`. Esse caminho versiona ~6 MB de binário — aceitável para portfólio, mas é por isso que não é o padrão.

---

## 11. Troubleshooting

### CI / GitHub Pages

| Sintoma | Causa | Solução |
|---|---|---|
| `Missing Unity License File and no Serial was found` | Secret `UNITY_LICENSE` não configurado | Siga o **Caminho A** da seção 10. Ou use o **Caminho B** para publicar sem licença |
| `Get Pages site failed` (`Not Found`) seguido de `Create Pages site failed` (`Resource not accessible by integration`) | Pages nunca foi habilitado neste repositório. Criar o site pela primeira vez via API é uma operação restrita que o token do workflow não tem permissão para fazer — só funciona feita manualmente, uma vez, pela interface | Settings → Pages → *Build and deployment* → Source: **GitHub Actions**. Depois rode o workflow de novo — os workflows já verificam isso antes de tentar publicar e explicam o mesmo passo se faltar |
| Deploy roda mas o site fica 404 | Primeiro deploy ainda propagando | Aguarde 1–2 min; confira a URL em Settings → Pages |
| Página abre em branco no Pages, sem erro | Jekyll processou o build | Os workflows criam `.nojekyll` automaticamente — se publicou à mão, crie o arquivo na raiz do build |
| `Another deployment is in progress` | Dois workflows concorrendo | Já mitigado pelo `concurrency` nos workflows; aguarde o anterior terminar |

### Runtime

| Sintoma | Causa provável | Solução |
|---|---|---|
| Tela preta ao abrir o build | Aberto via `file://` | Sirva por HTTP (ver seção 9) |
| Erro de `Content-Encoding` no console | Servidor não envia header de Brotli | **Não afeta o Pages**: o build usa `decompressionFallback`, então o loader descomprime em JS. Ver nota abaixo |
| Botões não respondem | Cena sem `EventSystem` + `InputSystemUIInputModule` | O projeto usa só o Input System novo; o módulo legado não capta cliques |
| Nada é renderizado na cena | Câmera com `z = 0`, no mesmo plano dos sprites | Câmera 2D deve ficar em `z = -10` |
| Progresso não salva | Fase aberta direto, sem passar pelo Boot | Comece por `Boot.unity` — o `CampaignManager` vive lá |
| `Another Unity instance is running` no build CLI | Editor aberto no mesmo projeto | Feche o Editor antes de rodar o build por linha de comando |
| Áudio mudo no navegador | Política de autoplay do browser | Clique na página uma vez; o áudio começa após a primeira interação |
| Fase parece longa/curta demais | Ritmo nunca foi medido em playtest | Ver nota abaixo |
| Sprite some da cena e o Inspector mostra "None (Sprite)" | `.meta` malformado: uma chave YAML com `[]` na linha de baixo, na mesma indentação, faz o Unity **recusar o arquivo inteiro** e ignorar a textura em silêncio | Junte na mesma linha (`sprites: []`). Foi o que aconteceu com as 9 imagens de `Art/Odisseia/Backgrounds/`: estavam no repositório mas nenhuma cena conseguia usá-las |

**Nota sobre compressão e GitHub Pages**: o build usa Brotli **com `decompressionFallback` ativado** (`webGLCompressionFormat: 0` + `webGLDecompressionFallback: 1`). Isso gera arquivos `.unityweb` que o próprio loader descomprime em JavaScript, sem depender de o servidor mandar `Content-Encoding: br` — que é justamente o que o GitHub Pages **não** faz. Por isso o build funciona no Pages sem configuração extra. Foi verificado servindo o build por um HTTP server sem headers especiais (o mesmo cenário do Pages): carregou completo, sem erro no console.

**Nota honesta sobre balanceamento**: as durações (3-7 min) e as dificuldades foram dimensionadas pela matemática de pulo/velocidade, **não medidas em playtest real**. Os números que mais provavelmente vão precisar de ajuste depois de você jogar: velocidade do perseguidor (Fase 6), dreno da resistência às sereias (Fase 9), drenagem da fome (Fase 11) e a dificuldade do combate em grupo (Fase 15).

---

## 12. Verificação automatizada

Duas ferramentas de Editor, ambas rodáveis por linha de comando. Nenhuma entra no build.

| Ferramenta | O que responde |
|---|---|
| `CampaignValidation.Run` | Estrutura: as 16 etapas estão na ordem oficial no `CampaignManager` e no Build Settings? Cada cena abre com o `levelId` certo, `LevelGoal` e Player? A migração de save preserva o progresso? **E a geometria: alguma área letal engloba o ponto onde Odisseu nasce, ou cruza o chão?** |
| `CampaignProbe.Run` | Runtime: roda a campanha **em play mode** e reporta o que o jogador teria — o que ficou desbloqueado ao concluir uma fase, em que nó Odisseu para no mapa e para onde o botão de entrar levaria. |

```bash
Unity.exe -batchmode -quit -projectPath . -executeMethod CampaignValidation.Run -logFile val.log
```

```bash
Unity.exe -batchmode -projectPath . -executeMethod CampaignProbe.Run -logFile probe.log
```

A sonda **não** leva `-quit` (ela encerra sozinha) e **apaga o save do Editor** para partir de um jogo novo — o save do build Web, que vive no localStorage do navegador, não é tocado.

Duas armadilhas embutidas nela, caso precise mexer: entrar em play mode dispara *domain reload*, que zera os estáticos e cancela a inscrição em `EditorApplication.update` (por isso o reload é desligado durante a execução e restaurado no fim); e em batchmode o `EditorApplication.update` dispara muito mais rápido que os frames do jogo, então esperas precisam ser em tempo de parede, não em contagem de ticks.

---

## 13. Estado atual

- ✅ Compila sem erros
- ✅ 16 fases jogáveis + menus + tela final (21 cenas em Build Settings)
- ✅ Save/load validado por teste automatizado (round-trip campo a campo)
- ✅ Validação estrutural das 21 cenas: **0 problemas**
- ✅ Build WebGL gerado pela CLI: **9,0 MB**
- ✅ **Build aberto e verificado num navegador real**: engine inicializada, contexto WebGL 2.0 criado, física/Input System/áudio ativos, todos os assets em HTTP 200, barra de carregamento concluída, **zero erros no console**

### Validado jogando (desktop)

- ✅ Arco: disparo, consumo de flecha e contador na HUD
- ✅ Escudo: defesa ativa segurando a tecla, **com e sem escudo o dano sai correto** (frontal reduzido, pelas costas cheio)
- ✅ Menu de opções: remapeamento de teclas, recusa de conflito e persistência

### Pendências conhecidas

- ⏳ **Controles mobile nunca testados em dispositivo real** — os botões `DEF` e `BOW` e o layout 2×2 foram implementados e compilam, mas ninguém tocou neles num celular. Testar abrindo o build por HTTP no celular na mesma rede, ou no Chrome com o emulador de dispositivo (F12 → modo dispositivo → **recarregar**, porque `MobilePlatformDetector` decide no load). Verificar: toque simultâneo de mover + defender, mover + arco, e se os botões não se sobrepõem em telas estreitas.
- ⏳ **Não** passou por playtest humano completo — balanceamento é estimativa e o ritmo das 16 fases não foi medido.
- ⏳ **Ítaca — O Chamado e Cítera ainda são placeholder**: o traçado, o ritmo dos perigos e os tempos dos raios de Cítera foram calculados no papel, não jogados.

### Próximos passos sugeridos

- Validar os controles mobile num dispositivo real (ver Pendências)
- Playtest completo para calibrar dificuldade e ritmo
- Arte definitiva para as fases que ainda usam placeholder geométrico (inclusive o prólogo e Cítera)
- Música e efeitos definitivos (hoje procedurais)
- Diálogos condicionais usando `DecisionFlags` (a decisão da Fase 11 já é registrada, mas nada a consome ainda)
