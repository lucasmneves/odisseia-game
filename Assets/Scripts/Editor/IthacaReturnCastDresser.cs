using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Odisseia.Systems;

/// <summary>
/// Elenco das Fases 14 (Ítaca Return) e 15 (Pretendentes) — Docs/Characters/Fase14/ITHACA_RETURN_CAST.md e
/// Docs/Characters/Fase15/FINAL_CAST.md:
///
/// Unity.exe -batchmode -nographics -quit -projectPath . -executeMethod IthacaReturnCastDresser.Dress14
/// Unity.exe -batchmode -nographics -quit -projectPath . -executeMethod IthacaReturnCastDresser.Dress15
/// Unity.exe -batchmode -quit -projectPath . -executeMethod IthacaReturnCastDresser.Poses14   (fotos, sem salvar)
/// Unity.exe -batchmode -quit -projectPath . -executeMethod IthacaReturnCastDresser.Poses15
///
/// Tudo é SCENE OVERRIDE:
/// - NPCs (NPCDialogue) saem da arte pintada (123–140 px/un) para os masters v3: sprite do Body + SpriteAnimator
///   no Idle, pés no chão, olhando para a esquerda (de onde o jogador chega). O NPCDialogue e o colisor dele não mudam.
/// - Os EnemyBasic carmesim viram PRETENDENTES por EnemyFactionOverride — o prefab fica intacto.
/// - Ordem de desenho: o Body do jogador está na ordem 0 (prefab). NPCs vão a -1 — atrás dele, sem empate. O dresser
///   LISTA o cenário grande que se desenha por cima do jogador (ordem 0..9), como a jangada da Fase 13.
///
/// Idempotente.
/// </summary>
public static class IthacaReturnCastDresser
{
    private const string Cena14 = "Assets/Scenes/Levels/Level_14_Itaca_Return.unity";
    private const string Cena15 = "Assets/Scenes/Levels/Level_15_Pretendentes.unity";
    private const string Npcs = "Odisseia/Characters/NPCs/";
    private const string Pretendente = "Odisseia/Enemies/CHR_Suitor";
    private const float TopoDoChao = -2f;
    // Ordem do Body do jogador no prefab Player (medida pelo GlobalCastAudit nas 16 cenas). Até o Final Polish
    // este dresser supunha 2 — errado: o 2 do prefab é o ShieldVisual.
    private const int PlanoDoJogador = 0;

    public static void Dress14() => Vestir(Cena14, "ItacaReturnCast",
        ("NPC_Eumeu", "CHR_Eumaeus"), ("NPC_Telemaco", "CHR_Telemachus_Adult"));

    public static void Dress15() => Vestir(Cena15, "PretendentesCast",
        ("NPC_Penelope", "CHR_Penelope"), ("NPC_Telemaco", "CHR_Telemachus_Adult"));

    private static void Vestir(string cena, string tag, params (string npc, string folha)[] elenco)
    {
        EditorSceneManager.OpenScene(cena, OpenSceneMode.Single);
        int inimigos = EnemyFactionOverride.Aplicar(Pretendente, tag);
        if (inimigos <= 0) { EditorApplication.Exit(1); return; }

        foreach (var (npc, folha) in elenco)
        {
            GameObject go = GameObject.Find(npc);
            Transform corpo = go != null ? go.transform.Find("Body") : null;
            Sprite parada = Parada(Npcs + folha);
            if (corpo == null || parada == null)
            {
                Debug.LogError($"[{tag}] {npc}/Body ou folha {folha} não encontrados");
                EditorApplication.Exit(1);
                return;
            }

            var sr = corpo.GetComponent<SpriteRenderer>();
            sr.sprite = parada;
            sr.flipX = true;          // os masters olham para a direita; o jogador chega pela esquerda
            sr.color = Color.white;
            sr.sortingOrder = PlanoDoJogador - 1;   // atrás do jogador, sem empate; na frente do cenário de fundo
            corpo.localScale = Vector3.one;
            // Pés no chão: a caixa do sprite começa nos pés (pivô BottomCenter, pés na última linha).
            corpo.position += new Vector3(0f, TopoDoChao - sr.bounds.min.y, 0f);

            var animador = corpo.GetComponent<SpriteAnimator>() ?? corpo.gameObject.AddComponent<SpriteAnimator>();
            var so = new SerializedObject(animador);
            so.FindProperty("resourcePath").stringValue = Npcs + folha;
            so.FindProperty("defaultState").stringValue = "Idle";
            so.FindProperty("defaultFramesPerSecond").floatValue = 5.5f + Mathf.Repeat(go.transform.position.x * 0.37f, 1.2f);
            so.ApplyModifiedPropertiesWithoutUndo();
            Debug.Log($"[{tag}] {npc} = {folha}: x {sr.bounds.min.x:0.00}..{sr.bounds.max.x:0.00} y {sr.bounds.min.y:0.00}..{sr.bounds.max.y:0.00}");
        }

        LiberarNpcs(tag, elenco.Select(e => GameObject.Find(e.npc).transform.Find("Body").GetComponent<SpriteRenderer>()).ToArray());
        if (!OrdemSemEmpate(tag)) { EditorApplication.Exit(1); return; }

        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[{tag}] {inimigos} pretendentes; {elenco.Length} NPCs.");
        EditorApplication.Exit(0);
    }

