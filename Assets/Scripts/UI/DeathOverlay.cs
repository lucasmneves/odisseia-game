using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Odisseia.Systems;

namespace Odisseia.UI
{
    /// <summary>
    /// Feedback de morte: um aviso curto na tela ao perder uma vida, antes de o jogador
    /// reaparecer no checkpoint. Não interrompe o fluxo nem exige clique.
    ///
    /// Escuta <see cref="LivesCounter.LifeLost"/> em vez de <c>HealthSystem.Died</c>
    /// de propósito: na última morte não existe checkpoint para onde voltar, e este
    /// aviso não deve competir com a tela de fim de jogo. Como o evento só dispara
    /// quando ainda sobra vida, os dois casos nunca se cruzam — nem dependem da ordem
    /// em que os componentes recebem o <c>Died</c>.
    /// </summary>
    public class DeathOverlay : MonoBehaviour
    {
        // Não referencia mais o HealthSystem: quem decide se houve perda de vida (e se
        // ainda há checkpoint para onde voltar) é o LivesCounter.
        [SerializeField] private GameObject panel;
        [SerializeField] private Text messageText;
        [Tooltip("Reserva só para quando a chave não estiver na tabela de idiomas.")]
        [SerializeField] private string message = "Você caiu — retornando ao último checkpoint";
        [SerializeField] private string messageKey = "ui.death.message";
        [SerializeField] private float displayDuration = 1.4f;

        private Coroutine routine;

        private void Awake()
        {
            panel?.SetActive(false);
        }

        private void OnEnable()
        {
            LivesCounter.LifeLost += OnLifeLost;
        }

        private void OnDisable()
        {
            LivesCounter.LifeLost -= OnLifeLost;
        }

        private void OnLifeLost(int remaining)
        {
            if (routine != null)
            {
                StopCoroutine(routine);
            }

            routine = StartCoroutine(ShowRoutine(remaining));
        }

        private IEnumerator ShowRoutine(int remaining)
        {
            if (messageText != null)
            {
                // Tudo pela tabela (13B.6): antes a frase e o "vida(s) restante(s)" eram fixos em português.
                string frase = !string.IsNullOrEmpty(messageKey) && Localization.Has(messageKey) ? Localization.Get(messageKey) : message;
                string vidas = Localization.Get(remaining == 1 ? "ui.death.lifeLeft" : "ui.death.livesLeft", remaining);
                messageText.text = $"{frase}\n{vidas}";
            }

            panel?.SetActive(true);
            yield return new WaitForSeconds(displayDuration);
            panel?.SetActive(false);
            routine = null;
        }
    }
}
