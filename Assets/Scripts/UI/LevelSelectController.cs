using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Odisseia.Core;
using Odisseia.Systems;

namespace Odisseia.UI
{
    /// <summary>
    /// Tela "A jornada": a lista das etapas da campanha à esquerda e o mapa do
    /// Mediterrâneo à direita.
    ///
    /// As entradas são construídas em runtime porque dependem do save do jogador — o
    /// que está desbloqueado e o que já foi concluído muda a cada partida. A ordem vem
    /// do <see cref="CampaignManager"/>, que é a fonte de verdade da campanha; esta
    /// tela nunca lista fases por conta própria.
    ///
    /// A montagem da cena (coluna, rolagem, mapa) fica no LevelSelectSceneBuilder,
    /// script de Editor. Aqui só entra o que depende do progresso.
    /// </summary>
    public class LevelSelectController : MonoBehaviour, ICancelHandler
    {
        [SerializeField] private Transform listContainer;
        [SerializeField] private Button backButton;
        [SerializeField] private string backSceneName = SceneLoader.MainMenu;

        [Header("Entradas")]
        [SerializeField] private float entryHeight = 44f;

        private readonly List<Button> entries = new List<Button>();
        private MenuNavigator navigator;

        private void Awake()
        {
            EventSystemBootstrap.EnsureExists();

            backButton?.onClick.AddListener(GoBack);
            PopulateList();
            SetupNavigation();
        }

        private void GoBack()
        {
            AudioManager.PlayUiCancel();
            SceneLoader.Load(backSceneName);
        }

        /// <summary>ESC / botão B: volta ao menu principal.</summary>
        public void OnCancel(BaseEventData eventData) => GoBack();

        private void PopulateList()
        {
            if (listContainer == null)
            {
                return;
            }

            CampaignManager campaign = CampaignManager.Instance;
            if (campaign == null)
            {
                // Cena aberta direto no Editor, sem passar pelo Boot: não existe
                // campanha carregada e não há de onde tirar a lista. Dizer isso é
                // melhor do que mostrar uma coluna vazia, que parece defeito.
                ShowEmptyState();
                return;
            }

            foreach (LevelDefinition level in campaign.Levels.Where(l => l != null).OrderBy(l => l.Order))
            {
                bool unlocked = campaign.IsUnlocked(level.LevelId);
                bool completed = campaign.IsCompleted(level.LevelId);
                string sceneName = level.SceneName;

                Button button = CreateEntryButton(level, unlocked, completed);
                button.interactable = unlocked;
                button.onClick.AddListener(() => SceneLoader.LoadWithLoadingScreen(sceneName, level.DisplayName));

                entries.Add(button);
            }
        }

        /// <summary>
        /// Foco inicial na etapa mais avançada que dá para jogar — é onde o jogador
        /// parou, e não faz sentido obrigá-lo a rolar a lista inteira até lá.
        /// </summary>
        private void SetupNavigation()
        {
            navigator = gameObject.AddComponent<MenuNavigator>();

            var itens = new List<Button>(entries);
            if (backButton != null)
            {
                itens.Add(backButton);
            }

            navigator.SetItems(itens);
            navigator.SetCancelTarget(this);

            Button alvo = entries.LastOrDefault(b => b.interactable) ?? backButton;
            navigator.Select(alvo);
        }

        private void ShowEmptyState()
        {
            Debug.LogWarning("[LevelSelect] Sem CampaignManager — abra o jogo pela cena Boot " +
                             "para a lista de fases ser montada.", this);

            var go = new GameObject("EmptyState");
            go.transform.SetParent(listContainer, false);
            go.AddComponent<RectTransform>();

            var size = go.AddComponent<LayoutElement>();
            size.preferredHeight = 120f;
            size.minHeight = 120f;

            Text aviso = CreateLabel(go.transform, "Label",
                Localization.Get("ui.levelSelect.noCampaign"),
                TextAnchor.MiddleCenter);
            aviso.color = UITheme.TextSecondary;
            aviso.horizontalOverflow = HorizontalWrapMode.Wrap;
        }

        private Button CreateEntryButton(LevelDefinition level, bool unlocked, bool completed)
        {
            var go = new GameObject("LevelEntry_" + level.LevelId);
            go.transform.SetParent(listContainer, false);
            go.AddComponent<RectTransform>();

            // O VerticalLayoutGroup controla a largura; a altura vem daqui.
            var size = go.AddComponent<LayoutElement>();
            size.preferredHeight = entryHeight;
            size.minHeight = entryHeight;

            var image = go.AddComponent<Image>();
            image.color = UITheme.ButtonNormal;

            var button = go.AddComponent<Button>();
            button.targetGraphic = image;

            // Número da etapa, numa faixa fixa à esquerda, para os nomes alinharem
            // mesmo com "1." e "16." tendo larguras diferentes.
            Text ordem = CreateLabel(go.transform, "Order", $"{level.Order:00}", TextAnchor.MiddleCenter);
            var ordemRect = (RectTransform)ordem.transform;
            ordemRect.anchorMin = new Vector2(0f, 0f);
            ordemRect.anchorMax = new Vector2(0f, 1f);
            ordemRect.pivot = new Vector2(0f, 0.5f);
            ordemRect.sizeDelta = new Vector2(44f, 0f);
            ordemRect.anchoredPosition = Vector2.zero;
            ordem.color = UITheme.TextAccent;

            Text nome = CreateLabel(go.transform, "Label", level.DisplayName, TextAnchor.MiddleLeft);
            var nomeRect = (RectTransform)nome.transform;
            nomeRect.anchorMin = new Vector2(0f, 0f);
            nomeRect.anchorMax = new Vector2(1f, 1f);
            nomeRect.offsetMin = new Vector2(48f, 0f);
            nomeRect.offsetMax = new Vector2(-34f, 0f);

            // Marca de estado à direita: concluída ou bloqueada.
            string marca = completed ? "✓" : unlocked ? string.Empty : "✕";
            if (!string.IsNullOrEmpty(marca))
            {
                Text estado = CreateLabel(go.transform, "State", marca, TextAnchor.MiddleCenter);
                var estadoRect = (RectTransform)estado.transform;
                estadoRect.anchorMin = new Vector2(1f, 0f);
                estadoRect.anchorMax = new Vector2(1f, 1f);
                estadoRect.pivot = new Vector2(1f, 0.5f);
                estadoRect.sizeDelta = new Vector2(32f, 0f);
                estadoRect.anchoredPosition = Vector2.zero;
                estado.color = completed ? UITheme.Collectible : UITheme.TextSecondary;
            }

            // O MenuButton assume cor, destaque e som — os mesmos do menu principal,
            // inclusive o cinza de desabilitado para as etapas ainda bloqueadas.
            go.AddComponent<MenuButton>();

            return button;
        }

        private static Text CreateLabel(Transform parent, string name, string content, TextAnchor alignment)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var text = go.AddComponent<Text>();
            text.font = UITheme.Font;
            text.text = content;
            text.fontSize = UITheme.FontBody;
            text.alignment = alignment;
            text.color = UITheme.TextPrimary;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        /// <summary>Etapas listadas, na ordem da campanha — usado pela sonda.</summary>
        public IReadOnlyList<Button> Entries => entries;
    }
}
