using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Odisseia.Systems;

namespace Odisseia.Core
{
    /// <summary>
    /// Estado de progressão da campanha: fases concluídas/desbloqueadas, coletáveis
    /// totais e melhores pontuações. Persiste entre cenas (mesmo objeto do
    /// GameManager) e é apoiado pelo SaveSystem.
    /// </summary>
    public class CampaignManager : MonoBehaviour
    {
        public static CampaignManager Instance { get; private set; }

        [SerializeField] private List<LevelDefinition> levels = new List<LevelDefinition>();

        private SaveData saveData;

        public IReadOnlyList<LevelDefinition> Levels => levels;
        public int TotalCollectibles => saveData.totalCollectibles;

        public float MasterVolume
        {
            get => saveData.masterVolume;
            set
            {
                saveData.masterVolume = value;
                SaveSystem.Save(saveData);
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                return;
            }

            Instance = this;
            saveData = SaveSystem.Load();
        }

        public bool IsUnlocked(string levelId)
        {
            return saveData.unlockedLevelIds.Contains(levelId);
        }

        public bool IsCompleted(string levelId)
        {
            return saveData.completedLevelIds.Contains(levelId);
        }

        public LevelDefinition GetLevel(string levelId)
        {
            return levels.FirstOrDefault(level => level.LevelId == levelId);
        }

        public LevelDefinition GetNextLevel(string levelId)
        {
            List<LevelDefinition> ordered = levels.OrderBy(level => level.Order).ToList();
            int index = ordered.FindIndex(level => level.LevelId == levelId);

            if (index < 0 || index + 1 >= ordered.Count)
            {
                return null;
            }

            return ordered[index + 1];
        }

        public int GetBestScore(string levelId)
        {
            LevelScoreEntry entry = saveData.bestScores.FirstOrDefault(score => score.levelId == levelId);
            return entry != null ? entry.score : 0;
        }

        public void CompleteLevel(string levelId, int collectiblesGained, int score)
        {
            if (!saveData.completedLevelIds.Contains(levelId))
            {
                saveData.completedLevelIds.Add(levelId);
            }

            saveData.totalCollectibles += collectiblesGained;
            UpdateBestScore(levelId, score);

            LevelDefinition next = GetNextLevel(levelId);
            if (next != null && !saveData.unlockedLevelIds.Contains(next.LevelId))
            {
                saveData.unlockedLevelIds.Add(next.LevelId);
            }

            SaveSystem.Save(saveData);
        }

        public void StartNewGame()
        {
            SaveSystem.DeleteSave();
            saveData = SaveSystem.Load();
            DecisionFlags.Reset();
        }

        private void UpdateBestScore(string levelId, int score)
        {
            LevelScoreEntry entry = saveData.bestScores.FirstOrDefault(s => s.levelId == levelId);
            if (entry == null)
            {
                saveData.bestScores.Add(new LevelScoreEntry { levelId = levelId, score = score });
            }
            else if (score > entry.score)
            {
                entry.score = score;
            }
        }
    }
}
