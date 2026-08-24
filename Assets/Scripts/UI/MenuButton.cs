using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Odisseia.Systems;

namespace Odisseia.UI
{
    /// <summary>
    /// Estados visuais e sonoros de um botão de menu, por cima do <see cref="Button"/>
    /// que a cena já traz — não substitui o Selectable, só reage a ele.
    ///
    /// Existe porque o ColorBlock do Unity só troca cor: não dá deslocamento, não toca
    /// som e, principalmente, não distingue "o mouse passou por cima" de "o teclado
    /// selecionou". Num menu que precisa funcionar igual no mouse, no teclado e no
    /// controle, esses dois casos têm que produzir o mesmo destaque — senão navegar de
    /// controle fica sem feedback nenhum.
    ///
    /// O visual segue o <see cref="UITheme"/>: azul escuro com borda dourada em repouso,
    /// azul mais claro e um empurrãozinho para a direita em destaque, comprimido ao
    /// pressionar, acinzentado quando desabilitado. Nada de glow ou blur.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class MenuButton : MonoBehaviour,
        IPointerEnterHandler, IPointerExitHandler,
        IPointerDownHandler, IPointerUpHandler,
        ISelectHandler, IDeselectHandler
    {
        [Header("Destaque")]
        [Tooltip("Deslocamento horizontal, em pixels, quando o item está em foco.")]
        [SerializeField] private float highlightShift = 8f;
        [SerializeField] private float highlightScale = 1.03f;

        [Header("Pressionado")]
        [SerializeField] private float pressedScale = 0.97f;

        [Header("Som")]
        [SerializeField] private bool playHoverSound = true;
        [SerializeField] private bool playConfirmSound = true;

        [Header("Marcador de foco (opcional)")]
        [Tooltip("Ornamento mostrado só quando o item está em foco.")]
        [SerializeField] private GameObject focusMarker;

        private Button button;
        private Image background;
        private Text[] labels;
        private Color[] labelBaseColors;
        private RectTransform rect;

        private Vector2 basePosition;
        private Vector3 baseScale;
        private bool layoutDriven;
        private bool highlighted;
        private bool pressed;
        private bool wasInteractable = true;

        private void Awake()
        {
            button = GetComponent<Button>();
            background = GetComponent<Image>();
            rect = (RectTransform)transform;

            // Todos os rótulos, não só o primeiro: uma entrada da seleção de fases tem
            // número, nome e marca de estado. Recolorir só um deixaria o nome branco e
            // vivo numa fase bloqueada, que é o oposto do que o estado quer dizer.
            //
            // A cor de cada rótulo é guardada como base e o estado desloca a partir
            // dela, para o dourado do número e o do "concluída" não se perderem.
            labels = GetComponentsInChildren<Text>(includeInactive: true);
            labelBaseColors = new Color[labels.Length];
            for (int i = 0; i < labels.Length; i++)
            {
                labelBaseColors[i] = labels[i].color;
            }

            basePosition = rect.anchoredPosition;
            baseScale = rect.localScale;

            // Sob um grupo de layout, quem manda na posição é o grupo. Escrever
            // anchoredPosition aqui brigaria com ele: a posição guardada no Awake é
            // anterior ao primeiro cálculo de layout, então destacar o item o
            // teleportaria para um lugar antigo — na prática, ele some da lista.
            //
            // Nesses casos o destaque fica só na cor e na escala, que o layout não
            // controla. Fora de layout (o menu principal), o deslocamento continua.
            layoutDriven = GetComponentInParent<LayoutGroup>() != null;

            // O Button continua responsável pelo clique; a cor fica com este componente,
            // senão os dois brigam pelo mesmo Image a cada frame.
            button.transition = Selectable.Transition.None;
        }

        private void OnEnable()
        {
            highlighted = false;
            pressed = false;
            wasInteractable = button.interactable;
            Apply();
        }

        private void Update()
        {
            // interactable pode mudar a qualquer momento (o Continuar depende de existir
            // save), e Selectable não avisa ninguém quando muda.
            if (button.interactable != wasInteractable)
            {
                wasInteractable = button.interactable;

                if (!wasInteractable)
                {
                    highlighted = false;
                    pressed = false;
                }

                Apply();
            }
        }

        // ---------------------------------------------------------------- eventos

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!button.interactable)
            {
                return;
            }

            // Mouse por cima também move o foco do EventSystem: assim o teclado continua
            // de onde o mouse parou, em vez de saltar para outro item.
            EventSystem.current?.SetSelectedGameObject(gameObject);
            SetHighlighted(true, withSound: playHoverSound);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            pressed = false;

            if (EventSystem.current == null || EventSystem.current.currentSelectedGameObject != gameObject)
            {
                SetHighlighted(false, withSound: false);
            }
            else
            {
                Apply();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!button.interactable)
            {
                return;
            }

            pressed = true;
            Apply();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!pressed)
            {
                return;
            }

            pressed = false;
            Apply();

            if (playConfirmSound && button.interactable)
            {
                AudioManager.PlayUiConfirm();
            }
        }

        /// <summary>Foco via teclado/controle — mesmo destaque do mouse.</summary>
        public void OnSelect(BaseEventData eventData) => SetHighlighted(true, withSound: playHoverSound);

        public void OnDeselect(BaseEventData eventData) => SetHighlighted(false, withSound: false);

        // ---------------------------------------------------------------- visual

        private void SetHighlighted(bool value, bool withSound)
        {
            if (highlighted == value)
            {
                return;
            }

            highlighted = value && button.interactable;

            if (highlighted && withSound)
            {
                AudioManager.PlayUiHover();
            }

            Apply();
        }

        private void Apply()
        {
            if (background != null)
            {
                background.color = !button.interactable
                    ? UITheme.ButtonDisabled
                    : pressed
                        ? UITheme.ButtonPressed
                        : highlighted
                            ? UITheme.ButtonHighlight
                            : UITheme.ButtonNormal;
            }

            if (labels != null)
            {
                for (int i = 0; i < labels.Length; i++)
                {
                    if (labels[i] == null)
                    {
                        continue;
                    }

                    Color baseColor = labelBaseColors[i];

                    labels[i].color = !button.interactable
                        ? Color.Lerp(baseColor, UITheme.TextSecondary * 0.55f, 0.75f)
                        : highlighted
                            ? Color.Lerp(baseColor, UITheme.TextAccent, 0.6f)
                            : baseColor;
                }
            }

            if (rect != null)
            {
                float scale = pressed ? pressedScale : highlighted ? highlightScale : 1f;
                rect.localScale = baseScale * scale;

                if (!layoutDriven)
                {
                    rect.anchoredPosition = basePosition +
                        new Vector2(highlighted && !pressed ? highlightShift : 0f, 0f);
                }
            }

            if (focusMarker != null)
            {
                focusMarker.SetActive(highlighted && button.interactable);
            }
        }

        /// <summary>
        /// Reancora o repouso. Necessário quando a posição é definida em runtime depois
        /// do Awake — senão o botão volta para a posição de origem no primeiro hover.
        /// </summary>
        public void RefreshBasePosition()
        {
            if (rect != null)
            {
                basePosition = rect.anchoredPosition;
            }
        }
    }
}
