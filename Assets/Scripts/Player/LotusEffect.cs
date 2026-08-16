using System;
using UnityEngine;

namespace Odisseia.Player
{
    /// <summary>
    /// Sonolência do lótus: acumula enquanto o jogador está dentro de uma LotusZone,
    /// decai fora dela. Reduz a velocidade progressivamente e, se atingir o máximo,
    /// trava o controle por alguns segundos (o jogador "esquece" tudo por um instante).
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class LotusEffect : MonoBehaviour
    {
        [SerializeField] private float buildRate = 25f;
        [SerializeField] private float decayRate = 15f;
        [SerializeField] private float maxDrowsiness = 100f;
        [SerializeField] private float stunDuration = 2f;
        [SerializeField] private float speedMultiplierAtMax = 0.35f;

        private PlayerController controller;
        private PlayerInputLock inputLock;
        private int zoneCount;
        private float stunTimer;

        public float Drowsiness { get; private set; }
        public bool IsStunned => stunTimer > 0f;

        /// <summary>Sonolência normalizada (0..1).</summary>
        public event Action<float> DrowsinessChanged;

        private void Awake()
        {
            controller = GetComponent<PlayerController>();
            inputLock = GetComponent<PlayerInputLock>();
        }

        public void EnterZone()
        {
            zoneCount++;
        }

        public void ExitZone()
        {
            zoneCount = Mathf.Max(0, zoneCount - 1);
        }

        private void Update()
        {
            float previous = Drowsiness;

            if (stunTimer > 0f)
            {
                stunTimer -= Time.deltaTime;
                if (stunTimer <= 0f)
                {
                    inputLock?.SetLocked(false);
                    Drowsiness = maxDrowsiness * 0.5f;
                }
            }
            else if (zoneCount > 0)
            {
                Drowsiness = Mathf.Min(maxDrowsiness, Drowsiness + buildRate * Time.deltaTime);
                if (Drowsiness >= maxDrowsiness)
                {
                    stunTimer = stunDuration;
                    inputLock?.SetLocked(true);
                }
            }
            else
            {
                Drowsiness = Mathf.Max(0f, Drowsiness - decayRate * Time.deltaTime);
            }

            float normalized = Drowsiness / maxDrowsiness;
            if (controller != null)
            {
                controller.SpeedMultiplier = Mathf.Lerp(1f, speedMultiplierAtMax, normalized);
            }

            if (!Mathf.Approximately(previous, Drowsiness))
            {
                DrowsinessChanged?.Invoke(normalized);
            }
        }
    }
}
