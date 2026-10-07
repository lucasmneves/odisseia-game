using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;
using Odisseia.Core;
using Odisseia.Player;
using Odisseia.Systems;

/// <summary>
/// Prova o ícone do escudo no HUD (Etapa 13B.3) em play mode, NO MUDO:
///
/// Unity.exe -batchmode -projectPath . -executeMethod HudShieldProbe.Run [-shieldOut Docs/QA/_hud/hud_shield.png]
/// (sem -nographics e sem -quit; sai sozinho, código 0 = OK)
///
/// Por fase e passada (inglês, português, mobile com os controles de toque forçados): o ícone existe, é o icon_shield em
/// 28×28, fica dentro da tela e não encosta em nenhum outro elemento visível (textos medidos pelos glifos); pronto =
/// opaco; segurar X (teclado — o botão DEF do toque alimenta a mesma tecla) e LT (gamepad) ergue o ícone 3 un. e soltar o baixa;
/// um golpe de frente bloqueado pisca dourado e a conta da defesa continua 10 → 2. Grava o canto do HUD de cada passada e
/// o HUD de Troia em 5 resoluções. Restaura idioma, mobile, som e opções de play mode; não salva cena.
/// </summary>
public static class HudShieldProbe
{
    private static readonly string[] Fases =
    {
        "Level_01_Itaca_Prologue", "Level_02_Troia", "Level_08_Circe", "Level_10_Sereias", "Level_12_GadoDoSol",
        "Level_15_Pretendentes", "Level_16_Final",
    };

    private enum Modo { Ingles, Portugues, Mobile }

    private static readonly List<(string fase, Modo modo)> passadas = new List<(string, Modo)>();
    private static int atual, passo, falhas;
    private static double acordar;
    private static bool opcoesAtivas, mudoOriginal;
    private static EnterPlayModeOptions opcoes;
    private static Language idiomaOriginal;
    private static InputSettings.BackgroundBehavior fundo;
    private static InputSettings.EditorInputBehaviorInPlayMode editorIn;
    private static string saida;
    private static Keyboard kb;
    private static Gamepad pad;
    private static Image icone;
    private static float baseY;
    private static PlayerShield escudo;
    private static readonly List<(string rotulo, Texture2D foto)> fotos = new List<(string, Texture2D)>();

    public static void Run()
    {
        string[] a = System.Environment.GetCommandLineArgs();
        int i = System.Array.IndexOf(a, "-shieldOut");
        saida = i >= 0 && i < a.Length - 1 ? a[i + 1] : "Docs/QA/_hud/hud_shield.png";

        mudoOriginal = EditorUtility.audioMasterMute;
        EditorUtility.audioMasterMute = true;
        AudioListener.volume = 0f;

        // Sem janela com foco no batch: o input virtual tem de ir para o jogo (como no GamepadMenuProbe). Restaurado no fim.
        fundo = InputSystem.settings.backgroundBehavior;
        editorIn = InputSystem.settings.editorInputBehaviorInPlayMode;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;

        idiomaOriginal = Localization.Current;
        foreach (string f in Fases) { passadas.Add((f, Modo.Ingles)); passadas.Add((f, Modo.Portugues)); }
        foreach (string f in new[] { "Level_02_Troia", "Level_10_Sereias", "Level_16_Final" }) { passadas.Add((f, Modo.Mobile)); }

        opcoesAtivas = EditorSettings.enterPlayModeOptionsEnabled;
        opcoes = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;

        Preparar(passadas[0]);
        EditorSceneManager.OpenScene($"Assets/Scenes/Levels/{passadas[0].fase}.unity", OpenSceneMode.Single);
        atual = 0; passo = 0;
        EditorApplication.update += Tick;
        EditorApplication.EnterPlaymode();
    }

    private static void Preparar((string fase, Modo modo) p)
    {
        bool pt = p.modo == Modo.Portugues || (p.modo == Modo.Mobile && p.fase != "Level_02_Troia");
        Localization.Current = pt ? Language.Portuguese : Language.English;
        MobilePlatformDetector.DebugOverride(p.modo == Modo.Mobile ? true : (bool?)false);
    }

