using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Odisseia.Systems;

/// <summary>
/// Veste a fase 15 com o palácio de Odisseu tomado pelos pretendentes:
///
/// Unity.exe -batchmode -quit -nographics -projectPath . -executeMethod PretendentesSceneDresser.Run
///
/// Mesmo desenho dos vestidores das fases 02 a 14 — não é sistema novo.
///
/// ## A mesma casa, ocupada — e a arte vem de três lugares
///
/// O briefing pede que as fases 01, 14 e 15 pareçam o mesmo lugar. Por isso cada objeto aqui é
/// contado pela ORIGEM, e a origem é a tese:
///
/// - **Da fase 01** — o portão, as colunas, o arsenal de Odisseu (lança, escudo, espada, feixe de
///   flechas), o campo de treino inteiro (boneco, poste, alvo, suporte de armas), mesa, banco,
///   barril, caixote, ânfora e a silhueta de montanhas. **Os pretendentes estão usando as coisas
///   dele.** O suporte de armas da fase 01, onde Odisseu treinava, agora é onde eles deixam as
///   lanças deles; o boneco de treino dele virou alvo de bêbado. Não há asset novo que diga isso
///   tão bem quanto o asset velho no lugar errado.
/// - **Da fase 14** — o céu de fim de tarde, agora escurecido para noite (é literalmente o mesmo
///   céu, horas depois), os braseiros e o piso de laje do bairro do palácio.
/// - **Novos** — só o que os pretendentes TROUXERAM: estandartes do javali, barricadas, fogueira
///   de assar, mesas de banquete, vinho derramado, ânforas quebradas, o trono ocupado por lanças.
///
/// ## Luz natural vira luz artificial
///
/// A fase 14 termina num anoitecer frio. Esta começa na noite e, conforme se entra no domínio dos
/// pretendentes, a luz passa a vir do fogo: fogueira no pátio, tochas no salão, e um degradê
/// laranja que ESQUENTA o salão em direção ao trono. É a inversão do véu da fase 14 — mesma
/// ferramenta, sentido contrário — e é ela que marca "isto já não é a Ítaca de Odisseu".
///
/// ## Gameplay antes de decoração
///
/// Os quatro pretendentes lutam no salão em x=24, 28, 32 e 36. Nenhuma mesa, estandarte ou tocha
/// entra em ordem de desenho à frente deles: tudo que decora o salão fica em ordem negativa, e as
/// mesas ficam ENTRE as posições dos inimigos, nunca em cima.
/// </summary>
public static class PretendentesSceneDresser
{
    private const string ScenePath = "Assets/Scenes/Levels/Level_15_Pretendentes.unity";
    private const string Raiz = "Assets/Art/Environments/Pretendentes/";
    private const string Ithaca = "Assets/Art/Environments/Ithaca/";
    private const string ItacaReturn = "Assets/Art/Environments/ItacaReturn/";
    private const string RaizDoCenario = "PretendentesScenery";

    private const float TopoDoChao = -2f;
    private const float CameraInicial = 0f;

    private static float CameraMin = -16f;
    private static float CameraMax = 44f;

    /// <summary>Onde o salão começa. Sai do <c>Floor_3_Hall</c>, não de um número escrito aqui.</summary>
    private static float InicioDoSalao = 20f;

    /// <summary>
    /// A câmera desta cena vai até x=48 e enxerga 8,9 unidades para cada lado. Chão, faixas e
    /// véus precisam cobrir ATÉ ONDE ELA ENXERGA, não até onde o chão acaba — foi um buraco
    /// magenta no último quadro da fase 14 que ensinou isso.
    /// </summary>
    private const float AlcanceDaCamera = 57f;

    private const float SuperficieDoTile = 16f / 42.857143f;

    /// <summary>
    /// O tom de noite aplicado ao céu reusado da fase 14. É o mesmo arquivo; só a hora mudou.
    /// </summary>
    private static readonly Color Noite = new Color(0.36f, 0.40f, 0.60f, 1f);

    private static Transform cenario;
    private static int daFase01, daFase14, novos;

    [MenuItem("Odisseia/Vestir Pretendentes")]
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
        daFase01 = 0; daFase14 = 0; novos = 0;

        List<GameObject> chaos = Plataformas();
        if (chaos.Count == 0)
        {
            Debug.LogError("[Pretendentes] nenhum Floor_* encontrado — cenário não montado");
            return false;
        }

