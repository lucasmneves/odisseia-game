using UnityEngine;
using UnityEngine.InputSystem;

namespace Odisseia.Player
{
    /// <summary>
    /// Escalada de beiradas: quando Odisseu encosta num degrau ou caixa na altura certa e
    /// o jogador pula, ele sobe por cima em vez de bater na parede.
    ///
    /// A detecção é feita com dois testes que precisam discordar entre si — é isso que
    /// distingue uma beirada de uma parede alta. O teste na altura do peito precisa
    /// <b>encontrar</b> geometria (existe algo à frente para subir) e o teste na altura da
    /// cabeça precisa estar <b>livre</b> (o obstáculo termina antes do topo da cabeça).
    /// Uma parede inteira acerta os dois e é corretamente ignorada.
    ///
    /// Confirmada a beirada, um terceiro teste procura o chão do topo apontando para baixo,
    /// e é a altura desse ponto — não a do obstáculo — que vira o destino. Assim degraus
    /// irregulares funcionam sem tuning por objeto.
    ///
    /// Durante a subida o movimento normal fica suspenso via
    /// <see cref="PlayerController.MovementSuspended"/>, e não desligando o
    /// <see cref="PlayerController"/>: desligá-lo derrubaria o action map, que é
    /// compartilhado com ataque, escudo e arco.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(PlayerController))]
    public class PlayerClimb : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private string actionMapName = "Player";
        [Tooltip("A mesma ação do pulo: encostar na beirada e pular sobe por cima.")]
        [SerializeField] private string climbActionName = "Jump";

        [Header("Detecção")]
        [SerializeField] private LayerMask climbableLayer;
        [Tooltip("Altura do teste que precisa ENCONTRAR o obstáculo.")]
        [SerializeField] private float chestHeight = 0.45f;
        [Tooltip("Altura do teste que precisa estar LIVRE — acima disso não é escalável.")]
        [SerializeField] private float headHeight = 1.25f;
        [Tooltip("Alcance à frente dos dois testes.")]
        [SerializeField] private float reach = 0.45f;
        [Tooltip("Folga acima do topo para caber em pé depois de subir.")]
        [SerializeField] private float clearance = 1.5f;

        [Header("Subida")]
        [Tooltip("Duração da subida, em segundos.")]
        [SerializeField] private float climbDuration = 0.45f;
        [Tooltip("Quanto o personagem avança para dentro do topo ao terminar.")]
        [SerializeField] private float landingInset = 0.35f;

        private Rigidbody2D rb;
        private PlayerController controller;
        private InputActionMap playerMap;
        private InputAction climbAction;

        private float climbTimer;
        private Vector2 climbFrom;
        private Vector2 climbTo;

        /// <summary>Verdadeiro durante toda a animação de subida.</summary>
        public bool IsClimbing { get; private set; }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            controller = GetComponent<PlayerController>();

            if (inputActions != null)
            {
                playerMap = inputActions.FindActionMap(actionMapName, throwIfNotFound: false);
                climbAction = playerMap?.FindAction(climbActionName);
            }
        }

        private void OnEnable()
        {
            playerMap?.Enable();
            if (climbAction != null)
            {
                climbAction.performed += OnClimbPerformed;
            }
        }

        private void OnDisable()
        {
            if (climbAction != null)
            {
                climbAction.performed -= OnClimbPerformed;
            }

            // Sair no meio da subida (cutscene, morte, troca de cena) não pode deixar o
            // movimento suspenso para sempre.
            if (IsClimbing)
            {
                EncerrarSubida();
            }
        }

        private void OnClimbPerformed(InputAction.CallbackContext context)
        {
            if (IsClimbing || controller == null || !controller.enabled)
            {
                return;
            }

            if (TentarAcharBeirada(out Vector2 destino))
            {
                IniciarSubida(destino);
            }
        }

        /// <summary>
        /// Procura uma beirada à frente do personagem. Devolve a posição em que ele deve
        /// terminar de pé em cima dela.
        /// </summary>
        private bool TentarAcharBeirada(out Vector2 destino)
        {
            destino = default;

            float dir = Mathf.Sign(transform.localScale.x) >= 0f ? 1f : -1f;
            // O sprite é espelhado no visualRoot, não na raiz, então a direção real vem do
            // sentido em que o personagem está andando; parado, assume-se a última direção.
            if (controller != null)
            {
                dir = controller.FacingSign;
            }

            Vector2 baseP = transform.position;
            Vector2 frente = new Vector2(dir, 0f);

            // 1. Na altura do peito tem que haver algo.
            RaycastHit2D peito = Physics2D.Raycast(baseP + new Vector2(0f, chestHeight), frente, reach, climbableLayer);
            if (peito.collider == null)
            {
                return false;
            }

            // 2. Na altura da cabeça tem que estar livre — senão é parede, não beirada.
            RaycastHit2D cabeca = Physics2D.Raycast(baseP + new Vector2(0f, headHeight), frente, reach, climbableLayer);
            if (cabeca.collider != null)
            {
                return false;
            }

            // 3. De cima para baixo, logo além da quina, para achar a altura real do topo.
            Vector2 sonda = baseP + new Vector2(dir * (reach + landingInset), headHeight);
            RaycastHit2D topo = Physics2D.Raycast(sonda, Vector2.down, headHeight - chestHeight, climbableLayer);
            if (topo.collider == null)
            {
                return false;
            }

            // 4. Precisa caber em pé lá em cima.
            Vector2 pouso = new Vector2(sonda.x, topo.point.y);
            if (Physics2D.OverlapBox(pouso + new Vector2(0f, clearance * 0.5f),
                    new Vector2(0.4f, clearance * 0.9f), 0f, climbableLayer) != null)
            {
                return false;
            }

            destino = pouso;
            return true;
        }

        private void IniciarSubida(Vector2 destino)
        {
            IsClimbing = true;
            climbTimer = 0f;
            climbFrom = rb.position;
            climbTo = destino;

            controller.MovementSuspended = true;
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        private void FixedUpdate()
        {
            if (!IsClimbing)
            {
                return;
            }

            climbTimer += Time.fixedDeltaTime;
            float t = climbDuration > 0f ? Mathf.Clamp01(climbTimer / climbDuration) : 1f;

            // Sobe primeiro, avança depois: seguir a diagonal faria o corpo atravessar a
            // quina do obstáculo no meio do caminho.
            float subida = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / 0.65f));
            float avanco = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((t - 0.45f) / 0.55f));

            rb.MovePosition(new Vector2(
                Mathf.Lerp(climbFrom.x, climbTo.x, avanco),
                Mathf.Lerp(climbFrom.y, climbTo.y, subida)));

            if (t >= 1f)
            {
                EncerrarSubida();
            }
        }

        private void EncerrarSubida()
        {
            IsClimbing = false;
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.linearVelocity = Vector2.zero;

            if (controller != null)
            {
                controller.MovementSuspended = false;
            }
        }

        private void OnDrawGizmosSelected()
        {
            float dir = Application.isPlaying && controller != null ? controller.FacingSign : 1f;
            Vector2 baseP = transform.position;

            Gizmos.color = Color.yellow;   // precisa ENCONTRAR
            Gizmos.DrawLine(baseP + new Vector2(0f, chestHeight),
                            baseP + new Vector2(dir * reach, chestHeight));

            Gizmos.color = Color.cyan;     // precisa estar LIVRE
            Gizmos.DrawLine(baseP + new Vector2(0f, headHeight),
                            baseP + new Vector2(dir * reach, headHeight));
        }
    }
}
