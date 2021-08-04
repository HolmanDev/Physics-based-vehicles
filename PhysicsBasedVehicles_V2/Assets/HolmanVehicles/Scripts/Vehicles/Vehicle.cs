using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Vehicle : MonoBehaviour
{
    // General
    public Camera Cam;
    public List<VehiclePart> Parts = new List<VehiclePart>();
    [HideInInspector] public Rigidbody Rb;
    [HideInInspector] public VehicleControl VehicleControl;

    // General physics
    public float VehicleMass { get; private set; } // Kg
    public Vector3 CenterOfMass { get; private set; } // Local
    public float AirDensity { get; private set; } = 1.225f;
    public Vector3 Gravity { get; private set; } = new Vector3(0f, -9.82f, 0f); // Gravitational force felt by this object. This is not universal.

    // Debugging
    public DebugTools Debug; [Serializable] public class DebugTools {
        public bool DebugEnabled;

        public bool _airTunnelEnabled = false;
        [Tooltip("Euler Angles, worldspace")] public Vector3 _airTunnelRotation = Vector3.zero;
        [Tooltip("Worldspace")] public Vector3 _airTunnelPosition = Vector3.zero;

        public bool _testFlightEnabled = false;
        [Tooltip("Worldspace")] public Vector3 _testFlightV = Vector3.zero;

        public bool DrawLift = false;
        public bool DrawDrag = false;
        public bool DrawTangentualVelocity = false;
    }

    public void InitializeParts()
    {
        foreach(VehiclePart part in Parts)
        {
            part.Initialize(this);
        }
    }

    public Vector3 GetTotalThrust() {
        Vector3 thrust = new Vector3();

        foreach(VehiclePart part in Parts)
        {
            if(part is Engine) {
                thrust += ((Engine) part).GetEngineThrustVector();
            }
        }

        return thrust;
    }

    private void Awake()
    {
        // Set up all connections between the scripts
        Rb = GetComponent<Rigidbody>();
        UpdateInertiaTensor();
        VehicleControl = GetComponent<VehicleControl>();
        VehicleControl.Vehicle = this;
        InitializeParts();

        #region Debug
        if (Debug.DebugEnabled) 
        {
            Rb.velocity = Debug._testFlightV;
            //DebugManager.instance.SetOverlayItem(new OverlayItem("Velocity", 0, " : ", " m/s"));
        }
        #endregion
    }

    private void Update()
    {
        VehicleControl.UpdateControl();

        #region Debug
        if (Debug.DebugEnabled)
        {
            DebugManager.instance.SetOverlayItem(new OverlayItem("Speed", Rb.velocity.magnitude, ": ", " m/s"), 0);
            foreach(VehiclePart part in Parts)
            {
                if(part is IAerodynamicPart)
                {
                    (part as IAerodynamicPart).Aerodynamics.DrawPartAeorynamicForces(part);
                }
            }
        }
        #endregion
    }

    private void FixedUpdate() {
        CalculateMass();
        if(Parts.Count == 0) {
            Rb.isKinematic = true;
        }
        
        Gravity = Vector3.down * 9.82f;

        Rb.AddForce(Gravity * Rb.mass);
        //DebugManager.instance.SetOverlayItem(DebugManager.instance.GetOverlayItem("speed"), Rb.velocity.magnitude);
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.yellow;
        //Gizmos.DrawSphere(transform.position + transform.TransformDirection(Rb.centerOfMass), 0.3f);
        Gizmos.DrawSphere(transform.position + transform.TransformDirection(CenterOfMass), 0.3f);
        Gizmos.color = Color.blue;
        Vector3 centerOfLift = new Vector3();
        float totalLift = 0;
        foreach(VehiclePart part in Parts) {
            if (part is IAerodynamicPart)
            {
                Aerodynamics aerodynamics = (part as IAerodynamicPart).Aerodynamics;

                Vector3 worldAC = aerodynamics.GetAdjustedWorldAerodynamicCenter(part); //part.transform.position + part.transform.TransformDirection(aerodynamics.LocalAerodynamicCenter);
                Vector3 relativePosition = worldAC - transform.position;
                float magnitude = aerodynamics.LiftVector.magnitude;
                centerOfLift += magnitude * relativePosition;
                totalLift += magnitude;
            }
        }
        centerOfLift /= totalLift;
        Gizmos.DrawSphere(centerOfLift + transform.position, 0.3f);
    }

    private void LateUpdate()
    {
        if (Debug._airTunnelEnabled) { AirTunnel(); }
    }

    /// <summary>
    /// Finds all parts attatched to this vehicle using their "part" tag and connects them.
    /// </summary>
    public void FindParts()
    {
        Transform[] partTransforms = transform.GetChildrenWithTag("Part");
        List<VehiclePart> parts = new List<VehiclePart>();
        foreach (Transform partTransform in partTransforms)
        {
            if (partTransform.TryGetComponent(out VehiclePart vehiclePart))
            {
                parts.Add(vehiclePart);
                vehiclePart.Initialize(this);
            }
        }
        Parts = parts;
    }

    /// <summary>
    /// Calculate the total mass and the center of mass for the vehicle. Doesn't bother with the inertia tensor.
    /// </summary>
    public void CalculateMass()
    {
        CenterOfMass = Vector3.zero;
        VehicleMass = 0;

        for (int i = 0; i < Parts.Count; i++)
        {
            // Iterate mass
            VehiclePart part = Parts[i];
            Vector3 worldCOM = part.GetAdjustedWorldCenterOfMass();
            Vector3 vehicleLocalCOM = transform.InverseTransformPoint(worldCOM);//.normalized * (part.centerOfMass - transform.position).magnitude;

            CenterOfMass += vehicleLocalCOM * part.GetWeight();
            VehicleMass += part.GetWeight();
        }

        if (VehicleMass > 0) {
            CenterOfMass /= VehicleMass;
        } else {
            CenterOfMass = Vector3.zero;
        }

        Rb.centerOfMass = CenterOfMass; // Relative to the transforms origin.
        Rb.mass = VehicleMass;
    }

    /// <summary>
    /// Update the vehicle's inertia tensor.
    /// </summary>
    public void UpdateInertiaTensor() // !! This and CalculateMass should be called when parts are attached or removed.
    {
        GameObject inertiaTempObject = new GameObject();
        inertiaTempObject.transform.position = Vector3.zero;
        inertiaTempObject.transform.rotation = Quaternion.identity;
        PartCollidersToBoxColliders(inertiaTempObject);
        Rigidbody rigidbody = inertiaTempObject.AddComponent<Rigidbody>();
        rigidbody.mass = Rb.mass;
        rigidbody.ResetInertiaTensor();

        Rb.inertiaTensor = rigidbody.inertiaTensor;
        Rb.inertiaTensorRotation = Quaternion.identity;

        DestroyImmediate(inertiaTempObject);
    }

    /// <summary>
    /// Takes this vehicle's colliders, makes box collider versions of them and adds those to colliderHolder.
    /// </summary>
    private void PartCollidersToBoxColliders(GameObject colliderHolder)
    {
        for (int i = 0; i < Parts.Count; i++)
        {
            Collider[] colliders = Parts[i].GetComponents<Collider>();
            if (colliders != null)
            {
                for (int t = 0; t < colliders.Length; t++)
                {
                    // Skip if the collider is a trigger or if it's disabled.
                    if (colliders[t].isTrigger || !colliders[t].enabled || !colliders[t].gameObject.activeSelf) continue;

                    // Convert the collider to a box collider.
                    BoxCollider newCollider = colliderHolder.AddComponent<BoxCollider>();
                    if (colliders[t] is BoxCollider)
                    {
                        newCollider.CopyFrom(colliders[t] as BoxCollider);
                    }
                    else if (colliders[t] is MeshCollider)
                    {
                        newCollider.CopyFrom(colliders[t] as MeshCollider);
                    }
                    else if (colliders[t] is CapsuleCollider)
                    {
                        newCollider.CopyFrom(colliders[t] as CapsuleCollider);
                    }

                    // Recontextualize the box collider.
                    newCollider.size = newCollider.size.RecontextualizeVector(colliders[t].transform, transform);
                    newCollider.center = newCollider.center.RecontextualizePoint(colliders[t].transform, transform);
                }
            }
        }
    }

    /// <summary>
    /// Get the active command module. Returns null if no active command module is found.
    /// </summary>
    public VehiclePart GetCommandingPart()
    {
        foreach(VehiclePart part in Parts)
        {
            if(part.CommandingPart)
            {
                return part;
            }
        }

        return null;
    }

    /// <summary>
    /// Lock the airplane to a position and simulate aerodynamic forces. Very niche usecase.
    /// </summary>
    private void AirTunnel()
    {
        transform.position = Debug._airTunnelPosition;
        Vector3 newRotation = transform.rotation.eulerAngles;
        newRotation.x = Debug._airTunnelRotation.x;
        newRotation.y = Debug._airTunnelRotation.y;
        transform.rotation = Quaternion.Euler(newRotation);
    }
}
