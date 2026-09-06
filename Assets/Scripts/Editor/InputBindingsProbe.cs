using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using Odisseia.Core;
using Odisseia.Systems;

/// <summary>
/// Confere as teclas padrão do jogo lendo o asset de controles carregado em runtime —
/// não o arquivo .inputactions no disco.
///
/// A diferença importa: o <see cref="KeyRebindService"/> aplica overrides salvos por
/// cima do asset. Ler o arquivo diria o que está escrito lá; ler o asset em runtime diz
/// o que o jogador realmente vai apertar.
///
/// Unity.exe -batchmode -quit -projectPath . -executeMethod InputBindingsProbe.Check
/// </summary>
public static class InputBindingsProbe
{
    /// <summary>Ação → tecla esperada, no mapa Player.</summary>
    private static readonly (string acao, string tecla)[] Esperado =
    {
        ("Jump", "<Keyboard>/space"),
        ("Attack", "<Keyboard>/z"),
        ("Shield", "<Keyboard>/x"),
        ("Bow", "<Keyboard>/c"),
        ("Sprint", "<Keyboard>/leftShift"),
        ("Interact", "<Keyboard>/e"),
        ("Pause", "<Keyboard>/escape"),
    };

    [MenuItem("Odisseia/Conferir teclas padrao")]
    public static void Check()
    {
        bool falhou = false;

        // Sem overrides: a sonda mede o PADRÃO, não o que alguém remapeou nesta máquina.
        PlayerPrefs.DeleteKey("Odisseia.Bindings");
        PlayerPrefs.Save();

        var catalogo = AssetDatabase.LoadAssetAtPath<GameAssets>("Assets/Resources/GameAssets.asset");
        InputActionAsset asset = catalogo != null
            ? catalogo.PlayerControls
            : AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/ScriptableObjects/PlayerControls.inputactions");

        if (asset == null)
        {
            Debug.LogError("[Teclas] asset de controles não encontrado");
            EditorApplication.Exit(1);
            return;
        }

        asset.RemoveAllBindingOverrides();

        InputActionMap player = asset.FindActionMap("Player", throwIfNotFound: false);
        if (player == null)
        {
            Debug.LogError("[Teclas] mapa Player ausente");
            EditorApplication.Exit(1);
            return;
        }

        foreach ((string acao, string tecla) in Esperado)
        {
            InputAction action = player.FindAction(acao);
            if (action == null)
            {
                Debug.LogError($"[Teclas] ação ausente: {acao}");
                falhou = true;
                continue;
            }

            string obtido = action.bindings
                .Where(b => !b.isComposite && b.effectivePath.StartsWith("<Keyboard>"))
                .Select(b => b.effectivePath)
                .FirstOrDefault();

            string legivel = KeyRebindService.GetDisplayString(action,
                action.bindings.ToList().FindIndex(b => !b.isComposite && b.effectivePath == obtido));

            Debug.Log($"[Teclas] {acao,-10} {obtido,-22} ({legivel})");

            if (obtido != tecla)
            {
                Debug.LogError($"[Teclas] {acao}: esperado {tecla}, obtido {obtido}");
                falhou = true;
            }
        }

        // Duas ações na mesma tecla quebrariam uma das duas em silêncio.
        var teclas = player.bindings
            .Where(b => !b.isComposite && b.effectivePath.StartsWith("<Keyboard>"))
            .GroupBy(b => b.effectivePath)
            .Where(g => g.Count() > 1)
            .ToArray();

        foreach (var grupo in teclas)
        {
            Debug.LogError($"[Teclas] {grupo.Key} está em {grupo.Count()} ações do mapa Player");
            falhou = true;
        }

        // O "avançar diálogo" espelha o ataque de propósito: mesmo dedo, mesma tecla.
        InputActionMap dialogo = asset.FindActionMap("Dialogue", throwIfNotFound: false);
        InputAction avancar = dialogo?.FindAction("Advance");
        if (avancar != null)
        {
            string[] caminhos = avancar.bindings.Select(b => b.effectivePath).ToArray();
            Debug.Log("[Teclas] Dialogue/Advance: " + string.Join(", ", caminhos));

            if (!caminhos.Contains("<Keyboard>/z"))
            {
                Debug.LogError("[Teclas] avançar diálogo deixou de espelhar a tecla de ataque");
                falhou = true;
            }
        }

        Debug.Log(falhou ? "[Teclas] RESULTADO: FALHOU" : "[Teclas] RESULTADO: OK");
        EditorApplication.Exit(falhou ? 1 : 0);
    }
}
