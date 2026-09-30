using UnityEngine;
using Odisseia.Systems;

namespace Odisseia.Enemies
{
    /// <summary>
    /// Traduz o ciclo do <see cref="BossController"/> em animação — o equivalente do
    /// <see cref="EnemyAnimator"/> para o chefe. Só escuta os dois eventos que o controlador já
    /// dispara; não toca em tempo, alvo, dano nem pontos de ataque.
    ///
    /// AttackTelegraphed -> Telegraph (ergue a clava e a segura durante o aviso)
    /// AttackExecuted    -> Attack    (o golpe no chão; o último quadro fica por holdAfterAttack)
    /// resto do tempo    -> Idle
    ///
    /// Sem Hit e sem Death de propósito: o chefe dos Ciclopes não tem colisor e tem 999 de vida — o
    /// BossController é de esquiva, não de derrota. Estado que nenhum evento aciona não entra na folha.
    /// </summary>
    public class BossAnimator : MonoBehaviour
    {
        private const string StateIdle = "Idle";
        private const string StateTelegraph = "Telegraph";
        private const string StateAttack = "Attack";

        [SerializeField] private BossController boss;
        [SerializeField] private SpriteAnimator animator;
        [Tooltip("Quanto o quadro final do golpe fica na tela antes de voltar ao Idle.")]
        [SerializeField] private float holdAfterAttack = 0.35f;

        private float holdTimer = -1f;

        private void Awake()
        {
            if (boss == null) { boss = GetComponentInParent<BossController>(); }
            if (animator == null) { animator = GetComponentInChildren<SpriteAnimator>(); }
        }

        private void OnEnable()
        {
            if (boss == null) { return; }
            boss.AttackTelegraphed += OnTelegraphed;
            boss.AttackExecuted += OnExecuted;
        }

        private void OnDisable()
        {
            if (boss == null) { return; }
            boss.AttackTelegraphed -= OnTelegraphed;
            boss.AttackExecuted -= OnExecuted;
        }

        private void OnTelegraphed()
        {
            holdTimer = -1f;
            if (animator != null) { animator.Play(StateTelegraph, restart: true); }
        }

        private void OnExecuted()
        {
            holdTimer = -1f;
            if (animator != null) { animator.Play(StateAttack, restart: true); }
        }

        private void Update()
        {
            if (animator == null || animator.CurrentState != StateAttack || !animator.IsFinished)
            {
                return;
            }

            // O golpe terminou: segura o quadro do impacto um instante e volta a respirar.
            if (holdTimer < 0f) { holdTimer = holdAfterAttack; }
            holdTimer -= Time.deltaTime;
            if (holdTimer <= 0f)
            {
                holdTimer = -1f;
                animator.Play(StateIdle);
            }
        }
    }
}
