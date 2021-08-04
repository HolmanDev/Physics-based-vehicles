using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ProceduralColor
{
    [SerializeField] private MeshRenderer _meshRenderer = default;
    private MaterialPropertyBlock _propBlock;

    public Color Color = new Color(1, 1, 1, 1);
    public Texture2D Texture;

    /// <summary>
    /// Sets a new color and texture for the tank container. MOVE TO VEHICLE PART?
    /// </summary>
    public void Repaint(Color color, Texture2D texture)
    {
        if (_propBlock == null)
            _propBlock = new MaterialPropertyBlock();

        Color = color;
        Texture = texture;

        _meshRenderer.GetPropertyBlock(_propBlock);
        _propBlock.SetColor("_Color", Color);
        _meshRenderer.SetPropertyBlock(_propBlock);
        _meshRenderer.sharedMaterial.SetTexture("_MainTex", Texture);

    }
}
