using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Odisseia.Systems;

/// <summary>
/// Veste a fase 14 com o retorno a Ítaca:
///
/// Unity.exe -batchmode -quit -nographics -projectPath . -executeMethod ItacaReturnSceneDresser.Run
///
/// Mesmo desenho dos vestidores das fases 02 a 13 — não é sistema novo.
///
/// ## Esta fase é feita de REUSO, e o número é a tese
///
/// Dos objetos de cenário montados aqui, a grande maioria é arte da FASE 01, carregada direto de
/// <c>Assets/Art/Environments/Ithaca/</c> sem cópia e sem variação: o palácio, a casa de Odisseu,
/// a casa pequena, o armazém, o portão, a coluna, o muro, a oliveira, o cipreste, as pedras, o
/// navio, o poste de cais, a rede, a âncora, a ânfora, o barril, a carroça, o banco, a cerca.
/// Só SEIS assets são novos, e cada um existe para dizer que o tempo passou.
///
/// Isso não é economia — é o requisito. O briefing pede que o jogador reconheça Ítaca
/// imediatamente, e reconhecimento não se produz com arte parecida: produz-se com a MESMA arte.
/// A paleta também é a da fase 01, sem trocar um hex (ver o cabeçalho de
/// <c>Tools/build-itacaret.js</c>).
///
/// ## O que mudou, então
///
/// Três coisas, e as três são deliberadas:
///
/// 1. **A LUZ.** A fase 01 acontece sob céu azul de meio-dia; esta, num fim de tarde dourado que
///    vira anoitecer no último terço. Mesmo lugar, outra hora — é a diferença que o olho lê
///    primeiro e a que não exige nenhum asset novo.
/// 2. **O ESTADO.** Casa tomada de hera, cerca quebrada, barco apodrecendo na praia, mato
///    crescido no que antes era cuidado. Os prédios reusados ficam INTACTOS de propósito: o
///    contraste entre o que resistiu e o que não resistiu é o que conta a passagem do tempo.
///    Se tudo estivesse arruinado, seria outra cidade.
/// 3. **O QUE PASSOU A EXISTIR.** A barraca de feira e os braseiros acesos não têm equivalente
///    na fase 01. A feira diz que a vila seguiu viva sem ele; os braseiros, que alguém está
///    esperando alguma coisa esta noite.
///
/// A geometria da cena já conta a história e a arte a segue: <c>NPC_Eumeu</c> em x=−1..1 é o
/// primeiro reencontro, os dois <c>EnemyBasic</c> em 8 e 16 são homens dos pretendentes na vila,
/// <c>NPC_Telemaco</c> em 27..29 é o filho, e o <c>LevelGoal</c> em 37,4..38,6 é a porta de casa.
/// </summary>
public static class ItacaReturnSceneDresser
{
    private const string ScenePath = "Assets/Scenes/Levels/Level_14_Itaca_Return.unity";
    private const string Raiz = "Assets/Art/Environments/ItacaReturn/";

    /// <summary>A arte da fase 01, carregada de onde ela já vive. Não há cópia local.</summary>
    private const string Ithaca = "Assets/Art/Environments/Ithaca/";

    /// <summary>
    /// O kit da fase 15. Os sinais de ocupação que aparecem no fim desta fase são os MESMOS
    /// sprites que a fase 15 usa — é o que faz a passagem de uma para a outra ler como a mesma
    /// ocupação crescendo, e não como duas ideias diferentes de invasão.
    /// </summary>
    private const string Pretendentes = "Assets/Art/Environments/Pretendentes/";
    private const string RaizDoCenario = "ItacaReturnScenery";

    private const float TopoDoChao = -2f;
    private const float CameraInicial = 0f;

    private static float CameraMin = -18f;
    private static float CameraMax = 42f;

    /// <summary>Onde o bairro do palácio começa — o chão troca de grama para laje ali.</summary>
    private static float InicioDoPalacio = 20f;

    private const float SuperficieDoTile = 16f / 42.857143f;

    private static Transform cenario;
    private static int reusados, novos;

    [MenuItem("Odisseia/Vestir Ítaca Return")]
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
        reusados = 0; novos = 0;

