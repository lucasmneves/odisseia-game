# FULL CAMPAIGN PLAYTEST & INTEGRATION QA

2026-09-30. Primeira passagem: **registro, sem correção** (nenhum bug do jogo foi corrigido; nenhum asset gerado;
nenhum commit). As únicas mudanças no repositório são ferramentas de QA em `Assets/Scripts/Editor/` e este documento.

## 0. Método — o que "jogado" significa aqui

| Camada | O que é | Cobertura |
|---|---|---|
| **Bot de playtest** (`QaPlaytestBot`) | joga em play mode pelo **mesmo input do jogador**: teclado virtual do Input System, as mesmas ações. Nada é teleportado nem marcado por código. Segura direita, pula obstáculo/buraco, aperta Interagir, ataca inimigo à frente, recua quando empaca, solta a direção quando gruda na parede. Registra spawn, câmera, HUD, objetivos, mortes (com causa), travamentos, erros de console e a **transição** (cena seguinte, save, nó do mapa) | **16/16 fases**, 2 rodadas completas + repetições das que falharam |
| **Teste de mecânicas** (`QaMechanicsTest`) | casos reproduzíveis por input real, com medida: melee, cooldown, escudo frente/costas, defesa no ar, arco, sem munição, morte/respawn, pausa, fim de jogo; e a trajetória de pulo contra a plataforma de Calipso | Troia (02) e Calipso (13) |
| **Build WebGL no navegador** | o build real em `localhost`, jogado com teclado | menu, mapa, início da Fase 01, Settings, emulação mobile |
| **Probes estáticas** | `PrologueProbe`, `JumpReachProbe`, `CampaignProbe`, `ColliderNearProbe` | campanha inteira |

Reproduzir: `Unity.exe -batchmode -projectPath . -executeMethod QaPlaytestBot.Run [-qaFrom N -qaTo M]` (relatório em
`Logs/qa_playtest.txt`) · `-executeMethod QaMechanicsTest.Run` (`Logs/qa_mecanicas.txt`) · `QaMechanicsTest.Calipso`.
Logs desta passagem: `Logs/qa_playtest_rodada2.txt`, `Logs/qa_playtest_r3_*.txt`, `Logs/qa_mecanicas_final.txt`.

**Limites honestos:**
- O bot é simples de propósito. **Fase não concluída pelo bot ≠ fase impossível** — cada uma foi investigada (§3).
- **Não testado por falta de hardware:** controle Xbox, DualSense, iOS e Android reais. Mobile só por emulação (retrato).
- O painel do navegador é pequeno; a jogatina no build Web cobriu menus e o começo da Fase 01, não as 16 fases.

## 1. Campanha configurada (Build Settings = CampaignValidation = SaveSystem)

`Boot → MainMenu → WorldMap / LevelSelect`, e as fases:

01 `Level_01_Itaca_Prologue` · 02 `Level_02_Troia` · 03 `Level_03_Cicones` · 04 `Level_04_Citera` · 05 `Level_05_Ciclopes` ·
06 `Level_06_Eolo` · 07 `Level_07_Lestrigoes` · 08 `Level_08_Circe` · 09 `Level_09_MundoDosMortos` · 10 `Level_10_Sereias` ·
11 `Level_11_CilaCaribdis` · 12 `Level_12_GadoDoSol` · 13 `Level_13_Calipso` · 14 `Level_14_Itaca_Return` ·
15 `Level_15_Pretendentes` · 16 `Level_16_Final` → `Ending`. Toda fase sai para o `WorldMap` (a 16, para `Ending`).

## 2. Resultado por fase

