using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Odisseia.Systems;

/// <summary>
/// Elenco da Fase 06 — Éolo (Docs/Characters/Fase06/AEOLUS_CAST.md):
///
/// Unity.exe -batchmode -nographics -quit -projectPath . -executeMethod AeolusCastDresser.Dress
/// Unity.exe -batchmode -quit -projectPath . -executeMethod AeolusCastDresser.Poses   (fotos, sem salvar)
///
/// O CastProbe achou só o Odisseu. Mas o palácio foi montado com o vão central RESERVADO para o
/// Éolo (EoloSceneDresser: "é onde Éolo, o Odisseu e o diálogo entram depois"). Ele entra como
/// figura de fundo — no alto dos degraus, diante da porta de bronze, junto ao navio de saída —
/// sem fala nova e sem gameplay: nenhum script, nenhum colisor.
///
/// O odre dos ventos (Art_WindBag) volta à escala 1: a arte foi refeita em tamanho nativo. O
/// WindBagPickup (colisor, cargas, coleta) não é tocado.
///
/// Idempotente.
/// </summary>
public static class AeolusCastDresser
{
    private const string ScenePath = "Assets/Scenes/Levels/Level_06_Eolo.unity";
    private const string RaizDoElenco = "AeolusCast";
    private const string Folha = "Odisseia/Characters/NPCs/CHR_Aeolus";

    public static void Dress()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject antigo = GameObject.Find(RaizDoElenco);
        if (antigo != null) { Object.DestroyImmediate(antigo); }

        Sprite parado = null;
        foreach (Object a in AssetDatabase.LoadAllAssetsAtPath("Assets/Resources/" + Folha + ".png"))
        {
            if (a is Sprite s && s.name == "CHR_Aeolus_Idle_00") { parado = s; }
        }
        GameObject porta = GameObject.Find("Palace_Door");
        if (parado == null || porta == null)
        {
            Debug.LogError("[AeolusCast] folha CHR_Aeolus ou Palace_Door não encontrados");
            EditorApplication.Exit(1);
            return;
        }

        // A porta é posta pela base, no piso do pórtico (topo dos degraus): é ali que ele fica.
        // Um pouco à esquerda do centro da porta, para ela continuar lendo como porta atrás dele.
        float piso = porta.transform.position.y;
        float x = porta.transform.position.x - 0.9f;

        var raiz = new GameObject(RaizDoElenco).transform;
        var go = new GameObject("Aeolus");
        go.transform.SetParent(raiz, false);
        var corpo = new GameObject("Body");
        corpo.transform.SetParent(go.transform, false);
        var sr = corpo.AddComponent<SpriteRenderer>();
        sr.sprite = parado;
        sr.flipX = true;        // olha para a esquerda: é de lá que o Odisseu chega ao palácio
        sr.sortingOrder = -11;  // na frente da porta (-15) e das colunas (-13); atrás do mastro (-7) e do jogador (2)
        go.transform.position = new Vector3(x, piso - parado.bounds.min.y, 0f);

        var animador = corpo.AddComponent<SpriteAnimator>();
        var so = new SerializedObject(animador);
        so.FindProperty("resourcePath").stringValue = Folha;
        so.FindProperty("defaultState").stringValue = "Idle";
        // O Idle é o vento no manto e no cabelo: um pouco mais lento que o dos mortais.
        so.FindProperty("defaultFramesPerSecond").floatValue = 5f;
        so.ApplyModifiedPropertiesWithoutUndo();

        GameObject odre = GameObject.Find("Art_WindBag");
        if (odre != null) { odre.transform.localScale = Vector3.one; }

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[AeolusCast] Éolo em x={x:0.00}, pés em y={piso:0.00} (piso do pórtico); " +
                  $"odre {(odre != null ? "em escala 1" : "NÃO encontrado")}");
        EditorApplication.Exit(odre != null ? 0 : 1);
    }

    /// <summary>Éolo ao lado do Odisseu, e o odre de perto — sem salvar a cena.</summary>
    public static void Poses()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject eolo = GameObject.Find(RaizDoElenco + "/Aeolus");
        GameObject jogador = GameObject.Find("Player");
        var corpoDoJogador = jogador.transform.Find("Visual/Body");
        // Odisseu no chão, ao pé dos degraus, olhando para o Éolo.
        jogador.transform.position += new Vector3(eolo.transform.position.x - 2.6f - jogador.transform.position.x,
            -2f - corpoDoJogador.position.y, 0f);
        const string pasta = "Docs/Characters/Fase06/_capturas/";
        System.IO.Directory.CreateDirectory(pasta);
        PrologueScreenshot.Render(eolo.transform.position.x, 0f, pasta + "eolo_com_odisseu.png");
        GameObject odre = GameObject.Find("Art_WindBag");
        jogador.transform.position += new Vector3(odre.transform.position.x - 1.6f - jogador.transform.position.x, 0f, 0f);
        PrologueScreenshot.Render(odre.transform.position.x, 0f, pasta + "odre_com_odisseu.png");
        EditorApplication.Exit(0);
    }
}
