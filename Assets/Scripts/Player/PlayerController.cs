using Unity.AppUI.Editor;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public enum ControlDevice
    {
        Keyboard,
        Gamepad1,
        Gamepad2
    }

    public Emitter? touchingEmitter;
    public const int RUMBLE_DURATION_SECONDS = 2;
    public AudioSource UniversalAudioSource;
    [Header("Input")]
    [Tooltip("Drag your PlayerInput.inputactions asset here (same asset on both players)")]
    [SerializeField] private InputActionAsset inputActionsAsset;

    [Tooltip("Which device controls THIS player")]
    [SerializeField] private ControlDevice controlDevice = ControlDevice.Keyboard;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    private InputActionAsset inputActions; // this player's own copy
    private InputAction leftAction;
    private InputAction rightAction;
    private InputAction northAction;
    private InputAction southAction;
    private InputAction eastAction;
    private InputAction westAction;
    private InputAction leftShoulderAction;
    private InputAction rightShoulderAction;
    public Rigidbody2D rb2D;

    private void Awake()
    {
        // Each player gets its own copy so they can be locked to different devices
        inputActions = Instantiate(inputActionsAsset);

        InputActionMap playerMap = inputActions.FindActionMap("Player", true);
        leftAction = playerMap.FindAction("Left", true);
        rightAction = playerMap.FindAction("Right", true);
        northAction = playerMap.FindAction("North Button", true);
        eastAction = playerMap.FindAction("East Button", true);
        southAction = playerMap.FindAction("South Button", true);
        westAction = playerMap.FindAction("West Button", true);
        leftShoulderAction = playerMap.FindAction("Left Shoulder", true);
        rightShoulderAction = playerMap.FindAction("Right Shoulder", true);
        leftShoulderAction.performed +=
                context => VibrateLeftOrRight(GameManager.Instance.getNearestEmitter(transform));
        southAction.performed +=
                context => SetActiveEmitter(TONE.CROSS);
        westAction.performed +=
                context => SetActiveEmitter(TONE.SQUARE);
        eastAction.performed +=
                context => SetActiveEmitter(TONE.CIRCLE);
        northAction.performed +=
                context => SetActiveEmitter(TONE.TRIANGLE);

        //rightShoulderAction.performed +=
        //    context => TryAssignNote(GameManager.Instance.getNearestEmitter(transform));
    }

    public void SetTouchingEmitter(Emitter e)
    {
        touchingEmitter = e;
    }
    public void UnsetTouchingEmitter()
    {
        touchingEmitter = null;
    }

    private void SetActiveEmitter(TONE tone)
    {
        if (touchingEmitter)
        {
            touchingEmitter.Assign(tone);
            Debug.Log("Set touching emitter to " + tone);
        } else
        {
            Debug.Log("No touching emitter found.");
        }
    }

    private void VibrateLeftOrRight(GameObject o)
    {
        if (transform.position.x > o.transform.position.x)
        {
            Debug.Log("left");
            HapticsManager.Instance.Rumble(Motor.Left, RUMBLE_DURATION_SECONDS);
        } else
        {
            Debug.Log("right");
            HapticsManager.Instance.Rumble(Motor.Right, RUMBLE_DURATION_SECONDS);
        }
    }

    private void OnEnable()
    {
        AssignDevice();
        inputActions.Enable();
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
        inputActions.Disable();
    }

    private void OnDestroy()
    {
        if (inputActions != null)
            Destroy(inputActions);
    }

    // Re-check devices when a gamepad is plugged in or unplugged
    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (change == InputDeviceChange.Added ||
            change == InputDeviceChange.Removed ||
            change == InputDeviceChange.Reconnected ||
            change == InputDeviceChange.Disconnected)
        {
            AssignDevice();
        }
    }

    private void AssignDevice()
    {
        InputDevice device = null;

        switch (controlDevice)
        {
            case ControlDevice.Keyboard:
                device = Keyboard.current;
                break;
            case ControlDevice.Gamepad1:
                if (Gamepad.all.Count > 0) device = Gamepad.all[0];
                break;
            case ControlDevice.Gamepad2:
                if (Gamepad.all.Count > 1) device = Gamepad.all[1];
                break;
        }

        // Lock this player to one device (empty list = no input if it's not connected)
        inputActions.devices = device != null
            ? new InputDevice[] { device }
            : new InputDevice[0];
    }
    public void OnCollisionEnter2D(Collision2D collision)
    {
       if(collision.gameObject.CompareTag("walls"))
        {
            
            UniversalAudioSource.PlayOneShot(AudioManager.Instance.GetWallsClip());
        }
    }

    private void Update()
    {
        float direction = rightAction.ReadValue<float>() - leftAction.ReadValue<float>();

        if (rb2D != null)
        {
            rb2D.linearVelocity = new Vector2(direction * moveSpeed, rb2D.linearVelocity.y);
        }
        else
        {
            transform.Translate(Vector3.right * direction * moveSpeed * Time.deltaTime);
        }
    }

    public bool isLeftShoulderButtonPressed()
    {
        return leftShoulderAction.WasPressedThisFrame();
    }
    public bool isRightShoulderButtonPressed()
    {
        return rightShoulderAction.WasPressedThisFrame();
    }

    public bool isRightShoulderButtonHeld()
    {
        return rightShoulderAction.WasPerformedThisFrame();
    }

    public bool isNorthActionButtonPressed()
    {
        return northAction.WasPressedThisFrame();
    }

    public bool isSouthActionButtonPressed()
    {
        return southAction.WasPressedThisFrame();
    }

    public bool isEastActionButtonPressed()
    {
        return eastAction.WasPressedThisFrame();
    }

    public bool isWestActionButtonPressed()
    {
        return westAction.WasPressedThisFrame();
    }
}