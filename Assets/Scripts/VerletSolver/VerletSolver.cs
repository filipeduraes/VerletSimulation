using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Sample.Solver
{
    public class VerletSolver
    {
        public List<Dot> Dots { get; } = new();

        public int Iterations { get; set; }

        public VerletSolver(int iterations)
        {
            Iterations = iterations;
        }

        public void AddForceToAllDots(Vector3 force)
        {
            Parallel.ForEach(Dots, dot => dot.AddForce(force));
        }
        
        public async UniTask SolveAsync(float deltaTime)
        {
            await UniTask.RunOnThreadPool
            (
                () =>
                {
                    ApplyPhysicsToDots(deltaTime);
                    ConstraintLength();
                }
            );
        }

        private void ApplyPhysicsToDots(float deltaTime)
        {
            float squaredDeltaTime = deltaTime * deltaTime;
            
            foreach (Dot dot in Dots.ToList())
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
            for (int i = 0; i < Iterations; i++)
            {
                foreach (Dot dotA in Dots.ToList())
                {
                    if (dotA == null)
                    {
                        continue;
                    }
                    
                    foreach (Connection connection in dotA.Connections.ToList())
                    {
                        Dot dotB = connection?.Other(dotA);

                        if (dotB == null)
                        {
                            continue;
                        }
                        
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