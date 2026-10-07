using UnityEngine;
using UnityEngine.UI;
using Odisseia.Core;
using Odisseia.Player;
using Odisseia.Systems;

namespace Odisseia.UI
{
    /// <summary>
    /// HUD simples: vida do jogador, contador de coletáveis, munição do arco e o estado do escudo.
    /// </summary>
    public class HUD : MonoBehaviour
    {
        [SerializeField] private Text healthText;
        [SerializeField] private Text collectiblesText;
        [SerializeField] private Text arrowsText;
        [SerializeField] private Text livesText;
        [SerializeField] private Text experienceText;
        [SerializeField] private HealthSystem playerHealth;

        private PlayerBow playerBow;
        private PlayerShield playerShield;

        private void Awake()
        {
            // O arco vive no mesmo GameObject do HealthSystem que a cena já referencia,
            // então não é preciso ligar mais nada no Inspector das 16 fases.
            if (playerHealth != null)
            {
                playerBow = playerHealth.GetComponent<PlayerBow>();
                playerShield = playerHealth.GetComponent<PlayerShield>();
            }

            // As cenas existentes só têm os Texts de vida e coletáveis; os demais são
            // criados em runtime, empilhados abaixo, para não editar o HUD Canvas das
            // 16 fases.
            int slot = 1;
            if (collectiblesText != null)
            {
                if (arrowsText == null && playerBow != null)
                {
                    arrowsText = CreateStackedLabel(collectiblesText, slot++, "ArrowsText");
                }

                if (livesText == null)
                {
                    livesText = CreateStackedLabel(collectiblesText, slot++, "LivesText");
                }

                if (experienceText == null)
                {
                    experienceText = CreateStackedLabel(collectiblesText, slot, "ExperienceText");
                }
                slot++;

                // Escudo (13B.3): só o ícone, na linha abaixo do XP — o PlayerShield não tem número (nem stamina nem
                // durabilidade) para mostrar. A linha é um Text vazio para o ícone sair pelo mesmo AddIcon dos outros;
                // criada ANTES dos ícones, que empurram os textos para a direita.
                if (playerShield != null)
                {
                    shieldRow = CreateStackedLabel(collectiblesText, slot, "ShieldRow");
                    shieldRow.text = string.Empty;
                }
            }

            // Ícone à esquerda de cada contador (Asset Completion PXL-019): o ícone É o rótulo. Sem ele o texto volta ao
            // glifo/palavra de antes — ver Rotulo().
            temIcone[0] = AddIcon(healthText, "icon_health");
            temIcone[1] = AddIcon(collectiblesText, "icon_coin");
            temIcone[2] = AddIcon(arrowsText, "icon_arrows");
            temIcone[3] = AddIcon(livesText, "icon_lives");
            temIcone[4] = AddIcon(experienceText, "icon_xp");

            if (shieldRow != null && AddIcon(shieldRow, "icon_shield"))
            {
                shieldIcon = shieldRow.transform.parent.Find("Icon_icon_shield").GetComponent<Image>();
                posicaoDoEscudo = shieldIcon.rectTransform.anchoredPosition;
                shieldIcon.gameObject.SetActive(false); // o LateUpdate liga quando o escudo estiver disponível
            }
        }

        private Text shieldRow;
        private Image shieldIcon;
        private int escudoMostrado = -1;
        private float flashDoBloqueio;
        private Vector2 posicaoDoEscudo;

        /// <summary>
        /// Defendendo (tecla, gamepad ou toque — a mesma ação), o ícone sobe 3 unidades: o escudo erguido. Pronto, fica opaco
        /// como os outros ícones (meio apagado ele sumia no muro escuro da Fase 15 e no bege das Sereias). Passo inteiro,
        /// para a pixel art não borrar. Só reflete o estado do PlayerShield.
        /// </summary>
        private const float ErguidoEmUnidades = 3f;

