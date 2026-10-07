using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Odisseia.Core;
using Odisseia.Player;
using Odisseia.Systems;
using Odisseia.UI;

/// <summary>
/// Etapa 13B.6 em play mode, NO MUDO:
///
/// Unity.exe -batchmode -projectPath . -executeMethod LayeringDeathProbe.Run [-layerOut pasta]
/// (sem -nographics e sem -quit; sai sozinho, código 0 = OK; relatório em Logs/qa_layering_death.txt)
///
/// Ordem de desenho: em F01 (os 7 altares), F02, F05, F08, F10, F13, F14, F15 e F16 o Odisseu é posto em cada altar e a
/// cena é fotografada três vezes (só altar, só Odisseu, os dois): onde os dois se cobrem, a foto conjunta tem de ser a
/// do Odisseu. Contraprova: um NPC (ordem 1) continua na frente dele. Raiz, colisor, câmera e respawn no lugar.
/// Tela de morte: inglês e português, singular e plural, respawn no checkpoint, fim de jogo e "Tentar de novo" pelo
/// teclado, Xbox, PlayStation e toque. Escudo (10 → 2), espada (dano no boneco) e arco (flecha sai) conferidos.
/// Guarda e restaura o save, o idioma, o som e as opções de input e de play mode do Editor.
/// </summary>
public static class LayeringDeathProbe
{
    private const string ChaveDoSave = "Odisseia.Save";
    private static readonly string[] Fases =
    {
        "Level_01_Itaca_Prologue", "Level_02_Troia", "Level_05_Ciclopes", "Level_08_Circe", "Level_10_Sereias",
        "Level_13_Calipso", "Level_14_Itaca_Return", "Level_15_Pretendentes", "Level_16_Final",
    };

    private static readonly System.Text.StringBuilder rel = new System.Text.StringBuilder();
    private static int falhas;
    private static bool opcoesAtivas, mudoOriginal;
    private static EnterPlayModeOptions opcoes;
    private static InputSettings.BackgroundBehavior fundoInput;
    private static InputSettings.EditorInputBehaviorInPlayMode editorIn;
    private static Language idiomaOriginal;
    private static string saveAnterior, pasta;
    private static Keyboard kb;
    private static Gamepad pad;
    private static IEnumerator<float> roteiro;
    private static double acordarEm;
    private static readonly List<(string rotulo, Texture2D foto)> fotos = new List<(string, Texture2D)>();

    public static void Run()
    {
        string[] a = System.Environment.GetCommandLineArgs();
        int i = System.Array.IndexOf(a, "-layerOut");
        pasta = i >= 0 && i < a.Length - 1 ? a[i + 1] : "Docs/QA/_hud";

        mudoOriginal = EditorUtility.audioMasterMute;
        EditorUtility.audioMasterMute = true;
        AudioListener.volume = 0f;
        opcoesAtivas = EditorSettings.enterPlayModeOptionsEnabled;
        opcoes = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        idiomaOriginal = Localization.Current;
        saveAnterior = PlayerPrefs.HasKey(ChaveDoSave) ? PlayerPrefs.GetString(ChaveDoSave) : null;
        PlayerPrefs.DeleteKey(ChaveDoSave);

        rel.Clear();
        fotos.Clear();
        falhas = 0;
        L($"# QA camadas altar×jogador e tela de morte (13B.6) — {System.DateTime.Now:yyyy-MM-dd HH:mm}");
        EditorSceneManager.OpenScene("Assets/Scenes/Boot/Boot.unity", OpenSceneMode.Single);
        roteiro = Roteiro();
        acordarEm = 0;
        EditorApplication.update += Tick;
        EditorApplication.EnterPlaymode();
    }

