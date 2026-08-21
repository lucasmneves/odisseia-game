using UnityEngine;
using Odisseia.Core;
using Odisseia.Systems;

namespace Odisseia.Player
{
    /// <summary>
    /// Ao morrer, consome uma vida e retorna Odisseu ao último checkpoint (ou ao ponto
    /// de spawn, se nenhum foi ativado). Sem vidas restantes não há respawn — o
    /// <see cref="LivesCounter"/> dispara o fim de jogo.
    ///
    /// Este é o único ponto do projeto que consome vida, de propósito: se dois lugares
    /// descontassem, uma morte custaria duas.
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
            // Era a última vida: fica onde caiu e o fim de jogo assume a tela.
            if (!LivesCounter.LoseLife())
            {
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                }

                return;
            }

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
