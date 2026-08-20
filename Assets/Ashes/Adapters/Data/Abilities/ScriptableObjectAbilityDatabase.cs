using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class AbilityCategoryGroup
{
    public string CategoryName;
    public List<AbilityTemplateSO> Abilities = new List<AbilityTemplateSO>();
}

public class ScriptableObjectAbilityDatabase : MonoBehaviour, IAbilityDatabase
{
    [Tooltip("Click the three dots in the top right of this component and select 'Auto-Populate' to fill this list!")]
    public List<AbilityCategoryGroup> CategorizedAbilities = new List<AbilityCategoryGroup>();

    private Dictionary<string, AbilityTemplate> abilityCache;

    public void Initialize()
    {
        abilityCache = new Dictionary<string, AbilityTemplate>();

        // Loop through categories, then loop through abilities to build the flat dictionary
        foreach (var group in CategorizedAbilities)
        {
            foreach (var asset in group.Abilities)
            {
                if (asset != null && !string.IsNullOrEmpty(asset.AbilityId))
                {
                    abilityCache[asset.AbilityId] = asset.ToDomain();
                }
            }
        }
    }

    public AbilityTemplate GetAbility(string abilityId)
    {
        if (abilityCache != null && abilityCache.TryGetValue(abilityId, out var ability))
        {
            return ability;
        }

        Debug.LogError($"[AbilityDatabase] Could not find ability with ID: {abilityId}");
        return null;
    }

#if UNITY_EDITOR
    [ContextMenu("Auto-Populate Database")]
    private void AutoPopulate()
    {
        CategorizedAbilities.Clear();
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:AbilityTemplateSO");
        
        // Temporary dictionary to group them by Category name
        Dictionary<string, List<AbilityTemplateSO>> groupedAbilities = new Dictionary<string, List<AbilityTemplateSO>>();

        int totalCount = 0;

        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<AbilityTemplateSO>(path);
            
            if (asset != null)
            {
                string cat = string.IsNullOrEmpty(asset.Category) ? "Uncategorized" : asset.Category;
                
                if (!groupedAbilities.ContainsKey(cat))
                {
                    groupedAbilities[cat] = new List<AbilityTemplateSO>();
                }
                
                groupedAbilities[cat].Add(asset);
                totalCount++;
            }
        }

        // Convert the temporary dictionary into our serializable Inspector list
        foreach (var kvp in groupedAbilities)
        {
            var group = new AbilityCategoryGroup
            {
                CategoryName = kvp.Key,
                Abilities = kvp.Value
            };

            // Sort abilities within this category alphabetically by ID
            group.Abilities.Sort((a, b) => string.Compare(a.AbilityId, b.AbilityId));

            CategorizedAbilities.Add(group);
        }

        // Sort the categories themselves alphabetically (e.g. "Black Magic" comes before "White Magic")
        CategorizedAbilities.Sort((a, b) => string.Compare(a.CategoryName, b.CategoryName));

        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"Successfully auto-populated {totalCount} abilities across {CategorizedAbilities.Count} categories!");
    }
#endif
}