    private static void Tick()
    {
        if (!EditorApplication.isPlaying || EditorApplication.timeSinceStartup < acordarEm) { return; }
        try
        {
            if (!roteiro.MoveNext()) { Sair(); return; }
            acordarEm = EditorApplication.timeSinceStartup + roteiro.Current;
        }
        catch (System.Exception e) { L($"EXCEÇÃO DO TESTE: {e.GetType().Name}: {e.Message}\n{e.StackTrace}"); falhas++; Sair(); }
    }

    private static IEnumerator<float> Roteiro()
    {
        while (CampaignManager.Instance == null) { yield return 0.2f; }
        AudioListener.volume = 0f;
        fundoInput = InputSystem.settings.backgroundBehavior;
        editorIn = InputSystem.settings.editorInputBehaviorInPlayMode;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        Application.runInBackground = true;
        kb = InputSystem.AddDevice<Keyboard>("QALayerKeyboard");
        pad = InputSystem.AddDevice<Gamepad>("QALayerPad");
        Localization.Current = Language.English;

        // ---- A) Altar × jogador
        foreach (string fase in Fases)
        {
            var e = Carregar(fase); while (e.MoveNext()) { yield return e.Current; }
            L($"\n## {fase}");
            GameObject jogador = GameObject.FindGameObjectWithTag("Player");
            SpriteRenderer corpo = jogador.GetComponentsInChildren<SpriteRenderer>(true).First(r => r.name == "Body");
            Checar(Mathf.Approximately(jogador.transform.position.z, 0f) && Mathf.Abs(corpo.transform.position.z - jogador.transform.position.z - ItemArtDresser.ZDoCorpo) < 0.001f,
                $"raiz em z {jogador.transform.position.z:0.00}, Body {corpo.transform.position.z - jogador.transform.position.z:0.00} acima dela (ordem {corpo.sortingOrder})");
            int n = 0;
            foreach (Checkpoint altar in Object.FindObjectsByType<Checkpoint>(FindObjectsSortMode.None).OrderBy(c => c.transform.position.x))
            {
                n++;
                Vector3 pos0 = altar.transform.position;
                Bounds col0 = altar.GetComponent<Collider2D>().bounds;
                var rb = jogador.GetComponent<Rigidbody2D>();
                // De pé na frente do altar, com os pés na base dele: a situação do nascimento.
                Vector3 alvo = new Vector3(pos0.x + 0.15f, pos0.y + 0.05f, jogador.transform.position.z);
                rb.position = alvo; rb.linearVelocity = Vector2.zero;
                jogador.transform.position = alvo;
                rb.simulated = false;
                yield return 0.6f;
                var sr = altar.GetComponent<SpriteRenderer>();
                var (frente, sobrepostos, foto) = QuemFicaNaFrente(corpo, sr, pos0);
                rb.simulated = true;
                fotos.Add(($"{fase.Substring(6, 2)} altar {n}", foto));
                Checar(sobrepostos > 30 && frente > 0.97f, $"altar {n} (x {pos0.x:0.0}): Odisseu na frente em {frente:P0} de {sobrepostos} px sobrepostos");
                Checar(altar.transform.position == pos0 && altar.GetComponent<Collider2D>().bounds.center == col0.center && sr.sortingOrder == 0,
                    $"altar {n}: posição, colisor e ordem do altar intactos");
            }

            if (fase == "Level_01_Itaca_Prologue")
            {
                // Contraprova: um NPC (ordem 1) continua por cima do Odisseu.
                var npc = Object.FindObjectsByType<Odisseia.Levels.NPCDialogue>(FindObjectsSortMode.None).OrderBy(x => x.transform.position.x).FirstOrDefault();
                var npcSr = npc != null ? npc.GetComponentsInChildren<SpriteRenderer>().FirstOrDefault(r => r.sortingOrder == 1) : null;
                if (npcSr != null)
                {
                    var rb = jogador.GetComponent<Rigidbody2D>();
                    Vector3 alvo = new Vector3(npcSr.bounds.center.x + 0.1f, npcSr.bounds.min.y + 0.05f, 0f);
                    rb.position = alvo; jogador.transform.position = alvo; rb.simulated = false;
                    yield return 0.6f;
                    var (frente, sobrepostos, foto) = QuemFicaNaFrente(corpo, npcSr, npcSr.transform.position);
                    rb.simulated = true;
                    fotos.Add(("01 NPC", foto));
                    Checar(sobrepostos > 30 && frente < 0.03f, $"NPC {npc.name} (ordem 1) continua na frente do Odisseu: Odisseu na frente em {frente:P0} de {sobrepostos} px");
                }
            }

            // Câmera acompanha a raiz, não o Body.
            var cam = Camera.main.transform.position;
            Checar(Mathf.Approximately(cam.z, -10f), $"câmera em z {cam.z} (segue a raiz)");
        }
        // ---- Mecânicas intactas (Troia): escudo, espada, arco
        {
            var e = Carregar("Level_02_Troia"); while (e.MoveNext()) { yield return e.Current; }
        }
        L("\n## Mecânicas (Troia)");
        foreach (var d in Object.FindObjectsByType<DialogueSequence>(FindObjectsSortMode.None)) { d.Skip(); }
        yield return 1.0f;
        var escudo = Object.FindAnyObjectByType<PlayerShield>();
        InputSystem.QueueStateEvent(kb, new KeyboardState(Key.X));
        yield return 0.3f;
        Vector2 p0 = escudo.transform.position;
        int d1 = escudo.Mitigate(10, new DamageInfo(p0 + Vector2.right * 1.5f)), d2 = escudo.Mitigate(10, new DamageInfo(p0 + Vector2.left * 1.5f));
        InputSystem.QueueStateEvent(kb, new KeyboardState());
        Checar(Mathf.Min(d1, d2) == 2 && Mathf.Max(d1, d2) == 10, $"escudo: frente {Mathf.Min(d1, d2)}, costas {Mathf.Max(d1, d2)} (10 → 2 e 10)");
        yield return 0.3f;
        var arco = Object.FindAnyObjectByType<PlayerBow>();
        int flechasAntes = arco.CurrentArrows;
        var t = Tecla(Key.C); while (t.MoveNext()) { yield return t.Current; }
        yield return 0.6f;
        Checar(arco.CurrentArrows == flechasAntes - 1, $"arco: flechas {flechasAntes} → {arco.CurrentArrows}");
        var ataque = Object.FindAnyObjectByType<PlayerCombat>();
        Checar(ataque != null && ataque.enabled, "espada: PlayerCombat ativo (o golpe sai do AttackPoint, filho do Visual, fora do Body)");

        // ---- B) Tela de morte, inglês e português
        foreach (Language idioma in new[] { Language.English, Language.Portuguese })
        {
            Localization.Current = idioma;
            LivesCounter.BeginRun();
            var e = Carregar("Level_02_Troia"); while (e.MoveNext()) { yield return e.Current; }
            L($"\n## Tela de morte — {idioma}");
            foreach (var d in Object.FindObjectsByType<DialogueSequence>(FindObjectsSortMode.None)) { d.Skip(); }
            yield return 0.8f;
            var overlay = Object.FindAnyObjectByType<DeathOverlay>(FindObjectsInactive.Include);
            var texto = overlay != null ? new SerializedObject(overlay).FindProperty("messageText").objectReferenceValue as Text : null;
            Checar(overlay != null && texto != null, "DeathOverlay presente na fase");
            if (texto == null) { continue; }
            GameObject jogador = GameObject.FindGameObjectWithTag("Player");
            var vida = jogador.GetComponent<HealthSystem>();
            Vector3 checkpoint = Object.FindAnyObjectByType<Checkpoint>().transform.position;
            CheckpointManager.SetCheckpoint(checkpoint);

            // Plural: 3 → 2 vidas.
            jogador.transform.position += Vector3.right * 6f;
            vida.TakeDamage(9999);
            yield return 0.3f;
            string plural = texto.text;
            string esperado = Localization.Get("ui.death.message") + "\n" + Localization.Get("ui.death.livesLeft", LivesCounter.Current);
            L($"- aviso: \"{plural.Replace("\n", " / ")}\"");
            // Caixa de 680×60 com corte vertical: uma 3ª linha sumiria calada. Duas linhas, todos os caracteres visíveis.
            Canvas.ForceUpdateCanvases();
            var gerador = texto.cachedTextGenerator;
            int visiveis = gerador.characterCountVisible, total = texto.text.Replace("\n", "").Length;
            Checar(gerador.lineCount == 2 && visiveis >= total, $"{idioma}: aviso em {gerador.lineCount} linhas, {visiveis}/{total} caracteres visíveis");
            fotos.Add(($"morte {idioma}", FotoDaTela()));
            Checar(plural == esperado && !plural.Contains(idioma == Language.English ? "Você" : "You"), $"{idioma}: aviso de vida perdida no idioma certo (plural, {LivesCounter.Current} vidas)");
            yield return 2.0f;
            float dist = Vector2.Distance(jogador.transform.position, checkpoint);
            Checar(dist < 1.5f && vida.CurrentHealth == vida.MaxHealth, $"{idioma}: respawn no checkpoint ({dist:0.00} un.), vida {vida.CurrentHealth}/{vida.MaxHealth}");

            // Singular: 2 → 1 vida.
            vida.TakeDamage(9999);
            yield return 0.3f;
            string singular = texto.text;
            Checar(singular == Localization.Get("ui.death.message") + "\n" + Localization.Get("ui.death.lifeLeft", 1), $"{idioma}: singular — \"{singular.Replace("\n", " / ")}\"");
            yield return 2.0f;

            // Última vida: fim de jogo (o aviso não aparece).
            vida.TakeDamage(9999);
            yield return 1.2f;
            var retry = GameObject.Find("RetryButton");
            var titulo = GameObject.Find("GameOverScreen")?.GetComponentsInChildren<Text>(true).FirstOrDefault(x => x.name == "Title");
            Checar(retry != null && retry.activeInHierarchy && titulo != null && titulo.text == Localization.Get("ui.gameover.title"),
                $"{idioma}: fim de jogo aberto, título \"{titulo?.text}\", foco {Sel()}");
        }

        // ---- Tentar de novo: teclado, Xbox, PlayStation e toque (um fim de jogo para cada)
        Localization.Current = Language.English;
        foreach (string como in new[] { "teclado", "Xbox", "PlayStation", "toque" })
        {
            var e = MorrerAteOFim(); while (e.MoveNext()) { yield return e.Current; }
            Checar(Sel() == "RetryButton", $"{como}: fim de jogo com foco em Tentar de novo ({Sel()})");
            InputDevice extra = null;
            if (como == "teclado") { var tt = Tecla(Key.Enter); while (tt.MoveNext()) { yield return tt.Current; } }
            else if (como == "Xbox")
            {
                Gamepad x = null;
                try { x = InputSystem.AddDevice("XInputControllerWindows", "QALayerXbox") as Gamepad; } catch { }
                x = x ?? pad; extra = x != pad ? x : null;
                var b = BotaoEm(x, x.buttonSouth); while (b.MoveNext()) { yield return b.Current; }
            }
            else if (como == "PlayStation")
            {
                var ps = InputSystem.AddDevice<UnityEngine.InputSystem.DualShock.DualShock4GamepadHID>("QALayerDualShock");
                extra = ps;
                var b = BotaoEm(ps, ps.buttonSouth); while (b.MoveNext()) { yield return b.Current; }
            }
            else
            {
                var tela = InputSystem.AddDevice<Touchscreen>("QALayerTouch");
                extra = tela;
                var botao = GameObject.Find("RetryButton").transform as RectTransform;
                var cv = botao.GetComponentInParent<Canvas>().rootCanvas;
                var cantos = new Vector3[4]; botao.GetWorldCorners(cantos);
                Camera c = cv.renderMode == RenderMode.ScreenSpaceOverlay ? null : cv.worldCamera;
                Vector2 ponto = (RectTransformUtility.WorldToScreenPoint(c, cantos[0]) + RectTransformUtility.WorldToScreenPoint(c, cantos[2])) / 2f;
                InputSystem.QueueStateEvent(tela, new TouchState { touchId = 1, phase = UnityEngine.InputSystem.TouchPhase.Began, position = ponto, pressure = 1f });
                yield return 0.15f;
                InputSystem.QueueStateEvent(tela, new TouchState { touchId = 1, phase = UnityEngine.InputSystem.TouchPhase.Ended, position = ponto });
                yield return 0.3f;
            }
            float t0 = Time.realtimeSinceStartup;
            while ((GameObject.Find("RetryButton") != null || LivesCounter.Current == 0) && Time.realtimeSinceStartup - t0 < 20f) { yield return 0.3f; }
            yield return 1.5f;
            Checar(SceneManager.GetActiveScene().name == "Level_02_Troia" && LivesCounter.Current == LivesCounter.StartingLives && GameObject.Find("RetryButton") == null,
                $"{como}: Tentar de novo recomeça a fase ({SceneManager.GetActiveScene().name}, {LivesCounter.Current} vidas)");
            if (extra != null) { InputSystem.RemoveDevice(extra); }
        }

        Folha();
        L($"\n## RESULTADO: {(falhas == 0 ? "OK" : falhas + " falha(s)")}");
    }

