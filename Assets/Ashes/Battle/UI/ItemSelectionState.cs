using System;
using System.Collections.Generic;
using System.Linq;

public class ItemSelectionState : IInputState, IMenuState
{
    // Cache a list of KVP (ItemId, Quantity)
    private List<KeyValuePair<string, int>> inventorySnapshot = new();

    private int currentIndex = 0;
    private int columns = 3; // TODO: Decide final grid size

    // --- IMenuState ---
    public IReadOnlyList<string> MenuOptions
    {
        get
        {
            List<string> displayNames = new();
            foreach (var kvp in inventorySnapshot)
            {
                // TODO: When we build the ItemDatabase, we will look up the real name here.
                // For now, we just print the ID and the Quantity.
                displayNames.Add($"{kvp.Key} x{kvp.Value}");
            }
            return displayNames;
        }
    }
    public int CurrentIndex => currentIndex;

    public void Enter(PlayerTurnController context)
    {
        // 1. Get the current consumables from the PartyManager
        var consumables = context.Party.Inventory.GetConsumables();
        inventorySnapshot = consumables.ToList();

        // 2. Safely clamp the cursor
        if (!string.IsNullOrEmpty(context.SelectedItemId))
        {
            int foundIndex = inventorySnapshot.FindIndex(kvp => kvp.Key == context.SelectedItemId);
            if (foundIndex >= 0)
            {
                currentIndex = foundIndex;
            }
        }

        if (currentIndex >= inventorySnapshot.Count)
        {
            currentIndex = 0;
        }
    }

    public void ProcessInput(PlayerTurnController context, InputButton button)
    {
        int listSize = inventorySnapshot.Count;

        if (listSize == 0)
        {
            return;
        }

        int minInRow = (currentIndex / columns) * columns;
        int maxInRow = Math.Min(minInRow + columns - 1, listSize - 1);

        switch (button)
        {
            case InputButton.Right:
                currentIndex++;

                if (currentIndex > maxInRow)
                {
                    currentIndex = minInRow; // Wrap to left
                }
                break;

            case InputButton.Left:
                currentIndex--;

                if (currentIndex < minInRow)
                {
                    currentIndex += maxInRow; // Wrap to right
                }
                break;

            case InputButton.Down:
                // Jump down a row
                currentIndex += columns;

                if (currentIndex >= listSize)
                {
                    currentIndex %= columns; // Wrap to top
                }
                break;

            case InputButton.Up:
                // Jump up a row
                currentIndex -= columns;

                if (currentIndex < 0)
                {
                    int col = currentIndex + columns;
                    currentIndex = col;

                    while (currentIndex + columns < listSize)
                    {
                        currentIndex += columns;
                    }
                }
                break;

            case InputButton.Confirm:
                string selectedItemId = inventorySnapshot[currentIndex].Key;
                
                ItemTemplate itemData = context.Simulation.BattleContext.ItemDatabase.GetItem(selectedItemId);

                Ability useItemAbility = new UseItemAbility(itemData);

                // Validation
                if (itemData != null)
                {
                    bool canCast = true;
                    foreach (var req in useItemAbility.Requirements)
                    {
                        if (!req.MeetsRequirement(context.ActiveActorId.Value, context.Simulation.BattleContext))
                        {
                            canCast = false;
                            break;
                        }
                    }

                    if (canCast)
                    {
                        // Remember the item for Cursor Memory
                        context.SelectedItemId = selectedItemId; 
                        context.SelectedAbility = useItemAbility;
                        context.ChangeState(new TargetingActorState());
                    }
                    else
                    {
                        context.Simulation.Events.Publish(new PlayerFeedbackEvent("Not enough items!"));
                    }
                }
                break;

            case InputButton.Cancel:
                context.RevertToPreviousState();
                break;
        }        
    }

    public void ProcessAnalogInput(PlayerTurnController context, float x, float y, float deltaTime) { }

    public void Exit(PlayerTurnController context) { }
}