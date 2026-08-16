using UnityEngine;
using UnityEngine.InputSystem;
using Odisseia.Systems;

namespace Odisseia.Player
{
    /// <summary>
    /// Movimentação horizontal e pulo de Odisseu. Lê as ações do Input System
    /// (não referencia teclas diretamente).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private string actionMapName = "Player";
        [SerializeField] private string moveActionName = "Move";
        [SerializeField] private string jumpActionName = "Jump";

        [Header("Movimento")]
        [SerializeField] private float maxSpeed = 6f;
        [SerializeField] private float acceleration = 40f;
        [SerializeField] private float deceleration = 50f;

        [Header("Pulo")]
        [SerializeField] private float jumpForce = 12f;
        [SerializeField] private float gravityScale = 3f;

        [Header("Detecção de chão")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.15f;
        [SerializeField] private LayerMask groundLayer;

        [Header("Visual")]
        [SerializeField] private Transform visualRoot;

        private Rigidbody2D rb;
        private InputActionMap playerMap;
        private InputAction moveAction;
        private InputAction jumpAction;

        private float moveInput;
        private bool jumpQueued;
        private bool facingRight = true;
        private float ownVelocityX;
        private MovingPlatform currentPlatform;

        public bool IsGrounded { get; private set; }

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

            if (inputActions != null)
            {
                playerMap = inputActions.FindActionMap(actionMapName, throwIfNotFound: false);
                if (playerMap != null)
                {
                    moveAction = playerMap.FindAction(moveActionName);
                    jumpAction = playerMap.FindAction(jumpActionName);
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
            if (IsGrounded)
            {
                jumpQueued = true;
            }
        }

        private void Update()
        {
            moveInput = moveAction != null ? moveAction.ReadValue<float>() : 0f;
        }

        private void FixedUpdate()
        {
            CheckGrounded();
            ApplyHorizontalMovement();
            ApplyJump();
            ApplyPlatformCarry();
            UpdateFacing();
        }

        private void CheckGrounded()
        {
            Collider2D hit = groundCheck != null
                ? Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer)
                : null;

            IsGrounded = hit != null;
            currentPlatform = hit != null ? hit.GetComponent<MovingPlatform>() : null;
        }

        private void ApplyHorizontalMovement()
        {
            float targetSpeed = moveInput * maxSpeed * SpeedMultiplier;
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

            jumpQueued = false;
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
