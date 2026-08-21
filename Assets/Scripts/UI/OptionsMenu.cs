using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Odisseia.Systems;

namespace Odisseia.UI
{
    /// <summary>
    /// Tela de opções com remapeamento de teclas. Monta o próprio Canvas em runtime e
    /// persiste entre cenas, no mesmo padrão de <see cref="ScreenFader"/> e
    /// <see cref="LoadingScreen"/> — assim funciona tanto no menu principal quanto no
    /// pause das 16 fases, sem editar nenhuma cena.
    /// </summary>
    public class OptionsMenu : MonoBehaviour
    {
        private const float ReferenceWidth = 960f;
        private const float ReferenceHeight = 600f;
        private const float RowHeight = 34f;
        private const float PanelWidth = 620f;
        private const float RowsTop = 62f;      // espaço do título
        private const float BottomArea = 124f;  // mensagem + botões
        private const float MaxPanelHeight = 560f;

        private static OptionsMenu instance;

        private GameObject root;
        private Text messageText;
        private readonly List<(KeyRebindService.Entry entry, Text keyLabel)> rows = new();

        private InputActionRebindingExtensions.RebindingOperation activeOperation;
        private float previousTimeScale = 1f;
        private InputActionMap playerMap;
        private bool mapWasEnabled;

        public static bool IsOpen => instance != null && instance.root != null && instance.root.activeSelf;

