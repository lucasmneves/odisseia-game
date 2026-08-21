using UnityEngine;
using Odisseia.Core;
using Odisseia.Systems;

namespace Odisseia.Combat
{
    /// <summary>
    /// Projétil do arco. Viaja em linha reta na direção do disparo e some ao acertar
    /// um alvo, bater no cenário ou esgotar o tempo de vida — nunca fica órfão em cena,
    /// o que importa no WebGL.
    ///
    /// O dano passa pelo <see cref="HealthSystem"/> normal do alvo; não existe sistema
    /// de vida paralelo aqui.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Arrow : MonoBehaviour
    {
        [SerializeField] private float speed = 15f;
        [SerializeField] private int damage = 35;
        [SerializeField] private float lifetime = 3f;
        [Tooltip("Camadas que a flecha pode atingir e causar dano.")]
        [SerializeField] private LayerMask targetLayers;
        [Tooltip("Camadas que apenas param a flecha (chão, paredes, plataformas).")]
        [SerializeField] private LayerMask obstacleLayers;

        private Rigidbody2D rb;
        private bool consumed;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            Collider2D col = GetComponent<Collider2D>();
            col.isTrigger = true;
        }

        /// <summary>
        /// Configura e lança a flecha. Os parâmetros vêm do <c>PlayerBow</c>, para que
        /// balanceamento fique num lugar só em vez de espalhado por prefab e script.
        /// </summary>
        public void Launch(Vector2 direction, float arrowSpeed, int arrowDamage, float arrowLifetime,
            LayerMask targets, LayerMask obstacles)
        {
            speed = arrowSpeed;
            damage = arrowDamage;
            lifetime = arrowLifetime;
            targetLayers = targets;
            obstacleLayers = obstacles;

            Vector2 dir = direction.sqrMagnitude < 0.0001f ? Vector2.right : direction.normalized;

            if (rb == null)
            {
                rb = GetComponent<Rigidbody2D>();
            }

            rb.linearVelocity = dir * speed;

            // Aponta a ponta da flecha para onde ela viaja.
            transform.right = dir;

            // Rede de segurança: mesmo sem acertar nada, a flecha se remove sozinha.
            Destroy(gameObject, lifetime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (consumed)
            {
                return;
            }

            int otherLayer = 1 << other.gameObject.layer;

            if ((targetLayers.value & otherLayer) != 0 && other.TryGetComponent(out HealthSystem health))
            {
                consumed = true;
                health.TakeDamage(damage, new DamageInfo(transform.position));
                Vanish();
                return;
            }

            if ((obstacleLayers.value & otherLayer) != 0)
            {
                consumed = true;
                Vanish();
            }
        }

        private void Vanish()
        {
            Sprite sprite = GameAssets.Instance != null ? GameAssets.Instance.PlaceholderSprite : null;
            VfxBurst.Spawn(sprite, transform.position, new Color(0.9f, 0.85f, 0.6f), 3, 1.8f, 0.18f);
            Destroy(gameObject);
        }
    }
}
