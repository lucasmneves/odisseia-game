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
        private Text statsText;
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

            statsText.text =
                $"Experiência acumulada: {ExperienceCounter.Total} XP\n" +
                $"Itens coletados: {CollectibleCounter.Count}";

            // Congela o jogo e cala o input de gameplay: o jogador não deve continuar
            // controlando Odisseu por baixo da tela.
            Time.timeScale = 0f;
            playerMap = KeyRebindService.Asset?.FindActionMap("Player", throwIfNotFound: false);
            playerMap?.Disable();

            root.SetActive(true);
            AudioManager.PlayDeath();
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
            canvas.sortingOrder = 600;

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

            CreateText(root.transform, "Title", "FIM DA JORNADA", UITheme.FontTitle,
                UITheme.TextAccent, new Vector2(900f, 70f), new Vector2(0f, 120f));

            CreateText(root.transform, "Message",
                "Odisseu não chegou a Ítaca desta vez.", UITheme.FontHeading,
                UITheme.TextPrimary, new Vector2(900f, 44f), new Vector2(0f, 52f));

            statsText = CreateText(root.transform, "Stats", string.Empty, UITheme.FontBody,
                UITheme.TextSecondary, new Vector2(900f, 70f), new Vector2(0f, -14f));

            CreateButton(root.transform, "MenuButton", "Menu principal",
                new Vector2(280f, 54f), new Vector2(0f, -110f), BackToMenu);

            CreateText(root.transform, "Hint",
                "As fases já conquistadas continuam desbloqueadas.", UITheme.FontBody,
                UITheme.TextSecondary, new Vector2(900f, 34f), new Vector2(0f, -170f));

            root.SetActive(false);
        }

        private static void CreateButton(Transform parent, string name, string label,
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
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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
