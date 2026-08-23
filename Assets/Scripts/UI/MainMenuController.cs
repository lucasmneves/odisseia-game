using UnityEngine;
using UnityEngine.UI;
using Odisseia.Core;
using Odisseia.Systems;
using Odisseia.WorldMap;

namespace Odisseia.UI
{
    /// <summary>
    /// Menu principal: Continue e New Game levam ao mapa da jornada (o mapa é que
    /// posiciona Odisseu no ponto de progresso), além de Level Select e Settings.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Button continueButton;
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button levelSelectButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private string levelSelectSceneName = SceneLoader.LevelSelect;

        private void Awake()
        {
            // Toda jornada começa aqui — por Novo Jogo, por Continuar ou depois de um
            // fim de jogo. Reiniciar no carregamento do menu cobre os três caminhos num
            // lugar só, inclusive quem passa pelo Seletor de Fases.
            LivesCounter.BeginRun();
            ExperienceCounter.BeginRun();

            continueButton?.onClick.AddListener(OnContinueClicked);
            newGameButton?.onClick.AddListener(OnNewGameClicked);
            levelSelectButton?.onClick.AddListener(OnLevelSelectClicked);
            settingsButton?.onClick.AddListener(OnSettingsClicked);

            if (continueButton != null)
            {
                continueButton.interactable = SaveSystem.HasSave();
            }
        }

        /// <summary>
        /// Continuar leva ao mapa, não direto a uma fase: o mapa é que sabe posicionar
        /// Odisseu no ponto de progresso e mostrar o que já foi conquistado.
        /// </summary>
        private void OnContinueClicked()
        {
            // Sem recado de fase recém-jogada, o mapa cai no critério dele: a concluída
            // mais avançada, ou o começo da jornada.
            WorldMapSession.Clear();
            SceneLoader.Load(SceneLoader.WorldMap);
        }

        private void OnNewGameClicked()
        {
            CampaignManager.Instance?.StartNewGame();
            WorldMapSession.Clear();
            SceneLoader.Load(SceneLoader.WorldMap);
        }

        private void OnLevelSelectClicked()
        {
            SceneLoader.Load(levelSelectSceneName);
        }

        private void OnSettingsClicked()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(!settingsPanel.activeSelf);
            }
        }
    }
}
