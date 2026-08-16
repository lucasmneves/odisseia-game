using System;
using UnityEngine;

namespace Odisseia.Player
{
    /// <summary>
    /// Recurso simples que drena com o tempo (fome da tripulação, Fase 11). Desligado
    /// por padrão no Player.prefab — só é ativado na cena que realmente usa a mecânica,
    /// para não afetar as demais fases. Ao chegar a zero, Odisseu fica fraco
    /// (velocidade reduzida via PlayerController.SpeedMultiplier, já existente).
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class HungerMeter : MonoBehaviour
    {
        [SerializeField] private float maxHunger = 100f;
        [SerializeField] private float drainRate = 3f;
        [SerializeField] private float weakSpeedMultiplier = 0.6f;

        private PlayerController controller;

        public float Hunger { get; private set; }

        /// <summary>Fome normalizada (0..1).</summary>
        public event Action<float> HungerChanged;

        private void Awake()
        {
            controller = GetComponent<PlayerController>();
            Hunger = maxHunger;
        }

        public void Restore(float amount)
        {
            Hunger = Mathf.Min(maxHunger, Hunger + amount);
            HungerChanged?.Invoke(Hunger / maxHunger);
        }

        public void RestoreFull()
        {
            Restore(maxHunger);
        }

        private void Update()
        {
            float previous = Hunger;
            Hunger = Mathf.Max(0f, Hunger - drainRate * Time.deltaTime);

            if (controller != null)
            {
                controller.SpeedMultiplier = Hunger <= 0f ? weakSpeedMultiplier : 1f;
            }

            if (!Mathf.Approximately(previous, Hunger))
            {
                HungerChanged?.Invoke(Hunger / maxHunger);
            }
        }
    }
}
