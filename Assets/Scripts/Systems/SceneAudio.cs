using UnityEngine;
using Odisseia.Core;

namespace Odisseia.Systems
{
    /// <summary>
    /// Define qual música toca nesta cena. Um componente por cena — é assim que a
    /// "música por fase" fica organizada sem espalhar lógica de áudio pelos scripts
    /// de gameplay.
    /// </summary>
    public class SceneAudio : MonoBehaviour
    {
        public enum Track
        {
            None,
            Menu,
            LevelCalm,
            LevelTense,
            Victory,
        }

        [SerializeField] private Track track = Track.LevelCalm;

        private void Start()
        {
            AudioLibrary library = GameAssets.Instance != null ? GameAssets.Instance.Audio : null;
            if (library == null)
            {
                return;
            }

            AudioClip clip = track switch
            {
                Track.Menu => library.MenuMusic,
                Track.LevelCalm => library.LevelMusicCalm,
                Track.LevelTense => library.LevelMusicTense,
                Track.Victory => library.VictoryMusic,
                _ => null,
            };

            if (clip != null)
            {
                AudioManager.Instance.PlayMusic(clip);
            }
            else
            {
                AudioManager.Instance.StopMusic();
            }
        }
    }
}
