using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Unit {
    m, dm, cm, mm, 
    m2, dm2, cm2, mm2, 
    m3, dm3, l, cm3, ml, mm3
};

public static class IntExtensions
{
    /// <summary>
    /// Return all non-zero items in integer array.
    /// </summary>
    public static int[] GetNonZeroItems(this int[] array)
    {
        List<int> nonZeroItems = new List<int>();

        for(int i = 0; i < array.Length; i++)
        {
            if(array[i] != 0)
            {
                nonZeroItems.Add(array[i]);
            }
        }

        return nonZeroItems.ToArray();
    }

    /// <summary>
    /// Convert from m3 to dm3.
    /// </summary>
    public static int Convert(this int value, Unit unitOld, Unit unitNew)
    {
        throw new System.NotImplementedException();
    }
}
