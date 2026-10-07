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
using Odisseia.Systems;
using Odisseia.UI;

/// <summary>
/// Tela final (Etapa 13B.7) em play mode, NO MUDO, pelo caminho do jogo (Boot → Final):
///
/// Unity.exe -batchmode -projectPath . -executeMethod EndingProbe.Run [-endingOut pasta]
/// (sem -nographics e sem -quit; sai sozinho, código 0 = OK; relatório em Logs/qa_ending.txt)
///
/// Visual (inglês e português): título, mensagem e botões pela tabela; borda e marcador "►" só no foco; botões, título e
/// mensagem dentro da tela, sem se sobrepor e visíveis (pixel), sem cobrir o casal nem Telêmaco, em 1280×720, 1920×1080,
/// 960×540, 2340×1080 e 1024×768 (PNG por idioma). Entrada: teclado (←/→, Enter), Xbox (D-pad, A), PlayStation (✕, ○) e
/// toque; "Jogar novamente" recomeça a campanha no Prólogo e "Voltar ao menu" vai ao menu. Música: o tema em laço.
/// Guarda e restaura o save, o idioma, o som e as opções de input e de play mode do Editor.
/// </summary>
public static class EndingProbe
{
    private const string ChaveDoSave = "Odisseia.Save";
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

    public static void Run()
    {
        string[] a = System.Environment.GetCommandLineArgs();
        int i = System.Array.IndexOf(a, "-endingOut");
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
        falhas = 0;
        L($"# QA tela final (13B.7) — {System.DateTime.Now:yyyy-MM-dd HH:mm}");
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
        kb = InputSystem.AddDevice<Keyboard>("QAEndKeyboard");
        pad = InputSystem.AddDevice<Gamepad>("QAEndPad");

        // ---- Visual, nos dois idiomas
        foreach (Language idioma in new[] { Language.English, Language.Portuguese })
        {
            Localization.Current = idioma;
            var e = Carregar(SceneLoader.Ending); while (e.MoveNext()) { yield return e.Current; }
            L($"\n## Visual — {idioma}");
            Visual(idioma);
        }
        ConferirMusica();

        // ---- Teclado: ← / → e Enter em "Voltar ao menu"
        Localization.Current = Language.English;
        {
            var e = Carregar(SceneLoader.Ending); while (e.MoveNext()) { yield return e.Current; }
        }
        L("\n## Teclado");
        Checar(Sel() == "PlayAgainButton", $"foco inicial em Jogar novamente: {Sel()}");
        var t = Tecla(Key.RightArrow); while (t.MoveNext()) { yield return t.Current; }
        Checar(Sel() == "BackToMenuButton", $"→: {Sel()}");
        t = Tecla(Key.LeftArrow); while (t.MoveNext()) { yield return t.Current; }
        Checar(Sel() == "PlayAgainButton", $"←: {Sel()}");
        t = Tecla(Key.RightArrow); while (t.MoveNext()) { yield return t.Current; }
        t = Tecla(Key.Enter); while (t.MoveNext()) { yield return t.Current; }
        { var w = Esperar(SceneLoader.MainMenu); while (w.MoveNext()) { yield return w.Current; } }
        Checar(SceneManager.GetActiveScene().name == SceneLoader.MainMenu, $"Enter em Voltar ao menu: {SceneManager.GetActiveScene().name}");

        // ---- Xbox: D-pad e A em "Jogar novamente" (recomeça a campanha no Prólogo)
        L("\n## Xbox virtual");
        CampaignManager.Instance.CompleteLevel(CampaignManager.Instance.Levels.First().LevelId, 0, 0);
        Checar(SaveSystem.HasSave(), "progresso criado antes (CompleteLevel)");
        {
            var e = Carregar(SceneLoader.Ending); while (e.MoveNext()) { yield return e.Current; }
        }
        Gamepad xbox = null;
        try { xbox = InputSystem.AddDevice("XInputControllerWindows", "QAEndXbox") as Gamepad; } catch { }
        Gamepad x = xbox ?? pad;
        var b = BotaoEm(x, x.dpad.right); while (b.MoveNext()) { yield return b.Current; }
        Checar(Sel() == "BackToMenuButton", $"D-pad →: {Sel()} ({InputDeviceTracker.Family})");
        b = BotaoEm(x, x.dpad.left); while (b.MoveNext()) { yield return b.Current; }
        Checar(Sel() == "PlayAgainButton", $"D-pad ←: {Sel()}");
        b = BotaoEm(x, x.buttonSouth); while (b.MoveNext()) { yield return b.Current; }
        { var w = Esperar(SceneLoader.FirstLevel); while (w.MoveNext()) { yield return w.Current; } }
        bool zerado = CampaignManager.Instance.Levels.All(l => !CampaignManager.Instance.IsCompleted(l.LevelId));
        Checar(SceneManager.GetActiveScene().name == SceneLoader.FirstLevel && zerado,
            $"A em Jogar novamente: {SceneManager.GetActiveScene().name}, progresso zerado {zerado}");
        if (xbox != null) { InputSystem.RemoveDevice(xbox); }

        // ---- PlayStation: ○ não faz nada; ✕ em "Voltar ao menu"
        L("\n## PlayStation virtual");
        {
            var e = Carregar(SceneLoader.Ending); while (e.MoveNext()) { yield return e.Current; }
        }
        var ps = InputSystem.AddDevice<UnityEngine.InputSystem.DualShock.DualShock4GamepadHID>("QAEndDualShock");
        b = BotaoEm(ps, ps.buttonEast); while (b.MoveNext()) { yield return b.Current; }
        Checar(SceneManager.GetActiveScene().name == SceneLoader.Ending && Sel() != "(nada)", $"○ na tela final não faz nada: {SceneManager.GetActiveScene().name}, foco {Sel()} ({InputDeviceTracker.Family})");
        EventSystem.current.SetSelectedGameObject(GameObject.Find("BackToMenuButton"));
        yield return 0.2f;
        b = BotaoEm(ps, ps.buttonSouth); while (b.MoveNext()) { yield return b.Current; }
        { var w = Esperar(SceneLoader.MainMenu); while (w.MoveNext()) { yield return w.Current; } }
        Checar(SceneManager.GetActiveScene().name == SceneLoader.MainMenu, $"✕ em Voltar ao menu: {SceneManager.GetActiveScene().name}");
        InputSystem.RemoveDevice(ps);

        // ---- Toque
        L("\n## Toque (mobile forçado)");
        MobilePlatformDetector.DebugOverride(true);
        foreach (var m in Resources.FindObjectsOfTypeAll<MobileControlsRoot>().Where(m => m.gameObject.scene.IsValid())) { Object.DestroyImmediate(m.gameObject); }
        _ = MobileControlsRoot.Instance;
        {
            var e = Carregar(SceneLoader.Ending); while (e.MoveNext()) { yield return e.Current; }
        }
        var raiz = Object.FindAnyObjectByType<MobileControlsRoot>();
        var botoes = new[] { GameObject.Find("PlayAgainButton"), GameObject.Find("BackToMenuButton") };
        var sobre = raiz != null
            ? raiz.GetComponentsInChildren<Graphic>().Where(g => g.isActiveAndEnabled && g.color.a > 0.05f && g.GetComponentInParent<CanvasGroup>()?.alpha != 0f)
                .Where(g => botoes.Any(bt => NaTela((RectTransform)bt.transform).Overlaps(NaTela(g.rectTransform)))).Select(g => g.name).ToList()
            : new List<string>();
        Checar(sobre.Count == 0, "nenhum controle de toque por cima dos botões" + (sobre.Count > 0 ? ": " + string.Join(", ", sobre) : ""));
        var tela = InputSystem.AddDevice<Touchscreen>("QAEndTouch");
        Vector2 ponto = NaTela((RectTransform)botoes[1].transform).center;
        InputSystem.QueueStateEvent(tela, new TouchState { touchId = 1, phase = UnityEngine.InputSystem.TouchPhase.Began, position = ponto, pressure = 1f });
        yield return 0.15f;
        InputSystem.QueueStateEvent(tela, new TouchState { touchId = 1, phase = UnityEngine.InputSystem.TouchPhase.Ended, position = ponto });
        { var w = Esperar(SceneLoader.MainMenu); while (w.MoveNext()) { yield return w.Current; } }
        Checar(SceneManager.GetActiveScene().name == SceneLoader.MainMenu, $"tocar em Voltar ao menu: {SceneManager.GetActiveScene().name}");
        InputSystem.RemoveDevice(tela);
        MobilePlatformDetector.DebugOverride(null);

        L($"\n## RESULTADO: {(falhas == 0 ? "OK" : falhas + " falha(s)")}");
    }

