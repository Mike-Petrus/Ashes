using System.Collections.Generic;

public class RootMenuPhase1State : IInputState, IMenuState
{
    // --- IMenuState ---
    public IReadOnlyList<string> MenuOptions => menuOptions;
    public int CurrentIndex { get; private set; } = 0;

    private List<string> menuOptions = new();

    public void Enter(PlayerTurnController context)
    {
        PopulateMenuOptions(context);

        string lastSelection = string.IsNullOrEmpty(context.SelectedPhase1Option) ? menuOptions[0] : context.SelectedPhase1Option;
        CurrentIndex = menuOptions.IndexOf(lastSelection);

        if (CurrentIndex < 0)
        {
            CurrentIndex = 0;
        }
    }

    public void ProcessInput(PlayerTurnController context, InputButton button)
    {
        switch (button)
        {
            case InputButton.Up:
                CurrentIndex--;

                if (CurrentIndex < 0)
                {
                    CurrentIndex = menuOptions.Count - 1;
                }
                break;

            case InputButton.Down:
                CurrentIndex++;

                if (CurrentIndex >= menuOptions.Count)
                {
                    CurrentIndex = 0;
                }
                break;

            case InputButton.Confirm:
                string selection = menuOptions[CurrentIndex];
                context.SelectedPhase1Option = selection;

                HandleAbilitySelection(context, selection);
                break;

            case InputButton.Cancel:
                context.SelectedPhase1Option = null;
                context.ActiveActorId = null;
                context.RevertToPreviousState();
                break;

            case InputButton.Pursuit:
                context.PursuitEnabled = !context.PursuitEnabled;
                PopulateMenuOptions(context);
                break;
        }
    }

    private void PopulateMenuOptions(PlayerTurnController context)
    {
        menuOptions.Clear();
        menuOptions.Add("Attack");

        var actor = context.Simulation.Actors.GetActor(context.ActiveActorId.Value);
        
        foreach (string category in actor.Abilities.AvailableAbilities.Keys)
        {
            if (category == "Weapon Skill")
            {
                continue;
            }
            menuOptions.Add(category);
        }

        menuOptions.Add(context.PursuitEnabled ? "Follow" : "Move");
        menuOptions.Add("Items");        
    }

    private void HandleAbilitySelection(PlayerTurnController context, string selection)
    {
        switch (selection)
        {
            case "Attack":
                ValidateAttack(context);
                break;

            case "Move":
                context.ChangeState(new TargetingMoveState());
                break;

            case "Follow":
                context.SelectedAbility = new DummyAbility("system_follow", range: 1f);
                context.ChangeState(new TargetingActorState());
                break;

            case "Items":
                context.ChangeState(new ItemSelectionState());
                break;

            default:
                // TODO: Handle edge cases
                // Right now it is not very robust, but will handle fallthrough and assume anything
                // outside Attack/Move/Items is an ability category
                context.ChangeState(new AbilitySelectionState(selection));
                break;
        }
    }

    private void ValidateAttack(PlayerTurnController context)
    {
        // 1. Get the actual attack ability from the actor's memory
        var actor = context.Simulation.Actors.GetActor(context.ActiveActorId.Value);

        if (!actor.Abilities.AvailableAbilities.TryGetValue("Weapon Skill", out var attackList) || attackList.Count == 0)
        {
            context.Simulation.Events.Publish(new PlayerFeedbackEvent("No Attack Found!"));
            return;
        }

        Ability attackAbility = attackList[0];

        // 2. Validate it (e.g. Are they Disarmed?)
        bool canAttack = true;
        foreach (var req in attackAbility.Requirements)
        {
            if (!req.MeetsRequirement(context.ActiveActorId.Value, context.Simulation.BattleContext))
            {
                canAttack = false;
                return;
            }
        }

        // 3. Execute or Reject
        if (canAttack)
        {
            context.SelectedAbility = attackAbility;
            context.ChangeState(new TargetingActorState());
        }
        else
        {
            context.Simulation.Events.Publish(new PlayerFeedbackEvent("Cannot Attack!"));
        }
    }

    public void ProcessAnalogInput(PlayerTurnController context, float x, float y, float deltaTime) { }
    public void Exit(PlayerTurnController context)
    {
        menuOptions.Clear();
    }
}