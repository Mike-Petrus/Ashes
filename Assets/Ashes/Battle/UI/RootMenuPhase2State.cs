using System.Collections.Generic;

public class RootMenuPhase2State : IInputState, IMenuState
{
    private List<string> currentMenuOptions = new();
    private string selection;
    private int currentIndex = 0;

    // --- IMenuState ---
    public IReadOnlyList<string> MenuOptions => currentMenuOptions;
    public int CurrentIndex => currentIndex;

    public void Enter(PlayerTurnController context)
    {
        currentMenuOptions.Clear();
        PopulateMenuOptions(context, context.Builder.LastStepAdded());

        currentIndex = currentMenuOptions.IndexOf(selection ?? currentMenuOptions[0]);

        if (currentIndex < 0)
        {
            currentIndex = 0;
        }

        // TODO: Tell UI to draw currentMenuOptions for Phase 2
    }

    public void ProcessInput(PlayerTurnController context, InputButton button)
    {
        switch (button)
        {
            case InputButton.Up:
                currentIndex--;

                if (currentIndex < 0)
                {
                    currentIndex = currentMenuOptions.Count - 1;
                }

                // Tell UI to move cursor
                break;

            case InputButton.Down:
                currentIndex++;

                if (currentIndex >= currentMenuOptions.Count)
                {
                    currentIndex = 0;
                }

                // Tell UI to move cursor
                break;

            case InputButton.Confirm:
                selection = currentMenuOptions[currentIndex];

                HandleAbilitySelection(context, selection);
                break;

            case InputButton.Cancel:
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
                currentMenuOptions.Add("Attack");

                var actor = context.Simulation.Actors.GetActor(context.ActiveActorId.Value);
                currentMenuOptions.AddRange(actor.Abilities.AvailableAbilities.Keys);

                currentMenuOptions.Add("Items");
                currentMenuOptions.Add("Wait");

                break;

            case AbilityStep:
                currentMenuOptions.Add("Move");
                currentMenuOptions.Add("Wait");

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
                context.SelectedAbility = new BasicAttackAbility();
                context.ChangeState(new TargetingActorState());

                break;

            case "Move":
                context.ChangeState(new TargetingMoveState());

                break;

            case "Wait":
                context.Builder.AddStep(new WaitStep(context.ActiveActorId.Value));
                context.SubmitCommand();

                break;

            case "Items":
                //context.ChangeState(new ItemSelectionState());

                break;

            default:
                // TODO: Handle edge cases
                // Right now it is not very robust, but will handle fallthrough and assume anything
                // outside Attack/Move/Items is an ability category
                context.ChangeState(new AbilitySelectionState(selection));
                break;
        }
    }

    public void ProcessAnalogInput(PlayerTurnController context, float x, float y, float deltaTime) { }
    public void Exit(PlayerTurnController context)
    {
        // Update/hide UI
        currentMenuOptions.Clear();
    }
}