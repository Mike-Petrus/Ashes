using System;
using System.Collections.Generic;

public class AbilitySelectionState : IInputState, IMenuState
{
        // --- IMenuState ---
    public IReadOnlyList<string> MenuOptions => menuOptions;
    public int CurrentIndex { get; private set; } = 0;

    private string category;
    private List<string> menuOptions = new();
    private List<Ability> availableAbilities = new();
    
    private int columns = 3; // Define our grid size. TODO: Decide final size later

    public AbilitySelectionState(string abilityCategory)
    {
        category = abilityCategory;
    }

    public void Enter(PlayerTurnController context)
    {
        menuOptions.Clear();
        availableAbilities.Clear();

        var actor = context.Simulation.Actors.GetActor(context.ActiveActorId.Value);

        if (actor.Abilities.AvailableAbilities.TryGetValue(category, out var abilities))
        {
            availableAbilities.AddRange(abilities);
        }

        if (availableAbilities.Count == 0)
        {
            menuOptions.Add("Empty");
            CurrentIndex = 0;
            return;
        }

        // Cache options
        foreach (var ability in availableAbilities)
        {
            menuOptions.Add(ability.Name);
        }
        
        // Cursor Memory
        CurrentIndex = availableAbilities.IndexOf(context.SelectedAbility ?? availableAbilities[0]);
        if (CurrentIndex < 0)
        {
            CurrentIndex = 0;
        }
    }

    public void ProcessInput(PlayerTurnController context, InputButton button)
    {
        int listSize = availableAbilities.Count;

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
                Ability selected = availableAbilities[CurrentIndex];
                TryCastAbility(context, selected);
                break;

            case InputButton.Cancel:
                context.RevertToPreviousState();
                break;

            case InputButton.Pursuit:
                context.TogglePursuit();
                break;
                
            case InputButton.FreeAim:
                context.ToggleFreeAim();
                break;
        }
    }

    public void ProcessAnalogInput(PlayerTurnController context, float x, float y, float deltaTime) { }
    public void Exit(PlayerTurnController context) { }

    private void TryCastAbility(PlayerTurnController context, Ability ability)
    {
        // Check requirements here
        bool canCast = true;
        foreach (var req in ability.Requirements)
        {
            if (!req.MeetsRequirement(context.ActiveActorId.Value, context.Simulation.BattleContext))
            {
                canCast = false;
                return;
            }
        }

        if (canCast)
        {
            context.SelectedAbility = ability;

            SelectTargetingState(context, context.PursuitEnabled, ability.Mode);
        }
        else
        {
            // TODO: Make message more specific depending on which requirements are not met
            context.Simulation.Events.Publish(new PlayerFeedbackEvent("Ability not available!"));
        }    
    }

    private void SelectTargetingState(PlayerTurnController context, bool pursuitEnabled, TargetingMode targetingMode)
    {
        if (pursuitEnabled)
        {
            switch(targetingMode)
            {
                case TargetingMode.Self:
                    context.ChangeState(new TargetingSelfState());
                    break;

                case TargetingMode.PointAoE:
                    context.ChangeState(new TargetingFreeAimState());
                    break;

                default:
                    context.ChangeState(new TargetingActorState());
                    break;
            }
        }
        else
        {
            switch (targetingMode)
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
        }
    }
}