using UnityEngine;

namespace Odisseia.UI
{
    /// <summary>
    /// Aviso de "gire o aparelho" quando o mobile está em portrait. WebGL não tem uma
    /// API de orientation lock confiável entre navegadores (Safari iOS não suporta
    /// screen.orientation.lock) — em vez de forçar landscape, avisa e esconde os
    /// controles até o jogador girar, sem travar o jogo.
    ///
    /// Campos públicos porque este componente é sempre montado por código
    /// (MobileControlsRoot), nunca configurado à mão no Inspector.
    /// </summary>
    public class RotateDevicePrompt : MonoBehaviour
    {
        public GameObject panel;
        public CanvasGroup controlsToHide;

        private bool lastWasPortrait;
        private bool initialized;

        private void OnEnable()
        {
            initialized = false;
        }

        private void Update()
        {
            bool isPortrait = Screen.height > Screen.width;
            if (!initialized || isPortrait != lastWasPortrait)
            {
                initialized = true;
                lastWasPortrait = isPortrait;
                Refresh(isPortrait);
            }
        }

        private void Refresh(bool isPortrait)
        {
            if (panel != null)
            {
                panel.SetActive(isPortrait);
            }

            if (controlsToHide != null)
            {
                controlsToHide.alpha = isPortrait ? 0f : 1f;
                controlsToHide.interactable = !isPortrait;
                controlsToHide.blocksRaycasts = !isPortrait;
            }
        }
    }
}
