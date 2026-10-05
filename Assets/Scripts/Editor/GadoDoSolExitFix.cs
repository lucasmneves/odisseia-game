using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// QA-06 — Gado do Sol: enquanto a saída espera o vento (TimeGatedActivator, 25 s), dava para passar do navio e cair do
/// fim da fase, perdendo vidas:
///
/// Unity.exe -batchmode -nographics -quit -projectPath . -executeMethod GadoDoSolExitFix.Run
///
/// Medido: o Floor_3 acaba em x = 40, a câmera vê até x = 42 e o vestidor desenha o chão até lá — o jogador andava num
/// chão que parecia sólido e caía por ele (o bot chegou a x = 46). A correção fecha a ponta como as outras fases fecham as
/// delas (Wall_Troia): parede-limite na layer Ground, com ARTE da própria fase — muro de pedra seca empilhado, um terraço
/// subindo no fim do campo —, porque parede invisível é indistinguível de bug (README).
///
/// Colisor: face em x = fim do chão, 4,5 un acima dele (o pulo duplo sobe 3,43; a escalada de beirada precisa da cabeça
/// livre). Nada antes da saída muda. Objeto próprio (Wall_End) fora da raiz do vestidor, com a arte como filha: rodar o
/// GadoDoSolSceneDresser de novo não o apaga. Idempotente.
/// </summary>
public static class GadoDoSolExitFix
{
    private const string ScenePath = "Assets/Scenes/Levels/Level_12_GadoDoSol.unity";
    private const string Muro = "Assets/Art/Environments/GadoDoSol/Fields/gado_drystone_wall.png";
    private const string Nome = "Wall_End";
    private const float AlturaAcimaDoChao = 4.5f;

    public static void Run()
    {
        bool ok = Aplicar();
        if (Application.isBatchMode) { EditorApplication.Exit(ok ? 0 : 1); }
    }

    private static bool Aplicar()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject chao = GameObject.Find("Floor_3");
        var colChao = chao != null ? chao.GetComponent<BoxCollider2D>() : null;
        Sprite muro = AssetDatabase.LoadAssetAtPath<Sprite>(Muro);
        if (colChao == null || muro == null) { Debug.LogError("[Gado] sem Floor_3 ou sem o muro de pedra seca"); return false; }

        Bounds b = colChao.bounds;
        float topo = b.max.y;

        GameObject parede = GameObject.Find(Nome);
        if (parede != null) { Object.DestroyImmediate(parede); }
        parede = new GameObject(Nome);
        parede.layer = LayerMask.NameToLayer("Ground");

        // Colisor com a face esquerda no fim do chão e a base enterrada no próprio chão.
        const float espessura = 1f;
        parede.transform.position = new Vector3(b.max.x + espessura * 0.5f, topo + AlturaAcimaDoChao * 0.5f - 0.5f, 0f);
        var col = parede.AddComponent<BoxCollider2D>();
        col.size = new Vector2(espessura, AlturaAcimaDoChao + 1f);

        // Arte: fiadas do muro (pivô na base) com a borda esquerda na face do colisor, em espelho alternado.
        // Fiadas sobrepostas em 20%: o topo do muro é irregular e, encostadas, deixavam buraco entre elas.
        float altura = muro.bounds.size.y * 0.8f, largura = muro.bounds.size.x;
        for (int i = 0; i * altura < AlturaAcimaDoChao; i++)
        {
            var fiada = new GameObject($"Terrace_{i}");
            fiada.transform.SetParent(parede.transform, true);
            fiada.transform.position = new Vector3(b.max.x + largura * 0.5f, topo + i * altura, 0f);
            var sr = fiada.AddComponent<SpriteRenderer>();
            sr.sprite = muro;
            sr.flipX = i % 2 == 1;
            sr.sortingOrder = -1;
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"[Gado] {Nome}: face em x = {b.max.x:0.00}, colisor até y = {topo + AlturaAcimaDoChao:0.00}");
        return true;
    }
}
