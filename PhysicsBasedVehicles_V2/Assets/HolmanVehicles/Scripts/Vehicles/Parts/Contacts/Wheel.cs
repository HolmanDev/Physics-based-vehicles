using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Make aerodynamic interface?
public class Wheel : VehiclePart
{
    public WheelConfig WheelConfig { get { return PartConfig as WheelConfig; } set { PartConfig = value; } } // The set makes no sense. Shouldn't it set PartConfig?

    [Header("Wheel")]

    public WheelCollider WheelCollider; // perhaps protected or internal?
    public Transform VisualWheel;
    private Vector3 _visualWheelDefaultVehiclePosition;

    // These should be public because they can change during runtime
    public float MotorTorque = 5000f;
    public float BrakeTorque = 10000f;
    public float MaxSteerAngle = 10f;
    public bool SteerEnabled = false;
    public bool DriveEnabled = false;
    public bool BrakeEnabled = false;

    public ProceduralColor ProceduralColor = new ProceduralColor();
    private Vector3 defaultDirection;
    private Vector3 upDirection;
    public LayerMask groundMask;
    private float steerAngle = 0;

    private void OnValidate()
    {
        ProceduralColor.Repaint(ProceduralColor.Color, ProceduralColor.Texture);
        //WheelCollider.radius = WheelConfig.radius;
    }

    private void Start()
    {
        defaultDirection = WheelConfig.localForwardDirection.RecontextualizeDirection(transform, Vehicle.transform);
        _visualWheelDefaultVehiclePosition = Vehicle.transform.InverseTransformPoint(VisualWheel.position);
        upDirection = Vector3.Cross(defaultDirection, WheelConfig.localRightDirection.RecontextualizeDirection(transform, Vehicle.transform));
        WheelCollider.ConfigureVehicleSubsteps(5, 12, 15);
    }

    private void Update()
    {
        
    }

    private void FixedUpdate()
    {
        UpdateVisualWheels();
        Handle();
    }

    public Vector3 GetUpDirection()
    {
        defaultDirection = WheelConfig.localForwardDirection.RecontextualizeDirection(transform, Vehicle.transform);
        upDirection = Vector3.Cross(defaultDirection, WheelConfig.localRightDirection.RecontextualizeDirection(transform, Vehicle.transform)); // normalize?
        return upDirection;
    }

    public void UpdateVisualWheels()
    {
        WheelCollider.GetWorldPose(out Vector3 pos, out Quaternion quat);
        float offset = (Vehicle.transform.InverseTransformPoint(pos) - transform.localPosition).magnitude;
        VisualWheel.position = Vehicle.transform.TransformPoint(_visualWheelDefaultVehiclePosition + upDirection * offset);
        Vector3 worldUpDirection = Vehicle.transform.TransformDirection(upDirection);
        Vector3 worldDefaultDirection = Vehicle.transform.TransformDirection(defaultDirection);
        Quaternion desiredRotation = Quaternion.LookRotation(worldDefaultDirection.Rotate(steerAngle, worldUpDirection), worldUpDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, 5f * Time.deltaTime);
    }

    public void Handle()
    {
        if (DriveEnabled) {
            float motorTorque = 0;

            if (Input.GetKey(KeyCode.UpArrow))
            {
                motorTorque += MotorTorque;
            }

            if (Input.GetKey(KeyCode.DownArrow))
            {
                motorTorque += -MotorTorque;
            }

            WheelCollider.motorTorque = motorTorque;
        }

        if (SteerEnabled)
        {
            steerAngle = 0;

            if (Input.GetKey(KeyCode.LeftArrow))
            {
                steerAngle += -MaxSteerAngle;
            }

            if (Input.GetKey(KeyCode.RightArrow))
            {
                steerAngle += MaxSteerAngle;
            }

            WheelCollider.steerAngle = steerAngle;
        }

        if (BrakeEnabled)
        {
            float brakeTorque = 0;

            if (Input.GetKey(KeyCode.B))
            {
                brakeTorque = BrakeTorque;
            }

            WheelCollider.brakeTorque = brakeTorque;
        }
    }

    public static void ImplementConfigValues(ref WheelCollider wheelCollider, WheelConfig wheelConfig)
    {
        wheelCollider.radius = wheelConfig.radius;
        wheelCollider.wheelDampingRate = wheelConfig.wheelDampingRate;
        wheelCollider.suspensionDistance = wheelConfig.suspensionDistance;
        wheelCollider.center = wheelConfig.center;

        JointSpring suspensionSpring = new JointSpring();
        suspensionSpring.spring = wheelConfig.spring;
        suspensionSpring.damper = wheelConfig.damper;
        suspensionSpring.targetPosition = 0;
        wheelCollider.suspensionSpring = suspensionSpring;

        WheelFrictionCurve forwardFriction = new WheelFrictionCurve();
        forwardFriction.extremumSlip = 0.4f;
        forwardFriction.extremumValue = 1.0f;
        forwardFriction.asymptoteSlip = 0.8f;
        forwardFriction.asymptoteValue = 0.5f;
        forwardFriction.stiffness = wheelConfig.stiffness;
        wheelCollider.forwardFriction = forwardFriction;

        WheelFrictionCurve sidewaysFriction = new WheelFrictionCurve();
        sidewaysFriction.extremumSlip = 0.2f;
        sidewaysFriction.extremumValue = 1.0f;
        sidewaysFriction.asymptoteSlip = 0.5f;
        sidewaysFriction.asymptoteValue = 0.75f;
        sidewaysFriction.stiffness = wheelConfig.stiffness;
        wheelCollider.sidewaysFriction = sidewaysFriction;
    }

    public override float GetWeight()
    {
        return WheelConfig.mass;
    }

    public override void DestroyConnections()
    {
        Destroy(WheelCollider.gameObject);
    }
}
