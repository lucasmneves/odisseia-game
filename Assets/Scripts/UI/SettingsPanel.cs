using UnityEngine;
using UnityEngine.UI;
using Odisseia.Core;

namespace Odisseia.UI
{
    /// <summary>
    /// Painel de configurações básicas (volume). Persistido via CampaignManager/SaveSystem
    /// quando disponível; caso contrário só ajusta o volume da sessão atual.
    /// </summary>
    public class SettingsPanel : MonoBehaviour
    {
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private Button closeButton;

        private void Awake()
        {
            closeButton?.onClick.AddListener(Close);

            if (volumeSlider != null)
            {
                float current = CampaignManager.Instance != null
                    ? CampaignManager.Instance.MasterVolume
                    : AudioListener.volume;

                volumeSlider.value = current;
                volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            }
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
