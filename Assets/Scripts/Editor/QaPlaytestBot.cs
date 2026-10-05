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
using Odisseia.Levels;
using Odisseia.Player;
using Odisseia.Systems;
using Odisseia.WorldMap;

/// <summary>
/// FULL CAMPAIGN PLAYTEST — um bot que JOGA as fases em play mode, pelo mesmo caminho do jogador: um teclado
/// virtual do Input System, as mesmas ações (Move, Jump, Attack, Interact, Shield, Bow). Nada é teleportado e
/// nenhum objetivo é marcado por código: se a fase fecha, foi jogando.
///
/// Unity.exe -batchmode -projectPath . -executeMethod QaPlaytestBot.Run [-qaFrom 1] [-qaTo 16] [-qaTimeScale 2] [-qaOlhar 1.1]
/// (sem -quit: o bot encerra sozinho; relatório em Logs/qa_playtest.txt)
///
/// Comportamento (deliberadamente simples, para ser reproduzível): segura "direita"; pula quando fica parado
/// contra algo ou quando não há chão à frente (pulo duplo se preciso); aperta Interagir a cada 0,3 s; ataca inimigo
/// vivo à frente. Quando para de avançar, tenta recuar e voltar; se nada adianta por muito tempo, registra PRESO e
/// passa para a próxima fase. Diálogo/cutscene (controle desligado) não conta como preso.
///
/// Depois da saída de cada fase confere a TRANSIÇÃO: a cena seguinte, o desbloqueio da próxima fase e o nó em que o
/// mapa deixa o Odisseu. Também registra: spawn, câmera seguindo, HUD, objetivos, mortes, erros/exceções no console.
///
/// ATENÇÃO: usa o save do Editor (PlayerPrefs) — apaga-o no início. Não toca no save do build Web.
/// </summary>
public static class QaPlaytestBot
{
    private static readonly string[] Fases =
    {
        "Level_01_Itaca_Prologue", "Level_02_Troia", "Level_03_Cicones", "Level_04_Citera", "Level_05_Ciclopes",
        "Level_06_Eolo", "Level_07_Lestrigoes", "Level_08_Circe", "Level_09_MundoDosMortos", "Level_10_Sereias",
        "Level_11_CilaCaribdis", "Level_12_GadoDoSol", "Level_13_Calipso", "Level_14_Itaca_Return",
        "Level_15_Pretendentes", "Level_16_Final",
    };

    private const int LayerChao = 8;
    private const float LimiteDeJogo = 420f;        // segundos de JOGO por fase
    private const float SemProgressoDesiste = 60f;  // segundos de jogo sem avançar em x

    private enum Passo { Boot, Carregar, Jogar, Saida, Mapa, Fim }

    private static Passo passo;
    private static int indice, ate;
    private static double marco;
    private static float escalaDeTempo = 2f;
    private static float olharFrente = 1.1f;
    private static bool segundoPuloUsado;
    private static float traceDe = float.NaN, traceAte;

