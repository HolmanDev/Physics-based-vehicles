using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Perhaps make more general?
[System.Serializable]
public class Aerodynamics
{
    private float _aoa;
    public float AoA => _aoa;

    // Aero forces
    [SerializeField] private float _liftMultiplier = 1;
    [SerializeField] private float _dragMultiplier = 1;
    [SerializeField, Tooltip("m^2")] private float _area = 1; // m^2
    public float Area => _area;
    [SerializeField] private Vector3 _localAerodynamicCenter = Vector3.zero; // Will be calculated in the future
    public Vector3 LocalAerodynamicCenter => _localAerodynamicCenter;
    [SerializeField] private AnimationCurve _liftCurve = default;
    public AnimationCurve LiftCurve => _liftCurve;
    [SerializeField] private AnimationCurve _dragCurve = default;
    public AnimationCurve DragCurve => _dragCurve;

    #region Debug
    private Vector3 _liftVector = new Vector3();
    public Vector3 LiftVector => _liftVector;
    private Vector3 _dragVector = new Vector3();
    public Vector3 DragVector => _dragVector;
    #endregion

    /// <summary>
    /// Execute all aerodynamic functionalities.
    /// </summary>
    public void ExecuteAerodynamicForces(VehiclePart part, float angle) 
    {
        Vehicle vehicle = part.Vehicle;

        // Thank you to Ivan "gasgiant" Pensionerov for the physics substep code!
        // https://github.com/gasgiant/Aircraft-Physics
        // https://youtu.be/p3jDJ9FtTyM
        ForceAndTorque forceAndTorqueThisFrame = CalculateForces(part, angle, vehicle.Rb.velocity, vehicle.Rb.angularVelocity);
        Vector3 velocityPrediction = PredictVelocity(vehicle, 
            forceAndTorqueThisFrame.Force + vehicle.GetTotalThrust() + Physics.gravity * vehicle.Rb.mass, 0.5f);
        Vector3 angularVelocityPrediction = PredictAngularVelocity(vehicle, forceAndTorqueThisFrame.Torque, 0.5f);
        ForceAndTorque predictedForceAndTorque = CalculateForces(part, angle, velocityPrediction, angularVelocityPrediction);
        ForceAndTorque forceAndTorque = (forceAndTorqueThisFrame + predictedForceAndTorque) * 0.5f;
        
        vehicle.Rb.AddForce(forceAndTorque.Force);
        vehicle.Rb.AddTorque(forceAndTorque.Torque);
    }

    public ForceAndTorque CalculateForces(VehiclePart part, float angle, Vector3 velocity, Vector3 angularVelocity) {
        Vector3 worldAC = part.transform.position + part.transform.TransformDirection(LocalAerodynamicCenter);
        Vector3 relativePosition = worldAC - part.Vehicle.transform.position - part.Vehicle.Rb.centerOfMass;
        
        Vector3 tangentialVelocity = Vector3.Cross(angularVelocity, relativePosition);
        velocity += tangentialVelocity;       

        Vector3 lift = GetLiftVector(part, angle, velocity);
        Vector3 drag = GetDragVector(part, angle, velocity);
        Vector3 aerodynamicForce = lift + drag;
        Vector3 torque = Vector3.Cross(relativePosition, aerodynamicForce); // τ = r × F

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
    /// Returns the angle of attack of the part
    /// </summary>
    public float GetAoA(VehiclePart part, float angle, Vector3 v)
    {
        float aoa;
        Vehicle vehicle = part.Vehicle;
        Vector3 velocityVehicleSpace = vehicle.transform.InverseTransformDirection(v);
        Vector3 partRightDirectionVehicleSpace = 
            part.LocalRightDirection.RecontextualizeDirection(part.transform, vehicle.transform).normalized;
        velocityVehicleSpace.Project(partRightDirectionVehicleSpace); // partRightDirectionVehicleSpace is perpendicular to the plane
        Vector3 partDefaultForwardDirectionVehicleSpace = 
            part.LocalForwardDirection.RecontextualizeDirection(part.transform, vehicle.transform).normalized;
        Vector3 partForwardDirectionVehicleSpace = partDefaultForwardDirectionVehicleSpace.Rotate(angle, partRightDirectionVehicleSpace);
        aoa = Vector3.SignedAngle(partForwardDirectionVehicleSpace, velocityVehicleSpace, partRightDirectionVehicleSpace);

        return aoa;
    }

    /// <summary>
    /// Returns the lift vector given the vehicles velocity. 
    /// Not normalized, world space.
    /// </summary>
    public Vector3 GetLiftVector(VehiclePart part, float angle, Vector3 v, float? forcedAoA = null)
    {
        Vector3 newLiftVector = new Vector3();
        float aoa = forcedAoA ?? GetAoA(part, angle, v);
        float Cl = GetLiftCoefficient(_liftCurve, aoa);
        float lift = GetLiftMagnitude(Cl, part.Vehicle.AirDensity, v.sqrMagnitude, _area) * _liftMultiplier;
        newLiftVector = lift * Vector3.Cross(v, part.transform.TransformDirection(part.LocalRightDirection)).normalized;

        #region Debug
        if(forcedAoA == null) {
            _aoa = aoa;
            _liftVector = newLiftVector;
        }
        #endregion

        return newLiftVector;
    }

    /// <summary>
    /// Get lift coefficient from lift curve based on angle of attack.
    /// </summary>
    private float GetLiftCoefficient(AnimationCurve liftCurve, float angleOfAttack)
    {
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
    public Vector3 GetDragVector(VehiclePart part, float angle, Vector3 v, float? forcedAoA = null)
    {
        Vector3 newDragVector = new Vector3();
        float aoa = forcedAoA ?? GetAoA(part, angle, v);
        float Cd = GetDragCoefficient(_dragCurve, aoa);
        float drag = GetDragMagnitude(Cd, part.Vehicle.AirDensity, v.sqrMagnitude, _area);
        newDragVector = drag * -v.normalized;

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
        return dragCurve.Evaluate(angleOfAttack / 1000f) * _dragMultiplier;
    }

    /// <summary>
    /// Get drag magnitude based on the drag equation. 
    /// D = 0.5 * Cd * ρ * TAS^2 * A
    /// </summary>
    private float GetDragMagnitude(float cd, float rho, float TAS2, float area)
    {
        return 0.5f * cd * rho * TAS2 * area;
    }
}
