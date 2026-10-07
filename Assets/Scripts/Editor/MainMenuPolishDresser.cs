using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Odisseia.UI;

/// <summary>
/// Polish do menu principal (Etapa 13B.5), só com o que a cena já tem:
///
/// Unity.exe -batchmode -nographics -quit -projectPath . -executeMethod MainMenuPolishDresser.Run
///
/// - Fundo na proporção da arte (3:2): esticado para 16:9 o logo e o Odisseu ficavam 18% mais largos. AspectRatioFitter
///   (FitInParent) e faixas laterais na cor da moldura pintada (#08131E), que a arte já tem nas quatro bordas.
/// - Borda dourada nos quatro botões (Outline; a cor por estado fica com o MenuButton, como o resto do visual dele).
/// - Marcador "►" dourado logo antes do rótulo do item em foco (o focusMarker do MenuButton, que não estava ligado), e sem o
///   empurrão para a direita: com o marcador, ele só tirava a coluna do alinhamento com o logo.
///
/// Idempotente. Não mexe em textos, ordem, navegação, cenas nem save.
/// </summary>
public static class MainMenuPolishDresser
{
    private const string Cena = "Assets/Scenes/Menu/MainMenu.unity";
    private static readonly Color Moldura = new Color32(8, 19, 30, 255);
    private static readonly string[] Botoes = { "ContinueButton", "NewGameButton", "LevelSelectButton", "SettingsButton" };

    public static void Run()
    {
        bool ok = Executar();
        if (Application.isBatchMode) { EditorApplication.Exit(ok ? 0 : 1); }
    }

    [MenuItem("Odisseia/Polir menu principal")]
    public static void Menu() => Executar();

    private static bool Executar()
    {
        var cena = EditorSceneManager.OpenScene(Cena, OpenSceneMode.Single);
        var fundo = GameObject.Find("Background");
        if (fundo == null) { Debug.LogError("[Menu] Background ausente"); return false; }
        var arte = fundo.GetComponent<Image>();
        Transform canvas = fundo.transform.parent;

        // Faixas: atrás do fundo, tela inteira, na cor da moldura.
        Transform faixa = canvas.Find("Letterbox");
        if (faixa == null)
        {
            var go = new GameObject("Letterbox", typeof(RectTransform), typeof(Image));
            faixa = go.transform;
            faixa.SetParent(canvas, false);
        }
        // Primeiro filho = desenhado antes de tudo. (Pôr no índice do fundo empurrava a faixa para DEPOIS dele numa
        // segunda execução, e ela cobria a arte e os botões — os botões são filhos do fundo.)
        faixa.SetSiblingIndex(0);
        var r = (RectTransform)faixa;
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
        var img = faixa.GetComponent<Image>();
        img.color = Moldura;
        img.raycastTarget = false;

        var fitter = fundo.GetComponent<AspectRatioFitter>() ?? fundo.AddComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        fitter.aspectRatio = arte.sprite.rect.width / arte.sprite.rect.height;
        arte.preserveAspect = true;

        // Canvas no modo original (largura/altura 0,5). Uma tentativa com Shrink na 13B.7 foi desfeita: o "►" em cima do
        // rótulo se resolve no MenuButton (PlaceMarker), em qualquer resolução.
        var escala = canvas.GetComponent<CanvasScaler>();
        if (escala != null) { escala.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight; }

        foreach (string nome in Botoes)
        {
            var botao = GameObject.Find(nome);
            if (botao == null) { Debug.LogError("[Menu] " + nome + " ausente"); return false; }
            PolirBotao(botao);
        }

        EditorSceneManager.MarkSceneDirty(cena);
        EditorSceneManager.SaveScene(cena);
        Debug.Log($"[Menu] fundo {fitter.aspectRatio:0.000} (FitInParent) com faixas; borda e marcador nos {Botoes.Length} botões");
        return true;
    }

    /// <summary>
    /// O visual de botão do menu principal, para qualquer tela (também o Final, EndingPolishDresser): MenuButton (se
    /// faltar), borda dourada por Outline e o marcador "►" na borda esquerda, ligado só no foco; sem empurrão lateral.
    /// </summary>
    public static void PolirBotao(GameObject botao)
    {
        if (botao.GetComponent<MenuButton>() == null) { botao.AddComponent<MenuButton>(); }

        {
            var borda = botao.GetComponent<Outline>() ?? botao.AddComponent<Outline>();
            borda.effectColor = UITheme.ButtonBorder;
            borda.effectDistance = new Vector2(2f, -2f);
            borda.useGraphicAlpha = true;

            Text rotulo = botao.GetComponentInChildren<Text>(true);
            Transform marca = botao.transform.Find("FocusMarker");
            if (marca == null)
            {
                var go = new GameObject("FocusMarker", typeof(RectTransform), typeof(Text));
                marca = go.transform;
                marca.SetParent(botao.transform, false);
            }
            var mr = (RectTransform)marca;
            // Encostado à esquerda do rótulo: o MenuButton o reposiciona no foco pela largura do texto (PlaceMarker).
            mr.anchorMin = mr.anchorMax = new Vector2(0.5f, 0.5f);
            mr.pivot = new Vector2(1f, 0.5f);
            mr.sizeDelta = new Vector2(40f, 40f);
            mr.anchoredPosition = Vector2.zero;
            var t = marca.GetComponent<Text>();
            t.text = "►";
            t.font = rotulo != null ? rotulo.font : t.font;
            t.fontSize = rotulo != null ? rotulo.fontSize : 26;
            t.alignment = TextAnchor.MiddleRight;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.color = UITheme.TextAccent;
            t.raycastTarget = false;
            marca.gameObject.SetActive(false);

            var so = new SerializedObject(botao.GetComponent<MenuButton>());
            so.FindProperty("focusMarker").objectReferenceValue = marca.gameObject;
            so.FindProperty("highlightShift").floatValue = 0f;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
