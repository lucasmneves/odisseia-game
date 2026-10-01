using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Odisseia.Levels;
using Odisseia.Systems;

/// <summary>
/// Veste a fase 16 — o palácio de Ítaca na manhã seguinte, a prova do arco e o reencontro:
///
/// Unity.exe -batchmode -quit -nographics -projectPath . -executeMethod FinalSceneDresser.Run
/// Unity.exe -batchmode -quit -projectPath . -executeMethod FinalSceneDresser.Shots
///
/// Mesmo desenho dos vestidores das fases 02 a 15 — não é sistema novo. Só cria objetos sob
/// <see cref="RaizDoCenario"/> e desliga o DESENHO dos placeholders: nenhum colisor, script,
/// posição de gameplay, inimigo, câmera ou objetivo é tocado. Os anéis do <see cref="BowChallenge"/>
/// continuam sendo os mesmos <see cref="AxeRing"/>, com o mesmo trigger; o machado é um objeto à
/// parte posicionado em cima de cada um.
///
/// ## A mesma casa, outra hora — e a arte vem de três lugares
///
/// - **Da fase 15** — a parede do salão, o trono e o piso; **da 14** os braseiros; **da 01** as
///   colunas da porta e o feixe de flechas. É o salão onde a fase 15 terminou, na manhã seguinte.
/// - **Novos** (Docs/Environment/Fase16) — só o que a 16 tem e as outras não: os machados da
///   prova, o arco de Odisseu, a lareira do mégaron, o tear de Penélope, o céu do amanhecer, a
///   fachada do palácio vista de fora e duas peças de primeiro plano.
///
/// ## A luz conta o fim
///
/// A 15 começa na noite e esquenta com fogo em direção ao trono. A 16 inverte: começa do lado de
/// fora, no amanhecer, e a luz que entra no salão é de sol — fachos oblíquos e um véu de ouro
/// pálido que CLAREIA em direção ao fim, onde a jornada termina. Nenhuma sombra noturna.
///
/// ## Gameplay antes de decoração
///
/// Os anéis estão em x=10, 18, 26 e 34 e o objetivo em 38. Lareira e tear ficam ENTRE anéis
/// (22 e 30), em ordem negativa, atrás do Odisseu (ordem 2). O primeiro plano fica só nas
/// pontas da fase, longe do caminho e dos anéis.
/// </summary>
public static class FinalSceneDresser
{
    private const string ScenePath = "Assets/Scenes/Levels/Level_16_Final.unity";
    private const string Raiz = "Assets/Art/Environments/Final/";
    private const string Ithaca = "Assets/Art/Environments/Ithaca/";
    private const string ItacaReturn = "Assets/Art/Environments/ItacaReturn/";
    private const string Pretendentes = "Assets/Art/Environments/Pretendentes/";
    private const string RaizDoCenario = "FinalScenery";

    private const float TopoDoChao = -2f;
    private const float SuperficieDoTile = 16f / 42.857143f;

    private static float CameraMin = -16f;
    private static float CameraMax = 40f;

    /// <summary>A porta do salão: o começo do <c>Floor_2</c>, lido da cena.</summary>
    private static float InicioDoSalao = 0f;

    /// <summary>
    /// Até onde a câmera enxerga: o <c>maxBounds.x</c> do CameraFollow desta cena é 44. Chão,
    /// paredes e véus cobrem até aqui, com folga — a fase 14 ensinou com um buraco magenta.
    /// </summary>
    private const float AlcanceDaCamera = 50f;

    /// <summary>Cor MEDIDA da primeira linha de <c>final_bg_dawn</c> (1534 de 1534 px).</summary>
    private const string CorDoTopoDoCeu = "#b0c5d1";

    /// <summary>
    /// A parede da 15 é a mesma; só a hora mudou. Na 15 ela era vista à luz de tocha; aqui um
    /// toque de manhã a deixa um pouco mais clara e quente, sem pintar outra parede.
    /// </summary>
    private static readonly Color Manha = new Color(1f, 0.97f, 0.92f, 1f);

    /// <summary>Tom do primeiro plano: silhueta contra a luz, ainda legível como pedra e folha.</summary>
    private static readonly Color ContraLuz = new Color(0.42f, 0.40f, 0.46f, 1f);

    private static Transform cenario;
    private static int reusados, novos;

