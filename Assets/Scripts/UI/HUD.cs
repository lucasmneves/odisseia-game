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
        [SerializeField] private Text livesText;
        [SerializeField] private Text experienceText;
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

            // As cenas existentes só têm os Texts de vida e coletáveis; os demais são
            // criados em runtime, empilhados abaixo, para não editar o HUD Canvas das
            // 16 fases.
            if (collectiblesText != null)
            {
                int slot = 1;

                if (arrowsText == null && playerBow != null)
                {
                    arrowsText = CreateStackedLabel(collectiblesText, slot++, "ArrowsText");
                }

                if (livesText == null)
                {
                    livesText = CreateStackedLabel(collectiblesText, slot++, "LivesText");
                }

                if (experienceText == null)
                {
                    experienceText = CreateStackedLabel(collectiblesText, slot, "ExperienceText");
                }
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
            LivesCounter.Changed += OnLivesChanged;
            ExperienceCounter.Changed += OnExperienceChanged;

            RefreshHealth();
            RefreshCollectibles();
            RefreshArrows();
            RefreshLives();
            RefreshExperience();
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
            LivesCounter.Changed -= OnLivesChanged;
            ExperienceCounter.Changed -= OnExperienceChanged;
        }

        private void OnLivesChanged(int current) => RefreshLives();

        private void OnExperienceChanged(int total, int towardNext) => RefreshExperience();

        private void RefreshLives()
        {
            if (livesText != null)
            {
                livesText.text = $"Vidas {LivesCounter.Current}";
            }
        }

        private void RefreshExperience()
        {
            if (experienceText != null)
            {
                experienceText.text =
                    $"XP {ExperienceCounter.TowardNextLife}/{ExperienceCounter.ExperiencePerLife}";
            }
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

        /// <summary>
        /// Clona o posicionamento de um Text existente, deslocado <paramref name="slot"/>
        /// linhas abaixo. Mantém a HUD alinhada sem depender de layout group.
        /// </summary>
        private static Text CreateStackedLabel(Text reference, int slot, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(reference.transform.parent, false);

            var source = (RectTransform)reference.transform;
            float step = source.sizeDelta.y + 4f;

            var rect = (RectTransform)go.transform;
            rect.anchorMin = source.anchorMin;
            rect.anchorMax = source.anchorMax;
            rect.pivot = source.pivot;
            rect.sizeDelta = source.sizeDelta;
            rect.anchoredPosition = source.anchoredPosition + new Vector2(0f, -step * slot);

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
