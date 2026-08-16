using UnityEngine;

namespace Odisseia.UI
{
    /// <summary>
    /// Paleta e medidas compartilhadas por toda a UI (menu, gameplay, pause, game over,
    /// vitória, seleção de fases). Centraliza as constantes para que as telas fiquem
    /// visualmente consistentes — e para que ajustar o tema seja mudar um arquivo só.
    /// </summary>
    public static class UITheme
    {
        // Fundo e painéis
        public static readonly Color PanelBackground = new Color(0.08f, 0.10f, 0.16f, 0.92f);
        public static readonly Color OverlayDim = new Color(0.04f, 0.05f, 0.09f, 0.78f);
        public static readonly Color DialoguePanel = new Color(0.08f, 0.10f, 0.16f, 0.88f);

        // Texto
        public static readonly Color TextPrimary = new Color(0.97f, 0.96f, 0.92f);
        public static readonly Color TextSecondary = new Color(0.76f, 0.78f, 0.84f);
        public static readonly Color TextAccent = new Color(1f, 0.82f, 0.42f);

        // Botões
        public static readonly Color ButtonNormal = new Color(0.20f, 0.34f, 0.55f);
        public static readonly Color ButtonHighlight = new Color(0.28f, 0.46f, 0.70f);
        public static readonly Color ButtonPressed = new Color(0.15f, 0.26f, 0.43f);
        public static readonly Color ButtonDisabled = new Color(0.22f, 0.24f, 0.30f, 0.6f);

        // Feedback
        public static readonly Color Health = new Color(0.90f, 0.35f, 0.40f);
        public static readonly Color Collectible = new Color(1f, 0.85f, 0.30f);

        // Tamanhos de fonte
        public const int FontTitle = 46;
        public const int FontHeading = 30;
        public const int FontBody = 22;
        public const int FontButton = 24;
        public const int FontHud = 22;
    }
}
