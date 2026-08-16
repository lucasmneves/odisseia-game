using UnityEngine;

namespace Odisseia.Systems
{
    /// <summary>
    /// Câmera 2D com follow suave, zoom e limites configuráveis.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraFollow : MonoBehaviour
    {
        [Header("Alvo")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector2 offset = new Vector2(0f, 1f);
        [SerializeField] private float smoothTime = 0.15f;

        [Header("Zoom")]
        [SerializeField] private float orthographicSize = 5f;

        [Header("Limites")]
        [SerializeField] private bool useBounds;
        [SerializeField] private Vector2 minBounds;
        [SerializeField] private Vector2 maxBounds;

        private static CameraFollow active;

        private Camera cam;
        private Vector3 velocity;

        // A posição "limpa" do follow é mantida separada da posição final do transform:
        // sem isso, o deslocamento do shake entraria no SmoothDamp do frame seguinte
        // e a câmera brigaria consigo mesma.
        private Vector3 basePosition;
        private float shakeTimeRemaining;
        private float shakeDuration;
        private float shakeMagnitude;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = orthographicSize;
            basePosition = transform.position;
        }

        private void OnEnable()
        {
            active = this;
        }

        private void OnDisable()
        {
            if (active == this)
            {
                active = null;
            }
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        /// <summary>Screen shake leve. Ignorado silenciosamente se não houver câmera ativa.</summary>
        public static void ShakeActive(float duration, float magnitude)
        {
            if (active != null)
            {
                active.Shake(duration, magnitude);
            }
        }

        public void Shake(float duration, float magnitude)
        {
            // Um shake mais forte substitui um mais fraco em andamento, em vez de somar —
            // evita acumular tremor quando vários golpes acontecem juntos.
            if (duration <= shakeTimeRemaining && magnitude <= shakeMagnitude)
            {
                return;
            }

            shakeDuration = duration;
            shakeTimeRemaining = duration;
            shakeMagnitude = magnitude;
        }

        private void LateUpdate()
        {
            if (target != null)
            {
                Vector3 desiredPosition = new Vector3(
                    target.position.x + offset.x,
                    target.position.y + offset.y,
                    basePosition.z);

                basePosition = Vector3.SmoothDamp(basePosition, desiredPosition, ref velocity, smoothTime);

                if (useBounds)
                {
                    float halfHeight = cam.orthographicSize;
                    float halfWidth = halfHeight * cam.aspect;

                    basePosition.x = Mathf.Clamp(basePosition.x, minBounds.x + halfWidth, maxBounds.x - halfWidth);
                    basePosition.y = Mathf.Clamp(basePosition.y, minBounds.y + halfHeight, maxBounds.y - halfHeight);
                }
            }

            transform.position = basePosition + CalculateShakeOffset();
        }

        private Vector3 CalculateShakeOffset()
        {
            if (shakeTimeRemaining <= 0f)
            {
                return Vector3.zero;
            }

            shakeTimeRemaining -= Time.unscaledDeltaTime;
            if (shakeTimeRemaining <= 0f)
            {
                return Vector3.zero;
            }

            float strength = shakeMagnitude * (shakeTimeRemaining / shakeDuration);
            return new Vector3(Random.Range(-1f, 1f) * strength, Random.Range(-1f, 1f) * strength, 0f);
        }
    }
}
