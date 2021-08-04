using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal.VersionControl;
using UnityEngine;
using UnityEngine.UI;

public class DebugManager : MonoBehaviour
{
    public static DebugManager instance;

    [SerializeField] private Text overlay = default;
    private List<OverlayItem> overlayItems = new List<OverlayItem>();

    private Dictionary<string, LineRenderer> _lineRendererPool = new Dictionary<string, LineRenderer>();

    public Material lineRendererMaterial;

    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(gameObject);
        } else
        {
            instance = this;
        }
    }

    /// <summary>
    /// Get an overlay item from the debug overlay by name.
    /// </summary>
    public OverlayItem GetOverlayItem(string name)
    {
        foreach(OverlayItem item in overlayItems)
        {
            if(item.Name == name)
            {
                return item;
            }
        }

        return null;
    }

    /// <summary>
    /// Set the value of an item in the debug overlay, or add it if it doesn't exist.
    /// </summary>
    public void SetOverlayItem(OverlayItem item, int? index = null)
    {
        foreach (OverlayItem overlayItem in overlayItems)
        {
            if (overlayItem.Name == item.Name)
            {
                overlayItem.Value = item.Value;
                RefreshDebugOverlay();
                return;
            }
        }

        if(index != null && index < overlayItems.Count)
        {
            overlayItems[(int) index] = item;
            RefreshDebugOverlay();
            return;
        }

        overlayItems.Add(item);
        RefreshDebugOverlay();
    }

    /// <summary>
    /// Remove an overlay item from the debug overlay by name.
    /// </summary>
    public void RemoveOverlayItem(string name)
    {
        overlayItems.Remove(GetOverlayItem(name));
        RefreshDebugOverlay();
    }

    public void RefreshDebugOverlay()
    {
        string text = "";

        foreach(OverlayItem item in overlayItems)
        {
            text += item.Name + item.Prefix + item.Value + item.Suffix + "\n";
        }

        overlay.text = text;
    }

    /// <summary>
    /// Draw a vector in world space.
    /// </summary>
    public void UpdateVisualVector(string name, Vector3 origin, Vector3 direction, Color color, float width)
    {
        LineRenderer lr = GetLineRendererFromPool(name);
        lr.SetPosition(0, origin);
        lr.SetPosition(1, origin + direction);
        lr.startWidth = width;
        lr.endWidth = 0;
        lr.material = lineRendererMaterial;
        lr.startColor = color;
        lr.endColor = color;
    }

    private LineRenderer GetLineRendererFromPool(string name)
    {
        if(_lineRendererPool.ContainsKey(name))
        {
            return _lineRendererPool[name];
        } else
        {
            GameObject newGameObject = new GameObject();
            LineRenderer newLr = newGameObject.AddComponent<LineRenderer>();
            _lineRendererPool.Add(name, newLr);
            return newLr;
        }
    }

    private void RemoveLineRendererFromPool(string name)
    {
        Destroy(_lineRendererPool[name].gameObject);
        _lineRendererPool.Remove(name);
    }
}

public class OverlayItem
{
    public string Name;
    public dynamic Value;
    public string Prefix;
    public string Suffix;

    public OverlayItem(string name, dynamic value, string prefix, string suffix)
    {
        Name = name;
        Value = value;
        Prefix = prefix;
        Suffix = suffix;
    }
}
