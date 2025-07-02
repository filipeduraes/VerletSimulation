using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sample.Solver;
using UnityEngine;

namespace Sample.Visual
{
    public class Simulation : MonoBehaviour
    {
        [SerializeField] private Transform dotVisual;
        [SerializeField] private LineRenderer connectionVisual;
        [SerializeField] private int width = 32;
        [SerializeField] private int height = 16;
        [SerializeField] private int iterationCount = 7;

        private readonly Vector2Int[] _directions =
        {
            Vector2Int.up, 
            Vector2Int.down, 
            Vector2Int.left,
            Vector2Int.right 
        };

        private Transform _dotsContainer;
        private Transform _connectionsContainer;
        private Dot[,] _dotGrid;
        private readonly Dictionary<Dot, Transform> _dotVisuals = new();
        private readonly Dictionary<Connection, LineRenderer> _connectionVisuals = new();
        private VerletSolver _solver;

        private void Start()
        {
            _dotGrid = new Dot[width, height];
            _solver = new VerletSolver(iterationCount);

            _dotsContainer = new GameObject("Dots").transform;
            _connectionsContainer = new GameObject("Connections").transform;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Dot newDot = new(transform.position + new Vector3(x, -y));
                    Transform newDotVisual = Instantiate(dotVisual, newDot.CurrentPosition, Quaternion.identity, _dotsContainer);

                    _dotVisuals[newDot] = newDotVisual;
                    _dotGrid[x, y] = newDot;
                    _solver.Dots.Add(newDot);
                }
            }

            for (int x = 0; x < width; x += 8)
            {
                Dot dot = _dotGrid[x, 0];
                dot.IsLocked = true;
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vector2Int currentDotIndex = new(x, y);
                    Dot currentDot = GetDotAtIndex(currentDotIndex);
                    
                    foreach (Vector2Int direction in _directions)
                    {
                        Vector2Int neighbourIndex = currentDotIndex + direction;
                    
                        if (IndexIsValid(neighbourIndex))
                        {
                            Dot neighbourDot = GetDotAtIndex(neighbourIndex);
                            Connection connection = Dot.Connect(currentDot, neighbourDot);

                            LineRenderer newConnectionVisual = Instantiate(connectionVisual, _connectionsContainer);
                            newConnectionVisual.positionCount = 2;
                            _connectionVisuals[connection] = newConnectionVisual;
                        }
                    }
                }
            }
        }

        private void FixedUpdate()
        {
            float fixedDeltaTime = Time.fixedDeltaTime;
            
            UniTask.RunOnThreadPool(async () =>
            {
                _solver.AddForceToAllDots(Vector3.down * 9.8f);
                await _solver.SolveAsync(fixedDeltaTime);
            }).Forget();
        }

        private void Update()
        {
            foreach (Dot dot in _dotGrid)
            {
                Transform visual = _dotVisuals[dot];
                visual.position = dot.CurrentPosition;

                foreach (Connection connection in dot.Connections)
                {
                    LineRenderer currentConnectionVisual = _connectionVisuals[connection];
                    
                    currentConnectionVisual.SetPosition(0, dot.CurrentPosition);
                    currentConnectionVisual.SetPosition(1, connection.Other(dot).CurrentPosition);
                }
            }
        }

        private Dot GetDotAtIndex(Vector2Int index)
        {
            return _dotGrid[index.x, index.y];
        }

        private bool IndexIsValid(Vector2Int index)
        {
            return index.x >= 0 && index.y >= 0 && index.x < width && index.y < height;
        }
    }
}