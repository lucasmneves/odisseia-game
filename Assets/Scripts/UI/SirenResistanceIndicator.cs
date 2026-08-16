using UnityEngine;
using UnityEngine.UI;
using Odisseia.Player;

namespace Odisseia.UI
{
    /// <summary>
    /// Barra de resistência ao canto das sereias na HUD (Image com fill horizontal).
    /// </summary>
    public class SirenResistanceIndicator : MonoBehaviour
    {
        [SerializeField] private SirenResistance target;
        [SerializeField] private Image fillImage;

        private void OnEnable()
        {
            if (target != null)
            {
                target.ResistanceChanged += OnResistanceChanged;
            }
        }

        private void OnDisable()
        {
            if (target != null)
            {
                target.ResistanceChanged -= OnResistanceChanged;
            }
        }

        private void OnResistanceChanged(float normalized)
        {
            if (fillImage != null)
            {
                fillImage.fillAmount = normalized;
            }
        }
    }
}
