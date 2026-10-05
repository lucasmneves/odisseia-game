using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Odisseia.Systems;

namespace Odisseia.UI
{
    /// <summary>
    /// Tela de configurações: áudio (geral, música, efeitos), controles e gráficos
    /// (qualidade, resolução, tela cheia).
    ///
    /// Monta o próprio Canvas em runtime e persiste entre cenas, no mesmo padrão de
    /// <see cref="OptionsMenu"/> — então serve tanto ao menu principal quanto ao pause
    /// das fases, sem editar cena nenhuma. Quem guarda e aplica os valores é o
    /// <see cref="SettingsManager"/>; esta classe só desenha e liga eventos.
    ///
    /// O remapeamento de teclas continua no <see cref="OptionsMenu"/>, que já existia e
    /// faz isso bem — aqui há só o botão que o abre.
    /// </summary>
    public class SettingsScreen : MonoBehaviour
    {
        private const float ReferenceWidth = 1280f;
        private const float ReferenceHeight = 720f;
        private const float PanelWidth = 620f;
        private const float RowHeight = 46f;

        private static SettingsScreen instance;

        private GameObject root;
        private Button closeButton;
        private float y;
        private RectTransform content;
        private RectTransform borderRect;

        private readonly List<Button> focusables = new List<Button>();

        public static bool IsOpen => instance != null && instance.root != null && instance.root.activeSelf;

        private static SettingsScreen Instance
        {
            get
            {
                if (instance == null)
                {
                    var go = new GameObject("SettingsScreen");
                    instance = go.AddComponent<SettingsScreen>();
                    DontDestroyOnLoad(go);
                    instance.Build();
                }

                return instance;
            }
        }

        public static void Open() => Instance.Show();

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            if (root == null)
            {
                Build();
            }

            // Os rótulos desta tela são resolvidos uma vez, na montagem. Trocar o
            // idioma aqui dentro exige refazê-la — inclusive porque a própria linha do
            // idioma muda de nome.
            Localization.Changed += Rebuild;
        }

        private void OnDestroy()
        {
            Localization.Changed -= Rebuild;
        }

        /// <summary>
        /// Refaz a tela no idioma novo, reabrindo se estava aberta.
        ///
        /// Adiado um frame de propósito: o evento de idioma nasce do clique num botão
        /// DESTA tela, e destruí-la no meio do próprio callback deixaria o resto do
        /// tratador mexendo em objetos já marcados para destruição.
        /// </summary>
        private void Rebuild() => StartCoroutine(RebuildRoutine());

        private System.Collections.IEnumerator RebuildRoutine()
        {
            yield return null;

            bool estavaAberta = IsOpen;

            if (root != null)
            {
                // Desativa antes de destruir: Destroy só age no fim do frame, e sem
                // isso a tela velha e a nova apareceriam sobrepostas por um instante.
                root.SetActive(false);
                Destroy(root);
            }

            root = null;
            focusables.Clear();
            closeButton = null;

            Build();

            if (estavaAberta)
            {
                Show();
            }
        }

        /// <summary>
        /// Celular deitado (~2,16:1): o Canvas de referência 1280×720 com match 0,5 fica com ~650 unidades de altura, e o
        /// painel (~700) saía cortado embaixo — o botão de fechar ficava fora da tela. Quando não cabe, o painel e a moldura
        /// encolhem por escala até caber, com folga; em tela que comporta, nada muda. Refeito a cada quadro aberto, porque o
        /// aparelho pode girar com a tela aberta.
        /// </summary>
        private void AjustarAoTamanhoDaTela()
        {
            float disponivel = ((RectTransform)root.transform).rect.height - 24f;
            float altura = content.sizeDelta.y + 6f;
            float escala = altura > disponivel && disponivel > 0f ? disponivel / altura : 1f;
            content.localScale = borderRect.localScale = new Vector3(escala, escala, 1f);
        }

        private void Update()
        {
            if (IsOpen)
            {
                AjustarAoTamanhoDaTela();
            }

            // O B/Esc que acabou de fechar a tela de controles não fecha esta junto.
            if (!IsOpen || OptionsMenu.IsOpen || OptionsMenu.ClosedThisFrame)
            {
                return;
            }

            bool voltar =
                (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) ||
                (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame);

            if (voltar)
            {
                AudioManager.PlayUiCancel();
                Close();
            }
        }

        private void Show()
        {
            EventSystemBootstrap.EnsureExists();
            SettingsManager.Initialize();

            root.SetActive(true);
            EventSystem.current?.SetSelectedGameObject(closeButton.gameObject);
        }

