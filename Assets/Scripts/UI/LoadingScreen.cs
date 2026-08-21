using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Odisseia.UI
{
    /// <summary>
    /// Tela de carregamento entre fases. Substitui a antiga tela de "fase concluída":
    /// ao terminar uma fase o jogo vai direto para a próxima, sem botão no meio.
    ///
    /// Segue o mesmo padrão do <see cref="ScreenFader"/> — cria-se sozinha na primeira
    /// utilização, monta o próprio Canvas e persiste entre cenas —, então nenhuma cena
    /// precisa conter o overlay e nenhuma cena nova entra no build.
    /// </summary>
    public class LoadingScreen : MonoBehaviour
    {
        /// <summary>
        /// As fases são pequenas e carregam quase instantaneamente. Sem um tempo mínimo
        /// a tela piscaria por poucos frames, o que fica pior do que não ter loader.
        /// </summary>
        private const float MinimumDisplay = 0.9f;

        private const float ReferenceWidth = 1920f;
        private const float ReferenceHeight = 1080f;
        private const float BarWidth = 520f;
        private const float BarHeight = 14f;

        private static LoadingScreen instance;

        private GameObject root;
        private Text titleText;
        private RectTransform barFill;
        private Coroutine routine;

        public static LoadingScreen Instance
        {
            get
            {
                if (instance == null)
                {
                    var go = new GameObject("LoadingScreen");
                    instance = go.AddComponent<LoadingScreen>();
                    DontDestroyOnLoad(go);
                    instance.Build();
                }

                return instance;
            }
        }

        /// <summary>
        /// Mostra o loader e carrega a cena, ativando-a só quando o carregamento termina.
        /// Espera-se que a tela já esteja escurecida pelo <see cref="ScreenFader"/>.
        /// </summary>
        public static void Begin(string sceneName, string title)
        {
            Instance.StartLoad(sceneName, title);
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

        private void StartLoad(string sceneName, string title)
        {
            if (routine != null)
            {
                return;
            }

            routine = StartCoroutine(LoadRoutine(sceneName, title));
        }

        private IEnumerator LoadRoutine(string sceneName, string title)
        {
            titleText.text = string.IsNullOrEmpty(title) ? "Carregando..." : title;
            SetProgress(0f);
            root.SetActive(true);

            // A tela chega aqui escurecida pelo SceneLoader; o fade-in revela o loader.
            ScreenFader.Instance.FadeIn();

            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            op.allowSceneActivation = false;

            float elapsed = 0f;
            float shown = 0f;

            // Com allowSceneActivation desligado, op.progress trava em 0.9.
            while (op.progress < 0.9f || elapsed < MinimumDisplay)
            {
                elapsed += Time.unscaledDeltaTime;

                float real = Mathf.Clamp01(op.progress / 0.9f);
                float paced = Mathf.Clamp01(elapsed / MinimumDisplay);
                // A barra nunca anda para trás e nunca ultrapassa o progresso real.
                shown = Mathf.Max(shown, Mathf.Min(real, paced));
                SetProgress(shown);

                yield return null;
            }

            SetProgress(1f);
            yield return new WaitForSecondsRealtime(0.12f);

            // Escurece antes de ativar, esconde o loader enquanto a tela está preta e
            // deixa o ScreenFader clarear sozinho ao entrar na cena nova.
            bool faded = false;
            ScreenFader.Instance.FadeOutThen(() => faded = true);
            while (!faded)
            {
                yield return null;
            }

            root.SetActive(false);
            routine = null;
            op.allowSceneActivation = true;
        }

        private void SetProgress(float value)
        {
            if (barFill != null)
            {
                barFill.sizeDelta = new Vector2(BarWidth * Mathf.Clamp01(value), BarHeight);
            }
        }

        private void Build()
        {
            var canvasGO = new GameObject("LoadingCanvas");
            canvasGO.transform.SetParent(transform, false);

            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            // Abaixo do ScreenFader (999), para o fade cobrir o loader nas transições.
            canvas.sortingOrder = 998;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            root = new GameObject("Root", typeof(RectTransform));
            root.transform.SetParent(canvasGO.transform, false);
            Stretch((RectTransform)root.transform);

            var background = root.AddComponent<Image>();
            background.color = new Color(0.03f, 0.04f, 0.07f, 1f);
            // Segura qualquer clique enquanto carrega.
            background.raycastTarget = true;

            titleText = CreateText(root.transform, "Title", UITheme.FontTitle, UITheme.TextAccent,
                new Vector2(1400f, 80f), new Vector2(0f, 60f));

            CreateText(root.transform, "Hint", UITheme.FontBody, UITheme.TextSecondary,
                new Vector2(900f, 40f), new Vector2(0f, 0f)).text = "Carregando...";

            CreateBar(root.transform);

            root.SetActive(false);
        }

        private void CreateBar(Transform parent)
        {
            var trackGO = new GameObject("BarTrack", typeof(RectTransform));
            trackGO.transform.SetParent(parent, false);
            var trackRect = (RectTransform)trackGO.transform;
            trackRect.anchorMin = new Vector2(0.5f, 0.5f);
            trackRect.anchorMax = new Vector2(0.5f, 0.5f);
            trackRect.pivot = new Vector2(0.5f, 0.5f);
            trackRect.sizeDelta = new Vector2(BarWidth, BarHeight);
            trackRect.anchoredPosition = new Vector2(0f, -60f);

            var track = trackGO.AddComponent<Image>();
            track.color = new Color(1f, 1f, 1f, 0.12f);
            track.raycastTarget = false;

            var fillGO = new GameObject("BarFill", typeof(RectTransform));
            fillGO.transform.SetParent(trackGO.transform, false);
            barFill = (RectTransform)fillGO.transform;
            // Ancorado à esquerda: crescer é só aumentar a largura.
            barFill.anchorMin = new Vector2(0f, 0.5f);
            barFill.anchorMax = new Vector2(0f, 0.5f);
            barFill.pivot = new Vector2(0f, 0.5f);
            barFill.anchoredPosition = Vector2.zero;
            barFill.sizeDelta = new Vector2(0f, BarHeight);

            var fill = fillGO.AddComponent<Image>();
            fill.color = UITheme.Collectible;
            fill.raycastTarget = false;
        }

        private static Text CreateText(Transform parent, string name, int fontSize, Color color,
            Vector2 size, Vector2 position)
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
