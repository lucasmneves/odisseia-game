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

## 16. Etapa 13 — polish (2026-10-04)

Decisões do usuário: fonte **DejaVu Sans**; **remover** os emojis; **manter** o menu principal; ligar os ícones de
fome/lótus/cera/escudo. Mapa geográfico, contraste do HUD/Ending e chama do altar ficaram fora desta rodada.

### Fonte (QA-10 resolvido)

- `Assets/Resources/Fonts/DejaVuSans.ttf` (release oficial 2.37 do GitHub `dejavu-fonts`; licença Bitstream Vera/domínio
  público, ao lado em `DejaVuSans-LICENSE.txt`). Cobertura conferida no `cmap`: acentos do PT, —, ✕ □ △ ○, ◄ ►, ✓, ← →, aspas.
- `UITheme.Font` (cai na embutida se o arquivo faltar); as 11 chamadas à fonte embutida passaram a usá-la; `FontBootstrap`
  troca a embutida nos textos de cada cena carregada (~245 textos nas 20 cenas, sem reescrever cena). Negrito das cenas:
  sintético (sem arquivo Bold).
- Build: dados 15,57 → **15,91 MB** (+0,34 MB). No navegador: "ITHACA **—** THE CALL", "**[Y/△]** Play", "Ithaca **—** before
  the war"; 0 erros.

### Emojis removidos

O WebGL não desenha emoji (nem a DejaVu tem): 🏠 e 🥸 do objetivo, 🍖 da fome, 🌸 do lótus, 🔗 do mastro e 🌬️ do vento
deixavam um buraco no começo do texto. Saíram da tabela e do código. Mastro e vento eram texto fixo em PT na cena → chaves
`ui.siren.mast` e `ui.wind.waiting` (o texto da cena fica de reserva). O letreiro "Disfarçado de mendigo" da Fase 14 não
tinha `LocalizedText` (o da Fase 01 tem) → componente adicionado à cena com a chave `ui.hud.disguised`, que já existia sem
uso (única edição de cena: +14 linhas).

### Ícones dos indicadores (`IndicatorIcon`, escala inteira)

| Indicador | Ícone | Onde |
|---|---|---|
| Fome (Gado do Sol) | `icon_hunger` (30×28) | dentro do painel, à esquerda; texto pela tabela ("Hunger/Fome: 92%") |
| Resistência ao canto (Sereias) | `icon_wax` (20×24) | à esquerda da barra |
| Sonolência (Lotófagos, fora da campanha) | `icon_lotus` (17×12 → 2×) | dentro do painel; chave nova `ui.hud.lotus` |
| Escudo | `icon_shield` (28×28) | **ligado na 13B.3:** linha própria abaixo do XP, só o ícone (ver §18, 13B.3) |

### Testes

`LocalizationProbe` OK (288 chaves; 16 rótulos de cena) · `PlaceholderProbe` OK (Fase 14) · `GamepadMenuProbe` OK com a
fonte nova (texto mais largo não quebrou menus) · `HudShot` de Gado do Sol, Sereias, Ítaca (retorno), Troia, mapa e Final ·
Build WebGL Success · navegador (`?mudo=1`, controle simulado) com os glifos visíveis e 0 erros.

### Visto e não feito (fora das escolhas desta rodada)

- **Contraste:** números do HUD e o contador do vento somem sobre nuvens claras (Gado do Sol); título do Final fraco sobre
  as folhas. → **Feito na 2ª rodada (abaixo).**
- Nas capturas do `HudShot`, pedras do primeiro plano de Sereias passam por cima da caixa de diálogo — efeito da captura
  (Canvas em Screen Space Camera); no jogo o Canvas é Overlay. Conferir no navegador ao passar por Sereias.
- A tela de Controles continua com rótulos fixos em PT. → **Traduzida na 3ª rodada (abaixo).**

### Contraste (2ª rodada da Etapa 13, pedido do usuário)

- `TextContrast` (sem reescrever cena): textos **soltos** sobre o cenário — filhos diretos do "HUD Canvas" (vida, moedas,
  flechas, vidas, XP, letreiro, objetivo, contador do vento) e do "Ending Canvas" (título e mensagem) — ganham contorno
  escuro (`Outline` 2 un., 90%). O que está em painel ou botão já tem fundo e fica igual.
- **Final:** só o contorno não bastava (letra fina e clara contra as folhas da oliveira) → faixa escura translúcida (60%)
  atrás do título e da mensagem, do tamanho do texto mais largo (acompanha o idioma), ancorada no alto como os textos. 1ª
  tentativa ancorada no centro ficava fora da tela: o Canvas ainda tem 1280×960 quando a cena carrega.
- Conferido por `HudShot`: Gado do Sol (números e "Waiting for a fair wind" legíveis sobre as nuvens), Sereias (céu claro),
  Fase 01, Final.

### Tela de Controles traduzida (3ª rodada da Etapa 13)

- **"Mover (Negative)" era bug:** o asset grava as partes do eixo como "Negative"/"Positive" e o `KeyRebindService`
  comparava com "negative" — as chaves `ui.rebind.moveLeft/moveRight`, que já existiam, nunca eram usadas. Comparação sem
  diferenciar maiúsculas.
- Nomes das ações saíram do `switch` fixo em PT para `ui.rebind.*` (+ `ui.rebind.sprint`); título, dica, botões e
  mensagens para `ui.controls.*`, com `LocalizedText` nos textos fixos (a tela é montada uma vez e sobrevive à troca de
  idioma); motivos da captura (cancelado, ação inválida, tecla já usada) para `ui.rebind.reason.*`.
- **Nomes de tecla traduzidos** (`ui.key.<controle>`): SPACE/ESPAÇO, LEFT ARROW/SETA ESQUERDA, LEFT SHIFT/SHIFT ESQUERDO,
  ESC, Enter, Tab… Letras e números ficam como o Input System escreve. Vale também para as dicas do tutorial
  ("Pressione ESPAÇO para pular").
- Testes: `LocalizationProbe` OK (317 chaves) · `InputBindingsProbe` OK · `GamepadMenuProbe` OK, com checagem nova que lê a
  tela inteira em inglês e em português (sem "(Negative)", "Move left"/"Mover para a esquerda", "SPACE"/"ESPAÇO",
  "Restore defaults"/"Restaurar padrões").

## 17. ETAPA 13A — CONTROLS LOCALIZATION (2026-10-04)

**Problema:** "Negative"/"Positive" não eram reconhecidos porque o código comparava em minúsculas com os valores
capitalizados do asset — a tela mostrava "Mover (Negative)". Além disso, nomes de ação, título, botões, mensagens e motivos
da captura estavam fixos em PT, e os nomes de tecla em EN.

