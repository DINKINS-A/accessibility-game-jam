using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Emitter : MonoBehaviour
{
    private TONE currentTone;
    private short repititions;
    private int playerID;
    private PlayerController playerController;
    void Start()
    {
        repititions = 0;
        //gameObject.SendMessage("Enqueue", 5.0);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //Debug.Log("OnTriggerEnter2D" + collision.name);
        playerController = collision.GetComponent<PlayerController>();
        playerController.SetTouchingEmitter(this);
        Debug.Log("set touching emitter");
    }
    //private void OnCollisionEnter2D(Collision2D collision)
    //{

    //    Debug.Log(collision.collider.transform.position.x.ToString());
    //    playerController = collision.gameObject.GetComponent<PlayerController>();
    //}
    //private void OnCollisionExit2D(Collision2D collision)
    //{
    //    playerController = null;
    //}
    //Upon collision with another GameObject, this GameObject will reverse direction
    private void OnTriggerExit2D(Collider2D collision)
    {
        //playerController.UnsetTouchingEmitter();
        //playerController = null;
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
        this.currentTone = currentTone;
        AudioManager.Instance.PlayClip(AudioManager.Instance.GetSuccessClip());
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