    private static string Nome => $"{passadas[atual].fase} [{passadas[atual].modo}]";
    private static double Agora => EditorApplication.timeSinceStartup;
    private static void Falha(string m) { falhas++; Debug.LogError($"[Escudo] {Nome}: FALHA — {m}"); }

    private static void Tick()
    {
        if (!EditorApplication.isPlaying || Agora < acordar) { return; }
        try { Passo(); }
        catch (System.Exception e) { Falha(e.ToString()); Proxima(); }
    }

    private static void Passo()
    {
        switch (passo)
        {
            case 0:
                AudioListener.volume = 0f;
                if (kb == null) { kb = InputSystem.AddDevice<Keyboard>("QAShieldKeyboard"); pad = InputSystem.AddDevice<Gamepad>("QAShieldPad"); }
                acordar = Agora + 3.0; passo = 10;
                return;

            case 10: // diálogo de abertura (trava o jogador): pula como o jogador faria
                int abertos = 0;
                foreach (Odisseia.UI.DialogueSequence d in Object.FindObjectsByType<Odisseia.UI.DialogueSequence>()) { d.Skip(); abertos++; }
                Debug.Log($"[Escudo] {Nome}: {abertos} diálogo(s) pulado(s)");
                acordar = Agora + 1.0; passo = 1;
                return;

            case 1: // presença, tamanho, posição e sobreposição
                var hud = Object.FindObjectsByType<Odisseia.UI.HUD>().FirstOrDefault();
                escudo = Object.FindObjectsByType<PlayerShield>().FirstOrDefault();
                icone = hud != null ? hud.GetComponentsInChildren<Image>(true).FirstOrDefault(im => im.name == "Icon_icon_shield") : null;
                if (hud == null || escudo == null || icone == null) { Falha($"HUD {hud != null}, escudo {escudo != null}, ícone {icone != null}"); Proxima(); return; }
                if (!escudo.IsAvailable) { Falha("escudo indisponível nesta fase"); }
                if (!icone.gameObject.activeInHierarchy) { Falha("ícone desligado"); }
                if (icone.sprite == null || icone.sprite.name != "icon_shield") { Falha($"sprite {icone.sprite?.name}"); }
                var r = (RectTransform)icone.transform;
                if (r.sizeDelta != new Vector2(28, 28)) { Falha($"tamanho {r.sizeDelta}"); }
                if (icone.color != Color.white) { Falha($"pronto deveria ser opaco e sem tinta, cor {icone.color}"); }
                baseY = r.anchoredPosition.y;
                Rect rIcone = NaTela(r);
                if (rIcone.xMin < 0 || rIcone.yMin < 0 || rIcone.xMax > Screen.width || rIcone.yMax > Screen.height) { Falha($"cortado: {rIcone} na tela {Screen.width}×{Screen.height}"); }
                var colisoes = new List<string>();
                foreach (Graphic g in Object.FindObjectsByType<Graphic>())
                {
                    if (g == icone || !g.gameObject.activeInHierarchy || !g.enabled || g.canvas == null || g.color.a < 0.05f) { continue; }
                    if (g.GetComponentInParent<Canvas>().rootCanvas.renderMode == RenderMode.WorldSpace) { continue; }
                    Rect rg = Extensao(g);
                    if (rg.width <= 0 || rg.width * rg.height > 0.4f * Screen.width * Screen.height) { continue; } // fundos de tela
                    if (rg.Overlaps(rIcone)) { colisoes.Add($"{g.name} {rg}"); }
                }
                if (colisoes.Count > 0) { Falha("encosta em: " + string.Join("; ", colisoes)); }
                Debug.Log($"[Escudo] {Nome}: ícone {icone.sprite.name} {r.sizeDelta} em {r.anchoredPosition}, tela {rIcone} de {Screen.width}×{Screen.height}, sem sobreposição: {colisoes.Count == 0}");
                fotos.Add(($"{passadas[atual].fase.Substring(6, 2)} {passadas[atual].modo}", Foto(1280, 720, true)));
                if (passadas[atual].modo == Modo.Mobile) { BotoesDeToque(); }
                InputSystem.QueueStateEvent(kb, new KeyboardState(Key.X));
                acordar = Agora + 0.3; passo = 2;
                return;

            case 2: // teclado (e toque) segurando
                Conferir("teclado X segurado", bloqueando: true);
                if (atual == 2) { fotos.Add(("02 defendendo", Foto(1280, 720, true))); }
                // Golpe pela frente: um dos lados está na frente, conforme o Odisseu estiver virado.
                Vector2 p0 = escudo.transform.position;
                int d1 = escudo.Mitigate(10, new DamageInfo(p0 + Vector2.right * 1.5f));
                int d2 = escudo.Mitigate(10, new DamageInfo(p0 + Vector2.left * 1.5f));
                int bloqueado = Mathf.Min(d1, d2), costas = Mathf.Max(d1, d2);
                if (bloqueado != 2 || costas != 10) { Falha($"conta da defesa mudou: frente {bloqueado}, costas {costas} (esperado 2 e 10)"); }
                passo = 3; acordar = Agora + 0.05;
                return;

            case 3: // brilho do bloqueio
                if (icone.color != Odisseia.UI.UITheme.TextAccent) { Falha($"sem brilho dourado no bloqueio ({icone.color})"); }
                else { Debug.Log($"[Escudo] {Nome}: bloqueio 10 → 2 de frente, 10 pelas costas; ícone piscou dourado — OK"); }
                acordar = Agora + 0.4; passo = 4;
                return;

            case 4:
                Conferir("depois do brilho, ainda segurando", bloqueando: true);
                InputSystem.QueueStateEvent(kb, new KeyboardState());
                acordar = Agora + 0.3; passo = 5;
                return;

            case 5:
                Conferir("teclado solto", bloqueando: false);
                InputSystem.QueueStateEvent(pad, new GamepadState { leftTrigger = 1f });
                acordar = Agora + 0.3; passo = 6;
                return;

            case 6:
                Conferir("gamepad LT segurado", bloqueando: true);
                InputSystem.QueueStateEvent(pad, new GamepadState());
                acordar = Agora + 0.3; passo = 7;
                return;

            case 7:
                Conferir("gamepad solto", bloqueando: false);
                if (atual == 2) // Troia em inglês: o canto do HUD em várias resoluções
                {
                    foreach (Vector2Int t in new[] { new Vector2Int(1280, 720), new Vector2Int(1920, 1080), new Vector2Int(960, 540), new Vector2Int(2340, 1080), new Vector2Int(1024, 768) })
                    {
                        fotos.Add(($"{t.x}x{t.y}", Foto(t.x, t.y, true)));
                    }
                }
                Proxima();
                return;
        }
    }