| # | Fase | Carrega · spawn · câmera · HUD | Bot concluiu? | Mortes | Transição observada |
|---|---|---|---|---|---|
| 01 | Ítaca | ok | **não** (bot) — ver QA-11 | 0 | não observada pelo bot; lógica 01→02 confirmada pelo `CampaignProbe` |
| 02 | Troia | ok | **sim** (55 s) ×3 | 2 (quedas) | → WorldMap, 03 desbloqueada, mapa no nó 03 ✓ |
| 03 | Cícones | ok | **sim** (17 s) ×2 | 0 | ✓ |
| 04 | Citera | ok | **1 de 3** | 2–3 (quedas no mar) | ✓ (rodada 1) |
| 05 | Ciclopes | ok | **sim** (19 s) ×2 | 0 | ✓ |
| 06 | Éolo | ok | **sim** (26 s) ×2 | 1 | ✓ |
| 07 | Lestrigões | ok | **não — BLOCKER QA-01** (3/3) | 3 no nascimento | — |
| 08 | Circe | ok | **sim** (15 s) ×2 | 0 | ✓ |
| 09 | Mundo dos Mortos | ok | **sim** (18 s) ×2 | 0 | ✓ |
| 10 | Sereias | ok | **1 de 2** (15 s) | 0–3 | ✓ |
| 11 | Cila e Caríbdis | ok | **não** (0 de 2) — QA-02 | 3 sob os golpes | — |
| 12 | Gado do Sol | ok | **sim** (32 s) | 2 (QA-06) | ✓ |
| 13 | Calipso | ok | **não** (bot) — QA-04; passável por pulo duplo (medido) | 0 | — |
| 14 | Ítaca Return | ok | **sim** (19 s) | 0 | ✓ |
| 15 | Pretendentes | ok | **sim** (17 s) | 0 | ✓ → nó 16 |
| 16 | Final | ok | **sim** (15 s) | 0 | → **Ending** ✓ |

Todas as 16 carregam, com jogador, câmera e HUD. Nenhum erro/exceção de console registrado durante as fases.

## 3. Bugs

