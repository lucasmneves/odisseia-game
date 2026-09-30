using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Odisseia.Enemies;
using Odisseia.Levels;
using Odisseia.Systems;

/// <summary>
/// FINAL CHARACTER ART POLISH (Docs/Characters/FINAL_CHARACTER_ART_POLISH.md) — as correções de CENA:
///
/// Unity.exe -batchmode -nographics -quit -projectPath . -executeMethod FinalPolishDresser.Dress
/// Unity.exe -batchmode -quit -projectPath . -executeMethod FinalPolishDresser.Shots -polishShots antes|depois
///
/// Tudo é SCENE OVERRIDE, idempotente, rodando DEPOIS dos dressers de cenário e de elenco de cada fase. Nenhum
/// prefab, script de jogo, colisor de gameplay ou master é alterado. Os assets refeitos por código (Caríbdis,
/// peças do palácio de Éolo, árvore da Fase 12) vêm de Tools/ e só são LIGADOS aqui.
///
/// Ordem de desenho: o Body do jogador está na ordem 0 em todas as cenas (prefab Player). Personagem parado
/// empatado com ele vai a -1; cenário grande que o cobre num ponto de parada desce para trás dele.
/// </summary>
public static class FinalPolishDresser
{
    private const string Levels = "Assets/Scenes/Levels/";
    private const float TopoDoChao = -2f;
    private const int OrdemDoJogador = 0;

    public static void Dress()
    {
        bool ok = true;
        ok &= Cena("Level_01_Itaca_Prologue", Fase01);
        ok &= Cena("Level_02_Troia", () => DesempatarPersonagens("TroyCast"));
        ok &= Cena("Level_04_Citera", () => DesempatarPersonagens("CyteraCast"));
        ok &= Cena("Level_06_Eolo", Fase06);
        ok &= Cena("Level_08_Circe", () => DesempatarPersonagens("CirceCast"));
        ok &= Cena("Level_09_MundoDosMortos", () => DesempatarPersonagens("UnderworldCast"));
        ok &= Cena("Level_11_CilaCaribdis", Fase11);
        ok &= Cena("Level_12_GadoDoSol", Fase12);
        ok &= Cena("Level_13_Calipso", Fase13);
        EditorApplication.Exit(ok ? 0 : 1);
    }

    private static bool Cena(string nome, System.Func<bool> corrigir)
    {
        EditorSceneManager.OpenScene(Levels + nome + ".unity", OpenSceneMode.Single);
        bool ok = corrigir();
        if (!ok) { Debug.LogError($"[Polish] {nome}: FALHOU — cena não salva"); return false; }
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[Polish] {nome}: salva");
        return true;
    }

    // ------------------------------------------------------------------ P-08 — Fase 01

    /// <summary>Os parceiros de treino (EnemyBasic carmesim pintado) viram soldados de Ítaca.</summary>
    private static bool Fase01()
    {
        int n = EnemyFactionOverride.Aplicar("Odisseia/Enemies/CHR_Ithaca_Sparring", "Polish");
        Debug.Log($"[Polish] P-08: {n} parceiros de treino vestidos de soldado de Ítaca");
        return n > 0;
    }

    // ------------------------------------------------------------------ P-03 — Fase 06

