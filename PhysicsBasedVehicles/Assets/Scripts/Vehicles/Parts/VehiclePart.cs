using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VehiclePartType {Engine, Container, Structural, MoveableStructural};

public abstract class VehiclePart : MonoBehaviour
{
    public VehiclePart[] ChildParts;
    public GameObject Explosion;
    public Vector3 centerOfMass;
    [Tooltip("Local lateral direction")] public Vector3 LocalRightDirection;
    public Vector3 LocalForwardDirection;
    [HideInInspector] public Vehicle Vehicle;
    [HideInInspector] public VehiclePartType Type; // Code smell?
    [SerializeField, Tooltip("(m/s)^2")] private float _explosionVelocitySquared = 2500f;

    private void OnDrawGizmos() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position + transform.TransformDirection(centerOfMass), 0.1f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Ground")
        {
            Vector3 velocity = Vehicle.Rb.GetVelocityAtPoint(centerOfMass, Space.Self);
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
        if(velocity.sqrMagnitude > _explosionVelocitySquared)
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
        Vehicle.Cam.GetComponent<CameraFollowVehicle>().ShakeCamera(1f, 0.8f, 1f, 10f, 0.5f, 0.25f, 0.7f, 2f, 35f, 0.7f);
        GameObject explosionObj = Instantiate(Explosion, transform.position, transform.rotation);

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
}
