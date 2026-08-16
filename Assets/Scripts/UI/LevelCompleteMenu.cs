using UnityEngine;
using UnityEngine.UI;
using Odisseia.Systems;

namespace Odisseia.UI
{
    /// <summary>
    /// Tela de "FASE CONCLUÍDA": botão Continuar leva à próxima cena configurada.
    /// </summary>
    public class LevelCompleteMenu : MonoBehaviour
    {
        [SerializeField] private Button continueButton;
        [SerializeField] private string nextSceneName = SceneLoader.MainMenu;

        private void Awake()
        {
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(OnContinueClicked);
            }
        }

        private void OnContinueClicked()
        {
            SceneLoader.Load(nextSceneName);
        }
    }
}