    /// <summary>
    /// 13B.4: o rótulo de cada botão de toque é o da tabela de idiomas (a mesma chave da dica do ControlHints) e troca
    /// junto com o idioma, sem recriar os botões.
    /// </summary>
    private static void BotoesDeToque()
    {
        var botoes = new[] { ("AttackButton", "Attack"), ("ShieldButton", "Shield"), ("JumpButton", "Jump"), ("BowButton", "Bow"), ("InteractButton", "Interact") };
        Language antes = Localization.Current;
        foreach (Language idioma in new[] { antes, antes == Language.English ? Language.Portuguese : Language.English })
        {
            Localization.Current = idioma;
            var vistos = new List<string>();
            foreach (var (nome, acao) in botoes)
            {
                GameObject go = GameObject.Find(nome);
                Text t = go != null ? go.GetComponentInChildren<Text>(true) : null;
                string esperado = Localization.Get(ControlHints.TouchKey(acao));
                vistos.Add($"{nome}={t?.text}");
                if (t == null || t.text != esperado) { Falha($"{idioma}: {nome} mostra \"{t?.text}\" (esperado \"{esperado}\")"); }
            }
            Debug.Log($"[Escudo] {Nome}: botões de toque em {idioma}: {string.Join(" · ", vistos)}");
        }
        Localization.Current = antes;
    }

