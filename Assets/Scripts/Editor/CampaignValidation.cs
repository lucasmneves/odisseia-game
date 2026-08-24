using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Odisseia.Core;
using Odisseia.Levels;
using Odisseia.Systems;

/// <summary>
/// Confere a ordem oficial da campanha sem precisar abrir o Editor à mão:
///
/// Unity.exe -batchmode -quit -projectPath . -executeMethod CampaignValidation.Run
///
/// Verifica que as 16 etapas estão na ordem certa no CampaignManager e no Build
/// Settings, que cada cena de fase carrega e se identifica com o próprio id, que a
/// cadeia de desbloqueio vai do prólogo ao final sem buracos, e que a migração de save
/// traz um jogador antigo para a numeração nova sem perder progresso.
/// </summary>
public static class CampaignValidation
{
    /// <summary>Ordem oficial. Mudar aqui só com instrução explícita.</summary>
    private static readonly string[] Ordem =
    {
        "Level_01_Itaca_Prologue",
        "Level_02_Troia",
        "Level_03_Cicones",
        "Level_04_Citera",
        "Level_05_Ciclopes",
        "Level_06_Eolo",
        "Level_07_Lestrigoes",
        "Level_08_Circe",
        "Level_09_MundoDosMortos",
        "Level_10_Sereias",
        "Level_11_CilaCaribdis",
        "Level_12_GadoDoSol",
        "Level_13_Calipso",
        "Level_14_Itaca_Return",
        "Level_15_Pretendentes",
        "Level_16_Final",
    };

    private static readonly string[] ForaDaCampanha = { "Lotofagos", "Feacios" };

    private static readonly List<string> Falhas = new List<string>();

    public static void Run()
    {
        Falhas.Clear();

        ValidarDefinicoes();
        ValidarBoot();
        ValidarBuildSettings();
        ValidarCenas();
        ValidarMigracaoDeSave();

        if (Falhas.Count == 0)
        {
            Debug.Log($"[CampaignValidation] OK — {Ordem.Length} etapas na ordem oficial.");
            EditorApplication.Exit(0);
            return;
        }

        foreach (string falha in Falhas)
        {
            Debug.LogError("[CampaignValidation] " + falha);
        }

        Debug.LogError($"[CampaignValidation] {Falhas.Count} problema(s).");
        EditorApplication.Exit(1);
    }

    private static void Falhar(string mensagem) => Falhas.Add(mensagem);

    // ------------------------------------------------------------------ assets

    private static void ValidarDefinicoes()
    {
        string[] guids = AssetDatabase.FindAssets("t:LevelDefinition");
        var porId = new Dictionary<string, LevelDefinition>();

        foreach (string guid in guids)
        {
            string caminho = AssetDatabase.GUIDToAssetPath(guid);
            var def = AssetDatabase.LoadAssetAtPath<LevelDefinition>(caminho);

            if (caminho.Contains("_ForaDaCampanha"))
            {
                continue;
            }

            if (porId.ContainsKey(def.LevelId))
            {
                Falhar($"LevelDefinition duplicado: {def.LevelId}");
                continue;
            }

            porId[def.LevelId] = def;
        }

        if (porId.Count != Ordem.Length)
        {
            Falhar($"esperava {Ordem.Length} LevelDefinition na campanha, achei {porId.Count}");
        }

        for (int i = 0; i < Ordem.Length; i++)
        {
            if (!porId.TryGetValue(Ordem[i], out LevelDefinition def))
            {
                Falhar($"LevelDefinition ausente: {Ordem[i]}");
                continue;
            }

            if (def.Order != i + 1)
            {
                Falhar($"{Ordem[i]}: order={def.Order}, esperado {i + 1}");
            }

            if (def.SceneName != Ordem[i])
            {
                Falhar($"{Ordem[i]}: sceneName={def.SceneName}");
            }

            if (string.IsNullOrEmpty(def.DisplayName))
            {
                Falhar($"{Ordem[i]}: displayName vazio");
            }
        }

        foreach (string removida in ForaDaCampanha)
        {
            if (porId.Keys.Any(id => id.Contains(removida)))
            {
                Falhar($"{removida} continua na campanha");
            }
        }
    }

