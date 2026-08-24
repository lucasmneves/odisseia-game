using UnityEngine;
using Odisseia.Core;
using Odisseia.Systems;

namespace Odisseia.Levels
{
    /// <summary>
    /// Perigo cíclico e telegrafado da tempestade — o raio que cai, a onda que quebra
    /// sobre o convés. Alterna três estados em laço: descanso (inofensivo e apagado),
    /// aviso (pisca, ainda inofensivo) e golpe (opaco e letal por um instante).
    ///
    /// O aviso existe para o perigo ser justo: o jogador aprende o ritmo e atravessa
    /// entre os golpes, em vez de tomar dano por não ter como prever. É o mesmo
    /// contrato do <see cref="TidalHazard"/> — dano por contato, sem combate — só que
    /// aqui o que muda é o tempo, não a posição.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class StormHazard : MonoBehaviour
    {
        private enum Phase
        {
            Resting,
            Warning,
            Striking,
        }

        [Header("Ritmo (segundos)")]
        [SerializeField] private float restDuration = 1.8f;
        [SerializeField] private float warnDuration = 1f;
        [SerializeField] private float strikeDuration = 0.45f;

        [Tooltip("Desloca o início do ciclo, para vários perigos não baterem juntos.")]
        [SerializeField] private float phaseOffset;

        [Header("Dano")]
        [Tooltip("0 ou menos = letal (mesmo efeito do KillZone).")]
        [SerializeField] private int damage;

        [Header("Visual (placeholder)")]
        [SerializeField] private SpriteRenderer visual;
        [SerializeField] private Color restColor = new Color(0.6f, 0.7f, 0.9f, 0.06f);
        [SerializeField] private Color warnColor = new Color(0.9f, 0.9f, 0.6f, 0.35f);
        [SerializeField] private Color strikeColor = new Color(1f, 1f, 0.85f, 0.95f);
        [SerializeField] private float warnBlinkSpeed = 8f;

        [Header("Câmera")]
        [SerializeField] private float shakeOnStrike = 0.12f;

        private Collider2D area;
        private Phase phase = Phase.Resting;
        private float timer;

        private float CycleLength => Mathf.Max(0.1f, restDuration + warnDuration + strikeDuration);

        private void Awake()
        {
            area = GetComponent<Collider2D>();
            area.isTrigger = true;

            if (visual == null)
            {
                visual = GetComponent<SpriteRenderer>();
            }

            // O deslocamento entra como tempo já decorrido no ciclo: cada perigo nasce
            // num ponto diferente do laço sem precisar de coroutine nem de Invoke.
            timer = Mathf.Repeat(phaseOffset, CycleLength);
            ApplyPhaseForTimer();
        }

        private void Update()
        {
            timer = Mathf.Repeat(timer + Time.deltaTime, CycleLength);

            Phase previous = phase;
            ApplyPhaseForTimer();

            if (phase == Phase.Striking && previous != Phase.Striking)
            {
                OnStrikeBegan();
            }

            UpdateVisual();
        }

        private void ApplyPhaseForTimer()
        {
            if (timer < restDuration)
            {
                phase = Phase.Resting;
            }
            else if (timer < restDuration + warnDuration)
            {
                phase = Phase.Warning;
            }
            else
            {
                phase = Phase.Striking;
            }
        }

        private void OnStrikeBegan()
        {
            CameraFollow.ShakeActive(strikeDuration, shakeOnStrike);
        }

        private void UpdateVisual()
        {
            if (visual == null)
            {
                return;
            }

            switch (phase)
            {
                case Phase.Resting:
                    visual.color = restColor;
                    break;

                case Phase.Warning:
                    // Pisca entre repouso e aviso: leitura clara de "vai cair aqui".
                    float t = Mathf.PingPong(Time.time * warnBlinkSpeed, 1f);
                    visual.color = Color.Lerp(restColor, warnColor, t);
                    break;

                case Phase.Striking:
                    visual.color = strikeColor;
                    break;
            }
        }

        private void OnTriggerEnter2D(Collider2D other) => TryHit(other);

        private void OnTriggerStay2D(Collider2D other) => TryHit(other);

        private void TryHit(Collider2D other)
        {
            if (phase != Phase.Striking)
            {
                return;
            }

            if (!other.TryGetComponent(out HealthSystem health))
            {
                return;
            }

            health.TakeDamage(damage > 0 ? damage : health.CurrentHealth);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.9f, 0.3f, 0.5f);
            Gizmos.DrawWireCube(transform.position, transform.lossyScale);
        }
    }
}
