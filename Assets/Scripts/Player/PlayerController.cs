using UnityEngine;
using UnityEngine.InputSystem;
using Odisseia.Systems;

namespace Odisseia.Player
{
    /// <summary>
    /// Movimentação horizontal, pulo e agachamento de Odisseu. Lê as ações do Input System
    /// (não referencia teclas diretamente).
    ///
    /// Agachar encolhe o <see cref="BoxCollider2D"/> para baixo — os pés ficam na mesma
    /// linha — e limita a velocidade. Levantar depende de haver espaço livre acima, então
    /// soltar o botão debaixo de um teto mantém o personagem agachado de propósito.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private string actionMapName = "Player";
        [SerializeField] private string moveActionName = "Move";
        [SerializeField] private string jumpActionName = "Jump";
        [SerializeField] private string crouchActionName = "Crouch";
        [SerializeField] private string sprintActionName = "Sprint";

        [Header("Movimento")]
        [SerializeField] private float maxSpeed = 6f;
        [SerializeField] private float acceleration = 40f;
        [SerializeField] private float deceleration = 50f;

        [Header("Correr")]
        [Tooltip("Multiplicador da velocidade enquanto o sprint está segurado (Shift ou R1).")]
        [SerializeField] private float sprintMultiplier = 1.5f;

        [Header("Agachar")]
        [Tooltip("Fração da velocidade normal enquanto agachado.")]
        [SerializeField] private float crouchSpeedMultiplier = 0.45f;
        [Tooltip("Altura do collider agachado, como fração da altura em pé. 0,73 é a medida " +
                 "real do sprite agachado (44px de 60), para o collider não descolar da arte.")]
        [SerializeField] private float crouchHeightFactor = 0.73f;

        [Header("Pulo")]
        [Tooltip("Impulso vertical. 10,04 dá 70% da altura do pulo antigo (que era 12): a " +
                 "altura sobe com o QUADRADO do impulso, então 70% de altura pede 12 x raiz(0,7), " +
                 "e não 12 x 0,7 — este último daria 49% e um pulo bem mais curto que o pedido.")]
        [SerializeField] private float jumpForce = 10.04f;
        [Tooltip("Pulos antes de tocar o chão de novo. 2 = pulo duplo; 1 volta ao comportamento antigo.")]
        [SerializeField] private int maxJumps = 2;
        [SerializeField] private float gravityScale = 3f;

        [Header("Detecção de chão")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.15f;
        [SerializeField] private LayerMask groundLayer;

        [Header("Visual")]
        [SerializeField] private Transform visualRoot;

        private Rigidbody2D rb;
        private BoxCollider2D body;
        private InputActionMap playerMap;
        private InputAction moveAction;
        private InputAction jumpAction;
        private InputAction crouchAction;
        private InputAction sprintAction;

        private float moveInput;
        private bool crouchHeld;
        private bool sprintHeld;
        private bool jumpQueued;

        /// <summary>Pulos já gastos desde o último contato com o chão.</summary>
        private int jumpsUsed;
        private bool facingRight = true;
        private float ownVelocityX;
        private MovingPlatform currentPlatform;

        private Vector2 standingSize;
        private Vector2 standingOffset;

        public bool IsGrounded { get; private set; }

        /// <summary>
        /// +1 olhando para a direita, -1 para a esquerda. O espelhamento acontece na escala
        /// do <c>visualRoot</c>, não na raiz, então quem precisa da direção do personagem
        /// (a escalada, por exemplo) tem que perguntar aqui em vez de ler o transform.
        /// </summary>
        public float FacingSign => facingRight ? 1f : -1f;

        /// <summary>
        /// Velocidade horizontal de referência, sem sprint, agachamento ou efeito externo.
        /// O <see cref="Odisseia.Player.PlayerAnimator"/> a usa como denominador para saber a
        /// que fração da corrida normal o personagem está indo.
        /// </summary>
        public float MaxSpeed => maxSpeed;

        /// <summary>Sprint pedido e permitido — ver a nota em ApplyHorizontalMovement.</summary>
        public bool IsSprinting => sprintHeld && !IsCrouching;

        /// <summary>Verdadeiro enquanto o collider está encolhido — inclusive quando o
        /// jogador já soltou o botão mas continua preso debaixo de um teto.</summary>
        public bool IsCrouching { get; private set; }

        /// <summary>
        /// Suspende movimento e pulo sem desligar o componente. É o que a escalada usa:
        /// desligar o <see cref="PlayerController"/> derrubaria o action map inteiro, e o
        /// mapa é compartilhado com ataque, escudo e arco.
        /// </summary>
        public bool MovementSuspended { get; set; }

        /// <summary>Multiplicador de velocidade (1 = normal). Usado por efeitos como a sonolência do lótus.</summary>
        public float SpeedMultiplier { get; set; } = 1f;

        /// <summary>
        /// Velocidade horizontal adicional aplicada por forças externas (ex.: correntes de
        /// vento). É um deslocamento constante somado por cima do movimento do jogador, não
        /// cumulativo — por isso só afeta o eixo X (verticalmente isso brigaria com a gravidade).
        /// </summary>
        public float ExternalVelocityX { get; set; }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = gravityScale;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            body = GetComponent<BoxCollider2D>();
            if (body != null)
            {
                standingSize = body.size;
                standingOffset = body.offset;
            }

            if (inputActions != null)
            {
                playerMap = inputActions.FindActionMap(actionMapName, throwIfNotFound: false);
                if (playerMap != null)
                {
                    moveAction = playerMap.FindAction(moveActionName);
                    jumpAction = playerMap.FindAction(jumpActionName);
                    crouchAction = playerMap.FindAction(crouchActionName);
                    sprintAction = playerMap.FindAction(sprintActionName);
                }
            }
        }

