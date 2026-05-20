using System.Collections.Generic;

public class SharedInventory
{
    // ItemId : Quanitty
    private Dictionary<string, int> consumableItems = new();

    // TODO: Add Dictionaries for equipment, key items, etc.

    public void AddItem(string itemId, int amount = 1)
    {
        if (consumableItems.ContainsKey(itemId))
        {
            consumableItems[itemId] += amount;
        }
        else
        {
            consumableItems[itemId] = amount;
        }
    }

    public bool HasItem(string itemId, int amount = 1)
    {
        return consumableItems.ContainsKey(itemId) && consumableItems[itemId] >= amount;
    }

    public bool ConsumeItem(string itemId, int amount = 1)
    {
        if (HasItem(itemId, amount))
        {
            consumableItems[itemId] -= amount;

            if (consumableItems[itemId] <= 0)
            {
                consumableItems.Remove(itemId);
            }
            
            return true;
        }
        return false;
    }

    public IReadOnlyDictionary<string, int> GetConsumables()
    {
        return consumableItems;
    }
}