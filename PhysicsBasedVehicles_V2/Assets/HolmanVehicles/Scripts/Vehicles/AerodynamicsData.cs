using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AerodynamicsData
{
    [SerializeField] public float liftMultiplier = 1;
    [SerializeField] public float dragMultiplier = 1;
    [SerializeField, Tooltip("m^2")] public float area = 1; // m^2
    [SerializeField] public Vector3 localAerodynamicCenter = Vector3.zero; // Will be calculated in the future
    [Tooltip("Cl over aoa"), SerializeField] public AnimationCurve liftCurve = default;
    [Tooltip("Cd over aoa"), SerializeField] public AnimationCurve dragCurve = default;
}