**Correção** (commit `39c8a12`): comparação case-insensitive e centralização das traduções de ações (`ui.rebind.*`),
textos da tela (`ui.controls.*`), motivos da captura (`ui.rebind.reason.*`) e nomes de tecla (`ui.key.*`) na
`LocalizationTable`, pelo `Localization` existente. Textos fixos da tela com `LocalizedText` (mudam com a tela aberta).

**Validação (esta subetapa):** o `GamepadMenuProbe` ganhou, por idioma:

| Verificação | English | Português |
|---|---|---|
| Troca com a tela aberta (título e botões) | — | CONTROLES · Restaurar padrões · Fechar |
| Linhas e teclas | Move left/right · SPACE · LEFT/RIGHT ARROW · LEFT/RIGHT SHIFT | Mover para a esquerda/direita · ESPAÇO · SETA ESQUERDA/DIREITA · SHIFT ESQUERDO/DIREITO |
| Nada da outra língua | sem Mover/ESPAÇO/SETA/Fechar… | sem Move/SPACE/ARROW/Close… |
| Sem "(Negative)"/"(Positive)" | OK | OK |
| Conflito (Interagir → Z) | Not changed — key already used by "Attack". (tecla fica E) | Não alterado — tecla já usada por "Atacar". |
| Cancelamento por Esc | Not changed — cancelled. (tela continua aberta) | Não alterado — cancelado. |
| Dica de pausa no teclado | ESC | ESC |
| Tutorial de Troia | Press SPACE to jump. | Pressione ESPAÇO para pular. |

Revisão de código: sem texto fixo restante em `OptionsMenu`/`KeyRebindService` (só "—" para tecla vazia); sem dependência
circular (`KeyRebindService` → `Localization`, nunca o contrário); lógica de idioma só na tabela. Duplicação aceita, por
contexto: "CONTROLES" (seção das Configurações × título da tela) e "Fechar" (`ui.settings.close` é "FECHAR", em caixa
alta) — chaves separadas para cada tela poder mudar sozinha.

**Resultado: PASS.** Regressão e WebGL abaixo.

**Regressão (13A):** compilação 0 erros · `LocalizationProbe` OK (317 chaves) · `GamepadSetup` OK · `InputBindingsProbe` OK
(Pausa = ESC) · `CampaignValidation` OK · `PlaceholderProbe` OK nas 16 fases · `GamepadMenuProbe` OK (inclui foco, D-pad,
B, captura, Xbox/PlayStation/genérico e as checagens acima) · Build WebGL Success (dados 15,91 MB).

**WebGL (`?mudo=1`, controle simulado pela Gamepad API):** menu → A em Settings → A no ">" do idioma troca para Português
na hora (tela inteira refeita) → D-pad até Personalizar → A abre CONTROLES todo em português (Mover para a esquerda, SETA
ESQUERDA, ESPAÇO, SHIFT ESQUERDO, Restaurar padrões, Fechar; foco em Fechar) → B fecha e o foco volta a Personalizar →
volta para English → Customize abre CONTROLS em inglês (Move left, LEFT ARROW, SPACE, LEFT SHIFT, Restore defaults, Close)
→ B, B volta ao menu. 0 erros no console.

**Achados para a 13B (não corrigidos aqui):** nomes de qualidade gráfica em inglês nas Configurações em português ("High" —
vêm do `QualitySettings` do Unity); rótulo "Restaurar padrões" encostado nas bordas do botão de 196 un. (DejaVu é mais larga
que a fonte antiga).

## 18. ETAPA 13B — POLISH VISUAL (parcial, 2026-10-05)

### Correções prévias

- **Qualidade gráfica em inglês nas Configurações em português ("High"):** os nomes vêm do `QualitySettings` do Unity
  (`ProjectSettings/QualitySettings.asset`: Very Low … Ultra). `SettingsManager.QualityDisplayNames` traduz pela tabela
  (`ui.quality.<nome sem espaço>`), na mesma ordem — a lógica de qualidade continua por índice, sem mudança. PT: Muito
  baixa · Baixa · Média · Alta · Muito alta · Ultra; EN igual ao Unity.
- **"Restaurar padrões" encostando nas bordas:** botões de baixo da tela de Controles de 196 → 240 un. (simétricos, vão de
  12), corpo da letra mantido. Medido no `GamepadMenuProbe`: "Restaurar padrões" 214 e "Restore defaults" 190 + folga 24
  cabem em 240.

### 13B.1 — Mapa geográfico (feito)

- Fundo: `map_aegean` (arte existente, 604×340) em escala inteira 3× atrás do caminho (42,3 × 23,8 un.), ordem −20.
- Caminho redesenhado pelo mar, 52 pontos, 16 paradas na ordem oficial (o `WorldMapGeographyDresser` confere a ordem):
  Ítaca = ilha grande da esquerda (prólogo SO, retorno SE, pretendentes no centro, final ao norte); leste: Troia (costa do
  alto à direita), Cícones (península), Citera, Ciclopes, Éolo (ilhas da direita); oeste: Lestrigões, Circe (ilhas de
  baixo), Mundo dos Mortos (penhasco da beira do mundo); Sereias (ilhota), Cila e Caríbdis (estreito entre a ilha do centro
  e a península), Gado do Sol (ilha do centro), Calipso (ilhota) e volta a Ítaca. Rota conferida sobre a arte antes de
  gravar; menor distância entre paradas 2,76 un. (emblema 1,3 un.).
- `WorldMapManager`: com a arte, a câmera fica dentro dela e os brilhos de onda extras não são criados (a arte já tem
  ondas). Câmera 6 (era 5), deslocada 1 un. para baixo do navio (o painel da fase cobre o terço de baixo); navio 10/9
  (era 4,5/3,5 — caminho de ~190 un., era 60). Título, subtítulo e progresso do mapa com o contorno do `TextContrast`.
- Desbloqueio, nós, emblemas, louros, aro, trilha (navegada/por navegar), navio, painel e entrada na fase: inalterados.
- Capturas (`HudShot -hudMapProgress 0/3/8/15`): fases bloqueadas em cinza, concluídas com louro, atual com aro, navio em
  posições diferentes. A faixa de mar nas bordas de algumas capturas é da captura (aspecto 4:3 do batch); no jogo é 16:9.
- Polish: com o navio no alto de Ítaca, "Stage 15 of 16" encosta no emblema do Final.
- Regressão: compilação 0 erros; `LocalizationProbe` (323 chaves), `GamepadSetup`, `InputBindingsProbe`,
  `CampaignValidation` e `GamepadMenuProbe` OK; build WebGL Success (dados 15,91 MB). **Mapa no WebGL ainda não conferido:**
  a janela do app estava sem foco e o navegador limitou a página a 2 fps (o limite já registrado na Etapa 10).

