using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RS25 : TEngine
{
    /// <summary>
    /// Get raw gimbal strength in multiple directions based on user input on current frame. 
    /// Return format: (D, A, W, S)
    /// </summary>
    public override Vector4 GetGimbalInput()
    {
        Vector4 gimbal = new Vector4();

        // Gimbal based on keyboard input
        float partGimbalLimit = EngineConfig.gimbalLimit;

        gimbal.x = Input.GetKey(KeyCode.D) ? partGimbalLimit : 0;
        gimbal.y = Input.GetKey(KeyCode.A) ? partGimbalLimit : 0;
        gimbal.z = Input.GetKey(KeyCode.W) ? partGimbalLimit : 0;
        gimbal.w = Input.GetKey(KeyCode.S) ? partGimbalLimit : 0;

        return gimbal;
    }

    /// <summary>
    /// Execute gimbal maneuver if possible.
    /// </summary>
    public override void TryGimbal(Vector4 gimbal)
    {
        Vector3 localGimbalDirection = new Vector3(gimbal.w - gimbal.z, 0, gimbal.x - gimbal.y);

        GimbalRotation(localGimbalDirection);
        GimbalThrustDirection(localGimbalDirection);
    }

    /// <summary>
    /// Rotate engine around gimbal pivot.
    /// </summary>
    public override void GimbalRotation(Vector3 localGimbalDirection)
    {
        _gimbalPivot.localRotation = Quaternion.Euler(_defaultGimbalRotation + localGimbalDirection.
                    RecontextualizeDirection(transform, _gimbalPivot));
    }

    /// <summary>
    /// Align thrust direction with gimbal direction.
    /// </summary>
    public override void GimbalThrustDirection(Vector3 localGimbalDirection)
    {
        _thrustDirection = Quaternion.Euler(_defaultEngineRotation + localGimbalDirection);
    }
}
