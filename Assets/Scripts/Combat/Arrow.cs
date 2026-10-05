using UnityEngine;
using Odisseia.Core;
using Odisseia.Enemies;
using Odisseia.Levels;
using Odisseia.Systems;

namespace Odisseia.Combat
{
    /// <summary>
    /// Projétil do arco. Viaja em linha reta na direção do disparo e some ao acertar
    /// um alvo, bater no cenário ou esgotar o tempo de vida — nunca fica órfão em cena,
    /// o que importa no WebGL.
    ///
    /// O dano passa pelo <see cref="HealthSystem"/> normal do alvo; não existe sistema
    /// de vida paralelo aqui.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Arrow : MonoBehaviour
    {
        [SerializeField] private float speed = 15f;
        [SerializeField] private int damage = 35;
        [SerializeField] private float lifetime = 3f;
        [Tooltip("Camadas que a flecha pode atingir e causar dano.")]
        [SerializeField] private LayerMask targetLayers;
        [Tooltip("Camadas que apenas param a flecha (chão, paredes, plataformas).")]
        [SerializeField] private LayerMask obstacleLayers;

        /// <summary>
        /// Raio do gatilho de coleta da flecha fincada. A haste tem 0,7 x 0,2 un: encostar nela
        /// exigiria mira. Este valor é "chegar perto", medido contra a largura do Odisseu.
        /// </summary>
        private const float RaioDeColeta = 0.75f;

        private Rigidbody2D rb;
        private bool consumed;
        private bool impactoMostrado;

        /// <summary>Fincada num alvo parado: parou de voar e virou munição a recolher.</summary>
        private bool fincada;

        /// <summary>Tempo de voo que resta. Zera e some; congela ao fincar.</summary>
        private float restante;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;

            Collider2D col = GetComponent<Collider2D>();
            col.isTrigger = true;
        }

        /// <summary>
        /// Configura e lança a flecha. Os parâmetros vêm do <c>PlayerBow</c>, para que
        /// balanceamento fique num lugar só em vez de espalhado por prefab e script.
        /// </summary>
        public void Launch(Vector2 direction, float arrowSpeed, int arrowDamage, float arrowLifetime,
            LayerMask targets, LayerMask obstacles)
        {
            speed = arrowSpeed;
            damage = arrowDamage;
            lifetime = arrowLifetime;
            targetLayers = targets;
            obstacleLayers = obstacles;

            Vector2 dir = direction.sqrMagnitude < 0.0001f ? Vector2.right : direction.normalized;

            if (rb == null)
            {
                rb = GetComponent<Rigidbody2D>();
            }

            rb.linearVelocity = dir * speed;

            // Aponta a ponta da flecha para onde ela viaja.
            transform.right = dir;

            // Rede de segurança: mesmo sem acertar nada, a flecha se remove sozinha.
            //
            // O prazo é contado em Update, e não por Destroy(gameObject, lifetime), porque uma
            // remoção agendada não pode ser cancelada — e uma flecha fincada tem de PARAR de
            // contar, senão desaparece do alvo alguns segundos depois de acertar.
            restante = lifetime;
        }

