using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// N-07 do Final Polish — escudo do Odisseu (Docs/Characters/Shield/SHIELD_ART.md):
///
/// Unity.exe -batchmode -quit -projectPath . -executeMethod ShieldArtFix.Shots -shieldShots antes|depois
/// Unity.exe -batchmode -nographics -quit -projectPath . -executeMethod ShieldArtFix.Apply
///
/// A auditoria achou que o ESCUDO JÁ EXISTE: o master do Odisseu desenha um escudo hoplita redondo de bronze
/// nos estados Shield (6 quadros) e ShieldHold (4), que o PlayerAnimator toca enquanto PlayerShield.IsBlocking,
/// no chão. O ShieldVisual — PlaceholderSquare azul, 0,16 × 0,85 un, ordem 2 — era desenhado POR CIMA desse
/// escudo, em todas as 16 fases.
///
/// Apply desliga só o SpriteRenderer do ShieldVisual no prefab Player. O objeto, a referência no PlayerShield,
/// o SetActive(blocking), a posição, a escala e a ordem 2 ficam como estão: nenhuma lógica muda, e se um dia
/// houver arte própria para a defesa no ar, basta dar um sprite ao renderer e religá-lo.
/// </summary>
public static class ShieldArtFix
{
    private const string Prefab = "Assets/Prefabs/Player.prefab";
    private const string Cena = "Assets/Scenes/Levels/Level_03_Cicones.unity";

    public static void Apply()
    {
        GameObject raiz = PrefabUtility.LoadPrefabContents(Prefab);
        Transform sv = raiz.transform.Find("Visual/ShieldVisual");
        var sr = sv != null ? sv.GetComponent<SpriteRenderer>() : null;
        if (sr == null) { Debug.LogError("[Shield] Visual/ShieldVisual sem SpriteRenderer"); PrefabUtility.UnloadPrefabContents(raiz); EditorApplication.Exit(1); return; }
        Debug.Log($"[Shield] ShieldVisual: pos {sv.localPosition} escala {sv.localScale} sprite {sr.sprite.name} ppu {sr.sprite.pixelsPerUnit} " +
                  $"ordem {sr.sortingOrder} layer {sr.sortingLayerName} cor {sr.color} ativo {sv.gameObject.activeSelf} renderer {sr.enabled}");
        sr.enabled = false;
        PrefabUtility.SaveAsPrefabAsset(raiz, Prefab);
        PrefabUtility.UnloadPrefabContents(raiz);
        Debug.Log("[Shield] renderer do ShieldVisual desligado no prefab Player");
        EditorApplication.Exit(0);
    }

    /// <summary>
    /// Odisseu parado, erguendo o escudo, em guarda, e defendendo no ar — com o ShieldVisual ligado como o
    /// PlayerShield o liga na defesa. Sem salvar a cena.
    /// </summary>
    public static void Shots()
    {
        string[] a = System.Environment.GetCommandLineArgs();
        int i = System.Array.IndexOf(a, "-shieldShots");
        if (i < 0) { Debug.LogError("[Shield] falta -shieldShots antes|depois"); EditorApplication.Exit(1); return; }
        string sufixo = a[i + 1];
        const string pasta = "Docs/Characters/Shield/_capturas/";
        System.IO.Directory.CreateDirectory(pasta);

        EditorSceneManager.OpenScene(Cena, OpenSceneMode.Single);
        GameObject jogador = GameObject.Find("Player");
        var corpo = jogador.transform.Find("Visual/Body").GetComponent<SpriteRenderer>();
        var escudo = jogador.transform.Find("Visual/ShieldVisual").gameObject;
        var quadros = new System.Collections.Generic.Dictionary<string, Sprite>();
        foreach (Object o in AssetDatabase.LoadAllAssetsAtPath("Assets/Resources/Odisseia/Characters/CHR_Odysseus.png"))
        {
            if (o is Sprite s) { quadros[s.name] = s; }
        }
        float x = jogador.transform.position.x;
        foreach (var (quadro, defendendo, nome) in new[]
        {
            ("Idle_00", false, "parado"),
            ("Run_03", false, "movimento"),
            ("Shield_03", true, "erguendo"),
            ("ShieldHold_00", true, "defesa"),
            ("Jump_01", true, "defesa_no_ar"),
        })
        {
            corpo.sprite = quadros["CHR_Odysseus_" + quadro];
            escudo.SetActive(defendendo);   // o que o PlayerShield faz com SetActive(blocking)
            PrologueScreenshot.Render(x + 2f, 0f, pasta + $"{nome}_{sufixo}.png");
        }
        EditorApplication.Exit(0);
    }
}
