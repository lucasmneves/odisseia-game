using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Polish da tela final (Etapa 13B.7), só com o que a cena já tem:
///
/// Unity.exe -batchmode -nographics -quit -projectPath . -executeMethod EndingPolishDresser.Run
///
/// - "Jogar novamente" e "Voltar ao menu" com o mesmo visual de botão do menu principal (MenuButton, borda dourada e
///   marcador "►" no foco — MainMenuPolishDresser.PolirBotao). Antes eram Buttons com troca de cor, e o foco quase não
///   se distinguia.
/// - Fundo da câmera no marrom escuro das paredes do quarto (#2A1D16): a arte é 16:9 (no WebGL, tela cheia); em telas mais
///   largas as faixas laterais saíam no azul padrão (#314D79), que destoava do quarto.
/// - Canvas escalando pela altura, como a câmera do quarto (em telas largas os botões cobriam o casal).
///
/// Idempotente. Não mexe em textos, cenário, câmera (posição e tamanho), fluxo nem save.
/// </summary>
public static class EndingPolishDresser
{
    private const string Cena = "Assets/Scenes/Menu/Ending.unity";
    private static readonly Color Faixa = new Color32(42, 29, 22, 255);

    public static void Run()
    {
        bool ok = Executar();
        if (Application.isBatchMode) { EditorApplication.Exit(ok ? 0 : 1); }
    }

    [MenuItem("Odisseia/Polir tela final")]
    public static void Menu() => Executar();

    private static bool Executar()
    {
        var cena = EditorSceneManager.OpenScene(Cena, OpenSceneMode.Single);
        foreach (string nome in new[] { "PlayAgainButton", "BackToMenuButton" })
        {
            var botao = GameObject.Find(nome);
            if (botao == null) { Debug.LogError("[Final] " + nome + " ausente"); return false; }
            MainMenuPolishDresser.PolirBotao(botao);

            // 240 → 290 (posições ±155, vão de 20 mantido): "Jogar novamente" ocupava o botão inteiro e o "►" do foco caía
            // em cima do "J". Medido no EndingProbe (marcador × glifos do rótulo, nos dois idiomas).
            var r = (RectTransform)botao.transform;
            r.sizeDelta = new Vector2(290f, r.sizeDelta.y);
            r.anchoredPosition = new Vector2(nome == "PlayAgainButton" ? -155f : 155f, r.anchoredPosition.y);
        }

        Camera cam = Camera.main;
        if (cam == null) { Debug.LogError("[Final] sem câmera"); return false; }
        cam.backgroundColor = Faixa;

        // A câmera do quarto segue a ALTURA da tela (tamanho fixo); o Canvas seguia a LARGURA (match 0). Em telas mais
        // largas que 16:9 os botões cresciam mais que o cenário e "Jogar novamente" cobria o casal (2340×1080). Com match 1
        // os dois escalam juntos; em 16:9 (o WebGL) as duas medidas dão o mesmo fator e nada muda.
        var escala = GameObject.Find("Ending Canvas")?.GetComponent<UnityEngine.UI.CanvasScaler>();
        if (escala == null) { Debug.LogError("[Final] Ending Canvas sem CanvasScaler"); return false; }
        escala.matchWidthOrHeight = 1f;

        EditorSceneManager.MarkSceneDirty(cena);
        EditorSceneManager.SaveScene(cena);
        Debug.Log("[Final] botões com o visual do menu; fundo da câmera #2A1D16; Canvas pela altura");
        return true;
    }
}
