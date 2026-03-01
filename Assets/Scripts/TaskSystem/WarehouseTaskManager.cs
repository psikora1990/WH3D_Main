using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Centralized task system that tracks warehouse jobs and assigns them to workers.
/// </summary>
public class WarehouseTaskManager : MonoBehaviour
{
    private readonly List<WarehouseTask> taskQueue = new List<WarehouseTask>();

    /// <summary>
    /// Current read-only view of queued tasks.
    /// </summary>
    public IReadOnlyList<WarehouseTask> TaskQueue => taskQueue;

    /// <summary>
    /// Enqueue a new task for workers.
    /// </summary>
    public void EnqueueTask(WarehouseTask task)
    {
        if (task == null)
        {
            return;
        }

        taskQueue.Add(task);
    }

    /// <summary>
    /// Returns the nearest available task and marks it assigned.
    /// Workers can call this to request work.
    /// </summary>
    public WarehouseTask RequestTask(Vector3 workerPosition)
    {
        RemoveCompletedTasks();

        WarehouseTask nearestTask = null;
        float nearestDistanceSqr = float.PositiveInfinity;

        foreach (WarehouseTask task in taskQueue)
        {
            if (task == null || task.IsAssigned || task.IsCompleted)
            {
                continue;
            }

            float distanceSqr = (task.TargetPosition - workerPosition).sqrMagnitude;
            if (distanceSqr < nearestDistanceSqr)
            {
                nearestDistanceSqr = distanceSqr;
                nearestTask = task;
            }
        }

        if (nearestTask != null)
        {
            nearestTask.IsAssigned = true;
        }

        return nearestTask;
    }

    /// <summary>
    /// Marks a task complete. Completed tasks are removed from the queue.
    /// </summary>
    public void CompleteTask(WarehouseTask task)
    {
        if (task == null)
        {
            return;
        }

        task.IsCompleted = true;
        RemoveCompletedTasks();
    }

    /// <summary>
    /// Releases an assigned task back to the queue.
    /// </summary>
    public void ReleaseTask(WarehouseTask task)
    {
        if (task == null || task.IsCompleted)
        {
            return;
        }

        task.IsAssigned = false;
    }

    /// <summary>
    /// Removes tasks that are finished or invalid.
    /// </summary>
    public void RemoveCompletedTasks()
    {
        taskQueue.RemoveAll(task => task == null || task.IsCompleted);
    }
}