    // ---------------------------------------------------------------- visual

    private static readonly Vector2Int[] Resolucoes =
    {
        new Vector2Int(1280, 720), new Vector2Int(1920, 1080), new Vector2Int(960, 540), new Vector2Int(2340, 1080), new Vector2Int(1024, 768),
    };

    private static void Visual(Language idioma)
    {
        var textos = new Dictionary<string, string> { ["Title"] = "ui.ending.title", ["Message"] = "ui.ending.body" };
        foreach (var par in textos)
        {
            var tx = GameObject.Find(par.Key)?.GetComponent<Text>();
            Checar(tx != null && tx.text == Localization.Get(par.Value), $"{idioma}, {par.Key} = \"{tx?.text}\"");
        }
        var botoes = new Dictionary<string, string> { ["PlayAgainButton"] = "ui.ending.playAgain", ["BackToMenuButton"] = "ui.ending.backToMenu" };
        string foco = Sel();
        foreach (var par in botoes)
        {
            var go = GameObject.Find(par.Key);
            var item = go.GetComponent<Button>();
            string rotulo = go.GetComponentsInChildren<Text>(true).First(x => x.name != "FocusMarker").text;
            var marca = go.transform.Find("FocusMarker");
            var borda = go.GetComponent<Outline>();
            bool emFoco = par.Key == foco;
            Color esperado = emFoco ? UITheme.TextAccent : UITheme.ButtonBorder;
            Checar(rotulo == Localization.Get(par.Value) && go.GetComponent<MenuButton>() != null && marca != null && marca.gameObject.activeInHierarchy == emFoco && borda != null && borda.effectColor == esperado,
                $"{idioma}, {par.Key}: \"{rotulo}\", foco={emFoco}, marcador={marca?.gameObject.activeInHierarchy}, borda={borda?.effectColor}");
        }

        var fotos = new List<Texture2D>();
        foreach (Vector2Int res in Resolucoes)
        {
            fotos.Add(Foto(res.x, res.y, out List<string> problemas));
            Checar(problemas.Count == 0, $"{idioma}, {res.x}×{res.y}: textos e botões na tela, visíveis, sem sobreposição e sem cobrir os personagens" + (problemas.Count > 0 ? " — " + string.Join("; ", problemas) : ""));
        }
        Folha(fotos, $"{pasta}/ending_{(idioma == Language.English ? "en" : "pt")}.png");
    }

