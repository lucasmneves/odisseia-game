using UnityEngine;
using UnityEngine.UI;
using Odisseia.Core;
using Odisseia.Systems;

namespace Odisseia.UI
{
    /// <summary>
    /// HUD simples: vida do jogador e contador de coletáveis.
    /// </summary>
    public class HUD : MonoBehaviour
    {
        [SerializeField] private Text healthText;
        [SerializeField] private Text collectiblesText;
        [SerializeField] private HealthSystem playerHealth;

        private void OnEnable()
        {
            if (playerHealth != null)
            {
                playerHealth.Damaged += OnPlayerDamaged;
                playerHealth.Died += OnPlayerDied;
            }

            CollectibleCounter.CountChanged += OnCollectiblesChanged;

            RefreshHealth();
            RefreshCollectibles();
        }

        private void OnDisable()
        {
            if (playerHealth != null)
            {
                playerHealth.Damaged -= OnPlayerDamaged;
                playerHealth.Died -= OnPlayerDied;
            }

            CollectibleCounter.CountChanged -= OnCollectiblesChanged;
        }

        private void OnPlayerDamaged(int amount, int currentHealth)
        {
            RefreshHealth();
        }

        private void OnPlayerDied()
        {
            RefreshHealth();
        }

        private void OnCollectiblesChanged(int count)
        {
            RefreshCollectibles();
        }

        private void RefreshHealth()
        {
            if (healthText != null && playerHealth != null)
            {
                healthText.text = $"♥ {playerHealth.CurrentHealth}/{playerHealth.MaxHealth}";
            }
        }

        private void RefreshCollectibles()
        {
            if (collectiblesText != null)
            {
                collectiblesText.text = $"★ {CollectibleCounter.Count}";
            }
        }
    }
}
