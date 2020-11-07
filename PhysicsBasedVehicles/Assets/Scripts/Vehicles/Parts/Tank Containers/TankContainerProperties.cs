using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*public class TankContainerProperties : MonoBehaviour
{
    public Tank[] _tanks;

    [Tooltip("m")]
    public float _diameter;
    [Tooltip("m")]
    public float _height;

    private TTankContainerStructure _tankContainerStructure;
    
    public void DebugSetTankContainerStructure()
    {
        _tankContainerStructure = GetComponent<TTankContainerStructure>();
    }

    public void Initialize(Vehicle vehicle)
    {
        _tankContainerStructure = GetComponent<TTankContainerStructure>();
    }

    /// <summary>
    /// Get the volume of the entire tank container in litres
    /// </summary>
    public float GetVolume(string unit)
    {
        float volumeInM3 = (_diameter / 2) * (_diameter / 2) * Mathf.PI * _height;

        if (unit == "m3")
        {
            return volumeInM3;
        }
        if (unit == "l" || unit == "dm3")
        {
            return volumeInM3 / 1000f;
        } else if (unit == "ml" || unit == "cm3")
        {
            return volumeInM3 / 1000000f;
        } else if(unit == null)
        {
            throw new System.ArgumentException("Unit cannot be null");
        } else
        {
            throw new System.ArgumentException("Unknown unit : " + "\'" + unit + "\'");
        }
    }

    /// <summary>
    /// Get the volume of the tank container's wall in litres
    /// </summary>
    public float GetWallVolume(string unit, float thickness)
    {
        float volumeInM3 = ((_diameter / 2) * (_diameter / 2) * Mathf.PI - ((_diameter / 2 - thickness) * (_diameter / 2 - thickness) * Mathf.PI)) * _height;

        if (unit == "m3")
        {
            return volumeInM3;
        }
        if (unit == "l" || unit == "dm3")
        {
            return volumeInM3 * 1000f;
        }
        else if (unit == "ml" || unit == "cm3")
        {
            return volumeInM3 * 1000000f;
        }
        else if (unit == null)
        {
            throw new System.ArgumentException("Unit cannot be null");
        }
        else
        {
            throw new System.ArgumentException("Unknown unit : " + "\'" + unit + "\'");
        }
    }

    /// <summary>
    /// Update the tank's properties
    /// </summary>
    public void UpdateTank()
    {
        _tankContainerStructure.Repaint(_tankContainerStructure.Color, _tankContainerStructure.Texture);
        UpdateTankStructure();
    }

    /// <summary>
    /// Update the dimensions of the tank
    /// </summary>
    public void UpdateTankStructure()
    {
        _tankContainerStructure.Top.transform.localScale = new Vector3(_diameter * 100, _diameter * 100, 100);
        _tankContainerStructure.Core.transform.localScale = new Vector3(_diameter * 100, _diameter * 100, _height * 625);
        _tankContainerStructure.Bottom.transform.localScale = new Vector3(_diameter * 100, _diameter * 100, 100);

        _tankContainerStructure.Top.transform.localPosition = new Vector3(0, _height - 0.15f, 0);
        _tankContainerStructure.Bottom.transform.localPosition = new Vector3(0, -_height + 0.15f, 0);
    }

    public float GetWeight()
    {
        float weight = 0;
        for (int i = 0; i < _tanks.Length; i++)
        {
            weight += _tanks[i].propellantConfig.density * _tanks[i].propellantVolume;
        }
        weight += _tankContainerStructure.ContainerStructureConfig.materialDensity * GetWallVolume("l", 0.00635f);

        return weight;
    }
}

[System.Serializable]
public class Tank
{
    public Propellant propellantConfig;
    [Tooltip("l")]
    public float propellantVolume;
}
*/