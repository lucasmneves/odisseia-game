using UnityEngine;
using UnityEngine.InputSystem;
using Odisseia.Core;
using Odisseia.Player;
using Odisseia.Systems;
using Odisseia.UI;

namespace Odisseia.Levels
{
    /// <summary>
    /// Ponto de interação com um objeto do cenário: barris de suprimento, pilha de
    /// escudos, remos, velas, o casco de um navio.
    ///
    /// É o irmão de cenário do <see cref="NPCDialogue"/>. Os dois usam a mesma ação
    /// Interact e o mesmo <see cref="TutorialPrompt"/>; a diferença é o que acontece
    /// depois: NPC é conversa (e dá para repetir), objeto é tarefa (acontece uma vez,
    /// muda de aparência e conta para o objetivo do ato). Juntar os dois num componente
    /// só daria um componente cheio de "se for objeto, então...".
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class InteractPoint : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private string actionMapName = "Player";
        [SerializeField] private string interactActionName = "Interact";

        [Header("Mensagens")]
        [Tooltip("Chave da dica mostrada ao chegar perto. Aceita {0} = botão de interagir.")]
        [SerializeField] private string promptKey = "ui.interact.prompt";
        [Tooltip("Chave da mensagem mostrada ao usar o ponto.")]
        [SerializeField] private string resultKey;
        [SerializeField] private float resultDuration = 2.5f;
        [SerializeField] private TutorialPrompt prompt;

        [Header("Efeito")]
        [SerializeField] private LevelObjective objective;
        [Tooltip("Fala opcional — usada pelos pontos que valem uma cena (o ferreiro).")]
        [SerializeField] private DialogueSequence dialogue;
        [SerializeField] private PlayerInputLock playerLock;
        [SerializeField] private GameObject[] enableOnUse;
        [SerializeField] private SpriteRenderer doneRenderer;
        [SerializeField] private Color doneColor = new Color(0.45f, 0.75f, 0.45f);

        private InputAction interactAction;
        private bool playerInRange;
        private bool used;
        private bool busy;

        public bool Used => used;

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;

            if (inputActions != null)
            {
                InputActionMap map = inputActions.FindActionMap(actionMapName, throwIfNotFound: false);
                interactAction = map?.FindAction(interactActionName);
            }
        }

        private void OnEnable()
        {
            if (interactAction != null)
            {
                interactAction.performed += OnInteractPerformed;
            }

            InputDeviceTracker.Changed += RefrescarDica;
        }

        private void OnDisable()
        {
            if (interactAction != null)
            {
                interactAction.performed -= OnInteractPerformed;
            }

            InputDeviceTracker.Changed -= RefrescarDica;
            playerInRange = false;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            playerInRange = true;

            // Fica na tela enquanto o jogador estiver perto: uma dica que some sozinha
            // deixa o ponto parecendo cenário.
            if (!used && !busy)
            {
                prompt?.ShowPersistent(this, ControlHints.Instruction(promptKey, "Interact"));
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = false;

                if (!busy)
                {
                    prompt?.Hide(this);
                }
            }
        }

        /// <summary>Reescreve a dica quando o jogador troca de dispositivo.</summary>
        private void RefrescarDica()
        {
            if (playerInRange && !used && !busy)
            {
                prompt?.ShowPersistent(this, ControlHints.Instruction(promptKey, "Interact"));
            }
        }

        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            if (!playerInRange || used || busy)
            {
                return;
            }

            used = true;
            busy = true;

            if (doneRenderer != null)
            {
                doneRenderer.color = doneColor;
            }

            Sprite sprite = GameAssets.Instance != null ? GameAssets.Instance.PlaceholderSprite : null;
            VfxBurst.Spawn(sprite, transform.position, doneColor, 6, 2.5f, 0.3f);
            AudioManager.PlayCollect();

            foreach (GameObject go in enableOnUse ?? System.Array.Empty<GameObject>())
            {
                if (go != null)
                {
                    go.SetActive(true);
                }
            }

            if (dialogue != null)
            {
                playerLock?.SetLocked(true);
                dialogue.Completed += OnDialogueCompleted;
                dialogue.Play();
                return;
            }

            if (!string.IsNullOrEmpty(resultKey))
            {
                prompt?.Show(this, Localization.Get(resultKey), resultDuration);
            }

            Finish();
        }

        private void OnDialogueCompleted()
        {
            dialogue.Completed -= OnDialogueCompleted;
            playerLock?.SetLocked(false);
            Finish();
        }

        private void Finish()
        {
            busy = false;
            objective?.Report(this);
        }
    }
}
