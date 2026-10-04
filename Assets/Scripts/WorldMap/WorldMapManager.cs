using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using Odisseia.Core;
using Odisseia.Systems;

namespace Odisseia.WorldMap
{
    /// <summary>
    /// Monta o mapa a partir da campanha e do save, posiciona Odisseu e trata a entrada
    /// nas fases.
    ///
    /// Não guarda progresso próprio: quem sabe o que está desbloqueado é o
    /// <see cref="CampaignManager"/>, apoiado no <c>SaveSystem</c>. Aqui isso só vira
    /// estado visual de nó e limite de caminhada.
    ///
    /// A apresentação (painéis, textos, avisos) fica no <see cref="WorldMapUI"/>, e a
    /// aparência de cada nó no <see cref="LevelNodeView"/> — este componente cuida das
    /// regras, não do desenho.
    /// </summary>
    public class WorldMapManager : MonoBehaviour
    {
        [Header("Cena")]
        [SerializeField] private WorldMapPath path;
        [SerializeField] private WorldMapPlayerController player;
        [SerializeField] private WorldMapUI ui;
        [SerializeField] private Transform nodesParent;
        [SerializeField] private Transform pathParent;

        [Header("Prefabs (trocáveis por arte definitiva)")]
        [SerializeField] private LevelNode nodePrefab;
        [SerializeField] private SpriteRenderer pathSegmentPrefab;
        [SerializeField] private float pathSegmentThickness = 0.14f;

        [Header("Traçado padrão (usado só se o caminho estiver vazio)")]
        [SerializeField] private float defaultSpacing = 4f;
        [SerializeField] private int defaultIntermediatePoints = 2;

        [Header("Input")]
        [SerializeField] private string actionMapName = "Player";
        [SerializeField] private string enterActionName = "Interact";

        [Header("Câmera")]
        [SerializeField] private CameraFollow cameraFollow;
        [SerializeField] private float cameraPadding = 6f;

        [Header("Caminhada automática após concluir uma fase")]
        [Tooltip("Espera antes de Odisseu sair andando, para o anúncio ser lido.")]
        [SerializeField] private float autoTravelDelay = 1.2f;
        [SerializeField] private float autoTravelSpeed = 3.5f;

        private readonly List<LevelNode> nodes = new List<LevelNode>();
        private InputAction enterAction;
        private LevelNode nearestNode;
        private bool entering;

        private void Awake()
        {
            InputActionAsset asset = GameAssets.Instance != null ? GameAssets.Instance.PlayerControls : null;
            InputActionMap map = asset != null ? asset.FindActionMap(actionMapName, throwIfNotFound: false) : null;
            enterAction = map?.FindAction(enterActionName);
            map?.Enable();
        }

        private void OnEnable()
        {
            if (enterAction != null)
            {
                enterAction.performed += OnEnterPerformed;
            }
        }

        private void OnDisable()
        {
            if (enterAction != null)
            {
                enterAction.performed -= OnEnterPerformed;
            }
        }

        private void Start()
        {
            BuildPathIfEmpty();
            BuildNodes();
            DrawPath();
            RefreshStates();
            PlacePlayer();
            ConfigureCamera();
            MontarMar();
            AnnounceIfJustCompleted();
        }

        // ---------------------------------------------------------------- construção

        private void BuildPathIfEmpty()
        {
            if (path == null || !path.IsEmpty)
            {
                return;
            }

            // Nasce funcional mesmo sem traçado desenhado no Inspector.
            path.GenerateStraightLine(LevelCount, defaultSpacing, defaultIntermediatePoints);
        }

        private int LevelCount => OrderedLevels().Count;

        private List<LevelDefinition> OrderedLevels()
        {
            CampaignManager campaign = CampaignManager.Instance;
            if (campaign == null)
            {
                return new List<LevelDefinition>();
            }

            return campaign.Levels.Where(level => level != null).OrderBy(level => level.Order).ToList();
        }

