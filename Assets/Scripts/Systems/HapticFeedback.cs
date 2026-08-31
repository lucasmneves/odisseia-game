using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Odisseia.Systems
{
    /// <summary>
    /// Vibração curta no controle, quando houver controle e o jogador quiser.
    ///
    /// É acessório de propósito: nada no jogo depende dela. Sem gamepad, com a opção
    /// desligada, ou num navegador cuja Gamepad API não expõe vibração, as chamadas
    /// simplesmente não fazem nada — e o jogo segue igual. Essa é a única postura
    /// segura no WebGL, onde o suporte a haptics varia por navegador e por controle.
    ///
    /// Os motores são desligados por tempo, num objeto persistente: um controle que
    /// fica vibrando porque a cena trocou no meio do pulso é pior que não vibrar.
    /// </summary>
    public class HapticFeedback : MonoBehaviour
    {
        private static HapticFeedback instancia;
        private Coroutine pulso;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (instancia != null)
            {
                return;
            }

            var go = new GameObject("HapticFeedback");
            instancia = go.AddComponent<HapticFeedback>();
            DontDestroyOnLoad(go);
        }

        /// <summary>Toque leve: bloqueio com escudo, coleta.</summary>
        public static void Light() => Pulse(0.15f, 0.25f, 0.10f);

        /// <summary>Toque médio: dano recebido, evento importante.</summary>
        public static void Medium() => Pulse(0.35f, 0.55f, 0.18f);

        /// <summary>
        /// Aciona os motores por um tempo. Silencioso quando não há o que acionar —
        /// quem chama não precisa perguntar se existe controle.
        /// </summary>
        public static void Pulse(float baixa, float alta, float segundos)
        {
            if (!SettingsManager.Vibration || instancia == null)
            {
                return;
            }

            Gamepad pad = Gamepad.current;
            if (pad == null)
            {
                return;
            }

            if (instancia.pulso != null)
            {
                instancia.StopCoroutine(instancia.pulso);
            }

            instancia.pulso = instancia.StartCoroutine(instancia.Rotina(pad, baixa, alta, segundos));
        }

        /// <summary>Para os motores agora. Usado ao desligar a opção e ao sair do jogo.</summary>
        public static void Stop()
        {
            Gamepad.current?.SetMotorSpeeds(0f, 0f);
        }

        private IEnumerator Rotina(Gamepad pad, float baixa, float alta, float segundos)
        {
            pad.SetMotorSpeeds(baixa, alta);

            // Tempo não escalado: uma pausa no meio do pulso não pode deixar o controle
            // vibrando até o jogador despausar.
            yield return new WaitForSecondsRealtime(segundos);

            pad.SetMotorSpeeds(0f, 0f);
            pulso = null;
        }

        private void OnApplicationQuit()
        {
            Stop();
        }

        private void OnDisable()
        {
            Stop();
        }
    }
}
