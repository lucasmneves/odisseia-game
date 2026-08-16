using UnityEngine;
using Odisseia.Core;
using Odisseia.Systems;

namespace Odisseia.Player
{
    /// <summary>
    /// Ao morrer, retorna Odisseu ao último checkpoint (ou ao ponto de spawn, se nenhum foi ativado).
    /// </summary>
    [RequireComponent(typeof(HealthSystem))]
    public class PlayerRespawn : MonoBehaviour
    {
        private HealthSystem health;
        private Rigidbody2D rb;
        private Vector3 spawnPosition;

        private void Awake()
        {
            health = GetComponent<HealthSystem>();
            rb = GetComponent<Rigidbody2D>();
            spawnPosition = transform.position;
        }

        private void OnEnable()
        {
            health.Died += OnDied;
        }

        private void OnDisable()
        {
            health.Died -= OnDied;
        }

        private void OnDied()
        {
            Vector3 respawnPosition = CheckpointManager.HasCheckpoint
                ? CheckpointManager.LastCheckpointPosition
                : spawnPosition;

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }

            transform.position = respawnPosition;
            health.ResetHealth();
        }
    }
}
