using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Odisseia.Systems;

/// <summary>
/// Veste a fase 05 com o cenário de Ciclopes:
///
/// Unity.exe -batchmode -quit -nographics -projectPath . -executeMethod CiclopesSceneDresser.Run
///
/// Mesmo desenho dos vestidores de Troia, Cícones e Cytera — não é sistema novo.
///
/// O que esta fase tem de próprio é a **transição de exterior para caverna**, que o briefing
/// pede que aconteça pelo próprio cenário. Ela acontece em três coisas ao mesmo tempo,
/// governadas pela posição em x:
///
/// 1. o terreno troca de terra selvagem para rocha de caverna;
/// 2. o teto de estalactites entra e desce;
/// 3. um véu escuro cobre o céu, cada vez mais opaco.
///
/// A fronteira sai da geometria: <c>Floor_Narrow</c> é o corredor que estreita, e
/// <c>Floor_3_Boss</c> é a câmara de Polifemo. Nenhuma coordenada de transição está escrita
/// aqui — se o level design mover o corredor, a caverna acompanha.
/// </summary>
public static class CiclopesSceneDresser
{
    private const string ScenePath = "Assets/Scenes/Levels/Level_05_Ciclopes.unity";
    private const string Raiz = "Assets/Art/Environments/Ciclopes/";
    private const string RaizDoCenario = "CiclopesScenery";

    private const float TopoDoChao = -2f;

    /// <summary>Onde a câmera está no primeiro quadro; o ParallaxLayer mede a partir daqui.</summary>
    private const float CameraInicial = 0f;
    /// <summary>Meio do passeio: o jogador vai de x=-14 a x=48.</summary>
    private const float CameraMeio = 17f;
    private const float CameraCurso = 62f;

    private static Transform cenario;
    private static float bocaDaCaverna;   // x onde o exterior acaba

