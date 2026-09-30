using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Odisseia.Systems;

/// <summary>
/// Veste a fase 12 com a ilha do gado sagrado de Hélio:
///
/// Unity.exe -batchmode -quit -nographics -projectPath . -executeMethod GadoDoSolSceneDresser.Run
///
/// Mesmo desenho dos vestidores das fases 02 a 11 — não é sistema novo.
///
/// ## A tese desta fase é a MENTIRA DA BELEZA, e ela tem uma direção
///
/// Cila e Caribdis avisa o tempo todo: basalto preto, tempestade, redemoinho à vista. Aqui é o
/// contrário — a fase precisa parecer segura para que a decisão de comer o gado pese. Então
/// não há um único sinal de perigo no primeiro terço: céu azul, mar calmo, pasto, oliveiras.
///
/// E há uma DIREÇÃO no tempo, que é o que o briefing pede como "transformação gradual":
///
///   x −18 a −2  praia e costa      céu aberto, mar calmo, sem nada construído
///   x −2 a 18   campos e pastagens olival, muros secos, o rebanho
///   x 18 a 40   templo e sagrado   calcário dourado, altar, menires — e o céu fechando
///
/// O escurecimento do último terço é feito por VÉUS de cor chapada com alpha crescente, a mesma
/// ferramenta que Ciclopes usa para a boca da caverna. Não é shader, não é iluminação dinâmica,
/// não é segunda câmera: são quatro SpriteRenderers, e é o que cabe no orçamento de WebGL.
///
/// ## A escala do rebanho é medida, não estimada
///
/// O boi gerado mede 3,22 x 2,19 unidades, e o Odisseu tem 1,4 de altura. Um boi de verdade
/// tem cerca de 1,5 m na cernelha contra 1,75 m de um homem, ou seja **o boi deve ser MAIS
/// BAIXO que o personagem**, não uma vez e meia mais alto. É a armadilha já registrada no
/// projeto — escala de prop gerado é sempre grande demais — e aqui ela custaria a leitura: um
/// boi maior que o herói deixa de ser gado e vira monstro.
/// </summary>
public static class GadoDoSolSceneDresser
{
    private const string ScenePath = "Assets/Scenes/Levels/Level_12_GadoDoSol.unity";
    private const string Raiz = "Assets/Art/Environments/GadoDoSol/";
    private const string Cytera = "Assets/Art/Environments/Cytera/";
    private const string RaizDoCenario = "GadoDoSolScenery";

    private const float TopoDoChao = -2f;

    /// <summary>Reserva; o valor real vem da Main Camera da cena.</summary>
    private const float CameraInicial = 0f;

    private static float CameraMin = -18f;
    private static float CameraMax = 40f;

    /// <summary>
    /// Onde a área sagrada começa. Sai do <c>Floor_3</c> na montagem, não de um número escrito
    /// aqui — é o mesmo princípio do resto do vestidor.
    /// </summary>
    private static float InicioDoSagrado = 18f;

    /// <summary>
    /// Quanto a arte do tile de topo sobe para a superfície pisável encostar no colisor.
    /// Medido por <c>Tools/build-ground-tiles.js</c>: 16px de um tile de 32.
    /// </summary>
    private const float SuperficieDoTile = 16f / 42.857143f;

    private static Transform cenario;

