using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Build de linha de comando:
/// Unity.exe -batchmode -quit -projectPath . -executeMethod BuildScript.BuildWebGL
/// </summary>
public static class BuildScript
{
    private const string OutputPath = "Builds/WebGL";

    public static void BuildWebGL()
    {
        ApplyWebGLSettings();

        string[] scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = OutputPath,
            target = BuildTarget.WebGL,
            targetGroup = BuildTargetGroup.WebGL,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);

        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"BuildWebGL succeeded: {report.summary.totalSize} bytes at {OutputPath}");
            EditorApplication.Exit(0);
        }
        else
        {
            Debug.LogError($"BuildWebGL failed: {report.summary.result}");
            EditorApplication.Exit(1);
        }
    }

    /// <summary>
    /// Configurações voltadas a entregar um build Web pequeno e que carrega rápido.
    /// Aplicadas pelo próprio script para que o build da CI produza exatamente o mesmo
    /// resultado do build local, sem depender de alguém ter marcado as opções à mão.
    /// </summary>
    public static void ApplyWebGLSettings()
    {
        // Compressão Brotli: menor payload de rede. Requer o servidor enviar
        // Content-Encoding: br (o servidor de teste do Unity já faz isso).
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
        PlayerSettings.WebGL.dataCaching = true;
        PlayerSettings.WebGL.decompressionFallback = true;

        // Sem exceções = wasm menor e mais rápido. O jogo não depende de try/catch
        // em runtime para nada de gameplay.
        PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.None;
        PlayerSettings.WebGL.debugSymbolMode = WebGLDebugSymbolMode.Off;

        // Otimização por tamanho e stripping agressivo do código não utilizado.
        var webgl = NamedBuildTarget.WebGL;
        PlayerSettings.SetIl2CppCodeGeneration(webgl, Il2CppCodeGeneration.OptimizeSize);
        PlayerSettings.SetManagedStrippingLevel(webgl, ManagedStrippingLevel.High);

        PlayerSettings.stripUnusedMeshComponents = true;
        PlayerSettings.bakeCollisionMeshes = true;

        // Splash screen do Unity desligada onde a licença permite; em Personal ela
        // permanece, então isto é um no-op silencioso nesse caso.
        PlayerSettings.SplashScreen.showUnityLogo = false;
    }
}
