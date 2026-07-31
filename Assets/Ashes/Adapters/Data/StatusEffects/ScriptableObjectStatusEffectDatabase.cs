using UnityEngine;
using System.Collections.Generic;

public class ScriptableObjectStatusEffectDatabase : MonoBehaviour, IStatusEffectDatabase
{
    [Tooltip("Click the three dots in the top right of this component and select 'Auto-Populate' to fill this list!")]
    public List<StatusBlueprintSO> StatusAssets = new List<StatusBlueprintSO>();

    private Dictionary<string, StatusEffectTemplate> _statusCache;

    public void Initialize()
    {
        _statusCache = new Dictionary<string, StatusEffectTemplate>();

        foreach (var asset in StatusAssets)
        {
            if (asset != null && !string.IsNullOrEmpty(asset.StatusId))
            {
                _statusCache[asset.StatusId] = asset.ToDomain();
            }
        }
    }

    public StatusEffectTemplate GetStatusEffect(string statusId)
    {
        if (_statusCache != null && _statusCache.TryGetValue(statusId, out var status))
        {
            return status;
        }

        Debug.LogError($"[StatusDatabase] Could not find status with ID: {statusId}");
        return null;
    }

#if UNITY_EDITOR
    [ContextMenu("Auto-Populate Database")]
    private void AutoPopulate()
    {
        StatusAssets.Clear();
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:StatusBlueprintSO");
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            StatusAssets.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<StatusBlueprintSO>(path));
        }
        
        StatusAssets.Sort((a, b) => 
        {
            if (a == null || b == null) return 0;
            return string.Compare(a.StatusId, b.StatusId);
        });
        
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"Successfully auto-populated {StatusAssets.Count} statuses into the database!");
    }
#endif
}