    [MenuItem("Odisseia/Vestir Ciclopes")]
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
            Debug.LogError("[Ciclopes] nenhum Floor_* encontrado — cenário não montado");
            return false;
        }

        GameObject corredor = chaos.FirstOrDefault(g => g.name == "Floor_Narrow");
        bocaDaCaverna = corredor != null ? Caixa(corredor).min.x : 16f;

        DesligarPlaceholders();
        MontarParallax();
        VestirChao(chaos);
        MontarCaverna(chaos);
        PovoarExterior(chaos);
        PovoarCamaraDePolifemo(chaos);
        MontarPrimeiroPlano();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[Ciclopes] cenário montado: {cenario.childCount} objetos, boca da caverna em x={bocaDaCaverna:0.0}");
        return true;
    }

    /// <summary>
    /// Desliga o DESENHO dos placeholders cobertos, nunca o objeto: colisor, nome e referências
    /// do level design continuam. Foi o PlaceholderProbe que mostrou, em Cícones, que esquecer
    /// isto deixa o retângulo chapado desenhando por cima da arte nova.
    /// </summary>
    private static void DesligarPlaceholders()
    {
        string[] prefixos = { "Floor_", "Platform_", "Rock_", "Narrow_Rock_", "Sky_Background" };
        foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (go == null || !prefixos.Any(pre => go.name.StartsWith(pre))) { continue; }
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null) { sr.enabled = false; }
        }
    }

    // ---------------------------------------------------------------- fundo

    private static void MontarParallax()
    {
        Bloco("Sky_Fill", 17f, 9f, 130f, 34f, Cor("#6481a0"), -60);
        Bloco("Valley_Fill", 17f, -10f, 130f, 12f, Cor("#543823"), -30);

        Camada("BG_Sky", "Background/ciclopes_bg_sky.png", 1.00f, -50, -6f);
        Camada("BG_Mountains", "Background/ciclopes_bg_mountains.png", 0.90f, -46, -0.5f);
        Camada("BG_Sea", "Background/ciclopes_bg_sea.png", 0.80f, -44, -2.2f);
        Camada("BG_Hills", "Background/ciclopes_bg_hills.png", 0.68f, -42, -1.6f);
        Camada("BG_Cliffs", "Background/ciclopes_bg_cliffs.png", 0.52f, -40, -2.0f);
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

    // ---------------------------------------------------------------- chão

    /// <summary>
    /// O terreno de cada trecho sai da POSIÇÃO dele: terra selvagem antes da boca da caverna,
    /// rocha depois. É a progressão do briefing dita pela geometria, e não por uma lista
    /// paralela de coordenadas que sairia de sincronia com o level design.
    /// </summary>
    private static void VestirChao(List<GameObject> chaos)
    {
        foreach (GameObject go in chaos)
        {
            Bounds b = Caixa(go);
            bool dentro = b.center.x >= bocaDaCaverna;
            string tile = dentro ? "Cave/ciclopes_tiles_cavefloor.png" : "Gameplay/ciclopes_tiles_wild.png";

            Faixa($"Ground_{go.name}", b.center.x, b.max.y, b.size.x, tile, -10);
            Bloco($"Bank_{go.name}", b.center.x, b.max.y - 9f, b.size.x, 12f,
                dentro ? Cor("#272a29") : Cor("#543823"), -12);
        }

        // A saliência e as pedras de apoio são plataformas de gameplay: precisam ler como
        // plataforma, não como decoração. Recebem pedregulho na medida do colisor.
        foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
                     .Where(g => g != null && (g.name.StartsWith("Rock_") || g.name.StartsWith("Narrow_Rock_")
                         || g.name.StartsWith("Platform_"))).OrderBy(g => g.transform.position.x))
        {
            if (go.GetComponent<Collider2D>() == null) { continue; }
            Bounds b = Caixa(go);

            // Plataforma LARGA E BAIXA é laje, não pedregulho. Vestida de pedregulho ela
            // estourava: EscalarPara usa o maior dos dois fatores para cobrir o colisor, e num
            // colisor de 4,0 x 0,5 isso multiplica a altura por 0,89 — um pedregulho de 3,3 un
            // flutuando 0,7 un acima do chão, que é o que a captura mostrou.
            if (b.size.x >= 1.5f)
            {
                bool dentro = b.center.x >= bocaDaCaverna;
                Faixa($"Art_{go.name}", b.center.x, b.max.y, b.size.x,
                    dentro ? "Cave/ciclopes_tiles_cavefloor.png" : "Gameplay/ciclopes_tiles_wild.png", -2);
                continue;
            }

            GameObject arte = Prop($"Art_{go.name}", b.center.x, b.min.y,
                "Props/ciclopes_boulder_cluster.png", -2);
            EscalarPara(arte, b);
        }
    }

    private static void Faixa(string nome, float centroX, float topoY, float largura, string caminho, int ordem)
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

    // ---------------------------------------------------------------- caverna

    /// <summary>
    /// A caverna, montada em vez de desenhada como um sprite de "boca de caverna".
    ///
    /// Gerar esse sprite falhou duas vezes — o modelo devolve alvenaria com arco, que é a
    /// arquitetura que o briefing proíbe — e pintá-lo por código saiu pior, porque rocha é
    /// orgânica. Montar de peças resolve os dois: a parede vem do tileset gerado, o vão vem de
    /// um bloco escuro, e a moldura vem dos pedregulhos.
    ///
    /// O escurecimento é progressivo em três véus de opacidade crescente, e não um corte: o
    /// briefing pede que a caverna fique gradualmente mais escura.
    /// </summary>
    private static void MontarCaverna(List<GameObject> chaos)
    {
        float fim = chaos.Max(g => Caixa(g).max.x);
        float largura = fim - bocaDaCaverna;

        // Rocha maciça atrás de TODA a região da caverna, subindo bem acima do que a câmera
        // alcança. Sem ela o céu de meio-dia continuava aparecendo acima da faixa de parede —
        // dentro de uma caverna.
        Bloco("Cave_Backdrop", bocaDaCaverna + largura * 0.5f, 8f, largura + 4f, 26f,
            Cor("#272a29"), -25);

        // Parede ao fundo e teto de estalactites, do corredor até o fim da fase.
        Faixa("Cave_Wall", bocaDaCaverna + largura * 0.5f, 5.5f, largura + 4f,
            "Cave/ciclopes_tiles_cavewall.png", -20);
        Faixa("Cave_Ceiling", bocaDaCaverna + largura * 0.5f, 4.2f, largura + 4f,
            "Cave/ciclopes_stalactites.png", -8);

        // Véus: três faixas de preto cada vez mais opacas. Um corte seco na boca leria como
        // parede invisível; o degradê lê como profundidade.
        float[] alphas = { 0.30f, 0.52f, 0.72f };
        for (int i = 0; i < alphas.Length; i++)
        {
            float x0 = bocaDaCaverna + largura * (i / (float)alphas.Length);
            float w = largura / alphas.Length + 0.5f;
            Bloco($"Cave_Veil_{i}", x0 + w * 0.5f, 2f, w, 22f,
                new Color(0.06f, 0.07f, 0.07f, alphas[i]), -6);
        }

        // Moldura da boca: pedregulhos empilhados no limite entre exterior e caverna. Sem
        // eles a passagem é um corte vertical seco entre céu e rocha, que lê como erro de
        // montagem em vez de entrada.
        for (int i = 0; i < 2; i++)
        {
            // No CHAO, e nao empilhados: a primeira versao punha o segundo e o terceiro em
            // y=-0,5 e y=+1, flutuando no ar. E pequenos: o pedregulho tem 4,48 un de largura,
            // e a 0,85 ele sozinho tomava um quinto da tela.
            GameObject p2 = Prop($"Cave_Mouth_Rock_{i}", bocaDaCaverna - 1.2f + i * 1.6f,
                TopoDoChao, "Props/ciclopes_boulder_large.png", -5);
            if (p2 == null) { continue; }
            float k = 0.5f - i * 0.14f;
            p2.transform.localScale = new Vector3(k, k, 1f);
        }

        // Pilares naturais espaçados, que dão profundidade ao vão sem fechar o caminho.
        for (int i = 0; i < 4; i++)
        {
            float x = bocaDaCaverna + 3f + i * (largura - 6f) / 3f;
            GameObject pilar = Prop($"Cave_Pillar_{i}", x, TopoDoChao, "Cave/ciclopes_rock_pillar.png", -15);
            if (pilar != null) { pilar.transform.localScale = new Vector3(1f, 0.85f + i * 0.06f, 1f); }
        }

        // Tochas: a única fonte de luz do interior, e o que mantém a leitura do gameplay
        // enquanto o véu escurece.
        for (int i = 0; i < 5; i++)
        {
            float x = bocaDaCaverna + 2f + i * (largura - 4f) / 4f;
            GameObject t = Prop($"Cave_Torch_{i}", x, TopoDoChao + 1.6f, "Props/ciclopes_torch_wall.png", -4);
            if (t != null) { t.transform.localScale = new Vector3(1.3f, 1.3f, 1f); }
        }
    }

    // ---------------------------------------------------------------- povoamento

    /// <summary>Praia, costa e interior da ilha: vegetação rareando conforme avança.</summary>
    private static void PovoarExterior(List<GameObject> chaos)
    {
        foreach (GameObject go in chaos)
        {
            Bounds b = Caixa(go);
            if (b.center.x >= bocaDaCaverna) { continue; }

            // Densidade de vegetação cai com a distância: é assim que "menos vegetação, mais
            // rocha" acontece pelo cenário, como o briefing pede.
            float t = Mathf.InverseLerp(-18f, bocaDaCaverna, b.center.x);
            int arvores = Mathf.Max(0, Mathf.RoundToInt(Mathf.Lerp(3f, 0f, t)));
            for (int i = 0; i < arvores; i++)
            {
                float x = b.min.x + b.size.x * (i + 0.5f) / arvores;
                Prop($"Olive_{go.name}_{i}", x, TopoDoChao, "Midground/ciclopes_wild_olive.png", -16);
            }

            int pedras = Mathf.RoundToInt(Mathf.Lerp(1f, 2f, t));
            for (int i = 0; i < pedras; i++)
            {
                float x = b.min.x + b.size.x * (i + 0.8f) / (pedras + 1);
                GameObject r = Prop($"Rocks_{go.name}_{i}", x, TopoDoChao,
                    "Props/ciclopes_boulder_cluster.png", -3);
                // O agrupamento tem 4,48 un de largura, um quarto da tela: em escala 1 ele
                // competia com as plataformas em vez de decorar o chão.
                if (r != null) { r.transform.localScale = new Vector3(0.3f, 0.3f, 1f); }
            }
        }
    }

    /// <summary>
    /// §"CAVERNA DE POLIFEMO". Tudo aqui existe para transmitir ESCALA: os objetos são de um
    /// tamanho que só faz sentido para quem é grande. O boss não é criado — o espaço central
    /// fica livre para ele ser colocado depois.
    /// </summary>
    private static void PovoarCamaraDePolifemo(List<GameObject> chaos)
    {
        GameObject camara = chaos.FirstOrDefault(g => g.name == "Floor_3_Boss");
        if (camara == null) { return; }

        Bounds b = Caixa(camara);
        Prop("Polifemo_Pen", b.min.x + 2.5f, TopoDoChao, "Special/ciclopes_giant_pen.png", -14);
        Prop("Polifemo_Jars", b.min.x + 6.5f, TopoDoChao, "Special/ciclopes_giant_jars.png", -13);
        Prop("Polifemo_Logs", b.max.x - 8.5f, TopoDoChao, "Special/ciclopes_giant_logs.png", -13);
        Prop("Polifemo_Staff", b.max.x - 4.0f, TopoDoChao, "Special/ciclopes_giant_staff.png", -12);
        Prop("Polifemo_Bones", b.min.x + 9.5f, TopoDoChao, "Props/ciclopes_bones.png", -2);
        Prop("Polifemo_Bones_2", b.max.x - 6.0f, TopoDoChao, "Props/ciclopes_bones.png", -2);

        // A fogueira fica no MEIO do trecho, que é onde o combate acontece: ela é o ponto de
        // luz que mantém o chão legível na parte mais escura da fase.
        GameObject fogo = Prop("Polifemo_Fire", b.center.x, TopoDoChao, "Special/ciclopes_giant_fire.png", -11);
        if (fogo != null) { fogo.transform.localScale = new Vector3(1.4f, 1.4f, 1f); }
    }

    /// <summary>
    /// Primeiro plano sem <see cref="ParallaxLayer"/>: o fator dele é limitado a [0,1] e
    /// primeiro plano exigiria mais que 1. Em vez de mexer num componente que serve quatro
    /// fases, os pedregulhos da frente ficam parados com ordem acima do jogador.
    /// </summary>
    private static void MontarPrimeiroPlano()
    {
        float[] posicoes = { -16f, -2f, 14f, 30f, 45f };
        for (int i = 0; i < posicoes.Length; i++)
        {
            GameObject go = Prop($"FG_Rock_{i}", posicoes[i], TopoDoChao - 1.9f,
                "Props/ciclopes_boulder_large.png", 12);
            if (go == null) { continue; }
            // 0,55 e enterrado 1,9: sobra só a calota na borda de baixo do quadro. A 1,1 o
            // pedregulho ocupava 4,9 un de uma tela de 17,8 e escondia o personagem.
            go.transform.localScale = new Vector3(0.55f, 0.55f, 1f);
        }
    }

    // ---------------------------------------------------------------- utilidades

    private static IEnumerable<GameObject> PorPrefixo(string prefixo) =>
        Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
            .Where(g => g != null && g.name.StartsWith(prefixo) && g.GetComponent<Collider2D>() != null)
            .OrderBy(g => g.transform.position.x);

    private static Bounds Caixa(GameObject go) => go.GetComponent<Collider2D>().bounds;

    private static void EscalarPara(GameObject go, Bounds alvo)
    {
        if (go == null) { return; }
        Sprite s = go.GetComponent<SpriteRenderer>().sprite;
        // Escala UNIFORME: deformar pixel art para caber num colisor denuncia na hora.
        float k = Mathf.Max(alvo.size.x / s.bounds.size.x, alvo.size.y / s.bounds.size.y);
        go.transform.localScale = new Vector3(k, k, 1f);
        go.transform.position = new Vector3(alvo.center.x, alvo.min.y, 0f);
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
        if (s == null) { Debug.LogWarning("[Ciclopes] sprite não encontrado: " + Raiz + relativo); }
        return s;
    }

    private static Color Cor(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color c);
        return c;
    }
}
