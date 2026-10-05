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

Todas as 16 carregam, com jogador, câmera e HUD. Nenhum erro/exceção de console **resolvido** pelo QA-04 (§12) durante as fases.

## 3. Bugs

| ID | Fase | Severidade | Problema | Reprodução | Causa provável | Status |
|----|------|------------|----------|------------|----------------|--------|
| QA-01 | 07 | ~~**BLOCKER**~~ → **corrigido (BUG-001, §9)** | O gigante perseguidor mata o Odisseu **durante a fala de abertura**, antes de o jogador ter controle; o respawn cai de novo dentro da fala. 2 vidas perdidas sem poder agir, a 3ª com margem mínima → fim de jogo em 3–4 s | Entrar na Fase 07. Reproduzido 3/3 (bot, rodadas 1–3): mortes em t = 1 s e 2 s em x = −18 (nascimento), com o colisor do perseguidor em x −21,3..−18,3 | `PursuerHazard` anda (5,5 un/s) desde o 1º quadro, sem esperar a `LevelIntro` (trava 3 s). Nasce em x = −24, a 4,2 un do jogador (−18): chega em 0,8 s. `resetOffsetX` −8 põe o respawn de novo ao alcance durante a trava. Jogador anda 6 un/s: fuga com 0,5 un/s de margem | **corrigido** — BUG-001 |
| QA-02 | 11 | MEDIUM (verificar com humano) | A Cila mata no convés: bot perdeu as 3 vidas em ~8 s nas 2 tentativas | Entrar na Fase 11 e andar direto pelo convés (x 0..10) | Golpes a cada 2,8 s em x = 3, 6, 9 (raio 1,4) cobrem ~8,8 un do convés; exige esperar o golpe e atravessar na janela de ~2,1 s. Possível, mas não validado por humano | **corrigido** (§11) |
| QA-03 | todas | MEDIUM | A tela de **fim de jogo só oferece voltar ao menu** — não há "tentar de novo a fase". Congela o tempo e desliga os controles | Perder as 3 vidas (medido no teste de mecânicas) | `GameOverScreen.Show` → só `BackToMenu`. Somado ao QA-01, a Fase 07 vira um loop menu → mapa → fase → fim de jogo | **corrigido** (§11) |
| QA-04 | todas (visto em 01, 10, 13) | MEDIUM | O Odisseu **gruda na lateral** de plataformas no ar enquanto a direção é segurada — não sobe nem cai | Calipso: correr para a direita contra a `Platform_Grove` (x 12) e pular. Medido: preso no ar em x = 11,70 com o pé a −1,05..−1,75. Soltar a direção → cai; pulo duplo → passa (pousa em x = 19) | Colisor do jogador sem `PhysicsMaterial2D` (e sem material padrão no Physics2D): o atrito contra a parede segura o corpo | **corrigido** (§11) |
| QA-05 | 01 | LOW | Apertar Interagir logo que uma conversa termina, ainda perto do NPC, **reabre a mesma conversa** | Falar com a Penélope e apertar E repetidamente (a rodada 1 do bot ficou 6 min nisso) | `NPCDialogue` permite conversar de novo ao fim (`talking` volta a falso); só não reabre DURANTE. Sai andando | **corrigido** (§12) |
| QA-06 | 12 | MEDIUM | Enquanto a saída espera o vento (25 s), dá para **passar do ponto da saída e cair do fim da fase**, perdendo vidas | Correr direto até o fim: o bot chegou a x = 46 (saída em 34,5..37,5) e caiu; 2 mortes | O `LevelGoal` fica inativo até o `TimeGatedActivator`; não há limite físico depois dele | **corrigido** (Etapa 6) |
| QA-07 | web | MEDIUM | No desktop o **canvas é fixo em 960 × 600**: janela menor corta o jogo (barras de rolagem); maior não escala | Abrir o build numa janela < 960 px ou > 1080 p | Template padrão `unity-desktop` com tamanho fixo | **corrigido** (Etapa 6) |
| QA-08 | mobile | LOW (confirmar em aparelho) | Em retrato (emulação) o jogo desenha em ~57% da tela e o aviso "Gire o aparelho…" é cortado nas duas bordas | Emulação Pixel 8 retrato | Aviso de orientação existe; texto sem quebra. A área parcial pode ser artefato de DPR da emulação | **corrigido** (Etapa 6) |
| QA-09 | UI | LOW | Com o idioma **English**, textos ficam em português: "JOGAR" (mapa), "Vidas" (HUD), "Carregando…" (loading), "Gire o aparelho…" | Jogo novo em inglês (padrão) | Strings fora da tabela de localização | **corrigido** (§13) |
| QA-10 | UI | LOW | **Travessão ausente** nos títulos: "Ithaca  The Call", "Ithaca  before the war" (espaço duplo) | Mapa e loading da Fase 01 | Fonte sem o glifo "—" | **corrigido** (§14; aparelho real não testado) |
| QA-11 | 01 | LOW (bot) | Bot não concluiu a Fase 01 (preso no portão do salão com Explorar 3/4) | — | Limitação do bot: na rodada 1 (Interagir contínuo) o objetivo chegou a 4/4, Convocação 1/1, Família 1/2. `PrologueProbe` confirma todos os atos fecháveis. **Fase 01 não foi jogada inteira nesta passagem** | **melhorou**: < 5 s (§13) |
| QA-12 | 04 | LOW | Citera: quedas no mar entre as plataformas do navio (bot 1 de 3) | Rodadas 1–3 | Dificuldade de plataforma com ondas; concluída na rodada 1 | **corrigido** (§13) |
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

