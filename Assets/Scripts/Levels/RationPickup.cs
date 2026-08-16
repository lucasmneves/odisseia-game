using UnityEngine;
using Odisseia.Player;

namespace Odisseia.Levels
{
    /// <summary>
    /// Comida legítima (peixe, forragem) — restaura parte da fome sem consequências,
    /// ao contrário do gado sagrado.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class RationPickup : MonoBehaviour
    {
        [SerializeField] private float restoreAmount = 35f;

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out HungerMeter hunger))
            {
                hunger.Restore(restoreAmount);
                Destroy(gameObject);
            }
        }
    }
}
