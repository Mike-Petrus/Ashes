public class TargetingSelfState : IInputState
{
    private TargetInfo targetInfo;
    private bool isValid;
    private string currentErrorMessage = "";

    public void Enter(PlayerTurnController context)
    {
        var activeActor  = context.Simulation.Actors.GetActor(context.ActiveActorId.Value);
        targetInfo = TargetInfo.ForSelf(activeActor.Id);

        // Query RangeSystem. In future could have Null-Magic Zone or spells that can only be self cast on certain terrain (e.g. water)
        isValid = TargetingUtility.IsTargetInRange(context, targetInfo, out currentErrorMessage);

        // Draw the cursor on the actor's position or future position if they move first
        SimVector3 displayPosition = TargetingUtility.GetOriginPosition(context);
        TargetingUtility.UpdateCursorVisuals(context, displayPosition, isValid);
    }

    public void ProcessInput(PlayerTurnController context, InputButton button)
    {

        switch (button)
        {
            case InputButton.Confirm:
                if (isValid)
                {
                    context.Simulation.Events.Publish(new CursorMovedEvent(new SimVector3(), false));
                    context.Builder.AddStep(new AbilityStep(context.ActiveActorId.Value, context.SelectedAbility, targetInfo));

                    // Standard Command sequence flow
                    if (context.Builder.Size == 1)
                    {
                        context.ChangeState(new RootMenuPhase2State());
                    }
                    else
                    {
                        context.SubmitCommand();
                    }
                }
                else
                {
                    context.Simulation.Events.Publish(new PlayerFeedbackEvent("Cannot target self!"));
                }
                break;

            case InputButton.Cancel:
                context.Simulation.Events.Publish(new CursorMovedEvent(new SimVector3(), false));
                context.RevertToPreviousState();
                break;

            case InputButton.Pursuit:
                context.Simulation.Events.Publish(new PlayerFeedbackEvent("Cannot use Pursuit on self!"));
                break;

            case InputButton.FreeAim:
                // Shouldn't matter if you toggle here because it will only effect the next command
                // Even if you turn off toggle snap in this state, your state will not change
                context.Simulation.Events.Publish(new PlayerFeedbackEvent($"Cannot free aim with {context.SelectedAbility.Name}!"));
                break;
                
        }
    }

    public void ProcessAnalogInput(PlayerTurnController context, float x, float y, float deltaTime) { }

    public void Exit(PlayerTurnController context)
    {
        context.Simulation.Events.Publish(new CursorMovedEvent(new SimVector3(), false));
    }
}