using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseEngineSound : MonoBehaviour
{
    [HideInInspector] public Engine Engine;

    [HideInInspector] public AudioSource AudioSource;

    [SerializeField] internal float _maxVolume = 1f;
    [HideInInspector] public float MaxVolume => _maxVolume;

    /// <summary>
    /// Plays the engine ignition sound once.
    /// </summary>
    public virtual IEnumerator PlayIgnitionSound()
    {
        AudioSource.volume = 0;
        AudioManagement.instance.PlayAudioClip(AudioSource, Engine.EngineConfig.ignitionSound);

        // Wait until ignition sound is over
        yield return new WaitForSeconds(AudioManagement.instance.FindAudioClip(Engine.EngineConfig.ignitionSound).length);

        // Play running sound if the engine hasn't been shut off
        if (Engine._burning)
        {
            PlayRunningSound();
        }
    }

    /// <summary>
    /// Plays the engine shutdown sound once.
    /// </summary>
    public virtual IEnumerator PlayShutdownSound()
    {
        AudioManagement.instance.PlayAudioClip(AudioSource, Engine.EngineConfig.shutdownSound);

        // Wait until shutdown sound is over
        yield return new WaitForSeconds(AudioManagement.instance.FindAudioClip(Engine.EngineConfig.shutdownSound).length);

        // Floor the volume
        AudioSource.volume = 0;
        AudioSource.Stop();
    }

    /// <summary>
    /// Plays the engine running sound continiously after being started.
    /// </summary>
    public virtual void PlayRunningSound()
    {
        AudioManagement.instance.PlayAudioClip(AudioSource, Engine.EngineConfig.burningSound);
        AudioSource.loop = true;
    }
}