| ID | Fase | Severidade | Problema | Reprodução | Causa provável | Status |
|----|------|------------|----------|------------|----------------|--------|
| QA-01 | 07 | ~~**BLOCKER**~~ → **corrigido (BUG-001, §9)** | O gigante perseguidor mata o Odisseu **durante a fala de abertura**, antes de o jogador ter controle; o respawn cai de novo dentro da fala. 2 vidas perdidas sem poder agir, a 3ª com margem mínima → fim de jogo em 3–4 s | Entrar na Fase 07. Reproduzido 3/3 (bot, rodadas 1–3): mortes em t = 1 s e 2 s em x = −18 (nascimento), com o colisor do perseguidor em x −21,3..−18,3 | `PursuerHazard` anda (5,5 un/s) desde o 1º quadro, sem esperar a `LevelIntro` (trava 3 s). Nasce em x = −24, a 4,2 un do jogador (−18): chega em 0,8 s. `resetOffsetX` −8 põe o respawn de novo ao alcance durante a trava. Jogador anda 6 un/s: fuga com 0,5 un/s de margem | **corrigido** — BUG-001 |
| QA-02 | 11 | MEDIUM (verificar com humano) | A Cila mata no convés: bot perdeu as 3 vidas em ~8 s nas 2 tentativas | Entrar na Fase 11 e andar direto pelo convés (x 0..10) | Golpes a cada 2,8 s em x = 3, 6, 9 (raio 1,4) cobrem ~8,8 un do convés; exige esperar o golpe e atravessar na janela de ~2,1 s. Possível, mas não validado por humano | aberto |
| QA-03 | todas | MEDIUM | A tela de **fim de jogo só oferece voltar ao menu** — não há "tentar de novo a fase". Congela o tempo e desliga os controles | Perder as 3 vidas (medido no teste de mecânicas) | `GameOverScreen.Show` → só `BackToMenu`. Somado ao QA-01, a Fase 07 vira um loop menu → mapa → fase → fim de jogo | aberto |
| QA-04 | todas (visto em 01, 10, 13) | MEDIUM | O Odisseu **gruda na lateral** de plataformas no ar enquanto a direção é segurada — não sobe nem cai | Calipso: correr para a direita contra a `Platform_Grove` (x 12) e pular. Medido: preso no ar em x = 11,70 com o pé a −1,05..−1,75. Soltar a direção → cai; pulo duplo → passa (pousa em x = 19) | Colisor do jogador sem `PhysicsMaterial2D` (e sem material padrão no Physics2D): o atrito contra a parede segura o corpo | aberto |
| QA-05 | 01 | LOW | Apertar Interagir logo que uma conversa termina, ainda perto do NPC, **reabre a mesma conversa** | Falar com a Penélope e apertar E repetidamente (a rodada 1 do bot ficou 6 min nisso) | `NPCDialogue` permite conversar de novo ao fim (`talking` volta a falso); só não reabre DURANTE. Sai andando | aberto |
| QA-06 | 12 | MEDIUM | Enquanto a saída espera o vento (25 s), dá para **passar do ponto da saída e cair do fim da fase**, perdendo vidas | Correr direto até o fim: o bot chegou a x = 46 (saída em 34,5..37,5) e caiu; 2 mortes | O `LevelGoal` fica inativo até o `TimeGatedActivator`; não há limite físico depois dele | aberto |
| QA-07 | web | MEDIUM | No desktop o **canvas é fixo em 960 × 600**: janela menor corta o jogo (barras de rolagem); maior não escala | Abrir o build numa janela < 960 px ou > 1080 p | Template padrão `unity-desktop` com tamanho fixo | aberto |
| QA-08 | mobile | LOW (confirmar em aparelho) | Em retrato (emulação) o jogo desenha em ~57% da tela e o aviso "Gire o aparelho…" é cortado nas duas bordas | Emulação Pixel 8 retrato | Aviso de orientação existe; texto sem quebra. A área parcial pode ser artefato de DPR da emulação | aberto |
| QA-09 | UI | LOW | Com o idioma **English**, textos ficam em português: "JOGAR" (mapa), "Vidas" (HUD), "Carregando…" (loading), "Gire o aparelho…" | Jogo novo em inglês (padrão) | Strings fora da tabela de localização | aberto |
| QA-10 | UI | LOW | **Travessão ausente** nos títulos: "Ithaca  The Call", "Ithaca  before the war" (espaço duplo) | Mapa e loading da Fase 01 | Fonte sem o glifo "—" | aberto |
| QA-11 | 01 | LOW (bot) | Bot não concluiu a Fase 01 (preso no portão do salão com Explorar 3/4) | — | Limitação do bot: na rodada 1 (Interagir contínuo) o objetivo chegou a 4/4, Convocação 1/1, Família 1/2. `PrologueProbe` confirma todos os atos fecháveis. **Fase 01 não foi jogada inteira nesta passagem** | registrado |
| QA-12 | 04 | LOW | Citera: quedas no mar entre as plataformas do navio (bot 1 de 3) | Rodadas 1–3 | Dificuldade de plataforma com ondas; concluída na rodada 1 | registrado |
| QA-13 | todas | POLISH | Diálogo disparado **em pleno pulo** deixa o Odisseu **parado no ar** até a fala acabar | Calipso: pular através do `DialogueTrigger_Grove` (x 9–11) — medido: pé em −1,18 por > 1 s | `PlayerInputLock` desliga o `PlayerController` e zera a velocidade; a queda parece depender do controlador | aberto |
| QA-14 | todas | POLISH / FUTURO | **Defesa aérea possui efeito mecânico, mas não possui representação visual** | Pular e segurar X — medido: IsBlocking=True, animador Fall, nenhum escudo desenhado | `Jump`/`Fall` sem escudo; placeholder desligado (N-07) | registrado |
| QA-15 | UI | POLISH | HUD com números sem rótulo (0/100, 0, 10/10, "Vidas 3", XP) sobre o céu, às vezes atrás das nuvens | Qualquer fase | HUD de texto puro | aberto |
| QA-16 | mapa | POLISH | Nós do mapa-múndi são quadrados escuros sem arte | Jogo novo → mapa | Arte do mapa não produzida | aberto |
| QA-17 | web | LOW | Fase 01 leva ~20 s para aparecer (barra + ~10 s de tela preta) | Mapa → JOGAR | Cena grande; tela preta sem indicador depois da barra | registrado |
| QA-18 | web | LOW | Aviso de console: `JS_FileSystem_Sync()` depreciado | Qualquer carga | API de sincronização do `persistentDataPath` | registrado |
| QA-19 | 01 | LOW | `JumpReachProbe` marca 5 superfícies a +5 un (e o píer a +12) como inalcançáveis | Probe | Provável falso positivo (topos de portão/estrutura acima do chão da cidade); o `PrologueProbe` passa | a confirmar |
| QA-20 | 02 | POLISH | Geometria sem arte na Troia (N-08 do Final Polish) | `PlaceholderProbe` | Pré-existente | registrado |

## 4. Combate (medido por input real, Troia)

| Caso | Resultado |
|---|---|
| Melee | 20 de dano por golpe; dois toques em 0,15 s = 1 golpe (cooldown 0,4 s) ✓ |
| Escudo de frente | **40 → 8** de dano em 4 s (redução 80%, = `damageReduction`), animador `Shield` ✓ |
| Escudo de costas | 40 de dano (passa inteiro, como o código define) ✓ |
| Defesa no ar | bloqueia, sem visual (QA-14) |
| Arco | consome 1 flecha; acerta a 4 un (40 → 5) ✓ |
| Sem munição | não dispara, evento `OutOfArrows`, contador fica em 0 ✓ |
| Morte / respawn | −1 vida, volta com 100/100, controle ativo, anda ✓ |
| Pausa | Esc pausa (timeScale 0) e retoma ✓ |
| Fim de jogo | congela, desliga controles, só "menu" (QA-03) |

