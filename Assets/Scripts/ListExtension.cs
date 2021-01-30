using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ListExtension
{
    public static T TakeLast<T>(this List<T> list)
    {
        int index = list.Count - 1;
        var result = list[index];
        list.RemoveAt(index);
        return result;
    }
}
