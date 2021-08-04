using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowVehicle : MonoBehaviour
{
    public float smoothTime = 20;
    public Vehicle Vehicle;
    public float distance = 10;
    public Vector3 lookOffset = Vector3.zero;
    public Vector3 cameraOffset = Vector3.zero;
    public float height = 0;
    private Vector3 currentDirection;

    void Awake() {
        transform.position = Vehicle.transform.position + Vehicle.transform.TransformDirection(cameraOffset * distance);
        currentDirection = cameraOffset * distance;
    }

    private void FixedUpdate()
    {
        if (Vehicle.GetCommandingPart() != null)
        {
            FollowTarget(Vehicle.transform, Time.deltaTime);
        }
    }

    /// <summary>
    /// Follow the target.
    /// </summary>
    private void FollowTarget(Transform target, float timeframe)
    {
        Vector3 desiredDirection = target.TransformDirection(cameraOffset);
        Vector3 newDirection = Vector3.Slerp(currentDirection.normalized, desiredDirection.normalized, smoothTime);
        newDirection -= Vector3.Dot(newDirection, target.right) * target.right;
        transform.position = target.position + currentDirection.normalized * distance + target.up * height;
        transform.LookAt(target.position + target.TransformDirection(lookOffset), Vector3.Cross(target.position + lookOffset - transform.position, target.right));
        currentDirection = newDirection;
    }

    /// <summary>
    /// Tries to shake the camera. Output varies based on current and previous inputs. (Needs fine tuning)
    /// </summary>
    public void ShakeCamera(float duration, float speed, float smoothRatio, float magnitude, float falloff,
        float angularMagnitude, float angularFalloff, float rotSmoothRatio, float rotMagnitude, float rotFalloff)
    {
        StartCoroutine(CameraShake(duration, speed, smoothRatio, magnitude, falloff,
                    angularMagnitude, angularFalloff, rotSmoothRatio, rotMagnitude, rotFalloff));
    }

    /// <summary>
    /// Shakes the camera. FIX LERP INCREMENT AND REFACTOR
    /// </summary>
    private IEnumerator CameraShake(float duration, float speed, float smoothRatio, float magnitude, float falloff, 
        float angularMagnitude, float angularFalloff, float rotSmoothRatio, float rotMagnitude, float rotFalloff)
    {
        Vector3 originalPos = transform.position;
        Vector3 originalDir = transform.forward;
        Quaternion originalRot = transform.rotation;
        Quaternion originalLocalRot = transform.localRotation;

        float elapsed = 0.0f;

        while(elapsed < duration)
        {
            float PosX = Random.Range(-1f, 1f) * magnitude;
            float PosY = Random.Range(-1f, 1f) * magnitude;
            Vector3 swayPos = originalPos + transform.TransformDirection(new Vector3(PosX, PosY, 0));

            float DirX = Random.Range(-1f, 1f) * angularMagnitude;
            float DirY = Random.Range(-1f, 1f) * angularMagnitude;
            Vector3 swayDir = originalDir + transform.TransformDirection(new Vector3(DirX, DirY, 0));

            float RotZ = Random.Range(-1f, 1f) * rotMagnitude;
            Vector3 swayRot = originalLocalRot.eulerAngles;
            swayRot.z += RotZ;

        float swayTime = Random.Range(0.1f * speed, 0.2f * speed);
            float swayElapsed = 0.0f;

            while (swayElapsed < swayTime && swayElapsed + elapsed < duration)
            {
                transform.position = Vector3.Lerp(transform.position, swayPos, smoothRatio * Time.deltaTime);
                transform.forward = Vector3.Slerp(transform.forward, swayDir, smoothRatio * Time.deltaTime);
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(swayRot), rotSmoothRatio * Time.deltaTime);

                swayElapsed += Time.deltaTime;

                yield return null;
            }

            magnitude *= falloff;
            angularMagnitude *= angularFalloff;
            rotMagnitude *= rotFalloff;

            elapsed += swayTime;
        }

        int i = 0;
        while(i < 100)
        {
            transform.position = Vector3.Lerp(transform.position, originalPos, smoothRatio * Time.deltaTime);
            transform.forward = Vector3.Slerp(transform.forward, originalDir, smoothRatio * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, originalRot, smoothRatio * Time.deltaTime);

            i++;
            yield return null;
        }
    }
}
