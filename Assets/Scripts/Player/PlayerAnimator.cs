using UnityEngine;
using Odisseia.Core;
using Odisseia.Systems;

namespace Odisseia.Player
{
    /// <summary>
    /// Escolhe o estado de animação de Odisseu a partir do que o gameplay já expõe
    /// (velocidade do Rigidbody, <see cref="PlayerController.IsGrounded"/> e os eventos
    /// do <see cref="HealthSystem"/>). Não decide nada de jogo: só observa e traduz
    /// para o <see cref="SpriteAnimator"/>.
    ///
    /// Ataque e dano travam o estado pela duração do clipe, senão a animação seria
    /// substituída no frame seguinte por Idle/Run e mal apareceria.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerAnimator : MonoBehaviour
    {
        private const string StateIdle = "Idle";
        private const string StateRun = "Run";
        private const string StateJump = "Jump";
        private const string StateAttack = "AttackLight";
        private const string StateDamage = "Damage";
        private const string StateDeath = "Death";

        [SerializeField] private SpriteAnimator animator;
        [Tooltip("Velocidade horizontal mínima para trocar de Idle para Run.")]
        [SerializeField] private float runThreshold = 0.2f;

        private Rigidbody2D rb;
        private PlayerController controller;
        private HealthSystem health;
        private PlayerCombat combat;

        private float lockTimer;
        private bool deathPlayed;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            controller = GetComponent<PlayerController>();
            health = GetComponent<HealthSystem>();
            combat = GetComponent<PlayerCombat>();

            if (animator == null)
            {
                animator = GetComponentInChildren<SpriteAnimator>();
            }
        }

        private void OnEnable()
        {
            if (health != null)
            {
                health.Damaged += OnDamaged;
                health.Died += OnDied;
            }

            if (combat != null)
            {
                combat.Attacked += OnAttacked;
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.Damaged -= OnDamaged;
                health.Died -= OnDied;
            }

            if (combat != null)
            {
                combat.Attacked -= OnAttacked;
            }
        }

        private void OnAttacked()
        {
            Trigger(StateAttack);
        }

        private void OnDamaged(int amount, int currentHealth)
        {
            if (currentHealth > 0)
            {
                Trigger(StateDamage);
            }
        }

        private void OnDied()
        {
            if (animator == null)
            {
                return;
            }

            deathPlayed = true;
            lockTimer = 0f;
            animator.Play(StateDeath, restart: true);
        }

        private void Trigger(string state)
        {
            if (animator == null || deathPlayed || !animator.HasState(state))
            {
                return;
            }

            animator.Play(state, restart: true);
            lockTimer = animator.GetStateDuration(state);
        }

        private void Update()
        {
            if (animator == null)
            {
                return;
            }

            // Respawn: o PlayerRespawn devolve a vida pelo HealthSystem.ResetHealth().
            if (deathPlayed)
            {
                if (health != null && !health.IsDead)
                {
                    deathPlayed = false;
                    lockTimer = 0f;
                }
                else
                {
                    return;
                }
            }

            if (lockTimer > 0f)
            {
                lockTimer -= Time.deltaTime;
                return;
            }

            bool grounded = controller == null || controller.IsGrounded;

            if (!grounded)
            {
                animator.Play(StateJump);
                return;
            }

            float speed = rb != null ? Mathf.Abs(rb.linearVelocity.x) : 0f;
            animator.Play(speed > runThreshold ? StateRun : StateIdle);
        }
    }
}
