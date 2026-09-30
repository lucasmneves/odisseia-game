using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Odisseia.Systems;

/// <summary>
/// Veste a fase 08 com o cenário de Circe:
///
/// Unity.exe -batchmode -quit -nographics -projectPath . -executeMethod CirceSceneDresser.Run
///
/// Mesmo desenho dos vestidores das fases 02 a 07 — não é sistema novo.
///
/// Duas coisas são próprias desta fase:
///
/// **Não há céu.** O que se vê acima é luz filtrada pela copa, dourada e abafada. Pôr o azul
/// das outras fases aqui abriria um buraco de céu no meio da mata.
///
/// **A progressão é de atmosfera, não de terreno.** O briefing pede que fique gradualmente
/// mais misteriosa, e isso acontece por três camadas que ganham peso conforme o jogador avança:
/// névoa, feixes de luz e partículas. As duas <c>TransformationZone</c> da cena marcam onde a
/// magia é mais densa, e é a partir delas que a densidade sobe.
/// </summary>
public static class CirceSceneDresser
{
    private const string ScenePath = "Assets/Scenes/Levels/Level_08_Circe.unity";
    private const string Raiz = "Assets/Art/Environments/Circe/";
    private const string RaizDoCenario = "CirceScenery";

    private const float TopoDoChao = -2f;

    private const float CameraInicial = 0f;
    /// <summary>Meio do passeio: o jogador vai de x=-14 a x=38.</summary>
    private const float CameraMeio = 12f;
    private const float CameraCurso = 52f;

    private static Transform cenario;

    [MenuItem("Odisseia/Vestir Circe")]
    public static void Vestir() => Executar();

    public static void Run()
    {
        bool ok = Executar();
        if (Application.isBatchMode) { EditorApplication.Exit(ok ? 0 : 1); }
    }

    private static bool Executar()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        GameObject antigo = GameObject.Find(RaizDoCenario);
        while (antigo != null) { Object.DestroyImmediate(antigo); antigo = GameObject.Find(RaizDoCenario); }

        var raiz = new GameObject(RaizDoCenario);
        cenario = raiz.transform;

        List<GameObject> chaos = PorPrefixo("Floor_").ToList();
        if (chaos.Count == 0)
        {
            Debug.LogError("[Circe] nenhum Floor_* encontrado — cenário não montado");
            return false;
        }