        public static OptionsMenu Instance
        {
            get
            {
                if (instance == null)
                {
                    var go = new GameObject("OptionsMenu");
                    instance = go.AddComponent<OptionsMenu>();
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
        }

        private void Show()
        {
            KeyRebindService.EnsureLoaded();
            EventSystemBootstrap.EnsureExists();

            // Abrir pelo menu principal não pausa nada; abrir pelo pause já chega com
            // timeScale 0 e precisa continuar assim ao fechar.
            previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;

            SuspendGameplayInput();

            RebuildRows();
            SetMessage("Clique numa tecla para trocar. Esc cancela.");
            root.SetActive(true);
            AudioManager.PlayUiClick();
        }

        private void Close()
        {
            CancelActiveOperation();
            RestoreGameplayInput();
            Time.timeScale = previousTimeScale;
            root.SetActive(false);
            AudioManager.PlayUiClick();
        }

        /// <summary>
        /// Desliga o mapa "Player" enquanto o menu está aberto. Sem isso, as teclas
        /// pressionadas durante um remapeamento continuariam chegando ao jogo — apertar
        /// a tecla do arco dispararia uma flecha, e o Esc que cancela a captura também
        /// alternaria o pause por baixo do menu.
        /// </summary>
        private void SuspendGameplayInput()
        {
            playerMap = KeyRebindService.Asset?.FindActionMap("Player", throwIfNotFound: false);
            if (playerMap == null)
            {
                return;
            }

            mapWasEnabled = playerMap.enabled;
            playerMap.Disable();
        }

        private void RestoreGameplayInput()
        {
            if (playerMap == null)
            {
                return;
            }

            if (mapWasEnabled)
            {
                playerMap.Enable();
            }
            else
            {
                // Fora do gameplay o PauseMenu mantém só a ação de pause viva.
                playerMap.FindAction("Pause")?.Enable();
            }
        }

        private void CancelActiveOperation()
        {
            if (activeOperation != null)
            {
                activeOperation.Cancel();
                activeOperation = null;
            }
        }

        private void OnDisable() => CancelActiveOperation();

        // ------------------------------------------------------------------ linhas

        private Transform rowsParent;
        private RectTransform panelRect;

        private void RebuildRows()
        {
            foreach (Transform child in rowsParent)
            {
                Destroy(child.gameObject);
            }
            rows.Clear();

            List<KeyRebindService.Entry> entries = KeyRebindService.GetEntries();

            if (entries.Count == 0)
            {
                SetMessage("Controles indisponíveis nesta cena.");
                return;
            }

            for (int i = 0; i < entries.Count; i++)
            {
                CreateRow(entries[i], i);
            }

            ResizeToFit(entries.Count);
        }

        /// <summary>
        /// O painel cresce com a quantidade de ações remapeáveis. "Mover" sozinho rende
        /// 4 linhas (WASD e setas), então um tamanho fixo faria as linhas invadirem os
        /// botões — e quebraria de novo a cada ação nova.
        /// </summary>
        private void ResizeToFit(int rowCount)
        {
            float rowsHeight = rowCount * RowHeight;

            if (rowsParent is RectTransform rowsRect)
            {
                rowsRect.sizeDelta = new Vector2(0f, rowsHeight);
            }

            if (panelRect != null)
            {
                float desired = RowsTop + rowsHeight + BottomArea;
                panelRect.sizeDelta = new Vector2(PanelWidth, Mathf.Min(desired, MaxPanelHeight));
            }
        }

        private void CreateRow(KeyRebindService.Entry entry, int index)
        {
            var rowGO = new GameObject($"Row_{index}", typeof(RectTransform));
            rowGO.transform.SetParent(rowsParent, false);
            var rowRect = (RectTransform)rowGO.transform;
            rowRect.anchorMin = new Vector2(0f, 1f);
            rowRect.anchorMax = new Vector2(1f, 1f);
            rowRect.pivot = new Vector2(0.5f, 1f);
            rowRect.offsetMin = new Vector2(0f, 0f);
            rowRect.offsetMax = new Vector2(0f, 0f);
            rowRect.sizeDelta = new Vector2(0f, RowHeight - 6f);
            rowRect.anchoredPosition = new Vector2(0f, -index * RowHeight);

            // rótulo da ação
            Text label = CreateText(rowRect, "Label", entry.Label, UITheme.FontBody, UITheme.TextPrimary,
                TextAnchor.MiddleLeft);
            var labelRect = (RectTransform)label.transform;
            labelRect.anchorMin = new Vector2(0f, 0f);
            labelRect.anchorMax = new Vector2(0.62f, 1f);
            labelRect.offsetMin = new Vector2(16f, 0f);
            labelRect.offsetMax = Vector2.zero;

            // botão com a tecla atual
            var buttonGO = new GameObject("KeyButton", typeof(RectTransform));
            buttonGO.transform.SetParent(rowRect, false);
            var buttonRect = (RectTransform)buttonGO.transform;
            buttonRect.anchorMin = new Vector2(0.64f, 0f);
            buttonRect.anchorMax = new Vector2(1f, 1f);
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = new Vector2(-16f, 0f);

            var image = buttonGO.AddComponent<Image>();
            image.color = Color.white;

            var button = buttonGO.AddComponent<Button>();
            button.targetGraphic = image;
            button.colors = ButtonColors();

            Text keyLabel = CreateText(buttonRect, "Key",
                KeyRebindService.GetDisplayString(entry.Action, entry.BindingIndex),
                UITheme.FontButton, UITheme.TextPrimary, TextAnchor.MiddleCenter);
            var keyRect = (RectTransform)keyLabel.transform;
            keyRect.anchorMin = Vector2.zero;
            keyRect.anchorMax = Vector2.one;
            keyRect.offsetMin = Vector2.zero;
            keyRect.offsetMax = Vector2.zero;

            rows.Add((entry, keyLabel));

            KeyRebindService.Entry captured = entry;
            Text capturedLabel = keyLabel;
            button.onClick.AddListener(() => BeginRebind(captured, capturedLabel));
        }

        private void BeginRebind(KeyRebindService.Entry entry, Text keyLabel)
        {
            if (activeOperation != null)
            {
                return;
            }

            keyLabel.text = "...";
            SetMessage($"Pressione a nova tecla para \"{entry.Label}\". Esc cancela.");
            AudioManager.PlayUiClick();

            activeOperation = KeyRebindService.StartRebind(entry.Action, entry.BindingIndex, reason =>
            {
                activeOperation = null;
                RefreshKeyLabels();

                SetMessage(reason == null
                    ? $"\"{entry.Label}\" atualizado."
                    : $"Não alterado — {reason}.");
            });
        }

        private void RefreshKeyLabels()
        {
            foreach ((KeyRebindService.Entry entry, Text keyLabel) in rows)
            {
                if (keyLabel != null)
                {
                    keyLabel.text = KeyRebindService.GetDisplayString(entry.Action, entry.BindingIndex);
                }
            }
        }

        private void SetMessage(string message)
        {
            if (messageText != null)
            {
                messageText.text = message;
            }
        }

        // ------------------------------------------------------------------ montagem

        private void Build()
        {
            var canvasGO = new GameObject("OptionsCanvas");
            canvasGO.transform.SetParent(transform, false);

            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            // Acima do HUD e do pause (0/1), abaixo do loader (998) e do fade (999).
            canvas.sortingOrder = 500;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            root = new GameObject("Root", typeof(RectTransform));
            root.transform.SetParent(canvasGO.transform, false);
            Stretch((RectTransform)root.transform);

            var dim = root.AddComponent<Image>();
            dim.color = UITheme.OverlayDim;

            // painel central
            var panelGO = new GameObject("Panel", typeof(RectTransform));
            panelGO.transform.SetParent(root.transform, false);
            panelRect = (RectTransform)panelGO.transform;
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            // altura provisória; ResizeToFit ajusta pelo número real de linhas
            panelRect.sizeDelta = new Vector2(PanelWidth, 520f);
            panelRect.anchoredPosition = Vector2.zero;

            var panelImage = panelGO.AddComponent<Image>();
            panelImage.color = UITheme.PanelBackground;

            Text title = CreateText(panelRect, "Title", "CONTROLES", UITheme.FontHeading,
                UITheme.TextAccent, TextAnchor.MiddleCenter);
            var titleRect = (RectTransform)title.transform;
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.sizeDelta = new Vector2(0f, 46f);
            titleRect.anchoredPosition = new Vector2(0f, -14f);

            // container das linhas
            var rowsGO = new GameObject("Rows", typeof(RectTransform));
            rowsGO.transform.SetParent(panelRect, false);
            var rowsRect = (RectTransform)rowsGO.transform;
            rowsRect.anchorMin = new Vector2(0f, 1f);
            rowsRect.anchorMax = new Vector2(1f, 1f);
            rowsRect.pivot = new Vector2(0.5f, 1f);
            rowsRect.sizeDelta = new Vector2(0f, RowHeight * 8f);
            rowsRect.anchoredPosition = new Vector2(0f, -RowsTop);
            rowsParent = rowsRect;

            messageText = CreateText(panelRect, "Message", string.Empty, UITheme.FontBody,
                UITheme.TextSecondary, TextAnchor.MiddleCenter);
            var messageRect = (RectTransform)messageText.transform;
            messageRect.anchorMin = new Vector2(0f, 0f);
            messageRect.anchorMax = new Vector2(1f, 0f);
            messageRect.pivot = new Vector2(0.5f, 0f);
            messageRect.sizeDelta = new Vector2(-24f, 40f);
            messageRect.anchoredPosition = new Vector2(0f, 66f);

            CreateActionButton(panelRect, "ResetButton", "Restaurar padrões",
                new Vector2(0.5f, 0f), new Vector2(-104f, 18f), new Vector2(196f, 44f), () =>
                {
                    CancelActiveOperation();
                    KeyRebindService.ResetAll();
                    RefreshKeyLabels();
                    SetMessage("Controles restaurados.");
                    AudioManager.PlayUiClick();
                });

            CreateActionButton(panelRect, "CloseButton", "Fechar",
                new Vector2(0.5f, 0f), new Vector2(104f, 18f), new Vector2(196f, 44f), Close);

            root.SetActive(false);
        }

        private void CreateActionButton(RectTransform parent, string name, string label,
            Vector2 anchor, Vector2 position, Vector2 size, UnityEngine.Events.UnityAction onClick)
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
            button.colors = ButtonColors();
            button.onClick.AddListener(onClick);

            Text text = CreateText(rect, "Label", label, UITheme.FontButton, UITheme.TextPrimary,
                TextAnchor.MiddleCenter);
            Stretch((RectTransform)text.transform);
        }

        /// <summary>
        /// A Image do botão fica branca e são estas cores que pintam cada estado —
        /// o ColorBlock multiplica pela cor da Image, então manter a Image neutra deixa
        /// os valores do UITheme saírem exatamente como estão definidos.
        /// </summary>
        private static ColorBlock ButtonColors()
        {
            ColorBlock colors = ColorBlock.defaultColorBlock;
            colors.normalColor = UITheme.ButtonNormal;
            colors.highlightedColor = UITheme.ButtonHighlight;
            colors.pressedColor = UITheme.ButtonPressed;
            colors.selectedColor = UITheme.ButtonHighlight;
            colors.disabledColor = UITheme.ButtonDisabled;
            colors.fadeDuration = 0.08f;
            return colors;
        }

        private static Text CreateText(Transform parent, string name, string content, int fontSize,
            Color color, TextAnchor anchor)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var text = go.AddComponent<Text>();
            text.text = content;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = anchor;
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
