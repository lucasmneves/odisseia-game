using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Odisseia.Systems;

/// <summary>
/// Veste a fase 06 com o cenário de Eolo:
///
/// Unity.exe -batchmode -quit -nographics -projectPath . -executeMethod EoloSceneDresser.Run
///
/// Mesmo desenho dos vestidores das fases 02 a 05 — não é sistema novo.
///
/// O que Eolo tem de próprio são duas coisas:
///
/// **As plataformas são ilhas FLUTUANTES.** Cada <c>Floor_*</c> ganha, além da faixa de chão,
/// uma faixa de rocha esfarrapada por baixo — é ela que diz que aquilo está no ar. Sem isso a
/// fase parece um terreno comum com o fundo azul.
///
/// **O vento se move.** Duas camadas de <see cref="ScrollingLayer"/> deslizam por tempo, e não
/// por câmera: uma de linhas de vento e uma de folhas. É o que o briefing pede como
/// implementação simples e performática no lugar de um sistema de partículas que não existe.
/// </summary>
public static class EoloSceneDresser
{
    private const string ScenePath = "Assets/Scenes/Levels/Level_06_Eolo.unity";
    private const string Raiz = "Assets/Art/Environments/Eolo/";
    private const string RaizDoCenario = "EoloScenery";

    private const float TopoDoChao = -2f;

    private const float CameraInicial = 0f;
    /// <summary>Meio do passeio: o jogador vai de x=-14 a x=46.</summary>
    private const float CameraMeio = 16f;
    private const float CameraCurso = 60f;

    private static Transform cenario;

