using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Odisseia.UI
{
    /// <summary>
    /// Troca a fonte embutida do Unity pela <see cref="UITheme.Font"/> em todo texto de cena.
    ///
    /// As 20 cenas trazem ~245 textos com a fonte embutida gravada no arquivo. Em vez de
    /// reescrever as cenas, cada cena carregada tem os textos trocados uma vez — o que é
    /// montado por código já nasce com a <see cref="UITheme.Font"/>. Só troca a fonte
    /// embutida (ou nenhuma): um texto que um dia tiver fonte própria fica com ela.
    /// </summary>
    public static class FontBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) => Apply(scene);

        public static void Apply(Scene scene)
        {
            Font alvo = UITheme.Font;

            foreach (GameObject raiz in scene.GetRootGameObjects())
            {
                foreach (Text text in raiz.GetComponentsInChildren<Text>(includeInactive: true))
                {
                    if (text.font == alvo)
                    {
                        continue;
                    }

                    if (text.font == null || IsBuiltin(text.font))
                    {
                        text.font = alvo;
                    }
                }
            }
        }

        /// <summary>A embutida aparece como LegacyRuntime (Unity 6) ou Arial (cenas antigas).</summary>
        private static bool IsBuiltin(Font f) => f.name == "LegacyRuntime" || f.name == "Arial";
    }
}
