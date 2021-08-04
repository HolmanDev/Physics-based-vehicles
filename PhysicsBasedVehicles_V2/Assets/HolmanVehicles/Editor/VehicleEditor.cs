using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Collections;

[CustomEditor(typeof(Vehicle))]
public class VehicleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Vehicle vehicle = (Vehicle)target;

        if (GUILayout.Button("Find parts"))
        {
            vehicle.FindParts();
        }

        if (GUILayout.Button("Calculate mass"))
        {
            vehicle.Rb = vehicle.GetComponent<Rigidbody>();
            vehicle.CalculateMass();
            vehicle.UpdateInertiaTensor();
        }
    }
}
