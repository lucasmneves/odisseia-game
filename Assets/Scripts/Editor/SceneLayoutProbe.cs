using System.Globalization;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Lista a geometria de uma cena — nome, posição e extensão de cada colisor e marcador:
///
/// Unity.exe -batchmode -quit -nographics -projectPath . -executeMethod SceneLayoutProbe.Listar
///           -cena Assets/Scenes/Levels/Level_03_Cicones.unity
///
/// Vestir um cenário exige saber onde o chão começa e termina. Ler isso do YAML da cena é
/// impraticável: posição de objeto aninhado depende do pai, e o pai aparece depois no arquivo.
/// </summary>
public static class SceneLayoutProbe
{
    public static void Listar()
    {
        // Sem default: um flag com nome errado despejava a cena de Cícones em silêncio, e o dump
        // parece legítimo. Foi assim que uma auditoria da fase errada passou por verdadeira.
        string cena = null;
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "-cena") { cena = args[i + 1]; }
        }
        if (string.IsNullOrEmpty(cena))
        {
            Debug.LogError("[Layout] falta -cena <caminho da cena>");
            if (Application.isBatchMode) { EditorApplication.Exit(1); }
            return;
        }

        EditorSceneManager.OpenScene(cena, OpenSceneMode.Single);
        Debug.Log("[Layout] " + cena);

        foreach (Collider2D c in Object.FindObjectsByType<Collider2D>(FindObjectsSortMode.None)
                     .OrderBy(c => c.bounds.min.x))
        {
            Bounds b = c.bounds;
            Debug.Log(string.Format(CultureInfo.InvariantCulture,
                "[Layout]   {0,-24} {1,-16} x {2,7:0.0} .. {3,7:0.0}   y {4,6:0.0} .. {5,6:0.0}   layer {6}",
                c.gameObject.name, c.isTrigger ? "(gatilho)" : "solido",
                b.min.x, b.max.x, b.min.y, b.max.y, LayerMask.LayerToName(c.gameObject.layer)));
        }

        foreach (SpriteRenderer sr in Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None)
                     .OrderBy(s => s.transform.position.x))
        {
            Debug.Log(string.Format(CultureInfo.InvariantCulture,
                // "(off)" importa: um renderer desligado ainda aparece no dump, e sem essa marca
                // não dá para distinguir "placeholder coberto" de "placeholder ainda aceso".
                "[Sprite]   {0,-24} pos ({1:0.0}, {2:0.0})  escala ({3:0.00}, {4:0.00})  ordem {5}  sprite {6}{7}",
                sr.gameObject.name, sr.transform.position.x, sr.transform.position.y,
                sr.transform.lossyScale.x, sr.transform.lossyScale.y, sr.sortingOrder,
                sr.sprite != null ? sr.sprite.name : "—",
                sr.enabled && sr.gameObject.activeInHierarchy ? "" : "  (off)"));
        }

        if (Application.isBatchMode) { EditorApplication.Exit(0); }
    }
}
