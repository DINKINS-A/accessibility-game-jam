using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class Emitter : MonoBehaviour
{
    private TONE currentTone;
    private int repetitions;
    private int playerID;
    private PlayerController playerController;

    private void Awake()
    {
        repetitions = 0;
    }

    void Start()
    {
        currentTone = TONE.NONE;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log("OnTriggerEnter2D" + collision.name);
        playerController = collision.GetComponent<PlayerController>();
        playerController.SetTouchingEmitter(this);
        AudioManager.Instance.PlayEmitterLocatedClip();
        EmitterLocated(playerController);
        Debug.Log("set touching emitter");
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        playerController.UnsetTouchingEmitter();
        playerController = null;
    }

    // Update is called once per frame
    void Update()
    {
    }

    /*
     * Assign which player this emitter belongs to
     */

    public void Assign(TONE currentTone)
    {
        if (this.currentTone != TONE.NONE)
        {
            AudioManager.Instance.PlayFailClip();
            return;
        }
        this.currentTone = currentTone;
        for (int count = 0; count < repetitions; ++count)
            GameManager.Instance.Enqueue(currentTone);
        AudioManager.Instance.PlaySuccessClip();
    }

    
    public int GetAssignment() {
        return playerID;
    }

    /*
 * Get the number of times this emitter will play a sound
 */
    public int GetRepititions()
    {
        return repetitions;
    }

    /*
     * Set the number of times this emitter will play a sound
     */
    public void SetRepititions(int repetitions)
    {
        this.repetitions = repetitions;
    }

    /*
     * Called when the player interacts with this emitter
     */
    private async void EmitterLocated(PlayerController controller)
    {
        for (int count = 0; count < 5; count++)
        {
            GamepadTarget target = name.Equals("DualSenseGamepadHID") ? GamepadTarget.Gamepad1 : GamepadTarget.Gamepad2;
            HapticsManager.Instance.Rumble(Motor.Left, 0.025f, target);
            await Task.Delay(500);
        }
    }
}
