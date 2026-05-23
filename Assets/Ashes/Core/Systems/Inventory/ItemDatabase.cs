using System.Collections.Generic;

public class ItemDatabase
{
    private Dictionary<string, Item> items = new();

    public ItemDatabase()
    {
        // Registering a Potion
        items.Add("Potion", new Item(
            "Potion", 
            "Potion", 
            "Restores 50 HP", 
            ItemType.Consumable, 
            20f,
            0f, 
            TargetingMode.SingleTarget, 
            TargetAlignment.Everyone, 
            new List<Effect> { new HealEffect(50) }
        ));
    }

    public Item GetItem(string id) => items[id];
}