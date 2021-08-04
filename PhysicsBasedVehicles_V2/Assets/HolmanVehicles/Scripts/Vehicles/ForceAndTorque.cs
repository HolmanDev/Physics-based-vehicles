using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Thank you to Ivan "gasgiant" Pensionerov for the physics substep code!
// https://github.com/gasgiant/Aircraft-Physics
// https://youtu.be/p3jDJ9FtTyM
public class ForceAndTorque {
    public Vector3 Force;
    public Vector3 Torque;

    public ForceAndTorque(Vector3 force, Vector3 torque) {
        Force = force;
        Torque = torque;
    }

    public static ForceAndTorque operator +(ForceAndTorque a, ForceAndTorque b) => new ForceAndTorque(a.Force + b.Force, a.Torque + b.Torque);

    public static ForceAndTorque operator *(float f, ForceAndTorque a) => new ForceAndTorque(f * a.Force, f * a.Torque);

    public static ForceAndTorque operator *(ForceAndTorque a, float f) => f * a;
}