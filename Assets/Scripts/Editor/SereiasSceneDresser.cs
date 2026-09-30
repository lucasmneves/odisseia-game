using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Odisseia.Systems;

/// <summary>
/// Veste a fase 10 com o cenário das Sereias:
///
/// Unity.exe -batchmode -quit -nographics -projectPath . -executeMethod SereiasSceneDresser.Run
///
/// Mesmo desenho dos vestidores das fases 02 a 09 — não é sistema novo.
///
/// **A tese desta fase é que ela precisa ser BONITA.** O briefing pede belo, hipnótico e com o
/// perigo escondido, e isso é uma restrição de arte, não de decoração: se o cenário avisar o
/// jogador — céu carregado, cor de alerta, monstro à vista — a fase deixa de funcionar. O
/// perigo entra só pelos destroços e pelos ossos das embarcações, que são objetos pequenos
/// dentro de um lugar claro e convidativo.
///
/// A geometria da cena já conta a história e a arte a segue: <c>MastAnchor</c> em x=−5 é onde
/// Odisseu se amarra, <c>SirenZone</c> (−4 a 14) é a passagem perigosa, e o navio da fuga está
/// no <c>LevelGoal</c> em x=32.
/// </summary>
public static class SereiasSceneDresser
{
    private const string ScenePath = "Assets/Scenes/Levels/Level_10_Sereias.unity";
    private const string Raiz = "Assets/Art/Environments/Sereias/";
    private const string RaizDoCenario = "SereiasScenery";

    private const float TopoDoChao = -2f;

    /// <summary>Odisseu nasce em x=−14, e é de lá que a câmera parte.</summary>
    private const float CameraInicial = -14f;
    private const float CameraMeio = 8f;
    private const float CameraCurso = 52f;

    private static Transform cenario;

