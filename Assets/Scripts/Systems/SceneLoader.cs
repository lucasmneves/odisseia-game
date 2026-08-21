using UnityEngine.SceneManagement;
using Odisseia.UI;

namespace Odisseia.Systems
{
    /// <summary>
    /// Sistema básico de troca de cenas por nome, com transição de fade.
    /// </summary>
    public static class SceneLoader
    {
        public const string Boot = "Boot";
        public const string MainMenu = "MainMenu";
        public const string LevelSelect = "LevelSelect";
        public const string LevelComplete = "LevelComplete";
        public const string Ending = "Ending";
        public const string Level01Troia = "Level_01_Troia";

        /// <summary>
        /// Escurece a tela e então carrega a cena (o fade-in do outro lado é automático,
        /// via ScreenFader). O Time.timeScale é restaurado aqui porque a troca de cena
        /// pode partir de um menu de pause.
        /// </summary>
        public static void Load(string sceneName)
        {
            UnityEngine.Time.timeScale = 1f;
            ScreenFader.Instance.FadeOutThen(() => SceneManager.LoadScene(sceneName));
        }

        /// <summary>
        /// Carrega mostrando a tela de carregamento. É o caminho usado entre fases:
        /// escurece, mostra o loader com o nome da próxima fase e entra nela sozinho,
        /// sem tela intermediária de "fase concluída" e sem clique do jogador.
        /// </summary>
        public static void LoadWithLoadingScreen(string sceneName, string title = null)
        {
            UnityEngine.Time.timeScale = 1f;
            ScreenFader.Instance.FadeOutThen(() => LoadingScreen.Begin(sceneName, title));
        }

        /// <summary>Carrega imediatamente, sem transição (usado em casos de emergência/testes).</summary>
        public static void LoadImmediate(string sceneName)
        {
            UnityEngine.Time.timeScale = 1f;
            SceneManager.LoadScene(sceneName);
        }
    }
}