    private static void ValidarBoot()
    {
        Scene boot = EditorSceneManager.OpenScene("Assets/Scenes/Boot/Boot.unity", OpenSceneMode.Single);
        CampaignManager campaign = Object.FindAnyObjectByType<CampaignManager>();

        if (campaign == null)
        {
            Falhar("Boot.unity sem CampaignManager");
            return;
        }

        List<LevelDefinition> niveis = campaign.Levels.Where(l => l != null).OrderBy(l => l.Order).ToList();

        if (niveis.Count != campaign.Levels.Count)
        {
            Falhar("CampaignManager tem entrada(s) nula(s) na lista de fases");
        }

        if (niveis.Count != Ordem.Length)
        {
            Falhar($"CampaignManager lista {niveis.Count} fases, esperado {Ordem.Length}");
            return;
        }

        for (int i = 0; i < Ordem.Length; i++)
        {
            if (niveis[i].LevelId != Ordem[i])
            {
                Falhar($"CampaignManager posicao {i + 1}: {niveis[i].LevelId}, esperado {Ordem[i]}");
            }
        }

        // A cadeia de "próxima fase" precisa ir do prólogo ao final sem buraco.
        string atual = Ordem[0];
        for (int i = 1; i < Ordem.Length; i++)
        {
            LevelDefinition proxima = campaign.GetNextLevel(atual);
            if (proxima == null || proxima.LevelId != Ordem[i])
            {
                Falhar($"GetNextLevel({atual}) = {(proxima == null ? "null" : proxima.LevelId)}, esperado {Ordem[i]}");
                return;
            }

            atual = proxima.LevelId;
        }

        if (campaign.GetNextLevel(Ordem[Ordem.Length - 1]) != null)
        {
            Falhar("a ultima etapa nao deveria ter proxima fase");
        }
    }

    private static void ValidarBuildSettings()
    {
        List<string> cenas = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => System.IO.Path.GetFileNameWithoutExtension(s.path))
            .ToList();

        foreach (string removida in ForaDaCampanha)
        {
            if (cenas.Any(c => c.Contains(removida)))
            {
                Falhar($"{removida} ainda esta no Build Settings");
            }
        }

        List<string> fases = cenas.Where(c => c.StartsWith("Level_")).ToList();

        if (!fases.SequenceEqual(Ordem))
        {
            Falhar("ordem das fases no Build Settings: " + string.Join(", ", fases));
        }

