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
    /// Não há tabela de teclas escrita aqui: o rótulo do desktop sai do
    /// <see cref="KeyRebindService"/>, que é quem sabe o binding atual, e o rótulo do
    /// mobile é o mesmo texto estampado nos botões criados pelo
    /// <see cref="Odisseia.UI.MobileControlsRoot"/>.
    /// </summary>
    public static class ControlHints
    {
        /// <summary>Rótulo estampado em cada botão de toque, na mesma grafia da UI.</summary>
        private static readonly Dictionary<string, string> TouchLabels = new Dictionary<string, string>
        {
            { "Move", "◄ ►" },
            { "Jump", "JUMP" },
            { "Attack", "ATK" },
            { "Shield", "DEF" },
            { "Bow", "BOW" },
            { "Interact", "USE" },
        };

        /// <summary>
        /// Botões de controle, na grafia de cada família.
        ///
        /// O Generic mostra as duas — um controle que o navegador não identificou não
        /// justifica arriscar o símbolo errado, e "A/✕" é legível para quem tem
        /// qualquer um dos dois na mão.
        /// </summary>
        private static readonly Dictionary<string, (string xbox, string playstation, string generico)> GamepadLabels =
            new Dictionary<string, (string, string, string)>
            {
                { "Jump", ("A", "✕", "A/✕") },
                { "Attack", ("X", "□", "X/□") },
                { "Shield", ("LT", "L2", "LT/L2") },
                { "Bow", ("RT", "R2", "RT/R2") },
                { "Interact", ("Y", "△", "Y/△") },
                { "Sprint", ("RB", "R1", "RB/R1") },
                { "Pause", ("Menu", "Options", "Menu/Options") },
            };

        /// <summary>Como o jogador aciona esta ação agora: "Z" no teclado, "X" no controle, "ATK" no toque.</summary>
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
                    return TouchLabels.TryGetValue(actionName, out string touch) ? touch : actionName;
                default:
                    return KeyLabel(actionName);
            }
        }

        /// <summary>
        /// Rótulo do controle. O analógico é a única ação sem símbolo universal, então
        /// vira texto traduzido em vez de um desenho que ninguém reconheceria.
        /// </summary>
        private static string GamepadLabel(string actionName)
        {
            if (actionName == "Move")
            {
                return Localization.Has("ctrl.gamepad.move")
                    ? Localization.Get("ctrl.gamepad.move")
                    : "Left Stick";
            }

            if (!GamepadLabels.TryGetValue(actionName, out var rotulos))
            {
                return actionName;
            }

            switch (InputDeviceTracker.Family)
            {
                case GamepadFamily.Xbox:
                    return rotulos.xbox;
                case GamepadFamily.PlayStation:
                    return rotulos.playstation;
                default:
                    return rotulos.generico;
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

        /// <summary>
        /// Tecla atual da ação, já com remapeamento aplicado. O Move é um composto: não
        /// existe "a tecla do Move", então as duas metades entram juntas ("A/D").
        /// </summary>
        private static string KeyLabel(string actionName)
        {
            InputActionAsset asset = KeyRebindService.Asset;
            InputActionMap map = asset != null ? asset.FindActionMap("Player", throwIfNotFound: false) : null;
            InputAction action = map?.FindAction(actionName);

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

                if (binding.name == "negative" && negativa == null)
                {
                    negativa = KeyRebindService.GetDisplayString(action, i);
                }
                else if (binding.name == "positive" && positiva == null)
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
