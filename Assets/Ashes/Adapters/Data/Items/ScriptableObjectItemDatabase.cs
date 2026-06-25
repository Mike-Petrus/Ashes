using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class ItemCategoryGroup
{
    public string CategoryName;
    public List<ItemTemplateSO> Items = new List<ItemTemplateSO>();
}

public class ScriptableObjectItemDatabase : MonoBehaviour, IItemDatabase
{
    [Tooltip("Click the three dots in the top right of this component and select 'Auto-Populate' to fill this list!")]
    public List<ItemCategoryGroup> CategorizedItems = new List<ItemCategoryGroup>();

    private Dictionary<string, ItemTemplate> _itemCache;

    public void Initialize()
    {
        _itemCache = new Dictionary<string, ItemTemplate>();

        // Loop through categories, then loop through abilities to build the flat dictionary
        foreach (var group in CategorizedItems)
        {
            foreach (var asset in group.Items)
            {
                if (asset != null && !string.IsNullOrEmpty(asset.ItemId))
                {
                    _itemCache[asset.ItemId] = asset.ToDomain();
                }
            }
        }
    }

    public ItemTemplate GetItem(string itemId)
    {
        if (_itemCache != null && _itemCache.TryGetValue(itemId, out var item))
        {
            return item;
        }

        Debug.LogError($"[ItemDatabase] Could not find item with ID: {itemId}");
        return null;
    }

#if UNITY_EDITOR
    [ContextMenu("Auto-Populate Database")]
    private void AutoPopulate()
    {
        CategorizedItems.Clear();
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:ItemTemplateSO");

        // Temporary dictionary to group them by Category name
        Dictionary<string, List<ItemTemplateSO>> groupedItems = new Dictionary<string, List<ItemTemplateSO>>();

        int totalCount = 0;

        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemTemplateSO>(path);
            
            if (asset != null)
            {
                string cat = string.IsNullOrEmpty(asset.Type.ToString()) ? "Uncategorized" : asset.Type.ToString();
                
                if (!groupedItems.ContainsKey(cat))
                {
                    groupedItems[cat] = new List<ItemTemplateSO>();
                }
                
                groupedItems[cat].Add(asset);
                totalCount++;
            }
        }

        // Convert the temporary dictionary into our serializable Inspector list
        foreach (var kvp in groupedItems)
        {
            var group = new ItemCategoryGroup
            {
                CategoryName = kvp.Key,
                Items = kvp.Value
            };

            // Sort abilities within this category alphabetically by ID
            group.Items.Sort((a, b) => string.Compare(a.ItemId, b.ItemId));

            CategorizedItems.Add(group);
        }

        // Sort the categories themselves alphabetically (e.g. "Consumable" comes before "Weapon")
        CategorizedItems.Sort((a, b) => string.Compare(a.CategoryName, b.CategoryName));
        
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"Successfully auto-populated {totalCount} items across {CategorizedItems.Count} categories!");
    }
#endif
}