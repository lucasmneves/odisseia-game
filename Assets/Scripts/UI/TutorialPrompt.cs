using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Odisseia.UI
{
    /// <summary>
    /// Caixa de texto curta e reutilizável para dicas de tutorial (ex.: "Pressione SPACE para pular").
    /// </summary>
    public class TutorialPrompt : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Text messageText;

        private Coroutine activeRoutine;

        public void Show(string message, float duration)
        {
            if (activeRoutine != null)
            {
                StopCoroutine(activeRoutine);
            }

            activeRoutine = StartCoroutine(ShowRoutine(message, duration));
        }

        private IEnumerator ShowRoutine(string message, float duration)
        {
            if (messageText != null)
            {
                messageText.text = message;
            }

            if (panel != null)
            {
                panel.SetActive(true);
            }

            yield return new WaitForSeconds(duration);

            if (panel != null)
            {
                panel.SetActive(false);
            }

            activeRoutine = null;
        }
    }
}
