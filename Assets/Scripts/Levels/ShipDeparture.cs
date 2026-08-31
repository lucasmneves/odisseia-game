using System.Collections;
using UnityEngine;
using Odisseia.Player;
using Odisseia.Systems;
using Odisseia.UI;

namespace Odisseia.Levels
{
    /// <summary>
    /// A partida de Ítaca: Odisseu embarca, o navio se afasta do porto e a fase acaba.
    ///
    /// Odisseu no convés é um sprite filho do navio, e não o jogador carregado pelo
    /// casco. Durante a cena o controle está travado, e um Rigidbody dinâmico travado
    /// em cima de um objeto que se move por transform briga com a física — trocar o
    /// jogador por uma silhueta é o truque de cutscene que evita esse problema inteiro.
    ///
    /// Quem encerra a fase continua sendo o <see cref="LevelGoal"/>, chamado no fim da
    /// travessia: a conclusão, o salvamento e a volta ao mapa da jornada seguem num
    /// lugar só.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ShipDeparture : MonoBehaviour
    {
        [Header("Cena")]
        [SerializeField] private GameObject player;
        [SerializeField] private PlayerInputLock playerLock;
        [SerializeField] private Transform ship;
        [Tooltip("Silhueta de Odisseu no convés, ligada quando o jogador embarca.")]
        [SerializeField] private GameObject odysseusOnDeck;
        [SerializeField] private CameraFollow cameraFollow;

        [Header("Falas")]
        [Tooltip("Fala do embarque, antes do navio zarpar.")]
        [SerializeField] private DialogueSequence boardingDialogue;

        [Header("Travessia")]
        [SerializeField] private float sailSpeed = 4f;
        [SerializeField] private float sailSeconds = 4f;

        [Header("Fim")]
        [SerializeField] private LevelGoal goal;

        private bool started;

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (started || !other.CompareTag("Player"))
            {
                return;
            }

            started = true;
            StartCoroutine(DepartureRoutine());
        }

        private IEnumerator DepartureRoutine()
        {
            playerLock?.SetLocked(true);

            if (boardingDialogue != null)
            {
                bool done = false;
                void OnCompleted() => done = true;

                boardingDialogue.Completed += OnCompleted;
                boardingDialogue.Play();

                while (!done)
                {
                    yield return null;
                }

                boardingDialogue.Completed -= OnCompleted;
            }

            if (player != null)
            {
                player.SetActive(false);
            }

            if (odysseusOnDeck != null)
            {
                odysseusOnDeck.SetActive(true);
            }

            if (cameraFollow != null && ship != null)
            {
                cameraFollow.SetTarget(ship);
            }

            float elapsed = 0f;
            while (elapsed < sailSeconds)
            {
                elapsed += Time.deltaTime;

                if (ship != null)
                {
                    ship.position += Vector3.right * (sailSpeed * Time.deltaTime);
                }

                yield return null;
            }

            goal?.Trigger();
        }
    }
}