    /// <summary>
    /// No alto do primeiro pulo: projeta a queda à frente (velocidade horizontal real, gravidade efetiva do corpo) e
    /// procura onde o pé cai. Pousa → não precisa. Bate na LATERAL de algo mais alto que o pé naquele ponto, ou não acha
    /// chão em 5 un → precisa do segundo pulo. É o que um jogador faz olhando a plataforma chegar.
    /// </summary>
    private static bool PrecisaDoSegundoPulo(Rigidbody2D corpo, float direcao)
    {
        float vx = Mathf.Max(Mathf.Abs(corpo.linearVelocity.x), 3f);
        float vy = corpo.linearVelocity.y;
        float g = Mathf.Abs(Physics2D.gravity.y * corpo.gravityScale);
        Vector2 pe = corpo.position;
        for (float d = 0.1f; d <= 6f; d += 0.1f)
        {
            float tempo = d / vx;
            float peY = pe.y + vy * tempo - 0.5f * g * tempo * tempo;
            if (peY < pe.y - 6f) { break; }

            // Na BORDA DA FRENTE da cápsula (meia largura 0,3): é ela que chega ao canto da plataforma, e o fundo
            // arredondado desliza para cima dele — o pouso medido no QaMechanicsTest.TroiaVaos.
            float x = pe.x + direcao * (d + 0.3f);

            // O CORPO (do pé até a cabeça, 1,4 un) bateria em algo ali? Lateral de plataforma → precisa subir. Só o corpo
            // conta: teto ou rocha acima da cabeça não bloqueia (a primeira versão olhava 4 un acima e confundia teto
            // com parede, gastando o segundo pulo perto do chão).
            if (Physics2D.OverlapBox(new Vector2(x, peY + 0.8f), new Vector2(0.05f, 1.2f), 0f, 1 << LayerChao) != null)
            {
                return true;
            }

            // Chão logo abaixo do pé: pousa aqui.
            if (Physics2D.Raycast(new Vector2(x, peY + 0.05f), Vector2.down, 0.2f, 1 << LayerChao).collider != null)
            {
                return false;
            }
        }
        return true;   // nada para pousar à frente
    }
    private static bool opcoesOriginaisAtivas;
    private static EnterPlayModeOptions opcoesOriginais;
    private static Keyboard teclado;
    private static bool mudoOriginal;
    private static InputSettings.BackgroundBehavior fundoOriginal;
    private static InputSettings.EditorInputBehaviorInPlayMode editorOriginal;
    private static readonly StringBuilder relatorio = new StringBuilder();

    // estado da fase
    private static Transform jogador;
    private static Rigidbody2D rb;
    private static PlayerController controle;
    private static HealthSystem vida;
    private static LevelManager nivel;
    private static float inicioTrava;
    private static bool varrendo;
    private static float grudadoDesde = -1f, soltarAte;
    private static bool avisouGrudado;
    private static Vector3 ultimaPos;
    private static bool travaLonga;
    private static float inicio, ultimoAvanco, xMax, proximoE, proximoZ, pausaAte, pularAte, recuarAte, ultimaCamera;
    private static int saltosSemAvanco, mortes, tentativasDeRecuo;
    private static readonly List<string> eventos = new List<string>();
    private static readonly List<string> erros = new List<string>();
    private static readonly HashSet<Key> segurando = new HashSet<Key>();

