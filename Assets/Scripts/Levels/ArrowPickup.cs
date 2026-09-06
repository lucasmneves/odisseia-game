using UnityEngine;
using Odisseia.Core;
using Odisseia.Player;
using Odisseia.Systems;

namespace Odisseia.Levels
{
    /// <summary>
    /// Repõe flechas do arco. Segue o mesmo padrão dos outros pickups da fase
    /// (trigger + flutuação simples), mas não mexe no <c>CollectibleCounter</c> —
    /// munição não é pontuação.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ArrowPickup : MonoBehaviour
    {
        [Header("Munição")]
        [SerializeField] private int amount = 5;

        [Header("Animação simples")]
        [SerializeField] private float bobHeight = 0.15f;
        [SerializeField] private float bobSpeed = 2f;

        private Vector3 startPosition;
        private bool collected;

        /// <summary>
        /// Flecha cravada no cenário, montada em tempo de execução por <c>Arrow</c>. Não
        /// flutua: ela está espetada em algo, e flutuar denunciaria que é um pickup solto.
        /// </summary>
        private bool fincada;

        private void Awake()
        {
            startPosition = transform.position;
            GetComponent<Collider2D>().isTrigger = true;
        }

        private void Update()
        {
            // `fincada` sai antes de tudo: a linha abaixo escreve position em todo quadro, e
            // numa flecha filha de plataforma móvel isso a prenderia ao ponto do impacto
            // enquanto a plataforma anda embaixo dela.
            if (collected || fincada)
            {
                return;
            }

            float offset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = startPosition + Vector3.up * offset;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Recolher(other);
        }

        /// <summary>
        /// Também em Stay: quem chega com a aljava cheia não dispara Enter de novo ao esvaziá-la
        /// parado ali, e a flecha ficaria intocável a um passo do jogador.
        /// </summary>
        private void OnTriggerStay2D(Collider2D other)
        {
            Recolher(other);
        }

        /// <summary>
        /// Converte esta flecha, já cravada, em munição a recolher: sem flutuação e valendo a
        /// única flecha que foi gasta para atirá-la.
        /// </summary>
        public void ConfigurarComoFlechaFincada()
        {
            fincada = true;
            amount = 1;
        }

        private void Recolher(Collider2D other)
        {
            if (collected || !other.CompareTag("Player"))
            {
                return;
            }

            if (!other.TryGetComponent(out PlayerBow bow))
            {
                return;
            }

            // Aljava cheia: o pickup fica onde está para ser pego mais tarde, em vez
            // de sumir sem dar nada.
            int added = bow.AddArrows(amount);
            if (added <= 0)
            {
                return;
            }

            collected = true;

            Sprite sprite = GameAssets.Instance != null ? GameAssets.Instance.PlaceholderSprite : null;
            VfxBurst.Spawn(sprite, transform.position, new Color(0.85f, 0.75f, 0.45f), 6, 2.2f, 0.3f);
            AudioManager.PlayCollect();

            Destroy(gameObject);
        }
    }
}
