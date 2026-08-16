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

        public void PlaySfx(AudioClip clip, float volumeScale = 1f)
        {
            if (clip == null)
            {
                return;
            }

            EnsureSources();
            sfxSource.PlayOneShot(clip, Mathf.Clamp01(volumeScale));
        }

        // ---- Atalhos por evento de jogo (evitam espalhar GameAssets.Instance pelo código) ----

        private static AudioLibrary Library => GameAssets.Instance != null ? GameAssets.Instance.Audio : null;

        public static void Sfx(AudioClip clip, float volumeScale = 1f)
        {
            if (clip != null)
            {
                Instance.PlaySfx(clip, volumeScale);
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
    }
}
