using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SurfaceControls
{
    [HideInInspector] public ControlSurface Part;

    [SerializeField] private bool partSASEnabled = true;
    [SerializeField] private PID PID = default;
    [SerializeField] private KeyCode[] _keys = default;
    [SerializeField, Tooltip("Degrees")] private float _rotationLimit = 16f; // Degrees
    [SerializeField] private float _rotationSpeed = 16f; // Degrees / s
    [SerializeField] private Vector3 _defaultLocalRotation = Vector3.zero; // Euler
    [SerializeField] private float _damping = 1.0031f;
    [SerializeField] private float _rotationSpeedDamping = 1.0031f;
    [SerializeField] private Axis _impactAxis = Axis.x;
    public float Angle { get; private set; } // Degrees

    /// <summary>
    /// Get the input from the keys. 0 = not pressed, 1 = pressed.
    /// </summary>
    public int[] GetMoveInput()
    {
        int[] isPressed = new int[_keys.Length];

        for (int i = 0; i < _keys.Length; i++)
        {
            if (Input.GetKey(_keys[i]))
            {
                isPressed[i] = 1;
            }
        }

        return isPressed;
    }

    /// <summary>
    /// Try to move the part, either manually or with SAS.
    /// </summary>
    public void TryMove(int[] keysPressed, bool useSAS, float SASStrength)
    {
        useSAS &= partSASEnabled;
        UpdateAngleManual(keysPressed);

        if (keysPressed.GetNonZeroItems().Length == 0)
        {
            if (useSAS)
            {
                UpdateAngleSAS(SASStrength);
            }
            else
            {
                float revertSpeed = 10f;
                Angle = Mathf.Lerp(Angle, 0, revertSpeed * Time.deltaTime);
            }
        }

        UpdateVisualAngle(Angle);
    }

    /// <summary>
    /// Update the angle based on user input.
    /// </summary>
    private void UpdateAngleManual(int[] keysPressed)
    {
        Vector3 worldAC = Part.Aerodynamics.GetAdjustedWorldAerodynamicCenter(Part);
        Vector3 velocity = Part.Vehicle.Rb.GetPointVelocity(worldAC);

        float speedLimitDamping = 0.05f + 0.95f * Mathf.Pow(_damping, -velocity.magnitude); // !! Improve this
        float rotationSpeedDamping = 0.05f + 0.95f * Mathf.Pow(_rotationSpeedDamping, -velocity.magnitude); // !! Improve this

        if (keysPressed[0] == 1)
        {
            Angle += _rotationSpeed * Time.deltaTime * rotationSpeedDamping;
        }
        if (keysPressed[1] == 1)
        {
            Angle -= _rotationSpeed * Time.deltaTime * rotationSpeedDamping;
        }

        Angle = Mathf.Clamp(Angle, -_rotationLimit * speedLimitDamping, _rotationLimit * speedLimitDamping);

        #region Debug
        if (Part.Vehicle.Debug.DebugEnabled)
        {
            DebugManager.instance.SetOverlayItem(new OverlayItem("speedDamping", speedLimitDamping, ": ", " m/s"), 1);
        }
        #endregion
    }

    /// <summary>
    /// Update the angle based on a PID controller.
    /// </summary>
    private void UpdateAngleSAS(float strength)
    {
        Vector3 worldAC = Part.Aerodynamics.GetAdjustedWorldAerodynamicCenter(Part); //transform.position + transform.TransformDirection(MoveableStructuralConfig.aerodynamics.LocalAerodynamicCenter);
        Vector3 velocity = Part.Vehicle.Rb.GetPointVelocity(worldAC); // hmm
        float speedLimitDamping = 0.05f + 0.95f * Mathf.Pow(_damping, -velocity.magnitude);
        float rotationSpeedDamping = 0.05f + 0.95f * Mathf.Pow(_rotationSpeedDamping, -velocity.magnitude); // !! Improve this

        // Calculate force vector
        float maxAngle = _rotationLimit * speedLimitDamping;
        float forcedAoA = Part.MoveableStructuralConfig.aerodynamicsData.liftCurve.GetExtremeX(-maxAngle / 1000f, maxAngle / 1000f, 1 / 1000f) * 1000f;
        float sign = Mathf.Sign(Part.MoveableStructuralConfig.aerodynamicsData.liftCurve.GetExtremeY(-maxAngle / 1000f, maxAngle / 1000f, 1 / 1000f));
        Vector3 lift = Part.Aerodynamics.GetLiftVector(Part, 0, velocity, forcedAoA);
        Vector3 drag = Part.Aerodynamics.GetDragVector(Part, 0, velocity, forcedAoA);
        Vector3 aerodynamicForce = lift + drag;

        // τ = r × F
        Vector3 relativePosition = worldAC - Part.Vehicle.Rb.worldCenterOfMass;
        //Vector3 predictedTorque = Part.Vehicle.transform.InverseTransformDirection(
            //Vector3.Cross(worldAC - Part.Vehicle.transform.position - Part.Vehicle.Rb.centerOfMass, aerodynamicForce)); // partspace*/
        Vector3 predictedTorque = Part.Vehicle.transform.InverseTransformDirection(Vector3.Cross(relativePosition, aerodynamicForce)); // partspace
        Vector3 currentVehicleAngularVelocity = Part.Vehicle.transform.InverseTransformDirection(Part.Vehicle.Rb.angularVelocity); // pitch, yaw, roll
        float currentValue = currentVehicleAngularVelocity[(int)_impactAxis];

        // See to which extent the simulated torque is in impactAxis. Determines the sign of the error.
        float torqueAlignmentCof = Vector3.Dot(predictedTorque.normalized, _impactAxis.GetVector3()) * 20f;

        // Calculate error
        float error = currentValue * torqueAlignmentCof;
        float cof = Mathf.Clamp(PID.Update(error, Time.deltaTime), -1, 1);

        // Correct for error
        Angle += sign * _rotationSpeed * Time.deltaTime * cof * strength * rotationSpeedDamping;
        //if(Mathf.Abs(error) < 0.1f) _angle = 0; // Uncomment this line if you want, but it can cause some jumping
        Angle = Mathf.Clamp(Angle, -maxAngle, maxAngle);

        #region Debug
        if (Part.Vehicle.Debug.DebugEnabled)
            DebugManager.instance.SetOverlayItem(new OverlayItem("speedDamping", speedLimitDamping, ": ", " m/s"), 1);
        #endregion
    }

    /// <summary>
    /// Update the model so that it ligns up with the calculated angle.
    /// </summary>
    private void UpdateVisualAngle(float angle)
    {
        Part.transform.localRotation = Quaternion.Euler(_defaultLocalRotation) * Quaternion.AngleAxis(angle, Part.PartConfig.localRightDirection);
    }

    public void SetDefaultLocalRotation(Vector3 eulerAngles)
    {
        _defaultLocalRotation = eulerAngles;
    }
}
