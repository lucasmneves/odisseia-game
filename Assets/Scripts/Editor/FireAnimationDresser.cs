using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Odisseia.Systems;

/// <summary>
/// Fogo em loop: liga um <see cref="SpriteAnimator"/> em todo SpriteRenderer que desenha um fogo
/// ESTÁTICO que tem par animado no pack (Asset Completion e Polish Pass 02):
///
/// Unity.exe -batchmode -nographics -quit -projectPath . -executeMethod FireAnimationDresser.Run
///
/// As folhas animadas têm o MESMO canvas e o mesmo pivô do sprite original (só a chama se mexe,
/// D-061), então a troca não toca em posição, escala, ordem nem cor do renderer: só acrescenta o
/// animador. Elas ficam em <c>Resources/Odisseia/Environments/Fire/</c> porque o SpriteAnimator
/// carrega por <c>Resources.LoadAll</c>; os quadros já vêm em pingue-pongue (0-1-2-3-2-1), então o
/// loop simples não tem emenda.
///
/// **Rodar DEPOIS dos vestidores de cena.** Eles destroem e recriam a própria raiz, e com ela o
/// animador. Idempotente: renderer que já tem o animador certo fica como está.
/// </summary>
public static class FireAnimationDresser
{
    private const string Pasta = "Odisseia/Environments/Fire/";

    /// <summary>Sprite estático → folha animada em Resources.</summary>
    private static readonly Dictionary<string, string> Pares = new Dictionary<string, string>
    {
        ["Assets/Art/Environments/Troy/Camp/troy_campfire_01.png"] = "troy_campfire_01_anim",
        ["Assets/Art/Environments/Ciclopes/Props/ciclopes_torch_wall.png"] = "ciclopes_torch_wall_anim",
        ["Assets/Art/Environments/Ciclopes/Special/ciclopes_giant_fire.png"] = "ciclopes_giant_fire_anim",
        ["Assets/Art/Environments/Eolo/Palace/eolo_brazier_native.png"] = "eolo_brazier_native_anim",
        ["Assets/Art/Environments/MundoDosMortos/Props/mortos_brazier_tall.png"] = "mortos_brazier_tall_spectral_anim",
        ["Assets/Art/Environments/ItacaReturn/Props/itaca_ret_brazier.png"] = "itaca_ret_brazier_anim",
        ["Assets/Art/Environments/Pretendentes/VFX/pret_torch_stand.png"] = "pret_torch_stand_anim",
        ["Assets/Art/Environments/Pretendentes/Courtyard/pret_cooking_fire.png"] = "pret_cooking_fire_anim",
        ["Assets/Art/Environments/Final/Props/final_hearth_fire.png"] = "final_hearth_fire_anim",
    };

    private static readonly string[] Cenas =
    {
        "Level_02_Troia", "Level_03_Cicones", "Level_05_Ciclopes", "Level_06_Eolo", "Level_07_Lestrigoes",
        "Level_09_MundoDosMortos", "Level_14_Itaca_Return", "Level_15_Pretendentes", "Level_16_Final",
    };

    [MenuItem("Odisseia/Animar fogos")]
    public static void Animar() => Executar();

    public static void Run()
    {
        bool ok = Executar();
        if (Application.isBatchMode) { EditorApplication.Exit(ok ? 0 : 1); }
    }

    private static bool Executar()
    {
        foreach (string folha in Pares.Values)
        {
            if (Resources.LoadAll<Sprite>(Pasta + folha).Length == 0)
            {
                Debug.LogError($"[Fogo] folha ausente em Resources/{Pasta}{folha}");
                return false;
            }
        }

        int total = 0;
        foreach (string cena in Cenas)
        {
            var scene = EditorSceneManager.OpenScene($"Assets/Scenes/Levels/{cena}.unity", OpenSceneMode.Single);
            int novos = 0, ja = 0;
            foreach (SpriteRenderer sr in Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include))
            {
                if (sr.sprite == null || !Pares.TryGetValue(AssetDatabase.GetAssetPath(sr.sprite), out string folha))
                {
                    continue;
                }

                string caminho = Pasta + folha;
                var anim = sr.GetComponent<SpriteAnimator>();
                if (anim != null)
                {
                    var atual = new SerializedObject(anim);
                    if (atual.FindProperty("resourcePath").stringValue == caminho) { ja++; continue; }
                    Debug.LogWarning($"[Fogo] {cena}/{sr.name} já tem SpriteAnimator de outra folha — não tocado");
                    continue;
                }

                anim = sr.gameObject.AddComponent<SpriteAnimator>();
                var so = new SerializedObject(anim);
                so.FindProperty("resourcePath").stringValue = caminho;
                // O estado é lido do nome do quadro: <folha>_anim_NN → "anim".
                so.FindProperty("defaultState").stringValue = "anim";
                // 7,5 a 9 fps pela posição: fogos vizinhos não pulsam em uníssono. Determinístico, então
                // rodar de novo dá a mesma cena.
                int passo = Mathf.Abs(Mathf.RoundToInt(sr.transform.position.x * 7f + sr.transform.position.y * 3f)) % 4;
                so.FindProperty("defaultFramesPerSecond").floatValue = 7.5f + passo * 0.5f;
                so.ApplyModifiedProperties();
                novos++;
            }

            if (novos > 0)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }

