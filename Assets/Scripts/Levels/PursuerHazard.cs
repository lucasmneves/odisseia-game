using System.Collections;
using UnityEngine;
using Odisseia.Core;

namespace Odisseia.Levels
{
    /// <summary>
    /// Ameaça que se move a velocidade constante e mata instantaneamente ao tocar
    /// (usada como perseguição — gigante, avalanche, maré alta etc.). Reaproveita o
    /// padrão do KillZone para o dano; ao ouvir a morte do alvo, reposiciona-se atrás
    /// da posição de respawn após um frame, para não ficar "perdida" no nível.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PursuerHazard : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private HealthSystem targetHealth;
        [SerializeField] private float speed = 5.5f;
        [SerializeField] private float resetOffsetX = -8f;

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void OnEnable()
        {
            if (targetHealth != null)
            {
                targetHealth.Died += OnTargetDied;
            }
        }

        private void OnDisable()
        {
            if (targetHealth != null)
            {
                targetHealth.Died -= OnTargetDied;
            }
        }

        private void Update()
        {
            transform.position += Vector3.right * (speed * Time.deltaTime);
        }

        private void OnTargetDied()
        {
            StartCoroutine(ResetAfterRespawn());
        }

        private IEnumerator ResetAfterRespawn()
        {
            yield return null;

            if (target != null)
            {
                transform.position = new Vector3(target.position.x + resetOffsetX, transform.position.y,
                    transform.position.z);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out HealthSystem health))
            {
                health.TakeDamage(health.CurrentHealth);
            }
        }
    }
}
