using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Odisseia.Core;

namespace Odisseia.UI
{
    /// <summary>
    /// Feedback de morte: um aviso curto na tela ao perder toda a vida, antes de o
    /// jogador reaparecer no checkpoint. Não interrompe o fluxo nem exige clique — a
    /// regra de respawn continua exatamente a mesma de antes; isto é só a camada de
    /// comunicação que faltava para a morte não passar despercebida.
    /// </summary>
    public class DeathOverlay : MonoBehaviour
    {
        [SerializeField] private HealthSystem playerHealth;
        [SerializeField] private GameObject panel;
        [SerializeField] private Text messageText;
        [SerializeField] private string message = "Você caiu — retornando ao último checkpoint";
        [SerializeField] private float displayDuration = 1.4f;

        private Coroutine routine;

        private void Awake()
        {
            panel?.SetActive(false);
        }

        private void OnEnable()
        {
            if (playerHealth != null)
            {
                playerHealth.Died += OnPlayerDied;
            }
        }

        private void OnDisable()
        {
            if (playerHealth != null)
            {
                playerHealth.Died -= OnPlayerDied;
            }
        }

        private void OnPlayerDied()
        {
            if (routine != null)
            {
                StopCoroutine(routine);
            }

            routine = StartCoroutine(ShowRoutine());
        }

        private IEnumerator ShowRoutine()
        {
            if (messageText != null)
            {
                messageText.text = message;
            }

            panel?.SetActive(true);
            yield return new WaitForSeconds(displayDuration);
            panel?.SetActive(false);
            routine = null;
        }
    }
}
