using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Odisseia.Systems;

/// <summary>
/// Veste a fase 03 com o cenário de Cícones:
///
/// Unity.exe -batchmode -quit -nographics -projectPath . -executeMethod CiconesSceneDresser.Run
///
/// Segue o mesmo desenho do <see cref="TroySceneDresser"/> — não é um sistema novo, é o mesmo
/// procedimento aplicado a outra fase.
///
/// **Idempotente:** apaga a raiz <c>CiconesScenery</c> antes de montar, então rodar de novo
/// não empilha cópias. Nada fora dessa raiz é criado, e o único objeto pré-existente tocado é
/// o `Sky_Background` de placeholder, que é desligado (não apagado).
///
/// **Lê o chão dos colisores `Floor_*`**, em vez de repetir as coordenadas aqui. Se o level
/// design mudar a largura de um trecho, o cenário acompanha sozinho.
///
/// **Os vãos não são preenchidos.** Entre Floor_1 e Floor_2 há 3 unidades de abismo e entre
/// Floor_2 e Floor_3 outras 3. Desenhar chão contínuo por cima esconderia o buraco que o
/// jogador precisa ver para pular.
/// </summary>
public static class CiconesSceneDresser
{
    private const string ScenePath = "Assets/Scenes/Levels/Level_03_Cicones.unity";
    private const string Raiz = "Assets/Art/Environments/Cicones/";
    private const string RaizDoCenario = "CiconesScenery";

    /// <summary>Topo do chão, lido dos colisores mas fixado aqui como conferência.</summary>
    private const float TopoDoChao = -2f;

    private static Transform cenario;

    [MenuItem("Odisseia/Vestir Cicones")]
    public static void Vestir() => Executar();

    public static void Run()
    {
        bool ok = Executar();
        if (Application.isBatchMode) { EditorApplication.Exit(ok ? 0 : 1); }
    }

    private static bool Executar()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        LimparCenarioAntigo();
        var raiz = new GameObject(RaizDoCenario);
        cenario = raiz.transform;

        DesligarFundoDePlaceholder();
        DesligarPlaceholdersDeGameplay();
        VestirMuroDaEntrada();
        MontarParallax();

        List<Bounds> trechos = TrechosDeChao();
        if (trechos.Count == 0)
        {
            Debug.LogError("[Cicones] nenhum Floor_* encontrado — cenário não montado");
            return false;
        }

