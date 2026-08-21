using UnityEngine;
using Odisseia.Core;
using Odisseia.Player;
using Odisseia.Systems;

namespace Odisseia.Levels
{
    /// <summary>
    /// Repõe flechas do arco. Segue o mesmo padrão dos outros pickups da fase
    /// (trigger + flutuação simples), mas não mexe no <c>CollectibleCounter</c> —
    /// munição não é pontuação.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ArrowPickup : MonoBehaviour
    {
        [Header("Munição")]
        [SerializeField] private int amount = 5;

        [Header("Animação simples")]
        [SerializeField] private float bobHeight = 0.15f;
        [SerializeField] private float bobSpeed = 2f;

        private Vector3 startPosition;
        private bool collected;

        private void Awake()
        {
            startPosition = transform.position;
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void Update()
        {
            if (collected)
            {
                return;
            }

            float offset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = startPosition + Vector3.up * offset;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (collected || !other.CompareTag("Player"))
            {
                return;
            }

            if (!other.TryGetComponent(out PlayerBow bow))
            {
                return;
            }

            // Aljava cheia: o pickup fica onde está para ser pego mais tarde, em vez
            // de sumir sem dar nada.
            int added = bow.AddArrows(amount);
            if (added <= 0)
            {
                return;
            }

            collected = true;

            Sprite sprite = GameAssets.Instance != null ? GameAssets.Instance.PlaceholderSprite : null;
            VfxBurst.Spawn(sprite, transform.position, new Color(0.85f, 0.75f, 0.45f), 6, 2.2f, 0.3f);
            AudioManager.PlayCollect();

            Destroy(gameObject);
        }
    }
}
