using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Perhaps make more general? Instanced only for debugging.
[System.Serializable]
public class Aerodynamics
{
    private float _aoa;
    public float AoA => _aoa;

    #region Debug
    private Vector3 _liftVector = new Vector3();
    public Vector3 LiftVector => _liftVector;
    private Vector3 _dragVector = new Vector3();
    public Vector3 DragVector => _dragVector;
    private Vector3 _liftDirection = new Vector3();
    public Vector3 LiftDirection => _liftDirection;
    private Vector3 _worldVelocityDirection = new Vector3();
    public Vector3 WorldVelocityDirection => _worldVelocityDirection;
    public Vector3 WorldTangentualVelocity { get; private set; }
    #endregion

    /// <summary>
    /// Execute all aerodynamic functionalities.
    /// </summary>
    public void ExecuteAerodynamicForces(VehiclePart part, float angle) // !! Make it so that part is already known. Constructor?
    {
        Vehicle vehicle = part.Vehicle;

        // Thank you to Ivan "gasgiant" Pensionerov for the physics substep code!
        // https://github.com/gasgiant/Aircraft-Physics
        // https://youtu.be/p3jDJ9FtTyM
        ForceAndTorque forceAndTorqueThisFrame = CalculateForces(part, angle, vehicle.Rb.velocity, vehicle.Rb.angularVelocity);
        Vector3 velocityPrediction = PredictVelocity(vehicle, 
            forceAndTorqueThisFrame.Force + vehicle.GetTotalThrust() + vehicle.Gravity * vehicle.Rb.mass, 0.5f);
        Vector3 angularVelocityPrediction = PredictAngularVelocity(vehicle, forceAndTorqueThisFrame.Torque, 0.5f);
        ForceAndTorque predictedForceAndTorque = CalculateForces(part, angle, velocityPrediction, angularVelocityPrediction);
        ForceAndTorque forceAndTorque = (forceAndTorqueThisFrame + predictedForceAndTorque) * 0.5f;

        vehicle.Rb.AddForce(forceAndTorqueThisFrame.Force);
        vehicle.Rb.AddTorque(forceAndTorqueThisFrame.Torque);
    }

    public ForceAndTorque CalculateForces(VehiclePart part, float angle, Vector3 vehicleWorldVelocity, Vector3 vehicleWorldAngularVelocity) {
        Vector3 worldAC = GetAdjustedWorldAerodynamicCenter(part);
        Vector3 relativePosition = worldAC - part.Vehicle.Rb.worldCenterOfMass;
        Vector3 partWorldVelocity = RigidbodyExtensions.GetChildVelocity(part.Vehicle.Rb.worldCenterOfMass, vehicleWorldVelocity, vehicleWorldAngularVelocity, worldAC);
        if(part is IAerodynamicPart)
        {
            Vector3 tangentualWorldVelocity = partWorldVelocity - vehicleWorldVelocity;
            WorldTangentualVelocity = tangentualWorldVelocity;
        }

        Vector3 lift = GetLiftVector(part, angle, partWorldVelocity);
        Vector3 drag = GetDragVector(part, angle, partWorldVelocity);
        Vector3 aerodynamicForce = lift + drag;
        Vector3 torque = Vector3.Cross(relativePosition, aerodynamicForce); // τ = r × F.

        return new ForceAndTorque(aerodynamicForce, torque);
    }

    private Vector3 PredictVelocity(Vehicle vehicle, Vector3 force, float timestep)
    {
        return vehicle.Rb.velocity + Time.fixedDeltaTime * timestep * force / vehicle.Rb.mass;
    }

    private Vector3 PredictAngularVelocity(Vehicle vehicle, Vector3 torque, float timestep)
    {
        Quaternion inertiaTensorWorldRotation = vehicle.Rb.rotation * vehicle.Rb.inertiaTensorRotation;
        Vector3 torqueInDiagonalSpace = Quaternion.Inverse(inertiaTensorWorldRotation) * torque;
        Vector3 angularVelocityChangeInDiagonalSpace;
        angularVelocityChangeInDiagonalSpace.x = torqueInDiagonalSpace.x / vehicle.Rb.inertiaTensor.x;
        angularVelocityChangeInDiagonalSpace.y = torqueInDiagonalSpace.y / vehicle.Rb.inertiaTensor.y;
        angularVelocityChangeInDiagonalSpace.z = torqueInDiagonalSpace.z / vehicle.Rb.inertiaTensor.z;

        return vehicle.Rb.angularVelocity + Time.fixedDeltaTime * timestep
            * (inertiaTensorWorldRotation * angularVelocityChangeInDiagonalSpace);
    }

