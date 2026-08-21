using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Odisseia.Player;
using Odisseia.Systems;

namespace Odisseia.UI
{
    /// <summary>
    /// Controles virtuais de toque (setas de movimento contínuo + pulo/ataque
    /// discretos). Cria-se sozinho na primeira utilização e persiste entre cenas, no
    /// mesmo padrão de ScreenFader/AudioManager.
    ///
    /// Não existe uma segunda implementação de gameplay aqui. Cada botão é um
    /// OnScreenButton do próprio Input System apontando para o MESMO control path de
    /// teclado que a ação correspondente já usa (ex.: Jump -> "&lt;Keyboard&gt;/space").
    /// O toque simula a tecla sendo pressionada; PlayerController e PlayerCombat
    /// continuam lendo só da InputAction, sem nenhuma ideia de que o toque existe:
    ///
    ///   Teclado real  ---\                                  /--- PlayerController
    ///                     >-- InputAction (Move/Jump/Attack) --
    ///   OnScreenButton ---/          (Input System)             \--- PlayerCombat
    /// </summary>
    public class MobileControlsRoot : MonoBehaviour
    {
        private const float ReferenceWidth = 960f;
        private const float ReferenceHeight = 600f;

        private static MobileControlsRoot instance;

        private CanvasGroup buttonsGroup;
        private readonly Dictionary<(int size, int radius), Sprite> roundedSpriteCache = new();

        public static MobileControlsRoot Instance
        {
            get
            {
                if (instance == null)
                {
                    var go = new GameObject("MobileControlsRoot");
                    instance = go.AddComponent<MobileControlsRoot>();
                    DontDestroyOnLoad(go);
                }

                return instance;
            }
        }

        // Constrói-se sozinho assim que o primeiro frame do jogo roda, sem depender de
        // nenhuma outra cena/script chamar Instance explicitamente.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            _ = Instance;
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            // Precisa existir independente de mobile ou não: os botões do PauseMenu já
            // dependiam disto silenciosamente.
            EventSystemBootstrap.EnsureExists();

            if (!MobilePlatformDetector.IsMobile)
            {
                Debug.Log("[MobileControlsRoot] Dispositivo não-mobile detectado — controles touch não serão criados.");
                gameObject.SetActive(false);
                return;
            }

            // No WebGL o Input System costuma registrar um dispositivo Keyboard virtual
            // mesmo sem teclado físico, mas isto garante que ele exista de qualquer
            // forma — sem ele os OnScreenButton não teriam o que simular.
            if (Keyboard.current == null)
            {
                InputSystem.AddDevice<Keyboard>();
            }

            Build();
            SceneManager.sceneLoaded += OnSceneLoaded;
            RefreshVisibilityForActiveScene();
            Debug.Log("[MobileControlsRoot] Dispositivo mobile detectado — controles touch criados (Left/Right/Jump/Attack/Shield/Bow).");
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => RefreshVisibilityForActiveScene();

        private void RefreshVisibilityForActiveScene()
        {
            // Só faz sentido durante as fases: as cenas de menu não têm PlayerController
            // para controlar.
            bool hasPlayer = FindAnyObjectByType<PlayerController>() != null;
            buttonsGroup.gameObject.SetActive(hasPlayer);
        }

        private void Build()
        {
            var canvasGO = new GameObject("MobileControlsCanvas");
            canvasGO.transform.SetParent(transform, false);

            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            // Abaixo do HUD/Pause/Diálogo (todos em sortingOrder 0 ou 1): quando um
            // desses painéis abre por cima, ele cobre os botões em vez de sobrepor.
            canvas.sortingOrder = -1;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            // Tudo que é botão fica dentro deste retângulo, que se encolhe para não
            // ficar atrás de notch/Dynamic Island/barra de gestos.
            var safeAreaGO = new GameObject("SafeArea", typeof(RectTransform));
            safeAreaGO.transform.SetParent(canvasGO.transform, false);
            var safeAreaRect = (RectTransform)safeAreaGO.transform;
            safeAreaRect.anchorMin = Vector2.zero;
            safeAreaRect.anchorMax = Vector2.one;
            safeAreaRect.offsetMin = Vector2.zero;
            safeAreaRect.offsetMax = Vector2.zero;
            safeAreaGO.AddComponent<SafeAreaController>();

            var groupGO = new GameObject("Buttons", typeof(RectTransform));
            groupGO.transform.SetParent(safeAreaRect, false);
            var groupRect = (RectTransform)groupGO.transform;
            groupRect.anchorMin = Vector2.zero;
            groupRect.anchorMax = Vector2.one;
            groupRect.offsetMin = Vector2.zero;
            groupRect.offsetMax = Vector2.zero;
            buttonsGroup = groupGO.AddComponent<CanvasGroup>();

            const float margin = 24f;
            const float dpadSize = 130f;
            const float actionSize = 96f;
            const float gap = 14f;

            // Esquerda: setas de movimento contínuo (segurar = anda, soltar = para).
            CreateOnScreenButton(groupRect, "LeftButton", "<Keyboard>/a", "◄",
                new Vector2(0f, 0f), new Vector2(margin, margin),
                new Vector2(dpadSize, dpadSize), cornerRadius: 28f);

            CreateOnScreenButton(groupRect, "RightButton", "<Keyboard>/d", "►",
                new Vector2(0f, 0f), new Vector2(margin * 2f + dpadSize, margin),
                new Vector2(dpadSize, dpadSize), cornerRadius: 28f);

            // Direita: bloco 2x2 de ações. Cada botão aponta para o MESMO control path
            // de teclado da ação — ATK/JUMP são toques discretos e SHIELD funciona
            // segurando, porque o OnScreenButton mantém a tecla pressionada.
            //
            //     BOW   JUMP
            //     SHD   ATK
            float col1 = margin;
            float col0 = margin + actionSize + gap;
            float row0 = margin;
            float row1 = margin + actionSize + gap;

            CreateOnScreenButton(groupRect, "AttackButton", "<Keyboard>/j", "ATK",
                new Vector2(1f, 0f), new Vector2(-col1, row0),
                new Vector2(actionSize, actionSize), cornerRadius: actionSize / 2f);

            CreateOnScreenButton(groupRect, "ShieldButton", "<Keyboard>/k", "DEF",
                new Vector2(1f, 0f), new Vector2(-col0, row0),
                new Vector2(actionSize, actionSize), cornerRadius: actionSize / 2f);

            CreateOnScreenButton(groupRect, "JumpButton", "<Keyboard>/space", "JUMP",
                new Vector2(1f, 0f), new Vector2(-col1, row1),
                new Vector2(actionSize, actionSize), cornerRadius: actionSize / 2f);

            CreateOnScreenButton(groupRect, "BowButton", "<Keyboard>/l", "BOW",
                new Vector2(1f, 0f), new Vector2(-col0, row1),
                new Vector2(actionSize, actionSize), cornerRadius: actionSize / 2f);

            GameObject rotatePanel = CreateRotatePanel(canvasGO.transform);
            var rotatePrompt = canvasGO.AddComponent<RotateDevicePrompt>();
            rotatePrompt.panel = rotatePanel;
            rotatePrompt.controlsToHide = buttonsGroup;
        }

