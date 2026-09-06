using System.Globalization;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Mede a extensão jogável de cada fase — quanto cenário cada uma precisa, em unidades:
///
/// Unity.exe -batchmode -quit -nographics -projectPath . -executeMethod LevelExtentProbe.Medir
///
/// Existe porque "quantos assets essa fase precisa" é uma pergunta de comprimento, não de
/// gosto. Uma fase de 350 un e uma de 60 un pedem quantidades muito diferentes de arte, e
/// contar objetos na cena não responde isso.
/// </summary>
public static class LevelExtentProbe
{
    public static void Medir()
    {
        foreach (string caminho in AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes/Levels" })
                     .Select(AssetDatabase.GUIDToAssetPath).OrderBy(c => c))
        {
            EditorSceneManager.OpenScene(caminho, OpenSceneMode.Single);

            var cols = UnityEngine.Object.FindObjectsByType<Collider2D>(FindObjectsSortMode.None)
                .Where(c => !c.isTrigger).ToArray();
            var srs = UnityEngine.Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
            int parallax = UnityEngine.Object.FindObjectsByType<Odisseia.Systems.ParallaxLayer>(FindObjectsSortMode.None).Length;

            if (cols.Length == 0) { continue; }

            float x0 = cols.Min(c => c.bounds.min.x), x1 = cols.Max(c => c.bounds.max.x);
            float y0 = cols.Min(c => c.bounds.min.y), y1 = cols.Max(c => c.bounds.max.y);

            Debug.Log(string.Format(CultureInfo.InvariantCulture,
                "[Extensao] {0,-34} {1,7:0} un de largura ({2:0} a {3:0}), {4:0} de altura | " +
                "{5,3} colliders solidos, {6,3} sprites, {7,2} camadas de parallax",
                System.IO.Path.GetFileNameWithoutExtension(caminho), x1 - x0, x0, x1, y1 - y0,
                cols.Length, srs.Length, parallax));
        }

        if (Application.isBatchMode) { EditorApplication.Exit(0); }
    }
}
