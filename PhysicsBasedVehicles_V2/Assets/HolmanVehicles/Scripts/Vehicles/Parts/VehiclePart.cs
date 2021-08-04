using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class VehiclePart : MonoBehaviour
{
    public VehiclePart[] ChildParts;
    [HideInInspector] public Vehicle Vehicle;
    public float ExplosionVelocitySquared => PartConfig.explosionVelocity * PartConfig.explosionVelocity;
    public VehiclePartConfig PartConfig;
    [HideInInspector] public bool Mirrored = false;
    public Vector3 MirrorVector { get { return new Vector3(Mirrored ? -1f : 1f, 1f, 1f); } } // Multiply with vector to mirror it.
    public bool CommandingPart = false;

    private void OnDrawGizmos() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(GetAdjustedWorldCenterOfMass(), 0.1f);
    }

    private void OnTriggerEnter(Collider other)
    {
        LayerMask groundMask = LevelManagement.instance.groundMask;
        if (groundMask == (groundMask | (1 << other.gameObject.layer)))
        {
            Vector3 velocity = Vehicle.Rb.GetVelocityAtPoint(GetAdjustedLocalCenterOfMass(), Space.Self); // !! Use other method
            HandleCollision(velocity);
        }
        
    }

    /// <summary>
    /// Initializes all connections.
    /// </summary>
    public virtual void Initialize(Vehicle vehicle) {
        Vehicle = vehicle;
    }

    /// <summary>
    /// Executes the proper response to a collision.
    /// </summary>
    public void HandleCollision(Vector3 velocity)
    {
        if(velocity.sqrMagnitude > ExplosionVelocitySquared)
        {
            Explode();
        }
    }

    /// <summary>
    /// Destroys the part in an explosion.
    /// </summary>
    public void Explode()
    {
        // Explode children
        foreach (VehiclePart childPart in ChildParts)
        {
            if (childPart != null)
            {
                childPart.Explode();
            }
        }

        // VFX
        if(Vehicle.Cam.TryGetComponent(out CameraFollowVehicle cameraFollowVehicle))
        {
            cameraFollowVehicle.ShakeCamera(1f, 0.8f, 1f, 10f, 0.5f, 0.25f, 0.7f, 2f, 35f, 0.7f);
        }
        GameObject explosionObj = Instantiate(PartConfig.explosion, transform.position, transform.rotation);

        // SFX
        AudioSource audioSource = explosionObj.AddComponent<AudioSource>();
        AudioManagement.instance.PlayAudioClip(audioSource, "Explosion");
        float volume = 1f / AudioManagement.instance.ActiveAudioSources.Count;
        AudioManagement.instance.UpdateAllVolume(volume, "Explosion");

        DestroyPart();
    }

    /// <summary>
    /// Destroys the part and properly removes all connections.
    /// </summary>
    public void DestroyPart()
    {
        Vehicle.Parts.RemoveAndFill(this);
        DestroyConnections();
        Destroy(gameObject);
    }

    public virtual void DestroyConnections() { }

    public abstract float GetWeight();

    /// <summary>
    /// Get the center of mass adjusted for mirror et cetera.
    /// </summary>
    public Vector3 GetAdjustedLocalCenterOfMass()
    {
        return Vector3.Scale(PartConfig.centerOfMass, MirrorVector);
    }

    /// <summary>
    /// Get the center of mass in world space adjusted for mirror et cetera.
    /// </summary>
    public Vector3 GetAdjustedWorldCenterOfMass()
    {
        return transform.position + transform.TransformDirection(GetAdjustedLocalCenterOfMass());
    }
}
