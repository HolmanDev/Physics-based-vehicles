using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseEngineEffect : MonoBehaviour
{
    // General
    [HideInInspector] public TEngine Engine;

    // Cached effects
    internal GameObject _groundDust;
    internal ParticleSystem _groundDustParticleSystem;
    internal GameObject _plume;
    internal ParticleSystem.MainModule _exhaust;
    public float ExhaustLifetime = 1;

    /// <summary>
    /// Casts ray out of the engine that is then used to display a ground dust effect.
    /// </summary>
    public virtual void GroundEffectRaycast()
    {
        RaycastHit groundHit;

        Physics.Raycast(transform.position, transform.TransformDirection(-Engine.LocalForwardDirection), out groundHit, 20f, LevelManagement.instance.groundMask);

        if (groundHit.collider != null && Engine._burning)
        {
            UpdateGroundDust(groundHit);
        }
        else
        {
            RemoveGroundDust();
        }
    }

    /// <summary>
    /// Creates the ground dust and assigns _groundDust and _groundDustParticleSystem.
    /// </summary>
    public virtual void CreateGroundDust(RaycastHit groundHit)
    {
        _groundDust = Instantiate(Engine.EngineConfig._groundDust,
                groundHit.point, Quaternion.Euler(groundHit.normal));
        _groundDust.transform.Rotate(new Vector3(-90, 0, 0));
        _groundDustParticleSystem = _groundDust.GetComponent<ParticleSystem>();
    }

    /// <summary>
    /// Send an update to the ground dust. Ground dust is created if it doesn't already exist.
    /// </summary>
    public virtual void UpdateGroundDust(RaycastHit groundHit)
    {
        // Create ground dust object if it doesn't already exist
        if (_groundDustParticleSystem == null)
        {
            CreateGroundDust(groundHit);
        }

        if (!_groundDustParticleSystem.isPlaying)
        {
            _groundDustParticleSystem.Play();
        }

        // Move ground dust to under engine
        _groundDust.transform.position = groundHit.point;
    }

    /// <summary>
    /// Stops the ground dust particle effect from playign, effectively removing it.
    /// </summary>
    public void RemoveGroundDust()
    {
        if (_groundDustParticleSystem != null)
        {
            if (_groundDustParticleSystem.isPlaying)
            {
                _groundDustParticleSystem.Stop();
            }
        }
    }

    /// <summary>
    /// Creates the plume.
    /// </summary>
    public virtual void CreatePlume()
    {
        EngineConfig engineConfig = Engine.EngineConfig;

        _plume = Instantiate(engineConfig.plume, Engine._gimbalPivot);
        _plume.transform.localPosition = engineConfig.plumePositionOffset;
        _plume.transform.localEulerAngles = engineConfig.plumeRotationOffset;
        _exhaust = _plume.transform.GetChildWithTag("Exhaust").GetComponent<ParticleSystem>().main;
    }

    /// <summary>
    /// Removes the plume associated with this engine.
    /// </summary>
    public virtual void RemovePlume()
    {
        Destroy(_plume);
        _exhaust.startLifetime = 0;
    }
}