    /// <summary>
    /// Returns the angle of attack of the part in degrees
    /// </summary>
    public float GetAoA(VehiclePart part, float angle, Vector3 worldVelocity)
    {
        float aoa;
        Vehicle vehicle = part.Vehicle;
        // Get the velocity in vehicle space
        Vector3 velocityVehicleSpace = vehicle.transform.InverseTransformDirection(worldVelocity);
        // Get the right direction of the part in vehicle space
        Vector3 partRightDirectionVehicleSpace = 
            part.PartConfig.localRightDirection.RecontextualizeDirection(part.transform, vehicle.transform).normalized;
        // Flatten the velocity in vehicle space to a plane perpendicular to the part's right direction in vehicle space
        velocityVehicleSpace.Project(partRightDirectionVehicleSpace);
        // Get the default forward direction of the part in vehicle space
        Vector3 partDefaultForwardDirectionVehicleSpace = 
            part.PartConfig.localForwardDirection.RecontextualizeDirection(part.transform, vehicle.transform).normalized;
        // Get the current forward direction the part in vehicle space
        Vector3 partForwardDirectionVehicleSpace = partDefaultForwardDirectionVehicleSpace.Rotate(angle, partRightDirectionVehicleSpace);
        // Get the angle between the current forward direction of the part in vehicle space and the flattened velocity in vehicle space.
        aoa = Vector3.SignedAngle(partForwardDirectionVehicleSpace, velocityVehicleSpace, partRightDirectionVehicleSpace);

        // This does the same thing. Is it faster?
        /*Vector3 partUpDirectionVehicleSpace = Vector3.Cross(partForwardDirectionVehicleSpace, partRightDirectionVehicleSpace);
        float upAmount = Vector3.Dot(velocityVehicleSpace, partUpDirectionVehicleSpace);
        float forwardAmount = Vector3.Dot(velocityVehicleSpace, partForwardDirectionVehicleSpace);
        float aoa2 = Mathf.Atan2(-upAmount, forwardAmount) * Mathf.Rad2Deg;
        Debug.Log(aoa/aoa2);*/

        return aoa;
    }

    /// <summary>
    /// Returns the lift vector given the vehicles velocity. 
    /// Not normalized, world space.
    /// </summary>
    public Vector3 GetLiftVector(VehiclePart part, float angle, Vector3 worldVelocity, float? forcedAoA = null)
    {
        AerodynamicsData aeroData = part.PartConfig.aerodynamicsData;
        Vector3 newLiftVector = new Vector3();
        float aoa = forcedAoA ?? GetAoA(part, angle, worldVelocity);
        float Cl = GetLiftCoefficient(aeroData.liftCurve, aoa);
        float lift = GetLiftMagnitude(Cl, part.Vehicle.AirDensity, worldVelocity.sqrMagnitude, aeroData.area) * aeroData.liftMultiplier;
        Vector3 liftDirection = Vector3.Cross(worldVelocity, part.transform.TransformDirection(part.PartConfig.localRightDirection)).normalized;
        newLiftVector = lift * liftDirection;
        #region Debug
        if(forcedAoA == null) {
            _aoa = aoa;
            _liftVector = newLiftVector;
            _liftDirection = Vector3.Cross(worldVelocity, part.transform.TransformDirection(part.PartConfig.localRightDirection)).normalized;
            _worldVelocityDirection = worldVelocity.normalized;
        }
        #endregion

        return newLiftVector;
    }

    /// <summary>
    /// Get lift coefficient from lift curve based on angle of attack.
    /// </summary>
    private float GetLiftCoefficient(AnimationCurve liftCurve, float angleOfAttack)
    {
        // Divide angle of attack by 1000 to fit curve scaling.
        return liftCurve.Evaluate(angleOfAttack / 1000f);
    }

    /// <summary>
    /// Get lift magnitude based on the lift equation. 
    /// L = 0.5 * Cl * ρ * TAS^2 * A
    /// </summary>
    private float GetLiftMagnitude(float cl, float rho, float TAS2, float area)
    {
        return 0.5f * cl * rho * TAS2 * area;
    }

