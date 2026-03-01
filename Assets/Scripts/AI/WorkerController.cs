using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Basic worker AI for warehouse gameplay.
/// Requests tasks, moves to task locations, performs timed work, then reports completion.
/// </summary>
public class WorkerController : MonoBehaviour
{
    public enum WorkerState
    {
        Idle,
        MovingToTask,
        Working
    }

    [Header("References")]
    [SerializeField] private NavMeshAgent navMeshAgent;
    [Tooltip("Assign the WarehouseTaskManager component here. It must implement IWarehouseTaskManager.")]
    [SerializeField] private MonoBehaviour taskManagerBehaviour;

    [Header("Work Settings")]
    [SerializeField] private float workDurationSeconds = 2f;
    [SerializeField] private float arrivalThreshold = 0.2f;

    public WorkerState CurrentState { get; private set; } = WorkerState.Idle;

    private IWarehouseTaskManager taskManager;
    private WorkerTask currentTask;
    private float workTimer;

    private void Awake()
    {
        if (navMeshAgent == null)
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
        }

        taskManager = taskManagerBehaviour as IWarehouseTaskManager;
        if (taskManagerBehaviour != null && taskManager == null)
        {
            Debug.LogError($"{nameof(WorkerController)} requires a task manager that implements {nameof(IWarehouseTaskManager)}.", this);
        }
    }

    private void Update()
    {
        if (taskManager == null || navMeshAgent == null)
        {
            return;
        }

        switch (CurrentState)
        {
            case WorkerState.Idle:
                TryAcquireTask();
                break;

            case WorkerState.MovingToTask:
                UpdateMovementToTask();
                break;

            case WorkerState.Working:
                UpdateWorkTimer();
                break;
        }
    }

    private void TryAcquireTask()
    {
        if (!taskManager.TryGetNextTask(this, out WorkerTask nextTask))
        {
            return;
        }

        currentTask = nextTask;
        navMeshAgent.isStopped = false;
        navMeshAgent.SetDestination(currentTask.Position);
        CurrentState = WorkerState.MovingToTask;
    }

    private void UpdateMovementToTask()
    {
        if (navMeshAgent.pathPending)
        {
            return;
        }

        float remainingDistance = navMeshAgent.remainingDistance;
        if (remainingDistance > navMeshAgent.stoppingDistance + arrivalThreshold)
        {
            return;
        }

        navMeshAgent.isStopped = true;
        workTimer = workDurationSeconds;
        CurrentState = WorkerState.Working;
    }

    private void UpdateWorkTimer()
    {
        workTimer -= Time.deltaTime;
        if (workTimer > 0f)
        {
            return;
        }

        taskManager.CompleteTask(currentTask);
        currentTask = default;
        CurrentState = WorkerState.Idle;
    }
}

/// <summary>
/// Shared task data for worker assignments.
/// </summary>
public struct WorkerTask
{
    public int Id;
    public Vector3 Position;
}

/// <summary>
/// Contract that WarehouseTaskManager should implement.
/// </summary>
public interface IWarehouseTaskManager
{
    bool TryGetNextTask(WorkerController worker, out WorkerTask task);
    void CompleteTask(WorkerTask task);
}