    private static Texture2D Foto(int w, int h, out List<string> problemas)
    {
        problemas = new List<string>();
        Camera cam = Camera.main;
        var rt = new RenderTexture(w, h, 24) { filterMode = FilterMode.Point };
        var canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c => c.isRootCanvas && c.renderMode == RenderMode.ScreenSpaceOverlay).ToList();
        foreach (Canvas c in canvases) { c.renderMode = RenderMode.ScreenSpaceCamera; c.worldCamera = cam; c.planeDistance = 1f; }
        cam.targetTexture = rt;
        Escalar();
        Canvas.ForceUpdateCanvases();
        cam.Render();

        // Caixas de UI (textos pelos glifos) e dos personagens, no espaço da imagem.
        var caixas = new List<(string nome, Rect r)>();
        foreach (string nome in new[] { "PlayAgainButton", "BackToMenuButton" }) { caixas.Add((nome, Caixa((RectTransform)GameObject.Find(nome).transform, cam))); }
        foreach (string nome in new[] { "Title", "Message" })
        {
            var tx = GameObject.Find(nome).GetComponent<Text>();
            caixas.Add((nome, Glifos(tx, cam)));
        }
        foreach (var (nome, r) in caixas)
        {
            if (r.xMin < 0 || r.yMin < 0 || r.xMax > w || r.yMax > h) { problemas.Add($"{nome} fora da tela {r}"); }
        }
        // O "►" do foco não pode cair em cima do rótulo: cada botão recebe o foco e o marcador ligado é medido
        // (o MenuButton o põe logo antes do rótulo só quando ganha o foco).
        GameObject focoAntes = EventSystem.current.currentSelectedGameObject;
        foreach (string nome in new[] { "PlayAgainButton", "BackToMenuButton" })
        {
            var go = GameObject.Find(nome);
            EventSystem.current.SetSelectedGameObject(go);
            Canvas.ForceUpdateCanvases();
            var marca = (RectTransform)go.transform.Find("FocusMarker");
            var rotulo = go.GetComponentsInChildren<Text>(true).First(x => x.name != "FocusMarker");
            var mt = marca.GetComponent<Text>();
            Rect rm = Caixa(marca, cam);
            float larguraDoGlifo = mt.preferredWidth * rm.height / Mathf.Max(1f, marca.rect.height);
            Rect glifo = Rect.MinMaxRect(rm.xMax - larguraDoGlifo, rm.yMin, rm.xMax, rm.yMax); // alinhado à direita
            Rect rl = Glifos(rotulo, cam);
            if (!marca.gameObject.activeInHierarchy) { problemas.Add($"{nome}: marcador não ligou no foco"); }
            if (glifo.xMax + 2 > rl.xMin && glifo.xMin < rl.xMax) { problemas.Add($"{nome}: ► ({glifo.xMin:0}–{glifo.xMax:0}) encosta no rótulo ({rl.xMin:0}–{rl.xMax:0})"); }
            if (glifo.xMin < Caixa((RectTransform)go.transform, cam).xMin - 2) { problemas.Add($"{nome}: ► sai do botão pela esquerda"); }
        }
        EventSystem.current.SetSelectedGameObject(focoAntes);
        Canvas.ForceUpdateCanvases();
        for (int i = 0; i < caixas.Count; i++)
        {
            for (int j = i + 1; j < caixas.Count; j++)
            {
                Rect a = caixas[i].r, b = caixas[j].r;
                if (a.xMin + 1 < b.xMax && b.xMin + 1 < a.xMax && a.yMin + 1 < b.yMax && b.yMin + 1 < a.yMax) { problemas.Add($"{caixas[i].nome} × {caixas[j].nome}"); }
            }
        }
        foreach (string personagem in new[] { "Reunion", "Telemachus" })
        {
            var sr = GameObject.Find(personagem)?.GetComponent<SpriteRenderer>();
            if (sr == null) { problemas.Add(personagem + " ausente"); continue; }
            Vector2 p0 = cam.WorldToScreenPoint(sr.bounds.min), p1 = cam.WorldToScreenPoint(sr.bounds.max);
            float sx = w / (float)cam.pixelWidth, sy = h / (float)cam.pixelHeight;
            var rp = Rect.MinMaxRect(p0.x * sx, p0.y * sy, p1.x * sx, p1.y * sy);
            foreach (var (nome, r) in caixas.Where(c => c.nome.EndsWith("Button")))
            {
                Rect inter = Rect.MinMaxRect(Mathf.Max(r.xMin, rp.xMin), Mathf.Max(r.yMin, rp.yMin), Mathf.Min(r.xMax, rp.xMax), Mathf.Min(r.yMax, rp.yMax));
                // Até 20% da altura do personagem (os pés no chão) é tolerado; mais que isso, o botão o cobre.
                if (inter.width > 0 && inter.height > rp.height * 0.2f) { problemas.Add($"{nome} cobre {personagem} ({inter.height / rp.height:P0} da altura)"); }
            }
        }

