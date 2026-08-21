using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Odisseia.Core;

namespace Odisseia.Systems
{
    /// <summary>
    /// Remapeamento de teclas. Usa o mecanismo de binding overrides do próprio Input
    /// System — nenhuma camada de input paralela: as ações continuam sendo as mesmas,
    /// só o caminho do controle muda.
    ///
    /// Os overrides são gravados em PlayerPrefs sob uma chave PRÓPRIA, separada do
    /// save da campanha. É de propósito: "Novo Jogo" apaga o progresso
    /// (<see cref="SaveSystem.DeleteSave"/>) e seria péssimo perder os controles junto.
    /// </summary>
    public static class KeyRebindService
    {
        private const string PrefsKey = "Odisseia.Bindings";

        /// <summary>Disparado quando qualquer binding muda (rebind ou restauração).</summary>
        public static event Action BindingsChanged;

        private static bool loaded;

        /// <summary>Uma linha remapeável da tela de opções.</summary>
        public readonly struct Entry
        {
            public readonly InputAction Action;
            public readonly int BindingIndex;
            public readonly string Label;

            public Entry(InputAction action, int bindingIndex, string label)
            {
                Action = action;
                BindingIndex = bindingIndex;
                Label = label;
            }
        }

        /// <summary>Asset de controles, alcançado pelo catálogo carregado de Resources.</summary>
        public static InputActionAsset Asset =>
            GameAssets.Instance != null ? GameAssets.Instance.PlayerControls : null;

        /// <summary>
        /// Aplica os overrides antes da primeira cena, para o jogo já começar com as
        /// teclas do jogador — sem depender de nenhuma cena lembrar de chamar isto.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void AutoLoad()
        {
            EnsureLoaded();
        }

        /// <summary>
        /// Aplica os overrides salvos. Idempotente e barato — pode ser chamado de
        /// qualquer Awake sem coordenação entre cenas.
        /// </summary>
        public static void EnsureLoaded()
        {
            if (loaded)
            {
                return;
            }

            loaded = true;

            InputActionAsset asset = Asset;
            if (asset == null)
            {
                return;
            }

            string json = PlayerPrefs.GetString(PrefsKey, string.Empty);
            if (!string.IsNullOrEmpty(json))
            {
                asset.LoadBindingOverridesFromJson(json);
            }
        }

        public static void Save()
        {
            InputActionAsset asset = Asset;
            if (asset == null)
            {
                return;
            }

            PlayerPrefs.SetString(PrefsKey, asset.SaveBindingOverridesAsJson());
            PlayerPrefs.Save();
        }

        /// <summary>Volta tudo ao padrão do asset.</summary>
        public static void ResetAll()
        {
            InputActionAsset asset = Asset;
            if (asset == null)
            {
                return;
            }

            asset.RemoveAllBindingOverrides();
            PlayerPrefs.DeleteKey(PrefsKey);
            PlayerPrefs.Save();
            BindingsChanged?.Invoke();
        }

        /// <summary>
        /// Linhas remapeáveis do mapa "Player", na ordem em que fazem sentido para o
        /// jogador. Bindings compostos (o eixo do Move) entram pelas suas PARTES —
        /// a raiz do composto não é uma tecla.
        /// </summary>
        public static List<Entry> GetEntries()
        {
            var entries = new List<Entry>();
            InputActionAsset asset = Asset;
            InputActionMap map = asset != null ? asset.FindActionMap("Player", throwIfNotFound: false) : null;
            if (map == null)
            {
                return entries;
            }

            // Pause fica de fora: Esc também serve para pular diálogo (mapa "Dialogue"),
            // e remapear só um dos dois deixaria os dois fora de sincronia.
            string[] order = { "Move", "Jump", "Attack", "Shield", "Bow", "Interact" };

            foreach (string actionName in order)
            {
                InputAction action = map.FindAction(actionName);
                if (action == null)
                {
                    continue;
                }

                int alternate = 0;
                for (int i = 0; i < action.bindings.Count; i++)
                {
                    InputBinding binding = action.bindings[i];

                    if (binding.isComposite)
                    {
                        continue;
                    }

                    if (!binding.effectivePath.StartsWith("<Keyboard>"))
                    {
                        continue;
                    }

                    string label = DescribeBinding(actionName, binding, alternate);
                    entries.Add(new Entry(action, i, label));
                    alternate++;
                }
            }

            return entries;
        }

        private static string DescribeBinding(string actionName, InputBinding binding, int alternate)
        {
            string baseLabel;

            if (binding.isPartOfComposite)
            {
                baseLabel = binding.name switch
                {
                    "negative" => "Mover para a esquerda",
                    "positive" => "Mover para a direita",
                    _ => $"{Translate(actionName)} ({binding.name})",
                };
                // Move tem dois compostos (WASD e setas); o segundo é o alternativo.
                return alternate >= 2 ? $"{baseLabel} (alt.)" : baseLabel;
            }

            baseLabel = Translate(actionName);
            return alternate >= 1 ? $"{baseLabel} (alt.)" : baseLabel;
        }

