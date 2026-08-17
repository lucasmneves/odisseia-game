using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Odisseia.UI
{
    /// <summary>
    /// Realça o botão enquanto pressionado. Sem algum feedback visual imediato, um
    /// botão touch "parece" não ter respondido ao toque mesmo quando respondeu.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class TouchButtonFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private float pressedAlphaMultiplier = 1.6f;

        private Image image;
        private Color baseColor;

        private void Awake()
        {
            image = GetComponent<Image>();
            baseColor = image.color;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            Color c = baseColor;
            c.a = Mathf.Clamp01(baseColor.a * pressedAlphaMultiplier);
            image.color = c;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            image.color = baseColor;
        }
    }
}
