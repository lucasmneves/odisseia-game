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
        public List<string> completedLevelIds = new List<string>();
        public List<string> unlockedLevelIds = new List<string>();
        public List<LevelScoreEntry> bestScores = new List<LevelScoreEntry>();
        public int totalCollectibles;
        public float masterVolume = 1f;
    }

    /// <summary>
    /// Persistência local via PlayerPrefs — funciona em WebGL (usa o localStorage do
    /// navegador) sem precisar de backend nem de arquivos em disco.
    /// </summary>
    public static class SaveSystem
    {
        private const string SaveKey = "Odisseia.Save";
        private const string FirstLevelId = "Level_01_Troia";

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
            return data ?? CreateDefault();
        }

        public static void Save(SaveData data)
        {
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();
        }

        public static void DeleteSave()
        {
            PlayerPrefs.DeleteKey(SaveKey);
        }

        private static SaveData CreateDefault()
        {
            var data = new SaveData();
            data.unlockedLevelIds.Add(FirstLevelId);
            return data;
        }
    }
}
