using System.Collections;
using UnityEngine;
using Odisseia.Core;
using Odisseia.Enemies;
using Odisseia.Systems;
using Odisseia.UI;

namespace Odisseia.Levels
{
    /// <summary>
    /// Fase 07 (Lestrigões) — segura a perseguição enquanto o jogador não tem controle.
    /// BUG-001 de Docs/QA/FULL_CAMPAIGN_PLAYTEST.md.
    ///
    /// O <see cref="PursuerHazard"/> anda desde o primeiro quadro e a <see cref="LevelIntro"/> trava o jogador
    /// durante a fala de abertura: com o gigante nascendo a 4,2 un do Odisseu a 5,5 un/s, ele matava antes de o
    /// jogador poder se mexer — e o respawn caía de novo dentro da mesma fala. Fim de jogo em ~4 s.
    ///
    /// Este componente só existe na cena da Fase 07 (posto pelo <c>LestrigoesChaseFix</c>) e não altera nenhum
    /// sistema: enquanto a guarda está fechada, o perseguidor fica parado (componente e colisor desligados, arte
    /// em Idle) e os gigantes não iniciam ataques. Ela abre:
    /// - na abertura, no evento REAL de fim da fala (o mesmo <see cref="DialogueSequence.Completed"/> que a
    ///   LevelIntro usa para devolver o controle) — sem timer;
    /// - depois de cada morte, ao fim de uma janela curta de respawn: o respawn não trava o jogador nem tem
    ///   evento de "controle recuperado", então aqui o tempo é o único sinal possível.
    /// E fecha de vez quando a fase é concluída: a LevelGoal trava o jogador na fala final, e o gigante o alcançava
    /// ali, tirando uma vida depois da vitória.
    /// O reposicionamento do perseguidor na morte continua sendo o do próprio PursuerHazard (8 un atrás do
    /// ponto de respawn); nenhum teletransporte novo foi acrescentado.
    /// </summary>
    public class LestrigoesChaseGuard : MonoBehaviour
    {
        [SerializeField] private PursuerHazard pursuer;
        [SerializeField] private BossController[] giants;
        [Tooltip("A fala de abertura (a mesma da LevelIntro). O fim dela libera a perseguição.")]
        [SerializeField] private DialogueSequence introDialogue;
        [SerializeField] private HealthSystem playerHealth;
        [Tooltip("Segundos de jogo, depois de reaparecer, com o gigante parado e sem ataques.")]
        [SerializeField] private float respawnGrace = 1.5f;
        [Tooltip("A fase. Concluída, a LevelGoal trava o jogador na fala final — a perseguição para até a troca de cena.")]
        [SerializeField] private LevelManager levelManager;

        private bool concluida;

        private Collider2D pursuerCollider;
        private SpriteAnimator pursuerArt;
        private Coroutine graceRoutine;

        /// <summary>Verdadeiro enquanto a perseguição e os ataques estão suspensos.</summary>
        public bool IsHolding { get; private set; }

        private void Awake()
        {
            if (pursuer != null)
            {
                pursuerCollider = pursuer.GetComponent<Collider2D>();
                pursuerArt = pursuer.GetComponentInChildren<SpriteAnimator>(true);
            }
            // Fecha já no Awake: nenhum Update do perseguidor ou dos gigantes roda antes da fala começar.
            Hold(true);
        }

        private void OnEnable()
        {
            if (playerHealth != null)
            {
                playerHealth.Died += OnPlayerDied;
            }
        }

        private void OnDisable()
        {
            if (playerHealth != null)
            {
                playerHealth.Died -= OnPlayerDied;
            }
            if (introDialogue != null)
            {
                introDialogue.Completed -= OnIntroCompleted;
            }
        }

        private void Start()
        {
            // No Awake a arte pode ainda não ter carregado os clipes (ordem de Awake não é garantida): reaplica a pose.
            if (IsHolding)
            {
                Hold(true);
            }
            if (introDialogue != null)
            {
                introDialogue.Completed += OnIntroCompleted;
            }
            else
            {
                OnIntroCompleted();
            }
        }

        /// <summary>
        /// Fala final: a LevelGoal chama LevelManager.CompleteLevel e trava o jogador enquanto a fala toca. Sem isto o
        /// gigante alcançava o Odisseu travado na saída e tirava uma vida DEPOIS da fase vencida (visto nas 3 rodadas
        /// do bot). A LevelGoal não emite evento; o estado real de conclusão é o sinal.
        /// </summary>
        private void Update()
        {
            if (!concluida && levelManager != null && levelManager.IsCompleted)
            {
                concluida = true;
                if (graceRoutine != null)
                {
                    StopCoroutine(graceRoutine);
                    graceRoutine = null;
                }
                Hold(true);
            }
        }

        private void OnIntroCompleted()
        {
            if (introDialogue != null)
            {
                introDialogue.Completed -= OnIntroCompleted;
            }
            if (!concluida)
            {
                Hold(false);
            }
        }

        private void OnPlayerDied()
        {
            // Última vida: o fim de jogo assume a tela; não há respawn a proteger.
            if (LivesCounter.IsGameOver)
            {
                return;
            }
            if (graceRoutine != null)
            {
                StopCoroutine(graceRoutine);
            }
            graceRoutine = StartCoroutine(RespawnGrace());
        }

        private IEnumerator RespawnGrace()
        {
            Hold(true);
            yield return new WaitForSeconds(respawnGrace);
            graceRoutine = null;
            if (!concluida)
            {
                Hold(false);
            }
        }

        private void Hold(bool hold)
        {
            IsHolding = hold;
            if (pursuer != null)
            {
                pursuer.enabled = !hold;
            }
            // Mensagens de trigger chegam mesmo a componentes desligados: sem desligar o colisor, o gigante
            // parado ainda mataria ao toque.
            if (pursuerCollider != null)
            {
                pursuerCollider.enabled = !hold;
            }
            if (pursuerArt != null)
            {
                string estado = hold ? "Idle" : "Run";
                if (pursuerArt.HasState(estado))
                {
                    pursuerArt.Play(estado);
                }
            }
            if (giants != null)
            {
                foreach (BossController giant in giants)
                {
                    if (giant != null)
                    {
                        giant.enabled = !hold;
                    }
                }
            }
        }
    }
}
