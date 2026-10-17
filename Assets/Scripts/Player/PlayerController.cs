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
    public Rigidbody2D rb2D;

    private void Awake()
    {
        // Each player gets its own copy so they can be locked to different devices
        inputActions = Instantiate(inputActionsAsset);

        InputActionMap playerMap = inputActions.FindActionMap("Player", true);
        leftAction = playerMap.FindAction("Left", true);
        rightAction = playerMap.FindAction("Right", true);

    
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
}