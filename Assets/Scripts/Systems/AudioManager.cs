using UnityEngine;
using Odisseia.Core;

namespace Odisseia.Systems
{
    /// <summary>
    /// Tocador central de música e efeitos. Cria-se sozinho na primeira utilização
    /// (para que qualquer cena funcione isoladamente no Editor, sem passar pelo Boot)
    /// e persiste entre cenas. Dois AudioSources: um para música em loop, um para
    /// efeitos one-shot.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager instance;

        private AudioSource musicSource;
        private AudioSource sfxSource;
        private AudioClip currentMusic;

        public static AudioManager Instance
        {
            get
            {
                if (instance == null)
                {
                    var go = new GameObject("AudioManager");
                    instance = go.AddComponent<AudioManager>();
                    DontDestroyOnLoad(go);
                }

                return instance;
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureSources();
        }

        private void EnsureSources()
        {
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
                musicSource.volume = 0.35f;
            }

            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
                sfxSource.volume = 0.6f;
            }
        }

        /// <summary>
        /// Volumes separados de música e efeitos, vindos do <see cref="SettingsManager"/>.
        /// São multiplicadores por canal; o volume geral continua no AudioListener, então
        /// os dois se combinam sem um anular o outro.
        /// </summary>
        public void SetMixLevels(float musicVolume, float sfxVolume)
        {
            EnsureSources();
            musicSource.volume = Mathf.Clamp01(musicVolume) * 0.35f;
            sfxSource.volume = Mathf.Clamp01(sfxVolume) * 0.6f;
        }

        /// <summary>Troca a música em loop. Não reinicia se já for a mesma faixa.</summary>
        public void PlayMusic(AudioClip clip)
        {
            EnsureSources();

            if (clip == null || currentMusic == clip)
            {
                return;
            }

            currentMusic = clip;
            musicSource.clip = clip;
            musicSource.Play();
        }

        public void StopMusic()
        {
            EnsureSources();
            currentMusic = null;
            musicSource.Stop();
        }

        public void PlaySfx(AudioClip clip, float volumeScale = 1f, float pitch = 1f)
        {
            if (clip == null)
            {
                return;
            }

            EnsureSources();
            sfxSource.pitch = Mathf.Clamp(pitch, 0.4f, 2.5f);
            sfxSource.PlayOneShot(clip, Mathf.Clamp01(volumeScale));
        }

        // ---- Atalhos por evento de jogo (evitam espalhar GameAssets.Instance pelo código) ----

        private static AudioLibrary Library => GameAssets.Instance != null ? GameAssets.Instance.Audio : null;

        public static void Sfx(AudioClip clip, float volumeScale = 1f, float pitch = 1f)
        {
            if (clip != null)
            {
                Instance.PlaySfx(clip, volumeScale, pitch);
            }
        }

        public static void PlayAttack() => Sfx(Library?.SfxAttack, 0.5f);
        public static void PlayHit() => Sfx(Library?.SfxHit, 0.7f);
        public static void PlayPlayerDamage() => Sfx(Library?.SfxPlayerDamage, 0.8f);
        public static void PlayDeath() => Sfx(Library?.SfxDeath, 0.8f);
        public static void PlayCollect() => Sfx(Library?.SfxCollect, 0.6f);
        public static void PlayCheckpoint() => Sfx(Library?.SfxCheckpoint, 0.7f);
        public static void PlayJump() => Sfx(Library?.SfxJump, 0.35f);
        public static void PlayUiClick() => Sfx(Library?.SfxUiClick, 0.5f);

        // ---- Eventos de UI de menu ----
        //
        // Cada evento tem um slot próprio na AudioLibrary, hoje vazio. Enquanto os
        // clipes definitivos não chegam, todos caem no clique já existente com volume
        // e pitch diferentes: o menu responde de forma distinguível sem precisar
        // entrar áudio provisório ruim no projeto. Preencher o slot na library
        // substitui o improviso, sem tocar em código.

        /// <summary>Cursor passou por cima de um item — discreto, quase um tique.</summary>
        public static void PlayUiHover()
        {
            AudioLibrary library = Library;
            if (library?.SfxUiHover != null)
            {
                Sfx(library.SfxUiHover, 0.35f);
                return;
            }

            Sfx(library?.SfxUiClick, 0.18f, 1.5f);
        }

        /// <summary>Escolha confirmada.</summary>
        public static void PlayUiConfirm()
        {
            AudioLibrary library = Library;
            if (library?.SfxUiConfirm != null)
            {
                Sfx(library.SfxUiConfirm, 0.6f);
                return;
            }

            Sfx(library?.SfxUiClick, 0.55f, 1.15f);
        }

        /// <summary>Voltou atrás ou cancelou.</summary>
        public static void PlayUiCancel()
        {
            AudioLibrary library = Library;
            if (library?.SfxUiCancel != null)
            {
                Sfx(library.SfxUiCancel, 0.5f);
                return;
            }

            Sfx(library?.SfxUiClick, 0.45f, 0.72f);
        }
    }
}
