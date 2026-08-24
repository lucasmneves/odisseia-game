using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Odisseia.Core;
using Odisseia.Systems;
using Odisseia.WorldMap;

namespace Odisseia.UI
{
    /// <summary>
    /// Menu principal: Continuar, Novo Jogo, Seletor de Fases e Configurações.
    ///
    /// Continuar e Novo Jogo levam ao mapa da jornada — é o mapa que sabe posicionar
    /// Odisseu no ponto de progresso e mostrar o que já foi conquistado.
    ///
    /// A lógica de cada ação mora num método daqui, não no <c>onClick</c> do Inspector:
    /// assim dá para ler o fluxo do menu inteiro num arquivo só, e teclado/controle
    /// disparam exatamente o mesmo caminho do mouse.
    /// </summary>
    public class MainMenuController : MonoBehaviour, ICancelHandler
    {
        [Header("Botões")]
        [SerializeField] private Button continueButton;
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button levelSelectButton;
        [SerializeField] private Button settingsButton;

        [Header("Navegação")]
        [SerializeField] private MenuNavigator navigator;

        [Header("Aviso")]
        [Tooltip("Texto curto mostrado abaixo do menu. Vazio, o aviso é só ignorado.")]
        [SerializeField] private Text noticeText;
        [SerializeField] private float noticeDuration = 2.4f;

        [Header("Cenas")]
        [SerializeField] private string levelSelectSceneName = SceneLoader.LevelSelect;

        private float noticeTimer;

        private void Awake()
        {
            // Toda jornada começa aqui — por Novo Jogo, por Continuar ou depois de um
            // fim de jogo. Reiniciar no carregamento do menu cobre os três caminhos num
            // lugar só, inclusive quem passa pelo Seletor de Fases.
            LivesCounter.BeginRun();
            ExperienceCounter.BeginRun();

            SettingsManager.Initialize();
            EventSystemBootstrap.EnsureExists();

            continueButton?.onClick.AddListener(OnContinueClicked);
            newGameButton?.onClick.AddListener(OnNewGameClicked);
            levelSelectButton?.onClick.AddListener(OnLevelSelectClicked);
            settingsButton?.onClick.AddListener(OnSettingsClicked);

            RefreshContinueAvailability();
            HideNotice();
        }

        private void OnEnable()
        {
            SettingsScreen.Closed += OnSettingsClosed;
        }

        private void OnDisable()
        {
            SettingsScreen.Closed -= OnSettingsClosed;
        }

        private void Start()
        {
            // A música é responsabilidade do SceneAudio da cena, não daqui: com os dois
            // mandando, um sobrescreveria o outro e a faixa reiniciaria ao abrir o menu.
            FocusFirstAction();
        }

        private void Update()
        {
            if (noticeTimer > 0f)
            {
                noticeTimer -= Time.unscaledDeltaTime;
                if (noticeTimer <= 0f)
                {
                    HideNotice();
                }
            }
        }

        // ---------------------------------------------------------------- estado

        /// <summary>
        /// Continuar só existe se houver save. Sem isso ele viraria um Novo Jogo
        /// disfarçado — o jogador clicaria esperando retomar e perderia o progresso.
        /// </summary>
        private void RefreshContinueAvailability()
        {
            if (continueButton != null)
            {
                continueButton.interactable = SaveSystem.HasSave();
            }

            navigator?.BuildNavigation();
        }

        /// <summary>Foco inicial no Continuar; sem save, no Novo Jogo.</summary>
        private void FocusFirstAction()
        {
            if (navigator != null)
            {
                navigator.FocusFirstAvailable();
                return;
            }

            Button alvo = continueButton != null && continueButton.interactable ? continueButton : newGameButton;
            if (alvo != null)
            {
                EventSystem.current?.SetSelectedGameObject(alvo.gameObject);
            }
        }

        // ---------------------------------------------------------------- ações

        /// <summary>
        /// Continuar leva ao mapa, não direto a uma fase: o mapa é que sabe posicionar
        /// Odisseu no ponto de progresso e mostrar o que já foi conquistado.
        /// </summary>
        private void OnContinueClicked()
        {
            if (!SaveSystem.HasSave())
            {
                // Só acontece se o save sumir entre abrir o menu e clicar.
                RefreshContinueAvailability();
                ShowNotice(Localization.Get("ui.menu.noSave"));
                return;
            }

            // Sem recado de fase recém-jogada, o mapa cai no critério dele: a concluída
            // mais avançada, ou o começo da jornada.
            WorldMapSession.Clear();
            SceneLoader.Load(SceneLoader.WorldMap);
        }

        /// <summary>
        /// Novo Jogo apaga o progresso, então pede confirmação quando há o que perder.
        /// Sem save, começa direto — não há nada a confirmar.
        /// </summary>
        private void OnNewGameClicked()
        {
            if (!SaveSystem.HasSave())
            {
                StartNewGame();
                return;
            }

            ConfirmDialog.Show(
                Localization.Get("ui.confirm.newGame.title"),
                Localization.Get("ui.confirm.newGame.body"),
                Localization.Get("ui.confirm.yes"),
                Localization.Get("ui.confirm.no"),
                StartNewGame);
        }

        private void StartNewGame()
        {
            CampaignManager.Instance?.StartNewGame();
            WorldMapSession.Clear();
            RefreshContinueAvailability();
            SceneLoader.Load(SceneLoader.WorldMap);
        }

        private void OnLevelSelectClicked()
        {
            SceneLoader.Load(levelSelectSceneName);
        }

        private void OnSettingsClicked()
        {
            SettingsScreen.Open();
        }

        private void OnSettingsClosed()
        {
            // Volume mudou lá dentro; nada mais a recarregar, mas o foco tem que voltar.
            FocusFirstAction();
        }

        /// <summary>ESC / botão B no menu principal: não há para onde voltar.</summary>
        public void OnCancel(BaseEventData eventData)
        {
        }

        // ---------------------------------------------------------------- aviso

        private void ShowNotice(string message)
        {
            if (noticeText == null)
            {
                Debug.Log("[MainMenu] " + message);
                return;
            }

            noticeText.text = message;
            noticeText.gameObject.SetActive(true);
            noticeTimer = noticeDuration;
        }

        private void HideNotice()
        {
            noticeTimer = 0f;
            noticeText?.gameObject.SetActive(false);
        }

        /// <summary>Itens do menu, de cima para baixo — usado pelo navegador.</summary>
        public IEnumerable<Button> MenuItems
        {
            get
            {
                yield return continueButton;
                yield return newGameButton;
                yield return levelSelectButton;
                yield return settingsButton;
            }
        }
    }
}
