using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DefaultTankContainerStructure))]
public class FuelTankEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        DefaultTankContainerStructure fuelTank = (DefaultTankContainerStructure)target;

        if (GUILayout.Button("Update Tank"))
        {
            fuelTank.UpdateTank();
        }
    }
}
