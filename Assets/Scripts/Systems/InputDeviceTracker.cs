using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Odisseia.Systems
{
    /// <summary>Como o jogador está comandando o jogo AGORA.</summary>
    public enum InputDeviceKind
    {
        Keyboard,
        Gamepad,
        Touch,
    }

    /// <summary>Família de botões de um controle, para a UI desenhar o símbolo certo.</summary>
    public enum GamepadFamily
    {
        /// <summary>Modelo não identificado. A UI mostra as duas grafias.</summary>
        Generic,
        Xbox,
        PlayStation,
    }

    /// <summary>
    /// Descobre qual dispositivo o jogador está usando e avisa quem mostra instruções.
    ///
    /// Isto NÃO é uma camada de input: nenhum comando passa por aqui. O gameplay
    /// continua lendo as mesmas <c>InputAction</c> de sempre e não sabe de onde veio o
    /// comando — o que este serviço decide é só a APRESENTAÇÃO ("aperte Z" ou
    /// "aperte X"). Misturar as duas coisas seria criar um segundo caminho de input.
    ///
    /// Troca pelo último uso, não pelo que está conectado: quem joga de teclado com um
    /// controle plugado na mesa continua vendo teclas até encostar no controle.
    /// </summary>
    public class InputDeviceTracker : MonoBehaviour
    {
        /// <summary>Movimento de analógico/gatilho abaixo disto é ruído, não intenção.</summary>
        private const float LimiarDeUso = 0.4f;

        private static InputDeviceTracker instancia;

        /// <summary>Controle cuja família já foi resolvida. Evita refazer a conta por frame.</summary>
        private static InputDevice padConhecido;

        /// <summary>Disparado quando o jogador troca de dispositivo (ou de modelo de controle).</summary>
        public static event Action Changed;

        public static InputDeviceKind Current { get; private set; } = InputDeviceKind.Keyboard;

        public static GamepadFamily Family { get; private set; } = GamepadFamily.Generic;

        public static bool UsingGamepad => Current == InputDeviceKind.Gamepad;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (instancia != null)
            {
                return;
            }

            var go = new GameObject("InputDeviceTracker");
            instancia = go.AddComponent<InputDeviceTracker>();
            DontDestroyOnLoad(go);
        }

        private void Awake()
        {
            // Num aparelho de toque o padrão é toque, senão a primeira dica da fase
            // sairia falando de teclas que não existem ali.
            Current = MobilePlatformDetector.IsMobile ? InputDeviceKind.Touch : InputDeviceKind.Keyboard;
            AtualizarFamilia();
        }

        private void Update()
        {
            if (GamepadEmUso())
            {
                Adotar(InputDeviceKind.Gamepad);
                return;
            }

            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            {
                Adotar(InputDeviceKind.Keyboard);
                return;
            }

            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                Adotar(InputDeviceKind.Touch);
            }
        }

        /// <summary>
        /// Só os controles que o jogo usa. Varrer <c>allControls</c> seria mais geral e
        /// custaria uma alocação por frame — e o jogo tem seis ações.
        /// </summary>
        private static bool GamepadEmUso()
        {
            Gamepad pad = Gamepad.current;
            if (pad == null)
            {
                return false;
            }

            if (pad.buttonSouth.wasPressedThisFrame || pad.buttonEast.wasPressedThisFrame
                || pad.buttonWest.wasPressedThisFrame || pad.buttonNorth.wasPressedThisFrame
                || pad.startButton.wasPressedThisFrame || pad.selectButton.wasPressedThisFrame
                || pad.dpad.left.wasPressedThisFrame || pad.dpad.right.wasPressedThisFrame
                || pad.dpad.up.wasPressedThisFrame || pad.dpad.down.wasPressedThisFrame)
            {
                return true;
            }

            return Mathf.Abs(pad.leftStick.x.ReadValue()) > LimiarDeUso
                   || Mathf.Abs(pad.leftStick.y.ReadValue()) > LimiarDeUso
                   || pad.leftTrigger.ReadValue() > LimiarDeUso
                   || pad.rightTrigger.ReadValue() > LimiarDeUso;
        }

        private static void Adotar(InputDeviceKind tipo)
        {
            GamepadFamily familiaAntes = Family;

            if (tipo == InputDeviceKind.Gamepad)
            {
                AtualizarFamilia();
            }

            if (Current == tipo && Family == familiaAntes)
            {
                return;
            }

            Current = tipo;
            Changed?.Invoke();
        }

        /// <summary>
        /// Descobre se o controle é Xbox ou PlayStation.
        ///
        /// Resolvido UMA vez por dispositivo e guardado. Antes isto rodava a cada frame
        /// em que o controle era usado, e bastava uma leitura ambígua num frame para o
        /// rótulo piscar entre "Y" e "Y/△" no meio de uma conversa.
        ///
        /// E <see cref="GamepadFamily.Generic"/> significa "não sei", não "mudou": se
        /// já havia uma resposta concreta e a nova é genérica, a antiga fica. Trocar
        /// conhecimento por ignorância é justamente o que produzia a inconsistência.
        ///
        /// No Editor e em builds nativos o Input System entrega uma subclasse
        /// (<c>XInputController</c>, <c>DualShockGamepad</c>) e a resposta é direta. No
        /// WebGL não: o navegador entrega um gamepad genérico pela Gamepad API, e o que
        /// sobra é o texto de identificação — daí a busca por nome e pelos códigos de
        /// fabricante (045e Microsoft, 054c Sony).
        /// </summary>
        private static void AtualizarFamilia()
        {
            Gamepad pad = Gamepad.current;

            if (pad == null || ReferenceEquals(pad, padConhecido))
            {
                return;
            }

            padConhecido = pad;
            GamepadFamily descoberta = Identificar(pad);

            // Só sobrescreve com uma resposta concreta.
            if (descoberta != GamepadFamily.Generic || Family == GamepadFamily.Generic)
            {
                Family = descoberta;
            }

            Debug.Log("[InputDeviceTracker] controle: " + Descrever(pad) +
                      " -> " + Family);
        }

        private static GamepadFamily Identificar(Gamepad pad)
        {
            if (pad is UnityEngine.InputSystem.DualShock.DualShockGamepad)
            {
                return GamepadFamily.PlayStation;
            }

            if (pad is UnityEngine.InputSystem.XInput.XInputController)
            {
                return GamepadFamily.Xbox;
            }

            string id = Descrever(pad).ToLowerInvariant();

            // Xbox primeiro: o texto do Chrome para um controle Xbox costuma trazer
            // "XInput STANDARD GAMEPAD", e "standard gamepad" sozinho não decide nada.
            if (id.Contains("xbox") || id.Contains("x-box") || id.Contains("xinput")
                || id.Contains("microsoft") || id.Contains("045e"))
            {
                return GamepadFamily.Xbox;
            }

            if (id.Contains("dualsense") || id.Contains("dualshock") || id.Contains("sony")
                || id.Contains("playstation") || id.Contains("054c")
                || id.Contains("09cc") || id.Contains("05c4") || id.Contains("0ce6"))
            {
                return GamepadFamily.PlayStation;
            }

            return GamepadFamily.Generic;
        }

        /// <summary>
        /// Tudo que o dispositivo diz sobre si. No WebGL o texto útil pode aparecer em
        /// qualquer um destes campos, dependendo do navegador — por isso olhamos todos
        /// em vez de escolher um.
        /// </summary>
        private static string Descrever(Gamepad pad)
        {
            UnityEngine.InputSystem.Layouts.InputDeviceDescription d = pad.description;
            return string.Join(" ", pad.name, pad.displayName, d.product, d.manufacturer,
                d.interfaceName, d.deviceClass, d.capabilities);
        }
    }
}
