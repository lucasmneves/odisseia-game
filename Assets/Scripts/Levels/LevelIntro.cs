using UnityEngine;
using Odisseia.Player;
using Odisseia.Systems;
using Odisseia.UI;

namespace Odisseia.Levels
{
    /// <summary>
    /// Cutscene de abertura da fase: reseta progresso de checkpoint/coletáveis,
    /// trava o jogador, toca o diálogo introdutório e libera o controle ao final.
    /// </summary>
    public class LevelIntro : MonoBehaviour
    {
        [SerializeField] private PlayerInputLock playerLock;
        [SerializeField] private DialogueSequence introDialogue;

        private void Awake()
        {
            CheckpointManager.Reset();
            CollectibleCounter.Reset();
        }

        private void Start()
        {
            playerLock?.SetLocked(true);

            if (introDialogue != null)
            {
                introDialogue.Completed += OnIntroCompleted;
                introDialogue.Play();
            }
            else
            {
                OnIntroCompleted();
            }
        }

        private void OnIntroCompleted()
        {
            if (introDialogue != null)
            {
                introDialogue.Completed -= OnIntroCompleted;
            }

            playerLock?.SetLocked(false);
        }
    }
}
