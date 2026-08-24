using System;
using UnityEngine;

namespace Odisseia.Systems
{
    public enum Language
    {
        English = 0,
        Portuguese = 1,
    }

    /// <summary>
    /// Idioma do jogo e busca de texto por chave.
    ///
    /// Serviço estático sem MonoBehaviour, no mesmo padrão do
    /// <see cref="SettingsManager"/>: qualquer tela pergunta por aqui e ninguém mais
    /// guarda texto solto. A tabela em si mora em <see cref="LocalizationTable"/>.
    ///
    /// Chave que não existe volta a própria chave, e não string vazia: um rótulo
    /// aparecendo como "ui.menu.continuar" na tela grita que falta traduzir, enquanto
    /// um espaço em branco passa despercebido até alguém reclamar.
    /// </summary>
    public static class Localization
    {
        private const string PrefsKey = "Odisseia.Language";

        /// <summary>Disparado quando o idioma muda — quem mostra texto se reinscreve.</summary>
        public static event Action Changed;

        private static bool loaded;
        private static Language current = Language.English;

        /// <summary>
        /// Idioma atual. O padrão é inglês; o português é a alternativa, escolhida
        /// pelo jogador nas configurações.
        /// </summary>
        public static Language Current
        {
            get
            {
                EnsureLoaded();
                return current;
            }
            set
            {
                EnsureLoaded();

                if (current == value)
                {
                    return;
                }

                current = value;
                PlayerPrefs.SetInt(PrefsKey, (int)value);
                PlayerPrefs.Save();
                Changed?.Invoke();
            }
        }

        /// <summary>Nomes dos idiomas, sempre no próprio idioma.</summary>
        public static string[] LanguageNames { get; } = { "English", "Português" };

        public static string Get(string key)
        {
            EnsureLoaded();

            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            if (!LocalizationTable.Entries.TryGetValue(key, out string[] traducoes))
            {
                return key;
            }

            int indice = (int)current;
            if (indice < 0 || indice >= traducoes.Length || string.IsNullOrEmpty(traducoes[indice]))
            {
                // Sem tradução neste idioma, cai no inglês, que é o de referência.
                return traducoes.Length > 0 ? traducoes[0] : key;
            }

            return traducoes[indice];
        }

        /// <summary>Busca com substituição de {0}, {1}… — para textos com números.</summary>
        public static string Get(string key, params object[] args)
        {
            string formato = Get(key);
            return args == null || args.Length == 0 ? formato : string.Format(formato, args);
        }

        public static bool Has(string key) => !string.IsNullOrEmpty(key) && LocalizationTable.Entries.ContainsKey(key);

        private static void EnsureLoaded()
        {
            if (loaded)
            {
                return;
            }

            loaded = true;
            current = (Language)PlayerPrefs.GetInt(PrefsKey, (int)Language.English);
        }

        /// <summary>
        /// Carrega o idioma antes da primeira cena, sem depender de ninguem lembrar de
        /// chamar — mesmo padrao do KeyRebindService.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize() => EnsureLoaded();
    }
}
