using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Odisseia.Enemies;
using Odisseia.Levels;
using Odisseia.Systems;

/// <summary>
/// Elenco da Fase 07 — Lestrigões (Docs/Characters/Fase07/LESTRIGONS_CAST.md):
///
/// Unity.exe -batchmode -nographics -quit -projectPath . -executeMethod LestrigonCastDresser.Dress
/// Unity.exe -batchmode -quit -projectPath . -executeMethod LestrigonCastDresser.Poses   (fotos, sem salvar)
///
/// Três Lestrigões em jogo, um master (CHR_Lestrigon_Warrior):
/// - dois ARREMESSADORES (BossController "Giant"), placeholders de quadrados flutuando 3 un acima do chão:
///   vestidos pelo BossArtDresser (o mesmo do Polifemo) — Idle, Telegraph (erguer a pedra), Attack.
/// - um PERSEGUIDOR (PursuerHazard) que o LestrigoesSceneDresser deixou INVISÍVEL ao desligar os
///   placeholders: uma parede que mata ao toque sem nada desenhado. Ganha o gigante correndo, como
///   filho — o PursuerHazard, o colisor e a velocidade não mudam.
///
/// Idempotente.
/// </summary>
public static class LestrigonCastDresser
{
    private const string ScenePath = "Assets/Scenes/Levels/Level_07_Lestrigoes.unity";
    private const string Folha = "Odisseia/Enemies/CHR_Lestrigon_Warrior";
    private const float TopoDoChao = -2f;   // o mesmo do LestrigoesSceneDresser e dos pontos de ataque
    private const string ArteDoPerseguidor = "Art_Lestrigon";

    public static void Dress()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        int chefes = 0;
        foreach (BossController boss in Object.FindObjectsByType<BossController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            // Olham para a esquerda: o Odisseu vem de lá, com o perseguidor atrás dele.
            // Telegraph: 6 quadros a 9 FPS = 0,67 s, dentro do aviso de 0,7 s; segura a pedra no alto.
            var sr = BossArtDresser.Vestir(boss, Folha, TopoDoChao, true, new[] {
                new BossArtDresser.Estado("Idle", 5f, true),
                new BossArtDresser.Estado("Telegraph", 9f, false),
                new BossArtDresser.Estado("Attack", 10f, false),
            }, "Head");
            if (sr == null) { EditorApplication.Exit(1); return; }
            chefes++;
            Debug.Log($"[LestrigonCast] arremessador em x={boss.transform.position.x:0.0}, desenho {sr.bounds.size.x:0.00} x {sr.bounds.size.y:0.00} un");
        }

        PursuerHazard perseguidor = Object.FindFirstObjectByType<PursuerHazard>(FindObjectsInactive.Include);
        Sprite correndo = null;
        foreach (Object a in AssetDatabase.LoadAllAssetsAtPath("Assets/Resources/" + Folha + ".png"))
        {
            if (a is Sprite s && s.name == "CHR_Lestrigon_Warrior_Run_00") { correndo = s; }
        }
        if (perseguidor == null || correndo == null)
        {
            Debug.LogError("[LestrigonCast] perseguidor ou quadro Run não encontrados");
            EditorApplication.Exit(1);
            return;
        }

        Transform antiga = perseguidor.transform.Find(ArteDoPerseguidor);
        if (antiga != null) { Object.DestroyImmediate(antiga.gameObject); }
        var arte = new GameObject(ArteDoPerseguidor);
        arte.transform.SetParent(perseguidor.transform, false);
        // O PursuerHazard tem escala 3 x 4 — o placeholder era um quadrado esticado, e o colisor vem
        // dessa escala, então ela não pode mudar. O filho a anula: sem isso o gigante media 17,6 un
        // (o CastProbe acusou "ESCALA!").
        Vector3 herdada = perseguidor.transform.lossyScale;
        arte.transform.localScale = new Vector3(1f / herdada.x, 1f / herdada.y, 1f);
        // Centrado no colisor (3 x 4 un), pés no chão: o pivô é a linha dos pés.
        arte.transform.position = new Vector3(perseguidor.transform.position.x, TopoDoChao, 0f);
        var rp = arte.AddComponent<SpriteRenderer>();
        rp.sprite = correndo;
        rp.flipX = false;       // corre para a direita, atrás do jogador (a arte já olha para a direita)
        rp.sortingOrder = 1;    // atrás do jogador (2): quando o alcança, o jogador fica à frente dele
        var animador = arte.AddComponent<SpriteAnimator>();
        var so = new SerializedObject(animador);
        so.FindProperty("resourcePath").stringValue = Folha;
        so.FindProperty("defaultState").stringValue = "Run";   // ele nunca para: Run é o único estado
        so.FindProperty("defaultFramesPerSecond").floatValue = 10f;
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[LestrigonCast] {chefes} arremessadores vestidos; perseguidor em x={perseguidor.transform.position.x:0.0} com o gigante correndo.");
        EditorApplication.Exit(chefes == 2 ? 0 : 1);
    }

    /// <summary>Arremessador em cada pose com o Odisseu diante dele, e o perseguidor atrás do Odisseu — sem salvar.</summary>
    public static void Poses()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var quadros = new System.Collections.Generic.Dictionary<string, Sprite>();
        foreach (Object a in AssetDatabase.LoadAllAssetsAtPath("Assets/Resources/" + Folha + ".png"))
        {
            if (a is Sprite s) { quadros[s.name] = s; }
        }
        BossController primeiro = null;
        foreach (BossController b in Object.FindObjectsByType<BossController>(FindObjectsSortMode.None))
        {
            if (primeiro == null || b.transform.position.x < primeiro.transform.position.x) { primeiro = b; }
        }
        var sr = primeiro.transform.Find("Body").GetComponent<SpriteRenderer>();
        GameObject jogador = GameObject.Find("Player");
        var corpoDoJogador = jogador.transform.Find("Visual/Body");
        void PorJogador(float x) =>
            jogador.transform.position += new Vector3(x - jogador.transform.position.x, TopoDoChao - corpoDoJogador.position.y, 0f);

        const string pasta = "Docs/Characters/Fase07/_capturas/";
        System.IO.Directory.CreateDirectory(pasta);
        PorJogador(primeiro.transform.position.x - 3.2f);
        foreach (var (quadro, nome) in new[] { ("Idle_00", "repouso"), ("Telegraph_05", "aviso"), ("Attack_01", "arremesso") })
        {
            sr.sprite = quadros["CHR_Lestrigon_Warrior_" + quadro];
            PrologueScreenshot.Render(primeiro.transform.position.x - 1.5f, 0f, pasta + "lestrigon_" + nome + ".png");
        }

        // Perseguidor alcançando o Odisseu.
        PursuerHazard perseguidor = Object.FindFirstObjectByType<PursuerHazard>();
        PorJogador(perseguidor.transform.position.x + 3.5f);
        PrologueScreenshot.Render(perseguidor.transform.position.x + 2f, 0f, pasta + "perseguidor.png");
        EditorApplication.Exit(0);
    }
}