        private void CreateOnScreenButton(RectTransform parent, string name, string controlPath, string label,
            Vector2 anchor, Vector2 anchoredPosition, Vector2 size, float cornerRadius)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;

            var image = go.AddComponent<Image>();
            int texSize = Mathf.Max(32, Mathf.RoundToInt(Mathf.Max(size.x, size.y)));
            int texRadius = Mathf.RoundToInt(cornerRadius);
            image.sprite = GetRoundedSprite(texSize, texRadius);
            image.type = Image.Type.Sliced;
            // Semi-transparente e discreto: os controles não podem competir
            // visualmente com o gameplay.
            image.color = new Color(UITheme.ButtonNormal.r, UITheme.ButtonNormal.g, UITheme.ButtonNormal.b, 0.55f);

            go.AddComponent<OnScreenButton>().controlPath = controlPath;
            go.AddComponent<TouchButtonFeedback>();

            var labelGO = new GameObject("Label", typeof(RectTransform));
            labelGO.transform.SetParent(go.transform, false);
            var labelRect = (RectTransform)labelGO.transform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            var text = labelGO.AddComponent<Text>();
            text.text = label;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = label.Length > 2 ? 20 : 36;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = UITheme.TextPrimary;
            // O clique tem que chegar no botão por baixo, não ser interceptado pelo texto.
            text.raycastTarget = false;
        }

        private GameObject CreateRotatePanel(Transform parent)
        {
            var go = new GameObject("RotateDevicePanel", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var bg = go.AddComponent<Image>();
            bg.color = UITheme.OverlayDim;

            var textGO = new GameObject("Text", typeof(RectTransform));
            textGO.transform.SetParent(go.transform, false);
            var textRect = (RectTransform)textGO.transform;
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.sizeDelta = new Vector2(700f, 140f);
            textRect.anchoredPosition = Vector2.zero;

            var text = textGO.AddComponent<Text>();
            text.text = "Gire o aparelho para jogar em modo paisagem";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = UITheme.FontHeading;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = UITheme.TextPrimary;

            go.SetActive(false);
            return go;
        }

        private Sprite GetRoundedSprite(int size, int radius)
        {
            var key = (size, radius);
            if (roundedSpriteCache.TryGetValue(key, out Sprite cached))
            {
                return cached;
            }

            Sprite sprite = BuildRoundedSprite(size, radius);
            roundedSpriteCache[key] = sprite;
            return sprite;
        }

        // Gera um retângulo arredondado branco (com borda anti-aliased de 1px) em vez
        // de importar um asset de arte. Branco + alfa permite tingir com Image.color;
        // o Vector4 de borda no Sprite.Create faz 9-slice funcionar corretamente para
        // botões de tamanhos diferentes reaproveitando a mesma textura gerada.
        private static Sprite BuildRoundedSprite(int size, int radius)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            var pixels = new Color32[size * size];
            float r = radius;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = Mathf.Max(0f, Mathf.Max(r - x - 0.5f, x + 0.5f - (size - r)));
                    float dy = Mathf.Max(0f, Mathf.Max(r - y - 0.5f, y + 0.5f - (size - r)));
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    float alpha = Mathf.Clamp01(r - dist);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();

            float border = radius;
            return Sprite.Create(
                texture,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect,
                new Vector4(border, border, border, border));
        }
    }
}
