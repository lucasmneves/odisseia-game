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
/// - Checkpoint: altar grego, quadro 00 apagado e 01 aceso, nos dois sprites que o <c>Checkpoint</c> já troca; 01–06
///   como a chama em laço.
///
/// Só apresentação: o COLISOR fica do mesmo tamanho no mundo. Ele está no mesmo Transform cuja escala muda, então o raio
/// é recalculado (raio × escala antiga). Idempotente: rodar de novo dá o mesmo prefab.
/// </summary>
public static class ItemArtDresser
{
    public static void Run()
    {
        bool ok = Moeda() & Aljava() & Altar() & Poeira() & CorpoNaFrente();
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

        // 01–06: a chama em laço (mesmo canvas e pivô; só a chama e as fagulhas mudam).
        var chama = new Sprite[6];
        for (int i = 0; i < chama.Length; i++)
        {
            chama[i] = Quadro(folha, $"item_checkpoint_altar_{i + 1:00}");
            if (chama[i] == null) { return false; }
        }

        GameObject raiz = PrefabUtility.LoadPrefabContents(caminho);
        try
        {
            raiz.GetComponent<SpriteRenderer>().sprite = apagado;
            var so = new SerializedObject(raiz.GetComponent<Checkpoint>());
            so.FindProperty("inactiveSprite").objectReferenceValue = apagado;
            so.FindProperty("activeSprite").objectReferenceValue = aceso;
            SerializedProperty quadros = so.FindProperty("activeFrames");
            quadros.arraySize = chama.Length;
            for (int i = 0; i < chama.Length; i++) { quadros.GetArrayElementAtIndex(i).objectReferenceValue = chama[i]; }
            so.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(raiz, caminho);
            Debug.Log("[Itens] Checkpoint: altar apagado/aceso (quadros 00 e 01) e chama em laço (01–06)");
            return true;
        }
        finally { PrefabUtility.UnloadPrefabContents(raiz); }
    }

    /// <summary>Poeira de pulo e pouso no Player (só observa o controlador; ver PlayerDustFx).</summary>
    /// <summary>
    /// Odisseu na frente do altar (13B.6). Ordem 0 é a camada "interativa" de toda fase (chão, altares, moedas, plataformas,
    /// objetivo) e o Body do jogador também é 0: com ordem igual, o Unity (pipeline Built-in, câmera ortográfica) desempata
    /// pela distância à câmera, e tudo estava em z 0 — o Odisseu nascia às vezes atrás do altar. Só o sprite Body chega
    /// 0,05 para a câmera: ganha os empates da ordem 0 e nada mais muda (ordem 1+ — NPCs, portões, mastros, inimigos,
    /// primeiro plano — continua na frente; negativas, atrás). A raiz, o colisor e o respawn não mexem (a física 2D ignora z).
    /// Descartados: altar na ordem −2 (muros e cais do Prólogo o cobriam) e jogador na ordem 1 (empataria com NPCs e portões).
    /// </summary>
    private static bool CorpoNaFrente()
    {
        const string caminho = "Assets/Prefabs/Player.prefab";
        GameObject raiz = PrefabUtility.LoadPrefabContents(caminho);
        try
        {
            var corpo = raiz.GetComponentsInChildren<SpriteRenderer>(true).FirstOrDefault(r => r.name == "Body");
            if (corpo == null) { Debug.LogError("[Itens] Player sem Body"); return false; }
            Vector3 p = corpo.transform.localPosition;
            if (Mathf.Approximately(p.z, ZDoCorpo)) { Debug.Log("[Itens] Player: Body já na frente (z −0,05)"); return true; }
            corpo.transform.localPosition = new Vector3(p.x, p.y, ZDoCorpo);
            PrefabUtility.SaveAsPrefabAsset(raiz, caminho);
            Debug.Log("[Itens] Player: Body em z −0,05 (ganha os empates da ordem 0)");
            return true;
        }
        finally { PrefabUtility.UnloadPrefabContents(raiz); }
    }

    /// <summary>Distância do sprite do jogador para a câmera, em unidades: pequena, só para desempatar.</summary>
    public const float ZDoCorpo = -0.05f;

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