        private void OnEnable()
        {
            playerMap?.Enable();
            if (jumpAction != null)
            {
                jumpAction.performed += OnJumpPerformed;
            }
        }

        private void OnDisable()
        {
            if (jumpAction != null)
            {
                jumpAction.performed -= OnJumpPerformed;
            }
            playerMap?.Disable();
        }

        private void OnJumpPerformed(InputAction.CallbackContext context)
        {
            // No ar o toque também vale, enquanto sobrar pulo. Quem valida de verdade é o
            // ApplyJump: entre este callback e o FixedUpdate o personagem pode ter pousado
            // ou saído do chão.
            if (IsGrounded || jumpsUsed < maxJumps)
            {
                jumpQueued = true;
            }
        }

        private void Update()
        {
            moveInput = moveAction != null ? moveAction.ReadValue<float>() : 0f;

            // Lê o valor do controle em vez de IsPressed(), igual ao escudo — assim o
            // agachar funciona como um "segure", não como um toque.
            crouchHeld = crouchAction != null && crouchAction.ReadValue<float>() > 0.5f;
            sprintHeld = sprintAction != null && sprintAction.ReadValue<float>() > 0.5f;
        }

        private void FixedUpdate()
        {
            CheckGrounded();

            if (MovementSuspended)
            {
                // Durante a escalada quem manda na posição é o PlayerClimb.
                jumpQueued = false;
                ownVelocityX = 0f;

                // Agarrado na parede o personagem tem apoio, como no chão: sair da escalada
                // devolve os dois pulos.
                jumpsUsed = 0;
                return;
            }

            UpdateCrouch();
            ApplyHorizontalMovement();
            ApplyJump();
            ApplyPlatformCarry();
            UpdateFacing();
        }

        /// <summary>
        /// Agacha enquanto o botão estiver segurado e o personagem estiver no chão.
        ///
        /// Levantar não é o simples oposto de agachar: se houver teto logo acima, soltar o
        /// botão não pode devolver o collider inteiro, senão o personagem atravessa a
        /// geometria ou é empurrado para fora dela. Por isso o estado só volta a "em pé"
        /// quando o espaço acima está livre.
        /// </summary>
        private void UpdateCrouch()
        {
            if (body == null)
            {
                return;
            }

            bool quer = crouchHeld && IsGrounded;

            if (!quer && IsCrouching && !TemEspacoParaLevantar())
            {
                quer = true;
            }

            if (quer == IsCrouching)
            {
                return;
            }

            IsCrouching = quer;

            if (IsCrouching)
            {
                float altura = standingSize.y * crouchHeightFactor;
                body.size = new Vector2(standingSize.x, altura);
                // Mantém os pés na mesma linha: o collider encolhe para baixo, não para o centro.
                body.offset = new Vector2(standingOffset.x, standingOffset.y - (standingSize.y - altura) * 0.5f);
            }
            else
            {
                body.size = standingSize;
                body.offset = standingOffset;
            }
        }

