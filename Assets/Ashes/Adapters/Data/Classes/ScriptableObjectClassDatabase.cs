using UnityEngine;
using System.Collections.Generic;

public class ScriptableObjectClassDatabase : MonoBehaviour, IClassDatabase
{
    [Tooltip("Click the three dots in the top right of this component and select 'Auto-Populate' to fill this list!")]
    public List<ClassTemplateSO> ClassAssets = new List<ClassTemplateSO>();

    private Dictionary<string, ClassTemplate> _classCache;

    public void Initialize()
    {
        _classCache = new Dictionary<string, ClassTemplate>();

        foreach (var asset in ClassAssets)
        {
            if (asset != null && !string.IsNullOrEmpty(asset.ClassId))
            {
                _classCache[asset.ClassId] = asset.ToDomain();
            }
        }
    }

    public ClassTemplate GetClass(string classId)
    {
        if (_classCache != null && _classCache.TryGetValue(classId, out var classTemplate))
        {
            return classTemplate;
        }

        Debug.LogError($"[ClassDatabase] Could not find class with ID: {classId}");
        return null;
    }

#if UNITY_EDITOR
    [ContextMenu("Auto-Populate Database")]
    private void AutoPopulate()
    {
        ClassAssets.Clear();
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:ClassTemplateSO");
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            ClassAssets.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<ClassTemplateSO>(path));
        }

        ClassAssets.Sort((a, b) => 
        {
            if (a == null || b == null) return 0;
            return string.Compare(a.ClassId, b.ClassId);
        });
        
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"Successfully auto-populated {ClassAssets.Count} classes into the database!");
    }
#endif
}