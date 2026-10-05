using UnityEngine;
using UnityEngine.UI;
using Odisseia.Player;
using Odisseia.Systems;

namespace Odisseia.UI
{
    /// <summary>
    /// Indicador visual da sonolência do lótus na HUD: some quando não há efeito ativo. A flor (<c>icon_lotus</c>) é o
    /// rótulo visual; o texto vem da tabela de idiomas (antes, "🌸 Sonolência" fixo, e o emoji sumia no WebGL).
    /// </summary>
    public class LotusIndicator : MonoBehaviour
    {
        [SerializeField] private LotusEffect target;
        [SerializeField] private GameObject panel;
        [SerializeField] private Text label;

        private void Start()
        {
            IndicatorIcon.InsidePanel(label, "icon_lotus");
        }

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
                label.text = Localization.Get("ui.hud.lotus", Mathf.RoundToInt(normalized * 100f));
            }
        }
    }
}
