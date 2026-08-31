using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Odisseia.Player;
using Odisseia.Systems;
using Odisseia.UI;

namespace Odisseia.Levels
{
    /// <summary>O que uma etapa de treino espera do jogador.</summary>
    public enum TrainingAction
    {
        Move,
        Jump,
        Attack,
        SwordHit,
        Block,
        Bow,
        ArrowHit,
    }

    /// <summary>
    /// Campo de treinamento de Ítaca: apresenta espada, pulo, escudo e arco em ordem,
    /// uma coisa de cada vez, e só avança quando o jogador de fato executou o
    /// movimento.
    ///
    /// Não há combate novo aqui. O curso apenas ESCUTA os sistemas que já existem —
    /// PlayerCombat.Attacked, PlayerShield.Blocked, PlayerBow.Fired e a ação Jump — e
    /// conta. Um alvo acertado avisa pelo <see cref="TrainingTarget"/>. Por isso o
    /// tutorial não pode divergir do jogo: é o próprio jogo se reportando.
    ///
    /// Tolerância proposital: o jogador pode ficar quanto tempo quiser numa etapa,
    /// bater em qualquer ordem, morrer e voltar. O que ele não consegue é pular a
    /// etapa sem executá-la.
    /// </summary>
    public class TrainingCourse : MonoBehaviour
    {
        [Serializable]
        public class Step
        {
            [Tooltip("Chave da instrução. Aceita {0} = botão da ação (e .mobile para o toque).")]
            public string instructionKey;

            [Tooltip("Ação usada para resolver o {0} da instrução (Move/Jump/Attack/Shield/Bow).")]
            public string hintAction;

            public TrainingAction action = TrainingAction.Attack;

            [Tooltip("Quantas vezes. Em Move, conta SEGUNDOS de caminhada.")]
            public float required = 1f;

            public GameObject[] enableOnStart;
            public GameObject[] disableOnComplete;
        }

        [Header("Jogador")]
        [SerializeField] private PlayerCombat combat;
        [SerializeField] private PlayerShield shield;
        [SerializeField] private PlayerBow bow;
        [Tooltip("Vida do jogador. Serve para perceber o golpe que passou pelo escudo.")]
        [SerializeField] private Odisseia.Core.HealthSystem playerHealth;
        [SerializeField] private InputActionAsset inputActions;

        [Header("Apresentação")]
        [SerializeField] private TutorialPrompt prompt;
        [SerializeField] private float celebrationSeconds = 1.2f;
        [Tooltip("Chave do aviso curto de etapa concluída.")]
        [SerializeField] private string stepDoneKey = "tut.prologue.stepDone";

        [Tooltip("Chave do aviso de golpe que chegou pelas costas, com o escudo erguido.")]
        [SerializeField] private string blockedFromBehindKey = "tut.prologue.shieldBehind";

        [Header("Roteiro")]
        [SerializeField] private Step[] steps;

        [Header("Fim do treino")]
        [SerializeField] private LevelObjective objective;
        [SerializeField] private GameObject[] enableOnFinish;

        private InputAction moveAction;
        private InputAction jumpAction;

        private int index = -1;
        private float progress;

        /// <summary>Frame do último bloqueio válido, para separar o golpe aparado do que passou.</summary>
        private int frameDoUltimoBloqueio = -1;
        private Coroutine avisoDeCostas;
        private bool running;
        private bool waiting;

        public bool Finished { get; private set; }

        private void Awake()
        {
            if (inputActions != null)
            {
                InputActionMap map = inputActions.FindActionMap("Player", throwIfNotFound: false);
                moveAction = map?.FindAction("Move");
                jumpAction = map?.FindAction("Jump");
            }
        }

        private void OnEnable()
        {
            Subscribe(true);
            Localization.Changed += RefreshInstruction;

            // Pegar o controle no meio do treino tem que reescrever a instrução: ela
            // nomeia um botão, e o botão mudou.
            InputDeviceTracker.Changed += RefreshInstruction;

            // O curso vive num objeto desligado até o ato do treinamento começar; ligar
            // o objeto é o start, sem ninguém precisar chamar nada.
            if (!Finished && !running)
            {
                StartCourse();
            }
        }

        private void OnDisable()
        {
            Subscribe(false);
            Localization.Changed -= RefreshInstruction;
            InputDeviceTracker.Changed -= RefreshInstruction;
        }

        private void Subscribe(bool on)
        {
            if (combat != null)
            {
                if (on)
                {
                    combat.Attacked += OnAttacked;
                }
                else
                {
                    combat.Attacked -= OnAttacked;
                }
            }

            if (shield != null)
            {
                if (on)
                {
                    shield.Blocked += OnBlocked;
                }
                else
                {
                    shield.Blocked -= OnBlocked;
                }
            }

            if (bow != null)
            {
                if (on)
                {
                    bow.Fired += OnFired;
                    bow.OutOfArrows += OnOutOfArrows;
                }
                else
                {
                    bow.Fired -= OnFired;
                    bow.OutOfArrows -= OnOutOfArrows;
                }
            }

            if (jumpAction != null)
            {
                if (on)
                {
                    jumpAction.performed += OnJumped;
                }
                else
                {
                    jumpAction.performed -= OnJumped;
                }
            }

            if (playerHealth != null)
            {
                if (on)
                {
                    playerHealth.Damaged += OnPlayerDamaged;
                }
                else
                {
                    playerHealth.Damaged -= OnPlayerDamaged;
                }
            }
        }