        DesligarPlaceholders();
        MontarMata();
        VestirChao(chaos);
        PovoarFloresta(chaos);
        PovoarRuinas(chaos);
        MontarTemplo(chaos);
        MontarMagia();
        MontarPrimeiroPlano();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[Circe] cenário montado: {cenario.childCount} objetos sob {RaizDoCenario}");
        return true;
    }

    private static void DesligarPlaceholders()
    {
        string[] prefixos = { "Floor_", "Sky_Background", "LevelGoal", "TransformationZone_", "MolyHerb" };
        foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (go == null || !prefixos.Any(pre => go.name.StartsWith(pre))) { continue; }
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null) { sr.enabled = false; }
        }
    }

    // ---------------------------------------------------------------- mata

    /// <summary>
    /// Perspectiva aérea, a lição que Lestrigões cobrou: quanto mais longe, mais a camada é
    /// puxada para a cor da luz da copa e menos contraste ela tem. Numa floresta, em que tudo
    /// é verde, é o VALOR que separa os planos — sem isto a fase vira uma parede de folhas.
    /// </summary>
    private static Color Distancia(float fator)
    {
        return Color.Lerp(Color.white, Cor("#c2b094"), Mathf.Clamp01((fator - 0.30f) * 1.15f));
    }

    private static void MontarMata()
    {
        // Cobertura na cor da luz filtrada, não no azul das outras fases: não há céu aqui.
        Bloco("Canopy_Fill", 12f, 12f, 130f, 30f, Cor("#8d734e"), -60);
        Bloco("Floor_Fill", 12f, -10f, 130f, 12f, Cor("#261916"), -30);

        Camada("BG_Light", "Background/circe_canopy_light.png", 1.00f, -50, -4f, 1f);
        Camada("BG_Forest_Far", "Background/circe_forest_far.png", 0.86f, -46, -2.5f, 1f);
        Camada("BG_Forest_Mid", "Background/circe_forest_mid.png", 0.66f, -42, -2.5f, 1f);
        Camada("BG_Forest_Band", "Midground/circe_forest_band.png", 0.48f, -38, -2.0f, 1f);
    }

    private static GameObject Camada(string nome, string caminho, float fator, int ordem,
        float baseY, float alturas)
    {
        Sprite arte = Arte(caminho);
        if (arte == null) { return null; }

        const float larguraDaTela = 18f;
        float x0 = CameraMeio * (1f - fator) + CameraInicial * fator;
        float deslize = CameraCurso * (1f - fator);

        var go = new GameObject(nome);
        go.transform.SetParent(cenario, false);
        go.transform.position = new Vector3(x0, baseY, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = arte;
        sr.sortingOrder = ordem;
        sr.color = Distancia(fator);
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.tileMode = SpriteTileMode.Continuous;
        // Altura NATIVA vezes o número de repetições pedido. Esticar sem multiplicar inteiro
        // faria a faixa repetir cortada e aparecer uma listra.
        sr.size = new Vector2(deslize + larguraDaTela * 2f, arte.bounds.size.y * alturas);

        var parallax = go.AddComponent<ParallaxLayer>();
        var so = new SerializedObject(parallax);
        so.FindProperty("parallaxFactor").floatValue = fator;
        so.ApplyModifiedProperties();
        return go;
    }

    // ---------------------------------------------------------------- chão

    /// <summary>
    /// Chão de floresta na praia e na mata, laje de mármore no domínio de Circe. A fronteira é
    /// a <c>TransformationZone_Big</c>, que o level design já pôs onde a fase muda de caráter —
    /// ler dela em vez de escrever uma coordenada mantém arte e design em sincronia.
    /// </summary>
    private static void VestirChao(List<GameObject> chaos)
    {
        GameObject dominio = GameObject.Find("TransformationZone_Big");
        float xDominio = dominio != null && dominio.GetComponent<Collider2D>() != null
            ? Caixa(dominio).min.x
            : 18f;

        foreach (GameObject go in chaos)
        {
            Bounds b = Caixa(go);
            bool marmore = b.center.x >= xDominio;
            Faixa($"Ground_{go.name}", b.center.x, b.max.y, b.size.x,
                marmore ? "Gameplay/circe_tiles_marble.png" : "Gameplay/circe_tiles_forest.png", -10);
            Bloco($"Bank_{go.name}", b.center.x, b.max.y - 9f, b.size.x, 12f, Cor("#261916"), -12);
        }
    }

    // ---------------------------------------------------------------- povoamento

    private static void PovoarFloresta(List<GameObject> chaos)
    {
        foreach (GameObject go in chaos)
        {
            Bounds b = Caixa(go);
            int arvores = Mathf.Max(1, Mathf.RoundToInt(b.size.x / 9f));
            for (int i = 0; i < arvores; i++)
            {
                float x = b.min.x + b.size.x * (i + 0.5f) / arvores;
                GameObject a = Prop($"Tree_{go.name}_{i}", x, TopoDoChao, "Forest/circe_big_tree.png", -20);
                // Árvores do fundo recuam em valor e encolhem: é a mesma perspectiva aérea
                // aplicada a objeto, e é o que impede a mata de virar uma parede chapada.
                if (a == null) { continue; }
                a.transform.localScale = new Vector3(0.85f, 0.85f, 1f);
                a.GetComponent<SpriteRenderer>().color = Distancia(0.55f);
            }

            int fetos = Mathf.Max(1, Mathf.RoundToInt(b.size.x / 6f));
            for (int i = 0; i < fetos; i++)
            {
                float x = b.min.x + b.size.x * (i + 0.3f) / fetos;
                Encolher(Prop($"Fern_{go.name}_{i}", x, TopoDoChao, "Forest/circe_fern_cluster.png", -3), 0.7f);
            }
        }
    }

    /// <summary>Ruínas no trecho do meio: a transição entre a mata e o domínio.</summary>
    private static void PovoarRuinas(List<GameObject> chaos)
    {
        if (chaos.Count < 2) { return; }
        Bounds b = Caixa(chaos[1]);

        Prop("Ruin_Wall", b.min.x + 3f, TopoDoChao, "Ruins/circe_ruined_wall.png", -16);
        Prop("Ruin_Column", b.min.x + 9f, TopoDoChao, "Ruins/circe_fallen_column.png", -4);
        Encolher(Prop("Ruin_Statue", b.min.x + 14.5f, TopoDoChao, "Ruins/circe_mossy_statue.png", -6), 0.45f);
        Encolher(Prop("Basin", b.min.x + 11.5f, TopoDoChao, "Props/circe_stone_basin.png", -2), 0.8f);
        Encolher(Prop("Flowers_Ruins", b.min.x + 6f, TopoDoChao, "Magic/circe_glow_flowers.png", -2), 0.26f);
    }

    /// <summary>
    /// §"PALÁCIO/TEMPLO" e §"ÁREA MÁGICA". O vão da porta fica livre: é onde Circe, o Odisseu
    /// e o diálogo entram depois. Nada é gerado de personagem aqui.
    /// </summary>
    private static void MontarTemplo(List<GameObject> chaos)
    {
        GameObject dominio = chaos.LastOrDefault();
        if (dominio == null) { return; }

        Bounds b = Caixa(dominio);
        float centro = b.min.x + b.size.x * 0.55f;

        Prop("Temple", centro, TopoDoChao, "Palace/circe_temple_front.png", -18);
        Encolher(Prop("Statue_L", centro - 5.5f, TopoDoChao, "Ruins/circe_mossy_statue.png", -6), 0.45f);
        Encolher(Prop("Statue_R", centro + 5.5f, TopoDoChao, "Ruins/circe_mossy_statue.png", -6), 0.45f);
        Encolher(Prop("Alchemy", centro - 8.5f, TopoDoChao, "Props/circe_alchemy_table.png", -3), 0.85f);
        // A paleta de Circe ganhou rampa de terracota depois desta cena ficar arroxeada:
        Encolher(Prop("Amphorae", centro + 8f, TopoDoChao,
            "Props/circe_amphorae.png", -3), 0.8f);
        // 0,26 dá uma moita de ~1,2 un, pouco abaixo do ombro do Odisseu. A 0,9 as flores
        // ficavam maiores que ele e a magia deixava de ser sutil, que é o que o briefing pede.
        Encolher(Prop("Flowers_Temple_L", centro - 3.4f, TopoDoChao, "Magic/circe_glow_flowers.png", -2), 0.26f);
        Encolher(Prop("Flowers_Temple_R", centro + 3.4f, TopoDoChao, "Magic/circe_glow_flowers.png", -2), 0.26f);
    }

    /// <summary>
    /// A magia é sutil de propósito: três camadas de alpha baixo, e não neon.
    ///
    /// A névoa e as partículas deslizam com <see cref="ScrollingLayer"/>, que responde ao
    /// TEMPO — o parallax responde à câmera e sozinho deixaria tudo parado quando o jogador
    /// para. Feixe de luz fica imóvel de propósito: luz que atravessa a copa não anda.
    /// </summary>
    private static void MontarMagia()
    {
        Camada("Light_Shafts", "VFX/circe_light_shafts.png", 0.72f, -34, -3f, 2f);

        GameObject nevoa = Camada("Mist", "Magic/circe_mist_band.png", 0.30f, -8, -2.6f, 1f);
        if (nevoa != null)
        {
            // A névoa é VELADURA, não massa. Quantizada para a rampa de mármore ela subiu até o
            // branco puro e, opaca, lia como neve amontoada ao pé das árvores.
            var sr = nevoa.GetComponent<SpriteRenderer>();
            Color c = sr.color;
            sr.color = new Color(c.r, c.g, c.b, 0.28f);
        }
        Deslizar(nevoa, -0.55f);

        GameObject poeira = Camada("Motes", "VFX/circe_motes.png", 0.18f, 11, -3f, 2f);
        Deslizar(poeira, -0.9f);
    }

    private static void Deslizar(GameObject go, float velocidade)
    {
        if (go == null) { return; }
        var sc = go.AddComponent<ScrollingLayer>();
        var so = new SerializedObject(sc);
        so.FindProperty("speedX").floatValue = velocidade;
        so.ApplyModifiedProperties();
    }

    /// <summary>
    /// Rasteira de primeiro plano sem <see cref="ParallaxLayer"/>: o fator dele é limitado a
    /// [0,1] e primeiro plano exigiria mais que 1. Fica parada, baixa e enterrada, com ordem
    /// acima do jogador — a copa do mato, não o mato inteiro, para não esconder ninguém.
    /// </summary>
    private static void MontarPrimeiroPlano()
    {
        Sprite arte = Arte("VFX/circe_undergrowth.png");
        if (arte == null) { return; }

        var go = new GameObject("FG_Undergrowth");
        go.transform.SetParent(cenario, false);
        go.transform.position = new Vector3(12f, TopoDoChao - 2.15f, -1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = arte;
        sr.sortingOrder = 12;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.tileMode = SpriteTileMode.Continuous;
        sr.size = new Vector2(120f, arte.bounds.size.y);
    }

    // ---------------------------------------------------------------- utilidades

    private static IEnumerable<GameObject> PorPrefixo(string prefixo) =>
        Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
            .Where(g => g != null && g.name.StartsWith(prefixo) && g.GetComponent<Collider2D>() != null)
            .OrderBy(g => g.transform.position.x);

    private static Bounds Caixa(GameObject go) => go.GetComponent<Collider2D>().bounds;

    private static void Encolher(GameObject go, float k)
    {
        if (go != null) { go.transform.localScale = new Vector3(k, k, 1f); }
    }

    private static void Faixa(string nome, float centroX, float topoY, float largura,
        string caminho, int ordem)
    {
        Sprite arte = Arte(caminho);
        if (arte == null) { return; }

        float altura = arte.bounds.size.y;
        var go = new GameObject(nome);
        go.transform.SetParent(cenario, false);
        go.transform.position = new Vector3(centroX, topoY - altura, 0.5f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = arte;
        sr.sortingOrder = ordem;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.tileMode = SpriteTileMode.Continuous;
        sr.size = new Vector2(largura, altura);
    }

    private static GameObject Prop(string nome, float x, float y, string caminho, int ordem)
    {
        Sprite arte = Arte(caminho);
        if (arte == null) { return null; }

        var go = new GameObject(nome);
        go.transform.SetParent(cenario, false);
        go.transform.position = new Vector3(x, y, 0f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = arte;
        sr.sortingOrder = ordem;
        return go;
    }

    private static void Bloco(string nome, float x, float y, float largura, float altura,
        Color cor, int ordem)
    {
        Sprite quadrado = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Player/PlaceholderSquare.png");
        if (quadrado == null) { return; }

        var go = new GameObject(nome);
        go.transform.SetParent(cenario, false);
        go.transform.position = new Vector3(x, y, 2f);
        go.transform.localScale = new Vector3(largura, altura, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = quadrado;
        sr.color = cor;
        sr.sortingOrder = ordem;
    }

    private static Sprite Arte(string relativo)
    {
        string caminho = relativo.StartsWith("Assets/") ? relativo : Raiz + relativo;
        Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(caminho);
        if (s == null) { Debug.LogWarning("[Circe] sprite não encontrado: " + caminho); }
        return s;
    }

    private static Color Cor(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color c);
        return c;
    }
}
