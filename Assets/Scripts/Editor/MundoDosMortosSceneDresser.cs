using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Odisseia.Systems;

/// <summary>
/// Veste a fase 09 com o cenário do Mundo dos Mortos:
///
/// Unity.exe -batchmode -quit -nographics -projectPath . -executeMethod MundoDosMortosSceneDresser.Run
///
/// Mesmo desenho dos vestidores das fases 02 a 08 — não é sistema novo.
///
/// **O risco desta fase é ficar escura demais para ser jogada.** O briefing pede sombrio mas
/// jogável, e a solução não é clarear tudo: é dar à paleta quatro famílias de matiz distintas
/// (pedra fria azulada, água verde-morta, mármore pálido, fogo quente) e distribuir fontes de
/// luz pelo percurso. Lestrigões provou o oposto — oito rampas de cinza viraram uma massa
/// única e mataram a leitura.
///
/// Os dois <c>DialogueTrigger</c> da cena (Shades e Mother) marcam os momentos narrativos, e é
/// neles que o altar e o portão são ancorados: a arte segue o design, não uma coordenada.
/// </summary>
public static class MundoDosMortosSceneDresser
{
    private const string ScenePath = "Assets/Scenes/Levels/Level_09_MundoDosMortos.unity";
    private const string Raiz = "Assets/Art/Environments/MundoDosMortos/";
    private const string RaizDoCenario = "MortosScenery";

    private const float TopoDoChao = -2f;

    private const float CameraInicial = 0f;
    /// <summary>Meio do passeio: o jogador vai de x=-12 a x=34.</summary>
    private const float CameraMeio = 11f;
    private const float CameraCurso = 46f;

    private static Transform cenario;

