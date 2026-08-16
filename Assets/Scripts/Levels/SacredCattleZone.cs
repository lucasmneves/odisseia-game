using UnityEngine;
using UnityEngine.InputSystem;
using Odisseia.Core;
using Odisseia.Player;
using Odisseia.Systems;
using Odisseia.UI;

namespace Odisseia.Levels
{
    /// <summary>
    /// O gado sagrado de Hélio: pode ser comido (Interact) para restaurar a fome
    /// totalmente, mas tem um preço imediato e visível — ensina que a decisão teve
    /// consequência, sem precisar de UI nova além do TutorialPrompt já existente.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class SacredCattleZone : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private string actionMapName = "Player";
        [SerializeField] private string interactActionName = "Interact";

        [Header("Consequência")]
        [SerializeField] private TutorialPrompt prompt;
        [SerializeField] private int consequenceDamage = 20;
        [SerializeField] private string decisionFlagKey = "AteSacredCattle";

        private InputActionMap playerMap;
        private InputAction interactAction;
        private HungerMeter hungerInRange;
        private HealthSystem healthInRange;
        private bool consumed;

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
            if (consumed || !other.TryGetComponent(out HungerMeter hunger))
            {
                return;
            }

            hungerInRange = hunger;
            healthInRange = other.GetComponent<HealthSystem>();
            prompt?.Show("Pressione E para comer o gado sagrado de Hélio (isso terá consequências).", 3f);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.TryGetComponent(out HungerMeter hunger) && hunger == hungerInRange)
            {
                hungerInRange = null;
                healthInRange = null;
            }
        }

        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            if (consumed || hungerInRange == null)
            {
                return;
            }

            consumed = true;
            hungerInRange.RestoreFull();
            healthInRange?.TakeDamage(consequenceDamage);
            DecisionFlags.Set(decisionFlagKey);
            prompt?.Show("Você comeu o gado sagrado. Zeus vai cobrar isso.", 3f);

            if (TryGetComponent(out SpriteRenderer sr))
            {
                Color c = sr.color;
                c.a = 0.3f;
                sr.color = c;
            }
        }
    }
}
