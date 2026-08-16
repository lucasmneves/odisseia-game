using UnityEngine;
using UnityEngine.UI;
using Odisseia.Player;

namespace Odisseia.UI
{
    /// <summary>
    /// Indicador de fome na HUD (Fase 11).
    /// </summary>
    public class HungerIndicator : MonoBehaviour
    {
        [SerializeField] private HungerMeter target;
        [SerializeField] private Text label;

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
                label.text = $"🍖 Fome: {Mathf.RoundToInt(normalized * 100f)}%";
            }
        }
    }
}
