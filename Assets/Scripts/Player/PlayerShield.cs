using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Odisseia.Core;
using Odisseia.Systems;

namespace Odisseia.Player
{
    /// <summary>
    /// Defesa com escudo. Enquanto o botão está pressionado, Odisseu reduz o dano de
    /// golpes que chegam pelo arco frontal; golpes pelas costas passam inteiros.
    ///
    /// Implementa <see cref="IDamageMitigator"/> em vez de interceptar o
    /// <see cref="HealthSystem"/>: o sistema de vida continua sendo o único dono do
    /// dano, e aqui só se decide o quanto dele é absorvido.
    ///
    /// A estrutura já está pronta para stamina, durabilidade, parry e shield bash —
    /// nenhum deles implementado ainda, de propósito.
    /// </summary>
    [RequireComponent(typeof(HealthSystem))]
    public class PlayerShield : MonoBehaviour, IDamageMitigator
    {
        [Header("Input")]
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private string actionMapName = "Player";
        [SerializeField] private string shieldActionName = "Shield";

        [Header("Defesa")]
        [SerializeField] private bool shieldEnabled = true;
        [Tooltip("Fração do dano absorvida na defesa frontal. 0.8 = recebe 20% do dano.")]
        [SerializeField] [Range(0f, 1f)] private float damageReduction = 0.8f;
        [Tooltip("Abertura total do arco de defesa, em graus. 120 = 60 para cada lado da frente.")]
        [SerializeField] [Range(0f, 360f)] private float frontAngle = 120f;

        [Header("Visual")]
        [Tooltip("Raiz virada pelo PlayerController. Define para que lado o escudo aponta.")]
        [SerializeField] private Transform visualRoot;
        [Tooltip("Objeto ligado enquanto defende (placeholder até a arte final).")]
        [SerializeField] private GameObject shieldVisual;

        private InputActionMap playerMap;
        private InputAction shieldAction;

        /// <summary>Verdadeiro enquanto o botão de defesa está pressionado.</summary>
        public bool IsBlocking { get; private set; }

        /// <summary>Disparado quando a defesa liga/desliga.</summary>
        public event Action<bool> BlockingChanged;

        /// <summary>Disparado quando um golpe é efetivamente bloqueado.</summary>
        public event Action<int> Blocked;

        private void Awake()
        {
            if (inputActions != null)
            {
                playerMap = inputActions.FindActionMap(actionMapName, throwIfNotFound: false);
                shieldAction = playerMap?.FindAction(shieldActionName);
            }

            if (shieldAction == null)
            {
                Debug.LogWarning($"[PlayerShield] Ação '{shieldActionName}' não encontrada no map " +
                                 $"'{actionMapName}' — a defesa fica inativa.", this);
            }

            ApplyVisual(false);
        }

        private void OnEnable()
        {
            playerMap?.Enable();
        }

        private void OnDisable()
        {
            // Sair travado em defesa (diálogo, cutscene, morte) deixaria o jogador
            // imune sem poder agir.
            SetBlocking(false);
        }

        private void Update()
        {
            SetBlocking(shieldEnabled && ReadHeld());
        }

        /// <summary>
        /// Lê o valor do controle em vez de usar <c>IsPressed()</c>.
        /// A ação Shield é do tipo <c>Value</c> (como o Move), então o valor acompanha
        /// a tecla o tempo todo. Numa ação <c>Button</c> a fase volta para Waiting logo
        /// depois do Performed e o "segurar" se perderia — foi exatamente esse o bug.
        /// Vale igual para o botão touch, que alimenta o mesmo controle.
        /// </summary>
        private bool ReadHeld()
        {
            return shieldAction != null && shieldAction.ReadValue<float>() > 0.5f;
        }

        private void SetBlocking(bool value)
        {
            if (IsBlocking == value)
            {
                return;
            }

            IsBlocking = value;
            ApplyVisual(value);
            BlockingChanged?.Invoke(value);
        }

        private void ApplyVisual(bool blocking)
        {
            if (shieldVisual != null)
            {
                shieldVisual.SetActive(blocking);
            }
        }

        /// <inheritdoc />
        public int Mitigate(int amount, DamageInfo info)
        {
            if (!IsBlocking || !shieldEnabled)
            {
                return amount;
            }

            // Sem origem não há como saber a direção; ataques marcados como não
            // bloqueáveis ignoram o escudo por definição.
            if (!info.HasSource || !info.Blockable)
            {
                return amount;
            }

            if (!IsInFrontArc(info.Source))
            {
                return amount;
            }

            int reduced = Mathf.Max(0, Mathf.RoundToInt(amount * (1f - damageReduction)));
            OnBlocked(amount - reduced);
            return reduced;
        }

        /// <summary>
        /// O golpe está dentro do arco frontal? Compara a direção para a qual Odisseu
        /// olha com a direção de onde veio o ataque.
        /// </summary>
        private bool IsInFrontArc(Vector2 source)
        {
            Vector2 toSource = source - (Vector2)transform.position;

            // Origem praticamente em cima do jogador: trata como frontal em vez de
            // deixar o resultado depender de ruído numérico.
            if (toSource.sqrMagnitude < 0.0001f)
            {
                return true;
            }

            Vector2 facing = FacingRight ? Vector2.right : Vector2.left;
            return Vector2.Angle(facing, toSource) <= frontAngle * 0.5f;
        }

        /// <summary>
        /// Direção lida da escala do visual — é o mesmo sinal que o
        /// <see cref="PlayerController"/> inverte ao virar o personagem.
        /// </summary>
        private bool FacingRight => visualRoot == null || visualRoot.localScale.x >= 0f;

        private void OnBlocked(int absorbed)
        {
            Blocked?.Invoke(absorbed);

            Sprite sprite = GameAssets.Instance != null ? GameAssets.Instance.PlaceholderSprite : null;
            VfxBurst.Spawn(sprite, transform.position + Vector3.up * 0.7f,
                new Color(0.6f, 0.8f, 1f), 4, 2f, 0.22f);
            CameraFollow.ShakeActive(0.07f, 0.05f);
            AudioManager.PlayHit();
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 facing = (visualRoot == null || visualRoot.localScale.x >= 0f) ? Vector3.right : Vector3.left;
            Gizmos.color = new Color(0.4f, 0.7f, 1f);
            Quaternion half = Quaternion.Euler(0f, 0f, frontAngle * 0.5f);
            Gizmos.DrawRay(transform.position, half * facing * 2f);
            Gizmos.DrawRay(transform.position, Quaternion.Inverse(half) * facing * 2f);
        }
    }
}
