using System;
using System.Collections.Generic;
using UnityEngine;

namespace Odisseia.WorldMap
{
    /// <summary>
    /// O caminho da jornada: uma polilinha de pontos, onde alguns são paradas de fase.
    ///
    /// Odisseu anda por distância percorrida ao longo desta linha, não por posição livre
    /// — é o que mantém a navegação presa ao caminho sem precisar de colisor nenhum, e o
    /// que permite curvas e desvios no futuro sem mudar o controlador.
    ///
    /// As posições são editáveis no Inspector. Os pontos marcados como parada de fase
    /// correspondem, em ordem, às fases do <c>CampaignManager</c> — o caminho não guarda
    /// nome nem id de fase, para não virar uma segunda fonte de verdade.
    /// </summary>
    public class WorldMapPath : MonoBehaviour
    {
        [Serializable]
        public class PathPoint
        {
            public Vector2 position;

            [Tooltip("Marque nos pontos onde existe uma fase. A ordem segue a da campanha.")]
            public bool isLevelStop;
        }

        [SerializeField] private List<PathPoint> points = new List<PathPoint>();

        private readonly List<float> cumulativeDistance = new List<float>();
        private readonly List<int> levelStopIndices = new List<int>();
        private bool built;

        /// <summary>Índices dos pontos que são parada de fase, em ordem de caminho.</summary>
        public IReadOnlyList<int> LevelStopIndices
        {
            get
            {
                Build();
                return levelStopIndices;
            }
        }

        public int LevelStopCount => LevelStopIndices.Count;

        public float TotalLength
        {
            get
            {
                Build();
                return cumulativeDistance.Count > 0 ? cumulativeDistance[cumulativeDistance.Count - 1] : 0f;
            }
        }

        private void Awake() => Build();

        private void Build()
        {
            if (built)
            {
                return;
            }

            built = true;
            cumulativeDistance.Clear();
            levelStopIndices.Clear();

            float total = 0f;
            for (int i = 0; i < points.Count; i++)
            {
                if (i > 0)
                {
                    total += Vector2.Distance(points[i - 1].position, points[i].position);
                }

                cumulativeDistance.Add(total);

                if (points[i].isLevelStop)
                {
                    levelStopIndices.Add(i);
                }
            }
        }

        /// <summary>Reconstrói os índices — útil se o caminho for alterado em runtime.</summary>
        public void Invalidate()
        {
            built = false;
            Build();
        }

        public Vector2 GetPointPosition(int index)
        {
            Build();
            if (points.Count == 0)
            {
                return Vector2.zero;
            }

            index = Mathf.Clamp(index, 0, points.Count - 1);
            return points[index].position;
        }

        /// <summary>Distância acumulada até um ponto do caminho.</summary>
        public float GetDistanceAtPoint(int index)
        {
            Build();
            if (cumulativeDistance.Count == 0)
            {
                return 0f;
            }

            index = Mathf.Clamp(index, 0, cumulativeDistance.Count - 1);
            return cumulativeDistance[index];
        }

        /// <summary>Distância acumulada até a n-ésima parada de fase.</summary>
        public float GetDistanceAtLevelStop(int stopOrder)
        {
            Build();
            if (levelStopIndices.Count == 0)
            {
                return 0f;
            }

            stopOrder = Mathf.Clamp(stopOrder, 0, levelStopIndices.Count - 1);
            return GetDistanceAtPoint(levelStopIndices[stopOrder]);
        }

        public Vector2 GetLevelStopPosition(int stopOrder)
        {
            Build();
            if (levelStopIndices.Count == 0)
            {
                return Vector2.zero;
            }

            stopOrder = Mathf.Clamp(stopOrder, 0, levelStopIndices.Count - 1);
            return GetPointPosition(levelStopIndices[stopOrder]);
        }

        /// <summary>Posição no caminho a uma dada distância do início.</summary>
        public Vector2 Evaluate(float distance)
        {
            Build();

            if (points.Count == 0)
            {
                return Vector2.zero;
            }

            if (points.Count == 1)
            {
                return points[0].position;
            }

            distance = Mathf.Clamp(distance, 0f, TotalLength);

            for (int i = 1; i < points.Count; i++)
            {
                if (distance <= cumulativeDistance[i])
                {
                    float segmentStart = cumulativeDistance[i - 1];
                    float segmentLength = cumulativeDistance[i] - segmentStart;
                    float t = segmentLength > 0.0001f ? (distance - segmentStart) / segmentLength : 0f;
                    return Vector2.Lerp(points[i - 1].position, points[i].position, t);
                }
            }

            return points[points.Count - 1].position;
        }

        /// <summary>Pares de pontos consecutivos, para desenhar os segmentos do caminho.</summary>
        public IEnumerable<(Vector2 from, Vector2 to)> Segments()
        {
            Build();
            for (int i = 1; i < points.Count; i++)
            {
                yield return (points[i - 1].position, points[i].position);
            }
        }

        /// <summary>
        /// Gera um traçado reto com <paramref name="stopCount"/> paradas e pontos
        /// intermediários entre elas. Só é usado quando o caminho está vazio, para o
        /// mapa nascer funcional; qualquer traçado definido no Inspector tem precedência.
        /// </summary>
        public void GenerateStraightLine(int stopCount, float spacing, int intermediatePoints)
        {
            points.Clear();

            for (int stop = 0; stop < stopCount; stop++)
            {
                points.Add(new PathPoint
                {
                    position = new Vector2(stop * spacing, 0f),
                    isLevelStop = true,
                });

                bool isLast = stop == stopCount - 1;
                if (isLast)
                {
                    continue;
                }

                for (int mid = 1; mid <= intermediatePoints; mid++)
                {
                    float t = mid / (float)(intermediatePoints + 1);
                    points.Add(new PathPoint
                    {
                        position = new Vector2((stop + t) * spacing, 0f),
                        isLevelStop = false,
                    });
                }
            }

            Invalidate();
        }

        public bool IsEmpty => points.Count == 0;

        private void OnDrawGizmos()
        {
            if (points == null || points.Count == 0)
            {
                return;
            }

            for (int i = 0; i < points.Count; i++)
            {
                Gizmos.color = points[i].isLevelStop ? Color.yellow : new Color(0.4f, 0.6f, 1f);
                Gizmos.DrawWireSphere(points[i].position, points[i].isLevelStop ? 0.45f : 0.18f);

                if (i > 0)
                {
                    Gizmos.color = new Color(0.5f, 0.5f, 0.6f);
                    Gizmos.DrawLine(points[i - 1].position, points[i].position);
                }
            }
        }
    }
}
