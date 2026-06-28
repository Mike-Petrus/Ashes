using System.Collections.Generic;

public class RootMenuPhase2State : IInputState, IMenuState
{
    // --- IMenuState ---
    public IReadOnlyList<string> MenuOptions => menuOptions;
    public int CurrentIndex { get; private set; } = 0;

    private List<string> menuOptions = new();

    public void Enter(PlayerTurnController context)
    {
        menuOptions.Clear();
        PopulateMenuOptions(context, context.Builder.LastStepAdded());

        string lastSelection = string.IsNullOrEmpty(context.SelectedPhase2Option) ? menuOptions[0] : context.SelectedPhase2Option;
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

                // Tell UI to move cursor
                break;

            case InputButton.Down:
                CurrentIndex++;

                if (CurrentIndex >= menuOptions.Count)
                {
                    CurrentIndex = 0;
                }

                // Tell UI to move cursor
                break;

            case InputButton.Confirm:
                string selection = menuOptions[CurrentIndex];
                context.SelectedPhase2Option = selection;

                HandleAbilitySelection(context, selection);
                break;

            case InputButton.Cancel:
                context.SelectedPhase2Option = null;
                context.Builder.UndoLastStep();
                context.RevertToPreviousState();
                break;

            case InputButton.Pursuit:
                // TODO: probably should be disabled at this point, but we'll see in testing
                context.PursuitEnabled = !context.PursuitEnabled;
                break;
        }
    }

    private void PopulateMenuOptions(PlayerTurnController context, CommandStep lastStep)
    {
        switch (lastStep)
        {
            case MoveStep:
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

                menuOptions.Add("Items");
                menuOptions.Add("Wait");

                break;

            case AbilityStep:
                menuOptions.Add("Move");
                menuOptions.Add("Wait");

                break;

            case PursuitStep:
                // TODO: PursuitStep should technically never make it to this state, but for now, just break
                break;
        }
   
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

            case "Wait":
                context.Builder.AddStep(new WaitStep(context.ActiveActorId.Value));
                context.SubmitCommand();
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
        // Update/hide UI
        menuOptions.Clear();
    }
}