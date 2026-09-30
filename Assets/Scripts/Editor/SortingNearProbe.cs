using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Quem é desenhado em volta de um ponto: nome, ordem, caixa. Para achar o que tapa um personagem.
/// Unity.exe -batchmode -nographics -quit -projectPath . -executeMethod SortingNearProbe.Run -probeScene cena -probeX 0
/// Sem -probeScene ou -probeX, sai com erro (probe sem alvo não tem default).
/// </summary>
public static class SortingNearProbe
{
    public static void Run()
    {
        string[] a = System.Environment.GetCommandLineArgs();
        int i = System.Array.IndexOf(a, "-probeScene"), j = System.Array.IndexOf(a, "-probeX");
        if (i < 0 || j < 0) { Debug.LogError("[Sorting] faltam -probeScene e -probeX"); EditorApplication.Exit(1); return; }
        float x = float.Parse(a[j + 1], System.Globalization.CultureInfo.InvariantCulture);
        EditorSceneManager.OpenScene(a[i + 1], OpenSceneMode.Single);
        foreach (var s in Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None)
            .Where(s => s.enabled && s.sprite != null && s.bounds.min.x <= x + 1f && s.bounds.max.x >= x - 1f && s.bounds.min.y < -0.5f)
            .OrderBy(s => s.sortingOrder))
        {
            Debug.Log($"[Sorting] ordem {s.sortingOrder,4} {s.sortingLayerName} {s.name} x {s.bounds.min.x:0.00}..{s.bounds.max.x:0.00} y {s.bounds.min.y:0.00}..{s.bounds.max.y:0.00}");
        }
        EditorApplication.Exit(0);
    }
}