**Status: corrigido — commitado em 4267db0; reconfirmado em 2026-10-03 (§11).** Correção **só da Fase 07**, por componente de cena + override da cena.
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

## 10. Validação local da Fase 02 — Troia (2026-10-02, Unity 6000.5.8f1 local, depois do TROY-02)

| Verificação | Resultado |
|---|---|
| Carregamento, spawn (−28, −2), câmera, HUD, 3 inimigos, saída | ok (bot) |
| Elenco (`CastProbe`) | Odisseu 42,857 px/un; 3 troianos por override; 3 gregos; 0 Animator. Coletáveis ainda a 100 px/un (moeda nativa do pack não integrada) |
| `PlaceholderProbe` | 8 visíveis — só a geometria N-08 |
| `CampaignProbe` | 01 → desbloqueia 02; nó 02 disponível no mapa |
| Vãos (`QaMechanicsTest.TroiaVaos`, novo) | **os 4 vãos passam com pulo simples saindo da borda** (decolagem x≈3,65 → pouso na ponte em 7,24; Gauntlets idem). Pulo duplo da borda nos Gauntlets ultrapassa a plataforma de 3 un (erro de jogador, não da fase) |
| Bot (`QaPlaytestBot`) | **não conclui** (3 quedas, x 10, 32, 37) — **igual na cena anterior ao TROY-02**, 2 rodadas; não é regressão do cenário |
| Combate (`QaMechanicsTest.Run`, 3 rodadas) | cooldown, escudo 80%, costas, respawn, pausa, fim de jogo = 30/09. **Divergem, de forma determinística:** 1º golpe (Odisseu ainda no ar, posto 0,7 un acima do chão) e arco (animador `Damage` — inimigo patrulhando já em cima dele). Golpe duplo acerta e o evento de disparo sai: leitura é **artefato de posição do teste**, a confirmar por humano |

**QA-21 (bot, LOW):** o segundo pulo do bot exige `saltosSemAvanco > 0`, mas o contador zera assim que ele avança 0,25 un — logo
depois de decolar. Na prática o bot não dá pulo duplo em movimento, e o gatilho de 1,1 un antes da borda não alcança os
vãos com subida (ponte, Gauntlet_1). Acrescentado `-qaOlhar` (padrão 1,1 = comportamento registrado); com 0,5 o bot ainda
raspa a quina da ponte e gruda (QA-04). Fase passável por humano segundo as medidas acima; transição 02 → mapa não foi
re-observada pelo bot nesta rodada.

**QA-22 (design, LOW — verificar com humano):** os vãos com subida de Troia (3 un, +1,2) têm folga de ~0,3 un no pulo
simples saindo da borda. Passável, mas é a passagem mais apertada do tutorial de combate.

## 11. Etapa 7 — bloqueios de gameplay (2026-10-03, Unity 6000.5.8f1 local)

**Classificação dos problemas conhecidos:** BLOCKER: nenhum restante (QA-01 confirmado). HIGH: QA-03, QA-04, QA-06 —
**corrigidos abaixo**. MEDIUM: QA-13 (parado no ar em diálogo), QA-09/10 + achados da Etapa 6 (EN/PT misturado; painel do
mapa com texto atrás do botão), QA-02 (Cila, humano), QA-07 (canvas WebGL, Etapa 9). LOW: QA-05, 08, 12, 17, 18, 19, 21, 22.
Resolvidos na Etapa 6: QA-15, QA-16, QA-20.