        private void Update()
        {
            if (fincada || consumed || restante <= 0f)
            {
                return;
            }

            restante -= Time.deltaTime;
            if (restante <= 0f)
            {
                consumed = true;
                Vanish();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (consumed)
            {
                return;
            }

            int otherLayer = 1 << other.gameObject.layer;

            if ((targetLayers.value & otherLayer) != 0 && other.TryGetComponent(out HealthSystem health))
            {
                consumed = true;
                Impacto();
                health.TakeDamage(damage, new DamageInfo(transform.position));

                // Em inimigo a flecha some; em alvo parado ela fica fincada e pode ser
                // recolhida. Um inimigo anda, morre e é destruído — uma flecha presa nele
                // sairia andando pela fase e sumiria junto com ele, sem o jogador poder pegar.
                if (EhInimigo(other))
                {
                    Vanish();
                }
                else
                {
                    Fincar(other.transform);
                }

                return;
            }

            if ((obstacleLayers.value & otherLayer) != 0)
            {
                consumed = true;
                Impacto();
                Fincar(other.transform);
            }
        }

        /// <summary>
        /// Inimigo é o que se move e morre. A distinção não sai da layer — boneco de treino e
        /// inimigo compartilham a layer Enemy e o HealthSystem, de propósito, para a espada e a
        /// flecha acertarem os dois do mesmo jeito. O que separa é o componente de controle.
        /// </summary>
        private static bool EhInimigo(Collider2D alvo)
        {
            return alvo.GetComponentInParent<EnemyController>() != null
                || alvo.GetComponentInParent<BossController>() != null;
        }

        /// <summary>
        /// Crava a flecha onde ela acertou e a transforma em munição de volta.
        ///
        /// Vira filha do que acertou quando dá — ver <see cref="EscalaUniforme"/> — para
        /// acompanhar plataforma que se move em vez de ficar pendurada no ar.
        ///
        /// O recolhimento reaproveita o <see cref="ArrowPickup"/> que a fase já usa, em vez de
        /// um segundo caminho para a mesma coisa: ele já sabe conversar com a aljava, respeitar
        /// aljava cheia e tocar o som de coleta.
        /// </summary>
        private void Fincar(Transform alvo)
        {
            fincada = true;

            rb.linearVelocity = Vector2.zero;

            // Cinemático, e não `simulated = false`: desligar o corpo tiraria os colliders da
            // simulação junto, e a flecha fincada nunca mais seria detectada para a coleta.
            rb.bodyType = RigidbodyType2D.Kinematic;

            if (EscalaUniforme(alvo))
            {
                transform.SetParent(alvo, true);
            }

            // Alcance de coleta: encostar na haste de 0,7 x 0,2 un exigiria mira. "Chegar
            // perto" é um círculo generoso em volta dela.
            var alcance = gameObject.AddComponent<CircleCollider2D>();
            alcance.radius = RaioDeColeta;
            alcance.isTrigger = true;

            var pickup = gameObject.AddComponent<ArrowPickup>();
            pickup.ConfigurarComoFlechaFincada();
        }

        /// <summary>
        /// A flecha só vira filha de quem tem escala uniforme.
        ///
        /// Virar filha é o que a faz acompanhar uma plataforma que anda. Mas o chão e as
        /// paredes da fase são sprites esticados — um piso com escala (20, 1). Uma flecha
        /// girada dentro de um pai assim não é só espremida: ela CISALHA, porque a rotação
        /// dela entra entre a escala do pai e a do filho. Cenário parado não precisa de pai
        /// nenhum; então, quando a escala não é uniforme, a flecha fica onde caiu, no mundo.
        /// </summary>
        private static bool EscalaUniforme(Transform alvo)
        {
            if (alvo == null)
            {
                return false;
            }

            Vector3 escala = alvo.lossyScale;
            return Mathf.Abs(escala.x - escala.y) < 0.01f && Mathf.Abs(escala.x - escala.z) < 0.01f;
        }

        /// <summary>Lascas no ponto do acerto, viradas para o lado de onde a flecha veio. Só apresentação.</summary>
        private void Impacto()
        {
            impactoMostrado = VfxSheet.Play("fx_arrow_impact", transform.position, rb != null && rb.linearVelocity.x < 0f);
        }

        private void Vanish()
        {
            // Depois de um acerto as lascas já saíram; os quadradinhos ficam só para a flecha que some no ar.
            if (!impactoMostrado)
            {
                Sprite sprite = GameAssets.Instance != null ? GameAssets.Instance.PlaceholderSprite : null;
                VfxBurst.Spawn(sprite, transform.position, new Color(0.9f, 0.85f, 0.6f), 3, 1.8f, 0.18f);
            }
            Destroy(gameObject);
        }
    }
}