    [MenuItem("Odisseia/Vestir Final")]
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
            Debug.LogError("[Final] nenhum Floor_* encontrado — cenário não montado");
            return false;
        }

        CameraMin = chaos.Min(g => Caixa(g).min.x);
        CameraMax = chaos.Max(g => Caixa(g).max.x);
        GameObject salao = chaos.FirstOrDefault(g => g.name.StartsWith("Floor_2"));
        if (salao != null) { InicioDoSalao = Caixa(salao).min.x; }

        DesligarPlaceholders();
        MontarCeu();
        MontarPalacio();
        VestirChao(chaos);
        MontarExterior();
        MontarPortaDoSalao();
        MontarMachados();
        MontarSalao();
        MontarLuzDaManha();
        MontarPrimeiroPlano();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[Final] cenário montado: {cenario.childCount} objetos sob {RaizDoCenario} — " +
            $"{reusados} reusados das fases 01/14/15, {novos} novos");
        return true;
    }

    private static List<GameObject> Plataformas() =>
        Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
            .Where(g => g != null && g.name.StartsWith("Floor_") && g.GetComponent<Collider2D>() != null)
            .OrderBy(g => g.transform.position.x)
            .ToList();

    /// <summary>
    /// Desliga só o SpriteRenderer dos placeholders. Os anéis entram aqui também: o quadrado
    /// cinza esticado era o "machado"; o trigger e o <see cref="AxeRing"/> ficam como estão.
    /// </summary>
    private static void DesligarPlaceholders()
    {
        string[] prefixos = { "Floor_", "Sky_Background", "AxeRing_" };
        foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (go == null || !prefixos.Any(pre => go.name.StartsWith(pre))) { continue; }
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null) { sr.enabled = false; }
        }
    }

    // ---------------------------------------------------------------- céu e palácio

    /// <summary>
    /// O amanhecer sobre Ítaca: céu azul-claro que esquenta para pêssego no horizonte, sol baixo
    /// entre as montanhas, mar calmo. Acima dela a cobertura na cor medida do topo do céu.
    ///
    /// Base em y=0,3, e não no chão: com a base em −2,6 o horizonte caía em y≈0,6, ATRÁS da
    /// fachada do midground (topo em ≈3,2), e o sol, o mar e as montanhas sumiam — sobravam só
    /// nuvens acima dos telhados. Medido na prévia. Em 0,3 o horizonte fica em ≈3,5, logo acima
    /// dos frontões.
    /// </summary>
    private static void MontarCeu()
    {
        Bloco("Sky_Fill", Meio(), 16f, 200f, 36f, Cor(CorDoTopoDoCeu), -60);

        // Terra abaixo do piso, topo em −1,9 e não em −3: os tiles de Ítaca têm falhas na fileira
        // de superfície, e a 15 mediu até 118 px de magenta vazando ali (ver PretendentesSceneDresser).
        Bloco("Ground_Fill", Meio(), -17.95f, 200f, 32.1f, Cor("#3e3a33"), -12);

        if (Camada("BG_Dawn", Raiz + "Background/final_bg_dawn.png", 0.92f, -50, 0.3f) != null) { novos++; }
    }

    /// <summary>
    /// O palácio visto de fora, em parallax médio — a fachada com o pórtico e os ciprestes. Só
    /// aparece no pátio: do lado de dentro a parede do salão (ordem −30) passa à frente dele.
    /// </summary>
    private static void MontarPalacio()
    {
        GameObject palacio = Camada("MG_Palace", Raiz + "Midground/final_mg_palace_band.png", 0.55f, -45, -2.2f);
        if (palacio != null)
        {
            // Um pouco mais pálido que o primeiro plano: está mais longe, e a névoa da manhã
            // ainda não subiu.
            palacio.GetComponent<SpriteRenderer>().color = new Color(0.94f, 0.94f, 0.98f, 1f);
            novos++;
        }

        // A parede do salão, da 15. Começa na porta e vai até onde a câmera enxerga.
        GameObject parede = Faixa("Hall_Wall", (InicioDoSalao + AlcanceDaCamera) * 0.5f, TopoDoChao + 7.47f,
            AlcanceDaCamera - InicioDoSalao, Pretendentes + "GreatHall/pret_hall_wall_band.png", -30);
        if (parede != null) { parede.GetComponent<SpriteRenderer>().color = Manha; reusados++; }
        // Sem teto escuro (a 15 tinha um): acima da parede aparece o céu da manhã, com a câmera alta.
    }

    // ---------------------------------------------------------------- chão

    /// <summary>
    /// O mesmo piso de laje das fases 14 e 15. Na 15 o barranco era escurecido para a noite; aqui
    /// a cor fica mais perto da arte original — é dia.
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
            reusados++;

            float alturaDoTile = topo.GetComponent<SpriteRenderer>().sprite.bounds.size.y;
            GameObject corpo = Faixa($"GroundBody_{chao.name}", centro, b.max.y + SuperficieDoTile - alturaDoTile,
                largura, ItacaReturn + "Gameplay/ithaca_stone_ground_body.png", -11, 2.4f);
            if (corpo != null)
            {
                float k = salao ? 0.52f : 0.62f;
                corpo.GetComponent<SpriteRenderer>().color = new Color(k, k * 0.97f, k * 0.94f, 1f);
            }
            topo.GetComponent<SpriteRenderer>().color = salao ? new Color(0.96f, 0.92f, 0.86f, 1f) : Color.white;
        }
    }

    // ---------------------------------------------------------------- pátio

    /// <summary>
    /// O pátio, ao amanhecer. O Odisseu começa em x=−12. O arco espera em x=−4,4, de pé, ao lado
    /// do feixe de flechas da fase 01 — é ali que a prova começa. Em x=−6,4 ele caía na frente de
    /// um cipreste escuro do midground e sumia; aqui fica contra a parede clara, junto ao braseiro. Braseiros da 14, ainda
    /// acesos da noite, marcam a entrada.
    /// </summary>
    private static void MontarExterior()
    {
        foreach (float x in new[] { -14.6f, -2.4f })
        {
            Encolher(Reuso($"Brazier_{x:0.0}", x, TopoDoChao, ItacaReturn + "Props/itaca_ret_brazier.png", -6), 0.55f);
        }

        // O arco de Odisseu: 2,03 un na densidade nativa, maior que ele — é o arco que nenhum
        // pretendente conseguiu armar. Inclinado 10°, apoiado no chão pela ponta de baixo.
        GameObject arco = Novo("Bow_Odysseus", -4.4f, TopoDoChao, Raiz + "Props/final_bow_odysseus.png", -4);
        if (arco != null) { arco.transform.rotation = Quaternion.Euler(0f, 0f, -10f); }

        Encolher(Reuso("Arrows_Bundle", -5.3f, TopoDoChao, Ithaca + "Arsenal/ithaca_arrows_bundle_01.png", -3), 0.7f);
    }

    /// <summary>
    /// A porta do salão: as colunas da fase 01 sobre a junta entre o pátio e a parede da 15, como
    /// na 15 — marcam a entrada e escondem o corte reto onde a faixa começa.
    /// </summary>
    private static void MontarPortaDoSalao()
    {
        foreach (float dx in new[] { -0.9f, 0.9f })
        {
            Encolher(Reuso($"Hall_Door_Column_{dx:0.0}", InicioDoSalao + dx, TopoDoChao - 0.05f,
                Ithaca + "Architecture/ithaca_column_01.png", -28), 1.15f);
        }
    }

    // ---------------------------------------------------------------- a prova

    /// <summary>
    /// Um machado sobre cada <see cref="AxeRing"/>. A posição sai do anel, não de um número
    /// escrito aqui: se o level design mover um anel, o machado vai junto. Ordem −3: atrás do
    /// Odisseu, que passa pelo vão como a flecha.
    /// </summary>
    private static void MontarMachados()
    {
        foreach (AxeRing anel in Object.FindObjectsByType<AxeRing>(FindObjectsSortMode.None).OrderBy(a => a.transform.position.x))
        {
            float x = anel.transform.position.x;
            Novo($"Axe_{anel.name}", x, TopoDoChao, Raiz + "Props/final_axe_post.png", -3);
        }
    }

    // ---------------------------------------------------------------- salão

    /// <summary>
    /// O mégaron: a lareira no centro do salão, entre os anéis 18 e 26 — a lareira do mégaron era
    /// literalmente o centro da casa grega —; o tear de Penélope entre 26 e 34; o trono no fim,
    /// depois do objetivo.
    ///
    /// A lareira são DUAS peças no mesmo canvas e na mesma posição: base (ordem −6) e chama
    /// (ordem −5). A chama é um objeto próprio para poder ser animada por código depois, sem
    /// mexer na pedra.
    /// </summary>
    private static void MontarSalao()
    {
        Novo("Hearth_Base", 22f, TopoDoChao, Raiz + "Props/final_hearth_base.png", -6);
        Novo("Hearth_Fire", 22f, TopoDoChao, Raiz + "Props/final_hearth_fire.png", -5);

        Novo("Loom_Penelope", 30f, TopoDoChao, Raiz + "Props/final_loom_penelope.png", -6);

        foreach (float x in new[] { 3f, 39.8f })
        {
            Encolher(Reuso($"Hall_Brazier_{x:0.0}", x, TopoDoChao, ItacaReturn + "Props/itaca_ret_brazier.png", -7), 0.55f);
        }

        // O trono da 15, sem as lanças dos pretendentes em volta: voltou a ser de Odisseu. Fica
        // depois do objetivo e dentro do alcance da câmera (44), e não em cima dele.
        GameObject alvo = GameObject.Find("LevelGoal");
        float xAlvo = alvo != null ? alvo.transform.position.x : 38f;
        Encolher(Reuso("Throne", xAlvo + 4f, TopoDoChao, Pretendentes + "GreatHall/pret_throne.png", -8), 0.62f);
    }

    /// <summary>
    /// A luz do sol dentro do salão.
    ///
    /// - Fachos oblíquos (ordem −25: à frente da parede, atrás de tudo que se pisa ou se luta) nos
    ///   intervalos entre os anéis.
    /// - Um véu de ouro pálido que CLAREIA em direção ao fim — o inverso do véu de fogo da 15.
    ///   Sprite de 256×8 com alpha por coluna, esticado pelo transform; malha FullRect pelo mesmo
    ///   motivo do véu da 15 (a ponta esquerda é alpha zero).
    /// </summary>
    private static void MontarLuzDaManha()
    {
        foreach (float x in new[] { 6f, 14f, 22f, 30f, 38f })
        {
            Novo($"Dawn_Shaft_{x:0}", x, TopoDoChao, Raiz + "VFX/final_dawn_shaft.png", -25);
        }

        Sprite gradiente = Arte(Raiz + "VFX/final_dawn_gradient.png");
        if (gradiente == null) { return; }

        float x0 = CameraMin - 12f, ate = AlcanceDaCamera + 6f;
        var veu = new GameObject("DawnVeil");
        veu.transform.SetParent(cenario, false);
        veu.transform.position = new Vector3((x0 + ate) * 0.5f, -16f, 0f);
        veu.transform.localScale = new Vector3((ate - x0) / gradiente.bounds.size.x, 40f / gradiente.bounds.size.y, 1f);

        var sr = veu.AddComponent<SpriteRenderer>();
        sr.sprite = gradiente;
        sr.sortingOrder = 20;
        novos++;
    }

    // ---------------------------------------------------------------- primeiro plano

    /// <summary>
    /// Três peças, só nas pontas: pedras e coluna quebrada à esquerda do início (o Odisseu nasce em
    /// x=−12, à direita delas) e a coluna à direita do trono. Nenhuma fica entre o início e o
    /// objetivo, onde estão os anéis.
    /// </summary>
    private static void MontarPrimeiroPlano()
    {
        foreach (var (nome, x, arte) in new[]
        {
            ("FG_Stones_Start", -15.4f, "Foreground/final_fg_stones.png"),
            ("FG_Column_Start", -17.6f, "Foreground/final_fg_column.png"),
            ("FG_Column_End", 43.4f, "Foreground/final_fg_column.png"),
        })
        {
            GameObject go = Novo(nome, x, TopoDoChao - 0.25f, Raiz + arte, 30);
            if (go != null) { go.GetComponent<SpriteRenderer>().color = ContraLuz; }
        }
    }

    // ---------------------------------------------------------------- capturas

    /// <summary>
    /// Capturas para o relatório, com a câmera nos pontos pedidos pelo briefing. Precisa de GPU:
    /// rodar SEM -nographics.
    /// </summary>
    public static void Shots()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        const string pasta = "Docs/Environment/Fase16/_capturas/";
        System.IO.Directory.CreateDirectory(pasta);
        foreach (var (nome, x) in new[]
        {
            ("01_inicio", -7.3f), ("02_arco", -4.4f), ("03_machados", 14f),
            ("04_lareira", 22f), ("05_tear", 30f), ("06_fim", 33.3f),
        })
        {
            Foto(x, 0f, 6f, pasta + nome + ".png", 960, 540);
        }
        // Composição completa: uma câmera larga sobre a fase inteira.
        Foto((-18f + 44f) * 0.5f, 1f, 9f, pasta + "07_composicao.png", 1920, 540);
        if (Application.isBatchMode) { EditorApplication.Exit(0); }
    }

    private static void Foto(float x, float y, float tamanho, string arquivo, int w, int h)
    {
        var go = new GameObject("_Shot");
        var cam = go.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = tamanho;
        cam.transform.position = new Vector3(x, y, -10f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.magenta;   // buraco no cenário aparece, em vez de sumir
        var rt = new RenderTexture(w, h, 24);
        cam.targetTexture = rt;
        cam.Render();
        RenderTexture.active = rt;
        var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        tex.Apply();
        System.IO.File.WriteAllBytes(arquivo, tex.EncodeToPNG());
        RenderTexture.active = null;
        cam.targetTexture = null;
        Object.DestroyImmediate(rt);
        Object.DestroyImmediate(tex);
        Object.DestroyImmediate(go);
    }

    // ---------------------------------------------------------------- utilidades

    private static GameObject Reuso(string nome, float x, float y, string caminho, int ordem)
    {
        GameObject go = Prop(nome, x, y, caminho, ordem);
        if (go != null) { reusados++; }
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

        const float larguraDaTela = 22f;
        float camInicio = Camera.main != null ? Camera.main.transform.position.x : 0f;
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

    private static Sprite Arte(string caminho)
    {
        Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(caminho);
        if (s == null) { Debug.LogWarning("[Final] sprite não encontrado: " + caminho); }
        return s;
    }

    private static Color Cor(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color c);
        return c;
    }
}
