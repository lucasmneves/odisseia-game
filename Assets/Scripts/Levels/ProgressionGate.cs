using UnityEngine;
using Odisseia.Systems;
using Odisseia.UI;

namespace Odisseia.Levels
{
    /// <summary>
    /// Barreira que só libera a passagem quando um objetivo estiver concluído.
    ///
    /// A diferença que importa está em quem manda: o portão **pergunta** ao objetivo se
    /// ele já fechou, em vez de esperar alguém desligá-lo. Antes, quem abria o portão
    /// era o objetivo (uma lista de objetos para desativar ao completar) — e um único
    /// evento perdido, numa ordem de execução infeliz, deixava a barreira fechada para
    /// sempre com o ato já concluído. Isso é softlock: a fase segue "correta", e o
    /// jogador fica presoentre uma parede e um objetivo que não existe mais.
    ///
    /// Aqui a barreira reavalia em três momentos — ao iniciar, quando o objetivo avisa
    /// que fechou, e quando o jogador encosta nela. O terceiro é o que garante a
    /// recuperação: mesmo que o evento se perca, chegar no portão o abre.
    ///
    /// Não conhece o prólogo. Recebe um <see cref="LevelObjective"/> pelo Inspector e
    /// serve a qualquer fase.
    /// </summary>
    public class ProgressionGate : MonoBehaviour
    {
        [Header("Condição")]
        [Tooltip("Objetivo que abre este portão. Vazio = portão sempre aberto.")]
        [SerializeField] private LevelObjective requiredObjective;

        [Header("Barreira")]
        [Tooltip("O que impede a passagem. Só o colisor é desligado — o objeto do " +
                 "portão continua vivo para poder reavaliar.")]
        [SerializeField] private Collider2D blockCollider;

        [Tooltip("Aparência de portão fechado (a folha da porta). Some ao abrir.")]
        [SerializeField] private GameObject[] lockedVisuals;

        [Header("Aviso")]
        [Tooltip("Área que detecta o jogador chegando. Também é o gatilho da reavaliação.")]
        [SerializeField] private Collider2D noticeArea;

        [Tooltip("Chave da explicação: o que falta e, principalmente, para onde ir.")]
        [SerializeField] private string lockedMessageKey;

        [SerializeField] private TutorialPrompt prompt;

        private bool unlocked;
        private bool playerInRange;

        public bool IsUnlocked => unlocked;

        private void Awake()
        {
            if (noticeArea != null)
            {
                noticeArea.isTrigger = true;
            }

            if (blockCollider != null)
            {
                blockCollider.isTrigger = false;
            }
        }

        private void OnEnable()
        {
            if (requiredObjective != null)
            {
                requiredObjective.Completed += Reavaliar;
            }
        }

        private void OnDisable()
        {
            if (requiredObjective != null)
            {
                requiredObjective.Completed -= Reavaliar;
            }
        }

        private void Start()
        {
            Reavaliar();
        }

        /// <summary>
        /// Abre se a condição já estiver cumprida. Idempotente e barato — pode ser
        /// chamado de qualquer lugar, quantas vezes for.
        /// </summary>
        public void Reavaliar()
        {
            if (unlocked)
            {
                return;
            }

            if (requiredObjective == null || requiredObjective.IsCompleted)
            {
                Unlock();
            }
        }

        public void Unlock()
        {
            if (unlocked)
            {
                return;
            }

            unlocked = true;
            PrologueTrace.Log("portao ABRIU: " + name);

            // Só a barreira sai. O objeto continua ativo — desligar tudo tiraria de
            // cena o próprio componente que sabe reavaliar.
            if (blockCollider != null)
            {
                blockCollider.enabled = false;
            }

            foreach (GameObject visual in lockedVisuals ?? System.Array.Empty<GameObject>())
            {
                if (visual != null)
                {
                    visual.SetActive(false);
                }
            }

            if (playerInRange)
            {
                prompt?.Hide(this);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            playerInRange = true;

            // Recuperação: chegar no portão o faz conferir a condição de novo. Se o
            // objetivo fechou e o aviso se perdeu, é aqui que a fase se destrava.
            Reavaliar();

            if (unlocked)
            {
                return;
            }

            PrologueTrace.Log("portao FECHADO tocado: " + name + " (espera " +
                (requiredObjective != null ? requiredObjective.name : "nada") + ")");

            if (Localization.Has(lockedMessageKey))
            {
                prompt?.ShowPersistent(this, Localization.Get(lockedMessageKey));
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            playerInRange = false;
            prompt?.Hide(this);
        }
    }
}