        /// <summary>Testa a faixa que o collider ocuparia se voltasse ao tamanho de pé.</summary>
        private bool TemEspacoParaLevantar()
        {
            float alturaAgachado = standingSize.y * crouchHeightFactor;
            float sobra = standingSize.y - alturaAgachado;
            if (sobra <= 0f)
            {
                return true;
            }

            // Só a fatia que falta, um pouco mais estreita que o corpo para não
            // encostar em paredes laterais e travar o personagem agachado sem motivo.
            Vector2 tamanho = new Vector2(standingSize.x * 0.9f, sobra * 0.9f);
            Vector2 centro = (Vector2)transform.position + body.offset
                + new Vector2(0f, alturaAgachado * 0.5f + sobra * 0.5f);

            return Physics2D.OverlapBox(centro, tamanho, 0f, groundLayer) == null;
        }

        private void CheckGrounded()
        {
            Collider2D hit = groundCheck != null
                ? Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer)
                : null;

            bool noChao = hit != null;

            if (noChao && rb.linearVelocity.y <= 0.01f)
            {
                // Recarrega ao pousar. A checagem de velocidade não é detalhe: no quadro
                // seguinte ao pulo o pé ainda está dentro do raio de detecção, e sem ela o
                // contador zeraria ali mesmo — pulo infinito segurando o botão.
                jumpsUsed = 0;
            }
            else if (!noChao && IsGrounded && jumpsUsed == 0)
            {
                // Saiu do chão andando, sem pular: o pulo do chão fica para trás. Sem isto,
                // quem anda para fora de uma borda ganha os DOIS pulos no ar e flutua.
                jumpsUsed = 1;
            }

            IsGrounded = noChao;
            currentPlatform = hit != null ? hit.GetComponent<MovingPlatform>() : null;
        }

        private void ApplyHorizontalMovement()
        {
            // Agachar VENCE o sprint em vez de os dois se multiplicarem: correr rápido
            // agachado não é um estado que a arte ou o design do jogo tenham.
            float limite = maxSpeed * (IsCrouching ? crouchSpeedMultiplier
                : IsSprinting ? sprintMultiplier
                : 1f);
            float targetSpeed = moveInput * limite * SpeedMultiplier;
            float rate = Mathf.Abs(moveInput) > 0.01f ? acceleration : deceleration;
            ownVelocityX = Mathf.MoveTowards(ownVelocityX, targetSpeed, rate * Time.fixedDeltaTime);

            rb.linearVelocity = new Vector2(ownVelocityX + ExternalVelocityX, rb.linearVelocity.y);
        }

        private void ApplyPlatformCarry()
        {
            if (currentPlatform != null)
            {
                rb.position += (Vector2)currentPlatform.FrameDelta;
            }
        }

        private void ApplyJump()
        {
            if (!jumpQueued)
            {
                return;
            }

            if (!IsGrounded && jumpsUsed >= maxJumps)
            {
                jumpQueued = false;
                return;
            }

            // Pular agachado levanta primeiro. Debaixo de um teto não há para onde
            // levantar, então o pulo simplesmente não sai — em vez de o personagem
            // subir dentro da geometria.
            if (IsCrouching)
            {
                if (!TemEspacoParaLevantar())
                {
                    jumpQueued = false;
                    return;
                }

                crouchHeld = false;
                UpdateCrouch();
            }

            jumpQueued = false;
            jumpsUsed++;

            // Velocidade vertical ZERADA e reescrita, não somada: sem isso o segundo pulo
            // dado no topo do arco (subindo ainda) empilharia impulso e subiria bem mais que
            // o dado já caindo — a altura do pulo duplo mudaria conforme o tempo do toque.
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            AudioManager.PlayJump();
        }

        private void UpdateFacing()
        {
            if (visualRoot == null)
            {
                return;
            }

            if (moveInput > 0.01f && !facingRight)
            {
                facingRight = true;
            }
            else if (moveInput < -0.01f && facingRight)
            {
                facingRight = false;
            }
            else
            {
                return;
            }

            Vector3 scale = visualRoot.localScale;
            scale.x = Mathf.Abs(scale.x) * (facingRight ? 1f : -1f);
            visualRoot.localScale = scale;
        }

        private void OnDrawGizmosSelected()
        {
            if (groundCheck == null)
            {
                return;
            }

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