| ID | Correção | Prova |
|---|---|---|
| QA-01 (07) | já no código desde 4267db0 (`LestrigoesChaseGuard`) | `QaMechanicsTest.Lestrigoes`: abertura travada 3,0 s com 0 golpes e 0 mortes nas 3 entradas (direta, restart, reentrada); respawn com perseguidor parado a 8 un e colisor desligado. Bot 2/2 concluídas, transição → mapa no nó 08 |
| QA-04 (todas) | `PlayerPhysicsFix`: `Assets/Settings/Physics/Player_NoFriction.physicsMaterial2D` (atrito 0) no `CapsuleCollider2D` do prefab Player. Nada dependia do atrito: velocidade horizontal é escrita pelo controlador, plataformas móveis carregam por `FrameDelta`, chãos são caixas planas | `QaMechanicsTest.Calipso`: pé mais alto −1,75/−1,18/−1,05 (preso abaixo do topo −0,80) → **−0,31/−0,18/−0,19** (passa por cima); "grudado → solta a direção" agora **SUBIU**. Sem regressão: vãos de Troia iguais, combate idêntico, bot de Troia sem evento "GRUDADO" |
| QA-03 (todas) | `GameOverScreen`: botão **"Tentar de novo"** — recarrega a fase atual com `LivesCounter/ExperienceCounter.BeginRun()` (o mesmo reinício do menu). Textos da tela pela localização (`ui.gameover.*`) | `QaMechanicsTest.Run` (caso novo): "Try again" → cena recarregada, vidas 3, timeScale 1, mapa Player ligado, painel fechado |
| QA-06 (12) | `GadoDoSolExitFix`: `Wall_End` (layer Ground) com a face no fim do Floor_3 (x = 40) e 4,5 un acima do chão (> pulo duplo 3,43), desenhado com o muro de pedra seca da fase em 3 fiadas — o chão desenhado ia até x = 42 e o colisor acabava em 40 | Bot 2/2 **concluídas com 0 mortes** (antes 2 quedas): para em x = 39,7, espera o vento, sai → mapa no nó 13 |

**Ponta esquerda da Fase 12** (chão começa em −18, câmera vê até −20): mesmo padrão, não registrado em QA porque o jogador
nasce em −14 andando para a direita. Fica como LOW.

## 12. Etapa 8 — QA das 16 fases (2026-10-04, Unity 6000.5.8f1 local)

### Bot corrigido antes (QA-21 — ferramenta, não jogo)
O segundo pulo exigia `saltosSemAvanco > 0`, que zera assim que o corpo avança — nunca havia pulo duplo em movimento. Agora,
no alto do primeiro pulo, o bot projeta a queda (velocidade real, gravidade efetiva) e testa, ponto a ponto: o CORPO (caixa do
pé à cabeça, na borda da frente da cápsula) bateria numa lateral? → segundo pulo; há chão logo abaixo do pé? → pousa. Uma
primeira versão olhava 4 un acima e confundia teto com parede (gastava o segundo pulo perto do chão — causa das quedas de
Ciclopes e Éolo na rodada 1, **não** regressão da física). `-qaTrace x0,x1` registra a trajetória quadro a quadro.

### Rodada 2 do bot (campanha inteira, mesma sessão de save)
| Fase | Resultado | Mortes | Transição / mapa |
|---|---|---|---|
| 01 Ítaca | não (bot preso em Explorar 3/4 — QA-11) | 0 | `PrologueProbe` **OK**: cadeia de atos completa, 39 figuras apoiadas, alcance de todo objetivo |
| 02 Troia | **sim** (24 s) | 0 | → mapa, nó 03 ✓ |
| 03 Cícones | **sim** (17 s) | 0 | ✓ |
| 04 Citera | **sim** (24 s) | 1 | ✓ |
| 05 Ciclopes | **sim** (19 s) | 0 | ✓ |
| 06 Éolo | **sim** (12 s) | 0 | ✓ |
| 07 Lestrigões | **sim** (14 s) | 1 (perseguição) | ✓ |
| 08 Circe | **sim** (14 s) | 0 | ✓ |
| 09 Mundo dos Mortos | **sim** (18 s) | 0 | ✓ |
| 10 Sereias | **sim** (14 s) | 0 | ✓ |
| 11 Cila e Caríbdis | **sim** (12 s) — 1ª vez | 2 (golpes) | ✓ |
| 12 Gado do Sol | **sim** (31 s) | 0 | ✓ |
| 13 Calipso | **sim** (18 s) | 0 | ✓ |
| 14 Ítaca Return | **sim** (18 s) | 0 | ✓ |
| 15 Pretendentes | **sim** (17 s) | 0 | ✓ → nó 16 |
| 16 Final | **sim** (14 s) | 0 | → **Ending** ✓ |

