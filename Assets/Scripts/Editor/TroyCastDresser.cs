using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Odisseia.Systems;

/// <summary>
/// Elenco da Fase 02 — Troia, pelo Character Art Master (Docs/Characters/CHARACTER_ART_MASTER.md):
///
/// Unity.exe -batchmode -nographics -quit -projectPath . -executeMethod TroyCastDresser.Dress
///
/// Separado do TroySceneDresser de propósito: aquele veste o cenário e este só toca em
/// personagem, então rodar um não refaz o trabalho do outro.
///
/// 1. Os inimigos viram soldados troianos. O prefab EnemyBasic é compartilhado por sete fases,
///    então a troca é por OVERRIDE nas instâncias desta cena — as outras fases continuam com a
///    arte antiga até terem a sua facção. Só o visual muda: colisor, vida, patrulha e dano são do
///    EnemyController e ficam intactos.
/// 2. O acampamento grego ganha soldados de Micenas, figuras de fundo sem colisor.
///
/// Idempotente: pode rodar quantas vezes quiser.
/// </summary>
public static class TroyCastDresser
{
    private const string ScenePath = "Assets/Scenes/Levels/Level_02_Troia.unity";
    private const float GroundTop = -2f;

    private const string TroianoResources = "Odisseia/Enemies/CHR_Trojan_Soldier";
    private const string GregoResources = "Odisseia/Characters/NPCs/CHR_Greek_Soldier_Mycenae";
    private const string GregoArquivo = "Assets/Resources/" + GregoResources + ".png";
    private const string RaizDoElenco = "TroyCast";

    public static void Dress()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        Sprite grego = Quadro(GregoArquivo, "CHR_Greek_Soldier_Mycenae_Idle_00");
        int inimigos = EnemyFactionOverride.Aplicar(TroianoResources, "TroyCast");
        if (grego == null || inimigos < 0)
        {
            Debug.LogError("[TroyCast] folha não encontrada — rodar node Tools/build-cast-sheets.js antes");
            EditorApplication.Exit(1);
            return;
        }

        // Figuras do acampamento: fogueira (-21), suprimentos (-18) e saída. Nada perto de -28, onde o
        // Odisseu nasce — uma sentinela no estandarte ficava desenhada por cima dele.
        GameObject antigo = GameObject.Find(RaizDoElenco);
        if (antigo != null) { Object.DestroyImmediate(antigo); }
        var raiz = new GameObject(RaizDoElenco).transform;

        Grego(raiz, "Greek_Soldier_Fire", -22.6f, grego, false);    // à fogueira, de frente para ela
        Grego(raiz, "Greek_Soldier_Fire_2", -19.5f, grego, true);   // do outro lado, olhando de volta
        Grego(raiz, "Greek_Sentry_Front", -15.5f, grego, false);    // na saída, olhando para Troia

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[TroyCast] {inimigos} inimigos viraram troianos; 3 gregos no acampamento.");
        EditorApplication.Exit(inimigos > 0 ? 0 : 1);
    }

    private static void Grego(Transform pai, string nome, float x, Sprite arte, bool olharEsquerda)
    {
        var go = new GameObject(nome);
        go.transform.SetParent(pai, false);
        var corpo = new GameObject("Body");
        corpo.transform.SetParent(go.transform, false);

        var sr = corpo.AddComponent<SpriteRenderer>();
        sr.sprite = arte;
        sr.flipX = olharEsquerda;
        sr.sortingOrder = 0;   // atrás do jogador (2) e dos props de primeiro plano (1)

        go.transform.position = new Vector3(x, GroundTop - arte.bounds.min.y, 0f);

        var animador = corpo.AddComponent<SpriteAnimator>();
        var so = new SerializedObject(animador);
        so.FindProperty("resourcePath").stringValue = GregoResources;
        so.FindProperty("defaultState").stringValue = "Idle";
        // FPS pela posição, como na Fase 01: sem isso as figuras respiram em uníssono.
        so.FindProperty("defaultFramesPerSecond").floatValue = 5.5f + Mathf.Repeat(Mathf.Abs(x) * 0.37f, 1.2f);
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static Sprite Quadro(string arquivo, string nome)
    {
        foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(arquivo))
        {
            if (asset is Sprite s && s.name == nome) { return s; }
        }
        return null;
    }
}
