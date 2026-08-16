using UnityEngine;
using UnityEngine.UI;
using Odisseia.Player;

namespace Odisseia.UI
{
    /// <summary>
    /// Indicador visual da sonolência do lótus na HUD: some quando não há efeito ativo.
    /// </summary>
    public class LotusIndicator : MonoBehaviour
    {
        [SerializeField] private LotusEffect target;
        [SerializeField] private GameObject panel;
        [SerializeField] private Text label;

        private void OnEnable()
        {
            if (target != null)
            {
                target.DrowsinessChanged += OnDrowsinessChanged;
            }

            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        private void OnDisable()
        {
            if (target != null)
            {
                target.DrowsinessChanged -= OnDrowsinessChanged;
            }
        }

        private void OnDrowsinessChanged(float normalized)
        {
            bool active = normalized > 0.01f;

            if (panel != null)
            {
                panel.SetActive(active);
            }

            if (label != null)
            {
                label.text = $"🌸 Sonolência {Mathf.RoundToInt(normalized * 100f)}%";
            }
        }
    }
}
