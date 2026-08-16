using UnityEngine;
using Odisseia.Core;

namespace Odisseia.Levels
{
    /// <summary>
    /// Zona letal no fundo de poços/precipícios: mata instantaneamente quem tocar
    /// (o respawn no checkpoint é tratado pelo próprio HealthSystem/PlayerRespawn).
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class KillZone : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out HealthSystem health))
            {
                health.TakeDamage(health.CurrentHealth);
            }
        }
    }
}
