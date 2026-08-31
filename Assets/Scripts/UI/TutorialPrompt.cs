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

        /// <summary>
        /// Quem escreveu a mensagem que está na tela.
        ///
        /// A caixa é uma só e vários componentes escrevem nela — NPCs, pontos de
        /// interação, portões, o curso de treino. Sem dono, o que acontece é isto: o
        /// jogador entra no alcance da Penélope (a dica dela aparece) e meio metro
        /// depois sai da área de aviso do portão, que apaga a dica DELA. Andando na
        /// borda entre as duas, pisca.
        ///
        /// Com dono, apagar só funciona para quem escreveu.
        /// </summary>
        private object owner;

        /// <summary>
        /// Dica que fica na tela ate alguem mandar tirar. O tutorial de combate usa
        /// isto porque a instrucao precisa continuar visivel enquanto o jogador nao
        /// executou o movimento - um aviso que some sozinho deixaria quem demora sem
        /// saber o que fazer.
        /// </summary>
        public void ShowPersistent(object dono, string message)
        {
            owner = dono;

            if (activeRoutine != null)
            {
                StopCoroutine(activeRoutine);
                activeRoutine = null;
            }

            if (messageText != null)
            {
                messageText.text = message;
            }

            if (panel != null)
            {
                panel.SetActive(true);
            }
        }

        /// <summary>
        /// Apaga a dica — mas só se quem pediu foi quem escreveu. Um <c>dono</c> nulo
        /// força, para quem precisa limpar a tela de qualquer jeito.
        /// </summary>
        public void Hide(object dono)
        {
            if (dono != null && !ReferenceEquals(dono, owner))
            {
                return;
            }

            owner = null;

            if (activeRoutine != null)
            {
                StopCoroutine(activeRoutine);
                activeRoutine = null;
            }

            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        /// <summary>
        /// Aviso passageiro. Assume a caixa: uma mensagem nova e explícita tem
        /// prioridade sobre a que estava lá.
        /// </summary>
        public void Show(object dono, string message, float duration)
        {
            owner = dono;

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

            owner = null;
            activeRoutine = null;
        }
    }
}
