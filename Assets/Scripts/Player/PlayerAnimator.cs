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
    /// Ataque, arco e dano travam o estado pela duração do clipe, senão a animação seria
    /// substituída no frame seguinte por Idle/Run e mal apareceria. Defender é diferente:
    /// é pose mantida, testada a cada quadro enquanto o botão estiver segurado.
    ///
    /// Todo estado precisa de alguém que o acione. O arco e o escudo ficaram com arte
    /// pronta e evento pronto (<see cref="PlayerBow.Fired"/>,
    /// <see cref="PlayerShield.IsBlocking"/>) mas sem ninguém escutando — o resultado era
    /// a flecha saindo do personagem sem nenhuma animação. Ao adicionar um estado novo à
    /// folha, ligar o gatilho aqui é parte do trabalho, não um detalhe posterior.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerAnimator : MonoBehaviour
    {
        private const string StateIdle = "Idle";
        private const string StateRun = "Run";
        private const string StateJump = "Jump";
        private const string StateFall = "Fall";
        private const string StateCrouch = "Crouch";
        private const string StateCrouchWalk = "CrouchWalk";
        private const string StateClimb = "Climb";
        private const string StateAttack = "AttackLight";
        private const string StateAttackHeavy = "AttackHeavy";
        private const string StateBow = "Bow";
        private const string StateShield = "Shield";
        private const string StateShieldHold = "ShieldHold";
        private const string StateInteraction = "Interaction";
        private const string StateVictory = "Victory";
        private const string StateDamage = "Damage";
        private const string StateDeath = "Death";

        [SerializeField] private SpriteAnimator animator;
        [Tooltip("Velocidade horizontal mínima para trocar de Idle para Run.")]
        [SerializeField] private float runThreshold = 0.2f;
        [Tooltip("Velocidade vertical (negativa) a partir da qual Jump vira Fall.")]
        [SerializeField] private float fallThreshold = -0.5f;
        [Tooltip("Limites do ritmo da corrida, como fração do ritmo configurado para o Run.")]
        [SerializeField] private float ritmoMinimo = 0.55f;
        [SerializeField] private float ritmoMaximo = 2f;

        private Rigidbody2D rb;
        private PlayerController controller;
        private PlayerClimb climb;
        private HealthSystem health;
        private PlayerCombat combat;
        private PlayerBow bow;
        private PlayerShield shield;

        private float lockTimer;
        private bool deathPlayed;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            controller = GetComponent<PlayerController>();
            climb = GetComponent<PlayerClimb>();
            health = GetComponent<HealthSystem>();
            combat = GetComponent<PlayerCombat>();
            bow = GetComponent<PlayerBow>();
            shield = GetComponent<PlayerShield>();

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

            if (bow != null)
            {
                bow.Fired += OnFired;
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

            if (bow != null)
            {
                bow.Fired -= OnFired;
            }
        }

        private void OnAttacked()
        {
            bool forte = combat != null && combat.LastAttackWasHeavy;
            Trigger(forte ? StateAttackHeavy : StateAttack);
        }

        /// <summary>
        /// O <see cref="PlayerBow"/> dispara <c>Fired</c> depois de instanciar a flecha, então
        /// a animação entra junto com o tiro e não antes dele — mesma convenção do ataque
        /// corpo a corpo. Com 8 quadros a 14 fps o clipe dura ~0,57s, um pouco mais que o
        /// cooldown de 0,5s do arco, então a animação nunca é cortada no meio por um segundo
        /// disparo.
        /// </summary>
        private void OnFired()
        {
            Trigger(StateBow);
        }

        /// <summary>
        /// Toca a animação de interagir. Chamado pelo <c>InteractPoint</c>.
        ///
        /// Interagir e vencer são pedidos por objetos de <b>cena</b>, não por componentes do
        /// jogador — não existe um evento no jogador para o animador assinar, como acontece
        /// com ataque e arco. Por isso a direção se inverte aqui: o gameplay chama o animador.
        /// </summary>
        public void PlayInteraction()
        {
            Trigger(StateInteraction);
        }

        /// <summary>Toca a animação de vitória. Chamado pelo <c>LevelGoal</c> ao concluir a fase.</summary>
        public void PlayVictory()
        {
            Trigger(StateVictory);
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

        /// <summary>
        /// Toca o estado, ou o parente mais próximo que exista na folha.
        ///
        /// Um estado ausente não gera erro: o <see cref="SpriteAnimator"/> apenas ignora a
        /// chamada, e o personagem fica congelado na animação anterior — um bug que parece
        /// travamento de gameplay. O fallback transforma isso numa animação genérica,
        /// que é errada mas legível.
        /// </summary>
        private void Play(string state)
        {
            if (animator.HasState(state))
            {
                animator.Play(state);
                return;
            }

            switch (state)
            {
                case StateFall:
                case StateClimb:
                    animator.Play(StateJump);
                    break;
                case StateShieldHold:
                    animator.Play(StateShield);
                    break;
                case StateCrouchWalk:
                    Play(StateCrouch);
                    break;
                case StateCrouch:
                    animator.Play(StateIdle);
                    break;
                default:
                    animator.Play(state);
                    break;
            }
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

            // A escalada tem prioridade sobre o travamento de ataque/dano: ela move o
            // personagem por conta própria e ficaria em pose de ataque no meio da subida.
            if (climb != null && climb.IsClimbing)
            {
                lockTimer = 0f;
                Play(StateClimb);
                return;
            }

            if (lockTimer > 0f)
            {
                lockTimer -= Time.deltaTime;
                return;
            }

            bool grounded = controller == null || controller.IsGrounded;
            float verticalSpeed = rb != null ? rb.linearVelocity.y : 0f;

            if (!grounded)
            {
                Play(verticalSpeed < fallThreshold ? StateFall : StateJump);
                return;
            }

            // Defender é pose mantida, não disparo: fica no ar enquanto o botão estiver
            // segurado, por isso é testado aqui e não por evento com lockTimer.
            //
            // São dois clipes porque um só não serve. "Shield" é a transição de erguer o
            // escudo e não faz sentido em loop — o personagem ficaria levantando o escudo
            // repetidamente. Sem loop, ele congela no último quadro, que era a queixa.
            // Então: ergue uma vez e, terminada a subida, passa para a guarda que respira.
            if (shield != null && shield.IsBlocking)
            {
                bool subiu = animator.CurrentState == StateShield && animator.IsFinished;
                Play(subiu || animator.CurrentState == StateShieldHold
                    ? StateShieldHold
                    : StateShield);
                return;
            }

            float speed = rb != null ? Mathf.Abs(rb.linearVelocity.x) : 0f;

            if (controller != null && controller.IsCrouching)
            {
                Play(speed > runThreshold ? StateCrouchWalk : StateCrouch);
                return;
            }

            if (speed <= runThreshold)
            {
                animator.Play(StateIdle);
                return;
            }

            animator.Play(StateRun);
            animator.PlaybackScale = RitmoDaCorrida(speed);
        }

        /// <summary>
        /// A que ritmo tocar o ciclo de corrida, dada a velocidade real.
        ///
        /// O FPS configurado no prefab foi escolhido para a velocidade normal do
        /// <see cref="PlayerController"/>: naquele ritmo cada passada cobre a distância que o
        /// personagem realmente percorre, e o pé fica cravado no chão. Num sprint 1,5× mais
        /// rápido, o mesmo ritmo faria o pé PATINAR — que é exatamente a leitura de
        /// "andando rápido" que a corrida acabou de deixar de ter. Então o ritmo acompanha a
        /// velocidade, e a proporção se mantém em qualquer valor entre parado e sprint.
        ///
        /// Isso também acerta de graça os efeitos que mexem na velocidade: sob a sonolência
        /// do lótus, a corrida fica pesada em vez de acelerada no lugar.
        ///
        /// Os limites existem para os extremos: sem o piso, empurrado contra uma parede o
        /// personagem congelaria num quadro em vez de correr no lugar.
        /// </summary>
        private float RitmoDaCorrida(float speed)
        {
            float referencia = controller != null ? controller.MaxSpeed : 0f;
            if (referencia <= 0.01f)
            {
                return 1f;
            }

            return Mathf.Clamp(speed / referencia, ritmoMinimo, ritmoMaximo);
        }
    }
}
