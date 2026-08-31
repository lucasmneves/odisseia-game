using UnityEngine;

namespace Odisseia.Levels
{
    /// <summary>
    /// Rastro de diagnóstico do prólogo, impresso no console do navegador (F12).
    ///
    /// Existe porque o prólogo tem uma classe de falha que nenhuma checagem estática
    /// pega: a cena está certa, a fiação está certa, e mesmo assim o jogador não
    /// consegue avançar. O rastro responde as três perguntas na ordem — o jogador
    /// CHEGOU no NPC? o botão CHEGOU no jogo? a fala TERMINOU? — e cada resposta
    /// elimina metade das causas possíveis.
    ///
    /// Desligue <see cref="Ligado"/> quando o prólogo estiver estável; as chamadas
    /// somem do build junto com ele.
    /// </summary>
    public static class PrologueTrace
    {
        public const bool Ligado = true;

        public static void Log(string mensagem)
        {
            if (Ligado)
            {
                Debug.Log("[Prologo] " + mensagem);
            }
        }

        /// <summary>Posição do jogador, para localizar o relato no mapa da fase.</summary>
        public static string Onde(Component quem)
        {
            if (quem == null)
            {
                return string.Empty;
            }

            return " (x=" + quem.transform.position.x.ToString("0.0") + ")";
        }
    }
}
