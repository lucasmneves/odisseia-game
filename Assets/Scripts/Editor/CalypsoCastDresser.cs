using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Odisseia.Systems;

/// <summary>
/// Elenco da Fase 13 — Calipso (Docs/Characters/Fase13/CALYPSO_CAST.md):
///
/// Unity.exe -batchmode -nographics -quit -projectPath . -executeMethod CalypsoCastDresser.Dress
/// Unity.exe -batchmode -quit -projectPath . -executeMethod CalypsoCastDresser.Poses   (fotos, sem salvar)
///
/// Calipso não tinha figura nem fala: é citada pelo Odisseu, e a fala do DialogueTrigger_Raft ("Hermes trouxe
/// a ordem de Zeus: Calipso deve me deixar partir. Ela chora, mas obedece.") a põe presente na despedida.
/// Entra como figura de fundo junto à jangada, só com Idle — sem fala nova, sem script, sem colisor (D-023).
///
/// Idempotente. Nenhum prefab e nenhum script de jogo são tocados.
/// </summary>
public static class CalypsoCastDresser
{
    private const string ScenePath = "Assets/Scenes/Levels/Level_13_Calipso.unity";
    private const string Folha = "Odisseia/Characters/NPCs/CHR_Calypso";
    private const string RaizDoElenco = "CalypsoCast";
    private const float TopoDoChao = -2f;   // o mesmo do CalipsoSceneDresser
    // À esquerda da jangada (x 25,9..30,1), diante do palácio: ela fica na margem da terra, a jangada entre
    // ela e o mar. 2,8 un e não 1: a samambaia de primeiro plano FG_Ferns_3 (x 23,6..26,4, ordem 12) tapava os
    // pés dela, e figura sem pés à vista não lê apoiada no chão. Longe do nascimento do jogador (x < 0).
    private const float DistanciaDaJangada = 2.8f;

    public static void Dress()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject antigo = GameObject.Find(RaizDoElenco);
        if (antigo != null) { Object.DestroyImmediate(antigo); }

        GameObject jangada = GameObject.Find("CalipsoScenery/Raft");
        Sprite parada = null;
        foreach (Object a in AssetDatabase.LoadAllAssetsAtPath("Assets/Resources/" + Folha + ".png"))
        {
            if (a is Sprite s && s.name == "CHR_Calypso_Idle_00") { parada = s; }
        }
        if (jangada == null || parada == null)
        {
            Debug.LogError("[CalypsoCast] jangada ou folha da Calipso não encontradas");
            EditorApplication.Exit(1);
            return;
        }

        float x = jangada.GetComponent<SpriteRenderer>().bounds.min.x - DistanciaDaJangada;
        var raiz = new GameObject(RaizDoElenco).transform;
        var go = new GameObject("Calypso");
        go.transform.SetParent(raiz, false);
        var corpo = new GameObject("Body");
        corpo.transform.SetParent(go.transform, false);
        var sr = corpo.AddComponent<SpriteRenderer>();
        sr.sprite = parada;
        sr.flipX = true;        // a folha olha para a esquerda; aqui ela olha para a jangada e para o Odisseu
        sr.sortingOrder = 0;    // na frente do palácio (-18) e das flores (-3), atrás da jangada e do jogador (2)
        go.transform.position = new Vector3(x, TopoDoChao - parada.bounds.min.y, 0f);

        var animador = corpo.AddComponent<SpriteAnimator>();
        var so = new SerializedObject(animador);
        so.FindProperty("resourcePath").stringValue = Folha;
        so.FindProperty("defaultState").stringValue = "Idle";
        so.FindProperty("defaultFramesPerSecond").floatValue = 4f;   // a cabeça desce devagar
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[CalypsoCast] Calipso em x={x:0.00}, desenho x {sr.bounds.min.x:0.00}..{sr.bounds.max.x:0.00} " +
                  $"y {sr.bounds.min.y:0.00}..{sr.bounds.max.y:0.00}");
        EditorApplication.Exit(0);
    }

    /// <summary>Calipso sozinha, com o Odisseu no gatilho da jangada, no pico do Idle e a ilha — sem salvar.</summary>
    public static void Poses()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject calipso = GameObject.Find(RaizDoElenco + "/Calypso");
        var sr = calipso.GetComponentInChildren<SpriteRenderer>();
        GameObject jogador = GameObject.Find("Player");
        var corpo = jogador.transform.Find("Visual/Body");
        const string pasta = "Docs/Characters/Fase13/_capturas/";
        System.IO.Directory.CreateDirectory(pasta);
        float xc = calipso.transform.position.x;

        PrologueScreenshot.Render(xc, 0f, pasta + "calipso_idle.png");

        // Odisseu entre ela e a jangada, virado para ela. Só o x muda: a altura é a de nascimento da cena (a caixa
        // do sprite tem margem transparente abaixo dos pés). NO gatilho (x 27..29) ele some atrás da jangada —
        // ordem 2, empatada com a dele: pendência de cenário, registrada no CALYPSO_CAST.md.
        jogador.transform.position += new Vector3(xc + 1.6f - corpo.position.x, 0f, 0f);
        corpo.GetComponent<SpriteRenderer>().flipX = true;
        PrologueScreenshot.Render(xc + 1.5f, 0f, pasta + "calipso_e_odisseu.png");
        GameObject gatilho = GameObject.Find("DialogueTrigger_Raft");
        jogador.transform.position += new Vector3(gatilho.transform.position.x - corpo.position.x, 0f, 0f);
        corpo.GetComponent<SpriteRenderer>().flipX = false;
        PrologueScreenshot.Render(xc + 3f, 0f, pasta + "odisseu_no_gatilho_da_jangada.png");
        jogador.transform.position += new Vector3(xc + 1.6f - corpo.position.x, 0f, 0f);
        corpo.GetComponent<SpriteRenderer>().flipX = true;

        foreach (Object a in AssetDatabase.LoadAllAssetsAtPath("Assets/Resources/" + Folha + ".png"))
        {
            if (a is Sprite s && s.name == "CHR_Calypso_Idle_05") { sr.sprite = s; }
        }
        PrologueScreenshot.Render(xc + 1.5f, 0f, pasta + "despedida_pico_idle.png");
        foreach (float x in new[] { -8f, 10f, 20f, 36f })
        {
            PrologueScreenshot.Render(x, 0f, pasta + $"ogigia_{x:00}.png");
        }
        EditorApplication.Exit(0);
    }
}