        private void BuildNodes()
        {
            nodes.Clear();

            if (path == null || nodePrefab == null)
            {
                Debug.LogWarning("[WorldMapManager] Caminho ou prefab de nó ausente — o mapa fica vazio.", this);
                return;
            }

            List<LevelDefinition> levels = OrderedLevels();
            int stops = Mathf.Min(levels.Count, path.LevelStopCount);

            if (stops < levels.Count)
            {
                Debug.LogWarning($"[WorldMapManager] O caminho tem {path.LevelStopCount} paradas para " +
                                 $"{levels.Count} fases — as excedentes ficam de fora do mapa.", this);
            }

            for (int i = 0; i < stops; i++)
            {
                Vector2 position = path.GetLevelStopPosition(i);
                LevelNode node = Instantiate(nodePrefab, position, Quaternion.identity,
                    nodesParent != null ? nodesParent : transform);

                node.Bind(levels[i], i, path.GetDistanceAtLevelStop(i));
                nodes.Add(node);
            }
        }

        // ---------------------------------------------------------------- arte do mapa (Asset Completion / Polish 02)

        private const string PastaDoMapa = "Odisseia/Map/";

        /// <summary>Distância entre os pontos da trilha, em unidades.</summary>
        private const float PassoDaTrilha = 0.42f;

        /// <summary>Cor MEDIDA da água do map_aegean (#577ea2, 44,7% dos pixels): o mar de fundo do mapa.</summary>
        private static readonly Color CorDoMar = new Color(0x57 / 255f, 0x7e / 255f, 0xa2 / 255f);

        private readonly List<(SpriteRenderer ponto, float distancia)> trilha = new List<(SpriteRenderer, float)>();
        private Sprite pontoNavegado, pontoPorNavegar;

        /// <summary>
        /// Trilha pontilhada (map_trail_dot: 00 navegado, vinho; 01 por navegar, areia) no lugar dos segmentos
        /// esticados. Devolve false sem a arte — aí o desenho antigo segue.
        /// </summary>
        private bool DesenharTrilhaPontilhada(Transform parent)
        {
            Sprite[] quadros = Resources.LoadAll<Sprite>(PastaDoMapa + "map_trail_dot");
            if (quadros.Length < 2)
            {
                return false;
            }

            System.Array.Sort(quadros, (a, b) => string.CompareOrdinal(a.name, b.name));
            pontoNavegado = quadros[0];
            pontoPorNavegar = quadros[1];

            float acumulado = 0f, proximo = 0f;
            foreach ((Vector2 from, Vector2 to) in path.Segments())
            {
                float comprimento = Vector2.Distance(from, to);
                while (proximo <= acumulado + comprimento && comprimento > 0.0001f)
                {
                    Vector2 posicao = Vector2.Lerp(from, to, (proximo - acumulado) / comprimento);
                    var go = new GameObject("TrailDot");
                    go.transform.SetParent(parent, false);
                    go.transform.position = posicao;
                    var sr = go.AddComponent<SpriteRenderer>();
                    sr.sprite = pontoPorNavegar;
                    sr.sortingOrder = 1;
                    trilha.Add((sr, proximo));
                    proximo += PassoDaTrilha;
                }

                acumulado += comprimento;
            }

            return true;
        }

        /// <summary>Pinta de "navegado" a trilha até onde o jogador já pode chegar.</summary>
        private void PintarTrilha(float ate)
        {
            foreach ((SpriteRenderer ponto, float distancia) in trilha)
            {
                if (ponto != null) { ponto.sprite = distancia <= ate + 0.01f ? pontoNavegado : pontoPorNavegar; }
            }
        }

        /// <summary>
        /// Mar: a câmera limpa na cor da água do map_aegean e brilhos de onda (map_wave_glint, 4 quadros que nascem, abrem
        /// e somem) espalhados pela área que a câmera percorre, cada um num ritmo — posições de semente fixa, então o
        /// mapa é o mesmo a cada visita. Antes era um fundo azul-escuro chapado (QA-16).
        /// </summary>
        private void MontarMar()
        {
            Camera cam = cameraFollow != null ? cameraFollow.GetComponent<Camera>() : Camera.main;
            if (cam != null)
            {
                cam.backgroundColor = CorDoMar;
            }

            if (path == null || path.IsEmpty || Resources.LoadAll<Sprite>(PastaDoMapa + "map_wave_glint").Length == 0)
            {
                return;
            }

            var min = new Vector2(float.MaxValue, float.MaxValue);
            var max = new Vector2(float.MinValue, float.MinValue);
            foreach ((Vector2 from, Vector2 to) in path.Segments())
            {
                min = Vector2.Min(min, Vector2.Min(from, to));
                max = Vector2.Max(max, Vector2.Max(from, to));
            }

            min -= Vector2.one * cameraPadding;
            max += Vector2.one * cameraPadding;
            var raiz = new GameObject("Sea_Glints").transform;
            var sorteio = new System.Random(16);
            int total = Mathf.Clamp(Mathf.RoundToInt((max.x - min.x) * (max.y - min.y) / 9f), 8, 90);
            for (int i = 0; i < total; i++)
            {
                var go = new GameObject("Glint");
                go.transform.SetParent(raiz, false);
                go.transform.position = new Vector3(
                    Mathf.Lerp(min.x, max.x, (float)sorteio.NextDouble()),
                    Mathf.Lerp(min.y, max.y, (float)sorteio.NextDouble()), 0f);
                go.AddComponent<SpriteRenderer>().sortingOrder = 0;
                go.AddComponent<SpriteAnimator>().Configure(PastaDoMapa + "map_wave_glint", "glint",
                    2.5f + (float)sorteio.NextDouble() * 2.5f);
            }
        }

