using UnityEngine;

namespace Odisseia.Player
{
    /// <summary>
    /// Trava/destrava o controle de Odisseu (usado por cutscenes e diálogos).
    /// </summary>
    public class PlayerInputLock : MonoBehaviour
    {
        private PlayerController controller;
        private PlayerCombat combat;
        private Rigidbody2D rb;

        private void Awake()
        {
            controller = GetComponent<PlayerController>();
            combat = GetComponent<PlayerCombat>();
            rb = GetComponent<Rigidbody2D>();
        }

        public void SetLocked(bool locked)
        {
            if (controller != null)
            {
                controller.enabled = !locked;
            }

            if (combat != null)
            {
                combat.enabled = !locked;
            }

            if (locked && rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
    }
}
