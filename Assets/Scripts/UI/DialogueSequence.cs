using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Odisseia.Systems;

namespace Odisseia.UI
{
    [Serializable]
    public struct DialogueLine
    {
        [Tooltip("Chave de tradução. Preenchida, o texto vem da tabela de idiomas e o " +
                 "campo abaixo passa a ser só a referência de quem edita a cena.")]
        public string key;

        public string speaker;
        [TextArea] public string text;

        /// <summary>
        /// Texto a mostrar. A chave manda; sem chave (ou sem entrada na tabela) vale o
        /// que está escrito na cena — assim uma fala nova continua aparecendo enquanto
        /// a tradução não chega, em vez de sumir.
        /// </summary>
        public string ResolvedText =>
            !string.IsNullOrEmpty(key) && Localization.Has(key) ? Localization.Get(key) : text;

        /// <summary>
        /// Nome de quem fala. Os nomes ficam sob "speaker.*" na tabela; um nome sem
        /// entrada (ex.: narração, que vem vazio) passa direto.
        /// </summary>
        public string ResolvedSpeaker
        {
            get
            {
                if (string.IsNullOrEmpty(speaker))
                {
                    return string.Empty;
                }

                string chave = "speaker." + speaker.ToLowerInvariant()
                    .Replace("ê", "e").Replace("é", "e").Replace("ó", "o").Replace("í", "i");

                return Localization.Has(chave) ? Localization.Get(chave) : speaker;
            }
        }
    }

    /// <summary>
    /// Sistema de diálogo reutilizável (usado em cutscenes de abertura/fechamento de
    /// fase): sequência de falas (nome + texto), com avanço automático por tempo,
    /// avanço manual e pular a sequência inteira. Os dados podem vir de um
    /// ScriptableObject (<see cref="DialogueData"/>) ou de uma lista inline — útil
    /// para reaproveitar o mesmo diálogo em vários lugares sem duplicar texto.
    /// </summary>
    public class DialogueSequence : MonoBehaviour
    {
        [Header("Dados")]
        [SerializeField] private DialogueData data;
        [SerializeField] private DialogueLine[] lines;

        [Header("Ritmo")]
        [SerializeField] private float secondsPerLine = 3f;

        [Header("UI")]
        [SerializeField] private GameObject panel;
        [SerializeField] private Text speakerText;
        [SerializeField] private Text bodyText;

        [Header("Input (avançar / pular)")]
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private string actionMapName = "Dialogue";
        [SerializeField] private string advanceActionName = "Advance";
        [SerializeField] private string skipActionName = "Skip";

        private InputActionMap dialogueMap;
        private InputAction advanceAction;
        private InputAction skipAction;

        private Coroutine routine;
        private bool advanceRequested;
        private bool skipRequested;

        public event Action Completed;

        private DialogueLine[] ActiveLines => data != null ? data.Lines : lines;

        private void Awake()
        {
            if (inputActions == null)
            {
                return;
            }

            dialogueMap = inputActions.FindActionMap(actionMapName, throwIfNotFound: false);
            if (dialogueMap != null)
            {
                advanceAction = dialogueMap.FindAction(advanceActionName);
                skipAction = dialogueMap.FindAction(skipActionName);
            }
        }

        public void Play()
        {
            if (panel != null)
            {
                panel.SetActive(true);
            }

            advanceRequested = false;
            skipRequested = false;
            dialogueMap?.Enable();

            if (advanceAction != null)
            {
                advanceAction.performed += OnAdvancePerformed;
            }

            if (skipAction != null)
            {
                skipAction.performed += OnSkipPerformed;
            }

            if (routine != null)
            {
                StopCoroutine(routine);
            }

            routine = StartCoroutine(PlayRoutine());
        }

        /// <summary>Pula a linha atual imediatamente (chamável por UI, além do input).</summary>
        public void Advance()
        {
            advanceRequested = true;
        }

        /// <summary>Encerra a sequência inteira imediatamente (chamável por UI, além do input).</summary>
        public void Skip()
        {
            skipRequested = true;
        }

        private void OnAdvancePerformed(InputAction.CallbackContext context)
        {
            advanceRequested = true;
        }

        private void OnSkipPerformed(InputAction.CallbackContext context)
        {
            skipRequested = true;
        }

        private IEnumerator PlayRoutine()
        {
            DialogueLine[] activeLines = ActiveLines;

            if (activeLines != null)
            {
                foreach (DialogueLine line in activeLines)
                {
                    if (skipRequested)
                    {
                        break;
                    }

                    ShowLine(line);

                    advanceRequested = false;
                    float elapsed = 0f;
                    while (elapsed < secondsPerLine && !advanceRequested && !skipRequested)
                    {
                        elapsed += Time.deltaTime;
                        yield return null;
                    }
                }
            }

            EndSequence();
        }

        private void ShowLine(DialogueLine line)
        {
            if (speakerText != null)
            {
                string quemFala = line.ResolvedSpeaker;
                speakerText.text = quemFala;
                speakerText.gameObject.SetActive(!string.IsNullOrEmpty(quemFala));
            }

            if (bodyText != null)
            {
                bodyText.text = line.ResolvedText;
            }
        }

        private void EndSequence()
        {
            advanceRequested = false;
            skipRequested = false;

            if (advanceAction != null)
            {
                advanceAction.performed -= OnAdvancePerformed;
            }

            if (skipAction != null)
            {
                skipAction.performed -= OnSkipPerformed;
            }

            dialogueMap?.Disable();

            if (panel != null)
            {
                panel.SetActive(false);
            }

            routine = null;
            Completed?.Invoke();
        }
    }
}
