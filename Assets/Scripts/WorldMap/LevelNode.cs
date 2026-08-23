using System;
using UnityEngine;
using Odisseia.Core;

namespace Odisseia.WorldMap
{
    /// <summary>Situação de uma fase no mapa.</summary>
    public enum LevelNodeState
    {
        /// <summary>Ainda não desbloqueada — Odisseu não passa daqui.</summary>
        Locked,

        /// <summary>Desbloqueada e ainda não concluída.</summary>
        Available,

        /// <summary>Onde Odisseu está agora.</summary>
        Current,

        /// <summary>Já concluída.</summary>
        Completed,
    }

    /// <summary>
    /// Um ponto de fase no mapa. Não guarda cópia do progresso: os dados descritivos
    /// vêm do <see cref="LevelDefinition"/> da campanha e o estado é calculado pelo
    /// <see cref="WorldMapManager"/> a partir do save — o mapa não é uma segunda fonte
    /// de verdade sobre o que está desbloqueado.
    ///
    /// A aparência fica com o <see cref="LevelNodeView"/>, que escuta
    /// <see cref="StateChanged"/>. Este componente não conhece sprite nem cor.
    /// </summary>
    public class LevelNode : MonoBehaviour
    {
        public string LevelId { get; private set; }
        public string LevelName { get; private set; }
        public string SceneName { get; private set; }

        /// <summary>Posição na campanha, começando em 1.</summary>
        public int Order { get; private set; }

        /// <summary>Índice da parada no caminho, começando em 0.</summary>
        public int StopIndex { get; private set; }

        /// <summary>Distância desta fase ao longo do caminho.</summary>
        public float DistanceAlongPath { get; private set; }

        public LevelNodeState State { get; private set; } = LevelNodeState.Locked;

        /// <summary>Dá para entrar? Concluída também dá, para permitir rejogar.</summary>
        public bool IsEnterable => State != LevelNodeState.Locked;

        public event Action<LevelNodeState> StateChanged;

        public void Bind(LevelDefinition definition, int stopIndex, float distanceAlongPath)
        {
            LevelId = definition.LevelId;
            LevelName = definition.DisplayName;
            SceneName = definition.SceneName;
            Order = definition.Order;
            StopIndex = stopIndex;
            DistanceAlongPath = distanceAlongPath;
            name = $"LevelNode_{definition.Order:00}_{definition.LevelId}";
        }

        public void SetState(LevelNodeState state)
        {
            if (State == state)
            {
                return;
            }

            State = state;
            StateChanged?.Invoke(state);
        }

        /// <summary>Reemite o estado atual — usado quando a view acaba de se inscrever.</summary>
        public void NotifyState()
        {
            StateChanged?.Invoke(State);
        }
    }
}
