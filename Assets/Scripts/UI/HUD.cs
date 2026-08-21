using UnityEngine;
using UnityEngine.UI;
using Odisseia.Core;
using Odisseia.Player;
using Odisseia.Systems;

namespace Odisseia.UI
{
    /// <summary>
    /// HUD simples: vida do jogador, contador de coletáveis e munição do arco.
    /// </summary>
    public class HUD : MonoBehaviour
    {
        [SerializeField] private Text healthText;
        [SerializeField] private Text collectiblesText;
        [SerializeField] private Text arrowsText;
        [SerializeField] private HealthSystem playerHealth;

        private PlayerBow playerBow;

        private void Awake()
        {
            // O arco vive no mesmo GameObject do HealthSystem que a cena já referencia,
            // então não é preciso ligar mais nada no Inspector das 16 fases.
            if (playerHealth != null)
            {
                playerBow = playerHealth.GetComponent<PlayerBow>();
            }

            // As cenas existentes não têm um Text de flechas; criar em runtime evita
            // editar o HUD Canvas de todas elas.
            if (arrowsText == null && collectiblesText != null && playerBow != null)
            {
                arrowsText = CreateArrowsLabel(collectiblesText);
            }
        }

        private void OnEnable()
        {
            if (playerHealth != null)
            {
                playerHealth.Damaged += OnPlayerDamaged;
                playerHealth.Died += OnPlayerDied;
            }

            if (playerBow != null)
            {
                playerBow.ArrowsChanged += OnArrowsChanged;
                playerBow.OutOfArrows += OnOutOfArrows;
            }

            CollectibleCounter.CountChanged += OnCollectiblesChanged;

            RefreshHealth();
            RefreshCollectibles();
            RefreshArrows();
        }

        private void OnDisable()
        {
            if (playerHealth != null)
            {
                playerHealth.Damaged -= OnPlayerDamaged;
                playerHealth.Died -= OnPlayerDied;
            }

            if (playerBow != null)
            {
                playerBow.ArrowsChanged -= OnArrowsChanged;
                playerBow.OutOfArrows -= OnOutOfArrows;
            }

            CollectibleCounter.CountChanged -= OnCollectiblesChanged;
        }

        private void OnArrowsChanged(int current, int max)
        {
            RefreshArrows();
        }

        /// <summary>Pisca o contador em vermelho ao tentar atirar sem munição.</summary>
        private void OnOutOfArrows()
        {
            if (arrowsText != null)
            {
                arrowsText.color = UITheme.Health;
                CancelInvoke(nameof(RestoreArrowsColor));
                Invoke(nameof(RestoreArrowsColor), 0.4f);
            }
        }

        private void RestoreArrowsColor()
        {
            if (arrowsText != null)
            {
                arrowsText.color = playerBow != null && !playerBow.HasArrows
                    ? UITheme.TextSecondary
                    : UITheme.TextPrimary;
            }
        }

        private void RefreshArrows()
        {
            if (arrowsText == null)
            {
                return;
            }

            if (playerBow == null)
            {
                arrowsText.gameObject.SetActive(false);
                return;
            }

            arrowsText.text = $"➶ {playerBow.CurrentArrows}/{playerBow.MaxArrows}";
            RestoreArrowsColor();
        }

        /// <summary>Clona o posicionamento do contador de coletáveis, uma linha abaixo.</summary>
        private static Text CreateArrowsLabel(Text reference)
        {
            var go = new GameObject("ArrowsText", typeof(RectTransform));
            go.transform.SetParent(reference.transform.parent, false);

            var source = (RectTransform)reference.transform;
            var rect = (RectTransform)go.transform;
            rect.anchorMin = source.anchorMin;
            rect.anchorMax = source.anchorMax;
            rect.pivot = source.pivot;
            rect.sizeDelta = source.sizeDelta;
            rect.anchoredPosition = source.anchoredPosition + new Vector2(0f, -source.sizeDelta.y - 4f);

            var text = go.AddComponent<Text>();
            text.font = reference.font;
            text.fontSize = reference.fontSize;
            text.alignment = reference.alignment;
            text.color = reference.color;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
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
