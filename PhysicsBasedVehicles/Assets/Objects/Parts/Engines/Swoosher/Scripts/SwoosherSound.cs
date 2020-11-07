using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class SwoosherSound : BaseEngineSound
{
    /// <summary>
    /// Plays the engine ignition sound once.
    /// </summary>
    public override IEnumerator PlayIgnitionSound()
    {
        AudioSource.volume = 0;
        // Play running sound if the engine hasn't been shut off
        if (Engine._burning)
        {
            PlayRunningSound();
        }
        yield break;
    }

    /// <summary>
    /// Plays the engine shutdown sound once.
    /// </summary>
    public override IEnumerator PlayShutdownSound()
    {
        // Smoothly lower volume
        for (int i = 0; i < 50; i++)
        {
            if (Engine._burning)
            {
                yield break;
            }
            else
            {
                AudioSource.volume = Mathf.Lerp(AudioSource.volume, 0, 0.1f);
                yield return new WaitForSeconds(0.025f);
            }
        }

        // Floor the volume
        AudioSource.volume = 0;
        AudioSource.Stop();
    }
}
