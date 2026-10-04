using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Odisseia.Systems;

/// <summary>
/// Veste a Fase 02 (Troia) com a arte de cenário, sem tocar na jogabilidade:
///
/// Unity.exe -batchmode -nographics -quit -projectPath . -executeMethod TroySceneDresser.Run
/// ou pelo menu Odisseia &gt; Vestir cenario de Troia.
///
/// **Não é um SceneBuilder.** A cena de Troia foi montada à mão e funciona: quatro trechos de
/// chão, três levas de inimigos, tutoriais, KillZone e LevelGoal. Recriá-la por script para
/// poder vesti-la significaria reescrever a jogabilidade junto — risco sem retorno. Este
/// script só ACRESCENTA cenário, sob uma raiz própria, e apaga a raiz anterior antes de
/// recomeçar. Rodar duas vezes dá o mesmo resultado.
///
/// A geometria vem da cena, não de constantes: os trechos de chão são lidos pelos colisores
/// dos objetos <c>Floor_*</c>. Se o level design mudar a largura de um trecho, o cenário
/// acompanha sem ninguém precisar lembrar de atualizar dois lugares.
///
/// **Os vãos não são preenchidos.** Entre Floor_2 e Floor_3 há 10 unidades de abismo, e entre
/// Floor_3 e Floor_4 há 18, atravessados por plataformas. Desenhar chão contínuo por cima
/// deles mostraria piso onde não há colisor — a mesma classe de defeito que o README chama de
/// indistinguível de bug, só que ao contrário: o jogador cairia num chão que parecia sólido.
/// </summary>
public static class TroySceneDresser
{
    private const string ScenePath = "Assets/Scenes/Levels/Level_02_Troia.unity";
    private const string Raiz = "Assets/Art/Environments/Troy/";

    private const float GroundTop = -2f;
    private const float PixelsPorUnidade = 42.857143f;
    private const float LadoDoTile = 32f / PixelsPorUnidade;

    /// <summary>Índice do tile de topo plano na folha Wang; o sólido dele começa na metade.</summary>
    private const int TileTopo = 3;
    private const int TileCheio = 6;

    /// <summary>Nome da raiz que este script cria e destrói. Nada fora dela é tocado.</summary>
    private const string RaizDoCenario = "TroyScenery";

    private static Transform cenario;

    [MenuItem("Odisseia/Vestir cenario de Troia")]
    public static void Vestir()
    {
        Executar();
    }

    public static void Run()
    {
        bool ok = Executar();
        if (Application.isBatchMode)
        {
            EditorApplication.Exit(ok ? 0 : 1);
        }
    }

    private static bool Executar()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        if (Arte("Background/troy_bg_sky_war.png") == null)
        {
            Debug.LogError("[Troia] arte nao importada — rode 'node Tools/unity-import-troy.js'");
            return false;
        }

        LimparCenarioAntigo();

        var raiz = new GameObject(RaizDoCenario);
        cenario = raiz.transform;

