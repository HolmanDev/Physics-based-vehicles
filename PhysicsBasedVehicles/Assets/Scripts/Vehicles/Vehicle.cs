using System;
using System.Collections.Generic;
using UnityEngine;

public class Vehicle : MonoBehaviour
{
    // General
    public Camera Cam;
    public List<VehiclePart> Parts = new List<VehiclePart>();
    [HideInInspector] public Rigidbody Rb;
    [HideInInspector] public VehicleControl VehicleControl;

    // General physics
    [HideInInspector] public float VehicleMass; // Kg
    [HideInInspector] public Vector3 CenterOfMass; // Local
    private float _airDensity = 1.225f;
    [HideInInspector] public float AirDensity => _airDensity;

    // Debugging
    public DebugTools Debug; [Serializable] public class DebugTools {
        public bool DebugEnabled;

        public bool _airTunnelEnabled = false;
        [Tooltip("Euler Angles, worldspace")] public Vector3 _airTunnelRotation = Vector3.zero;
        [Tooltip("Worldspace")] public Vector3 _airTunnelPosition = Vector3.zero;

        public bool _testFlightEnabled = false;
        [Tooltip("Worldspace")] public Vector3 _testFlightV = Vector3.zero;
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
            if(part.Type == VehiclePartType.Engine) {
                thrust += ((TEngine) part).GetEngineThrustVector();
            }
        }

        return thrust;
    }

    private void Awake()
    {
        // Set up all connections between the scripts
        Rb = GetComponent<Rigidbody>();
        VehicleControl = GetComponent<VehicleControl>();
        VehicleControl.Vehicle = this;
        InitializeParts();

        #region Debug
        if(Debug.DebugEnabled) 
        {
            Rb.velocity = Debug._testFlightV;
        }
        #endregion
    }

    private void Update()
    {
        if (Debug._testFlightEnabled)
        {
            VehicleControl.UpdateControl();
        }

        CalculateMass();

        if(Parts.Count == 0) {
            Rb.isKinematic = true;
        }

        #region Debug
        if (Debug.DebugEnabled)
        {
            DebugManager.instance.SetOverlayItem(new OverlayItem("Speed", Rb.velocity.magnitude, ": ", " m/s"), 0);
        }
        #endregion
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position + transform.TransformDirection(Rb.centerOfMass), 0.3f);
        Gizmos.color = Color.blue;
        Vector3 centerOfLift = new Vector3();
        float totalLift = 0;
        foreach(VehiclePart part in Parts) {
            Aerodynamics aerodynamics = new Aerodynamics();
            if(part.Type == VehiclePartType.MoveableStructural) {
                aerodynamics = ((TMoveableStructure) part).Aerodynamics;
            } else if(part.Type == VehiclePartType.Structural) {
                aerodynamics = ((TStructuralPart) part).Aerodynamics;
            }

            Vector3 worldAC = transform.position + transform.TransformDirection(aerodynamics.LocalAerodynamicCenter);
            float magnitude = aerodynamics.LiftVector.magnitude;
            centerOfLift += magnitude * worldAC;
            totalLift += magnitude;
        }
        centerOfLift /= totalLift;
        Gizmos.DrawSphere(centerOfLift, 0.3f);
    }

    private void FixedUpdate() {
        Rb.inertiaTensorRotation = Quaternion.identity;
    }

    private void LateUpdate()
    {
        if (Debug._airTunnelEnabled) { AirTunnel(); }
    }

    /// <summary>
    /// Calculate the total mass and the center of mass for the vehicle.
    /// </summary>
    public void CalculateMass()
    {
        CenterOfMass = Vector3.zero;
        VehicleMass = 0;

        for (int i = 0; i < Parts.Count; i++)
        {
            // Iterate mass
            VehiclePart part = Parts[i];
            Vector3 worldCOM = part.transform.position + part.transform.TransformDirection(part.centerOfMass);
            Vector3 vehicleLocalCOM = transform.InverseTransformPoint(worldCOM);//.normalized * (part.centerOfMass - transform.position).magnitude;

            CenterOfMass += vehicleLocalCOM * part.GetWeight();
            VehicleMass += part.GetWeight();
        }

        if (VehicleMass > 0) {
            CenterOfMass /= VehicleMass;
        } else {
            CenterOfMass = Vector3.zero;
        }

        Rb.centerOfMass = CenterOfMass;
        Rb.mass = VehicleMass;
    }

    /// <summary>
    /// Get the active command module. Returns null if no active command module is found.
    /// </summary>
    public VehiclePart GetActiveCommandModule()
    {
        foreach(VehiclePart part in Parts)
        {
            if(part.name == "Cockpit")
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
