using UnityEngine;

namespace Odisseia.Core
{
    /// <summary>
    /// Biblioteca de áudio organizada por função (música por contexto + efeitos por
    /// evento de jogo). Todos os clipes são placeholders gerados proceduralmente por
    /// script de Editor — nenhum asset de terceiros, nenhuma questão de licença.
    /// </summary>
    [CreateAssetMenu(fileName = "AudioLibrary", menuName = "Odisseia/Audio Library")]
    public class AudioLibrary : ScriptableObject
    {
        [Header("Música")]
        [SerializeField] private AudioClip menuMusic;
        [SerializeField] private AudioClip levelMusicCalm;
        [SerializeField] private AudioClip levelMusicTense;
        [SerializeField] private AudioClip victoryMusic;

        [Header("Efeitos — combate")]
        [SerializeField] private AudioClip sfxAttack;
        [SerializeField] private AudioClip sfxHit;
        [SerializeField] private AudioClip sfxPlayerDamage;
        [SerializeField] private AudioClip sfxDeath;

        [Header("Efeitos — progressão")]
        [SerializeField] private AudioClip sfxCollect;
        [SerializeField] private AudioClip sfxCheckpoint;
        [SerializeField] private AudioClip sfxJump;

        [Header("Efeitos — UI")]
        [SerializeField] private AudioClip sfxUiClick;

        public AudioClip MenuMusic => menuMusic;
        public AudioClip LevelMusicCalm => levelMusicCalm;
        public AudioClip LevelMusicTense => levelMusicTense;
        public AudioClip VictoryMusic => victoryMusic;

        public AudioClip SfxAttack => sfxAttack;
        public AudioClip SfxHit => sfxHit;
        public AudioClip SfxPlayerDamage => sfxPlayerDamage;
        public AudioClip SfxDeath => sfxDeath;

        public AudioClip SfxCollect => sfxCollect;
        public AudioClip SfxCheckpoint => sfxCheckpoint;
        public AudioClip SfxJump => sfxJump;

        public AudioClip SfxUiClick => sfxUiClick;
    }
}