### Interrupção

O disco C: encheu (0 GB livres, por fora do projeto) e o Unity e o git pararam. A pasta `Documents\Claude` (8,1 GB, 86.056
arquivos) foi movida para **`E:\Claude`** a pedido do usuário (robocopy, 0 falhas, contagem e bytes conferidos). O projeto
agora está em `E:\Claude\Odisseia`.

### Ainda não feito na 13B

~~13B.2 chama do altar~~ (feito, abaixo) · ~~13B.3 ícone de escudo~~ (feito, abaixo) · ~~13B.4 fonte e símbolos PlayStation~~ (feito, abaixo) (revisão final; a DejaVu já resolve os
glifos) · ~~13B.5 menu principal~~ (feito, abaixo) · ~~13B.6 HUD~~ (feito, abaixo) · ~~13B.7 Ending~~ (feito, abaixo) · ~~13B.8 QA final~~ (feita, abaixo). PixelLab: 0 gerações.

### ETAPA 13B.2 — CHAMA DO ALTAR (2026-10-05)

- **Asset utilizado:** `Assets/Art/Items/item_checkpoint_altar.png` (já existia; 7 quadros 48×82, PPU 42,86, pivô no pé).
  Quadro 00 = apagado; 01–06 = a chama acesa em ciclo, mesmo canvas e pivô, corpo do altar idêntico (só chama e
  fagulhas mudam). Antes o jogo usava só o 00 e o 01, então o altar aceso ficava parado. Nenhum asset novo; PixelLab: 0.
- **Sistema utilizado:** o mesmo dos fogos de cenário, o `SpriteAnimator` (`FireAnimationDresser`). Ele só lia folhas de
  `Resources` agrupando os quadros pelo nome, o que juntaria o 00 apagado à chama; ganhou uma sobrecarga
  `Configure(estado, Sprite[], fps)` que toca uma lista de sprites pelo mesmo laço do `Update` (o caminho do jogador e dos
  fogos não muda). Nenhum segundo sistema de fogo, sem partículas.
- **Implementação:** `Checkpoint` tem `activeFrames` (01–06, preenchidos pelo `ItemArtDresser.Altar()`); ao acender, cria
  UMA vez o `SpriteAnimator` no próprio altar e toca a chama a 7,5–9 fps conforme a posição (mesma regra do
  `FireAnimationDresser`, altares vizinhos não pulsam juntos). O apagado continua parado. Movimento vertical e variação de
  tamanho vêm dos próprios quadros; transform, escala, pivô, colisor (gatilho 0,3×1,4), ordem e camada intactos.
  Pausa: congela com o `timeScale` 0, como os outros fogos. Sem Instantiate, coroutine nem objeto novo por quadro.
- **Fases afetadas:** todas as 16 (o altar é o `Checkpoint.prefab`, sem override de sprite nas cenas; o Prólogo tem 7).
- **Testes** (`AltarFlameProbe.Run`, play mode, **no mudo**: `audioMasterMute` + volume 0, restaurados no fim), Troia →
  Circe → Final → Troia de novo: 1. altar parado no quadro 00, sem animador; 2. o jogador posto no altar acende pelo
  gatilho; 2–3. jogador andando 4 un./s e câmera indo atrás (5,6–7,5 un.): 6 quadros, só 01–06, a chama nunca some;
  4. pausa: parada num quadro; 5. depois do Resume: anima de novo; 6–7. troca de cena, saída e reentrada: altar volta
  apagado e acende de novo. Posição, escala, colisor e ordem iguais em todas; nenhum objeto sobra (159→158, 129→128,
  109→109: a faísca do acendimento some). Fotos: `Docs/QA/_hud/altar_flame.png`. Compile 0 erros; `PlaceholderProbe` OK;
  `CampaignValidation` OK (16 etapas); build WebGL Success (dados 15,92 MB).
- **Resultado:** PASS.

**Pendência pré-existente (registrada, não corrigida):** o altar e o corpo do jogador estão empatados (camada Default,
ordem 0, z 0) desde o 1º commit, e o altar fica no ponto de nascimento de toda fase. Com o empate a ordem não é fixa:
numa rodada o Odisseu apareceu atrás do altar, na seguinte na frente. A ordem −2 no altar foi testada e descartada (o
usuário aprovou testar): no Prólogo o muro baixo (x=202, 237) e o poste do cais (x=320) passavam a cobrir o altar
(`Docs/QA/_hud/altar_ordem_menos2_vs_0.png`, esquerda −2 / direita 0); afastar o altar em z mexeria no respawn, que usa a
posição inteira. Decisão do usuário: manter 0 e tratar depois (13B.6 ou QA final), provavelmente pelo lado do jogador.
`AltarFlameProbe.Vizinhanca` lista o que encosta em cada altar e `AltarFlameProbe.Comparar` fotografa as duas ordens.

### ETAPA 13B.3 — ÍCONE DO ESCUDO NO HUD (2026-10-05)

- **Asset: reutilizado, não criado.** `Assets/Resources/Odisseia/UI/HUD/icon_shield.png` (aspis de bronze, 28×28), do mesmo
  pacote PXL-019 dos outros ícones do HUD e com a mesma importação (PPU 42,86, filtro Point, sem compressão). Já existia e
  estava sem uso. Outras fontes vistas e descartadas: `ithaca_shield_round_01` e `troy_broken_shield_01` (objetos de
  cenário, outra escala). PixelLab: 0.
- **Antes:** o HUD não mostrava o escudo de jeito nenhum (só vida, moedas, flechas, vidas e XP); no mobile só o botão de
  toque "DEF".
- **Implementado** (`HUD.cs`): linha nova na coluna da esquerda, abaixo do XP (y −200; ícone em x 20 como os outros),
  criada pelo mesmo `CreateStackedLabel` + `AddIcon` dos outros contadores, em tamanho nativo. Só o ícone, sem texto: o
  `PlayerShield` não tem número para mostrar, e assim nada muda entre PT e EN. Estados, só refletindo o que já existe:
  **disponível** = ícone opaco (`PlayerShield.IsAvailable`, getter novo só de leitura: `shieldEnabled` e a ação de input);
  **defendendo** (`IsBlocking`: tecla X, LT/L2 ou o botão DEF do toque, todos pela mesma ação) = o ícone sobe 3 un. (o
  escudo erguido; passo inteiro, sem borrar a pixel art); **golpe bloqueado** (evento `Blocked`) = pisca dourado 0,15 s;
  **indisponível** = sem ícone. A 1ª versão ("pronto" meio apagado) ficou ilegível no muro escuro da Fase 15 e no bege
  das Sereias e foi trocada. Mecânica, bindings, os outros contadores e o `PlayerShield` (fora o getter) intactos.
