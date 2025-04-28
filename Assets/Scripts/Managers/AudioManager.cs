using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("Background Music")]
    public AudioClip backgroundMusic;
    [Range(0f, 1f)]
    public float backgroundMusicVolume = 0.15f;

    [Header("Sound Effects")]
    [Range(0f, 1f)]
    public float soundEffectVolume = 0.3f;

    private AudioSource bgMusicSource;
    private AudioSource sfxSource;

    public static AudioManager Instance;
    private void Awake()
    {
        // Create AudioSource components
        bgMusicSource = gameObject.AddComponent<AudioSource>();
        sfxSource = gameObject.AddComponent<AudioSource>();

        // Configure background music
        bgMusicSource.clip = backgroundMusic;
        bgMusicSource.loop = true;
        bgMusicSource.playOnAwake = true;
        bgMusicSource.volume = backgroundMusicVolume;

        // Configure sound effects
        sfxSource.volume = soundEffectVolume;
    }

    private void Start()
    {
        // Play background music if clip is assigned
        if (bgMusicSource.clip != null)
        {
            bgMusicSource.Play();
        }
        else
        {
            Debug.LogWarning("Background music clip is not assigned in AudioManager!");
        }
    }

    public void PlaySoundEffect(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning("Attempted to play a null AudioClip!");
        }
    }

    // Update volume based on MainMenuManager's master volume
    public void SetMasterVolume(float masterVolume)
    {
        bgMusicSource.volume = backgroundMusicVolume * masterVolume;
        sfxSource.volume = soundEffectVolume * masterVolume;
    }
}