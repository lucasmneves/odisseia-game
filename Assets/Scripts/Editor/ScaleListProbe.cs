using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Lista renderers cujo nome começa com um prefixo: escala, modo de desenho, tamanho, sprite e pivô.
/// Unity.exe -batchmode -nographics -quit -projectPath . -executeMethod ScaleListProbe.Run -probeScene cena -probePrefix Palace_
/// Sem os dois argumentos sai com erro (probe sem alvo não tem default).
/// </summary>
public static class ScaleListProbe
{
    public static void Run()
    {
        string[] a = System.Environment.GetCommandLineArgs();
        int i = System.Array.IndexOf(a, "-probeScene"), j = System.Array.IndexOf(a, "-probePrefix");
        if (i < 0 || j < 0) { Debug.LogError("[Scale] faltam -probeScene e -probePrefix"); EditorApplication.Exit(1); return; }
        EditorSceneManager.OpenScene(a[i + 1], OpenSceneMode.Single);
        foreach (var s in Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Where(s => s.name.StartsWith(a[j + 1])).OrderBy(s => s.name))
        {
            var sp = s.sprite;
            Debug.Log($"[Scale] {s.name} | esc {s.transform.lossyScale.x:0.###} x {s.transform.lossyScale.y:0.###} | {s.drawMode} size {s.size.x:0.##}x{s.size.y:0.##} | " +
                $"{(sp != null ? AssetDatabase.GetAssetPath(sp) + $" {sp.rect.width}x{sp.rect.height} pivô {sp.pivot.x / sp.rect.width:0.##},{sp.pivot.y / sp.rect.height:0.##}" : "-")} | ordem {s.sortingOrder} | x {s.bounds.min.x:0.00}..{s.bounds.max.x:0.00} y {s.bounds.min.y:0.00}..{s.bounds.max.y:0.00}");
        }
        EditorApplication.Exit(0);
    }
}
