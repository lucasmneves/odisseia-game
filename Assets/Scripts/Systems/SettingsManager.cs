using UnityEngine;
using Odisseia.Core;

namespace Odisseia.Systems
{
    /// <summary>
    /// Configurações do jogador (áudio e gráficos) e sua persistência.
    ///
    /// É um serviço estático sem MonoBehaviour: qualquer tela lê e escreve por aqui, e
    /// ninguém mais toca em PlayerPrefs para configuração. Se um dia a persistência
    /// mudar de lugar, muda só este arquivo.
    ///
    /// O volume geral é a exceção: ele já morava no save da campanha antes desta tela
    /// existir (<see cref="CampaignManager.MasterVolume"/>), então continua lá e este
    /// serviço só delega — dois lugares gravando o mesmo valor acabariam discordando.
    /// Música e efeitos, que são novos, ficam em chaves próprias.
    /// </summary>
    public static class SettingsManager
    {
        private const string KeyMusic = "Odisseia.Settings.MusicVolume";
        private const string KeySfx = "Odisseia.Settings.SfxVolume";
        private const string KeyQuality = "Odisseia.Settings.Quality";
        private const string KeyFullscreen = "Odisseia.Settings.Fullscreen";
        private const string KeyResolution = "Odisseia.Settings.Resolution";

        private static bool loaded;

        private static float music = 0.8f;
        private static float sfx = 0.9f;
        private static int quality = -1;
        private static bool fullscreen;
        private static int resolutionIndex = -1;

        /// <summary>Volume geral (0..1). Multiplica tudo o que sai do jogo.</summary>
        public static float MasterVolume
        {
            get
            {
                EnsureLoaded();
                return CampaignManager.Instance != null
                    ? CampaignManager.Instance.MasterVolume
                    : AudioListener.volume;
            }
            set
            {
                EnsureLoaded();
                value = Mathf.Clamp01(value);

                AudioListener.volume = value;

                if (CampaignManager.Instance != null)
                {
                    CampaignManager.Instance.MasterVolume = value;
                }
            }
        }

        public static float MusicVolume
        {
            get { EnsureLoaded(); return music; }
            set
            {
                EnsureLoaded();
                music = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat(KeyMusic, music);
                PlayerPrefs.Save();
                Apply();
            }
        }

        public static float SfxVolume
        {
            get { EnsureLoaded(); return sfx; }
            set
            {
                EnsureLoaded();
                sfx = Mathf.Clamp01(value);
                PlayerPrefs.SetFloat(KeySfx, sfx);
                PlayerPrefs.Save();
                Apply();
            }
        }

        /// <summary>Índice em <see cref="QualitySettings.names"/>.</summary>
        public static int QualityLevel
        {
            get { EnsureLoaded(); return quality; }
            set
            {
                EnsureLoaded();
                quality = Mathf.Clamp(value, 0, Mathf.Max(0, QualitySettings.names.Length - 1));
                PlayerPrefs.SetInt(KeyQuality, quality);
                PlayerPrefs.Save();
                Apply();
            }
        }

        public static bool Fullscreen
        {
            get { EnsureLoaded(); return fullscreen; }
            set
            {
                EnsureLoaded();
                fullscreen = value;
                PlayerPrefs.SetInt(KeyFullscreen, fullscreen ? 1 : 0);
                PlayerPrefs.Save();
                Apply();
            }
        }

        public static int ResolutionIndex
        {
            get { EnsureLoaded(); return resolutionIndex; }
            set
            {
                EnsureLoaded();
                Vector2Int[] opcoes = AvailableResolutions;
                resolutionIndex = Mathf.Clamp(value, 0, Mathf.Max(0, opcoes.Length - 1));
                PlayerPrefs.SetInt(KeyResolution, resolutionIndex);
                PlayerPrefs.Save();
                Apply();
            }
        }

        /// <summary>
        /// Resoluções oferecidas fora do navegador. Lista fixa em passos 16:9 porque
        /// <c>Screen.resolutions</c> não é confiável em todos os alvos.
        /// </summary>
        public static Vector2Int[] AvailableResolutions { get; } =
        {
            new Vector2Int(1280, 720),
            new Vector2Int(1600, 900),
            new Vector2Int(1920, 1080),
        };

        /// <summary>
        /// Em WebGL o tamanho do canvas pertence à página, não ao jogo: o template
        /// define <c>canvas.style</c> e a Unity mantém o alvo de render casado com o
        /// tamanho DOM. Chamar <c>Screen.SetResolution</c> ali muda os atributos do
        /// canvas por baixo do CSS e o jogo sai do enquadramento — faixas pretas e
        /// conteúdo cortado.
        ///
        /// Quem pergunta é a tela de configurações, para não oferecer um controle
        /// que não faria nada.
        /// </summary>
        public static bool SupportsResolutionChange =>
#if UNITY_WEBGL && !UNITY_EDITOR
            false;
#else
            true;
#endif

        public static string[] QualityNames => QualitySettings.names;

        private static void EnsureLoaded()
        {
            if (loaded)
            {
                return;
            }

            loaded = true;

            music = PlayerPrefs.GetFloat(KeyMusic, 0.8f);
            sfx = PlayerPrefs.GetFloat(KeySfx, 0.9f);
            quality = PlayerPrefs.GetInt(KeyQuality, QualitySettings.GetQualityLevel());
            fullscreen = PlayerPrefs.GetInt(KeyFullscreen, Screen.fullScreen ? 1 : 0) == 1;
            resolutionIndex = PlayerPrefs.GetInt(KeyResolution, DefaultResolutionIndex());

            Apply();
        }

        private static int DefaultResolutionIndex()
        {
            Vector2Int[] opcoes = AvailableResolutions;
            for (int i = 0; i < opcoes.Length; i++)
            {
                if (opcoes[i].x == Screen.width && opcoes[i].y == Screen.height)
                {
                    return i;
                }
            }

            return 0;
        }

        /// <summary>Reaplica tudo. Chamado no boot e a cada alteração.</summary>
        public static void Apply()
        {
            EnsureLoaded();

            AudioManager.Instance.SetMixLevels(music, sfx);

            if (quality >= 0 && quality < QualitySettings.names.Length)
            {
                QualitySettings.SetQualityLevel(quality, applyExpensiveChanges: false);
            }

            ApplyDisplay();
        }

        private static void ApplyDisplay()
        {
            if (!SupportsResolutionChange)
            {
                // No navegador o tamanho é da página. Só a tela cheia é do jogo, e
                // mesmo essa só troca quando muda — escrever a cada Apply dispararia
                // pedidos de fullscreen sem gesto do usuário, que o navegador recusa.
                if (Screen.fullScreen != fullscreen)
                {
                    Screen.fullScreen = fullscreen;
                }

                return;
            }

            Vector2Int alvo = AvailableResolutions[Mathf.Clamp(resolutionIndex, 0, AvailableResolutions.Length - 1)];
            if (Screen.width != alvo.x || Screen.height != alvo.y || Screen.fullScreen != fullscreen)
            {
                Screen.SetResolution(alvo.x, alvo.y, fullscreen);
            }
        }

        /// <summary>Chamado no arranque, antes de qualquer tela aparecer.</summary>
        public static void Initialize() => EnsureLoaded();
    }
}
