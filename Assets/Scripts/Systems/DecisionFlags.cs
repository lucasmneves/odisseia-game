using System.Collections.Generic;

namespace Odisseia.Systems
{
    /// <summary>
    /// Registro simples e genérico de decisões do jogador ao longo da campanha
    /// (ex.: "comeu o gado sagrado"). Persiste durante a sessão de jogo; é resetado
    /// em CampaignManager.StartNewGame(), não a cada fase — decisões são de campanha,
    /// não de nível.
    /// </summary>
    public static class DecisionFlags
    {
        private static readonly HashSet<string> flags = new HashSet<string>();

        public static void Set(string key)
        {
            flags.Add(key);
        }

        public static bool IsSet(string key)
        {
            return flags.Contains(key);
        }

        public static void Reset()
        {
            flags.Clear();
        }
    }
}
