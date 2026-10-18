using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

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
        var player1 = GameObject.Find("Player 1");
        var player2 = GameObject.Find("Player 2");
        if (collision.gameObject.Equals(player1))
            EmitterLocated(playerController, GamepadTarget.Gamepad1);
        else if (collision.gameObject.Equals(player2))
            EmitterLocated(playerController, GamepadTarget.Gamepad2);
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
        Destroy(this.gameObject);
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
    private async void EmitterLocated(PlayerController controller, GamepadTarget target)
    {
        for (int count = 0; count < 3; count++)
        {
            HapticsManager.Instance.Rumble(Motor.Left, 0.025f, target);
            await Task.Delay(150);
            HapticsManager.Instance.Rumble(Motor.Left, 0.025f, target);
            await Task.Delay(350);
        }
    }
}
