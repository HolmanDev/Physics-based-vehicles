using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Wheel", menuName = "Vehicles/Part Configs/Wheel")]
public class WheelConfig : VehiclePartConfig
{
    // Everything in the configs are default values
    public float radius;
    public float wheelDampingRate;
    public float suspensionDistance;
    public Vector3 center;
    public float spring;
    public float damper;
    public float stiffness;
}
