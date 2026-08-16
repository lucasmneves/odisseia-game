using UnityEngine;
using Odisseia.Player;

namespace Odisseia.Levels
{
    /// <summary>
    /// Área de magia de Circe: transforma o jogador ao entrar (ver TransformationEffect).
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class TransformationZone : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out TransformationEffect effect))
            {
                effect.Transform();
            }
        }
    }
}
