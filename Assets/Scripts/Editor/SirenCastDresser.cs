using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Odisseia.Systems;

/// <summary>
/// Elenco da Fase 10 — Sereias (Docs/Characters/Fase10/SIRENS_CAST.md):
///
/// Unity.exe -batchmode -nographics -quit -projectPath . -executeMethod SirenCastDresser.Dress
/// Unity.exe -batchmode -quit -projectPath . -executeMethod SirenCastDresser.Poses   (fotos, sem salvar)
///
/// Nenhuma sereia estava na cena; o SereiasSceneDresser deixou "o vão do santuário e o espaço das
/// agulhas de rocha livres para elas". Três figuras — uma na porta do santuário, uma ao pé de cada
/// agulha —, sem colisor e sem script: o canto é a SirenZone e o véu Siren_Song, que não são tocados.
///
/// Idempotente.
/// </summary>
public static class SirenCastDresser
{
    private const string ScenePath = "Assets/Scenes/Levels/Level_10_Sereias.unity";
    private const string RaizDoElenco = "SirenCast";
    private const string Pasta = "Odisseia/Characters/NPCs/";
    private const float TopoDoChao = -2f;

    public static void Dress()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject antigo = GameObject.Find(RaizDoElenco);
        if (antigo != null) { Object.DestroyImmediate(antigo); }
        var raiz = new GameObject(RaizDoElenco).transform;
        GameObject jogador = GameObject.Find("Player");
        float nascimento = jogador != null ? jogador.transform.position.x : float.NaN;

        float santuario = X("Shrine"), agulhaE = X("Spire_L"), agulhaD = X("Spire_R");
        int n = 0;
        // Na porta escura do santuário, um pouco à esquerda: à direita da porta está o monte de pedras
        // da plataforma. Clara sobre o vão escuro, como a Circe no templo.
        n += Figura(raiz, "Siren_Shrine", "CHR_Siren", santuario - 0.5f, nascimento);
        // Ao pé das agulhas (a ponta é fina demais para apoiar uma figura), nas bordas da ilha.
        n += Figura(raiz, "Siren_Spire_L", "CHR_Siren_Violet", agulhaE + 1.1f, nascimento);
        n += Figura(raiz, "Siren_Spire_R", "CHR_Siren_Sea", agulhaD - 1.1f, nascimento);

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[SirenCast] {n} sereias na ilha.");
        EditorApplication.Exit(n == 3 ? 0 : 1);
    }

    private static float X(string nome)
    {
        GameObject go = GameObject.Find(nome);
        if (go == null) { Debug.LogError("[SirenCast] não encontrado: " + nome); return 0f; }
        return go.transform.position.x;
    }

    private static int Figura(Transform pai, string nome, string folha, float x, float nascimento)
    {
        if (!float.IsNaN(nascimento) && Mathf.Abs(x - nascimento) < 1.5f)
        {
            Debug.LogError($"[SirenCast] {nome} em x={x:0.0} cairia em cima do jogador (x={nascimento:0.0})");
            return 0;
        }
        Sprite parado = null;
        foreach (Object a in AssetDatabase.LoadAllAssetsAtPath("Assets/Resources/" + Pasta + folha + ".png"))
        {
            if (a is Sprite s && s.name == folha + "_Idle_00") { parado = s; }
        }
        if (parado == null) { Debug.LogError("[SirenCast] sem folha " + folha); return 0; }

        var go = new GameObject(nome);
        go.transform.SetParent(pai, false);
        var corpo = new GameObject("Body");
        corpo.transform.SetParent(go.transform, false);
        var sr = corpo.AddComponent<SpriteRenderer>();
        sr.sprite = parado;
        sr.flipX = true;          // cantam para quem chega: o Odisseu vem da esquerda, amarrado ao mastro
        sr.sortingOrder = -14;    // na frente do santuário (-16) e das agulhas (-15); atrás do véu do canto (-3) e do jogador
        go.transform.position = new Vector3(x, TopoDoChao - parado.bounds.min.y, 0f);

        var animador = corpo.AddComponent<SpriteAnimator>();
        var so = new SerializedObject(animador);
        so.FindProperty("resourcePath").stringValue = Pasta + folha;
        so.FindProperty("defaultState").stringValue = "Idle";
        so.FindProperty("defaultFramesPerSecond").floatValue = 4.5f + Mathf.Repeat(Mathf.Abs(x) * 0.37f, 1.2f);
        so.ApplyModifiedPropertiesWithoutUndo();
        Debug.Log($"[SirenCast] {nome} ({folha}) em x={x:0.0}");
        return 1;
    }

    /// <summary>Odisseu diante da sereia do santuário, e a ilha inteira — sem salvar.</summary>
    public static void Poses()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject jogador = GameObject.Find("Player");
        var corpoDoJogador = jogador.transform.Find("Visual/Body");
        float s = GameObject.Find(RaizDoElenco + "/Siren_Shrine").transform.position.x;
        jogador.transform.position += new Vector3(s - 2.4f - jogador.transform.position.x, TopoDoChao - corpoDoJogador.position.y, 0f);
        const string pasta = "Docs/Characters/Fase10/_capturas/";
        System.IO.Directory.CreateDirectory(pasta);
        PrologueScreenshot.Render(s - 1f, 0f, pasta + "sereia_com_odisseu.png");
        EditorApplication.Exit(0);
    }
}
