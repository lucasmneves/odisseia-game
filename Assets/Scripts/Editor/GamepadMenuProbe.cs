using System.Collections.Generic;
using System.Linq;
using System.Text;
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
/// ETAPA 12 — os menus por CONTROLE, em play mode, com um gamepad VIRTUAL do Input System (e um teclado virtual
/// para a captura de tecla e a troca de dispositivo):
///
/// Unity.exe -batchmode -projectPath . -executeMethod GamepadMenuProbe.Run   (sem -quit; relatório em Logs/qa_gamepad.txt)
///
/// Não substitui o controle físico: o caminho do navegador (Gamepad API) e o mapeamento de cada modelo real ficam
/// de fora. O que mede: foco inicial de cada tela, D-pad (cima/baixo/esquerda/direita), A confirma, B volta/cancela,
/// Start pausa, retorno de foco das Configurações e do pause, captura de tecla, e os textos de dica (mapa e Troia)
/// por dispositivo e por família (genérico, Xbox, PlayStation). Não altera nada do jogo; ao sair, restaura as teclas.
/// </summary>
public static class GamepadMenuProbe
{
    private static readonly StringBuilder rel = new StringBuilder();
    private static Gamepad pad;
    private static Keyboard kb;
    private static readonly List<InputDevice> extras = new List<InputDevice>();
    private static int falhas;
    private static bool mudoOriginal;
    private static InputSettings.BackgroundBehavior fundo;
    private static InputSettings.EditorInputBehaviorInPlayMode editorIn;
    private static bool opcoesAtivas;
    private static EnterPlayModeOptions opcoes;
    private static IEnumerator<float> roteiro;
    private static double acordarEm;

