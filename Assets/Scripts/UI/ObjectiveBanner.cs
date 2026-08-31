using UnityEngine;
using UnityEngine.UI;

namespace Odisseia.UI
{
    /// <summary>
    /// Faixa fixa no alto da tela com o objetivo atual ("Reúna os homens de Ítaca 3/10").
    ///
    /// Diferente do <see cref="TutorialPrompt"/>, que é um aviso passageiro: aqui o
    /// texto fica enquanto o objetivo existir, porque é a resposta para "e agora, o que
    /// eu faço?" numa fase longa. Um só componente por cena; quem escreve nele é o
    /// <see cref="Odisseia.Levels.LevelObjective"/>.
    /// </summary>
    public class ObjectiveBanner : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Text label;

        public void Show(string text)
        {
            if (label != null)
            {
                label.text = text;
            }

            if (panel != null)
            {
                panel.SetActive(!string.IsNullOrEmpty(text));
            }
        }

        public void Hide()
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }
    }
}
