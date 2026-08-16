using UnityEngine;
using Odisseia.Player;
using Odisseia.Systems;
using Odisseia.UI;

namespace Odisseia.Levels
{
    /// <summary>
    /// Objetivo da fase: ao ser alcançado pelo jogador, opcionalmente toca um
    /// diálogo de encerramento e carrega a próxima cena.
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
                SceneLoader.Load(nextSceneName);
            }
        }

        private void OnOutroCompleted()
        {
            outroDialogue.Completed -= OnOutroCompleted;
            SceneLoader.Load(nextSceneName);
        }
    }
}