        private void DrawPath()
        {
            if (path == null || pathSegmentPrefab == null)
            {
                return;
            }

            Transform parent = pathParent != null ? pathParent : transform;
            if (DesenharTrilhaPontilhada(parent))
            {
                return;
            }

            foreach ((Vector2 from, Vector2 to) in path.Segments())
            {
                Vector2 middle = (from + to) * 0.5f;
                float length = Vector2.Distance(from, to);
                if (length <= 0.0001f)
                {
                    continue;
                }

                SpriteRenderer segment = Instantiate(pathSegmentPrefab, middle, Quaternion.identity, parent);
                float angle = Mathf.Atan2(to.y - from.y, to.x - from.x) * Mathf.Rad2Deg;
                segment.transform.rotation = Quaternion.Euler(0f, 0f, angle);
                segment.transform.localScale = new Vector3(length, pathSegmentThickness, 1f);
            }
        }

        // ---------------------------------------------------------------- estado

        /// <summary>
        /// Traduz o progresso salvo em estado de nó. É o único lugar que faz essa
        /// tradução, então mapa e save nunca discordam.
        /// </summary>
        private void RefreshStates()
        {
            CampaignManager campaign = CampaignManager.Instance;

            foreach (LevelNode node in nodes)
            {
                LevelNodeState state;

                if (campaign == null)
                {
                    // Sem campanha carregada (abrir o mapa direto no Editor): tudo aberto,
                    // senão não dá para testar.
                    state = LevelNodeState.Available;
                }
                else if (campaign.IsCompleted(node.LevelId))
                {
                    state = LevelNodeState.Completed;
                }
                else if (campaign.IsUnlocked(node.LevelId))
                {
                    state = LevelNodeState.Available;
                }
                else
                {
                    state = LevelNodeState.Locked;
                }

                node.SetState(state);
            }

            if (player != null)
            {
                player.SetTravelLimit(FurthestReachableDistance());
            }

            PintarTrilha(FurthestReachableDistance());
        }

        /// <summary>Distância do nó desbloqueado mais avançado — é até onde dá para andar.</summary>
        private float FurthestReachableDistance()
        {
            float furthest = 0f;

            foreach (LevelNode node in nodes)
            {
                if (node.IsEnterable && node.DistanceAlongPath > furthest)
                {
                    furthest = node.DistanceAlongPath;
                }
            }

            return furthest;
        }

        private void PlacePlayer()
        {
            if (player == null || path == null || nodes.Count == 0)
            {
                return;
            }

            LevelNode target = ResolveStartNode();
            player.Initialize(path, target != null ? target.DistanceAlongPath : 0f, FurthestReachableDistance());
            MarkCurrent(target);
        }

        /// <summary>
        /// Onde Odisseu aparece: na fase de onde ele acabou de voltar, se houver esse
        /// recado; senão na concluída mais avançada; senão no começo.
        /// </summary>
        private LevelNode ResolveStartNode()
        {
            string focus = WorldMapSession.FocusLevelId;
            if (!string.IsNullOrEmpty(focus))
            {
                LevelNode focused = nodes.FirstOrDefault(node => node.LevelId == focus);
                if (focused != null)
                {
                    return focused;
                }
            }

            LevelNode lastCompleted = nodes.LastOrDefault(node => node.State == LevelNodeState.Completed);
            return lastCompleted ?? nodes[0];
        }

        private void MarkCurrent(LevelNode node)
        {
            nearestNode = node;

            if (node != null && node.State == LevelNodeState.Available)
            {
                node.SetState(LevelNodeState.Current);
            }

            ui?.ShowNode(node, TotalLevels: nodes.Count);
        }