Em todas: carregou, spawn, câmera seguindo, HUD, inimigos contados, saída presente. **15/16 pelo bot** (antes: 11/16 em 30/09).

### Probes
| | |
|---|---|
| `PlaceholderProbe` | **OK nas 16 fases** (primeira vez — Troia falhava) |
| `PrologueProbe` | OK |
| `GlobalCastAudit` | elenco inteiro a 42,857 px/un, escala 1; alertas de empate de ordem (checkpoint, moeda, objetivo, props) **idênticos à auditoria de 30/09** (N-05, profundidade intencional) |
| `LocalizationProbe` | **OK** — a probe lia `"speaker." + nome` como chave e falhava sempre (falso positivo corrigido na probe) |
| `CampaignValidation` | OK |

### Correções desta etapa (MEDIUM)
- **QA-09 / textos:** "JOGAR" (mapa e touch), "Carregando...", "Gire o aparelho…", prompt do gado de Hélio (agora com a tecla
  real via `ControlHints`), mensagem de remapear → localização (`ui.worldMap.enter`, `ui.mobile.rotate`, `ui.cattle.eatPrompt`,
  `ui.rebind.waiting`; `ui.loading` já existia). Falantes gravados em português nas cenas (Odisseu ×26, Telêmaco, Eumeu,
  Companheiro) não tinham chave — o nome saía em português sobre fala em inglês: chaves acrescentadas.
- **Painel do mapa:** o botão cobria a linha "Já concluída — [E] para jogar de novo" (painel 108 → 148).
- **QA-13:** medido (`QaMechanicsTest.DialogoNoAr`): travado no ar com o pé em −1,04, **cai até o chão em ~0,2 s**. O
  "parado no ar" de 30/09 era o atrito na parede (QA-04), já corrigido — o gatilho do bosque encosta na `Platform_Grove`.

### Não validado aqui
Fase 01 inteira por input (o bot não fecha o "Explorar"); dificuldade da Cila por humano (QA-02); mobile e gamepad físicos;
travessão no navegador (QA-10, Etapa 10).

## 13. Etapas 9 e 10 — build WebGL e navegador (2026-10-04)

- **Build:** `BuildScript.BuildWebGL` → **Build Finished, Result: Success**. Dados 17,67 → **14,84 MB**, wasm 4,40 MB (Brotli).
- **Template do projeto** `Assets/WebGLTemplates/Odisseia` (selecionado pelo `BuildScript`): canvas no maior 16:9 da janela com
  faixas pretas (QA-07); `autoSyncPersistentDataPath` (QA-18 — sem aviso no console, e o save persiste: volume e "Continue"
  sobrevivem ao recarregar); botão de tela cheia só no desktop; **`?mudo=1`** suspende todo áudio (testes sempre no mudo).
- **Navegador (painel do app, Chromium, mudo):** 0 erros no console; desktop detectado sem controles de toque; menu →
  seleção de fases → New Game → mapa → Fase 01; Fase 01 em **< 5 s** depois do clique (QA-17, antes ~20 s); 60,3 fps, quadro
  mais lento 16,8 ms, 46 MB de heap JS.
- **QA-10 confirmado no navegador:** o travessão some ("Ithaca  The Call"). Fonte embutida do Unity sem o glifo; no Editor o
  Windows supre. Decisão de fonte para a Etapa 13.
- Limites do painel: sem foco de janela o Unity pausa (runInBackground desligado) e emulações maiores que o painel são
  reduzidas — redimensionar com o jogo aberto e janelas grandes ficam para um navegador comum.

## 14. Etapa 11 — mobile por emulação (2026-10-04)

