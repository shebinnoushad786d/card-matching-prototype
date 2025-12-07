using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    public AudioClip flipSfx;
    public AudioClip matchSfx;
    public AudioClip mismatchSfx;
    public AudioClip winSfx;
    AudioSource src;

    protected override void OnInit()
    {
        src = gameObject.AddComponent<AudioSource>();
        src.playOnAwake = false;
        DontDestroyOnLoad(gameObject);
    }

    public void PlayFlip() => PlayOneShot(flipSfx);
    public void PlayMatch() => PlayOneShot(matchSfx);
    public void PlayMismatch() => PlayOneShot(mismatchSfx);
    public void PlayWin() => PlayOneShot(winSfx);

    void PlayOneShot(AudioClip c)
    {
        if (c == null) return;
        src.PlayOneShot(c);
    }
}