    /// <summary>
    /// Peças do palácio encolhidas no Transform (0,52–0,70) trocadas pelas versões reamostradas para a densidade
    /// nativa (Tools/downscale-native.js), em escala 1 — mesmo lugar, mesmo tamanho de tela, pixel do tamanho do
    /// resto do jogo. O entablamento (ESTICADO a 2,0) não tem versão nativa: ampliar não cria detalhe.
    /// </summary>
    private static bool Fase06()
    {
        const string P = "Assets/Art/Environments/Eolo/";
        var troca = new Dictionary<string, string>
        {
            { "eolo_column", P + "Palace/eolo_column_native.png" },
            { "eolo_bronze_door", P + "Palace/eolo_bronze_door_native.png" },
            { "eolo_steps", P + "Palace/eolo_steps_native.png" },
            { "eolo_brazier", P + "Palace/eolo_brazier_native.png" },
            { "eolo_statue", P + "Palace/eolo_statue_native.png" },
            { "eolo_banner", P + "Props/eolo_banner_native.png" },
        };
        int n = 0;
        foreach (var sr in Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .Where(s => s.name.StartsWith("Palace_") && s.sprite != null && troca.ContainsKey(s.sprite.name)))
        {
            Sprite nativo = AssetDatabase.LoadAssetAtPath<Sprite>(troca[sr.sprite.name]);
            if (nativo == null) { Debug.LogError("[Polish] P-03: sem " + troca[sr.sprite.name]); return false; }
            float k = sr.transform.localScale.y;
            Vector3 antesMin = sr.bounds.min, antesMax = sr.bounds.max;
            if (sr.drawMode == SpriteDrawMode.Tiled) { sr.size *= k; }
            sr.sprite = nativo;
            sr.transform.localScale = new Vector3(Mathf.Sign(sr.transform.localScale.x), 1f, 1f);
            n++;
            Debug.Log($"[Polish] P-03: {sr.name} escala {k:0.##} -> 1 ({nativo.name}); x {antesMin.x:0.00}..{antesMax.x:0.00} -> " +
                      $"{sr.bounds.min.x:0.00}..{sr.bounds.max.x:0.00}, y {antesMin.y:0.00}..{antesMax.y:0.00} -> {sr.bounds.min.y:0.00}..{sr.bounds.max.y:0.00}");
        }
        return n > 0;
    }

    // ------------------------------------------------------------------ P-01 e P-02 — Fase 11

    private static bool Fase11()
    {
        // P-02 — Caríbdis: a folha agora sai na densidade nativa (Tools/build-cila-fx.js). A escala volta a ser a
        // do CilaCaribdisSceneDresser — largura do marcador × 1,45 sobre a largura do quadro —, que dá ~1.
        GameObject arte = GameObject.Find("CilaCaribdisScenery/Charybdis_Art");
        GameObject marcador = GameObject.Find("CharybdisWhirlpool");
        Sprite quadro = Resources.LoadAll<Sprite>("Odisseia/Environments/FX_Charybdis").FirstOrDefault(s => s.name.EndsWith("_00"));
        if (arte == null || marcador == null || quadro == null) { Debug.LogError("[Polish] P-02: Caríbdis incompleta"); return false; }
        var sr = arte.GetComponent<SpriteRenderer>();
        float antes = arte.transform.localScale.x;
        sr.sprite = quadro;
        float k = marcador.GetComponent<Collider2D>().bounds.size.x * 1.45f / quadro.bounds.size.x;
        arte.transform.localScale = new Vector3(k, k, 1f);
        Debug.Log($"[Polish] P-02: Caríbdis escala {antes:0.###} -> {k:0.###}, quadro {quadro.rect.width}x{quadro.rect.height}, desenho x {sr.bounds.min.x:0.00}..{sr.bounds.max.x:0.00}");

        // P-01 — Cila: poleiro de basalto (o mesmo cila_crag_stack da fase) NA FRENTE da parte de baixo dela.
        // Os pés humanoides somem atrás da rocha e ela passa a estar SOBRE o rochedo — a Cila do mito —, em vez de
        // pairar no ar diante das colunas. A pilha nasce atrás do convés (ordem -19 < chão -10): não cobre nada de jogo.
        var boss = Object.FindFirstObjectByType<BossController>();
        var corpo = boss != null ? boss.transform.Find("Body")?.GetComponent<SpriteRenderer>() : null;
        Sprite pilha = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Environments/CilaCaribdis/Rocks/cila_crag_stack.png");
        if (corpo == null || pilha == null) { Debug.LogError("[Polish] P-01: Cila ou pilha não encontradas"); return false; }
        GameObject velho = GameObject.Find("FinalPolish_Cila");
        if (velho != null) { Object.DestroyImmediate(velho); }
        var raiz = new GameObject("FinalPolish_Cila");
        var poleiro = new GameObject("Scylla_Perch");
        poleiro.transform.SetParent(raiz.transform, false);
        var ps = poleiro.AddComponent<SpriteRenderer>();
        ps.sprite = pilha;
        ps.sortingOrder = corpo.sortingOrder + 1;   // Cila -20 -> pilha -19: à frente dela, atrás do convés e do jogador
        // Topo irregular (tampas hexagonais) na altura das COXAS: 0,85 un acima da base do desenho esconde pés e
        // canelas; torso, braços e as seis cabeças ficam livres.
        float topo = corpo.bounds.min.y + 0.85f;
        poleiro.transform.position = new Vector3(boss.transform.position.x, topo - pilha.bounds.size.y, 0f);
        Debug.Log($"[Polish] P-01: poleiro x {ps.bounds.min.x:0.00}..{ps.bounds.max.x:0.00}, topo y {ps.bounds.max.y:0.00}; Cila y {corpo.bounds.min.y:0.00}..{corpo.bounds.max.y:0.00}");
        return true;
    }

