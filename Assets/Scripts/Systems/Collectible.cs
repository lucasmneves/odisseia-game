using System.Collections;
using UnityEngine;

namespace Odisseia.Systems
{
    /// <summary>
    /// Item colecionável: flutua/gira, e ao ser tocado pelo jogador soma no contador
    /// e toca um efeito simples antes de ser destruído.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Collectible : MonoBehaviour
    {
        [Header("Animação simples")]
        [SerializeField] private float bobHeight = 0.15f;
        [SerializeField] private float bobSpeed = 2f;
        [SerializeField] private float rotateSpeed = 90f;

        [Header("Coleta")]
        [SerializeField] private int value = 1;
        [SerializeField] private GameObject collectEffectPrefab;
        [SerializeField] private float collectEffectDuration = 0.25f;

        private Vector3 startPosition;
        private bool collected;

        private void Awake()
        {
            startPosition = transform.position;
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void Update()
        {
            if (collected)
            {
                return;
            }

            float offset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = startPosition + Vector3.up * offset;
            transform.Rotate(Vector3.forward, rotateSpeed * Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (collected || !other.CompareTag("Player"))
            {
                return;
            }

            collected = true;
            CollectibleCounter.Add(value);
            GetComponent<Collider2D>().enabled = false;

            if (collectEffectPrefab != null)
            {
                Instantiate(collectEffectPrefab, transform.position, Quaternion.identity);
            }

            var sr = GetComponent<SpriteRenderer>();
            Color burstColor = sr != null ? sr.color : new Color(1f, 0.85f, 0.3f);
            Sprite sprite = Odisseia.Core.GameAssets.Instance != null
                ? Odisseia.Core.GameAssets.Instance.PlaceholderSprite
                : null;
            VfxBurst.Spawn(sprite, transform.position, burstColor, 8, 2.5f, 0.35f, 0.1f, 3f);
            AudioManager.PlayCollect();

            StartCoroutine(PlayCollectEffectAndDestroy());
        }

        private IEnumerator PlayCollectEffectAndDestroy()
        {
            var sr = GetComponent<SpriteRenderer>();
            Vector3 startScale = transform.localScale;
            Color startColor = sr != null ? sr.color : Color.white;
            float elapsed = 0f;

            while (elapsed < collectEffectDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / collectEffectDuration;
                transform.localScale = startScale * (1f + t);

                if (sr != null)
                {
                    Color c = startColor;
                    c.a = 1f - t;
                    sr.color = c;
                }

                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
