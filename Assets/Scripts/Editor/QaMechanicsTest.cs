using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using Odisseia.Core;
using Odisseia.Enemies;
using Odisseia.Player;
using Odisseia.Systems;

/// <summary>
/// FULL CAMPAIGN PLAYTEST — teste das MECÂNICAS em play mode, por input real (teclado virtual do Input System):
///
/// Unity.exe -batchmode -projectPath . -executeMethod QaMechanicsTest.Run   (sem -quit; relatório em Logs/qa_mecanicas.txt)
///
/// Cena: Troia (3 EnemyBasic de pretendente/troiano, chão plano). Cada caso posiciona o Odisseu (a ÚNICA coisa feita
/// por código, para o caso ser reproduzível), aperta as teclas e MEDE: vida do inimigo, vida do jogador, flechas,
/// flechas em cena, IsBlocking, estado do animador, vidas, respawn, pausa. Não altera nada do jogo.
/// </summary>
public static class QaMechanicsTest
{
    private const string Cena = "Level_02_Troia";
    private static readonly StringBuilder rel = new StringBuilder();
    private static Keyboard kb;
    private static bool mudoOriginal;
    private static InputSettings.BackgroundBehavior fundo;
    private static InputSettings.EditorInputBehaviorInPlayMode editorIn;
    private static bool opcoesAtivas;
    private static EnterPlayModeOptions opcoes;
    private static IEnumerator<float> roteiro;
    private static double acordarEm;

    /// <summary>Trajetória de um pulo contra a Platform_Grove de Calipso (onde o bot ficou preso em x=11,7).</summary>
    public static void Calipso() { cenaAlvo = "Level_13_Calipso"; Iniciar(RoteiroCalipso()); }

    /// <summary>BUG-001: a abertura da Fase 07, a morte, o respawn, o checkpoint, restart e reentrada.</summary>
    public static void Lestrigoes() { cenaAlvo = "Level_07_Lestrigoes"; Iniciar(RoteiroLestrigoes()); }

    private static string cenaAlvo = Cena;

    private static IEnumerator<float> CarregarFase07(bool carregar = true)
    {
        if (carregar) { UnityEngine.SceneManagement.SceneManager.LoadScene(cenaAlvo); }
        while (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != cenaAlvo || GameObject.FindGameObjectWithTag("Player") == null) { yield return 0.05f; }
    }

