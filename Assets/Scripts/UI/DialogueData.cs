using UnityEngine;

namespace Odisseia.UI
{
    /// <summary>
    /// Dados de um diálogo (sequência de falas) como ScriptableObject, para permitir
    /// reaproveitar o mesmo conteúdo em vários lugares sem duplicar dados na cena.
    /// </summary>
    [CreateAssetMenu(fileName = "DialogueData", menuName = "Odisseia/Dialogue Data")]
    public class DialogueData : ScriptableObject
    {
        [SerializeField] private DialogueLine[] lines;

        public DialogueLine[] Lines => lines;
    }
}
