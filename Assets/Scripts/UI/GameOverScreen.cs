using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Odisseia.Systems;

namespace Odisseia.UI
{
    /// <summary>
    /// Tela de fim de jogo: aparece quando acabam as vidas e leva ao menu principal,
    /// de onde o jogador decide por onde retomar. O progresso da campanha (fases
    /// desbloqueadas) é preservado — perder a jornada não apaga o que já foi conquistado.
    ///
    /// Monta-se em runtime e escuta <see cref="LivesCounter.GameOver"/>, então nenhuma
    /// cena precisa conter a tela e nenhuma cena nova entra no build.
    /// </summary>
    public class GameOverScreen : MonoBehaviour
    {
        private const float ReferenceWidth = 960f;
        private const float ReferenceHeight = 600f;

        private static GameOverScreen instance;

        private GameObject root;
        private Text titleText, messageText, statsText, hintText, retryLabel, menuLabel;
        private InputActionMap playerMap;

        /// <summary>
        /// Sobe sozinho no primeiro frame para já estar inscrito no evento quando a
        /// última vida acabar — não dá para depender de alguém abrir a tela antes.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (instance == null)
            {
                var go = new GameObject("GameOverScreen");
                instance = go.AddComponent<GameOverScreen>();
                DontDestroyOnLoad(go);
            }
        }

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
        }

        private void OnEnable()
        {
            LivesCounter.GameOver += Show;
        }

        private void OnDisable()
        {
            LivesCounter.GameOver -= Show;
        }

        private void Show()
        {
            EventSystemBootstrap.EnsureExists();

            // Lidos a cada exibição: o idioma pode ter mudado desde que a tela foi montada.
            titleText.text = Localization.Get("ui.gameover.title");
            messageText.text = Localization.Get("ui.gameover.message");
            statsText.text = Localization.Get("ui.gameover.stats", ExperienceCounter.Total, CollectibleCounter.Count);
            hintText.text = Localization.Get("ui.gameover.hint");
            retryLabel.text = Localization.Get("ui.gameover.retry");
            menuLabel.text = Localization.Get("ui.pause.menu");

            // Congela o jogo e cala o input de gameplay: o jogador não deve continuar
            // controlando Odisseu por baixo da tela.
            Time.timeScale = 0f;
            playerMap = KeyRebindService.Asset?.FindActionMap("Player", throwIfNotFound: false);
            playerMap?.Disable();

            root.SetActive(true);
            AudioManager.PlayDeath();
        }

        /// <summary>
        /// QA-03: recomeça a FASE atual com a jornada inteira de volta (vidas e experiência) — o mesmo reinício que o
        /// menu principal faz ao entrar. Antes, perder a última vida obrigava a passar por menu e mapa para tentar de novo.
        /// </summary>
        private void Retry()
        {
            root.SetActive(false);
            Time.timeScale = 1f;
            AudioManager.PlayUiClick();
            LivesCounter.BeginRun();
            ExperienceCounter.BeginRun();
            SceneLoader.Load(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        private void BackToMenu()
        {
            root.SetActive(false);
            Time.timeScale = 1f;
            AudioManager.PlayUiClick();

            // As vidas e a experiência são reiniciadas pelo MainMenuController ao entrar
            // no menu, então uma jornada nova sempre começa completa.
            SceneLoader.Load(SceneLoader.MainMenu);
        }

        private void Build()
        {
            var canvasGO = new GameObject("GameOverCanvas");
            canvasGO.transform.SetParent(transform, false);

            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            // Acima de tudo que é jogo, abaixo do loader e do fade de transição.
            canvas.sortingOrder = UITheme.Layer.GameOver;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            root = new GameObject("Root", typeof(RectTransform));
            root.transform.SetParent(canvasGO.transform, false);
            Stretch((RectTransform)root.transform);

            var background = root.AddComponent<Image>();
            background.color = new Color(0.05f, 0.03f, 0.05f, 0.94f);

            titleText = CreateText(root.transform, "Title", string.Empty, UITheme.FontTitle,
                UITheme.TextAccent, new Vector2(900f, 70f), new Vector2(0f, 120f));

            messageText = CreateText(root.transform, "Message",
                string.Empty, UITheme.FontHeading,
                UITheme.TextPrimary, new Vector2(900f, 44f), new Vector2(0f, 52f));

            statsText = CreateText(root.transform, "Stats", string.Empty, UITheme.FontBody,
                UITheme.TextSecondary, new Vector2(900f, 70f), new Vector2(0f, -14f));

            retryLabel = CreateButton(root.transform, "RetryButton", string.Empty,
                new Vector2(280f, 54f), new Vector2(-150f, -110f), Retry);
            menuLabel = CreateButton(root.transform, "MenuButton", string.Empty,
                new Vector2(280f, 54f), new Vector2(150f, -110f), BackToMenu);

            // Foco para teclado e controle: sem um botão selecionado, quem joga só de
            // controle chegava a esta tela sem ter como tentar de novo nem sair dela.
            var navigator = root.AddComponent<MenuNavigator>();
            navigator.SetItems(new[]
            {
                retryLabel.GetComponentInParent<Button>(),
                menuLabel.GetComponentInParent<Button>(),
            });

            hintText = CreateText(root.transform, "Hint",
                string.Empty, UITheme.FontBody,
                UITheme.TextSecondary, new Vector2(900f, 34f), new Vector2(0f, -170f));

            root.SetActive(false);
        }

        private static Text CreateButton(Transform parent, string name, string label,
            Vector2 size, Vector2 position, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;

            var image = go.AddComponent<Image>();
            image.color = Color.white;

            var button = go.AddComponent<Button>();
            button.targetGraphic = image;

            ColorBlock colors = ColorBlock.defaultColorBlock;
            colors.normalColor = UITheme.ButtonNormal;
            colors.highlightedColor = UITheme.ButtonHighlight;
            colors.pressedColor = UITheme.ButtonPressed;
            colors.selectedColor = UITheme.ButtonHighlight;
            colors.fadeDuration = 0.08f;
            button.colors = colors;
            button.onClick.AddListener(onClick);

            Text text = CreateText(rect, "Label", label, UITheme.FontButton,
                UITheme.TextPrimary, size, Vector2.zero);
            Stretch((RectTransform)text.transform);
            return text;
        }

        private static Text CreateText(Transform parent, string name, string content, int fontSize,
            Color color, Vector2 size, Vector2 position)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;

            var text = go.AddComponent<Text>();
            text.text = content;
            text.font = UITheme.Font;
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
