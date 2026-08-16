using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Odisseia.UI
{
    /// <summary>
    /// Transição entre cenas: fade-out antes de carregar, fade-in ao entrar. Cria-se
    /// sozinho na primeira utilização e persiste entre cenas, com o próprio Canvas em
    /// sortingOrder alto — assim nenhuma cena precisa conter o overlay manualmente.
    /// </summary>
    public class ScreenFader : MonoBehaviour
    {
        private const float DefaultDuration = 0.25f;

        private static ScreenFader instance;

        private Image overlay;
        private Coroutine routine;

        public static ScreenFader Instance
        {
            get
            {
                if (instance == null)
                {
                    var go = new GameObject("ScreenFader");
                    instance = go.AddComponent<ScreenFader>();
                    DontDestroyOnLoad(go);
                    instance.Build();
                }

                return instance;
            }
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

            if (overlay == null)
            {
                Build();
            }

            // Clareia automaticamente ao entrar em qualquer cena — nenhuma cena precisa
            // se lembrar de chamar FadeIn().
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            FadeIn();
        }

        private void Build()
        {
            var canvasGO = new GameObject("FadeCanvas");
            canvasGO.transform.SetParent(transform, false);

            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;

            var overlayGO = new GameObject("Overlay");
            overlayGO.transform.SetParent(canvasGO.transform, false);

            var rect = overlayGO.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            overlay = overlayGO.AddComponent<Image>();
            overlay.color = new Color(0.03f, 0.04f, 0.07f, 0f);
            overlay.raycastTarget = false;
        }

        /// <summary>Escurece a tela e então executa a ação (tipicamente carregar a cena).</summary>
        public void FadeOutThen(Action action, float duration = DefaultDuration)
        {
            if (routine != null)
            {
                StopCoroutine(routine);
            }

            routine = StartCoroutine(FadeRoutine(0f, 1f, duration, action));
        }

        /// <summary>Clareia a tela (chamado ao entrar numa cena nova).</summary>
        public void FadeIn(float duration = DefaultDuration)
        {
            if (routine != null)
            {
                StopCoroutine(routine);
            }

            routine = StartCoroutine(FadeRoutine(1f, 0f, duration, null));
        }

        private IEnumerator FadeRoutine(float from, float to, float duration, Action onComplete)
        {
            // Bloqueia cliques enquanto a tela está escurecendo, para o jogador não
            // conseguir disparar outra transição no meio da primeira.
            overlay.raycastTarget = true;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                SetAlpha(Mathf.Lerp(from, to, elapsed / duration));
                yield return null;
            }

            SetAlpha(to);
            overlay.raycastTarget = to > 0.5f;
            routine = null;

            onComplete?.Invoke();
        }

        private void SetAlpha(float alpha)
        {
            Color c = overlay.color;
            c.a = Mathf.Clamp01(alpha);
            overlay.color = c;
        }
    }
}
