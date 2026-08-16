using UnityEngine;
using Odisseia.Core;

namespace Odisseia.Levels
{
    /// <summary>
    /// Onda/maré que sobe e desce em um movimento senoidal (mesmo padrão de flutuação
    /// já usado em Collectible) e mata instantaneamente ao tocar — obriga o jogador a
    /// cronometrar a travessia entre as ilhotas em vez de andar livremente.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class TidalHazard : MonoBehaviour
    {
        [SerializeField] private float amplitude = 1.5f;
        [SerializeField] private float frequency = 0.5f;

        private float baseY;

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
            baseY = transform.position.y;
        }

        private void Update()
        {
            float y = baseY + Mathf.Sin(Time.time * frequency) * amplitude;
            transform.position = new Vector3(transform.position.x, y, transform.position.z);
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