- **Testes** (`HudShieldProbe.Run`, play mode **no mudo**): F01, F02, F08, F10, F12, F15 e F16 em inglês e português, e F02,
  F10 e F16 com os controles de toque forçados (`MobilePlatformDetector.DebugOverride`): 17 passadas, 102 conferências,
  **RESULTADO OK**. Em cada uma: `icon_shield` 28×28 ligado, dentro da tela, sem encostar em nenhum gráfico visível (textos
  medidos pelos glifos); X segurado (teclado/toque) e LT (gamepad) erguem, soltar abaixa; golpe de frente 10 → 2 e pelas
  costas 10 (a conta da defesa não mudou), com o brilho dourado. O diálogo de abertura (que trava o jogador) é pulado
  antes, como o jogador faria. Canto do HUD em 1280×720, 1920×1080, 960×540, 2340×1080 (celular) e 1024×768:
  `Docs/QA/_hud/hud_shield.png`. Compile 0 erros; `PlaceholderProbe` OK; `CampaignValidation` OK; build WebGL Success
  (dados 15,92 MB; total 20,7 MB). Não conferido no navegador: o painel do app fica oculto e a página para (0 fps).
- **Resultado:** PASS — ícone reutilizado de asset existente.

**Placeholders restantes no HUD (registrados, não corrigidos):**
1. **Botões de toque** (`MobileControlsRoot`): retângulos arredondados gerados por código com rótulos de texto "ATK", "DEF",
   "JUMP" e setas, só em inglês. DEF e arco podem reaproveitar `icon_shield`/`icon_arrows`; espada e pulo não têm ícone →
   **PIXELLAB-FUTURE** (ícones de toque: espada, pulo).
2. **Barra de resistência das Sereias:** retângulo azul chapado com borda cinza; só o ícone de cera é arte →
   **PIXELLAB-FUTURE** (moldura e preenchimento da barra).
3. **Painel da fome (Gado do Sol):** caixa escura lisa com texto (segue o `UITheme`, mas é painel chapado).
4. Fora do HUD, no mundo: as faíscas `VfxBurst` (checkpoint, coletável, flecha, golpe, escudo…) usam o `PlaceholderSprite`
   (quadrado tingido).

**Outros achados:** o `HudShot` não mutava — corrigido (`audioMasterMute` + volume 0, restaurados); uma captura de Troia
rodou antes da correção. Ainda sem mudo: `CampaignProbe`, `FireAnimationDresser.Conferir`, `LevelSelectProbe` e
`MainMenuProbe` (não usados nesta etapa). No `HudShot`, os objetos de primeiro plano das Sereias aparecem por cima do
diálogo: efeito da captura (o Canvas passa para 1 un. da câmera), não do jogo, onde o HUD é overlay.

### ETAPA 13B.4 — FONTE E SÍMBOLOS DE CONTROLE (2026-10-06)

**Busca:** símbolos PS/Xbox, A/B/X/Y, LT/RT/LB/RB/L1/R1/L2/R2, Start/Options, nomes de tecla, "[E]", "Press/Pressione" — no
código, na tabela de idiomas, nas cenas e nos prefabs. Tudo o que o jogador vê passa pelo `ControlHints` (tutoriais de
Troia e do Prólogo, mapa, NPC, pontos de interação, gado do sol) ou pelo `KeyRebindService` (tela Controles). Pause, Fase
concluída, Fim de jogo e Configurações não citam botão (navegação por foco). O diálogo não mostra dica de botão.
Os textos fixos em cena (3 dicas de Troia, mastro) são só reserva para chave ausente e não aparecem.

**Problemas encontrados:**
1. O rótulo do controle vinha de uma **tabela fixa** por ação — batia hoje com o binding, mas não era lido dele; **Crouch**,
   **Advance** e **Skip** (mapa Dialogue) não estavam nela e sairiam com o nome interno da ação.
2. **Teclado: a dica de mover dizia "Move"** em vez de "A/D" ("Use Move to move" no Prólogo e em Troia): o asset grafa as
   partes do composto "Negative"/"Positive" e a comparação era com minúscula. O teste antigo só olhava o Move fora do teclado.
3. A mesma comparação no `KeyRebindService.GetCompositePartPath`: as **setas de toque** nunca achavam o binding e caíam no
   fixo A/D — remapear "mover" deixaria a seta de toque sem efeito.
4. Rótulos de toque **só em inglês** (JUMP/ATK/DEF/BOW/USE): em português a dica dizia "Toque em JUMP para pular"; o botão
   "ENTRAR" do mapa lia a tradução uma vez só.
5. Tela Controles: **"Esc cancela"** fixo, também no controle (lá é B/○).

**Correções** (bindings, mapeamento de ações e mecânica intactos; sem sistema novo, sem fonte nova):
- `ControlHints`: o rótulo do controle sai do **binding de gamepad da própria ação** (qualquer mapa do asset), traduzido para a
  família: Sul A/✕, Leste B/○, Oeste X/□, Norte Y/△, LT/L2, RT/R2, LB/L1, RB/R1, Menu/Options, View/Create, LS/L3, RS/R3;
  direcional "D-pad ▼"/"Direcional ▼"; genérico "A/✕". Entrada `Cancel` (Esc / B / ○, os mesmos controles que as telas
  já leem). Partes do composto sem diferença de caixa.
- `KeyRebindService`: `GetPathDisplayString` (extraído do `GetDisplayString`, mesmo comportamento) e a comparação do composto
  sem caixa.
- Toque: chaves `ctrl.touch.*` (EN JUMP/ATK/DEF/BOW/USE · PT PULO/ATQ/DEF/ARCO/USAR) lidas pelo botão (`MobileControlsRoot`,
  com `LocalizedText`, troca com o idioma) e pela dica (`ControlHints`) — não divergem. "ENTRAR" do mapa idem.
- Tela Controles: "{0} cancela" com o botão do dispositivo em uso.
- Fonte: a DejaVu Sans já tem ✕ □ △ ○ ▲ ▼ ◄ ► ✓ — (conferido na tabela cmap). Nenhum asset novo; PixelLab: 0.

