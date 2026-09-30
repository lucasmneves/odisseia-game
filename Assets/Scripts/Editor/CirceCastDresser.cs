using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Odisseia.Systems;

/// <summary>
/// Elenco da Fase 08 — Circe (Docs/Characters/Fase08/CIRCE_CAST.md):
///
/// Unity.exe -batchmode -nographics -quit -projectPath . -executeMethod CirceCastDresser.Dress
/// Unity.exe -batchmode -quit -projectPath . -executeMethod CirceCastDresser.Poses   (fotos, sem salvar)
///
/// - Os dois EnemyBasic carmesim viram LOBOS ENCANTADOS da Circe, por override na cena
///   (EnemyFactionOverride) — o prefab compartilhado fica intacto.
/// - A Circe entra como figura no vão da porta do templo, que o CirceSceneDresser deixou reservado
///   para ela ("é onde Circe, o Odisseu e o diálogo entram depois"). Sem fala nova, sem script, sem colisor.
///
/// A transformação (TransformationZone/TransformationEffect) não é tocada: ela tinge o Odisseu, não
/// troca de sprite.
///
/// Idempotente.
/// </summary>
public static class CirceCastDresser
{
    private const string ScenePath = "Assets/Scenes/Levels/Level_08_Circe.unity";
    private const string Lobo = "Odisseia/Enemies/CHR_Circe_Wolf";
    private const string FolhaCirce = "Odisseia/Characters/NPCs/CHR_Circe";
    private const string RaizDoElenco = "CirceCast";
    private const float TopoDoChao = -2f;   // o mesmo do CirceSceneDresser

    public static void Dress()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        int lobos = EnemyFactionOverride.Aplicar(Lobo, "CirceCast");

        GameObject antigo = GameObject.Find(RaizDoElenco);
        if (antigo != null) { Object.DestroyImmediate(antigo); }
        GameObject templo = GameObject.Find("Temple");
        Sprite parada = null;
        foreach (Object a in AssetDatabase.LoadAllAssetsAtPath("Assets/Resources/" + FolhaCirce + ".png"))
        {
            if (a is Sprite s && s.name == "CHR_Circe_Idle_00") { parada = s; }
        }
        if (lobos <= 0 || templo == null || parada == null)
        {
            Debug.LogError("[CirceCast] inimigos, templo ou folha da Circe não encontrados");
            EditorApplication.Exit(1);
            return;
        }

        var raiz = new GameObject(RaizDoElenco).transform;
        var go = new GameObject("Circe");
        go.transform.SetParent(raiz, false);
        var corpo = new GameObject("Body");
        corpo.transform.SetParent(go.transform, false);
        var sr = corpo.AddComponent<SpriteRenderer>();
        sr.sprite = parada;
        sr.flipX = true;        // olha para a esquerda: é de lá que o Odisseu chega ao templo
        sr.sortingOrder = 0;    // na frente do templo (-18) e das flores (-2), atrás do jogador (2)
        // No centro do templo, diante da porta escura: o violeta e o creme leem contra o vão.
        go.transform.position = new Vector3(templo.transform.position.x, TopoDoChao - parada.bounds.min.y, 0f);

        var animador = corpo.AddComponent<SpriteAnimator>();
        var so = new SerializedObject(animador);
        so.FindProperty("resourcePath").stringValue = FolhaCirce;
        so.FindProperty("defaultState").stringValue = "Idle";
        so.FindProperty("defaultFramesPerSecond").floatValue = 5f;   // o vapor da taça sobe devagar
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[CirceCast] {lobos} lobos encantados; Circe em x={templo.transform.position.x:0.0} diante do templo.");
        EditorApplication.Exit(0);
    }

    /// <summary>Circe com o Odisseu diante dela, e um lobo com o Odisseu — sem salvar a cena.</summary>
    public static void Poses()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject circe = GameObject.Find(RaizDoElenco + "/Circe");
        GameObject jogador = GameObject.Find("Player");
        var corpoDoJogador = jogador.transform.Find("Visual/Body");
        void PorJogador(float x) =>
            jogador.transform.position += new Vector3(x - jogador.transform.position.x, TopoDoChao - corpoDoJogador.position.y, 0f);

        const string pasta = "Docs/Characters/Fase08/_capturas/";
        System.IO.Directory.CreateDirectory(pasta);
        PorJogador(circe.transform.position.x - 2.4f);
        PrologueScreenshot.Render(circe.transform.position.x - 1f, 0f, pasta + "circe_com_odisseu.png");

        var inimigo = Object.FindFirstObjectByType<Odisseia.Enemies.EnemyController>();
        PorJogador(inimigo.transform.position.x - 2.2f);
        PrologueScreenshot.Render(inimigo.transform.position.x - 1f, 0f, pasta + "lobo_com_odisseu.png");
        EditorApplication.Exit(0);
    }
}
