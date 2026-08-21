using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Odisseia.Systems;

namespace Odisseia.UI
{
    /// <summary>
    /// Menu de pause das fases. Usa a ação "Pause" do Input System, que existia desde
    /// a primeira etapa sem nenhum sistema ligado a ela.
    /// </summary>
    public class PauseMenu : MonoBehaviour
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

            CreateControlsButton();

            panel?.SetActive(false);
        }

        /// <summary>
        /// Insere "Controles" entre Reiniciar e Menu, clonando um botão existente e
        /// empurrando o de Menu para baixo. Feito em runtime para as 16 fases ganharem
        /// o botão sem editar 16 cenas.
        /// </summary>
        private void CreateControlsButton()
        {
            if (resumeButton == null || menuButton == null || panel == null)
            {
                return;
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

            controls.onClick.RemoveAllListeners();
            controls.onClick.AddListener(OptionsMenu.Open);

            // Menu principal desce uma linha e o painel cresce para acomodar.
            menuRect.anchoredPosition -= new Vector2(0f, spacing);

            if (panel.transform is RectTransform panelRect)
            {
                panelRect.sizeDelta += new Vector2(0f, spacing);
            }
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
