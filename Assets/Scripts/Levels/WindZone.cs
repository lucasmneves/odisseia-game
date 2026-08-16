using UnityEngine;
using Odisseia.Player;

namespace Odisseia.Levels
{
    /// <summary>
    /// Corrente de vento: empurra o jogador horizontalmente enquanto ele estiver dentro
    /// (via PlayerController.ExternalVelocityX, para não brigar com o cálculo de
    /// movimento) e aplica força física a qualquer outro Rigidbody2D no layer afetado
    /// (ex.: destroços leves).
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class WindZone : MonoBehaviour
    {
        [SerializeField] private float windForceX = 4f;
        [SerializeField] private float propForce = 6f;
        [SerializeField] private LayerMask affectedPropsLayer;

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.TryGetComponent(out PlayerController controller))
            {
                controller.ExternalVelocityX = windForceX;
                return;
            }

            if (((1 << other.gameObject.layer) & affectedPropsLayer) != 0 && other.attachedRigidbody != null)
            {
                other.attachedRigidbody.AddForce(new Vector2(windForceX, 0f) * propForce * Time.deltaTime,
                    ForceMode2D.Impulse);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.TryGetComponent(out PlayerController controller))
            {
                controller.ExternalVelocityX = 0f;
            }
        }
    }
}
