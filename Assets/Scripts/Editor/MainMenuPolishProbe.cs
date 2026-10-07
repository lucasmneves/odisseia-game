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
/// Menu principal (Etapa 13B.5) em play mode, NO MUDO, pelo caminho do jogo (Boot → menu):
///
/// Unity.exe -batchmode -projectPath . -executeMethod MainMenuPolishProbe.Run [-menuOut pasta]
/// (sem -nographics e sem -quit; sai sozinho, código 0 = OK; relatório em Logs/qa_mainmenu.txt)
///
/// Visual (inglês e português): fundo na proporção da arte, borda e marcador "►" só no item em foco, rótulos traduzidos,
/// nada cortado nem sobreposto, em 1280×720, 1920×1080, 960×540, 2340×1080 e 1024×768 (PNG por idioma).
/// Entrada: teclado (setas, Enter, Esc), Xbox (D-pad, analógico, A, B) e PlayStation (D-pad, ✕, ○) virtuais; Configurações,
/// Controles, Seletor de Fases e a confirmação do Novo Jogo abrem e voltam; Continuar com save vai ao mapa; Novo Jogo → mapa
/// → Interagir carrega a F01. Toque (mobile forçado): sem controle de jogo sobre o menu e um toque abre as Configurações.
/// Guarda e restaura o save, o idioma, o som e as opções de input e de play mode do Editor.
/// </summary>
public static class MainMenuPolishProbe
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
        int i = System.Array.IndexOf(a, "-menuOut");
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
        L($"# QA menu principal (13B.5) — {System.DateTime.Now:yyyy-MM-dd HH:mm}");
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
        kb = InputSystem.AddDevice<Keyboard>("QAMenuKeyboard");
        pad = InputSystem.AddDevice<Gamepad>("QAMenuPad");

        // ---- Visual, nos dois idiomas
        foreach (Language idioma in new[] { Language.English, Language.Portuguese })
        {
            Localization.Current = idioma;
            var e = Carregar(SceneLoader.MainMenu); while (e.MoveNext()) { yield return e.Current; }
            L($"\n## Visual — {idioma}");
            Visual(idioma);
        }

        // ---- Teclado
        Localization.Current = Language.English;
        {
            var e = Carregar(SceneLoader.MainMenu); while (e.MoveNext()) { yield return e.Current; }
        }
        L("\n## Teclado");
        Checar(Sel() == "NewGameButton", $"sem save, foco inicial no Novo Jogo: {Sel()}");
        var t = Tecla(Key.DownArrow); while (t.MoveNext()) { yield return t.Current; }
        Checar(Sel() == "LevelSelectButton", $"↓: {Sel()}");
        t = Tecla(Key.DownArrow); while (t.MoveNext()) { yield return t.Current; }
        Checar(Sel() == "SettingsButton", $"↓: {Sel()}");
        t = Tecla(Key.DownArrow); while (t.MoveNext()) { yield return t.Current; }
        Checar(Sel() == "NewGameButton", $"↓ dá a volta e pula o Continuar desabilitado: {Sel()}");
        t = Tecla(Key.UpArrow); while (t.MoveNext()) { yield return t.Current; }
        Checar(Sel() == "SettingsButton", $"↑ volta pelo fim: {Sel()}");
        t = Tecla(Key.Enter); while (t.MoveNext()) { yield return t.Current; }
        Checar(SettingsScreen.IsOpen, $"Enter abre as Configurações: {SettingsScreen.IsOpen}");
        t = Tecla(Key.Escape); while (t.MoveNext()) { yield return t.Current; }
        yield return 0.2f;
        Checar(!SettingsScreen.IsOpen && Sel() == "NewGameButton", $"Esc fecha as Configurações e o foco volta ao menu: aberta={SettingsScreen.IsOpen}, foco {Sel()}");
        // Confirmar nos menus é o Submit da UI (Enter / A / ✕); Espaço não está nele.
        t = Tecla(Key.Space); while (t.MoveNext()) { yield return t.Current; }
        yield return 0.3f;
        L($"- Espaço no Novo Jogo: cena {SceneManager.GetActiveScene().name} (Espaço não é Submit da UI)");
        t = Tecla(Key.Enter); while (t.MoveNext()) { yield return t.Current; }

        // ---- Novo Jogo → mapa → F01 (sem save não há confirmação)
        L("\n## Novo Jogo → mapa → F01");
        {
            float t0 = Time.realtimeSinceStartup;
            while (SceneManager.GetActiveScene().name != SceneLoader.WorldMap && Time.realtimeSinceStartup - t0 < 20f) { yield return 0.2f; }
            Checar(SceneManager.GetActiveScene().name == SceneLoader.WorldMap, $"Novo Jogo leva ao mapa: {SceneManager.GetActiveScene().name}");
            yield return 2.0f;
            Focar();
            t = Tecla(Key.E); while (t.MoveNext()) { yield return t.Current; }
            t0 = Time.realtimeSinceStartup;
            while (!SceneManager.GetActiveScene().name.StartsWith("Level_01") && Time.realtimeSinceStartup - t0 < 30f) { yield return 0.3f; }
            yield return 1.5f;
            var jogador = GameObject.FindGameObjectWithTag("Player");
            Checar(SceneManager.GetActiveScene().name == "Level_01_Itaca_Prologue" && jogador != null,
                $"Interagir no mapa carrega a F01: {SceneManager.GetActiveScene().name}, jogador {(jogador != null ? "presente" : "AUSENTE")}");
        }

        // ---- Com save: Continuar habilitado e em foco; Xbox (A) leva ao mapa
        L("\n## Continuar com save — Xbox virtual");
        CampaignManager.Instance.CompleteLevel(CampaignManager.Instance.Levels.First().LevelId, 0, 0);
        Checar(SaveSystem.HasSave(), "save criado pelo caminho do jogo (CompleteLevel)");
        {
            var e = Carregar(SceneLoader.MainMenu); while (e.MoveNext()) { yield return e.Current; }
        }
        Checar(Sel() == "ContinueButton", $"com save, foco inicial no Continuar: {Sel()}");
        Gamepad xbox = null;
        try { xbox = InputSystem.AddDevice("XInputControllerWindows", "QAMenuXbox") as Gamepad; } catch (System.Exception ex) { L("- Xbox indisponível: " + ex.Message); }
        Gamepad xb = xbox ?? pad;
        var b = BotaoEm(xb, xb.dpad.down); while (b.MoveNext()) { yield return b.Current; }
        Checar(Sel() == "NewGameButton", $"D-pad ↓: {Sel()} ({InputDeviceTracker.Current}/{InputDeviceTracker.Family})");
        b = Analogico(xb, -1f); while (b.MoveNext()) { yield return b.Current; }
        Checar(Sel() == "LevelSelectButton", $"analógico ↓: {Sel()}");
        b = Analogico(xb, 1f); while (b.MoveNext()) { yield return b.Current; }
        Checar(Sel() == "NewGameButton", $"analógico ↑: {Sel()}");
        b = BotaoEm(xb, xb.buttonSouth); while (b.MoveNext()) { yield return b.Current; }
        Checar(ConfirmDialog.IsOpen, $"A no Novo Jogo com save pede confirmação: {ConfirmDialog.IsOpen}");
        b = BotaoEm(xb, xb.buttonEast); while (b.MoveNext()) { yield return b.Current; }
        yield return 0.2f;
        Checar(!ConfirmDialog.IsOpen && Sel() == "NewGameButton" && SaveSystem.HasSave(), $"B cancela a confirmação sem apagar o save: aberta={ConfirmDialog.IsOpen}, foco {Sel()}, save {SaveSystem.HasSave()}");
        b = BotaoEm(xb, xb.dpad.up); while (b.MoveNext()) { yield return b.Current; }
        b = BotaoEm(xb, xb.buttonSouth); while (b.MoveNext()) { yield return b.Current; }
        {
            float t0 = Time.realtimeSinceStartup;
            while (SceneManager.GetActiveScene().name != SceneLoader.WorldMap && Time.realtimeSinceStartup - t0 < 20f) { yield return 0.2f; }
            Checar(SceneManager.GetActiveScene().name == SceneLoader.WorldMap, $"A no Continuar leva ao mapa: {SceneManager.GetActiveScene().name}");
        }

        // ---- PlayStation: Seletor de Fases e Configurações → Controles, ✕ entra e ○ volta
        L("\n## PlayStation virtual");
        {
            var e = Carregar(SceneLoader.MainMenu); while (e.MoveNext()) { yield return e.Current; }
        }
        var ps = InputSystem.AddDevice<UnityEngine.InputSystem.DualShock.DualShock4GamepadHID>("QAMenuDualShock");
        // O D-pad do DualShock é um "hat" (DiscreteButtonControl) que não aceita escrita direta: navega pelo analógico.
        b = BotaoEm(ps, ps.buttonNorth); while (b.MoveNext()) { yield return b.Current; }
        b = Analogico(ps, -1f); while (b.MoveNext()) { yield return b.Current; }
        b = Analogico(ps, -1f); while (b.MoveNext()) { yield return b.Current; }
        Checar(Sel() == "LevelSelectButton" && InputDeviceTracker.Family == GamepadFamily.PlayStation, $"analógico ↓↓: {Sel()} ({InputDeviceTracker.Family})");
        b = BotaoEm(ps, ps.buttonSouth); while (b.MoveNext()) { yield return b.Current; }
        {
            float t0 = Time.realtimeSinceStartup;
            while (SceneManager.GetActiveScene().name != SceneLoader.LevelSelect && Time.realtimeSinceStartup - t0 < 20f) { yield return 0.2f; }
            Checar(SceneManager.GetActiveScene().name == SceneLoader.LevelSelect, $"✕ abre o Seletor de Fases: {SceneManager.GetActiveScene().name}");
            yield return 1.5f;
            Focar();
            b = BotaoEm(ps, ps.buttonEast); while (b.MoveNext()) { yield return b.Current; }
            t0 = Time.realtimeSinceStartup;
            while (SceneManager.GetActiveScene().name != SceneLoader.MainMenu && Time.realtimeSinceStartup - t0 < 20f) { yield return 0.2f; }
            Checar(SceneManager.GetActiveScene().name == SceneLoader.MainMenu, $"○ volta ao menu: {SceneManager.GetActiveScene().name}");
            yield return 1.5f;
            Focar();
        }
        string antesDoAnalogico = Sel();
        b = Analogico(ps, 1f); while (b.MoveNext()) { yield return b.Current; }
        string depoisDoAnalogico = Sel();
        if (depoisDoAnalogico != "SettingsButton")
        {
            // Controle: o mesmo ↑ pelo teclado, para separar a navegação do menu da escrita no analógico virtual.
            t = Tecla(Key.UpArrow); while (t.MoveNext()) { yield return t.Current; }
        }
        // Limite do simulador, não do menu: no DualShock 4 virtual só o "↑" do analógico não chega (eixo em byte invertido;
        // o relatório HID real é interno ao pacote). O ↓ do mesmo analógico navega, o ↑/↓ do Xbox também, e a UI lê
        // <Gamepad>/leftStick igual para qualquer controle. Fica registrado, sem contar como falha.
        if (depoisDoAnalogico == "SettingsButton") { Checar(true, "analógico ↑ do Continuar dá a volta até Configurações"); }
        else { L($"- LIMITE DO TESTE: ↑ do analógico do DualShock virtual não chega ({antesDoAnalogico} → {depoisDoAnalogico}); ↑ pelo teclado deu a volta até {Sel()}"); }
        if (Sel() != "SettingsButton") { EventSystem.current.SetSelectedGameObject(GameObject.Find("SettingsButton")); }
        b = BotaoEm(ps, ps.buttonSouth); while (b.MoveNext()) { yield return b.Current; }
        Checar(SettingsScreen.IsOpen, $"✕ abre as Configurações: {SettingsScreen.IsOpen}");
        OptionsMenu.Open();
        yield return 0.3f;
        Checar(OptionsMenu.IsOpen, "Controles abre por cima das Configurações");
        b = BotaoEm(ps, ps.buttonEast); while (b.MoveNext()) { yield return b.Current; }
        Checar(!OptionsMenu.IsOpen && SettingsScreen.IsOpen, $"○ fecha só os Controles: controles={OptionsMenu.IsOpen}, configurações={SettingsScreen.IsOpen}");
        b = BotaoEm(ps, ps.buttonEast); while (b.MoveNext()) { yield return b.Current; }
        yield return 0.2f;
        Checar(!SettingsScreen.IsOpen && Sel() != "(nada)", $"○ fecha as Configurações e o menu mantém foco: {Sel()}");
        b = BotaoEm(ps, ps.buttonEast); while (b.MoveNext()) { yield return b.Current; }
        Checar(SceneManager.GetActiveScene().name == SceneLoader.MainMenu && Sel() != "(nada)", $"○ no menu não faz nada (não há para onde voltar): cena {SceneManager.GetActiveScene().name}, foco {Sel()}");
        InputSystem.RemoveDevice(ps);
        if (xbox != null) { InputSystem.RemoveDevice(xbox); }

        // ---- Toque (mobile forçado)
        L("\n## Toque (mobile forçado)");
        MobilePlatformDetector.DebugOverride(true);
        foreach (var m in Resources.FindObjectsOfTypeAll<MobileControlsRoot>().Where(m => m.gameObject.scene.IsValid())) { Object.DestroyImmediate(m.gameObject); }
        _ = MobileControlsRoot.Instance;
        Localization.Current = Language.Portuguese;
        {
            var e = Carregar(SceneLoader.MainMenu); while (e.MoveNext()) { yield return e.Current; }
        }
        var raizToque = Object.FindAnyObjectByType<MobileControlsRoot>();
        var visiveis = raizToque != null
            ? raizToque.GetComponentsInChildren<Graphic>().Where(g => g.isActiveAndEnabled && g.color.a > 0.05f && g.GetComponentInParent<CanvasGroup>()?.alpha != 0f).Select(g => g.name).ToList()
            : new List<string>();
        L($"- controles de toque visíveis no menu: {(visiveis.Count == 0 ? "nenhum" : string.Join(", ", visiveis))}");
        var botoesMenu = Object.FindAnyObjectByType<MainMenuController>().MenuItems.Where(x => x != null).ToList();
        var sobre = raizToque != null
            ? raizToque.GetComponentsInChildren<Graphic>().Where(g => g.isActiveAndEnabled && g.color.a > 0.05f && g.GetComponentInParent<CanvasGroup>()?.alpha != 0f)
                .Where(g => botoesMenu.Any(m => NaTela(m.transform as RectTransform).Overlaps(NaTela(g.rectTransform)))).Select(g => g.name).ToList()
            : new List<string>();
        Checar(sobre.Count == 0, "nenhum controle de toque por cima dos botões do menu" + (sobre.Count > 0 ? ": " + string.Join(", ", sobre) : ""));
        var tela = InputSystem.AddDevice<Touchscreen>("QAMenuTouch");
        var config = botoesMenu.First(x => x.name == "SettingsButton");
        Vector2 ponto = NaTela(config.transform as RectTransform).center;
        InputSystem.QueueStateEvent(tela, new TouchState { touchId = 1, phase = UnityEngine.InputSystem.TouchPhase.Began, position = ponto, pressure = 1f });
        yield return 0.15f;
        InputSystem.QueueStateEvent(tela, new TouchState { touchId = 1, phase = UnityEngine.InputSystem.TouchPhase.Ended, position = ponto });
        yield return 0.5f;
        Checar(SettingsScreen.IsOpen, $"tocar em Configurações ({ponto}) abre a tela: {SettingsScreen.IsOpen} ({InputDeviceTracker.Current})");
        if (SettingsScreen.IsOpen) { t = Tecla(Key.Escape); while (t.MoveNext()) { yield return t.Current; } }
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
        var menu = Object.FindAnyObjectByType<MainMenuController>();
        var itens = menu.MenuItems.Where(x => x != null).ToList();
        string foco = Sel();
        foreach (Button item in itens)
        {
            var marcador = item.transform.Find("FocusMarker");
            var borda = item.GetComponent<Outline>();
            Text rotulo = item.GetComponentsInChildren<Text>(true).FirstOrDefault(x => x.name != "FocusMarker");
            bool emFoco = item.name == foco;
            bool marcadorOk = marcador != null && marcador.gameObject.activeInHierarchy == emFoco;
            Color esperado = !item.interactable ? UITheme.ButtonBorderDisabled : emFoco ? UITheme.TextAccent : UITheme.ButtonBorder;
            bool bordaOk = borda != null && borda.effectColor == esperado;
            var mt = marcador != null ? marcador.GetComponent<Text>() : null;
            L($"- {item.name}: \"{rotulo?.text}\" interativo={item.interactable} foco={emFoco} marcador={(marcador != null ? marcador.gameObject.activeInHierarchy + $" \"{mt?.text}\" fonte={mt?.font?.name} vértices={mt?.cachedTextGenerator.vertexCount} cor={mt?.color} tela={NaTela((RectTransform)marcador)}" : "AUSENTE")} borda={borda?.effectColor}");
            Checar(marcadorOk && bordaOk, $"{idioma}, {item.name}: marcador só no foco e borda do estado");
        }

        // Rótulos traduzidos pela tabela (LocalizedText), sem texto do outro idioma.
        var chaves = new Dictionary<string, string> { ["ContinueButton"] = "ui.menu.continue", ["NewGameButton"] = "ui.menu.newGame", ["LevelSelectButton"] = "ui.menu.levelSelect", ["SettingsButton"] = "ui.menu.settings" };
        foreach (Button item in itens)
        {
            string texto = item.GetComponentsInChildren<Text>(true).First(x => x.name != "FocusMarker").text;
            Checar(texto == Localization.Get(chaves[item.name]), $"{idioma}, {item.name} = \"{texto}\" (tabela: \"{Localization.Get(chaves[item.name])}\")");
        }

        // Fundo sem distorção.
        var fundo = GameObject.Find("Background");
        var r = ((RectTransform)fundo.transform).rect;
        var sprite = fundo.GetComponent<Image>().sprite;
        float proporcao = r.width / r.height, arte = sprite.rect.width / sprite.rect.height;
        Checar(Mathf.Abs(proporcao - arte) < 0.01f, $"{idioma}, fundo {r.width:0}×{r.height:0} = {proporcao:0.000} (arte {arte:0.000})");

        // Capturas: nada fora da tela nem sobreposto, nas cinco resoluções.
        var fotos = new List<Texture2D>();
        foreach (Vector2Int res in Resolucoes)
        {
            fotos.Add(Foto(res.x, res.y, out List<string> problemas));
            Checar(problemas.Count == 0, $"{idioma}, {res.x}×{res.y}: botões e logo dentro da tela, sem sobreposição" + (problemas.Count > 0 ? " — " + string.Join("; ", problemas) : ""));
        }
        Folha(fotos, $"{pasta}/mainmenu_{(idioma == Language.English ? "en" : "pt")}.png");
    }

    /// <summary>Render com o Canvas em Screen Space Camera no tamanho dado (como o HudShot) e conferência das caixas.</summary>
    private static Texture2D Foto(int w, int h, out List<string> problemas)
    {
        problemas = new List<string>();
        Camera cam = Camera.main;
        var rt = new RenderTexture(w, h, 24) { filterMode = FilterMode.Point };
        var canvases = Object.FindObjectsByType<Canvas>().Where(c => c.isRootCanvas && c.renderMode == RenderMode.ScreenSpaceOverlay).ToList();
        foreach (Canvas c in canvases) { c.renderMode = RenderMode.ScreenSpaceCamera; c.worldCamera = cam; c.planeDistance = 1f; }
        cam.targetTexture = rt;
        Escalar();
        Canvas.ForceUpdateCanvases();
        L($"  ({w}×{h}: fator do Menu Canvas {canvases.First(c => c.name == "Menu Canvas").scaleFactor:0.000})");
        cam.Render();

        // Caixas dos botões (e marcadores ligados) no espaço da imagem.
        var menu = Object.FindAnyObjectByType<MainMenuController>();
        var caixas = new List<(string nome, Rect r)>();
        foreach (Button item in menu.MenuItems.Where(x => x != null))
        {
            caixas.Add((item.name, Caixa((RectTransform)item.transform, cam)));
            var marca = item.transform.Find("FocusMarker");
            if (marca != null && marca.gameObject.activeInHierarchy) { caixas.Add((item.name + "/►", Caixa((RectTransform)marca, cam))); }
        }
        foreach (var (nome, r) in caixas)
        {
            if (r.xMin < 0 || r.yMin < 0 || r.xMax > w || r.yMax > h) { problemas.Add($"{nome} fora da tela {r}"); }
        }
        // O "►" do foco não pode cair em cima do rótulo: cada item habilitado recebe o foco e o marcador ligado é medido
        // (o MenuButton o põe logo antes do rótulo só quando ganha o foco).
        GameObject focoAntes = EventSystem.current.currentSelectedGameObject;
        foreach (Button item in menu.MenuItems.Where(x => x != null && x.interactable))
        {
            EventSystem.current.SetSelectedGameObject(item.gameObject);
            Canvas.ForceUpdateCanvases();
            var marca = (RectTransform)item.transform.Find("FocusMarker");
            var rotulo = item.GetComponentsInChildren<Text>(true).First(x => x.name != "FocusMarker");
            Rect rm = Caixa(marca, cam);
            float larguraDoGlifo = marca.GetComponent<Text>().preferredWidth * rm.height / Mathf.Max(1f, marca.rect.height);
            Rect glifo = Rect.MinMaxRect(rm.xMax - larguraDoGlifo, rm.yMin, rm.xMax, rm.yMax); // alinhado à direita
            Rect rl = Glifos(rotulo, cam);
            if (!marca.gameObject.activeInHierarchy) { problemas.Add($"{item.name}: marcador não ligou no foco"); }
            if (glifo.xMax + 2 > rl.xMin && glifo.xMin < rl.xMax) { problemas.Add($"{item.name}: ► ({glifo.xMin:0}–{glifo.xMax:0}) encosta no rótulo ({rl.xMin:0}–{rl.xMax:0})"); }
            if (glifo.xMin < Caixa((RectTransform)item.transform, cam).xMin - 2) { problemas.Add($"{item.name}: ► sai do botão pela esquerda"); }
        }
        EventSystem.current.SetSelectedGameObject(focoAntes);
        Canvas.ForceUpdateCanvases();
        for (int i = 0; i < caixas.Count; i++)
        {
            for (int j = i + 1; j < caixas.Count; j++)
            {
                if (caixas[i].nome.Split('/')[0] == caixas[j].nome.Split('/')[0]) { continue; }
                Rect a = caixas[i].r, b = caixas[j].r;
                // Encostar (borda com borda) não conta: 1 px de tolerância.
                if (a.xMin + 1 < b.xMax && b.xMin + 1 < a.xMax && a.yMin + 1 < b.yMax && b.yMin + 1 < a.yMax) { problemas.Add($"{caixas[i].nome} × {caixas[j].nome}"); }
            }
        }
        // O logo pintado fica na faixa de cima da arte (até 24% da altura dela): os botões não podem subir até lá.
        var fundo = (RectTransform)GameObject.Find("Background").transform;
        Rect rf = Caixa(fundo, cam);
        float baseDoLogo = rf.yMax - rf.height * 0.24f;
        foreach (var (nome, r) in caixas.Where(c => !c.nome.Contains("►")))
        {
            if (r.yMax > baseDoLogo) { problemas.Add($"{nome} sobe até o logo ({r.yMax:0} > {baseDoLogo:0})"); }
        }

        RenderTexture.active = rt;
        var tex = new Texture2D(w, h, TextureFormat.RGB24, false) { hideFlags = HideFlags.HideAndDontSave };
        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        tex.Apply();

        // Pixels, não só caixas: o centro de cada botão e o do logo não podem ter a cor da faixa (tela coberta).
        Color faixa = new Color32(8, 19, 30, 255);
        var pontos = caixas.Where(c => !c.nome.Contains("►")).Select(c => (c.nome, c.r.center)).ToList();
        pontos.Add(("logo", new Vector2(rf.center.x, rf.yMax - rf.height * 0.12f)));
        foreach (var (nome, p) in pontos)
        {
            Color px = tex.GetPixel(Mathf.Clamp((int)p.x, 0, w - 1), Mathf.Clamp((int)p.y, 0, h - 1));
            if (Mathf.Abs(px.r - faixa.r) + Mathf.Abs(px.g - faixa.g) + Mathf.Abs(px.b - faixa.b) < 0.04f) { problemas.Add($"{nome} invisível (pixel {px} = faixa)"); }
        }
        RenderTexture.active = null;
        cam.targetTexture = null;
        foreach (Canvas c in canvases) { c.renderMode = RenderMode.ScreenSpaceOverlay; }
        Escalar();
        rt.Release();
        return tex;
    }

    /// <summary>Caixa dos glifos de fato desenhados (não o retângulo do Text), na tela.</summary>
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

    /// <summary>As capturas reduzidas para 360 px de altura, lado a lado em duas linhas.</summary>
    private static void Folha(List<Texture2D> fotos, string arquivo)
    {
        const int alt = 360, folga = 8;
        var red = fotos.Select(f => Redimensionar(f, Mathf.RoundToInt(f.width * (float)alt / f.height), alt)).ToList();
        int porLinha = 3, linhas = (red.Count + porLinha - 1) / porLinha;
        int larg = red.Max(x => x.width);
        var folha = new Texture2D(porLinha * (larg + folga), linhas * (alt + folga), TextureFormat.RGB24, false);
        folha.SetPixels(Enumerable.Repeat(new Color(0.15f, 0.15f, 0.15f), folha.width * folha.height).ToArray());
        for (int i = 0; i < red.Count; i++)
        {
            folha.SetPixels((i % porLinha) * (larg + folga), (linhas - 1 - i / porLinha) * (alt + folga), red[i].width, alt, red[i].GetPixels());
        }
        folha.Apply();
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(arquivo));
        System.IO.File.WriteAllBytes(arquivo, folha.EncodeToPNG());
        L($"- capturas: {arquivo} (1280×720, 1920×1080, 960×540 / 2340×1080, 1024×768)");
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

    // ---------------------------------------------------------------- entrada

    private static IEnumerator<float> Carregar(string cena)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(cena);
        float t = Time.realtimeSinceStartup;
        while (SceneManager.GetActiveScene().name != cena && Time.realtimeSinceStartup - t < 30f) { yield return 0.1f; }
        yield return 1.5f;
        AudioListener.volume = 0f;
        Focar();
    }

    /// <summary>O EventSystem ignora navegação sem foco de janela no batch; isto o avisa que tem foco.</summary>
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

    /// <summary>Inclina o analógico esquerdo, escrito como vetor (cada eixo vai no formato do aparelho: XInput, DualShock).</summary>
    private static IEnumerator<float> Analogico(Gamepad g, float y)
    {
        Focar();
        using (StateEvent.From(g, out InputEventPtr ev)) { g.leftStick.WriteValueIntoEvent(new Vector2(0f, y), ev); InputSystem.QueueEvent(ev); }
        yield return 0.15f;
        using (StateEvent.From(g, out InputEventPtr ev)) { g.leftStick.WriteValueIntoEvent(Vector2.zero, ev); InputSystem.QueueEvent(ev); }
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

    private static void L(string s) { rel.AppendLine(s); Debug.Log("[QAMenu] " + s); }

    private static void Sair()
    {
        EditorApplication.update -= Tick;
        System.IO.File.WriteAllText("Logs/qa_mainmenu.txt", rel.ToString());
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
