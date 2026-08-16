using System;
using UnityEngine;
using Odisseia.Core;

namespace Odisseia.Player
{
    /// <summary>
    /// Barra de resistência ao canto das sereias: drena enquanto o jogador está dentro
    /// de uma SirenZone (a não ser que esteja imune, "amarrado ao mastro"), regenera
    /// fora dela. Se chegar a zero, o canto vence — mesmo desfecho de morte usado no
    /// resto do jogo (HealthSystem.TakeDamage), para reaproveitar o respawn no checkpoint.
    /// </summary>
    [RequireComponent(typeof(HealthSystem))]
    public class SirenResistance : MonoBehaviour
    {
        [SerializeField] private float maxResistance = 100f;
        [SerializeField] private float drainRate = 20f;
        [SerializeField] private float regenRate = 15f;

        private HealthSystem health;
        private int zoneCount;
        private bool immune;

        public float Resistance { get; private set; }

        /// <summary>Resistência normalizada (0..1).</summary>
        public event Action<float> ResistanceChanged;

        private void Awake()
        {
            health = GetComponent<HealthSystem>();
            Resistance = maxResistance;
        }

        public void EnterZone()
        {
            zoneCount++;
        }

        public void ExitZone()
        {
            zoneCount = Mathf.Max(0, zoneCount - 1);
        }

        public void SetImmune(bool value)
        {
            immune = value;
        }

        private void Update()
        {
            float previous = Resistance;

            if (zoneCount > 0 && !immune)
            {
                Resistance = Mathf.Max(0f, Resistance - drainRate * Time.deltaTime);
                if (Resistance <= 0f)
                {
                    health.TakeDamage(health.CurrentHealth);
                    Resistance = maxResistance;
                }
            }
            else
            {
                Resistance = Mathf.Min(maxResistance, Resistance + regenRate * Time.deltaTime);
            }

            if (!Mathf.Approximately(previous, Resistance))
            {
                ResistanceChanged?.Invoke(Resistance / maxResistance);
            }
        }
    }
}
