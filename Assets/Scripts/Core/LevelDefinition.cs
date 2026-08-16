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
        public string DisplayName => displayName;
        public string SceneName => sceneName;
        public int Order => order;
    }
}
