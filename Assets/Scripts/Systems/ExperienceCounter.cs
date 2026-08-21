using System;

namespace Odisseia.Systems
{
    /// <summary>
    /// Experiência acumulada derrotando inimigos. A cada <see cref="ExperiencePerLife"/>
    /// pontos, Odisseu ganha uma vida — é o caminho do jogador para sustentar a jornada
    /// enfrentando inimigos em vez de só desviar deles.
    ///
    /// Depende de <see cref="LivesCounter"/> num sentido só: aqui está a regra de
    /// conversão, e lá está apenas a contagem.
    /// </summary>
    public static class ExperienceCounter
    {
        /// <summary>XP necessário para uma vida extra. 5 inimigos comuns.</summary>
        public const int ExperiencePerLife = 125;

        /// <summary>XP total acumulado na jornada.</summary>
        public static int Total { get; private set; }

        /// <summary>Quanto falta para a próxima vida.</summary>
        public static int TowardNextLife => Total - (livesGranted * ExperiencePerLife);

        private static int livesGranted;

        /// <summary>(total, progresso rumo à próxima vida) — a HUD escuta isto.</summary>
        public static event Action<int, int> Changed;

        public static void BeginRun()
        {
            Total = 0;
            livesGranted = 0;
            Changed?.Invoke(Total, TowardNextLife);
        }

        /// <summary>
        /// Credita experiência e converte em vidas. O laço cobre o caso de um prêmio
        /// grande valer mais de uma vida de uma vez.
        /// </summary>
        public static void Add(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            Total += amount;

            while (Total >= (livesGranted + 1) * ExperiencePerLife)
            {
                livesGranted++;
                LivesCounter.Gain();
            }

            Changed?.Invoke(Total, TowardNextLife);
        }
    }
}
