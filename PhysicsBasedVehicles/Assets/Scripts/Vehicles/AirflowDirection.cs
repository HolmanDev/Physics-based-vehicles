using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Axis {x = 0, y = 1, z = 2};

// Remove
[System.Serializable]
public class AirflowDirection
{
    [HideInInspector] public Vector3 direction; // Not implemented yet and might not ever be.

    public AnimationCurve LiftCurve;
    public AnimationCurve DragCurve;
}
