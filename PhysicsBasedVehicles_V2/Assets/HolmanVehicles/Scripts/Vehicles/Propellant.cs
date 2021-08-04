using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PropellantType {LqdFuel_Oxidizer, JetFuel_Oxidizer};

// Add Config to name
[CreateAssetMenu(fileName = "New Propellant", menuName = "Vehicles/Part Configs/Propellants")]
public class Propellant : ScriptableObject
{
    public string codeName;
    public string displayName;
    [Tooltip("g/ml")]
    public float density;
    [Tooltip("°C")]
    public float meltingPoint;
    [Tooltip("°C")]
    public float boilingPoint; //1234567890 = N/A
}
