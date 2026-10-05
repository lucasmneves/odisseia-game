using UnityEngine;
using UnityEngine.UI;
using Odisseia.Player;
using Odisseia.Systems;

namespace Odisseia.UI
{
    /// <summary>
    /// Indicador de fome na HUD (Gado do Sol). O pão (<c>icon_hunger</c>) é o rótulo visual; o texto vem da tabela de
    /// idiomas — antes era "🍖 Fome" fixo em português, e o emoji sumia no WebGL.
    /// </summary>
    public class HungerIndicator : MonoBehaviour
    {
        [SerializeField] private HungerMeter target;
        [SerializeField] private Text label;

        private void Start()
        {
            IndicatorIcon.InsidePanel(label, "icon_hunger");
            OnHungerChanged(1f);
        }

        private void OnEnable()
        {
            if (target != null)
            {
                target.HungerChanged += OnHungerChanged;
            }
        }

        private void OnDisable()
        {
            if (target != null)
            {
                target.HungerChanged -= OnHungerChanged;
            }
        }

        private void OnHungerChanged(float normalized)
        {
            if (label != null)
            {
                label.text = Localization.Get("ui.hud.hunger", Mathf.RoundToInt(normalized * 100f));
            }
        }
    }
}
