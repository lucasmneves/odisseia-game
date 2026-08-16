using System.Collections;
using UnityEngine;
using Odisseia.Player;
using Odisseia.UI;

namespace Odisseia.Levels
{
    /// <summary>
    /// Representação simples da estratégia de Odisseu contra as sereias: tocar o
    /// mastro concede imunidade temporária ao canto (SirenResistance) e avisa o
    /// jogador reaproveitando o TutorialPrompt já existente.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class MastAnchor : MonoBehaviour
    {
        [SerializeField] private float immunityDuration = 8f;
        [SerializeField] private TutorialPrompt prompt;
        [SerializeField] private string message = "🔗 Amarrado ao mastro — resistente ao canto das sereias.";

        private bool triggered;

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (triggered || !other.TryGetComponent(out SirenResistance resistance))
            {
                return;
            }

            triggered = true;
            prompt?.Show(message, immunityDuration);
            StartCoroutine(GrantImmunity(resistance));
        }

        private IEnumerator GrantImmunity(SirenResistance resistance)
        {
            resistance.SetImmune(true);
            yield return new WaitForSeconds(immunityDuration);
            resistance.SetImmune(false);
        }
    }
}
