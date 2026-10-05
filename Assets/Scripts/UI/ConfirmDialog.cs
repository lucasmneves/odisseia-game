using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Odisseia.Systems;

namespace Odisseia.UI
{
    /// <summary>
    /// Confirmação modal reutilizável (título, mensagem, Confirmar/Cancelar).
    ///
    /// Monta o próprio Canvas em runtime e persiste entre cenas, no mesmo padrão de
    /// <see cref="ScreenFader"/>, <see cref="LoadingScreen"/> e <see cref="OptionsMenu"/>
    /// — assim qualquer tela pode pedir uma confirmação sem que ninguém precise
    /// adicionar prefab ou editar cena.
    ///
    /// Enquanto está aberto ele toma o foco do EventSystem e o devolve ao fechar; sem
    /// isso o menu atrás continuaria respondendo a teclado e controle por baixo do
    /// diálogo.
    /// </summary>
    public class ConfirmDialog : MonoBehaviour
    {
        private const float ReferenceWidth = 1280f;
        private const float ReferenceHeight = 720f;

        private static ConfirmDialog instance;

        private GameObject root;
        private Text titleText;
        private Text messageText;
        private Button confirmButton;
        private Button cancelButton;

        private Action onConfirm;
        private GameObject previousSelection;

        public static bool IsOpen => instance != null && instance.root != null && instance.root.activeSelf;

        private static ConfirmDialog Instance
        {
            get
            {
                if (instance == null)
                {
                    var go = new GameObject("ConfirmDialog");
                    instance = go.AddComponent<ConfirmDialog>();
                    DontDestroyOnLoad(go);
                    instance.Build();
                }

                return instance;
            }
        }

        /// <summary>Abre a confirmação. <paramref name="confirm"/> só roda no "sim".</summary>
        public static void Show(string title, string message, string confirmLabel, string cancelLabel, Action confirm)
        {
            Instance.Open(title, message, confirmLabel, cancelLabel, confirm);
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

        private void Update()
        {
            if (!IsOpen)
            {
                return;
            }

            bool cancelou =
                (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) ||
                (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame);

            if (cancelou)
            {
                Cancel();
            }
        }

        // ---------------------------------------------------------------- fluxo

        private void Open(string title, string message, string confirmLabel, string cancelLabel, Action confirm)
        {
            EventSystemBootstrap.EnsureExists();

            onConfirm = confirm;
            titleText.text = title;
            messageText.text = message;

            SetLabel(confirmButton, confirmLabel);
            SetLabel(cancelButton, cancelLabel);

            previousSelection = EventSystem.current != null
                ? EventSystem.current.currentSelectedGameObject
                : null;

            root.SetActive(true);

            // Foco inicial no Cancelar: a ação destrutiva não deve ser a que sai por
            // engano em quem apertar confirmar duas vezes rápido.
            EventSystem.current?.SetSelectedGameObject(cancelButton.gameObject);
        }

        private void Confirm()
        {
            Action acao = onConfirm;
            Close();

            AudioManager.PlayUiConfirm();
            acao?.Invoke();
        }

        private void Cancel()
        {
            Close();
            AudioManager.PlayUiCancel();
        }

        private void Close()
        {
            onConfirm = null;
            root.SetActive(false);

            if (previousSelection != null && previousSelection.activeInHierarchy)
            {
                EventSystem.current?.SetSelectedGameObject(previousSelection);
            }

            previousSelection = null;
        }

        private static void SetLabel(Button button, string text)
        {
            Text label = button.GetComponentInChildren<Text>();
            if (label != null)
            {
                label.text = text;
            }
        }

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

            root = new GameObject("ConfirmDialogCanvas");
            root.transform.SetParent(transform, false);

            var canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = UITheme.Layer.Confirm;

            var scaler = root.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            root.AddComponent<GraphicRaycaster>();

            // Cortina: escurece e, por ser raycast target, impede clique no que está atrás.
            Image cortina = CreateImage(root.transform, "Dim", UITheme.OverlayDim);
            Stretch((RectTransform)cortina.transform);
            cortina.raycastTarget = true;

            // Moldura IRMÃ do painel e criada antes dele: em UI o filho desenha sempre
            // por cima do pai, então uma borda filha cobriria o fundo de dourado e o
            // texto ficaria ilegível.
            Image borda = CreateImage(root.transform, "Border", UITheme.TextAccent);
            var bordaRect = (RectTransform)borda.transform;
            bordaRect.anchorMin = bordaRect.anchorMax = new Vector2(0.5f, 0.5f);
            bordaRect.sizeDelta = new Vector2(566f, 266f);
            bordaRect.anchoredPosition = Vector2.zero;
            borda.raycastTarget = false;

            Image painel = CreateImage(root.transform, "Panel", UITheme.PanelBackground);
            var painelRect = (RectTransform)painel.transform;
            painelRect.anchorMin = painelRect.anchorMax = new Vector2(0.5f, 0.5f);
            painelRect.sizeDelta = new Vector2(560f, 260f);
            painelRect.anchoredPosition = Vector2.zero;

            titleText = CreateText(painel.transform, "Title", UITheme.FontHeading, UITheme.TextAccent);
            Anchor((RectTransform)titleText.transform, new Vector2(0f, 78f), new Vector2(520f, 44f));

            messageText = CreateText(painel.transform, "Message", UITheme.FontBody, UITheme.TextSecondary);
            Anchor((RectTransform)messageText.transform, new Vector2(0f, 14f), new Vector2(500f, 76f));

            confirmButton = CreateButton(painel.transform, "ConfirmButton", new Vector2(-124f, -76f));
            confirmButton.onClick.AddListener(Confirm);

            cancelButton = CreateButton(painel.transform, "CancelButton", new Vector2(124f, -76f));
            cancelButton.onClick.AddListener(Cancel);

            // Esquerda/direita entre os dois botões, em ciclo.
            confirmButton.navigation = new Navigation
            {
                mode = Navigation.Mode.Explicit,
                selectOnRight = cancelButton,
                selectOnLeft = cancelButton,
            };
            cancelButton.navigation = new Navigation
            {
                mode = Navigation.Mode.Explicit,
                selectOnLeft = confirmButton,
                selectOnRight = confirmButton,
            };

            root.SetActive(false);
        }

        private static Image CreateImage(Transform parent, string name, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            var image = go.AddComponent<Image>();
            image.color = color;
            return image;
        }

        private static Text CreateText(Transform parent, string name, int size, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();

            var text = go.AddComponent<Text>();
            text.font = UITheme.Font;
            text.fontSize = size;
            text.color = color;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        private static Button CreateButton(Transform parent, string name, Vector2 position)
        {
            Image image = CreateImage(parent, name, UITheme.ButtonNormal);
            Anchor((RectTransform)image.transform, position, new Vector2(220f, 54f));

            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;

            Text label = CreateText(image.transform, "Label", UITheme.FontButton, UITheme.TextPrimary);
            Stretch((RectTransform)label.transform);

            image.gameObject.AddComponent<MenuButton>();
            return button;
        }

        private static void Anchor(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
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
