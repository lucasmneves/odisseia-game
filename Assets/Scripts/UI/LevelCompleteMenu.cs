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

        /// <summary>
        /// Foco no Continuar: sem isto o A/✕ do controle não fazia nada aqui. No Start, não
        /// no Awake: o navegador garante um EventSystem, e no Awake o da cena pode ainda não
        /// estar ligado — nasceria um segundo, permanente.
        /// </summary>
        private void Start()
        {
            if (continueButton != null)
            {
                gameObject.AddComponent<MenuNavigator>().SetItems(new[] { continueButton });
            }
        }

        private void OnContinueClicked()
        {
            SceneLoader.Load(nextSceneName);
        }
    }
}