    [MenuItem("Odisseia/Vestir Sereias")]
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
            Debug.LogError("[Sereias] nenhum Floor_* encontrado — cenário não montado");
            return false;
        }

        DesligarPlaceholders();
        MontarMar(chaos);
        VestirChao(chaos);
        PovoarPraia(chaos);
        MontarIlhaDasSereias();
        MontarMastro();
        MontarNavioDaFuga();
        MontarPrimeiroPlano();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[Sereias] cenário montado: {cenario.childCount} objetos sob {RaizDoCenario}");
        return true;
    }

    private static void DesligarPlaceholders()
    {
        // "Mast" e "Sail" entram aqui porque são retângulos que marcam o navio da fuga: a arte
        // vai por cima, na medida deles, e o objeto do level design não é tocado.
        string[] prefixos =
        {
            "Floor_", "Sky_Background", "LevelGoal", "MastAnchor", "SirenZone",
            "Platform_SirenZone", "Mast", "Sail",
        };
        foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (go == null || !prefixos.Any(pre => go.name.StartsWith(pre))) { continue; }
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null) { sr.enabled = false; }
        }
    }

    // ---------------------------------------------------------------- mar

    private static void MontarMar(List<GameObject> chaos)
    {
        float x0 = Caixa(chaos[0]).min.x, x1 = chaos.Max(g => Caixa(g).max.x);

        // #e6debf e a cor MEDIDA da primeira linha util do bg_sea, depois do corte da nuvem
        // escura. Qualquer outro creme deixa uma emenda horizontal visivel onde a cobertura
        // encontra o topo da camada — foi assim na primeira montagem, com tres faixas de ceu
        // empilhadas. Azul aqui abriria um buraco frio numa paleta quente.
        Bloco("Sky_Fill", CameraMeio, 12f, 140f, 30f, Cor("#e6debf"), -60);

        Camada("BG_Sea", "Background/sereias_bg_sea.png", 0.92f, -50, -1.6f, 1f);
        Camada("BG_Islets", "Background/sereias_bg_islets.png", 0.80f, -46, -1.9f, 1f);

        // O mar de meio de campo é a faixa CONSTRUÍDA por código: a gerada veio com riscos
        // verticais e lia como cachoeira. Ela desliza por tempo, senão o mar congela sempre que
        // o jogador para — e mar parado de verdade ainda tem os reflexos andando.
        GameObject mar = Camada("Sea_Mid", "Ocean/sereias_water_surface.png", 0.55f, -40, -2.1f, 1f);
        Deslizar(mar, -0.18f);

        // A linha de espuma marca onde o mar encontra a praia ao fundo. É ela que impede que a
        // água e a areia se encostem num corte reto.
        GameObject espuma = Camada("Foam", "Ocean/sereias_foam_line.png", 0.55f, -38, -2.0f, 1f);
        Deslizar(espuma, -0.30f);

        // Lajes de rocha atrás do plano de jogo, com o mar aparecendo por trás delas.
        // baseY -4,2 e nao -1,4. A faixa desenha do pivo PARA CIMA, entao em -1,4 ela subia
        // ate y=4,57 e enchia o alto do quadro: o mar, que e o assunto desta fase, virava uma
        // fresta entre a rocha em cima e a areia embaixo. Rebaixada, so o topo dela aparece
        // acima da linha do chao, que e o que uma costa baixa faz.
        GameObject lajes = Camada("Rock_Ledges", "Midground/sereias_rock_ledges.png", 0.40f, -34, -4.2f, 1f);
        if (lajes != null)
        {
            // Recuo em valor, puxando para a luz do ceu. Sem isto as lajes de fundo tem o
            // mesmo contraste da rocha jogavel e os dois planos colam.
            lajes.GetComponent<SpriteRenderer>().color = Color.Lerp(Color.white, Cor("#f8ebcb"), 0.45f);
        }

        // NAO ha camada de veu. Ela existiu e foi removida: entrava como uma faixa quente de
        // alpha 0,55 e o que produzia era uma terceira banda cinza-esverdeada entre o ceu e a
        // rocha, exatamente a emenda que se queria evitar. As seis familias de matiz da paleta
        // ja separam os planos sozinhas — foi o que se escreveu ao montar a paleta, e o veu
        // contradizia isso. O asset segue em VFX/sereias_haze.png, sem uso.

        // Água escura abaixo da linha do chão, para o vão entre trechos não mostrar o céu.
        Bloco("Sea_Fill", (x0 + x1) * 0.5f, -9f, x1 - x0 + 40f, 12f, Cor("#3b666f"), -30);
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
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.tileMode = SpriteTileMode.Continuous;
        // Altura NATIVA vezes um inteiro: esticar sem múltiplo faz a faixa repetir cortada.
        sr.size = new Vector2(deslize + larguraDaTela * 2f, arte.bounds.size.y * alturas);

        var parallax = go.AddComponent<ParallaxLayer>();
        var so = new SerializedObject(parallax);
        so.FindProperty("parallaxFactor").floatValue = fator;
        so.ApplyModifiedProperties();
        return go;
    }

    // ---------------------------------------------------------------- chão

    /// <summary>
    /// Areia nas duas praias, laje de rocha na ilha das sereias. A troca de material não é
    /// enfeite: é o aviso silencioso de que o terreno mudou, e cai exatamente no
    /// <c>Floor_SirenZone</c>, que é onde o level design põe o perigo.
    /// </summary>
    private static void VestirChao(List<GameObject> chaos)
    {
        foreach (GameObject chao in chaos)
        {
            Bounds b = Caixa(chao);
            bool ilha = chao.name.Contains("SirenZone");
            // As faixas das pontas avancam 4 un para fora do colisor. O jogador nasce em x=-14
            // e a camera mostra ate x=-22,9, alem do inicio do chao em -18: sem a folga,
            // aparecia um retangulo chapado de mar no canto do quadro logo no spawn.
            float folgaEsq = b.min.x <= -17f ? 9f : 0f;
            float folgaDir = b.max.x >= 33f ? 4f : 0f;
            // Altura NATIVA do tile, e não uma faixa curta.
            //
            // Os tiles de borda (`*_edge`) existem e foram testados aqui: a faixa curta mais o
            // bloco de barranco produziam três barras horizontais empilhadas sob os pés do
            // jogador, e cada emenda entre elas aparecia. Uma praia não tem emenda — desce da
            // areia seca para a molhada num contínuo, e o tile inteiro já desenha isso. A fase
            // pede o contrário do Mundo dos Mortos: lá o chão era um passadiço sobre a água e
            // precisava acabar; aqui ele é a própria praia e precisa continuar.
            GameObject faixa = Faixa($"Ground_{chao.name}", b.center.x + (folgaDir - folgaEsq) * 0.5f,
                b.max.y, b.size.x + folgaEsq + folgaDir,
                ilha ? "Gameplay/sereias_tiles_rock.png"
                     : "Gameplay/sereias_tiles_sand.png", -10);
            if (faixa == null) { continue; }

            // A altura vem do SPRITE, nunca de uma constante escrita à mão. O pipeline recorta
            // esses tiles (faixa chapada do gerador, tiles de borda), e todo recorte mudava a
            // altura sem avisar ninguém — um número fixo aqui vira um vão ou uma sobreposição
            // silenciosa na próxima vez que a arte for regerada.
            float faceDoChao = faixa.GetComponent<SpriteRenderer>().sprite.bounds.size.y;

            // Barranco encostado na base do tile, e #776657 é a cor MEDIDA da última linha dele
            // — por isso a emenda entre os dois não aparece.
            Bloco($"Bank_{chao.name}", b.center.x + (folgaDir - folgaEsq) * 0.5f,
                b.max.y - faceDoChao - 6f, b.size.x + folgaEsq + folgaDir, 12f, Cor("#776657"), -12);

            // Linha de espuma no pé de cada trecho: é onde a água encosta na praia, e é o
            // detalhe que faz a areia ler como beira-mar e não como estrada de terra.
            if (!ilha)
            {
                Faixa($"Surf_{chao.name}", b.center.x, b.max.y - 0.15f, b.size.x,
                    "Ocean/sereias_foam_line.png", -9);
            }
        }
    }

    // ---------------------------------------------------------------- povoamento

    /// <summary>
    /// As duas praias. Belas primeiro: pedras lisas, capim e flores. Os destroços entram em
    /// número pequeno e baixos no quadro — o perigo tem de ser encontrado, não anunciado.
    /// </summary>
    private static void PovoarPraia(List<GameObject> chaos)
    {
        foreach (GameObject chao in chaos.Where(c => !c.name.Contains("SirenZone")))
        {
            Bounds b = Caixa(chao);
            bool primeira = b.min.x < 0f;

            for (int i = 0; i < 4; i++)
            {
                float t = (i + 0.5f) / 4f;
                Encolher(Prop($"Stones_{chao.name}_{i}", b.min.x + b.size.x * t, TopoDoChao,
                    "Beach/sereias_shore_stones.png", -8), 0.55f);
                Encolher(Prop($"Grass_{chao.name}_{i}", b.min.x + b.size.x * (t + 0.12f), TopoDoChao,
                    "Beach/sereias_sea_grass.png", -7), 0.7f);
            }

            // Um naufrágio por praia, e não mais. Na primeira ele é o presságio; na segunda, a
            // confirmação de que o jogador já passou pelo pior.
            Encolher(Prop($"Wreck_{chao.name}",
                primeira ? b.min.x + 4f : b.max.x - 6f, TopoDoChao,
                primeira ? "Shipwrecks/sereias_wreck_ribs.png" : "Shipwrecks/sereias_wreck_hull.png",
                -6), 0.75f);
        }
    }

    /// <summary>
    /// A ilha das sereias, entre x=−4 e x=14. É o único trecho com rocha, ruína e canto — e o
    /// <c>Platform_SirenZone</c> ganha pedregulhos por baixo, para a plataforma de gameplay
    /// parecer parte do terreno em vez de um degrau flutuante.
    /// </summary>
    private static void MontarIlhaDasSereias()
    {
        GameObject zona = GameObject.Find("SirenZone");
        if (zona == null) { return; }
        Bounds b = Caixa(zona);

        // O santuário no meio: colunas e verga reta, sem frontão. É o que promete que há algo
        // bom naquela ilha, e é a mentira central da fase.
        Encolher(Prop("Shrine", b.center.x - 1f, TopoDoChao, "Ruins/sereias_shrine.png", -16), 0.8f);
        Encolher(Prop("Fallen_Column", b.min.x + 3f, TopoDoChao, "Ruins/sereias_fallen_column.png", -6), 0.7f);

        // Agulhas de rocha nas bordas da ilha, para ela ter silhueta contra o mar.
        Encolher(Prop("Spire_L", b.min.x + 0.5f, TopoDoChao, "Rocks/sereias_rock_spire.png", -15), 0.85f);
        Encolher(Prop("Spire_R", b.max.x - 1.5f, TopoDoChao, "Rocks/sereias_rock_spire.png", -15), 0.7f);

        GameObject plataforma = GameObject.Find("Platform_SirenZone");
        if (plataforma != null)
        {
            Bounds pb = Caixa(plataforma);
            GameObject pedras = Prop("Platform_Rocks", pb.center.x, pb.max.y,
                "Rocks/sereias_boulders.png", -5);
            if (pedras != null)
            {
                Sprite s = pedras.GetComponent<SpriteRenderer>().sprite;
                // Largura da plataforma, e não uma escala fixa: a arte serve o design.
                float k = pb.size.x / s.bounds.size.x;
                pedras.transform.localScale = new Vector3(k, k, 1f);
                pedras.transform.position = new Vector3(pb.center.x, pb.max.y - s.bounds.size.y * k * 0.55f, 0f);
            }
        }

        // O canto. Alpha baixo e ordem ACIMA do jogador (−3 ainda está atrás dele; o efeito
        // passa por trás, nunca por cima) — o briefing proíbe que qualquer véu esconda jogador,
        // inimigo ou plataforma.
        GameObject canto = Prop("Siren_Song", b.center.x, TopoDoChao + 1.6f, "Special/sereias_song.png", -3);
        if (canto != null)
        {
            // Na primeira montagem os arcos mediam 9 un de altura centrados em y=1,5 e
            // cobriam o quadro inteiro: liam como ARRANHOES no ceu, nao como canto saindo da
            // ilha. Presos a linha do chao e em escala menor, ficam onde a fonte esta.
            canto.transform.localScale = new Vector3(0.9f, 0.5f, 1f);
            canto.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.85f);
            Deslizar(canto, -0.45f);
        }
    }

    /// <summary>
    /// O mastro em que Odisseu se amarra. Mastro e vela vêm de Cytera: é literalmente o mesmo
    /// navio da fase 04, e desenhar um segundo seria criar duas embarcações para uma viagem só.
    /// </summary>
    private static void MontarMastro()
    {
        GameObject ancora = GameObject.Find("MastAnchor");
        if (ancora == null) { return; }

        Bounds b = Caixa(ancora);
        GameObject mastro = Prop("Mast_Art", b.center.x, TopoDoChao,
            "Assets/Art/Environments/Cytera/Ship/cytera_mast_sail.png", -4);
        if (mastro == null) { return; }

        // Altura do marcador, não uma escala fixa: se o level design mudar a âncora, a arte
        // acompanha sozinha.
        Sprite s = mastro.GetComponent<SpriteRenderer>().sprite;
        float k = (b.size.y * 1.8f) / s.bounds.size.y;
        mastro.transform.localScale = new Vector3(k, k, 1f);
    }

    /// <summary>O navio da fuga, no objetivo. Mesmo casco e mesmo mastro de Cytera.</summary>
    private static void MontarNavioDaFuga()
    {
        GameObject alvo = GameObject.Find("LevelGoal");
        if (alvo == null) { return; }

        Bounds b = Caixa(alvo);
        Encolher(Prop("Escape_Deck", b.center.x, TopoDoChao,
            "Assets/Art/Environments/Cytera/Ship/cytera_deck_planks.png", -5), 0.9f);
        GameObject mastro = Prop("Escape_Mast", b.center.x, TopoDoChao,
            "Assets/Art/Environments/Cytera/Ship/cytera_mast_sail.png", -4);
        Encolher(mastro, 1.1f);
    }

    /// <summary>
    /// Primeiro plano sem <see cref="ParallaxLayer"/>: o fator dele é limitado a [0,1] e
    /// primeiro plano exigiria mais que 1. Pedras baixas com ordem acima do jogador — nunca o
    /// escondem porque ficam abaixo da linha do chão.
    /// </summary>
    private static void MontarPrimeiroPlano()
    {
        float[] posicoes = { -16f, -7f, 6f, 19f, 30f };
        for (int i = 0; i < posicoes.Length; i++)
        {
            GameObject go = Prop($"FG_Stones_{i}", posicoes[i], TopoDoChao - 1.4f,
                "Beach/sereias_shore_stones.png", 12);
            if (go == null) { continue; }
            go.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
        }
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

    private static void Deslizar(GameObject go, float velocidade)
    {
        if (go == null) { return; }
        var sc = go.AddComponent<ScrollingLayer>();
        var so = new SerializedObject(sc);
        so.FindProperty("speedX").floatValue = velocidade;
        so.ApplyModifiedProperties();
    }

    private static GameObject Faixa(string nome, float centroX, float topoY, float largura,
        string caminho, int ordem, float alturaFixa = 0f)
    {
        Sprite arte = Arte(caminho);
        if (arte == null) { return null; }

        float altura = alturaFixa > 0f ? alturaFixa : arte.bounds.size.y;
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
        if (s == null) { Debug.LogWarning("[Sereias] sprite não encontrado: " + caminho); }
        return s;
    }

    private static Color Cor(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color c);
        return c;
    }
}
