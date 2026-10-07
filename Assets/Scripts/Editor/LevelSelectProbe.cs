using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Odisseia.Core;
using Odisseia.UI;

/// <summary>
/// Sonda da tela de seleção de fases em play mode: confere que as 16 etapas aparecem
/// na ordem oficial, que o bloqueio segue o save, que a rolagem cobre a lista inteira
/// e que o mapa carregou sem distorcer.
///
/// Unity.exe -batchmode -projectPath . -executeMethod LevelSelectProbe.Run
/// (sem -quit: encerra sozinha; apaga o save do Editor)
/// </summary>
public static class LevelSelectProbe
{
    private static readonly string[] Ordem =
    {
        "Level_01_Itaca_Prologue", "Level_02_Troia", "Level_03_Cicones", "Level_04_Citera",
        "Level_05_Ciclopes", "Level_06_Eolo", "Level_07_Lestrigoes", "Level_08_Circe",
        "Level_09_MundoDosMortos", "Level_10_Sereias", "Level_11_CilaCaribdis",
        "Level_12_GadoDoSol", "Level_13_Calipso", "Level_14_Itaca_Return",
        "Level_15_Pretendentes", "Level_16_Final",
    };

    private static int passo;
    private static int frames;
    private static int total;
    private static bool falhou;

    private static bool opcoesAtivas, mudoOriginal;
    private static EnterPlayModeOptions opcoes;

    public static void Run()
    {
        // Todo teste no mudo (restaurado no Sair).
        mudoOriginal = EditorUtility.audioMasterMute;
        EditorUtility.audioMasterMute = true;
        AudioListener.volume = 0f;

        opcoesAtivas = EditorSettings.enterPlayModeOptionsEnabled;
        opcoes = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;

        PlayerPrefs.DeleteKey("Odisseia.Save");
        PlayerPrefs.Save();

        EditorSceneManager.OpenScene("Assets/Scenes/Boot/Boot.unity", OpenSceneMode.Single);

        passo = 0;
        frames = 0;
        total = 0;
        falhou = false;
        EditorApplication.update += Tick;
        EditorApplication.EnterPlaymode();
    }

    private static void Tick()
    {
        if (++total > 2000000)
        {
            Log("TIMEOUT no passo " + passo);
            Sair(1);
            return;
        }

        if (!EditorApplication.isPlaying)
        {
            return;
        }

        frames++;

        switch (passo)
        {
            case 0:
                if (CampaignManager.Instance == null)
                {
                    return;
                }

                // Progresso até Cítera, para haver bloqueadas e desbloqueadas na lista.
                for (int i = 0; i < 4; i++)
                {
                    CampaignManager.Instance.CompleteLevel(Ordem[i], 0, 0);
                }

                SceneManager.LoadScene("LevelSelect");
                Avancar();
                return;

            case 1:
                if (frames < 30)
                {
                    return;
                }

                Log("=== caminho normal (passando pelo Boot) ===");
                Inspecionar();
                DespejarLayout();

                // Abrir a cena direto, sem Boot, é o que qualquer um faz ao iterar no
                // Editor. Sem CampaignManager não há de onde tirar a lista de fases —
                // a tela precisa ao menos dizer isso, em vez de mostrar uma coluna vazia.
                Object.DestroyImmediate(CampaignManager.Instance.gameObject);
                SceneManager.LoadScene("LevelSelect");
                Avancar();
                return;

            case 2:
                if (frames < 30)
                {
                    return;
                }

                Log("=== cena aberta direto, sem Boot ===");
                var solto = Object.FindAnyObjectByType<LevelSelectController>();
                Log("entradas sem CampaignManager: " + (solto != null ? solto.Entries.Count : -1));
                DespejarLayout();

                Log(falhou ? "RESULTADO: FALHOU" : "RESULTADO: OK");
                Sair(falhou ? 1 : 0);
                return;
        }
    }

    private static void Avancar()
    {
        passo++;
        frames = 0;
    }

