using UnityEngine;
using Odisseia.Systems;

namespace Odisseia.Levels
{
    /// <summary>
    /// Limites da fase, em unidades de mundo: paredes nas pontas, enquadramento da
    /// câmera e uma rede de segurança caso o jogador saia mesmo assim.
    ///
    /// Tudo aqui é medido em coordenadas do mundo, nunca em pixels de tela. O jogo roda
    /// em navegador de desktop e de celular, com resoluções e proporções diferentes; um
    /// limite derivado de <c>Screen.width</c> mudaria de lugar conforme o aparelho, e a
    /// fase teria um tamanho diferente para cada jogador.
    ///
    /// As três camadas existem por motivos diferentes e não se substituem:
    ///
    ///   parede   -> impede de sair (colisão normal, o jogador simplesmente para)
    ///   câmera   -> impede de MOSTRAR o lado de fora
    ///   segurança-> conserta o que passou (bug, empurrão, física maluca)
    ///
    /// A rede de segurança é a última linha, não a primeira: se ela disparar em jogo
    /// normal, o buraco está nas paredes e é lá que se conserta.
    /// </summary>
    public class LevelBounds : MonoBehaviour
    {
        [Header("Área jogável (unidades de mundo)")]
        [SerializeField] private float playableMinX = -8f;
        [SerializeField] private float playableMaxX = 341f;

        [Header("Paredes")]
        [Tooltip("Colisores das pontas. Ficam fora do caminho e só barram o jogador.")]
        [SerializeField] private Collider2D leftWall;
        [SerializeField] private Collider2D rightWall;

        [Header("Câmera")]
        [SerializeField] private CameraFollow cameraFollow;
        [Tooltip("O que a câmera pode mostrar. Costuma ser um pouco mais largo que a " +
                 "área jogável, para caber a cena final.")]
        [SerializeField] private float cameraMinX = -10f;
        [SerializeField] private float cameraMaxX = 360f;
        [SerializeField] private float cameraMinY = -6f;
        [SerializeField] private float cameraMaxY = 8f;

        [Header("Rede de segurança")]
        [SerializeField] private Transform player;
        [Tooltip("Quanto o jogador pode passar do limite antes de ser resgatado.")]
        [SerializeField] private float rescueMargin = 6f;
        [Tooltip("Abaixo disto o jogador caiu do mundo.")]
        [SerializeField] private float rescueMinY = -20f;
        [Tooltip("Para onde voltar quando não há checkpoint ativo.")]
        [SerializeField] private Transform spawnPoint;

        private Rigidbody2D playerBody;

        private void Awake()
        {
            if (player != null)
            {
                playerBody = player.GetComponent<Rigidbody2D>();
            }
        }

        private void Start()
        {
            ApplyCameraBounds();
        }

        /// <summary>
        /// Prende o enquadramento à fase. Sem isto a câmera continua seguindo Odisseu
        /// até o fim e mostra o vazio depois da última parede — o jogador vê metade da
        /// tela sem mundo e conclui que caiu para fora.
        /// </summary>
        private void ApplyCameraBounds()
        {
            cameraFollow?.SetBounds(
                new Vector2(cameraMinX, cameraMinY),
                new Vector2(cameraMaxX, cameraMaxY));
        }

        private void LateUpdate()
        {
            if (player == null)
            {
                return;
            }

            Vector3 posicao = player.position;
            bool foraDoEixoX = posicao.x < playableMinX - rescueMargin
                               || posicao.x > playableMaxX + rescueMargin;
            bool caiu = posicao.y < rescueMinY;

            if (foraDoEixoX || caiu)
            {
                Resgatar(foraDoEixoX ? "saiu pela lateral" : "caiu do mundo");
            }
        }

        /// <summary>
        /// Devolve o jogador ao último ponto válido. Usa o checkpoint que a fase já
        /// registra — não existe segundo sistema de save aqui, e o resgate não custa
        /// vida: quem errou foi o mundo, não o jogador.
        /// </summary>
        private void Resgatar(string motivo)
        {
            Vector3 destino = CheckpointManager.HasCheckpoint
                ? CheckpointManager.LastCheckpointPosition
                : (spawnPoint != null ? spawnPoint.position : new Vector3(playableMinX + 4f, 0f, 0f));

            PrologueTrace.Log("REDE DE SEGURANCA (" + motivo + "): x=" +
                player.position.x.ToString("0.0") + " -> " + destino.x.ToString("0.0"));

            if (playerBody != null)
            {
                playerBody.linearVelocity = Vector2.zero;
            }

            player.position = destino;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.5f, 0.2f);
            Gizmos.DrawLine(new Vector3(playableMinX, -10f), new Vector3(playableMinX, 10f));
            Gizmos.DrawLine(new Vector3(playableMaxX, -10f), new Vector3(playableMaxX, 10f));

            Gizmos.color = new Color(0.3f, 0.7f, 1f);
            Gizmos.DrawLine(new Vector3(cameraMinX, cameraMinY), new Vector3(cameraMaxX, cameraMinY));
            Gizmos.DrawLine(new Vector3(cameraMinX, cameraMaxY), new Vector3(cameraMaxX, cameraMaxY));
        }
    }
}
