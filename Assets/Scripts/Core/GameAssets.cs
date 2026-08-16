using UnityEngine;

namespace Odisseia.Core
{
    /// <summary>
    /// Catálogo central de assets compartilhados (sprite placeholder + biblioteca de
    /// áudio). Carregado uma única vez de Resources na primeira utilização, para que
    /// scripts de feedback (VFX, som) não precisem de referência serializada em cada
    /// objeto de cada uma das 21 cenas.
    /// </summary>
    [CreateAssetMenu(fileName = "GameAssets", menuName = "Odisseia/Game Assets")]
    public class GameAssets : ScriptableObject
    {
        private const string ResourcePath = "GameAssets";

        private static GameAssets instance;
        private static bool loadAttempted;

        [Header("Sprites")]
        [SerializeField] private Sprite placeholderSprite;

        [Header("Áudio")]
        [SerializeField] private AudioLibrary audioLibrary;

        public Sprite PlaceholderSprite => placeholderSprite;
        public AudioLibrary Audio => audioLibrary;

        /// <summary>Pode retornar null se o asset não existir — todos os usos tratam isso.</summary>
        public static GameAssets Instance
        {
            get
            {
                if (instance == null && !loadAttempted)
                {
                    loadAttempted = true;
                    instance = Resources.Load<GameAssets>(ResourcePath);
                }

                return instance;
            }
        }
    }
}
