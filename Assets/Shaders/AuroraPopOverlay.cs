using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;


[RequireComponent(typeof(Image))]
public class AuroraPopOverlay : MonoBehaviour
{
    const int MaxPops = 8;  

    [Header("Shader")]
    [Tooltip("Material using the UI/AuroraPop shader. If empty, the Image's own material is used.")]
    [SerializeField] Material auroraMaterial;

    [Header("Timing (seconds)")]
    [SerializeField] float fadeIn = 0.15f;
    [SerializeField] float duration = 1.8f;

    [Header("Button colors (PlayStation style)")]
    [SerializeField] Color southColor = new Color(0.25f, 0.55f, 1f);   // Cross
    [SerializeField] Color eastColor = new Color(1f, 0.25f, 0.3f);     // Circle
    [SerializeField] Color northColor = new Color(0.2f, 1f, 0.55f);    // Triangle
    [SerializeField] Color westColor = new Color(1f, 0.4f, 0.9f);      // Square

    [Header("Vaibhav Testing- Keyboard keys for the 4 sound buttons")]
    [SerializeField] Key southKey = Key.K;   // Cross
    [SerializeField] Key eastKey = Key.L;    // Circle
    [SerializeField] Key northKey = Key.I;   // Triangle
    [SerializeField] Key westKey = Key.J;    // Square

    [Header("Debug")]
    [Tooltip("Log every pop to the Console.")]
    [SerializeField] bool logPops = true;
    [Tooltip("Show one pop when the scene starts, to check rendering without any input.")]
    [SerializeField] bool popOnStart = true;

    struct PopState
    {
        public Vector2 center;
        public Color color;
        public float startTime;
        public float seed;
    }

    readonly PopState[] pops = new PopState[MaxPops];
    readonly Vector4[] popData = new Vector4[MaxPops];
    readonly Vector4[] popColors = new Vector4[MaxPops];
    int nextSlot;
    static readonly int PopDataId = Shader.PropertyToID("_PopData");
    static readonly int PopColorsId = Shader.PropertyToID("_PopColors");
    static readonly int AuroraTimeId = Shader.PropertyToID("_AuroraTime");
    static readonly int AspectId = Shader.PropertyToID("_Aspect");

    void Awake()
    {
        var image = GetComponent<Image>();
        if (auroraMaterial != null) image.material = auroraMaterial;
        image.raycastTarget = false;   // don't block menu clicks

        for (int i = 0; i < MaxPops; i++) pops[i].startTime = float.NegativeInfinity;
    }

    void Start()
    {
        if (popOnStart) TestPop();
    }

    void OnDisable()
    {
        // Clear any pops still showing
        for (int i = 0; i < MaxPops; i++) popData[i] = Vector4.zero;
        Shader.SetGlobalVectorArray(PopDataId, popData);
    }

    [ContextMenu("Test Pop")]
    public void TestPop() => Pop(northColor, new Vector2(0.5f, 0.7f));

    /// <summary>Trigger a pop from gameplay code. center is in screen 0-1 coordinates.</summary>
    public void Pop(Color color, Vector2 center)
    {
        pops[nextSlot] = new PopState
        {
            center = center,
            color = color,
            startTime = Time.unscaledTime,
            seed = Random.value,
        };
        nextSlot = (nextSlot + 1) % MaxPops;

        if (logPops) Debug.Log($"Aurora pop {color} at {center}");
    }

    void Update()
    {
        PollInput();
        UpdateShader();
    }

    void PollInput()
    {
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            CheckButton(keyboard[southKey], southColor);
            CheckButton(keyboard[eastKey], eastColor);
            CheckButton(keyboard[northKey], northColor);
            CheckButton(keyboard[westKey], westColor);
        }

        foreach (var pad in Gamepad.all)
        {
            CheckButton(pad.buttonSouth, southColor);
            CheckButton(pad.buttonEast, eastColor);
            CheckButton(pad.buttonNorth, northColor);
            CheckButton(pad.buttonWest, westColor);
        }
    }

    void CheckButton(ButtonControl button, Color color)
    {
        if (button != null && button.wasPressedThisFrame)
            Pop(color, CenterFor(button.device));
    }

    void UpdateShader()
    {
        float now = Time.unscaledTime;

        for (int i = 0; i < MaxPops; i++)
        {
            float age = now - pops[i].startTime;
            float strength = 0f;
            if (age >= 0f && age < duration)
            {
                float rise = fadeIn > 0f ? Mathf.Clamp01(age / fadeIn) : 1f;
                float fall = 1f - Mathf.Clamp01((age - fadeIn) / Mathf.Max(0.01f, duration - fadeIn));
                strength = rise * fall * fall;   // squared for a soft tail
            }

            popData[i] = new Vector4(pops[i].center.x, pops[i].center.y, strength, pops[i].seed);
            popColors[i] = pops[i].color;
        }

        Shader.SetGlobalVectorArray(PopDataId, popData);
        Shader.SetGlobalVectorArray(PopColorsId, popColors);
        Shader.SetGlobalFloat(AuroraTimeId, now);
        Shader.SetGlobalFloat(AspectId, (float)Screen.width / Mathf.Max(1, Screen.height));
    }

    Vector2 CenterFor(InputDevice device)
    {
        float x = Random.Range(0.1f, 0.9f);

        if (device is Gamepad pad)
        {
            int index = Gamepad.all.IndexOfReference(pad);
            if (index == 0) x = Random.Range(0.1f, 0.4f);        // left player
            else if (index == 1) x = Random.Range(0.6f, 0.9f);   // right player
        }

        return new Vector2(x, Random.Range(0.55f, 0.8f));
    }
}
