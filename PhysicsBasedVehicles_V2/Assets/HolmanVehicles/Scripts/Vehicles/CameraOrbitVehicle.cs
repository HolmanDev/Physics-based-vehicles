using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOrbitVehicle : MonoBehaviour
{
    public Transform target;
    public float distance;
    public float sensativity = 1f;
    public float zoomSensativity = 1f;
    // Keep the local position relative to the vehicle. But only relative to the nose against the horizon?
    private Quaternion offset;
    float rotX = 0;
    float rotY = 0;

    private void Awake()
    {
        offset = Quaternion.LookRotation(Vector3.back);
        rotX = offset.eulerAngles.x;
        rotY = offset.eulerAngles.y;
    }

    private void Update()
    {
        if (target != null)
        {
            if (Input.GetKey(KeyCode.Mouse1))
            {
                rotY += Input.GetAxis("Mouse X") * sensativity;
                rotX += Input.GetAxis("Mouse Y") * sensativity;
                rotX = Mathf.Clamp(rotX, -89f, 89f);
                offset = Quaternion.Euler(rotX, rotY, 0f);
            }

            float deltaZoom = Input.mouseScrollDelta.y;
            distance -= zoomSensativity * deltaZoom;

            transform.position = target.position + offset * Vector3.forward * distance;
            transform.LookAt(target);
        }
    }
}