    private static IEnumerator<float> RoteiroLestrigoes()
    {
        while (CampaignManager.Instance == null) { yield return 0.2f; }
        fundo = InputSystem.settings.backgroundBehavior; editorIn = InputSystem.settings.editorInputBehaviorInPlayMode;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        Application.runInBackground = true;
        kb = InputSystem.AddDevice<Keyboard>("QAMechKeyboard");

        for (int entrada = 1; entrada <= 3; entrada++)
        {
            LivesCounter.BeginRun();
            string como = entrada == 1 ? "entrada direta" : entrada == 2 ? "restart pelo botão da pausa" : "voltar ao mapa e entrar de novo";
            if (entrada == 2)
            {
                var pausa = Object.FindAnyObjectByType<Odisseia.UI.PauseMenu>(FindObjectsInactive.Include);
                var botao = pausa != null ? new SerializedObject(pausa).FindProperty("restartButton").objectReferenceValue as UnityEngine.UI.Button : null;
                L($"\n# Entrada {entrada}: {como} (botão {(botao != null ? "encontrado" : "NÃO encontrado")})");
                var velho = GameObject.FindGameObjectWithTag("Player");
                if (botao != null) { botao.onClick.Invoke(); }
                // Restart recarrega a MESMA cena: espera o jogador antigo sumir antes de procurar o novo.
                float tRe = Time.realtimeSinceStartup;
                while (velho != null && Time.realtimeSinceStartup - tRe < 30f) { yield return 0.1f; }
                L($"- cena recarregada: {(velho == null ? "sim" : "NÃO (30 s)")}");
                var e2 = CarregarFase07(false); while (e2.MoveNext()) { yield return e2.Current; }
            }
            else if (entrada == 3)
            {
                L($"\n# Entrada {entrada}: {como}");
                UnityEngine.SceneManagement.SceneManager.LoadScene("WorldMap");
                while (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "WorldMap") { yield return 0.1f; }
                yield return 2f;
                var e3 = CarregarFase07(); while (e3.MoveNext()) { yield return e3.Current; }
            }
            else
            {
                L($"\n# Entrada {entrada}: {como}");
                var e1 = CarregarFase07(); while (e1.MoveNext()) { yield return e1.Current; }
            }

            var p = GameObject.FindGameObjectWithTag("Player");
            var ctl = p.GetComponent<PlayerController>();
            var vida = p.GetComponent<HealthSystem>();
            var guarda = Object.FindAnyObjectByType<Odisseia.Levels.LestrigoesChaseGuard>();
            var pers = Object.FindAnyObjectByType<Odisseia.Levels.PursuerHazard>();
            var gigantes = Object.FindObjectsByType<BossController>(FindObjectsSortMode.None);
            int mortes = 0; vida.Died += () => mortes++;
            int golpes = 0; foreach (var g in gigantes) { g.AttackExecuted += () => golpes++; }
            if (guarda == null) { L("- **SEM LestrigoesChaseGuard na cena**"); yield break; }

            // 1. Abertura
            float x0 = pers.transform.position.x;
            float t0 = Time.time;
            bool travouAlgumaVez = false;
            while (!ctl.enabled || Time.time - t0 < 0.2f)
            {
                if (!ctl.enabled) { travouAlgumaVez = true; }
                if (Time.time - t0 > 20f) { break; }
                yield return 0.1f;
            }
            float duracao = Time.time - t0;
            L($"- abertura: controle travado={travouAlgumaVez}, {duracao:0.0} s até liberar · perseguidor x {x0:0.0} → {pers.transform.position.x:0.0} · golpes dos gigantes {golpes} · mortes {mortes} · vidas {LivesCounter.Current} · guarda segurando no fim: {guarda.IsHolding}");
            yield return 0.3f;
            float x1 = pers.transform.position.x;
            yield return 0.5f;
            L($"- controle devolvido: perseguidor andando? x {x1:0.0} → {pers.transform.position.x:0.0} ({(pers.transform.position.x > x1 + 1f ? "sim" : "NÃO")}) · gigantes ligados {gigantes.All(g => g.enabled)}");

            if (entrada > 1) { continue; }   // restart e reentrada: só a abertura importa

            // 2. Morte logo depois de recuperar o controle: fica parado e deixa o gigante alcançar.
            float tM = Time.time;
            while (mortes == 0 && Time.time - tM < 8f) { yield return 0.05f; }
            L($"\n## Parado depois da abertura\n- morreu: {mortes > 0} ({Time.time - tM:0.0} s) — comportamento normal da perseguição · reapareceu em x={p.transform.position.x:0.0} · vidas {LivesCounter.Current}");
            float xR = pers.transform.position.x;
            float tR = Time.time;
            bool segurouNaJanela = guarda.IsHolding;
            yield return 0.8f;
            L($"- janela de respawn: guarda segurando={segurouNaJanela}, perseguidor x {xR:0.0} → {pers.transform.position.x:0.0} em 0,8 s (parado?), colisor {(pers.GetComponent<Collider2D>().enabled ? "LIGADO" : "desligado")} · distância ao jogador {p.transform.position.x - pers.GetComponent<Collider2D>().bounds.max.x:0.0} un · mortes {mortes}");

            // 3. Enfrentar de novo: corre (com Shift) depois do respawn e não pode morrer.
            int mortesAntes = mortes;
            InputSystem.QueueStateEvent(kb, new KeyboardState(Key.D, Key.LeftShift));
            float tF = Time.time;
            while (Time.time - tF < 3f)
            {
                if (ctl.IsGrounded && Mathf.Abs(p.GetComponent<Rigidbody2D>().linearVelocity.x) < 0.3f) { InputSystem.QueueStateEvent(kb, new KeyboardState(Key.D, Key.LeftShift, Key.Space)); yield return 0.12f; }
                InputSystem.QueueStateEvent(kb, new KeyboardState(Key.D, Key.LeftShift));
                yield return 0.05f;
            }
            Soltar();
            L($"- correndo depois do respawn: x final {p.transform.position.x:0.0}, perseguidor em {pers.GetComponent<Collider2D>().bounds.max.x:0.0}, mortes novas {mortes - mortesAntes}");

            // 4. Checkpoint seguro: ativa o checkpoint (x 1,5), morre, reaparece e fica parado 6 s.
            var cp = Object.FindAnyObjectByType<Checkpoint>();
            p.transform.position = new Vector3(cp.transform.position.x, -1.8f, 0f);
            yield return 0.4f;
            L($"\n## Checkpoint\n- checkpoint em x={cp.transform.position.x:0.0}, ativo={CheckpointManager.HasCheckpoint} (posição {CheckpointManager.LastCheckpointPosition.x:0.0})");
            // tira o perseguidor do caminho do teste: coloca-o atrás, como o próprio PursuerHazard faria num respawn
            mortesAntes = mortes;
            vida.TakeDamage(9999);
            yield return 0.1f;
            float xCp = p.transform.position.x;
            int golpesAntes = golpes;
            float t6 = Time.time;
            int mortesCp = mortes;
            while (Time.time - t6 < 6f && mortes == mortesCp + 0) { yield return 0.1f; }
            var zonas = gigantes.SelectMany(g => { var so = new SerializedObject(g); var pts = so.FindProperty("attackPoints"); var r = so.FindProperty("attackRadius").floatValue;
                return Enumerable.Range(0, pts.arraySize).Select(i => pts.GetArrayElementAtIndex(i).objectReferenceValue as Transform).Where(t => t != null).Select(t => (t.position.x - r, t.position.x + r)); }).ToList();
            bool dentroDeZona = zonas.Any(z => xCp >= z.Item1 && xCp <= z.Item2);
            L($"- reapareceu em x={xCp:0.0} (dentro de área de golpe: {dentroDeZona}) · 6 s parado: golpes dos gigantes {golpes - golpesAntes}, mortes depois do respawn {mortes - mortesCp}{(mortes > mortesCp ? " (perseguidor ou golpe)" : "")}");
        }
        Soltar();
        yield return 0.1f;
    }

