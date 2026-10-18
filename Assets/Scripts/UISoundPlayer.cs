using UnityEngine;

public class UISoundPlayer : MonoBehaviour
{
    [SerializeField] AudioSource audioSource;
    float lastPlayTime;
    AudioClip lastClip;

    public void Play(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;

        // ignore a duplicate call for the same clip within 0.1s
        if (clip == lastClip && Time.unscaledTime - lastPlayTime < 0.1f) return;

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();

        lastClip = clip;
        lastPlayTime = Time.unscaledTime;
    }
}