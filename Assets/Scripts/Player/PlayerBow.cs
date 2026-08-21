using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Odisseia.Combat;
using Odisseia.Core;
using Odisseia.Systems;

namespace Odisseia.Player
{
    /// <summary>
    /// Ataque à distância com munição limitada. Dispara uma <see cref="Arrow"/> na
    /// direção em que Odisseu está olhando, gastando uma flecha por disparo.
    ///
    /// Não há "modo arco": o disparo funciona direto, alternando livremente com espada
    /// e escudo. A única restrição é não atirar enquanto defende.
    /// </summary>
    public class PlayerBow : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private string actionMapName = "Player";
        [SerializeField] private string bowActionName = "Bow";

        [Header("Munição")]
        [SerializeField] private int maxArrows = 10;
        [SerializeField] private int currentArrows = 10;

        [Header("Disparo")]
        [SerializeField] private Arrow arrowPrefab;
        [Tooltip("De onde a flecha nasce. Precisa ficar fora do collider do jogador.")]
        [SerializeField] private Transform firePoint;
        [SerializeField] private float bowCooldown = 0.5f;
        [SerializeField] private float arrowSpeed = 15f;
        [SerializeField] private int arrowDamage = 35;
        [SerializeField] private float arrowLifetime = 3f;
        [SerializeField] private LayerMask targetLayer;
        [SerializeField] private LayerMask obstacleLayer;

        [Header("Visual")]
        [Tooltip("Raiz virada pelo PlayerController. Define a direção do disparo.")]
        [SerializeField] private Transform visualRoot;

        private InputActionMap playerMap;
        private InputAction bowAction;
        private PlayerShield shield;
        private float cooldownTimer;

        public int CurrentArrows => currentArrows;
        public int MaxArrows => maxArrows;
        public bool HasArrows => currentArrows > 0;

        /// <summary>(atual, máximo) — a HUD escuta isto.</summary>
        public event Action<int, int> ArrowsChanged;

        /// <summary>Disparo efetivo (já descontou a flecha).</summary>
        public event Action Fired;

        /// <summary>Tentou atirar sem munição.</summary>
        public event Action OutOfArrows;

        private void Awake()
        {
            shield = GetComponent<PlayerShield>();

            if (inputActions != null)
            {
                playerMap = inputActions.FindActionMap(actionMapName, throwIfNotFound: false);
                bowAction = playerMap?.FindAction(bowActionName);
            }

            currentArrows = Mathf.Clamp(currentArrows, 0, maxArrows);
        }

        private void Start()
        {
            // Depois do Awake de todo mundo, para a HUD já estar inscrita.
            ArrowsChanged?.Invoke(currentArrows, maxArrows);
        }

        private void OnEnable()
        {
            playerMap?.Enable();

            if (bowAction != null)
            {
                bowAction.performed += OnBowPerformed;
            }

            ArrowsChanged?.Invoke(currentArrows, maxArrows);
        }

        private void OnDisable()
        {
            if (bowAction != null)
            {
                bowAction.performed -= OnBowPerformed;
            }
        }

        private void Update()
        {
            if (cooldownTimer > 0f)
            {
                cooldownTimer -= Time.deltaTime;
            }
        }

        private void OnBowPerformed(InputAction.CallbackContext context)
        {
            TryFire();
        }

        private void TryFire()
        {
            if (cooldownTimer > 0f)
            {
                return;
            }

            // Defendendo não se atira: o escudo ocupa as mãos.
            if (shield != null && shield.IsBlocking)
            {
                return;
            }

            if (currentArrows <= 0)
            {
                OutOfArrows?.Invoke();
                AudioManager.PlayUiClick();
                return;
            }

            if (arrowPrefab == null)
            {
                Debug.LogWarning("[PlayerBow] Sem arrowPrefab atribuído — nada foi disparado.", this);
                return;
            }

            cooldownTimer = bowCooldown;
            currentArrows--;

            Vector2 direction = FacingRight ? Vector2.right : Vector2.left;
            Vector3 origin = firePoint != null ? firePoint.position : transform.position + Vector3.up * 0.7f;

            Arrow arrow = Instantiate(arrowPrefab, origin, Quaternion.identity);
            arrow.Launch(direction, arrowSpeed, arrowDamage, arrowLifetime, targetLayer, obstacleLayer);

            Sprite sprite = GameAssets.Instance != null ? GameAssets.Instance.PlaceholderSprite : null;
            VfxBurst.Spawn(sprite, origin, new Color(0.95f, 0.9f, 0.7f), 3, 1.6f, 0.16f);
            AudioManager.PlayAttack();

            Fired?.Invoke();
            ArrowsChanged?.Invoke(currentArrows, maxArrows);
        }

        /// <summary>
        /// Repõe munição respeitando o teto. Devolve quantas flechas realmente entraram
        /// — o <c>ArrowPickup</c> usa isso para não sumir quando a aljava está cheia.
        /// </summary>
        public int AddArrows(int amount)
        {
            if (amount <= 0)
            {
                return 0;
            }

            int added = Mathf.Min(amount, maxArrows - currentArrows);
            if (added <= 0)
            {
                return 0;
            }

            currentArrows += added;
            ArrowsChanged?.Invoke(currentArrows, maxArrows);
            return added;
        }

        private bool FacingRight => visualRoot == null || visualRoot.localScale.x >= 0f;
    }
}
