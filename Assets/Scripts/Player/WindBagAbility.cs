using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Odisseia.Player
{
    /// <summary>
    /// Saco dos ventos de Éolo: ao coletar cargas (<see cref="AddCharge"/>), o jogador
    /// pode usar a ação Interact para soltar uma rajada que empurra objetos leves ao
    /// redor (ex.: destroços bloqueando uma passagem). Reaproveita a ação "Interact",
    /// já existente no Input Actions e sem uso até esta fase.
    /// </summary>
    public class WindBagAbility : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private string actionMapName = "Player";
        [SerializeField] private string interactActionName = "Interact";

        [Header("Rajada")]
        [SerializeField] private float gustRadius = 3f;
        [SerializeField] private float gustForce = 8f;
        [SerializeField] private LayerMask affectedLayer;

        private InputActionMap playerMap;
        private InputAction interactAction;

        public int Charges { get; private set; }
        public event Action<int> ChargesChanged;
        public event Action GustUsed;

        private void Awake()
        {
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

        public void AddCharge(int amount = 1)
        {
            Charges += amount;
            ChargesChanged?.Invoke(Charges);
        }

        private void OnInteractPerformed(InputAction.CallbackContext context)
        {
            if (Charges <= 0)
            {
                return;
            }

            Charges--;
            ChargesChanged?.Invoke(Charges);
            GustUsed?.Invoke();

            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, gustRadius, affectedLayer);
            foreach (Collider2D hit in hits)
            {
                if (hit.attachedRigidbody == null)
                {
                    continue;
                }

                Vector2 direction = ((Vector2)hit.transform.position - (Vector2)transform.position).normalized;
                if (direction == Vector2.zero)
                {
                    direction = Vector2.right;
                }

                hit.attachedRigidbody.AddForce(direction * gustForce, ForceMode2D.Impulse);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.6f, 0.9f, 1f);
            Gizmos.DrawWireSphere(transform.position, gustRadius);
        }
    }
}
