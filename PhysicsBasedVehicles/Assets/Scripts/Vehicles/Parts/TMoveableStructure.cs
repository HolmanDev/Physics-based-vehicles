using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEditor;

// Perhaps combine with the regular structural parts? Rename to control surface?
public abstract class TMoveableStructure : VehiclePart
{
    [Header("Moveable Structure")]
    
    public MoveableStructuralConfig MoveableStructuralConfig;
    public ProceduralColor ProceduralColor = new ProceduralColor();
    public Aerodynamics Aerodynamics = new Aerodynamics();

    // Movement
    [SerializeField] private PID PID = default;
    [SerializeField] private KeyCode[] _keys = default;
    [SerializeField, Tooltip("Degrees")] private float _rotationLimit = 16f; // Degrees
    [SerializeField] private float _rotationSpeed = 16f; // Degrees / s
    [SerializeField] private Vector3 _defaultLocalRotation = Vector3.zero; // Euler
    [SerializeField] private float _damping = 1.0031f;
    [SerializeField] private Axis _impactAxis = Axis.x;
    private float _angle; // Degrees

    void Awake() {
        Type = VehiclePartType.MoveableStructural;
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position + transform.TransformDirection(centerOfMass), 0.1f);
        Gizmos.color = Color.blue;
        Vector3 worldAC = transform.position + transform.TransformDirection(Aerodynamics.LocalAerodynamicCenter);
        Gizmos.DrawSphere(worldAC, 0.1f);
        Gizmos.color = Color.white;
        Handles.Label(transform.position, Mathf.Round(Aerodynamics.AoA).ToString() + "°");
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

    private void OnValidate()
    {
        ProceduralColor.Repaint(ProceduralColor.Color, ProceduralColor.Texture);
    }

    private void FixedUpdate() {
        Aerodynamics.ExecuteAerodynamicForces(this, _angle);
    }

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
    public void TryMove(int[] keysPressed, bool SAS, float SASStrength)
    {
        UpdateAngleManual(keysPressed);
        
        if(keysPressed.GetNonZeroItems().Length == 0) {
            if (SAS)
            {
                UpdateAngleSAS(SASStrength);
            } else {
                float revertSpeed = 10f;
                _angle = Mathf.Lerp(_angle, 0, revertSpeed * Time.deltaTime);
            }
        }

        UpdateVisualAngle(_angle);
    }

    /// <summary>
    /// Update the angle based on user input.
    /// </summary>
    private void UpdateAngleManual(int[] keysPressed)
    {
        Vector3 worldAC = transform.position + transform.TransformDirection(centerOfMass);
        Vector3 velocity = Vehicle.Rb.GetPointVelocity(worldAC);

        float speedDamping = 0.05f + 0.95f * Mathf.Pow(_damping, -velocity.magnitude);

        if (keysPressed[0] == 1)
        {
            _angle += _rotationSpeed * Time.deltaTime;
        }
        if (keysPressed[1] == 1)
        {
            _angle -= _rotationSpeed * Time.deltaTime;
        }

        _angle = Mathf.Clamp(_angle, -_rotationLimit * speedDamping, _rotationLimit * speedDamping);

        #region Debug
        if(Vehicle.Debug.DebugEnabled) 
        {
            DebugManager.instance.SetOverlayItem(new OverlayItem("speedDamping", speedDamping, ": ", " m/s"), 1);
        }
        #endregion
    }

    /// <summary>
    /// Update the angle based on a PID controller.
    /// </summary>
    private void UpdateAngleSAS(float strength)
    {
        Vector3 worldAC = transform.position + transform.TransformDirection(Aerodynamics.LocalAerodynamicCenter);
        Vector3 velocity = Vehicle.Rb.GetPointVelocity(worldAC);
        float speedDamping = 0.05f + 0.95f * Mathf.Pow(_damping, -velocity.magnitude);

        // Calculate force vector
        float maxAngle = _rotationLimit * speedDamping;
        float forcedAoA = Aerodynamics.LiftCurve.GetExtremeX(-maxAngle / 1000f, maxAngle / 1000f, 1 / 1000f) * 1000f;
        float sign = Mathf.Sign(Aerodynamics.LiftCurve.GetExtremeY(-maxAngle / 1000f, maxAngle / 1000f, 1 / 1000f));
        Vector3 lift = Aerodynamics.GetLiftVector(this, 0, velocity, forcedAoA);
        Vector3 drag = Aerodynamics.GetDragVector(this, 0, velocity, forcedAoA);
        Vector3 aerodynamicForce = lift + drag;

        // τ = r × F
        Vector3 predictedTorque = Vehicle.transform.InverseTransformDirection(
            Vector3.Cross(worldAC - Vehicle.transform.position - Vehicle.Rb.centerOfMass, aerodynamicForce));
        Vector3 currentVehicleAngularVelocity = Vehicle.transform.InverseTransformDirection(Vehicle.Rb.angularVelocity); // pitch, yaw, roll
        float currentValue = currentVehicleAngularVelocity[(int) _impactAxis];

        // See to which extent the simulated torque is in impactAxis
        float torqueAlignmentCof = Vector3.Dot(predictedTorque.normalized, _impactAxis.GetVector3()) * 20f;

        // Calculate error
        float error = currentValue * torqueAlignmentCof;
        float cof = Mathf.Clamp(PID.Update(error, Time.deltaTime), -1, 1);
        
        // Correct for error
        _angle += sign * _rotationSpeed * Time.deltaTime * cof * strength;
        //if(Mathf.Abs(error) < 0.1f) _angle = 0; // Uncomment this line if you want, but it can cause some jumping
        _angle = Mathf.Clamp(_angle, -maxAngle, maxAngle);

        #region Debug
        if(Vehicle.Debug.DebugEnabled)
            DebugManager.instance.SetOverlayItem(new OverlayItem("speedDamping", speedDamping, ": ", " m/s"), 1);
        #endregion
    }

    /// <summary>
    /// Update the model so that it ligns up with the calculated angle.
    /// </summary>
    private void UpdateVisualAngle(float angle)
    {
        transform.localRotation = Quaternion.Euler(_defaultLocalRotation) * Quaternion.AngleAxis(angle, LocalRightDirection);
    }

    public override float GetWeight() {
        return MoveableStructuralConfig.weight;
    }
}