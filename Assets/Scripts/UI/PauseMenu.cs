using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Odisseia.Systems;

namespace Odisseia.UI
{
    /// <summary>
    /// Menu de pause das fases. Usa a ação "Pause" do Input System, que existia desde
    /// a primeira etapa sem nenhum sistema ligado a ela.
    /// </summary>
    public class PauseMenu : MonoBehaviour, ICancelHandler
    {
        [Header("Input")]
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private string actionMapName = "Player";
        [SerializeField] private string pauseActionName = "Pause";

        [Header("UI")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button menuButton;

        private InputAction pauseAction;

        public bool IsPaused { get; private set; }

        private void Awake()
        {
            if (inputActions != null)
            {
                InputActionMap map = inputActions.FindActionMap(actionMapName, throwIfNotFound: false);
                pauseAction = map?.FindAction(pauseActionName);
            }

            resumeButton?.onClick.AddListener(Resume);
            restartButton?.onClick.AddListener(RestartLevel);
            menuButton?.onClick.AddListener(BackToMenu);

            Button controls = CreateControlsButton();
            CreateNavigator(controls);

            panel?.SetActive(false);
        }

        /// <summary>
        /// Insere "Controles" entre Reiniciar e Menu, clonando um botão existente e
        /// empurrando o de Menu para baixo. Feito em runtime para as 16 fases ganharem
        /// o botão sem editar 16 cenas.
        /// </summary>
        private Button CreateControlsButton()
        {
            if (resumeButton == null || menuButton == null || panel == null)
            {
                return null;
            }

            var menuRect = (RectTransform)menuButton.transform;
            var resumeRect = (RectTransform)resumeButton.transform;
            float spacing = Mathf.Abs(resumeRect.anchoredPosition.y - menuRect.anchoredPosition.y) * 0.5f;
            if (spacing <= 1f)
            {
                spacing = resumeRect.sizeDelta.y + 10f;
            }

            Button controls = Instantiate(resumeButton, resumeButton.transform.parent);
            controls.name = "ControlsButton";
            ((RectTransform)controls.transform).anchoredPosition = menuRect.anchoredPosition;

            var label = controls.GetComponentInChildren<Text>();
            if (label != null)
            {
                label.text = "Controles";
            }

            // O clone herda o LocalizedText do Retomar, que reescrevia o rótulo para "Retomar"
            // ao ligar o painel — o pause mostrava dois "Retomar". A chave própria resolve.
            controls.GetComponentInChildren<LocalizedText>(true)?.SetKey("ui.pause.controls");

            controls.onClick.RemoveAllListeners();
            controls.onClick.AddListener(OptionsMenu.Open);

            // Menu principal desce uma linha e o painel cresce para acomodar.
            menuRect.anchoredPosition -= new Vector2(0f, spacing);

            if (panel.transform is RectTransform panelRect)
            {
                panelRect.sizeDelta += new Vector2(0f, spacing);
            }

            return controls;
        }

        /// <summary>
        /// Foco e "voltar" no painel. Sem um item selecionado, o controle abria o pause
        /// (Start) e não conseguia escolher nada — Reiniciar e Menu só existiam para o
        /// mouse. O B/Círculo fecha o pause, como o Start.
        /// </summary>
        private void CreateNavigator(Button controls)
        {
            if (panel == null)
            {
                return;
            }

            Button[] itens = { resumeButton, restartButton, controls, menuButton };

            // Os botões das 16 cenas têm a cor de "selecionado" padrão do Unity (quase branca)
            // com texto branco: com foco, o item ficava um retângulo branco ilegível. Antes
            // ninguém recebia foco e isso não aparecia. Selecionado = destacado, como no resto da UI.
            foreach (Button b in itens)
            {
                if (b != null)
                {
                    ColorBlock cores = b.colors;
                    cores.selectedColor = cores.highlightedColor;
                    b.colors = cores;
                }
            }

            var navigator = panel.AddComponent<MenuNavigator>();
            navigator.SetItems(itens);
            navigator.SetCancelTarget(this);
        }

        public void OnCancel(BaseEventData eventData)
        {
            // O B que fecha a tela de controles aberta por cima não pode fechar o pause junto.
            if (!IsPaused || OptionsMenu.IsOpen || OptionsMenu.ClosedThisFrame)
            {
                return;
            }

            Resume();
        }

        private void OnEnable()
        {
            if (pauseAction != null)
            {
                // O mapa "Player" é desligado durante cutscenes pelo PlayerInputLock, mas
                // a ação de pause precisa continuar respondendo — por isso é habilitada
                // individualmente aqui, além do mapa.
                pauseAction.Enable();
                pauseAction.performed += OnPausePerformed;
            }
        }

        private void OnDisable()
        {
            if (pauseAction != null)
            {
                pauseAction.performed -= OnPausePerformed;
            }

            // Garante que o jogo nunca fique congelado se a cena for descarregada pausada.
            Time.timeScale = 1f;
        }

        private void OnPausePerformed(InputAction.CallbackContext context)
        {
            if (IsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

        public void Pause()
        {
            IsPaused = true;
            Time.timeScale = 0f;
            panel?.SetActive(true);
            AudioManager.PlayUiClick();
        }

        public void Resume()
        {
            IsPaused = false;
            Time.timeScale = 1f;
            panel?.SetActive(false);
            AudioManager.PlayUiClick();
        }

        private void RestartLevel()
        {
            AudioManager.PlayUiClick();
            Time.timeScale = 1f;
            SceneLoader.Load(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

        private void BackToMenu()
        {
            AudioManager.PlayUiClick();
            Time.timeScale = 1f;
            SceneLoader.Load(SceneLoader.MainMenu);
        }
    }
}
