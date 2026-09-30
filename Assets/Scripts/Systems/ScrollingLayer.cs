using UnityEngine;

namespace Odisseia.Systems
{
    /// <summary>
    /// Desliza uma faixa que ladrilha, no tempo. É o que faz vento, nuvem e água parecerem em
    /// movimento sem folha de animação e sem sistema de partículas.
    ///
    /// **Não é um segundo <see cref="ParallaxLayer"/>.** Os dois movem a camada, mas por
    /// motivos opostos: o parallax responde à CÂMERA e é o que cria profundidade; este responde
    /// ao TEMPO e é o que cria movimento próprio. Uma fase de vento precisa dos dois, e uma
    /// camada pode carregar os dois componentes ao mesmo tempo.
    ///
    /// Custa um <c>transform.position</c> por quadro e nenhum GameObject por partícula — que é
    /// o que o orçamento de WebGL e mobile pede.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class ScrollingLayer : MonoBehaviour
    {
        [Tooltip("Unidades por segundo. Negativo desliza para a esquerda.")]
        [SerializeField] private float speedX = -2f;

        [Tooltip("Distância após a qual a camada volta ao começo. Zero = largura do sprite.")]
        [SerializeField] private float wrapDistance;

        private float periodo;
        private float andado;
        private float aplicado;

        private void Awake()
        {
            // O período é o do LADRILHO, não o da faixa inteira: a faixa é desenhada com o
            // sprite repetido, então voltar um ladrilho é indistinguível de não ter voltado.
            // Voltar a faixa inteira daria um salto visível a cada volta.
            var sr = GetComponent<SpriteRenderer>();
            periodo = wrapDistance > 0.001f ? wrapDistance
                : sr.sprite != null ? sr.sprite.bounds.size.x : 1f;
        }

        /// <summary>
        /// Roda em LateUpdate, depois do <see cref="ParallaxLayer"/>, e aplica o deslocamento
        /// como um DELTA sobre o que ele deixou. Guardar o valor já aplicado e subtraí-lo é o
        /// que permite os dois componentes conviverem no mesmo objeto sem um sobrescrever o
        /// outro — escrever a posição absoluta aqui apagaria o parallax.
        /// </summary>
        private void LateUpdate()
        {
            if (periodo <= 0.001f)
            {
                return;
            }

            andado = Mathf.Repeat(andado + speedX * Time.deltaTime, periodo);
            transform.position += new Vector3(andado - aplicado, 0f, 0f);
            aplicado = andado;
        }
    }
}
