using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Odisseia.Systems;
using Odisseia.UI;

/// <summary>
/// Sonda do menu principal em play mode. Confere o que só aparece rodando: se o
/// Continuar fica desabilitado sem save e habilitado com save, onde cai o foco inicial
/// em cada caso, se a navegação encadeia os itens e se as telas montadas em runtime
/// (confirmação e configurações) sobem sem erro.
///
/// Unity.exe -batchmode -projectPath . -executeMethod MainMenuProbe.Run
/// (sem -quit: encerra sozinha; apaga o save do Editor)
/// </summary>
public static class MainMenuProbe
{
    private const int Limite = 2000000;

    private static int passo;
    private static int frames;
    private static int total;
    private static bool falhou;

    private static bool opcoesAtivas;
    private static EnterPlayModeOptions opcoes;

    public static void Run()
    {
        opcoesAtivas = EditorSettings.enterPlayModeOptionsEnabled;
        opcoes = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;

        PlayerPrefs.DeleteKey("Odisseia.Save");
        PlayerPrefs.Save();

        resolucaoAntes = new Vector2Int(Screen.width, Screen.height);

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
        if (++total > Limite)
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
                if (Object.FindAnyObjectByType<Odisseia.Core.CampaignManager>() == null)
                {
                    return;
                }

                SceneManager.LoadScene("MainMenu");
                Avancar();
                return;

            case 1:
                if (frames < 30)
                {
                    return;
                }

                Log("--- sem save ---");
                InspecionarMenu(esperaContinuar: false);

                // Simula progresso e recarrega o menu.
                Odisseia.Core.CampaignManager.Instance.CompleteLevel("Level_01_Itaca_Prologue", 0, 0);
                Log("save criado: " + SaveSystem.HasSave());
                SceneManager.LoadScene("MainMenu");
                Avancar();
                return;

            case 2:
                if (frames < 30)
                {
                    return;
                }

                Log("--- com save ---");
                InspecionarMenu(esperaContinuar: true);

                Log("--- telas montadas em runtime ---");
                ConfirmDialog.Show("TESTE", "mensagem", "OK", "CANCELAR", null);
                Log("ConfirmDialog abriu? " + ConfirmDialog.IsOpen);

                SettingsScreen.Open();
                Log("SettingsScreen abriu? " + SettingsScreen.IsOpen);
                Log("resolucoes oferecidas: " + SettingsManager.AvailableResolutions.Length +
                    " | qualidade: " + SettingsManager.QualityNames.Length + " niveis");

                ConferirEnquadramento();
                DumparSettings();

                // O remapeamento é aberto de dentro das configurações: tem que ficar
                // por cima delas, senão o painel abre atrás e não dá para clicar nada.
                OptionsMenu.Open();
                ConferirEmpilhamento();

                Avancar();
                return;

            case 3:
                if (frames < 20)
                {
                    return;
                }

                ConferirMusica("no menu principal");
                SceneManager.LoadScene("WorldMap");
                Avancar();
                return;

            case 4:
                if (frames < 30)
                {
                    return;
                }

                // O ponto do tema único: atravessar a troca de cena sem cortar.
                ConferirMusica("depois de trocar de cena");

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

    private static void InspecionarMenu(bool esperaContinuar)
    {
        var controller = Object.FindAnyObjectByType<MainMenuController>();
        if (controller == null)
        {
            Erro("MainMenuController ausente");
            return;
        }

        Button[] itens = controller.MenuItems.Where(b => b != null).ToArray();
        Log("itens do menu: " + itens.Length);

        foreach (Button b in itens)
        {
            Log($"  {b.name,-20} interactable={b.interactable,-5} nav={b.navigation.mode}");
        }

        Button continuar = itens.FirstOrDefault(b => b.name == "ContinueButton");
        if (continuar == null)
        {
            Erro("ContinueButton ausente");
        }
        else if (continuar.interactable != esperaContinuar)
        {
            Erro($"Continuar deveria estar {(esperaContinuar ? "habilitado" : "desabilitado")}");
        }

        GameObject foco = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
        string esperado = esperaContinuar ? "ContinueButton" : "NewGameButton";
        Log("foco inicial: " + (foco != null ? foco.name : "nenhum") + " (esperado " + esperado + ")");

        if (foco == null || foco.name != esperado)
        {
            Erro("foco inicial errado");
        }

        // Background e logo.
        var fundo = GameObject.Find("Background")?.GetComponent<Image>();
        Log("background com sprite? " + (fundo != null && fundo.sprite != null));
        if (fundo == null || fundo.sprite == null)
        {
            Erro("background sem sprite");
        }

        Text titulo = GameObject.Find("Title")?.GetComponent<Text>();
        Text subtitulo = GameObject.Find("Subtitle")?.GetComponent<Text>();
        Log($"titulo='{titulo?.text}' subtitulo='{subtitulo?.text}'");
    }

    /// <summary>
    /// Despeja a árvore da tela de configurações com ordem de desenho, retângulo e cor.
    /// Sem conseguir tirar print daqui, é assim que dá para conferir contraste e
    /// sobreposição em vez de supor — a moldura dourada cobrindo o painel só apareceu
    /// porque alguém olhou a tela.
    /// </summary>
    private static void DumparSettings()
    {
        var tela = Object.FindAnyObjectByType<SettingsScreen>();
        if (tela == null)
        {
            Erro("SettingsScreen não encontrada na cena");
            return;
        }

        var canvas = tela.GetComponentInChildren<Canvas>(includeInactive: true);
        Log("--- arvore da tela de configuracoes (ordem = ordem de desenho) ---");
        Despejar(canvas.transform, 0);

        // Contraste: nenhum texto pode estar sobre um fundo de cor parecida.
        foreach (Text t in canvas.GetComponentsInChildren<Text>(includeInactive: true))
        {
            Image fundo = t.GetComponentInParent<Image>();
            if (fundo == null || string.IsNullOrEmpty(t.text))
            {
                continue;
            }

            float delta = Mathf.Abs(Luma(t.color) - Luma(fundo.color));
            if (delta < 0.18f)
            {
                Erro($"contraste baixo: '{t.text}' ({Hex(t.color)}) sobre {fundo.name} ({Hex(fundo.color)}) — delta {delta:0.00}");
            }
        }
    }

    /// <summary>
    /// Lista os Canvas abertos por ordem de desenho e cobra a regra: quem abre fica
    /// embaixo de quem foi aberto. A tela de remapeamento nasceu abaixo da de
    /// configurações justamente por não haver nada conferindo isso.
    /// </summary>
    private static void ConferirEmpilhamento()
    {
        Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Exclude)
            .Where(c => c.isRootCanvas)
            .OrderBy(c => c.sortingOrder)
            .ToArray();

        Log("--- canvas abertos, do fundo para a frente ---");
        foreach (Canvas c in canvases)
        {
            Log($"  {c.sortingOrder,4}  {c.name}");
        }

        // Cada tela tem que ter UM canvas. Dois significa Build rodando duas vezes, e o
        // duplicado invisível continua interceptando clique.
        foreach (IGrouping<string, Canvas> grupo in canvases.GroupBy(c => c.name))
        {
            if (grupo.Count() > 1)
            {
                Erro($"canvas duplicado: {grupo.Key} x{grupo.Count()}");
            }
        }

        int configuracoes = Ordem(canvases, "SettingsCanvas");
        int remapeamento = Ordem(canvases, "OptionsCanvas");

        if (configuracoes < 0 || remapeamento < 0)
        {
            Erro($"canvas esperado ausente (configurações={configuracoes}, remapeamento={remapeamento})");
            return;
        }

        if (remapeamento <= configuracoes)
        {
            Erro($"remapeamento ({remapeamento}) precisa ficar ACIMA das configurações ({configuracoes})");
        }
        else
        {
            Log($"ok: remapeamento ({remapeamento}) acima das configurações ({configuracoes})");
        }
    }