**Testes** (todos no mudo onde há play mode): compile 0 erros · `LocalizationProbe` OK (332 chaves) · `InputBindingsProbe` OK
(o "KeyBindingProbe" do pedido) · `GamepadSetup` OK (o "ControllerProbe") · `PlaceholderProbe` OK · `CampaignValidation` OK ·
`GamepadMenuProbe` OK (o "CampaignControllerMenuProbe"), agora com a **tabela de todas as ações** (Jump, Attack, Shield,
Bow, Interact, Sprint, Crouch, Pause, Advance, Skip, Cancel, Move) para controle genérico, PlayStation (DualShock 4
virtual), Xbox (XInput virtual), teclado e toque, em inglês e português — cada rótulo igual ao binding real, sem nome
interno ("Negative", "button…", "<Gamepad>"); mais as setas de toque seguindo o binding, Controles sem "(Negative)", Pause,
Fase concluída, Fim de jogo, mapa ("[Y/△] Play" / "[E] Play") e tutorial de Troia. `HudShieldProbe` OK, com os botões de
toque reais em F02/F10/F16 nos dois idiomas e trocando de idioma em jogo. Build WebGL Success (total 20,7 MB).

**Pendente (fora do escopo de controles):** a tela de morte (`DeathOverlay`, "Você caiu — retornando ao último
checkpoint" e "vida(s) restante(s)") é **só em português**, sem chave na tabela. No toque, ações sem botão na tela (correr,
agachar, pausa) caem no nome da tecla — hoje nenhuma dica as cita. A tela Controles só lista e remapeia o **teclado**,
embora a linha das Configurações diga "Teclado e controle".

### ETAPA 13B.5 — MENU PRINCIPAL (2026-10-06)

**Inspeção:** fundo `BG_MainMenu` (1536×1024, 3:2) com o logo "ODISSEIA — Jornada de Odisseu" pintado; quatro botões
(Continuar, Novo Jogo, Seletor de Fases, Configurações) com `MenuButton` e `MenuNavigator` (ciclo, foco garantido, voltar
num lugar só); rótulos por `LocalizedText`; aviso "sem save" traduzido; música pelo `SceneAudio` (tema "the-odyssey" em
laço); sem "Sair" (WebGL). Os textos `Title`/`Subtitle` da cena estão desligados (o logo é a arte). Existem também
`menu_bg_ithaca` e `menu_bg_ithaca_odysseus` (pixel art, sem título), não usados.

**Decisões do usuário:** manter a arte; o logo pintado vale como marca nos dois idiomas. Corrigir a proporção do fundo.

**Problemas encontrados:**
1. Fundo **esticado** para a tela: em 16:9 (o template WebGL é sempre 16:9) o logo e o Odisseu ficavam 18% mais largos;
   45% em 2340×1080 e 11% espremidos em 4:3.
2. A **borda dourada** que o `MenuButton` descreve não existia, e o foco era só um leve aumento + empurrão de 10 px para a
   direita, que tirava o item da coluna (visto no navegador na 13B.1).
3. `MainMenuProbe` (antigo) entrava em play mode **sem mudo** e deixava um **save de teste** no Editor, que fazia o mapa do
   `GamepadMenuProbe` falhar se rodado depois.

**Correções** (sem asset novo; fluxo de cenas, save, desbloqueio, mapa e bindings intactos):
- `MainMenuPolishDresser` (idempotente — a 2ª execução deixa a cena idêntica): fundo com `AspectRatioFitter` (FitInParent,
  1,5) e faixas na cor da moldura pintada (#08131E) atrás de tudo; `Outline` nos quatro botões; marcador "►" dourado na
  borda esquerda do item em foco (o `focusMarker` do `MenuButton`, que não estava ligado); empurrão em foco zerado.
- `MenuButton`: pinta a borda por estado quando o botão tem `Outline` (repouso dourado, foco `TextAccent`, desabilitado
  cinza). `UITheme`: `ButtonBorder`, `ButtonBorderDisabled`.
- `MainMenuProbe`: mudo e save do Editor devolvido no fim. `GamepadMenuProbe`: começa sempre sem save e o devolve no fim.
- Achado durante a etapa: numa 2ª execução, a 1ª versão do dresser punha a faixa por cima do fundo (menu todo escuro). Os
  testes de caixas passavam; o `MainMenuPolishProbe` agora confere **pixels** (centro de cada botão e do logo).

**Testes** (todos os de play mode no mudo): compile 0 erros · `PlaceholderProbe`, `LocalizationProbe` (332 chaves),
`InputBindingsProbe`, `GamepadSetup`, `CampaignValidation` OK · `GamepadMenuProbe` OK · `MainMenuProbe` OK (música no menu e
depois de trocar de cena: "the-odyssey", laço, sem reiniciar) · **`MainMenuPolishProbe` OK** (55 conferências), novo:
- visual EN e PT: borda e marcador só no foco, rótulos = tabela, fundo 1,500, nada fora da tela, sobreposto ou coberto, em
  1280×720, 1920×1080, 960×540, 2340×1080 e 1024×768 (`Docs/QA/_hud/mainmenu_en.png`, `mainmenu_pt.png`);
- teclado: foco inicial, ↑/↓ em ciclo pulando o Continuar desabilitado, Enter abre e Esc fecha as Configurações;
  Espaço não confirma (o Submit da UI é Enter / A / ✕);
- Novo Jogo → mapa → Interagir → **F01 carrega** com o jogador;
- Xbox virtual: com save o foco começa no Continuar; D-pad e analógico; A no Novo Jogo pede confirmação e B a cancela sem
  apagar o save; A no Continuar leva ao mapa;
- PlayStation virtual (família detectada): analógico ↓, ✕ abre o Seletor de Fases e ○ volta; ✕ abre as Configurações,
  Controles por cima, ○ fecha um de cada vez; ○ no menu não faz nada. **Limite do teste:** o ↑ do analógico do DualShock 4
  virtual não chega (eixo em byte invertido; o relatório HID real é interno ao pacote) — o ↓ dele e o ↑/↓ do Xbox navegam,
  e a UI lê `<Gamepad>/leftStick` igual para todos;
- toque (mobile forçado): nenhum controle de jogo visível sobre o menu; tocar em Configurações abre a tela.
- Os testes rodados em sequência (gamepad → menu → polish → gamepad) passam todos.

**Build WebGL não gerado:** C: com 4,48 GB livres (regra: só com ≥ 5 GB).

**Restante (registrado):** logo pintado em português nos dois idiomas (decisão: marca); botões continuam retângulos
chapados com borda — sem moldura em pixel art (não há asset; não vira PIXELLAB-FUTURE por ora); cores da UI mais escuras
que os valores do `UITheme` (espaço de cor linear — afeta o jogo todo); textos `Title`/`Subtitle` desligados e só em PT na
cena (inofensivos, não aparecem).

### ETAPA 13B.6 — HUD, CAMADAS E TELA DE MORTE (2026-10-06)

**A) Altar × jogador — análise** (`SortingProbe.Scan`, sem play mode, relatório em `Logs/qa_sorting.txt`):
1. Jogador: `Body` ordem 0 e `ShieldVisual` 2; sem SortingGroup; nenhum script muda a ordem em jogo.
2. Altares (`Checkpoint`): ordem 0, z 0 — o mesmo do jogador. Uma camada só (Default); pipeline Built-in, câmera ortográfica:
   com ordem igual, o Unity desempata pela distância à câmera, e tudo estava em z 0 — por isso o resultado variava.
