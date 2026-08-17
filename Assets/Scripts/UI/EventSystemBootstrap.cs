using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace Odisseia.UI
{
    /// <summary>
    /// Garante que exista um EventSystem com InputSystemUIInputModule na sessão.
    ///
    /// As 16 cenas de fase nunca tiveram um EventSystem — só as 4 cenas de menu têm o
    /// seu próprio (criado manualmente em cada uma). Sem EventSystem, nenhum Button de
    /// UI recebe clique nem toque: nem os botões do PauseMenu/DeathOverlay adicionados
    /// na etapa de polish, nem os novos controles touch. Chamado uma vez, antes de
    /// qualquer UI interativa ser construída.
    /// </summary>
    public static class EventSystemBootstrap
    {
        public static void EnsureExists()
        {
            if (EventSystem.current != null)
            {
                return;
            }

            var go = new GameObject("EventSystem (Mobile Bootstrap)");
            Object.DontDestroyOnLoad(go);

            go.AddComponent<EventSystem>();
            var module = go.AddComponent<InputSystemUIInputModule>();
            module.AssignDefaultActions();
        }
    }
}