            Debug.Log($"[Fogo] {cena}: {novos} animados agora, {ja} já estavam");
            total += novos + ja;
        }

        Debug.Log($"[Fogo] {total} fogos com loop nas {Cenas.Length} cenas");
        return true;
    }

    // ---------------------------------------------------------------- conferência em play mode

    /// <summary>
    /// Prova que o fogo ANIMA: abre cada cena em play mode, amostra o quadro de cada fogo por 1,5 s e
    /// conta quantos passaram por mais de um quadro. Sem GPU não há problema (-nographics serve);
    /// sem -quit (sai sozinho).
    ///
    /// Unity.exe -batchmode -nographics -projectPath . -executeMethod FireAnimationDresser.Conferir
    /// </summary>
    public static void Conferir()
    {
        cenaAtual = 0;
        falhas = 0;
        // Sem domain reload ao entrar em play mode: com ele o Tick registrado aqui some. Restaurado no fim.
        opcoesAtivas = EditorSettings.enterPlayModeOptionsEnabled;
        opcoes = EditorSettings.enterPlayModeOptions;
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
        EditorSceneManager.OpenScene($"Assets/Scenes/Levels/{Cenas[0]}.unity", OpenSceneMode.Single);
        EditorApplication.update += Tick;
        EditorApplication.EnterPlaymode();
    }

    private static int cenaAtual, falhas, amostras;
    private static bool opcoesAtivas;
    private static EnterPlayModeOptions opcoes;
    private static double proxima;
    private static readonly Dictionary<SpriteAnimator, HashSet<string>> vistos = new Dictionary<SpriteAnimator, HashSet<string>>();

    private static void Tick()
    {
        if (!EditorApplication.isPlaying || EditorApplication.timeSinceStartup < proxima) { return; }

        if (vistos.Count == 0 && amostras == 0)
        {
            foreach (SpriteAnimator a in Object.FindObjectsByType<SpriteAnimator>())
            {
                // Também a moeda do coletável (ItemArtDresser): mesma prova, mesma folha em Resources.
                string rp = new SerializedObject(a).FindProperty("resourcePath").stringValue;
                if (rp.StartsWith(Pasta) || rp == "Odisseia/Items/item_collectible_coin") { vistos[a] = new HashSet<string>(); }
            }
        }

        foreach (KeyValuePair<SpriteAnimator, HashSet<string>> par in vistos)
        {
            var sr = par.Key != null ? par.Key.GetComponent<SpriteRenderer>() : null;
            if (sr != null && sr.sprite != null) { par.Value.Add(sr.sprite.name); }
        }

        amostras++;
        proxima = EditorApplication.timeSinceStartup + 0.1;
        if (amostras < 15) { return; }

        int animando = 0;
        foreach (KeyValuePair<SpriteAnimator, HashSet<string>> par in vistos)
        {
            if (par.Value.Count > 1) { animando++; }
            else { falhas++; Debug.LogError($"[Fogo] {Cenas[cenaAtual]}/{(par.Key != null ? par.Key.name : "?")} parado em {string.Join(",", par.Value)}"); }
        }

        Debug.Log($"[Fogo] play mode {Cenas[cenaAtual]}: {animando}/{vistos.Count} fogos e moedas trocaram de quadro em 1,5 s");
        vistos.Clear();
        amostras = 0;
        cenaAtual++;
        if (cenaAtual >= Cenas.Length)
        {
            EditorApplication.update -= Tick;
            EditorApplication.ExitPlaymode();
            EditorSettings.enterPlayModeOptionsEnabled = opcoesAtivas;
            EditorSettings.enterPlayModeOptions = opcoes;
            EditorApplication.Exit(falhas == 0 ? 0 : 1);
            return;
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene(Cenas[cenaAtual]);
        proxima = EditorApplication.timeSinceStartup + 1.0;
    }
}