        VestirChao(trechos);
        PovoarPraia(trechos);
        PovoarCampos(trechos);
        PovoarVila(trechos);
        MontarPrimeiroPlano(trechos);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[Cicones] cenário montado: {cenario.childCount} objetos sob {RaizDoCenario}");
        return true;
    }

    private static void LimparCenarioAntigo()
    {
        GameObject antigo = GameObject.Find(RaizDoCenario);
        while (antigo != null)
        {
            Object.DestroyImmediate(antigo);
            antigo = GameObject.Find(RaizDoCenario);
        }
    }

    /// <summary>
    /// Desliga, não apaga: o bloco de céu de placeholder pode ser reativado se o cenário for
    /// removido, e apagá-lo deixaria a fase sem fundo nenhum nesse caso.
    /// </summary>
    private static void DesligarFundoDePlaceholder()
    {
        GameObject ceu = GameObject.Find("Sky_Background");
        if (ceu != null)
        {
            ceu.SetActive(false);
        }
    }

    /// <summary>
    /// Apaga o DESENHO dos placeholders que agora estão cobertos por arte, sem tocar nos
    /// colisores. Os <c>Floor_*</c> desenham em ordem 0 e o chão novo em -10, então o retângulo
    /// chapado deles ficava POR CIMA da areia — a captura não denunciava porque a faixa é fina
    /// e da mesma família de cor. Foi o PlaceholderProbe que achou.
    ///
    /// Desligar o renderer, e não apagar o objeto, mantém o colisor, o nome e qualquer
    /// referência que o level design tenha a ele.
    /// </summary>
    private static void DesligarPlaceholdersDeGameplay()
    {
        foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (go == null) { continue; }
            bool coberto = go.name.StartsWith("Floor_") || go.name == "Lintel";
            if (!coberto) { continue; }

            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null) { sr.enabled = false; }
        }
    }

    /// <summary>
    /// O <c>Wall_Village</c> é um colisor de gameplay com sprite de placeholder — um retângulo
    /// marrom de 1,5 x 5 na entrada da fase. Ele fica, porque é geometria; o que muda é o que
    /// se vê. Cobrir em vez de trocar o sprite mantém o objeto do level design intocado.
    /// </summary>
    private static void VestirMuroDaEntrada()
    {
        GameObject muro = GameObject.Find("Wall_Village");
        if (muro == null) { return; }

        var sr = muro.GetComponent<SpriteRenderer>();
        if (sr != null) { sr.enabled = false; }

        Bounds b = muro.GetComponent<Collider2D>().bounds;
        Sprite pedra = Arte("Architecture/cicones_drystone_wall.png");
        if (pedra == null) { return; }

        // Modo Tiled preenchendo o vão EXATO do colisor, em vez de três cópias empilhadas a
        // olho: empilhadas, a folga transparente acima da pedra de cada cópia virava um
        // intervalo vazio e as fiadas apareciam flutuando separadas no ar.
        var go = new GameObject("Wall_Stone");
        go.transform.SetParent(cenario, false);
        go.transform.position = new Vector3(b.center.x, b.min.y, 0f);

        var fiada = go.AddComponent<SpriteRenderer>();
        fiada.sprite = pedra;
        fiada.sortingOrder = -4;
        fiada.drawMode = SpriteDrawMode.Tiled;
        fiada.tileMode = SpriteTileMode.Continuous;
        fiada.size = new Vector2(b.size.x, b.size.y);
    }

    // ---------------------------------------------------------------- parallax

    private static void MontarParallax()
    {
        // Fundo de cobertura na cor do topo do degradê do céu. Sem ele, qualquer ponto acima da
        // faixa de céu fica magenta na captura e preto no jogo — foi o que a captura de Ítaca
        // mostrou quando a camada não cobria a fase inteira.
        Bloco("Sky_Fill", 7f, 8f, 120f, 34f, Cor("#6481a0"), -60);

        // Fator ALTO é longe: a camada acompanha a câmera e quase não desliza. Todas ladrilham,
        // então o fator pode descer até onde a profundidade realmente aparece.
        Camada("BG_Sky", "Background/cicones_bg_sky.png", 1.00f, -50, -6f);
        Camada("BG_Mountains", "Background/cicones_bg_mountains.png", 0.90f, -46, -0.5f);
        Camada("BG_Sea", "Background/cicones_bg_sea.png", 0.80f, -44, -2.2f);
        Camada("BG_Hills", "Background/cicones_bg_hills.png", 0.68f, -42, -1.6f);
        Camada("BG_Village", "Background/cicones_bg_village.png", 0.52f, -40, -2.0f);

        // Vale escuro abaixo da linha do horizonte. Sem ele o vão entre trechos de chão mostra
        // o céu — e como este degradê CLAREIA em direção ao horizonte, o abismo aparecia como
        // um retângulo brilhante, que lê como buraco na arte e não como profundidade.
        Bloco("Valley_Fill", 7f, -9f, 120f, 16f, Cor("#3e2819"), -30);
    }

    /// <summary>Onde a câmera está no primeiro quadro. O ParallaxLayer mede o deslocamento a
    /// partir daqui, então é o ponto em que fator nenhum desloca camada nenhuma.</summary>
    private const float CameraInicial = 0f;

    /// <summary>Meio do passeio da câmera: o jogador vai de x=-16 a x=32.</summary>
    private const float CameraMeio = 8f;

    /// <summary>Quanto a câmera percorre de ponta a ponta.</summary>
    private const float CameraCurso = 56f;

    /// <summary>
    /// Uma camada de parallax que cobre a fase inteira.
    ///
    /// A posição inicial NÃO é a borda esquerda da fase. O <see cref="ParallaxLayer"/> põe a
    /// camada em <c>x0 + (cameraX - CameraInicial) * fator</c>, então ancorar em x0 = borda
    /// esquerda só funciona quando a câmera também começa lá — que é o caso de Troia e não é o
    /// desta fase. Ancorando errado, a camada de fator 1,00 ficou a 38 unidades à esquerda da
    /// câmera para sempre, e o céu simplesmente não aparecia: a captura mostrou cobertura
    /// [-42 .. -6] com a câmera em x=14.
    ///
    /// O x0 certo sai de exigir que a camada esteja centrada na câmera no MEIO do percurso:
    ///     x0 + (CameraMeio - CameraInicial) * fator = CameraMeio
    /// donde x0 = CameraMeio * (1 - fator) + CameraInicial * fator.
    ///
    /// E a largura tem de cobrir o quanto a camada desliza na tela ao longo da fase,
    /// <c>curso * (1 - fator)</c>, mais uma tela inteira de folga de cada lado.
    /// </summary>
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
        // Altura NATIVA: esticar size.y faz a faixa repetir na vertical e aparecer uma listra.
        sr.size = new Vector2(deslize + larguraDaTela * 2f, arte.bounds.size.y);

        var parallax = go.AddComponent<ParallaxLayer>();
        var so = new SerializedObject(parallax);
        so.FindProperty("parallaxFactor").floatValue = fator;
        so.ApplyModifiedProperties();
    }

    // ---------------------------------------------------------------- chão

    private static List<Bounds> TrechosDeChao()
    {
        var trechos = new List<Bounds>();
        foreach (Collider2D c in Object.FindObjectsByType<Collider2D>(FindObjectsSortMode.None))
        {
            if (c == null || !c.gameObject.name.StartsWith("Floor_")) { continue; }
            trechos.Add(c.bounds);
        }
        return trechos.OrderBy(b => b.min.x).ToList();
    }

    /// <summary>
    /// Cada trecho recebe o terreno da sua área: areia no desembarque, terra com grama nos
    /// campos, terra na vila. É a progressão do §4.2 dita pela posição, não por uma lista
    /// paralela de coordenadas que sairia de sincronia com o level design.
    /// </summary>
    private static void VestirChao(List<Bounds> trechos)
    {
        for (int i = 0; i < trechos.Count; i++)
        {
            Bounds b = trechos[i];
            string tile = i == 0 ? "Terrain/cicones_tiles_sand.png"
                : "Terrain/cicones_tiles_earth_grass.png";

            // Altura NATIVA do tile. Esticar faz o SpriteDrawMode.Tiled repetir na vertical, e
            // a faixa de grama do topo aparece de novo no meio do barranco — visível na captura.
            Faixa($"Ground_{i}", b.center.x, b.max.y, b.size.x, tile, -10);
            // O barranco continua abaixo do tile com terra chapada, até fora do quadro.
            Bloco($"Bank_{i}", b.center.x, b.max.y - 3f - 6f, b.size.x, 12f, Cor("#3e2819"), -12);
        }
    }

    private static void Faixa(string nome, float centroX, float topoY, float largura,
        string caminho, int ordem)
    {
        Sprite arte = Arte(caminho);
        if (arte == null) { return; }

        float altura = arte.bounds.size.y;

        var go = new GameObject(nome);
        go.transform.SetParent(cenario, false);
        // O pivô do sprite está na base, então descer a altura põe o topo exatamente em topoY.
        go.transform.position = new Vector3(centroX, topoY - altura, 0.5f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = arte;
        sr.sortingOrder = ordem;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.tileMode = SpriteTileMode.Continuous;
        sr.size = new Vector2(largura, altura);
    }

    // ---------------------------------------------------------------- áreas

    /// <summary>§4.3 — desembarque: barco, suprimentos e vegetação costeira.</summary>
    private static void PovoarPraia(List<Bounds> trechos)
    {
        Bounds praia = trechos[0];
        Prop("Boat", praia.min.x + 2.5f, "Props/cicones_fishing_boat.png", -3);
        Prop("Supplies", praia.min.x + 7.5f, "../Troy/Camp/troy_supply_pile_01.png", -2);
        Prop("Grass_Beach_1", praia.min.x + 5.2f, "Nature/cicones_grass_tuft.png", -1);
        Prop("Grass_Beach_2", praia.min.x + 10.8f, "Nature/cicones_grass_tuft.png", -1);
        Prop("Bush_Beach", praia.min.x + 12.4f, "Nature/cicones_scrub_bush.png", -1);
    }

    /// <summary>§4.4 e §4.5 — campos e trilha: oliveiras, cercas, muro de pedra seca.</summary>
    private static void PovoarCampos(List<Bounds> trechos)
    {
        if (trechos.Count < 2) { return; }
        Bounds campo = trechos[1];

        Prop("Olive_1", campo.min.x + 1.5f, "Nature/cicones_olive_tree.png", -6);
        Prop("Olive_2", campo.min.x + 8.0f, "Nature/cicones_olive_tree.png", -6);
        Prop("Olive_3", campo.min.x + 15.5f, "Nature/cicones_olive_tree.png", -6);
        Prop("Fence_1", campo.min.x + 4.5f, "Props/cicones_wooden_fence.png", -4);
        Prop("Fence_2", campo.min.x + 11.0f, "Props/cicones_wooden_fence.png", -4);
        Prop("Wall_Field", campo.min.x + 13.0f, "Architecture/cicones_drystone_wall.png", -5);
        Prop("Bush_Field_1", campo.min.x + 6.6f, "Nature/cicones_scrub_bush.png", -2);
        Prop("Bush_Field_2", campo.min.x + 16.8f, "Nature/cicones_scrub_bush.png", -2);
        Prop("Cart", campo.min.x + 9.4f, "Props/cicones_handcart.png", -2);
        VestirObstaculo();
    }

    /// <summary>
    /// O <c>Obstacle_Low</c> é um degrau de 0,6 x 0,8 que o jogador pula ou agacha para passar.
    /// Continua sendo o colisor do level design; ganha só um monte de entulho por cima, na
    /// medida exata do colisor, para ler como obstáculo e não como caixa cinza.
    /// </summary>
    private static void VestirObstaculo()
    {
        GameObject obst = GameObject.Find("Obstacle_Low");
        if (obst == null) { return; }

        var placeholder = obst.GetComponent<SpriteRenderer>();
        if (placeholder != null) { placeholder.enabled = false; }

        Bounds b = obst.GetComponent<Collider2D>().bounds;
        GameObject arte = Prop("Obstacle_Rubble", b.center.x, "../Troy/Effects/troy_rubble_pile_01.png", -1);
        if (arte == null) { return; }

        Sprite s2 = arte.GetComponent<SpriteRenderer>().sprite;
        // Escala pela medida do colisor, nunca por um numero fixo: se o level design mudar a
        // altura do degrau, o entulho acompanha em vez de flutuar ou afundar.
        float k = b.size.y / s2.bounds.size.y;
        arte.transform.localScale = new Vector3(k, k, 1f);
        arte.transform.position = new Vector3(b.center.x, b.min.y, 0f);
    }

    /// <summary>
    /// §4.6, §4.7 e §4.8 — vila, conflito e landmark, nesta ordem ao longo do trecho: a
    /// intensidade tem de crescer conforme o jogador avança (§4.2).
    /// </summary>
    private static void PovoarVila(List<Bounds> trechos)
    {
        if (trechos.Count < 3) { return; }
        Bounds vila = trechos[2];

        // Vila: casas ao fundo, utilidades na frente delas.
        Prop("House_1", vila.min.x + 1.0f, "Architecture/cicones_house_small.png", -7);
        Prop("House_2", vila.min.x + 5.5f, "Architecture/cicones_house_large.png", -7);
        Prop("Well", vila.min.x + 4.0f, "Props/cicones_well.png", -3);
        Prop("Pithoi", vila.min.x + 8.4f, "Props/cicones_pithoi_baskets.png", -2);

        // Conflito: reuso do kit de Troia. A área 05 pede exatamente o que Troia já produziu,
        // e regerar seria pagar de novo por assets equivalentes (§18).
        Prop("Barricade", vila.min.x + 10.5f, "../Troy/Camp/troy_barricade_01.png", -2);
        Prop("Spears", vila.min.x + 11.8f, "../Troy/Camp/troy_spear_cluster_01.png", -2);
        Prop("Shield", vila.min.x + 12.6f, "../Troy/Camp/troy_broken_shield_01.png", -1);
        Prop("Campfire", vila.min.x + 9.6f, "../Troy/Camp/troy_campfire_01.png", -1);
        Prop("Smoke", vila.min.x + 9.6f, "../Troy/Effects/troy_smoke_column_01.png", -8);
        Prop("Rubble", vila.min.x + 13.4f, "../Troy/Effects/troy_rubble_pile_01.png", -2);

        // Landmark logo antes da saída: é o último marco visual da fase (§4.8).
        Prop("Shrine", vila.min.x + 14.2f, "Architecture/cicones_shrine_landmark.png", -6);
    }

    /// <summary>
    /// §8 FOREGROUND. Sem <see cref="ParallaxLayer"/>: o fator dele é limitado a [0,1], e
    /// primeiro plano exige fator maior que 1. Em vez de mexer no componente — que é sistema
    /// existente e serve as fases 1 e 2 — a vegetação da frente fica parada no plano de jogo
    /// com ordem acima do jogador. Dá a oclusão sem inventar um segundo sistema de parallax.
    /// </summary>
    private static void MontarPrimeiroPlano(List<Bounds> trechos)
    {
        float[] posicoes = { -19f, -8.5f, 4f, 16.5f, 27f };
        for (int i = 0; i < posicoes.Length; i++)
        {
            GameObject go = Prop($"FG_Bush_{i}", posicoes[i],
                i % 2 == 0 ? "Nature/cicones_scrub_bush.png" : "Nature/cicones_grass_tuft.png", 12);
            if (go == null) { continue; }
            // Maior e mais baixo que o mesmo arbusto ao fundo: é a diferença de tamanho que
            // faz o olho ler "perto", não a ordem de desenho sozinha.
            // 1,9 tomava um terço da tela e escondia o personagem — o §17 exige que o jogador
            // fique sempre legível. A 1,25, com o pé enterrado, sobra só a copa na borda baixa.
            go.transform.localScale = new Vector3(1.25f, 1.25f, 1f);
            go.transform.position = new Vector3(posicoes[i], TopoDoChao - 1.5f, -1f);
        }
    }

    // ---------------------------------------------------------------- utilidades

    private static GameObject Prop(string nome, float x, string caminho, int ordem)
    {
        Sprite arte = Arte(caminho);
        if (arte == null) { return null; }

        var go = new GameObject(nome);
        go.transform.SetParent(cenario, false);
        go.transform.position = new Vector3(x, TopoDoChao, 0f);

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
        string caminho = Raiz + relativo;
        if (relativo.StartsWith("../"))
        {
            caminho = "Assets/Art/Environments/" + relativo.Substring(3);
        }

        Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(caminho);
        if (s == null)
        {
            Debug.LogWarning("[Cicones] sprite não encontrado: " + caminho);
        }
        return s;
    }

    private static Color Cor(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color c);
        return c;
    }
}
