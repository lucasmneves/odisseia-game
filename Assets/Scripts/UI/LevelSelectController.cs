using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Odisseia.Core;
using Odisseia.Systems;

namespace Odisseia.UI
{
    /// <summary>
    /// Tela "Jornada de Odisseu": lista as fases da campanha (via CampaignManager),
    /// mostrando bloqueada/concluída, e carrega a fase escolhida ao clicar.
    /// As entradas são construídas em runtime porque dependem do save do jogador.
    /// </summary>
    public class LevelSelectController : MonoBehaviour
    {
        [SerializeField] private Transform listContainer;
        [SerializeField] private Button backButton;
        [SerializeField] private string backSceneName = SceneLoader.MainMenu;

        private void Awake()
        {
            backButton?.onClick.AddListener(() => SceneLoader.Load(backSceneName));
            PopulateList();
        }

        private void PopulateList()
        {
            CampaignManager campaign = CampaignManager.Instance;
            if (campaign == null || listContainer == null)
            {
                return;
            }

            foreach (LevelDefinition level in campaign.Levels.OrderBy(l => l.Order))
            {
                bool unlocked = campaign.IsUnlocked(level.LevelId);
                bool completed = campaign.IsCompleted(level.LevelId);
                string status = completed ? " ✓" : (unlocked ? "" : " 🔒");
                string label = $"{level.Order}. {level.DisplayName}{status}";
                string sceneName = level.SceneName;

                Button button = CreateEntryButton(label);
                button.interactable = unlocked;
                button.onClick.AddListener(() => SceneLoader.Load(sceneName));
            }
        }

        private Button CreateEntryButton(string label)
        {
            var go = new GameObject("LevelEntry");
            go.transform.SetParent(listContainer, false);
            go.AddComponent<RectTransform>();

            var image = go.AddComponent<Image>();
            image.color = new Color(0.2f, 0.35f, 0.55f);

            var button = go.AddComponent<Button>();
            button.targetGraphic = image;

            var textGO = new GameObject("Label");
            textGO.transform.SetParent(go.transform, false);
            var textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var text = textGO.AddComponent<Text>();
            text.text = label;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 20;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;

            return button;
        }
    }
}
