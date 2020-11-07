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
}

public class OverlayItem
{
    public string Name;
    public object Value;
    public string Prefix;
    public string Suffix;

    public OverlayItem(string name, object value, string prefix, string suffix)
    {
        Name = name;
        Value = value;
        Prefix = prefix;
        Suffix = suffix;
    }
}
