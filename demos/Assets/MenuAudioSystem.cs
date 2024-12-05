using UnityEngine;
using System.Collections.Generic;

public class MenuAudioSystem : MonoBehaviour
{
    public static MenuAudioSystem Instance { get; private set; }

    [SerializeField] private AudioClip buttonHoverAudio;
    [SerializeField] private AudioClip buttonClickAudio;
    [SerializeField] private AudioClip demoExitAudio;

    [SerializeField] private int maxAudioSources = 5;
    [SerializeField] private float defaultVolume = 1f;

    private readonly List<AudioSource> audioSources = new List<AudioSource>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            CreateNewAudioSource();
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private AudioSource CreateNewAudioSource()
    {
        var newSource = gameObject.AddComponent<AudioSource>();
        newSource.playOnAwake = false;
        audioSources.Add(newSource);
        return newSource;
    }

    private AudioSource RequestAudioSource()
    {
        foreach (var source in audioSources)
        {
            if (source != null && !source.isPlaying)
            {
                return source;
            }
        }

        if (audioSources.Count < maxAudioSources)
        {
            return CreateNewAudioSource();
        }

        float longestTime = 0f;
        AudioSource oldestSource = audioSources[0];

        foreach (var source in audioSources)
        {
            if (source.time > longestTime)
            {
                longestTime = source.time;
                oldestSource = source;
            }
        }

        return oldestSource;
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

    public void PlayClip(AudioClip clip, float volume = -1f)
    {
        if (clip == null)
        {
            Debug.LogWarning("Attempted to play null audio clip");
            return;
        }

        var source = RequestAudioSource();
        source.volume = volume < 0 ? defaultVolume : volume;
        source.clip = clip;
        source.Play();
    }



    private void OnDestroy()
    {
        foreach (var source in audioSources)
        {
            if (source != null)
            {
                Destroy(source);
            }
        }
        audioSources.Clear();
    }


}