## 5. Controles, web, mobile

- **Teclado:** mover A/D/setas, pular Espaço, atacar Z, interagir E, escudo X, arco C, agachar S, correr Shift, pausa Esc —
  funcionam no build Web e no bot. Não há ação de mouse no gameplay.
- **Gamepad:** **não testado (sem hardware)**. O asset tem binding de gamepad para todas as ações de gameplay
  (sul/oeste/norte, gatilhos, ombro direito, start, direcional e analógico). Settings tem "Customize" e vibração.
- **Web desktop:** carrega, menus e Settings respondem, detecção `IsMobile=False` correta, controles touch não criados ✓;
  canvas fixo (QA-07); carga lenta da Fase 01 (QA-17). Tela cheia e áudio audível não verificados (sem saída de áudio
  observável; o console registra o contexto de áudio retomado).
- **Web mobile (emulação Pixel 8, retrato):** `IsMobile=True` por user agent + toque, controles touch criados
  (Left/Right/Jump/Attack/Shield/Bow) ✓; aviso para girar ✓ (cortado, QA-08). **Paisagem com user agent mobile não é
  emulável aqui; iOS e Android reais não testados.** A regra "controles só em mobile" se manteve.

## 6. Performance (sintomas, sem benchmark)

Nenhum travamento ou queda perceptível no navegador nos trechos jogados; nenhum erro de memória no console. Carga da
Fase 01 ~20 s (QA-17). Em play mode, todas as fases rodaram a 2× sem erro.

## 7. Validação final

| | |
|---|---|
| `CampaignValidation` | **OK — 16 etapas na ordem oficial**, 0 erros de compilação |
| Build WebGL | **Build Finished, Result: Success** |
| Cenas / settings | nenhuma cena de fase gravada durante o QA; `ProjectSettings` idêntico ao commit (o bot restaura o que altera) |

## 8. TOP 10 correções recomendadas (por impacto)

1. **QA-01** — perseguidor de Lestrigões: começar a andar só depois da `LevelIntro` (ou quando o controle for liberado),
   e reposicionar no respawn fora do alcance durante a trava. Sem isso a campanha não passa da fase 07.
2. **QA-03** — fim de jogo com "tentar de novo a fase" (ou do último checkpoint), além de "menu".
3. **QA-04** — material físico sem atrito no colisor do jogador (atrito 0 na lateral): some o "grudar na parede" em todas as fases.
4. **QA-02** — Cila: validar com humano; se confirmar a dificuldade, alargar a janela entre golpes ou deixar um trecho seguro.
5. **QA-06** — Gado do Sol: limite físico depois da saída (ou saída visível e bloqueada) durante a espera do vento.
6. **QA-07** — template WebGL responsivo (canvas acompanhando a janela, mantendo a proporção).
7. **QA-13** — não suspender a gravidade quando um diálogo trava o jogador no ar.
8. **QA-09 / QA-10** — passar as strings restantes pela localização e usar uma fonte com travessão.
9. **QA-05** — pequeno intervalo antes de permitir reabrir a conversa com o mesmo NPC.
10. **QA-15 / QA-16 / QA-14** — HUD com ícones, arte do mapa-múndi e escudo na defesa aérea (polimento).

**Antes de corrigir:** repetir a Fase 01 inteira à mão (o bot não fecha a 01) e testar gamepad e mobile em aparelho real.

## 9. BUG-001 — Lestrigões — gigante mata durante abertura

**Status: corrigido (local, sem commit).** Correção **só da Fase 07**, por componente de cena + override da cena.
Nenhum sistema global alterado (`PursuerHazard`, `BossController`, `LevelIntro`, `PlayerRespawn`, `HealthSystem`, prefab
`Player` intocados); nenhum asset gerado.

### Causa raiz
- `PursuerHazard` move-se em todo `Update` (5,5 un/s) e mata no `OnTriggerEnter2D`, **sem saber de trava de controle**.
  Nasce em x = −24 (borda do colisor −22,5), a 4,2 un do spawn do Odisseu (−18): alcança em ~0,8 s.
- A `LevelIntro` trava o jogador por 3 s (1 fala) — o gigante chega antes do fim da fala.
- Na morte, o próprio `PursuerHazard` se põe 8 un atrás do respawn; com a fala ainda tocando, alcança de novo. 2 vidas
  perdidas sem agir → fim de jogo em 3–4 s.
