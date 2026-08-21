using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Odisseia.Core;
using Odisseia.Systems;

namespace Odisseia.UI
{
    /// <summary>
    /// Menu principal: Continue (retoma a fase desbloqueada mais avançada),
    /// New Game (reseta o progresso e começa do zero), Level Select e Settings.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Button continueButton;
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button levelSelectButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private string levelSelectSceneName = SceneLoader.LevelSelect;
        [SerializeField] private string firstLevelSceneName = SceneLoader.Level01Troia;

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

        private void OnContinueClicked()
        {
            string sceneName = firstLevelSceneName;
            CampaignManager campaign = CampaignManager.Instance;

            if (campaign != null)
            {
                LevelDefinition target = campaign.Levels
                    .Where(level => campaign.IsUnlocked(level.LevelId))
                    .OrderByDescending(level => level.Order)
                    .FirstOrDefault();

                if (target != null)
                {
                    sceneName = target.SceneName;
                }
            }

            SceneLoader.Load(sceneName);
        }

        private void OnNewGameClicked()
        {
            CampaignManager.Instance?.StartNewGame();
            SceneLoader.Load(firstLevelSceneName);
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