        List<GameObject> chaos = Plataformas();
        if (chaos.Count == 0)
        {
            Debug.LogError("[ItacaReturn] nenhum Floor_* encontrado — cenário não montado");
            return false;
        }

        CameraMin = chaos.Min(g => Caixa(g).min.x);
        CameraMax = chaos.Max(g => Caixa(g).max.x);
        GameObject terceiro = chaos.FirstOrDefault(g => g.name == "Floor_3");
        if (terceiro != null) { InicioDoPalacio = Caixa(terceiro).min.x; }

        DesligarPlaceholders();
        MontarCeuEHorizonte();
        VestirChao(chaos);
        MontarPorto();
        MontarVila();
        MontarPalacio();
        MontarVegetacao();
        MontarPrimeiroPlano();
        MontarAnoitecer();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[ItacaReturn] cenário montado: {cenario.childCount} objetos sob {RaizDoCenario} " +
            $"— {reusados} REUSADOS da fase 01, {novos} novos");
        return true;
    }

    private static List<GameObject> Plataformas() =>
        Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
            .Where(g => g != null && g.name.StartsWith("Floor_") && g.GetComponent<Collider2D>() != null)
            .OrderBy(g => g.transform.position.x)
            .ToList();

    private static void DesligarPlaceholders()
    {
        // "Roof" e "LevelGoal" entram porque marcam a porta de casa: a arte da casa de Odisseu,
        // que vem da fase 01, vai por cima na medida deles.
        string[] prefixos = { "Floor_", "Sky_Background", "LevelGoal", "Roof" };
        foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (go == null || !prefixos.Any(pre => go.name.StartsWith(pre))) { continue; }
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null) { sr.enabled = false; }
        }
    }

    // ---------------------------------------------------------------- céu e horizonte

    private static void MontarCeuEHorizonte()
    {
        // #99a593 é a cor MEDIDA da primeira linha de `itaca_ret_bg_dusk` (546 dos 1342 px). O topo
        // do céu de fim de tarde é o VERDE-ACINZENTADO frio que sobra acima do ouro, e não o ouro:
        // usar o ouro aqui deixaria uma emenda horizontal onde a cobertura encontra a camada.
        Bloco("Sky_Fill", Meio(), 16f, 170f, 36f, Cor("#99a593"), -60);

        // Abaixo da linha do chão: terra, no passo escuro de `Terra / caminho` da paleta de
        // Ítaca. Ordem −12, logo atrás do corpo do tile e à frente de toda a paisagem — em
        // ordem de fundo, a faixa de vila rebaixada reaparece debaixo do chão.
        // O TOPO deste bloco fica em y=−1,9, e não em −3 — e esta é uma correção medida, não
        // estética. Os tilesets de Ítaca têm falhas na fileira de superfície, e com o bloco
        // terminando em −3 nada cobria a faixa entre −3 e −2 atrás delas: a cor de fundo da
        // câmera vazava numa linha pontilhada MAGENTA exatamente sobre a linha do chão — até 118
        // pixels por quadro, contados na captura (linha y=378). Calipso não tem o defeito porque
        // os tiles dela têm a superfície fechada. Os 0,1 un de sobra acima do chão escondem no
        // máximo 4 px da base do que estiver atrás, e isso já fica sob o tile de superfície.
        Bloco("Ground_Fill", Meio(), -17.95f, 170f, 32.1f, Cor("#6b5d4e"), -12);

        Camada("BG_Dusk", "Background/itaca_ret_bg_dusk.png", 0.92f, -50, -1.6f, 1f);

        // A SILHUETA DE MONTANHAS DA FASE 01, sem tocar num pixel.
        //
        // É o reuso mais barato e o mais eficaz da fase: o horizonte de Ítaca é a primeira coisa
        // que aparece na tela e é literalmente o mesmo arquivo que a fase 01 desenha. Puxado
        // para a cor do céu de fim de tarde, ele diz "mesmo lugar, outra hora" antes de qualquer
        // prédio entrar em quadro.
        GameObject montanhas = Camada("BG_Mountains", Ithaca + "Background/ithaca_bg_mountains_far.png",
            0.86f, -48, -0.2f, 1f);
        if (montanhas != null)
        {
            montanhas.GetComponent<SpriteRenderer>().color = Distancia(0.86f);
            reusados++;
        }

        // A faixa de vila ao fundo, começando depois do porto.
        GameObject vila = Faixa("Village_Band", (InicioDaVila + CameraMax + 8f) * 0.5f, 1.6f,
            CameraMax + 8f - InicioDaVila, "Midground/itaca_ret_village_band.png", -40);
        if (vila != null) { vila.GetComponent<SpriteRenderer>().color = Distancia(0.55f); novos++; }
    }

    /// <summary>Onde a vila começa. O porto fica livre dela, para o mar aparecer na chegada.</summary>
    private const float InicioDaVila = -3f;

    /// <summary>Perspectiva aérea: cada camada é puxada para a cor do céu de fim de tarde.</summary>
    private static Color Distancia(float fator) =>
        Color.Lerp(Color.white, Cor("#99a593"), Mathf.Clamp01((fator - 0.30f) * 0.72f));

    // ---------------------------------------------------------------- chão

    /// <summary>
    /// Grama no porto e na vila, laje de pedra no bairro do palácio — e as duas folhas são as
    /// da FASE 01, extraídas dos mesmos tilesets Wang que ela usa.
    ///
    /// A troca de material no <c>Floor_3</c> é o aviso silencioso de que se chegou à parte alta
    /// da cidade, o mesmo recurso que Sereias, Gado do Sol e Calipso usam.
    /// </summary>
    private static void VestirChao(List<GameObject> chaos)
    {
        foreach (GameObject chao in chaos)
        {
            Bounds b = Caixa(chao);
            bool palacio = b.min.x >= InicioDoPalacio - 0.01f;

            float folgaEsq = b.min.x <= CameraMin + 0.01f ? 9f : 0f;
            // 12 e nao 8: a camera desta cena vai ate x=44 e enxerga ate 52,9, entao uma folga de 8
            // deixava um buraco MAGENTA — a cor de fundo da camera — no canto inferior direito do
            // ultimo quadro da fase. A folga tem de cobrir o alcance da camera, nao o fim do chao.
            float folgaDir = b.max.x >= CameraMax - 0.01f ? 12f : 0f;
            float largura = b.size.x + folgaEsq + folgaDir;
            float centro = b.center.x + (folgaDir - folgaEsq) * 0.5f;
            string prefixo = "Gameplay/ithaca_" + (palacio ? "stone" : "grass");

            GameObject topo = Faixa($"GroundTop_{chao.name}", centro, b.max.y + SuperficieDoTile,
                largura, prefixo + "_ground_top.png", -10);
            if (topo == null) { continue; }
            reusados++;

            float alturaDoTile = topo.GetComponent<SpriteRenderer>().sprite.bounds.size.y;
            GameObject corpo = Faixa($"GroundBody_{chao.name}", centro,
                b.max.y + SuperficieDoTile - alturaDoTile, largura, prefixo + "_ground_body.png",
                -11, 2.4f);
            // O barranco de LAJE é escurecido mais que o de grama (0,38 contra 0,55). A folha
            // de pedra da fase 01 é clara, e no tom da grama ela lia como um muro de tijolo
            // aparente ocupando o terço de baixo da tela em vez de terra sob o calçamento.
            if (corpo != null)
            {
                float k = palacio ? 0.38f : 0.55f;
                corpo.GetComponent<SpriteRenderer>().color = new Color(k, k * 0.94f, k * 0.86f, 1f);
            }
        }
    }

    // ---------------------------------------------------------------- porto

    /// <summary>
    /// O porto, no primeiro trecho. Tudo daqui vem da fase 01 menos o barco apodrecido.
    ///
    /// O navio vai com a vela RECOLHIDA (<c>ithaca_ship_01_furled</c>) e não aberta: na fase 01
    /// ele parte, aqui ele está parado. É a mesma embarcação, no estado oposto, e a fase 01 já
    /// tinha os dois sprites — a variação custou zero.
    /// </summary>
    private static void MontarPorto()
    {
        Encolher(Reuso("Ship_Moored", -15f, TopoDoChao, "Ships/ithaca_ship_01_furled.png", -6), 0.85f);

        foreach (float x in new[] { -11.5f, -9.5f, -7.5f })
        {
            Encolher(Reuso($"Dock_Post_{x:0}", x, TopoDoChao, "Port/ithaca_dock_post_01.png", -3), 0.9f);
        }
        Encolher(Reuso("Fishing_Net", -6f, TopoDoChao, "Port/ithaca_fishing_net_01.png", -2), 0.8f);
        Encolher(Reuso("Anchor", -12.5f, TopoDoChao, "Port/ithaca_anchor_01.png", -2), 0.7f);
        Encolher(Reuso("Rope_Coil", -4.5f, TopoDoChao, "Props/ithaca_rope_coil_01.png", -2), 0.7f);

        // O único objeto NOVO do porto. Um casco largado a apodrecer diz que faz tempo que
        // ninguém cuida deste cais — e ele fica ao lado do navio intacto, porque é o contraste
        // entre os dois que conta a passagem do tempo, não a ruína sozinha.
        Encolher(Novo("Boat_Derelict", -2.5f, TopoDoChao, "Harbor/itaca_ret_boat_derelict.png", -2), 0.62f);

        // O navio dos pretendentes, atracado no mesmo cais, ATRÁS da galera de Odisseu.
        //
        // O porto da fase 01 tinha um navio; agora tem dois, e o segundo tem vela vermelha com o
        // javali. É o primeiro sinal da invasão, e ele chega pelo mar — antes de qualquer
        // estandarte na vila, o jogador vê que alguém de fora está em Ítaca. Ordem −9: atrás do
        // navio de Odisseu (−6) e dos postes do cais (−3), para ler como fundeado na mesma baía.
        GameObject pretendente = Prop("Suitor_Ship", -9f, TopoDoChao, Pretendentes + "Invasion/pret_suitor_ship.png", -9);
        if (pretendente != null) { Encolher(pretendente, 0.72f); novos++; }
    }

    // ---------------------------------------------------------------- vila

    private static void MontarVila()
    {
        // Casas e armazém da fase 01, INTACTOS. Ver a seção 2 do cabeçalho: o que resistiu tem
        // de continuar em pé para o que não resistiu significar alguma coisa.
        Encolher(Reuso("House_Small_A", 2.5f, TopoDoChao - 0.1f,
            "Architecture/ithaca_house_small_01.png", -18), 0.9f);
        Encolher(Reuso("Warehouse", 9f, TopoDoChao - 0.1f,
            "Architecture/ithaca_warehouse_01.png", -19), 0.7f);
        Encolher(Reuso("House_Small_B", 16.5f, TopoDoChao - 0.1f,
            "Architecture/ithaca_house_small_01.png", -18), 0.95f);

        // A casa tomada de hera, entre duas casas em pé. É o objeto que carrega a passagem do
        // tempo na vila, e ele só funciona com vizinhas intactas dos dois lados.
        Encolher(Novo("House_Overgrown", 6f, TopoDoChao - 0.1f,
            "Variations/itaca_ret_house_overgrown.png", -18), 0.85f);

        // A feira: a vila seguiu viva sem ele.
        Encolher(Novo("Market_Stall", 12.5f, TopoDoChao, "Village/itaca_ret_market_stall.png", -4), 0.72f);
        Encolher(Reuso("Amphora", 14.2f, TopoDoChao, "Props/ithaca_amphora_01.png", -2), 0.75f);
        Encolher(Reuso("Barrel", 10.8f, TopoDoChao, "Props/ithaca_barrel_01.png", -2), 0.8f);
        Encolher(Reuso("Crate", 4f, TopoDoChao, "Props/ithaca_crate_01.png", -2), 0.8f);
        Encolher(Reuso("Cart", 19f, TopoDoChao, "Props/ithaca_cart_01.png", -3), 0.75f);
        Encolher(Reuso("Bench", 0.5f, TopoDoChao, "Props/ithaca_bench_01.png", -2), 0.8f);

        // Cerca inteira e cerca quebrada, no mesmo trecho e na mesma escala — o par é a imagem.
        Encolher(Reuso("Fence_Whole", -0.5f, TopoDoChao, "Props/ithaca_fence_01.png", -3), 0.8f);
        Encolher(Novo("Fence_Broken", 17.8f, TopoDoChao, "Variations/itaca_ret_fence_broken.png", -3), 0.8f);

        Encolher(Reuso("Wall_Low", 7.5f, TopoDoChao, "Architecture/ithaca_wall_low_01.png", -5), 0.9f);
    }

    // ---------------------------------------------------------------- palácio

    /// <summary>
    /// O bairro alto. Aqui está o momento de reconhecimento da fase.
    ///
    /// O palácio da fase 01 entra INTEIRO e no tamanho nativo, atrás do plano de jogo: ele é o
    /// maior asset do jogo (17,8 un de largura) e a coisa que o jogador viu no primeiro minuto
    /// de campanha. Reencontrá-lo é o pagamento que o briefing pede.
    ///
    /// A casa de Odisseu fica NO OBJETIVO, porque o objetivo é a porta de casa.
    /// </summary>
    private static void MontarPalacio()
    {
        // Escala 0,62, e não 0,90.
        //
        // Em 0,90 o palácio mede 16 × 6,3 unidades e a câmera mostra 17,8 × 10: ele tomava o
        // quadro inteiro e lia como uma PAREDE, sem céu nem mar em volta. O briefing pede o
        // oposto — que a arquitetura não bloqueie a visão — e o reencontro com o palácio
        // funciona melhor com ele inteiro em quadro do que com uma coluna dele em close.
        // Em 0,62 ele mede 11 × 4,3, sobra céu por cima e ainda é de longe o maior prédio
        // do jogo.
        Encolher(Reuso("Palace", InicioDoPalacio + 9f, TopoDoChao - 0.15f,
            "Architecture/ithaca_palace_01.png", -20), 0.62f);

        Encolher(Reuso("Gate", InicioDoPalacio + 1.5f, TopoDoChao - 0.05f,
            "Architecture/ithaca_gate_01.png", -6), 0.85f);
        Encolher(Reuso("Column", InicioDoPalacio + 17f, TopoDoChao - 0.05f,
            "Architecture/ithaca_column_01.png", -6), 0.8f);

        GameObject alvo = GameObject.Find("LevelGoal");
        float xCasa = alvo != null ? Caixa(alvo).center.x : 38f;
        Encolher(Reuso("House_Odysseus", xCasa, TopoDoChao - 0.1f,
            "Architecture/ithaca_house_odysseus_01.png", -8), 0.95f);

        // Dois braseiros acesos ladeando a porta de casa. São NOVOS e são o único fogo da fase:
        // alguém acendeu isto, hoje, e está esperando. É a tensão que prepara a fase 15.
        foreach (float dx in new[] { -2.6f, 2.6f })
        {
            Encolher(Novo($"Brazier_{dx:0}", xCasa + dx, TopoDoChao, "Props/itaca_ret_brazier.png", 3), 0.62f);
        }

        // A ÁREA JÁ CONTROLADA PELOS PRETENDENTES — o trecho que o briefing desta rodada pede e
        // que a primeira montagem não tinha.
        //
        // Estandarte do javali pendurado no palácio de Odisseu, uma barricada encostada no muro
        // depois de Telêmaco (x=27..29) e uma tocha dos pretendentes. São poucos objetos e todos
        // do kit da fase 15: a fase 14 não mostra a ocupação, ela a ANUNCIA, e o jogador
        // reencontra exatamente estes sprites, multiplicados, na fase seguinte.
        //
        // Tudo em ordem negativa, atrás do plano de jogo: a barricada é cenário, não obstáculo, e
        // não pode parecer colisão.
        foreach (float x in new[] { InicioDoPalacio + 5.5f, InicioDoPalacio + 12.5f })
        {
            GameObject bandeira = Prop($"Suitor_Banner_{x:0}", x, 0f, Pretendentes + "Invasion/pret_banner.png", -19);
            if (bandeira == null) { continue; }
            const float escala = 0.42f;
            Encolher(bandeira, escala);
            float altura = bandeira.GetComponent<SpriteRenderer>().sprite.bounds.size.y * escala;
            bandeira.transform.position = new Vector3(x, TopoDoChao + 3.9f - altura, 0f);
            novos++;
        }
        GameObject barricada = Prop("Suitor_Barricade", InicioDoPalacio + 11.8f, TopoDoChao,
            Pretendentes + "Invasion/pret_barricade.png", -7);
        if (barricada != null) { Encolher(barricada, 0.5f); novos++; }
        GameObject tocha = Prop("Suitor_Torch", InicioDoPalacio + 5f, TopoDoChao,
            Pretendentes + "VFX/pret_torch_stand.png", -7);
        if (tocha != null) { Encolher(tocha, 0.6f); novos++; }
    }

    // ---------------------------------------------------------------- vegetação

    /// <summary>
    /// Oliveiras, ciprestes e pedras da fase 01, mais o mato crescido que é novo.
    ///
    /// Todas as copas ficam em ordem NEGATIVA, atrás do plano de jogo — a seção de leitura do
    /// briefing é explícita, e esta fase tem inimigos em x=8 e x=16 que não podem sumir atrás de
    /// uma árvore.
    /// </summary>
    private static void MontarVegetacao()
    {
        var plantas = new (float x, string arte, float escala, int ordem)[]
        {
            (-3f, "Nature/ithaca_tree_cypress_01.png", 0.85f, -16),
            (1f, "Nature/ithaca_tree_olive_01.png", 0.9f, -16),
            (5f, "Nature/ithaca_tree_cypress_01.png", 0.75f, -17),
            (11f, "Nature/ithaca_tree_olive_01.png", 0.8f, -16),
            (15f, "Nature/ithaca_tree_cypress_01.png", 0.9f, -17),
            (21.5f, "Nature/ithaca_tree_cypress_01.png", 0.95f, -16),
            (26f, "Nature/ithaca_tree_olive_01.png", 0.85f, -16),
            (33f, "Nature/ithaca_tree_cypress_01.png", 0.9f, -17),
        };
        for (int i = 0; i < plantas.Length; i++)
        {
            var (x, arte, escala, ordem) = plantas[i];
            GameObject go = Reuso($"Tree_{x:0}", x, TopoDoChao - 0.1f, Ithaca + arte, ordem);
            if (go == null) { continue; }
            // Uma em cada duas espelhada: são dois sprites de árvore para oito posições.
            go.transform.localScale = new Vector3(i % 2 == 0 ? escala : -escala, escala, 1f);
            go.GetComponent<SpriteRenderer>().color = Distancia(0.40f);
        }

        foreach (var (x, arte, escala) in new[]
        {
            (-8f, "Nature/ithaca_rock_medium_01.png", 0.8f),
            (3.5f, "Nature/ithaca_bush_01.png", 0.8f),
            (13f, "Nature/ithaca_rock_small_01.png", 0.9f),
            (24f, "Nature/ithaca_bush_01.png", 0.75f),
            (30f, "Nature/ithaca_rock_medium_01.png", 0.7f),
        })
        {
            Encolher(Reuso($"Rock_{x:0}", x, TopoDoChao, Ithaca + arte, -2), escala);
        }

        // O mato crescido onde antes havia cuidado. Sempre ao pé de algo construído — mato no
        // meio do nada é paisagem, mato encostado numa parede é abandono.
        foreach (float x in new[] { 6.8f, 18.6f, 22.5f })
        {
            Encolher(Novo($"Overgrowth_{x:0}", x, TopoDoChao, "Vegetation/itaca_ret_overgrowth.png", -1), 0.7f);
        }

        // Duas peças em x≈44 MASCARANDO o eixo de espelho da faixa de vila.
        //
        // A faixa foi espelhada para ladrilhar, e espelho tem preço: no eixo, as duas metades
        // formam uma figura perfeitamente simétrica que o olho lê na hora como defeito — na
        // primeira montagem aparecia como um prédio branco em forma de borboleta bem ao lado da
        // porta de casa. O eixo cai em x≈44 (a faixa começa em −3 e o período é 31,31), e a
        // câmera chega a enxergar até 53, então ele ESTÁ em quadro no fim da fase.
        //
        // Cobrir é mais barato que regerar a faixa, e as peças são arte da fase 01 que já está
        // carregada.
        Encolher(Reuso("Mask_Cypress", 43.5f, TopoDoChao - 0.1f,
            Ithaca + "Nature/ithaca_tree_cypress_01.png", -17), 1.05f);
        Encolher(Reuso("Mask_House", 45.5f, TopoDoChao - 0.1f,
            Ithaca + "Architecture/ithaca_house_small_01.png", -18), 0.9f);
    }

    private static void MontarPrimeiroPlano()
    {
        foreach (var (x, i) in new[] { (-12f, 0), (0f, 1), (14f, 2), (28f, 3), (36f, 4) })
        {
            GameObject go = Novo($"FG_Grass_{i}", x, TopoDoChao - 0.5f,
                "Vegetation/itaca_ret_overgrowth.png", 12);
            Encolher(go, 0.58f + (i % 3) * 0.08f);
        }
    }

    // ---------------------------------------------------------------- anoitecer

    /// <summary>
    /// O anoitecer sobre o bairro do palácio.
    ///
    /// Terceira fase seguida a usar a mesma ferramenta e a terceira cor: azul frio em Gado do
    /// Sol (punição), âmbar em Calipso (partida), violeta-azulado aqui — porque o que esta fase
    /// prepara não é consequência nem despedida, é o CONFRONTO da fase 15.
    /// </summary>
    private static void MontarAnoitecer()
    {
        Sprite gradiente = Arte("VFX/itaca_ret_dusk_gradient.png");
        if (gradiente == null) { return; }

        // A rampa tem de chegar ao teto DENTRO do percurso, senão o anoitecer não acontece
        // onde o jogador está. Com x0=18 e fim em 56, o alpha na porta de casa media 0,15 —
        // invisível. Apertando a faixa para 24..46, o teto cai em x≈39, que é a porta.
        float x0 = InicioDoPalacio + 4f;
        float ate = CameraMax + 14f;

        var veu = new GameObject("DuskVeil");
        veu.transform.SetParent(cenario, false);
        // y = −16: o pivô está na BASE e o sprite desenha do pivô PARA CIMA.
        veu.transform.position = new Vector3((x0 + ate) * 0.5f, -16f, 0f);

        var sr = veu.AddComponent<SpriteRenderer>();
        sr.sprite = gradiente;
        // Escala no TRANSFORM: a malha "tight" recorta a metade transparente do degradê.
        veu.transform.localScale = new Vector3(
            (ate - x0) / gradiente.bounds.size.x, 40f / gradiente.bounds.size.y, 1f);
        sr.sortingOrder = 20;
        novos++;
    }

    // ---------------------------------------------------------------- utilidades

    /// <summary>Prop vindo da FASE 01. Conta separado, para o relatório saber o que é reuso.</summary>
    private static GameObject Reuso(string nome, float x, float y, string caminho, int ordem)
    {
        GameObject go = Prop(nome, x, y, caminho.StartsWith("Assets/") ? caminho : Ithaca + caminho, ordem);
        if (go != null) { reusados++; }
        return go;
    }

    /// <summary>Prop novo desta fase.</summary>
    private static GameObject Novo(string nome, float x, float y, string caminho, int ordem)
    {
        GameObject go = Prop(nome, x, y, caminho, ordem);
        if (go != null) { novos++; }
        return go;
    }

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

    private static GameObject Camada(string nome, string caminho, float fator, int ordem,
        float baseY, float alturas)
    {
        Sprite arte = Arte(caminho);
        if (arte == null) { return null; }

        const float larguraDaTela = 18f;
        float camInicio = Camera.main != null ? Camera.main.transform.position.x : CameraInicial;
        float curso = CameraMax - CameraMin;
        float x0 = camInicio * fator + Meio() * (1f - fator);

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
        if (s == null) { Debug.LogWarning("[ItacaReturn] sprite não encontrado: " + caminho); }
        return s;
    }

    private static Color Cor(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color c);
        return c;
    }
}
