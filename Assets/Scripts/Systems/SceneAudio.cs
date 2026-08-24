using UnityEngine;
using Odisseia.Core;

namespace Odisseia.Systems
{
    /// <summary>
    /// Define qual música toca nesta cena. Um componente por cena — é assim que a
    /// "música por fase" fica organizada sem espalhar lógica de áudio pelos scripts
    /// de gameplay.
    ///
    /// Hoje o jogo usa um TEMA ÚNICO: se a <see cref="AudioLibrary.MainTheme"/> estiver
    /// preenchida, ela toca em todas as telas e o <see cref="track"/> desta cena é
    /// ignorado. Como o <see cref="AudioManager.PlayMusic"/> não reinicia uma faixa que
    /// já está tocando, a música atravessa as trocas de cena sem cortar — do menu ao
    /// final, em loop.
    ///
    /// O campo <see cref="track"/> continua configurado cena a cena de propósito: no dia
    /// em que cada tela tiver a própria música, basta esvaziar o tema único na library.
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

            AudioClip clip = library.MainTheme != null
                ? library.MainTheme
                : track switch
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
