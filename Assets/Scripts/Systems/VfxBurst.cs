using UnityEngine;

namespace Odisseia.Systems
{
    /// <summary>
    /// Efeito visual leve e descartável: alguns quadradinhos coloridos que voam para
    /// fora e somem. Feito com SpriteRenderers em vez de ParticleSystem de propósito —
    /// para WebGL, um punhado de sprites é mais barato (e mais previsível) do que
    /// instanciar sistemas de partículas completos a cada golpe/coleta.
    /// Auto-destrói ao terminar; nada fica acumulado na cena.
    /// </summary>
    public class VfxBurst : MonoBehaviour
    {
        private struct Shard
        {
            public Transform transform;
            public SpriteRenderer renderer;
            public Vector3 velocity;
        }

        private Shard[] shards;
        private float duration;
        private float elapsed;
        private float gravity;

        /// <summary>
        /// Cria um burst na posição indicada. count é limitado a 12 para manter o
        /// custo previsível em Web.
        /// </summary>
        public static void Spawn(Sprite sprite, Vector3 position, Color color, int count = 6,
            float speed = 3f, float duration = 0.35f, float size = 0.12f, float gravity = 6f)
        {
            if (sprite == null)
            {
                return;
            }

            var go = new GameObject("VfxBurst");
            go.transform.position = position;
            var burst = go.AddComponent<VfxBurst>();
            burst.Initialize(sprite, color, Mathf.Clamp(count, 1, 12), speed, duration, size, gravity);
        }

        private void Initialize(Sprite sprite, Color color, int count, float speed, float lifetime,
            float size, float gravityValue)
        {
            duration = lifetime;
            gravity = gravityValue;
            shards = new Shard[count];

            for (int i = 0; i < count; i++)
            {
                var shardGO = new GameObject("Shard");
                shardGO.transform.SetParent(transform, false);
                shardGO.transform.localScale = Vector3.one * size;

                var sr = shardGO.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.color = color;
                sr.sortingOrder = 20;

                float angle = (360f / count) * i + Random.Range(-12f, 12f);
                float rad = angle * Mathf.Deg2Rad;
                var dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);

                shards[i] = new Shard
                {
                    transform = shardGO.transform,
                    renderer = sr,
                    velocity = dir * (speed * Random.Range(0.6f, 1.2f)),
                };
            }
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            if (t >= 1f)
            {
                Destroy(gameObject);
                return;
            }

            for (int i = 0; i < shards.Length; i++)
            {
                shards[i].velocity += Vector3.down * (gravity * Time.deltaTime);
                shards[i].transform.position += shards[i].velocity * Time.deltaTime;

                Color c = shards[i].renderer.color;
                c.a = 1f - t;
                shards[i].renderer.color = c;
            }
        }
    }
}
