using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Odisseia.Core;
using Odisseia.Systems;

namespace Odisseia.Player
{
    /// <summary>
    /// Ataque de espada de Odisseu: área circular no AttackPoint, com cooldown.
    /// </summary>
    public class PlayerCombat : MonoBehaviour
    {
        /// <summary>
        /// Disparado quando o golpe sai (depois do cooldown, acertando ou não).
        /// Existe para o <see cref="PlayerAnimator"/> tocar a animação no mesmo
        /// instante em que o dano e o som acontecem, sem duplicar a regra de cooldown.
        /// </summary>
        public event Action Attacked;

        [Header("Input")]
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private string actionMapName = "Player";
        [SerializeField] private string attackActionName = "Attack";

        [Header("Ataque")]
        [SerializeField] private Transform attackPoint;
        [SerializeField] private float attackRadius = 0.6f;
        [SerializeField] private int damage = 20;
        [SerializeField] private float cooldown = 0.4f;
        [SerializeField] private LayerMask targetLayer;

        private InputActionMap playerMap;
        private InputAction attackAction;
        private float cooldownTimer;

        private void Awake()
        {
            if (inputActions != null)
            {
                playerMap = inputActions.FindActionMap(actionMapName, throwIfNotFound: false);
                attackAction = playerMap?.FindAction(attackActionName);
            }
        }

        private void OnEnable()
        {
            if (attackAction != null)
            {
                attackAction.performed += OnAttackPerformed;
            }
        }

        private void OnDisable()
        {
            if (attackAction != null)
            {
                attackAction.performed -= OnAttackPerformed;
            }
        }

        private void Update()
        {
            if (cooldownTimer > 0f)
            {
                cooldownTimer -= Time.deltaTime;
            }
        }

        private void OnAttackPerformed(InputAction.CallbackContext context)
        {
            TryAttack();
        }

        private void TryAttack()
        {
            if (cooldownTimer > 0f || attackPoint == null)
            {
                return;
            }

            cooldownTimer = cooldown;
            Attacked?.Invoke();

            // Feedback do golpe em si (sai mesmo errando o alvo — o jogador precisa
            // sentir que a espada respondeu ao comando).
            Sprite sprite = GameAssets.Instance != null ? GameAssets.Instance.PlaceholderSprite : null;
            VfxBurst.Spawn(sprite, attackPoint.position, new Color(0.95f, 0.95f, 1f), 4, 2f, 0.2f, 0.1f, 2f);
            AudioManager.PlayAttack();

            var hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, targetLayer);
            bool hitSomething = false;

            foreach (var hit in hits)
            {
                if (hit.TryGetComponent(out HealthSystem health))
                {
                    health.TakeDamage(damage);
                    hitSomething = true;
                }
            }

            // O acerto ganha um shake extra por cima do que o DamageFeedback do alvo
            // já produz — é o que diferencia acertar de golpear o ar.
            if (hitSomething)
            {
                CameraFollow.ShakeActive(0.1f, 0.1f);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (attackPoint == null)
            {
                return;
            }

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }
}
