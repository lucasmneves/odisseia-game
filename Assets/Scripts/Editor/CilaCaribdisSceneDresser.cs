using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Odisseia.Levels;
using Odisseia.Systems;

/// <summary>
/// Veste a fase 11 com o estreito de Cila e Caribdis:
///
/// Unity.exe -batchmode -quit -nographics -projectPath . -executeMethod CilaCaribdisSceneDresser.Run
///
/// Mesmo desenho dos vestidores das fases 02 a 10 — não é sistema novo.
///
/// ## A tese desta fase é CLAUSTROFOBIA, e ela é medida, não sentida
///
/// Cytera (04) já é tempestade no mar e Sereias (10) já é mar bonito. O que sobra de próprio
/// para esta fase não é "mais tempestade": é o quadro FECHANDO. As paredes de basalto colunar
/// entram em parallax 0,30 nas duas bordas do percurso e sobem 12 unidades — o Odisseu tem
/// 1,4 —, de modo que o céu vira uma fresta entre elas. É a diferença entre um mar aberto com
/// ondas e um corredor de pedra de onde não se sai.
///
/// ## A leitura do gameplay é problema de VALOR, e o número está travado
///
/// A rocha jogável mede L 0,43 e a parede de fundo L 0,20. Essa distância de 0,23 é o que faz
/// a plataforma saltar da falésia numa fase inteira de pedra escura, e foi ela que exigiu
/// escurecer a parede por fator em vez de quantizá-la — ver o cabeçalho de
/// <c>Tools/build-cila.js</c>. Nada aqui pode encostar nesses dois valores sem refazer a conta.
///
/// A geometria da cena já conta a história e a arte a segue: <c>Island_2</c> em x=4,5..7,5 é
/// onde o marcador de Cila está (cabeça em y=4,6), <c>CharybdisWhirlpool</c> em x=14,5..18 é o
/// redemoinho, e o navio da fuga está no <c>LevelGoal</c> em x=32,5..35,5.
/// </summary>
public static class CilaCaribdisSceneDresser
{
    private const string ScenePath = "Assets/Scenes/Levels/Level_11_CilaCaribdis.unity";
    private const string Raiz = "Assets/Art/Environments/CilaCaribdis/";
    private const string Cytera = "Assets/Art/Environments/Cytera/";
    private const string RaizDoCenario = "CilaCaribdisScenery";

    private const float TopoDoChao = -2f;

    /// <summary>
    /// Reserva para o início da câmera, usada só se a cena não tiver Main Camera. O valor real
    /// vem da câmera da própria cena — ver <see cref="Camada"/>, e a razão está lá.
    /// </summary>
    private const float CameraInicial = 0f;

    /// <summary>
    /// Extensão que a câmera percorre, MEDIDA nos colisores do chão em <see cref="Executar"/>.
    /// Não são constantes: a fase pode ser esticada pelo level design.
    /// </summary>
    private static float CameraMin = -18f;
    private static float CameraMax = 38.5f;

    /// <summary>
    /// Quanto a arte do tile de topo precisa SUBIR para a superfície pisável encostar no
    /// colisor. Medido por <c>Tools/build-cila-ground.js</c>: num tile Wang de topo os cantos
    /// de cima são "upper" (ar), então a linha de pisada fica a 16px do topo de um tile de 32,
    /// e não no topo. Sem esta correção o jogador anda meio tile acima da rocha desenhada.
    /// </summary>
    private const float SuperficieDoTile = 16f / 42.857143f;

    private static Transform cenario;

    [MenuItem("Odisseia/Vestir Cila e Caribdis")]
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

        List<GameObject> plataformas = Plataformas();
        if (plataformas.Count == 0)
        {
            Debug.LogError("[Cila] nenhum Floor_*/Island_* encontrado — cenário não montado");
            return false;
        }

        // A extensão sai dos colisores, não de números escritos aqui: é o que faz o fundo
        // acompanhar sozinho se a fase for esticada.
        CameraMin = plataformas.Min(g => Caixa(g).min.x);
        CameraMax = plataformas.Max(g => Caixa(g).max.x);

