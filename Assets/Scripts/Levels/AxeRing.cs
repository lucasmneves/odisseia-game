using UnityEngine;

namespace Odisseia.Levels
{
    /// <summary>
    /// Uma "argola" (cabeça de machado) do desafio do arco — avisa o BowChallenge
    /// pai quando o jogador passa por ela.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class AxeRing : MonoBehaviour
    {
        [SerializeField] private BowChallenge challenge;

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player") && challenge != null)
            {
                challenge.NotifyRingEntered(this);
            }
        }
    }
}