        RenderTexture.active = rt;
        var tex = new Texture2D(w, h, TextureFormat.RGB24, false) { hideFlags = HideFlags.HideAndDontSave };
        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        tex.Apply();
        RenderTexture.active = null;
        cam.targetTexture = null;
        foreach (Canvas c in canvases) { c.renderMode = RenderMode.ScreenSpaceOverlay; }
        Escalar();
        rt.Release();

        // Pixels: o centro de cada botão tem a cor do botão, não a do cenário por trás (botão desenhado e visível).
        foreach (var (nome, r) in caixas.Where(c => c.nome.EndsWith("Button")))
        {
            Color px = tex.GetPixel((int)r.center.x, (int)(r.yMin + r.height * 0.2f));
            if (!(px.b > px.r && px.b > px.g)) { problemas.Add($"{nome}: pixel {px} não é o azul do botão (coberto?)"); }
        }
        return tex;
    }

    private static Rect Glifos(Text t, Camera cam)
    {
        IList<UIVertex> v = t.cachedTextGenerator.verts;
        if (v == null || v.Count == 0) { return Caixa(t.rectTransform, cam); }
        Vector2 min = Vector2.positiveInfinity, max = Vector2.negativeInfinity;
        float escala = 1f / t.pixelsPerUnit;
        foreach (UIVertex u in v)
        {
            Vector2 s = RectTransformUtility.WorldToScreenPoint(cam, t.rectTransform.TransformPoint(u.position * escala));
            min = Vector2.Min(min, s); max = Vector2.Max(max, s);
        }
        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    private static Rect Caixa(RectTransform r, Camera cam)
    {
        var cantos = new Vector3[4];
        r.GetWorldCorners(cantos);
        Vector2 a = RectTransformUtility.WorldToScreenPoint(cam, cantos[0]), b = RectTransformUtility.WorldToScreenPoint(cam, cantos[2]);
        return Rect.MinMaxRect(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y), Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y));
    }

    private static Rect NaTela(RectTransform r)
    {
        Canvas c = r.GetComponentInParent<Canvas>().rootCanvas;
        return Caixa(r, c.renderMode == RenderMode.ScreenSpaceOverlay ? null : c.worldCamera);
    }

    private static void Folha(List<Texture2D> fotos, string arquivo)
    {
        const int alt = 360, folga = 8, porLinha = 3;
        var red = fotos.Select(f => Redimensionar(f, Mathf.RoundToInt(f.width * (float)alt / f.height), alt)).ToList();
        int linhas = (red.Count + porLinha - 1) / porLinha, larg = red.Max(x => x.width);
        var folha = new Texture2D(porLinha * (larg + folga), linhas * (alt + folga), TextureFormat.RGB24, false);
        folha.SetPixels(Enumerable.Repeat(new Color(0.15f, 0.15f, 0.15f), folha.width * folha.height).ToArray());
        for (int i = 0; i < red.Count; i++) { folha.SetPixels((i % porLinha) * (larg + folga), (linhas - 1 - i / porLinha) * (alt + folga), red[i].width, alt, red[i].GetPixels()); }
        folha.Apply();
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(arquivo));
        System.IO.File.WriteAllBytes(arquivo, folha.EncodeToPNG());
        L($"- capturas: {arquivo}");
    }

    private static Texture2D Redimensionar(Texture2D src, int w, int h)
    {
        var dst = new Texture2D(w, h, TextureFormat.RGB24, false) { hideFlags = HideFlags.HideAndDontSave };
        Color[] s = src.GetPixels(), d = new Color[w * h];
        for (int y = 0; y < h; y++) { for (int x = 0; x < w; x++) { d[y * w + x] = s[(y * src.height / h) * src.width + x * src.width / w]; } }
        dst.SetPixels(d);
        dst.Apply();
        return dst;
    }

    private static void ConferirMusica()
    {
        AudioClip tema = GameAssets.Instance != null ? GameAssets.Instance.Audio?.MainTheme : null;
        var gerente = Object.FindAnyObjectByType<AudioManager>();
        AudioSource musica = gerente != null ? gerente.GetComponents<AudioSource>().FirstOrDefault(a => a.loop) : null;
        L($"\n## Música\n- clipe '{musica?.clip?.name}', laço {musica?.loop}, tocando {musica?.isPlaying}");
        Checar(musica != null && tema != null && musica.clip == tema && musica.loop, "o tema único em laço na tela final (o mesmo do menu e das fases)");
    }

    // ---------------------------------------------------------------- entrada

    private static IEnumerator<float> Carregar(string cena)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(cena);
        var w = Esperar(cena); while (w.MoveNext()) { yield return w.Current; }
        yield return 1.5f;
        AudioListener.volume = 0f;
        Focar();
    }

    private static IEnumerator<float> Esperar(string cena)
    {
        float t = Time.realtimeSinceStartup;
        while (SceneManager.GetActiveScene().name != cena && Time.realtimeSinceStartup - t < 30f) { yield return 0.2f; }
        yield return 0.5f;
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

    private static void L(string s) { rel.AppendLine(s); Debug.Log("[QAEnding] " + s); }

    private static void Sair()
    {
        EditorApplication.update -= Tick;
        System.IO.File.WriteAllText("Logs/qa_ending.txt", rel.ToString());
        if (kb != null) { InputSystem.RemoveDevice(kb); InputSystem.RemoveDevice(pad); }
        InputSystem.settings.backgroundBehavior = fundoInput;
        InputSystem.settings.editorInputBehaviorInPlayMode = editorIn;
        Localization.Current = idiomaOriginal;
        MobilePlatformDetector.DebugOverride(null);
        if (saveAnterior != null) { PlayerPrefs.SetString(ChaveDoSave, saveAnterior); } else { PlayerPrefs.DeleteKey(ChaveDoSave); }
        PlayerPrefs.Save();
        EditorApplication.ExitPlaymode();
        AudioListener.volume = 1f;
        EditorUtility.audioMasterMute = mudoOriginal;
        EditorSettings.enterPlayModeOptionsEnabled = opcoesAtivas;
        EditorSettings.enterPlayModeOptions = opcoes;
        EditorApplication.Exit(falhas == 0 ? 0 : 1);
    }

    /// <summary>
    /// O CanvasScaler só recalcula o fator no Update dele, pela janela do batch (640×480). Sem isto, a foto em outra
    /// resolução sairia com o fator velho — o layout não seria o daquela tela.
    /// </summary>
    private static void Escalar()
    {
        var handle = typeof(UnityEngine.UI.CanvasScaler).GetMethod("Handle", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        foreach (var s in Object.FindObjectsByType<UnityEngine.UI.CanvasScaler>(FindObjectsSortMode.None)) { handle.Invoke(s, null); }
        // Com o fator novo, os textos refazem a malha (o cache de glifos guardava a escala anterior).
        foreach (var g in Object.FindObjectsByType<UnityEngine.UI.Graphic>(FindObjectsSortMode.None)) { g.SetAllDirty(); }
        Canvas.ForceUpdateCanvases();
    }
}
