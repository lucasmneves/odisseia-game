using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Odisseia.UI
{
    [Serializable]
    public struct DialogueLine
    {
        public string speaker;
        [TextArea] public string text;
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
                speakerText.text = line.speaker;
                speakerText.gameObject.SetActive(!string.IsNullOrEmpty(line.speaker));
            }

            if (bodyText != null)
            {
                bodyText.text = line.text;
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
