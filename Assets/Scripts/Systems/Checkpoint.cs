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
        [Tooltip("Quadros da chama acesa, em laço (opcional). Só a arte troca: o objeto, o colisor e a ordem ficam.")]
        [SerializeField] private Sprite[] activeFrames;

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
            AnimateFlame();

            Sprite sprite = Odisseia.Core.GameAssets.Instance != null
                ? Odisseia.Core.GameAssets.Instance.PlaceholderSprite
                : null;
            VfxBurst.Spawn(sprite, transform.position, activeColor, 8, 3f, 0.4f, 0.1f, 2f);
            AudioManager.PlayCheckpoint();
        }

        /// <summary>
        /// Chama acesa em laço pelo mesmo <see cref="SpriteAnimator"/> dos fogos de cenário, criado uma vez só aqui
        /// (o altar apagado fica parado). 7,5 a 9 fps pela posição, como no FireAnimationDresser: altares vizinhos
        /// não pulsam em uníssono.
        /// </summary>
        private void AnimateFlame()
        {
            if (activeFrames == null || activeFrames.Length < 2 || spriteRenderer == null)
            {
                return;
            }

            SpriteAnimator animator = GetComponent<SpriteAnimator>();
            if (animator == null)
            {
                animator = gameObject.AddComponent<SpriteAnimator>();
            }

            Vector3 p = transform.position;
            int step = Mathf.Abs(Mathf.RoundToInt(p.x * 7f + p.y * 3f)) % 4;
            animator.Configure("lit", activeFrames, 7.5f + step * 0.5f);
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
