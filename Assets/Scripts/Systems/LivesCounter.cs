using System;

namespace Odisseia.Systems
{
    /// <summary>
    /// Vidas da jornada atual. Odisseu começa com três: perder uma devolve ao último
    /// checkpoint, perder todas encerra a jornada.
    ///
    /// É estático como <see cref="CollectibleCounter"/> e <see cref="CheckpointManager"/>,
    /// porque a contagem precisa atravessar as trocas de cena — só que, ao contrário
    /// desses dois, <b>não</b> é zerada pelo <c>LevelIntro</c> a cada fase: uma jornada
    /// atravessa as 16 fases.
    /// </summary>
    public static class LivesCounter
    {
        public const int DefaultStartingLives = 3;

        /// <summary>Vidas restantes.</summary>
        public static int Current { get; private set; } = DefaultStartingLives;

        /// <summary>Com quantas vidas a jornada começa.</summary>
        public static int StartingLives { get; private set; } = DefaultStartingLives;

        public static bool IsGameOver => Current <= 0;

        /// <summary>(vidas atuais) — a HUD escuta isto.</summary>
        public static event Action<int> Changed;

        /// <summary>Perdeu uma vida mas ainda restam outras: vai voltar ao checkpoint.</summary>
        public static event Action<int> LifeLost;

        /// <summary>Acabaram as vidas.</summary>
        public static event Action GameOver;

        /// <summary>Ganhou uma vida (por experiência, por exemplo).</summary>
        public static event Action<int> LifeGained;

        /// <summary>Começa uma jornada nova. Chamado ao entrar no menu principal.</summary>
        public static void BeginRun(int startingLives = DefaultStartingLives)
        {
            StartingLives = startingLives > 0 ? startingLives : DefaultStartingLives;
            Current = StartingLives;
            Changed?.Invoke(Current);
        }

        /// <summary>
        /// Consome uma vida. Dispara <see cref="LifeLost"/> se ainda sobrar alguma, ou
        /// <see cref="GameOver"/> se era a última — quem chama não precisa decidir qual
        /// dos dois eventos emitir, o que evita os dois caminhos saírem de sincronia.
        /// </summary>
        /// <returns>Verdadeiro se ainda há vidas (ou seja, se cabe respawn).</returns>
        public static bool LoseLife()
        {
            if (Current <= 0)
            {
                return false;
            }

            Current--;
            Changed?.Invoke(Current);

            if (Current > 0)
            {
                LifeLost?.Invoke(Current);
                return true;
            }

            GameOver?.Invoke();
            return false;
        }

        public static void Gain(int amount = 1)
        {
            if (amount <= 0 || IsGameOver)
            {
                return;
            }

            Current += amount;
            Changed?.Invoke(Current);
            LifeGained?.Invoke(Current);
        }
    }
}