3. **A ordem 0 é a camada "interativa" de toda fase:** chão, altares, moedas, objetivo, plataformas, pickups. Ordem 1: NPCs
   (30 no Prólogo), portões, mastros e velas, peças de guerra de Troia, gado, telhado; poeira do pulo. Ordem 2: inimigos,
   lança. 3+: primeiro plano (rochas, névoa, braseiros). Negativas: fundo e paredes.
4. Dependências da ordem do jogador: a poeira (1, por cima do pé) e o escudo (2). As "Rock_Cover" de Ciclopes têm o desenho
   desligado (só colisor), então nada na ordem 0 foi pensado para esconder o jogador.
5. Descartados: altar na ordem −2 (muros, palácio e cais do Prólogo passavam a cobri-lo — 13B.2) e jogador na ordem 1
   (empataria com NPCs, portões e mastros nas 16 fases).

**Solução (específica do jogador, sem mudar ordem nenhuma):** o sprite `Body` fica em z local −0,05 (0,05 mais perto da
câmera) — `ItemArtDresser.CorpoNaFrente`, idempotente. Ele ganha só os empates da ordem 0 (altar, chão, moedas,
plataformas); ordem 1+ continua por cima e as negativas por baixo, como antes. A raiz, o colisor, o `GroundCheck`, o
`AttackPoint`, o `FirePoint` e o escudo não se mexem; o respawn e a câmera usam a raiz; a física 2D ignora z. Nenhuma cena
sobrescreve a posição do Body.

**B) Tela de morte:** `DeathOverlay` passa a usar a tabela: `ui.death.message` ("You fell — returning to the last
checkpoint" / "Você caiu — retornando ao último checkpoint"), `ui.death.lifeLeft` ("{0} life left" / "{0} vida restante") e
`ui.death.livesLeft` ("{0} lives left" / "{0} vidas restantes"). O texto da cena ficou só como reserva para chave ausente.
Morte, respawn e checkpoint intactos. O fim de jogo já era traduzido (`ui.gameover.*`).

**Testes** (`LayeringDeathProbe.Run`, play mode **no mudo**, 72 conferências, **RESULTADO OK**):
- F01 (os 7 altares), F02, F05, F08, F10, F13, F14, F15, F16: Odisseu posto em cada altar e a cena fotografada três vezes
  (só altar, só Odisseu, os dois) — **Odisseu na frente em 100% dos 1270 px sobrepostos** em todos os 15 altares; altar
  com posição, colisor e ordem intactos; raiz em z 0 e Body −0,05; câmera em z −10. Contraprova: a NPC Penélope (ordem 1)
  continua na frente dele (0%). Fotos: `Docs/QA/_hud/altar_player_layering.png`.
- Mecânicas em Troia: escudo 10 → 2 de frente e 10 pelas costas; arco 10 → 9 flechas; espada (PlayerCombat) ativa.
- Tela de morte em inglês e português: plural ("2 lives left" / "2 vidas restantes") e singular ("1 life left" / "1 vida
  restante"), sem texto do outro idioma; texto em 2 linhas com todos os caracteres visíveis na caixa de 680×60; respawn no
  checkpoint com vida cheia; na última vida, fim de jogo com o título traduzido e foco em "Tentar de novo". Fotos:
  `Docs/QA/_hud/death_en.png`, `death_pt.png`.
- "Tentar de novo" pelo teclado (Enter), Xbox virtual (A), PlayStation virtual (✕) e toque: recomeça a fase com 3 vidas.
- Regressão: compile 0 erros · `PlaceholderProbe`, `LocalizationProbe` (335 chaves), `InputBindingsProbe`, `GamepadSetup`,
  `CampaignValidation`, `GamepadMenuProbe`, `MainMenuProbe`, `MainMenuPolishProbe` e `HudShieldProbe` (17 passadas) OK.

**C) Revisão do HUD** (capturas do `HudShieldProbe`, 7 fases × PT/EN + mobile, 5 resoluções): coluna alinhada, ícone do
escudo no lugar, nada cortado ou sobreposto; nada novo causado pela 13B.2–13B.6. Observações (registradas, não corrigidas):
~~em 960×540 o HUD inteiro fica pequeno~~ (artefato do teste, retirado na 13B.7); sobre nuvem branca (F01) os números perdem
contraste apesar do contorno. Continuam fora do escopo: barra das Sereias, painel da fome, ícones PIXELLAB-FUTURE,
faíscas `VfxBurst` placeholder, botões de toque sem pixel art.

**D) UITheme:** as cores da UI aparecem mais escuras que os valores do `UITheme` (provável espaço de cor linear). Não
investigado — possível trabalho futuro.

**Build WebGL não gerado:** C: com 4,41 GB livres (regra: só com ≥ 5 GB).

### ETAPA 13B.7 — TELA FINAL (2026-10-07)

**Inspeção:** quarto da cama na oliveira (`ending_bedroom_olive`, 604×340 = 16:9) com o casal (`Reunion`) e Telêmaco;
título e mensagem traduzidos (`ui.ending.*`) com faixa translúcida (etapa 13); "Jogar novamente" (zera o progresso e vai
ao Prólogo) e "Voltar ao menu", com `MenuNavigator` (foco, ←/→); música: o tema único "the-odyssey" em laço.

**Problemas encontrados:**
1. Os dois botões eram Buttons com troca de cor, sem o visual do menu (borda dourada, marcador): o foco quase não se via.
2. Em telas mais largas que 16:9 as faixas laterais saíam no azul padrão da câmera (#314D79), que destoava do quarto.
3. Em telas mais largas o Canvas (que seguia a largura) crescia mais que o quarto (a câmera segue a altura):
   "Jogar novamente" cobria o casal em 2340×1080.
4. Com rótulos longos ("Jogar novamente"; no menu, "SELECIONAR FASE") o "►" em posição fixa caía em cima da 1ª letra.
5. **Nos testes:** as fotos em várias resoluções (menu, HUD, final) saíam com o fator do CanvasScaler da janela do batch
   (640×480) e com glifos em cache da escala anterior — o layout fotografado não era o daquela resolução. A observação da
   13B.6 "HUD pequeno em 960×540" era isso, e foi retirada.

**Correções** (sem asset novo; textos, cenário, fluxo, save e música intactos):
- `EndingPolishDresser` (idempotente): os dois botões com `MainMenuPolishDresser.PolirBotao` (agora compartilhado:
  `MenuButton`, borda dourada, marcador), largura 240 → 290 (posições ±155, vão de 20); fundo da câmera #2A1D16 (marrom das
  paredes do quarto); Canvas do final pela altura (match 1), como a câmera — em 16:9 (o WebGL) nada muda. Conferido com
  medição honesta: com o match 0 original o botão cobre o casal em 2340×1080; com 1, não.
