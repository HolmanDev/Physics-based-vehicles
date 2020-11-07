using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AxisExtensions
{
    public static Vector3 GetVector3(this Axis axis) {
        if(axis == Axis.x) {
            return new Vector3(1, 0, 0);
        } else if(axis == Axis.y) {
            return new Vector3(0, 1, 0);
        } else {
            return new Vector3(0, 0, 1);
        }
    }
}
