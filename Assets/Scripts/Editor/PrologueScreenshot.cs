using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Odisseia.Systems;

/// <summary>
/// Renderiza a fase do prólogo para PNG, sem abrir o Editor e sem jogar:
///
/// Unity.exe -batchmode -quit -projectPath . -executeMethod PrologueScreenshot.Capture
///           -shotX 40 -shotY 0 -shotOut Docs/shot.png
///
/// Existe porque olhar é a única verificação que vale para cenário. As probes conferem
/// número — escala, pivô, filtro — e passam com o parallax empilhado na ordem errada ou uma
/// camada deixando buraco de céu. Nada disso vira erro; só fica feio.
///
/// **Sem <c>-nographics</c>.** Batchmode com -nographics não tem contexto gráfico e devolve
/// imagem preta. É o erro natural aqui, porque todas as outras chamadas do projeto usam
/// -nographics.
///
/// Não altera a cena: cria uma câmera própria, renderiza e desfaz. A cena nunca é salva.
/// </summary>
public static class PrologueScreenshot
{
    /// <summary>
    /// Cena padrão. Outra fase entra por <c>-shotScene</c>, para o capturador servir a todas em
    /// vez de virar um por fase.
    /// </summary>
    private const string CenaPadrao = "Assets/Scenes/Levels/Level_01_Itaca_Prologue.unity";

    /// <summary>Mesmo enquadramento do jogo: ortográfica de tamanho 5, em 16:9. A Fase 16 usa 6: <c>-shotSize 6</c>.</summary>
    private static float TamanhoOrtografico = 5f;
    private const int Largura = 960;
    private const int Altura = 540;

    /// <summary>
    /// Uma ou várias posições numa rodada. Abrir o Unity custa quase um minuto, e olhar o
    /// cenário exige comparar vários pontos da fase — uma chamada por ponto tornaria a
    /// iteração lenta demais para valer a pena.
    ///
    /// <c>-shotXs 2,100,200</c> grava <c>&lt;saida&gt;_002.png</c>, <c>_100.png</c>, ...
    /// </summary>
    public static void Capture()
    {
        float y = Argumento("-shotY", 0f);
        TamanhoOrtografico = Argumento("-shotSize", 5f);
        string saida = ArgumentoTexto("-shotOut", "Docs/Environment_Ithaca/_cena.png");
        string lista = ArgumentoTexto("-shotXs", null);

        EditorSceneManager.OpenScene(ArgumentoTexto("-shotScene", CenaPadrao), OpenSceneMode.Single);

        if (lista != null)
        {
            string raiz = saida.EndsWith(".png") ? saida.Substring(0, saida.Length - 4) : saida;
            foreach (string parte in lista.Split(','))
            {
                if (!float.TryParse(parte, NumberStyles.Float, CultureInfo.InvariantCulture, out float px))
                {
                    continue;
                }

                Render(px, y, $"{raiz}_{Mathf.RoundToInt(px):000}.png");
            }

            Encerrar(0);
            return;
        }

        Render(Argumento("-shotX", 40f), y, saida);
        Encerrar(0);
    }

