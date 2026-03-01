using System;
using UnityEngine;

/// <summary>
/// Unity-friendly replacement for a Python-style core game loop.
///
/// Attach this component to a GameObject in your first scene.
/// Hook gameplay systems to the public events to keep logic modular.
/// </summary>
public sealed class CoreGameLoop : MonoBehaviour
{
    [Header("Loop Settings")]
    [Tooltip("If true, targetFrameRate is applied on startup.")]
    [SerializeField] private bool useTargetFrameRate = true;

    [Tooltip("Application target frame rate. Use -1 for platform default.")]
    [SerializeField] private int targetFrameRate = 60;

    [Tooltip("If true, the loop can be paused by setting IsPaused.")]
    [SerializeField] private bool allowPause = true;

    /// <summary>
    /// Raised once when the loop initializes.
    /// </summary>
    public event Action OnLoopInitialized;

    /// <summary>
    /// Raised every frame while running and not paused.
    /// float = deltaTime.
    /// </summary>
    public event Action<float> OnLoopTick;

    /// <summary>
    /// Raised on fixed timestep updates while running and not paused.
    /// float = fixedDeltaTime.
    /// </summary>
    public event Action<float> OnPhysicsTick;

    /// <summary>
    /// Raised once when the loop is stopped.
    /// </summary>
    public event Action OnLoopStopped;

    public bool IsRunning { get; private set; }

    public bool IsPaused { get; private set; }

    private void Awake()
    {
        if (useTargetFrameRate)
        {
            Application.targetFrameRate = targetFrameRate;
        }

        IsRunning = true;
        IsPaused = false;
        OnLoopInitialized?.Invoke();
    }

    private void Update()
    {
        if (!IsRunning)
        {
            return;
        }

        if (allowPause && IsPaused)
        {
            return;
        }

        OnLoopTick?.Invoke(Time.deltaTime);
    }

    private void FixedUpdate()
    {
        if (!IsRunning)
        {
            return;
        }

        if (allowPause && IsPaused)
        {
            return;
        }

        OnPhysicsTick?.Invoke(Time.fixedDeltaTime);
    }

    private void OnApplicationQuit()
    {
        StopLoop();
    }

    /// <summary>
    /// Pause loop ticks if allowPause is enabled.
    /// </summary>
    public void PauseLoop()
    {
        if (!allowPause || !IsRunning)
        {
            return;
        }

        IsPaused = true;
    }

    /// <summary>
    /// Resume loop ticks after pause.
    /// </summary>
    public void ResumeLoop()
    {
        if (!IsRunning)
        {
            return;
        }

        IsPaused = false;
    }

    /// <summary>
    /// Stops the game loop and fires shutdown event once.
    /// </summary>
    public void StopLoop()
    {
        if (!IsRunning)
        {
            return;
        }

        IsRunning = false;
        IsPaused = false;
        OnLoopStopped?.Invoke();
    }
}
