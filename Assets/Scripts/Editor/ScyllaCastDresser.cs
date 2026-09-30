using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Odisseia.Enemies;

/// <summary>
/// Elenco da Fase 11 — Cila e Caríbdis (Docs/Characters/Fase11/CILA_CARIBDIS_CAST.md):
///
/// Unity.exe -batchmode -nographics -quit -projectPath . -executeMethod ScyllaCastDresser.Dress
/// Unity.exe -batchmode -quit -projectPath . -executeMethod ScyllaCastDresser.Poses   (fotos, sem salvar)
///
/// Cila é o BossController "Giant" que o CilaCaribdisSceneDresser deixou como marcador, com a falésia e
/// a caverna escavadas em volta dele. Vestida pelo BossArtDresser (o mesmo do Polifemo e dos Lestrigões).
/// Caríbdis é ENVIRONMENT (FX_Charybdis, do cenário) e não é tocada aqui.
///
/// Idempotente.
/// </summary>
public static class ScyllaCastDresser
{
    private const string ScenePath = "Assets/Scenes/Levels/Level_11_CilaCaribdis.unity";
    private const string Folha = "Odisseia/Enemies/CHR_Scylla";
    // A Cila NÃO desce ao chão: ela sai da caverna, acima do estreito, e as cabeças descem até o convés.
    // A base foi MEDIDA, e a janela é estreita:
    // - o vão escuro da caverna começa em y ≈ 3,5 (fração de pixels escuros por linha na captura);
    // - a câmera desta fase tem tamanho 6 e segue o jogador com +1 un: com o Odisseu no convés
    //   (y ≈ -2), o topo da tela fica em y ≈ 5,0.
    // A base do placeholder (1,0) punha o topo da Cila em 5,46 — cabeça cortada em jogo — e o corpo quase
    // todo ABAIXO da caverna, flutuando na frente das colunas. Em 0,55 o topo fica em 5,0: cabeça e ombros
    // dentro do vão escuro, serpentes descendo pela boca da caverna em direção ao convés.
    private const float PisoDaCaverna = 0.55f;

    public static void Dress()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        BossController boss = Object.FindFirstObjectByType<BossController>(FindObjectsInactive.Include);
        if (boss == null) { Debug.LogError("[ScyllaCast] chefe não encontrado"); EditorApplication.Exit(1); return; }

        // Olha para a esquerda: o Odisseu entra no estreito por lá.
        // Telegraph: 4 quadros a 6 FPS = 0,67 s, dentro do aviso de 0,7 s; segura as cabeças no alto.
        var sr = BossArtDresser.Vestir(boss, Folha, PisoDaCaverna, true, new[] {
            new BossArtDresser.Estado("Idle", 5f, true),
            new BossArtDresser.Estado("Telegraph", 6f, false),
            new BossArtDresser.Estado("Attack", 10f, false),
        }, "Head");
        if (sr == null) { EditorApplication.Exit(1); return; }
        // Atrás dos destroços e das pedras de primeiro plano, na frente da falésia (-24).
        sr.sortingOrder = -20;

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[ScyllaCast] Cila em x={boss.transform.position.x:0.0}, base y={PisoDaCaverna}, " +
                  $"desenho {sr.bounds.size.x:0.00} x {sr.bounds.size.y:0.00} un.");
        EditorApplication.Exit(0);
    }

    /// <summary>Cila em repouso, no aviso e no golpe, com o Odisseu embaixo — sem salvar.</summary>
    public static void Poses()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var quadros = new System.Collections.Generic.Dictionary<string, Sprite>();
        foreach (Object a in AssetDatabase.LoadAllAssetsAtPath("Assets/Resources/" + Folha + ".png"))
        {
            if (a is Sprite s) { quadros[s.name] = s; }
        }
        BossController boss = Object.FindFirstObjectByType<BossController>();
        var sr = boss.transform.Find("Body").GetComponent<SpriteRenderer>();
        GameObject jogador = GameObject.Find("Player");
        var corpo = jogador.transform.Find("Visual/Body");
        jogador.transform.position += new Vector3(boss.transform.position.x - 3f - jogador.transform.position.x,
            -2f - corpo.position.y, 0f);
        const string pasta = "Docs/Characters/Fase11/_capturas/";
        System.IO.Directory.CreateDirectory(pasta);
        foreach (var (q, nome) in new[] { ("Idle_00", "repouso"), ("Telegraph_03", "aviso"), ("Attack_03", "golpe") })
        {
            sr.sprite = quadros["CHR_Scylla_" + q];
            PrologueScreenshot.Render(boss.transform.position.x - 1f, 0f, pasta + "cila_" + nome + ".png");
        }
        EditorApplication.Exit(0);
    }
}
