using UnityEngine;
using Odisseia.Player;

namespace Odisseia.Levels
{
    /// <summary>
    /// Área especial da ilha dos Lotófagos: alimenta a sonolência do jogador
    /// enquanto ele permanecer dentro.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class LotusZone : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out LotusEffect effect))
            {
                effect.EnterZone();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.TryGetComponent(out LotusEffect effect))
            {
                effect.ExitZone();
            }
        }
    }
}
