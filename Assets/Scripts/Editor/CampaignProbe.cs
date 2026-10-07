using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Odisseia.Core;
using Odisseia.Systems;
using Odisseia.WorldMap;

/// <summary>
/// Sonda de diagnóstico: roda a campanha de verdade em play mode e reporta o que o
/// jogador teria em mãos. Responde perguntas de runtime que a leitura dos arquivos não
/// responde — o que ficou desbloqueado ao concluir uma fase, onde Odisseu nasce no
/// mapa e até onde ele consegue caminhar.
///
/// Unity.exe -batchmode -projectPath . -executeMethod CampaignProbe.Run
/// (sem -quit: a sonda encerra sozinha)
///
/// Entrar em play mode normalmente dispara um domain reload, que zera os estáticos e
/// cancela a inscrição em EditorApplication.update — a sonda ficaria rodando o jogo
/// para sempre. Por isso o reload é desligado durante a execução e restaurado no fim.
///
/// ATENÇÃO: apaga o save do Editor (PlayerPrefs) para partir de um jogo novo. Não
/// toca no save do build Web, que vive no localStorage do navegador.
/// </summary>
public static class CampaignProbe
{
    private const string Prologo = "Level_01_Itaca_Prologue";
    private const string Troia = "Level_02_Troia";

    private const int LimiteDeFrames = 2000000;

    private static int passo;
    private static int frames;
    private static int framesTotais;
    private static double marcoDeTempo;

    private static bool opcoesOriginaisAtivas;
    private static EnterPlayModeOptions opcoesOriginais;

    private static bool mudoOriginal;

    public static void Run()
    {
        // Todo teste no mudo (restaurado no Sair).
        mudoOriginal = EditorUtility.audioMasterMute;
        EditorUtility.audioMasterMute = true;
        AudioListener.volume = 0f;

        opcoesOriginaisAtivas = EditorSettings.enterPlayModeOptionsEnabled;
        opcoesOriginais = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;

        PlayerPrefs.DeleteKey("Odisseia.Save");
        PlayerPrefs.Save();

        EditorSceneManager.OpenScene("Assets/Scenes/Boot/Boot.unity", OpenSceneMode.Single);

        passo = 0;
        frames = 0;
        framesTotais = 0;
        EditorApplication.update += Tick;
        EditorApplication.EnterPlaymode();
    }

