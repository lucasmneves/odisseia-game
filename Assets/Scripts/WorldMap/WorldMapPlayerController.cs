using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Odisseia.Core;

namespace Odisseia.WorldMap
{
    /// <summary>
    /// Odisseu andando pelo mapa. Deliberadamente separado do <c>PlayerController</c> de
    /// gameplay: aqui não há física, pulo, gravidade nem combate — só uma posição ao
    /// longo do <see cref="WorldMapPath"/>.
    ///
    /// O movimento é por DISTÂNCIA percorrida no caminho, não por posição livre. Isso
    /// resolve de graça o requisito de não sair da trilha e o de não atravessar uma fase
    /// bloqueada: basta limitar a distância máxima.
    ///
    /// A entrada vem da MESMA ação "Move" do Input System usada nas fases, então o
    /// remapeamento de teclas e os botões de toque continuam valendo, sem sistema de
    /// input paralelo.
    /// </summary>
    public class WorldMapPlayerController : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField] private string actionMapName = "Player";
        [SerializeField] private string moveActionName = "Move";

        [Header("Movimento")]
        [SerializeField] private float moveSpeed = 4.5f;
        [Tooltip("Distância até um nó para considerar que Odisseu chegou nele.")]
        [SerializeField] private float arriveThreshold = 0.45f;

        [Header("Visual")]
        [Tooltip("Raiz virada conforme a direção da caminhada.")]
        [SerializeField] private Transform visualRoot;

        private WorldMapPath path;
        private InputActionMap playerMap;
        private InputAction moveAction;

        private float distance;
        private float maxDistance;
        private bool facingRight = true;

        /// <summary>Distância atual de Odisseu ao longo do caminho.</summary>
        public float Distance => distance;

        /// <summary>Verdadeiro enquanto o jogador empurra contra o limite de progresso.</summary>
        public bool BlockedByLock { get; private set; }

        /// <summary>
        /// Ignora a entrada do jogador. Usado enquanto o mapa move Odisseu sozinho (a
        /// caminhada até a fase recém-desbloqueada), para os dois não disputarem a
        /// mesma distância no mesmo frame.
        /// </summary>
        public bool InputSuspended { get; set; }

        /// <summary>Disparado quando a distância muda.</summary>
        public event Action<float> Moved;

        public void Initialize(WorldMapPath worldPath, float startDistance, float travelLimit)
        {
            path = worldPath;
            maxDistance = travelLimit;
            SetDistance(startDistance);
        }

        /// <summary>Até onde Odisseu pode caminhar (fim da última fase desbloqueada).</summary>
        public void SetTravelLimit(float limit)
        {
            maxDistance = Mathf.Max(0f, limit);
            SetDistance(distance);
        }

        public void SetDistance(float value)
        {
            if (path == null)
            {
                return;
            }

            distance = Mathf.Clamp(value, 0f, Mathf.Min(maxDistance, path.TotalLength));
            transform.position = path.Evaluate(distance);
            Moved?.Invoke(distance);
        }

        /// <summary>Verdadeiro se Odisseu está parado em cima de um nó.</summary>
        public bool IsAt(LevelNode node)
        {
            return node != null && Mathf.Abs(distance - node.DistanceAlongPath) <= arriveThreshold;
        }

        private void Awake()
        {
            InputActionAsset asset = GameAssets.Instance != null ? GameAssets.Instance.PlayerControls : null;
            if (asset != null)
            {
                playerMap = asset.FindActionMap(actionMapName, throwIfNotFound: false);
                moveAction = playerMap?.FindAction(moveActionName);
            }

            if (moveAction == null)
            {
                Debug.LogWarning($"[WorldMapPlayerController] Ação '{moveActionName}' não encontrada — " +
                                 "o mapa fica sem movimento.", this);
            }

            if (visualRoot == null)
            {
                visualRoot = transform;
            }
        }

        private void OnEnable() => playerMap?.Enable();

        private void Update()
        {
            if (path == null || moveAction == null || InputSuspended)
            {
                return;
            }

            float input = moveAction.ReadValue<float>();
            BlockedByLock = false;

            if (Mathf.Abs(input) < 0.01f)
            {
                return;
            }

            float next = distance + input * moveSpeed * Time.deltaTime;
            float limit = Mathf.Min(maxDistance, path.TotalLength);

            // Empurrando para frente já no limite: é o bloqueio da próxima fase.
            if (next > limit && distance >= limit - 0.001f)
            {
                BlockedByLock = true;
            }

            UpdateFacing(input);
            SetDistance(next);
        }

        /// <summary>
        /// O navio de Odisseu (map_ship_marker, flâmula carmesim) no lugar do retângulo: sprite nativo em escala 1 — o
        /// sinal do x continua sendo a direção, como o UpdateFacing espera. Sem a arte, fica o retângulo.
        /// </summary>
        private void Start()
        {
            var sr = visualRoot != null ? visualRoot.GetComponent<SpriteRenderer>() : null;
            Sprite navio = Resources.Load<Sprite>("Odisseia/Map/map_ship_marker");
            if (sr == null || navio == null)
            {
                return;
            }

            sr.sprite = navio;
            sr.color = Color.white;
            visualRoot.localScale = new Vector3(facingRight ? 1f : -1f, 1f, 1f);
        }

        private void UpdateFacing(float input)
        {
            bool right = input > 0f;
            if (right == facingRight || visualRoot == null)
            {
                return;
            }

            facingRight = right;
            Vector3 scale = visualRoot.localScale;
            scale.x = Mathf.Abs(scale.x) * (facingRight ? 1f : -1f);
            visualRoot.localScale = scale;
        }
    }
}
