using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Foto do jogo COM o HUD, em play mode — a captura de câmera comum não pega Canvas em Screen Space Overlay:
///
/// Unity.exe -batchmode -projectPath . -executeMethod HudShot.Capture [-hudScene Level_02_Troia] [-hudOut Docs/x.png]
/// (sem -nographics e sem -quit; sai sozinho)
///
/// Abre a fase pelo Boot como o jogo faz, espera o HUD montar, passa os Canvas para Screen Space Camera sobre a câmera
/// principal só durante o render (1280×720, a resolução de referência do CanvasScaler) e grava o PNG. Registra no log os
/// textos e ícones do HUD. Não salva cena nenhuma; restaura as opções de play mode.
/// </summary>
public static class HudShot
{
    private static string cena, saida;
    private static double acordar;
    private static int passo;
    private static bool opcoesAtivas;
    private static EnterPlayModeOptions opcoes;

    public static void Capture()
    {
        string[] a = System.Environment.GetCommandLineArgs();
        string Arg(string n, string d) { int i = System.Array.IndexOf(a, n); return i >= 0 && i < a.Length - 1 ? a[i + 1] : d; }
        cena = Arg("-hudScene", "Level_02_Troia");
        saida = Arg("-hudOut", "Docs/QA/_hud/" + System.IO.Path.GetFileNameWithoutExtension(cena) + ".png");

        opcoesAtivas = EditorSettings.enterPlayModeOptionsEnabled;
        opcoes = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;

        // -hudMapProgress N: o MAPA com as N primeiras fases concluídas, pelo caminho do jogo — Boot (CampaignManager),
        // CompleteLevel como o LevelGoal faz, e o WorldMap. Grava o save do EDITOR; o anterior é guardado e restaurado.
        progressoDoMapa = int.TryParse(Arg("-hudMapProgress", "-1"), out int n) ? n : -1;
        if (progressoDoMapa >= 0)
        {
            saveAnterior = PlayerPrefs.HasKey(ChaveDoSave) ? PlayerPrefs.GetString(ChaveDoSave) : null;
            PlayerPrefs.DeleteKey(ChaveDoSave);
            saida = Arg("-hudOut", $"Docs/QA/_hud/worldmap_{progressoDoMapa:00}.png");
            EditorSceneManager.OpenScene("Assets/Scenes/Boot/Boot.unity", OpenSceneMode.Single);
        }
        else
        {
            // Nome de fase (Level_02_Troia) ou caminho completo de cena (Assets/Scenes/WorldMap/WorldMap.unity).
            EditorSceneManager.OpenScene(cena.EndsWith(".unity") ? cena : "Assets/Scenes/Levels/" + cena + ".unity", OpenSceneMode.Single);
        }

        passo = 0;
        EditorApplication.update += Tick;
        EditorApplication.EnterPlaymode();
    }

    private const string ChaveDoSave = "Odisseia.Save";
    private static int progressoDoMapa = -1;
    private static string saveAnterior;

    private static void Tick()
    {
        if (!EditorApplication.isPlaying) { return; }

        if (progressoDoMapa >= 0 && passo == 0)
        {
            var campanha = Odisseia.Core.CampaignManager.Instance;
            if (campanha == null) { return; }
            foreach (var fase in campanha.Levels.Take(progressoDoMapa)) { campanha.CompleteLevel(fase.LevelId, 0, 0); }
            UnityEngine.SceneManagement.SceneManager.LoadScene("WorldMap");
            acordar = EditorApplication.timeSinceStartup + 4.0;
            passo = 1;
            return;
        }

        if (passo == 0) { acordar = EditorApplication.timeSinceStartup + 3.0; passo = 1; return; }
        if (EditorApplication.timeSinceStartup < acordar) { return; }

        try { Fotografar(); }
        catch (System.Exception e) { Debug.LogError("[HudShot] " + e); }
        finally
        {
            EditorApplication.update -= Tick;
            EditorApplication.ExitPlaymode();
            EditorSettings.enterPlayModeOptionsEnabled = opcoesAtivas;
            EditorSettings.enterPlayModeOptions = opcoes;
            if (progressoDoMapa >= 0)
            {
                if (saveAnterior != null) { PlayerPrefs.SetString(ChaveDoSave, saveAnterior); } else { PlayerPrefs.DeleteKey(ChaveDoSave); }
                PlayerPrefs.Save();
            }
            EditorApplication.Exit(0);
        }
    }

    private static void Fotografar()
    {
        var hud = Object.FindObjectsByType<Odisseia.UI.HUD>().FirstOrDefault();
        if (hud != null)
        {
            foreach (Text t in hud.GetComponentsInChildren<Text>(true).Where(t => t.gameObject.activeInHierarchy))
            {
                Debug.Log($"[HudShot] texto {t.name}: \"{t.text}\" em {((RectTransform)t.transform).anchoredPosition}");
            }
            foreach (Image i in hud.GetComponentsInChildren<Image>(true).Where(i => i.name.StartsWith("Icon_")))
            {
                var r = (RectTransform)i.transform;
                Debug.Log($"[HudShot] ícone {i.name}: {i.sprite?.name} {r.sizeDelta} em {r.anchoredPosition}");
            }
        }
        else { Debug.LogWarning("[HudShot] sem HUD na cena"); }

        Camera cam = Camera.main;
        var rt = new RenderTexture(1280, 720, 24) { filterMode = FilterMode.Point };
        var canvases = Object.FindObjectsByType<Canvas>().Where(c => c.isRootCanvas && c.renderMode == RenderMode.ScreenSpaceOverlay).ToList();
        foreach (Canvas c in canvases) { c.renderMode = RenderMode.ScreenSpaceCamera; c.worldCamera = cam; c.planeDistance = 1f; }
        cam.targetTexture = rt;
        Canvas.ForceUpdateCanvases();
        cam.Render();
        RenderTexture.active = rt;
        var tex = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
        tex.Apply();
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(saida));
        System.IO.File.WriteAllBytes(saida, tex.EncodeToPNG());
        RenderTexture.active = null;
        cam.targetTexture = null;
        foreach (Canvas c in canvases) { c.renderMode = RenderMode.ScreenSpaceOverlay; }
        Debug.Log($"[HudShot] {saida} ({canvases.Count} canvas)");
    }
}
