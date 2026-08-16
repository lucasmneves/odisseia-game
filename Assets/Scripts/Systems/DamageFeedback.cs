using System.Collections;
using UnityEngine;
using Odisseia.Core;

namespace Odisseia.Systems
{
    /// <summary>
    /// Feedback de dano/morte genérico, ligado a qualquer HealthSystem (Odisseu,
    /// inimigos, bosses): pisca os sprites em branco, solta um burst de partículas,
    /// sacode a câmera de leve e toca o som correspondente. Um único componente
    /// cobre todos os casos em vez de repetir feedback em cada script.
    /// </summary>
    [RequireComponent(typeof(HealthSystem))]
    public class DamageFeedback : MonoBehaviour
    {
        [SerializeField] private bool isPlayer;
        [SerializeField] private Color flashColor = Color.white;
        [SerializeField] private float flashDuration = 0.1f;
        [SerializeField] private Color burstColor = new Color(1f, 0.4f, 0.3f);
        [SerializeField] private float shakeMagnitude = 0.12f;

        private HealthSystem health;
        private SpriteRenderer[] renderers;
        private Color[] originalColors;
        private Coroutine flashRoutine;

        private void Awake()
        {
            health = GetComponent<HealthSystem>();
            renderers = GetComponentsInChildren<SpriteRenderer>();
            originalColors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                originalColors[i] = renderers[i].color;
            }
        }

        private void OnEnable()
        {
            health.Damaged += OnDamaged;
            health.Died += OnDied;
        }

        private void OnDisable()
        {
            health.Damaged -= OnDamaged;
            health.Died -= OnDied;
        }

        private void OnDamaged(int amount, int currentHealth)
        {
            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
                RestoreColors();
            }

            flashRoutine = StartCoroutine(FlashRoutine());

            Sprite sprite = GameAssets.Instance != null ? GameAssets.Instance.PlaceholderSprite : null;
            VfxBurst.Spawn(sprite, transform.position + Vector3.up * 0.6f, burstColor, 5, 2.5f, 0.3f);

            CameraFollow.ShakeActive(0.12f, shakeMagnitude);

            if (isPlayer)
            {
                AudioManager.PlayPlayerDamage();
            }
            else
            {
                AudioManager.PlayHit();
            }
        }

        private void OnDied()
        {
            Sprite sprite = GameAssets.Instance != null ? GameAssets.Instance.PlaceholderSprite : null;
            VfxBurst.Spawn(sprite, transform.position + Vector3.up * 0.6f, burstColor, 10, 3.5f, 0.45f, 0.15f);

            CameraFollow.ShakeActive(0.22f, shakeMagnitude * 1.8f);
            AudioManager.PlayDeath();
        }

        private IEnumerator FlashRoutine()
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    renderers[i].color = flashColor;
                }
            }

            yield return new WaitForSeconds(flashDuration);

            RestoreColors();
            flashRoutine = null;
        }

        private void RestoreColors()
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null)
                {
                    renderers[i].color = originalColors[i];
                }
            }
        }
    }
}
