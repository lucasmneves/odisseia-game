using System.Linq;
using UnityEditor;
using UnityEngine;
using Odisseia.Systems;

/// <summary>
/// Arte nativa dos itens de jogo nos PREFABS (Asset Completion PXL-007/008/014; valem para as 16 fases):
///
/// Unity.exe -batchmode -nographics -quit -projectPath . -executeMethod ItemArtDresser.Run
///
/// - Collectible: moeda nativa (Tools/build-coin-native.js, 0,40 × 0,44 un) em escala 1, no lugar da arte pintada a
///   100 px/un encolhida a 0,35.
/// - ArrowPickup: aljava em escala 1, no lugar do quadrado tingido a 0,4. (Hoje nenhuma cena instancia o prefab — a
///   flecha recolhida acrescenta o componente nela mesma.)
/// - Checkpoint: altar grego, quadro 00 apagado e 01 aceso, nos dois sprites que o <c>Checkpoint</c> já troca.
///
/// Só apresentação: o COLISOR fica do mesmo tamanho no mundo. Ele está no mesmo Transform cuja escala muda, então o raio
/// é recalculado (raio × escala antiga). Idempotente: rodar de novo dá o mesmo prefab.
/// </summary>
public static class ItemArtDresser
{
    public static void Run()
    {
        bool ok = Moeda() & Aljava() & Altar() & Poeira();
        AssetDatabase.SaveAssets();
        if (Application.isBatchMode) { EditorApplication.Exit(ok ? 0 : 1); }
    }

    private static bool Moeda()
    {
        const string caminho = "Assets/Prefabs/Collectible.prefab";
        Sprite quadro = Quadro("Assets/Resources/Odisseia/Items/item_collectible_coin.png", "item_collectible_coin_00");
        if (quadro == null) { return false; }

        GameObject raiz = PrefabUtility.LoadPrefabContents(caminho);
        try
        {
            EscalaUmMantendoColisor(raiz);
            raiz.GetComponent<SpriteRenderer>().sprite = quadro;

            var so = new SerializedObject(raiz.GetComponent<SpriteAnimator>());
            so.FindProperty("resourcePath").stringValue = "Odisseia/Items/item_collectible_coin";
            // O estado vem do nome do quadro: item_collectible_coin_NN → "coin".
            so.FindProperty("defaultState").stringValue = "coin";
            SerializedProperty estados = so.FindProperty("states");
            for (int i = 0; i < estados.arraySize; i++)
            {
                SerializedProperty nome = estados.GetArrayElementAtIndex(i).FindPropertyRelative("state");
                if (nome.stringValue == "Coin") { nome.stringValue = "coin"; }
            }
            so.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(raiz, caminho);
            Debug.Log($"[Itens] Collectible: moeda nativa, escala 1, raio {raiz.GetComponent<CircleCollider2D>().radius:0.###}");
            return true;
        }
        finally { PrefabUtility.UnloadPrefabContents(raiz); }
    }

    private static bool Aljava()
    {
        const string caminho = "Assets/Prefabs/ArrowPickup.prefab";
        Sprite aljava = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Items/item_arrow_quiver.png");
        if (aljava == null) { Debug.LogError("[Itens] sem item_arrow_quiver"); return false; }

        GameObject raiz = PrefabUtility.LoadPrefabContents(caminho);
        try
        {
            EscalaUmMantendoColisor(raiz);
            var sr = raiz.GetComponent<SpriteRenderer>();
            sr.sprite = aljava;
            sr.color = Color.white;   // a tinta ocre era o que fazia o quadrado ler como "flechas"
            PrefabUtility.SaveAsPrefabAsset(raiz, caminho);
            Debug.Log($"[Itens] ArrowPickup: aljava, escala 1, raio {raiz.GetComponent<CircleCollider2D>().radius:0.###}");
            return true;
        }
        finally { PrefabUtility.UnloadPrefabContents(raiz); }
    }

    private static bool Altar()
    {
        const string caminho = "Assets/Prefabs/Checkpoint.prefab";
        const string folha = "Assets/Art/Items/item_checkpoint_altar.png";
        Sprite apagado = Quadro(folha, "item_checkpoint_altar_00");
        Sprite aceso = Quadro(folha, "item_checkpoint_altar_01");
        if (apagado == null || aceso == null) { return false; }

        GameObject raiz = PrefabUtility.LoadPrefabContents(caminho);
        try
        {
            raiz.GetComponent<SpriteRenderer>().sprite = apagado;
            var so = new SerializedObject(raiz.GetComponent<Checkpoint>());
            so.FindProperty("inactiveSprite").objectReferenceValue = apagado;
            so.FindProperty("activeSprite").objectReferenceValue = aceso;
            so.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(raiz, caminho);
            Debug.Log("[Itens] Checkpoint: altar apagado/aceso (quadros 00 e 01)");
            return true;
        }
        finally { PrefabUtility.UnloadPrefabContents(raiz); }
    }

    /// <summary>Poeira de pulo e pouso no Player (só observa o controlador; ver PlayerDustFx).</summary>
    private static bool Poeira()
    {
        const string caminho = "Assets/Prefabs/Player.prefab";
        GameObject raiz = PrefabUtility.LoadPrefabContents(caminho);
        try
        {
            if (raiz.GetComponent<Odisseia.Player.PlayerDustFx>() == null)
            {
                raiz.AddComponent<Odisseia.Player.PlayerDustFx>();
                PrefabUtility.SaveAsPrefabAsset(raiz, caminho);
                Debug.Log("[Itens] Player: PlayerDustFx acrescentado");
            }
            else
            {
                Debug.Log("[Itens] Player: PlayerDustFx já estava");
            }
            return true;
        }
        finally { PrefabUtility.UnloadPrefabContents(raiz); }
    }

    /// <summary>Leva a escala a 1 e devolve ao colisor circular o mesmo raio NO MUNDO que ele tinha.</summary>
    private static void EscalaUmMantendoColisor(GameObject raiz)
    {
        float k = raiz.transform.localScale.x;
        if (Mathf.Approximately(k, 1f)) { return; }
        var col = raiz.GetComponent<CircleCollider2D>();
        if (col != null) { col.radius *= k; col.offset *= k; }
        raiz.transform.localScale = Vector3.one;
    }

    private static Sprite Quadro(string folha, string nome)
    {
        Sprite s = AssetDatabase.LoadAllAssetsAtPath(folha).OfType<Sprite>().FirstOrDefault(x => x.name == nome);
        if (s == null) { Debug.LogError($"[Itens] sem {nome} em {folha}"); }
        return s;
    }
}
