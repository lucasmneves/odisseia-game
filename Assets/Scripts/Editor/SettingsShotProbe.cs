using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Odisseia.Core;
using Odisseia.Systems;
using Odisseia.UI;

/// <summary>
/// Foto das Configurações e dos Controles no menu (Etapa 13B.8), NO MUDO, em 1280×720, 2340×1080 e 1024×768, com o
/// CanvasScaler recalculado; registra onde ficam os botões "Fechar" e a linha de idioma. Diagnóstico da auditoria.
///
/// Unity.exe -batchmode -projectPath . -executeMethod SettingsShotProbe.Run -shotOut pasta
/// </summary>
public static class SettingsShotProbe
{
    private static bool opcoesAtivas, mudoOriginal;
    private static EnterPlayModeOptions opcoes;
    private static IEnumerator<float> roteiro;
    private static double acordarEm;
    private static string pasta;

    public static void Run()
    {
        string[] a = System.Environment.GetCommandLineArgs();
        int i = System.Array.IndexOf(a, "-shotOut");
        pasta = i >= 0 && i < a.Length - 1 ? a[i + 1] : "Logs";
        mudoOriginal = EditorUtility.audioMasterMute;
        EditorUtility.audioMasterMute = true;
        AudioListener.volume = 0f;
        opcoesAtivas = EditorSettings.enterPlayModeOptionsEnabled;
        opcoes = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        EditorSceneManager.OpenScene("Assets/Scenes/Boot/Boot.unity", OpenSceneMode.Single);
        roteiro = Roteiro();
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
        catch (System.Exception e) { Debug.LogError("[Shot] " + e); Sair(); }
    }

    private static IEnumerator<float> Roteiro()
    {
        while (CampaignManager.Instance == null) { yield return 0.2f; }
        AudioListener.volume = 0f;
        SceneManager.LoadScene(SceneLoader.MainMenu);
        yield return 3f;
        // Cores da UI (item 12): a cor do Image (que o MenuButton escreve) e a do CanvasRenderer (tinta que a transição
        // ColorTint do Button deixa). O que aparece na tela é o produto das duas.
        foreach (string nome in new[] { "ContinueButton", "NewGameButton", "LevelSelectButton", "SettingsButton" })
        {
            var go = GameObject.Find(nome);
            var img = go.GetComponent<Image>();
            Color cr = go.GetComponent<CanvasRenderer>().GetColor();
            Color produto = img.color * cr;
            Debug.Log($"[Shot] cor {nome}: Image {img.color} × CanvasRenderer {cr} = {produto} · transição {go.GetComponent<Button>().transition}");
        }
        SettingsScreen.Open();
        yield return 0.5f;
        Fotos("settings");
        OptionsMenu.Open();
        yield return 0.5f;
        Fotos("controls");
    }

    private static void Fotos(string tela)
    {
        foreach (Vector2Int res in new[] { new Vector2Int(1280, 720), new Vector2Int(2340, 1080), new Vector2Int(1024, 768) })
        {
            Camera cam = Camera.main;
            var rt = new RenderTexture(res.x, res.y, 24);
            var canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Where(c => c.isRootCanvas && c.renderMode == RenderMode.ScreenSpaceOverlay).ToList();
            foreach (Canvas c in canvases) { c.renderMode = RenderMode.ScreenSpaceCamera; c.worldCamera = cam; c.planeDistance = 1f; }
            cam.targetTexture = rt;
            var handle = typeof(CanvasScaler).GetMethod("Handle", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            foreach (var s in Object.FindObjectsByType<CanvasScaler>(FindObjectsSortMode.None)) { handle.Invoke(s, null); }
            foreach (var g in Object.FindObjectsByType<Graphic>(FindObjectsSortMode.None)) { g.SetAllDirty(); }
            Canvas.ForceUpdateCanvases();
            // O painel se encaixa na tela no Update (AjustarAoTamanhoDaTela); a foto vem antes do próximo quadro, então
            // roda o ajuste aqui, como o quadro seguinte faria.
            var ajuste = typeof(SettingsScreen).GetMethod("AjustarAoTamanhoDaTela", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            foreach (var tela2 in Object.FindObjectsByType<SettingsScreen>(FindObjectsSortMode.None)) { ajuste?.Invoke(tela2, null); }
            Canvas.ForceUpdateCanvases();
            foreach (var t in Object.FindObjectsByType<Text>(FindObjectsSortMode.None).Where(t => t.isActiveAndEnabled && (t.text == "CLOSE" || t.text.StartsWith("Language") || t.text == "English")))
            {
                var cantos = new Vector3[4];
                t.rectTransform.GetWorldCorners(cantos);
                Vector2 p0 = RectTransformUtility.WorldToScreenPoint(cam, cantos[0]), p1 = RectTransformUtility.WorldToScreenPoint(cam, cantos[2]);
                Debug.Log($"[Shot] {tela} {res.x}×{res.y}: {t.transform.parent?.name}/{t.name} \"{t.text}\" canvas {t.canvas.rootCanvas.name} (fator {t.canvas.rootCanvas.scaleFactor:0.00}) tela {p0}–{p1}");
            }
            cam.Render();
            RenderTexture.active = rt;
            var tex = new Texture2D(res.x, res.y, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, res.x, res.y), 0, 0);
            tex.Apply();
            RenderTexture.active = null;
            cam.targetTexture = null;
            foreach (Canvas c in canvases) { c.renderMode = RenderMode.ScreenSpaceOverlay; }
            rt.Release();
            System.IO.Directory.CreateDirectory(pasta);
            System.IO.File.WriteAllBytes($"{pasta}/{tela}_{res.x}x{res.y}.png", tex.EncodeToPNG());
        }
    }

    private static void Sair()
    {
        EditorApplication.update -= Tick;
        EditorApplication.ExitPlaymode();
        AudioListener.volume = 1f;
        EditorUtility.audioMasterMute = mudoOriginal;
        EditorSettings.enterPlayModeOptionsEnabled = opcoesAtivas;
        EditorSettings.enterPlayModeOptions = opcoes;
        EditorApplication.Exit(0);
    }
}