    private static void Tick()
    {
        if (++framesTotais > LimiteDeFrames)
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

                Log("jogo novo -> desbloqueadas: " + Desbloqueadas());
                Log("jogo novo -> concluidas:    " + Concluidas());

                // Exatamente o que LevelManager.CompleteLevel faz ao tocar o LevelGoal.
                CampaignManager.Instance.CompleteLevel(Prologo, 0, 0);
                WorldMapSession.ReportLevelFinished(Prologo, completed: true);

                Log("apos concluir o prologo -> desbloqueadas: " + Desbloqueadas());
                Log("apos concluir o prologo -> concluidas:    " + Concluidas());
                Log($"IsCompleted({Prologo}) = {CampaignManager.Instance.IsCompleted(Prologo)}");
                Log($"IsUnlocked({Troia})    = {CampaignManager.Instance.IsUnlocked(Troia)}");

                SceneManager.LoadScene("WorldMap");
                passo = 1;
                frames = 0;
                return;

            case 1:
                // Deixa o Start do WorldMapManager montar os nós e posicionar Odisseu.
                if (frames < 30)
                {
                    return;
                }

                InspecionarMapa();
                Log("--- esperando a caminhada automatica ate a fase desbloqueada ---");
                marcoDeTempo = EditorApplication.timeSinceStartup;
                passo = 2;
                frames = 0;
                return;

            case 2:
                // Espera por tempo de parede, nao por ticks: em batchmode o
                // EditorApplication.update dispara muito mais rapido que os frames do
                // jogo, e contar ticks mediria quase nada.
                if (EditorApplication.timeSinceStartup - marcoDeTempo < 8.0)
                {
                    return;
                }

                InspecionarDestinoFinal();
                Sair(0);
                return;
        }
    }

    private static void InspecionarMapa()
    {
        var mapa = Object.FindAnyObjectByType<WorldMapManager>();
        var andarilho = Object.FindAnyObjectByType<WorldMapPlayerController>();
        LevelNode[] nos = Object.FindObjectsByType<LevelNode>(FindObjectsInactive.Include)
            .OrderBy(n => n.Order).ToArray();

        Log("nos no mapa: " + nos.Length);
        foreach (LevelNode n in nos.Take(4))
        {
            Log($"  no {n.Order:00} {n.LevelId,-26} estado={n.State,-9} " +
                $"dist={n.DistanceAlongPath:0.00} entravel={n.IsEnterable}");
        }

        if (andarilho == null)
        {
            Log("SEM WorldMapPlayerController na cena");
            return;
        }

        Log($"Odisseu nasce na distancia {andarilho.Distance:0.00}");
        LevelNode aqui = nos.FirstOrDefault(n => andarilho.IsAt(n));
        Log("  em cima do no: " + (aqui != null ? aqui.LevelId : "nenhum"));
        Log("  no considerado atual pelo mapa: " +
            (mapa != null && mapa.CurrentNode != null ? mapa.CurrentNode.LevelId : "nenhum"));

        LevelNode noTroia = nos.FirstOrDefault(n => n.LevelId == Troia);
        Log("  no de Troia existe? " + (noTroia != null));
    }

    /// <summary>
    /// O que importa de verdade: depois da caminhada automática, em que nó Odisseu
    /// está parado? Se for o prólogo, apertar "entrar" rejoga a MESMA fase.
    /// </summary>
    private static void InspecionarDestinoFinal()
    {
        var mapa = Object.FindAnyObjectByType<WorldMapManager>();
        var andarilho = Object.FindAnyObjectByType<WorldMapPlayerController>();
        LevelNode[] nos = Object.FindObjectsByType<LevelNode>(FindObjectsInactive.Include)
            .OrderBy(n => n.Order).ToArray();

        if (andarilho == null)
        {
            Log("SEM WorldMapPlayerController");
            return;
        }

        LevelNode aqui = nos.FirstOrDefault(n => andarilho.IsAt(n));
        Log($"Odisseu parou na distancia {andarilho.Distance:0.00}");
        Log("  em cima do no: " + (aqui != null ? aqui.LevelId : "nenhum"));
        Log("  entrada levaria para: " +
            (mapa != null && mapa.CurrentNode != null ? mapa.CurrentNode.SceneName : "nenhum"));
        Log("  input do jogador liberado? " + !andarilho.InputSuspended);

        bool ok = aqui != null && aqui.LevelId == Troia;
        Log(ok
            ? "RESULTADO: avanca para a fase 2 (Troia)"
            : "RESULTADO: NAO avancou — continua na fase 1");
    }

    private static string Desbloqueadas() => string.Join(", ", SaveSystem.Load().unlockedLevelIds);

    private static string Concluidas()
    {
        var lista = SaveSystem.Load().completedLevelIds;
        return lista.Count == 0 ? "(nenhuma)" : string.Join(", ", lista);
    }

    private static void Log(string m) => Debug.Log("[CampaignProbe] " + m);

    private static void Sair(int codigo)
    {
        EditorApplication.update -= Tick;
        EditorApplication.isPlaying = false;
        AudioListener.volume = 1f;
        EditorUtility.audioMasterMute = mudoOriginal;

        EditorSettings.enterPlayModeOptionsEnabled = opcoesOriginaisAtivas;
        EditorSettings.enterPlayModeOptions = opcoesOriginais;

        EditorApplication.delayCall += () => EditorApplication.Exit(codigo);
    }
}
