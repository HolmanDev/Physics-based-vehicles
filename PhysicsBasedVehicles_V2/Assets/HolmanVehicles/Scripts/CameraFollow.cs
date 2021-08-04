using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Vehicle target;
    public float distance;

    private void LateUpdate()
    {
        transform.position = target.transform.position + target.CenterOfMass + new Vector3(0, 0, -1) * distance;
        transform.LookAt(target.transform.position + target.CenterOfMass);
    }
}