    private static void Conferir(string etapa, bool bloqueando)
    {
        float subiu = icone.rectTransform.anchoredPosition.y - baseY;
        bool ok = escudo.IsBlocking == bloqueando && Mathf.Abs(subiu - (bloqueando ? 3f : 0f)) < 0.01f && icone.color == Color.white && icone.gameObject.activeInHierarchy;
        if (!ok) { Falha($"{etapa}: IsBlocking {escudo.IsBlocking}, subiu {subiu:0.0}, cor {icone.color}"); }
        else { Debug.Log($"[Escudo] {Nome}: {etapa} → IsBlocking {escudo.IsBlocking}, ícone subiu {subiu:0} un. — OK"); }
    }

    /// <summary>Retângulo na tela (pixels). Para Text, pela caixa dos glifos de fato desenhados.</summary>
    private static Rect Extensao(Graphic g)
    {
        if (g is Text t)
        {
            if (string.IsNullOrEmpty(t.text)) { return Rect.zero; }
            IList<UIVertex> v = t.cachedTextGenerator.verts;
            if (v == null || v.Count == 0) { return Rect.zero; }
            Vector2 min = Vector2.positiveInfinity, max = Vector2.negativeInfinity;
            float escala = 1f / t.pixelsPerUnit;
            foreach (UIVertex u in v)
            {
                Vector3 w = t.rectTransform.TransformPoint(u.position * escala);
                Vector2 s = RectTransformUtility.WorldToScreenPoint(Cam(g), w);
                min = Vector2.Min(min, s); max = Vector2.Max(max, s);
            }
            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }
        return NaTela(g.rectTransform);
    }

    private static Camera Cam(Graphic g)
    {
        Canvas c = g.canvas.rootCanvas;
        return c.renderMode == RenderMode.ScreenSpaceOverlay ? null : c.worldCamera;
    }

