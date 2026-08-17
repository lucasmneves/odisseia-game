using System.Globalization;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Odisseia.UI
{
    /// <summary>
    /// Encolhe este RectTransform para caber dentro da área segura da tela (notch,
    /// Dynamic Island, barra de gestos/home indicator). Screen.safeArea não reflete o
    /// notch real de um navegador em WebGL (só funciona nativamente em Android/iOS) —
    /// aqui os insets vêm de env(safe-area-inset-*) via JS, com Screen.safeArea como
    /// fallback fora do WebGL.
    /// </summary>
    public class SafeAreaController : MonoBehaviour
    {
        private RectTransform rect;
        private Vector2Int lastScreenSize = new Vector2Int(-1, -1);

        private void Awake()
        {
            rect = GetComponent<RectTransform>();
#if UNITY_WEBGL && !UNITY_EDITOR
            Odisseia_PatchViewportForSafeArea();
#endif
        }

        private void OnEnable() => Apply();

        private void Update()
        {
            if (Screen.width != lastScreenSize.x || Screen.height != lastScreenSize.y)
            {
                Apply();
            }
        }

        private void Apply()
        {
            lastScreenSize = new Vector2Int(Screen.width, Screen.height);

            Rect safeAreaPx = GetSafeAreaPixels();

            Vector2 anchorMin = safeAreaPx.position;
            Vector2 anchorMax = safeAreaPx.position + safeAreaPx.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private Rect GetSafeAreaPixels()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            string raw = Odisseia_GetSafeAreaInsets();
            if (!string.IsNullOrEmpty(raw))
            {
                string[] parts = raw.Split(',');
                if (parts.Length == 4 &&
                    TryParseCssPixels(parts[0], out float top) &&
                    TryParseCssPixels(parts[1], out float right) &&
                    TryParseCssPixels(parts[2], out float bottom) &&
                    TryParseCssPixels(parts[3], out float left))
                {
                    float width = Screen.width - left - right;
                    float height = Screen.height - top - bottom;
                    if (width > 0f && height > 0f)
                    {
                        return new Rect(left, bottom, width, height);
                    }
                }
            }
#endif
            return Screen.safeArea;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        private static bool TryParseCssPixels(string cssPx, out float value)
        {
            cssPx = cssPx.Trim();
            if (cssPx.EndsWith("px"))
            {
                cssPx = cssPx.Substring(0, cssPx.Length - 2);
            }

            return float.TryParse(cssPx, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        [DllImport("__Internal")]
        private static extern void Odisseia_PatchViewportForSafeArea();

        [DllImport("__Internal")]
        private static extern string Odisseia_GetSafeAreaInsets();
#endif
    }
}
