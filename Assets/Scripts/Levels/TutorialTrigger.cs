using UnityEngine;
using Odisseia.UI;

namespace Odisseia.Levels
{
    /// <summary>
    /// Ao ser tocado pelo jogador pela primeira vez, mostra uma dica curta de tutorial.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class TutorialTrigger : MonoBehaviour
    {
        [SerializeField] private TutorialPrompt prompt;
        [SerializeField] [TextArea] private string message;
        [SerializeField] private float displayDuration = 3f;

        private bool triggered;

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (triggered || !other.CompareTag("Player"))
            {
                return;
            }

            triggered = true;
            prompt?.Show(message, displayDuration);
        }
    }
}
