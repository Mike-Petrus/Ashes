public class TargetingSelfState : IInputState
{
    private TargetInfo targetInfo;
    private bool isValid;

    public void Enter(PlayerTurnController context)
    {
        var activeActor  = context.Simulation.Actors.GetActor(context.ActiveActorId.Value);
        var ability = context.SelectedAbility;
        
        targetInfo = TargetInfo.ForSelf(activeActor.Id);
        SimVector3 originPosition = GetOriginPosition(context, activeActor);

        // Query RangeSystem. In future could have Null-Magic Zone or spells that can only be self cast on certain terrain (e.g. water)
        isValid = context.Simulation.RangeSystem.IsInRange(originPosition, activeActor.Radius, ability, targetInfo);

        // Draw the cursor on the actor's position or future position if they move first
        context.Simulation.Events.Publish(new CursorMovedEvent(originPosition, true, isValid, ability.Mode, ability.Radius, ability.Angle, staticCenter: originPosition));
    }

    public void ProcessInput(PlayerTurnController context, InputButton button)
    {

        switch (button)
        {
            case InputButton.Confirm:
                if (isValid)
                {
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
                // Shouldn't matter if you toggle here because it won't effect the end result. Just turn it on for the next command
                // Only need to disable this if we add something like extra steps (> 2)
                context.PursuitEnabled = !context.PursuitEnabled;
                break;

            case InputButton.FreeAim:
                // Shouldn't matter if you toggle here because it will only effect the next command
                // Even if you turn off toggle snap in this state, your state will not change
                context.FreeAimEnabled = !context.FreeAimEnabled;

                context.Simulation.Events.Publish(new PlayerFeedbackEvent($"Cannot free aim with {context.SelectedAbility.Name}!"));
                break;
                
        }
    }

    public void ProcessAnalogInput(PlayerTurnController context, float x, float y, float deltaTime) { }

    public void Exit(PlayerTurnController context)
    {
        context.Simulation.Events.Publish(new CursorMovedEvent(new SimVector3(), false));
    }

    private SimVector3 GetOriginPosition(PlayerTurnController context, BattleActor actor)
    {
        SimVector3 originPosition = actor.Position;

        // If actor moved in Phase 1, use the future position
        if (context.Builder.Size > 0 && context.Builder.LastStepAdded() is MoveStep moveStep)
        {
            originPosition = moveStep.Destination;
        }

        return originPosition;
    }
}