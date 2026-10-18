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

    public AudioSource audioSource;

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

    public void PlayWallsClip() {
        audioSource.PlayOneShot(wallsClip);
    }

    public void PlayWinClip() { audioSource.PlayOneShot(winClip); }

    public void PlayFailClip() { audioSource.PlayOneShot(failClip); }

    public void PlaySuccessClip() { audioSource.PlayOneShot(successClip); }


    public void PlayTone(TONE tone)
    {
        switch (tone)
        {
            case TONE.TRIANGLE:
                PlayNorthTone();
                break;
            case TONE.CIRCLE:
                PlayEastTone();
                break;
            case TONE.CROSS:
                PlaySouthTone();
                break;
            case TONE.SQUARE:
                PlayWestTone();
                break;
            default:
                break;
        }
    }

    private void PlayNorthTone() { audioSource.PlayOneShot(northTone); }
    private void PlaySouthTone() { audioSource.PlayOneShot(southTone); }
    private void PlayWestTone() { audioSource.PlayOneShot(westTone); }
    private void PlayEastTone() { audioSource.PlayOneShot(eastTone); }
}
