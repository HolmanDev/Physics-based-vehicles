using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleControl : MonoBehaviour
{
    [SerializeField] private bool _SASEnabled = false;
    public bool SASEnabled => _SASEnabled;
    [SerializeField] private float _SASStrength = 1;
    private Vehicle _vehicle;
    public Vehicle Vehicle { set { _vehicle = value; } }

    public void UpdateControl()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            IgniteAllEngines();
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            ShutDownAllEngines();
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            ThrottleAllEnginesUp();
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            ThrottleAllEnginesDown();
        }

        if(Input.GetKeyDown(KeyCode.T))
        {
            if (_SASEnabled)
            {
                DisableSAS();
            }
            else
            {
                ActiveSAS();
            }
        }

        if (Input.GetKey(KeyCode.Keypad8))
        {
            _SASStrength = Mathf.Clamp01(_SASStrength + 0.5f * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.Keypad2))
        {
            _SASStrength = Mathf.Clamp01(_SASStrength - 0.5f * Time.deltaTime);            
        }

        #region Debug
        DebugManager.instance.SetOverlayItem(new OverlayItem("_SASStrength", _SASStrength, ": ", ""), 2);
        #endregion

        TryGimbalAllEngines();
        TryMoveAllMoveableStructures();
    }

    private void ActiveSAS()
    {
        _SASEnabled = true;
    }

    private void DisableSAS()
    {
        _SASEnabled = false;
    }

    private void TryGimbalAllEngines()
    {
        for (int i = 0; i < _vehicle.Parts.Count; i++)
        {
            if (_vehicle.Parts[i].Type == VehiclePartType.Engine)
            {
                TEngine engineType = (TEngine) _vehicle.Parts[i];

                if (engineType._gimbalEnabled)
                {
                    Vector4 gimbal = engineType.GetGimbalInput();
                    engineType.TryGimbal(gimbal);
                }
            }
        }
    }

    private void TryMoveAllMoveableStructures()
    {
        for (int i = 0; i < _vehicle.Parts.Count; i++)
        {
            if (_vehicle.Parts[i].Type == VehiclePartType.MoveableStructural)
            {
                TMoveableStructure moveableStructureType = (TMoveableStructure) _vehicle.Parts[i];
                
                moveableStructureType.TryMove(moveableStructureType.GetMoveInput(), _SASEnabled, _SASStrength);
            }
        }
    }

    private void ThrottleAllEnginesUp()
    {
        for (int i = 0; i < _vehicle.Parts.Count; i++)
        {
            if (_vehicle.Parts[i].Type == VehiclePartType.Engine)
            {
                TEngine engineType = (TEngine) _vehicle.Parts[i];

                engineType.ThrottleUp();
            }
        }
    }

    private void ThrottleAllEnginesDown()
    {
        for (int i = 0; i < _vehicle.Parts.Count; i++)
        {
            if (_vehicle.Parts[i].Type == VehiclePartType.Engine)
            {
                TEngine engineType = (TEngine) _vehicle.Parts[i];

                engineType.ThrottleDown();
            }
        }
    }

    private void ShutDownAllEngines()
    {
        for (int i = 0; i < _vehicle.Parts.Count; i++)
        {
            if (_vehicle.Parts[i].Type == VehiclePartType.Engine)
            {
                TEngine engineType = (TEngine) _vehicle.Parts[i];

                if (engineType._burning)
                {
                    engineType.ShutDown();
                }
            }
        }
    }

    private void IgniteAllEngines()
    {
        for (int i = 0; i < _vehicle.Parts.Count; i++)
        {
            if (_vehicle.Parts[i].Type == VehiclePartType.Engine)
            {
                TEngine engineType = (TEngine) _vehicle.Parts[i];

                if (!engineType._burning)
                {
                    engineType.Ignite();
                }
            }
        }
    }
}
