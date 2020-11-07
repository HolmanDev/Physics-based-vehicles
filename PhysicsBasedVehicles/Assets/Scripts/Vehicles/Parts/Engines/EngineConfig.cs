using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Engine", menuName = "Vehicles/Part Configs/Engine")]
public class EngineConfig : ScriptableObject
{
    public string codeName;
    public string displayName;
    [Tooltip("N")]
    public float thrustSL;
    [Tooltip("N")]
    public float thrustVac;
    [Tooltip("°")]
    public float gimbalLimit;
    [Tooltip("kg")]
    public float weight;
    public PropellantType propellant;
    public GameObject prop;
    public GameObject prefab;
    public string ignitionSound;
    public string burningSound;
    public string shutdownSound;
    public GameObject plume;
    public Vector3 plumePositionOffset;
    public Vector3 plumeRotationOffset;
    public GameObject _groundDust;
    public Vector3 localThrustDirection;
}