        private static string Translate(string actionName) => actionName switch
        {
            "Move" => "Mover",
            "Jump" => "Pular",
            "Attack" => "Atacar",
            "Shield" => "Defender",
            "Bow" => "Arco",
            "Interact" => "Interagir",
            _ => actionName,
        };

        /// <summary>Tecla atual de um binding, em texto legível (ex.: "J").</summary>
        public static string GetDisplayString(InputAction action, int bindingIndex)
        {
            if (action == null || bindingIndex < 0 || bindingIndex >= action.bindings.Count)
            {
                return "—";
            }

            string path = action.bindings[bindingIndex].effectivePath;
            if (string.IsNullOrEmpty(path))
            {
                return "—";
            }

            string display = InputControlPath.ToHumanReadableString(
                path, InputControlPath.HumanReadableStringOptions.OmitDevice);

            return string.IsNullOrEmpty(display) ? "—" : display.ToUpperInvariant();
        }

        /// <summary>
        /// Caminho de controle atualmente ligado a uma ação. Os botões de toque usam
        /// isto para continuarem apontando para a tecla certa depois de um remapeamento.
        /// </summary>
        public static string GetEffectivePath(string actionName, string fallback)
        {
            InputActionAsset asset = Asset;
            InputActionMap map = asset != null ? asset.FindActionMap("Player", throwIfNotFound: false) : null;
            InputAction action = map?.FindAction(actionName);

            if (action == null)
            {
                return fallback;
            }

            foreach (InputBinding binding in action.bindings)
            {
                if (binding.isComposite)
                {
                    continue;
                }

                if (binding.effectivePath.StartsWith("<Keyboard>"))
                {
                    return binding.effectivePath;
                }
            }

            return fallback;
        }

        /// <summary>
        /// Caminho de uma PARTE de composto (ex.: o "negative" do Move), para os botões
        /// de mover no toque.
        /// </summary>
        public static string GetCompositePartPath(string actionName, string partName, string fallback)
        {
            InputActionAsset asset = Asset;
            InputActionMap map = asset != null ? asset.FindActionMap("Player", throwIfNotFound: false) : null;
            InputAction action = map?.FindAction(actionName);

            if (action == null)
            {
                return fallback;
            }

            foreach (InputBinding binding in action.bindings)
            {
                if (binding.isPartOfComposite
                    && binding.name == partName
                    && binding.effectivePath.StartsWith("<Keyboard>"))
                {
                    return binding.effectivePath;
                }
            }

            return fallback;
        }

        /// <summary>
        /// Se <paramref name="path"/> já está em uso por outro binding do mapa, devolve
        /// o rótulo de quem usa. Serve para recusar o remapeamento em vez de deixar duas
        /// ações na mesma tecla.
        /// </summary>
        public static string FindConflict(string path, InputAction ignoreAction, int ignoreBindingIndex)
        {
            foreach (Entry entry in GetEntries())
            {
                if (entry.Action == ignoreAction && entry.BindingIndex == ignoreBindingIndex)
                {
                    continue;
                }

                if (entry.Action.bindings[entry.BindingIndex].effectivePath == path)
                {
                    return entry.Label;
                }
            }

            return null;
        }

        /// <summary>
        /// Inicia a captura da próxima tecla. O chamador recebe o resultado
        /// (<c>null</c> = sucesso, texto = motivo da recusa/cancelamento).
        /// </summary>
        public static InputActionRebindingExtensions.RebindingOperation StartRebind(
            InputAction action, int bindingIndex, Action<string> onFinish)
        {
            if (action == null)
            {
                onFinish?.Invoke("ação inválida");
                return null;
            }

            // A ação precisa estar desabilitada durante a captura.
            bool wasEnabled = action.enabled;
            action.Disable();

            return action.PerformInteractiveRebinding(bindingIndex)
                // Só teclado: o jogo não tem gamepad e o mouse é usado pelos menus.
                .WithControlsHavingToMatchPath("<Keyboard>")
                .WithCancelingThrough("<Keyboard>/escape")
                .OnCancel(operation =>
                {
                    operation.Dispose();
                    if (wasEnabled)
                    {
                        action.Enable();
                    }
                    onFinish?.Invoke("cancelado");
                })
                .OnComplete(operation =>
                {
                    string newPath = action.bindings[bindingIndex].effectivePath;
                    operation.Dispose();

                    string conflict = FindConflict(newPath, action, bindingIndex);
                    if (conflict != null)
                    {
                        // Desfaz: duas ações na mesma tecla quebrariam uma das duas.
                        action.RemoveBindingOverride(bindingIndex);
                        if (wasEnabled)
                        {
                            action.Enable();
                        }
                        onFinish?.Invoke($"tecla já usada por \"{conflict}\"");
                        return;
                    }

                    if (wasEnabled)
                    {
                        action.Enable();
                    }

                    Save();
                    BindingsChanged?.Invoke();
                    onFinish?.Invoke(null);
                })
                .Start();
        }
    }
}