        DesligarFundoDeItaca();
        MontarParallax();
        List<Bounds> trechos = TrechosDeChao();
        VestirChao(trechos);
        MontarMuralha();
        PovoarAcampamento(trechos);
        VestirGeometriaDeJogo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log($"[Troia] cenario vestido: {trechos.Count} trechos de chao, parallax e props");
        return true;
    }

    private static void LimparCenarioAntigo()
    {
        foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (go != null && go.name == RaizDoCenario && go.transform.parent == null)
            {
                Object.DestroyImmediate(go);
            }
        }
    }

    /// <summary>
    /// As três camadas pintadas da Fase 01 continuam na cena de Troia, com a arte de Ítaca —
    /// paisagem grega tranquila atrás de um cerco. Só o desenho é desligado; os objetos ficam,
    /// para quem quiser voltar atrás não precisar remontar nada.
    /// </summary>
    private static void DesligarFundoDeItaca()
    {
        foreach (string nome in new[] { "BG_Far", "BG_Mid", "BG_Near" })
        {
            GameObject go = GameObject.Find(nome);
            var sr = go != null ? go.GetComponent<SpriteRenderer>() : null;
            if (sr != null)
            {
                sr.enabled = false;
            }
        }
    }

    /// <summary>
    /// Fundo do TROY-02, nas cores do master aprovado (<c>troy_bg_master</c>): céu de fim de
    /// tarde, montanhas azuis e a cidade de Troia como peça única. As alturas reproduzem a prova
    /// de composição do pack (<c>Docs/Art/PixelLab/_troy02_city.png</c>) com o jogador no chão.
    /// </summary>
    private static void MontarParallax()
    {
        LerCamera();

        // Cobertura acima do céu, na cor MEDIDA da primeira linha de troy_bg_sky_war (380 dos
        // 380 px). O céu tem 5,6 un e a câmera mostra 10.
        Bloco("Sky_Fill", (cameraMin + cameraMax) * 0.5f, 12f, 140f, 30f, Cor("#ca8f67"), -60);

        // Fator ALTO é longe: a camada acompanha a câmera e quase não desliza na tela.
        Camada("BG_Troy_Sky", "Background/troy_bg_sky_war.png", 0.95f, -50, -1.6f);
        GameObject montanhas = Camada("BG_Troy_Mountains", "Background/troy_bg_mountains.png", 0.85f, -48, -2.1f);

        // A planície do master, por baixo das montanhas e presa a elas: quando a câmera sobe num
        // salto, o chão do mundo desce na tela e abre uma faixa entre ele e a base das camadas.
        // Sem esta faixa, o vão mostraria céu ABAIXO das montanhas. Cor MEDIDA da planície do
        // master (linhas 290-330, 52% dos pixels).
        if (montanhas != null)
        {
            var sr = montanhas.GetComponent<SpriteRenderer>();
            GameObject planicie = Bloco("BG_Troy_Plain", montanhas.transform.position.x,
                montanhas.transform.position.y - 5f, sr.size.x, 10f, Cor("#b28b54"), -49);
            planicie.transform.SetParent(montanhas.transform, true);
        }

        Cidade();

        // A faixa de névoa saiu do parallax. Como camada de largura inteira ela vira um
        // retângulo cinza atravessando a tela, com borda superior reta — lê como erro, não como
        // ar quente. Fumaça no horizonte fica melhor localizada, pelas colunas de fumaça.
        // A faixa de névoa saiu do parallax. Como camada de largura inteira ela vira um
        // retângulo cinza atravessando a tela, com borda superior reta — lê como erro, não como
        // ar quente. Fumaça no horizonte fica melhor localizada, pelas colunas de fumaça.
        Prop("FX_Smoke_1", 8f, "Effects/troy_smoke_column_01.png", -5);
        Prop("FX_Smoke_2", 34f, "Effects/troy_smoke_column_01.png", -5);
        Prop("FX_Smoke_3", 62f, "Effects/troy_smoke_column_01.png", -5);
    }

    /// <summary>
    /// Retângulo de cor chapada. É a ferramenta certa para fundo de cobertura, e a errada para
    /// qualquer outra coisa.
    /// </summary>
    private static GameObject Bloco(string nome, float x, float y, float largura, float altura,
        Color cor, int ordem)
    {
        Sprite quadrado = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Player/PlaceholderSquare.png");
        if (quadrado == null)
        {
            return null;
        }

        var go = new GameObject(nome);
        go.transform.SetParent(cenario, false);
        go.transform.position = new Vector3(x, y, 2f);
        go.transform.localScale = new Vector3(largura, altura, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = quadrado;
        sr.color = cor;
        sr.sortingOrder = ordem;
        return go;
    }

    // --- Câmera -----------------------------------------------------------------------------

    /// <summary>
    /// Meia largura de tela com folga: 16:9 dá 8,9 un, mas WebGL e mobile em paisagem chegam a
    /// 20:9 (11,1). A camada cobre a mais larga.
    /// </summary>
    private const float MeiaTela = 12f;

    /// <summary>Altura da câmera com o jogador no chão: pés em −2 mais o offset de 1 do follow.</summary>
    private const float CameraNoChao = GroundTop + 1f;

    private static Vector3 cameraInicio;
    private static float cameraMin;
    private static float cameraMax;

    /// <summary>
    /// O <see cref="ParallaxLayer"/> mede o deslocamento a partir de onde a câmera ESTÁ quando
    /// ele inicia — a posição dela na cena, porque o follow não salta para o jogador, desliza.
    /// O vestidor anterior supunha a câmera em x=−33 e ela está em 0: o céu saía 33 un fora do
    /// lugar, e o começo da fase mostrava só o bloco de cobertura. Lido da cena, não suposto.
    /// O percurso vem dos limites do <see cref="CameraFollow"/>.
    /// </summary>
    private static void LerCamera()
    {
        Camera principal = Camera.main;
        cameraInicio = principal != null ? principal.transform.position : Vector3.zero;
        cameraMin = -34f + 8.9f;
        cameraMax = 76f - 8.9f;

        var follow = principal != null ? principal.GetComponent<CameraFollow>() : null;
        if (follow != null)
        {
            var so = new SerializedObject(follow);
            if (so.FindProperty("useBounds").boolValue)
            {
                float meia = so.FindProperty("orthographicSize").floatValue * 16f / 9f;
                cameraMin = so.FindProperty("minBounds").vector2Value.x + meia;
                cameraMax = so.FindProperty("maxBounds").vector2Value.x - meia;
            }
        }
    }

    /// <summary>
    /// Posição de autoria para que, com a câmera em <paramref name="cameraAlvo"/>, a camada
    /// esteja em <paramref name="alvo"/> — o inverso do que o <see cref="ParallaxLayer"/> faz.
    /// </summary>
    private static float Autoria(float alvo, float cameraAlvo, float cameraDeInicio, float fator)
    {
        return alvo - (cameraAlvo - cameraDeInicio) * fator;
    }

    /// <summary>
    /// Faixa ladrilhada que cobre a tela em todo o percurso da câmera. <paramref name="baseY"/> é
    /// a altura da base com o jogador no chão.
    /// </summary>
    private static GameObject Camada(string nome, string caminho, float fator, int ordem, float baseY)
    {
        Sprite arte = Arte(caminho);
        if (arte == null)
        {
            return null;
        }

        // A camada desliza (1 − fator) do percurso em relação à tela; centrada no meio do
        // percurso, precisa de metade disso de cada lado, mais a tela.
        float meio = (cameraMin + cameraMax) * 0.5f;
        float deslize = (cameraMax - cameraMin) * (1f - fator);

        var go = new GameObject(nome);
        go.transform.SetParent(cenario, false);
        go.transform.position = new Vector3(
            Autoria(meio, meio, cameraInicio.x, fator),
            Autoria(baseY, CameraNoChao, cameraInicio.y, fator),
            1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = arte;
        sr.sortingOrder = ordem;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.tileMode = SpriteTileMode.Continuous;
        // Altura NATIVA: esticar size.y faz a faixa repetir na vertical e aparecer uma listra.
        sr.size = new Vector2(deslize + MeiaTela * 2f + 2f, arte.bounds.size.y);

        Parallax(go, fator);
        return go;
    }

    /// <summary>
    /// A cidade NÃO ladrilha: Troia é um marco, uma só. Entra pela direita no campo de batalha,
    /// fica inteira em quadro na aproximação da Área 4 (câmera em x≈50) e some atrás da muralha
    /// de gameplay no fim. A base afunda 0,66 un no chão — a mesma linha da prova de composição,
    /// onde a planície do master cobre o pé da muralha.
    /// </summary>
    private static void Cidade()
    {
        const float fator = 0.70f;
        const float cameraNoCentro = 50f;
        const float baseY = GroundTop - 0.66f;

        Sprite arte = Arte("Background/troy_bg_city_distant.png");
        if (arte == null)
        {
            return;
        }

        var go = new GameObject("BG_Troy_City");
        go.transform.SetParent(cenario, false);
        go.transform.position = new Vector3(
            Autoria(cameraNoCentro, cameraNoCentro, cameraInicio.x, fator),
            Autoria(baseY, CameraNoChao, cameraInicio.y, fator),
            1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = arte;
        sr.sortingOrder = -46;

        Parallax(go, fator);
    }

    private static void Parallax(GameObject go, float fator)
    {
        var parallax = go.AddComponent<ParallaxLayer>();
        var so = new SerializedObject(parallax);
        so.FindProperty("parallaxFactor").floatValue = fator;
        so.ApplyModifiedProperties();
    }

    private static Color Cor(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color cor);
        return cor;
    }

    /// <summary>
    /// Lê os trechos de chão dos colisores da cena, em vez de repetir as coordenadas aqui.
    /// </summary>
    private static List<Bounds> TrechosDeChao()
    {
        var trechos = new List<Bounds>();
        foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (go == null || !go.name.StartsWith("Floor_"))
            {
                continue;
            }

            var col = go.GetComponent<BoxCollider2D>();
            if (col != null)
            {
                trechos.Add(col.bounds);
            }
        }

        return trechos.OrderBy(b => b.min.x).ToList();
    }

    private static void VestirChao(List<Bounds> trechos)
    {
        Sprite topo = Tile("Terrain/troy_tiles_earth.png", TileTopo);
        Sprite cheio = Tile("Terrain/troy_tiles_earth.png", TileCheio);
        if (topo == null || cheio == null)
        {
            Debug.LogWarning("[Troia] tileset de terra ausente; chao segue como bloco");
            return;
        }

        for (int i = 0; i < trechos.Count; i++)
        {
            Bounds b = trechos[i];
            float largura = b.size.x;
            float centro = b.center.x;

            // O tile de topo é posicionado pelo CENTRO na linha do chão: os cantos de cima do
            // Wang são "upper" e o desenho sólido começa na metade do tile. Alinhar pelo topo
            // afundaria o personagem meio tile.
            Faixa($"Terrain_{i}_Top", centro, GroundTop, largura, LadoDoTile, topo, -1);

            const int fiadas = 6;
            float altura = fiadas * LadoDoTile;
            Faixa($"Terrain_{i}_Fill", centro, GroundTop - LadoDoTile * 0.5f - altura * 0.5f,
                largura, altura, cheio, -2);
        }

        // O desenho dos blocos sai; o colisor fica. É o mesmo padrão da Fase 01.
        foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (go == null || !go.name.StartsWith("Floor_"))
            {
                continue;
            }

            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.enabled = false;
            }
        }
    }

    private static void Faixa(string nome, float x, float y, float largura, float altura,
        Sprite tile, int ordem)
    {
        var go = new GameObject(nome);
        go.transform.SetParent(cenario, false);
        go.transform.position = new Vector3(x, y, 0f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = tile;
        sr.sortingOrder = ordem;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.tileMode = SpriteTileMode.Continuous;
        sr.size = new Vector2(largura, altura);
    }

    /// <summary>
    /// A muralha fecha o fim da fase, atrás do último trecho de chão: é o objetivo que o
    /// jogador vê crescer. Ela tem 11,9 un e a câmera mostra 10 — não cabe em quadro, e é isso
    /// que faz Troia parecer grande.
    /// </summary>
    // --- Geometria de jogo (N-08) -----------------------------------------------------------

    /// <summary>
    /// Os oito placeholders de gameplay que o PlaceholderProbe acusava, vestidos com arte que já existia (0 gerações).
    /// A posição sai do COLISOR de cada um, nunca de constante: o topo do desenho cai no topo do colisor, que é onde o pé
    /// pisa. Só o SpriteRenderer do placeholder é desligado — colisor, layer e scripts ficam como estão.
    ///
    /// O LevelGoal perde o mastro e a vela chapados: a narrativa da saída é o CAVALO ("o cavalo cumpriu o que a lança
    /// não conseguiu"), que não tem asset (PIXELLAB-FUTURE). Até lá, a escada de cerco já posta em x=71, sobre o gatilho,
    /// marca a chegada às muralhas.
    /// </summary>
    private static void VestirGeometriaDeJogo()
    {
        // Ponte e blocos têm pivô no CENTRO: centro = topo do colisor − meia altura do desenho.
        if (Colisor("Platform_Bridge", out Bounds ponte))
        {
            Sprite tabuas = Arte("Gameplay/troy_plank_bridge.png");
            if (tabuas != null)
            {
                Peca("Bridge_Planks", tabuas, ponte.center.x, ponte.max.y - tabuas.bounds.extents.y, -1, false);
            }
        }

        Sprite bloco = Arte("Gameplay/troy_fallen_block.png");
        for (int i = 1; i <= 3 && bloco != null; i++)
        {
            if (Colisor($"Gauntlet_{i}", out Bounds b))
            {
                // O do meio espelhado: três blocos idênticos em fila liam como ladrilho.
                Peca($"Gauntlet_Block_{i}", bloco, b.center.x, b.max.y - bloco.bounds.extents.y, -1, i == 2);
            }
        }

        // Pedra única (Tools/build-troy-obstacle.js), pivô na base, no chão sob o colisor de 0,6 × 0,8.
        if (Colisor("Obstacle_Low", out Bounds obstaculo))
        {
            Sprite pedra = Arte("Gameplay/troy_fallen_stone.png");
            if (pedra != null) { Peca("Obstacle_Stone", pedra, obstaculo.center.x, obstaculo.min.y, 1, false); }
        }

        // Limite esquerdo: é o lado GREGO (acampamento), então barricadas de toras do próprio acampamento, empilhadas até
        // a altura do colisor, em espelho alternado — não pedra troiana. (O tile cheio de madeira foi tentado e lia como
        // uma coluna de terra: o miolo dele é a mesma textura pontilhada do chão.)
        if (Colisor("Wall_Troia", out Bounds muro))
        {
            Sprite toras = Arte("Camp/troy_barricade_01.png");
            for (int i = 0; toras != null && muro.min.y + i * toras.bounds.size.y < muro.max.y - 0.5f; i++)
            {
                Peca($"Palisade_{i}", toras, muro.center.x, muro.min.y + i * toras.bounds.size.y, -1, i % 2 == 1);
            }
        }

        foreach (string nome in new[] { "Platform_Bridge", "Gauntlet_1", "Gauntlet_2", "Gauntlet_3", "Obstacle_Low", "Wall_Troia" })
        {
            DesligarDesenho(GameObject.Find(nome));
        }

        GameObject alvo = GameObject.Find("LevelGoal");
        if (alvo != null)
        {
            foreach (SpriteRenderer sr in alvo.GetComponentsInChildren<SpriteRenderer>(true)) { sr.enabled = false; }
        }
    }

    private static bool Colisor(string nome, out Bounds b)
    {
        GameObject go = GameObject.Find(nome);
        var col = go != null ? go.GetComponent<Collider2D>() : null;
        b = col != null ? col.bounds : default;
        if (col == null) { Debug.LogWarning($"[Troia] {nome} sem colisor na cena — não vestido"); }
        return col != null;
    }

    private static void Peca(string nome, Sprite arte, float x, float y, int ordem, bool espelhar)
    {
        var go = new GameObject(nome);
        go.transform.SetParent(cenario, false);
        go.transform.position = new Vector3(x, y, 0f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = arte;
        sr.sortingOrder = ordem;
        sr.flipX = espelhar;
    }

    private static void DesligarDesenho(GameObject go)
    {
        var sr = go != null ? go.GetComponent<SpriteRenderer>() : null;
        if (sr != null) { sr.enabled = false; }
    }

    private static void MontarMuralha()
    {
        Sprite muro = Arte("Architecture/troy_wall_section_01.png");
        if (muro == null)
        {
            return;
        }

        var go = new GameObject("Troy_Wall");
        go.transform.SetParent(cenario, false);
        // Começa em x=68, logo depois do último trecho jogável (44 a 74). Antes ela ocupava a
        // tela inteira já no meio da aproximação, e uma muralha que preenche o quadro deixa de
        // ler como muralha — vira textura de fundo. Assim ela ENTRA em quadro pela direita e
        // cresce conforme o jogador avança, que é o efeito da Área 4.
        go.transform.position = new Vector3(88f, GroundTop, 0.5f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = muro;
        sr.sortingOrder = -4;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.tileMode = SpriteTileMode.Continuous;
        sr.size = new Vector2(40f, muro.bounds.size.y);

        Prop("Troy_Gate", 84f, "Architecture/troy_gate_01.png", -3);
        Prop("Troy_Tower_L", 66f, "Architecture/troy_tower_01.png", -3);
        Prop("Troy_Tower_R", 100f, "Architecture/troy_tower_01.png", -3);
    }

    private static void PovoarAcampamento(List<Bounds> trechos)
    {
        if (trechos.Count == 0)
        {
            return;
        }

        // Área 1 e 2: chegada e acampamento, no primeiro trecho.
        Prop("Camp_Tent_1", -30f, "Camp/troy_tent_large_01.png", -1);
        Prop("Camp_Tent_2", -25f, "Camp/troy_tent_small_01.png", -1);
        Prop("Camp_Fire", -21f, "Camp/troy_campfire_01.png", 1);
        Prop("Camp_Supplies", -18f, "Camp/troy_supply_pile_01.png", 1);
        Prop("Camp_Banner", -27.5f, "Camp/troy_banner_pole_01.png", -1);

        // Área 3: campo de batalha, no segundo e terceiro trechos.
        Prop("War_Barricade_1", -10f, "Camp/troy_barricade_01.png", 1);
        Prop("War_Spears_1", -6f, "Camp/troy_spear_cluster_01.png", 1);
        // −2,5 e não 0: em 0 o escudo (ordem 1) cobria metade do altar do checkpoint (x=−1), e à direita esconderia a
        // moeda de x=2.
        Prop("War_Shield_1", -2.5f, "Camp/troy_broken_shield_01.png", 1);
        Prop("War_Banner_2", 2.5f, "Camp/troy_banner_pole_01.png", -1);
        Prop("War_Barricade_2", 17f, "Camp/troy_barricade_01.png", 1);
        Prop("War_Spears_2", 23f, "Camp/troy_spear_cluster_01.png", 1);

        // Área 4: aproximação das muralhas.
        Prop("War_Shield_2", 48f, "Camp/troy_broken_shield_01.png", 1);
        Prop("War_Spears_3", 52f, "Camp/troy_spear_cluster_01.png", 1);
        Prop("War_Rubble_1", 45f, "Effects/troy_rubble_pile_01.png", 1);
        Prop("War_Rubble_2", 68f, "Effects/troy_rubble_pile_01.png", 1);
        Prop("War_Ladder", 71f, "Effects/troy_siege_ladder_01.png", -2);

        // Vegetação rala: a fase é sítio de guerra em planalto seco.
        foreach (float x in new[] { -28f, -19f, -8f, 16f, 24f, 47f, 57f })
        {
            Prop($"Flora_{Mathf.RoundToInt(x)}", x, "Effects/troy_dry_bush_01.png", 0);
        }

        // Primeiro plano: pedras grandes na frente do jogador, dando profundidade.
        Prop("FG_Rocks_1", -14f, "Effects/troy_foreground_rocks_01.png", 3);
        Prop("FG_Rocks_2", 21f, "Effects/troy_foreground_rocks_01.png", 3);
    }

    private static GameObject Prop(string nome, float x, string caminho, int ordem)
    {
        Sprite arte = Arte(caminho);
        if (arte == null)
        {
            return null;
        }

        var go = new GameObject(nome);
        go.transform.SetParent(cenario, false);
        // O pivô da arte de cenário é BottomCenter: a base cai na linha do chão.
        go.transform.position = new Vector3(x, GroundTop, 0f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = arte;
        sr.sortingOrder = ordem;
        return go;
    }

    private static Sprite Arte(string relativo)
    {
        string caminho = Raiz + relativo;
        var direto = AssetDatabase.LoadAssetAtPath<Sprite>(caminho);
        if (direto != null)
        {
            return direto;
        }

        foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(caminho))
        {
            if (asset is Sprite sprite)
            {
                return sprite;
            }
        }

        return null;
    }

    /// <summary>
    /// Um sprite do tileset Wang pelo índice documentado — row-major a partir do topo da folha,
    /// que é a ordem do array no JSON do PixelLab.
    /// </summary>
    private static Sprite Tile(string relativo, int indice)
    {
        string caminho = Raiz + relativo;
        string kit = System.IO.Path.GetFileNameWithoutExtension(caminho);
        string alvo = kit + "_" + indice.ToString("00");

        foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(caminho))
        {
            if (asset is Sprite sprite && sprite.name == alvo)
            {
                return sprite;
            }
        }

        return null;
    }
}
