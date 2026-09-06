using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Odisseia.Systems;

/// <summary>
/// Veste a fase 04 com o cenário de Cytera:
///
/// Unity.exe -batchmode -quit -nographics -projectPath . -executeMethod CyteraSceneDresser.Run
///
/// Mesmo desenho do <see cref="CiconesSceneDresser"/> e do <see cref="TroySceneDresser"/> — não
/// é um sistema novo, é o mesmo procedimento numa fase diferente.
///
/// A diferença de Cytera é que a cena **já vem inteiramente estruturada** para o gameplay:
/// <c>Deck_Popa</c>, <c>Destroco_1..4</c>, <c>Onda_1..3</c>, <c>Rocha_1..4</c>, os três anéis do
/// <c>Redemoinho</c>, <c>Raio_1..3</c> e <c>Deck_Proa</c> já existem com colisores. Este script
/// **não cria geometria**: ele desliga o desenho de placeholder de cada um e põe a arte certa
/// exatamente na medida do colisor. Se o level design mudar uma rocha de lugar, a arte segue.
/// </summary>
public static class CyteraSceneDresser
{
    private const string ScenePath = "Assets/Scenes/Levels/Level_04_Citera.unity";
    private const string Raiz = "Assets/Art/Environments/Cytera/";
    private const string RaizDoCenario = "CyteraScenery";

    /// <summary>Onde a câmera está no primeiro quadro; o ParallaxLayer mede a partir daqui.</summary>
    private const float CameraInicial = 0f;
    /// <summary>Meio do passeio da câmera: o jogador vai de x=-15 a x=39.</summary>
    private const float CameraMeio = 12f;
    private const float CameraCurso = 54f;

    private static Transform cenario;

    [MenuItem("Odisseia/Vestir Cytera")]
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