        foreach (string obrigatoria in new[] { "Boot", "MainMenu", "WorldMap", "LevelSelect", "Ending" })
        {
            if (!cenas.Contains(obrigatoria))
            {
                Falhar($"cena {obrigatoria} fora do Build Settings");
            }
        }
    }

    // ------------------------------------------------------------------ cenas

    private static void ValidarCenas()
    {
        for (int i = 0; i < Ordem.Length; i++)
        {
            string caminho = $"Assets/Scenes/Levels/{Ordem[i]}.unity";
            EditorSceneManager.OpenScene(caminho, OpenSceneMode.Single);

            var manager = Object.FindAnyObjectByType<LevelManager>();
            if (manager == null)
            {
                Falhar($"{Ordem[i]}: sem LevelManager");
                continue;
            }

            if (manager.LevelId != Ordem[i])
            {
                Falhar($"{Ordem[i]}: LevelManager.levelId = {manager.LevelId}");
            }

            var goal = Object.FindAnyObjectByType<LevelGoal>();
            if (goal == null)
            {
                Falhar($"{Ordem[i]}: sem LevelGoal");
            }

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Falhar($"{Ordem[i]}: sem objeto com tag Player");
                continue;
            }

            ValidarGeometriaLetal(Ordem[i], player.transform.position);
        }
    }

    /// <summary>
    /// A escala do transform multiplica o <c>m_Size</c> do colisor: uma zona que já
    /// tem tamanho próprio e recebe escala vira uma área gigante, e o jogador nasce
    /// dentro dela — morre em laço até o game over, sem encostar em nada.
    ///
    /// Foi exatamente o que aconteceu com a KillZone do prólogo. Estas conferências
    /// existem para esse erro não voltar em silêncio: nenhuma área letal pode conter
    /// o ponto onde Odisseu nasce, nenhuma KillZone pode encostar no chão, e o chão
    /// tem que estar embaixo do ponto de spawn.
    /// </summary>
    private static void ValidarGeometriaLetal(string fase, Vector3 spawn)
    {
        foreach (Component letal in ColetarAreasLetais())
        {
            var area = letal.GetComponent<Collider2D>();
            if (area == null)
            {
                continue;
            }

            string tipo = letal.GetType().Name;

            if (area.bounds.Contains(new Vector3(spawn.x, spawn.y, area.bounds.center.z)))
            {
                Falhar($"{fase}: {tipo} '{letal.name}' engloba o spawn do jogador " +
                       $"({spawn.x:0.#}, {spawn.y:0.#}) — bounds {area.bounds}");
            }

            if (letal is KillZone)
            {
                foreach (Collider2D chao in ColetarChao())
                {
                    if (area.bounds.Intersects(chao.bounds))
                    {
                        Falhar($"{fase}: KillZone '{letal.name}' cruza o chão '{chao.name}' " +
                               $"— {area.bounds} x {chao.bounds}");
                    }
                }
            }
        }

        if (!ExisteChaoSob(spawn))
        {
            Falhar($"{fase}: o jogador nasce em ({spawn.x:0.#}, {spawn.y:0.#}) sem chão embaixo");
        }
    }

    private static List<Component> ColetarAreasLetais()
    {
        var letais = new List<Component>();
        letais.AddRange(Object.FindObjectsByType<KillZone>(FindObjectsSortMode.None));
        letais.AddRange(Object.FindObjectsByType<TidalHazard>(FindObjectsSortMode.None));
        letais.AddRange(Object.FindObjectsByType<StormHazard>(FindObjectsSortMode.None));
        return letais;
    }

    private static List<Collider2D> ColetarChao()
    {
        int groundLayer = LayerMask.NameToLayer("Ground");
        return Object.FindObjectsByType<Collider2D>(FindObjectsSortMode.None)
            .Where(c => c.gameObject.layer == groundLayer && !c.isTrigger)
            .ToList();
    }

    /// <summary>
    /// Chão logo abaixo do spawn, por sobreposição de caixas — sem consulta de física,
    /// que fora do play mode nem sempre responde.
    /// </summary>
    private static bool ExisteChaoSob(Vector3 spawn)
    {
        const float alcance = 6f;

        foreach (Collider2D chao in ColetarChao())
        {
            Bounds b = chao.bounds;
            bool alinhado = spawn.x >= b.min.x && spawn.x <= b.max.x;
            bool abaixo = b.max.y <= spawn.y + 0.5f && b.max.y >= spawn.y - alcance;

            if (alinhado && abaixo)
            {
                return true;
            }
        }

        return false;
    }

    // ------------------------------------------------------------------ save

    private static void ValidarMigracaoDeSave()
    {
        // Simula um save da numeracao antiga: jogador que tinha zerado ate Circe (7).
        var antigo = new SaveData
        {
            saveVersion = 0,
            completedLevelIds = new List<string>
            {
                "Level_01_Troia", "Level_02_Cicones", "Level_03_Lotofagos",
                "Level_04_Ciclopes", "Level_05_Eolo", "Level_06_Lestrigoes", "Level_07_Circe",
            },
            unlockedLevelIds = new List<string>
            {
                "Level_01_Troia", "Level_02_Cicones", "Level_03_Lotofagos",
                "Level_04_Ciclopes", "Level_05_Eolo", "Level_06_Lestrigoes",
                "Level_07_Circe", "Level_08_MundoDosMortos",
            },
            bestScores = new List<LevelScoreEntry>
            {
                new LevelScoreEntry { levelId = "Level_01_Troia", score = 3 },
                new LevelScoreEntry { levelId = "Level_03_Lotofagos", score = 2 },
            },
            totalCollectibles = 9,
        };

        PlayerPrefs.SetString("Odisseia.Save", JsonUtility.ToJson(antigo));
        SaveData migrado = SaveSystem.Load();

        if (migrado.saveVersion != SaveSystem.CurrentSaveVersion)
        {
            Falhar($"save migrado ficou na versao {migrado.saveVersion}");
        }

        if (migrado.totalCollectibles != 9)
        {
            Falhar("a migracao perdeu os coletaveis");
        }

        foreach (string esperado in new[]
                 {
                     "Level_02_Troia", "Level_03_Cicones", "Level_05_Ciclopes",
                     "Level_06_Eolo", "Level_07_Lestrigoes", "Level_08_Circe",
                 })
        {
            if (!migrado.completedLevelIds.Contains(esperado))
            {
                Falhar($"save migrado sem a fase concluida {esperado}");
            }
        }

        foreach (string removida in migrado.completedLevelIds
                     .Concat(migrado.unlockedLevelIds)
                     .Concat(migrado.bestScores.Select(s => s.levelId)))
        {
            if (ForaDaCampanha.Any(f => removida.Contains(f)))
            {
                Falhar($"save migrado ainda cita {removida}");
            }

            if (!Ordem.Contains(removida))
            {
                Falhar($"save migrado cita id desconhecido: {removida}");
            }
        }

        // Quem parou em Circe (8) precisa achar o Mundo dos Mortos (9) aberto.
        if (!migrado.unlockedLevelIds.Contains("Level_09_MundoDosMortos"))
        {
            Falhar("save migrado nao desbloqueou a etapa seguinte");
        }

        // E o prologo, que nem existia antes, tem que estar disponivel.
        if (!migrado.unlockedLevelIds.Contains("Level_01_Itaca_Prologue"))
        {
            Falhar("save migrado nao abriu o prologo");
        }

        // Jogo novo: so a primeira etapa aberta.
        PlayerPrefs.DeleteKey("Odisseia.Save");
        SaveData novo = SaveSystem.Load();

        if (novo.unlockedLevelIds.Count != 1 || novo.unlockedLevelIds[0] != Ordem[0])
        {
            Falhar("jogo novo nao comeca so com " + Ordem[0]);
        }

        if (novo.completedLevelIds.Count != 0)
        {
            Falhar("jogo novo ja nasce com fase concluida");
        }

        PlayerPrefs.DeleteKey("Odisseia.Save");
    }
}
