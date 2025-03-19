﻿#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

public static class OdinValueDrawerExtensions
{
    public static void ForceSaveParent<T>(this OdinValueDrawer<T> drawer)
    {
        var target = (Object)drawer.Property.Parent.ValueEntry.WeakSmartValue;
        EditorUtility.SetDirty(target);
        AssetDatabase.SaveAssetIfDirty(target);
    }
    
    public static void SetDirtyParent<T>(this OdinValueDrawer<T> drawer)
    {
        var target = (Object)drawer.Property.Parent.ValueEntry.WeakSmartValue;
        EditorUtility.SetDirty(target);
    }
}
#endif
