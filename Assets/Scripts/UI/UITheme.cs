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

        // Borda dourada dos botões que têm Outline (menu principal): o dourado do logo e dos louros.
        public static readonly Color ButtonBorder = new Color(0.78f, 0.60f, 0.30f, 0.9f);
        public static readonly Color ButtonBorderDisabled = new Color(0.45f, 0.45f, 0.50f, 0.35f);

        // Feedback
        public static readonly Color Health = new Color(0.90f, 0.35f, 0.40f);
        public static readonly Color Collectible = new Color(1f, 0.85f, 0.30f);

        private static Font font;

        /// <summary>
        /// Fonte de toda a UI: DejaVu Sans (<c>Resources/Fonts</c>, licença ao lado).
        ///
        /// A fonte embutida do Unity (LegacyRuntime) não tem travessão, ✕ □ △ ○, ◄ ► nem ✓: no
        /// Editor o Windows supre os glifos, no WebGL eles simplesmente somem ("Ítaca  antes da
        /// guerra", "[Y/] Jogar"). Sem o arquivo, cai na embutida para a UI nunca ficar sem texto.
        /// </summary>
        public static Font Font
        {
            get
            {
                if (font == null)
                {
                    font = Resources.Load<Font>("Fonts/DejaVuSans");
                    if (font == null)
                    {
                        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    }
                }

                return font;
            }
        }

        // Tamanhos de fonte
        public const int FontTitle = 46;
        public const int FontHeading = 30;
        public const int FontBody = 22;
        public const int FontButton = 24;
        public const int FontHud = 22;

        /// <summary>
        /// Ordem de empilhamento dos Canvas, do fundo para a frente.
        ///
        /// Cada tela que se monta em runtime escolhia o próprio número solto no meio do
        /// arquivo, e ninguém conseguia ver a pilha inteira — foi assim que a tela de
        /// remapeamento acabou ABAIXO da de configurações, que é justamente quem a abre.
        /// Com a lista aqui, dá para conferir a ordem num olhar antes de encaixar mais
        /// uma tela.
        ///
        /// A regra é simples: quem abre fica embaixo de quem foi aberto.
        /// </summary>
        public static class Layer
        {
            /// <summary>Controles de toque: atrás do HUD, para nunca cobrir informação.</summary>
            public const int MobileControls = -1;

            /// <summary>HUD, pause e diálogo vivem no 0/1 das próprias cenas.</summary>
            public const int SceneUI = 0;

            public const int GameOver = 600;

            /// <summary>Configurações, abertas pelo menu ou pelo pause.</summary>
            public const int Settings = 700;

            /// <summary>Remapeamento de teclas — aberto de dentro das configurações.</summary>
            public const int KeyRebinding = 750;

            /// <summary>Confirmação: pode ser pedida de qualquer uma das telas acima.</summary>
            public const int Confirm = 800;

            /// <summary>Transições de cena, sempre por cima de tudo.</summary>
            public const int Loading = 998;

            public const int Fade = 999;
        }
    }
}