    private static int Ordem(Canvas[] canvases, string nome)
    {
        Canvas c = canvases.FirstOrDefault(x => x.name == nome);
        return c != null ? c.sortingOrder : -1;
    }

    private static void Despejar(Transform t, int nivel)
    {
        var rect = t as RectTransform;
        var image = t.GetComponent<Image>();
        var texto = t.GetComponent<Text>();

        string cor = image != null ? " img=" + Hex(image.color) : texto != null ? " txt=" + Hex(texto.color) : "";
        string tamanho = rect != null ? $" [{rect.rect.width:0}x{rect.rect.height:0} @ {rect.anchoredPosition.x:0},{rect.anchoredPosition.y:0}]" : "";
        string conteudo = texto != null && !string.IsNullOrEmpty(texto.text) ? " \"" + texto.text + "\"" : "";

        Log(new string(' ', nivel * 2) + t.name + tamanho + cor + conteudo);

        for (int i = 0; i < t.childCount; i++)
        {
            Despejar(t.GetChild(i), nivel + 1);
        }
    }

    private static float Luma(Color c) => (0.299f * c.r + 0.587f * c.g + 0.114f * c.b) * c.a;

    private static string Hex(Color c) => ColorUtility.ToHtmlStringRGBA(c);

    private static Vector2Int resolucaoAntes;

