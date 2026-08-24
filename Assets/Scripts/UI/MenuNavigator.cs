using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Odisseia.Systems;

namespace Odisseia.UI
{
    /// <summary>
    /// Navegação de uma lista vertical de botões por teclado e controle, além do mouse.
    ///
    /// Faz três coisas que o EventSystem sozinho não faz:
    ///
    /// 1. Encadeia a navegação explicitamente (cima/baixo em ciclo), pulando itens
    ///    desabilitados — a navegação automática do Unity para no primeiro item
    ///    bloqueado e deixa o jogador preso.
    /// 2. Garante que sempre exista foco. Clicar no vazio ou desabilitar o item em foco
    ///    zera a seleção do EventSystem, e a partir daí teclado e controle param de
    ///    responder sem nenhum aviso.
    /// 3. Trata "voltar" (ESC / B / Circle) num lugar só.
    /// </summary>
    public class MenuNavigator : MonoBehaviour
    {
        [Tooltip("Ordem de navegação, de cima para baixo.")]
        [SerializeField] private List<Button> items = new List<Button>();

        [Tooltip("Chamado no ESC / botão B. Vazio, não faz nada.")]
        [SerializeField] private MonoBehaviour cancelTarget;

        private InputAction cancelAction;
        private GameObject lastSelected;

        private void Awake()
        {
            EventSystemBootstrap.EnsureExists();
        }

        private void OnEnable()
        {
            BuildNavigation();
            FocusFirstAvailable();
        }

        private void Update()
        {
            EnsureFocus();
            PollCancel();
        }

        // ---------------------------------------------------------------- foco

        /// <summary>
        /// Encadeia os itens habilitados num ciclo. Refeito sempre que a lista muda de
        /// estado (ex.: Continuar sendo desabilitado por não haver save).
        /// </summary>
        public void BuildNavigation()
        {
            List<Button> ativos = items.Where(b => b != null && b.gameObject.activeInHierarchy && b.interactable).ToList();

            foreach (Button item in items.Where(b => b != null))
            {
                var nav = new Navigation { mode = Navigation.Mode.Explicit };

                int index = ativos.IndexOf(item);
                if (index >= 0 && ativos.Count > 1)
                {
                    nav.selectOnUp = ativos[(index - 1 + ativos.Count) % ativos.Count];
                    nav.selectOnDown = ativos[(index + 1) % ativos.Count];
                }

                item.navigation = nav;
            }
        }

        /// <summary>Foca o primeiro item utilizável — é onde o jogador começa.</summary>
        public void FocusFirstAvailable()
        {
            Button alvo = items.FirstOrDefault(b => b != null && b.gameObject.activeInHierarchy && b.interactable);
            Select(alvo);
        }

        public void Select(Button item)
        {
            if (item == null || EventSystem.current == null)
            {
                return;
            }

            EventSystem.current.SetSelectedGameObject(item.gameObject);
            lastSelected = item.gameObject;
        }

        /// <summary>
        /// Sem isto, um clique fora dos botões (ou desabilitar o item em foco) deixa o
        /// EventSystem sem seleção, e o menu vira "só mouse" até alguém clicar de novo.
        /// </summary>
        private void EnsureFocus()
        {
            EventSystem system = EventSystem.current;
            if (system == null)
            {
                return;
            }

            GameObject atual = system.currentSelectedGameObject;

            if (atual != null && atual.activeInHierarchy)
            {
                var selectable = atual.GetComponent<Selectable>();
                if (selectable == null || selectable.interactable)
                {
                    lastSelected = atual;
                    return;
                }
            }

            Button alvo = items.FirstOrDefault(b => b != null && b.gameObject == lastSelected && b.interactable)
                          ?? items.FirstOrDefault(b => b != null && b.gameObject.activeInHierarchy && b.interactable);

            if (alvo != null)
            {
                system.SetSelectedGameObject(alvo.gameObject);
                lastSelected = alvo.gameObject;
            }
        }

        // ---------------------------------------------------------------- voltar

        private void PollCancel()
        {
            if (cancelTarget == null)
            {
                return;
            }

            bool pediuVoltar =
                (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) ||
                (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame);

            if (!pediuVoltar)
            {
                return;
            }

            AudioManager.PlayUiCancel();
            (cancelTarget as ICancelHandler)?.OnCancel(null);
        }

        /// <summary>Permite montar a lista em runtime, sem depender do Inspector.</summary>
        public void SetItems(IEnumerable<Button> novos)
        {
            items = novos.Where(b => b != null).ToList();
            BuildNavigation();
        }

        /// <summary>
        /// Quem responde ao ESC / botão B. Necessário quando o navegador é adicionado
        /// em runtime — aí não há Inspector para preencher o campo, e sem isso o
        /// "voltar" simplesmente não faz nada.
        /// </summary>
        public void SetCancelTarget(MonoBehaviour target)
        {
            cancelTarget = target;
        }
    }
}
