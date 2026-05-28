using UnityEngine;

public class AudioLoopManager : MonoBehaviour
{
    public static AudioLoopManager Instance { get; private set; }

    [SerializeField] private AudioSource audioSource;

    private AudioClip currentAudioSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (audioSource != null)
            audioSource.loop = true;
    }

    public void PlayAudio(AudioClip clip, float volume = 1f)
    {
        if (clip == null || audioSource == null)
            return;

        if (audioSource.isPlaying && currentAudioSource == clip)
            return;

        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.Play();

        currentAudioSource = clip;
    }

    public void StopAudio()
    {
        if (audioSource == null)
            return;

        audioSource.Stop();
        currentAudioSource = null;
    }
}