using UnityEngine;
using Odisseia.Player;
using Odisseia.UI;

namespace Odisseia.Levels
{
    /// <summary>
    /// Desafio final inspirado no arco de Odisseu: o jogador precisa passar por uma
    /// sequência de "argolas" (AxeRing) na ordem certa. Ao completar, revela o
    /// objetivo da fase (que começa desativado) e toca uma fala de clímax.
    /// </summary>
    public class BowChallenge : MonoBehaviour
    {
        [SerializeField] private AxeRing[] rings;
        [SerializeField] private GameObject goalToReveal;
        [SerializeField] private DialogueSequence completionDialogue;
        [SerializeField] private PlayerInputLock playerLock;

        private int nextIndex;
        private bool completed;

        private void Awake()
        {
            if (goalToReveal != null)
            {
                goalToReveal.SetActive(false);
            }
        }

        public void NotifyRingEntered(AxeRing ring)
        {
            if (completed || rings == null)
            {
                return;
            }

            int index = System.Array.IndexOf(rings, ring);
            if (index != nextIndex)
            {
                return;
            }

            nextIndex++;
            if (nextIndex >= rings.Length)
            {
                completed = true;
                Complete();
            }
        }

        private void Complete()
        {
            if (goalToReveal != null)
            {
                goalToReveal.SetActive(true);
            }

            if (completionDialogue != null)
            {
                playerLock?.SetLocked(true);
                completionDialogue.Completed += OnDialogueCompleted;
                completionDialogue.Play();
            }
        }

        private void OnDialogueCompleted()
        {
            completionDialogue.Completed -= OnDialogueCompleted;
            playerLock?.SetLocked(false);
        }
    }
}