    // ------------------------------------------------------------------ P-05 — Fase 12

    /// <summary>
    /// O rebanho lia como FILA: sete bois do mesmo tamanho, na mesma linha, espaçados por igual e quase todos
    /// virados para o mesmo lado. Recomposto em grupos — dois pares sobrepostos e bois soltos, lados alternados,
    /// os de trás um pouco mais altos na faixa de grama (profundidade) e uma ordem atrás. Os dois bois interativos
    /// (SacredCattle, gameplay) NÃO se movem. E os arbustos de primeiro plano que cobriam os cascos deles saem.
    /// </summary>
    private static bool Fase12()
    {
        var plano = new (string nome, float x, bool virado, int ordem, float recuo)[]
        {
            ("Herd_7", 5.35f, false, -3, 0.10f),   // par com o boi interativo de x=4, atrás dele
            ("Herd_9", 9.60f, true, -2, 0f),       // solto, olhando para trás
            ("Herd_16", 14.30f, true, -3, 0.10f),  // par com o boi interativo de x=13
            ("Herd_17", 17.90f, false, -2, 0f),    // solto, perto do templo
        };
        foreach (var (nome, x, virado, ordem, recuo) in plano)
        {
            GameObject go = GameObject.Find("GadoDoSolScenery/" + nome);
            if (go == null) { Debug.LogError("[Polish] P-05: sem " + nome); return false; }
            go.transform.position = new Vector3(x, TopoDoChao - 0.05f + recuo, go.transform.position.z);
            var sr = go.GetComponent<SpriteRenderer>();
            sr.flipX = virado;
            sr.sortingOrder = ordem;
            Debug.Log($"[Polish] P-05: {nome} -> x {x:0.00}, {(virado ? "esquerda" : "direita")}, ordem {ordem}");
        }
        var bois = Object.FindObjectsByType<SacredCattleZone>(FindObjectsSortMode.None).Select(z => z.GetComponent<SpriteRenderer>()).ToArray();
        LiberarDoPrimeiroPlano(bois);
        return true;
    }

    // ------------------------------------------------------------------ P-07 — Fase 13

    private static bool Fase13()
    {
        // A jangada (ordem 2) se desenhava POR CIMA do Odisseu (ordem 0) — no gatilho da despedida ele sumia.
        // Vai a -1: atrás dele, na frente do palácio (-18) e das flores (-3).
        GameObject jangada = GameObject.Find("CalipsoScenery/Raft");
        if (jangada == null) { Debug.LogError("[Polish] P-07: jangada não encontrada"); return false; }
        var sr = jangada.GetComponent<SpriteRenderer>();
        Debug.Log($"[Polish] P-07: jangada ordem {sr.sortingOrder} -> {OrdemDoJogador - 1}");
        sr.sortingOrder = OrdemDoJogador - 1;
        return DesempatarPersonagens("CalypsoCast");
    }

    // ------------------------------------------------------------------ comuns

    /// <summary>Figura parada na ordem do jogador empata com ele: vai a -1, a menos que isso a empate com cenário.</summary>
    private static bool DesempatarPersonagens(string raiz)
    {
        GameObject r = GameObject.Find(raiz);
        if (r == null) { Debug.LogError("[Polish] sem " + raiz); return false; }
        var cenas = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects()
            .SelectMany(g => g.GetComponentsInChildren<SpriteRenderer>(true)).ToArray();
        foreach (var f in r.GetComponentsInChildren<SpriteRenderer>(true).Where(s => s.sortingOrder == OrdemDoJogador))
        {
            int alvo = OrdemDoJogador - 1;
            var colide = cenas.Where(s => s != f && s.enabled && s.sprite != null && s.sortingOrder == alvo
                && !s.transform.IsChildOf(r.transform) && s.bounds.Intersects(f.bounds)).Select(s => s.name).ToList();
            if (colide.Count > 0)
            {
                Debug.LogWarning($"[Polish] {f.transform.parent.name}: a ordem {alvo} empataria com {string.Join(", ", colide)} — mantida em {f.sortingOrder}");
                continue;
            }
            f.sortingOrder = alvo;
            Debug.Log($"[Polish] {raiz}/{f.transform.parent.name}: ordem {OrdemDoJogador} -> {alvo}");
        }
        return true;
    }

