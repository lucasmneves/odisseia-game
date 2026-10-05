using UnityEngine;
using Odisseia.Systems;
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

        [Tooltip("Ação citada no {0} da dica (Move, Jump, Attack...). Vazia: vem do fim do nome da chave, " +
                 "\"..._Attack\" -> Attack.")]
        [SerializeField] private string actionName;

        [SerializeField] [TextArea] private string message;
        [SerializeField] private float displayDuration = 3f;

        /// <summary>
        /// A chave manda; sem chave (ou sem entrada na tabela) vale o texto da cena,
        /// para uma dica nova continuar aparecendo enquanto a tradução não chega.
        ///
        /// O botão entra pelo <see cref="ControlHints"/>, lido na hora do toque: "Z" no
        /// teclado (ou a tecla remapeada), "X/□" no controle, "ATK" no toque. Antes a
        /// tecla vinha escrita no texto, e a dica de Troia mentia fora do teclado padrão.
        /// </summary>
        private string ResolvedMessage =>
            !string.IsNullOrEmpty(messageKey) && Localization.Has(messageKey)
                ? ControlHints.Instruction(messageKey, ResolvedAction)
                : message;

        private string ResolvedAction
        {
            get
            {
                if (!string.IsNullOrEmpty(actionName))
                {
                    return actionName;
                }

                int corte = messageKey.LastIndexOf('_');
                return corte >= 0 ? messageKey.Substring(corte + 1) : string.Empty;
            }
        }

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