| Verificação | Resultado |
|---|---|
| Detecção | Android emulado (Pixel 8, 5 toques): `IsMobile=True (user agent + touch)`; controles criados (Left/Right/Jump/Attack/Shield/Bow) |
| Paisagem (480 × 222, proporção 2,16 de celular) | jogo em tela cheia; controles visíveis no mapa (mover + entrar) e na fase (USE, BOW, JUMP, DEF, ATK) |
| Retrato (111 × 240, proporção 0,46) | aviso de girar aparece |
| Save | persiste ao recarregar |

**Corrigido nesta etapa (build refeito e reverificado):**
- **Botão de tela cheia do template cobria o ATK** (e o "PLAY" do mapa) — defeito da Etapa 9; oculto no celular.
- **QA-08:** aviso de girar cortado nas duas bordas (largura fixa 700, sem quebra) → largura da tela com margem e quebra de linha.
- **Configurações em paisagem de celular:** o painel (~700 de altura) passava do Canvas (~650) e o botão Fechar saía da tela →
  encolhe por escala quando não cabe (`SettingsScreen.AjustarAoTamanhoDaTela`, a cada quadro aberto).
- **HUD com vida "0/100" no começo da fase (visto no WebGL)** e, pelo código, depois do respawn: o HUD podia inicializar antes
  do `HealthSystem`, e `ResetHealth()` não dispara evento → o HUD confere a vida por quadro e só reescreve quando muda.

**Não validado:** iOS e Android **físicos** (toque real, notch/área segura, desempenho de GPU móvel, Safari), mudança de
orientação com o jogo aberto, escala dos controles num aparelho real.

## 15. Etapa 12 — gamepad (2026-10-04)

> **GAMEPAD — NÃO VALIDADO FISICAMENTE.** Não há controle Xbox/PlayStation físico nesta sessão. A validação abaixo é por
> código, por controle **virtual** do Input System em play mode e por controle **simulado** pela Gamepad API no build WebGL.
> Falta confirmar com controle na mão: mapeamento real de cada modelo no navegador, vibração, zona morta do analógico.

### Bindings (`PlayerControls.inputactions`, inalterados)

| Ação | Xbox | PlayStation | Caminho |
|---|---|---|---|
| Mover | analógico esq. / D-pad ◄ ► | analógico esq. / D-pad ◄ ► | `leftStick/x` (zona morta 0,15–0,95), `dpad/left`, `dpad/right` |
| Pular | A | ✕ | `buttonSouth` |
| Atacar | X | □ | `buttonWest` |
| Interagir / entrar no mapa | Y | △ | `buttonNorth` |
| Escudo | LT | L2 | `leftTrigger` |
| Arco | RT | R2 | `rightTrigger` |
| Correr | RB | R1 | `rightShoulder` |
| Agachar | D-pad ▼ / analógico ▼ | D-pad ▼ / analógico ▼ | `dpad/down`, `leftStick/down` |
| Pausa | Menu (Start) | Options | `start` |
| Diálogo: avançar / pular | A / B | ✕ / ○ | `Dialogue/Advance` `buttonSouth`, `Dialogue/Skip` `buttonEast` |
| Menus: confirmar / voltar | A / B | ✕ / ○ | UI padrão do `InputSystemUIInputModule` + `MenuNavigator` (`buttonEast`) |

Família detectada pelo `InputDeviceTracker` (subclasse no Editor; no WebGL, texto do id: xbox/xinput/045e ou
dualsense/dualshock/sony/054c); controle não identificado mostra as duas grafias ("A/✕"). Remapeamento: só teclado (o
controle tem layout fixo).

### Correções

- **Menus sem foco (MEDIUM):** Pause, Fim de jogo, Fase concluída, Final e a tela de Controles não selecionavam botão
  nenhum — com só o controle, abria-se a tela e não se escolhia nada (no fim de jogo, nem "Tentar de novo"). Agora todos usam
  o `MenuNavigator` (foco inicial, cima/baixo e **esquerda/direita** em ciclo, B volta). Fase concluída e Final criam o
  navegador no `Start` (no `Awake` podia nascer um EventSystem extra).
- **Pause:** B/○ retoma, como o Start. Os 3 botões das 16 cenas tinham a cor de "selecionado" padrão do Unity (quase branca)
  com texto branco — com foco virava um retângulo branco ilegível (visto no build); agora selecionado = destaque. O botão
  "Controles" (clone do Retomar) herdava o `LocalizedText` do original e aparecia como um segundo "Resume" → chave própria
  `ui.pause.controls`.
