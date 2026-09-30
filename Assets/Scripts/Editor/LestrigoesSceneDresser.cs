using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Odisseia.Systems;

/// <summary>
/// Veste a fase 07 com o cenário de Lestrigões:
///
/// Unity.exe -batchmode -quit -nographics -projectPath . -executeMethod LestrigoesSceneDresser.Run
///
/// Mesmo desenho dos vestidores das fases 02 a 06 — não é sistema novo.
///
/// O que esta fase tem de próprio é **a escala**. O briefing pede que o lugar pareça grande
/// demais para o Odisseu, e isso não se resolve aumentando o personagem: resolve-se com uma
/// falésia de fundo duas vezes mais alta que a de Ciclopes, uma muralha que atravessa a fase
/// inteira acima da linha do olhar, e objetos de porto — poste de amarração, caixas, escada —
/// desenhados em tamanho que só faz sentido para quem é grande.
///
/// A progressão vai de PORTO (Floor_1) a CIDADE (Floor_2) a FORTIFICAÇÃO E FUGA (Floor_3), e
/// cada trecho recebe o terreno da sua área. A fronteira sai da geometria, nunca de coordenada
/// escrita à mão.
/// </summary>
public static class LestrigoesSceneDresser
{
    private const string ScenePath = "Assets/Scenes/Levels/Level_07_Lestrigoes.unity";
    private const string Raiz = "Assets/Art/Environments/Lestrigoes/";
    private const string RaizDoCenario = "LestrigoesScenery";

    private const float TopoDoChao = -2f;

    private const float CameraInicial = 0f;
    /// <summary>Meio do passeio: o jogador vai de x=-18 a x=42.</summary>
    private const float CameraMeio = 12f;
    private const float CameraCurso = 60f;

    private static Transform cenario;

