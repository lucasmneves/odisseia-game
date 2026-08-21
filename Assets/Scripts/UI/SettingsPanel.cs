using UnityEngine;
using UnityEngine.UI;
using Odisseia.Core;

namespace Odisseia.UI
{
    /// <summary>
    /// Painel de configurações básicas (volume) + acesso ao remapeamento de teclas.
    /// Persistido via CampaignManager/SaveSystem quando disponível; caso contrário só
    /// ajusta o volume da sessão atual.
    /// </summary>
    public class SettingsPanel : MonoBehaviour
    {
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private Button closeButton;

        private void Awake()
        {
            closeButton?.onClick.AddListener(Close);
            CreateControlsButton();

            if (volumeSlider != null)
            {
                float current = CampaignManager.Instance != null
                    ? CampaignManager.Instance.MasterVolume
                    : AudioListener.volume;

                volumeSlider.value = current;
                volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            }
        }

        /// <summary>
        /// Cria o botão "Controles" clonando o botão de fechar que a cena já traz —
        /// assim herda estilo, fonte e tamanho sem duplicar layout nem editar a cena.
        /// </summary>
        private void CreateControlsButton()
        {
            if (closeButton == null)
            {
                return;
            }

            Button controls = Instantiate(closeButton, closeButton.transform.parent);
            controls.name = "ControlsButton";

            var source = (RectTransform)closeButton.transform;
            var rect = (RectTransform)controls.transform;
            rect.anchoredPosition = source.anchoredPosition + new Vector2(0f, source.sizeDelta.y + 12f);

            var label = controls.GetComponentInChildren<Text>();
            if (label != null)
            {
                label.text = "Controles";
            }

            controls.onClick.RemoveAllListeners();
            controls.onClick.AddListener(OptionsMenu.Open);
        }

        private void Close()
        {
            gameObject.SetActive(false);
        }

        private void OnVolumeChanged(float value)
        {
            AudioListener.volume = value;

            if (CampaignManager.Instance != null)
            {
                CampaignManager.Instance.MasterVolume = value;
            }
        }
    }
}
