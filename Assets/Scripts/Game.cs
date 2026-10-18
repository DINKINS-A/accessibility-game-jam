using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    private Queue<TONE> submission;
    private Queue<TONE> answer;
    private const int TONE_INTERVAL = 500;


    /*
     * Public Attributes
     */
    public AudioClip triangleTone;
    public AudioClip squareTone;
    public AudioClip circleTone;
    public AudioClip crossTone;

    public static Game Instance {  get; private set; }

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        submission = new Queue<TONE>();
        answer = new Queue<TONE>();
        DontDestroyOnLoad(gameObject);
    }



    public bool CheckWin()
    {
        if (answer.Count == 0 || submission.Count == 0) return false;
        var a = submission.ToList();
        var b = answer.ToList();
        for (int i = 0; i < submission.Count; ++i)
        {

            if (!a[i].Equals(b[i]))
            {
                Debug.Log("Lose");
                return false;
            }
        }
        Debug.Log("Win");
        return true;
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
                    res.Enqueue(TONE.CROSS);
                    break;
                case 'B':
                    res.Enqueue(TONE.CIRCLE);
                    break;
                case 'C':
                    res.Enqueue(TONE.TRIANGLE);
                    break;
                case 'D':
                    res.Enqueue(TONE.SQUARE);
                    break;
                default:
                    break;
            }
        }
        return res;
    }

    public void Enqueue(TONE tone)
    {
        submission.Enqueue(tone);
    }

    /*
     * Compares the answer to the submission
     * Players the answer back to the players
     * The full song will play if they solve the puzzle
     */
    public async void CheckAnswer()
    {
        // Iterate over answer queue and play each tone using the audio manager
        foreach (var tone in answer)
        {
            Debug.Log(tone);
            AudioManager.Instance.PlayTone(tone);
            await Task.Delay(TONE_INTERVAL);
        }
    }

    /*
     * Plays the submission for the players
     * Happens by default at the beginning of the level
     */
    public async void PlaySubmission()
    {
        // Iterate over answer queue and play each tone using the audio manager
        foreach (var tone in submission) {
            AudioManager.Instance.PlayTone(tone);
            await Task.Delay(TONE_INTERVAL);
        }
    }

    /*
     * Resets the answer queue, allowing the players to start over
     */
    public void ResetAnswer()
    {
        submission.Clear();
    }
}
