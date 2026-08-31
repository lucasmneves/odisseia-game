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

        [Tooltip("Chave de tradução. Preenchida, o texto vem da tabela de idiomas.")]
        [SerializeField] private string messageKey;

        [SerializeField] [TextArea] private string message;
        [SerializeField] private float displayDuration = 3f;

        /// <summary>
        /// A chave manda; sem chave (ou sem entrada na tabela) vale o texto da cena,
        /// para uma dica nova continuar aparecendo enquanto a tradução não chega.
        /// </summary>
        private string ResolvedMessage =>
            !string.IsNullOrEmpty(messageKey) && Odisseia.Systems.Localization.Has(messageKey)
                ? Odisseia.Systems.Localization.Get(messageKey)
                : message;

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
            prompt?.Show(this, ResolvedMessage, displayDuration);
        }
    }
}
