using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public abstract class TStructuralPart : VehiclePart
{
    [Header("Structure")]

    public StructuralConfig StructuralConfig;
    public ProceduralColor ProceduralColor = new ProceduralColor();
    public Aerodynamics Aerodynamics = new Aerodynamics();

    private void OnDrawGizmos() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position + transform.TransformDirection(centerOfMass), 0.1f);
        Gizmos.color = Color.blue;
        Vector3 worldAC = transform.position + transform.TransformDirection(Aerodynamics.LocalAerodynamicCenter);
        Gizmos.DrawSphere(worldAC, 0.1f);
        Gizmos.color = Color.white;
        Handles.Label(transform.position, Mathf.Round(Aerodynamics.AoA).ToString() + "°");
    }

    private void OnValidate()
    {
        ProceduralColor.Repaint(ProceduralColor.Color, ProceduralColor.Texture);
    }

    private void Awake() {
        Type = VehiclePartType.Structural;
    }

    private void Update() {
        #region Debug
        if(Vehicle.Debug.DebugEnabled) 
        {
            Vector3 worldAC = transform.position + transform.TransformDirection(Aerodynamics.LocalAerodynamicCenter);
            Vector3 aerodynamicForce = Aerodynamics.LiftVector + Aerodynamics.DragVector;
            Debug.DrawRay(worldAC, aerodynamicForce.normalized * 3, Color.magenta, 0, false);
            Debug.DrawRay(worldAC, Aerodynamics.LiftVector.normalized * 3, Color.blue, 0, false);
            Debug.DrawRay(worldAC, Aerodynamics.DragVector.normalized * 3, Color.red, 0, false);
        }
        #endregion
    }

    private void FixedUpdate() {
        Aerodynamics.ExecuteAerodynamicForces(this, 0);
    }

    public override float GetWeight() {
        return StructuralConfig.weight;
    }
}
