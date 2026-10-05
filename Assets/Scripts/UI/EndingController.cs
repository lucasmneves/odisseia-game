using UnityEngine;
using UnityEngine.UI;
using Odisseia.Core;
using Odisseia.Systems;

namespace Odisseia.UI
{
    /// <summary>
    /// Tela final da campanha: Jogar novamente (reseta o progresso e volta ao prólogo em Ítaca)
    /// ou Voltar ao menu.
    /// </summary>
    public class EndingController : MonoBehaviour
    {
        [SerializeField] private Button playAgainButton;
        [SerializeField] private Button backToMenuButton;
        [SerializeField] private string firstLevelSceneName = SceneLoader.FirstLevel;

        private void Awake()
        {
            playAgainButton?.onClick.AddListener(OnPlayAgain);
            backToMenuButton?.onClick.AddListener(OnBackToMenu);
        }

        /// <summary>
        /// Foco para teclado e controle (começa em Jogar novamente). No Start pelo mesmo motivo
        /// do <see cref="LevelCompleteMenu"/>: o EventSystem da cena já está ligado.
        /// </summary>
        private void Start()
        {
            gameObject.AddComponent<MenuNavigator>().SetItems(new[] { playAgainButton, backToMenuButton });
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