    private static void Inspecionar()
    {
        var controller = Object.FindAnyObjectByType<LevelSelectController>();
        if (controller == null)
        {
            Erro("LevelSelectController ausente");
            return;
        }

        Button[] entradas = controller.Entries.ToArray();
        Log($"entradas na lista: {entradas.Length} (esperado {Ordem.Length})");

        if (entradas.Length != Ordem.Length)
        {
            Erro("a lista não tem as 16 etapas");
        }

        for (int i = 0; i < entradas.Length && i < Ordem.Length; i++)
        {
            string esperado = "LevelEntry_" + Ordem[i];
            string obtido = entradas[i].name;
            string rotulos = string.Join(" | ", entradas[i].GetComponentsInChildren<Text>().Select(t => t.text));

            Log($"  {i + 1:00} {obtido,-34} liberada={entradas[i].interactable,-5}  [{rotulos}]");

            if (obtido != esperado)
            {
                Erro($"posição {i + 1}: {obtido}, esperado {esperado}");
            }
        }

        // Altura das entradas: o VerticalLayoutGroup so respeita o
        // LayoutElement.preferredHeight se childControlHeight estiver ligado. Desligado,
        // ele usa a altura propria do RectTransform (100 por padrao) e a lista sai com
        // mais que o dobro do previsto — erro que nao gera aviso nenhum.
        foreach (Button entrada in entradas.Take(3))
        {
            var rect = (RectTransform)entrada.transform;
            var elemento = entrada.GetComponent<LayoutElement>();
            float pedida = elemento != null ? elemento.preferredHeight : -1f;

            if (elemento != null && Mathf.Abs(rect.rect.height - pedida) > 1f)
            {
                Erro($"{entrada.name}: altura {rect.rect.height:0}px, pedida {pedida:0}px " +
                     "(childControlHeight desligado?)");
            }
        }

        // Destaque não pode mover a entrada. Dentro de um grupo de layout quem manda na
        // posição é o grupo; se o botão também escrever anchoredPosition, o item pula
        // para uma posição velha ao receber o mouse — some da lista.
        foreach (Button entrada in entradas.Take(4))
        {
            var rect = (RectTransform)entrada.transform;
            Vector2 antes = rect.anchoredPosition;

            ExecuteEvents.Execute(entrada.gameObject, new PointerEventData(EventSystem.current),
                ExecuteEvents.pointerEnterHandler);
            Vector2 comMouse = rect.anchoredPosition;

            ExecuteEvents.Execute(entrada.gameObject, new PointerEventData(EventSystem.current),
                ExecuteEvents.pointerExitHandler);

            if (Vector2.Distance(antes, comMouse) > 0.5f)
            {
                Erro($"{entrada.name}: o destaque moveu a entrada de {antes} para {comMouse}");
            }
        }

        if (!falhou)
        {
            Log("destaque não move as entradas: ok");
        }

        // Concluí 4 etapas, então 5 devem estar liberadas (as 4 + a seguinte).
        int liberadas = entradas.Count(b => b.interactable);
        Log($"liberadas: {liberadas} (esperado 5)");
        if (liberadas != 5)
        {
            Erro("o bloqueio não está seguindo o save");
        }

        // Rolagem: o conteúdo tem que ser mais alto que a janela, senão as últimas
        // etapas ficariam inalcançáveis, e o ScrollRect precisa estar ligado.
        var scroll = Object.FindAnyObjectByType<ScrollRect>();
        if (scroll == null)
        {
            Erro("sem ScrollRect na lista");
        }
        else
        {
            float conteudo = scroll.content.rect.height;
            float janela = scroll.viewport.rect.height;
            Log($"rolagem: conteúdo {conteudo:0}px, janela {janela:0}px, viewport={scroll.viewport != null}, content={scroll.content != null}");

            if (conteudo <= janela)
            {
                Log("  (a lista coube sem rolar — ok, mas conferir em telas mais baixas)");
            }
        }

        // Mapa: sprite carregado e proporção preservada.
        var mapa = GameObject.Find("MapImage")?.GetComponent<Image>();
        var quadro = GameObject.Find("MapFrame")?.GetComponent<AspectRatioFitter>();

        if (mapa == null || mapa.sprite == null)
        {
            Erro("mapa sem sprite");
        }
        else
        {
            var rect = (RectTransform)mapa.transform;
            float proporcaoArte = mapa.sprite.rect.width / mapa.sprite.rect.height;
            float proporcaoTela = rect.rect.width / rect.rect.height;
            Log($"mapa: '{mapa.sprite.name}' {mapa.sprite.rect.width:0}x{mapa.sprite.rect.height:0} " +
                $"| proporção arte {proporcaoArte:0.000} vs desenhada {proporcaoTela:0.000}");

            if (Mathf.Abs(proporcaoArte - proporcaoTela) > 0.02f)
            {
                Erro("o mapa está distorcido");
            }
        }

        if (quadro == null)
        {
            Erro("MapFrame sem AspectRatioFitter");
        }

        if (GameObject.Find("MarkerLayer") == null)
        {
            Erro("MarkerLayer ausente — é onde o marcador do Odisseu vai entrar");
        }

        GameObject foco = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
        Log("foco inicial: " + (foco != null ? foco.name : "nenhum"));
        if (foco == null)
        {
            Erro("nenhum item em foco — teclado e controle não responderiam");
        }
    }

    /// <summary>Retângulos reais da coluna, para ver onde o layout foi parar.</summary>
    private static void DespejarLayout()
    {
        var painel = GameObject.Find("SidePanel");
        if (painel == null)
        {
            Erro("SidePanel ausente");
            return;
        }

        Log("--- coluna da esquerda ---");
        Despejar(painel.transform, 0, 0);
    }

    private static void Despejar(Transform t, int nivel, int indice)
    {
        // Uma lista de 16 entradas nao precisa aparecer inteira; as duas primeiras
        // bastam para julgar se o layout esta certo.
        if (nivel >= 4 && indice > 1)
        {
            return;
        }

        var rect = t as RectTransform;
        string tamanho = rect != null
            ? $" [{rect.rect.width:0}x{rect.rect.height:0} @ {rect.anchoredPosition.x:0},{rect.anchoredPosition.y:0}]"
            : "";

        var texto = t.GetComponent<Text>();
        string conteudo = texto != null ? $" \"{texto.text}\" fonte={texto.fontSize}" : "";

        Log(new string(' ', nivel * 2) + t.name + tamanho + conteudo);

        for (int i = 0; i < t.childCount; i++)
        {
            Despejar(t.GetChild(i), nivel + 1, i);
        }
    }

    private static void Log(string m) => Debug.Log("[LevelSelectProbe] " + m);

    private static void Erro(string m)
    {
        falhou = true;
        Debug.LogError("[LevelSelectProbe] " + m);
    }

    private static void Sair(int codigo)
    {
        EditorApplication.update -= Tick;
        EditorApplication.isPlaying = false;
        AudioListener.volume = 1f;
        EditorUtility.audioMasterMute = mudoOriginal;

        EditorSettings.enterPlayModeOptionsEnabled = opcoesAtivas;
        EditorSettings.enterPlayModeOptions = opcoes;

        EditorApplication.delayCall += () => EditorApplication.Exit(codigo);
    }
}
