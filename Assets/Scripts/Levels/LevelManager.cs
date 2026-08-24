using UnityEngine;
using Odisseia.Core;
using Odisseia.Systems;
using Odisseia.WorldMap;

namespace Odisseia.Levels
{
    /// <summary>
    /// Identifica a fase atual da cena e reporta sua conclusão para o
    /// CampaignManager (que por sua vez persiste via SaveSystem). Sem
    /// CampaignManager na cena (ex.: testar a fase sem passar pelo Boot),
    /// a fase continua jogável normalmente — só o progresso não é salvo.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] private string levelId = "Level_01_Itaca_Prologue";

        public string LevelId => levelId;
        public bool IsCompleted { get; private set; }

        public void CompleteLevel()
        {
            if (IsCompleted)
            {
                return;
            }

            IsCompleted = true;

            int collectibles = CollectibleCounter.Count;
            CampaignManager.Instance?.CompleteLevel(levelId, collectibles, collectibles);

            // Diz ao mapa onde colocar Odisseu e que houve conclusão agora. O progresso
            // em si já foi salvo acima — isto é só o recado de navegação.
            WorldMapSession.ReportLevelFinished(levelId, completed: true);
        }
    }
}
