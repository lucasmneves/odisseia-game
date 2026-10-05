using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Odisseia.Systems;
using Odisseia.UI;

/// <summary>
/// Monta a tela de seleção de fases: lista rolável à esquerda, mapa da jornada à
/// direita.
///
/// Unity.exe -batchmode -quit -projectPath . -executeMethod LevelSelectSceneBuilder.Build
/// ou pelo menu Odisseia > Montar tela de selecao de fases.
///
/// É script de Editor, e não YAML escrito à mão, porque esta tela precisa de
/// ScrollRect, máscara, layout vertical e AspectRatioFitter — componentes com muita
/// referência interna cruzada, em que um campo esquecido não dá erro, só um resultado
/// silenciosamente errado.
///
/// Idempotente: roda quantas vezes quiser, sempre reconstruindo do zero.
/// </summary>
public static class LevelSelectSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/Menu/LevelSelect.unity";
    // O mapa em pixel art do Asset Completion (PXL-016), no lugar da ilustração pintada mapa.png (1674×940, outra
    // linguagem). Mesma proporção 16:9 — o quadro com AspectRatioFitter acompanha a arte.
    private const string MapPath = "Assets/Art/Map/map_aegean.png";

    /// <summary>Fatia da largura ocupada pela coluna da lista.</summary>
    private const float SideWidth = 0.26f;

    private const float EntryHeight = 44f;
    private const float HeaderHeight = 64f;
    private const float FooterHeight = 72f;

    [MenuItem("Odisseia/Montar tela de selecao de fases")]
    public static void Build()
    {
        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

        var controller = Object.FindAnyObjectByType<LevelSelectController>();
        if (controller == null)
        {
            Debug.LogError("[LevelSelect] LevelSelectController não encontrado na cena");
            EditorApplication.Exit(1);
            return;
        }

        var canvas = controller.GetComponent<Canvas>();
        var canvasRect = (RectTransform)canvas.transform;

        // Reconstrói do zero para a montagem não depender do que sobrou de antes.
        for (int i = canvasRect.childCount - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(canvasRect.GetChild(i).gameObject);
        }

        ConfigurarEscala(canvas);

        RectTransform lista = MontarColunaEsquerda(canvasRect, out Button voltar);
        MontarMapa(canvasRect);

        LigarController(controller, lista, voltar);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("[LevelSelect] tela montada: lista à esquerda, mapa à direita");
        EditorApplication.Exit(0);
    }

    /// <summary>
    /// Relata o formato com que cada textura de UI entrou no build. Compressao de
    /// bloco (DXT/BC) exige dimensoes multiplas de 4; quando nao sao, o Unity cai
    /// silenciosamente para RGBA sem compressao e o asset pesa dez vezes mais.
    /// </summary>
    [MenuItem("Odisseia/Relatar import das texturas de UI")]
    public static void ReportarTexturas()
    {
        string[] caminhos =
        {
            MapPath,
            "Assets/Art/Odisseia/UI/MainMenu/BG_MainMenu.png",
        };

        foreach (string caminho in caminhos)
        {
            var textura = AssetDatabase.LoadAssetAtPath<Texture2D>(caminho);
            if (textura == null)
            {
                Debug.LogError("[Texturas] nao encontrada: " + caminho);
                continue;
            }

            bool multiploDeQuatro = textura.width % 4 == 0 && textura.height % 4 == 0;
            Debug.Log($"[Texturas] {System.IO.Path.GetFileName(caminho)}  " +
                      $"{textura.width}x{textura.height}  formato={textura.format}  " +
                      $"multiplo de 4={multiploDeQuatro}");
        }

        EditorApplication.Exit(0);
    }

    private static void ConfigurarEscala(Canvas canvas)
    {
        var scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            return;
        }

        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
    }

    // ------------------------------------------------------------------ esquerda

    private static RectTransform MontarColunaEsquerda(RectTransform pai, out Button voltar)
    {
        RectTransform coluna = Criar("SidePanel", pai);
        Esticar(coluna, new Vector2(0f, 0f), new Vector2(SideWidth, 1f));
        Fundo(coluna, UITheme.PanelBackground);

        // Cabeçalho
        RectTransform cabecalho = Criar("Header", coluna);
        cabecalho.anchorMin = new Vector2(0f, 1f);
        cabecalho.anchorMax = new Vector2(1f, 1f);
        cabecalho.pivot = new Vector2(0.5f, 1f);
        cabecalho.sizeDelta = new Vector2(0f, HeaderHeight);
        cabecalho.anchoredPosition = Vector2.zero;

        Texto(cabecalho, "Title", Localization.Get("ui.levelSelect.title"), UITheme.FontHeading, UITheme.TextAccent);

        // Rodapé com o botão de voltar
        RectTransform rodape = Criar("Footer", coluna);
        rodape.anchorMin = new Vector2(0f, 0f);
        rodape.anchorMax = new Vector2(1f, 0f);
        rodape.pivot = new Vector2(0.5f, 0f);
        rodape.sizeDelta = new Vector2(0f, FooterHeight);
        rodape.anchoredPosition = Vector2.zero;

        voltar = Botao(rodape, "BackButton", Localization.Get("ui.levelSelect.back"));
        var voltarRect = (RectTransform)voltar.transform;
        voltarRect.anchorMin = voltarRect.anchorMax = new Vector2(0.5f, 0.5f);
        voltarRect.sizeDelta = new Vector2(200f, 46f);
        voltarRect.anchoredPosition = Vector2.zero;

        // Lista rolável entre os dois. São 16 etapas: a 720 de altura de referência não
        // cabem todas, e sem rolagem as últimas ficariam inalcançáveis.
        RectTransform scroll = Criar("ScrollView", coluna);
        Esticar(scroll, Vector2.zero, Vector2.one);
        scroll.offsetMax = new Vector2(0f, -HeaderHeight);
        scroll.offsetMin = new Vector2(0f, FooterHeight);

        var rect = scroll.gameObject.AddComponent<ScrollRect>();
        rect.horizontal = false;
        rect.vertical = true;
        rect.movementType = ScrollRect.MovementType.Clamped;
        rect.scrollSensitivity = 32f;

        RectTransform viewport = Criar("Viewport", scroll);
        Esticar(viewport, Vector2.zero, Vector2.one);
        viewport.gameObject.AddComponent<RectMask2D>();

        RectTransform conteudo = Criar("Content", viewport);
        conteudo.anchorMin = new Vector2(0f, 1f);
        conteudo.anchorMax = new Vector2(1f, 1f);
        conteudo.pivot = new Vector2(0.5f, 1f);
        conteudo.sizeDelta = new Vector2(0f, 0f);
        conteudo.anchoredPosition = Vector2.zero;

        var layout = conteudo.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 4f;
        layout.padding = new RectOffset(10, 10, 8, 8);
        layout.childControlWidth = true;

        // Precisa ser true: com childControlHeight desligado o grupo ignora o
        // LayoutElement.preferredHeight das entradas e usa a altura propria do
        // RectTransform, que nasce 100 — as entradas saem com o dobro da altura
        // pedida e a lista inteira fica desproporcional.
        layout.childControlHeight = true;

        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        var fitter = conteudo.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        rect.viewport = viewport;
        rect.content = conteudo;

        return conteudo;
    }

    // ------------------------------------------------------------------ direita

    private static void MontarMapa(RectTransform pai)
    {
        RectTransform painel = Criar("MapPanel", pai);
        Esticar(painel, new Vector2(SideWidth, 0f), new Vector2(1f, 1f));
        Fundo(painel, new Color(0.05f, 0.08f, 0.14f, 1f));

        Sprite mapa = AssetDatabase.LoadAssetAtPath<Sprite>(MapPath);
        if (mapa == null)
        {
            Debug.LogError("[LevelSelect] mapa não encontrado em " + MapPath);
            return;
        }

        // O quadro guarda a proporção do mapa. É ele, e não a imagem esticada, que dá
        // um espaço de coordenadas estável — é onde o marcador de posição do Odisseu
        // vai poder ser ancorado depois, em coordenadas normalizadas.
        RectTransform quadro = Criar("MapFrame", painel);
        Esticar(quadro, Vector2.zero, Vector2.one);

        var proporcao = quadro.gameObject.AddComponent<AspectRatioFitter>();
        proporcao.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        proporcao.aspectRatio = mapa.rect.width / mapa.rect.height;

        RectTransform imagem = Criar("MapImage", quadro);
        Esticar(imagem, Vector2.zero, Vector2.one);
        var image = imagem.gameObject.AddComponent<Image>();
        image.sprite = mapa;
        image.raycastTarget = false;

        // Camada vazia por enquanto: cobre exatamente o mapa, então um marcador
        // colocado aqui com âncora normalizada cai no ponto certo em qualquer tela.
        RectTransform marcadores = Criar("MarkerLayer", quadro);
        Esticar(marcadores, Vector2.zero, Vector2.one);
    }

    private static void LigarController(LevelSelectController controller, RectTransform lista, Button voltar)
    {
        var so = new SerializedObject(controller);
        so.FindProperty("listContainer").objectReferenceValue = lista;
        so.FindProperty("backButton").objectReferenceValue = voltar;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // ------------------------------------------------------------------ primitivas

    private static RectTransform Criar(string nome, Transform pai)
    {
        var go = new GameObject(nome, typeof(RectTransform));
        go.transform.SetParent(pai, false);
        return (RectTransform)go.transform;
    }

    private static void Esticar(RectTransform rect, Vector2 min, Vector2 max)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static Image Fundo(RectTransform rect, Color cor)
    {
        var image = rect.gameObject.AddComponent<Image>();
        image.color = cor;
        return image;
    }

    private static Text Texto(RectTransform pai, string nome, string conteudo, int tamanho, Color cor)
    {
        RectTransform rect = Criar(nome, pai);
        Esticar(rect, Vector2.zero, Vector2.one);

        var texto = rect.gameObject.AddComponent<Text>();
        texto.font = Odisseia.UI.UITheme.Font;
        texto.text = conteudo;
        texto.fontSize = tamanho;
        texto.color = cor;
        texto.alignment = TextAnchor.MiddleCenter;
        texto.raycastTarget = false;
        return texto;
    }

    private static Button Botao(RectTransform pai, string nome, string rotulo)
    {
        RectTransform rect = Criar(nome, pai);
        Image fundo = Fundo(rect, UITheme.ButtonNormal);

        var botao = rect.gameObject.AddComponent<Button>();
        botao.targetGraphic = fundo;

        Texto(rect, "Label", rotulo, UITheme.FontButton, UITheme.TextPrimary);
        rect.gameObject.AddComponent<MenuButton>();

        return botao;
    }
}
