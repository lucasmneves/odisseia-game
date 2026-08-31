using System;
using System.Collections.Generic;
using UnityEngine;
using Odisseia.Player;
using Odisseia.Systems;
using Odisseia.UI;

namespace Odisseia.Levels
{
    /// <summary>
    /// Uma etapa da fase: um texto de objetivo, uma contagem e o que acontece quando
    /// ela fecha.
    ///
    /// É a peça que dá estrutura de atos ao prólogo sem inventar um "director" com a
    /// fase inteira escrita em código: cada objetivo aponta para o próximo
    /// (<see cref="next"/>) e liga/desliga objetos ao começar e ao terminar. O roteiro
    /// vira dados na cena — a mesma escolha que o projeto já faz com LevelDefinition.
    ///
    /// Quem completa não precisa saber contar: NPCs, pontos de interação e o
    /// treinamento chamam <see cref="Report"/> e o objetivo cuida de não contar a mesma
    /// fonte duas vezes.
    /// </summary>
    public class LevelObjective : MonoBehaviour
    {
        [Header("Texto")]
        [Tooltip("Chave de tradução do objetivo. Sem chave vale o texto abaixo.")]
        [SerializeField] private string titleKey;
        [SerializeField] private string title;
        [SerializeField] private bool showProgress = true;

        [Header("Contagem")]
        [SerializeField] private int required = 1;

        [Header("Cena")]
        [SerializeField] private ObjectiveBanner banner;
        [SerializeField] private GameObject[] enableOnStart;
        [SerializeField] private GameObject[] disableOnStart;
        [SerializeField] private GameObject[] enableOnComplete;
        [SerializeField] private GameObject[] disableOnComplete;

        [Header("Fechamento")]
        [Tooltip("Fala tocada ao completar, antes de passar para o próximo objetivo.")]
        [SerializeField] private DialogueSequence completionDialogue;
        [SerializeField] private PlayerInputLock playerLock;
        [SerializeField] private LevelObjective next;
        [Tooltip("Só o primeiro objetivo da cadeia começa sozinho.")]
        [SerializeField] private bool startsActive;

        private readonly HashSet<UnityEngine.Object> countedSources = new HashSet<UnityEngine.Object>();

        public event Action Completed;

        public bool IsActive { get; private set; }
        public bool IsCompleted { get; private set; }
        public int Current => countedSources.Count;
        public int Required => required;

        private void Start()
        {
            if (startsActive)
            {
                Begin();
            }
        }

        /// <summary>Entra em cena: liga o que este ato precisa e assume a faixa.</summary>
        public void Begin()
        {
            if (IsActive || IsCompleted)
            {
                return;
            }

            IsActive = true;
            PrologueTrace.Log("ato ABRIU: " + name + " (precisa de " + required + ")");
            SetAll(enableOnStart, true);
            SetAll(disableOnStart, false);
            RefreshBanner();

            // A contagem pode ter passado do necessário antes da etapa abrir (o jogador
            // conversou com alguém adiantado). Fechar aqui evita um objetivo que já
            // nasce cumprido e fica esperando um relato que não vem mais.
            if (Current >= required)
            {
                Complete();
            }
        }

        /// <summary>
        /// Registra uma contribuição. A mesma fonte só conta uma vez — falar de novo
        /// com o mesmo NPC não recruta um segundo homem.
        /// </summary>
        public void Report(UnityEngine.Object source)
        {
            if (IsCompleted)
            {
                return;
            }

            if (source == null || !countedSources.Add(source))
            {
                return;
            }

            PrologueTrace.Log("ato " + name + ": " + Current + "/" + required +
                " (relatou: " + source.name + ", ativo=" + IsActive + ")");
            RefreshBanner();

            // Só o ato ABERTO pode fechar. Um relato que chega adiantado (o jogador
            // alcançou alguém antes da hora) fica contabilizado e o Begin() aproveita
            // depois — fechar aqui pularia uma etapa da cadeia.
            if (IsActive && Current >= required)
            {
                Complete();
            }
        }

        private void Complete()
        {
            if (IsCompleted)
            {
                return;
            }

            IsCompleted = true;
            IsActive = false;

            PrologueTrace.Log("ato FECHOU: " + name);
            SetAll(enableOnComplete, true);
            SetAll(disableOnComplete, false);

            foreach (GameObject go in disableOnComplete ?? System.Array.Empty<GameObject>())
            {
                if (go != null)
                {
                    PrologueTrace.Log("  desligou: " + go.name + " (ativo agora: " + go.activeSelf + ")");
                }
            }
            AudioManager.PlayCheckpoint();
            Completed?.Invoke();

            if (completionDialogue != null)
            {
                playerLock?.SetLocked(true);
                completionDialogue.Completed += OnDialogueCompleted;
                completionDialogue.Play();
                return;
            }

            Advance();
        }

        private void OnDialogueCompleted()
        {
            completionDialogue.Completed -= OnDialogueCompleted;
            playerLock?.SetLocked(false);
            Advance();
        }

        private void Advance()
        {
            if (next != null)
            {
                next.Begin();
                return;
            }

            banner?.Hide();
        }

        private void RefreshBanner()
        {
            if (banner == null || !IsActive)
            {
                return;
            }

            string texto = !string.IsNullOrEmpty(titleKey) && Localization.Has(titleKey)
                ? Localization.Get(titleKey)
                : title;

            if (showProgress && required > 1)
            {
                texto = texto + "  " + Mathf.Min(Current, required) + "/" + required;
            }

            banner.Show(texto);
        }

        private static void SetAll(GameObject[] objects, bool value)
        {
            if (objects == null)
            {
                return;
            }

            foreach (GameObject go in objects)
            {
                if (go != null)
                {
                    go.SetActive(value);
                }
            }
        }
    }
}
