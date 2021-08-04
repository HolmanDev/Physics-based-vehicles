using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Structure : VehiclePart, IAerodynamicPart
{
    public StructuralConfig StructuralConfig
    {
        get
        {
            return PartConfig as StructuralConfig;
        }
        set
        {
            StructuralConfig = value;
        }
    }

    private Aerodynamics _aerodynamics = new Aerodynamics();
    public Aerodynamics Aerodynamics => _aerodynamics;

    [Header("Structure")]

    public ProceduralColor ProceduralColor = new ProceduralColor();

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(GetAdjustedWorldCenterOfMass(), 0.05f);
        Gizmos.color = Color.blue;
        Vector3 worldAC = Aerodynamics.GetAdjustedWorldAerodynamicCenter(this); //transform.position + transform.TransformDirection(StructuralConfig.aerodynamics.LocalAerodynamicCenter);
        Gizmos.DrawWireSphere(worldAC, 0.05f);
        Handles.color = Color.green;
        Handles.Label(transform.position, Mathf.Round(Aerodynamics.AoA).ToString() + "°");
    }

    private void OnValidate()
    {
        ProceduralColor.Repaint(ProceduralColor.Color, ProceduralColor.Texture);
    }

    private void Update()
    {
        #region Debug
        if (Vehicle.Debug.DebugEnabled)
        {
            Vector3 worldAC = Aerodynamics.GetAdjustedWorldAerodynamicCenter(this); //transform.position + transform.TransformDirection(StructuralConfig.aerodynamics.LocalAerodynamicCenter);
            Vector3 aerodynamicForce = Aerodynamics.LiftVector + Aerodynamics.DragVector;
            //Debug.DrawRay(worldAC, aerodynamicForce.normalized * 3, Color.magenta, 0, false);
            //Debug.DrawRay(worldAC, Aerodynamics.LiftVector.normalized * 3, Color.blue, 0, false);
            //Debug.DrawRay(worldAC, Aerodynamics.DragVector.normalized * 3, Color.red, 0, false);
            Debug.DrawRay(worldAC, Aerodynamics.LiftDirection.normalized * 3, Color.red, 0, false);
            Debug.DrawRay(worldAC, Aerodynamics.WorldVelocityDirection.normalized * 3, Color.blue, 0, false);
        }
        #endregion
    }

    private void FixedUpdate()
    {
        Aerodynamics.ExecuteAerodynamicForces(this, 0);
    }

    public override float GetWeight()
    {
        return StructuralConfig.mass;
    }
}
