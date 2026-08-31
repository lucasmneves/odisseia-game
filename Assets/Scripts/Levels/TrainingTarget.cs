using UnityEngine;
using Odisseia.Core;
using Odisseia.Systems;

namespace Odisseia.Levels
{
    /// <summary>
    /// Boneco de treino ou alvo de arco: leva dano como qualquer outra coisa do jogo
    /// (mesmo <see cref="HealthSystem"/>, mesma layer de alvo da espada e da flecha) e
    /// avisa o <see cref="TrainingCourse"/> de que o golpe chegou.
    ///
    /// A ação relatada é escolhida no Inspector, e não deduzida do dano recebido: um
    /// boneco de espada e um alvo de arco são objetos diferentes no cenário, então
    /// dizer qual é qual é dado, não adivinhação.
    ///
    /// A vida é alta de propósito. Um alvo que morre no primeiro golpe tiraria do
    /// jogador a chance de repetir o movimento — e a etapa ficaria impossível.
    /// </summary>
    [RequireComponent(typeof(HealthSystem))]
    public class TrainingTarget : MonoBehaviour
    {
        [SerializeField] private TrainingCourse course;
        [SerializeField] private TrainingAction reportAction = TrainingAction.SwordHit;

        [Header("Feedback")]
        [SerializeField] private SpriteRenderer body;
        [SerializeField] private Color hitColor = new Color(0.95f, 0.8f, 0.35f);

        private HealthSystem health;

        private void Awake()
        {
            health = GetComponent<HealthSystem>();
        }

        private void OnEnable()
        {
            health.Damaged += OnDamaged;
        }

        private void OnDisable()
        {
            health.Damaged -= OnDamaged;
        }

        private void OnDamaged(int amount, int currentHealth)
        {
            if (body != null)
            {
                body.color = hitColor;
            }

            Sprite sprite = GameAssets.Instance != null ? GameAssets.Instance.PlaceholderSprite : null;
            VfxBurst.Spawn(sprite, transform.position, hitColor, 5, 2.2f, 0.25f);

            course?.Report(reportAction);
        }
    }
}
