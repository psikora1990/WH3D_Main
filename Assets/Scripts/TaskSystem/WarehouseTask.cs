using System;
using UnityEngine;

/// <summary>
/// Represents a single warehouse job that can be assigned to a worker.
/// </summary>
[Serializable]
public class WarehouseTask
{
    public TaskType Type;
    public Vector3 TargetPosition;
    public bool IsAssigned;
    public bool IsCompleted;

    public WarehouseTask(TaskType type, Vector3 targetPosition)
    {
        Type = type;
        TargetPosition = targetPosition;
        IsAssigned = false;
        IsCompleted = false;
    }
}