- `MenuButton.PlaceMarker`: no foco, o "►" fica logo antes de onde o rótulo começa (pela largura do texto, em unidades do
  botão) — vale em qualquer resolução e idioma, no menu e no final. O menu principal ficou com o Canvas original (uma
  tentativa com Shrink foi desfeita).
- Testes (`MainMenuPolishProbe`, `HudShieldProbe`, `EndingProbe`): recalculam o CanvasScaler e refazem as malhas antes de
  cada foto; o marcador é medido com o foco em cada botão.

**Testes** (todos os de play mode no mudo): compile 0 erros · `LocalizationProbe` (335), `InputBindingsProbe`,
`GamepadSetup`, `PlaceholderProbe`, `CampaignValidation` OK · `GamepadMenuProbe`, `MainMenuProbe`, `MainMenuPolishProbe`
(55), `HudShieldProbe` (17 passadas), `LayeringDeathProbe` OK · **`EndingProbe` OK** (31 conferências), novo:
- EN e PT: título, mensagem e botões pela tabela; borda e marcador no foco; nada fora da tela, sobreposto, coberto (pixel)
  ou por cima dos personagens; "►" sem encostar no rótulo e dentro do botão — em 1280×720, 1920×1080, 960×540, 2340×1080 e
  1024×768 (`Docs/QA/_hud/ending_en.png`, `ending_pt.png`);
- música: "the-odyssey" em laço, a mesma do menu e das fases;
- teclado: foco inicial em Jogar novamente, ←/→, Enter em Voltar ao menu → menu;
- Xbox virtual: D-pad ←/→; A em Jogar novamente → Prólogo com o progresso zerado;
- PlayStation virtual: ○ não faz nada (não há para onde voltar); ✕ em Voltar ao menu → menu;
- toque (mobile forçado): nenhum controle de toque sobre os botões; tocar em Voltar ao menu → menu.

**Build WebGL não gerado:** C: com 3,95 GB livres (regra: só com ≥ 5 GB). O espaço cai ~0,5 GB por etapa (temporários do
Unity no C:).

**Restante:** rótulos do final em caixa normal ("Play again") e do menu em maiúsculas — texto da tabela, mantido.

### ETAPA 13B.8 — AUDITORIA FINAL / RELEASE AUDIT (2026-10-07)

Sem features, sem assets, sem refactor. Única mudança: mudo em `CampaignProbe` e `LevelSelectProbe` (os dois últimos que
entravam em play mode com som) e dois probes novos de auditoria (`ReleaseAuditProbe`, `SettingsShotProbe`). Todos os testes
de play mode rodaram no mudo. Log completo da auditoria: `Docs/QA/RELEASE_AUDIT_13B8_log.txt`.

**Compilação:** 0 erros. 247 avisos, **todos em scripts de Editor** (CS0618 — `FindObjectsByType` com `FindObjectsSortMode` e
`FindFirstObjectByType` obsoletos no Unity 6.5); **0 avisos no código do jogo**.

**Suíte** (todos OK, salvo o indicado):

| Probe | Resultado | Conferências |
|---|---|---|
| PlaceholderProbe · LocalizationProbe (335 chaves) · InputBindingsProbe · GamepadSetup · CampaignValidation · PrologueProbe | OK | edição |
| GamepadMenuProbe | OK | 73 |
| MainMenuProbe | OK (música "the-odyssey" em laço, sem reiniciar) | — |
| MainMenuPolishProbe | OK | 55 (1 limite do simulador: ↑ do analógico do DualShock virtual) |
| HudShieldProbe | OK | 102 (17 passadas) |
| LayeringDeathProbe | OK | 72 |
| EndingProbe | OK | 31 |
| AltarFlameProbe | OK | 16 |
| CampaignProbe · LevelSelectProbe | OK | — |
| QaMechanicsTest (Lestrigões, Calipso, vãos de Troia, Run) | iguais aos registros de referência | — |
| QaPlaytestBot 1→16 | 13/16 concluídas (ver abaixo) | — |
| ReleaseAuditProbe (novo) | 45 OK, 4 falhas triadas (abaixo) | 49 |

**16 fases** (bot jogando + auditoria em EN/PT): todas carregam com jogador, câmera, HUD, pausa, checkpoint, morte e respawn,
uma música só e 0 erros no console nas 32 passadas. Bot: 02, 03, 05–10, 12–16 concluídas com transição, desbloqueio e nó do
mapa corretos (16 → Final). Não concluídas:
- **01** — limitação do bot (só anda para a direita; "Explorar" pede voltar). `PrologueProbe`: cadeia de atos completa.
- **04 Citera** — 0/7 rodadas, em fases diferentes da onda: mortes pela onda (`TidalHazard`, senoide de ~12,6 s, mata ao tocar)
  e pelo raio (`StormHazard`, aviso piscando 1 s, golpe 0,45 s a cada 3,25 s). O bot não espera janela; nada mudou na fase
  nem nos perigos desde `92802a8` (na Etapa 8 ele passou por sorte de fase). Janelas legíveis — validação humana pendente.
- **11 Cila** — Game Over na campanha inteira; isolada, **3/3 concluídas sem mortes**. Variância; QA-02 (validação humana).

**Riscos conhecidos:** F07 Lestrigões (BUG-001) igual ao relatório da correção — abertura travada 3 s, 0 golpes, 0 mortes,
perseguidor parado a 8 un no respawn; F11 dificuldade, não bloqueio; F01 limite do bot; F13 Calipso concluída, mesma
trajetória do QA-04; F12 Gado do Sol: saída e parede-limite funcionando (concluída); F14/F15 sem problema de ordem e com
transição; F16 → Final → menu (EndingProbe).

**Mapa:** 16 nós em ordem; 0 concluídas = atual 1 + 15 bloqueados; 8 = 8 concluídos + 1 disponível + 7 bloqueados; 15 = 15 +
1. Trilha, navio, louros e entrada nas fases nas capturas (13B.1) e pelo bot. Conferência no navegador: pendente.

