using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    public static AudioManager Instance;

    [Header("Background Music")]
    public AudioClip backgroundMusic;

    [Header("Sound Effects")]
    public AudioClip[] soundEffects;

    private AudioSource bgMusicSource;
    private AudioSource sfxSource;

    
    private void Awake()
    {
        
        // Create AudioSource components
        bgMusicSource = gameObject.AddComponent<AudioSource>();
        sfxSource = gameObject.AddComponent<AudioSource>();
        
        //TODO: Sound effects volume adjustment
        sfxSource.volume = 0.3f;
        
        // TODO: Background music settings
        bgMusicSource.loop = true;
        bgMusicSource.clip = backgroundMusic;
        bgMusicSource.playOnAwake = true;
        bgMusicSource.volume = 0.26f; 
    }

    private void Start()
    {
        //TODO: Play background music
        bgMusicSource.Play();
    }

    public void PlaySoundEffect(string soundName)
    {
        AudioClip clip = GetSoundEffectByName(soundName);
        
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning("Sound not found: " + soundName);
        }
    }

    private AudioClip GetSoundEffectByName(string name)
    {
        foreach (AudioClip clip in soundEffects)
        {
            if (clip.name == name)
            {
                return clip;
            }
        }
        return null;
    }
    
}