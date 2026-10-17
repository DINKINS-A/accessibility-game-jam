using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public enum Motor { Left, Right, Both }
public enum GamepadTarget { Gamepad1, Gamepad2, Both }

public class HapticsManager : MonoBehaviour
{
    const string PrefKey = "rumbleIntensity";
    const int MaxPads = 2;

    static HapticsManager instance;
    public static HapticsManager Instance
    {
        get
        {
            if (instance == null)
            {
                var go = new GameObject("HapticsManager");
                instance = go.AddComponent<HapticsManager>();
            }
            return instance;
        }
    }

    public float Intensity { get; private set; } = 1f;     // 0 to 1, set by the slider
    public event Action<float> IntensityChanged;

    // Per-gamepad state: index 0 = Gamepad1, index 1 = Gamepad2
    readonly bool[] leftOn = new bool[MaxPads];     // heavy, low-frequency motor
    readonly bool[] rightOn = new bool[MaxPads];    // light, high-frequency motor
    readonly Coroutine[] leftRoutine = new Coroutine[MaxPads];
    readonly Coroutine[] rightRoutine = new Coroutine[MaxPads];

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap() { var _ = Instance; }

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);
        Intensity = PlayerPrefs.GetFloat(PrefKey, 1f);
    }

    public void SetIntensity(float value)
    {
        value = Mathf.Clamp01(value);
        if (Mathf.Approximately(value, Intensity)) return;

        Intensity = value;
        PlayerPrefs.SetFloat(PrefKey, Intensity);
        IntensityChanged?.Invoke(Intensity);

        // Update any pads that are currently rumbling
        for (int i = 0; i < MaxPads; i++) Apply(i);
    }

    // Rumble the left motor, the right motor, or both, on Gamepad1, Gamepad2, or both pads
    public void Rumble(Motor motor, float seconds, GamepadTarget target = GamepadTarget.Both)
    {
        if (target == GamepadTarget.Gamepad1 || target == GamepadTarget.Both)
            RumblePad(0, motor, seconds);

        if (target == GamepadTarget.Gamepad2 || target == GamepadTarget.Both)
            RumblePad(1, motor, seconds);
    }

    void RumblePad(int pad, Motor motor, float seconds)
    {
        if (motor == Motor.Left || motor == Motor.Both)
        {
            if (leftRoutine[pad] != null) StopCoroutine(leftRoutine[pad]);
            leftRoutine[pad] = StartCoroutine(MotorRoutine(pad, true, seconds));
        }

        if (motor == Motor.Right || motor == Motor.Both)
        {
            if (rightRoutine[pad] != null) StopCoroutine(rightRoutine[pad]);
            rightRoutine[pad] = StartCoroutine(MotorRoutine(pad, false, seconds));
        }
    }

    IEnumerator MotorRoutine(int pad, bool left, float seconds)
    {
        if (left) leftOn[pad] = true; else rightOn[pad] = true;
        Apply(pad);

        yield return new WaitForSecondsRealtime(seconds);

        if (left) leftOn[pad] = false; else rightOn[pad] = false;
        Apply(pad);
    }

    // Sends the active motors of one gamepad to the controller at the slider's intensity
    void Apply(int padIndex)
    {
        if (padIndex >= Gamepad.all.Count) return;   // that gamepad isn't connected
        Gamepad pad = Gamepad.all[padIndex];

        bool silent = Intensity <= 0f || (!leftOn[padIndex] && !rightOn[padIndex]);

        if (silent)
            pad.ResetHaptics();
        else
            pad.SetMotorSpeeds(leftOn[padIndex] ? Intensity : 0f,
                               rightOn[padIndex] ? Intensity : 0f);
    }

    void OnDisable()
    {
        foreach (var pad in Gamepad.all)
            pad.ResetHaptics();
    }
    void OnApplicationPause(bool paused) { if (paused) PlayerPrefs.Save(); }
    void OnApplicationQuit() { PlayerPrefs.Save(); }
}