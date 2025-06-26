using UnityEngine;

public struct Dot
{
    public Vector3 CurrentPosition { get; set; }
    public Vector3 LastPosition { get; set; }
    public Vector3 CurrentForce { get; set; }
    public float Mass { get; set; }
    public bool IsLocked { get; set; }

    public Dot(Vector3 currentPosition, float mass = 1.0f, bool isLocked = false)
    {
        CurrentPosition = currentPosition;
        LastPosition = CurrentPosition;
        CurrentForce = Vector3.zero;
        Mass = mass;
        IsLocked = isLocked;
    }
}