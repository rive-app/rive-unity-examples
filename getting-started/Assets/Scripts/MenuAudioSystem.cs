using System.Collections.Generic;
using UnityEngine;

public class MenuAudioSystem : MonoBehaviour
{

    public static MenuAudioSystem Instance { get; private set; }

    public AudioClip buttonHoverAudio;
    public AudioClip buttonClickAudio;
    public AudioClip demoExitAudio;

    private List<AudioSource> audioSources = new List<AudioSource>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist this object across scenes
        }
        else if (Instance != this)
        {
            Destroy(gameObject); // Destroy any duplicates
        }
    }

    public AudioSource RequestAudioSource()
    {
        foreach (var source in audioSources)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }

        // If no AudioSource is available, create a new one
        var newSource = gameObject.AddComponent<AudioSource>();
        audioSources.Add(newSource);
        return newSource;
    }


    public void PlayHoverSound()
    {
        PlayClip(buttonHoverAudio);
    }

    public void PlayClickSound()
    {
        PlayClip(buttonClickAudio);
    }

    public void PlayScreenEnter()
    {
        PlayClip(demoExitAudio);
    }

    public void PlayClip(AudioClip clip, float volume = 1f)
    {
        var source = RequestAudioSource();
        source.volume = volume;
        source.clip = clip;
        source.Play();
    }
}
