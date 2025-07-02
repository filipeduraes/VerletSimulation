using System.Collections.Generic;
using UnityEngine;

namespace Sample.Solver
{
    public class Dot
    {
        public Vector3 CurrentPosition { get; set; }
        public Vector3 LastPosition { get; set; }
        public Vector3 CurrentForce { get; private set; } = Vector3.zero;
        public bool IsLocked { get; set; }
        public List<Connection> Connections { get; } = new();
        public float Mass { get; }

        public Dot(Vector3 initialPosition, float mass = 1f, bool isLocked = false)
        {
            CurrentPosition = initialPosition;
            LastPosition = initialPosition;
            Mass = mass;
            IsLocked = isLocked;
        }

        public static Connection Connect(Dot dotA, Dot dotB, float length = -1f)
        {
            Connection connection = length < 0f 
                                    ? new Connection(dotA, dotB) 
                                    : new Connection(dotA, dotB, length);
            
            dotA.Connections.Add(connection);
            dotB.Connections.Add(connection);
            return connection;
        }

        public static void Disconnect(Connection connection)
        {
            List<Connection> dotAConnections = connection.DotA.Connections;
            List<Connection> dotBConnections = connection.DotB.Connections;

            if (dotAConnections.Contains(connection))
            {
                dotAConnections.Remove(connection);
            }

            if (dotBConnections.Contains(connection))
            {
                dotBConnections.Remove(connection);
            }
        }

        public void AddForce(Vector3 force)
        {
            CurrentForce += force;
        }

        public void ClearForce()
        {
            CurrentForce = Vector3.zero;
        }
    }
}