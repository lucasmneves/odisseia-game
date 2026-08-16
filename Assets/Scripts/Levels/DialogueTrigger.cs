using UnityEngine;
using Odisseia.Player;
using Odisseia.UI;

namespace Odisseia.Levels
{
    /// <summary>
    /// Toca um diálogo narrativo ao ser tocado pelo jogador pela primeira vez, no meio
    /// da fase (diferente de LevelIntro/LevelGoal, que só cobrem abertura/fechamento).
    /// Reaproveita DialogueSequence e PlayerInputLock exatamente como as cutscenes.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class DialogueTrigger : MonoBehaviour
    {
        [SerializeField] private DialogueSequence dialogue;
        [SerializeField] private PlayerInputLock playerLock;
        [SerializeField] private bool lockPlayer = true;

        private bool triggered;

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (triggered || !other.CompareTag("Player") || dialogue == null)
            {
                return;
            }

            triggered = true;

            if (lockPlayer)
            {
                playerLock?.SetLocked(true);
            }

            dialogue.Completed += OnCompleted;
            dialogue.Play();
        }

        private void OnCompleted()
        {
            dialogue.Completed -= OnCompleted;

            if (lockPlayer)
            {
                playerLock?.SetLocked(false);
            }
        }
    }
}
