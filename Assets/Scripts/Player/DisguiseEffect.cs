using UnityEngine;

namespace Odisseia.Player
{
    /// <summary>
    /// Disfarce de mendigo (Atena, Fase 14): move Odisseu para um layer de física que
    /// os inimigos não incluem no próprio playerLayer — eles simplesmente não o
    /// detectam, sem precisar alterar EnemyController. Desligado por padrão no
    /// Player.prefab; só a instância da Fase 14 começa disfarçada.
    /// </summary>
    public class DisguiseEffect : MonoBehaviour
    {
        [SerializeField] private int normalLayer = 9;
        [SerializeField] private int disguisedLayer = 12;
        [SerializeField] private bool startDisguised;

        public bool IsDisguised { get; private set; }

        private void Awake()
        {
            if (startDisguised)
            {
                SetDisguised(true);
            }
        }

        public void SetDisguised(bool value)
        {
            IsDisguised = value;
            gameObject.layer = value ? disguisedLayer : normalLayer;
        }
    }
}
