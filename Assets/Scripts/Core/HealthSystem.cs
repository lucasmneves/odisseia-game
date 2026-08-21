using System;
using UnityEngine;

namespace Odisseia.Core
{
    /// <summary>
    /// Componente de vida reutilizável (Player, inimigos, objetos destrutíveis).
    /// </summary>
    public class HealthSystem : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 100;

        public int MaxHealth => maxHealth;
        public int CurrentHealth { get; private set; }
        public bool IsDead { get; private set; }

        public event Action<int, int> Damaged; // (quantidade, vidaAtual)
        public event Action Died;

        private IDamageMitigator mitigator;

        private void Awake()
        {
            CurrentHealth = maxHealth;
            // Null para quase todo mundo (inimigos, bosses). Só o Odisseu tem escudo.
            mitigator = GetComponent<IDamageMitigator>();
        }

        /// <summary>Dano sem origem conhecida (queda, poço, afogamento) — não bloqueável.</summary>
        public void TakeDamage(int amount)
        {
            TakeDamage(amount, DamageInfo.Environmental);
        }

        /// <summary>
        /// Dano com contexto: quem tiver um <see cref="IDamageMitigator"/> (o escudo)
        /// pode reduzi-lo antes de ele ser aplicado.
        /// </summary>
        public void TakeDamage(int amount, DamageInfo info)
        {
            if (IsDead || amount <= 0)
            {
                return;
            }

            if (mitigator != null)
            {
                amount = mitigator.Mitigate(amount, info);

                // Bloqueio total: nada de evento de dano, mas o golpe não "some" —
                // quem bloqueou já emitiu o próprio feedback.
                if (amount <= 0)
                {
                    return;
                }
            }

            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            Damaged?.Invoke(amount, CurrentHealth);

            if (CurrentHealth == 0)
            {
                IsDead = true;
                Died?.Invoke();
            }
        }

        public void ResetHealth()
        {
            IsDead = false;
            CurrentHealth = maxHealth;
        }
    }
}