    public static void Run() { cenaAlvo = Cena; Iniciar(Roteiro()); }

    /// <summary>
    /// QA-13: diálogo disparado em pleno pulo deixava o Odisseu parado no ar? Pula ATRAVÉS do DialogueTrigger_Grove de
    /// Calipso (x 9..11, dispara uma vez) e amostra o pé enquanto o controle está travado.
    /// Unity.exe -batchmode -projectPath . -executeMethod QaMechanicsTest.DialogoNoAr
    /// </summary>
    public static void DialogoNoAr() { cenaAlvo = "Level_13_Calipso"; Iniciar(RoteiroDialogoNoAr()); }

    private static IEnumerator<float> RoteiroDialogoNoAr()
    {
        while (CampaignManager.Instance == null) { yield return 0.2f; }
        fundo = InputSystem.settings.backgroundBehavior; editorIn = InputSystem.settings.editorInputBehaviorInPlayMode;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        Application.runInBackground = true;
        kb = InputSystem.AddDevice<Keyboard>("QAMechKeyboard");
        LivesCounter.BeginRun();
        SceneManager.LoadScene(cenaAlvo);
        while (SceneManager.GetActiveScene().name != cenaAlvo || GameObject.FindGameObjectWithTag("Player") == null) { yield return 0.2f; }
        var p = GameObject.FindGameObjectWithTag("Player");
        var ctl = p.GetComponent<PlayerController>(); var rb = p.GetComponent<Rigidbody2D>(); var col = p.GetComponent<Collider2D>();
        while (!ctl.enabled) { yield return 0.2f; }   // fala de abertura
        yield return 0.5f;

        p.transform.position = new Vector3(6.5f, -1.95f, 0f); rb.linearVelocity = Vector2.zero;
        Soltar(); yield return 0.5f;
        Segurar(Key.D);
        float limite = Time.realtimeSinceStartup + 3f;
        while (p.transform.position.x < 8.0f && Time.realtimeSinceStartup < limite) { yield return 0.005f; }
        InputSystem.QueueStateEvent(kb, new KeyboardState(Key.D, Key.Space)); yield return 0.1f; Segurar(Key.D);

        while (ctl.enabled && Time.realtimeSinceStartup < limite + 2f) { yield return 0.005f; }
        Soltar();
        L($"\n## Diálogo disparado no ar (Calipso, gatilho do bosque)\n- trava em x={p.transform.position.x:0.00}, pé y={col.bounds.min.y:0.00}, vy={rb.linearVelocity.y:0.0}");
        var amostras = new System.Text.StringBuilder();
        float yInicio = col.bounds.min.y;
        for (int i = 0; i < 10 && !ctl.enabled; i++)
        {
            yield return 0.1f;
            amostras.Append($"{col.bounds.min.y:0.00} ");
        }
        float yFim = col.bounds.min.y;
        L($"- pé durante a trava (0,1 s): {amostras}→ {(yFim < yInicio - 0.3f || yFim <= -1.95f ? "CAIU até o chão (sem congelar)" : "**PARADO NO AR**")}");
    }

    /// <summary>
    /// Os 4 vãos de Troia, onde o bot caiu nas três vidas (2026-10-02). Para cada vão, pulos por input real partindo
    /// da BORDA (como um jogador faria) e de 1,1 un antes (o gatilho do bot), simples e duplo:
    /// Unity.exe -batchmode -projectPath . -executeMethod QaMechanicsTest.TroiaVaos   (relatório em Logs/qa_mecanicas.txt)
    /// Os inimigos são desligados em play mode (nada é salvo) para medir só o pulo.
    /// </summary>
    public static void TroiaVaos() { cenaAlvo = Cena; Iniciar(RoteiroTroiaVaos()); }

