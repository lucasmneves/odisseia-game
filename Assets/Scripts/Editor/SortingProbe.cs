using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Odisseia.Systems;

/// <summary>
/// Ordem de desenho nas fases (Etapa 13B.6), sem play mode:
///
/// Unity.exe -batchmode -nographics -quit -projectPath . -executeMethod SortingProbe.Scan
///
/// Por fase: quantos sprites há em cada ordem da camada Default, o que fica nas ordens −2..3 (nomes agrupados), a ordem
/// dos filhos do jogador, de inimigos/chefes e de NPCs, e o que encosta em cada altar. Base para decidir a ordem do
/// jogador sem quebrar cenário, inimigos e primeiro plano. Relatório em Logs/qa_sorting.txt.
/// </summary>
public static class SortingProbe
{
    public static void Scan()
    {
        var rel = new System.Text.StringBuilder();
        var porOrdemGlobal = new SortedDictionary<int, int>();
        foreach (string guid in AssetDatabase.FindAssets("t:Scene Level_", new[] { "Assets/Scenes/Levels" }).OrderBy(g => AssetDatabase.GUIDToAssetPath(g)))
        {
            string caminho = AssetDatabase.GUIDToAssetPath(guid);
            EditorSceneManager.OpenScene(caminho, OpenSceneMode.Single);
            var todos = Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            rel.AppendLine($"\n## {System.IO.Path.GetFileNameWithoutExtension(caminho)} — {todos.Length} sprites");

            var porOrdem = todos.GroupBy(r => r.sortingOrder).OrderBy(g => g.Key);
            rel.AppendLine("- por ordem: " + string.Join(" · ", porOrdem.Select(g => $"{g.Key}:{g.Count()}")));
            foreach (var g in porOrdem) { porOrdemGlobal[g.Key] = (porOrdemGlobal.TryGetValue(g.Key, out int n) ? n : 0) + g.Count(); }

            var jogador = GameObject.FindGameObjectWithTag("Player");
            var doJogador = jogador != null ? jogador.GetComponentsInChildren<SpriteRenderer>(true) : new SpriteRenderer[0];
            rel.AppendLine("- jogador: " + string.Join(", ", doJogador.Select(r => $"{r.name}={r.sortingOrder}")));

            var inimigos = todos.Where(r => r.GetComponentInParent<Odisseia.Enemies.EnemyController>() != null || r.GetComponentInParent<Odisseia.Enemies.BossController>() != null);
            rel.AppendLine("- inimigos/chefes: " + Agrupar(inimigos));
            var npcs = todos.Where(r => r.GetComponentInParent<Odisseia.Levels.NPCDialogue>() != null);
            rel.AppendLine("- NPCs: " + Agrupar(npcs));

            for (int o = -2; o <= 3; o++)
            {
                var nesta = todos.Where(r => r.sortingOrder == o && !doJogador.Contains(r) && !inimigos.Contains(r) && !npcs.Contains(r));
                if (nesta.Any()) { rel.AppendLine($"- ordem {o}: " + Agrupar(nesta)); }
            }

            foreach (Checkpoint c in Object.FindObjectsByType<Checkpoint>(FindObjectsSortMode.None).OrderBy(c => c.transform.position.x))
            {
                var a = c.GetComponent<SpriteRenderer>();
                var perto = todos.Where(r => r != a && r.enabled && r.bounds.Intersects(a.bounds) && !doJogador.Contains(r) && r.sortingOrder >= -3 && r.sortingOrder <= 3)
                    .Select(r => $"{r.name}={r.sortingOrder}");
                rel.AppendLine($"- altar x={c.transform.position.x:0.0} (ordem {a.sortingOrder}, z {c.transform.position.z}): {string.Join(", ", perto)}");
            }
        }
        rel.AppendLine("\n## Total nas 16 fases por ordem\n- " + string.Join(" · ", porOrdemGlobal.Select(p => $"{p.Key}:{p.Value}")));
        System.IO.File.WriteAllText("Logs/qa_sorting.txt", rel.ToString());
        Debug.Log("[Sorting] Logs/qa_sorting.txt");
        if (Application.isBatchMode) { EditorApplication.Exit(0); }
    }

    /// <summary>Nomes sem o sufixo numérico ("Rock_4" → "Rock"), com contagem e ordem.</summary>
    private static string Agrupar(IEnumerable<SpriteRenderer> rs) =>
        string.Join(", ", rs.GroupBy(r => (System.Text.RegularExpressions.Regex.Replace(r.name, @"[_ ]?\(?\d+\)?$", ""), r.sortingOrder))
            .OrderBy(g => g.Key.Item2).Select(g => $"{g.Key.Item1}={g.Key.Item2}{(g.Count() > 1 ? "×" + g.Count() : "")}"));
}
