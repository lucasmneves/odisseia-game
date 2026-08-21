using System.Collections.Generic;
using UnityEngine;

namespace Odisseia.Systems
{
    /// <summary>
    /// Reproduz animações trocando o sprite de um <see cref="SpriteRenderer"/> a partir
    /// de uma folha fatiada.
    ///
    /// Faz o papel de um Animator/AnimatorController — que este projeto nunca teve.
    /// A troca direta de sprite evita AnimationClips e máquinas de estado para um caso
    /// que é só "escolher a sequência certa", custa menos em WebGL e mantém a linha do
    /// resto do projeto (componentes pequenos, sem dependências externas).
    ///
    /// Os clipes vêm de <c>Resources.LoadAll</c>: cada sprite da folha se chama
    /// <c>&lt;Folha&gt;_&lt;Estado&gt;_&lt;NN&gt;</c> e o estado é lido do próprio nome, então
    /// refatiar a folha no Editor não quebra nenhuma referência serializada.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteAnimator : MonoBehaviour
    {
        [System.Serializable]
        public class StateSettings
        {
            public string state;
            public float framesPerSecond = 10f;
            public bool loop = true;
        }

        [Header("Folha")]
        [Tooltip("Caminho dentro de Resources, sem extensão. Ex.: Odisseia/Characters/CHR_Odysseus")]
        [SerializeField] private string resourcePath;
        [SerializeField] private string defaultState = "Idle";
        [SerializeField] private float defaultFramesPerSecond = 10f;

        [Header("Ajuste por estado (opcional)")]
        [SerializeField] private StateSettings[] states;

        private SpriteRenderer target;
        private readonly Dictionary<string, Sprite[]> clips = new Dictionary<string, Sprite[]>();
        private readonly Dictionary<string, StateSettings> settings = new Dictionary<string, StateSettings>();

        private Sprite[] current;
        private StateSettings currentSettings;
        private float timer;
        private int frame;

        /// <summary>Verdadeiro quando um clipe sem loop chegou ao último quadro.</summary>
        public bool IsFinished { get; private set; }

        public string CurrentState { get; private set; }

        /// <summary>Duração total do clipe, em segundos (0 se o estado não existir).</summary>
        public float GetStateDuration(string state)
        {
            if (string.IsNullOrEmpty(state) || !clips.TryGetValue(state, out Sprite[] frames))
            {
                return 0f;
            }

            float fps = settings.TryGetValue(state, out StateSettings s) && s.framesPerSecond > 0f
                ? s.framesPerSecond
                : defaultFramesPerSecond;

            return fps > 0f ? frames.Length / fps : 0f;
        }

        public bool HasState(string state) => !string.IsNullOrEmpty(state) && clips.ContainsKey(state);

        private void Awake()
        {
            target = GetComponent<SpriteRenderer>();

            if (states != null)
            {
                foreach (StateSettings s in states)
                {
                    if (s != null && !string.IsNullOrEmpty(s.state))
                    {
                        settings[s.state] = s;
                    }
                }
            }

            LoadClips();
        }

        private void Start()
        {
            if (CurrentState == null)
            {
                Play(defaultState);
            }
        }

        private void LoadClips()
        {
            if (string.IsNullOrEmpty(resourcePath))
            {
                return;
            }

            Sprite[] all = Resources.LoadAll<Sprite>(resourcePath);
            if (all == null || all.Length == 0)
            {
                Debug.LogWarning($"[SpriteAnimator] Nenhum sprite em Resources/{resourcePath} — o objeto fica com o sprite atual.", this);
                return;
            }

            // Agrupa por estado lido do nome: <Folha>_<Estado>_<NN>
            var buckets = new Dictionary<string, List<(int index, Sprite sprite)>>();
            foreach (Sprite sprite in all)
            {
                string name = sprite.name;
                int last = name.LastIndexOf('_');
                if (last <= 0)
                {
                    continue;
                }

                if (!int.TryParse(name.Substring(last + 1), out int index))
                {
                    continue;
                }

                string head = name.Substring(0, last);
                int sep = head.LastIndexOf('_');
                string state = sep >= 0 ? head.Substring(sep + 1) : head;

                if (!buckets.TryGetValue(state, out List<(int, Sprite)> list))
                {
                    list = new List<(int, Sprite)>();
                    buckets[state] = list;
                }
                list.Add((index, sprite));
            }

            foreach (KeyValuePair<string, List<(int index, Sprite sprite)>> pair in buckets)
            {
                pair.Value.Sort((a, b) => a.index.CompareTo(b.index));
                var frames = new Sprite[pair.Value.Count];
                for (int i = 0; i < frames.Length; i++)
                {
                    frames[i] = pair.Value[i].sprite;
                }
                clips[pair.Key] = frames;
            }
        }

        /// <summary>
        /// Troca o estado. Repetir o estado atual não reinicia a animação, a não ser
        /// que <paramref name="restart"/> seja verdadeiro — é o que permite chamar
        /// Play("Run") todo frame sem travar no primeiro quadro.
        /// </summary>
        public void Play(string state, bool restart = false)
        {
            if (string.IsNullOrEmpty(state))
            {
                return;
            }

            if (CurrentState == state && !restart)
            {
                return;
            }

            if (!clips.TryGetValue(state, out Sprite[] frames) || frames.Length == 0)
            {
                return;
            }

            CurrentState = state;
            current = frames;
            settings.TryGetValue(state, out currentSettings);
            timer = 0f;
            frame = 0;
            IsFinished = false;

            if (target != null)
            {
                target.sprite = current[0];
            }
        }

        private void Update()
        {
            if (current == null || current.Length == 0 || target == null)
            {
                return;
            }

            bool loop = currentSettings == null || currentSettings.loop;
            float fps = currentSettings != null && currentSettings.framesPerSecond > 0f
                ? currentSettings.framesPerSecond
                : defaultFramesPerSecond;

            if (fps <= 0f)
            {
                return;
            }

            if (!loop && frame >= current.Length - 1)
            {
                IsFinished = true;
                return;
            }

            timer += Time.deltaTime;
            float step = 1f / fps;

            while (timer >= step)
            {
                timer -= step;
                frame++;

                if (frame >= current.Length)
                {
                    if (loop)
                    {
                        frame = 0;
                    }
                    else
                    {
                        frame = current.Length - 1;
                        IsFinished = true;
                        break;
                    }
                }
            }

            target.sprite = current[frame];
        }
    }
}
