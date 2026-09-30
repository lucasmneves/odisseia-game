using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Odisseia.Enemies;
using Odisseia.Systems;

/// <summary>
/// Elenco da Fase 05 — Ciclopes (Docs/Characters/Fase05/CYCLOPES_CAST.md):
///
/// Unity.exe -batchmode -nographics -quit -projectPath . -executeMethod CyclopsCastDresser.Dress
///
/// O CastProbe achou só o Odisseu e o Polifemo — e o Polifemo era placeholder: três quadrados
/// (corpo, cabeça, olho) flutuando 3 un acima do chão da câmara, sobre a fogueira. Este script
/// troca o DESENHO e liga a animação. Não toca no BossController (tempo, dano, pontos de ataque),
/// no HealthSystem nem em colisor (o chefe não tem nenhum). O objeto é da cena, não de prefab.
///
/// Idempotente.
/// </summary>
public static class CyclopsCastDresser
{
    private const string ScenePath = "Assets/Scenes/Levels/Level_05_Ciclopes.unity";
    private const string Folha = "Odisseia/Enemies/CHR_Polyphemus";
    private const float TopoDoChao = -2f;   // o mesmo do CiclopesSceneDresser e dos pontos de ataque

    public static void Dress()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        BossController boss = Object.FindFirstObjectByType<BossController>(FindObjectsInactive.Include);
        if (boss == null) { Debug.LogError("[CyclopsCast] chefe não encontrado"); EditorApplication.Exit(1); return; }

        // Olha para a esquerda (é de lá que o Odisseu entra na câmara). Cabeça e olho eram quadrados.
        // Telegraph: 6 quadros em ~0,85 s, dentro do aviso de 0,9 s; segura o último (clava no alto).
        // Attack: 2 quadros rápidos; o BossAnimator segura o impacto e volta ao Idle.
        var sr = BossArtDresser.Vestir(boss, Folha, TopoDoChao, true, new[] {
            new BossArtDresser.Estado("Idle", 5f, true),
            new BossArtDresser.Estado("Telegraph", 7f, false),
            new BossArtDresser.Estado("Attack", 10f, false),
        }, "Head", "Eye");
        if (sr == null) { EditorApplication.Exit(1); return; }

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[CyclopsCast] Polifemo em x={boss.transform.position.x:0.0}, pés em y={TopoDoChao}, " +
                  $"desenho {sr.bounds.size.x:0.00} x {sr.bounds.size.y:0.00} un; BossAnimator ligado.");
        EditorApplication.Exit(0);
    }

    /// <summary>
    /// Fotografa o chefe em cada pose, com o Odisseu no chão diante dele — sem salvar a cena
    /// (sem -nographics, ou a imagem sai preta):
    ///
    /// Unity.exe -batchmode -quit -projectPath . -executeMethod CyclopsCastDresser.Poses
    ///
    /// A captura comum só vê o estado de edição (Idle_00 e o jogador no ponto de nascimento).
    /// </summary>
    public static void Poses()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        BossController boss = Object.FindFirstObjectByType<BossController>(FindObjectsInactive.Include);
        var sr = boss.transform.Find("Body").GetComponent<SpriteRenderer>();
        var quadros = new System.Collections.Generic.Dictionary<string, Sprite>();
        foreach (Object a in AssetDatabase.LoadAllAssetsAtPath("Assets/Resources/" + Folha + ".png"))
        {
            if (a is Sprite s) { quadros[s.name] = s; }
        }

        // Odisseu no chão, entre o primeiro ponto de ataque (x=26) e o chefe.
        GameObject jogador = GameObject.Find("Player");
        var corpoDoJogador = jogador.transform.Find("Visual/Body");
        jogador.transform.position += new Vector3(27.2f - jogador.transform.position.x, TopoDoChao - corpoDoJogador.position.y, 0f);

        const string pasta = "Docs/Characters/Fase05/_capturas/";
        foreach (var (quadro, nome) in new[] { ("CHR_Polyphemus_Idle_00", "repouso"), ("CHR_Polyphemus_Telegraph_05", "aviso"), ("CHR_Polyphemus_Attack_01", "golpe") })
        {
            sr.sprite = quadros[quadro];
            PrologueScreenshot.Render(28.5f, 0f, pasta + "polifemo_" + nome + ".png");
        }
        EditorApplication.Exit(0);   // cena não é salva: as poses só existem na foto
    }
}
