using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Odisseia.Core;
using Odisseia.Systems;
using Odisseia.UI;

/// <summary>
/// Auditoria final (Etapa 13B.8) em play mode, NO MUDO — o que os outros probes não cobrem:
///
/// Unity.exe -batchmode -projectPath . -executeMethod ReleaseAuditProbe.Run
/// (sem -nographics e sem -quit; sai sozinho, código 0 = OK; relatório em Logs/qa_audit.txt)
///
/// 1. As 16 fases, em inglês e em português: carrega, jogador, câmera segue, HUD, pausa/retoma, checkpoint, morte →
///    respawn, uma música só (a do tema), varredura de textos (idioma errado, "(Negative)", nomes internos do Input
///    System, chave crua), sprites fora de escala e erros no console.
/// 2. Mapa com 0, 8 e 15 fases concluídas: 16 nós e quantos ficam concluídos / atual / bloqueados.
/// 3. Telas em 1280×720, 1920×1080, 960×540, 2340×1080 e 1024×768 (CanvasScaler recalculado antes de cada foto), em
///    inglês e português: Mapa, Pausa, Configurações, Controles, Fim de jogo, Fase concluída — nenhum texto fora da tela
///    nem por cima de outro texto.
/// 4. Áudio: o volume de música chega à fonte; trocar de cena não duplica a música.
/// Guarda e restaura o save, o idioma, o volume, o som e as opções de play mode do Editor.
/// </summary>
public static class ReleaseAuditProbe
{
    private const string ChaveDoSave = "Odisseia.Save";
    private static readonly string[] Fases =
    {
        "Level_01_Itaca_Prologue", "Level_02_Troia", "Level_03_Cicones", "Level_04_Citera", "Level_05_Ciclopes",
        "Level_06_Eolo", "Level_07_Lestrigoes", "Level_08_Circe", "Level_09_MundoDosMortos", "Level_10_Sereias",
        "Level_11_CilaCaribdis", "Level_12_GadoDoSol", "Level_13_Calipso", "Level_14_Itaca_Return",
        "Level_15_Pretendentes", "Level_16_Final",
    };

    private static readonly System.Text.StringBuilder rel = new System.Text.StringBuilder();
    private static int falhas, erros;
    private static readonly List<string> errosVistos = new List<string>();
    private static bool opcoesAtivas, mudoOriginal;
    private static EnterPlayModeOptions opcoes;
    private static Language idiomaOriginal;
    private static float musicaOriginal;
    private static string saveAnterior;
    private static IEnumerator<float> roteiro;
    private static double acordarEm;
    private static HashSet<string> soIngles, soPortugues;

    public static void Run()
    {
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

        // Textos que só existem num idioma (iguais nos dois — "DEF", "ENTER" — não contam).
        soIngles = new HashSet<string>(LocalizationTable.Entries.Values.Where(v => v.Length > 1 && v[0] != v[1] && v[0].Length > 3 && !v[0].Contains("{")).Select(v => v[0]));
        soPortugues = new HashSet<string>(LocalizationTable.Entries.Values.Where(v => v.Length > 1 && v[0] != v[1] && v[1].Length > 3 && !v[1].Contains("{")).Select(v => v[1]));
        soIngles.ExceptWith(soPortugues);
        soPortugues.ExceptWith(soIngles);

        rel.Clear();
        falhas = 0; erros = 0; errosVistos.Clear();
        L($"# Auditoria final (13B.8) — {System.DateTime.Now:yyyy-MM-dd HH:mm}");
        Application.logMessageReceived -= AoLogar;
        Application.logMessageReceived += AoLogar;
        EditorSceneManager.OpenScene("Assets/Scenes/Boot/Boot.unity", OpenSceneMode.Single);
        roteiro = Roteiro();
        acordarEm = 0;
        EditorApplication.update += Tick;
        EditorApplication.EnterPlaymode();
    }

