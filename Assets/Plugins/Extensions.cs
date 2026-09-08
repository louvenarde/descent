using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;

public static class PluginExtensions
{
#if UNITY_EDITOR
    public static System.Type AssetDatabase_GetMainAssetTypeAtPath(string assetPath)
    {
        var loaded = AssetDatabase.LoadMainAssetAtPath(assetPath);
        System.Type type = loaded.GetType();
        UnityEngine.Object.Destroy(loaded);
        return type;
    }
#endif
}
