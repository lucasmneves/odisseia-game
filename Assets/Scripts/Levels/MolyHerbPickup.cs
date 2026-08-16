using UnityEngine;
using Odisseia.Player;

namespace Odisseia.Levels
{
    /// <summary>
    /// Erva de moly (dada por Hermes no mito): cura a transformação de Circe na hora.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class MolyHerbPickup : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out TransformationEffect effect))
            {
                effect.Cure();
                Destroy(gameObject);
            }
        }
    }
}