    private static void AoLogar(string msg, string pilha, LogType tipo)
    {
        if (tipo != LogType.Error && tipo != LogType.Exception && tipo != LogType.Assert) { return; }
        if (msg.StartsWith("[Audit]")) { return; }
        erros++;
        if (errosVistos.Count < 40) { errosVistos.Add($"{SceneManager.GetActiveScene().name}: {msg.Split('\n')[0]}"); }
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

    /// <summary>-auditParte N roda só uma parte: 1 fases, 2 mapa, 3 telas, 4 áudio (0 ou ausente = todas).</summary>
    private static int SoParte()
    {
        string[] a = System.Environment.GetCommandLineArgs();
        int i = System.Array.IndexOf(a, "-auditParte");
        return i >= 0 && i < a.Length - 1 && int.TryParse(a[i + 1], out int n) ? n : 0;
    }

    private static IEnumerator<float> Roteiro()
    {
        int parte = SoParte();
        while (CampaignManager.Instance == null) { yield return 0.2f; }
        AudioListener.volume = 0f;
        Application.runInBackground = true;
        musicaOriginal = SettingsManager.MusicVolume;

        // ---- 1) As 16 fases
        L("\n## 1. Fases (EN e PT)");
        foreach (string fase in (parte == 0 || parte == 1) ? Fases : new string[0])
        {
            foreach (Language idioma in new[] { Language.English, Language.Portuguese })
            {
                Localization.Current = idioma;
                LivesCounter.BeginRun();
                int errosAntes = erros;
                var e = Carregar(fase); while (e.MoveNext()) { yield return e.Current; }
                foreach (var d in Object.FindObjectsByType<DialogueSequence>(FindObjectsSortMode.None)) { d.Skip(); }
                yield return 0.6f;
                var problemas = new List<string>();
                var notas = new List<string>();

                var jogador = GameObject.FindGameObjectWithTag("Player");
                var cam = Camera.main;
                if (jogador == null) { problemas.Add("sem jogador"); }
                if (cam == null) { problemas.Add("sem câmera"); }
                var hud = Object.FindAnyObjectByType<HUD>();
                if (hud == null || !hud.GetComponentsInChildren<Text>().Any(t => t.text.Contains("/"))) { problemas.Add("HUD ausente ou vazio"); }

                if (jogador != null && cam != null && idioma == Language.English)
                {
                    // Câmera segue: o jogador vai 4 un. para a direita.
                    float cx = cam.transform.position.x;
                    var rb = jogador.GetComponent<Rigidbody2D>();
                    Vector3 volta = jogador.transform.position;
                    rb.position = (Vector2)volta + Vector2.right * 4f; jogador.transform.position = volta + Vector3.right * 4f;
                    yield return 1.2f;
                    if (Mathf.Abs(cam.transform.position.x - cx) < 0.5f) { notas.Add($"câmera não acompanhou ({cx:0.0} → {cam.transform.position.x:0.0}; pode ser limite da fase)"); }
                    rb.position = volta; jogador.transform.position = volta;

                    // Pausa e retomada.
                    var pausa = Object.FindAnyObjectByType<PauseMenu>(FindObjectsInactive.Include);
                    if (pausa == null) { problemas.Add("sem PauseMenu"); }
                    else
                    {
                        pausa.Pause();
                        bool pausou = Time.timeScale == 0f && pausa.IsPaused;
                        pausa.Resume();
                        if (!pausou || Time.timeScale != 1f) { problemas.Add("pausa/retomada não funcionou"); }
                    }

                    // Checkpoint, morte e respawn.
                    var altar = Object.FindObjectsByType<Checkpoint>(FindObjectsSortMode.None).OrderBy(c => c.transform.position.x).FirstOrDefault();
                    if (altar == null) { problemas.Add("sem checkpoint"); }
                    else
                    {
                        var col = altar.GetComponent<Collider2D>();
                        rb.position = col.bounds.center; jogador.transform.position = col.bounds.center;
                        yield return 0.4f;
                        if (!CheckpointManager.HasCheckpoint || Vector2.Distance(CheckpointManager.LastCheckpointPosition, altar.transform.position) > 0.01f) { problemas.Add("checkpoint não registrou"); }
                        var vida = jogador.GetComponent<HealthSystem>();
                        int vidas = LivesCounter.Current;
                        rb.position = (Vector2)altar.transform.position + Vector2.right * 3f;
                        jogador.transform.position = altar.transform.position + Vector3.right * 3f;
                        vida.TakeDamage(9999);
                        yield return 2.5f;
                        jogador = GameObject.FindGameObjectWithTag("Player");
                        vida = jogador.GetComponent<HealthSystem>();
                        float dist = Vector2.Distance(jogador.transform.position, altar.transform.position);
                        if (LivesCounter.Current != vidas - 1 || dist > 1.5f || vida.CurrentHealth != vida.MaxHealth)
                        {
                            problemas.Add($"respawn: vidas {vidas}→{LivesCounter.Current}, {dist:0.0} un. do altar, vida {vida.CurrentHealth}/{vida.MaxHealth}");
                        }
                    }

                    // Música: uma fonte em laço tocando, o tema.
                    var tema = GameAssets.Instance != null ? GameAssets.Instance.Audio?.MainTheme : null;
                    var tocando = Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None).Where(a => a.loop && a.isPlaying && a.clip != null && a.clip.length > 30f).ToList();
                    if (tocando.Count != 1 || tocando[0].clip != tema) { problemas.Add($"música: {tocando.Count} fonte(s) longas em laço ({string.Join(", ", tocando.Select(a => a.clip.name))})"); }

                    // Fora de escala: sprite de jogo (ordem −5..5) com mais de 20 un. de altura ou largura.
                    var gigantes = Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None)
                        .Where(r => r.enabled && r.sprite != null && r.sortingOrder >= -5 && r.sortingOrder <= 5 && (r.bounds.size.y > 20f || r.bounds.size.x > 20f) && !r.name.StartsWith("Floor") && !r.name.Contains("Terrain"))
                        .Select(r => $"{r.name} {r.bounds.size.x:0}×{r.bounds.size.y:0}").ToList();
                    if (gigantes.Count > 0) { notas.Add("grandes na camada de jogo: " + string.Join(", ", gigantes)); }
                }

                // Textos: idioma, nomes internos, chaves cruas — HUD, avisos e as falas da fase.
                var vistos = Object.FindObjectsByType<Text>(FindObjectsSortMode.None).Where(t => t.isActiveAndEnabled && !string.IsNullOrWhiteSpace(t.text)).Select(t => (t.name, t.text)).ToList();
                foreach (var d in Object.FindObjectsByType<DialogueSequence>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                {
                    var linhas = new SerializedObject(d).FindProperty("lines");
                    for (int i = 0; linhas != null && i < linhas.arraySize; i++)
                    {
                        string chave = linhas.GetArrayElementAtIndex(i).FindPropertyRelative("key").stringValue;
                        if (!string.IsNullOrEmpty(chave)) { vistos.Add(($"fala {chave}", Localization.Has(chave) ? Localization.Get(chave) : "<<" + chave + ">>")); }
                    }
                }
                foreach (var (nome, texto) in vistos)
                {
                    if (texto.Contains("(Negative)") || texto.Contains("(Positive)") || texto.Contains("<Keyboard>") || texto.Contains("<Gamepad>") || texto.Contains("buttonSouth")) { problemas.Add($"nome interno em {nome}: \"{texto}\""); }
                    if (texto.StartsWith("<<") || System.Text.RegularExpressions.Regex.IsMatch(texto, @"^(ui|dlg|tut|ctrl|speaker)\.[A-Za-z0-9_.]+$")) { problemas.Add($"chave crua em {nome}: \"{texto}\""); }
                    var outro = idioma == Language.English ? soPortugues : soIngles;
                    if (outro.Contains(texto.Trim())) { problemas.Add($"texto do outro idioma em {nome}: \"{texto}\""); }
                }

                int novosErros = erros - errosAntes;
                if (novosErros > 0) { problemas.Add($"{novosErros} erro(s) no console"); }
                Checar(problemas.Count == 0, $"{fase} [{idioma}]: {vistos.Count} textos/falas conferidos" + (problemas.Count > 0 ? " — " + string.Join("; ", problemas) : ""));
                foreach (string n in notas) { L($"  - nota: {n}"); }
            }
        }

        // ---- 2) Mapa
        L("\n## 2. Mapa");
        Localization.Current = Language.English;
        foreach (int concluidas in (parte == 0 || parte == 2) ? new[] { 0, 8, 15 } : new int[0])
        {
            CampaignManager.Instance.StartNewGame();
            foreach (var nivel in CampaignManager.Instance.Levels.Take(concluidas)) { CampaignManager.Instance.CompleteLevel(nivel.LevelId, 0, 0); }
            var e = Carregar(SceneLoader.WorldMap); while (e.MoveNext()) { yield return e.Current; }
            yield return 1.5f;
            var nos = Object.FindObjectsByType<Odisseia.WorldMap.LevelNode>(FindObjectsSortMode.None).OrderBy(n => n.Order).ToList();
            var porEstado = nos.GroupBy(n => n.State).ToDictionary(g => g.Key, g => g.Count());
            int c = porEstado.TryGetValue(Odisseia.WorldMap.LevelNodeState.Completed, out int x1) ? x1 : 0;
            int bloqueados = porEstado.TryGetValue(Odisseia.WorldMap.LevelNodeState.Locked, out int x2) ? x2 : 0;
            bool ordem = nos.Select(n => n.Order).SequenceEqual(Enumerable.Range(nos.First().Order, nos.Count));
            Checar(nos.Count == 16 && c == concluidas && bloqueados == 16 - concluidas - 1 && ordem,
                $"{concluidas} concluídas: {nos.Count} nós em ordem={ordem}; " + string.Join(", ", porEstado.Select(p => $"{p.Key} {p.Value}")));
        }

        // ---- 3) Telas em várias resoluções
        L("\n## 3. Telas × resoluções");
        foreach (Language idioma in (parte == 0 || parte == 3) ? new[] { Language.English, Language.Portuguese } : new Language[0])
        {
            Localization.Current = idioma;
            CampaignManager.Instance.StartNewGame();
            var e = Carregar(SceneLoader.WorldMap); while (e.MoveNext()) { yield return e.Current; }
            Telas($"Mapa [{idioma}]", null);

            e = Carregar("Level_02_Troia"); while (e.MoveNext()) { yield return e.Current; }
            foreach (var d in Object.FindObjectsByType<DialogueSequence>(FindObjectsSortMode.None)) { d.Skip(); }
            yield return 0.6f;
            var pausa = Object.FindAnyObjectByType<PauseMenu>(FindObjectsInactive.Include);
            pausa.Pause(); yield return 0.3f;
            Telas($"Pausa [{idioma}]", "HUD"); // o painel da pausa fica no "HUD Canvas" da fase
            SettingsScreen.Open(); yield return 0.3f;
            Telas($"Configurações [{idioma}]", "Settings");
            OptionsMenu.Open(); yield return 0.3f;
            Telas($"Controles [{idioma}]", "Options");
            // Configurações e Controles persistem entre cenas: fechar antes de seguir (senão entram nas outras fotos).
            // Pelo Close das próprias telas (privado), como o botão Fechar faria.
            foreach (var tela in Object.FindObjectsByType<OptionsMenu>(FindObjectsSortMode.None)) { typeof(OptionsMenu).GetMethod("Close", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.Invoke(tela, null); }
            yield return 0.2f;
            foreach (var tela in Object.FindObjectsByType<SettingsScreen>(FindObjectsSortMode.None)) { typeof(SettingsScreen).GetMethod("Close", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)?.Invoke(tela, null); }
            yield return 0.3f;
            if (SettingsScreen.IsOpen || OptionsMenu.IsOpen) { L($"  - aviso: telas ainda abertas (configurações {SettingsScreen.IsOpen}, controles {OptionsMenu.IsOpen})"); }
            e = Carregar(SceneLoader.MainMenu); while (e.MoveNext()) { yield return e.Current; }

            LivesCounter.BeginRun();
            e = Carregar("Level_02_Troia"); while (e.MoveNext()) { yield return e.Current; }
            foreach (var d in Object.FindObjectsByType<DialogueSequence>(FindObjectsSortMode.None)) { d.Skip(); }
            yield return 0.6f;
            var vida = GameObject.FindGameObjectWithTag("Player").GetComponent<HealthSystem>();
            float t0 = Time.realtimeSinceStartup;
            while (LivesCounter.Current > 0 && Time.realtimeSinceStartup - t0 < 30f) { vida.TakeDamage(9999); yield return 2.2f; vida = GameObject.FindGameObjectWithTag("Player").GetComponent<HealthSystem>(); }
            yield return 1.0f;
            Telas($"Fim de jogo [{idioma}]", "GameOver");

            Time.timeScale = 1f;
            e = Carregar(SceneLoader.LevelComplete); while (e.MoveNext()) { yield return e.Current; }
            Telas($"Fase concluída [{idioma}]", null);
        }

        // ---- 4) Áudio
        L("\n## 4. Áudio");
        if (parte == 0 || parte == 4)
        {
            var e = Carregar(SceneLoader.MainMenu); while (e.MoveNext()) { yield return e.Current; }
            var gerente = Object.FindAnyObjectByType<AudioManager>();
            var fonte = gerente.GetComponents<AudioSource>().FirstOrDefault(a => a.loop);
            SettingsManager.MusicVolume = 0f; yield return 0.2f;
            float zero = fonte.volume;
            SettingsManager.MusicVolume = 1f; yield return 0.2f;
            float cheio = fonte.volume;
            SettingsManager.MusicVolume = musicaOriginal;
            Checar(zero == 0f && cheio > 0f, $"volume de música 0 → fonte {zero:0.00}; 1 → {cheio:0.00}");
            int fontesAntes = Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None).Count(a => a.loop && a.isPlaying);
            e = Carregar("Level_05_Ciclopes"); while (e.MoveNext()) { yield return e.Current; }
            e = Carregar(SceneLoader.MainMenu); while (e.MoveNext()) { yield return e.Current; }
            int fontesDepois = Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None).Count(a => a.loop && a.isPlaying);
            Checar(fontesDepois == fontesAntes, $"fontes em laço tocando no menu: {fontesAntes} antes, {fontesDepois} depois de ir à fase e voltar (sem música duplicada)");
        }

