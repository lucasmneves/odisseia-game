using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Odisseia.Systems;

/// <summary>
/// Elenco da Fase 04 — Citera (Docs/Characters/Fase04/CYTHERA_CAST.md):
///
/// Unity.exe -batchmode -nographics -quit -projectPath . -executeMethod CyteraCastDresser.Dress
///
/// O CastProbe achou só o Odisseu: nenhum inimigo, nenhum NPC. Mas a fase fala com a tripulação
/// ("Segurem-se! Não deixem o navio virar!") e um "Companheiro" responde — e o navio estava vazio.
/// Os homens são os mesmos que embarcaram na Fase 01 (Marinheiro, Remador, Elpenor), então a arte
/// é REUTILIZADA, sem geração: figuras de fundo sobre os conveses, sem colisor e sem gameplay.
///
/// A altura vem do colisor do convés (topo do BoxCollider2D), não de um número digitado: o convés
/// é plataforma de gameplay e o desenho dele segue o colisor. Ninguém fica a menos de 1,5 un do
/// ponto onde o Odisseu nasce.
/// </summary>
public static class CyteraCastDresser
{
    private const string ScenePath = "Assets/Scenes/Levels/Level_04_Citera.unity";
    private const string RaizDoElenco = "CyteraCast";
    private const string Pasta = "Odisseia/Characters/NPCs/";

    public static void Dress()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        GameObject antigo = GameObject.Find(RaizDoElenco);
        if (antigo != null) { Object.DestroyImmediate(antigo); }

        Bounds popa = Topo("Deck_Popa"), proa = Topo("Deck_Proa");
        GameObject jogador = GameObject.Find("Player");
        float nascimento = jogador != null ? jogador.transform.position.x : float.NaN;
        Debug.Log($"[CyteraCast] popa x {popa.min.x:0.0}..{popa.max.x:0.0} topo {popa.max.y:0.00}; " +
                  $"proa x {proa.min.x:0.0}..{proa.max.x:0.0} topo {proa.max.y:0.00}; jogador nasce em x={nascimento:0.0}");

        var raiz = new GameObject(RaizDoElenco).transform;
        int n = 0;
        // Popa: o Odisseu nasce na ré (x=-15). Um homem na ponta da popa, atrás dele; o outro à frente,
        // por onde o jogador passa ao sair do navio — o primeiro rosto que ele vê.
        n += Tripulante(raiz, "Crew_Sailor", "CHR_Villager_Sailor", popa.min.x + 0.8f, popa.max.y, false, nascimento);
        n += Tripulante(raiz, "Crew_Rower", "CHR_Villager_Sailor_Ochre", popa.max.x - 4.5f, popa.max.y, true, nascimento);
        // Proa: Elpenor espera quem chega, olhando para a esquerda.
        n += Tripulante(raiz, "Crew_Elpenor", "CHR_Villager_Sailor_Olive", proa.max.x - 2.0f, proa.max.y, true, nascimento);

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[CyteraCast] {n} tripulantes nos conveses.");
        EditorApplication.Exit(n == 3 ? 0 : 1);
    }

    /// <summary>Tira o elenco da cena — para comparar a fase com e sem ele.</summary>
    public static void Remove()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject antigo = GameObject.Find(RaizDoElenco);
        if (antigo != null) { Object.DestroyImmediate(antigo); }
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        EditorApplication.Exit(0);
    }

    /// <summary>Retângulo do colisor sólido do convés (o topo é o chão em que se pisa).</summary>
    private static Bounds Topo(string nome)
    {
        GameObject go = GameObject.Find(nome);
        var col = go != null ? go.GetComponent<Collider2D>() : null;
        if (col == null) { Debug.LogError("[CyteraCast] sem colisor em " + nome); return new Bounds(); }
        return col.bounds;
    }

    private static int Tripulante(Transform pai, string nome, string folha, float x, float chao, bool olharEsquerda, float nascimento)
    {
        if (!float.IsNaN(nascimento) && Mathf.Abs(x - nascimento) < 1.5f)
        {
            Debug.LogError($"[CyteraCast] {nome} em x={x:0.0} cairia em cima do jogador (x={nascimento:0.0})");
            return 0;
        }

        Sprite parado = null;
        foreach (Object a in AssetDatabase.LoadAllAssetsAtPath("Assets/Resources/" + Pasta + folha + ".png"))
        {
            if (a is Sprite s && s.name == folha + "_Idle_00") { parado = s; }
        }
        if (parado == null) { Debug.LogError("[CyteraCast] sem folha " + folha); return 0; }

        var go = new GameObject(nome);
        go.transform.SetParent(pai, false);
        var corpo = new GameObject("Body");
        corpo.transform.SetParent(go.transform, false);
        var sr = corpo.AddComponent<SpriteRenderer>();
        sr.sprite = parado;
        sr.flipX = olharEsquerda;
        sr.sortingOrder = 0;   // na frente das tábuas, atrás do jogador (2)
        go.transform.position = new Vector3(x, chao - parado.bounds.min.y, 0f);

        var animador = corpo.AddComponent<SpriteAnimator>();
        var so = new SerializedObject(animador);
        so.FindProperty("resourcePath").stringValue = Pasta + folha;
        so.FindProperty("defaultState").stringValue = "Idle";
        so.FindProperty("defaultFramesPerSecond").floatValue = 5.5f + Mathf.Repeat(Mathf.Abs(x) * 0.37f, 1.2f);
        so.ApplyModifiedPropertiesWithoutUndo();
        Debug.Log($"[CyteraCast] {nome} ({folha}) em x={x:0.0}, pés em y={chao:0.00}");
        return 1;
    }
}
