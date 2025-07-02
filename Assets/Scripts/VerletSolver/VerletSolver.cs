using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Sample.Solver
{
    public class VerletSolver
    {
        public List<Dot> Dots { get; } = new();

        private readonly int _iterations;

        public VerletSolver(int iterations)
        {
            _iterations = iterations;
        }

        public void AddForceToAllDots(Vector3 force)
        {
            Parallel.ForEach(Dots, dot => dot.AddForce(force));
        }
        
        public async UniTask SolveAsync(float deltaTime)
        {
            await UniTask.RunOnThreadPool(() => ApplyPhysicsToDots(deltaTime));
            await UniTask.RunOnThreadPool(ConstraintLength);
        }

        private void ApplyPhysicsToDots(float deltaTime)
        {
            float squaredDeltaTime = deltaTime * deltaTime;
            
            foreach (Dot dot in Dots)
            {
                if (dot.IsLocked)
                {
                    continue;
                }
            
                Vector3 acceleration = dot.CurrentForce / dot.Mass;
                Vector3 positionVariation = acceleration * squaredDeltaTime;
                Vector3 oldPosition = dot.CurrentPosition;
                
                dot.CurrentPosition += dot.CurrentPosition - dot.LastPosition;
                dot.CurrentPosition += positionVariation;
                dot.LastPosition = oldPosition;
                dot.ClearForce();
            }
        }
        
        private void ConstraintLength()
        {
            for (int i = 0; i < _iterations; i++)
            {
                foreach (Dot dotA in Dots)
                {
                    foreach (Connection connection in dotA.Connections)
                    {
                        Dot dotB = connection.Other(dotA);
                        Vector3 center = (dotA.CurrentPosition + dotB.CurrentPosition) / 2f;
                        Vector3 direction = (dotA.CurrentPosition - dotB.CurrentPosition).normalized;
                        Vector3 connectionSize = direction * connection.Length / 2f;

                        if (!dotA.IsLocked)
                        {
                            dotA.CurrentPosition = center + connectionSize;
                        }

                        if (!dotB.IsLocked)
                        {
                            dotB.CurrentPosition = center - connectionSize;
                        }
                    }
                }
            }
        }
    }
}