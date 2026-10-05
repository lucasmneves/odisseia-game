using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Odisseia.Core;
using Odisseia.Systems;

namespace Odisseia.Player
{
    /// <summary>
    /// Ataque de espada de Odisseu: área circular no AttackPoint, com cooldown.
    ///
    /// Dois golpes: o normal e, quando o jogador encadeia um segundo dentro da janela de
    /// combo, o forte — mais dano, alcance maior e cooldown mais longo. Sai pela mesma
    /// tecla de propósito: o golpe forte existia só como animação na folha, sem nenhuma
    /// forma de chegar até ele, e uma tecla nova seria mais uma coisa para o jogador
    /// aprender por um movimento que a sequência já entrega naturalmente.
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

        [Header("Golpe forte")]
        [Tooltip("Um segundo golpe dentro desta janela sai como golpe forte. Zero desliga a sequência.")]
        [SerializeField] private float comboWindow = 0.8f;
        [SerializeField] private int heavyDamage = 35;
        [SerializeField] private float heavyRadius = 0.85f;
        [Tooltip("Multiplicador de cooldown do golpe forte — ele é mais lento de propósito.")]
        [SerializeField] private float heavyCooldownScale = 1.5f;

        private InputActionMap playerMap;
        private InputAction attackAction;
        private PlayerShield shield;
        private float cooldownTimer;
        private float comboTimer;

        /// <summary>
        /// Verdadeiro se o golpe que acabou de sair foi o forte. O
        /// <see cref="PlayerAnimator"/> lê isto no <c>Attacked</c> para escolher entre
        /// AttackLight e AttackHeavy — assim o evento continua com a mesma assinatura e
        /// quem só conta golpes (o TrainingCourse) não precisa saber da diferença.
        /// </summary>
        public bool LastAttackWasHeavy { get; private set; }

        private void Awake()
        {
            shield = GetComponent<PlayerShield>();

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

            if (comboTimer > 0f)
            {
                comboTimer -= Time.deltaTime;
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

            // Defendendo não se ataca — o escudo ocupa as mãos.
            if (shield != null && shield.IsBlocking)
            {
                return;
            }

            // Encadear: o segundo golpe dentro da janela sai forte. Depois dele a
            // corrente zera, senão o jogador ficaria só martelando golpes fortes.
            bool forte = comboTimer > 0f;
            LastAttackWasHeavy = forte;
            comboTimer = forte ? 0f : comboWindow;

            cooldownTimer = forte ? cooldown * heavyCooldownScale : cooldown;
            float raio = forte ? heavyRadius : attackRadius;
            int dano = forte ? heavyDamage : damage;

            Attacked?.Invoke();

            // Feedback do golpe em si (sai mesmo errando o alvo — o jogador precisa
            // sentir que a espada respondeu ao comando).
            // Arco do golpe (o pesado tem o seu); o lado vem do attackPoint, que o PlayerController espelha ao virar.
            bool paraEsquerda = attackPoint.position.x < transform.position.x;
            if (!VfxSheet.Play(forte ? "fx_heavy_slash" : "fx_slash_v2", attackPoint.position, paraEsquerda))
            {
                Sprite sprite = GameAssets.Instance != null ? GameAssets.Instance.PlaceholderSprite : null;
                VfxBurst.Spawn(sprite, attackPoint.position, new Color(0.95f, 0.95f, 1f), 4, 2f, 0.2f, 0.1f, 2f);
            }
            AudioManager.PlayAttack();

            var hits = Physics2D.OverlapCircleAll(attackPoint.position, raio, targetLayer);
            bool hitSomething = false;

            foreach (var hit in hits)
            {
                if (hit.TryGetComponent(out HealthSystem health))
                {
                    health.TakeDamage(dano, new DamageInfo(attackPoint.position));
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
