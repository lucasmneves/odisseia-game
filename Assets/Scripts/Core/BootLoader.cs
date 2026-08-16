using UnityEngine;
using Odisseia.Systems;

namespace Odisseia.Core
{
    /// <summary>
    /// Garante que o GameManager exista e encaminha para o menu principal.
    /// </summary>
    public class BootLoader : MonoBehaviour
    {
        private void Start()
        {
            SceneLoader.Load(SceneLoader.MainMenu);
        }
    }
}