        DesligarPlaceholders();
        MontarCeuEMar();
        VestirDecks();
        VestirDestrocos();
        VestirRochas();
        VestirOndas();
        VestirRedemoinho();
        VestirRaios();
        MontarClima();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[Cytera] cenário montado: {cenario.childCount} objetos sob {RaizDoCenario}");
        return true;
    }

    /// <summary>
    /// Apaga o DESENHO dos placeholders, nunca o objeto: o colisor, o nome e as referências do
    /// level design continuam intactos. Foi o PlaceholderProbe que mostrou, em Cícones, que
    /// esquecer isto deixa o retângulo chapado desenhando por cima da arte nova.
    /// </summary>
    private static void DesligarPlaceholders()
    {
        string[] prefixos =
        {
            "Deck_", "Destroco_", "Onda_", "Rocha_", "Raio_", "Redemoinho",
            "Sky_Background", "Mar_Superficie", "Mast", "Sail", "LevelGoal",
        };

        foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (go == null) { continue; }
            if (!prefixos.Any(pre => go.name.StartsWith(pre))) { continue; }

            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null) { sr.enabled = false; }
        }
    }

    // ------------------------------------------------------------- céu e mar

    private static void MontarCeuEMar()
    {
        // Cobertura na cor mais escura do céu. Em Cícones o vão sem cobertura virou um buraco
        // brilhante; aqui o risco é o oposto (buraco preto), e a solução é a mesma.
        Bloco("Sky_Fill", 12f, 14f, 130f, 30f, Cor("#1e211c"), -60);
        // Água escura abaixo de tudo, para nenhum vão mostrar o vazio.
        Bloco("Sea_Fill", 12f, -12f, 130f, 14f, Cor("#1c1f1a"), -58);

        // O céu começa NA linha d'água e sobe: assim o degradê inteiro cabe no que a câmera vê
        // (de -6 a +4), em vez de a câmera pegar só o miolo dele e a tempestade ler chapada.
        Camada("BG_Sky", "Background/cytera_sky_storm.png", 1.00f, -50, -3f);
        EspalharNuvens();

        // Mar: profundo embaixo, superfície em cima, espuma na linha d'água. Três faixas em vez
        // de uma imagem de mar — é o que o §11 pede e o que permite deslocar cada uma numa
        // velocidade para o mar parecer vivo.
        // As três alturas saem da linha d'água em -3,0, logo abaixo das plataformas em -2:
        // superfície de 2,99 un ocupa -6,0 a -3,0 e cobre tudo que a câmera vê para baixo.
        Camada("Sea_Deep", "Ocean/cytera_deep_water.png", 0.35f, -22, -8.3f);
        Camada("Sea_Surface", "Ocean/cytera_ocean_surface.png", 0.18f, -20, -6.0f);
        Camada("Sea_Foam", "Ocean/cytera_foam_line.png", 0.10f, -18, -3.2f);
    }

    /// <summary>
    /// Nuvens como objetos avulsos, não como camada que ladrilha. Ladrilhada, a mesma forma
    /// aparecia na mesma altura a cada 9 unidades e a repetição lia mais que a nuvem. Variando
    /// altura, escala e espelhamento, oito cópias cobrem a fase sem denunciar que são oito.
    ///
    /// Elas não usam <see cref="ParallaxLayer"/> — nuvem parada no céu de uma fase de 54 un é
    /// indistinguível de nuvem com parallax alto, e um componente a menos por objeto é o que
    /// o §16 pede para WebGL.
    /// </summary>
    private static void EspalharNuvens()
    {
        float[] xs = { -14f, -4f, 3f, 11f, 18f, 26f, 33f, 40f };
        float[] ys = { 2.6f, 1.4f, 3.1f, 2.0f, 3.4f, 1.7f, 2.9f, 2.2f };
        float[] ks = { 1.0f, 0.72f, 1.25f, 0.85f, 1.1f, 0.65f, 1.35f, 0.9f };

        for (int i = 0; i < xs.Length; i++)
        {
            GameObject nuvem = Prop($"Cloud_{i}", xs[i], ys[i], "Background/cytera_storm_cloud.png", -46);
            if (nuvem == null) { continue; }
            nuvem.transform.localScale = new Vector3(i % 3 == 1 ? -ks[i] : ks[i], ks[i], 1f);
        }
    }

    // ------------------------------------------------------------- navio e plataformas

    /// <summary>Deck de popa e de proa: faixa de tábuas na medida exata do colisor.</summary>
    private static void VestirDecks()
    {
        foreach (GameObject go in PorPrefixo("Deck_"))
        {
            Bounds b = Caixa(go);
            Faixa($"Art_{go.name}", b.center.x, b.max.y, b.size.x, 2.6f,
                "Ship/cytera_deck_planks.png", -6);
        }

        // O mastro fica na popa, onde o jogador começa: é o que identifica a plataforma inicial
        // como o navio de Odisseu e não como mais um destroço.
        GameObject popa = PorPrefixo("Deck_").FirstOrDefault();
        if (popa != null)
        {
            Bounds b = Caixa(popa);
            Prop("Art_Mast", b.min.x + 3.5f, b.max.y, "Ship/cytera_mast_sail.png", -7);
        }

        // Um segundo mastro na proa marca a chegada. O LevelGoal continua sendo o gatilho; o
        // que muda é que o jogador vê um navio no fim em vez de um retângulo marrom.
        GameObject proa = PorPrefixo("Deck_").LastOrDefault();
        if (proa != null && proa != popa)
        {
            Bounds b = Caixa(proa);
            Prop("Art_Mast_Proa", b.max.x - 3.5f, b.max.y, "Ship/cytera_mast_sail.png", -7);
        }
    }

    /// <summary>Destroços: pranchas amarradas, na largura do colisor.</summary>
    private static void VestirDestrocos()
    {
        foreach (GameObject go in PorPrefixo("Destroco_"))
        {
            Bounds b = Caixa(go);
            GameObject arte = Prop($"Art_{go.name}", b.center.x, b.min.y, "Ship/cytera_wreckage.png", -4);
            EscalarPara(arte, b);
        }
    }

    /// <summary>
    /// Rochas. As altas ficam nas que o colisor deixa mais estreitas; as largas, no resto —
    /// a escolha sai da MEDIDA do colisor, não de uma lista escrita à mão que sairia de
    /// sincronia com o level design.
    /// </summary>
    private static void VestirRochas()
    {
        foreach (GameObject go in PorPrefixo("Rocha_"))
        {
            Bounds b = Caixa(go);
            string arte = b.size.x >= 2.8f ? "Rocks/cytera_rock_wide.png" : "Rocks/cytera_rock_tall.png";
            GameObject r = Prop($"Art_{go.name}", b.center.x, b.min.y, arte, -3);
            EscalarPara(r, b);
        }
    }

    /// <summary>Ondas: a arte é maior que o colisor de propósito, porque a crista passa por cima.</summary>
    private static void VestirOndas()
    {
        foreach (GameObject go in PorPrefixo("Onda_"))
        {
            Bounds b = Caixa(go);
            GameObject onda = Prop($"Art_{go.name}", b.center.x, b.min.y, "Ocean/cytera_wave_large.png", 6);
            if (onda == null) { continue; }

            Sprite s = onda.GetComponent<SpriteRenderer>().sprite;
            // Escala pela LARGURA e deixa a altura seguir: forçar a altura do colisor achataria
            // a crista, e é a crista que avisa o jogador que ali tem perigo.
            float k = (b.size.x * 2.1f) / s.bounds.size.x;
            onda.transform.localScale = new Vector3(k, k, 1f);

            Prop($"Art_{go.name}_Spray", b.center.x, b.max.y - 0.3f, "Effects/cytera_spray.png", 7);
        }
    }

    private static void VestirRedemoinho()
    {
        var aneis = new[] { "Redemoinho_Anel_1", "Redemoinho_Anel_2", "Redemoinho_Anel_3" };
        for (int i = 0; i < aneis.Length; i++)
        {
            GameObject go = GameObject.Find(aneis[i]);
            if (go == null) { continue; }

            GameObject arte = Prop($"Art_{go.name}", go.transform.position.x, go.transform.position.y,
                $"Effects/cytera_ring_{i + 1}.png", 4 + i);
            if (arte == null) { continue; }

            // Centrado no anel, não apoiado nele: o pivô do sprite é a base, e um anel de
            // espuma gira em torno do próprio centro.
            Sprite s = arte.GetComponent<SpriteRenderer>().sprite;
            arte.transform.position = new Vector3(go.transform.position.x,
                go.transform.position.y - s.bounds.size.y * 0.5f, 0f);
        }
    }

    private static void VestirRaios()
    {
        foreach (GameObject go in PorPrefixo("Raio_"))
        {
            Bounds b = Caixa(go);
            GameObject raio = Prop($"Art_{go.name}", b.center.x, b.min.y, "Weather/cytera_lightning.png", 9);
            if (raio == null) { continue; }

            Sprite s = raio.GetComponent<SpriteRenderer>().sprite;
            float k = b.size.y / s.bounds.size.y;
            raio.transform.localScale = new Vector3(k, k, 1f);
        }
    }

    /// <summary>
    /// Chuva em duas camadas com fatores diferentes. Uma camada só de chuva lê como textura
    /// colada na tela; duas, deslizando a velocidades diferentes, leem como volume de água
    /// entre a câmera e o cenário.
    /// </summary>
    private static void MontarClima()
    {
        Camada("Rain_Back", "Weather/cytera_rain.png", 0.55f, -2, -6f);
        Camada("Rain_Front", "Weather/cytera_rain.png", 0.15f, 14, -6f);
    }

    // ------------------------------------------------------------- utilidades

    private static IEnumerable<GameObject> PorPrefixo(string prefixo) =>
        Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
            .Where(g => g != null && g.name.StartsWith(prefixo) && g.GetComponent<Collider2D>() != null)
            .OrderBy(g => g.transform.position.x);

    private static Bounds Caixa(GameObject go) => go.GetComponent<Collider2D>().bounds;

    /// <summary>Escala uniforme para o sprite cobrir o colisor sem deformar o pixel art.</summary>
    private static void EscalarPara(GameObject go, Bounds alvo)
    {
        if (go == null) { return; }
        Sprite s = go.GetComponent<SpriteRenderer>().sprite;
        float k = Mathf.Max(alvo.size.x / s.bounds.size.x, alvo.size.y / s.bounds.size.y);
        go.transform.localScale = new Vector3(k, k, 1f);
        go.transform.position = new Vector3(alvo.center.x, alvo.min.y, 0f);
    }

    private static void Camada(string nome, string caminho, float fator, int ordem, float baseY)
    {
        Sprite arte = Arte(caminho);
        if (arte == null) { return; }

        const float larguraDaTela = 18f;
        float x0 = CameraMeio * (1f - fator) + CameraInicial * fator;
        float deslize = CameraCurso * (1f - fator);

        var go = new GameObject(nome);
        go.transform.SetParent(cenario, false);
        go.transform.position = new Vector3(x0, baseY, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = arte;
        sr.sortingOrder = ordem;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.tileMode = SpriteTileMode.Continuous;
        // Altura NATIVA: esticar size.y repete a faixa na vertical e cria uma listra.
        sr.size = new Vector2(deslize + larguraDaTela * 2f, arte.bounds.size.y);

        var parallax = go.AddComponent<ParallaxLayer>();
        var so = new SerializedObject(parallax);
        so.FindProperty("parallaxFactor").floatValue = fator;
        so.ApplyModifiedProperties();
    }

    private static void Faixa(string nome, float centroX, float topoY, float largura, float altura,
        string caminho, int ordem)
    {
        Sprite arte = Arte(caminho);
        if (arte == null) { return; }

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
        Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(Raiz + relativo);
        if (s == null) { Debug.LogWarning("[Cytera] sprite não encontrado: " + Raiz + relativo); }
        return s;
    }

    private static Color Cor(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color c);
        return c;
    }
}
