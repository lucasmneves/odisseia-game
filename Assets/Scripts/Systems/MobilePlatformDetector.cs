using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Odisseia.Systems
{
    /// <summary>
    /// Decide, uma única vez por sessão, se o jogo está rodando num dispositivo mobile.
    /// Não usa tamanho de tela como critério — só sinais de plataforma/dispositivo. Uma
    /// janela de desktop pequena não deve virar "mobile", e um celular em paisagem
    /// (que muda a proporção da tela) não pode virar "desktop" por causa disso.
    /// </summary>
    public static class MobilePlatformDetector
    {
        private static bool? cached;

        public static bool IsMobile => cached ??= Evaluate();

        /// <summary>
        /// Só para QA/depuração: força um resultado sem precisar de um dispositivo real.
        /// Passe null para voltar à detecção automática.
        /// </summary>
        public static void DebugOverride(bool? value) => cached = value;

        private static bool Evaluate()
        {
            bool result = EvaluateInternal(out string source);
            Debug.Log($"[MobilePlatformDetector] IsMobile={result} (sinal decisivo: {source})");
            return result;
        }

        private static bool EvaluateInternal(out string source)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            // Application.isMobilePlatform é sempre false em WebGL — é o Unity Player
            // rodando em WebAssembly dentro de QUALQUER navegador, não o Android/iOS
            // nativo. Quem sabe se o navegador é mobile é o próprio navegador.
            if (Odisseia_IsMobileBrowser() == 1)
            {
                source = "navegador WebGL (user agent + touch)";
                return true;
            }
#endif
            // Cobre um eventual build nativo (Android/iOS) do mesmo projeto no futuro.
            if (Application.isMobilePlatform)
            {
                source = "Application.isMobilePlatform";
                return true;
            }

            // Fallback para os demais contextos (ex.: Editor com Device Simulator, ou
            // uma build de desktop com tela touch): touch presente e mouse ausente é o
            // padrão de um dispositivo touch-only.
            bool hasTouch = Touchscreen.current != null;
            bool hasMouse = Mouse.current != null;
            source = hasTouch && !hasMouse ? "touchscreen sem mouse" : "nenhum sinal mobile";
            return hasTouch && !hasMouse;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern int Odisseia_IsMobileBrowser();
#endif
    }
}
