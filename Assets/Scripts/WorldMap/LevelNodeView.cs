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

        /// <summary>
        /// Reaplica o estado depois do Bind (que vem logo após o Instantiate, no mesmo quadro). Sem isto o nó BLOQUEADO
        /// ficava com o quadrado: o estado dele nunca muda, então o Apply só tinha rodado antes de a ordem ser conhecida.
        /// </summary>
        private void Start()
        {
            if (node != null)
            {
                Apply(node.State);
            }
        }

        private void OnDisable()
        {
            if (node != null)
            {
                node.StateChanged -= Apply;
            }
        }

        // ---------------------------------------------------------------- emblemas (Asset Completion PXL-018)

        private const string PastaDoMapa = "Odisseia/Map/";

        private bool emblemaTentado;
        private Sprite emblema, emblemaBloqueado;
        private GameObject louro, aro;

        /// <summary>
        /// Emblema da fase (<c>emblem_NN</c>, NN = ordem na campanha) e a variante bloqueada; louro atrás quando
        /// concluída e aro animado na frente quando atual. Carregado no primeiro Apply com a ordem conhecida — o nó é
        /// instanciado (Awake/OnEnable) antes do Bind que diz qual fase ele é. Sem a arte, fica o quadrado colorido.
        /// </summary>
        private void TentarEmblema()
        {
            if (emblemaTentado || node == null || node.Order <= 0)
            {
                return;
            }

            emblemaTentado = true;
            emblema = Resources.Load<Sprite>($"{PastaDoMapa}Emblems/emblem_{node.Order:00}");
            emblemaBloqueado = Resources.Load<Sprite>($"{PastaDoMapa}Emblems/emblem_{node.Order:00}_locked");
            if (emblema == null)
            {
                return;
            }

            // O emblema já é a marca de concluído (com o louro); o ✓ antigo sairia por cima dele.
            if (completedMark != null) { completedMark.SetActive(false); completedMark = null; }

            Sprite folhaDeLouro = Resources.Load<Sprite>(PastaDoMapa + "States/map_state_completed_wreath");
            if (folhaDeLouro != null)
            {
                louro = Camada("Wreath", folhaDeLouro, spriteRenderer.sortingOrder - 1);
            }

            // Ativo ao receber o animador (o Awake dele, que pega o SpriteRenderer, só roda em objeto ativo).
            aro = Camada("CurrentRing", null, spriteRenderer.sortingOrder + 1, ativo: true);
            var animador = aro.AddComponent<Odisseia.Systems.SpriteAnimator>();
            animador.Configure(PastaDoMapa + "States/map_state_current_ring", "ring", 8f);
            aro.SetActive(false);
        }

        private GameObject Camada(string nome, Sprite sprite, int ordem, bool ativo = false)
        {
            var go = new GameObject(nome);
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = ordem;
            go.SetActive(ativo);
            return go;
        }

        private void Apply(LevelNodeState state)
        {
            if (spriteRenderer == null)
            {
                return;
            }

            TentarEmblema();
            if (emblema != null)
            {
                // Arte nativa (42,857 px/un) em escala 1: o emblema tem 1,31 un. O nó atual não cresce — o aro
                // animado é o que o marca; o pulso continua, sutil.
                spriteRenderer.sprite = state == LevelNodeState.Locked && emblemaBloqueado != null ? emblemaBloqueado : emblema;
                spriteRenderer.color = Color.white;
                targetScale = 1f;
                transform.localScale = Vector3.one;
                if (louro != null) { louro.SetActive(state == LevelNodeState.Completed); }
                if (aro != null) { aro.SetActive(state == LevelNodeState.Current); }
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