        /// <summary>
        /// Acompanha o escudo a cada quadro em vez de por evento: o PlayerShield acha a ação de input no Awake dele, que
        /// pode rodar depois do deste HUD. Só reescreve a cor quando o estado muda (ou durante o brilho do bloqueio).
        /// </summary>
        private void RefreshShield()
        {
            if (shieldIcon == null || playerShield == null)
            {
                return;
            }

            bool disponivel = playerShield.IsAvailable;
            int estado = !disponivel ? 0 : playerShield.IsBlocking ? 2 : 1;
            bool brilhando = flashDoBloqueio > 0f;
            if (brilhando)
            {
                flashDoBloqueio -= Time.unscaledDeltaTime;
            }

            if (estado == escudoMostrado && !brilhando)
            {
                return;
            }

            escudoMostrado = brilhando ? -1 : estado; // durante o brilho, reaplica no quadro seguinte
            shieldIcon.gameObject.SetActive(disponivel);
            shieldIcon.color = brilhando && flashDoBloqueio > 0f ? UITheme.TextAccent : Color.white;
            shieldIcon.rectTransform.anchoredPosition = posicaoDoEscudo + new Vector2(0f, estado == 2 ? ErguidoEmUnidades : 0f);
        }

        /// <summary>Golpe absorvido pela defesa: o ícone pisca dourado por um instante.</summary>
        private void OnShieldBlocked(int absorvido)
        {
            flashDoBloqueio = 0.15f;
            escudoMostrado = -1;
        }

        private const string PastaDosIcones = "Odisseia/UI/HUD/";

        /// <summary>Largura reservada ao ícone + respiro antes do número, em unidades do Canvas (1280×720).</summary>
        private const float LarguraDoIcone = 36f;

        /// <summary>vida, coletáveis, flechas, vidas, XP — se o ícone daquele contador entrou.</summary>
        private readonly bool[] temIcone = new bool[5];

        /// <summary>
        /// Põe o ícone à esquerda do Text, na mesma âncora, e empurra o texto para a direita. Tamanho NATIVO do sprite
        /// (1 px = 1 unidade do Canvas de referência): os ícones têm 23–32 px e a linha 32, e pixel art reescalada por
        /// fator quebrado fica irregular.
        /// </summary>
        private static bool AddIcon(Text text, string icone)
        {
            if (text == null)
            {
                return false;
            }

            Sprite sprite = Resources.Load<Sprite>(PastaDosIcones + icone);
            if (sprite == null)
            {
                return false;
            }

            var source = (RectTransform)text.transform;
            var go = new GameObject("Icon_" + icone, typeof(RectTransform));
            go.transform.SetParent(source.parent, false);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = source.anchorMin;
            rect.anchorMax = source.anchorMax;
            rect.pivot = new Vector2(0f, 0.5f);
            rect.sizeDelta = new Vector2(sprite.rect.width, sprite.rect.height);
            // Centro vertical da linha do texto, qualquer que seja o pivô dele: P.y + altura × (0,5 − pivô.y).
            rect.anchoredPosition = source.anchoredPosition
                + new Vector2(0f, source.sizeDelta.y * (0.5f - source.pivot.y));

            var image = go.AddComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            image.preserveAspect = true;

            source.anchoredPosition += new Vector2(LarguraDoIcone, 0f);
            return true;
        }

        /// <summary>O prefixo antigo (glifo ou palavra) só quando o contador ficou sem ícone.</summary>
        private string Rotulo(int indice, string semIcone) => temIcone[indice] ? string.Empty : semIcone;

        private void OnEnable()
        {
            if (playerHealth != null)
            {
                playerHealth.Damaged += OnPlayerDamaged;
                playerHealth.Died += OnPlayerDied;
            }

            if (playerBow != null)
            {
                playerBow.ArrowsChanged += OnArrowsChanged;
                playerBow.OutOfArrows += OnOutOfArrows;
            }

            if (playerShield != null)
            {
                playerShield.Blocked += OnShieldBlocked;
            }

            CollectibleCounter.CountChanged += OnCollectiblesChanged;
            LivesCounter.Changed += OnLivesChanged;
            ExperienceCounter.Changed += OnExperienceChanged;

            RefreshHealth();
            RefreshCollectibles();
            RefreshArrows();
            RefreshLives();
            RefreshExperience();
        }

        private void OnDisable()
        {
            if (playerHealth != null)
            {
                playerHealth.Damaged -= OnPlayerDamaged;
                playerHealth.Died -= OnPlayerDied;
            }

            if (playerBow != null)
            {
                playerBow.ArrowsChanged -= OnArrowsChanged;
                playerBow.OutOfArrows -= OnOutOfArrows;
            }

            if (playerShield != null)
            {
                playerShield.Blocked -= OnShieldBlocked;
            }

            CollectibleCounter.CountChanged -= OnCollectiblesChanged;
            LivesCounter.Changed -= OnLivesChanged;
            ExperienceCounter.Changed -= OnExperienceChanged;
        }

