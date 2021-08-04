using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Engine : VehiclePart
{
    public EngineConfig EngineConfig { get { return PartConfig as EngineConfig; } set { EngineConfig = value; } }

    [Header("Engine")]
    
    //[SerializeField] private EngineConfig _engineConfig = default;
    //[HideInInspector] public EngineConfig EngineConfig => _engineConfig;
    [HideInInspector] public BaseEngineSound EngineSound;
    [HideInInspector] public BaseEngineEffect EngineEffect;

    [HideInInspector] public bool _burning;
    [HideInInspector] public float Throttle = 0;
    public bool _gimbalEnabled = true;
    public Transform _gimbalPivot;
    [HideInInspector] public Vector3 _defaultGimbalRotation;
    [HideInInspector] public Vector3 _defaultEngineRotation;
    [HideInInspector] public Quaternion _thrustDirection;

    public override void Initialize(Vehicle vehicle)
    {
        Vehicle = vehicle;
        EngineSound = GetComponent<BaseEngineSound>();
        EngineSound.Engine = this;
        EngineSound.AudioSource = GetComponent<AudioSource>();
        EngineEffect = GetComponent<BaseEngineEffect>();
        EngineEffect.Engine = this;

        // Default rotations for gimbal
        _defaultGimbalRotation = _gimbalPivot.localRotation.eulerAngles;
        _defaultEngineRotation = transform.localRotation.eulerAngles;
    }

    private void FixedUpdate() {
        EngineEffect.GroundEffectRaycast();

        if (_burning)
        {
            Burn();
            AddForces();
        }
    }

    /// <summary>
    /// Get the non-normalized local thrust vector of a single engine.
    /// </summary>
    public Vector3 GetEngineThrustVector()
    {
        return GetEngineThrustDirection() * EngineConfig.thrustSL * Throttle;
    }

    /// <summary>
    /// Get a normalized local direction of an engine's thrust. 
    /// Depends on the thrust direction specified in the config.
    /// </summary>
    private Vector3 GetEngineThrustDirection()
    {
        Debug.DrawRay(transform.position, (-transform.TransformDirection(Matrix4x4.Rotate(_thrustDirection).MultiplyVector(EngineConfig.localThrustDirection))).normalized * 3, Color.green, Time.deltaTime, false);
        return Matrix4x4.Rotate(_thrustDirection).MultiplyVector(EngineConfig.localThrustDirection);
    }

    /// <summary>
    /// Executes the functionality associated with burning. Should in most cases be updated frequently.
    /// </summary>
    public virtual void Burn()
    {
        // Move to function within EngineEffect?
        //EngineEffect._exhaust.startLifetime = Throttle * EngineEffect.ExhaustLifetime;
        for (int i = 0; i < EngineEffect.ExhaustOpacities.Length; i++)
        {
            Color color = EngineEffect._exhausts[i].startColor.color;
            color.a = Throttle * Throttle * Throttle * Throttle * EngineEffect.ExhaustOpacities[i];
            EngineEffect._exhausts[i].startColor = color;
        }
        EngineSound.AudioSource.volume = EngineSound.MaxVolume * Throttle;
    }

    public virtual void AddForces() {
        Vector3 worldCOM = GetAdjustedWorldCenterOfMass(); //transform.position + transform.TransformDirection(centerOfMass);
        //Vector3 vehicleLocalCOM = Vehicle.transform.InverseTransformPoint(worldCOM).normalized * centerOfMass.magnitude;

        Vehicle.Rb.AddForceAtPosition(-transform.TransformDirection(GetEngineThrustVector()), worldCOM);
    }

    /// <summary>
    /// Ignite the engine.
    /// </summary>
    public virtual void Ignite()
    {
        _burning = true;
        
        EngineEffect.CreatePlume();

        StartCoroutine(EngineSound.PlayIgnitionSound());
    }

    /// <summary>
    /// Shut down the engine.
    /// </summary>
    public virtual void ShutDown()
    {
        _burning = false;
        Throttle = 0;

        EngineEffect.RemovePlume();

        StartCoroutine(EngineSound.PlayShutdownSound());
    }

    /// <summary>
    /// Get raw gimbal strength in multiple directions based on user input on current frame. 
    /// There is no standard implementation of gimbal.
    /// Return format: (D, A, W, S)
    /// </summary>
    public virtual Vector4 GetGimbalInput()
    {
        throw new System.Exception("This engine doesn't gimbal.");
    }

    /// <summary>
    /// Execute gimbal maneuver if possible.
    /// There is no standard implementation of gimbal.
    /// </summary>
    public virtual void TryGimbal(Vector4 gimbal)
    {
        throw new System.Exception("This engine doesn't gimbal.");
    }

    /// <summary>
    /// Rotate engine around gimbal pivot.
    /// There is no standard implementation of gimbal.
    /// </summary>
    public virtual void GimbalRotation(Vector3 localGimbalDirection)
    {
        throw new System.Exception("This engine doesn't gimbal.");
    }

    /// <summary>
    /// Align thrust direction with gimbal direction.
    /// There is no standard implementation of gimbal.
    /// </summary>
    public virtual void GimbalThrustDirection(Vector3 localGimbalDirection)
    {
        throw new System.Exception("This engine doesn't gimbal.");
    }

    /// <summary>
    /// Throttle up the engine, i.e increase the thrust.
    /// </summary>
    public void ThrottleUp()
    {
        if (_burning)
        {
            Throttle = Mathf.Clamp(Throttle + 0.5f * Time.deltaTime, 0, 1);
        }
    }

    /// <summary>
    /// Throttle down the engine, i.e decrease the thrust.
    /// </summary>
    public void ThrottleDown()
    {
        if (_burning)
        {
            Throttle = Mathf.Clamp(Throttle - 0.5f * Time.deltaTime, 0, 1);
        }
    }

    public override float GetWeight() {
        return PartConfig.mass;
    }
}