    /// <summary>
    /// Aplica à mão o deslocamento que o <see cref="Odisseia.Systems.ParallaxLayer"/> faria em
    /// <c>LateUpdate</c>, e devolve as posições originais para desfazer depois.
    ///
    /// Sem isto a foto mente: em modo de edição o componente nunca roda, então as camadas
    /// aparecem na posição de autoria. No começo da fase a diferença é zero e a imagem parece
    /// certa — mais adiante o fundo simplesmente some, e a conclusão natural (errada) é que a
    /// camada não cobre a fase.
    /// </summary>
    private static Dictionary<Transform, Vector3> AplicarParallax(float cameraX, float cameraY)
    {
        var originais = new Dictionary<Transform, Vector3>();

        Camera principal = Camera.main;
        Vector3 inicio = principal != null ? principal.transform.position : Vector3.zero;

        // UnityEngine.Object qualificado: com `using System` no arquivo, `Object` sozinho é
        // ambíguo com System.Object.
        foreach (ParallaxLayer camada in UnityEngine.Object.FindObjectsByType<ParallaxLayer>(FindObjectsSortMode.None))
        {
            var so = new SerializedObject(camada);
            SerializedProperty prop = so.FindProperty("parallaxFactor");
            if (prop == null)
            {
                continue;
            }

            Transform t = camada.transform;
            originais[t] = t.position;
            float fator = prop.floatValue;
            t.position = new Vector3(
                t.position.x + (cameraX - inicio.x) * fator,
                t.position.y + (cameraY - inicio.y) * fator,
                t.position.z);

            var sr = camada.GetComponent<SpriteRenderer>();
            float meia = sr != null ? sr.size.x * 0.5f : 0f;
            Debug.Log($"[Shot]   {camada.name} fator {fator:0.00} -> x {t.position.x:0.0}" +
                      (sr != null ? $", cobre [{t.position.x - meia:0.0} .. {t.position.x + meia:0.0}]" : string.Empty));
        }

        Debug.Log($"[Shot] camera em {cameraX:0.0}, inicio da camera da cena em {inicio.x:0.0}, {originais.Count} camadas");
        return originais;
    }

    // internal: o CyclopsCastDresser.Poses fotografa poses do chefe montadas em memória.
    internal static void Render(float x, float y, string saida)
    {
        Dictionary<Transform, Vector3> originais = AplicarParallax(x, y);
        var go = new GameObject("__ShotCamera");
        Camera cam = go.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = TamanhoOrtografico;
        cam.transform.position = new Vector3(x, y, -10f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.magenta;   // buraco de cobertura fica gritante em vez de passar batido

        var rt = new RenderTexture(Largura, Altura, 24, RenderTextureFormat.ARGB32)
        {
            antiAliasing = 1,
            filterMode = FilterMode.Point,
        };

        Texture2D imagem = null;
        try
        {
            cam.targetTexture = rt;
            cam.Render();

            RenderTexture anterior = RenderTexture.active;
            RenderTexture.active = rt;
            imagem = new Texture2D(Largura, Altura, TextureFormat.RGB24, false);
            imagem.ReadPixels(new Rect(0, 0, Largura, Altura), 0, 0);
            imagem.Apply();
            RenderTexture.active = anterior;

            Directory.CreateDirectory(Path.GetDirectoryName(saida));
            File.WriteAllBytes(saida, imagem.EncodeToPNG());
            Debug.Log($"[Shot] {saida} — camera em ({x.ToString("0.0", CultureInfo.InvariantCulture)}, {y.ToString("0.0", CultureInfo.InvariantCulture)}), {Largura}x{Altura}");
        }
        catch (Exception e)
        {
            Debug.LogError("[Shot] falhou: " + e.Message);
        }
        finally
        {
            cam.targetTexture = null;
            RenderTexture.active = null;
            UnityEngine.Object.DestroyImmediate(go);
            rt.Release();
            UnityEngine.Object.DestroyImmediate(rt);
            if (imagem != null)
            {
                UnityEngine.Object.DestroyImmediate(imagem);
            }

            // A cena não pode sair alterada: cada foto desfaz o deslocamento que aplicou.
            foreach (KeyValuePair<Transform, Vector3> par in originais)
            {
                if (par.Key != null)
                {
                    par.Key.position = par.Value;
                }
            }
        }
    }

    private static float Argumento(string nome, float padrao)
    {
        string bruto = ArgumentoTexto(nome, null);
        return bruto != null && float.TryParse(bruto, NumberStyles.Float, CultureInfo.InvariantCulture, out float v)
            ? v
            : padrao;
    }

    private static string ArgumentoTexto(string nome, string padrao)
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == nome)
            {
                return args[i + 1];
            }
        }

        return padrao;
    }

    private static void Encerrar(int codigo)
    {
        if (Application.isBatchMode)
        {
            EditorApplication.Exit(codigo);
        }
    }
}
