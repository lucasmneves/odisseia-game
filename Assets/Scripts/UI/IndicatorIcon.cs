using UnityEngine;
using UnityEngine.UI;

namespace Odisseia.UI
{
    /// <summary>
    /// Ícone dos indicadores de fase (fome, lótus, cera) — os que não passam pelo <see cref="HUD"/>,
    /// cada um com o próprio painel na cena. Mesma regra dos ícones do HUD: arte de
    /// <c>Resources/Odisseia/UI/HUD/</c>, escala INTEIRA (pixel art reescalada por fator quebrado fica irregular).
    /// Sem a arte, não faz nada e o indicador continua só texto.
    /// </summary>
    public static class IndicatorIcon
    {
        private const string Pasta = "Odisseia/UI/HUD/";

        /// <summary>Altura que o ícone procura ter: ícones minúsculos (o lótus tem 17×12) dobram.</summary>
        private const float AlturaAlvo = 28f;

        private const float Respiro = 6f;

        /// <summary>
        /// Põe o ícone dentro do painel, na borda esquerda, e encolhe o rótulo (esticado no painel) para
        /// começar depois dele. Devolve a largura ocupada, 0 se não havia arte.
        /// </summary>
        public static float InsidePanel(Text label, string icone)
        {
            if (label == null || !(label.transform.parent is RectTransform painel))
            {
                return 0f;
            }

            Image image = Create(painel, icone, new Vector2(0f, 0.5f), new Vector2(Respiro, 0f));
            if (image == null)
            {
                return 0f;
            }

            float largura = image.rectTransform.sizeDelta.x + Respiro;
            label.rectTransform.offsetMin += new Vector2(largura, 0f);
            return largura;
        }

        /// <summary>Põe o ícone fora do elemento, colado à esquerda dele (barras).</summary>
        public static bool LeftOf(RectTransform alvo, string icone)
        {
            return alvo != null && Create(alvo, icone, new Vector2(1f, 0.5f), new Vector2(-Respiro, 0f)) != null;
        }

        private static Image Create(RectTransform pai, string icone, Vector2 pivo, Vector2 posicao)
        {
            Sprite sprite = Resources.Load<Sprite>(Pasta + icone);
            if (sprite == null || pai.Find("Icon_" + icone) != null)
            {
                return null;
            }

            float maior = Mathf.Max(sprite.rect.width, sprite.rect.height);
            float escala = Mathf.Max(1f, Mathf.Floor(AlturaAlvo / maior));

            var go = new GameObject("Icon_" + icone, typeof(RectTransform));
            go.transform.SetParent(pai, false);

            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = pivo;
            rect.sizeDelta = new Vector2(sprite.rect.width, sprite.rect.height) * escala;
            rect.anchoredPosition = posicao;

            var image = go.AddComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            image.preserveAspect = true;
            return image;
        }
    }
}
