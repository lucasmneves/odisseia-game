using UnityEngine;

namespace Odisseia.Systems
{
    /// <summary>
    /// Ao ser tocado pelo jogador, salva a posição no CheckpointManager e se ativa.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Checkpoint : MonoBehaviour
    {
        [Header("Arte por estado (opcional)")]
        [Tooltip("Se os dois sprites estiverem preenchidos, o estado é comunicado " +
                 "trocando o sprite. Sem eles, cai na tinta abaixo.")]
        [SerializeField] private Sprite activeSprite;
        [SerializeField] private Sprite inactiveSprite;

        [Header("Tinta (usada quando não há sprite por estado)")]
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
            if (spriteRenderer == null)
            {
                return;
            }

            Sprite target = isActive ? activeSprite : inactiveSprite;

            // Com arte dos dois estados, trocar o sprite comunica melhor do que
            // tingir — tingir de verde pintaria o poste inteiro.
            if (target != null)
            {
                spriteRenderer.sprite = target;
                spriteRenderer.color = Color.white;
                return;
            }

            spriteRenderer.color = isActive ? activeColor : inactiveColor;
        }
    }
}
