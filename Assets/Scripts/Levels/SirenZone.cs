using UnityEngine;
using Odisseia.Player;

namespace Odisseia.Levels
{
    /// <summary>
    /// Zona de influência do canto das sereias: alimenta o dreno de SirenResistance
    /// enquanto o jogador permanece dentro. O empurrão físico em si é feito por um
    /// WindZone no mesmo objeto (reaproveitado, não duplicado).
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class SirenZone : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out SirenResistance resistance))
            {
                resistance.EnterZone();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.TryGetComponent(out SirenResistance resistance))
            {
                resistance.ExitZone();
            }
        }
    }
}
