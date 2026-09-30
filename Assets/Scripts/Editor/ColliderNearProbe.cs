using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Colisores 2D numa faixa de x: nome, layer, trigger, caixa. Para entender onde o jogador fica preso.
/// Unity.exe -batchmode -nographics -quit -projectPath . -executeMethod ColliderNearProbe.Run -probeScene cena -probeX 9 -probeX2 18
/// Sem os argumentos sai com erro (probe sem alvo não tem default).
/// </summary>
public static class ColliderNearProbe
{
    public static void Run()
    {
        string[] a = System.Environment.GetCommandLineArgs();
        int i = System.Array.IndexOf(a, "-probeScene"), j = System.Array.IndexOf(a, "-probeX"), k = System.Array.IndexOf(a, "-probeX2");
        if (i < 0 || j < 0 || k < 0) { Debug.LogError("[Col] faltam -probeScene -probeX -probeX2"); EditorApplication.Exit(1); return; }
        var ci = System.Globalization.CultureInfo.InvariantCulture;
        float x0 = float.Parse(a[j + 1], ci), x1 = float.Parse(a[k + 1], ci);
        EditorSceneManager.OpenScene(a[i + 1], OpenSceneMode.Single);
        foreach (var c in Object.FindObjectsByType<Collider2D>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Where(c => c.bounds.max.x >= x0 && c.bounds.min.x <= x1).OrderBy(c => c.bounds.min.x))
        {
            Debug.Log($"[Col] {c.name} ({c.GetType().Name}) layer {LayerMask.LayerToName(c.gameObject.layer)} trigger={c.isTrigger} ativo={c.gameObject.activeInHierarchy && c.enabled} " +
                      $"x {c.bounds.min.x:0.00}..{c.bounds.max.x:0.00} y {c.bounds.min.y:0.00}..{c.bounds.max.y:0.00}");
        }
        EditorApplication.Exit(0);
    }
}
