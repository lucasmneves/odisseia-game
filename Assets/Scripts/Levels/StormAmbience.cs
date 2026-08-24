using UnityEngine;
using UnityEngine.UI;
using Odisseia.Systems;

namespace Odisseia.Levels
{
    /// <summary>
    /// Clima da tempestade: chuva em diagonal, clarões de relâmpago e tremor de câmera.
    /// É só ambientação — não causa dano nem bloqueia nada (quem machuca é o
    /// <see cref="StormHazard"/> e o <see cref="TidalHazard"/>).
    ///
    /// A chuva é um punhado de sprites reciclados em vez de um ParticleSystem: são
    /// poucos objetos, o custo em WebGL é desprezível e a arte definitiva entra depois
    /// trocando só o sprite no Inspector.
    /// </summary>
    public class StormAmbience : MonoBehaviour
    {
        [Header("Chuva (placeholder)")]
        [SerializeField] private Sprite raindropSprite;
        [SerializeField] private int raindropCount = 60;
        [SerializeField] private Vector2 rainVelocity = new Vector2(-5f, -16f);
        [SerializeField] private Vector2 rainAreaSize = new Vector2(28f, 16f);
        [SerializeField] private Vector2 raindropScale = new Vector2(0.05f, 0.55f);
        [SerializeField] private Color rainColor = new Color(0.72f, 0.82f, 0.95f, 0.5f);
        [SerializeField] private int rainSortingOrder = 40;

        [Header("Relâmpago")]
        [SerializeField] private Image flashOverlay;
        [SerializeField] private Vector2 flashIntervalRange = new Vector2(3.5f, 8f);
        [SerializeField] private float flashPeakAlpha = 0.55f;
        [SerializeField] private float flashFadeSpeed = 3.5f;
        [SerializeField] private float flashShakeMagnitude = 0.18f;
        [SerializeField] private float flashShakeDuration = 0.5f;

        [Header("Seguir a câmera")]
        [SerializeField] private Transform followTarget;

        private Transform rainRoot;
        private Transform[] raindrops;
        private float nextFlashTime;
        private float flashAlpha;

        private void Start()
        {
            if (followTarget == null && Camera.main != null)
            {
                followTarget = Camera.main.transform;
            }

            BuildRain();
            ScheduleNextFlash();

            if (flashOverlay != null)
            {
                SetFlashAlpha(0f);
            }
        }

        private void Update()
        {
            FollowCamera();
            MoveRain();
            UpdateFlash();
        }

        // ------------------------------------------------------------------ chuva

        private void BuildRain()
        {
            if (raindropSprite == null || raindropCount <= 0)
            {
                return;
            }

            rainRoot = new GameObject("RainRoot").transform;
            rainRoot.SetParent(transform, worldPositionStays: false);

            raindrops = new Transform[raindropCount];

            for (int i = 0; i < raindropCount; i++)
            {
                var drop = new GameObject($"Raindrop_{i:00}");
                drop.transform.SetParent(rainRoot, worldPositionStays: false);
                drop.transform.localPosition = RandomRainPosition();
                drop.transform.localScale = new Vector3(raindropScale.x, raindropScale.y, 1f);

                SpriteRenderer renderer = drop.AddComponent<SpriteRenderer>();
                renderer.sprite = raindropSprite;
                renderer.color = rainColor;
                renderer.sortingOrder = rainSortingOrder;

                raindrops[i] = drop.transform;
            }
        }

        private Vector3 RandomRainPosition()
        {
            return new Vector3(
                Random.Range(-rainAreaSize.x * 0.5f, rainAreaSize.x * 0.5f),
                Random.Range(-rainAreaSize.y * 0.5f, rainAreaSize.y * 0.5f),
                0f);
        }

        private void MoveRain()
        {
            if (raindrops == null)
            {
                return;
            }

            Vector3 step = (Vector3)rainVelocity * Time.deltaTime;
            float halfWidth = rainAreaSize.x * 0.5f;
            float halfHeight = rainAreaSize.y * 0.5f;

            foreach (Transform drop in raindrops)
            {
                Vector3 local = drop.localPosition + step;

                // Reciclagem: a gota que sai por baixo (ou pela lateral, por causa do
                // vento) volta pelo topo, então a chuva nunca "acaba".
                if (local.y < -halfHeight || local.x < -halfWidth || local.x > halfWidth)
                {
                    local.y = halfHeight;
                    local.x = Random.Range(-halfWidth, halfWidth);
                }

                drop.localPosition = local;
            }
        }

        private void FollowCamera()
        {
            if (followTarget == null)
            {
                return;
            }

            Vector3 position = followTarget.position;
            position.z = 0f;
            transform.position = position;
        }

        // ------------------------------------------------------------------ relâmpago

        private void ScheduleNextFlash()
        {
            nextFlashTime = Time.time + Random.Range(flashIntervalRange.x, flashIntervalRange.y);
        }

        private void UpdateFlash()
        {
            if (flashOverlay == null)
            {
                return;
            }

            if (Time.time >= nextFlashTime)
            {
                flashAlpha = flashPeakAlpha;
                CameraFollow.ShakeActive(flashShakeDuration, flashShakeMagnitude);
                ScheduleNextFlash();
            }

            if (flashAlpha > 0f)
            {
                flashAlpha = Mathf.MoveTowards(flashAlpha, 0f, flashFadeSpeed * Time.deltaTime);
                SetFlashAlpha(flashAlpha);
            }
        }

        private void SetFlashAlpha(float alpha)
        {
            Color color = flashOverlay.color;
            color.a = alpha;
            flashOverlay.color = color;
        }
    }
}