    public static void Run()
    {
        opcoesAtivas = EditorSettings.enterPlayModeOptionsEnabled;
        opcoes = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        mudoOriginal = EditorUtility.audioMasterMute;
        EditorUtility.audioMasterMute = true;
        AudioListener.volume = 0f;
        rel.Clear();
        falhas = 0;
        L($"# QA gamepad (virtual) — {System.DateTime.Now:yyyy-MM-dd HH:mm}");
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
            if (!roteiro.MoveNext()) { Sair(falhas == 0 ? 0 : 1); return; }
            acordarEm = EditorApplication.timeSinceStartup + roteiro.Current;
        }
        catch (System.Exception e) { L($"EXCEÇÃO DO TESTE: {e.GetType().Name}: {e.Message}\n{e.StackTrace}"); falhas++; Sair(1); }
    }

    // ------------------------------------------------------------------ roteiro

    private static IEnumerator<float> Roteiro()
    {
        while (CampaignManager.Instance == null) { yield return 0.2f; }
        fundo = InputSystem.settings.backgroundBehavior;
        editorIn = InputSystem.settings.editorInputBehaviorInPlayMode;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        Application.runInBackground = true;
        kb = InputSystem.AddDevice<Keyboard>("QAGamepadKeyboard");
        pad = InputSystem.AddDevice<Gamepad>("QAGamepad");
        KeyRebindService.EnsureLoaded();
        KeyRebindService.ResetAll();

        // ---- Menu principal
        var e = Carregar(SceneLoader.MainMenu); while (e.MoveNext()) { yield return e.Current; }
        L("\n## Menu principal");
        Checar(EventSystem.current != null, $"EventSystem presente (ativos: {Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length})");
        string antes = Sel();
        Checar(antes != "(nada)", $"foco inicial: {antes}");
        e = Botao(GamepadButton.DpadDown); while (e.MoveNext()) { yield return e.Current; }
        string depois = Sel();
        Checar(depois != antes, $"D-pad baixo: {antes} -> {depois}");
        e = Botao(GamepadButton.DpadUp); while (e.MoveNext()) { yield return e.Current; }
        Checar(Sel() == antes, $"D-pad cima volta: {Sel()}");
        e = Botao(GamepadButton.DpadRight); while (e.MoveNext()) { yield return e.Current; }
        Checar(Sel() == depois, $"D-pad direita = próximo: {Sel()}");
        e = Botao(GamepadButton.DpadLeft); while (e.MoveNext()) { yield return e.Current; }
        Checar(Sel() == antes, $"D-pad esquerda = anterior: {Sel()}");
        L($"- dispositivo detectado: {InputDeviceTracker.Current} / {InputDeviceTracker.Family}");

        // ---- Configurações + Controles (por cima)
        L("\n## Configurações → Controles");
        SettingsScreen.Open();
        yield return 0.3f;
        string focoConfig = Sel();
        Checar(SettingsScreen.IsOpen && focoConfig != "(nada)", $"Configurações abertas, foco: {focoConfig}");
        OptionsMenu.Open();
        yield return 0.3f;
        Checar(OptionsMenu.IsOpen && Sel() == "CloseButton", $"Controles abertos, foco inicial: {Sel()} (esperado CloseButton)");

        e = Botao(GamepadButton.DpadUp); while (e.MoveNext()) { yield return e.Current; }
        Checar(Sel() == "ResetButton", $"D-pad cima: {Sel()} (esperado ResetButton)");
        e = Botao(GamepadButton.DpadUp); while (e.MoveNext()) { yield return e.Current; }
        string linha = Sel();
        Checar(linha == "KeyButton", $"D-pad cima de novo: {linha} (esperado a última linha de tecla)");

        // Captura: A abre, B cancela só a captura.
        e = Botao(GamepadButton.South); while (e.MoveNext()) { yield return e.Current; }
        string rotuloCaptura = RotuloSel();
        Checar(rotuloCaptura == "...", $"A numa linha abre a captura: rótulo \"{rotuloCaptura}\"");
        e = Botao(GamepadButton.East); while (e.MoveNext()) { yield return e.Current; }
        Checar(OptionsMenu.IsOpen && RotuloSel() != "...", $"B cancela só a captura: tela aberta={OptionsMenu.IsOpen}, rótulo \"{RotuloSel()}\"");

        // Captura: Esc do teclado cancela só a captura (não fecha a tela).
        e = Botao(GamepadButton.South); while (e.MoveNext()) { yield return e.Current; }
        e = Tecla(Key.Escape); while (e.MoveNext()) { yield return e.Current; }
        Checar(OptionsMenu.IsOpen && RotuloSel() != "...", $"Esc cancela só a captura: tela aberta={OptionsMenu.IsOpen}, rótulo \"{RotuloSel()}\"");

        // Captura de verdade: A, depois a tecla K.
        string rotuloAntes = RotuloSel();
        e = Botao(GamepadButton.South); while (e.MoveNext()) { yield return e.Current; }
        e = Tecla(Key.K); while (e.MoveNext()) { yield return e.Current; }
        yield return 0.3f;
        Checar(RotuloSel() == "K", $"captura grava a tecla: \"{rotuloAntes}\" -> \"{RotuloSel()}\"");
        KeyRebindService.ResetAll();

        // B fecha Controles e devolve o foco às Configurações, que continuam abertas.
        e = Botao(GamepadButton.East); while (e.MoveNext()) { yield return e.Current; }
        Checar(!OptionsMenu.IsOpen && SettingsScreen.IsOpen, $"B fecha Controles: Controles={OptionsMenu.IsOpen}, Configurações={SettingsScreen.IsOpen}");
        Checar(Sel() == focoConfig, $"foco volta para {Sel()} (era {focoConfig})");
        // Caminho real: foco em "Personalizar", A abre Controles, B fecha e o foco volta para "Personalizar".
        var personalizar = Object.FindObjectsByType<Button>(FindObjectsSortMode.None)
            .FirstOrDefault(b => b.GetComponentInChildren<Text>()?.text == Localization.Get("ui.settings.customize"));
        Checar(personalizar != null, $"botão Personalizar: {personalizar?.name}");
        if (personalizar != null)
        {
            // Chega no Personalizar pelo D-pad, como o jogador (do Fechar para cima).
            for (int i = 0; i < 12 && Sel() != personalizar.name; i++)
            {
                e = Botao(GamepadButton.DpadUp); while (e.MoveNext()) { yield return e.Current; }
                L($"  · cima → {Sel()}");
            }
            e = Botao(GamepadButton.South); while (e.MoveNext()) { yield return e.Current; }
            Checar(OptionsMenu.IsOpen && Sel() == "CloseButton", $"A em Personalizar abre Controles: {OptionsMenu.IsOpen}, foco {Sel()}");
            e = Botao(GamepadButton.East); while (e.MoveNext()) { yield return e.Current; }
            var g = EventSystem.current.currentSelectedGameObject;
            Checar(!OptionsMenu.IsOpen && g == personalizar.gameObject, $"B volta o foco para Personalizar: foco {Sel()} ({g?.GetComponentInChildren<Text>()?.text}) · EventSystem.current={EventSystem.current.name}");
        }
        e = Botao(GamepadButton.East); while (e.MoveNext()) { yield return e.Current; }
        yield return 0.2f;
        Checar(!SettingsScreen.IsOpen && Sel() != "(nada)", $"B fecha Configurações: aberta={SettingsScreen.IsOpen}, foco {Sel()}");

        // Tradução da tela de Controles (etapa 13), aberta sozinha: trocar o idioma com as Configurações por baixo as
        // reconstrói (o jogo não permite isso, a troca é feita dentro delas).
        OptionsMenu.Open();
        yield return 0.3f;
        // Etapa 13A: rótulos, teclas, mensagens e dicas na língua do jogo, nos dois idiomas — e nada da outra língua.
        Language idiomaOriginal = Localization.Current;
        var idiomas = new[]
        {
            (Language.English,
                new[] { "CONTROLS", "Move left", "Move right", "SPACE", "LEFT ARROW", "RIGHT ARROW", "LEFT SHIFT", "RIGHT SHIFT", "Restore defaults", "Close", "Select a key" },
                new[] { "Mover", "ESPAÇO", "SETA", "ESQUERD", "Fechar", "Restaurar", "Selecione" },
                "key already used by \"Attack\"", "Not changed — cancelled.", "Press SPACE to jump."),
            (Language.Portuguese,
                new[] { "CONTROLES", "Mover para a esquerda", "Mover para a direita", "ESPAÇO", "SETA ESQUERDA", "SETA DIREITA", "SHIFT ESQUERDO", "SHIFT DIREITO", "Restaurar padrões", "Fechar", "Selecione uma tecla" },
                new[] { "Move ", "SPACE", "ARROW", "LEFT", "Close", "Restore", "Select" },
                "tecla já usada por \"Atacar\"", "Não alterado — cancelado.", "Pressione ESPAÇO para pular."),
        };

        foreach (var (idioma, esperado, proibido, conflito, cancelado, tutorial) in idiomas)
        {
            // Troca com a tela aberta: título e botões (LocalizedText) mudam na hora, sem reiniciar nada.
            Localization.Current = idioma;
            yield return 0.2f;
            string vivo = string.Join(" | ", OptionsMenu.Instance.GetComponentsInChildren<Text>(false)
                .Where(t => t.name == "Title" || t.transform.parent.name.EndsWith("Button") && t.transform.parent.name != "KeyButton")
                .Select(t => t.text));
            Checar(esperado.Count(x => vivo.Contains(x)) == 3, $"{idioma}, troca com a tela aberta: {vivo}");

            // Fecha e reabre (o caminho do jogo: troca nas Configurações, depois Personalizar): linhas na nova língua.
            e = Botao(GamepadButton.East); while (e.MoveNext()) { yield return e.Current; }
            OptionsMenu.Open();
            yield return 0.3f;
            string tela = string.Join(" | ", OptionsMenu.Instance.GetComponentsInChildren<Text>(false).Select(t => t.text));
            L($"- {idioma}: {tela}");
            var faltam = esperado.Where(x => !tela.Contains(x)).ToList();
            var sobram = proibido.Where(x => tela.Contains(x)).ToList();
            Checar(!tela.Contains("(Negative)") && !tela.Contains("(Positive)") && faltam.Count == 0 && sobram.Count == 0,
                $"{idioma}: tela traduzida (faltam: {string.Join(", ", faltam)}; da outra língua: {string.Join(", ", sobram)})");

            // Nomes de qualidade gráfica (vêm do Quality Settings do Unity, em inglês) na língua do jogo.
            string qualidades = string.Join(", ", SettingsManager.QualityDisplayNames);
            bool qualidadeOk = idioma == Language.English
                ? qualidades.Contains("High") && !qualidades.Contains("Alta")
                : qualidades.Contains("Alta") && qualidades.Contains("Média") && !qualidades.Contains("High") && !qualidades.Contains("Medium");
            Checar(qualidadeOk && SettingsManager.QualityDisplayNames.Length == SettingsManager.QualityNames.Length,
                $"{idioma}, qualidade: {qualidades}");

            // Rótulos dos botões de baixo cabem com folga (a DejaVu é mais larga que a fonte antiga).
            foreach (Button b in OptionsMenu.Instance.GetComponentsInChildren<Button>(false).Where(b => b.name == "ResetButton" || b.name == "CloseButton"))
            {
                Text r = b.GetComponentInChildren<Text>();
                float largura = ((RectTransform)b.transform).rect.width;
                Checar(r.preferredWidth + 24f <= largura, $"{idioma}, botão \"{r.text}\": texto {r.preferredWidth:0} + folga 24 cabe em {largura:0}");
            }

            // Mensagens da captura: tecla em conflito (Z do Atacar) e cancelamento por Esc. A linha do Interagir fica com E.
            var interagir = OptionsMenu.Instance.GetComponentsInChildren<Button>(false).Last(b => b.name == "KeyButton");
            Text mensagem = OptionsMenu.Instance.GetComponentsInChildren<Text>(false).First(t => t.name == "Message");
            EventSystem.current.SetSelectedGameObject(interagir.gameObject);
            yield return 0.1f;
            e = Botao(GamepadButton.South); while (e.MoveNext()) { yield return e.Current; }
            e = Tecla(Key.Z); while (e.MoveNext()) { yield return e.Current; }
            yield return 0.2f;
            Checar(mensagem.text.Contains(conflito) && interagir.GetComponentInChildren<Text>().text == "E",
                $"{idioma}, conflito: \"{mensagem.text}\" (tecla ficou {interagir.GetComponentInChildren<Text>().text})");
            EventSystem.current.SetSelectedGameObject(interagir.gameObject);
            e = Botao(GamepadButton.South); while (e.MoveNext()) { yield return e.Current; }
            e = Tecla(Key.Escape); while (e.MoveNext()) { yield return e.Current; }
            yield return 0.2f;
            Checar(OptionsMenu.IsOpen && mensagem.text == cancelado, $"{idioma}, cancelamento: \"{mensagem.text}\" (tela aberta: {OptionsMenu.IsOpen})");

            // Dicas com os mesmos nomes de tecla (o teclado é o dispositivo agora, pelo Esc acima).
            string teclaPausa = ControlHints.Button("Pause");
            string pulo = ControlHints.Instruction("tut.Level_02_Troia.Tutorial_Jump", "Jump");
            Checar(InputDeviceTracker.Current == InputDeviceKind.Keyboard && teclaPausa == "ESC" && pulo == tutorial,
                $"{idioma}, dicas no teclado: pausa \"{teclaPausa}\", tutorial \"{pulo}\"");
        }

        KeyRebindService.ResetAll();
        Localization.Current = idiomaOriginal;
        e = Botao(GamepadButton.East); while (e.MoveNext()) { yield return e.Current; }
        Checar(!OptionsMenu.IsOpen, "B fecha Controles aberta sozinha");

        // ---- Mapa-múndi: dica por dispositivo
        e = Carregar(SceneLoader.WorldMap); while (e.MoveNext()) { yield return e.Current; }
        L("\n## Mapa-múndi");
        e = Botao(GamepadButton.DpadRight, 0.05f); while (e.MoveNext()) { yield return e.Current; }
        yield return 0.3f;
        string dicaPad = TextoDe("NodeHint");
        e = Tecla(Key.LeftShift); while (e.MoveNext()) { yield return e.Current; }
        string dicaTeclado = TextoDe("NodeHint");
        L($"- controle: \"{dicaPad}\" · teclado: \"{dicaTeclado}\"");
        Checar(dicaPad != null && !dicaPad.Contains("[E]") && dicaPad.Contains("Y/△"), "controle genérico mostra Y/△, não [E]");
        Checar(dicaTeclado != null && dicaTeclado.Contains("[E]"), "teclado continua mostrando [E]");

        // ---- Troia: dicas, pause e fim de jogo
        e = Carregar("Level_02_Troia"); while (e.MoveNext()) { yield return e.Current; }
        L("\n## Troia — dicas do tutorial");
        e = Botao(GamepadButton.North); while (e.MoveNext()) { yield return e.Current; }
        DicasTroia("controle genérico", "A/✕", "X/□");
        var ps = InputSystem.AddDevice<UnityEngine.InputSystem.DualShock.DualShock4GamepadHID>("QADualShock");
        extras.Add(ps);
        e = BotaoEm(ps, ps.buttonNorth); while (e.MoveNext()) { yield return e.Current; }
        DicasTroia("PlayStation", "✕", "□");
        Gamepad xpad = null;
        try { xpad = InputSystem.AddDevice("XInputControllerWindows", "QAXbox") as Gamepad; } catch (System.Exception ex) { L("- Xbox: " + ex.Message); }
        if (xpad != null)
        {
            extras.Add(xpad);
            e = BotaoEm(xpad, xpad.buttonNorth); while (e.MoveNext()) { yield return e.Current; }
            DicasTroia("Xbox", "A", "X");
        }
        else
        {
            L("- Xbox: layout XInputControllerWindows indisponível neste Editor — pulado");
        }
        e = Tecla(Key.LeftShift); while (e.MoveNext()) { yield return e.Current; }
        DicasTroia("teclado", "SPACE", "Z");

        // De volta ao gamepad virtual genérico.
        e = Botao(GamepadButton.North); while (e.MoveNext()) { yield return e.Current; }

        L("\n## Pause");
        var pausa = Object.FindAnyObjectByType<PauseMenu>(FindObjectsInactive.Include);
        var mapa = KeyRebindService.Asset?.FindActionMap("Player");
        var acaoPausa = mapa?.FindAction("Pause");
        L($"- antes: mapa Player={mapa?.enabled}, ação Pause={acaoPausa?.enabled}, Gamepad.current={Gamepad.current?.name}, gamepads={string.Join(",", Gamepad.all.Select(g => g.name))}, controles da ação={string.Join(",", acaoPausa?.controls.Select(c => c.path) ?? new string[0])}");
        int disparos = 0; System.Action<InputAction.CallbackContext> conta = _ => disparos++;
        if (acaoPausa != null) { acaoPausa.performed += conta; }
        e = Tecla(Key.Escape); while (e.MoveNext()) { yield return e.Current; }
        L($"- Esc (teclado): pausado={pausa?.IsPaused}, disparos={disparos}");
        if (pausa != null && pausa.IsPaused) { e = Tecla(Key.Escape); while (e.MoveNext()) { yield return e.Current; } }
        e = Botao(GamepadButton.Start); while (e.MoveNext()) { yield return e.Current; }
        Checar(pausa != null && pausa.IsPaused, $"Start pausa: {pausa?.IsPaused} (timeScale {Time.timeScale})");
        var retomar = new SerializedObject(pausa).FindProperty("resumeButton").objectReferenceValue as Button;
        Checar(retomar != null && Sel() == retomar.name, $"foco inicial: {Sel()} (esperado {retomar?.name})");
        var painelPausa = new SerializedObject(pausa).FindProperty("panel").objectReferenceValue as GameObject;
        var botoesPausa = painelPausa != null ? painelPausa.GetComponentsInChildren<Button>(true) : new Button[0];
        var controles = botoesPausa.FirstOrDefault(b => b.name == "ControlsButton");
        string rotuloControles = controles?.GetComponentInChildren<Text>()?.text;
        Checar(rotuloControles == Localization.Get("ui.pause.controls"), $"botão Controles do pause rotulado \"{rotuloControles}\" (não \"{Localization.Get("ui.pause.resume")}\")");
        Checar(botoesPausa.Length >= 4 && botoesPausa.All(b => b.colors.selectedColor == b.colors.highlightedColor),
            $"cor de selecionado = cor de destaque nos {botoesPausa.Length} botões do pause (antes: quase branca, texto ilegível)");
        e = Botao(GamepadButton.DpadDown); while (e.MoveNext()) { yield return e.Current; }
        e = Botao(GamepadButton.DpadDown); while (e.MoveNext()) { yield return e.Current; }
        Checar(Sel() == "ControlsButton", $"D-pad baixo ×2: {Sel()} (esperado ControlsButton)");
        e = Botao(GamepadButton.South); while (e.MoveNext()) { yield return e.Current; }
        Checar(OptionsMenu.IsOpen && Sel() == "CloseButton", $"A abre Controles: {OptionsMenu.IsOpen}, foco {Sel()}");
        e = Botao(GamepadButton.East); while (e.MoveNext()) { yield return e.Current; }
        Checar(!OptionsMenu.IsOpen && pausa.IsPaused && Time.timeScale == 0f, $"B fecha só Controles: Controles={OptionsMenu.IsOpen}, pausado={pausa.IsPaused}, timeScale {Time.timeScale}");
        Checar(Sel() == "ControlsButton", $"foco volta para {Sel()}");
        e = Botao(GamepadButton.East); while (e.MoveNext()) { yield return e.Current; }
        Checar(!pausa.IsPaused && Time.timeScale == 1f, $"B retoma: pausado={pausa.IsPaused}, timeScale {Time.timeScale}");
        e = Botao(GamepadButton.Start); while (e.MoveNext()) { yield return e.Current; }
        e = Botao(GamepadButton.Start); while (e.MoveNext()) { yield return e.Current; }
        Checar(!pausa.IsPaused, $"Start pausa e Start retoma: pausado={pausa.IsPaused}");

        L("\n## Fim de jogo");
        var p = GameObject.FindGameObjectWithTag("Player");
        var vida = p.GetComponent<HealthSystem>();
        float t0 = Time.realtimeSinceStartup;
        while (LivesCounter.Current > 0 && Time.realtimeSinceStartup - t0 < 30f) { vida.TakeDamage(9999); yield return 0.6f; }
        yield return 0.5f;
        Checar(Sel() == "RetryButton", $"foco inicial: {Sel()} (esperado RetryButton)");
        e = Botao(GamepadButton.DpadRight); while (e.MoveNext()) { yield return e.Current; }
        Checar(Sel() == "MenuButton", $"D-pad direita: {Sel()}");
        e = Botao(GamepadButton.DpadLeft); while (e.MoveNext()) { yield return e.Current; }
        Checar(Sel() == "RetryButton", $"D-pad esquerda: {Sel()}");
        e = Botao(GamepadButton.South); while (e.MoveNext()) { yield return e.Current; }
        float t1 = Time.realtimeSinceStartup;
        GameObject novo = null;
        while (Time.realtimeSinceStartup - t1 < 15f)
        {
            novo = GameObject.FindGameObjectWithTag("Player");
            if (novo != null && novo != p) { break; }
            yield return 0.2f;
        }
        Checar(novo != null && novo != p && LivesCounter.Current > 0, $"A em \"Tentar de novo\" recomeça a fase: {SceneManager.GetActiveScene().name}, vidas {LivesCounter.Current}, timeScale {Time.timeScale}");

        // ---- Fase concluída
        e = Carregar(SceneLoader.LevelComplete); while (e.MoveNext()) { yield return e.Current; }
        L("\n## Fase concluída");
        yield return 0.3f;
        L($"- EventSystems ativos: {EventSystemsAtivos()} (o persistente das fases + o da cena — anterior à Etapa 12)");
        string focoLc = Sel();
        Checar(focoLc != "(nada)", $"foco inicial: {focoLc}");
        e = Botao(GamepadButton.South); while (e.MoveNext()) { yield return e.Current; }
        float t2 = Time.realtimeSinceStartup;
        while (SceneManager.GetActiveScene().name == SceneLoader.LevelComplete && Time.realtimeSinceStartup - t2 < 15f) { yield return 0.2f; }
        Checar(SceneManager.GetActiveScene().name != SceneLoader.LevelComplete, $"A confirma: foi para {SceneManager.GetActiveScene().name}");

        // ---- Final
        e = Carregar(SceneLoader.Ending); while (e.MoveNext()) { yield return e.Current; }
        L("\n## Final");
        yield return 0.3f;
        L($"- EventSystems ativos: {EventSystemsAtivos()} (o persistente das fases + o da cena — anterior à Etapa 12)");
        string focoEnd = Sel();
        Checar(focoEnd == "PlayAgainButton", $"foco inicial: {focoEnd}");
        e = Botao(GamepadButton.DpadDown); while (e.MoveNext()) { yield return e.Current; }
        Checar(Sel() == "BackToMenuButton", $"D-pad baixo: {Sel()}");
        e = Botao(GamepadButton.South); while (e.MoveNext()) { yield return e.Current; }
        float t3 = Time.realtimeSinceStartup;
        while (SceneManager.GetActiveScene().name != SceneLoader.MainMenu && Time.realtimeSinceStartup - t3 < 15f) { yield return 0.2f; }
        Checar(SceneManager.GetActiveScene().name == SceneLoader.MainMenu, $"A em \"Voltar ao menu\": {SceneManager.GetActiveScene().name}");

        L($"\n## RESULTADO: {(falhas == 0 ? "OK" : falhas + " falha(s)")}");
    }

    // ------------------------------------------------------------------ ajudantes

    private static void DicasTroia(string como, string pulo, string ataque)
    {
        var dicas = Object.FindObjectsByType<Odisseia.Levels.TutorialTrigger>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        var prop = typeof(Odisseia.Levels.TutorialTrigger).GetProperty("ResolvedMessage",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        var textos = dicas.Select(d => (d.name, texto: (string)prop.GetValue(d))).OrderBy(x => x.name).ToList();
        L($"- {como} ({InputDeviceTracker.Current}/{InputDeviceTracker.Family}): " + string.Join(" · ", textos.Select(x => $"{x.name}=\"{x.texto}\"")));
        string jump = textos.FirstOrDefault(x => x.name.EndsWith("Jump")).texto ?? string.Empty;
        string atk = textos.FirstOrDefault(x => x.name.EndsWith("Attack")).texto ?? string.Empty;
        string move = textos.FirstOrDefault(x => x.name.EndsWith("Move")).texto ?? string.Empty;
        bool ok = jump.Contains(pulo) && atk.Contains(ataque);
        if (como != "teclado")
        {
            ok &= !jump.Contains("SPACE") && !atk.Contains(" Z ") && !move.Contains("A/D");
        }
        Checar(ok, $"{como}: pulo cita \"{pulo}\", ataque cita \"{ataque}\"");
    }

    private static IEnumerator<float> Carregar(string cena)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(cena);
        float t = Time.realtimeSinceStartup;
        while (SceneManager.GetActiveScene().name != cena && Time.realtimeSinceStartup - t < 30f) { yield return 0.1f; }
        yield return 1.5f;
        Focar();
    }

    /// <summary>O EventSystem ignora navegação sem foco de janela no Editor em batchmode; isto o avisa que tem foco.</summary>
    private static void Focar()
    {
        foreach (var es in Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None))
        {
            es.SendMessage("OnApplicationFocus", true, SendMessageOptions.DontRequireReceiver);
        }
    }

    private static int EventSystemsAtivos() =>
        Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Count(es => es.isActiveAndEnabled);

    private static IEnumerator<float> Botao(GamepadButton b, float segurar = 0.12f)
    {
        Focar();
        InputSystem.QueueStateEvent(pad, new GamepadState().WithButton(b));
        yield return segurar;
        InputSystem.QueueStateEvent(pad, new GamepadState());
        yield return 0.25f;
    }

    /// <summary>Aperta um botão de qualquer layout (DualShock/XInput têm formato de estado próprio).</summary>
    private static IEnumerator<float> BotaoEm(InputDevice dev, UnityEngine.InputSystem.Controls.ButtonControl b)
    {
        using (StateEvent.From(dev, out InputEventPtr ev)) { b.WriteValueIntoEvent(1f, ev); InputSystem.QueueEvent(ev); }
        yield return 0.12f;
        using (StateEvent.From(dev, out InputEventPtr ev)) { b.WriteValueIntoEvent(0f, ev); InputSystem.QueueEvent(ev); }
        yield return 0.25f;
    }

    private static IEnumerator<float> Tecla(Key k)
    {
        InputSystem.QueueStateEvent(kb, new KeyboardState(k));
        yield return 0.12f;
        InputSystem.QueueStateEvent(kb, new KeyboardState());
        yield return 0.25f;
    }

    private static string Sel()
    {
        GameObject g = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
        return g != null ? g.name : "(nada)";
    }

    private static string RotuloSel()
    {
        GameObject g = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
        return g != null ? g.GetComponentInChildren<Text>()?.text : null;
    }

    private static string TextoDe(string nome)
    {
        var t = Object.FindObjectsByType<Text>(FindObjectsSortMode.None).FirstOrDefault(x => x.name == nome);
        return t != null ? t.text : null;
    }

    private static void Checar(bool ok, string s)
    {
        if (!ok) { falhas++; }
        L($"- {(ok ? "OK" : "**FALHA**")} {s}");
    }

    private static void L(string s) { rel.AppendLine(s); Debug.Log("[QAGamepad] " + s); }

    private static void Sair(int c)
    {
        EditorApplication.update -= Tick;
        try { KeyRebindService.ResetAll(); } catch { }
        System.IO.File.WriteAllText("Logs/qa_gamepad.txt", rel.ToString());
        EditorSettings.enterPlayModeOptionsEnabled = opcoesAtivas;
        EditorSettings.enterPlayModeOptions = opcoes;
        foreach (var d in extras) { if (d != null && d.added) { InputSystem.RemoveDevice(d); } }
        if (pad != null && pad.added) { InputSystem.RemoveDevice(pad); }
        if (kb != null && kb.added) { InputSystem.RemoveDevice(kb); }
        InputSystem.settings.backgroundBehavior = fundo;
        InputSystem.settings.editorInputBehaviorInPlayMode = editorIn;
        Time.timeScale = 1f;
        AudioListener.volume = 1f;
        EditorUtility.audioMasterMute = mudoOriginal;
        EditorApplication.ExitPlaymode();
        EditorApplication.Exit(c);
    }
}
