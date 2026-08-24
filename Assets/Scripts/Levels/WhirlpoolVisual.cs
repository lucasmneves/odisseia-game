using UnityEngine;

namespace Odisseia.Levels
{
    /// <summary>
    /// Aparência do redemoinho — só isso. Quem puxa Odisseu é a <see cref="WindZone"/>
    /// no mesmo objeto, e quem mata é a zona letal embaixo.
    ///
    /// Existe porque uma força que empurra o jogador para trás sem nada na tela é
    /// indistinguível de bug: o giro e a pulsação dizem onde a água está girando e o
    /// quanto ela alcança. É placeholder geométrico, trocável pela arte definitiva sem
    /// mexer em gameplay.
    /// </summary>
    public class WhirlpoolVisual : MonoBehaviour
    {
        [Header("Giro")]
        [SerializeField] private float degreesPerSecond = -70f;

        [Tooltip("Cada anel gira um pouco mais devagar que o de fora, para dar profundidade.")]
        [SerializeField] private float innerSpeedFalloff = 0.55f;

        [Header("Pulsação")]
        [SerializeField] private float pulseAmplitude = 0.06f;
        [SerializeField] private float pulseSpeed = 1.8f;

        [Header("Anéis (placeholder)")]
        [SerializeField] private Transform[] rings;

        private Vector3[] baseScales;

        private void Awake()
        {
            if (rings == null || rings.Length == 0)
            {
                // Sem anéis ligados no Inspector, gira os próprios filhos.
                rings = new Transform[transform.childCount];
                for (int i = 0; i < transform.childCount; i++)
                {
                    rings[i] = transform.GetChild(i);
                }
            }

            baseScales = new Vector3[rings.Length];
            for (int i = 0; i < rings.Length; i++)
            {
                baseScales[i] = rings[i] != null ? rings[i].localScale : Vector3.one;
            }
        }

        private void Update()
        {
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmplitude;

            for (int i = 0; i < rings.Length; i++)
            {
                Transform ring = rings[i];
                if (ring == null)
                {
                    continue;
                }

                float speed = degreesPerSecond * Mathf.Pow(innerSpeedFalloff, i);
                ring.Rotate(0f, 0f, speed * Time.deltaTime);
                ring.localScale = baseScales[i] * pulse;
            }
        }
    }
}
