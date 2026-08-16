using UnityEngine;
using Odisseia.Player;

namespace Odisseia.Levels
{
    /// <summary>
    /// Coletável especial que concede cargas do saco dos ventos ao jogador.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class WindBagPickup : MonoBehaviour
    {
        [SerializeField] private int chargesGranted = 3;

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out WindBagAbility ability))
            {
                ability.AddCharge(chargesGranted);
                Destroy(gameObject);
            }
        }
    }
}