- Não existe invulnerabilidade pós-respawn no projeto (o dano do perseguidor é ambiental, o escudo não bloqueia).
- **Achado na investigação:** o checkpoint (x = 6,0) ficava **dentro** da área do golpe do gigante em x = 5 (raio 1,4 →
  3,6..6,4): quem morria depois dele reaparecia sob o golpe.
- **Achado no teste da correção:** o mesmo defeito na **fala final** — a `LevelGoal` trava o jogador na saída e o gigante
  o alcançava, tirando uma vida **depois** da fase vencida (visto nas 3 rodadas do bot; as vidas valem para a campanha).

### Solução
`LestrigoesChaseGuard` (novo, `Assets/Scripts/Levels/`), presente só na cena 07. Enquanto a guarda está fechada: o
`PursuerHazard` fica desligado **e com o colisor desligado** (mensagens de trigger chegam a componentes desligados), a arte
do gigante vai para Idle e os dois `BossController` não iniciam ataques. A guarda:

| Momento | Fecha | Abre |
|---|---|---|
| Abertura | no `Awake` (antes de qualquer `Update`) | no `DialogueSequence.Completed` da fala de abertura — o mesmo evento com que a `LevelIntro` devolve o controle (sem timer) |
| Depois de cada morte | no `HealthSystem.Died` (se não for a última vida) | após **1,5 s de jogo** — o respawn não trava o jogador nem emite "controle recuperado", então o tempo é o único sinal |
| Conclusão da fase | quando `LevelManager.IsCompleted` fica verdadeiro | não abre mais (a cena troca) |

O reposicionamento do perseguidor na morte é o que o `PursuerHazard` já fazia; **nenhum teletransporte novo**.
O checkpoint passou para **x = 1,5**, fora de todas as áreas de golpe (a primeira começa em 3,6).

### Arquivos
| Arquivo | Tipo |
|---|---|
| `Assets/Scripts/Levels/LestrigoesChaseGuard.cs` | código novo (componente da cena 07) |
| `Assets/Scripts/Editor/LestrigoesChaseFix.cs` | override da cena (põe a guarda, liga as referências, move o checkpoint); idempotente |
| `Assets/Scenes/Levels/Level_07_Lestrigoes.unity` | cena: + `LestrigoesChaseGuard`, checkpoint x 6,0 → 1,5 |
| `Assets/Scripts/Editor/QaMechanicsTest.cs` | teste `QaMechanicsTest.Lestrigoes` |
| `Assets/Scripts/Editor/QaPlaytestBot.cs`, `QaMechanicsTest.cs` | testes agora rodam **sem som** (Editor mudo, restaurado no fim) |

### Testes (`QaMechanicsTest.Lestrigoes`, input real — `Logs/qa_bug001_teste.txt`)
| Caso | Resultado |
|---|---|
| Entrada direta — abertura | controle travado 3,0 s; perseguidor parado em x = −24; **0 golpes, 0 mortes**; libera no fim da fala |
| Parado logo depois de recuperar o controle | o gigante alcança em ~0,8 s — perseguição normal (ver risco abaixo) |
| Janela de respawn | perseguidor **parado a 8 un**, colisor desligado, 0 avanço em 0,8 s |
| Enfrentar de novo (correndo com Shift) | chegou a x = 3,4, gigante em −11,1, **0 mortes** |
| Checkpoint | ativa em x = 1,5; respawn **fora** de toda área de golpe |
| Restart pelo botão da pausa | abertura protegida de novo (0 mortes, 0 golpes) |
| Voltar ao mapa e entrar de novo | abertura protegida de novo (0 mortes, 0 golpes) |

**Bot (`QaPlaytestBot -qaFrom 7 -qaTo 7`), 3 rodadas depois da correção:** **3/3 concluídas** (16–17 s), transição →
WorldMap, Circe desbloqueada, mapa no nó 08. 1 morte por rodada durante a fuga (o bot não corre com Shift), **nenhuma na
saída**. Antes: 0/3, fim de jogo em 3–4 s.

### Risco que fica (não é blocker)
Com o controle devolvido, o gigante está a 4,2 un e é 0,5 un/s mais lento que o Odisseu andando (3,5 un/s mais lento
correndo). Quem ficar parado depois da fala morre em ~0,8 s — a margem de reação é curta. É a perseguição como foi
desenhada; afastar a largada é decisão de design, não foi mudada.

### Regressão
| | |
|---|---|
| `CampaignValidation` | **OK — 16 etapas na ordem oficial**, 0 erros de compilação |
| Build WebGL | **Build Finished, Result: Success** |