        private void Close()
        {
            root.SetActive(false);
            Closed?.Invoke();
        }

        /// <summary>Avisado ao fechar, para quem abriu devolver o foco.</summary>
        public static event Action Closed;

        // ---------------------------------------------------------------- construção

        private void Build()
        {
            // AddComponent ja dispara o Awake, que constroi; o getter chama Build de
            // novo logo em seguida. Sem esta guarda cada tela nasce com DOIS Canvas —
            // o duplicado fica invisivel e ainda por cima intercepta clique.
            if (root != null)
            {
                return;
            }

            root = new GameObject("SettingsCanvas");
            root.transform.SetParent(transform, false);

            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = UITheme.Layer.Settings;

            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            root.AddComponent<GraphicRaycaster>();

            Image cortina = Panel(root.transform, "Dim", UITheme.OverlayDim);
            Stretch((RectTransform)cortina.transform);

            // A moldura é IRMÃ do painel e vem antes dele, não filha: em UI, filho
            // desenha sempre por cima do pai — uma borda filha cobriria o fundo
            // inteiro de dourado e deixaria o texto ilegível.
            Image borda = Panel(root.transform, "Border", UITheme.TextAccent);
            var bordaRect = (RectTransform)borda.transform;
            bordaRect.anchorMin = bordaRect.anchorMax = new Vector2(0.5f, 0.5f);
            bordaRect.anchoredPosition = Vector2.zero;
            borda.raycastTarget = false;

            Image painel = Panel(root.transform, "Panel", UITheme.PanelBackground);
            content = (RectTransform)painel.transform;
            content.anchorMin = content.anchorMax = new Vector2(0.5f, 0.5f);
            content.sizeDelta = new Vector2(PanelWidth, 600f);
            content.anchoredPosition = Vector2.zero;

            borderRect = bordaRect;

            y = 250f;

            Label(Localization.Get("ui.settings.title"), UITheme.FontHeading, UITheme.TextAccent);
            y -= 12f;

            Section(Localization.Get("ui.settings.section.audio"));
            Slider(Localization.Get("ui.settings.masterVolume"), SettingsManager.MasterVolume, v => SettingsManager.MasterVolume = v);
            Slider(Localization.Get("ui.settings.music"), SettingsManager.MusicVolume, v => SettingsManager.MusicVolume = v);
            Slider(Localization.Get("ui.settings.sfx"), SettingsManager.SfxVolume, v => SettingsManager.SfxVolume = v);

            Section(Localization.Get("ui.settings.section.controls"));
            Row(Localization.Get("ui.settings.keyboardGamepad"), Localization.Get("ui.settings.customize"), OptionsMenu.Open);

            Section(Localization.Get("ui.settings.section.graphics"));
            Stepper(Localization.Get("ui.settings.quality"), SettingsManager.QualityNames,
                () => SettingsManager.QualityLevel, i => SettingsManager.QualityLevel = i);

            // No navegador quem manda no tamanho é a página; oferecer o controle seria
            // mostrar algo que não muda nada.
            if (SettingsManager.SupportsResolutionChange)
            {
                string[] resolucoes = new string[SettingsManager.AvailableResolutions.Length];
                for (int i = 0; i < resolucoes.Length; i++)
                {
                    Vector2Int r = SettingsManager.AvailableResolutions[i];
                    resolucoes[i] = $"{r.x} x {r.y}";
                }

                Stepper(Localization.Get("ui.settings.resolution"), resolucoes,
                    () => SettingsManager.ResolutionIndex, i => SettingsManager.ResolutionIndex = i);
            }

            Toggle(Localization.Get("ui.settings.fullscreen"), SettingsManager.Fullscreen, v => SettingsManager.Fullscreen = v);
            Toggle(Localization.Get("ui.settings.vibration"), SettingsManager.Vibration, v => SettingsManager.Vibration = v);

            // Os nomes dos idiomas ficam sempre no próprio idioma ("English",
            // "Português"): quem abriu a tela sem entender o que está escrito precisa
            // conseguir reconhecer o seu na lista.
            Section(Localization.Get("ui.settings.section.language"));
            Stepper(Localization.Get("ui.settings.language"), Localization.LanguageNames,
                () => (int)Localization.Current, i => Localization.Current = (Language)i);

            y -= 14f;
            closeButton = WideButton(Localization.Get("ui.settings.close"), () =>
            {
                AudioManager.PlayUiConfirm();
                Close();
            });

            // Altura do painel conforme o conteúdo realmente ocupou.
            float usado = 250f - y + 40f;
            content.sizeDelta = new Vector2(PanelWidth, Mathf.Max(320f, usado));
            borderRect.sizeDelta = content.sizeDelta + new Vector2(6f, 6f);

            LinkNavigation();
            root.SetActive(false);
        }

