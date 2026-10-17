using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Emitter : MonoBehaviour
{
    private AudioClip currentTone;
    private short repititions;
    private int playerID;
    
    void Start()
    {
        repititions = 0;
        //gameObject.SendMessage("Enqueue", 5.0);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject)
        {

        }
    }
    //Upon collision with another GameObject, this GameObject will reverse direction
    private void OnTriggerEnter(Collider other)
    {
        // TODO: Player Input Manager component
        //  limit 2 players
        //  assign persistent player identity on join (left or right) 
        //  PlayerInput.playerIndex identifies the joined player,
        //  store player index in player controller class, get in collider, retrieve index

        //PlayerController player =
        //    other.GetComponentInParent<PlayerController>();

        //if (player == null)
        //    return;

        //if (player.pressedButton != -1)
        //{
        //    assignedInt = player.pressedButton;

        //    Debug.Log(
        //        $"Player {player.playerId} assigned value {assignedInt}"
        //    );
        //}

        //if (other.CompareTag("Player"))
        //{
        //    playerOnPlatform = true;
        //    Debug.Log("Press E to activate platform");
        //}
    }

    private void OnTriggerExit(Collider other)
    {
        //if (other.CompareTag("Player"))
        //{
        //    playerOnPlatform = false;
        //}
    }

    // Update is called once per frame
    void Update()
    {
    }

    /*
     * Assign which player this emitter belongs to
     */

    public void Assign(int playerID)
    {
        this.playerID = playerID;
    }

    
    public int GetAssignment() {
        return playerID;
    }

    /*
 * Set the number of times this emitter will play a sound
 */
    public void GetRepititions(short repititions)
    {
        this.repititions = repititions;
    }

    /*
     * Set the number of times this emitter will play a sound
     */
    public void SetRepititions(short repititions)
    {
        this.repititions = repititions;
    }

    /*
     * Called when the player interacts with this emitter
     */
    void TriggerRepitionPulse()
    {
        for (int count = 0; count < repititions; count++)
        {
            //HapticsManager.Instance.Pulse(10, 10, 1);
        }
    }
}
