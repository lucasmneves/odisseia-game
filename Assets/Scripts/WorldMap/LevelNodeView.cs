using UnityEngine;

namespace Odisseia.WorldMap
{
    /// <summary>
    /// Aparência de um <see cref="LevelNode"/> conforme o estado. É o único ponto que
    /// conhece cor e tamanho do nó — trocar por arte definitiva depois é substituir o
    /// prefab e, se preciso, este componente, sem tocar na lógica do mapa.
    ///
    /// Tudo é campo serializado de propósito: nada de sprite ou cor fixados no código.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class LevelNodeView : MonoBehaviour
    {
        [Header("Cores por estado")]
        [SerializeField] private Color lockedColor = new Color(0.32f, 0.32f, 0.36f);
        [SerializeField] private Color availableColor = new Color(0.35f, 0.62f, 0.95f);
        [SerializeField] private Color currentColor = new Color(1f, 0.82f, 0.42f);
        [SerializeField] private Color completedColor = new Color(0.42f, 0.78f, 0.48f);

        [Header("Escala por estado")]
        [SerializeField] private float baseScale = 0.6f;
        [SerializeField] private float currentScaleMultiplier = 1.35f;

        [Header("Marca de concluído")]
        [Tooltip("Objeto opcional ligado quando a fase está concluída (o ✓).")]
        [SerializeField] private GameObject completedMark;

        [Header("Pulso do nó atual")]
        [SerializeField] private float pulseAmplitude = 0.08f;
        [SerializeField] private float pulseSpeed = 3f;

        private SpriteRenderer spriteRenderer;
        private LevelNode node;
        private float targetScale;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            node = GetComponentInParent<LevelNode>();
            targetScale = baseScale;
        }

        private void OnEnable()
        {
            if (node != null)
            {
                node.StateChanged += Apply;
                node.NotifyState();
            }
        }

        private void OnDisable()
        {
            if (node != null)
            {
                node.StateChanged -= Apply;
            }
        }

        private void Apply(LevelNodeState state)
        {
            if (spriteRenderer == null)
            {
                return;
            }

            spriteRenderer.color = state switch
            {
                LevelNodeState.Available => availableColor,
                LevelNodeState.Current => currentColor,
                LevelNodeState.Completed => completedColor,
                _ => lockedColor,
            };

            targetScale = state == LevelNodeState.Current
                ? baseScale * currentScaleMultiplier
                : baseScale;

            transform.localScale = Vector3.one * targetScale;

            if (completedMark != null)
            {
                completedMark.SetActive(state == LevelNodeState.Completed);
            }
        }

        private void Update()
        {
            // Só o nó atual pulsa — é o que dá para o jogador achar onde ele está.
            if (node == null || node.State != LevelNodeState.Current || pulseAmplitude <= 0f)
            {
                return;
            }

            float pulse = 1f + Mathf.Sin(Time.unscaledTime * pulseSpeed) * pulseAmplitude;
            transform.localScale = Vector3.one * (targetScale * pulse);
        }
    }
}
