using UnityEngine;
using UnityEngine.UI;
using Odisseia.Systems;

namespace Odisseia.UI
{
    /// <summary>
    /// Liga um <see cref="Text"/> de cena a uma chave da tabela de idiomas.
    ///
    /// Só para texto que já vem escrito na cena — o que é montado em runtime pergunta
    /// direto ao <see cref="Localization"/>. Reaplica sozinho quando o idioma muda,
    /// então trocar de idioma não exige recarregar a cena.
    ///
    /// Sem chave, ou com chave que não está na tabela, deixa o texto da cena como
    /// está: um rótulo novo continua legível enquanto a tradução não chega.
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class LocalizedText : MonoBehaviour
    {
        [SerializeField] private string key;

        private Text target;

        private void Awake()
        {
            target = GetComponent<Text>();
        }

        private void OnEnable()
        {
            Apply();
            Localization.Changed += Apply;
        }

        private void OnDisable()
        {
            Localization.Changed -= Apply;
        }

        private void Apply()
        {
            if (target == null || string.IsNullOrEmpty(key) || !Localization.Has(key))
            {
                return;
            }

            target.text = Localization.Get(key);
        }

        /// <summary>Permite definir a chave ao montar a UI por código.</summary>
        public void SetKey(string newKey)
        {
            key = newKey;
            Apply();
        }
    }
}
