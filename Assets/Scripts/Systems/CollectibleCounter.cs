using System;

namespace Odisseia.Systems
{
    /// <summary>
    /// Contador global de coletáveis da sessão de jogo.
    /// </summary>
    public static class CollectibleCounter
    {
        public static int Count { get; private set; }

        public static event Action<int> CountChanged;

        public static void Add(int amount = 1)
        {
            Count += amount;
            CountChanged?.Invoke(Count);
        }

        public static void Reset()
        {
            Count = 0;
            CountChanged?.Invoke(Count);
        }
    }
}