    /// <summary>
    /// Primeiro plano (ordem ≥ 10) sobre um NPC parado o enterra: o dresser de cenário da 14 pôs as touceiras
    /// FG_Grass_1 e FG_Grass_3 exatamente em x = 0 e 28, onde estão Eumeu e Telêmaco, cobrindo-os até o joelho.
    /// O NPC não pode sair do lugar (o colisor do diálogo está inteiro sob a grama), então a GRAMA anda: o mínimo
    /// para o lado até liberar o corpo com folga. Override de cena; o dresser de cenário não é tocado.
    /// </summary>
    private static void LiberarNpcs(string tag, SpriteRenderer[] corpos)
    {
        const float Folga = 0.3f;
        var raizes = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (SpriteRenderer fg in raizes.SelectMany(r => r.GetComponentsInChildren<SpriteRenderer>(true))
            .Where(s => s.sortingOrder >= 10 && s.enabled && s.sprite != null && s.bounds.size.x < 10f))
        {
            foreach (SpriteRenderer npc in corpos)
            {
                Bounds f = fg.bounds, n = npc.bounds;
                if (f.max.x <= n.min.x - Folga + 0.001f || f.min.x >= n.max.x + Folga - 0.001f || f.max.y <= n.min.y) { continue; }
                float paraEsquerda = (n.min.x - Folga) - f.max.x;   // negativo
                float paraDireita = (n.max.x + Folga) - f.min.x;    // positivo
                float passo = -paraEsquerda <= paraDireita ? paraEsquerda : paraDireita;
                fg.transform.position += new Vector3(passo, 0f, 0f);
                Debug.Log($"[{tag}] {fg.name} (ordem {fg.sortingOrder}) cobria {npc.transform.parent.name}: movido {passo:+0.00;-0.00} un, " +
                          $"agora x {fg.bounds.min.x:0.00}..{fg.bounds.max.x:0.00}");
            }
        }
    }

    /// <summary>
    /// Cenário grande (1 un ou mais de altura) entre o plano do jogador e o primeiro plano (>= 10) se desenha POR
    /// CIMA do Odisseu — foi a jangada da Fase 13. Só lista (profundidade pode ser intencional); não falha.
    /// </summary>
    private static bool OrdemSemEmpate(string tag)
    {
        var raizes = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        var sobre = raizes.SelectMany(r => r.GetComponentsInChildren<SpriteRenderer>(true))
            .Where(s => s.sortingOrder >= PlanoDoJogador && s.sortingOrder < 10 && s.enabled && s.sprite != null
                && s.bounds.size.y >= 1f && s.bounds.min.y < 0f
                && s.GetComponentInParent<Odisseia.Enemies.EnemyController>(true) == null
                && s.GetComponentInParent(System.Type.GetType("Odisseia.Player.PlayerController, Assembly-CSharp"), true) == null)
            .Select(s => $"{s.name} (ordem {s.sortingOrder})").ToList();
        Debug.Log($"[{tag}] cenário grande por cima do jogador (ordem {PlanoDoJogador}..9): " + (sobre.Count == 0 ? "nenhum" : string.Join(", ", sobre)));
        return true;
    }

    public static void Poses14() => Poses(Cena14, "Fase14",
        ("NPC_Eumeu", "odisseu_e_eumeu"), ("NPC_Telemaco", "odisseu_e_telemaco"));

    public static void Poses15() => Poses(Cena15, "Fase15",
        ("NPC_Penelope", "odisseu_e_penelope"), ("NPC_Telemaco", "odisseu_e_telemaco"));

    /// <summary>Odisseu no início, diante de cada NPC, diante de um pretendente, e o fim — sem salvar.</summary>
    private static void Poses(string cena, string fase, params (string npc, string foto)[] encontros)
    {
        EditorSceneManager.OpenScene(cena, OpenSceneMode.Single);
        GameObject jogador = GameObject.Find("Player");
        var corpo = jogador.transform.Find("Visual/Body");
        string pasta = $"Docs/Characters/{fase}/_capturas/";
        System.IO.Directory.CreateDirectory(pasta);
        // Só o x muda: a altura é a de nascimento (a caixa do sprite tem margem transparente abaixo dos pés).
        void Por(float x) => jogador.transform.position += new Vector3(x - corpo.position.x, 0f, 0f);

        PrologueScreenshot.Render(corpo.position.x + 3f, 0f, pasta + "odisseu_inicio.png");
        foreach (var (npc, foto) in encontros)
        {
            float x = GameObject.Find(npc).transform.position.x;
            Por(x - 1.6f);
            PrologueScreenshot.Render(x - 0.5f, 0f, pasta + foto + ".png");
        }
        var inimigo = Object.FindObjectsByType<Odisseia.Enemies.EnemyController>(FindObjectsSortMode.None)
            .OrderBy(e => e.transform.position.x).First();
        Por(inimigo.transform.position.x - 2.2f);
        PrologueScreenshot.Render(inimigo.transform.position.x - 1f, 0f, pasta + "odisseu_e_pretendente.png");
        GameObject objetivo = GameObject.Find("LevelGoal");
        Por(objetivo.transform.position.x - 1.5f);
        PrologueScreenshot.Render(objetivo.transform.position.x - 3f, 0f, pasta + "fim_da_fase.png");
        EditorApplication.Exit(0);
    }

    private static Sprite Parada(string folha)
    {
        string nome = folha.Substring(folha.LastIndexOf('/') + 1) + "_Idle_00";
        return AssetDatabase.LoadAllAssetsAtPath("Assets/Resources/" + folha + ".png").OfType<Sprite>().FirstOrDefault(s => s.name == nome);
    }
}
