using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class VehiclePartConfig : ScriptableObject
{
    [Tooltip("Kg")] public float mass;
    public GameObject explosion;
    [Tooltip("m/s")] public float explosionVelocity;
    public AerodynamicsData aerodynamicsData;
    [Tooltip("m, Local")]public Vector3 centerOfMass;
    [Tooltip("Local lateral direction")] public Vector3 localRightDirection;
    public Vector3 localForwardDirection;
}