        /// <summary>
        /// A câmera segue Odisseu e fica presa à extensão real do caminho, para não
        /// mostrar vazio além das pontas do mapa.
        /// </summary>
        private void ConfigureCamera()
        {
            if (cameraFollow == null || player == null)
            {
                return;
            }

            cameraFollow.SetTarget(player.transform);

            if (path == null || path.IsEmpty)
            {
                return;
            }

            var min = new Vector2(float.MaxValue, float.MaxValue);
            var max = new Vector2(float.MinValue, float.MinValue);

            foreach ((Vector2 from, Vector2 to) in path.Segments())
            {
                min = Vector2.Min(min, Vector2.Min(from, to));
                max = Vector2.Max(max, Vector2.Max(from, to));
            }

            // Caminho de um ponto só não gera segmento: sem limites úteis a calcular.
            if (min.x > max.x)
            {
                return;
            }

            var padding = new Vector2(cameraPadding, cameraPadding);
            cameraFollow.SetBounds(min - padding, max + padding);
        }

        private void AnnounceIfJustCompleted()
        {
            if (!WorldMapSession.JustCompleted)
            {
                return;
            }

            LevelNode completed = nodes.FirstOrDefault(node => node.LevelId == WorldMapSession.FocusLevelId);
            LevelNode unlocked = null;

            if (completed != null)
            {
                unlocked = nodes.FirstOrDefault(node =>
                    node.Order == completed.Order + 1 && node.IsEnterable);
            }

            ui?.ShowCompletionAnnouncement(completed, unlocked);
            WorldMapSession.Consume();

            if (completed != null && unlocked != null)
            {
                StartCoroutine(TravelToUnlocked(unlocked));
            }
        }

        /// <summary>
        /// Depois de concluir uma fase, Odisseu chega ao mapa em cima do nó que acabou
        /// de vencer — e é dali que a jornada continua. Sem esta caminhada ele fica
        /// parado no nó já concluído, e quem apertar "entrar" cai de volta na MESMA
        /// fase, como se o jogo não tivesse avançado.
        ///
        /// Andar sozinho até a próxima parada resolve isso e ainda mostra o progresso
        /// acontecendo, em vez de teletransportar.
        /// </summary>
        private IEnumerator TravelToUnlocked(LevelNode unlocked)
        {
            if (player == null)
            {
                yield break;
            }

            float from = player.Distance;
            float to = unlocked.DistanceAlongPath;

            if (Mathf.Approximately(from, to))
            {
                MarkCurrent(unlocked);
                yield break;
            }

            player.InputSuspended = true;

            // Um respiro para o anúncio de conclusão aparecer antes de Odisseu sair.
            yield return new WaitForSeconds(autoTravelDelay);

            float duration = Mathf.Max(0.2f, Mathf.Abs(to - from) / Mathf.Max(0.01f, autoTravelSpeed));
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                player.SetDistance(Mathf.Lerp(from, to, elapsed / duration));
                yield return null;
            }

            player.SetDistance(to);
            player.InputSuspended = false;

            MarkCurrent(unlocked);
        }

        // ---------------------------------------------------------------- interação

        private void Update()
        {
            if (player == null || nodes.Count == 0)
            {
                return;
            }

            LevelNode closest = nodes.FirstOrDefault(node => player.IsAt(node));

            if (closest != nearestNode)
            {
                nearestNode = closest;
                ui?.ShowNode(closest, TotalLevels: nodes.Count);
            }

            if (player.BlockedByLock)
            {
                ui?.ShowLockedFeedback();
            }
        }

        private void OnEnterPerformed(InputAction.CallbackContext context)
        {
            TryEnterCurrentNode();
        }

        /// <summary>Entra na fase do nó em que Odisseu está. Ligado também ao botão de toque.</summary>
        public void TryEnterCurrentNode()
        {
            if (entering || nearestNode == null || player == null || !player.IsAt(nearestNode))
            {
                return;
            }

            if (!nearestNode.IsEnterable)
            {
                ui?.ShowLockedFeedback();
                return;
            }

            entering = true;
            AudioManager.PlayUiClick();
            SceneLoader.LoadWithLoadingScreen(nearestNode.SceneName, nearestNode.LevelName);
        }

        /// <summary>Nó em que Odisseu está agora, ou nulo se está entre fases.</summary>
        public LevelNode CurrentNode => nearestNode;
    }
}
