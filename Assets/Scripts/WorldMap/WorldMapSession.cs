namespace Odisseia.WorldMap
{
    /// <summary>
    /// Recado de curta duração entre uma fase e o mapa: qual fase o jogador acabou de
    /// jogar, e se ela foi concluída agora.
    ///
    /// Não é save. O progresso continua inteiro no <c>SaveSystem</c>; isto responde só
    /// "onde colocar Odisseu quando o mapa abrir" e "vale anunciar um desbloqueio".
    /// Derivar isso do save daria a fase concluída de MAIOR ordem, o que colocaria
    /// Odisseu no lugar errado ao rejogar uma fase antiga.
    /// </summary>
    public static class WorldMapSession
    {
        /// <summary>Fase em que Odisseu deve aparecer ao abrir o mapa (pode ser nulo).</summary>
        public static string FocusLevelId { get; private set; }

        /// <summary>Verdadeiro se a fase em foco acabou de ser concluída.</summary>
        public static bool JustCompleted { get; private set; }

        public static void ReportLevelFinished(string levelId, bool completed)
        {
            FocusLevelId = levelId;
            JustCompleted = completed;
        }

        /// <summary>Consumido pelo mapa depois de posicionar o jogador e anunciar.</summary>
        public static void Consume()
        {
            JustCompleted = false;
        }

        /// <summary>Limpa tudo — usado ao começar uma jornada nova.</summary>
        public static void Clear()
        {
            FocusLevelId = null;
            JustCompleted = false;
        }
    }
}
