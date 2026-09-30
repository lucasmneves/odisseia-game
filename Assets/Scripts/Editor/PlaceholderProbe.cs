using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Lista o que ainda desenha o quadrado placeholder na fase:
///
/// Unity.exe -batchmode -quit -projectPath . -executeMethod PlaceholderProbe.Run
///
/// Existe porque "não tem mais retângulo colorido" é fácil de afirmar e difícil de conferir a
/// olho: a fase tem quase trezentos objetos espalhados por 349 unidades, e uma captura mostra
/// dezoito de cada vez. Contar referências ao sprite no arquivo da cena também engana — os
/// colisores de chão e as paredes de limite continuam com o sprite atribuído e o renderer
/// desligado, de propósito.
///
/// O que conta é o que o jogador vê. Por isso a probe separa três casos: visível agora,
/// escondido mas que ainda vai aparecer (o objeto nasce desligado e um ato o liga), e
/// desenho desligado de propósito.
/// </summary>
public static class PlaceholderProbe
{
    /// <summary>Cena padrao. Outra entra por <c>-probeScene</c>, para o probe servir todas as
    /// fases em vez de virar um por fase.</summary>
    private const string ScenePadrao = "Assets/Scenes/Levels/Level_01_Itaca_Prologue.unity";

    private static string ScenePath
    {
        get
        {
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "-probeScene") { return args[i + 1]; }
            }
            return ScenePadrao;
        }
    }
    private const string PlaceholderPath = "Assets/Art/Player/PlaceholderSquare.png";

    /// <summary>
    /// Objetos que usam o quadrado como PREENCHIMENTO CHAPADO, não como arte que falta.
    ///
    /// A distinção não dá para tirar do sprite: um retângulo de cor é a ferramenta certa para
    /// um fundo de cobertura, e a errada para uma casa. Fica aqui, com o motivo de cada um,
    /// porque uma lista com motivo escrito é revisável — um número solto na saída não é.
    /// </summary>
    private static readonly Dictionary<string, string> Justificados = new Dictionary<string, string>
    {
        ["World/Background/Sky"] = "fundo de cobertura, na cor do topo do degradê do céu; garante que nenhuma camada de parallax deixe buraco",

        // Cicones. Sao preenchimentos CHAPADOS de proposito: cor solida e a ferramenta certa
        // para o que fica atras de tudo e nao pode ter textura competindo com o cenario.
        ["CiconesScenery/Sky_Fill"] = "fundo de cobertura na cor do topo do degradê do céu, para nenhuma camada de parallax deixar buraco",
        ["CiconesScenery/Valley_Fill"] = "vale escuro abaixo do horizonte; sem ele o vão entre trechos de chão mostra o céu, que clareia perto do horizonte e lê como buraco brilhante",
        ["CiconesScenery/Bank_0"] = "barranco abaixo do tile de chão, fora do alcance de leitura; textura ali competiria com o terreno jogável",
        ["CiconesScenery/Bank_1"] = "idem Bank_0",
        ["CiconesScenery/Bank_2"] = "idem Bank_0",
        // Desligado de proposito pelo CiconesSceneDresser e substituido pelas cinco camadas de
        // parallax. Fica na cena, e nao apagado, para a fase nao ficar sem fundo nenhum caso
        // o cenario seja removido.
        ["Sky_Background"] = "placeholder de céu desligado pelo CiconesSceneDresser; ninguém o liga de volta, e é isso que se espera",

        // Cytera. Mesmo papel dos de Cicones, com os valores invertidos: numa tempestade o
        // risco de vao descoberto e buraco PRETO, nao buraco brilhante.
        ["CyteraScenery/Sky_Fill"] = "cobertura acima do céu de tempestade, na cor mais escura da rampa de nuvem",
        ["CyteraScenery/Sea_Fill"] = "água escura abaixo das faixas de mar; nenhuma textura ali é vista pela câmera",

        // Ciclopes.
        ["CiclopesScenery/Sky_Fill"] = "fundo de cobertura na cor do topo do degradê do céu",
        ["CiclopesScenery/Valley_Fill"] = "vale escuro abaixo do horizonte, fora do alcance de leitura",
        ["CiclopesScenery/Cave_Backdrop"] = "rocha maciça atrás da caverna; sem ela o céu aparece acima da faixa de parede",
        ["CiclopesScenery/Cave_Veil_0"] = "véu de escurecimento progressivo da caverna: cor chapada com alpha é a ferramenta certa para isso",
        ["CiclopesScenery/Cave_Veil_1"] = "idem Cave_Veil_0",
        ["CiclopesScenery/Cave_Veil_2"] = "idem Cave_Veil_0",
        ["CiclopesScenery/Bank_Floor_1"] = "barranco abaixo do tile de chão; textura ali competiria com o terreno jogável",
        ["CiclopesScenery/Bank_Floor_2"] = "idem Bank_Floor_1",
        ["CiclopesScenery/Bank_Floor_Narrow"] = "idem Bank_Floor_1",
        ["CiclopesScenery/Bank_Floor_3_Boss"] = "idem Bank_Floor_1",
        ["CiclopesScenery/Bank_Floor_4"] = "idem Bank_Floor_1",

        // Polifemo NAO e cenario: e o chefe da fase, e o briefing desta rodada proibe gerar
        // personagens. Fica registrado aqui para o probe nao mascarar a pendencia dele.
        // Polyphemus/Body, Head e Eye saíram daqui em 2026-09-29: o chefe ganhou arte
        // (CyclopsCastDresser). Se um quadrado voltar, o probe precisa acusar.

        // O brilho da saida e uma AFFORDANCE de gameplay, nao decoracao: marcar a saida com
        // uma forma de cor chapada e o uso certo do quadrado, do mesmo jeito que os veus.
        ["ExitGlow"] = "brilho que marca a saída da fase; forma de cor chapada é a ferramenta certa para um glow",

        // Eolo.
        ["EoloScenery/Sky_Fill"] = "cobertura acima do céu; aqui o degradê escurece para cima, então a cobertura é a cor profunda",
        ["EoloScenery/Air_Fill"] = "abaixo das ilhas não há chão nenhum: é ar, e o azul continua",

        // Lestrigoes.
        ["LestrigoesScenery/Sky_Fill"] = "fundo de cobertura na cor do topo do degradê do céu",
        ["LestrigoesScenery/Valley_Fill"] = "vale escuro abaixo do horizonte, fora do alcance de leitura",
        ["LestrigoesScenery/Bank_0"] = "barranco abaixo do tile de chão; textura ali competiria com o terreno jogável",
        ["LestrigoesScenery/Bank_1"] = "idem Bank_0",
        ["LestrigoesScenery/Bank_2"] = "idem Bank_0",

        // Os gigantes NAO sao cenario: sao os inimigos da fase, e o briefing desta rodada
        // proibe gerar personagens. Ficam registrados para o probe nao mascarar a pendencia.
        // Giant/Body e Giant/Head saíram daqui em 2026-09-29: os Lestrigões ganharam arte
        // (LestrigonCastDresser). Se um quadrado voltar, o probe precisa acusar.

        // Circe.
        ["CirceScenery/Canopy_Fill"] = "cobertura acima da copa, na cor da luz filtrada; nesta fase não há céu",
        ["CirceScenery/Floor_Fill"] = "terra escura abaixo do tile de chão, fora do alcance de leitura",
        ["CirceScenery/Bank_Floor_1"] = "barranco abaixo do tile de chão; textura ali competiria com o terreno jogável",
        ["CirceScenery/Bank_Floor_2"] = "idem Bank_Floor_1",
        ["CirceScenery/Bank_Floor_3"] = "idem Bank_Floor_1",

        // Mundo dos Mortos. O briefing proibe preto absoluto, entao a cobertura desta fase e a
        // pedra mais funda da paleta (#151615) e nao #000 — e por isso ela aparece aqui, como
        // preenchimento consciente, em vez de passar batida.
        ["MortosScenery/Rock_Fill"] = "rocha macica acima e atras de tudo; sem ela o topo do quadro fica sem nada desenhado, e o briefing proibe preto absoluto",
        ["MortosScenery/Depth_Fill"] = "rocha funda abaixo da linha do rio, fora do alcance de leitura da camera",
        ["MortosScenery/River_Deep"] = "agua funda ATRAS da faixa do rio; sem ela a agua terminava numa linha reta contra a rocha preta, e um degrade chapado e a ferramenta certa para profundidade",
        ["MortosScenery/Bank_0"] = "barranco abaixo do tile de chao; textura ali competiria com o terreno jogavel",
        ["MortosScenery/Bank_1"] = "idem Bank_0",
        ["MortosScenery/Bank_2"] = "idem Bank_0",

        // Sereias.
        ["SereiasScenery/Sky_Fill"] = "cobertura acima do ceu, em #e6debf — a cor MEDIDA da primeira linha do bg_sea; qualquer outro creme deixaria uma emenda horizontal visivel",
        ["SereiasScenery/Sea_Fill"] = "agua escura abaixo da linha do chao, fora do alcance de leitura; sem ela o vao entre trechos mostra o ceu",
        ["SereiasScenery/Bank_Floor_1"] = "barranco abaixo do tile de areia, em #776657 — a cor MEDIDA da ultima linha do proprio tile, para a emenda entre os dois desaparecer",
        ["SereiasScenery/Bank_Floor_SirenZone"] = "idem Bank_Floor_1",
        ["SereiasScenery/Bank_Floor_2"] = "idem Bank_Floor_1",

        // Fase 11 — Cila e Caribdis. Só dois preenchimentos, e não há barranco: o corpo da
        // plataforma aqui é o próprio tile Wang escurecido, não um retângulo chapado.
        ["CilaCaribdisScenery/Sky_Fill"] = "cobertura acima do céu, em #847c77 — a cor MEDIDA da primeira linha de cila_bg_walls_far (274 dos 672 px dela); qualquer outro cinza deixaria uma emenda horizontal visível",
        ["CilaCaribdisScenery/Deep_Fill"] = "água funda abaixo da linha do chão, no passo Profunda da rampa Agua funda; sem ela o vão entre as ilhas mostrava o fundo da câmera em magenta",

        // Fase 12 — Gado do Sol.
        ["GadoDoSolScenery/Sky_Fill"] = "cobertura acima do céu, em #6dafd1 — a cor MEDIDA da primeira linha de gado_bg_hills (387 dos 672 px)",
        ["GadoDoSolScenery/Ground_Fill"] = "terra da ilha abaixo da linha do chão, no passo Profunda da rampa Terra; é terra e não mar, senão o pasto leria como ilha flutuante",


        // Fase 13 — Calipso.
        ["CalipsoScenery/Sky_Fill"] = "cobertura acima do céu, em #bb995e — a cor MEDIDA da primeira linha de calipso_bg_sea_horizon (846 dos 1342 px)",
        ["CalipsoScenery/Ground_Fill"] = "terra da ilha abaixo da linha do chão, no passo Profunda da rampa Madeira; é terra e não mar, senão a floresta leria como ilha flutuante",


        // Fase 14 — Itaca Return. A cobertura de ceu NAO e dourada: o topo do ceu de fim de
        // tarde e o verde-acinzentado frio que sobra acima do ouro, e usar o ouro deixaria uma
        // emenda horizontal onde a cobertura encontra a camada.
        ["ItacaReturnScenery/Sky_Fill"] = "cobertura acima do céu, em #99a593 — a cor MEDIDA da primeira linha de itaca_ret_bg_dusk (546 dos 1342 px)",
        ["ItacaReturnScenery/Ground_Fill"] = "terra abaixo da linha do chão, no passo Profunda da rampa Terra / caminho da paleta da FASE 01",


        // Fase 15 — Pretendentes.
        ["PretendentesScenery/Sky_Fill"] = "cobertura acima do céu: a cor MEDIDA da primeira linha de itaca_ret_bg_dusk (#99a593) multiplicada pelo mesmo tom de noite aplicado ao céu reusado, calculada no vestidor para não haver emenda",
        ["PretendentesScenery/Ground_Fill"] = "terra abaixo do piso, no passo Profunda de Pedra; topo em −1,9 para cobrir as falhas da fileira de superfície dos tiles de Ítaca",
        ["PretendentesScenery/Hall_Ceiling"] = "teto escuro do salão acima da parede, em Noite Profunda; o salão é interior, e sem ele o céu noturno aparecia por cima com a câmera alta",        ["LevelGoal/MagicGlow"] = "brilho mágico que marca a saída; forma de cor chapada é a ferramenta certa para um glow",
        ["World/Sea_Deep"] = "água profunda abaixo da faixa de oceano, no passo Profunda da rampa de Água",
        ["LevelGoal"] = "x=420, 50 un além do alcance máximo da câmera (o navio anda 16 un e a câmera trava em 360)",
        ["LevelGoal/Roof"] = "idem LevelGoal",
        ["Player/Visual/ShieldVisual"] = "inativo e sem quem ligue: a referência no PlayerShield é nula de propósito, e o OdysseusSheetProbe cobra isso",
    };

    [MenuItem("Odisseia/Listar placeholders da fase")]
    public static void Listar()
    {
        Executar();
    }

    public static void Run()
    {
        bool limpo = Executar();
        if (Application.isBatchMode)
        {
            EditorApplication.Exit(limpo ? 0 : 1);
        }
    }

    private static bool Executar()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        var placeholder = AssetDatabase.LoadAssetAtPath<Sprite>(PlaceholderPath);
        if (placeholder == null)
        {
            Debug.LogError("[Placeholder] sprite placeholder nao encontrado: " + PlaceholderPath);
            return false;
        }

        var visiveis = new List<SpriteRenderer>();
        var adormecidos = new List<SpriteRenderer>();
        var desligados = new List<SpriteRenderer>();

        foreach (SpriteRenderer sr in Object.FindObjectsByType<SpriteRenderer>(
                     FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (sr.sprite != placeholder)
            {
                continue;
            }

            if (!sr.enabled)
            {
                desligados.Add(sr);
            }
            else if (!sr.gameObject.activeInHierarchy)
            {
                adormecidos.Add(sr);
            }
            else
            {
                visiveis.Add(sr);
            }
        }

        var pendentes = visiveis.Concat(adormecidos)
            .Where(sr => !Justificados.ContainsKey(Caminho(sr.transform)))
            .ToList();

        Relatar("VISIVEL agora", visiveis.Where(sr => !Justificados.ContainsKey(Caminho(sr.transform))).ToList());
        // "Inativo" não é o mesmo que "vai aparecer": o ShieldVisual do Player está inativo e
        // ninguém o liga, porque a referência no PlayerShield é nula de propósito — a arte do
        // aspis está na folha do personagem, e o OdysseusSheetProbe cobra esse campo vazio.
        // Quem lê a lista precisa conferir o gatilho, não só o estado.
        Relatar("inativo (conferir se algo liga)",
            adormecidos.Where(sr => !Justificados.ContainsKey(Caminho(sr.transform))).ToList());

        Debug.Log($"[Placeholder] {Justificados.Count} justificados como preenchimento chapado:");
        foreach (KeyValuePair<string, string> j in Justificados)
        {
            Debug.Log($"[Placeholder]   {j.Key} — {j.Value}");
        }

        Debug.Log($"[Placeholder] {desligados.Count} com o desenho desligado de proposito " +
                  "(colisores de chao e paredes de limite)");
        Debug.Log($"[Placeholder] total bruto: {visiveis.Count} visiveis, {adormecidos.Count} inativos, " +
                  $"{desligados.Count} desligados");
        Debug.Log(pendentes.Count == 0
            ? "[Placeholder] OK — nenhum placeholder pendente na fase"
            : $"[Placeholder] {pendentes.Count} PENDENTE(S) — arte faltando");
        return pendentes.Count == 0;
    }

    private static void Relatar(string titulo, List<SpriteRenderer> lista)
    {
        if (lista.Count == 0)
        {
            Debug.Log($"[Placeholder] {titulo}: nenhum");
            return;
        }

        Debug.Log($"[Placeholder] {titulo}: {lista.Count}");
        foreach (SpriteRenderer sr in lista.OrderBy(s => s.transform.position.x))
        {
            Vector3 p = sr.transform.position;
            Vector3 e = sr.transform.lossyScale;
            Debug.Log($"[Placeholder]   {Caminho(sr.transform)}  x={p.x:0.0} y={p.y:0.0}  {e.x:0.0}x{e.y:0.0} un");
        }
    }

    private static string Caminho(Transform t)
    {
        string nome = t.name;
        for (Transform pai = t.parent; pai != null; pai = pai.parent)
        {
            nome = pai.name + "/" + nome;
        }

        return nome;
    }
}
