using System.Collections.Generic;
using UnityEngine;

public class VerletSolver
{
    private readonly List<Dot> _dots;
    private readonly List<Connection> _connections;
    private readonly Dictionary<int, int> _dotToConnectionMap = new();
    private readonly int _iterationCount;

    public VerletSolver(List<Dot> dots, List<Connection> connections, int iterationCount)
    {
        _dots = dots;
        _connections = connections;
        _iterationCount = iterationCount;

        for (int connectionIndex = 0; connectionIndex < _connections.Count; connectionIndex++)
        {
            Connection connection = _connections[connectionIndex];
            _dotToConnectionMap[connection.FirstDotIndex] = connectionIndex;
            _dotToConnectionMap[connection.SecondDotIndex] = connectionIndex;
        }
    }
    
    public void Solve(float deltaTime)
    {
        SimulatePhysics(deltaTime);
        ApplyConstraints();
    }
    
    public void AddForceToAllDots(Vector3 force)
    {
        for (int i = 0; i < _dots.Count; i++)
        {
            AddForceToDot(i, force);
        }
    }

    public void AddForceToDot(int dotIndex, Vector3 force)
    {
        Dot dot = _dots[dotIndex];
        dot.CurrentForce = force;
        _dots[dotIndex] = dot;
    }
    
    public void AddDot(Dot dot)
    {
        _dots.Add(dot);
    }

    public void CreateConnection(int firstDotIndex, int secondDotIndex)
    {
        Dot firstDot = _dots[firstDotIndex];
        Dot secondDot = _dots[secondDotIndex];
        float distance = Vector3.Distance(firstDot.CurrentPosition, secondDot.CurrentPosition);

        Connection connection = new(firstDotIndex, secondDotIndex, distance);
        _connections.Add(connection);

        _dotToConnectionMap[firstDotIndex] = _connections.Count - 1;
        _dotToConnectionMap[secondDotIndex] = _connections.Count - 1;
    }

    public void RemoveDot(int dotIndex)
    {
        int connectionIndex = _dotToConnectionMap[dotIndex];
        BreakConnection(connectionIndex);
    }

    public void BreakConnection(int connectionIndex)
    {
        Connection connection = _connections[connectionIndex];
        int firstDotIndex = connection.FirstDotIndex;
        int secondDotIndex = connection.SecondDotIndex;
        
        _dots.RemoveAt(firstDotIndex);
        _dots.RemoveAt(secondDotIndex);
        _connections.RemoveAt(connectionIndex);
    }
    
    private void SimulatePhysics(float deltaTime)
    {
        float squaredDeltaTime = deltaTime * deltaTime;

        for (int dotIndex = 0; dotIndex < _dots.Count; dotIndex++)
        {
            Dot dot = _dots[dotIndex];
            
            if (dot.IsLocked)
            {
                continue;
            }

            Vector3 oldPosition = dot.CurrentPosition;
            Vector3 acceleration = dot.CurrentForce / dot.Mass;
            Vector3 positionVariation = acceleration * squaredDeltaTime;

            dot.CurrentPosition += dot.CurrentPosition - dot.LastPosition;
            dot.CurrentPosition += positionVariation;
            dot.LastPosition = oldPosition;
            
            dot.CurrentForce = Vector3.zero;
        }
    }

    private void ApplyConstraints()
    {
        for (int i = 0; i < _iterationCount; i++)
        {
            for (int dotIndex = 0; dotIndex < _dots.Count; dotIndex++)
            {
                int connectionIndex = _dotToConnectionMap[dotIndex];
                Connection connection = _connections[connectionIndex];

                Dot firstDot = _dots[connection.FirstDotIndex];
                Dot secondDot = _dots[connection.SecondDotIndex];
                
                Vector3 center = (firstDot.CurrentPosition + secondDot.CurrentPosition) / 2f;
                Vector3 direction = (firstDot.CurrentPosition - secondDot.CurrentPosition).normalized;
                Vector3 connectionSize = direction * connection.Length / 2f;

                if (!firstDot.IsLocked)
                {
                    firstDot.CurrentPosition = center + connectionSize;
                }

                if (!secondDot.IsLocked)
                {
                    secondDot.CurrentPosition = center - connectionSize;
                }
                
                _dots[connection.FirstDotIndex] = firstDot;
                _dots[connection.SecondDotIndex] = secondDot;
            }
        }
    }
}
