using UnityEngine;

namespace Odisseia.Core
{
    /// <summary>
    /// Dados de catálogo de uma fase da campanha (identidade, nome de exibição,
    /// cena e ordem). Não contém nenhuma lógica de gameplay.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelDefinition", menuName = "Odisseia/Level Definition")]
    public class LevelDefinition : ScriptableObject
    {
        [SerializeField] private string levelId;
        [SerializeField] private string displayName;
        [SerializeField] private string sceneName;
        [SerializeField] private int order;

        public string LevelId => levelId;

        /// <summary>
        /// Nome mostrado ao jogador, no idioma atual. A chave é derivada do id
        /// ("level.Level_02_Troia"), então não há um segundo campo para manter em dia:
        /// renomear a fase é mexer num lugar só, a tabela de idiomas.
        ///
        /// Sem entrada na tabela vale o campo do asset, para uma fase nova continuar
        /// aparecendo com nome enquanto a tradução não chega.
        /// </summary>
        public string DisplayName
        {
            get
            {
                string chave = "level." + levelId;
                return Odisseia.Systems.Localization.Has(chave)
                    ? Odisseia.Systems.Localization.Get(chave)
                    : displayName;
            }
        }
        public string SceneName => sceneName;
        public int Order => order;
    }
}
