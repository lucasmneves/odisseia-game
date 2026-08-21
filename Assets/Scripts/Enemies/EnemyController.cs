using UnityEngine;
using Odisseia.Core;

namespace Odisseia.Enemies
{
    /// <summary>
    /// Inimigo básico: patrulha, detecta e persegue o jogador, causa dano por contato próximo.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(HealthSystem))]
    public class EnemyController : MonoBehaviour
    {
        private enum State
        {
            Patrol,
            Chase
        }

        [Header("Patrulha")]
        [SerializeField] private float patrolSpeed = 1.5f;
        [SerializeField] private float patrolDistance = 3f;

        [Header("Detecção")]
        [SerializeField] private float detectionRadius = 4f;
        [SerializeField] private float loseTargetRadius = 6f;
        [SerializeField] private LayerMask playerLayer;

        [Header("Perseguição")]
        [SerializeField] private float chaseSpeed = 2.5f;

        [Header("Ataque")]
        [SerializeField] private float attackRange = 0.8f;
        [SerializeField] private int damage = 10;
        [SerializeField] private float attackCooldown = 1f;
        [Tooltip("Se o golpe pode ser bloqueado pelo escudo do jogador.")]
        [SerializeField] private bool attackBlockable = true;

        [Header("Morte")]
        [Tooltip("Atraso antes de remover o objeto. Dá tempo da animação de morte aparecer; " +
                 "a física e o collider já são desligados no instante da morte.")]
        [SerializeField] private float destroyDelay = 0.6f;

        private Rigidbody2D rb;
        private HealthSystem health;
        private Transform playerTransform;
        private State state;
        private Vector3 spawnPosition;
        private int patrolDirection = 1;
        private float attackTimer;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            health = GetComponent<HealthSystem>();
            rb.gravityScale = 3f;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            spawnPosition = transform.position;
        }

        private void OnEnable()
        {
            health.Died += OnDied;
        }

        private void OnDisable()
        {
            health.Died -= OnDied;
        }

        private void Update()
        {
            if (attackTimer > 0f)
            {
                attackTimer -= Time.deltaTime;
            }
        }

        private void FixedUpdate()
        {
            if (health.IsDead)
            {
                return;
            }

            DetectPlayer();

            switch (state)
            {
                case State.Patrol:
                    Patrol();
                    break;
                case State.Chase:
                    Chase();
                    break;
            }
        }

        private void DetectPlayer()
        {
            if (state == State.Patrol)
            {
                Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);
                if (hit != null)
                {
                    playerTransform = hit.transform;
                    state = State.Chase;
                }
            }
            else if (state == State.Chase && playerTransform != null)
            {
                float distance = Vector2.Distance(transform.position, playerTransform.position);
                if (distance > loseTargetRadius)
                {
                    state = State.Patrol;
                    playerTransform = null;
                }
            }
        }

        private void Patrol()
        {
            float distanceFromSpawn = transform.position.x - spawnPosition.x;
            if (distanceFromSpawn >= patrolDistance)
            {
                patrolDirection = -1;
            }
            else if (distanceFromSpawn <= -patrolDistance)
            {
                patrolDirection = 1;
            }

            rb.linearVelocity = new Vector2(patrolDirection * patrolSpeed, rb.linearVelocity.y);
        }

        private void Chase()
        {
            if (playerTransform == null)
            {
                state = State.Patrol;
                return;
            }

            float distance = Vector2.Distance(transform.position, playerTransform.position);

            if (distance <= attackRange)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
                TryAttack();
                return;
            }

            float direction = Mathf.Sign(playerTransform.position.x - transform.position.x);
            rb.linearVelocity = new Vector2(direction * chaseSpeed, rb.linearVelocity.y);
        }

        private void TryAttack()
        {
            if (attackTimer > 0f)
            {
                return;
            }

            attackTimer = attackCooldown;

            if (playerTransform.TryGetComponent(out HealthSystem playerHealth))
            {
                playerHealth.TakeDamage(damage, new DamageInfo(transform.position, attackBlockable));
            }
        }

        private void OnDied()
        {
            rb.simulated = false;
            if (TryGetComponent(out Collider2D col))
            {
                col.enabled = false;
            }
            Destroy(gameObject, destroyDelay);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
