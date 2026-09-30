using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Odisseia.Systems;

/// <summary>
/// Veste a fase 13 com a ilha de Calipso:
///
/// Unity.exe -batchmode -quit -nographics -projectPath . -executeMethod CalipsoSceneDresser.Run
///
/// Mesmo desenho dos vestidores das fases 02 a 12 — não é sistema novo.
///
/// ## A tese desta fase é "um paraíso que virou prisão", e ela se faz por COMPOSIÇÃO
///
/// O briefing é explícito: a melancolia não pode ser pintada, tem de ser percebida. Então não há
/// nenhuma cor triste nesta paleta — a ilha é a coisa mais bonita do jogo até aqui. O que carrega
/// a prisão são três decisões de arranjo, nenhuma delas de cor:
///
/// 1. **O mar está SEMPRE visível**, do primeiro ao último metro, e é a única camada que nunca
///    muda. Numa fase de floresta densa isso é escolha: o horizonte aberto atrás de tudo é o que
///    lembra, a cada tela, que existe um lugar para onde ir e que ele não está ao alcance.
/// 2. **Os objetos de gente vêm sempre sozinhos.** Um banco, uma fonte, uma coluna quebrada — um
///    de cada, nunca dois juntos, nunca com nada em cima. Móvel isolado num jardim bonito é a
///    imagem de alguém que não está lá.
/// 3. **A luz baixa conforme se anda.** O degradê âmbar do último terço é o sol se pondo sobre a
///    saída, e não uma ameaça — é o oposto exato do véu de tempestade de Gado do Sol, mesma
///    ferramenta e cor contrária.
///
/// A geometria da cena já conta a história e a arte a segue: <c>DialogueTrigger_Grove</c> em
/// x=9..11 é a conversa no bosque, <c>Platform_Grove</c> em 12..16 é a subida até a cachoeira,
/// <c>DialogueTrigger_Raft</c> em 27..29 é onde a jangada é construída, e o <c>LevelGoal</c> em
/// 38,8..41,2 é a praia da partida.
/// </summary>
public static class CalipsoSceneDresser
{
    private const string ScenePath = "Assets/Scenes/Levels/Level_13_Calipso.unity";
    private const string Raiz = "Assets/Art/Environments/Calipso/";
    private const string RaizDoCenario = "CalipsoScenery";

    private const float TopoDoChao = -2f;
    private const float CameraInicial = 0f;

    private static float CameraMin = -16f;
    private static float CameraMax = 44f;

    /// <summary>
    /// Onde a praia de saída começa dentro do <c>Floor_3</c>. A fase tem três trechos de chão e
    /// QUATRO ambientes, então o último trecho é dividido em dois materiais — floresta até aqui,
    /// areia daqui em diante.
    /// </summary>
    private const float InicioDaPraiaFinal = 33f;

    /// <summary>
    /// Onde a floresta começa. A praia de chegada fica livre dela, para o mar aparecer do chão
    /// até o céu na abertura da fase.
    /// </summary>
    private const float InicioDaMata = -1f;

    /// <summary>
    /// Quanto a arte do tile de topo sobe para a superfície pisável encostar no colisor.
    /// Medido por <c>Tools/build-ground-tiles.js</c>: 16px de um tile de 32.
    /// </summary>
    private const float SuperficieDoTile = 16f / 42.857143f;

    private static Transform cenario;

    [MenuItem("Odisseia/Vestir Calipso")]
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

        List<GameObject> chaos = Plataformas();
        if (chaos.Count == 0)
        {
            Debug.LogError("[Calipso] nenhum Floor_* encontrado — cenário não montado");
            return false;
        }

        CameraMin = chaos.Min(g => Caixa(g).min.x);
        CameraMax = chaos.Max(g => Caixa(g).max.x);