**Localização:** nada do outro idioma, nenhum "(Negative)", nome interno do Input System nem chave crua em 16 fases × EN/PT
(HUD, avisos e todas as falas), menu, mapa, tutoriais, Controles, morte, fim de jogo, fase concluída e final.

**Telas × resoluções** (1280×720, 1920×1080, 960×540, 2340×1080, 1024×768; CanvasScaler e encaixe recalculados antes da foto):
Mapa, Pausa, Controles, Fim de jogo, Fase concluída, menu, HUD e final OK nos dois idiomas. **Configurações: o botão
"Fechar" sai do painel** (achado abaixo).

**Áudio:** volume de música 0 → fonte 0; uma fonte de música só, também depois de ir a uma fase e voltar; tema único em laço
no menu, nas fases e no final.

**Achados novos (não corrigidos):**
1. **MEDIUM — "Fechar" das Configurações fora do painel.** Os itens são postos de cima para baixo a partir de y=+250, mas o
   painel é centralizado com a altura usada: sobra uma faixa vazia no topo e o último item (Fechar) passa da borda de baixo.
   No WebGL (sem a linha de Resolução) o botão sai 21 un. da moldura e perde ~4 px na borda da tela em 1280×720 — visível e
   clicável, e Esc/B fecham. No Editor/standalone (com a linha de Resolução) fica pela metade. Anterior à 13B. Foto:
   `Docs/QA/_hud/settings_close_1280x720_antes.png` (corrigido no RC polish: `settings_close_1280x720_corrigido.png`).
2. **LOW — cores dos botões mais escuras (item 12).** Não é espaço de cor (o projeto é **Gamma**) nem captura: medido, o
   `Image` tem a cor certa (0,20; 0,34; 0,55), mas o `CanvasRenderer` guarda a tinta da transição ColorTint do Button, que o
   `MenuButton` desliga depois — a tela mostra o produto (0,04; 0,12; 0,30). Correção futura de uma linha (zerar a tinta do
   CanvasRenderer quando o MenuButton desliga a transição).
3. **LOW — respawn sem invulnerabilidade perto de inimigo** (Circe: lobo em cima do altar; Mundo dos Mortos): vida 80/100 e
   70/100 2,5 s depois do respawn. Já registrado como decisão de design (sem invulnerabilidade pós-respawn).
4. **LOW — avisos CS0618** em scripts de Editor.
5. Artefatos de teste corrigidos nesta etapa (não são do jogo): Gado do Sol e Final não usam `LevelGoal`; Configurações
   deixadas abertas contaminavam as fotos seguintes; o encaixe do painel só roda no Update; câmera "parada" em F07 é a
   abertura travada.

**Build WebGL: BLOQUEADO PELO AMBIENTE** — C: com 3,76 GB (regra ≥ 5 GB). O último build válido é o da 13B.4 (20,7 MB).

**Matriz de release:**

| Área | Status | Severidade | Observação |
|---|---|---|---|
| Compile | OK | — | 0 erros; avisos só em Editor (LOW) |
| 16 fases | OK | MEDIUM (F04/F11) | 13/16 pelo bot; F01 limite do bot; F04/F11 dificuldade, validação humana |
| Mapa | OK | — | navegador pendente |
| Teclado | OK | — | todas as ações e menus |
| Xbox | OK (virtual) | PHYSICAL-TEST | controle físico pendente |
| PlayStation | OK (virtual) | PHYSICAL-TEST | ↑ do analógico do DualShock virtual: limite do simulador |
| Toque | OK (forçado no Editor) | PHYSICAL-TEST | aparelho pendente |
| PT/EN | OK | — | — |
| HUD | OK | — | — |
| Menu principal | OK | LOW | cor dos botões (tinta dupla) |
| Configurações | Falha visual | MEDIUM | "Fechar" fora do painel |
| Final | OK | — | — |
| Save | OK | — | Novo jogo, Continuar, desbloqueio, fim de jogo, tentar de novo, jogar novamente |
| Áudio | OK | — | — |
| WebGL | Bloqueado | ENVIRONMENT | C: 3,76 GB; último build 13B.4 |
| Controle físico | Pendente | PHYSICAL-TEST | — |
| Mobile físico | Pendente | PHYSICAL-TEST | — |
| Navegador | Pendente | ENVIRONMENT | painel do app sem renderizar |
| PixelLab-Future | Pendente | PIXELLAB-FUTURE | cosmético, não bloqueia |

**Decisão:** nenhum BLOCKER nem HIGH de código. Release candidate **sim pelo código**; o artefato depende de gerar o WebGL
(espaço no C:) e do teste no navegador.

### RC POLISH (2026-10-07)

Os dois ajustes autorizados depois da auditoria; nada mais mudou.

1. **"Fechar" das Configurações** — `SettingsScreen.EncaixarPainelNosItens`: o painel mede a caixa de todos os itens,
   centraliza a pilha e fica com a altura dela + 20 de folga; o repouso dos botões (`MenuButton`) é reancorado. O "Fechar"
   fica dentro da moldura em todas as resoluções (1280×720: y 33–80; antes −50…−1) e some a faixa vazia do topo.
   Antes/depois: `Docs/QA/_hud/settings_close_1280x720_antes.png` · `settings_close_1280x720_corrigido.png`.
2. **Cor dos botões** — `MenuButton.Apply` deixa a tinta do `CanvasRenderer` neutra a cada estado. Medido: repouso
   (0,20; 0,34; 0,55) e foco (0,28; 0,46; 0,70), as cores do `UITheme` — antes (0,04; 0,12; 0,30). O item em foco agora se
   distingue bem dos outros (menu e final).

**Testes** (todos os de play mode no mudo): compile 0 erros; LocalizationProbe, InputBindingsProbe, GamepadSetup,
PlaceholderProbe, CampaignValidation, PrologueProbe OK; GamepadMenuProbe 73, MainMenuProbe OK, MainMenuPolishProbe 55,
HudShieldProbe 17 passadas, LayeringDeathProbe 72, EndingProbe 31, AltarFlameProbe, CampaignProbe e LevelSelectProbe OK;
QaMechanicsTest igual às referências (os 16 resultados dos vãos de Troia idênticos); ReleaseAuditProbe 47 OK — as
Configurações passam nas 5 resoluções em EN/PT; restam só as 2 LOW já registradas (respawn perto de inimigo em Circe e no
Mundo dos Mortos). 0 erros no console.

**WebGL RC:** Build Finished, Result: Success — 20,7 MB (dados 15,91 MB, wasm 4,62 MB, framework 66 KB, loader 118 KB).
Gerado depois da última alteração de código. C: 6,52 GB.

**Pendente (fora do código):** navegador real, controles Xbox/PlayStation físicos, Android/iPhone, validação humana de F04
(Citera) e F11 (Cila).
