using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Structure", menuName = "Vehicles/Part Configs/Structure")]
public class StructuralConfig : ScriptableObject
{
    [Tooltip("Kg")]
    public float weight;
}
