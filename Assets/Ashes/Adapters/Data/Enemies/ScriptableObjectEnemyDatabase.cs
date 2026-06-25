using UnityEngine;
using System.Collections.Generic;

public class ScriptableObjectEnemyDatabase : MonoBehaviour, IEnemyDatabase
{
    [Tooltip("Click the three dots in the top right of this component and select 'Auto-Populate' to fill this list!")]
    public List<EnemyTemplateSO> EnemyAssets = new List<EnemyTemplateSO>();

    // The Pure C# Cache
    private Dictionary<string, EnemyTemplate> _enemyCache;

    public void Initialize()
    {
        _enemyCache = new Dictionary<string, EnemyTemplate>();

        foreach (var asset in EnemyAssets)
        {
            if (asset != null && !string.IsNullOrEmpty(asset.EnemyId))
            {
                _enemyCache[asset.EnemyId] = asset.ToDomain();
            }
        }
    }

    public EnemyTemplate GetEnemy(string enemyId)
    {
        if (_enemyCache != null && _enemyCache.TryGetValue(enemyId, out var enemy))
        {
            return enemy;
        }

        Debug.LogError($"[EnemyDatabase] Could not find enemy with ID: {enemyId}");
        return null;
    }

#if UNITY_EDITOR
    [ContextMenu("Auto-Populate Database")]
    private void AutoPopulate()
    {
        EnemyAssets.Clear();
        // Finds all assets of type EnemyTemplateSO anywhere in the Unity Project
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:EnemyTemplateSO");
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            EnemyAssets.Add(UnityEditor.AssetDatabase.LoadAssetAtPath<EnemyTemplateSO>(path));
        }

        EnemyAssets.Sort((a, b) => 
        {
            if (a == null || b == null) return 0;
            return string.Compare(a.EnemyId, b.EnemyId);
        });
        
        // Marks the scene as dirty so Unity knows to save these changes
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"Successfully auto-populated {EnemyAssets.Count} enemies into the database!");
    }
#endif
}