        DesligarPlaceholders();
        MontarCeuEMar(plataformas);
        MontarParedes();
        VestirChao(plataformas);
        MontarFalesiaDeCila();
        MontarCaribdis();
        MontarNavioDaFuga();
        MontarDestrocos();
        MontarClima();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[Cila] cenário montado: {cenario.childCount} objetos sob {RaizDoCenario}");
        return true;
    }

    /// <summary>Chão jogável: os dois trechos longos e as quatro ilhas de passagem.</summary>
    private static List<GameObject> Plataformas() =>
        Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
            .Where(g => g != null && (g.name.StartsWith("Floor_") || g.name.StartsWith("Island_")))
            .Where(g => g.GetComponent<Collider2D>() != null)
            .OrderBy(g => g.transform.position.x)
            .ToList();

    private static void DesligarPlaceholders()
    {
        // "Mast" e "Sail" entram aqui porque são retângulos que marcam o navio da fuga: a arte
        // vai por cima, na medida deles, e o objeto do level design não é tocado.
        //
        // "Giant" NÃO entra: é o marcador de Cila, e o briefing proíbe gerar a personagem neste
        // pipeline. Ele continua aceso e registrado como placeholder legítimo — o que esta fase
        // entrega é o AMBIENTE que o acomoda.
        string[] prefixos =
        {
            "Floor_", "Island_", "Sky_Background", "LevelGoal", "Mast", "Sail",
            "TidalHazard", "CharybdisWhirlpool",
        };
        foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (go == null || !prefixos.Any(pre => go.name.StartsWith(pre))) { continue; }
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null) { sr.enabled = false; }
        }
    }

    // ---------------------------------------------------------------- céu e mar

    private static void MontarCeuEMar(List<GameObject> plataformas)
    {
        // #847c77 é a cor MEDIDA da primeira linha de `cila_bg_walls_far` (274 dos 672 px dela). Qualquer outro tom
        // deixa uma emenda horizontal visível onde a cobertura encontra o topo da camada — foi
        // a lição das Sereias, e vale igual aqui.
        Bloco("Sky_Fill", (CameraMin + CameraMax) * 0.5f, 15f, 150f, 34f, Cor("#847c77"), -60);

        // Bloco de AGUA FUNDA abaixo da linha do chao. Na primeira montagem ele nao existia e o
        // resultado foi magenta — a cor de fundo da camera — aparecendo nos vaos entre as
        // ilhas, porque a cobertura de ceu comecava em y=-3 e nada desenhava abaixo disso.
        // #081e1d é o passo Profunda da rampa "Agua funda": é literalmente o mar sem luz.
        Bloco("Deep_Fill", (CameraMin + CameraMax) * 0.5f, -16f, 150f, 28f, Cor("#081e1d"), -58);

        // A ÚNICA camada que carrega céu. Duas empilhadas produzem faixas horizontais com
        // emendas duras atravessando a tela — foi exatamente o que aconteceu em Sereias com
        // `bg_sea` e `bg_islets` juntos.
        Camada("BG_Walls_Far", "Background/cila_bg_walls_far.png", 0.90f, -50, -3.4f, 1f);

        // O mar de meio de campo. Desliza por tempo: mar de tempestade parado quando o jogador
        // para é a coisa que mais denuncia cenário estático, e a fase inteira promete violência.
        // baseY -6 e nao -3,2. A faixa desenha do pivo PARA CIMA, entao em -3,2 o topo dela subia a
        // y=1,3 e as ondas apareciam FLUTUANDO no meio do quadro, entre as colunas de basalto —
        // agua no ar. Em -6 o topo fica em -1,5, logo acima da linha do chao: o mar preenche os
        // vaos entre as ilhas, que e onde o mar de um estreito de fato esta.
        GameObject mar = Camada("Sea_Churn", "Ocean/cila_sea_churn.png", 0.55f, -40, -6f, 1f);
        Deslizar(mar, -1.1f);
    }

    /// <summary>
    /// As paredes que fecham o quadro — o assunto da fase.
    ///
    /// Ficam em parallax 0,30 (meio de campo) e NÃO em primeiro plano: em primeiro plano elas
    /// cobririam o jogador, e a seção de leitura do briefing proíbe. O aperto vem da ALTURA e
    /// da frequência delas, não de estarem à frente.
    ///
    /// A parede de basalto tem 11,95 un de altura contra 1,4 un do Odisseu: uma só já é oito
    /// vezes o personagem. Espaçadas a cada ~9 unidades, elas nunca deixam o quadro sem pedra
    /// nas bordas, que é o que produz a sensação de corredor.
    /// </summary>
    private static void MontarParedes()
    {
        // CINCO paredes, e não sete. Com sete espaçadas a cada 9 unidades e parallax 0,30 elas
        // se encavalavam: o quadro virava basalto de ponta a ponta e a fresta de céu do fundo —
        // que é o que dá a MEDIDA do aperto — nunca aparecia. Claustrofobia precisa da fresta;
        // sem ela não é um corredor, é uma parede.
        // As dez peças penduram em DOIS contêineres, cada um com UM ParallaxLayer.
        //
        // Na primeira montagem cada parede e cada pináculo carregava o seu próprio componente, e
        // a fase saiu com 13 camadas de parallax contra 5 a 8 das outras onze fases — o dobro
        // de escritas de transform por quadro, para um efeito idêntico, já que todas as paredes
        // andam ao mesmo fator e todos os pináculos também.
        //
        // Agrupadas, sobram três: paredes, pináculos e as duas faixas de fundo. É a seção de
        // performance do briefing — "evitar excesso de GameObjects" — resolvida sem mudar um
        // pixel do que se vê.
        GameObject paredes = Grupo("Walls_Parallax", 0.30f);
        float[] posicoes = { -19f, -6f, 8f, 22f, 36f };
        for (int i = 0; i < posicoes.Length; i++)
        {
            GameObject go = Prop($"Wall_{i}", posicoes[i], TopoDoChao - 1.2f,
                "Midground/cila_wall_columnar.png", -30);
            if (go == null) { continue; }
            go.transform.SetParent(paredes.transform, true);
            // Larguras alternadas e algumas espelhadas: cinco cópias do mesmo sprite na mesma
            // escala leem como repetição de textura, não como falésia.
            float k = 1f + (i % 3) * 0.22f;
            go.transform.localScale = new Vector3(i % 2 == 0 ? k : -k, k, 1f);
        }

        // Pináculos soltos entre as paredes, mais ao fundo e menores. Eles preenchem o intervalo
        // sem fechar mais o corredor, e a escala menor é o que os põe atrás.
        GameObject pinaculosPai = Grupo("Crags_Parallax", 0.20f);
        float[] pinaculos = { -6.5f, 2.5f, 13f, 24f, 33f };
        for (int i = 0; i < pinaculos.Length; i++)
        {
            GameObject go = Prop($"Crag_{i}", pinaculos[i], TopoDoChao - 0.8f,
                "Rocks/cila_crag_stack.png", -34);
            if (go == null) { continue; }
            go.transform.SetParent(pinaculosPai.transform, true);
            float k = 0.55f + (i % 2) * 0.15f;
            go.transform.localScale = new Vector3(k, k, 1f);
            // Recuo em VALOR, puxando para a cor do céu. É a perspectiva aérea que Lestrigões
            // cobrou e que Circe e Mortos herdaram — sem ela o pináculo de fundo tem o mesmo
            // contraste da parede de meio e os dois planos colam.
            go.GetComponent<SpriteRenderer>().color = Distancia(0.20f);
        }
    }

    /// <summary>
    /// Perspectiva aérea: cada camada é puxada para a cor do fundo na proporção do parallax.
    /// Nenhum asset novo, nenhum shader — só <c>sr.color</c>.
    /// </summary>
    private static Color Distancia(float fator) =>
        Color.Lerp(Color.white, Cor("#847c77"), Mathf.Clamp01((0.62f - fator) * 1.1f));

    // ---------------------------------------------------------------- chão

    /// <summary>
    /// Rocha jogável em todos os trechos. Não há troca de material aqui, e a ausência é
    /// decidida: em Sereias a troca areia/rocha era o aviso silencioso de que o terreno mudou,
    /// mas nesta fase TUDO é o mesmo estreito de basalto — inventar um segundo material seria
    /// mentir sobre o lugar.
    /// </summary>
    private static void VestirChao(List<GameObject> plataformas)
    {
        foreach (GameObject chao in plataformas)
        {
            Bounds b = Caixa(chao);

            // As pontas avançam para fora do colisor: o jogador nasce em x=−14 e a câmera
            // mostra além do início do chão em −18, então sem a folga aparece um retângulo
            // vazio no canto do quadro logo no spawn.
            float folgaEsq = b.min.x <= -17f ? 9f : 0f;
            float folgaDir = b.max.x >= 38f ? 6f : 0f;
            float largura = b.size.x + folgaEsq + folgaDir;
            float centro = b.center.x + (folgaDir - folgaEsq) * 0.5f;

            // A faixa de topo sobe pela medida do tile, não por meio tile chutado.
            GameObject topo = Faixa($"GroundTop_{chao.name}", centro, b.max.y + SuperficieDoTile,
                largura, "Gameplay/cila_rock_ground_top.png", -10);
            if (topo == null) { continue; }

            // A altura vem do SPRITE, nunca de constante escrita à mão: o pipeline recorta
            // esses tiles, e todo recorte muda a altura sem avisar ninguém.
            float alturaDoTile = topo.GetComponent<SpriteRenderer>().sprite.bounds.size.y;

            // Corpo sólido descendo até bem abaixo da linha de morte, para não haver fresta
            // entre a plataforma e o fundo quando a câmera baixa.
            GameObject corpo = Faixa($"GroundBody_{chao.name}", centro,
                b.max.y + SuperficieDoTile - alturaDoTile, largura,
                "Gameplay/cila_rock_ground_body.png", -11, 6f);

            // O corpo é ESCURECIDO, e essa é a diferença entre "chão" e "parede de tijolo".
            //
            // Na primeira montagem topo e corpo tinham o mesmo valor, e as seis unidades de
            // corpo ladrilhado ocupavam a metade de baixo do quadro como uma grade regular e
            // clara — lia como alvenaria, o defeito que esta fase já combateu na geração. O que
            // conserta não é trocar o tile: é lembrar que a face SOB a plataforma não recebe luz.
            // Escurecida, ela vira a sombra em que a plataforma se apoia, e a linha de pisada
            // passa a ser a coisa mais clara do quadro — que é o que o jogador precisa ver.
            if (corpo != null)
            {
                corpo.GetComponent<SpriteRenderer>().color = new Color(0.42f, 0.44f, 0.46f, 1f);
            }
        }
    }

    // ---------------------------------------------------------------- Cila

    /// <summary>
    /// A falésia com a caverna, atrás do marcador de Cila.
    ///
    /// O briefing proíbe gerar a personagem aqui, então o que esta fase entrega é o lugar que a
    /// acomoda: um vão de basalto grande o bastante para uma criatura de escala, acima da linha
    /// de jogo, com destroços nas pedras embaixo. A posição sai do MARCADOR, não de um número
    /// escrito à mão — se o level design mover Cila, a caverna acompanha.
    /// </summary>
    private static void MontarFalesiaDeCila()
    {
        GameObject cila = GameObject.Find("Giant");
        if (cila == null)
        {
            Debug.LogWarning("[Cila] marcador \"Giant\" não encontrado — falésia não posicionada");
            return;
        }

        GameObject falesia = Prop("Scylla_Cliff", cila.transform.position.x, 0f,
            "Scylla/cila_cave_cliff.png", -24);
        if (falesia == null) { return; }

        // Escala 1,15: a caverna escavada mede ~3,5 un de vão em escala nativa, e o marcador de
        // Cila tem corpo de 2x3. O vão precisa engolir a criatura, não emoldurá-la justa.
        const float escala = 1.15f;
        falesia.transform.localScale = new Vector3(escala, escala, 1f);

        // A ALTURA É CALCULADA a partir do marcador, e a primeira montagem provou por que.
        //
        // Com a base fixada em y=−3 a caverna ficava em y≈7,9 — fora do quadro, porque a câmera
        // ortográfica de tamanho 5 só mostra até y=5. A falésia aparecia, o vão não: a única
        // coisa que justificava o asset estava acima da tela, e nada no console reclamou.
        //
        // `Tools/build-cila.js` escava o vão centrado a 30% da altura contando do TOPO, ou seja
        // a 70% contando da base. Então para o vão cair na altura da CABEÇA do marcador, a base
        // desce essa fração da altura já escalada. Se o level design mover Cila, a caverna
        // acompanha em X e em Y.
        Sprite arte = falesia.GetComponent<SpriteRenderer>().sprite;
        float alturaDoVao = FindHead(cila);
        falesia.transform.position = new Vector3(falesia.transform.position.x,
            alturaDoVao - arte.bounds.size.y * escala * 0.70f, 0f);

        // Ordem −24 põe a falésia ATRÁS do marcador (ordem 0/1) e à frente das paredes (−30):
        // Cila sai da caverna, e não fica colada na frente dela.
    }

    /// <summary>
    /// Altura da cabeça do marcador de Cila — é onde o vão da caverna tem de cair.
    ///
    /// Lê o filho "Head" quando existe, e usa o topo do colisor como reserva. O marcador é
    /// arte provisória e pode ser trocado pelo pipeline de personagem; ler a cabeça pelo nome
    /// e ter uma reserva evita que a caverna se desalinhe em silêncio quando isso acontecer.
    /// </summary>
    private static float FindHead(GameObject cila)
    {
        Transform cabeca = cila.transform.Find("Head");
        if (cabeca != null) { return cabeca.position.y; }

        var col = cila.GetComponentInChildren<Collider2D>();
        return col != null ? col.bounds.max.y : cila.transform.position.y + 3f;
    }

    // ---------------------------------------------------------------- Caribdis

    /// <summary>
    /// O redemoinho, no marcador que o level design já tinha.
    ///
    /// A animação é a folha de 8 quadros construída por <c>Tools/build-cila-fx.js</c> e tocada
    /// pelo <see cref="SpriteAnimator"/> que já existe — nenhum sistema novo, nenhum shader,
    /// nenhum ParticleSystem. São 8 sprites numa textura e uma troca de sprite por quadro, que
    /// é o que o orçamento de WebGL e mobile do briefing pede.
    /// </summary>
    private static void MontarCaribdis()
    {
        GameObject marcador = GameObject.Find("CharybdisWhirlpool");
        if (marcador == null)
        {
            Debug.LogWarning("[Cila] marcador \"CharybdisWhirlpool\" não encontrado");
            return;
        }

        Bounds b = Caixa(marcador);
        var go = new GameObject("Charybdis_Art");
        go.transform.SetParent(cenario, false);
        // No centro do marcador em X e na LINHA DE ÁGUA em Y: o redemoinho é um buraco na
        // superfície do mar, então ele pertence à altura do chão, não ao meio do colisor.
        go.transform.position = new Vector3(b.center.x, TopoDoChao - 0.35f, 0f);

        var sr = go.AddComponent<SpriteRenderer>();
        // Ordem 6: acima do chão (−10) e do mar, abaixo do primeiro plano. O jogador precisa
        // ver o redemoinho por inteiro — é ele que comunica o perigo antes de matar.
        sr.sortingOrder = 6;

        var anim = go.AddComponent<SpriteAnimator>();
        var so = new SerializedObject(anim);
        so.FindProperty("resourcePath").stringValue = "Odisseia/Environments/FX_Charybdis";
        so.FindProperty("defaultState").stringValue = "Spin";
        // 9 fps: a 8 quadros isso dá uma volta a cada 0,9 s. Mais rápido vira serrilha
        // piscando, mais lento deixa de ler como sucção.
        so.FindProperty("defaultFramesPerSecond").floatValue = 9f;
        so.ApplyModifiedProperties();

        // A largura do marcador manda na escala, como no mastro das Sereias: se o level design
        // alargar o redemoinho, a arte acompanha sozinha.
        Sprite quadro = Resources.LoadAll<Sprite>("Odisseia/Environments/FX_Charybdis")
            .FirstOrDefault(s => s.name.EndsWith("_00"));
        if (quadro != null)
        {
            sr.sprite = quadro;
            float k = (b.size.x * 1.45f) / quadro.bounds.size.x;
            go.transform.localScale = new Vector3(k, k, 1f);
        }
        else
        {
            Debug.LogWarning("[Cila] FX_Charybdis não encontrado em Resources — rode " +
                "Tools/build-cila-fx.js");
        }
    }

    // ---------------------------------------------------------------- navio

    /// <summary>
    /// O navio da fuga, no objetivo. Casco e mastro vêm de Cytera: é literalmente a mesma
    /// embarcação da fase 04, e desenhar uma segunda seria criar duas frotas para uma viagem só.
    /// </summary>
    private static void MontarNavioDaFuga()
    {
        GameObject alvo = GameObject.Find("LevelGoal");
        if (alvo == null) { return; }

        Bounds b = Caixa(alvo);
        Encolher(Prop("Escape_Deck", b.center.x, TopoDoChao, Cytera + "Ship/cytera_deck_planks.png", -5), 1.0f);
        Encolher(Prop("Escape_Mast", b.center.x, TopoDoChao, Cytera + "Ship/cytera_mast_sail.png", -4), 1.15f);
    }

    /// <summary>
    /// Destroços. São os únicos objetos QUENTES da fase — madeira contra basalto e teal — e por
    /// isso fazem duas coisas ao mesmo tempo: contam que outros navios morreram aqui e dão ao
    /// olho o único ponto de matiz numa tela fria.
    ///
    /// Todos ficam ABAIXO da linha do chão ou encostados nela, e nenhum passa de ordem 12: a
    /// regra da fase é que nada de decoração cruze o plano do jogador.
    /// </summary>
    private static void MontarDestrocos()
    {
        // Costelas de naufrágio nas ilhas de passagem, que é onde o perigo aperta.
        //
        // A ALTURA VARIA de propósito. Na primeira montagem os sete destroços estavam todos na
        // mesma linha e na mesma escala, e o resultado atravessava o quadro como uma CERCA — um
        // objeto contínuo, que é o oposto de destroço. Espalhados em y e em escala, voltam a ler
        // como coisas que caíram onde calhou.
        foreach (var (x, y, escala) in new[] { (0.2f, -0.15f, 0.55f), (11.8f, -0.45f, 0.42f), (20.5f, -0.05f, 0.62f) })
        {
            GameObject go = Prop($"Wreck_{x:0}", x, TopoDoChao + y,
                "Shipwrecks/cila_wreck_ribs.png", -2);
            Encolher(go, escala);
        }

        // Tábuas soltas junto ao redemoinho e na praia de chegada: são o rastro do que Caribdis
        // já engoliu. Três, e não quatro — a quarta caía justo no vão entre as ilhas e ficava
        // boiando no ar.
        foreach (var (x, y, escala) in new[] { (13.4f, 0.05f, 0.42f), (19.4f, -0.1f, 0.5f), (29.6f, 0.05f, 0.38f) })
        {
            GameObject go = Prop($"Debris_{x:0}", x, TopoDoChao + y,
                "Props/cila_debris_timbers.png", 3);
            Encolher(go, escala);
        }
    }

    // ---------------------------------------------------------------- clima

    /// <summary>
    /// Chuva, clarões e tremor pelo <see cref="StormAmbience"/> que a fase 04 já usa.
    ///
    /// O briefing pede chuva, relâmpago e vento e manda reutilizar o que existe. Este componente
    /// faz as três coisas com sprites reciclados e um Image de tela cheia — sem ParticleSystem,
    /// que é o que mantém a conta de WebGL previsível.
    /// </summary>
    private static void MontarClima()
    {
        if (Object.FindFirstObjectByType<StormAmbience>() != null) { return; }

        var go = new GameObject("StormAmbience");
        go.transform.SetParent(cenario, false);
        var storm = go.AddComponent<StormAmbience>();

        var so = new SerializedObject(storm);
        // A gota usa a arte de chuva de Cytera; o placeholder do componente é um retângulo.
        so.FindProperty("raindropSprite").objectReferenceValue =
            AssetDatabase.LoadAssetAtPath<Sprite>(Cytera + "Weather/cytera_rain.png");
        so.FindProperty("raindropCount").intValue = 70;
        // Chuva mais inclinada e mais rápida que a de Cytera: aqui o vento está canalizado
        // entre duas paredes, e chuva vertical num corredor não lê como tempestade.
        so.FindProperty("rainVelocity").vector2Value = new Vector2(-7f, -18f);
        so.FindProperty("rainColor").colorValue = new Color(0.72f, 0.82f, 0.95f, 0.42f);

        // O clarão precisa de um Image de tela cheia no Canvas do HUD.
        GameObject canvas = GameObject.Find("HUD Canvas");
        if (canvas != null)
        {
            var overlay = new GameObject("StormFlash", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            overlay.transform.SetParent(canvas.transform, false);
            var rect = (RectTransform)overlay.transform;
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
            // Primeiro filho: o clarão fica ATRÁS do HUD. Um relâmpago que apaga o contador de
            // vidas é um bug de leitura, não ambientação.
            overlay.transform.SetAsFirstSibling();

            var img = overlay.GetComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0f);
            img.raycastTarget = false;                     // nunca roubar toque do controle virtual
            so.FindProperty("flashOverlay").objectReferenceValue = img;
        }
        so.ApplyModifiedProperties();
    }

    // ---------------------------------------------------------------- utilidades

    private static Bounds Caixa(GameObject go) => go.GetComponent<Collider2D>().bounds;

    private static void Encolher(GameObject go, float k)
    {
        if (go != null) { go.transform.localScale = new Vector3(k, k, 1f); }
    }

    /// <summary>
    /// Contêiner com um único <see cref="ParallaxLayer"/>, para as peças que andam juntas.
    ///
    /// Os filhos entram com <c>SetParent(pai, true)</c> — mantendo a posição de mundo —, então o
    /// contêiner fica na origem e só desloca. Um componente move dez sprites.
    /// </summary>
    private static GameObject Grupo(string nome, float fator)
    {
        var go = new GameObject(nome);
        go.transform.SetParent(cenario, false);
        go.AddComponent<ParallaxLayer>();
        Fator(go, fator);
        return go;
    }

    private static void Fator(GameObject go, float fator)
    {
        var so = new SerializedObject(go.GetComponent<ParallaxLayer>());
        so.FindProperty("parallaxFactor").floatValue = fator;
        so.ApplyModifiedProperties();
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
    /// Posiciona e dimensiona uma camada de parallax para ela cobrir a fase INTEIRA.
    ///
    /// ## O erro que este método corrigiu, e que não dava sinal nenhum
    ///
    /// A primeira versão veio copiada das Sereias e assumia que a câmera começa onde o jogador
    /// nasce (x=−14). Nesta cena a Main Camera está autorada em **x=0**. O
    /// <see cref="ParallaxLayer"/> desloca a camada a partir da posição REAL da câmera, então
    /// todas as camadas de fundo ficaram 14 unidades fora do lugar.
    ///
    /// E o sintoma não aparece onde o erro está: no começo da fase tudo parecia certo, e só no
    /// objetivo (x=34) surgia uma faixa cinza chapada na borda direita — o bloco de céu
    /// aparecendo onde o fundo tinha acabado. A conclusão natural seria "a camada é estreita
    /// demais", e alargá-la teria escondido o defeito sem corrigi-lo.
    ///
    /// Então nada aqui é constante escrita à mão: o início vem da câmera da cena e a extensão
    /// vem dos colisores do chão. Se o level design mover a câmera ou esticar a fase, o fundo
    /// acompanha.
    ///
    /// A conta: com fator f, a camada fica em <c>x0 + (camX − camInicio)·f</c>, e portanto se
    /// afasta do centro da tela em <c>(camX − camInicio)·(f − 1)</c>. Centrando esse afastamento
    /// no meio do percurso, o máximo vira metade do curso vezes (1 − f) — e a largura
    /// necessária é isso mais uma tela inteira de folga de cada lado.
    /// </summary>
    private static GameObject Camada(string nome, string caminho, float fator, int ordem,
        float baseY, float alturas)
    {
        Sprite arte = Arte(caminho);
        if (arte == null) { return null; }

        const float larguraDaTela = 18f;
        float camInicio = Camera.main != null ? Camera.main.transform.position.x : CameraInicial;
        float curso = CameraMax - CameraMin;
        float x0 = camInicio * fator + (CameraMin + CameraMax) * 0.5f * (1f - fator);
        float deslize = curso * (1f - fator);

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

        go.AddComponent<ParallaxLayer>();
        Fator(go, fator);
        return go;
    }

    private static GameObject Faixa(string nome, float centroX, float topoY, float largura,
        string caminho, int ordem, float alturaFixa = 0f)
    {
        Sprite arte = Arte(caminho);
        if (arte == null) { return null; }

        float altura = alturaFixa > 0f ? alturaFixa : arte.bounds.size.y;
        var go = new GameObject(nome);
        go.transform.SetParent(cenario, false);
        // O pivô está na base, então a faixa desenha de baixo para cima: para o TOPO dela cair
        // em topoY, a origem tem de descer a altura inteira.
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
        if (s == null) { Debug.LogWarning("[Cila] sprite não encontrado: " + caminho); }
        return s;
    }

    private static Color Cor(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color c);
        return c;
    }
}
