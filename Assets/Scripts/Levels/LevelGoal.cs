using UnityEngine;
using Odisseia.Core;
using Odisseia.Player;
using Odisseia.Systems;
using Odisseia.UI;

namespace Odisseia.Levels
{
    /// <summary>
    /// Objetivo da fase: ao ser alcançado pelo jogador, opcionalmente toca um
    /// diálogo de encerramento e carrega a próxima cena.
    ///
    /// A passagem é direta — fase, tela de carregamento, próxima fase. Não há mais
    /// tela de "fase concluída" no meio pedindo um clique.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class LevelGoal : MonoBehaviour
    {
        [SerializeField] private string nextSceneName = SceneLoader.MainMenu;
        [SerializeField] private DialogueSequence outroDialogue;
        [SerializeField] private PlayerInputLock playerLock;
        [SerializeField] private LevelManager levelManager;

        private bool triggered;

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (triggered || !other.CompareTag("Player"))
            {
                return;
            }

            triggered = true;
            levelManager?.CompleteLevel();

            if (outroDialogue != null)
            {
                playerLock?.SetLocked(true);
                outroDialogue.Completed += OnOutroCompleted;
                outroDialogue.Play();
            }
            else
            {
                GoToNextScene();
            }
        }

        private void OnOutroCompleted()
        {
            outroDialogue.Completed -= OnOutroCompleted;
            GoToNextScene();
        }

        private void GoToNextScene()
        {
            SceneLoader.LoadWithLoadingScreen(nextSceneName, ResolveNextTitle());
        }

        /// <summary>
        /// Nome de exibição da próxima fase, para o loader anunciar aonde o jogador está
        /// indo. Sem CampaignManager (fase aberta direto no Editor) volta null e o loader
        /// mostra só "Carregando...".
        /// </summary>
        private string ResolveNextTitle()
        {
            CampaignManager campaign = CampaignManager.Instance;
            if (campaign == null || string.IsNullOrEmpty(nextSceneName))
            {
                return null;
            }

            foreach (LevelDefinition level in campaign.Levels)
            {
                if (level != null && level.SceneName == nextSceneName)
                {
                    return level.DisplayName;
                }
            }

            return null;
        }
    }
}