    [MenuItem("Odisseia/Vestir Eolo")]
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
            Debug.LogError("[Eolo] nenhum Floor_* encontrado — cenário não montado");
            return false;
        }

        DesligarPlaceholders();
        MontarCeu();
        VestirIlhas(chaos);
        VestirPlataformaMovel();
        PovoarTravessia(chaos);
        MontarPalacio(chaos);
        VestirInterativos();
        MontarVento();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[Eolo] cenário montado: {cenario.childCount} objetos sob {RaizDoCenario}");
        return true;
    }

    private static void DesligarPlaceholders()
    {
        string[] prefixos = { "Floor_", "MovingPlatform_", "Rubble", "Sky_Background", "WindZone_",
            "WindBagPickup", "Mast", "Sail", "LevelGoal" };
        foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (go == null || !prefixos.Any(pre => go.name.StartsWith(pre))) { continue; }
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null) { sr.enabled = false; }
        }
    }

    // ---------------------------------------------------------------- céu

    private static void MontarCeu()
    {
        // Cobertura na cor mais PROFUNDA do céu, e não na mais clara: aqui o degradê escurece
        // para cima, então o que falta cobrir acima da faixa é a parte escura.
        Bloco("Sky_Fill", 16f, 14f, 130f, 30f, Cor("#253337"), -60);
        // Abaixo das ilhas não há chão nenhum: é ar até embaixo. O azul continua.
        Bloco("Air_Fill", 16f, -12f, 130f, 16f, Cor("#6480a0"), -58);

        Camada("BG_Sky", "Background/eolo_sky_high.png", 1.00f, -50, -4f, false);
        Camada("BG_Clouds_Far", "Background/eolo_clouds_far.png", 0.86f, -46, 1.5f, false);
        Camada("BG_Clouds_Near", "Background/eolo_clouds_near.png", 0.62f, -42, -4.5f, false);
    }

    // ---------------------------------------------------------------- ilhas

    /// <summary>
    /// Cada trecho de chão vira uma ilha: faixa de grama e rocha em cima, rocha esfarrapada
    /// pendurada embaixo. A largura sai do colisor, então mover um trecho no level design move
    /// a ilha junto.
    /// </summary>
    private static void VestirIlhas(List<GameObject> chaos)
    {
        foreach (GameObject go in chaos)
        {
            Bounds b = Caixa(go);
            Faixa($"Island_{go.name}", b.center.x, b.max.y, b.size.x,
                "Gameplay/eolo_tiles_island.png", -10);

            // A base esfarrapada é o que faz a plataforma ler como FLUTUANTE. Sem ela, uma
            // faixa de grama sobre o azul lê como um terreno qualquer com o fundo errado.
            Sprite baixo = Arte("Gameplay/eolo_island_underside.png");
            if (baixo == null) { continue; }
            float topoDaBase = b.max.y - Arte("Gameplay/eolo_tiles_island.png").bounds.size.y;
            Faixa($"Under_{go.name}", b.center.x, topoDaBase, b.size.x,
                "Gameplay/eolo_island_underside.png", -11);
        }
    }

    /// <summary>
    /// A plataforma móvel atravessa o vão de 7,5 un. Ela ganha uma ilha pequena, e não uma
    /// faixa: uma faixa esticada num objeto que se move denunciaria a costura em movimento.
    /// </summary>
    private static void VestirPlataformaMovel()
    {
        GameObject plat = GameObject.Find("MovingPlatform_Gap");
        if (plat == null) { return; }

        Bounds b = Caixa(plat);
        Sprite arte = Arte("Gameplay/eolo_small_island.png");
        if (arte == null) { return; }

        // Filha da plataforma, para acompanhar o movimento dela. A escala é uniforme e sai da
        // LARGURA: forçar a altura achataria a base esfarrapada, que é o que a identifica.
        var go = new GameObject("Art_MovingPlatform");
        go.transform.SetParent(plat.transform, false);
        float k = b.size.x / arte.bounds.size.x;
        go.transform.localScale = new Vector3(k, k, 1f);
        go.transform.position = new Vector3(b.center.x, b.max.y - arte.bounds.size.y * k * 0.28f, 0f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = arte;
        sr.sortingOrder = -3;
    }

    /// <summary>Ciprestes, ponte e ruínas ao longo da travessia — antes do palácio.</summary>
    private static void PovoarTravessia(List<GameObject> chaos)
    {
        float xPalacio = chaos.Max(g => Caixa(g).min.x);

        foreach (GameObject go in chaos)
        {
            Bounds b = Caixa(go);
            if (b.min.x >= xPalacio) { continue; }

            Prop($"Cypress_{go.name}", b.min.x + 1.6f, TopoDoChao, "Props/eolo_cypress.png", -8);
            if (b.size.x > 8f)
            {
                Prop($"Cypress_{go.name}_b", b.max.x - 2.2f, TopoDoChao, "Props/eolo_cypress.png", -8);
            }
        }

        // Ponte de corda sobre o vão que a plataforma móvel atravessa: ela não é caminho — é
        // o que explica visualmente que ali existe um vão para atravessar.
        GameObject antes = chaos.OrderBy(g => Caixa(g).max.x).FirstOrDefault(g => Caixa(g).max.x > 10f);
        if (antes != null)
        {
            Bounds b = Caixa(antes);
            GameObject ponte = Prop("Rope_Bridge", b.max.x + 3.5f, TopoDoChao + 2.2f,
                "Gameplay/eolo_rope_bridge.png", -9);
            if (ponte != null) { ponte.transform.localScale = new Vector3(1.2f, 1f, 1f); }
        }

        // Ruína: o altar de Cícones reaproveitado. O briefing pede "pequenas ruínas" e proíbe
        // duplicar o que já existe — e uma estrutura grega antiga serve às duas fases.
        GameObject ruina = GameObject.Find("Rubble");
        if (ruina != null && ruina.GetComponent<Collider2D>() != null)
        {
            Bounds b = Caixa(ruina);
            GameObject arte = Prop("Art_Rubble", b.center.x, b.min.y,
                "Assets/Art/Environments/Cicones/Architecture/cicones_shrine_landmark.png", -2);
            EscalarPara(arte, b);
        }
    }

    /// <summary>
    /// §"PALÁCIO DE EOLO". Montado de peças modulares — degraus, colunas, entablamento, porta,
    /// braseiros, estátuas, estandartes — e não como uma fachada única. O vão central fica
    /// livre: é onde Éolo, o Odisseu e o diálogo entram depois.
    /// </summary>
    private static void MontarPalacio(List<GameObject> chaos)
    {
        GameObject terraco = chaos.OrderByDescending(g => Caixa(g).min.x).FirstOrDefault();
        if (terraco == null) { return; }

        Bounds b = Caixa(terraco);
        float baseX = b.min.x + 2.5f;
        float larguraFachada = Mathf.Min(13f, b.size.x - 4f);
        float centro = baseX + larguraFachada * 0.5f;

        // ESCALA. A coluna nasce com 7,47 un e a câmera mostra 10: em tamanho nativo ela sozinha
        // ocupava três quartos da altura da tela, o entablamento ficava acima do quadro e a
        // fachada nunca aparecia inteira. A 0,52 o conjunto — degraus, coluna e entablamento —
        // fecha em 6,4 un e cabe com folga, que é o que faz o palácio ler como palácio.
        const float k = 0.52f;

        Sprite col = Arte("Palace/eolo_column.png");
        Sprite deg = Arte("Palace/eolo_steps.png");
        float alturaColuna = (col != null ? col.bounds.size.y : 7.5f) * k;
        float alturaDegrau = (deg != null ? deg.bounds.size.y : 3f) * k;
        float pisoDoPortico = TopoDoChao + alturaDegrau;

        FaixaEscalada("Palace_Steps", centro, pisoDoPortico, larguraFachada,
            "Palace/eolo_steps.png", -14, k);

        // Cinco posições, e a do meio fica VAZIA: é onde a porta entra, e é onde Éolo, o
        // Odisseu e o diálogo cabem depois sem nada na frente.
        for (int i = 0; i < 5; i++)
        {
            if (i == 2) { continue; }
            float x = baseX + larguraFachada * (i + 0.5f) / 5f;
            GameObject c = Prop($"Palace_Column_{i}", x, pisoDoPortico, "Palace/eolo_column.png", -13);
            if (c != null) { c.transform.localScale = new Vector3(k, k, 1f); }
        }

        GameObject porta = Prop("Palace_Door", centro, pisoDoPortico, "Palace/eolo_bronze_door.png", -15);
        if (porta != null) { porta.transform.localScale = new Vector3(k, k, 1f); }

        // Entablamento em UMA peca, na largura nativa do sprite. Ladrilhado ele repetia a
        // peca inteira — cornija, friso e tudo — e a viga aparecia quebrada em blocos soltos
        // sobre cada par de colunas, em vez de atravessar a fachada.
        {
            Sprite ent = Arte("Palace/eolo_entablature.png");
            if (ent != null)
            {
                float ke = larguraFachada / ent.bounds.size.x;
                GameObject e = Prop("Palace_Entablature", centro,
                    pisoDoPortico + alturaColuna + 0.2f, "Palace/eolo_entablature.png", -12);
                if (e != null) { e.transform.localScale = new Vector3(ke, ke, 1f); }
            }
        }

        foreach (var par in new[] { (-0.8f, "L"), (larguraFachada + 0.8f, "R") })
        {
            GameObject br = Prop($"Palace_Brazier_{par.Item2}", baseX + par.Item1, TopoDoChao,
                "Palace/eolo_brazier.png", -4);
            if (br != null) { br.transform.localScale = new Vector3(0.62f, 0.62f, 1f); }

            GameObject st = Prop($"Palace_Statue_{par.Item2}",
                baseX + par.Item1 + (par.Item2 == "L" ? -1.8f : 1.8f), TopoDoChao,
                "Palace/eolo_statue.png", -6);
            if (st != null) { st.transform.localScale = new Vector3(0.6f, 0.6f, 1f); }
        }

        // Estandartes no entablamento: são eles que mostram o vento no palácio, onde não há
        // vegetação para inclinar.
        for (int i = 0; i < 2; i++)
        {
            float x = baseX + larguraFachada * (i == 0 ? 0.20f : 0.80f);
            GameObject ban = Prop($"Palace_Banner_{i}", x, pisoDoPortico + alturaColuna - 0.2f,
                "Props/eolo_banner.png", -11);
            if (ban != null) { ban.transform.localScale = new Vector3(0.7f, 0.7f, 1f); }
        }
    }

    /// <summary>
    /// Os dois objetos de gameplay que ainda desenhavam retângulo: o odre dos ventos e o
    /// marcador de saída.
    ///
    /// O odre é o item central da fase e ganhou arte própria. O marcador de saída **reaproveita
    /// o mastro de Cytera** em vez de gerar outro: a saída de Eolo é o embarque, e o navio já
    /// foi desenhado uma vez — o briefing pede para reutilizar em vez de duplicar.
    /// </summary>
    private static void VestirInterativos()
    {
        GameObject odre = GameObject.Find("WindBagPickup");
        if (odre != null && odre.GetComponent<Collider2D>() != null)
        {
            Bounds b = Caixa(odre);
            GameObject arte = Prop("Art_WindBag", b.center.x, b.min.y, "Props/eolo_windbag.png", -1);
            // O colisor do pickup tem 0,5 un — é a caixa de coleta, não o tamanho do objeto.
            // Escalar por ele deixaria o odre do tamanho de uma moeda.
            // Escala 1 desde 2026-09-29: o odre foi refeito em tamanho nativo (26x26 px = 0,61 un,
            // Docs/Characters/Fase06/AEOLUS_CAST.md). O antigo tinha 144 px encolhidos a 0,34 —
            // o triplo da densidade do projeto, e a corda se partia na tela.
            if (arte != null) { arte.transform.localScale = Vector3.one; }
        }

        GameObject saida = GameObject.Find("LevelGoal");
        if (saida != null)
        {
            GameObject arte = Prop("Art_Goal_Ship", saida.transform.position.x, TopoDoChao,
                "Assets/Art/Environments/Cytera/Ship/cytera_mast_sail.png", -7);
            if (arte != null) { arte.transform.localScale = new Vector3(0.8f, 0.8f, 1f); }
        }
    }

    /// <summary>
    /// Vento em duas camadas que deslizam no TEMPO, com velocidades diferentes. Uma só leria
    /// como textura colada na tela; duas, a velocidades distintas, leem como ar em movimento
    /// entre a câmera e o cenário.
    ///
    /// Sem sistema de partículas: o projeto não tem um, e o briefing pede a implementação mais
    /// simples que funcione. Duas faixas custam dois transforms por quadro.
    /// </summary>
    private static void MontarVento()
    {
        GameObject linhas = Camada("Wind_Lines", "Wind/eolo_wind_lines.png", 0.45f, -1, -3f, true);
        Deslizar(linhas, -3.5f);

        GameObject folhas = Camada("Wind_Motes", "Wind/eolo_wind_motes.png", 0.10f, 13, -3f, true);
        Deslizar(folhas, -6.5f);
    }

    private static void Deslizar(GameObject go, float velocidade)
    {
        if (go == null) { return; }
        var sc = go.AddComponent<ScrollingLayer>();
        var so = new SerializedObject(sc);
        so.FindProperty("speedX").floatValue = velocidade;
        so.ApplyModifiedProperties();
    }

    // ---------------------------------------------------------------- utilidades

    private static GameObject Camada(string nome, string caminho, float fator, int ordem,
        float baseY, bool alto)
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
        // Altura NATIVA na horizontal; as camadas de vento repetem também na vertical, porque
        // vento preenche o ar todo e não só uma faixa.
        sr.size = new Vector2(deslize + larguraDaTela * 2f,
            alto ? arte.bounds.size.y * 3f : arte.bounds.size.y);

        var parallax = go.AddComponent<ParallaxLayer>();
        var so = new SerializedObject(parallax);
        so.FindProperty("parallaxFactor").floatValue = fator;
        so.ApplyModifiedProperties();
        return go;
    }

    /// <summary>
    /// Faixa que ladrilha com o tile REDUZIDO. Passar só a altura menor esticaria o desenho;
    /// o que encolhe é o ladrilho, nos dois eixos, para o degrau do palácio continuar sendo um
    /// degrau proporcional e não um degrau achatado.
    /// </summary>
    private static void FaixaEscalada(string nome, float centroX, float topoY, float largura,
        string caminho, int ordem, float k)
    {
        Sprite arte = Arte(caminho);
        if (arte == null) { return; }

        float altura = arte.bounds.size.y * k;
        var go = new GameObject(nome);
        go.transform.SetParent(cenario, false);
        go.transform.position = new Vector3(centroX, topoY - altura, 0.5f);
        go.transform.localScale = new Vector3(k, k, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = arte;
        sr.sortingOrder = ordem;
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.tileMode = SpriteTileMode.Continuous;
        // size é em unidades LOCAIS: dividir pela escala mantém a largura final em mundo.
        sr.size = new Vector2(largura / k, arte.bounds.size.y);
    }

    private static void Faixa(string nome, float centroX, float topoY, float largura,
        string caminho, int ordem)
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

    private static IEnumerable<GameObject> PorPrefixo(string prefixo) =>
        Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
            .Where(g => g != null && g.name.StartsWith(prefixo) && g.GetComponent<Collider2D>() != null)
            .OrderBy(g => g.transform.position.x);

    private static Bounds Caixa(GameObject go) => go.GetComponent<Collider2D>().bounds;

    private static void EscalarPara(GameObject go, Bounds alvo)
    {
        if (go == null) { return; }
        Sprite s = go.GetComponent<SpriteRenderer>().sprite;
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

    /// <summary>Aceita caminho relativo à pasta de Eolo ou um caminho completo de Assets, que é
    /// como o altar reaproveitado de Cícones entra.</summary>
    private static Sprite Arte(string relativo)
    {
        string caminho = relativo.StartsWith("Assets/") ? relativo : Raiz + relativo;
        Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(caminho);
        if (s == null) { Debug.LogWarning("[Eolo] sprite não encontrado: " + caminho); }
        return s;
    }

    private static Color Cor(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color c);
        return c;
    }
}
