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

        if (Arte("Background/troy_bg_sky.png") == null)
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

    private static void MontarParallax()
    {
        // Fundo de cobertura primeiro, na cor do topo do degradê do céu. Sem ele, qualquer
        // ponto acima da faixa de céu fica sem nada — e a captura mostrou exatamente isso.
        // O sprite de céu tem 5,97 un e a câmera mostra 10.
        Bloco("Sky_Fill", 20f, 6f, 140f, 30f, new Color(100f / 255f, 129f / 255f, 160f / 255f), -60);

        // Fator ALTO é longe: a camada acompanha a câmera e quase não desliza na tela.
        // Os três ladrilham, então o fator pode descer até onde a profundidade aparece — em
        // Ítaca as telas pintadas não ladrilhavam e obrigavam a 0,93-0,97, parallax quase nulo.
        Camada("BG_Troy_Sky", "Background/troy_bg_sky.png", 1.00f, -50, -1.5f);
        Camada("BG_Troy_City", "Background/troy_bg_city.png", 0.86f, -46, -1.2f);
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
    private static void Bloco(string nome, float x, float y, float largura, float altura,
        Color cor, int ordem)
    {
        Sprite quadrado = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Player/PlaceholderSquare.png");
        if (quadrado == null)
        {
            return;
        }

        var go = new GameObject(nome);
        go.transform.SetParent(cenario, false);
        go.transform.position = new Vector3(x, y, 2f);
        go.transform.localScale = new Vector3(largura, altura, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = quadrado;
        sr.color = cor;
        sr.sortingOrder = ordem;
    }

    private static void Camada(string nome, string caminho, float fator, int ordem, float baseY)
    {
        Sprite arte = Arte(caminho);
        if (arte == null)
        {
            return;
        }

        // Troia vai de x=-33 a x=74; a câmera passeia por ~107 unidades.
        const float comprimento = 110f;
        const float larguraDaTela = 18f;
        float deslize = comprimento * (1f - fator);

        var go = new GameObject(nome);
        go.transform.SetParent(cenario, false);
        go.transform.position = new Vector3(-33f + deslize * 0.5f, baseY, 1f);

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
        Prop("War_Shield_1", 0f, "Camp/troy_broken_shield_01.png", 1);
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