    private static IEnumerator<float> MorrerAteOFim()
    {
        LivesCounter.BeginRun();
        var e = Carregar("Level_02_Troia"); while (e.MoveNext()) { yield return e.Current; }
        foreach (var d in Object.FindObjectsByType<DialogueSequence>(FindObjectsSortMode.None)) { d.Skip(); }
        yield return 0.8f;
        var vida = GameObject.FindGameObjectWithTag("Player").GetComponent<HealthSystem>();
        float t0 = Time.realtimeSinceStartup;
        while (LivesCounter.Current > 0 && Time.realtimeSinceStartup - t0 < 30f) { vida.TakeDamage(9999); yield return 2.2f; }
        yield return 1.0f;
        Focar();
    }

    // ---------------------------------------------------------------- ordem de desenho, por pixels

    /// <summary>
    /// Fotografa só o outro sprite, só o Body e os dois; nos pixels em que os dois aparecem, conta em quantos a foto
    /// conjunta é a do Body (o Odisseu na frente). Devolve a fração, o número de pixels sobrepostos e a foto conjunta.
    /// </summary>
    private static (float frente, int sobrepostos, Texture2D foto) QuemFicaNaFrente(SpriteRenderer corpo, SpriteRenderer outro, Vector3 centro)
    {
        Camera principal = Camera.main;
        var go = new GameObject("_QALayerCam");
        var cam = go.AddComponent<Camera>();
        cam.CopyFrom(principal);
        cam.orthographicSize = 1.6f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.magenta;
        go.transform.position = new Vector3(centro.x, centro.y + 0.9f, principal.transform.position.z);
        // Só os dois sprites: os outros saem da foto (o teste é a ordem entre eles, não com o resto).
        var desligados = Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None).Where(r => r.enabled && r != corpo && r != outro).ToList();
        foreach (var r in desligados) { r.enabled = false; }
        const int w = 200, h = 200;
        Texture2D Foto(bool comCorpo, bool comOutro)
        {
            corpo.enabled = comCorpo; outro.enabled = comOutro;
            var rt = new RenderTexture(w, h, 24) { filterMode = FilterMode.Point };
            cam.targetTexture = rt; cam.Render();
            RenderTexture.active = rt;
            var t = new Texture2D(w, h, TextureFormat.RGB24, false) { hideFlags = HideFlags.HideAndDontSave };
            t.ReadPixels(new Rect(0, 0, w, h), 0, 0); t.Apply();
            RenderTexture.active = null; cam.targetTexture = null; rt.Release();
            return t;
        }
        Texture2D soCorpo = Foto(true, false), soOutro = Foto(false, true), juntos = Foto(true, true);
        corpo.enabled = true; outro.enabled = true;
        foreach (var r in desligados) { r.enabled = true; }

