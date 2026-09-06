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
