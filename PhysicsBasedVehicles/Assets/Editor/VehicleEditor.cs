using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Vehicle))]
public class VehicleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Vehicle vehicle = (Vehicle)target;

        if(GUILayout.Button("Calculate Parts")) {
            vehicle.CalculateMass();
        }
    }
}
