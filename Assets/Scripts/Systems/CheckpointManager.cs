using UnityEngine;

namespace Odisseia.Systems
{
    /// <summary>
    /// Guarda a posição do último checkpoint ativado na sessão de jogo.
    /// </summary>
    public static class CheckpointManager
    {
        public static bool HasCheckpoint { get; private set; }
        public static Vector3 LastCheckpointPosition { get; private set; }

        public static void SetCheckpoint(Vector3 position)
        {
            LastCheckpointPosition = position;
            HasCheckpoint = true;
        }

        public static void Reset()
        {
            HasCheckpoint = false;
        }
    }
}
