using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TTankContainerStructure : VehiclePart
{
    [Header("Tank Container Structure")]

    public ContainerStructureConfig ContainerStructureConfig;

    [SerializeField] private GameObject _top = default;
    [SerializeField] private GameObject _core = default;
    [SerializeField] private GameObject _bottom = default;

    [SerializeField, Tooltip("m")] private float _radius = 1;
    [SerializeField, Tooltip("m")] private float _height = 1;

    public ProceduralColor ProceduralColor = new ProceduralColor();
    
    public Aerodynamics Aerodynamics = new Aerodynamics();

    public Tank[] _tanks;

    private void Awake() {
        Type = VehiclePartType.Container;
    }

    private void OnValidate() {
        ProceduralColor.Repaint(ProceduralColor.Color, ProceduralColor.Texture);
    }

    /// <summary>
    /// Update the tank's properties
    /// </summary>
    public void UpdateTank()
    {
        UpdateTankStructure();
    }

    /// <summary>
    /// Update the dimensions of the tank
    /// </summary>
    public void UpdateTankStructure()
    {
        _top.transform.localScale = new Vector3(_radius * 100, _radius * 100, 100);
        _core.transform.localScale = new Vector3(_radius * 100, _radius * 100, _height * 625);
        _bottom.transform.localScale = new Vector3(_radius * 100, _radius * 100, 100);

        _top.transform.localPosition = new Vector3(0, _height - 0.15f, 0);
        _bottom.transform.localPosition = new Vector3(0, -_height + 0.15f, 0);
    }

    /// <summary>
    /// Get the volume of the entire tank container in litres
    /// </summary>
    public float GetVolume(string unit)
    {
        float volumeInM3 = _radius * _radius * Mathf.PI * _height; // Wrong?

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
        float volumeInM3 = ((_radius / 2) * (_radius / 2) * Mathf.PI - ((_radius / 2 - thickness) * (_radius / 2 - thickness) * Mathf.PI)) * _height;

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

    public override float GetWeight()
    {
        float weight = 0;
        for (int i = 0; i < _tanks.Length; i++)
        {
            weight += _tanks[i].propellantConfig.density * _tanks[i].propellantVolume;
        }
        weight += ContainerStructureConfig.materialDensity * GetWallVolume("l", 0.00635f);

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