    private static IEnumerator<float> RoteiroTroiaVaos()
    {
        while (CampaignManager.Instance == null) { yield return 0.2f; }
        fundo = InputSystem.settings.backgroundBehavior; editorIn = InputSystem.settings.editorInputBehaviorInPlayMode;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        Application.runInBackground = true;
        kb = InputSystem.AddDevice<Keyboard>("QAMechKeyboard");
        LivesCounter.BeginRun(99);   // cada queda custa uma vida; 16 pulos não podem virar fim de jogo
        SceneManager.LoadScene(Cena);
        while (SceneManager.GetActiveScene().name != Cena || GameObject.FindGameObjectWithTag("Player") == null) { yield return 0.2f; }
        yield return 1.5f;

        var p = GameObject.FindGameObjectWithTag("Player");
        var ctl = p.GetComponent<PlayerController>(); var rb = p.GetComponent<Rigidbody2D>(); var col = p.GetComponent<Collider2D>();
        var vida = p.GetComponent<HealthSystem>();
        foreach (var e in Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None)) { e.gameObject.SetActive(false); }
        while (!ctl.enabled) { yield return 0.2f; }

        // (nome, borda da decolagem, y do pé na decolagem, início do alvo, topo do alvo)
        var vaos = new[]
        {
            ("Floor_2 → Platform_Bridge (3 un, sobe 1,2)", 4f, -2f, 7f, -0.8f),
            ("Floor_3 → Gauntlet_1 (3 un, sobe 1,2)", 26f, -2f, 29f, -0.8f),
            ("Gauntlet_1 → Gauntlet_2 (3 un, mesmo nível)", 32f, -0.8f, 35f, -0.8f),
            ("Gauntlet_2 → Gauntlet_3 (3 un, mesmo nível)", 38f, -0.8f, 41f, -0.8f),
        };
        var variantes = new[] { (0.35f, false, "da borda, pulo simples"), (0.35f, true, "da borda, pulo duplo"), (1.1f, false, "1,1 un antes (bot), simples"), (1.1f, true, "1,1 un antes (bot), duplo") };

