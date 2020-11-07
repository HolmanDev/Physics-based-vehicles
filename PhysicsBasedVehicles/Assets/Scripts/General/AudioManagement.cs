using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManagement : MonoBehaviour
{
    public static AudioManagement instance;

    public AudioClip[] audioClips;
    [HideInInspector] public List<AudioSource> ActiveAudioSources = new List<AudioSource>();

    private void Awake()
    {
        // Singleton
        if(instance != null)
        {
            Destroy(gameObject);
        } else
        {
            instance = this;
        }
    }

    /// <summary>
    /// Finds an audio clip with the specified name from the archive.
    /// </summary>
    public AudioClip FindAudioClip(string name)
    {
        for(int i = 0; i < audioClips.Length; i++)
        {
            if(audioClips[i].name == name)
            {
                return audioClips[i];
            }
        }

        throw new System.Exception("Could not find audio clip with name '" + name + "'");
    }

    /// <summary>
    /// Plays an audio clip with a specified name from the archive. Default volume is 1, but can be adjusted.
    /// </summary>
    public void PlayAudioClip(AudioSource audioSauce, string name, float volume = 1)
    {
        if (!ActiveAudioSources.Contains(audioSauce))
        {
            ActiveAudioSources.Add(audioSauce);
        }
        audioSauce.clip = FindAudioClip(name);
        audioSauce.volume = volume;
        audioSauce.Play();
        StartCoroutine(TryRemoveAudioSourceFromActiveList(audioSauce));
    }

    /// <summary>
    /// Tries to remove an audio source from the active list. Will abort if the clip is replayed or
    /// another clip starts playing.
    /// </summary>
    private IEnumerator TryRemoveAudioSourceFromActiveList(AudioSource audioSauce)
    {
        float length = audioSauce.clip.length;

        yield return new WaitForSeconds(length);

        if (audioSauce != null)
        {
            if (Mathf.Abs(audioSauce.time - length) > 0.1f)
            {
                yield break;
            }
            ActiveAudioSources.Remove(audioSauce);
        }
    }

    /// <summary>
    /// Update the volume of all active audio sources that fit the clip name.
    /// If no clip name is specified, all active audio sources are selected.
    /// </summary>
    public void UpdateAllVolume(float volume, string clipName = null)
    {
        foreach(AudioSource audioSource in ActiveAudioSources)
        {
            if (clipName == null || clipName == audioSource.clip.name)
            {
                audioSource.volume = volume;
            }
        }
    }
}
