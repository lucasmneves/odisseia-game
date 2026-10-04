using UnityEngine;
using Odisseia.Systems;

namespace Odisseia.Player
{
    /// <summary>
    /// Poeira nos pés ao pular e ao pousar (PXL-005 e o pouso do Polish Pass 02). Só OBSERVA o
    /// <see cref="PlayerController"/> — lê <c>IsGrounded</c> e a velocidade vertical, não escreve nada nele nem no
    /// Rigidbody — e toca a folha com o <see cref="VfxSheet"/>. Tirar o componente devolve o jogo exatamente ao que era.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class PlayerDustFx : MonoBehaviour
    {
        [Tooltip("Queda mínima (velocidade vertical no ar, para baixo) para o pouso levantar poeira. Abaixo disso é um degrau.")]
        [SerializeField] private float quedaMinima = 4f;
        [Tooltip("Velocidade de subida a partir da qual a saída do chão conta como pulo (e não como cair de uma borda).")]
        [SerializeField] private float subidaDoPulo = 2f;
        [Tooltip("Ordem de desenho: 1 põe a poeira na frente dos pés (o Odisseu está na 0).")]
        [SerializeField] private int ordem = 1;

        private PlayerController controller;
        private Rigidbody2D rb;
        private bool estavaNoChao = true;
        private float quedaNoAr;

        private void Awake()
        {
            controller = GetComponent<PlayerController>();
            rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            if (controller == null || rb == null)
            {
                return;
            }

            bool noChao = controller.IsGrounded;
            float vy = rb.linearVelocity.y;

            if (!noChao)
            {
                quedaNoAr = Mathf.Min(quedaNoAr, vy);
            }

            if (estavaNoChao && !noChao && vy > subidaDoPulo)
            {
                VfxSheet.Play("fx_jump_dust", transform.position, false, 14f, ordem);
            }
            else if (!estavaNoChao && noChao)
            {
                if (quedaNoAr < -quedaMinima)
                {
                    VfxSheet.Play("fx_land_dust", transform.position, false, 14f, ordem);
                }

                quedaNoAr = 0f;
            }

            estavaNoChao = noChao;
        }
    }
}