- **Tela de Controles:** abre com foco em **Fechar** (no controle, A numa linha inicia captura que só o teclado completa);
  B/○ ou Esc cancela só a captura em andamento, senão fecha; o foco volta a quem abriu (Configurações ou Pause); o B que a
  fecha não fecha também o Pause nem as Configurações por baixo (`OptionsMenu.ClosedThisFrame`).
- **Dicas por dispositivo:** mapa-múndi trocou "[E] Jogar" fixo por `ControlHints` ("[Y/△] Jogar", "[E] Jogar", no toque
  "Toque em JOGAR") e reescreve ao trocar de dispositivo; tutorial de Troia trocou "Z", "SPACE" e "A/D" fixos por `{0}`
  (`TutorialTrigger` → `ControlHints.Instruction`, ação tirada do fim da chave) — vale também para tecla remapeada.
- `ControlHints`: rótulo de Correr no controle (RB/R1). `GamepadSetup` passa a conferir Agachar e Correr.

### Testes

| Teste | Resultado |
|---|---|
| Compilação | 0 erros |
| `GamepadSetup.Run` | OK — todos os caminhos de gamepad, PlayerHaptics no prefab |
| `InputBindingsProbe.Check` | OK — teclado padrão intacto |
| `LocalizationProbe.Check` | OK — 285 chaves, 2 idiomas |
| `PlaceholderProbe.Run` (16 fases, `-probeScene`) | OK nas 16 |
| `CampaignValidation.Run` | OK — 16 etapas na ordem oficial |
| `GamepadMenuProbe.Run` (novo; play mode, controle virtual + DualShock4 + XInput virtuais; `Logs/qa_gamepad.txt`) | **OK** |
| Build WebGL | Build Finished, Result: Success |
| Navegador (`?mudo=1`, controle simulado pela Gamepad API → `WebGLGamepad`, família Generic) | OK, ver abaixo |

`GamepadMenuProbe` mede: menu principal (foco, D-pad nas 4 direções); Configurações → Controles (foco no Fechar, captura
aberta por A e cancelada por B e por Esc sem fechar a tela, captura gravando "K", B fecha Controles e devolve o foco, B fecha
Configurações); mapa (controle "[Y/△] Play", teclado "[E] Play"); Troia (genérico "A/✕"/"X/□"/"Left Stick", PlayStation
"✕"/"□", Xbox "A"/"X", teclado "SPACE"/"Z" — sem Z/SPACE/A/D no controle); Pause (Start pausa, foco em Retomar, D-pad até
Controles, A abre, B fecha só Controles com o jogo ainda pausado e o foco de volta, B retoma, Start/Start); Fim de jogo (foco
em Tentar de novo, ◄ ►, A recomeça a fase); Fase concluída (foco, A segue); Final (foco em Jogar novamente, ▼, A volta ao menu).

No build (navegador): D-pad e A no menu; A abre Configurações; D-pad até Personalizar; A abre Controles com foco em Fechar;
A numa linha abre a captura ("..."), B cancela só ela; B fecha Controles e o foco volta a Personalizar; B fecha
Configurações; A em Continue → mapa com "[Y/△] Play" e, após Shift, "[E] Play"; Y entra na Fase 01; A avança o diálogo;
Start pausa; D-pad até Controles, A abre, B volta ao Pause, B retoma; 0 erros no console.

### Observações (não são da Etapa 12)

- **Símbolos do PlayStation somem no WebGL:** o mapa mostrou **"[Y/] Play"** — o △ não existe na fonte embutida, como o
  travessão (QA-10). Mesma causa e mesma correção (fonte) → **Etapa 13**. Afeta ✕ □ △ ○ em controle PlayStation e genérico.
- **Dois EventSystems nas cenas de menu:** o `MobileControlsRoot` cria um persistente no boot (as fases não têm EventSystem)
  e as cenas de menu trazem o seu. Funciona (só o primeiro processa), é anterior a esta etapa.
- **Pausa indisponível durante a abertura com diálogo** (mapa Player desligado): igual no teclado — fluxo de gameplay, não do
  controle.
- A tela de Controles ainda tem textos fixos em PT ("Mover (Negative)", "Fechar", mensagens) mesmo em inglês.
- Teste no painel do app: com o mouse parado sobre um botão, fechar uma tela por cima devolve o foco ao botão sob o mouse
  (hover seleciona) — comportamento normal do mouse, não do controle.
