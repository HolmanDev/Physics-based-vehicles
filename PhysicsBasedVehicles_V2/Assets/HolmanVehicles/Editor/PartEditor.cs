using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;

[CustomEditor(typeof(VehiclePart), true), CanEditMultipleObjects]
public class PartEditor : Editor
{
    private readonly Dictionary<Type, Type> _partConfigDict = new Dictionary<Type, Type>()
    {
        { typeof(Structure), typeof(StructuralConfig) },
        { typeof(ControlSurface), typeof(MoveableStructuralConfig) },
        { typeof(Engine), typeof(EngineConfig) },
        { typeof(Wheel), typeof(WheelConfig) }
    };

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        VehiclePart part = (VehiclePart)target;
        VehiclePartConfig config = part.PartConfig;
        Type desiredConfig = _partConfigDict[part.GetType()];

        if (config == null || config.GetType() != desiredConfig)
        {
            EditorGUILayout.HelpBox("Wrong config type! Use " + desiredConfig, MessageType.Error);
        }

        if(GUILayout.Button("Mirror (" + part.Mirrored + ")"))
        {
            part.Mirrored = !part.Mirrored;
            EditorUtility.SetDirty(part);
            SceneView.RepaintAll(); // !! GOD COMMAND
        }

        if (part is Wheel)
        {
            Wheel wheel = part as Wheel;
            if (GUILayout.Button("Generate Wheel Collider"))
            {
                WheelCollider wheelCollider = wheel.WheelCollider;
                if (wheelCollider == null)
                {
                    GameObject wheelColliderObject = new GameObject();
                    wheelColliderObject.transform.SetParent(wheel.Vehicle.transform);
                    wheelColliderObject.transform.localPosition = Vector3.zero;
                    wheelColliderObject.name = "[" + wheel.name + " collider]";
                    wheelCollider = wheelColliderObject.AddComponent<WheelCollider>();
                }
                Wheel.ImplementConfigValues(ref wheelCollider, wheel.WheelConfig);
                Vector3 offset = wheel.GetUpDirection() * wheel.WheelConfig.suspensionDistance * 0.5f;
                wheelCollider.transform.position = wheel.transform.position + wheel.Vehicle.transform.TransformDirection(offset);
                wheel.WheelCollider = wheelCollider;
                EditorUtility.SetDirty(wheel);
            }
        }

        if(part is ControlSurface)
        {
            ControlSurface moveableStructure = part as ControlSurface;
            if(GUILayout.Button("Save default rotation"))
            {
                moveableStructure.SurfaceControls.SetDefaultLocalRotation(moveableStructure.transform.localEulerAngles);
                EditorUtility.SetDirty(part);
            }
        }

        /*if (part is AerodynamicPart)
        {
            if((part as AerodynamicPart).Aerodynamics == null)
            {
                EditorGUILayout.HelpBox("No aerodynamic data assigned! Click button below.", MessageType.Error);
            }
            if (GUILayout.Button("Input aerodynamic data"))
            {
                if (config != null)
                {
                    (part as AerodynamicPart).Aerodynamics = new Aerodynamics(config.aerodynamicsData);
                }
            }
        }*/
    }
}
