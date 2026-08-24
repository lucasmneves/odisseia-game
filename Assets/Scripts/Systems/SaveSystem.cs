using System;
using System.Collections.Generic;
using UnityEngine;

namespace Odisseia.Systems
{
    [Serializable]
    public class LevelScoreEntry
    {
        public string levelId;
        public int score;
    }

    [Serializable]
    public class SaveData
    {
        /// <summary>
        /// Versão do formato. Save antigo grava 0 aqui (o campo não existia), e é isso
        /// que o <see cref="SaveSystem"/> usa para saber que precisa migrar os ids.
        /// </summary>
        public int saveVersion;

        public List<string> completedLevelIds = new List<string>();
        public List<string> unlockedLevelIds = new List<string>();
        public List<LevelScoreEntry> bestScores = new List<LevelScoreEntry>();
        public int totalCollectibles;
        public float masterVolume = 1f;
    }

    /// <summary>
    /// Persistência local via PlayerPrefs — funciona em WebGL (usa o localStorage do
    /// navegador) sem precisar de backend nem de arquivos em disco.
    ///
    /// O save guarda ids de fase, não índices. Quando a ordem da campanha muda, os ids
    /// antigos continuam no save do jogador: por isso existe a migração abaixo, que
    /// renumera o que ele já conquistou em vez de apagar o progresso.
    /// </summary>
    public static class SaveSystem
    {
        private const string SaveKey = "Odisseia.Save";

        /// <summary>
        /// 4 = campanha de 16 etapas começando no prólogo em Ítaca, com Cítera na 4
        /// e Lotófagos/Feácios fora da campanha.
        /// </summary>
        public const int CurrentSaveVersion = 4;

        public const string FirstLevelId = "Level_01_Itaca_Prologue";

        /// <summary>
        /// Id antigo → id novo. Fases que saíram da campanha (Lotófagos, Feácios)
        /// apontam para null: o que o jogador fez nelas é descartado, mas nada além
        /// disso se perde.
        /// </summary>
        private static readonly Dictionary<string, string> LegacyLevelIds = new Dictionary<string, string>
        {
            { "Level_01_Troia", "Level_02_Troia" },
            { "Level_02_Cicones", "Level_03_Cicones" },
            { "Level_03_Lotofagos", null },
            { "Level_04_Ciclopes", "Level_05_Ciclopes" },
            { "Level_05_Eolo", "Level_06_Eolo" },
            { "Level_06_Lestrigoes", "Level_07_Lestrigoes" },
            { "Level_07_Circe", "Level_08_Circe" },
            { "Level_08_MundoDosMortos", "Level_09_MundoDosMortos" },
            { "Level_09_Sereias", "Level_10_Sereias" },
            { "Level_10_CilaCaribdis", "Level_11_CilaCaribdis" },
            { "Level_11_GadoDoSol", "Level_12_GadoDoSol" },
            { "Level_12_Calipso", "Level_13_Calipso" },
            { "Level_13_Feacios", null },
            { "Level_14_Itaca", "Level_14_Itaca_Return" },
            { "Level_15_Pretendentes", "Level_15_Pretendentes" },
            { "Level_16_Final", "Level_16_Final" },
        };

        /// <summary>
        /// Ordem oficial da campanha, usada só pela migração para reconstruir o que o
        /// jogador tem desbloqueado. A fonte de verdade em jogo continua sendo a lista
        /// de <c>LevelDefinition</c> do CampaignManager.
        /// </summary>
        private static readonly string[] CampaignOrder =
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

        public static bool HasSave()
        {
            return PlayerPrefs.HasKey(SaveKey);
        }

        public static SaveData Load()
        {
            if (!PlayerPrefs.HasKey(SaveKey))
            {
                return CreateDefault();
            }

            string json = PlayerPrefs.GetString(SaveKey);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            if (data == null)
            {
                return CreateDefault();
            }

            if (Migrate(data))
            {
                Save(data);
            }

            return data;
        }

        public static void Save(SaveData data)
        {
            data.saveVersion = CurrentSaveVersion;
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
        }

        public static void DeleteSave()
        {
            PlayerPrefs.DeleteKey(SaveKey);
        }

        /// <summary>
        /// Traz um save antigo para a numeração atual. Devolve true se mexeu em algo
        /// (aí o chamador regrava).
        /// </summary>
        private static bool Migrate(SaveData data)
        {
            if (data.saveVersion >= CurrentSaveVersion)
            {
                return false;
            }

            data.completedLevelIds = TranslateIds(data.completedLevelIds);
            data.unlockedLevelIds = TranslateIds(data.unlockedLevelIds);

            var scores = new List<LevelScoreEntry>();
            foreach (LevelScoreEntry entry in data.bestScores)
            {
                string translated = Translate(entry.levelId);
                if (translated == null)
                {
                    continue;
                }

                entry.levelId = translated;
                scores.Add(entry);
            }

            data.bestScores = scores;

            // O prólogo é novo e vem antes de tudo: quem já jogava merece encontrá-lo
            // aberto, senão a campanha migrada começaria travada.
            EnsureUnlockChain(data);

            data.saveVersion = CurrentSaveVersion;
            return true;
        }

        private static List<string> TranslateIds(List<string> ids)
        {
            var result = new List<string>();

            foreach (string id in ids)
            {
                string translated = Translate(id);
                if (translated != null && !result.Contains(translated))
                {
                    result.Add(translated);
                }
            }

            return result;
        }

        /// <summary>
        /// Id atual para um id salvo. Ids que já estão no formato novo passam direto;
        /// fases removidas da campanha devolvem null.
        /// </summary>
        private static string Translate(string levelId)
        {
            if (string.IsNullOrEmpty(levelId))
            {
                return null;
            }

            if (LegacyLevelIds.TryGetValue(levelId, out string translated))
            {
                return translated;
            }

            return Array.IndexOf(CampaignOrder, levelId) >= 0 ? levelId : null;
        }

        /// <summary>
        /// Garante que a primeira fase esteja aberta e que exista a próxima etapa
        /// desbloqueada depois da concluída mais avançada — a campanha nova tem duas
        /// paradas que o save antigo não conhecia (o prólogo e Cítera), e sem isso o
        /// jogador ficaria parado num nó sem continuação.
        /// </summary>
        private static void EnsureUnlockChain(SaveData data)
        {
            if (!data.unlockedLevelIds.Contains(FirstLevelId))
            {
                data.unlockedLevelIds.Insert(0, FirstLevelId);
            }

            int furthestCompleted = -1;
            foreach (string id in data.completedLevelIds)
            {
                furthestCompleted = Mathf.Max(furthestCompleted, Array.IndexOf(CampaignOrder, id));
            }

            for (int i = 0; i <= furthestCompleted + 1 && i < CampaignOrder.Length; i++)
            {
                if (!data.unlockedLevelIds.Contains(CampaignOrder[i]))
                {
                    data.unlockedLevelIds.Add(CampaignOrder[i]);
                }
            }
        }

        private static SaveData CreateDefault()
        {
            var data = new SaveData();
            data.saveVersion = CurrentSaveVersion;
            data.unlockedLevelIds.Add(FirstLevelId);
            return data;
        }
    }
}