    /// <summary>
    /// Returns the drag vector given the vehicles velocity. 
    /// Not normalized, world space.
    /// </summary>
    public Vector3 GetDragVector(VehiclePart part, float angle, Vector3 worldVelocity, float? forcedAoA = null)
    {
        AerodynamicsData aeroData = part.PartConfig.aerodynamicsData;
        Vector3 newDragVector = new Vector3();
        float aoa = forcedAoA ?? GetAoA(part, angle, worldVelocity);
        float Cd = GetDragCoefficient(aeroData.dragCurve, aoa);
        float drag = GetDragMagnitude(Cd, part.Vehicle.AirDensity, worldVelocity.sqrMagnitude, aeroData.area) * aeroData.dragMultiplier;
        newDragVector = drag * -worldVelocity.normalized;

        #region Debug
        if(forcedAoA == null) {
            _aoa = aoa;
            _dragVector = newDragVector;
        }
        #endregion

        return newDragVector;
    }

    /// <summary>
    /// Get drag coefficient from drag curve based on angle of attack.
    /// </summary>
    private float GetDragCoefficient(AnimationCurve dragCurve, float angleOfAttack)
    {
        return dragCurve.Evaluate(angleOfAttack / 1000f);
    }

    /// <summary>
    /// Get drag magnitude based on the drag equation. 
    /// D = 0.5 * Cd * ρ * TAS^2 * A
    /// </summary>
    private float GetDragMagnitude(float cd, float rho, float TAS2, float area)
    {
        return 0.5f * cd * rho * TAS2 * area;
    }

    /// <summary>
    /// Get the aerodynamic center adjusted for mirror et cetera.
    /// </summary>
    public Vector3 GetAdjustedLocalAerodynamicCenter(VehiclePart part)
    {
        AerodynamicsData aeroData = part.PartConfig.aerodynamicsData;
        return Vector3.Scale(aeroData.localAerodynamicCenter, part.MirrorVector);
    }

    /// <summary>
    /// Get the aerodynamic center in world space adjusted for mirror et cetera.
    /// </summary>
    public Vector3 GetAdjustedWorldAerodynamicCenter(VehiclePart part)
    {
        return part.transform.position + part.transform.TransformDirection(GetAdjustedLocalAerodynamicCenter(part));
    }

    public void DrawPartAeorynamicForces(VehiclePart part)
    {
        if (!(part is IAerodynamicPart))
        {
            throw new System.Exception("Part must implement IAerodynamicPart");
        }
        Vehicle vehicle = part.Vehicle;
        Vector3 aerodynamicCenter = GetAdjustedWorldAerodynamicCenter(part);
        float forceDisplayScale = 0.5f / vehicle.Rb.mass;
        Vector3 liftVector = LiftVector;
        Vector3 dragVector = DragVector;
        Vector3 tangentialVelocityVector = WorldTangentualVelocity;
        if (vehicle.Debug.DrawLift)
            DrawLift(part.GetInstanceID().ToString(), aerodynamicCenter, liftVector * forceDisplayScale, Color.blue, 0.2f);
        if (vehicle.Debug.DrawDrag)
            DrawDrag(part.GetInstanceID().ToString(), aerodynamicCenter, dragVector * forceDisplayScale, Color.red, 0.2f);
        if (vehicle.Debug.DrawTangentualVelocity)
            DrawTangentualVelocity(part.GetInstanceID().ToString(), aerodynamicCenter, tangentialVelocityVector, Color.magenta, 0.2f);
    }

    public void DrawDrag(string instanceID, Vector3 aerodynamicCenter, Vector3 vector, Color color, float width)
    {
        DebugManager.instance.UpdateVisualVector("D_" + instanceID, aerodynamicCenter, vector, color, width);
    }

    public void DrawLift(string instanceID, Vector3 aerodynamicCenter, Vector3 vector, Color color, float width)
    {
        DebugManager.instance.UpdateVisualVector("L_" + instanceID, aerodynamicCenter, vector, color, width);
    }

    public void DrawTangentualVelocity(string instanceID, Vector3 origin, Vector3 vector, Color color, float width)
    {
        DebugManager.instance.UpdateVisualVector("A_" + instanceID, origin, vector, color, width);
    }
}