    [MenuItem("Odisseia/Vestir Lestrigoes")]
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
            Debug.LogError("[Lestrigoes] nenhum Floor_* encontrado — cenário não montado");
            return false;
        }

        DesligarPlaceholders();
        MontarParallax(chaos);
        VestirChao(chaos);
        PovoarPorto(chaos);
        PovoarCidade(chaos);
        PovoarFortificacao(chaos);
        VestirNaufragioDeFundo();
        VestirSaida();
        MontarPrimeiroPlano();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[Lestrigoes] cenário montado: {cenario.childCount} objetos sob {RaizDoCenario}");
        return true;
    }

    private static void DesligarPlaceholders()
    {
        string[] prefixos = { "Floor_", "Obstacle_", "Sky_Background", "LevelGoal", "PursuerHazard",
            "WreckedShip_Background", "Mast", "Sail" };
        foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (go == null || !prefixos.Any(pre => go.name.StartsWith(pre))) { continue; }
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null) { sr.enabled = false; }
        }
    }

    // ---------------------------------------------------------------- fundo

    private static void MontarParallax(List<GameObject> chaos)
    {
        Bloco("Sky_Fill", 12f, 12f, 130f, 34f, Cor("#9ab0b6"), -60);
        Bloco("Valley_Fill", 12f, -10f, 130f, 12f, Cor("#3a3d3d"), -30);

        Camada("BG_Sky", "Background/lestrigoes_bg_sky.png", 1.00f, -50, -6f);
        Camada("BG_Mountains", "Background/lestrigoes_bg_mountains.png", 0.90f, -46, -0.5f);
        Camada("BG_Sea", "Background/lestrigoes_bg_sea.png", 0.80f, -44, -2.2f);
        Camada("BG_Hills", "Background/lestrigoes_bg_hills.png", 0.68f, -42, -1.6f);

        // A falésia próxima é o que estabelece a escala antes de qualquer objeto: 5,97 un de
        // altura contra as 4,11 de Cícones, subindo além do topo do quadro.
        Camada("BG_Cliffs", "Background/lestrigoes_bg_cliffs.png", 0.50f, -40, -4.97f);

        // O backdrop de falésia colunar cobre só a faixa BAIXA do fundo, atrás das casas. Como
        // faixa alta ele se somava à falésia de parallax e apagava o céu inteiro.
        float x0 = Caixa(chaos[0]).min.x, x1 = chaos.Max(g => Caixa(g).max.x);
        GameObject backdrop = Faixa("Cliff_Backdrop", (x0 + x1) * 0.5f, 2.2f, x1 - x0 + 6f,
            "Midground/lestrigoes_cliff_wall.png", -36);
        if (backdrop != null) { backdrop.GetComponent<SpriteRenderer>().color = Distancia(0.62f); }
    }

    /// <summary>
    /// Perspectiva aérea: quanto mais longe, mais a camada é puxada para a cor do céu.
    ///
    /// Sem isto Lestrigões vira uma massa cinza única. A paleta da fase é quase monocromática
    /// de propósito — é uma costa de pedra fria — e o efeito colateral é que falésia de fundo,
    /// muralha de meio e chão jogável medem luminância parecida e deixam de se distinguir. O
    /// briefing exige que o jogador separe chão, plataforma e parede num relance, e aqui quem
    /// faz essa separação é o VALOR, não o material.
    ///
    /// É um tint por renderer: nenhum asset novo, nenhum shader, nenhum sistema.
    /// </summary>
    private static Color Distancia(float fator)
    {
        return Color.Lerp(Color.white, Cor("#c8dadf"), Mathf.Clamp01((fator - 0.35f) * 1.25f));
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
        sr.color = Distancia(fator);
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.tileMode = SpriteTileMode.Continuous;
        // Altura NATIVA: esticar size.y repete a faixa na vertical e cria uma listra.
        sr.size = new Vector2(deslize + larguraDaTela * 2f, arte.bounds.size.y);

        var parallax = go.AddComponent<ParallaxLayer>();
        var so = new SerializedObject(parallax);
        so.FindProperty("parallaxFactor").floatValue = fator;
        so.ApplyModifiedProperties();
    }

    // ---------------------------------------------------------------- chão

    /// <summary>
    /// Porto tem cais de pedra, cidade tem laje, fuga tem rocha nua. A escolha sai do ÍNDICE
    /// do trecho, e o índice sai da posição em x — se o level design reordenar os trechos, o
    /// terreno acompanha.
    /// </summary>
    private static void VestirChao(List<GameObject> chaos)
    {
        string[] tiles =
        {
            "Gameplay/lestrigoes_tiles_planks.png",   // porto: deck de madeira
            "Gameplay/lestrigoes_tiles_quay.png",     // cidade: laje de pedra
            "Gameplay/lestrigoes_tiles_rock.png",     // fuga: rocha nua
        };

        for (int i = 0; i < chaos.Count; i++)
        {
            Bounds b = Caixa(chaos[i]);
            Faixa($"Ground_{i}", b.center.x, b.max.y, b.size.x, tiles[Mathf.Min(i, tiles.Length - 1)], -10);
            Bloco($"Bank_{i}", b.center.x, b.max.y - 9f, b.size.x, 12f, Cor("#3a3d3d"), -12);
        }

        // Os obstáculos são degraus de gameplay: viram blocos de pedra na medida do colisor,
        // para lerem como obstáculo e não como caixa cinza.
        foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
                     .Where(g => g != null && g.name.StartsWith("Obstacle_") && g.GetComponent<Collider2D>() != null))
        {
            Bounds b = Caixa(go);
            Faixa($"Art_{go.name}", b.center.x, b.max.y, b.size.x,
                "Gameplay/lestrigoes_tiles_quay.png", -2);
        }
    }

    // ---------------------------------------------------------------- áreas

    /// <summary>
    /// Porto. O navio dos Lestrigões é o **barco de Cícones ampliado**, e ao lado dele o mesmo
    /// barco em tamanho normal: é a comparação lado a lado que transmite a escala, e não o
    /// tamanho absoluto de nenhum dos dois. Reusar em vez de gerar também é o que o briefing
    /// pede.
    /// </summary>
    private static void PovoarPorto(List<GameObject> chaos)
    {
        Bounds b = Caixa(chaos[0]);

        GameObject grande = Prop("Ship_Giant", b.min.x + 5f, TopoDoChao,
            "Assets/Art/Environments/Cicones/Props/cicones_fishing_boat.png", -8);
        if (grande != null) { grande.transform.localScale = new Vector3(2.4f, 2.4f, 1f); }

        GameObject pequeno = Prop("Ship_Small", b.min.x + 13.5f, TopoDoChao,
            "Assets/Art/Environments/Cicones/Props/cicones_fishing_boat.png", -4);
        if (pequeno != null) { pequeno.transform.localScale = new Vector3(0.75f, 0.75f, 1f); }

        Prop("Mooring_1", b.min.x + 2f, TopoDoChao, "Harbor/lestrigoes_mooring.png", -3);
        Prop("Mooring_2", b.min.x + 11f, TopoDoChao, "Harbor/lestrigoes_mooring.png", -3);
        Prop("Crates_1", b.min.x + 9f, TopoDoChao, "Harbor/lestrigoes_giant_crates.png", -5);
        Encolher(Prop("Scrub_Harbor", b.min.x + 16.5f, TopoDoChao, "Props/lestrigoes_coastal_scrub.png", -2), 0.5f);
    }

    /// <summary>Cidade: casas grandes demais e a torre, com a muralha correndo atrás.</summary>
    private static void PovoarCidade(List<GameObject> chaos)
    {
        if (chaos.Count < 2) { return; }
        Bounds b = Caixa(chaos[1]);

        // A muralha atravessa o trecho inteiro atrás das casas: é o elemento que diz que a
        // cidade é fortificada sem ocupar nenhum espaço jogável.
        GameObject muro = Faixa("City_Wall", b.center.x, TopoDoChao + 4.6f, b.size.x,
            "Midground/lestrigoes_giant_wall.png", -22);
        if (muro != null) { muro.GetComponent<SpriteRenderer>().color = Distancia(0.48f); }

        Prop("House_1", b.min.x + 3f, TopoDoChao, "City/lestrigoes_giant_house.png", -16);
        Prop("House_2", b.min.x + 10.5f, TopoDoChao, "City/lestrigoes_giant_house.png", -16);
        Prop("Tower", b.min.x + 18f, TopoDoChao, "City/lestrigoes_watchtower.png", -18);

        Prop("Stair_City", b.min.x + 6.5f, TopoDoChao, "Gameplay/lestrigoes_stone_stair.png", -6);
        Prop("Crates_City", b.min.x + 14f, TopoDoChao, "Harbor/lestrigoes_giant_crates.png", -5);
        Encolher(Prop("Scrub_City", b.min.x + 21f, TopoDoChao, "Props/lestrigoes_coastal_scrub.png", -2), 0.5f);
    }

    /// <summary>
    /// §"ELEMENTOS DE PERIGO" e a fuga. Todo o kit de destruição vem de Troia e de Cytera —
    /// barricada, entulho, fumaça e destroços já existem e o briefing proíbe duplicar.
    /// </summary>
    private static void PovoarFortificacao(List<GameObject> chaos)
    {
        if (chaos.Count < 3) { return; }
        Bounds b = Caixa(chaos[2]);

        Prop("Wreck", b.min.x + 3f, TopoDoChao, "Harbor/lestrigoes_broken_ship.png", -7);
        Prop("Barricade", b.min.x + 9f, TopoDoChao, "Assets/Art/Environments/Troy/Camp/troy_barricade_01.png", -3);
        Prop("Rubble_1", b.min.x + 12f, TopoDoChao, "Assets/Art/Environments/Troy/Effects/troy_rubble_pile_01.png", -3);
        Prop("Smoke_1", b.min.x + 6f, TopoDoChao, "Assets/Art/Environments/Troy/Effects/troy_smoke_column_01.png", -20);
        Prop("Smoke_2", b.min.x + 14f, TopoDoChao, "Assets/Art/Environments/Troy/Effects/troy_smoke_column_01.png", -20);
        Prop("Fire", b.min.x + 12.8f, TopoDoChao, "Assets/Art/Environments/Troy/Camp/troy_campfire_01.png", -2);
        Prop("Debris", b.min.x + 16f, TopoDoChao, "Assets/Art/Environments/Cytera/Ship/cytera_wreckage.png", -3);
    }

    /// <summary>
    /// A saída da fase é o reembarque. O marcador ganha o mastro de Cytera, que já existe — o
    /// briefing proíbe duplicar o que pode ser reutilizado.
    /// </summary>
    private static void VestirSaida()
    {
        GameObject saida = GameObject.Find("LevelGoal");
        if (saida == null) { return; }
        Encolher(Prop("Art_Goal_Ship", saida.transform.position.x, TopoDoChao,
            "Assets/Art/Environments/Cytera/Ship/cytera_mast_sail.png", -7), 0.85f);
    }

    /// <summary>
    /// O <c>WreckedShip_Background</c> é um marcador de cenário do level design, desenhado
    /// como retângulo. Recebe o casco quebrado na medida dele, e um pouco mais alto: um
    /// naufrágio é mais alto que o marcador que o representa.
    /// </summary>
    private static void VestirNaufragioDeFundo()
    {
        GameObject marcador = GameObject.Find("WreckedShip_Background");
        if (marcador == null) { return; }

        var sr = marcador.GetComponent<SpriteRenderer>();
        float largura = sr != null ? sr.bounds.size.x : 4f;
        GameObject arte = Prop("Art_WreckBackground", marcador.transform.position.x,
            marcador.transform.position.y - 0.5f, "Harbor/lestrigoes_broken_ship.png", -24);
        if (arte != null)
        {
            Sprite a = arte.GetComponent<SpriteRenderer>().sprite;
            float k = largura / a.bounds.size.x;
            arte.transform.localScale = new Vector3(k, k, 1f);
        }
    }

    /// <summary>
    /// Primeiro plano sem <see cref="ParallaxLayer"/>: o fator dele é limitado a [0,1] e
    /// primeiro plano exigiria mais que 1. Arbustos parados com ordem acima do jogador, baixos
    /// e enterrados, para não esconderem ninguém.
    /// </summary>
    private static void MontarPrimeiroPlano()
    {
        float[] posicoes = { -15f, -2f, 12f, 27f, 39f };
        for (int i = 0; i < posicoes.Length; i++)
        {
            GameObject go = Prop($"FG_Scrub_{i}", posicoes[i], TopoDoChao - 1.7f,
                "Props/lestrigoes_coastal_scrub.png", 12);
            if (go == null) { continue; }
            // 0,6 e enterrado 1,7: sobra só a copa na borda de baixo. A 1,3 ele ocupava um
            // quarto da tela e escondia o personagem.
            go.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
        }
    }

    // ---------------------------------------------------------------- utilidades

    private static IEnumerable<GameObject> PorPrefixo(string prefixo) =>
        Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
            .Where(g => g != null && g.name.StartsWith(prefixo) && g.GetComponent<Collider2D>() != null)
            .OrderBy(g => g.transform.position.x);

    private static Bounds Caixa(GameObject go) => go.GetComponent<Collider2D>().bounds;

    private static GameObject Faixa(string nome, float centroX, float topoY, float largura,
        string caminho, int ordem)
    {
        Sprite arte = Arte(caminho);
        if (arte == null) { return null; }

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
        return go;
    }

    /// <summary>Escala uniforme, para não deformar o pixel art.</summary>
    private static void Encolher(GameObject go, float k)
    {
        if (go != null) { go.transform.localScale = new Vector3(k, k, 1f); }
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

    /// <summary>Aceita caminho relativo à pasta da fase ou completo de Assets, que é como os
    /// assets reaproveitados de Troia, Cícones e Cytera entram.</summary>
    private static Sprite Arte(string relativo)
    {
        string caminho = relativo.StartsWith("Assets/") ? relativo : Raiz + relativo;
        Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(caminho);
        if (s == null) { Debug.LogWarning("[Lestrigoes] sprite não encontrado: " + caminho); }
        return s;
    }

    private static Color Cor(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color c);
        return c;
    }
}
