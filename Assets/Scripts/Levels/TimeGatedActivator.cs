using UnityEngine;
using UnityEngine.UI;

namespace Odisseia.Levels
{
    /// <summary>
    /// Ativa um GameObject (ex.: o objetivo da fase) só depois de um tempo de espera —
    /// representa a mecânica de "tempo" (esperar o vento mudar etc.). Genérico e
    /// reutilizável para qualquer fase futura que precise de um portão baseado em tempo.
    /// </summary>
    public class TimeGatedActivator : MonoBehaviour
    {
        [SerializeField] private float delay = 25f;
        [SerializeField] private GameObject target;
        [SerializeField] private Text countdownText;
        [SerializeField] private string countdownFormat = "🌬️ Aguardando vento favorável: {0}s";

        private float timer;
        private bool activated;

        private void Awake()
        {
            if (target != null)
            {
                target.SetActive(false);
            }

            timer = delay;
        }

        private void Update()
        {
            if (activated)
            {
                return;
            }

            timer -= Time.deltaTime;

            if (countdownText != null)
            {
                countdownText.text = string.Format(countdownFormat, Mathf.Max(0, Mathf.CeilToInt(timer)));
            }

            if (timer <= 0f)
            {
                activated = true;

                if (target != null)
                {
                    target.SetActive(true);
                }

                if (countdownText != null)
                {
                    countdownText.gameObject.SetActive(false);
                }
            }
        }
    }
}
