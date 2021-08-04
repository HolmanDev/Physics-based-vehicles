using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEditor;

public interface IAerodynamicPart
{
    Aerodynamics Aerodynamics { get; }
}

// !! Perhaps combine with the regular structural parts? Rename to control surface?
public class ControlSurface : VehiclePart, IAerodynamicPart
{
    public MoveableStructuralConfig MoveableStructuralConfig
    {
        get
        {
            return PartConfig as MoveableStructuralConfig;
        }
        set
        {
            MoveableStructuralConfig = value;
        }
    }

    private Aerodynamics _aerodynamics = new Aerodynamics();
    public Aerodynamics Aerodynamics => _aerodynamics;

    [Header("Moveable Structure")]

    public ProceduralColor ProceduralColor = new ProceduralColor();

    // Movement
    public SurfaceControls SurfaceControls = new SurfaceControls();

    void Awake()
    {
        SurfaceControls.Part = this;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(GetAdjustedWorldCenterOfMass(), 0.05f);
        Gizmos.color = Color.blue;
        Vector3 worldAC = Aerodynamics.GetAdjustedWorldAerodynamicCenter(this); //transform.position + transform.TransformDirection(MoveableStructuralConfig.aerodynamics.GetAdjustedAerodynamicCenter(this));
        Gizmos.DrawWireSphere(worldAC, 0.05f);
        Handles.color = Color.green;
        Handles.Label(transform.position, Mathf.Round(Aerodynamics.AoA).ToString() + "°");
    }

    private void Update()
    {
        #region Debug
        if (Vehicle.Debug.DebugEnabled)
        {
            Vector3 worldAC = Aerodynamics.GetAdjustedWorldAerodynamicCenter(this); //transform.position + transform.TransformDirection(MoveableStructuralConfig.aerodynamics.GetAdjustedAerodynamicCenter(this));
            Vector3 aerodynamicForce = Aerodynamics.LiftVector + Aerodynamics.DragVector;
            //Debug.DrawRay(worldAC, aerodynamicForce.normalized * 3, Color.magenta, 0, false);
            //Debug.DrawRay(worldAC, Aerodynamics.LiftVector.normalized * 3, Color.blue, 0, false);
            //Debug.DrawRay(worldAC, Aerodynamics.DragVector.normalized * 3, Color.red, 0, false);
            Debug.DrawRay(worldAC, Aerodynamics.LiftDirection.normalized * 3, Color.red, 0, false);
            Debug.DrawRay(worldAC, Aerodynamics.WorldVelocityDirection.normalized * 3, Color.blue, 0, false);
        }
        #endregion
    }

    private void OnValidate()
    {
        ProceduralColor.Repaint(ProceduralColor.Color, ProceduralColor.Texture);
    }

    private void FixedUpdate()
    {
        Aerodynamics.ExecuteAerodynamicForces(this, SurfaceControls.Angle);
    }

    public override float GetWeight()
    {
        return MoveableStructuralConfig.mass;
    }
}