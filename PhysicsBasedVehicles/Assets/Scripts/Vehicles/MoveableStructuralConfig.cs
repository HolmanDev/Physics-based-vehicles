using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Moveable Structure", menuName = "Vehicles/Part Configs/Moveable Structure")]
public class MoveableStructuralConfig : ScriptableObject
{
    [Tooltip("Kg")]
    public float weight;
}
