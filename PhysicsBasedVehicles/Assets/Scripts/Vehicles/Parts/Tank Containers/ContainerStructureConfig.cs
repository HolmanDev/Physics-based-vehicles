using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Container Structure", menuName = "Vehicles/Part Configs/Container Structure")]
public class ContainerStructureConfig : ScriptableObject
{
    public float baseMass;
    public float materialDensity;

    public float _maxWidth;
    public float _maxHeight;

    public GameObject _top;
    public GameObject _core;
    public GameObject _lower;
}
