using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Odisseia.UI;

namespace Odisseia.WorldMap
{
    /// <summary>
    /// UI temporária do mapa: título, painel da fase em foco, aviso de fase bloqueada e
    /// anúncio de região desbloqueada.
    ///
    /// Monta-se em runtime, no mesmo padrão do resto da UI do projeto, e é a única parte
    /// do mapa que conhece texto e cor de painel — o <see cref="WorldMapManager"/> só
    /// chama métodos daqui. Substituir por arte definitiva depois é reescrever esta
    /// classe, sem tocar na lógica.
    /// </summary>
    public class WorldMapUI : MonoBehaviour
    {
        private const float ReferenceWidth = 960f;
        private const float ReferenceHeight = 600f;

        [SerializeField] private string mapTitle = "A ODISSEIA";
        [SerializeField] private string mapSubtitle = "Jornada de Odisseu";
        [SerializeField] private string lockedMessage = "Complete a fase anterior para continuar sua jornada.";
        [SerializeField] private float lockedMessageDuration = 1.8f;
        [SerializeField] private float announcementDuration = 3.2f;

        private Text progressText;
        private Text nodeNameText;
        private Text nodeHintText;
        private GameObject nodePanel;

        private Text lockedText;
        private GameObject lockedPanel;
        private Coroutine lockedRoutine;

        private GameObject announcementPanel;
        private Text announcementText;
        private Coroutine announcementRoutine;

        private Button enterButton;
        private WorldMapManager manager;

        private void Awake()
        {
            manager = FindAnyObjectByType<WorldMapManager>();
            EventSystemBootstrap.EnsureExists();
            Build();
        }

        /// <summary>Mostra os dados do nó em foco, ou esconde o painel se não houver.</summary>
        public void ShowNode(LevelNode node, int TotalLevels)
        {
            if (node == null)
            {
                nodePanel.SetActive(false);
                return;
            }

            nodePanel.SetActive(true);
            nodeNameText.text = node.LevelName.ToUpperInvariant();
            progressText.text = $"Fase {node.Order} de {TotalLevels}";

            bool enterable = node.IsEnterable;
            nodeHintText.text = enterable
                ? (node.State == LevelNodeState.Completed
                    ? "Já concluída — [E] para jogar de novo"
                    : "[E] Jogar")
                : "Bloqueada";

            enterButton.gameObject.SetActive(enterable);
        }

        public void ShowLockedFeedback()
        {
            if (lockedRoutine != null)
            {
                return;
            }

            lockedRoutine = StartCoroutine(LockedRoutine());
        }

        private IEnumerator LockedRoutine()
        {
            lockedText.text = lockedMessage;
            lockedPanel.SetActive(true);
            yield return new WaitForSeconds(lockedMessageDuration);
            lockedPanel.SetActive(false);
            lockedRoutine = null;
        }

        /// <summary>Anuncia a conclusão e, se houver, a região recém-desbloqueada.</summary>
        public void ShowCompletionAnnouncement(LevelNode completed, LevelNode unlocked)
        {
            if (completed == null)
            {
                return;
            }

            string text = $"FASE CONCLUÍDA\n\n✓ {completed.LevelName}";
            if (unlocked != null)
            {
                text += $"\n\nNova região desbloqueada:\n{unlocked.LevelName.ToUpperInvariant()}";
            }

            announcementText.text = text;

            if (announcementRoutine != null)
            {
                StopCoroutine(announcementRoutine);
            }

            announcementRoutine = StartCoroutine(AnnouncementRoutine());
        }

        private IEnumerator AnnouncementRoutine()
        {
            announcementPanel.SetActive(true);
            yield return new WaitForSeconds(announcementDuration);
            announcementPanel.SetActive(false);
            announcementRoutine = null;
        }

        // ---------------------------------------------------------------- montagem

        private void Build()
        {
            var canvasGO = new GameObject("WorldMapCanvas");
            canvasGO.transform.SetParent(transform, false);

            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();
            Transform root = canvasGO.transform;

            // topo
            CreateText(root, "Title", mapTitle, UITheme.FontHeading, UITheme.TextAccent,
                new Vector2(0.5f, 1f), new Vector2(0f, -26f), new Vector2(600f, 40f));
            CreateText(root, "Subtitle", mapSubtitle, UITheme.FontBody, UITheme.TextSecondary,
                new Vector2(0.5f, 1f), new Vector2(0f, -60f), new Vector2(600f, 28f));
            progressText = CreateText(root, "Progress", string.Empty, UITheme.FontBody,
                UITheme.TextSecondary, new Vector2(0.5f, 1f), new Vector2(0f, -88f), new Vector2(600f, 28f));

            // painel do nó (rodapé)
            nodePanel = CreatePanel(root, "NodePanel", new Vector2(0.5f, 0f),
                new Vector2(0f, 18f), new Vector2(560f, 108f));
            nodeNameText = CreateText(nodePanel.transform, "NodeName", string.Empty, UITheme.FontHeading,
                UITheme.TextPrimary, new Vector2(0.5f, 1f), new Vector2(0f, -12f), new Vector2(520f, 38f));
            nodeHintText = CreateText(nodePanel.transform, "NodeHint", string.Empty, UITheme.FontBody,
                UITheme.TextSecondary, new Vector2(0.5f, 1f), new Vector2(0f, -52f), new Vector2(520f, 28f));

            enterButton = CreateButton(nodePanel.transform, "EnterButton", "JOGAR",
                new Vector2(0.5f, 0f), new Vector2(0f, 12f), new Vector2(200f, 40f));
            enterButton.onClick.AddListener(() => manager?.TryEnterCurrentNode());

            nodePanel.SetActive(false);

            // aviso de bloqueio
            lockedPanel = CreatePanel(root, "LockedPanel", new Vector2(0.5f, 0.5f),
                new Vector2(0f, -150f), new Vector2(620f, 56f));
            lockedText = CreateText(lockedPanel.transform, "LockedText", string.Empty, UITheme.FontBody,
                UITheme.TextPrimary, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(600f, 48f));
            lockedPanel.SetActive(false);

            // anúncio de conclusão
            announcementPanel = CreatePanel(root, "AnnouncementPanel", new Vector2(0.5f, 0.5f),
                new Vector2(0f, 40f), new Vector2(560f, 220f));
            announcementText = CreateText(announcementPanel.transform, "AnnouncementText", string.Empty,
                UITheme.FontBody, UITheme.TextPrimary, new Vector2(0.5f, 0.5f), Vector2.zero,
                new Vector2(520f, 200f));
            announcementPanel.SetActive(false);
        }

        private static GameObject CreatePanel(Transform parent, string name, Vector2 anchor,
            Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, anchor.y);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;

            var image = go.AddComponent<Image>();
            image.color = UITheme.PanelBackground;
            return go;
        }

        private static Text CreateText(Transform parent, string name, string content, int fontSize,
            Color color, Vector2 anchor, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, anchor.y);
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

        private static Button CreateButton(Transform parent, string name, string label,
            Vector2 anchor, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0f);
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

            Text text = CreateText(rect, "Label", label, UITheme.FontButton, UITheme.TextPrimary,
                new Vector2(0.5f, 0.5f), Vector2.zero, size);
            var textRect = (RectTransform)text.transform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return button;
        }
    }
}
