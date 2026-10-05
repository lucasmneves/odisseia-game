using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using Odisseia.Player;

/// <summary>
/// Ajustes de projeto que o suporte a controle exige e que não cabem numa cena:
/// o componente de vibração no prefab do Player, e a conferência de que o asset de
/// Input Actions tem mesmo os caminhos de gamepad.
///
/// Unity.exe -batchmode -quit -projectPath . -executeMethod GamepadSetup.Run
/// ou pelo menu Odisseia &gt; Preparar suporte a controle.
///
/// Idempotente: rodar de novo não duplica nada.
/// </summary>
public static class GamepadSetup
{
    private const string PlayerPrefab = "Assets/Prefabs/Player.prefab";
    private const string ControlsPath = "Assets/ScriptableObjects/PlayerControls.inputactions";

    /// <summary>Ações do mapa Player e o caminho de gamepad que cada uma tem que ter.</summary>
    private static readonly (string acao, string caminho)[] Esperado =
    {
        ("Move", "<Gamepad>/leftStick/x"),
        ("Move", "<Gamepad>/dpad/left"),
        ("Move", "<Gamepad>/dpad/right"),
        ("Jump", "<Gamepad>/buttonSouth"),
        ("Attack", "<Gamepad>/buttonWest"),
        ("Shield", "<Gamepad>/leftTrigger"),
        ("Bow", "<Gamepad>/rightTrigger"),
        ("Interact", "<Gamepad>/buttonNorth"),
        ("Pause", "<Gamepad>/start"),
        ("Crouch", "<Gamepad>/dpad/down"),
        ("Sprint", "<Gamepad>/rightShoulder"),
    };

    private static readonly (string acao, string caminho)[] EsperadoDialogo =
    {
        ("Advance", "<Gamepad>/buttonSouth"),
        ("Skip", "<Gamepad>/buttonEast"),
    };

    private static int falhas;

    [MenuItem("Odisseia/Preparar suporte a controle")]
    public static void Run()
    {
        falhas = 0;

        AdicionarVibracaoAoPlayer();
        ConferirBindings();

        if (falhas == 0)
        {
            Debug.Log("[GamepadSetup] OK — Player com vibração e todos os caminhos de gamepad no lugar.");
            Encerrar(0);
            return;
        }

        Debug.LogError("[GamepadSetup] " + falhas + " problema(s).");
        Encerrar(1);
    }

    private static void Falhar(string mensagem)
    {
        Debug.LogError("[GamepadSetup] " + mensagem);
        falhas++;
    }

    /// <summary>
    /// Põe o <see cref="PlayerHaptics"/> no prefab, e não na instância de uma cena: o
    /// Player é o mesmo nas 16 fases, e vibração só na fase 1 seria pior que vibração
    /// nenhuma.
    /// </summary>
    private static void AdicionarVibracaoAoPlayer()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefab);
        if (prefab == null)
        {
            Falhar("Player.prefab não encontrado");
            return;
        }

        if (prefab.GetComponent<PlayerHaptics>() != null)
        {
            Debug.Log("[GamepadSetup] Player já tinha PlayerHaptics.");
            return;
        }

        GameObject raiz = PrefabUtility.LoadPrefabContents(PlayerPrefab);
        raiz.AddComponent<PlayerHaptics>();
        PrefabUtility.SaveAsPrefabAsset(raiz, PlayerPrefab);
        PrefabUtility.UnloadPrefabContents(raiz);

        Debug.Log("[GamepadSetup] PlayerHaptics adicionado ao Player.prefab.");
    }

    /// <summary>
    /// Confere caminho por caminho. O asset é JSON e foi editado fora do Editor, então
    /// vale confirmar que o Unity leu o que se esperava — um caminho digitado errado
    /// não dá erro de import, só uma ação que nunca dispara.
    /// </summary>
    private static void ConferirBindings()
    {
        var asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(ControlsPath);
        if (asset == null)
        {
            Falhar("PlayerControls.inputactions não encontrado");
            return;
        }

        ConferirMapa(asset, "Player", Esperado);
        ConferirMapa(asset, "Dialogue", EsperadoDialogo);
    }

    private static void ConferirMapa(InputActionAsset asset, string mapa, (string acao, string caminho)[] esperado)
    {
        InputActionMap map = asset.FindActionMap(mapa, throwIfNotFound: false);
        if (map == null)
        {
            Falhar("mapa '" + mapa + "' não existe");
            return;
        }

        foreach ((string acao, string caminho) in esperado)
        {
            InputAction action = map.FindAction(acao);
            if (action == null)
            {
                Falhar(mapa + ": ação '" + acao + "' não existe");
                continue;
            }

            bool achou = false;
            foreach (InputBinding binding in action.bindings)
            {
                if (binding.path == caminho)
                {
                    achou = true;
                    break;
                }
            }

            if (!achou)
            {
                Falhar(mapa + "/" + acao + ": falta o caminho " + caminho);
            }
        }

        // O teclado tem que continuar lá. É o requisito que mais fácil se perde ao
        // mexer no asset, e o mais barato de conferir.
        foreach (InputAction action in map.actions)
        {
            bool temTeclado = false;
            foreach (InputBinding binding in action.bindings)
            {
                if (binding.path != null && binding.path.StartsWith("<Keyboard>"))
                {
                    temTeclado = true;
                    break;
                }
            }

            if (!temTeclado)
            {
                Falhar(mapa + "/" + action.name + ": perdeu o binding de teclado");
            }
        }
    }

    private static void Encerrar(int codigo)
    {
        if (Application.isBatchMode)
        {
            EditorApplication.Exit(codigo);
        }
    }
}