    private static Rect NaTela(RectTransform r)
    {
        var cantos = new Vector3[4];
        r.GetWorldCorners(cantos);
        Canvas c = r.GetComponentInParent<Canvas>().rootCanvas;
        Camera cam = c.renderMode == RenderMode.ScreenSpaceOverlay ? null : c.worldCamera;
        Vector2 a = RectTransformUtility.WorldToScreenPoint(cam, cantos[0]);
        Vector2 b = RectTransformUtility.WorldToScreenPoint(cam, cantos[2]);
        return Rect.MinMaxRect(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y), Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y));
    }

    /// <summary>Render com o HUD (como o HudShot) e recorte do canto superior esquerdo, 34% × 42% da tela.</summary>
    private static Texture2D Foto(int w, int h, bool canto)
    {
        Camera cam = Camera.main;
        var rt = new RenderTexture(w, h, 24) { filterMode = FilterMode.Point };
        var canvases = Object.FindObjectsByType<Canvas>().Where(c => c.isRootCanvas && c.renderMode == RenderMode.ScreenSpaceOverlay).ToList();
        foreach (Canvas c in canvases) { c.renderMode = RenderMode.ScreenSpaceCamera; c.worldCamera = cam; c.planeDistance = 1f; }
        cam.targetTexture = rt;
        Escalar();
        Canvas.ForceUpdateCanvases();
        cam.Render();
        RenderTexture.active = rt;
        int cw = canto ? Mathf.RoundToInt(w * 0.34f) : w, ch = canto ? Mathf.RoundToInt(h * 0.42f) : h;
        var tex = new Texture2D(cw, ch, TextureFormat.RGB24, false) { hideFlags = HideFlags.HideAndDontSave };
        tex.ReadPixels(new Rect(0, h - ch, cw, ch), 0, 0);
        tex.Apply();
        RenderTexture.active = null;
        cam.targetTexture = null;
        foreach (Canvas c in canvases) { c.renderMode = RenderMode.ScreenSpaceOverlay; }
        Escalar();
        rt.Release();
        return tex;
    }

    private static void Proxima()
    {
        InputSystem.QueueStateEvent(kb, new KeyboardState());
        InputSystem.QueueStateEvent(pad, new GamepadState());
        atual++;
        if (atual >= passadas.Count) { Encerrar(); return; }
        Preparar(passadas[atual]);
        if (passadas[atual].modo == Modo.Mobile && passadas[atual - 1].modo != Modo.Mobile)
        {
            // O MobileControlsRoot nasce uma vez (DontDestroyOnLoad) e se desliga fora do mobile: recria com o mobile forçado.
            foreach (var m in Resources.FindObjectsOfTypeAll<Odisseia.UI.MobileControlsRoot>().Where(m => m.gameObject.scene.IsValid()))
            {
                Object.DestroyImmediate(m.gameObject);
            }
            _ = Odisseia.UI.MobileControlsRoot.Instance;
        }
        passo = 0;
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(passadas[atual].fase);
        acordar = Agora + 0.5;
    }

    private static void Encerrar()
    {
        EditorApplication.update -= Tick;
        // Folha: cada recorte reduzido/ampliado (vizinho mais próximo) para 300 px de altura, 4 por linha.
        const int alt = 300, porLinha = 4, folga = 6;
        var redim = fotos.Select(f => Redimensionar(f.foto, Mathf.RoundToInt(f.foto.width * (float)alt / f.foto.height), alt)).ToList();
        int larg = redim.Max(t => t.width);
        int linhas = (redim.Count + porLinha - 1) / porLinha;
        var folha = new Texture2D(porLinha * (larg + folga), linhas * (alt + folga), TextureFormat.RGB24, false);
        folha.SetPixels(Enumerable.Repeat(new Color(0.1f, 0.1f, 0.1f), folha.width * folha.height).ToArray());
        for (int i = 0; i < redim.Count; i++)
        {
            int x = (i % porLinha) * (larg + folga), y = (linhas - 1 - i / porLinha) * (alt + folga);
            folha.SetPixels(x, y, redim[i].width, alt, redim[i].GetPixels());
        }
        folha.Apply();
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(saida));
        System.IO.File.WriteAllBytes(saida, folha.EncodeToPNG());
        Debug.Log($"[Escudo] {saida}: {string.Join(" | ", fotos.Select((f, i) => $"{i + 1}={f.rotulo}"))}");

        Localization.Current = idiomaOriginal;
        InputSystem.settings.backgroundBehavior = fundo;
        InputSystem.settings.editorInputBehaviorInPlayMode = editorIn;
        MobilePlatformDetector.DebugOverride(null);
        if (kb != null) { InputSystem.RemoveDevice(kb); InputSystem.RemoveDevice(pad); }
        Debug.Log(falhas == 0 ? $"[Escudo] RESULTADO: OK ({passadas.Count} passadas)" : $"[Escudo] RESULTADO: {falhas} FALHA(S)");
        EditorApplication.ExitPlaymode();
        AudioListener.volume = 1f;
        EditorUtility.audioMasterMute = mudoOriginal;
        EditorSettings.enterPlayModeOptionsEnabled = opcoesAtivas;
        EditorSettings.enterPlayModeOptions = opcoes;
        EditorApplication.Exit(falhas == 0 ? 0 : 1);
    }

    private static Texture2D Redimensionar(Texture2D src, int w, int h)
    {
        var dst = new Texture2D(w, h, TextureFormat.RGB24, false) { hideFlags = HideFlags.HideAndDontSave };
        Color[] s = src.GetPixels(), d = new Color[w * h];
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++) { d[y * w + x] = s[(y * src.height / h) * src.width + x * src.width / w]; }
        }
        dst.SetPixels(d);
        dst.Apply();
        return dst;
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