        /// <summary>
        /// Golpe que passou pelo escudo erguido só pode ter vindo pelas costas — e o
        /// jogador não tem como saber disso sozinho: ele segurou o botão, apanhou, e o
        /// contador não andou. Sem esta explicação a etapa parece quebrada, que foi
        /// exatamente como ela pareceu.
        /// </summary>
        private void OnPlayerDamaged(int amount, int currentHealth)
        {
            if (!running || waiting || steps == null || index < 0 || index >= steps.Length)
            {
                return;
            }

            if (steps[index].action != TrainingAction.Block)
            {
                return;
            }

            bool escudoErguido = shield != null && shield.IsBlocking;
            bool aparouAgora = Time.frameCount == frameDoUltimoBloqueio;

            if (!escudoErguido || aparouAgora)
            {
                return;
            }

            if (avisoDeCostas != null)
            {
                StopCoroutine(avisoDeCostas);
            }

            avisoDeCostas = StartCoroutine(AvisarGolpePelasCostas());
        }

        private IEnumerator AvisarGolpePelasCostas()
        {
            prompt?.ShowPersistent(this, Localization.Get(blockedFromBehindKey));
            yield return new WaitForSeconds(1.6f);
            avisoDeCostas = null;
            RefreshInstruction();
        }

        private void StartCourse()
        {
            running = true;
            index = -1;
            NextStep();
        }

        private void NextStep()
        {
            index++;
            progress = 0f;

            if (steps == null || index >= steps.Length)
            {
                FinishCourse();
                return;
            }

            SetAll(steps[index].enableOnStart, true);
            RefillArrowsIfNeeded();
            RefreshInstruction();
        }

        /// <summary>
        /// Enche a aljava nas etapas do arco. Sem isto o tutorial pode ficar
        /// IMPOSSIVEL: dez flechas erradas e a etapa de acertar alvos não teria mais
        /// como ser cumprida. Fora do treino a munição continua sendo recurso.
        /// </summary>
        private void RefillArrowsIfNeeded()
        {
            if (bow == null || steps == null || index < 0 || index >= steps.Length)
            {
                return;
            }

            TrainingAction action = steps[index].action;
            if (action == TrainingAction.Bow || action == TrainingAction.ArrowHit)
            {
                bow.AddArrows(bow.MaxArrows);
            }
        }

        private void OnOutOfArrows()
        {
            RefillArrowsIfNeeded();
        }

        private void RefreshInstruction()
        {
            if (!running || waiting || steps == null || index < 0 || index >= steps.Length)
            {
                return;
            }

            prompt?.ShowPersistent(this, BuildInstruction(steps[index]));
        }

        private string BuildInstruction(Step step)
        {
            string texto = string.IsNullOrEmpty(step.hintAction)
                ? Localization.Get(step.instructionKey)
                : ControlHints.Instruction(step.instructionKey, step.hintAction);

            // Contagem só quando há mais de uma repetição: um "0/1" não informa nada e
            // ainda faz a dica parecer tarefa burocrática.
            if (step.required > 1f && step.action != TrainingAction.Move)
            {
                texto = texto + "   " + Mathf.FloorToInt(progress) + "/" + Mathf.RoundToInt(step.required);
            }

            return texto;
        }

        private void Update()
        {
            if (!running || waiting || steps == null || index < 0 || index >= steps.Length)
            {
                return;
            }

            Step step = steps[index];

            if (step.action == TrainingAction.Move && moveAction != null
                && Mathf.Abs(moveAction.ReadValue<float>()) > 0.2f)
            {
                progress += Time.deltaTime;

                if (progress >= step.required)
                {
                    CompleteStep();
                }
            }
        }

        private void OnAttacked()
        {
            Report(TrainingAction.Attack);
        }

        private void OnFired()
        {
            Report(TrainingAction.Bow);
        }

        private void OnBlocked(int absorbed)
        {
            frameDoUltimoBloqueio = Time.frameCount;
            Report(TrainingAction.Block);
        }

        private void OnJumped(InputAction.CallbackContext context)
        {
            Report(TrainingAction.Jump);
        }

        /// <summary>
        /// Chamado pelos alvos de treino e pelos eventos do jogador. Só conta quando é
        /// exatamente o que a etapa atual pede: bater de espada durante a etapa do arco
        /// não adianta o tutorial, mas também não é punido.
        /// </summary>
        public void Report(TrainingAction action)
        {
            if (!running || waiting || steps == null || index < 0 || index >= steps.Length)
            {
                return;
            }

            Step step = steps[index];
            if (step.action != action)
            {
                return;
            }

            progress += 1f;
            RefreshInstruction();

            if (progress >= step.required)
            {
                CompleteStep();
            }
        }

        private void CompleteStep()
        {
            waiting = true;
            SetAll(steps[index].disableOnComplete, false);
            AudioManager.PlayCollect();
            prompt?.ShowPersistent(this, Localization.Get(stepDoneKey));
            StartCoroutine(NextStepAfterPause());
        }

        private IEnumerator NextStepAfterPause()
        {
            yield return new WaitForSeconds(celebrationSeconds);
            waiting = false;
            NextStep();
        }

        private void FinishCourse()
        {
            running = false;
            Finished = true;
            prompt?.Hide(this);
            SetAll(enableOnFinish, true);
            objective?.Report(this);
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