    public static void Run()
    {
        string[] a = System.Environment.GetCommandLineArgs();
        int De(string n, int d) { int i = System.Array.IndexOf(a, n); return i >= 0 && int.TryParse(a[i + 1], out int v) ? v : d; }
        indice = Mathf.Clamp(De("-qaFrom", 1), 1, 16) - 1;
        ate = Mathf.Clamp(De("-qaTo", 16), 1, 16) - 1;
        int k = System.Array.IndexOf(a, "-qaTimeScale");
        if (k >= 0) { float.TryParse(a[k + 1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out escalaDeTempo); }
        // -qaOlhar: a que distância à frente o bot procura chão para decidir o pulo. O padrão 1,1 é o das rodadas
        // registradas; 0,5 pula da borda, como um jogador (medido em QaMechanicsTest.TroiaVaos).
        olharFrente = 1.1f;
        // -qaTrace x0,x1: registra no log, quadro a quadro, posição, velocidade, chão e pulo dentro dessa faixa de x.
        traceDe = float.NaN;
        int tr = System.Array.IndexOf(a, "-qaTrace");
        if (tr >= 0)
        {
            string[] faixa = a[tr + 1].Split(',');
            float.TryParse(faixa[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out traceDe);
            float.TryParse(faixa[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out traceAte);
        }
        int o = System.Array.IndexOf(a, "-qaOlhar");
        if (o >= 0) { float.TryParse(a[o + 1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out olharFrente); }

        opcoesOriginaisAtivas = EditorSettings.enterPlayModeOptionsEnabled;
        opcoesOriginais = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        PlayerPrefs.DeleteKey("Odisseia.Save");
        PlayerPrefs.Save();

        // Sem som durante o teste (pedido do usuário): muta o Editor inteiro e restaura no Sair.
        mudoOriginal = EditorUtility.audioMasterMute;
        EditorUtility.audioMasterMute = true;
        relatorio.Clear();
        Linha($"# QA playtest bot — fases {indice + 1}..{ate + 1}, timeScale {escalaDeTempo}, {System.DateTime.Now:yyyy-MM-dd HH:mm}");
        Application.logMessageReceived += AoLogar;
        EditorSceneManager.OpenScene("Assets/Scenes/Boot/Boot.unity", OpenSceneMode.Single);
        passo = Passo.Boot;
        marco = EditorApplication.timeSinceStartup;
        EditorApplication.update += Tick;
        EditorApplication.EnterPlaymode();
    }

    private static void AoLogar(string msg, string pilha, LogType tipo)
    {
        if ((tipo == LogType.Error || tipo == LogType.Exception || tipo == LogType.Assert) && !msg.StartsWith("[QA]"))
        {
            string curta = msg.Split('\n')[0];
            if (erros.Count < 40) { erros.Add($"{tipo}: {curta}"); }
        }
    }

    private static double Parede => EditorApplication.timeSinceStartup - marco;

    private static void Tick()
    {
        if (!EditorApplication.isPlaying) { return; }
        try
        {
            switch (passo)
            {
                case Passo.Boot:
                    if (CampaignManager.Instance == null) { if (Parede > 60) { Linha("BOOT não subiu o CampaignManager"); Sair(1); } return; }
                    if (teclado == null)
                    {
                        // Batchmode não tem foco: sem isto o Input System descarta a entrada do teclado virtual.
                        // É um asset do projeto — restaurado no Sair.
                        fundoOriginal = InputSystem.settings.backgroundBehavior;
                        editorOriginal = InputSystem.settings.editorInputBehaviorInPlayMode;
                        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                        Application.runInBackground = true;
                        teclado = InputSystem.AddDevice<Keyboard>("QABotKeyboard");
                    }
                    Time.timeScale = escalaDeTempo;
                    AudioListener.volume = 0f;
                    CarregarFase();
                    return;
                case Passo.Carregar: Carregando(); return;
                case Passo.Jogar: Jogando(); return;
                case Passo.Saida: Saindo(); return;
                case Passo.Mapa: NoMapa(); return;
            }
        }
        catch (System.Exception e)
        {
            Linha($"EXCEÇÃO DO BOT no passo {passo}: {e.GetType().Name}: {e.Message}");
            Sair(1);
        }
    }

    // ------------------------------------------------------------------ fase

    private static void CarregarFase()
    {
        LivesCounter.BeginRun();
        CheckpointManager.Reset();
        erros.Clear();
        eventos.Clear();
        jogador = null;
        SceneManager.LoadScene(Fases[indice]);
        passo = Passo.Carregar;
        marco = EditorApplication.timeSinceStartup;
    }

    private static void Carregando()
    {
        if (SceneManager.GetActiveScene().name != Fases[indice]) { if (Parede > 60) { Fechar("NÃO CARREGOU", false); } return; }
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        nivel = Object.FindAnyObjectByType<LevelManager>();
        if (p == null || nivel == null) { if (Parede > 30) { Fechar(p == null ? "SEM JOGADOR" : "SEM LevelManager", false); } return; }

        jogador = p.transform;
        rb = p.GetComponent<Rigidbody2D>();
        controle = p.GetComponent<PlayerController>();
        vida = p.GetComponent<HealthSystem>();
        ultimaPos = p.transform.position;
        mortes = 0;
        vida.Died += () => { mortes++; eventos.Add($"t={Time.time - inicio:0}s morte em x={ultimaPos.x:0.0}, y={ultimaPos.y:0.0} (vidas {LivesCounter.Current}) — {CausaProvavel(ultimaPos)}"); };
        inicio = ultimoAvanco = ultimaCamera = inicioTrava = Time.time;
        travaLonga = false;
        grudadoDesde = -1f; soltarAte = 0f; avisouGrudado = false; varrendo = false;
        Time.timeScale = escalaDeTempo;
        KeyRebindService.Asset?.FindActionMap("Player")?.Enable();
        xMax = jogador.position.x;
        proximoE = proximoZ = pausaAte = pularAte = recuarAte = 0f;
        saltosSemAvanco = tentativasDeRecuo = 0;

        var cam = Camera.main;
        var hud = Object.FindAnyObjectByType<Odisseia.UI.HUD>();
        Linha($"\n## {indice + 1:00} {Fases[indice]}");
        Linha($"- carregou: sim · spawn ({jogador.position.x:0.0}, {jogador.position.y:0.0}) · câmera {(cam != null ? "sim" : "NÃO")} · HUD {(hud != null && hud.isActiveAndEnabled ? "sim" : "NÃO")} · " +
              $"inimigos {Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None).Length} · objetivos {Object.FindObjectsByType<LevelObjective>(FindObjectsSortMode.None).Length} · " +
              $"saída {(Object.FindAnyObjectByType<LevelGoal>(FindObjectsInactive.Include) != null ? "sim" : "NÃO")}");
        passo = Passo.Jogar;
    }

    private static void Jogando()
    {
        float t = Time.time;
        if (!vida.IsDead) { ultimaPos = jogador.position; }
        if (nivel != null && nivel.IsCompleted)
        {
            Linha($"- **concluída** em {t - inicio:0} s de jogo · mortes {mortes} · x final {jogador.position.x:0.0}");
            Soltar();
            passo = Passo.Saida;
            marco = EditorApplication.timeSinceStartup;
            return;
        }
        if (LivesCounter.IsGameOver)
        {
            // A GameOverScreen congela o tempo e desliga o mapa Player: desfaz, senão as fases seguintes rodam congeladas.
            var tela = GameObject.Find("GameOverScreen");
            foreach (var c in tela != null ? tela.GetComponentsInChildren<Canvas>(true) : new Canvas[0]) { c.gameObject.SetActive(false); }
            KeyRebindService.Asset?.FindActionMap("Player")?.Enable();
            Time.timeScale = escalaDeTempo;
            Fechar($"GAME OVER em x={jogador.position.x:0.0}", true);
            return;
        }
        if (t - inicio > LimiteDeJogo) { Fechar($"TEMPO ESGOTADO ({LimiteDeJogo:0} s) em x={jogador.position.x:0.0}, x máx {xMax:0.0}", true); return; }

        // câmera seguindo
        if (t - ultimaCamera > 2f && Camera.main != null)
        {
            ultimaCamera = t;
            float dx = Mathf.Abs(Camera.main.transform.position.x - jogador.position.x);
            float dy = jogador.position.y - Camera.main.transform.position.y;
            float meia = Camera.main.orthographicSize;
            if (dx > meia * Camera.main.aspect * 0.9f || Mathf.Abs(dy) > meia * 0.95f)
            {
                Evento($"jogador FORA DA CÂMERA em ({jogador.position.x:0.0},{jogador.position.y:0.0}), câmera ({Camera.main.transform.position.x:0.0},{Camera.main.transform.position.y:0.0})");
            }
        }

        bool travado = controle == null || !controle.enabled || controle.MovementSuspended;
        if (travado)
        {
            // Diálogo/cutscene não é "preso". E NÃO se aperta Interagir: os diálogos avançam sozinhos, e apertar
            // ao lado do NPC reabria a conversa sem fim (foi o que prendeu a 1ª rodada na Penélope).
            Soltar();
            ultimoAvanco = t;
            proximoE = t + 3f;   // ao destravar, 3 s sem Interagir: tempo de sair do alcance do NPC
            if (t - inicioTrava > 60f && !travaLonga) { travaLonga = true; Evento($"controle TRAVADO há mais de 60 s em x={jogador.position.x:0.0}"); }
            return;
        }
        inicioTrava = t;
        travaLonga = false;

        if (jogador.position.x > xMax + 0.25f) { xMax = jogador.position.x; ultimoAvanco = t; saltosSemAvanco = 0; tentativasDeRecuo = 0; }

        var teclas = new HashSet<Key>();
        bool recuando = t < recuarAte;
        // Grudado na lateral no ar (atrito do colisor contra a parede enquanto segura a direção): solta a direção um
        // instante, como um jogador faria. Registrado uma vez por fase.
        bool noAr = !controle.IsGrounded && Mathf.Abs(rb.linearVelocity.y) < 0.05f && Mathf.Abs(rb.linearVelocity.x) < 0.1f;
        if (noAr) { if (grudadoDesde < 0f) { grudadoDesde = t; } } else { grudadoDesde = -1f; }
        if (grudadoDesde >= 0f && t - grudadoDesde > 0.3f)
        {
            soltarAte = t + 0.25f; grudadoDesde = -1f;
            if (!avisouGrudado) { avisouGrudado = true; Evento($"GRUDADO na parede no ar em ({jogador.position.x:0.0},{jogador.position.y:0.0}) — soltou a direção"); }
        }
        if (t >= soltarAte) { teclas.Add(recuando ? Key.A : Key.D); }

        // pular: parado contra algo, ou sem chão à frente
        bool noChao = controle.IsGrounded;
        bool parado = Mathf.Abs(rb.linearVelocity.x) < 0.3f;
        Vector2 frente = (Vector2)jogador.position + new Vector2(recuando ? -olharFrente : olharFrente, 0.3f);
        bool semChao = Physics2D.Raycast(frente, Vector2.down, 4f, 1 << LayerChao).collider == null;
        if (noChao && t >= pularAte) { segundoPuloUsado = false; }
        if (t < pularAte) { teclas.Add(Key.Space); }
        else if (noChao && (parado && t - ultimoAvanco > 0.35f || semChao))
        {
            pularAte = t + 0.2f;
            saltosSemAvanco++;
            teclas.Add(Key.Space);
        }
        else if (!noChao && !segundoPuloUsado && rb.linearVelocity.y < 0.5f
            && (parado && saltosSemAvanco > 0 || PrecisaDoSegundoPulo(rb, recuando ? -1f : 1f)))
        {
            // QA-21: antes o segundo pulo exigia saltosSemAvanco > 0, que zera assim que o corpo avança 0,25 un — logo
            // depois de decolar. Na prática o bot nunca dava pulo duplo em movimento.
            segundoPuloUsado = true;
            pularAte = t + 0.15f;   // segundo pulo no alto do primeiro
            teclas.Add(Key.Space);
        }

        if (!float.IsNaN(traceDe) && jogador.position.x >= traceDe && jogador.position.x <= traceAte)
        {
            Debug.Log($"[QATrace] t={t:0.00} x={jogador.position.x:0.00} y={jogador.position.y:0.00} vx={rb.linearVelocity.x:0.0} vy={rb.linearVelocity.y:0.0} chao={noChao} semChao={semChao} teclas={string.Join("+", teclas)} seg={segundoPuloUsado}");
        }

        // interagir e atacar
        if (t > proximoE || recuando && varrendo && t > proximoE - 2.7f) { proximoE = t + 0.3f; teclas.Add(Key.E); }
        var alvo = Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None)
            .Select(e => e.GetComponent<HealthSystem>())
            .FirstOrDefault(h => h != null && !h.IsDead && Mathf.Abs(h.transform.position.y - jogador.position.y) < 1.6f
                && (h.transform.position.x - jogador.position.x) * (recuando ? -1 : 1) is float d && d > -0.3f && d < 2.4f);
        if (alvo != null && t > proximoZ) { proximoZ = t + 0.35f; teclas.Add(Key.Z); }

        // sem progresso: recua e volta; depois desiste
        if (t - ultimoAvanco > 8f && !recuando && tentativasDeRecuo < 4)
        {
            tentativasDeRecuo++;
            // Objetivo aberto (ex.: um ponto de exame que ficou para trás antes de um portão): volta longe, examinando.
            varrendo = Object.FindObjectsByType<LevelObjective>(FindObjectsSortMode.None).Any(o => o.IsActive && !o.IsCompleted);
            recuarAte = t + (varrendo ? 2.5f * tentativasDeRecuo : 1.2f);
            ultimoAvanco = t - 4f;
            Evento($"sem avanço em x={jogador.position.x:0.0}, y={jogador.position.y:0.0} — recuando (tentativa {tentativasDeRecuo})");
        }
        if (t - ultimoAvanco > SemProgressoDesiste || tentativasDeRecuo >= 4 && t - ultimoAvanco > 10f)
        {
            Fechar($"PRESO em x={jogador.position.x:0.0}, y={jogador.position.y:0.0} (x máx {xMax:0.0}) — {Contexto()}", true);
            return;
        }

        Segurar(teclas);
    }

    /// <summary>O que estava perto do Odisseu quando ele morreu: perseguidor, zona de morte, inimigo, altura.</summary>
    private static string CausaProvavel(Vector3 onde)
    {
        var partes = new List<string>();
        foreach (var ph in Object.FindObjectsByType<PursuerHazard>(FindObjectsSortMode.None))
        {
            var c = ph.GetComponent<Collider2D>();
            partes.Add($"perseguidor {ph.name} x {(c != null ? c.bounds.min.x : ph.transform.position.x):0.0}..{(c != null ? c.bounds.max.x : ph.transform.position.x):0.0}");
        }
        foreach (var kz in Object.FindObjectsByType<KillZone>(FindObjectsSortMode.None))
        {
            var c = kz.GetComponent<Collider2D>();
            if (c != null && c.bounds.Contains(new Vector3(onde.x, onde.y, c.bounds.center.z))) { partes.Add($"DENTRO da KillZone {kz.name}"); }
        }
        var inim = Object.FindObjectsByType<EnemyController>(FindObjectsSortMode.None).Where(e => Vector2.Distance(e.transform.position, onde) < 2f).Select(e => e.name).ToArray();
        if (inim.Length > 0) { partes.Add("inimigo perto: " + string.Join(",", inim)); }
        if (onde.y < -3.5f) { partes.Add($"caiu (y {onde.y:0.0})"); }
        foreach (var th in Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).Where(m => m.GetType().Name == "TidalHazard"))
        {
            var c = th.GetComponent<Collider2D>();
            if (c != null && c.enabled && c.bounds.Contains(new Vector3(onde.x, onde.y, c.bounds.center.z))) { partes.Add($"DENTRO do TidalHazard {th.name}"); }
        }
        var chefe = Object.FindAnyObjectByType<BossController>();
        if (chefe != null) { partes.Add($"chefe {chefe.name} em x={chefe.transform.position.x:0.0}"); }
        return partes.Count > 0 ? string.Join("; ", partes) : "causa não identificada";
    }

    private static string Contexto()
    {
        var abertos = Object.FindObjectsByType<LevelObjective>(FindObjectsSortMode.None).Where(o => o.IsActive && !o.IsCompleted)
            .Select(o => $"{o.name} {o.Current}/{o.Required}").ToArray();
        var goal = Object.FindAnyObjectByType<LevelGoal>(FindObjectsInactive.Include);
        return $"objetivos abertos: {(abertos.Length > 0 ? string.Join("; ", abertos) : "nenhum")} · saída {(goal == null ? "ausente" : goal.gameObject.activeInHierarchy ? $"ativa em x={goal.transform.position.x:0.0}" : "INATIVA")}";
    }

    private static void Saindo()
    {
        string cena = SceneManager.GetActiveScene().name;
        if (cena == Fases[indice])
        {
            if (Parede > 45) { Linha("- transição: **NÃO saiu da cena** em 45 s depois de concluir"); ProximaFase(); }
            return;
        }
        var salvo = SaveSystem.Load();
        string prox = indice + 1 < Fases.Length ? Fases[indice + 1] : null;
        Linha($"- transição: → **{cena}** · concluída no save: {salvo.completedLevelIds.Contains(Fases[indice])}" +
              (prox != null ? $" · próxima ({prox}) desbloqueada: {salvo.unlockedLevelIds.Contains(prox)}" : ""));
        if (cena == "WorldMap") { passo = Passo.Mapa; marco = EditorApplication.timeSinceStartup; return; }
        ProximaFase();
    }

    private static void NoMapa()
    {
        if (Parede < 9) { return; }   // caminhada automática do mapa até o nó desbloqueado
        var mapa = Object.FindAnyObjectByType<WorldMapManager>();
        string no = mapa != null && mapa.CurrentNode != null ? mapa.CurrentNode.LevelId : "nenhum";
        string prox = indice + 1 < Fases.Length ? Fases[indice + 1] : "-";
        Linha($"- mapa: Odisseu no nó **{no}** (esperado {prox}) → {(no == prox ? "OK" : "DIVERGE")}");
        ProximaFase();
    }

    private static void ProximaFase()
    {
        FecharErros();
        indice++;
        if (indice > ate) { Linha("\n# fim"); Sair(0); return; }
        CarregarFase();
    }

    private static void Fechar(string motivo, bool jogou)
    {
        Soltar();
        Linha($"- **NÃO CONCLUÍDA**: {motivo} · mortes {mortes}");
        FecharErros();
        indice++;
        if (indice > ate) { Linha("\n# fim"); Sair(0); return; }
        CarregarFase();
    }

    private static void FecharErros()
    {
        if (jogador != null)
        {
            var objs = Object.FindObjectsByType<LevelObjective>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (objs.Length > 0)
            {
                Linha("- objetivos: " + string.Join(" · ", objs.OrderBy(o => o.transform.GetSiblingIndex())
                    .Select(o => $"{o.name} {o.Current}/{o.Required}{(o.IsCompleted ? " ✓" : o.IsActive ? " (ativo)" : "")}")));
            }
        }
        foreach (string e in eventos.Distinct().Take(15)) { Linha("  - " + e); }
        if (erros.Count > 0)
        {
            Linha($"- console: {erros.Count} erro(s)");
            foreach (string e in erros.Distinct().Take(10)) { Linha("  - " + e); }
        }
        erros.Clear();
        eventos.Clear();
    }

    // ------------------------------------------------------------------ teclado virtual

    private static void Segurar(HashSet<Key> teclas)
    {
        segurando.Clear();
        foreach (Key k in teclas) { segurando.Add(k); }
        Enviar();
    }

    private static void Tocar(Key k) { segurando.Clear(); segurando.Add(k); Enviar(); }

    private static void Soltar() { segurando.Clear(); Enviar(); }

    private static void Enviar()
    {
        if (teclado == null) { return; }
        InputSystem.QueueStateEvent(teclado, new KeyboardState(segurando.ToArray()));
    }

    private static void Evento(string e) { if (eventos.Count < 60) { eventos.Add($"t={Time.time - inicio:0}s {e}"); } }

    private static void Linha(string s) { relatorio.AppendLine(s); Debug.Log("[QA] " + s); }

    private static void Sair(int codigo)
    {
        EditorApplication.update -= Tick;
        Application.logMessageReceived -= AoLogar;
        System.IO.File.WriteAllText("Logs/qa_playtest.txt", relatorio.ToString());
        EditorSettings.enterPlayModeOptionsEnabled = opcoesOriginaisAtivas;
        EditorSettings.enterPlayModeOptions = opcoesOriginais;
        if (teclado != null)
        {
            InputSystem.RemoveDevice(teclado);
            teclado = null;
            InputSystem.settings.backgroundBehavior = fundoOriginal;
            InputSystem.settings.editorInputBehaviorInPlayMode = editorOriginal;
        }
        Time.timeScale = 1f;
        AudioListener.volume = 1f;
        EditorUtility.audioMasterMute = mudoOriginal;
        EditorApplication.ExitPlaymode();
        EditorApplication.Exit(codigo);
    }
}
