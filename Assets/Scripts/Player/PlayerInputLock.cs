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
        private PlayerShield shield;
        private PlayerBow bow;
        private Rigidbody2D rb;

        private void Awake()
        {
            controller = GetComponent<PlayerController>();
            combat = GetComponent<PlayerCombat>();
            shield = GetComponent<PlayerShield>();
            bow = GetComponent<PlayerBow>();
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

            // O escudo se desliga sozinho no OnDisable, então travar aqui também evita
            // ficar defendendo durante uma cutscene.
            if (shield != null)
            {
                shield.enabled = !locked;
            }

            if (bow != null)
            {
                bow.enabled = !locked;
            }

            if (locked && rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
    }
}
