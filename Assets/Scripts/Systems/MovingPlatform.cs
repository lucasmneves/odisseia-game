using UnityEngine;

namespace Odisseia.Systems
{
    /// <summary>
    /// Plataforma cinemática que oscila entre dois pontos. O PlayerController detecta
    /// esse componente no chão sob os pés e soma o <see cref="FrameDelta"/> à própria
    /// posição, para ser "carregado" de forma determinística (sem depender de atrito).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class MovingPlatform : MonoBehaviour
    {
        [SerializeField] private Vector3 pointA;
        [SerializeField] private Vector3 pointB;
        [SerializeField] private float speed = 2f;
        [SerializeField] private bool startAtPointA = true;

        private Rigidbody2D rb;
        private Vector3 target;

        public Vector3 FrameDelta { get; private set; }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            target = startAtPointA ? pointB : pointA;
        }

        private void FixedUpdate()
        {
            Vector3 previous = rb.position;
            Vector3 newPos = Vector3.MoveTowards(previous, target, speed * Time.fixedDeltaTime);
            rb.MovePosition(newPos);
            FrameDelta = newPos - previous;

            if (Vector3.Distance(newPos, target) < 0.05f)
            {
                target = target == pointA ? pointB : pointA;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(pointA, pointB);
            Gizmos.DrawWireSphere(pointA, 0.2f);
            Gizmos.DrawWireSphere(pointB, 0.2f);
        }
    }
}