        // ---------------------------------------------------------------- linhas

        private void Label(string texto, int tamanho, Color cor)
        {
            Text t = CreateText(content, texto, texto, tamanho, cor);
            Place((RectTransform)t.transform, new Vector2(PanelWidth - 60f, 40f));
            y -= 44f;
        }

        private void Section(string titulo)
        {
            y -= 6f;
            Text t = CreateText(content, "Section_" + titulo, titulo, UITheme.FontBody, UITheme.TextAccent);
            Place((RectTransform)t.transform, new Vector2(PanelWidth - 60f, 28f));
            y -= 32f;
        }

        private void Slider(string titulo, float valor, Action<float> aoMudar)
        {
            Text rotulo = CreateText(content, "Label_" + titulo, titulo, UITheme.FontBody, UITheme.TextPrimary);
            rotulo.alignment = TextAnchor.MiddleLeft;
            var rotuloRect = (RectTransform)rotulo.transform;
            rotuloRect.anchorMin = rotuloRect.anchorMax = new Vector2(0.5f, 0.5f);
            rotuloRect.sizeDelta = new Vector2(220f, RowHeight);
            rotuloRect.anchoredPosition = new Vector2(-(PanelWidth / 2f) + 140f, y);

            var go = new GameObject("Slider_" + titulo);
            go.transform.SetParent(content, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(240f, 18f);
            rect.anchoredPosition = new Vector2(60f, y);

            var slider = go.AddComponent<Slider>();

            Image fundo = Panel(go.transform, "Background", UITheme.ButtonPressed);
            Stretch((RectTransform)fundo.transform);

            var areaPreenchida = new GameObject("Fill Area");
            areaPreenchida.transform.SetParent(go.transform, false);
            var areaRect = areaPreenchida.AddComponent<RectTransform>();
            Stretch(areaRect);

            Image preenchimento = Panel(areaPreenchida.transform, "Fill", UITheme.TextAccent);
            Stretch((RectTransform)preenchimento.transform);

            slider.fillRect = (RectTransform)preenchimento.transform;
            slider.targetGraphic = fundo;
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.SetValueWithoutNotify(valor);

            Text percentual = CreateText(content, "Value_" + titulo, Percentual(valor), UITheme.FontBody, UITheme.TextSecondary);
            var percRect = (RectTransform)percentual.transform;
            percRect.anchorMin = percRect.anchorMax = new Vector2(0.5f, 0.5f);
            percRect.sizeDelta = new Vector2(80f, RowHeight);
            percRect.anchoredPosition = new Vector2((PanelWidth / 2f) - 62f, y);

            slider.onValueChanged.AddListener(v =>
            {
                percentual.text = Percentual(v);
                aoMudar(v);
            });

            y -= RowHeight;
        }

        private static string Percentual(float v) => Mathf.RoundToInt(v * 100f) + "%";

        private void Row(string titulo, string acao, Action aoClicar)
        {
            Text rotulo = CreateText(content, "Label_" + titulo, titulo, UITheme.FontBody, UITheme.TextPrimary);
            rotulo.alignment = TextAnchor.MiddleLeft;
            var rotuloRect = (RectTransform)rotulo.transform;
            rotuloRect.anchorMin = rotuloRect.anchorMax = new Vector2(0.5f, 0.5f);
            rotuloRect.sizeDelta = new Vector2(260f, RowHeight);
            rotuloRect.anchoredPosition = new Vector2(-(PanelWidth / 2f) + 160f, y);

            Button botao = SmallButton(acao, new Vector2(120f, y), aoClicar);
            focusables.Add(botao);
            y -= RowHeight;
        }

        private void Stepper(string titulo, string[] opcoes, Func<int> ler, Action<int> escrever)
        {
            Text rotulo = CreateText(content, "Label_" + titulo, titulo, UITheme.FontBody, UITheme.TextPrimary);
            rotulo.alignment = TextAnchor.MiddleLeft;
            var rotuloRect = (RectTransform)rotulo.transform;
            rotuloRect.anchorMin = rotuloRect.anchorMax = new Vector2(0.5f, 0.5f);
            rotuloRect.sizeDelta = new Vector2(220f, RowHeight);
            rotuloRect.anchoredPosition = new Vector2(-(PanelWidth / 2f) + 140f, y);

            Text valor = CreateText(content, "Value_" + titulo, Atual(opcoes, ler()), UITheme.FontBody, UITheme.TextAccent);
            var valorRect = (RectTransform)valor.transform;
            valorRect.anchorMin = valorRect.anchorMax = new Vector2(0.5f, 0.5f);
            valorRect.sizeDelta = new Vector2(150f, RowHeight);
            valorRect.anchoredPosition = new Vector2(95f, y);

            void Mover(int passo)
            {
                if (opcoes.Length == 0)
                {
                    return;
                }

                int novo = (ler() + passo + opcoes.Length) % opcoes.Length;
                escrever(novo);
                valor.text = Atual(opcoes, novo);
                AudioManager.PlayUiClick();
            }

            focusables.Add(SmallButton("<", new Vector2(-10f, y), () => Mover(-1), 44f));
            focusables.Add(SmallButton(">", new Vector2(190f, y), () => Mover(1), 44f));

            y -= RowHeight;
        }

        private static string Atual(string[] opcoes, int i)
        {
            return opcoes.Length == 0 ? "—" : opcoes[Mathf.Clamp(i, 0, opcoes.Length - 1)];
        }

        private void Toggle(string titulo, bool valor, Action<bool> aoMudar)
        {
            Text rotulo = CreateText(content, "Label_" + titulo, titulo, UITheme.FontBody, UITheme.TextPrimary);
            rotulo.alignment = TextAnchor.MiddleLeft;
            var rotuloRect = (RectTransform)rotulo.transform;
            rotuloRect.anchorMin = rotuloRect.anchorMax = new Vector2(0.5f, 0.5f);
            rotuloRect.sizeDelta = new Vector2(220f, RowHeight);
            rotuloRect.anchoredPosition = new Vector2(-(PanelWidth / 2f) + 140f, y);

            bool estado = valor;
            Button botao = null;

            botao = SmallButton(estado ? Localization.Get("ui.common.yes") : Localization.Get("ui.common.no"), new Vector2(120f, y), () =>
            {
                estado = !estado;
                aoMudar(estado);

                Text t = botao.GetComponentInChildren<Text>();
                if (t != null)
                {
                    t.text = estado ? Localization.Get("ui.common.yes") : Localization.Get("ui.common.no");
                }

                AudioManager.PlayUiClick();
            });

            focusables.Add(botao);
            y -= RowHeight;
        }

        // ---------------------------------------------------------------- widgets

        private Button SmallButton(string texto, Vector2 posicao, Action aoClicar, float largura = 150f)
        {
            Image image = Panel(content, "Button_" + texto, UITheme.ButtonNormal);
            var rect = (RectTransform)image.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(largura, 34f);
            rect.anchoredPosition = posicao;

            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.onClick.AddListener(() => aoClicar());

            Text label = CreateText(image.transform, "Label", texto, UITheme.FontBody, UITheme.TextPrimary);
            Stretch((RectTransform)label.transform);

            var feedback = image.gameObject.AddComponent<MenuButton>();
            feedback.enabled = true;

            return button;
        }

        private Button WideButton(string texto, Action aoClicar)
        {
            Button b = SmallButton(texto, new Vector2(0f, y), aoClicar, 260f);
            var rect = (RectTransform)b.transform;
            rect.sizeDelta = new Vector2(260f, 48f);
            b.GetComponent<MenuButton>()?.RefreshBasePosition();
            focusables.Add(b);
            y -= 56f;
            return b;
        }

        private void LinkNavigation()
        {
            for (int i = 0; i < focusables.Count; i++)
            {
                focusables[i].navigation = new Navigation
                {
                    mode = Navigation.Mode.Explicit,
                    selectOnUp = focusables[(i - 1 + focusables.Count) % focusables.Count],
                    selectOnDown = focusables[(i + 1) % focusables.Count],
                };
            }
        }

        // ---------------------------------------------------------------- primitivas

        private static Image Panel(Transform parent, string name, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            var image = go.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static Text CreateText(Transform parent, string name, string texto, int tamanho, Color cor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();

            var text = go.AddComponent<Text>();
            text.font = UITheme.Font;
            text.text = texto;
            text.fontSize = tamanho;
            text.color = cor;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        private void Place(RectTransform rect, Vector2 tamanho)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = tamanho;
            rect.anchoredPosition = new Vector2(0f, y);
        }

        private static void Stretch(RectTransform rect, float padding = 0f)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(padding, padding);
            rect.offsetMax = new Vector2(-padding, -padding);
        }
    }
}
