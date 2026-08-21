using UnityEngine;
using Odisseia.Core;
using Odisseia.Systems;

namespace Odisseia.Enemies
{
    /// <summary>
    /// Traduz o estado do inimigo básico em animação, observando a velocidade do
    /// Rigidbody e os eventos do <see cref="HealthSystem"/>. Não toca em patrulha,
    /// detecção, perseguição nem dano — o <see cref="EnemyController"/> segue sendo
    /// o único dono desse comportamento.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class EnemyAnimator : MonoBehaviour
    {
        private const string StateIdle = "Idle";
        private const string StateRun = "Run";
        private const string StateHit = "Hit";
        private const string StateDeath = "Death";

        [SerializeField] private SpriteAnimator animator;
        [SerializeField] private float runThreshold = 0.15f;
        [Tooltip("Raiz virada pelo movimento. Se vazio, usa o transform do próprio animador.")]
        [SerializeField] private Transform visualRoot;

        private Rigidbody2D rb;
        private HealthSystem health;

        private float lockTimer;
        private bool deathPlayed;
        private bool facingRight = true;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            health = GetComponent<HealthSystem>();

            if (animator == null)
            {
                animator = GetComponentInChildren<SpriteAnimator>();
            }

            if (visualRoot == null && animator != null)
            {
                visualRoot = animator.transform;
            }
        }

        private void OnEnable()
        {
            if (health != null)
            {
                health.Damaged += OnDamaged;
                health.Died += OnDied;
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.Damaged -= OnDamaged;
                health.Died -= OnDied;
            }
        }

        private void OnDamaged(int amount, int currentHealth)
        {
            if (animator == null || deathPlayed || currentHealth <= 0 || !animator.HasState(StateHit))
            {
                return;
            }

            animator.Play(StateHit, restart: true);
            lockTimer = animator.GetStateDuration(StateHit);
        }

        private void OnDied()
        {
            if (animator == null)
            {
                return;
            }

            deathPlayed = true;
            animator.Play(StateDeath, restart: true);
        }

        private void Update()
        {
            if (animator == null || deathPlayed)
            {
                return;
            }

            float vx = rb != null ? rb.linearVelocity.x : 0f;
            UpdateFacing(vx);

            if (lockTimer > 0f)
            {
                lockTimer -= Time.deltaTime;
                return;
            }

            animator.Play(Mathf.Abs(vx) > runThreshold ? StateRun : StateIdle);
        }

        /// <summary>
        /// Os sprites são desenhados virados para a direita; a virada é feita
        /// invertendo a escala, do mesmo jeito que o PlayerController já faz.
        /// </summary>
        private void UpdateFacing(float vx)
        {
            if (visualRoot == null || Mathf.Abs(vx) <= runThreshold)
            {
                return;
            }

            bool right = vx > 0f;
            if (right == facingRight)
            {
                return;
            }

            facingRight = right;
            Vector3 scale = visualRoot.localScale;
            scale.x = Mathf.Abs(scale.x) * (facingRight ? 1f : -1f);
            visualRoot.localScale = scale;
        }
    }
}
