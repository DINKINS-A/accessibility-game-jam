using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Simple countdown timer. Attach to any GameObject.
/// Optionally assign a TMP_Text to display the remaining time.
/// </summary>
public class CountdownTimer : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float duration = 60f;
    [SerializeField] private bool startOnAwake = true;
    [Tooltip("Ignore Time.timeScale (keeps counting while the game is paused).")]
    [SerializeField] private bool useUnscaledTime = false;

    [Header("Optional UI")]
    [SerializeField] private TMP_Text label;

   

    public event Action<float> OnTick;   // Remaining seconds, every frame
    public event Action OnFinished;

    public float Remaining { get; private set; }
    public bool IsRunning { get; private set; }
    public float Progress => duration > 0f ? 1f - Remaining / duration : 1f; // 0 -> 1

    private void Awake()
    {
        Remaining = duration;
        UpdateLabel();
        if (startOnAwake) StartTimer();
    }

    private void Update()
    {
        if (!IsRunning) return;

        Remaining -= useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        if (Remaining <= 0f)
        {
            Remaining = 0f;
            IsRunning = false;
            UpdateLabel();
            OnTick?.Invoke(Remaining);
            OnFinished?.Invoke();
            
            return;
        }

        UpdateLabel();
        OnTick?.Invoke(Remaining);
    }

    /// <summary>Start (or restart) the countdown. Pass a value to change the duration.</summary>
    public void StartTimer(float newDuration = -1f)
    {
        if (newDuration > 0f) duration = newDuration;
        Remaining = duration;
        IsRunning = true;
    }

    public void Pause() => IsRunning = false;

    public void Resume()
    {
        if (Remaining > 0f) IsRunning = true;
    }

    public void ResetTimer()
    {
        IsRunning = false;
        Remaining = duration;
        UpdateLabel();
    }

    public void AddTime(float seconds)
    {
        Remaining = Mathf.Max(0f, Remaining + seconds);
    }

    /// <summary>Formatted as MM:SS.</summary>
    public string FormattedTime
    {
        get
        {
            int total = Mathf.CeilToInt(Remaining);
            return $"{total / 60:00}:{total % 60:00}";
        }
    }

    private void UpdateLabel()
    {
        if (label != null) label.text = FormattedTime;
    }
}