using UnityEngine;
using UnityEngine.UI;
using Odisseia.Player;

namespace Odisseia.UI
{
    /// <summary>
    /// Barra de resistência ao canto das sereias na HUD (Image com fill horizontal), com a cera (<c>icon_wax</c>) à
    /// esquerda — a cera nos ouvidos dos companheiros é o que a barra mede.
    /// </summary>
    public class SirenResistanceIndicator : MonoBehaviour
    {
        [SerializeField] private SirenResistance target;
        [SerializeField] private Image fillImage;

        private void Start()
        {
            if (fillImage != null)
            {
                IndicatorIcon.LeftOf(fillImage.transform.parent as RectTransform, "icon_wax");
            }
        }

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
