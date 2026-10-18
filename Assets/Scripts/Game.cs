using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public enum TONE
{
    NONE,
    TRIANGLE,
    CIRCLE,
    CROSS,
    SQUARE
}

public class Game : MonoBehaviour
{
    /*
     * Private Attributes
     */
    private Queue<TONE> solution;
    private Queue<TONE> answer;


    /*
     * Public Attributes
     */
    public AudioSource audioSource;
    public AudioClip triangleTone;
    public AudioClip squareTone;
    public AudioClip circleTone;
    public AudioClip crossTone;


    void Start()
    {
        solution = new Queue<TONE>();
        answer = new Queue<TONE>();
    }

    public void SetAnswer(Queue<TONE> answer)
    {
        this.answer = answer;
    }

    public Queue<TONE> ParseAnswerLine(string answerS)
    {
        Queue<TONE> res = new Queue<TONE>();
        foreach(var c in answerS) {
            switch (c)
            {
                case 'A':
                    res.Append(TONE.CROSS);
                    break;
                case 'B':
                    res.Append(TONE.CIRCLE);
                    break;
                case 'C':
                    res.Append(TONE.TRIANGLE);
                    break;
                case 'D':
                    res.Append(TONE.SQUARE);
                    break;
                default:
                    break;
            }
        }
        return res;
    }

    public void Enqueue(TONE tone)
    {
        answer.Enqueue(tone);
    }

    /*
     * Compares the answer to the solution
     * Players the answer back to the players
     * The full song will play if they solve the puzzle
     */
    public void CheckAnswer()
    {
        // Iterate over answer queue and play each tone using the audio manager
        foreach (var tone in answer)
        {
            switch (tone)
            {
                case TONE.TRIANGLE:
                    audioSource.PlayOneShot(triangleTone);
                    break;
                case TONE.CIRCLE:
                    audioSource.PlayOneShot(circleTone);
                    break;
                case TONE.CROSS:
                    audioSource.PlayOneShot(crossTone);
                    break;
                case TONE.SQUARE:
                    audioSource.PlayOneShot(squareTone);
                    break;
                default:
                    break;
            }
        }
    }

    /*
     * Plays the solution for the players
     * Happens by default at the beginning of the level
     */
    public void PlaySolution()
    {
        // Iterate over answer queue and play each tone using the audio manager
        foreach (var tone in solution) {
            switch (tone) {
                case TONE.TRIANGLE:
                    audioSource.PlayOneShot(triangleTone);
                    break;
                case TONE.CIRCLE:
                    audioSource.PlayOneShot(circleTone);
                    break;
                case TONE.CROSS:
                    audioSource.PlayOneShot(crossTone);
                    break;
                case TONE.SQUARE:
                    audioSource.PlayOneShot(squareTone);
                    break;
                default:
                    break;
            }
        }
    }

    /*
     * Resets the answer queue, allowing the players to start over
     */
    public void ResetAnswer()
    {
        answer.Clear();
    }
}