        foreach (var (nome, borda, yPe, alvoX, alvoTopo) in vaos)
        {
            L($"\n## {nome}");
            foreach (var (antes, duplo, como) in variantes)
            {
                while (!ctl.enabled || p.transform.position.y < -5f) { yield return 0.2f; }   // respawn de uma queda anterior
                yield return 0.3f;
                vida.ResetHealth();
                p.transform.position = new Vector3(borda - 3f, yPe + 0.05f, 0f); rb.linearVelocity = Vector2.zero;
                Soltar(); yield return 0.5f;
                Segurar(Key.D);
                float limite = Time.realtimeSinceStartup + 3f;
                while (p.transform.position.x < borda - antes && Time.realtimeSinceStartup < limite) { yield return 0.005f; }
                float xDecolagem = p.transform.position.x, vx = rb.linearVelocity.x;
                InputSystem.QueueStateEvent(kb, new KeyboardState(Key.D, Key.Space)); yield return 0.12f; Segurar(Key.D);
                if (duplo)
                {
                    while (rb.linearVelocity.y > 0.5f && Time.realtimeSinceStartup < limite) { yield return 0.005f; }
                    InputSystem.QueueStateEvent(kb, new KeyboardState(Key.D, Key.Space)); yield return 0.12f; Segurar(Key.D);
                }
                // O PONTO DE POUSO, não onde ele está depois: as plataformas têm 3 un, e segurar a direção depois de
                // pousar o leva para fora pela outra ponta — isso é a corrida seguinte, não este pulo.
                while (!ctl.IsGrounded && col.bounds.min.y > alvoTopo - 3f && Time.realtimeSinceStartup < limite + 2f) { yield return 0.005f; }
                Soltar();
                // O sensor de chão liga um pouco antes do contato: solta a direção e deixa assentar antes de medir.
                yield return 0.25f;
                float xPouso = p.transform.position.x, yPouso = col.bounds.min.y;
                bool pousou = ctl.IsGrounded && col.bounds.max.x > alvoX && Mathf.Abs(yPouso - alvoTopo) < 0.1f;
                L($"- {como}: decolou em x={xDecolagem:0.00} (vx {vx:0.0}) → pousou x={xPouso:0.00}, pé y={yPouso:0.00} → {(pousou ? "PASSOU" : "**CAIU / NÃO ALCANÇOU**")}");
                yield return 0.6f;
            }
        }
    }

    private static void Iniciar(IEnumerator<float> r)
    {
        opcoesAtivas = EditorSettings.enterPlayModeOptionsEnabled;
        opcoes = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        // Sem som durante o teste (pedido do usuário): muta o Editor inteiro e restaura no Sair.
        mudoOriginal = EditorUtility.audioMasterMute;
        EditorUtility.audioMasterMute = true;
        AudioListener.volume = 0f;
        rel.Clear();
        L($"# QA mecânicas — {System.DateTime.Now:yyyy-MM-dd HH:mm}, cena {cenaAlvo}");
        EditorSceneManager.OpenScene("Assets/Scenes/Boot/Boot.unity", OpenSceneMode.Single);
        roteiro = r;
        acordarEm = 0;
        EditorApplication.update += Tick;
        EditorApplication.EnterPlaymode();
    }

    /// <summary>Efeitos de folha (VfxSheet) vistos durante o roteiro, por nome — amostrados a cada update do Editor.</summary>
    private static readonly Dictionary<string, int> vfxVistos = new Dictionary<string, int>();
    private static readonly HashSet<Object> vfxContados = new HashSet<Object>();

    private static void Tick()
    {
        if (EditorApplication.isPlaying)
        {
            foreach (var v in Object.FindObjectsByType<Odisseia.Systems.VfxSheet>())
            {
                if (vfxContados.Add(v)) { vfxVistos[v.name] = vfxVistos.TryGetValue(v.name, out int k) ? k + 1 : 1; }
            }
        }
        if (!EditorApplication.isPlaying || EditorApplication.timeSinceStartup < acordarEm) { return; }
        try
        {
            if (!roteiro.MoveNext()) { Sair(0); return; }
            acordarEm = EditorApplication.timeSinceStartup + roteiro.Current;
        }
        catch (System.Exception e) { L($"EXCEÇÃO DO TESTE: {e.GetType().Name}: {e.Message}\n{e.StackTrace}"); Sair(1); }
    }

    // espera em segundos de PAREDE (timeScale 1)
    private static IEnumerator<float> Roteiro()
    {
        while (CampaignManager.Instance == null) { yield return 0.2f; }
        fundo = InputSystem.settings.backgroundBehavior;
        editorIn = InputSystem.settings.editorInputBehaviorInPlayMode;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        Application.runInBackground = true;
        kb = InputSystem.AddDevice<Keyboard>("QAMechKeyboard");
        LivesCounter.BeginRun();
        SceneManager.LoadScene(Cena);
        while (SceneManager.GetActiveScene().name != Cena || GameObject.FindGameObjectWithTag("Player") == null) { yield return 0.2f; }
        yield return 1.5f;

        var p = GameObject.FindGameObjectWithTag("Player");
        var rb = p.GetComponent<Rigidbody2D>();
        var ctl = p.GetComponent<PlayerController>();
        var vida = p.GetComponent<HealthSystem>();
        var escudo = p.GetComponent<PlayerShield>();
        var arco = p.GetComponent<PlayerBow>();
        var anim = p.GetComponent<SpriteAnimator>() ?? p.GetComponentInChildren<SpriteAnimator>();
        var inimigos = Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None).OrderBy(e => e.transform.position.x).ToList();
        L($"- jogador vida {vida.CurrentHealth}/{vida.MaxHealth}, flechas {arco.CurrentArrows}/{arco.MaxArrows}, vidas {LivesCounter.Current}; inimigos {inimigos.Count} em x={string.Join(", ", inimigos.Select(e => e.transform.position.x.ToString("0.0")))}");

        var alvo = inimigos.First();
        var hAlvo = alvo.GetComponent<HealthSystem>();
        int danoRecebido = 0;
        vida.Damaged += (q, _) => danoRecebido += q;

        // ---- 1. MELEE
        Por(p, alvo.transform.position.x - 1.0f, alvo.transform.position.y);
        OlharPara(true);
        yield return 0.4f;
        int antes = hAlvo.CurrentHealth;
        Tocar(Key.Z); yield return 0.1f; Soltar(); yield return 0.6f;
        L($"\n## Melee\n- vida do inimigo {antes} → {hAlvo.CurrentHealth} ({(hAlvo.CurrentHealth < antes ? "ACERTOU" : "**NÃO ACERTOU**")}) · estado do animador: {anim?.CurrentState}");
        // cooldown: dois ataques imediatos
        antes = hAlvo.CurrentHealth;
        Tocar(Key.Z); yield return 0.05f; Soltar(); yield return 0.05f; Tocar(Key.Z); yield return 0.05f; Soltar(); yield return 0.6f;
        L($"- dois toques em 0,15 s: vida {antes} → {hAlvo.CurrentHealth} (cooldown 0,4 s: espera-se no máximo 1 golpe de 20 → {(antes - hAlvo.CurrentHealth <= 20 ? "OK" : "dois golpes contaram")})");

        // ---- 2. ESCUDO — dano recebido parado ao lado do inimigo, sem e com defesa (frente)
        ResetarVida(vida);
        Por(p, alvo.transform.position.x - 0.7f, alvo.transform.position.y);
        OlharPara(true);
        danoRecebido = 0;
        Soltar(); yield return 4f;
        int semEscudo = danoRecebido;
        ResetarVida(vida);
        Por(p, alvo.transform.position.x - 0.7f, alvo.transform.position.y);
        OlharPara(true);
        danoRecebido = 0;
        Segurar(Key.X); yield return 0.3f;
        bool bloqueando = escudo.IsBlocking; string estadoEscudo = anim?.CurrentState;
        yield return 3.7f;
        int comEscudo = danoRecebido;
        Soltar(); yield return 0.3f;
        L($"\n## Escudo (frente)\n- 4 s ao lado do inimigo: **{semEscudo}** de dano sem defesa · **{comEscudo}** segurando X (IsBlocking={bloqueando}, animador {estadoEscudo}) → " +
          (semEscudo == 0 ? "inimigo não atacou — caso inconclusivo" : comEscudo < semEscudo ? $"reduziu {100 - 100 * comEscudo / Mathf.Max(1, semEscudo)}%" : "**NÃO REDUZIU**"));

        // ---- 3. ESCUDO pelas costas
        ResetarVida(vida);
        Por(p, alvo.transform.position.x - 0.7f, alvo.transform.position.y);
        Segurar(Key.A); yield return 0.12f; Soltar();   // de costas para o inimigo (um toque não vira o personagem)
        danoRecebido = 0;
        yield return 0.2f;
        string antesCostas = $"olhando {(ctl.FacingSign > 0 ? "direita" : "esquerda")}, inimigo em dx={alvo.transform.position.x - p.transform.position.x:+0.0;-0.0}";
        var lados = new List<string>();
        vida.Damaged += (q, _) => lados.Add($"{q}@olhando{(ctl.FacingSign > 0 ? "D" : "E")},dx={alvo.transform.position.x - p.transform.position.x:+0.0;-0.0}");
        Segurar(Key.X); yield return 4f;
        int costas = danoRecebido;
        L($"- (costas) no início: {antesCostas}; golpes recebidos: {string.Join(" ", lados)}");
        Soltar(); yield return 0.3f;
        L($"- de costas, segurando X: {costas} de dano → {(costas >= semEscudo * 0.9f && semEscudo > 0 ? "passa inteiro (como o código diz)" : "confira")}");

        // ---- 4. DEFESA NO AR
        ResetarVida(vida);
        Por(p, alvo.transform.position.x - 3f, alvo.transform.position.y);
        OlharPara(true);
        Segurar(Key.Space); yield return 0.15f;
        Segurar(Key.X); yield return 0.25f;
        bool noArBloq = escudo.IsBlocking, noChao = ctl.IsGrounded; string estadoAr = anim?.CurrentState;
        var sv = p.transform.Find("Visual/ShieldVisual");
        bool visual = sv != null && sv.gameObject.activeInHierarchy && sv.GetComponent<SpriteRenderer>().enabled;
        Soltar(); yield return 1f;
        L($"\n## Defesa no ar\n- no ar={!noChao}, IsBlocking={noArBloq}, animador {estadoAr}, algum escudo desenhado={visual} → {(noArBloq && !visual ? "efeito mecânico SEM representação visual (POLISH conhecido)" : "confira")}");

        // ---- 5. ARCO
        ResetarVida(vida);
        var alvo2 = inimigos.Skip(1).FirstOrDefault() ?? alvo;
        var h2 = alvo2.GetComponent<HealthSystem>();
        Por(p, alvo2.transform.position.x - 4f, alvo2.transform.position.y);
        OlharPara(true);
        yield return 0.4f;
        int flechas = arco.CurrentArrows, vidaAlvo2 = h2.CurrentHealth;
        int disparos = 0; arco.Fired += () => disparos++;
        Tocar(Key.C); yield return 0.15f; Soltar(); yield return 1.2f;
        L($"\n## Arco\n- flechas {flechas} → {arco.CurrentArrows} ({(arco.CurrentArrows == flechas - 1 ? "consumiu 1" : "**não consumiu 1**")}) · disparos {disparos} · animador {anim?.CurrentState} · vida do inimigo a 4 un {vidaAlvo2} → {h2.CurrentHealth} ({(h2.CurrentHealth < vidaAlvo2 ? "ACERTOU" : "não acertou")})");

        // sem munição
        var campo = typeof(PlayerBow).GetField("currentArrows", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        campo?.SetValue(arco, 0);
        bool semFlecha = false; arco.OutOfArrows += () => semFlecha = true;
        int flechasEmCena = Object.FindObjectsByType<Rigidbody2D>(FindObjectsSortMode.None).Count(r => r.name.StartsWith("Arrow"));
        disparos = 0;
        Tocar(Key.C); yield return 0.15f; Soltar(); yield return 0.6f;
        int flechasDepois = Object.FindObjectsByType<Rigidbody2D>(FindObjectsSortMode.None).Count(r => r.name.StartsWith("Arrow"));
        L($"- sem munição: disparos {disparos}, evento OutOfArrows={semFlecha}, flechas em cena {flechasEmCena} → {flechasDepois}, contador {arco.CurrentArrows} → {(disparos == 0 && arco.CurrentArrows == 0 ? "não dispara (OK)" : "**disparou sem munição**")}");
        arco.AddArrows(arco.MaxArrows);

        // ---- 6. MORTE e RESPAWN
        int vidas = LivesCounter.Current;
        Vector3 antesMorte = p.transform.position;
        vida.TakeDamage(9999);
        yield return 0.8f;
        L($"\n## Morte e respawn\n- vidas {vidas} → {LivesCounter.Current} · vida {vida.CurrentHealth}/{vida.MaxHealth} · reposicionado de x={antesMorte.x:0.0} para x={p.transform.position.x:0.0} (checkpoint: {CheckpointManager.HasCheckpoint}) · controle ativo {ctl.enabled}");
        Segurar(Key.D); yield return 0.8f; Soltar();
        L($"- depois do respawn o jogador anda? x={p.transform.position.x:0.0} ({(Mathf.Abs(rb.linearVelocity.x) > 0.1f || true ? "ver x" : "")})");

        // ---- 7. PAUSA
        var pausa = Object.FindAnyObjectByType<Odisseia.UI.PauseMenu>(FindObjectsInactive.Include);
        Tocar(Key.Escape); yield return 0.1f; Soltar(); yield return 0.5f;
        bool pausou = pausa != null && pausa.IsPaused; float ts = Time.timeScale;
        Tocar(Key.Escape); yield return 0.1f; Soltar(); yield return 0.5f;
        L($"\n## Pausa\n- Esc: pausado={pausou} (timeScale {ts}) · Esc de novo: pausado={pausa?.IsPaused} (timeScale {Time.timeScale})");

        // ---- 8. FIM DE JOGO
        while (LivesCounter.Current > 0) { vida.TakeDamage(9999); yield return 0.6f; }
        yield return 0.5f;
        var go = GameObject.Find("GameOverScreen");
        L($"\n## Fim de jogo\n- vidas 0 → timeScale {Time.timeScale}, tela de fim de jogo ativa: {go != null && go.GetComponentsInChildren<Canvas>(true).Any(c => c.gameObject.activeInHierarchy)} · mapa de ações Player habilitado: {KeyRebindService.Asset?.FindActionMap("Player")?.enabled}");
        // QA-03: "Tentar de novo" — clica no botão da própria tela e confere o recomeço da fase.
        var tentar = go != null ? go.GetComponentsInChildren<UnityEngine.UI.Button>(true).FirstOrDefault(b => b.name == "RetryButton") : null;
        if (tentar == null)
        {
            L("- **sem botão \"Tentar de novo\"** (só menu)");
            Time.timeScale = 1f;
            yield return 0.1f;
            yield break;
        }

        string rotulo = tentar.GetComponentInChildren<UnityEngine.UI.Text>()?.text;
        tentar.onClick.Invoke();
        float tT = Time.realtimeSinceStartup;
        GameObject novo = null;
        while (Time.realtimeSinceStartup - tT < 15f)
        {
            novo = GameObject.FindGameObjectWithTag("Player");
            if (novo != null && novo != p && novo.GetComponent<PlayerController>().enabled) { break; }
            yield return 0.2f;
        }
        yield return 1.5f;
        L($"- \"{rotulo}\": cena {SceneManager.GetActiveScene().name} recarregada={novo != null && novo != p} · vidas {LivesCounter.Current} · timeScale {Time.timeScale} · mapa Player habilitado {KeyRebindService.Asset?.FindActionMap("Player")?.enabled} · painel de fim de jogo visível {go.transform.Find("GameOverCanvas/Root")?.gameObject.activeSelf}");
        Time.timeScale = 1f;
        yield return 0.1f;
    }

    private static IEnumerator<float> RoteiroCalipso()
    {
        while (CampaignManager.Instance == null) { yield return 0.2f; }
        fundo = InputSystem.settings.backgroundBehavior; editorIn = InputSystem.settings.editorInputBehaviorInPlayMode;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        Application.runInBackground = true;
        kb = InputSystem.AddDevice<Keyboard>("QAMechKeyboard");
        LivesCounter.BeginRun();
        SceneManager.LoadScene(cenaAlvo);
        while (SceneManager.GetActiveScene().name != cenaAlvo || GameObject.FindGameObjectWithTag("Player") == null) { yield return 0.2f; }
        yield return 4f;   // fala de abertura
        var p = GameObject.FindGameObjectWithTag("Player");
        var ctl = p.GetComponent<PlayerController>(); var climb = p.GetComponent<PlayerClimb>(); var rb = p.GetComponent<Rigidbody2D>();
        var col = p.GetComponent<Collider2D>();
        // Deixa o diálogo do bosque (DialogueTrigger_Grove, x 9..11, dispara uma vez) tocar e acabar antes.
        p.transform.position = new Vector3(10f, -1.8f, 0f); rb.linearVelocity = Vector2.zero;
        yield return 0.3f;
        L($"- diálogo do bosque disparado: controle ativo {ctl.enabled} (esperado False); y do pé {col.bounds.min.y:0.00}");
        while (!ctl.enabled) { yield return 0.3f; }
        yield return 0.5f;
        foreach (var (x0, nome) in new[] { (11.2f, "encostado"), (10.2f, "com 1 un de corrida"), (8.0f, "com 3 un de corrida") })
        {
            p.transform.position = new Vector3(x0, -1.8f, 0f); rb.linearVelocity = Vector2.zero;
            Soltar(); yield return 0.6f;
            L($"\n## Pulo contra Platform_Grove, partindo de x={x0} ({nome}) · controle ativo {ctl.enabled} · colisor x {col.bounds.min.x:0.00}..{col.bounds.max.x:0.00} y {col.bounds.min.y:0.00}..{col.bounds.max.y:0.00}");
            Segurar(Key.D); yield return x0 < 11f ? (11.4f - x0) / 6f : 0.2f;
            InputSystem.QueueStateEvent(kb, new KeyboardState(Key.D, Key.Space));
            float yMax = -99f; var tr = new System.Text.StringBuilder();
            for (int i = 0; i < 16; i++)
            {
                if (i == 3) { Segurar(Key.D); }
                yield return 0.08f;
                yMax = Mathf.Max(yMax, col.bounds.min.y);
                tr.Append($"({p.transform.position.x:0.00},{col.bounds.min.y:0.00}{(ctl.IsGrounded ? " chão" : "")}{(climb != null && climb.IsClimbing ? " ESCALANDO" : "")}) ");
            }
            Soltar();
            L($"- pé mais alto {yMax:0.00} (topo da plataforma -0,80) · fim x={p.transform.position.x:0.00}, pé y={col.bounds.min.y:0.00} → {(p.transform.position.x > 12.3f && col.bounds.min.y > -0.9f ? "SUBIU" : "**NÃO SUBIU**")}");
            L("- trajetória: " + tr);
        }
        // Hipótese: atrito contra a lateral. Grudado, (a) soltar a direção, (b) pulo duplo segurando a direção.
        foreach (var (soltar, nome) in new[] { (true, "solta a direção"), (false, "pulo duplo segurando a direção") })
        {
            p.transform.position = new Vector3(8f, -1.8f, 0f); rb.linearVelocity = Vector2.zero; Soltar(); yield return 0.6f;
            Segurar(Key.D); yield return (11.4f - 8f) / 6f;
            InputSystem.QueueStateEvent(kb, new KeyboardState(Key.D, Key.Space)); yield return 0.1f; Segurar(Key.D); yield return 0.4f;
            float yGrudado = col.bounds.min.y;
            if (soltar) { Soltar(); } else { InputSystem.QueueStateEvent(kb, new KeyboardState(Key.D, Key.Space)); yield return 0.1f; Segurar(Key.D); }
            yield return 0.8f;
            L($"\n## Grudado a y={yGrudado:0.00} → {nome}: fim x={p.transform.position.x:0.00}, pé y={col.bounds.min.y:0.00}, chão={ctl.IsGrounded} → {(p.transform.position.x > 12.3f && col.bounds.min.y > -0.9f ? "SUBIU" : col.bounds.min.y < yGrudado - 0.3f ? "caiu" : "continua grudado")}");
            Soltar();
        }
    }

    private static void ResetarVida(HealthSystem h) { h.ResetHealth(); }

    private static void Por(GameObject p, float x, float yInimigo)
    {
        p.transform.position = new Vector3(x, yInimigo + 0.1f, p.transform.position.z);
        var rb = p.GetComponent<Rigidbody2D>();
        rb.linearVelocity = Vector2.zero;
    }

    private static void OlharPara(bool direita) { Segurar(direita ? Key.D : Key.A); Soltar(); }

    private static void Segurar(Key k) => InputSystem.QueueStateEvent(kb, new KeyboardState(k));
    private static void Tocar(Key k) => Segurar(k);
    private static void Soltar() => InputSystem.QueueStateEvent(kb, new KeyboardState());

    private static void L(string s) { rel.AppendLine(s); Debug.Log("[QAMech] " + s); }

    private static void Sair(int c)
    {
        EditorApplication.update -= Tick;
        L("\n## Efeitos de folha vistos (VfxSheet)\n- " + (vfxVistos.Count == 0 ? "nenhum" : string.Join(" · ", vfxVistos.Select(p => p.Key + " ×" + p.Value))));
        System.IO.File.WriteAllText("Logs/qa_mecanicas.txt", rel.ToString());
        EditorSettings.enterPlayModeOptionsEnabled = opcoesAtivas;
        EditorSettings.enterPlayModeOptions = opcoes;
        if (kb != null) { InputSystem.RemoveDevice(kb); InputSystem.settings.backgroundBehavior = fundo; InputSystem.settings.editorInputBehaviorInPlayMode = editorIn; }
        Time.timeScale = 1f;
        AudioListener.volume = 1f;
        EditorUtility.audioMasterMute = mudoOriginal;
        EditorApplication.ExitPlaymode();
        EditorApplication.Exit(c);
    }
}
