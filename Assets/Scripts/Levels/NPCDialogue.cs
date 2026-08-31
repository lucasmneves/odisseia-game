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

        [Header("Objetivo (opcional)")]
        [Tooltip("Etapa da fase que este NPC ajuda a fechar. Conta uma vez só, na " +
                 "primeira conversa completa. Vazio nas fases que não usam objetivos.")]
        [SerializeField] private LevelObjective objective;

        [Header("Início automático")]
        [Tooltip("Começa a conversa ao chegar perto, sem esperar o botão. Para as falas " +
                 "que travam o progresso da fase — quem não achar o botão fica preso.")]
        [SerializeField] private bool autoStart;

        [Header("Diálogo")]
        [SerializeField] private DialogueSequence dialogue;
        [SerializeField] private PlayerInputLock playerLock;
        [SerializeField] private TutorialPrompt prompt;
        [Tooltip("Reserva. O texto normal vem da tabela de idiomas.")]
        [SerializeField] private string promptMessage = "Pressione E para conversar";

        private InputActionMap playerMap;
        private InputAction interactAction;
        private bool playerInRange;
        private bool talking;
        private bool jaConversou;

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

            // A dica fica na tela enquanto o jogador está por perto; se ele trocar de
            // teclado para controle nesse meio-tempo, ela passa a nomear o botão errado.
            Odisseia.Systems.InputDeviceTracker.Changed += MostrarDica;
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
            PrologueTrace.Log(name + ": jogador ENTROU no alcance" + PrologueTrace.Onde(other));

            // Conversa obrigatória começa sozinha. Depender do botão aqui transforma
            // "não achei a tecla" em "a fase travou": o ato não fecha, o portão
            // seguinte não abre, e nada na tela explica o que faltou.
            if (autoStart && !talking && !jaConversou)
            {
                Conversar();
                return;
            }

            MostrarDica();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = false;
                PrologueTrace.Log(name + ": jogador SAIU do alcance");
                prompt?.Hide(this);
            }
        }

        /// <summary>
        /// A dica fica na tela enquanto o jogador estiver ao alcance, e não por alguns
        /// segundos. Um aviso que some sozinho pune quem passou correndo: o NPC
        /// continua conversável, mas nada mais diz isso, e o jogador segue adiante
        /// achando que ali não havia nada.
        ///
        /// O texto nomeia o botão certo em cada plataforma — tecla no desktop (já
        /// considerando remapeamento) e botão de toque no mobile.
        /// </summary>
        private void MostrarDica()
        {
            if (talking || !playerInRange)
            {
                return;
            }

            prompt?.ShowPersistent(this,
                Odisseia.Systems.Localization.Has("ui.npc.interactPrompt")
                    ? Odisseia.Systems.ControlHints.Instruction("ui.npc.interactPrompt", "Interact")
                    : promptMessage);
        }

        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            if (!playerInRange || talking || dialogue == null)
            {
                PrologueTrace.Log(name + ": botao de interagir chegou, mas ignorado" +
                    " (perto=" + playerInRange + " conversando=" + talking +
                    " temFala=" + (dialogue != null) + ")");
                return;
            }

            PrologueTrace.Log(name + ": INTERAGIU, tocando a fala");
            Conversar();
        }

        private void Conversar()
        {
            talking = true;
            jaConversou = true;
            prompt?.Hide(this);
            playerLock?.SetLocked(true);
            dialogue.Completed += OnDialogueCompleted;
            dialogue.Play();
        }

        private void OnDialogueCompleted()
        {
            dialogue.Completed -= OnDialogueCompleted;
            talking = false;
            PrologueTrace.Log(name + ": fala TERMINOU");
            playerLock?.SetLocked(false);

            // Ainda ao lado do NPC: a dica volta, porque dá para conversar de novo.
            MostrarDica();

            // O objetivo ignora relatos repetidos, então conversar de novo com o mesmo
            // pescador não recruta um segundo homem.
            objective?.Report(this);
        }
    }
}
