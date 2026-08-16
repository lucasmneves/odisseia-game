using UnityEngine;
using UnityEngine.InputSystem;
using Odisseia.Player;
using Odisseia.UI;

namespace Odisseia.Levels
{
    /// <summary>
    /// NPC conversável: ao entrar no alcance, mostra uma dica ("Pressione E para
    /// conversar", reaproveitando TutorialPrompt); pressionar Interact toca a
    /// DialogueSequence do NPC. Pode ser conversado mais de uma vez. Usado pelos NPCs
    /// das Fases 13, 14 e 15 — nenhuma lógica nova por personagem, só dados diferentes.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class NPCDialogue : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private string actionMapName = "Player";
        [SerializeField] private string interactActionName = "Interact";

        [Header("Diálogo")]
        [SerializeField] private DialogueSequence dialogue;
        [SerializeField] private PlayerInputLock playerLock;
        [SerializeField] private TutorialPrompt prompt;
        [SerializeField] private string promptMessage = "Pressione E para conversar";

        private InputActionMap playerMap;
        private InputAction interactAction;
        private bool playerInRange;
        private bool talking;

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;

            if (inputActions != null)
            {
                playerMap = inputActions.FindActionMap(actionMapName, throwIfNotFound: false);
                interactAction = playerMap?.FindAction(interactActionName);
            }
        }

        private void OnEnable()
        {
            if (interactAction != null)
            {
                interactAction.performed += OnInteractPerformed;
            }
        }

        private void OnDisable()
        {
            if (interactAction != null)
            {
                interactAction.performed -= OnInteractPerformed;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            playerInRange = true;

            if (!talking)
            {
                prompt?.Show(promptMessage, 2f);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = false;
            }
        }

        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            if (!playerInRange || talking || dialogue == null)
            {
                return;
            }

            talking = true;
            playerLock?.SetLocked(true);
            dialogue.Completed += OnDialogueCompleted;
            dialogue.Play();
        }

        private void OnDialogueCompleted()
        {
            dialogue.Completed -= OnDialogueCompleted;
            talking = false;
            playerLock?.SetLocked(false);
        }
    }
}
