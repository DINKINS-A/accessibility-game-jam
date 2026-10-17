using UnityEngine;

public class AudioManager : MonoBehaviour
{

    public AudioClip northTone;
    public AudioClip southTone;
    public AudioClip eastTone;
    public AudioClip westTone;
    public AudioClip wallsClip;
    public AudioClip winClip;
    public AudioClip loseClip;
    public AudioClip failClip;
    public AudioClip successClip;

    public static AudioManager Instance { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public AudioClip GetNorthTone() { return northTone; }

    public AudioClip GetSouthTone() { return southTone; }

    public AudioClip GetEastTone() { return eastTone; }

    public AudioClip GetWestTone() { return westTone; }

    public AudioClip GetWallsClip() { return wallsClip; }

    public AudioClip GetWinClip() { return winClip; }

    public AudioClip GetFailClip() { return failClip; }

    public AudioClip GetSuccessClip() { return successClip; }
}