        L($"\n## Erros no console durante a auditoria: {erros}");
        foreach (string s in errosVistos.Distinct()) { L("- " + s); }
        L($"\n## RESULTADO: {(falhas == 0 ? "OK" : falhas + " falha(s)")}");
    }

    // ---------------------------------------------------------------- telas

    private static readonly Vector2Int[] Resolucoes =
    {
        new Vector2Int(1280, 720), new Vector2Int(1920, 1080), new Vector2Int(960, 540), new Vector2Int(2340, 1080), new Vector2Int(1024, 768),
    };

    /// <summary>
    /// Em cada resolução: todos os textos visíveis (da tela pedida, ou de todas se null) dentro da imagem e sem um texto
    /// por cima de outro do mesmo Canvas. Fotos com o CanvasScaler recalculado e as malhas refeitas.
    /// </summary>
    private static void Telas(string nome, string canvasContem)
    {
        var problemas = new List<string>();
        int conferidos = 0;
        foreach (Vector2Int res in Resolucoes)
        {
            Camera cam = Camera.main;
            var rt = new RenderTexture(res.x, res.y, 24);
            var canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c => c.isRootCanvas && c.renderMode == RenderMode.ScreenSpaceOverlay).ToList();
            foreach (Canvas c in canvases) { c.renderMode = RenderMode.ScreenSpaceCamera; c.worldCamera = cam; c.planeDistance = 1f; }
            cam.targetTexture = rt;
            Escalar();
            var textos = Object.FindObjectsByType<Text>(FindObjectsSortMode.None)
                .Where(t => t.isActiveAndEnabled && !string.IsNullOrWhiteSpace(t.text) && t.color.a > 0.05f && t.canvas != null)
                .Where(t => canvasContem == null || t.canvas.rootCanvas.name.Contains(canvasContem))
                .Where(t => t.GetComponentInParent<CanvasGroup>() == null || t.GetComponentInParent<CanvasGroup>().alpha > 0.05f)
                .Select(t => (t, r: Glifos(t, cam))).Where(x => x.r.width > 0).ToList();
            conferidos += textos.Count;
            foreach (var (t, r) in textos)
            {
                if (r.xMin < -1 || r.yMin < -1 || r.xMax > res.x + 1 || r.yMax > res.y + 1) { problemas.Add($"{res.x}×{res.y} {t.name} \"{Curto(t.text)}\" fora da tela"); }
            }
            for (int i = 0; i < textos.Count; i++)
            {
                for (int j = i + 1; j < textos.Count; j++)
                {
                    if (textos[i].t.canvas.rootCanvas != textos[j].t.canvas.rootCanvas) { continue; }
                    Rect a = textos[i].r, b = textos[j].r;
                    if (a.xMin + 2 < b.xMax && b.xMin + 2 < a.xMax && a.yMin + 2 < b.yMax && b.yMin + 2 < a.yMax)
                    {
                        problemas.Add($"{res.x}×{res.y} \"{Curto(textos[i].t.text)}\" × \"{Curto(textos[j].t.text)}\"");
                    }
                }
            }
            cam.targetTexture = null;
            foreach (Canvas c in canvases) { c.renderMode = RenderMode.ScreenSpaceOverlay; }
            rt.Release();
            Escalar();
        }
        Checar(problemas.Count == 0, $"{nome}: {conferidos} textos em 5 resoluções, nenhum cortado ou sobreposto" + (problemas.Count > 0 ? " — " + string.Join("; ", problemas.Distinct().Take(8)) : ""));
    }

    private static string Curto(string s) { s = s.Replace("\n", " "); return s.Length > 28 ? s.Substring(0, 28) + "…" : s; }

    private static Rect Glifos(Text t, Camera cam)
    {
        IList<UIVertex> v = t.cachedTextGenerator.verts;
        if (v == null || v.Count == 0) { return Rect.zero; }
        Vector2 min = Vector2.positiveInfinity, max = Vector2.negativeInfinity;
        float escala = 1f / t.pixelsPerUnit;
        Camera c = t.canvas.rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : cam;
        foreach (UIVertex u in v)
        {
            Vector2 s = RectTransformUtility.WorldToScreenPoint(c, t.rectTransform.TransformPoint(u.position * escala));
            min = Vector2.Min(min, s); max = Vector2.Max(max, s);
        }
        return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
    }

    /// <summary>O CanvasScaler só recalcula no Update, pela janela do batch: recalcula e refaz as malhas antes da foto.</summary>
    private static void Escalar()
    {
        var handle = typeof(CanvasScaler).GetMethod("Handle", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        foreach (var s in Object.FindObjectsByType<CanvasScaler>(FindObjectsSortMode.None)) { handle.Invoke(s, null); }
        foreach (var g in Object.FindObjectsByType<Graphic>(FindObjectsSortMode.None)) { g.SetAllDirty(); }
        Canvas.ForceUpdateCanvases();
        // O painel das Configurações se encaixa na tela no Update dele: roda o ajuste antes da foto, como o quadro faria.
        var ajuste = typeof(SettingsScreen).GetMethod("AjustarAoTamanhoDaTela", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        foreach (var t in Object.FindObjectsByType<SettingsScreen>(FindObjectsSortMode.None)) { ajuste?.Invoke(t, null); }
        Canvas.ForceUpdateCanvases();
    }

    // ---------------------------------------------------------------- base

    private static IEnumerator<float> Carregar(string cena)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(cena);
        float t = Time.realtimeSinceStartup;
        while (SceneManager.GetActiveScene().name != cena && Time.realtimeSinceStartup - t < 30f) { yield return 0.1f; }
        yield return 2.0f;
        AudioListener.volume = 0f;
        foreach (var es in Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None)) { es.SendMessage("OnApplicationFocus", true, SendMessageOptions.DontRequireReceiver); }
    }

    private static void Checar(bool ok, string s)
    {
        if (!ok) { falhas++; }
        L((ok ? "- OK " : "- **FALHA** ") + s);
    }

    private static void L(string s) { rel.AppendLine(s); Debug.Log("[Audit] " + s); }

    private static void Sair()
    {
        EditorApplication.update -= Tick;
        Application.logMessageReceived -= AoLogar;
        System.IO.File.WriteAllText("Logs/qa_audit.txt", rel.ToString());
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