    /// <summary>
    /// Abrir o jogo não pode mudar o tamanho da janela. Em WebGL o canvas pertence à
    /// página: a Unity mantém o alvo de render casado com o tamanho DOM, e um
    /// Screen.SetResolution muda os atributos do canvas por baixo do CSS — o jogo
    /// aparece deslocado, com faixa preta e conteúdo cortado.
    ///
    /// Aqui só dá para conferir que ninguém mexeu na resolução ao carregar as telas;
    /// o comportamento específico de WebGL fica atrás de #if e não roda no Editor.
    /// </summary>
    private static void ConferirEnquadramento()
    {
        var agora = new Vector2Int(Screen.width, Screen.height);
        Log($"resolução: {resolucaoAntes.x}x{resolucaoAntes.y} -> {agora.x}x{agora.y}");

        if (resolucaoAntes != Vector2Int.zero && agora != resolucaoAntes)
        {
            Erro($"abrir as telas mudou a resolução de {resolucaoAntes} para {agora} — " +
                 "em WebGL isso tira o jogo do enquadramento");
        }
    }

    private static AudioClip temaAnterior;

    /// <summary>
    /// Confere que o tema único está carregado, em loop, e que é o MESMO clipe entre
    /// cenas — trocar o clipe reiniciaria a música a cada tela, que é exatamente o que
    /// o tema único existe para evitar.
    ///
    /// Em batchmode não há dispositivo de áudio, então <c>isPlaying</c> não é
    /// confiável; o que dá para afirmar com certeza é qual clipe está na fonte e se ela
    /// está marcada como loop.
    /// </summary>
    private static void ConferirMusica(string momento)
    {
        AudioClip tema = Odisseia.Core.GameAssets.Instance != null
            ? Odisseia.Core.GameAssets.Instance.Audio?.MainTheme
            : null;

        if (tema == null)
        {
            Erro("AudioLibrary sem MainTheme — o tema único não está ligado");
            return;
        }

        var gerente = Object.FindAnyObjectByType<AudioManager>();
        if (gerente == null)
        {
            Erro("AudioManager ausente");
            return;
        }

        AudioSource musica = gerente.GetComponents<AudioSource>().FirstOrDefault(a => a.loop);
        if (musica == null)
        {
            Erro("nenhuma fonte de música em loop no AudioManager");
            return;
        }

        Log($"música {momento}: clipe='{(musica.clip != null ? musica.clip.name : "nenhum")}' " +
            $"loop={musica.loop} duração={(musica.clip != null ? musica.clip.length : 0f):0.0}s");

        if (musica.clip != tema)
        {
            Erro($"a fonte não está com o tema único (esperado '{tema.name}')");
        }

        if (temaAnterior != null && !ReferenceEquals(temaAnterior, musica.clip))
        {
            Erro("o clipe mudou entre cenas — a música reiniciaria");
        }

        temaAnterior = musica.clip;
    }

    private static void Log(string m) => Debug.Log("[MainMenuProbe] " + m);

    private static void Erro(string m)
    {
        falhou = true;
        Debug.LogError("[MainMenuProbe] " + m);
    }

    private static void Sair(int codigo)
    {
        EditorApplication.update -= Tick;
        EditorApplication.isPlaying = false;

        EditorSettings.enterPlayModeOptionsEnabled = opcoesAtivas;
        EditorSettings.enterPlayModeOptions = opcoes;

        EditorApplication.delayCall += () => EditorApplication.Exit(codigo);
    }
}
