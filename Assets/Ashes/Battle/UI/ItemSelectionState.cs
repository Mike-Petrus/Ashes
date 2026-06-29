using System;
using System.Collections.Generic;

public class ItemSelectionState : IInputState, IMenuState
{
    // --- IMenuState ---
    public IReadOnlyList<string> MenuOptions => menuOptions;
    public int CurrentIndex { get; private set; } = 0;

    // Cache a list of KVP (ItemId, Quantity)
    private List<string> menuOptions = new();
    private List<KeyValuePair<string, int>> inventorySnapshot = new();

    private int columns = 3; // TODO: Decide final grid size

    public void Enter(PlayerTurnController context)
    {
        // 1. Get the current consumables from the PartyManager
        var consumables = context.Party.Inventory.GetConsumables();
        
        if (consumables.Count == 0)
        {
            menuOptions.Add("Empty");
        }
        else
        {
            // BattleMenuUI updates every frame. Build the Inventory once on Enter
            foreach (var kvp in consumables)
            {
                inventorySnapshot.Add(kvp);

                // Query database for display name
                ItemTemplate template = context.Simulation.BattleContext.ItemDatabase.GetItem(kvp.Key);
                string displayName = template != null ? template.Name : kvp.Key;

                menuOptions.Add($"{displayName} x{kvp.Value}");
            }
        }

        // 2. Safely clamp the cursor (Cursor Memory)
        if (!string.IsNullOrEmpty(context.SelectedItemId))
        {
            int foundIndex = inventorySnapshot.FindIndex(kvp => kvp.Key == context.SelectedItemId);
            if (foundIndex >= 0)
            {
                CurrentIndex = foundIndex;
            }
        }

        if (CurrentIndex >= inventorySnapshot.Count)
        {
            CurrentIndex = 0;
        }
    }

    public void ProcessInput(PlayerTurnController context, InputButton button)
    {
        int listSize = inventorySnapshot.Count;

        if (listSize == 0)
        {
            if (button == InputButton.Cancel)
            {
                context.RevertToPreviousState();
            }
            return;
        }

        int minInRow = (CurrentIndex / columns) * columns;
        int maxInRow = Math.Min(minInRow + columns - 1, listSize - 1);

        switch (button)
        {
            case InputButton.Right:
                CurrentIndex++;

                if (CurrentIndex > maxInRow)
                {
                    CurrentIndex = minInRow; // Wrap to left
                }
                break;

            case InputButton.Left:
                CurrentIndex--;

                if (CurrentIndex < minInRow)
                {
                    CurrentIndex += maxInRow; // Wrap to right
                }
                break;

            case InputButton.Down:
                // Jump down a row
                CurrentIndex += columns;

                if (CurrentIndex >= listSize)
                {
                    CurrentIndex %= columns; // Wrap to top
                }
                break;

            case InputButton.Up:
                // Jump up a row
                CurrentIndex -= columns;

                if (CurrentIndex < 0)
                {
                    int col = CurrentIndex + columns;
                    CurrentIndex = col;

                    while (CurrentIndex + columns < listSize)
                    {
                        CurrentIndex += columns;
                    }
                }
                break;

            case InputButton.Confirm:
                string selectedItemId = inventorySnapshot[CurrentIndex].Key;
                ItemTemplate itemData = context.Simulation.BattleContext.ItemDatabase.GetItem(selectedItemId);

                if (itemData != null)
                {
                    TryUseItem(context, selectedItemId, itemData);
                }
                break;

            case InputButton.Cancel:
                context.RevertToPreviousState();
                break;

            case InputButton.Pursuit:
                context.PursuitEnabled = !context.PursuitEnabled;
                break;
                
            case InputButton.FreeAim:
                context.FreeAimEnabled = !context.FreeAimEnabled;
                break;
        }        
    }

    public void ProcessAnalogInput(PlayerTurnController context, float x, float y, float deltaTime) { }
    public void Exit(PlayerTurnController context) { }

    private void TryUseItem(PlayerTurnController context, string itemId, ItemTemplate itemData)
    {
        Ability useItemAbility = new UseItemAbility(itemData);

        bool canCast = true;
        foreach (var req in useItemAbility.Requirements)
        {
            if (!req.MeetsRequirement(context.ActiveActorId.Value, context.Simulation.BattleContext))
            {
                canCast = false;
                return;
            }
        }

        if (canCast)
        {
            // Remember the item for Cursor Memory
            context.SelectedItemId = itemId; 
            context.SelectedAbility = useItemAbility;
            
            switch (useItemAbility.Mode)
            {
                case TargetingMode.SingleTarget:
                case TargetingMode.ActorAoE:
                    context.ChangeState(new TargetingActorState());
                    break;

                case TargetingMode.Self:
                    context.ChangeState(new TargetingSelfState());
                    break;

                case TargetingMode.Directional:
                case TargetingMode.PointAoE:
                    context.ChangeState(new TargetingFreeAimState());
                    break;

                case TargetingMode.HybridAoE:
                    if (context.FreeAimEnabled)
                    {
                        context.ChangeState(new TargetingFreeAimState());
                    }
                    else
                    {
                        context.ChangeState(new TargetingActorState());
                    }
                    break;
            }
            return;
        }
        else
        {
            context.Simulation.Events.Publish(new PlayerFeedbackEvent("Not enough items!"));
        }
    }
}