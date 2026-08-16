using UnityEngine;

namespace Odisseia.Systems
{
    /// <summary>
    /// Ao ser tocado pelo jogador, salva a posição no CheckpointManager e se ativa.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Checkpoint : MonoBehaviour
    {
        [SerializeField] private Color activeColor = new Color(0.3f, 0.9f, 0.4f);
        [SerializeField] private Color inactiveColor = Color.white;

        private SpriteRenderer spriteRenderer;
        private bool activated;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            GetComponent<Collider2D>().isTrigger = true;
            SetVisual(false);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (activated || !other.CompareTag("Player"))
            {
                return;
            }

            activated = true;
            CheckpointManager.SetCheckpoint(transform.position);
            SetVisual(true);

            Sprite sprite = Odisseia.Core.GameAssets.Instance != null
                ? Odisseia.Core.GameAssets.Instance.PlaceholderSprite
                : null;
            VfxBurst.Spawn(sprite, transform.position, activeColor, 8, 3f, 0.4f, 0.1f, 2f);
            AudioManager.PlayCheckpoint();
        }

        private void SetVisual(bool isActive)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = isActive ? activeColor : inactiveColor;
            }
        }
    }
}
