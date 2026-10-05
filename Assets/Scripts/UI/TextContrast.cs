using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Odisseia.UI
{
    /// <summary>
    /// Contorno escuro nos textos que ficam soltos sobre o cenário: os do HUD das fases (vida, moedas, flechas, vidas,
    /// XP, letreiro, objetivo, contador do vento) e o título e a mensagem do Final.
    ///
    /// Sem ele, o texto claro sumia sobre nuvem e céu claro (Gado do Sol) e sobre as folhas da oliveira (Final). A regra
    /// é "filho direto do Canvas do HUD ou do Final": o que está num painel ou num botão já tem fundo próprio e fica como
    /// está. Aplicado a cada cena carregada, sem reescrever as cenas, e só uma vez por texto.
    ///
    /// No Final só o contorno não basta: a letra fina e clara do título disputa com as folhas da oliveira. Ali os textos
    /// soltos ganham também uma faixa escura translúcida atrás, do tamanho do texto.
    /// </summary>
    public static class TextContrast
    {
        /// <summary>Nomes dos Canvas cujos textos soltos ganham contorno.</summary>
        private static readonly string[] Canvases = { "HUD Canvas", "Ending Canvas" };

        private static readonly Color Cor = new Color(0.06f, 0.05f, 0.08f, 0.9f);

        /// <summary>Canvas cujos textos soltos ganham, além do contorno, a faixa atrás.</summary>
        private const string CanvasComFaixa = "Ending Canvas";

        private static readonly Color CorDaFaixa = new Color(0.05f, 0.04f, 0.06f, 0.6f);

        /// <summary>Folga da faixa em volta do texto, em unidades do Canvas de referência.</summary>
        private static readonly Vector2 Folga = new Vector2(40f, 10f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            var paraFaixa = new List<Text>();

            foreach (GameObject raiz in scene.GetRootGameObjects())
            {
                foreach (Text text in raiz.GetComponentsInChildren<Text>(includeInactive: true))
                {
                    Transform pai = text.transform.parent;
                    if (pai != null && pai.GetComponent<Canvas>() != null
                        && System.Array.IndexOf(Canvases, pai.name) >= 0)
                    {
                        Apply(text);

                        if (pai.name == CanvasComFaixa && text.gameObject.activeSelf)
                        {
                            paraFaixa.Add(text);
                        }
                    }
                }
            }

            Band(paraFaixa);
        }

        /// <summary>
        /// Uma faixa só atrás de todos os textos dados (irmãos no mesmo Canvas): da borda de cima do primeiro à de baixo
        /// do último, na largura do texto mais largo — o que se lê, não a caixa de 900 do layout.
        /// </summary>
        public static void Band(List<Text> textos)
        {
            if (textos.Count == 0 || !(textos[0].transform.parent is RectTransform pai) || pai.Find("ContrastBand") != null)
            {
                return;
            }

            float esquerda = float.MaxValue, direita = float.MinValue, baixo = float.MaxValue, cima = float.MinValue;
            int ordem = int.MaxValue;

            foreach (Text text in textos)
            {
                RectTransform r = text.rectTransform;
                Rect caixa = r.rect;
                Vector2 centro = (Vector2)r.localPosition + caixa.center;
                float meiaLargura = Mathf.Min(text.preferredWidth, caixa.width) * 0.5f;

                esquerda = Mathf.Min(esquerda, centro.x - meiaLargura);
                direita = Mathf.Max(direita, centro.x + meiaLargura);
                baixo = Mathf.Min(baixo, r.localPosition.y + caixa.yMin);
                cima = Mathf.Max(cima, r.localPosition.y + caixa.yMax);
                ordem = Mathf.Min(ordem, r.GetSiblingIndex());
            }

            var go = new GameObject("ContrastBand", typeof(RectTransform));
            go.transform.SetParent(pai, false);
            go.transform.SetSiblingIndex(ordem);

            // Ancorada no mesmo ponto dos textos (no Final, o alto da tela): o Canvas ainda não tem o tamanho final quando a
            // cena carrega, e uma faixa ancorada no centro ficava fora da tela depois que o CanvasScaler ajustava a altura.
            Vector2 ancora = textos[0].rectTransform.anchorMin;
            Vector2 pontoDaAncora = pai.rect.min + Vector2.Scale(pai.rect.size, ancora);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = ancora;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(direita - esquerda, cima - baixo) + Folga * 2f;
            rect.anchoredPosition = new Vector2((esquerda + direita) * 0.5f, (baixo + cima) * 0.5f) - pontoDaAncora;

            var image = go.AddComponent<Image>();
            image.color = CorDaFaixa;
            image.raycastTarget = false;
        }

        /// <summary>
        /// Contorno de 2 unidades do Canvas de referência (1280×720): fino o bastante para o texto do HUD (22) não
        /// engrossar, largo o bastante para separar do céu claro também no título grande.
        /// </summary>
        public static void Apply(Text text)
        {
            if (text == null || text.GetComponent<Shadow>() != null)
            {
                return;
            }

            var contorno = text.gameObject.AddComponent<Outline>();
            contorno.effectColor = Cor;
            contorno.effectDistance = new Vector2(2f, -2f);
            contorno.useGraphicAlpha = true;
        }
    }
}