    [MenuItem("Odisseia/Vestir Mundo dos Mortos")]
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
            Debug.LogError("[Mortos] nenhum Floor_* encontrado — cenário não montado");
            return false;
        }

        DesligarPlaceholders();
        MontarCaverna(chaos);
        VestirChao(chaos);
        MontarRio(chaos);
        PovoarEntrada(chaos);
        PovoarRuinas(chaos);
        VestirDecorDeRuina();
        MontarSaida();
        MontarAltares();
        MontarAtmosfera();
        MontarPrimeiroPlano();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[Mortos] cenário montado: {cenario.childCount} objetos sob {RaizDoCenario}");
        return true;
    }

    private static void DesligarPlaceholders()
    {
        // "BG_" desliga as TRÊS camadas pintadas que a cena já trazia
        // (BG_Fase03_Underworld_Far/Mid/Near). Não é gosto: medidas, elas têm 128.014, 73.862 e
        // 71.130 cores únicas, contra 51 do sprite do Odisseu. São pinturas suavizadas de outro
        // pipeline, exatamente a classe que o briefing proíbe — e ainda são vermelho-lava,
        // contra a direção azulada/arroxeada pedida. Ficam na cena, desligadas, para poderem
        // voltar se alguém quiser.
        string[] prefixos = { "Floor_", "Sky_Background", "LevelGoal", "DialogueTrigger_",
            "BG_", "Mist_Patch", "Ruin_Decor" };
        foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (go == null || !prefixos.Any(pre => go.name.StartsWith(pre))) { continue; }
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null) { sr.enabled = false; }
        }
    }

    // ---------------------------------------------------------------- caverna

    /// <summary>
    /// Perspectiva aérea, a regra que Lestrigões cobrou e que agora nasce com a fase: quanto
    /// mais longe, mais a camada é puxada para a cor do fundo. Aqui o alvo é a pedra funda, e
    /// não um céu — não existe céu no subterrâneo.
    /// </summary>
    private static Color Distancia(float fator)
    {
        return Color.Lerp(Color.white, Cor("#4d5459"), Mathf.Clamp01((fator - 0.30f) * 1.2f));
    }

    private static void MontarCaverna(List<GameObject> chaos)
    {
        // Preto absoluto é proibido pelo briefing: a cobertura é a pedra mais funda, não #000.
        Bloco("Rock_Fill", 11f, 10f, 120f, 30f, Cor("#151615"), -60);
        Bloco("Depth_Fill", 11f, -10f, 120f, 12f, Cor("#151615"), -30);

        // Duas alturas nativas, não uma. Com uma só, o topo das duas bandas caía em y≈0 e o
        // terço superior do quadro virava um vazio preto separado da parede por uma emenda
        // horizontal dura — dava para ver na captura onde a arte acabava. A arcada repetida
        // verticalmente lê como caverna de pé-direito alto, que é o que a fase quer.
        Camada("BG_Cavern", "Background/mortos_cavern_far.png", 0.92f, -50, -2f, 2f);
        Camada("BG_Wall", "Midground/mortos_cavern_wall.png", 0.66f, -44, -1.5f, 2f);

        // Teto de estalactites correndo a fase inteira: é ele que diz "subterrâneo" antes de
        // qualquer objeto, e custa uma camada.
        float x0 = Caixa(chaos[0]).min.x, x1 = chaos.Max(g => Caixa(g).max.x);
        GameObject teto = Faixa("Cave_Ceiling", (x0 + x1) * 0.5f, 6.4f, x1 - x0 + 6f,
            "Midground/mortos_stalactites.png", -20);
        if (teto != null) { teto.GetComponent<SpriteRenderer>().color = Distancia(0.42f); }
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
        sr.color = Distancia(fator);
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
    /// Chão de caverna na entrada e no caminho, laje antiga nas ruínas. A troca acontece no
    /// último trecho, que é onde o <c>DialogueTrigger_Mother</c> e o objetivo estão.
    /// </summary>
    private static void VestirChao(List<GameObject> chaos)
    {
        for (int i = 0; i < chaos.Count; i++)
        {
            Bounds b = Caixa(chaos[i]);
            bool ruinas = i == chaos.Count - 1;
            // Tiles de BORDA, recortados do topo pelo build-mortos-edge-tiles.js.
            //
            // Encurtar a faixa pelo size do renderer nao serve: com pivo na base o Tiled
            // desenha de baixo para cima e o corte guarda a BASE do tile — que no chao de
            // caverna e a parte desenhada para se dissolver no escuro (L 0,17). A face sob o
            // jogador saia preta no trecho de caverna e normal no de ruina (base L 0,26), e
            // essa diferenca era o corte vertical duro em x=20. Recortados do topo, os dois
            // materiais medem L 0,52 e a juncao deixa de ser uma linha.
            GameObject chao = Faixa($"Ground_{i}", b.center.x, b.max.y, b.size.x,
ruinas ? "Gameplay/mortos_tiles_ruinfloor_edge.png"
                       : "Gameplay/mortos_tiles_cavefloor_edge.png", -10);
            // A laje de ruína nasce mais clara que o chão de caverna (L 0,436 contra 0,358), e
            // é ela que fica no trecho final — o mais povoado. Sem isto o piso ofusca as
            // colunas e as estátuas que deveriam ser o assunto ali.
            // Os dois tiles de borda medem a mesma luminancia, entao levam o mesmo fator: o
            // que separa os trechos passa a ser o desenho da pedra, nao o brilho.
            Apagar(chao, 0.72f);

            // Junção entre trechos. Os colisores são contíguos (…−16..0, 0..20, 20..38), então
            // a troca de textura cai numa linha vertical perfeita e lê como corte de imagem, não
            // como chão. Um monte de pedras em cima da emenda desmancha a linha — é o mesmo
            // recurso usado nas junções das fases anteriores, e não custa asset novo.
            if (i > 0)
            {
                GameObject junta = Prop($"Joint_{i}", b.min.x, TopoDoChao,
                    "Props/mortos_shore_rocks.png", -7);
                Encolher(junta, 0.5f);
                Apagar(junta, 0.8f, 0.3f);
            }
            Bloco($"Bank_{i}", b.center.x, b.max.y - 9f, b.size.x, 12f, Cor("#151615"), -12);
        }
    }

    /// <summary>
    /// O rio corre ABAIXO da linha do chão, no vão entre trechos e ao longo do fundo. Não é
    /// plataforma nem obstáculo — é a paisagem que dá nome à fase, e por isso fica atrás do
    /// plano de jogo e nunca cruza onde o jogador pisa.
    /// </summary>
    private static void MontarRio(List<GameObject> chaos)
    {
        float x0 = Caixa(chaos[0]).min.x, x1 = chaos.Max(g => Caixa(g).max.x);
        // Ordem −11, entre o barranco (−12) e o chão (−10). Na primeira montagem o rio estava em
        // −18, ATRÁS do barranco, e por isso só aparecia nas frestas entre os trechos de chão:
        // na captura ele lia como manchas de grama soltas no fundo, não como um rio.
        //
        // À frente do barranco ele vira uma faixa contínua logo abaixo da linha do chão, e o
        // caminho passa a ser um passadiço sobre a água — que é a imagem que dá nome à fase.
        // Fundo de agua funda ATRAS da faixa, cobrindo ate o pe do quadro. Sem ele, abaixo
        // do rio sobrava o preenchimento de rocha preto e a agua terminava numa linha reta.
        Bloco("River_Deep", (x0 + x1) * 0.5f, -6f, x1 - x0 + 20f, 8f, Cor("#111f0f"), -13);

        // Topo em -2, a MESMA linha do chao, e nao -3,1. Sob o passadico a agua fica escondida
        // pela face do chao (ordem -10, a frente), entao subir nao custa nada ali — e resolve
        // as pontas: alem do fim do nivel a faixa antiga comecava so em -3,1 e sobrava um
        // retangulo preto entre a linha do chao e a agua, visivel na captura em x=32.
        GameObject rio = Faixa("River", (x0 + x1) * 0.5f, -2f, x1 - x0 + 20f,
            "River/mortos_river_band.png", -11);
        // Escuro e puxado para o azul. Na cor nativa a agua ocupava o terco inferior do
        // quadro em verde saturado e continuava lendo como GRAMADO, agora um gramado com
        // riscos. Rio dos mortos e um teal quase preto: recua para tras do plano de jogo em
        // vez de disputar com ele, e os riscos claros passam a ser o unico sinal de agua.
        Apagar(rio, 0.62f, 0.55f);
        Deslizar(rio, -0.7f);

        // Pontas do passadiço. Sem elas o caminho acaba num corte vertical perfeito contra a
        // água — dá para ver no spawn, que fica em x=−12, a menos de uma tela do início em −16.
        // Um monte de pedra em cada extremidade transforma o corte no lugar onde o caminho
        // nasce da margem.
        foreach (float x in new[] { x0, x1 })
        {
            GameObject ponta = Prop($"Cap_{x:0}", x, TopoDoChao, "Props/mortos_shore_rocks.png", -7);
            Encolher(ponta, 0.7f);
            Apagar(ponta, 0.8f, 0.3f);
        }

        // Pedras na margem, do lado de cá, para a água não encostar direto no barranco. Precisam
        // ficar à FRENTE do rio (−9), senão a água passa por cima delas.
        for (int i = 0; i < 6; i++)
        {
            float x = x0 + (x1 - x0) * (i + 0.5f) / 6f;
            Encolher(Prop($"Shore_{i}", x, -3.0f, "Props/mortos_shore_rocks.png", -9), 0.55f);
        }
    }

    // ---------------------------------------------------------------- povoamento

    /// <summary>Entrada: árvores mortas e o portão de pedra que marca a passagem.</summary>
    private static void PovoarEntrada(List<GameObject> chaos)
    {
        Bounds b = Caixa(chaos[0]);
        Encolher(Prop("Gate", b.min.x + 2.5f, TopoDoChao, "Ruins/mortos_stone_gate.png", -16), 0.85f);

        // A árvore morta é o asset mais claro da fase (L 0,507) e na captura leu como árvore
        // coberta de NEVE, não como galho seco — branco puro contra pedra escura vira inverno.
        // Puxá-la para o azul-pedra devolve o galho morto sem regerar nada.
        GameObject t1 = Prop("Dead_Tree_1", b.min.x + 8f, TopoDoChao, "Props/mortos_dead_tree.png", -8);
        Encolher(t1, 0.8f);
        Apagar(t1, 0.60f, 0.45f);
        GameObject t2 = Prop("Dead_Tree_2", b.min.x + 13f, TopoDoChao, "Props/mortos_dead_tree.png", -8);
        Encolher(t2, 0.65f);
        Apagar(t2, 0.52f, 0.5f);
    }

    /// <summary>Caminho e ruínas: colunas quebradas, muros e estátuas, adensando ao fim.</summary>
    private static void PovoarRuinas(List<GameObject> chaos)
    {
        for (int i = 1; i < chaos.Count; i++)
        {
            Bounds b = Caixa(chaos[i]);
            // A densidade sobe com o trecho: o briefing pede que fique progressivamente mais
            // sobrenatural, e isso acontece por acúmulo, não por um corte.
            int colunas = 2 + i;
            for (int k = 0; k < colunas; k++)
            {
                float x = b.min.x + b.size.x * (k + 0.5f) / colunas;
                GameObject c = Prop($"Column_{i}_{k}", x, TopoDoChao,
                    "Ruins/mortos_broken_column.png", -15);
                if (c == null) { continue; }
                // Alturas alternadas: colunas iguais lado a lado leem como cerca.
                float k2 = k % 2 == 0 ? 0.9f : 0.62f;
                c.transform.localScale = new Vector3(k2, k2, 1f);
                c.GetComponent<SpriteRenderer>().color = Distancia(0.40f);
            }

            Encolher(Prop($"Wall_{i}", b.min.x + 3f, TopoDoChao, "Ruins/mortos_ruined_wall.png", -17), 0.9f);
            Encolher(Prop($"Statue_{i}", b.max.x - 4f, TopoDoChao, "Ruins/mortos_broken_statue.png", -6), 0.55f);
        }
    }

    /// <summary>
    /// O <c>Ruin_Decor</c> é um marcador de cenário do level design, desenhado como retângulo.
    /// Vira uma coluna quebrada na medida dele.
    /// </summary>
    private static void VestirDecorDeRuina()
    {
        foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
                     .Where(g => g != null && g.name.StartsWith("Ruin_Decor")))
        {
            var sr = go.GetComponent<SpriteRenderer>();
            float altura = sr != null ? sr.bounds.size.y : 1.8f;
            GameObject arte = Prop($"Art_{go.name}", go.transform.position.x, TopoDoChao,
                "Ruins/mortos_broken_column.png", -6);
            if (arte == null) { continue; }
            Sprite a = arte.GetComponent<SpriteRenderer>().sprite;
            float k = altura / a.bounds.size.y;
            arte.transform.localScale = new Vector3(k, k, 1f);
        }
    }

    /// <summary>
    /// A saída. O <c>PortalGlow</c> é um quadrado branco de 3×3 — e é o único placeholder da
    /// cena que NÃO deve ser apenas desligado: ele é a pista de gameplay que diz onde a fase
    /// acaba, e apagá-lo tiraria informação do jogador em vez de sujeira da tela.
    ///
    /// Então a marca é apagada e a MESMA informação é redesenhada por cima, na medida dela: um
    /// portão de pedra e a luz das almas saindo de dentro. O objeto do level design não é
    /// tocado — nem o sprite, nem a escala — como em todo o resto da cena.
    /// </summary>
    private static void MontarSaida()
    {
        GameObject marca = GameObject.Find("PortalGlow");
        if (marca == null) { return; }

        var srMarca = marca.GetComponent<SpriteRenderer>();
        Vector3 onde = marca.transform.position;
        float alturaDaMarca = srMarca != null ? srMarca.bounds.size.y : 3f;
        if (srMarca != null) { srMarca.enabled = false; }

        Encolher(Prop("Exit_Gate", onde.x, TopoDoChao, "Ruins/mortos_stone_gate.png", -16), 0.95f);

        GameObject luz = Prop("Exit_Glow", onde.x, onde.y, "Souls/mortos_soul_lights.png", -2);
        if (luz == null) { return; }
        Sprite s = luz.GetComponent<SpriteRenderer>().sprite;
        float k = alturaDaMarca / Mathf.Max(0.01f, s.bounds.size.y);
        luz.transform.localScale = new Vector3(k, k, 1f);
        luz.GetComponent<SpriteRenderer>().color = new Color(0.72f, 0.86f, 0.95f, 0.75f);
    }

    /// <summary>
    /// §"TEMPLO / ALTAR". Os altares vão nos <c>DialogueTrigger</c>, que é onde a narrativa
    /// acontece — e são também as fontes de luz que mantêm esses pontos legíveis. O espaço em
    /// volta fica livre para Odisseu, as sombras e o diálogo entrarem depois.
    /// </summary>
    private static void MontarAltares()
    {
        foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
                     .Where(g => g != null && g.name.StartsWith("DialogueTrigger_")
                         && g.GetComponent<Collider2D>() != null)
                     .OrderBy(g => g.transform.position.x))
        {
            Bounds b = Caixa(go);
            Encolher(Prop($"Altar_{go.name}", b.center.x, TopoDoChao, "Special/mortos_altar.png", -4), 0.75f);
            Encolher(Prop($"Brazier_{go.name}_L", b.min.x - 1.8f, TopoDoChao,
                "Props/mortos_brazier_tall.png", -3), 0.7f);
            Encolher(Prop($"Brazier_{go.name}_R", b.max.x + 1.8f, TopoDoChao,
                "Props/mortos_brazier_tall.png", -3), 0.7f);
        }
    }

    /// <summary>
    /// Névoa e almas. As duas deslizam por TEMPO com <see cref="ScrollingLayer"/> — o parallax
    /// responde à câmera e sozinho deixaria tudo congelado quando o jogador para, que é
    /// exatamente o oposto do que uma fase de almas precisa.
    ///
    /// A névoa é veladura de alpha baixo: o briefing proíbe que ela esconda jogador, inimigo ou
    /// plataforma, e manchas fracas somadas dão atmosfera sem cobrir nada.
    /// </summary>
    private static void MontarAtmosfera()
    {
        GameObject nevoaFundo = Camada("Mist_Back", "VFX/mortos_mist.png", 0.50f, -13, -2.9f, 1f);
        Deslizar(nevoaFundo, -0.35f);

        GameObject almas = Camada("Souls", "Souls/mortos_soul_lights.png", 0.34f, -5, -2.5f, 2f);
        Deslizar(almas, -0.5f);

        GameObject nevoaFrente = Camada("Mist_Front", "VFX/mortos_mist.png", 0.12f, 10, -2.7f, 1f);
        if (nevoaFrente != null)
        {
            var sr = nevoaFrente.GetComponent<SpriteRenderer>();
            Color c = sr.color;
            sr.color = new Color(c.r, c.g, c.b, 0.45f);
        }
        Deslizar(nevoaFrente, -0.85f);
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
    /// Primeiro plano sem <see cref="ParallaxLayer"/>: o fator dele é limitado a [0,1] e
    /// primeiro plano exigiria mais que 1. Pedras baixas e enterradas com ordem acima do
    /// jogador — nunca o escondem.
    /// </summary>
    private static void MontarPrimeiroPlano()
    {
        float[] posicoes = { -14f, -3f, 9f, 22f, 32f };
        for (int i = 0; i < posicoes.Length; i++)
        {
            GameObject go = Prop($"FG_Rocks_{i}", posicoes[i], TopoDoChao - 1.5f,
                "Props/mortos_shore_rocks.png", 12);
            if (go == null) { continue; }
            go.transform.localScale = new Vector3(0.75f, 0.75f, 1f);
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

    /// <summary>
    /// Escurece um renderer puxando-o para a pedra fria, em vez de multiplicar por cinza.
    ///
    /// Medido na primeira montagem, o chão de ruína (L 0,436) e a árvore morta (L 0,507) eram as
    /// superfícies mais CLARAS da fase — mais que a parede e quase tanto quanto o fogo. Num
    /// subterrâneo isso inverte a leitura: o olho vai para o piso e não para a chama, e a fase
    /// perde a hierarquia que a mantém jogável.
    ///
    /// Escurecer para o azul-pedra, e não para cinza neutro, evita o defeito de Lestrigões — lá
    /// tudo dessaturou junto e a fase virou uma massa única.
    /// </summary>
    /// <param name="k">quanto sobra do brilho: 0,72 = escurece 28%</param>
    /// <param name="frieza">quanto do matiz vai para o azul-pedra, 0 a 1</param>
    private static void Apagar(GameObject go, float k, float frieza = 0.25f)
    {
        if (go == null) { return; }
        var sr = go.GetComponent<SpriteRenderer>();
        if (sr == null) { return; }
        sr.color = Color.Lerp(Color.white, Cor("#8fa6b8"), frieza) * k;
    }

    /// <param name="alturaFixa">
    /// altura em unidades; 0 usa a nativa do sprite. Encurtar RECORTA o tile pela base — o que
    /// serve para uma face de chão que morre na água, e não para uma banda que se repete
    /// verticalmente, onde altura fora do múltiplo nativo mostra tile cortado no meio.
    /// </param>
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
        if (s == null) { Debug.LogWarning("[Mortos] sprite não encontrado: " + caminho); }
        return s;
    }

    private static Color Cor(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color c);
        return c;
    }
}