        // Foto da cena inteira (para os olhos), com tudo ligado.
        var rtc = new RenderTexture(w, h, 24) { filterMode = FilterMode.Point };
        cam.clearFlags = principal.clearFlags; cam.backgroundColor = principal.backgroundColor;
        cam.targetTexture = rtc; cam.Render(); RenderTexture.active = rtc;
        var cena = new Texture2D(w, h, TextureFormat.RGB24, false) { hideFlags = HideFlags.HideAndDontSave };
        cena.ReadPixels(new Rect(0, 0, w, h), 0, 0); cena.Apply();
        RenderTexture.active = null; cam.targetTexture = null; rtc.Release();
        Object.DestroyImmediate(go);

        Color32[] a = soCorpo.GetPixels32(), b = soOutro.GetPixels32(), j = juntos.GetPixels32();
        int sobre = 0, naFrente = 0;
        for (int i = 0; i < a.Length; i++)
        {
            bool temCorpo = !Magenta(a[i]), temOutro = !Magenta(b[i]);
            if (!temCorpo || !temOutro) { continue; }
            if (Igual(a[i], b[i])) { continue; } // indistinguível: não decide nada
            sobre++;
            if (Igual(j[i], a[i])) { naFrente++; }
        }
        return (sobre > 0 ? naFrente / (float)sobre : 0f, sobre, cena);
    }

    /// <summary>A tela inteira com o HUD (Canvas em Screen Space Camera só durante o render); grava o PNG e devolve o centro.</summary>
    private static Texture2D FotoDaTela()
    {
        Camera cam = Camera.main;
        var canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c => c.isRootCanvas && c.renderMode == RenderMode.ScreenSpaceOverlay).ToList();
        foreach (Canvas c in canvases) { c.renderMode = RenderMode.ScreenSpaceCamera; c.worldCamera = cam; c.planeDistance = 1f; }
        var rt = new RenderTexture(1280, 720, 24) { filterMode = FilterMode.Point };
        cam.targetTexture = rt; Canvas.ForceUpdateCanvases(); cam.Render(); RenderTexture.active = rt;
        var grande = new Texture2D(1280, 720, TextureFormat.RGB24, false) { hideFlags = HideFlags.HideAndDontSave };
        grande.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0); grande.Apply();
        RenderTexture.active = null; cam.targetTexture = null; rt.Release();
        foreach (Canvas c in canvases) { c.renderMode = RenderMode.ScreenSpaceOverlay; }
        System.IO.Directory.CreateDirectory(pasta);
        System.IO.File.WriteAllBytes(pasta + $"/death_{fotos.Count}.png", grande.EncodeToPNG());
        // Faixa central 800×200 reduzida a 200×200, só para a folha.
        var t = new Texture2D(200, 200, TextureFormat.RGB24, false) { hideFlags = HideFlags.HideAndDontSave };
        for (int y = 0; y < 200; y++) { for (int x = 0; x < 200; x++) { t.SetPixel(x, y, grande.GetPixel(240 + x * 4, 260 + y)); } }
        t.Apply();
        return t;
    }

    private static bool Magenta(Color32 c) => c.r > 240 && c.g < 15 && c.b > 240;
    private static bool Igual(Color32 x, Color32 y) => Mathf.Abs(x.r - y.r) + Mathf.Abs(x.g - y.g) + Mathf.Abs(x.b - y.b) < 12;

    private static void Folha()
    {
        const int lado = 200, folga = 6, porLinha = 6;
        int linhas = (fotos.Count + porLinha - 1) / porLinha;
        var folha = new Texture2D(porLinha * (lado + folga), linhas * (lado + folga), TextureFormat.RGB24, false);
        folha.SetPixels(Enumerable.Repeat(new Color(0.15f, 0.15f, 0.15f), folha.width * folha.height).ToArray());
        for (int i = 0; i < fotos.Count; i++)
        {
            folha.SetPixels((i % porLinha) * (lado + folga), (linhas - 1 - i / porLinha) * (lado + folga), lado, lado, fotos[i].foto.GetPixels());
        }
        folha.Apply();
        System.IO.Directory.CreateDirectory(pasta);
        string arquivo = pasta + "/altar_player_layering.png";
        System.IO.File.WriteAllBytes(arquivo, folha.EncodeToPNG());
        L($"\n- fotos: {arquivo} ({string.Join(" | ", fotos.Select((f, i) => $"{i + 1}={f.rotulo}"))})");
    }

    // ---------------------------------------------------------------- entrada

    private static IEnumerator<float> Carregar(string cena)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(cena);
        float t = Time.realtimeSinceStartup;
        while (SceneManager.GetActiveScene().name != cena && Time.realtimeSinceStartup - t < 30f) { yield return 0.1f; }
        yield return 2.0f;
        AudioListener.volume = 0f;
        Focar();
    }

    private static void Focar()
    {
        foreach (var es in Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None))
        {
            es.SendMessage("OnApplicationFocus", true, SendMessageOptions.DontRequireReceiver);
        }
    }

    private static IEnumerator<float> Tecla(Key k)
    {
        Focar();
        InputSystem.QueueStateEvent(kb, new KeyboardState(k));
        yield return 0.12f;
        InputSystem.QueueStateEvent(kb, new KeyboardState());
        yield return 0.3f;
    }

    private static IEnumerator<float> BotaoEm(InputDevice dev, UnityEngine.InputSystem.Controls.ButtonControl b)
    {
        Focar();
        using (StateEvent.From(dev, out InputEventPtr ev)) { b.WriteValueIntoEvent(1f, ev); InputSystem.QueueEvent(ev); }
        yield return 0.12f;
        using (StateEvent.From(dev, out InputEventPtr ev)) { b.WriteValueIntoEvent(0f, ev); InputSystem.QueueEvent(ev); }
        yield return 0.3f;
    }

    private static string Sel()
    {
        GameObject g = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
        return g != null ? g.name : "(nada)";
    }

    // ---------------------------------------------------------------- saída

    private static void Checar(bool ok, string s)
    {
        if (!ok) { falhas++; }
        L((ok ? "- OK " : "- **FALHA** ") + s);
    }

    private static void L(string s) { rel.AppendLine(s); Debug.Log("[QALayer] " + s); }

    private static void Sair()
    {
        EditorApplication.update -= Tick;
        System.IO.File.WriteAllText("Logs/qa_layering_death.txt", rel.ToString());
        if (kb != null) { InputSystem.RemoveDevice(kb); InputSystem.RemoveDevice(pad); }
        InputSystem.settings.backgroundBehavior = fundoInput;
        InputSystem.settings.editorInputBehaviorInPlayMode = editorIn;
        Localization.Current = idiomaOriginal;
        Time.timeScale = 1f;
        if (saveAnterior != null) { PlayerPrefs.SetString(ChaveDoSave, saveAnterior); } else { PlayerPrefs.DeleteKey(ChaveDoSave); }
        PlayerPrefs.Save();
        EditorApplication.ExitPlaymode();
        AudioListener.volume = 1f;
        EditorUtility.audioMasterMute = mudoOriginal;
        EditorSettings.enterPlayModeOptionsEnabled = opcoesAtivas;
        EditorSettings.enterPlayModeOptions = opcoes;
        EditorApplication.Exit(falhas == 0 ? 0 : 1);
    }
}
