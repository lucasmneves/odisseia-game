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

        private void Awake()
        {
            CurrentHealth = maxHealth;
        }

        public void TakeDamage(int amount)
        {
            if (IsDead || amount <= 0)
            {
                return;
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
