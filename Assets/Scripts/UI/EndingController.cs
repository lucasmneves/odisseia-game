using UnityEngine;
using UnityEngine.UI;
using Odisseia.Core;
using Odisseia.Systems;

namespace Odisseia.UI
{
    /// <summary>
    /// Tela final da campanha: Jogar novamente (reseta o progresso e volta à Fase 1)
    /// ou Voltar ao menu.
    /// </summary>
    public class EndingController : MonoBehaviour
    {
        [SerializeField] private Button playAgainButton;
        [SerializeField] private Button backToMenuButton;
        [SerializeField] private string firstLevelSceneName = SceneLoader.Level01Troia;

        private void Awake()
        {
            playAgainButton?.onClick.AddListener(OnPlayAgain);
            backToMenuButton?.onClick.AddListener(OnBackToMenu);
        }

        private void OnPlayAgain()
        {
            CampaignManager.Instance?.StartNewGame();
            SceneLoader.Load(firstLevelSceneName);
        }

        private void OnBackToMenu()
        {
            SceneLoader.Load(SceneLoader.MainMenu);
        }
    }
}