        private void OnLivesChanged(int current) => RefreshLives();

        private void OnExperienceChanged(int total, int towardNext) => RefreshExperience();

        private void RefreshLives()
        {
            if (livesText != null)
            {
                livesText.text = $"{Rotulo(3, "Vidas ")}{LivesCounter.Current}";
            }
        }

        private void RefreshExperience()
        {
            if (experienceText != null)
            {
                experienceText.text =
                    $"{Rotulo(4, "XP ")}{ExperienceCounter.TowardNextLife}/{ExperienceCounter.ExperiencePerLife}";
            }
        }

        private void OnArrowsChanged(int current, int max)
        {
            RefreshArrows();
        }

        /// <summary>Pisca o contador em vermelho ao tentar atirar sem munição.</summary>
        private void OnOutOfArrows()
        {
            if (arrowsText != null)
            {
                arrowsText.color = UITheme.Health;
                CancelInvoke(nameof(RestoreArrowsColor));
                Invoke(nameof(RestoreArrowsColor), 0.4f);
            }
        }

        private void RestoreArrowsColor()
        {
            if (arrowsText != null)
            {
                arrowsText.color = playerBow != null && !playerBow.HasArrows
                    ? UITheme.TextSecondary
                    : UITheme.TextPrimary;
            }
        }

        private void RefreshArrows()
        {
            if (arrowsText == null)
            {
                return;
            }

            if (playerBow == null)
            {
                arrowsText.gameObject.SetActive(false);
                return;
            }

            arrowsText.text = $"{Rotulo(2, "➶ ")}{playerBow.CurrentArrows}/{playerBow.MaxArrows}";
            RestoreArrowsColor();
        }

        /// <summary>
        /// Clona o posicionamento de um Text existente, deslocado <paramref name="slot"/>
        /// linhas abaixo. Mantém a HUD alinhada sem depender de layout group.
        /// </summary>
        private static Text CreateStackedLabel(Text reference, int slot, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(reference.transform.parent, false);

            var source = (RectTransform)reference.transform;
            float step = source.sizeDelta.y + 4f;

            var rect = (RectTransform)go.transform;
            rect.anchorMin = source.anchorMin;
            rect.anchorMax = source.anchorMax;
            rect.pivot = source.pivot;
            rect.sizeDelta = source.sizeDelta;
            rect.anchoredPosition = source.anchoredPosition + new Vector2(0f, -step * slot);

            var text = go.AddComponent<Text>();
            text.font = reference.font;
            text.fontSize = reference.fontSize;
            text.alignment = reference.alignment;
            text.color = reference.color;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        private void OnPlayerDamaged(int amount, int currentHealth)
        {
            RefreshHealth();
        }

        private void OnPlayerDied()
        {
            RefreshHealth();
        }

        private void OnCollectiblesChanged(int count)
        {
            RefreshCollectibles();
        }

        private int vidaMostrada = -1;

        /// <summary>
        /// A vida também é conferida a cada quadro, além dos eventos: (1) o HUD pode inicializar ANTES do HealthSystem do
        /// Odisseu (ordem entre objetos não é garantida) e lia 0 — visto no WebGL: "0/100" no começo da fase até o primeiro
        /// dano; (2) o respawn devolve a vida por ResetHealth(), que não dispara evento. Só reescreve quando o número muda.
        /// </summary>
        private void LateUpdate()
        {
            if (playerHealth != null && playerHealth.CurrentHealth != vidaMostrada)
            {
                RefreshHealth();
            }

            RefreshShield();
        }

        private void RefreshHealth()
        {
            if (healthText != null && playerHealth != null)
            {
                vidaMostrada = playerHealth.CurrentHealth;
                healthText.text = $"{Rotulo(0, "♥ ")}{playerHealth.CurrentHealth}/{playerHealth.MaxHealth}";
            }
        }

        private void RefreshCollectibles()
        {
            if (collectiblesText != null)
            {
                collectiblesText.text = $"{Rotulo(1, "★ ")}{CollectibleCounter.Count}";
            }
        }
    }
}
