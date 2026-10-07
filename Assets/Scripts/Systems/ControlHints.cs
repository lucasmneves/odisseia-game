using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace Odisseia.Systems
{
    /// <summary>
    /// Texto de instrução de tutorial que muda com o contexto do jogador.
    ///
    /// Existe porque a mesma dica precisa dizer coisas diferentes em cada plataforma:
    /// no desktop o jogador procura uma TECLA (que ainda por cima é remapeável), no
    /// mobile ele procura um BOTÃO na tela. Sem isto o tutorial mentiria para metade
    /// dos jogadores.
    ///
    /// Não há tabela de teclas nem de botões escrita aqui: o rótulo do desktop sai do
    /// <see cref="KeyRebindService"/>, que é quem sabe o binding atual; o do controle sai
    /// do binding de gamepad da própria ação, traduzido para a grafia da família
    /// (<see cref="PadControls"/>); e o do mobile é o mesmo texto estampado nos botões
    /// criados pelo <see cref="Odisseia.UI.MobileControlsRoot"/> (<see cref="TouchKey"/>).
    /// </summary>
    public static class ControlHints
    {
        /// <summary>
        /// Chave da tabela de tradução do rótulo de cada botão de toque. O
        /// MobileControlsRoot estampa o botão pela MESMA chave, então a dica e o botão
        /// nunca divergem — nem de idioma ("Toque em PULO", não "Toque em JUMP").
        /// </summary>
        public static string TouchKey(string actionName) => "ctrl.touch." + actionName.ToLowerInvariant();

        /// <summary>
        /// Controles do gamepad (nome do control path do Input System) na grafia de cada
        /// família: Xbox, PlayStation e Generic.
        ///
        /// O Generic mostra as duas — um controle que o navegador não identificou não
        /// justifica arriscar o símbolo errado, e "A/✕" é legível para quem tem
        /// qualquer um dos dois na mão. O direcional é igual nas duas famílias e vem
        /// traduzido (<see cref="DpadKeys"/>).
        /// </summary>
        private static readonly Dictionary<string, (string xbox, string playstation)> PadControls =
            new Dictionary<string, (string, string)>
            {
                { "buttonSouth", ("A", "✕") },
                { "buttonEast", ("B", "○") },
                { "buttonWest", ("X", "□") },
                { "buttonNorth", ("Y", "△") },
                { "leftTrigger", ("LT", "L2") },
                { "rightTrigger", ("RT", "R2") },
                { "leftShoulder", ("LB", "L1") },
                { "rightShoulder", ("RB", "R1") },
                { "start", ("Menu", "Options") },
                { "select", ("View", "Create") },
                { "leftStickPress", ("LS", "L3") },
                { "rightStickPress", ("RS", "R3") },
            };

        private static readonly Dictionary<string, string> DpadKeys = new Dictionary<string, string>
        {
            { "dpad/up", "ctrl.gamepad.dpadUp" },
            { "dpad/down", "ctrl.gamepad.dpadDown" },
            { "dpad/left", "ctrl.gamepad.dpadLeft" },
            { "dpad/right", "ctrl.gamepad.dpadRight" },
        };

        /// <summary>
        /// "Cancelar/voltar" dos menus não é uma ação do asset: as telas leem Esc e o botão
        /// Leste direto (OptionsMenu, SettingsScreen, MenuNavigator). Mesmos controles aqui.
        /// </summary>
        public const string Cancel = "Cancel";
        private const string CancelKey = "<Keyboard>/escape";
        private const string CancelPad = "<Gamepad>/buttonEast";

        /// <summary>Como o jogador aciona esta ação agora: "Z" no teclado, "X" no controle, "ATQ" no toque.</summary>
        public static string Button(string actionName)
        {
            if (string.IsNullOrEmpty(actionName))
            {
                return string.Empty;
            }

            switch (InputDeviceTracker.Current)
            {
                case InputDeviceKind.Gamepad:
                    return GamepadLabel(actionName);
                case InputDeviceKind.Touch:
                    return TouchLabel(actionName);
                default:
                    return KeyLabel(actionName);
            }
        }

        private static string TouchLabel(string actionName)
        {
            if (actionName == "Move")
            {
                return "◄ ►";
            }

            string key = TouchKey(actionName);
            if (Localization.Has(key))
            {
                return Localization.Get(key);
            }

            // Sem botão de toque para a ação (Crouch, Sprint, Cancel...): a tecla é o melhor que se tem.
            return KeyLabel(actionName);
        }

        /// <summary>
        /// Rótulo do controle, lido do binding de gamepad da ação. O analógico é a única
        /// ação sem símbolo universal, então vira texto traduzido em vez de um desenho
        /// que ninguém reconheceria.
        /// </summary>
        private static string GamepadLabel(string actionName)
        {
            if (actionName == "Move")
            {
                return Localization.Has("ctrl.gamepad.move")
                    ? Localization.Get("ctrl.gamepad.move")
                    : "Left Stick";
            }

            string path = actionName == Cancel ? CancelPad : FirstPath(FindAction(actionName), "<Gamepad>");
            return path != null ? PadLabel(path) : actionName;
        }

        /// <summary>"&lt;Gamepad&gt;/buttonSouth" → "A", "✕" ou "A/✕", conforme a família identificada.</summary>
        public static string PadLabel(string path)
        {
            int barra = path.IndexOf('/');
            string control = barra >= 0 ? path.Substring(barra + 1) : path;

            if (DpadKeys.TryGetValue(control, out string dpad))
            {
                return Localization.Get(dpad);
            }

            if (control.StartsWith("leftStick"))
            {
                return Localization.Get("ctrl.gamepad.move");
            }

            if (!PadControls.TryGetValue(control, out var rotulos))
            {
                return control;
            }

            switch (InputDeviceTracker.Family)
            {
                case GamepadFamily.Xbox:
                    return rotulos.xbox;
                case GamepadFamily.PlayStation:
                    return rotulos.playstation;
                default:
                    return rotulos.xbox + "/" + rotulos.playstation;
            }
        }

        /// <summary>
        /// Dica pronta para a tela. A chave base é a versão de teclado; havendo uma
        /// entrada com o sufixo ".mobile", ela vale no toque — é o que permite trocar o
        /// verbo ("Pressione" vira "Toque") e não só o nome do botão.
        /// </summary>
        public static string Instruction(string key, params string[] actionNames)
        {
            string chave = key;

            // A variante ".mobile" troca o VERBO ("toque em" no lugar de "pressione"),
            // então vale só para o toque. No controle a frase de teclado serve: o que
            // muda é o nome do botão, e disso o Button() já cuida.
            if (InputDeviceTracker.Current == InputDeviceKind.Touch
                && Localization.Has(key + ".mobile"))
            {
                chave = key + ".mobile";
            }

            if (actionNames == null || actionNames.Length == 0)
            {
                return Localization.Get(chave);
            }

            var rotulos = new object[actionNames.Length];
            for (int i = 0; i < actionNames.Length; i++)
            {
                rotulos[i] = Button(actionNames[i]);
            }

            return Localization.Get(chave, rotulos);
        }

        /// <summary>A ação em qualquer mapa do asset ("Jump" no Player, "Skip" no Dialogue).</summary>
        private static InputAction FindAction(string actionName)
        {
            InputActionAsset asset = KeyRebindService.Asset;
            return asset != null ? asset.FindAction(actionName, throwIfNotFound: false) : null;
        }

        /// <summary>Primeiro binding da ação no dispositivo dado (já com override), inclusive partes de composto.</summary>
        private static string FirstPath(InputAction action, string device)
        {
            if (action == null)
            {
                return null;
            }

            foreach (InputBinding binding in action.bindings)
            {
                if (!binding.isComposite && binding.effectivePath.StartsWith(device))
                {
                    return binding.effectivePath;
                }
            }

            return null;
        }

        /// <summary>
        /// Tecla atual da ação, já com remapeamento aplicado. O Move é um composto: não
        /// existe "a tecla do Move", então as duas metades entram juntas ("A/D").
        /// </summary>
        private static string KeyLabel(string actionName)
        {
            if (actionName == Cancel)
            {
                return KeyRebindService.GetPathDisplayString(CancelKey);
            }

            InputAction action = FindAction(actionName);

            if (action == null)
            {
                return actionName;
            }

            string negativa = null;
            string positiva = null;

            for (int i = 0; i < action.bindings.Count; i++)
            {
                InputBinding binding = action.bindings[i];

                if (binding.isComposite || !binding.effectivePath.StartsWith("<Keyboard>"))
                {
                    continue;
                }

                if (!binding.isPartOfComposite)
                {
                    return KeyRebindService.GetDisplayString(action, i);
                }

                // O asset grafa "Negative"/"Positive": comparar sem caixa (antes a dica de teclado saía "Move").
                if (string.Equals(binding.name, "negative", System.StringComparison.OrdinalIgnoreCase) && negativa == null)
                {
                    negativa = KeyRebindService.GetDisplayString(action, i);
                }
                else if (string.Equals(binding.name, "positive", System.StringComparison.OrdinalIgnoreCase) && positiva == null)
                {
                    positiva = KeyRebindService.GetDisplayString(action, i);
                }
            }

            if (negativa != null && positiva != null)
            {
                return negativa + "/" + positiva;
            }

            return negativa ?? positiva ?? actionName;
        }
    }
}
