using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sample.Solver;
using UnityEngine;

namespace Sample.Visual
{
    public class Simulation : MonoBehaviour
    {
        [SerializeField] private VisualDot dotVisual;
        [SerializeField] private LineRenderer connectionVisual;
        [SerializeField] private int width = 32;
        [SerializeField] private int height = 16;
        [SerializeField] private int iterationCount = 7;
        [SerializeField] private float threshold = 0.5f;

        private readonly Vector2Int[] _directions =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right 
        };

        private VerletSolver _solver;
        private Transform _dotsContainer;
        private Transform _connectionsContainer;
        private readonly Dictionary<Dot, VisualDot> _dotToVisual = new();
        private readonly Dictionary<VisualDot, Dot> _visualToDot = new();
        private readonly Dictionary<Connection, LineRenderer> _connectionVisuals = new();
        private bool _simulationIsRunning = true;

        private void OnValidate()
        {
            if (_solver != null)
            {
                _solver.Iterations = iterationCount;
            }
        }

        private void Start()
        {
            Dot[,] dotGrid = new Dot[width, height];
            _solver = new VerletSolver(iterationCount);

            _dotsContainer = new GameObject("Dots").transform;
            _connectionsContainer = new GameObject("Connections").transform;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vector3 dotPosition = transform.position + new Vector3(x, -y);
                    Dot newDot = CreateDot(dotPosition);

                    dotGrid[x, y] = newDot;
                }
            }

            for (int x = 0; x < width; x += 8)
            {
                Dot dot = dotGrid[x, 0];
                dot.IsLocked = true;
                _dotToVisual[dot].SetIsLocked(dot.IsLocked);
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vector2Int currentDotIndex = new(x, y);
                    Dot currentDot = dotGrid[currentDotIndex.x, currentDotIndex.y];
                    
                    foreach (Vector2Int direction in _directions)
                    {
                        Vector2Int neighbourIndex = currentDotIndex + direction;
                    
                        if (IndexIsValid(neighbourIndex))
                        {
                            Dot neighbourDot = dotGrid[neighbourIndex.x, neighbourIndex.y];
                            CreateConnection(currentDot, neighbourDot);
                        }
                    }
                }
            }
        }

        private void FixedUpdate()
        {
            if (!_simulationIsRunning)
            {
                return;
            }
            
            float fixedDeltaTime = Time.fixedDeltaTime;
            
            UniTask.RunOnThreadPool(async () =>
            {
                _solver.AddForceToAllDots(Vector3.down * 9.8f);
                await _solver.SolveAsync(fixedDeltaTime);
            }).Forget();
        }

        private void LateUpdate()
        {
            if (!_simulationIsRunning)
            {
                return;
            }

            foreach (Dot dot in _solver.Dots)
            {
                VisualDot visual = _dotToVisual[dot];
                visual.transform.position = dot.CurrentPosition;

                foreach (Connection connection in dot.Connections)
                {
                    LineRenderer currentConnectionVisual = _connectionVisuals[connection];
                    
                    currentConnectionVisual.SetPosition(0, dot.CurrentPosition);
                    currentConnectionVisual.SetPosition(1, connection.Other(dot).CurrentPosition);
                }
            }
        }

        public void ToggleSimulationRunning()
        {
            _simulationIsRunning = !_simulationIsRunning;
        }
        
        public bool IsRunning()
        {
            return _simulationIsRunning;
        }
        
        public void BreakConnectionOnPosition(Vector3 position)
        {
            foreach (Connection connection in _connectionVisuals.Keys)
            {
                Vector3 dotAPosition = connection.DotA.CurrentPosition;
                Vector3 dotBPosition = connection.DotB.CurrentPosition;

                Vector3 bToA = dotBPosition - dotAPosition;
                Vector3 pointToA = position - dotAPosition;
                
                float t = Mathf.Clamp01(Vector3.Dot(pointToA, bToA) / bToA.sqrMagnitude);
                Vector3 closestPoint = Vector3.Lerp(dotAPosition, dotBPosition, t);

                float sqrMagnitude = (position - closestPoint).sqrMagnitude;
                
                if (sqrMagnitude < threshold * threshold)
                {
                    BreakConnection(connection);
                    return;
                }
            }
        }
        
        public void CreateConnection(VisualDot firstDotVisual, VisualDot secondDotVisual)
        {
            Dot firstDot = _visualToDot[firstDotVisual];
            Dot secondDot = _visualToDot[secondDotVisual];
            CreateConnection(firstDot, secondDot);
        }
        
        public VisualDot CreateAndConnectDot(Vector3 dotPosition, VisualDot connectedDot)
        {
            Dot dot = CreateDot(dotPosition);
            CreateConnection(dot, _visualToDot[connectedDot]);
            return _dotToVisual[dot];
        }

        private void CreateConnection(Dot firstDot, Dot secondDot)
        {
            Connection connection = Dot.Connect(firstDot, secondDot);

            LineRenderer newConnectionVisual = Instantiate(connectionVisual, _connectionsContainer);
            newConnectionVisual.positionCount = 2;
            newConnectionVisual.SetPosition(0, firstDot.CurrentPosition);
            newConnectionVisual.SetPosition(1, secondDot.CurrentPosition);
            _connectionVisuals[connection] = newConnectionVisual;
        }
        
        private Dot CreateDot(Vector3 dotPosition)
        {
            Dot newDot = new(dotPosition);
            VisualDot newDotVisual = Instantiate(dotVisual, newDot.CurrentPosition, Quaternion.identity, _dotsContainer);
            newDotVisual.OnLockRequested += ToggleDotLock;

            _dotToVisual[newDot] = newDotVisual;
            _visualToDot[newDotVisual] = newDot;
                    
            _solver.Dots.Add(newDot);
            return newDot;
        }

        private void BreakConnection(Connection connection)
        {
            LineRenderer lineRenderer = _connectionVisuals[connection];
            Destroy(lineRenderer.gameObject);
            _connectionVisuals.Remove(connection);
            Dot.Disconnect(connection);
                    
            if (connection.DotA.Connections.Count == 0)
            {
                RemoveDot(connection.DotA);
            }

            if (connection.DotB.Connections.Count == 0)
            {
                RemoveDot(connection.DotB);
            }
        }

        private void RemoveDot(Dot dotB)
        {
            VisualDot visualDot = _dotToVisual[dotB];
                
            _solver.Dots.Remove(dotB);
            _dotToVisual.Remove(dotB);
            _visualToDot.Remove(visualDot);

            Destroy(visualDot.gameObject);
        }
        
        private void ToggleDotLock(VisualDot visualDot)
        {
            Dot dot = _visualToDot[visualDot];
            dot.IsLocked = !dot.IsLocked;
            visualDot.SetIsLocked(dot.IsLocked);
        }

        private bool IndexIsValid(Vector2Int index)
        {
            return index.x >= 0 && index.y >= 0 && index.x < width && index.y < height;
        }
    }
}