        CameraMin = chaos.Min(g => Caixa(g).min.x);
        CameraMax = chaos.Max(g => Caixa(g).max.x);
        GameObject salao = chaos.FirstOrDefault(g => g.name.StartsWith("Floor_3"));
        if (salao != null) { InicioDoSalao = Caixa(salao).min.x; }

        DesligarPlaceholders();
        MontarCeuNoturno();
        MontarParedes();
        VestirChao(chaos);
        MontarExterior();
        MontarPatio();
        MontarPortaDoSalao();
        MontarSalao();
        MontarLuzDeFogo();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[Pretendentes] cenário montado: {cenario.childCount} objetos sob {RaizDoCenario} — " +
            $"{daFase01} da fase 01, {daFase14} da fase 14, {novos} novos");
        return true;
    }

    private static List<GameObject> Plataformas() =>
        Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
            .Where(g => g != null && g.name.StartsWith("Floor_") && g.GetComponent<Collider2D>() != null)
            .OrderBy(g => g.transform.position.x)
            .ToList();

    private static void DesligarPlaceholders()
    {
        string[] prefixos = { "Floor_", "Sky_Background", "LevelGoal" };
        foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (go == null || !prefixos.Any(pre => go.name.StartsWith(pre))) { continue; }
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null) { sr.enabled = false; }
        }
    }

    // ---------------------------------------------------------------- céu

    /// <summary>
    /// O céu da fase 14, escurecido para noite.
    ///
    /// Não é um céu parecido: é o mesmo arquivo (<c>itaca_ret_bg_dusk</c>) com a cor multiplicada
    /// por um azul de noite. A fase 14 termina ao anoitecer e esta começa horas depois, e o céu que
    /// o jogador acabou de ver continua lá em cima.
    ///
    /// A cobertura de céu é a cor MEDIDA da primeira linha do asset (#99a593) multiplicada pelo
    /// MESMO tom — calculada aqui, e não escrita à mão, para a emenda entre as duas não aparecer.
    /// </summary>
    private static void MontarCeuNoturno()
    {
        Bloco("Sky_Fill", Meio(), 16f, 180f, 36f, Cor("#99a593") * Noite, -60);

        // Terra abaixo do piso: passo Profunda de `Pedra`. Ordem −12, logo atrás do corpo do tile.
        // O TOPO deste bloco fica em y=−1,9, e não em −3 — e esta é uma correção medida, não
        // estética. Os tilesets de Ítaca têm falhas na fileira de superfície, e com o bloco
        // terminando em −3 nada cobria a faixa entre −3 e −2 atrás delas: a cor de fundo da
        // câmera vazava numa linha pontilhada MAGENTA exatamente sobre a linha do chão — até 118
        // pixels por quadro, contados na captura (linha y=378). Calipso não tem o defeito porque
        // os tiles dela têm a superfície fechada. Os 0,1 un de sobra acima do chão escondem no
        // máximo 4 px da base do que estiver atrás, e isso já fica sob o tile de superfície.
        Bloco("Ground_Fill", Meio(), -17.95f, 180f, 32.1f, Cor("#3e3a33"), -12);

        GameObject ceu = Camada("BG_Night", ItacaReturn + "Background/itaca_ret_bg_dusk.png", 0.92f, -50, -1.6f);
        if (ceu != null) { ceu.GetComponent<SpriteRenderer>().color = Noite; daFase14++; }

        GameObject montanhas = Camada("BG_Mountains", Ithaca + "Background/ithaca_bg_mountains_far.png", 0.86f, -48, -0.2f);
        if (montanhas != null)
        {
            montanhas.GetComponent<SpriteRenderer>().color = Color.Lerp(Noite, Color.black, 0.25f);
            daFase01++;
        }
    }

    // ---------------------------------------------------------------- paredes

    /// <summary>
    /// O muro do pátio do lado de fora, a parede do salão do lado de dentro.
    ///
    /// As duas são faixas presas ao mundo, e não camadas de parallax: cada uma precisa COMEÇAR e
    /// TERMINAR num ponto exato (a porta do salão, em <see cref="InicioDoSalao"/>), e uma camada de
    /// parallax não tem borda fixa em x — encurtá-la só muda onde a borda passeia. Medido em Calipso.
    /// </summary>
    private static void MontarParedes()
    {
        GameObject muro = Faixa("Courtyard_Wall", (CameraMin - 12f + InicioDoSalao) * 0.5f, TopoDoChao + 6.72f,
            InicioDoSalao - CameraMin + 12f, "Courtyard/pret_courtyard_wall_band.png", -40);
        if (muro != null) { muro.GetComponent<SpriteRenderer>().color = new Color(0.78f, 0.78f, 0.86f, 1f); novos++; }

        GameObject parede = Faixa("Hall_Wall", (InicioDoSalao + AlcanceDaCamera) * 0.5f, TopoDoChao + 7.47f,
            AlcanceDaCamera - InicioDoSalao, "GreatHall/pret_hall_wall_band.png", -30);
        if (parede != null) { novos++; }

        // O salão é INTERIOR: acima da parede não há céu, há teto escuro. Sem este bloco, com a
        // câmera alta, o céu noturno do exterior aparecia por cima do salão e a fase inteira
        // parecia ao ar livre.
        Bloco("Hall_Ceiling", (InicioDoSalao + AlcanceDaCamera) * 0.5f, 20f,
            AlcanceDaCamera - InicioDoSalao, 30f, Cor("#0f1118"), -35);
    }

    // ---------------------------------------------------------------- chão

    /// <summary>
    /// Laje de pedra em toda a fase — o mesmo piso do bairro do palácio da fase 14, extraído do
    /// mesmo tileset da fase 01. O salão ganha barranco mais escuro: é interior, e a luz lá vem
    /// de tocha, não do céu.
    /// </summary>
    private static void VestirChao(List<GameObject> chaos)
    {
        foreach (GameObject chao in chaos)
        {
            Bounds b = Caixa(chao);
            bool salao = b.min.x >= InicioDoSalao - 0.01f;

            float esquerda = b.min.x <= CameraMin + 0.01f ? CameraMin - 12f : b.min.x;
            float direita = b.max.x >= CameraMax - 0.01f ? AlcanceDaCamera : b.max.x;
            float largura = direita - esquerda, centro = (esquerda + direita) * 0.5f;

            GameObject topo = Faixa($"GroundTop_{chao.name}", centro, b.max.y + SuperficieDoTile, largura,
                ItacaReturn + "Gameplay/ithaca_stone_ground_top.png", -10);
            if (topo == null) { continue; }
            daFase14++;

            float alturaDoTile = topo.GetComponent<SpriteRenderer>().sprite.bounds.size.y;
            GameObject corpo = Faixa($"GroundBody_{chao.name}", centro, b.max.y + SuperficieDoTile - alturaDoTile,
                largura, ItacaReturn + "Gameplay/ithaca_stone_ground_body.png", -11, 2.4f);
            if (corpo != null)
            {
                float k = salao ? 0.30f : 0.40f;
                corpo.GetComponent<SpriteRenderer>().color = new Color(k, k * 0.96f, k * 1.08f, 1f);
            }
            // O topo do piso também recebe a noite, mais leve que o barranco: é o que o jogador
            // pisa, e precisa continuar sendo a linha mais clara do quadro.
            topo.GetComponent<SpriteRenderer>().color = salao
                ? new Color(0.82f, 0.74f, 0.66f, 1f) : new Color(0.72f, 0.74f, 0.86f, 1f);
        }
    }

    // ---------------------------------------------------------------- exterior

    /// <summary>
    /// A entrada, do lado de fora do muro. Aqui a ocupação ainda é acampamento: fogueira de assar,
    /// barricada, barris e ânforas amontoados — o que um bando faz quando se instala na porta dos
    /// outros.
    /// </summary>
    private static void MontarExterior()
    {
        Encolher(Novo("Barricade_Out", -10f, TopoDoChao, "Invasion/pret_barricade.png", -4), 0.6f);
        Encolher(Novo("Cooking_Fire", -6.5f, TopoDoChao, "Courtyard/pret_cooking_fire.png", -3), 0.55f);
        Encolher(Novo("Amphorae_Out", -3.8f, TopoDoChao, "Banquet/pret_amphora_group.png", -3), 0.55f);

        foreach (float x in new[] { -13f, -1.8f })
        {
            Encolher(Novo($"Torch_{x:0}", x, TopoDoChao, "VFX/pret_torch_stand.png", -7), 0.62f);
        }

        Encolher(Reuso01("Barrel_Out", -14.5f, TopoDoChao, "Props/ithaca_barrel_01.png", -3), 0.8f);
        Encolher(Reuso01("Crate_Out", -12f, TopoDoChao, "Props/ithaca_crate_01.png", -3), 0.8f);

        // O portão da fase 01 marca a entrada do pátio. É o mesmo portão; agora aberto para quem
        // não devia passar.
        Encolher(Reuso01("Gate", -0.3f, TopoDoChao - 0.05f, "Architecture/ithaca_gate_01.png", -8), 0.9f);
    }

    // ---------------------------------------------------------------- pátio

    /// <summary>
    /// O pátio. Penélope em x=1..3, checkpoint em 6, Telêmaco em 15..17.
    ///
    /// **O campo de treino é o da fase 01, inteiro.** Boneco, poste, alvo e suporte de armas são os
    /// mesmos quatro sprites em que Odisseu treinava no prólogo. Agora estão cercados de ânfora e
    /// com as lanças dos pretendentes no suporte. O checkpoint em x=6 fica livre.
    /// </summary>
    private static void MontarPatio()
    {
        Encolher(Reuso01("Training_Dummy", 8.2f, TopoDoChao, "Training/ithaca_training_dummy_01.png", -5), 0.85f);
        Encolher(Reuso01("Practice_Post", 9.8f, TopoDoChao, "Training/ithaca_practice_post_01.png", -5), 0.85f);
        Encolher(Reuso01("Weapon_Rack", 11.6f, TopoDoChao, "Training/ithaca_weapon_rack_01.png", -5), 0.85f);
        Encolher(Reuso01("Training_Target", 13.4f, TopoDoChao, "Training/ithaca_training_target_01.png", -5), 0.8f);

        // O arsenal de Odisseu, largado no chão.
        Encolher(Reuso01("Shield_Dropped", 4.2f, TopoDoChao, "Arsenal/ithaca_shield_round_01.png", -2), 0.7f);
        Encolher(Reuso01("Spear_Leaning", 4.9f, TopoDoChao, "Arsenal/ithaca_spear_01.png", -3), 0.8f);
        Encolher(Reuso01("Arrows", 12.6f, TopoDoChao, "Arsenal/ithaca_arrows_bundle_01.png", -3), 0.7f);

        Encolher(Reuso01("Table_Court", 18.6f, TopoDoChao, "Props/ithaca_table_01.png", -4), 0.8f);
        Encolher(Novo("Feast_Court", 18.2f, TopoDoChao, "Banquet/pret_spilled_feast.png", -2), 0.55f);
        Encolher(Reuso01("Amphora_Court", 14.3f, TopoDoChao, "Props/ithaca_amphora_01.png", -3), 0.75f);

        // Os braseiros da fase 14 — os mesmos que ladeavam a porta de casa — agora dentro do pátio.
        foreach (float x in new[] { 3.8f, 19.2f })
        {
            Encolher(Reuso14($"Brazier_{x:0}", x, TopoDoChao, "Props/itaca_ret_brazier.png", -6), 0.55f);
        }
    }

    /// <summary>
    /// A porta do salão: duas colunas da fase 01 em cima da junta entre o muro do pátio e a parede
    /// do salão. Elas marcam a entrada E mascaram a emenda entre as duas faixas, que sem nada em
    /// cima leria como um corte vertical reto.
    /// </summary>
    private static void MontarPortaDoSalao()
    {
        foreach (float dx in new[] { -0.9f, 0.9f })
        {
            Encolher(Reuso01($"Hall_Door_Column_{dx:0.0}", InicioDoSalao + dx, TopoDoChao - 0.05f,
                "Architecture/ithaca_column_01.png", -28), 1.15f);
        }
    }

    // ---------------------------------------------------------------- salão

    /// <summary>
    /// O grande salão, onde se luta.
    ///
    /// Mesas ENTRE as posições dos inimigos (24, 28, 32, 36), nunca em cima; tudo em ordem negativa.
    /// O trono vai no objetivo, porque o objetivo desta fase é retomar o lugar dele.
    /// </summary>
    private static void MontarSalao()
    {
        foreach (float x in new[] { 26f, 34f })
        {
            Encolher(Novo($"Banquet_Table_{x:0}", x, TopoDoChao, "Banquet/pret_banquet_table.png", -6), 0.58f);
        }
        foreach (var (x, arte, escala) in new[]
        {
            (22.6f, "Banquet/pret_bench_toppled.png", 0.55f),
            (30f, "Banquet/pret_spilled_feast.png", 0.55f),
            (38.5f, "Banquet/pret_spilled_feast.png", 0.5f),
            (44.5f, "Banquet/pret_amphora_group.png", 0.6f),
        })
        {
            Encolher(Novo($"Hall_{x:0}", x, TopoDoChao, arte, -3), escala);
        }

        foreach (float x in new[] { 21.8f, 30f, 38f, 46f })
        {
            Encolher(Novo($"Hall_Torch_{x:0}", x, TopoDoChao, "VFX/pret_torch_stand.png", -7), 0.62f);
        }

        // Estandartes do javali pendurados na parede do salão. Ordem −29: à frente da parede e atrás
        // de todo o resto. Altura calculada para o topo cair no friso, e não um número chutado.
        foreach (float x in new[] { 24f, 32f, 40f })
        {
            GameObject bandeira = Novo($"Banner_{x:0}", x, 0f, "Invasion/pret_banner.png", -29);
            if (bandeira == null) { continue; }
            const float escala = 0.55f;
            Encolher(bandeira, escala);
            float altura = bandeira.GetComponent<SpriteRenderer>().sprite.bounds.size.y * escala;
            bandeira.transform.position = new Vector3(x, TopoDoChao + 6.2f - altura, 0f);
        }

        GameObject alvo = GameObject.Find("LevelGoal");
        float xTrono = alvo != null ? Caixa(alvo).center.x : 42f;
        Encolher(Novo("Throne", xTrono, TopoDoChao, "GreatHall/pret_throne.png", -8), 0.62f);
    }

    /// <summary>
    /// A luz de fogo esquentando o salão em direção ao trono — ver a seção 2 do cabeçalho.
    /// Um sprite de 256×8 com alpha por coluna, esticado pelo transform: a malha "tight" recorta a
    /// metade transparente de um degradê, e com Sliced/Tiled o véu não aparecia.
    /// </summary>
    private static void MontarLuzDeFogo()
    {
        Sprite gradiente = Arte("VFX/pret_firelight_gradient.png");
        if (gradiente == null) { return; }

        float x0 = InicioDoSalao, ate = AlcanceDaCamera + 6f;
        var veu = new GameObject("FirelightVeil");
        veu.transform.SetParent(cenario, false);
        // y = −16: pivô na BASE, o sprite desenha do pivô para cima.
        veu.transform.position = new Vector3((x0 + ate) * 0.5f, -16f, 0f);
        veu.transform.localScale = new Vector3((ate - x0) / gradiente.bounds.size.x, 40f / gradiente.bounds.size.y, 1f);

        var sr = veu.AddComponent<SpriteRenderer>();
        sr.sprite = gradiente;
        sr.sortingOrder = 20;
        novos++;
    }

    // ---------------------------------------------------------------- utilidades

    private static GameObject Reuso01(string nome, float x, float y, string caminho, int ordem)
    {
        GameObject go = Prop(nome, x, y, Ithaca + caminho, ordem);
        if (go != null) { daFase01++; }
        return go;
    }

    private static GameObject Reuso14(string nome, float x, float y, string caminho, int ordem)
    {
        GameObject go = Prop(nome, x, y, ItacaReturn + caminho, ordem);
        if (go != null) { daFase14++; }
        return go;
    }

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

    private static GameObject Camada(string nome, string caminho, float fator, int ordem, float baseY)
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
        sr.size = new Vector2(curso * (1f - fator) + larguraDaTela * 2f, arte.bounds.size.y);

        var parallax = go.AddComponent<ParallaxLayer>();
        var so = new SerializedObject(parallax);
        so.FindProperty("parallaxFactor").floatValue = fator;
        so.ApplyModifiedProperties();
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
        // Pivô na base: a faixa desenha para cima, então a origem desce a altura inteira.
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

    private static void Bloco(string nome, float x, float y, float largura, float altura, Color cor, int ordem)
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
        if (s == null) { Debug.LogWarning("[Pretendentes] sprite não encontrado: " + caminho); }
        return s;
    }

    private static Color Cor(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color c);
        return c;
    }
}
