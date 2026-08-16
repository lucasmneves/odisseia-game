using System;
using System.Collections;
using UnityEngine;
using Odisseia.Core;

namespace Odisseia.Enemies
{
    /// <summary>
    /// Comportamento de mini-boss reutilizável: ataques telegrafados em pontos fixos do
    /// cenário, com aviso visual antes do golpe. Não persegue nem precisa ser derrotado —
    /// a ideia é priorizar esquiva, uso do ambiente e fuga em vez de combate direto.
    /// Cada ponto de ataque é um Transform com um SpriteRenderer usado como aviso.
    /// </summary>
    public class BossController : MonoBehaviour
    {
        [Header("Ataque")]
        [SerializeField] private Transform[] attackPoints;
        [SerializeField] private float attackInterval = 3f;
        [SerializeField] private float telegraphDuration = 0.8f;
        [SerializeField] private float attackRadius = 1.5f;
        [SerializeField] private int damage = 40;
        [SerializeField] private float recoveryDuration = 0.4f;
        [SerializeField] private LayerMask targetLayer;

        public event Action AttackTelegraphed;
        public event Action AttackExecuted;

        private float timer;
        private bool active = true;

        public void SetActive(bool value)
        {
            active = value;
        }

        private void Update()
        {
            if (!active || attackPoints == null || attackPoints.Length == 0)
            {
                return;
            }

            timer += Time.deltaTime;
            if (timer >= attackInterval)
            {
                timer = 0f;
                StartCoroutine(AttackRoutine());
            }
        }

        private IEnumerator AttackRoutine()
        {
            active = false;

            foreach (Transform point in attackPoints)
            {
                SetMarkerVisible(point, true);
            }

            AttackTelegraphed?.Invoke();
            yield return new WaitForSeconds(telegraphDuration);

            AttackExecuted?.Invoke();
            foreach (Transform point in attackPoints)
            {
                if (point == null)
                {
                    continue;
                }

                Collider2D[] hits = Physics2D.OverlapCircleAll(point.position, attackRadius, targetLayer);
                foreach (Collider2D hit in hits)
                {
                    if (hit.TryGetComponent(out HealthSystem health))
                    {
                        health.TakeDamage(damage);
                    }
                }

                SetMarkerVisible(point, false);
            }

            yield return new WaitForSeconds(recoveryDuration);
            active = true;
        }

        private static void SetMarkerVisible(Transform point, bool visible)
        {
            if (point != null && point.TryGetComponent(out SpriteRenderer marker))
            {
                marker.enabled = visible;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (attackPoints == null)
            {
                return;
            }

            Gizmos.color = Color.red;
            foreach (Transform point in attackPoints)
            {
                if (point != null)
                {
                    Gizmos.DrawWireSphere(point.position, attackRadius);
                }
            }
        }
    }
}