    [MenuItem("Odisseia/Vestir Gado do Sol")]
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
            Debug.LogError("[Gado] nenhum Floor_* encontrado — cenário não montado");
            return false;
        }

        CameraMin = chaos.Min(g => Caixa(g).min.x);
        CameraMax = chaos.Max(g => Caixa(g).max.x);

        GameObject terceiro = chaos.FirstOrDefault(g => g.name == "Floor_3");
        if (terceiro != null) { InicioDoSagrado = Caixa(terceiro).min.x; }

        DesligarPlaceholders();
        MontarCeuEMar();
        MontarColinasEOlival();
        VestirChao(chaos);
        MontarCampos();
        MontarRebanho();
        MontarTemplo();
        MontarAreaSagrada();
        MontarNavio();
        MontarPrimeiroPlano();
        MontarTempestadeQueChega();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[Gado] cenário montado: {cenario.childCount} objetos sob {RaizDoCenario}");
        return true;
    }

    private static List<GameObject> Plataformas() =>
        Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
            .Where(g => g != null && g.name.StartsWith("Floor_") && g.GetComponent<Collider2D>() != null)
            .OrderBy(g => g.transform.position.x)
            .ToList();

    private static void DesligarPlaceholders()
    {
        // "SacredCattle" entra aqui porque é um GATILHO com retângulo de marcação: a arte do boi
        // vai por cima, na posição dele, e o objeto de gameplay não é tocado. O mesmo para o
        // mastro e a vela do navio da saída.
        string[] prefixos =
        {
            "Floor_", "Sky_Background", "LevelGoal", "Mast", "Sail", "SacredCattle",
            "RationPickup",
        };
        foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (go == null || !prefixos.Any(pre => go.name.StartsWith(pre))) { continue; }
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null) { sr.enabled = false; }
        }
    }

    // ---------------------------------------------------------------- céu, mar, colinas

    private static void MontarCeuEMar()
    {
        // #6dafd1 é a cor MEDIDA da primeira linha de `gado_bg_hills` (387 dos 672 px). Mesma regra das Sereias
        // e de Cila: cobertura fora da cor medida deixa uma emenda horizontal na tela.
        Bloco("Sky_Fill", Meio(), 16f, 160f, 36f, Cor("#6dafd1"), -60);

        // A terra da ilha abaixo da linha do chão. É terra e não mar, e por isso usa o passo
        // escuro da rampa "Terra": numa fase que começa na praia, mar sob o pasto leria como ilha
        // flutuante.
        //
        // ORDEM −12, e não −58. Em −58 ela ficava atrás de TODAS as camadas de fundo, e o
        // resultado só aparecia quando a câmera descia: o olival, que vive em −40 e está
        // rebaixado para esconder o corpo dele atrás do barranco, reaparecia DEBAIXO do chão,
        // com árvores crescendo dentro da terra. Este bloco não é fundo — é a massa de terra, e
        // massa de terra tapa o que está atrás dela. Em −12 ela fica logo atrás do corpo do
        // tile (−11) e à frente de tudo que é paisagem.
        Bloco("Ground_Fill", Meio(), -18f, 160f, 30f, Cor("#4d3137"), -12);

        // A ÚNICA camada que carrega céu.
        Camada("BG_Hills", "Background/gado_bg_hills.png", 0.90f, -50, -3.2f, 1f);

        // O mar só existe no PRIMEIRO TERÇO, atrás da praia. Estendê-lo pela fase inteira poria
        // oceano atrás do templo, que fica no interior da ilha.
        // Ordem −39, e não −44.
        //
        // Em −44 o mar ficava ATRÁS do olival (−40). Como o olival foi rebaixado para esconder o
        // tronco atrás do barranco, ele passou a cobrir justamente a faixa de altura em que o
        // mar aparecia, e a praia — que é o primeiro passo da progressão que o briefing pede —
        // simplesmente não existia na tela. Na costa o mar está NA FRENTE do interior da ilha,
        // então −39 é a ordem certa. A ALTURA continua em +1,1: subida para +2,6 o mar virava um
        // paredão de água do chão às colinas, e o jogador lia como se andasse numa faixa no meio
        // do oceano. O que faltava era ordem, não tamanho — 1,1 unidade de água acima da linha
        // do chão é o que uma costa mostra de fato vista de lado.
        // O olival é contínuo por toda a fase, então o mar cobri-lo
        // só acontece nas 26 unidades da praia, que é onde deve acontecer.
        GameObject mar = Faixa("Sea_Calm", CameraMin + 8f, TopoDoChao + 1.1f, 26f,
            "Ocean/gado_sea_calm.png", -39);
        if (mar != null)
        {
            var sr = mar.GetComponent<SpriteRenderer>();
            sr.color = Distancia(0.60f);
            Deslizar(mar, -0.12f);          // mar calmo: quase parado, mas não congelado
        }
    }

    /// <summary>
    /// A faixa de olival, em parallax de meio de campo. Começa depois da costa: um olival na
    /// beira da praia contradiz a progressão praia → campos que o briefing pede.
    /// </summary>
    private static void MontarColinasEOlival()
    {
        GameObject olival = Camada("Grove_Band", "Midground/gado_grove_band.png", 0.55f, -40, -5.4f, 1f);
        if (olival == null) { return; }

        // BAIXADA, e não reduzida em escala.
        //
        // Em tamanho nativo e assente em y=−3 a faixa cobria de −3 a +3 — o QUADRO INTEIRO. O
        // resultado era um paredão de oliveiras do chão ao céu: as colinas do fundo sumiam atrás
        // dele e a ilha, que o briefing pede aberta e ensolarada, lia como floresta fechada.
        //
        // A primeira correção foi encolher o transform para 0,52. Resolveu a altura e QUEBROU a
        // largura: encolher o transform encolhe as duas dimensões, e a faixa passou a cobrir 32
        // un em vez de 62 — surgiu uma borda vertical dura no meio da tela onde o olival
        // simplesmente acabava.
        //
        // O que a faixa precisa não é ser menor: é estar mais BAIXA, com o corpo dela escondido
        // atrás do barranco e só as copas aparecendo acima da linha do chão. Em y=−5,4 a faixa
        // vai de −5,4 a 0,57, e o que se vê acima do chão em −2 são 2,6 unidades de copa — que é
        // exatamente o que um olival a alguma distância mostra por cima de um desnível.
        olival.GetComponent<SpriteRenderer>().color = Distancia(0.55f);
    }

    /// <summary>
    /// Perspectiva aérea: cada camada é puxada para a cor do céu na proporção do parallax. A
    /// fase tem seis famílias de matiz e poderia dispensar isto — mas o pasto de fundo e o
    /// pasto jogável são o MESMO verde, e sem o recuo em valor os dois planos colam.
    /// </summary>
    private static Color Distancia(float fator) =>
        Color.Lerp(Color.white, Cor("#6dafd1"), Mathf.Clamp01((fator - 0.30f) * 0.85f));

    // ---------------------------------------------------------------- chão

    /// <summary>
    /// Grama nos dois primeiros trechos, laje de calcário no terceiro.
    ///
    /// A troca de material é o AVISO SILENCIOSO de que se entrou em terreno sagrado — a mesma
    /// ferramenta que Sereias usa na ilha das sereias, e a única que funciona sem UI: o jogador
    /// percebe que pisa em pedra lavrada antes de ver o templo.
    /// </summary>
    private static void VestirChao(List<GameObject> chaos)
    {
        foreach (GameObject chao in chaos)
        {
            Bounds b = Caixa(chao);
            bool sagrado = b.min.x >= InicioDoSagrado - 0.01f;

            float folgaEsq = b.min.x <= CameraMin + 0.01f ? 9f : 0f;
            float folgaDir = b.max.x >= CameraMax - 0.01f ? 8f : 0f;
            float largura = b.size.x + folgaEsq + folgaDir;
            float centro = b.center.x + (folgaDir - folgaEsq) * 0.5f;
            string prefixo = sagrado ? "Gameplay/gado_stone" : "Gameplay/gado_grass";

            GameObject topo = Faixa($"GroundTop_{chao.name}", centro, b.max.y + SuperficieDoTile,
                largura, prefixo + "_ground_top.png", -10);
            if (topo == null) { continue; }

            // Altura do SPRITE, nunca constante: o pipeline recorta esses tiles.
            float alturaDoTile = topo.GetComponent<SpriteRenderer>().sprite.bounds.size.y;

            GameObject corpo = Faixa($"GroundBody_{chao.name}", centro,
                b.max.y + SuperficieDoTile - alturaDoTile, largura,
                prefixo + "_ground_body.png", -11, 3f);

            // Corpo escurecido: a face sob a plataforma não recebe luz, e sem isso as sete
            // unidades de tile ladrilhado leem como parede de tijolo em vez de barranco.
            if (corpo != null)
            {
                corpo.GetComponent<SpriteRenderer>().color = new Color(0.46f, 0.43f, 0.38f, 1f);
            }
        }
    }

    // ---------------------------------------------------------------- campos

    /// <summary>
    /// Oliveiras, ciprestes e muros secos ao longo dos campos.
    ///
    /// **Nenhuma árvore fica na faixa de altura do jogador na frente dele.** Oliveira e árvore
    /// grande vão para ordem NEGATIVA (atrás do plano de jogo) e só os arbustos baixos ficam à
    /// frente. A seção de leitura do briefing é explícita: vegetação não pode esconder inimigo
    /// nem plataforma, e uma copa de 5 unidades na ordem do jogador esconderia as duas coisas.
    /// </summary>
    private static void MontarCampos()
    {
        // Oliveiras e ciprestes atrás do plano de jogo, alternando para o olho não achar ritmo.
        var arvores = new (float x, string arte, float escala, int ordem)[]
        {
            (-15.5f, "Fields/gado_olive_tree.png", 0.62f, -20),
            (-9f, "Fields/gado_cypress.png", 0.70f, -20),
            (-4.5f, "Fields/gado_olive_tree.png", 0.55f, -21),
            (1.5f, "Fields/gado_shade_tree_big.png", 0.72f, -20),
            (7f, "Fields/gado_cypress.png", 0.80f, -21),
            (10.5f, "Fields/gado_olive_tree.png", 0.66f, -20),
            (16f, "Fields/gado_cypress.png", 0.62f, -21),
            (25f, "Fields/gado_cypress.png", 0.85f, -20),
            (31f, "Fields/gado_cypress.png", 0.85f, -20),
        };
        foreach (var (x, arte, escala, ordem) in arvores)
        {
            GameObject go = Prop($"Tree_{x:0}", x, TopoDoChao - 0.1f, arte, ordem);
            Encolher(go, escala);
            // Recuo leve em valor: as árvores estão ATRÁS do plano de jogo e precisam parecer.
            if (go != null) { go.GetComponent<SpriteRenderer>().color = Distancia(0.40f); }
        }

        // Muros secos dividindo os campos — é o que faz o lugar ler como PASTO e não como mato.
        foreach (float x in new[] { -12f, 2f, 15f })
        {
            GameObject go = Prop($"Wall_{x:0}", x, TopoDoChao - 0.05f,
                "Fields/gado_drystone_wall.png", -3);
            Encolher(go, 0.75f);
        }
    }

    /// <summary>
    /// O rebanho, nos gatilhos que o level design já tinha, mais alguns animais de ambiente.
    ///
    /// A escala vem da MEDIDA, não do gosto: o boi gerado tem 2,19 un de altura e o Odisseu
    /// 1,4. O fator 0,55 leva o boi a 1,20 un — mais baixo que o herói, que é a proporção real
    /// entre um boi e um homem. Bezerro em 0,42.
    /// </summary>
    private static void MontarRebanho()
    {
        const float EscalaBoi = 0.55f;
        const float EscalaBezerro = 0.42f;

        // Os dois gatilhos de gameplay: aqui a arte tem de cair EXATAMENTE sobre o marcador,
        // senão o jogador interage com um boi que não está ali.
        GameObject[] gatilhos = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
            .Where(g => g != null && g.name.StartsWith("SacredCattle") && g.GetComponent<Collider2D>() != null)
            .OrderBy(g => g.transform.position.x)
            .ToArray();

        for (int i = 0; i < gatilhos.Length; i++)
        {
            Bounds b = Caixa(gatilhos[i]);
            // Alterna parado e pastando: dois bois idênticos lado a lado leem como cópia.
            string arte = i % 2 == 0 ? "SacredCattle/gado_cattle_idle.png"
                                     : "SacredCattle/gado_cattle_grazing.png";
            GameObject go = Prop($"Cattle_{i}", b.center.x, TopoDoChao, arte, 1);
            Encolher(go, EscalaBoi);
        }

        // Rebanho de ambiente, atrás do plano de jogo. É o que faz "rebanho sagrado" ler como
        // rebanho e não como dois bois soltos — mas eles ficam em ordem negativa para nunca
        // competirem com os dois que o jogador pode de fato comer.
        var ambiente = new (float x, string arte, float escala)[]
        {
            (-3f, "SacredCattle/gado_cattle_grazing.png", EscalaBoi * 0.9f),
            (6.5f, "SacredCattle/gado_calf.png", EscalaBezerro),
            (8.5f, "SacredCattle/gado_cattle_idle.png", EscalaBoi * 0.85f),
            (15.5f, "SacredCattle/gado_cattle_grazing.png", EscalaBoi * 0.8f),
            (16.8f, "SacredCattle/gado_calf.png", EscalaBezerro * 0.9f),
        };
        foreach (var (x, arte, escala) in ambiente)
        {
            GameObject go = Prop($"Herd_{x:0}", x, TopoDoChao - 0.05f, arte, -2);
            Encolher(go, escala);
            if (go != null) { go.GetComponent<SpriteRenderer>().color = Distancia(0.38f); }
        }
    }

    // ---------------------------------------------------------------- templo

    /// <summary>
    /// O templo de Hélio, dominando o último terço.
    ///
    /// Fica atrás do plano de jogo (ordem −18) e assente na linha do chão. Não é cenário de
    /// fundo com parallax: um templo que desliza em relação ao chão deixaria de parecer
    /// construído NA ilha, e o briefing pede que ele seja o lugar, não uma pintura.
    /// </summary>
    private static void MontarTemplo()
    {
        GameObject templo = Prop("Temple", InicioDoSagrado + 8f, TopoDoChao - 0.1f,
            "Temple/gado_temple.png", -18);
        if (templo == null) { return; }

        // 0,80 leva o templo a 7,2 un de altura — cinco vezes o Odisseu. Um templo grego real
        // tem 10 a 13 m contra 1,75 m de um homem, então a proporção está certa; em escala
        // nativa (8,96 un) ele encheria a tela e esconderia a plataforma.
        Encolher(templo, 0.80f);

        // Colunas caídas no caminho até ele: dizem que o lugar é antigo sem custar geração nova.
        foreach (var (x, escala) in new[] { (InicioDoSagrado + 1.5f, 0.55f), (InicioDoSagrado + 16f, 0.48f) })
        {
            Encolher(Prop($"Column_{x:0}", x, TopoDoChao - 0.05f,
                "Temple/gado_ruined_column.png", -4), escala);
        }
    }

    /// <summary>
    /// A área sagrada: altar e menires, onde o evento narrativo acontece.
    ///
    /// Fica ENTRE o templo e a saída, e não dentro do templo, porque o briefing pede que ela
    /// funcione como ponto narrativo com espaço para Odisseu e a tripulação — o que exige chão
    /// livre, não um interior.
    /// </summary>
    private static void MontarAreaSagrada()
    {
        Encolher(Prop("Altar", InicioDoSagrado + 12f, TopoDoChao, "SacredArea/gado_altar.png", -1), 0.62f);
        Encolher(Prop("Stones", InicioDoSagrado + 4f, TopoDoChao - 0.05f,
            "SacredArea/gado_standing_stones.png", -3), 0.7f);
    }

    /// <summary>O navio da saída, no objetivo. Mesmo casco e mastro de Cytera.</summary>
    private static void MontarNavio()
    {
        GameObject alvo = GameObject.Find("LevelGoal");
        if (alvo == null) { return; }

        Bounds b = Caixa(alvo);
        Encolher(Prop("Exit_Deck", b.center.x, TopoDoChao, Cytera + "Ship/cytera_deck_planks.png", -5), 1.0f);
        Encolher(Prop("Exit_Mast", b.center.x, TopoDoChao, Cytera + "Ship/cytera_mast_sail.png", -4), 1.15f);
    }

    /// <summary>
    /// Primeiro plano: só arbustos e flores baixos, e só ABAIXO da linha do chão.
    ///
    /// Sem <see cref="ParallaxLayer"/> — o fator dele é limitado a [0,1] e primeiro plano
    /// exigiria mais que 1. Ficam em ordem acima do jogador mas nunca o escondem, porque a
    /// altura deles é 1,05 un contra 1,4 do personagem e a base está enterrada.
    /// </summary>
    private static void MontarPrimeiroPlano()
    {
        float[] posicoes = { -16f, -10f, -1f, 5f, 12f, 20f, 27f, 34f };
        for (int i = 0; i < posicoes.Length; i++)
        {
            GameObject go = Prop($"FG_Shrubs_{i}", posicoes[i], TopoDoChao - 0.55f,
                "Pasture/gado_shrubs_flowers.png", 12);
            Encolher(go, 0.75f + (i % 3) * 0.12f);
        }
    }

    // ---------------------------------------------------------------- consequência

    /// <summary>
    /// A tempestade que chega — a transformação gradual que o briefing pede.
    ///
    /// ## Por que véus de cor chapada, e não iluminação
    ///
    /// A mudança tem de acontecer AO LONGO DO EIXO X: claro no começo, dourado no templo,
    /// fechado na saída. Uma luz global não sabe onde o jogador está sem código novo; um shader
    /// custaria material próprio; e o URP 2D de luzes seria a única coisa pesada da fase.
    ///
    /// Quatro retângulos com alpha crescente resolvem: como cada um cobre um trecho de x e a
    /// câmera anda, o resultado é exatamente um escurecimento progressivo — e o jogador nunca vê
    /// a borda entre eles porque elas se sobrepõem por 4 unidades.
    ///
    /// É a mesma ferramenta que Ciclopes usa para a boca da caverna, e está registrada no
    /// PlaceholderProbe como preenchimento legítimo.
    ///
    /// A cor é #46506b — o azul-acinzentado frio da tempestade — e não cinza puro: cinza sobre
    /// verde dá verde sujo, e o que se quer é a luz MUDANDO de temperatura, não sujeira.
    /// </summary>
    private static void MontarTempestadeQueChega()
    {
        // O véu começa DEPOIS do altar: antes disso a fase precisa continuar mentindo.
        float x0 = InicioDoSagrado + 10f;
        // Vai ALÉM do fim da fase, e o degradê tem um platô nos últimos 30% para isso não
        // clarear de volta. Terminando exatamente em CameraMax, a borda direita do véu caía
        // dentro do quadro no embarque e lia como um corte vertical entre escuro e claro.
        float ate = CameraMax + 14f;

        // UM sprite com o alpha variando por coluna, esticado sobre o trecho.
        //
        // A primeira montagem usou quatro retângulos de alpha uniforme se sobrepondo. Cada
        // fronteira entre dois virou um degrau visível e o fim da fase ficou com quatro painéis
        // translúcidos de borda reta atravessando a tela — leitura de interface, não de clima.
        // Alpha uniforme não vira degradê por empilhamento; o degradê tem de estar NA IMAGEM.
        Sprite gradiente = Arte("VFX/gado_storm_gradient.png");
        if (gradiente != null)
        {
            var veu = new GameObject("StormVeil");
            veu.transform.SetParent(cenario, false);
            // y = −16 e não 4. O pivô do sprite está na BASE, então ele desenha do pivô PARA
            // CIMA: em y=4 o véu ocupava de 4 a 44, inteiramente acima do que a câmera mostra
            // (−5 a 5). O objeto existia, não havia erro no console, e não desenhava nada onde
            // alguém pudesse ver. É a mesma armadilha que pôs as ondas de Cila flutuando no ar.
            veu.transform.position = new Vector3((x0 + ate) * 0.5f, -16f, 0f);

            var sr = veu.AddComponent<SpriteRenderer>();
            sr.sprite = gradiente;
            // Escala no TRANSFORM, e não `drawMode`/`size`.
            //
            // Com `SpriteDrawMode.Sliced` o véu não apareceu na tela. Sliced e Tiled trabalham
            // sobre as bordas do sprite e sobre a malha dele, e a malha "tight" que o importador
            // gera por padrão RECORTA regiões totalmente transparentes — que neste sprite são
            // justamente a metade esquerda, onde o alpha começa em zero. O objeto existia, sem
            // erro nenhum no console, e desenhava nada.
            //
            // Esticar pelo transform não depende de malha nem de borda: multiplica o quad
            // inteiro, alpha zero incluído.
            veu.transform.localScale = new Vector3(
                (ate - x0) / gradiente.bounds.size.x, 40f / gradiente.bounds.size.y, 1f);
            // Ordem 20: acima de tudo, inclusive do jogador. É o AR entre a câmera e a cena que
            // muda, então cobrir o personagem é o comportamento certo — e com alpha máximo de
            // 0,60 ele continua legível.
            sr.sortingOrder = 20;
        }

        // Nuvens de tempestade entrando por cima, no fim da fase. Vêm de Cytera — é a mesma
        // tempestade que persegue Odisseu desde a fase 04, e desenhar outra seria inventar um
        // segundo clima para a mesma viagem.
        foreach (var (x, escala, a) in new[] { (x0 + 4f, 1.1f, 0.55f), (x0 + 14f, 1.4f, 0.75f), (ate - 4f, 1.6f, 0.9f) })
        {
            GameObject nuvem = Prop($"StormCloud_{x:0}", x, 1.5f,
                Cytera + "Background/cytera_storm_cloud.png", -46);
            if (nuvem == null) { continue; }
            nuvem.transform.localScale = new Vector3(escala, escala, 1f);
            var sr = nuvem.GetComponent<SpriteRenderer>();
            sr.color = new Color(1f, 1f, 1f, a);
            nuvem.AddComponent<ParallaxLayer>();
            Fator(nuvem, 0.75f);
        }
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
    /// Camada de parallax dimensionada para cobrir a fase inteira. Mesma conta do vestidor de
    /// Cila e Caribdis, e pelo mesmo motivo: o início vem da câmera REAL da cena, não de uma
    /// constante — foi assumir que a câmera começa onde o jogador nasce que deixou uma faixa
    /// chapada na borda do quadro lá.
    /// </summary>
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
        if (s == null) { Debug.LogWarning("[Gado] sprite não encontrado: " + caminho); }
        return s;
    }

    private static Color Cor(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color c);
        return c;
    }
}