        DesligarPlaceholders();
        MontarCeuEMar();
        MontarFloresta();
        VestirChao(chaos);
        MontarCachoeira();
        MontarPalacioEJardim();
        MontarMelancolia();
        MontarJangada();
        MontarPrimeiroPlano();
        MontarFimDeTarde();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[Calipso] cenário montado: {cenario.childCount} objetos sob {RaizDoCenario}");
        return true;
    }

    private static List<GameObject> Plataformas() =>
        Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
            .Where(g => g != null && g.name.StartsWith("Floor_") && g.GetComponent<Collider2D>() != null)
            .OrderBy(g => g.transform.position.x)
            .ToList();

    private static void DesligarPlaceholders()
    {
        // Os gatilhos de diálogo entram aqui porque são retângulos de marcação: a arte vai por
        // cima, na medida deles, e o objeto de gameplay não é tocado.
        string[] prefixos =
        {
            "Floor_", "Sky_Background", "LevelGoal", "Platform_Grove", "DialogueTrigger_",
        };
        foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (go == null || !prefixos.Any(pre => go.name.StartsWith(pre))) { continue; }
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null) { sr.enabled = false; }
        }
    }

    // ---------------------------------------------------------------- céu e mar

    private static void MontarCeuEMar()
    {
        // #bb995e é a cor MEDIDA da primeira linha de `calipso_bg_sea_horizon` (846 dos 1342 px). Cobertura fora
        // da cor medida deixa uma emenda horizontal na tela — lição das Sereias, e ela vale em
        // todas as fases desde então.
        Bloco("Sky_Fill", Meio(), 16f, 170f, 36f, Cor("#bb995e"), -60);

        // Abaixo da linha do chão é TERRA de ilha, no passo escuro de `Madeira` — não água.
        // Mar sob a floresta leria como ilha flutuante, que é o erro que Gado do Sol quase
        // cometeu. Ordem −12: logo atrás do corpo do tile, e à frente de toda a paisagem, para a
        // faixa de floresta rebaixada não reaparecer debaixo do chão.
        Bloco("Ground_Fill", Meio(), -18f, 170f, 30f, Cor("#5d403e"), -12);

        // A ÚNICA camada que carrega céu, e a que nunca sai de cena — ver a seção 1 do cabeçalho.
        //
        // baseY −1,2 e não −4,4, e esta é a correção que salvou a tese da fase. Em −4,4 a linha
        // do horizonte deste sprite caía em y≈−0,4, ou seja ABAIXO das copas da mata, e a faixa
        // de floresta cobria o mar inteiro: a primeira montagem não tinha um pixel de oceano em
        // tela nenhuma. Numa fase cujo assunto é estar preso numa ilha, isso não é um detalhe de
        // fundo — é a fase deixando de dizer a única coisa que ela precisa dizer.
        //
        // Subida, a linha do horizonte vai para y≈2,8 e as copas topam em ~1,1: sobra uma faixa
        // de mar aberto por cima das árvores, que é exatamente o que se vê de dentro de uma ilha.
        Camada("BG_Sea_Horizon", "Background/calipso_bg_sea_horizon.png", 0.92f, -50, -1.2f, 1f);

        // Linha de espuma SÓ NAS DUAS PRAIAS, e não atravessando a fase.
        //
        // Em faixa única de ponta a ponta ela corria por dentro da floresta e do jardim, com
        // ondas quebrando ao pé da fonte — espuma é onde a água encontra a areia, e em mais
        // nenhum lugar. São duas faixas porque a fase tem duas praias: a de chegada e a de
        // partida.
        //
        // Desliza a −0,08: Calipso é o mar mais CALMO da campanha, quase parado, mas mar
        // parado de verdade ainda tem o reflexo andando.
        // Ordem −30: ATRAS do chao (−10), e nao a frente. Em −8 a espuma desenhava POR CIMA da areia
        // e do barranco, com ondas quebrando no meio da praia em vez de na beira dela. A agua esta
        // alem da areia, entao ela pertence atras.
        Deslizar(Faixa("Foam_Arrival", (CameraMin - 9f + InicioDaMata) * 0.5f, TopoDoChao + 0.9f,
            InicioDaMata - CameraMin + 9f, "Ocean/calipso_foam_line.png", -30), -0.08f);
        Deslizar(Faixa("Foam_Departure", (InicioDaPraiaFinal + CameraMax + 8f) * 0.5f,
            TopoDoChao + 0.9f, CameraMax + 8f - InicioDaPraiaFinal,
            "Ocean/calipso_foam_line.png", -30), -0.08f);
    }

    /// <summary>
    /// A faixa de floresta, BAIXADA para só as copas aparecerem acima da linha do chão.
    ///
    /// Em tamanho nativo e assente na linha do chão ela cobriria o quadro inteiro e a ilha, que
    /// precisa parecer aberta e cercada de mar, leria como floresta fechada — foi exatamente o
    /// que aconteceu com o olival de Gado do Sol. Encolher o transform NÃO serve: encolhe também
    /// a largura e abre uma borda vertical dura no meio da tela. Baixar serve.
    /// </summary>
    private static void MontarFloresta()
    {
        // A mata é uma FAIXA fixa no mundo, e não uma camada de parallax — e a razão é uma
        // impossibilidade, não uma preferência.
        //
        // A tentativa anterior foi recortar a camada de parallax pela esquerda. Não funciona:
        // uma camada de parallax se desloca em relação ao mundo a cada quadro, então ela não
        // TEM borda esquerda fixa em x — encurtá-la só muda onde a borda passeia. A mata
        // continuou cobrindo a praia, que é justamente o lugar onde o mar precisa aparecer do
        // chão até o céu.
        //
        // Presa ao mundo, ela começa onde a ilha começa e fica lá. O custo é perder o parallax
        // dessa faixa; o mar atrás dela continua com o dele, e é o mar que carrega a
        // profundidade nesta fase.
        // A faixa termina em InicioDaPraiaFinal: as DUAS praias ficam livres de mata. A de chegada
        // abre a fase com mar do chao ao ceu; a de partida fecha com a mesma imagem, e e essa
        // rima que faz a saida ler como saida.
        GameObject mata = Faixa("Forest_Band", (InicioDaMata + InicioDaPraiaFinal) * 0.5f, 1.12f,
            InicioDaPraiaFinal - InicioDaMata, "Midground/calipso_forest_band.png", -40);
        if (mata != null) { mata.GetComponent<SpriteRenderer>().color = Distancia(0.58f); }

        // Árvores antigas e ciprestes ao longo do trecho de floresta, ATRÁS do plano de jogo.
        // Nenhuma copa entra na ordem do jogador: a seção de leitura do briefing é explícita, e
        // uma copa de 7 unidades na frente esconderia plataforma e inimigo de uma vez.
        // As árvores em −1 e 33 não são decoração: elas MASCARAM as duas pontas da faixa de
        // mata. Uma faixa presa ao mundo tem borda esquerda e direita visíveis, e sem nada em
        // cima delas o olho lê um corte vertical reto no meio da floresta. Uma copa larga na
        // junta resolve, e é de graça — o asset já está na cena.
        var arvores = new (float x, string arte, float escala, int ordem)[]
        {
            (-1f, "Forest/calipso_tree_ancient.png", 0.70f, -20),      // máscara da ponta esquerda
            (4f, "Forest/calipso_cypress.png", 0.72f, -21),
            (8f, "Forest/calipso_tree_ancient.png", 0.55f, -21),
            (12.5f, "Forest/calipso_cypress.png", 0.80f, -20),
            (19f, "Forest/calipso_tree_ancient.png", 0.68f, -20),
            (23f, "Forest/calipso_cypress.png", 0.66f, -21),
            (28.5f, "Forest/calipso_cypress.png", 0.74f, -21),
            (33f, "Forest/calipso_tree_ancient.png", 0.66f, -20),      // máscara da ponta direita
        };
        foreach (var (x, arte, escala, ordem) in arvores)
        {
            GameObject go = Prop($"Tree_{x:0}", x, TopoDoChao - 0.1f, arte, ordem);
            if (go == null) { continue; }
            // Uma cópia em cada duas é ESPELHADA. São três sprites de árvore para oito
            // posições, e sem espelhar duas árvores idênticas lado a lado leem como erro de
            // montagem, não como bosque.
            int i = System.Array.IndexOf(arvores, (x, arte, escala, ordem));
            go.transform.localScale = new Vector3(i % 2 == 0 ? escala : -escala, escala, 1f);
            go.GetComponent<SpriteRenderer>().color = Distancia(0.42f);
        }
    }

    /// <summary>
    /// Perspectiva aérea: cada camada é puxada para a cor do céu na proporção do parallax.
    /// Nenhum asset novo, nenhum shader — só <c>sr.color</c>.
    /// </summary>
    private static Color Distancia(float fator) =>
        Color.Lerp(Color.white, Cor("#bb995e"), Mathf.Clamp01((fator - 0.30f) * 0.80f));

    // ---------------------------------------------------------------- chão

    /// <summary>
    /// Areia na praia de chegada, chão de floresta no miolo, areia de novo na praia de partida.
    ///
    /// A troca de material é o único aviso de que o ambiente mudou, e é o mesmo recurso que
    /// Sereias usa na ilha e Gado do Sol no terreno sagrado. O <c>Floor_3</c> é DIVIDIDO em dois
    /// materiais porque ele sozinho cobre dois ambientes — a fase tem três trechos de chão e
    /// quatro ambientes, e forçar um material por trecho faria o palácio nascer na areia.
    /// </summary>
    private static void VestirChao(List<GameObject> chaos)
    {
        foreach (GameObject chao in chaos)
        {
            Bounds b = Caixa(chao);
            bool praiaInicial = b.max.x <= 0.01f;

            float folgaEsq = b.min.x <= CameraMin + 0.01f ? 9f : 0f;
            float folgaDir = b.max.x >= CameraMax - 0.01f ? 8f : 0f;

            if (!praiaInicial && b.max.x > InicioDaPraiaFinal && b.min.x < InicioDaPraiaFinal)
            {
                // O trecho da partida: floresta até a linha, areia depois dela.
                Trecho($"{chao.name}_Mata", b.min.x - folgaEsq, InicioDaPraiaFinal, b.max.y, "grass");
                Trecho($"{chao.name}_Praia", InicioDaPraiaFinal, b.max.x + folgaDir, b.max.y, "sand");
                continue;
            }

            Trecho(chao.name, b.min.x - folgaEsq, b.max.x + folgaDir, b.max.y,
                praiaInicial ? "sand" : "grass");
        }
    }

    private static void Trecho(string nome, float x0, float x1, float topoY, string material)
    {
        float largura = x1 - x0, centro = (x0 + x1) * 0.5f;
        string prefixo = "Gameplay/calipso_" + material;

        GameObject topo = Faixa($"GroundTop_{nome}", centro, topoY + SuperficieDoTile, largura,
            prefixo + "_ground_top.png", -10);
        if (topo == null) { return; }

        // Altura do SPRITE, nunca constante escrita à mão: o pipeline recorta estes tiles, e
        // todo recorte muda a altura sem avisar ninguém.
        float alturaDoTile = topo.GetComponent<SpriteRenderer>().sprite.bounds.size.y;

        GameObject corpo = Faixa($"GroundBody_{nome}", centro,
            topoY + SuperficieDoTile - alturaDoTile, largura, prefixo + "_ground_body.png", -11, 2.4f);

        // Corpo escurecido: a face sob a plataforma não recebe luz. Sem isso as unidades de tile
        // ladrilhado leem como parede de tijolo em vez de barranco — medido em Cila e Caribdis.
        if (corpo != null)
        {
            corpo.GetComponent<SpriteRenderer>().color = new Color(0.50f, 0.46f, 0.41f, 1f);
        }
    }

    // ---------------------------------------------------------------- set-pieces

    /// <summary>
    /// A cachoeira, atrás da plataforma elevada do bosque.
    ///
    /// A posição sai do <c>Platform_Grove</c>, não de um número escrito à mão: a plataforma é a
    /// subida, e a queda d'água é o que justifica visualmente que se suba ali. Se o level design
    /// mover a plataforma, a cachoeira acompanha.
    /// </summary>
    private static void MontarCachoeira()
    {
        GameObject plataforma = GameObject.Find("Platform_Grove");
        if (plataforma == null)
        {
            Debug.LogWarning("[Calipso] Platform_Grove não encontrada — cachoeira não posicionada");
            return;
        }

        Bounds b = Caixa(plataforma);
        GameObject falesia = Prop("Waterfall_Cliff", b.center.x + 1.5f, TopoDoChao - 0.2f,
            "Waterfall/calipso_cliff_waterfall.png", -22);
        Encolher(falesia, 0.85f);

        // A plataforma em si ganha rocha embaixo, senão ela flutua.
        GameObject pedras = Prop("Grove_Rocks", b.center.x, b.min.y - 0.15f,
            "Beach/calipso_shore_rocks.png", -6);
        Encolher(pedras, 0.7f);
    }

    /// <summary>
    /// O palácio de Calipso e o jardim, no trecho depois do bosque.
    ///
    /// Fica ATRÁS do plano de jogo (ordem −18) e assente na linha do chão. Não é camada de
    /// parallax: um palácio que desliza em relação ao chão deixa de parecer construído NA ilha.
    /// </summary>
    private static void MontarPalacioEJardim()
    {
        GameObject palacio = Prop("Palace", 26.5f, TopoDoChao - 0.15f,
            "Palace/calipso_palace.png", -18);
        // 0,72 leva o palácio a 4,7 un de altura — três vezes e meia o Odisseu. É uma MORADA,
        // não um templo: o de Hélio, na fase 12, mede 7,2 e precisa dominar; este precisa
        // parecer um lugar onde se vive, e um lugar onde se vive há anos demais.
        Encolher(palacio, 0.72f);

        Encolher(Prop("Fountain", 21f, TopoDoChao, "Garden/calipso_fountain.png", -2), 0.55f);

        foreach (float x in new[] { 19.5f, 24f, 29.5f })
        {
            Encolher(Prop($"Flowers_{x:0}", x, TopoDoChao + 0.05f,
                "Garden/calipso_garden_flowers.png", -3), 0.6f);
        }
    }

    /// <summary>
    /// Os objetos de melancolia — e a regra deles é o que os faz funcionar.
    ///
    /// **Um de cada, nunca dois juntos, nunca com nada em cima.** O briefing pede que a
    /// melancolia venha da composição e não da cor, e o que produz isso é o isolamento: um banco
    /// sozinho com uma coluna quebrada ao lado, a doze unidades de qualquer outra coisa, num
    /// jardim que é a coisa mais bonita do jogo. Dois bancos lado a lado viram mobiliário; um
    /// banco sozinho vira ausência.
    /// </summary>
    private static void MontarMelancolia()
    {
        Encolher(Prop("Bench_Ruin_A", 16.5f, TopoDoChao, "Props/calipso_bench_ruin.png", -2), 0.62f);
        Encolher(Prop("Bench_Ruin_B", 32f, TopoDoChao, "Props/calipso_bench_ruin.png", -2), 0.55f);
    }

    /// <summary>
    /// A jangada, no gatilho de diálogo que o level design já tinha.
    ///
    /// É o assunto narrativo da fase e o único objeto de madeira crua num lugar de mármore e
    /// folhagem — por isso ela tem rampa própria na paleta. Fica em ordem POSITIVA, à frente do
    /// plano de jogo: é a única coisa nesta ilha que o jogador precisa notar.
    /// </summary>
    private static void MontarJangada()
    {
        GameObject gatilho = GameObject.Find("DialogueTrigger_Raft");
        float x = gatilho != null ? Caixa(gatilho).center.x : 28f;
        Encolher(Prop("Raft", x, TopoDoChao, "Special/calipso_raft.png", 2), 0.62f);

        // Pedras na praia de partida, entre a jangada e o objetivo.
        foreach (float px in new[] { -13f, -5f, 35f, 39f })
        {
            Encolher(Prop($"Shore_Rocks_{px:0}", px, TopoDoChao - 0.1f,
                "Beach/calipso_shore_rocks.png", -2), 0.5f);
        }
    }

    /// <summary>
    /// Primeiro plano: samambaias baixas, e só ABAIXO da linha do chão.
    ///
    /// Sem <see cref="ParallaxLayer"/> — o fator dele é limitado a [0,1] e primeiro plano
    /// exigiria mais que 1. Ficam em ordem acima do jogador mas nunca o escondem, porque a
    /// altura delas é 2,15 un contra 1,4 do personagem e a base está enterrada.
    /// </summary>
    private static void MontarPrimeiroPlano()
    {
        // CINCO, e não oito. Com oito elas formavam uma sebe contínua atravessando a base do
        // quadro; primeiro plano é pontuação, não moldura.
        float[] posicoes = { -9f, 4f, 16f, 25f, 36f };
        for (int i = 0; i < posicoes.Length; i++)
        {
            GameObject go = Prop($"FG_Ferns_{i}", posicoes[i], TopoDoChao - 0.75f,
                "Foreground/calipso_ferns.png", 12);
            Encolher(go, 0.52f + (i % 3) * 0.08f);
        }
    }

    /// <summary>
    /// O sol baixando sobre a saída.
    ///
    /// Um sprite de 256×8 px com o alpha variando por coluna, esticado sobre o último trecho.
    /// Quatro retângulos de alpha uniforme NÃO servem: cada fronteira vira um degrau visível e o
    /// resultado lê como interface — foi o que aconteceu na primeira montagem de Gado do Sol.
    /// Alpha uniforme não vira degradê por empilhamento; o degradê tem de estar na imagem.
    /// </summary>
    private static void MontarFimDeTarde()
    {
        Sprite gradiente = Arte("VFX/calipso_dusk_gradient.png");
        if (gradiente == null) { return; }

        float x0 = InicioDaPraiaFinal - 6f;
        float ate = CameraMax + 14f;

        var veu = new GameObject("DuskVeil");
        veu.transform.SetParent(cenario, false);
        // y = −16, e não um valor positivo: o pivô está na BASE e o sprite desenha do pivô PARA
        // CIMA. É o erro que em Gado do Sol pôs o véu inteiro acima do que a câmera mostra, sem
        // uma linha no console.
        veu.transform.position = new Vector3((x0 + ate) * 0.5f, -16f, 0f);

        var sr = veu.AddComponent<SpriteRenderer>();
        sr.sprite = gradiente;
        // Escala no TRANSFORM, e não `drawMode`/`size`: Sliced e Tiled trabalham sobre a malha,
        // e a malha "tight" recorta regiões totalmente transparentes — que neste sprite são a
        // metade esquerda inteira, onde o alpha começa em zero.
        veu.transform.localScale = new Vector3(
            (ate - x0) / gradiente.bounds.size.x, 40f / gradiente.bounds.size.y, 1f);
        // Ordem 20: acima de tudo, inclusive do jogador. É o AR entre a câmera e a cena que
        // muda, e com alpha máximo de 0,42 ele continua perfeitamente legível.
        sr.sortingOrder = 20;
    }

    // ---------------------------------------------------------------- utilidades

    private static float Meio() => (CameraMin + CameraMax) * 0.5f;

    private static Bounds Caixa(GameObject go) => go.GetComponent<Collider2D>().bounds;

    private static void Encolher(GameObject go, float k)
    {
        if (go != null) { go.transform.localScale = new Vector3(k, k, 1f); }
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
    /// Camada de parallax dimensionada para cobrir a fase inteira. O início vem da câmera REAL
    /// da cena, não de uma constante: a Main Camera não fica onde o jogador nasce, e assumir que
    /// ficava deixou uma faixa chapada na borda do quadro em Cila e Caribdis — visível só no
    /// objetivo, a 50 unidades de onde o erro estava.
    /// </summary>
    private static GameObject Camada(string nome, string caminho, float fator, int ordem,
        float baseY, float alturas, float comecaEm = float.NegativeInfinity)
    {
        Sprite arte = Arte(caminho);
        if (arte == null) { return null; }

        const float larguraDaTela = 18f;
        float camInicio = Camera.main != null ? Camera.main.transform.position.x : CameraInicial;
        // `comecaEm` recorta a camada pela esquerda. Uma camada de parallax normalmente cobre a
        // fase inteira, mas a mata de Calipso não pode: ela precisa parar antes da praia.
        float esquerda = float.IsNegativeInfinity(comecaEm) ? CameraMin : Mathf.Max(CameraMin, comecaEm);
        float meio = (esquerda + CameraMax) * 0.5f;
        float curso = CameraMax - esquerda;
        float x0 = camInicio * fator + meio * (1f - fator);

        var go = new GameObject(nome);
        go.transform.SetParent(cenario, false);
        go.transform.position = new Vector3(x0, baseY, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = arte;
        sr.sortingOrder = ordem;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.tileMode = SpriteTileMode.Continuous;
        sr.size = new Vector2(curso * (1f - fator) + larguraDaTela * 2f, arte.bounds.size.y * alturas);

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
        // Pivô na base: a faixa desenha de baixo para cima, então a origem desce a altura toda
        // para o TOPO dela cair em topoY.
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
        if (s == null) { Debug.LogWarning("[Calipso] sprite não encontrado: " + caminho); }
        return s;
    }

    private static Color Cor(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color c);
        return c;
    }
}