    /// <summary>Primeiro plano (ordem >= 10) sobre personagem parado anda o mínimo para o lado (D-048).</summary>
    private static void LiberarDoPrimeiroPlano(SpriteRenderer[] corpos)
    {
        const float Folga = 0.3f;
        foreach (SpriteRenderer fg in Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None)
            .Where(s => s.sortingOrder >= 10 && s.enabled && s.sprite != null && s.bounds.size.x < 10f))
        {
            foreach (SpriteRenderer c in corpos.Where(c => c != null))
            {
                Bounds f = fg.bounds, n = c.bounds;
                if (f.max.x <= n.min.x - Folga + 0.001f || f.min.x >= n.max.x + Folga - 0.001f || f.max.y <= n.min.y + 0.1f) { continue; }
                float esq = (n.min.x - Folga) - f.max.x, dir = (n.max.x + Folga) - f.min.x;
                float passo = -esq <= dir ? esq : dir;
                fg.transform.position += new Vector3(passo, 0f, 0f);
                Debug.Log($"[Polish] {fg.name} (ordem {fg.sortingOrder}) cobria {c.name}: movido {passo:+0.00;-0.00} un");
            }
        }
    }

    // ------------------------------------------------------------------ capturas

    /// <summary>Uma foto por pendência, com o Odisseu no chão ao lado quando faz sentido — sem salvar nada.</summary>
    public static void Shots()
    {
        string[] a = System.Environment.GetCommandLineArgs();
        int i = System.Array.IndexOf(a, "-polishShots");
        if (i < 0) { Debug.LogError("[Polish] falta -polishShots antes|depois"); EditorApplication.Exit(1); return; }
        string sufixo = a[i + 1];
        const string pasta = "Docs/Characters/_final_polish/";
        System.IO.Directory.CreateDirectory(pasta);

        Foto("Level_01_Itaca_Prologue", () =>
        {
            GameObject p = GameObject.Find("World/Act4_Training/Sparring_Partner");
            if (p == null) { return float.NaN; }
            p.SetActive(true);
            PorJogador(p.transform.position.x - 2.2f);
            return p.transform.position.x - 1f;
        }, 0f, pasta + $"P08_fase01_parceiro_{sufixo}.png");
        Foto("Level_06_Eolo", () => 42.5f, 0.5f, pasta + $"P03_palacio_eolo_{sufixo}.png");
        Foto("Level_11_CilaCaribdis", () => { PorJogador(3f); return 5f; }, 0f, pasta + $"P01_cila_{sufixo}.png");
        Foto("Level_11_CilaCaribdis", () => 16f, 0f, pasta + $"P02_caribdis_{sufixo}.png");
        Foto("Level_12_GadoDoSol", () => { PorJogador(2.4f); return 6f; }, -0.5f, pasta + $"P05_rebanho_a_{sufixo}.png");
        Foto("Level_12_GadoDoSol", () => 14f, -0.5f, pasta + $"P05_rebanho_b_{sufixo}.png");
        Foto("Level_12_GadoDoSol", () => 1.5f, 0f, pasta + $"P06_arvore_{sufixo}.png");
        Foto("Level_13_Calipso", () => { PorJogador(GameObject.Find("DialogueTrigger_Raft").transform.position.x); return 27f; }, 0f, pasta + $"P07_jangada_{sufixo}.png");
        EditorApplication.Exit(0);
    }

    private static void Foto(string cena, System.Func<float> preparar, float y, string saida)
    {
        EditorSceneManager.OpenScene(Levels + cena + ".unity", OpenSceneMode.Single);
        float x = preparar();
        if (float.IsNaN(x)) { Debug.LogWarning("[Polish] sem alvo para " + saida); return; }
        PrologueScreenshot.Render(x, y, saida);
    }

    /// <summary>Só o x muda: a altura é a de nascimento (a caixa do sprite tem margem transparente abaixo dos pés).</summary>
    private static void PorJogador(float x)
    {
        GameObject j = GameObject.Find("Player");
        Transform corpo = j.transform.Find("Visual/Body");
        j.transform.position += new Vector3(x - corpo.position.x, 0f, 0f);
    }
}
