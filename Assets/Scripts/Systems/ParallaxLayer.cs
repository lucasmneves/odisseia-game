using UnityEngine;

namespace Odisseia.Systems
{
    /// <summary>
    /// Camada de fundo com parallax: acompanha a câmera a uma fração da velocidade,
    /// dando sensação de profundidade sem custo (um único sprite por camada, nenhuma
    /// alocação por frame).
    /// </summary>
    public class ParallaxLayer : MonoBehaviour
    {
        [SerializeField] [Range(0f, 1f)] private float parallaxFactor = 0.3f;

        private Transform cameraTransform;
        private Vector3 startPosition;
        private Vector3 cameraStartPosition;

        private void Start()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                cameraTransform = mainCamera.transform;
                cameraStartPosition = cameraTransform.position;
            }

            startPosition = transform.position;
        }

        private void LateUpdate()
        {
            if (cameraTransform == null)
            {
                return;
            }

            Vector3 delta = cameraTransform.position - cameraStartPosition;
            transform.position = new Vector3(
                startPosition.x + delta.x * parallaxFactor,
                startPosition.y + delta.y * parallaxFactor,
                startPosition.z);
        }
    }
}
