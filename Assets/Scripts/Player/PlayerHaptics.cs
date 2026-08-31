using UnityEngine;
using Odisseia.Core;
using Odisseia.Systems;

namespace Odisseia.Player
{
    /// <summary>
    /// Traduz o que já acontece com Odisseu em vibração no controle.
    ///
    /// É um ouvinte, não um participante: assina os eventos que o combate já dispara
    /// (<see cref="HealthSystem.Damaged"/>, <see cref="PlayerShield.Blocked"/>) e não
    /// devolve nada. Nenhuma linha de espada, escudo, arco ou dano precisou mudar para
    /// isto existir — e remover este componente devolve o jogo ao estado anterior.
    ///
    /// O ataque de propósito NÃO vibra: sai a cada 0,4 s e o controle viraria um
    /// chocalho. Vibra o que é reação — levar o golpe e aparar o golpe.
    /// </summary>
    public class PlayerHaptics : MonoBehaviour
    {
        private HealthSystem health;
        private PlayerShield shield;

        private void Awake()
        {
            health = GetComponent<HealthSystem>();
            shield = GetComponent<PlayerShield>();
        }

        private void OnEnable()
        {
            if (health != null)
            {
                health.Damaged += OnDamaged;
            }

            if (shield != null)
            {
                shield.Blocked += OnBlocked;
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.Damaged -= OnDamaged;
            }

            if (shield != null)
            {
                shield.Blocked -= OnBlocked;
            }
        }

        private void OnDamaged(int amount, int currentHealth)
        {
            HapticFeedback.Medium();
        }

        private void OnBlocked(int absorbed)
        {
            HapticFeedback.Light();
        }
    }
}
