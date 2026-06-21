public class TargetingFreeAimState : IInputState
{
    // Cached Data
    private SimVector3? savedCursorPosition = null;

    private bool isValidPosition = true;
    private string currentErrorMessage = "";

    private PlayerTurnController currentContext;

    public void Enter(PlayerTurnController context)
    {
        currentContext = context;

        if (!savedCursorPosition.HasValue)
        {
            var activeActor = context.Simulation.Actors.GetActor(context.ActiveActorId.Value);
            context.CurrentCursorPosition = activeActor.Position;
        }
        else
        {
            context.CurrentCursorPosition = savedCursorPosition.Value;
        }

        ValidateCursorPosition(context);
        UpdateCursorVisuals(context);
    }

    public void ProcessInput(PlayerTurnController context, InputButton button)
    {
        // TODO: we can add target cycling without target snapping
        // simplying move cursor to position of next target

        switch (button)
        {
            case InputButton.Confirm:
                bool isValidMode = context.SelectedAbility.Mode == TargetingMode.HybridAoE ||
                    context.SelectedAbility.Mode == TargetingMode.PointAoE ||
                    context.SelectedAbility.Mode == TargetingMode.Directional;

                if (!isValidMode || !isValidPosition)
                {
                    context.Simulation.Events.Publish(new PlayerFeedbackEvent(currentErrorMessage));
                    // Play error sound
                    return;
                }    

                var targetInfo = TargetInfo.ForPosition(context.CurrentCursorPosition, context.SelectedAbility.Mode);

                context.Simulation.Events.Publish(new CursorMovedEvent(new SimVector3(), false));
                context.Builder.AddStep(new AbilityStep(context.ActiveActorId.Value, context.SelectedAbility, targetInfo));  
                
                // Is Command complete?
                if (context.Builder.Size >= 2)
                {
                    context.SubmitCommand();
                }
                else
                {
                    context.ChangeState(new RootMenuPhase2State());
                }

                break;

            case InputButton.Cancel:
                context.Simulation.Events.Publish(new CursorMovedEvent(new SimVector3(), false));
                context.RevertToPreviousState();
                break;

            case InputButton.Pursuit:
                // TODO: Should probably be able to toggle here as long as in Phase 1
                // context.PursuitEnabled = !context.PursuitEnabled;
                break;

            case InputButton.TargetSnap:
                // TODO: Maybe check if Mode == PointAoE or Self? Most states should allow snapping
                context.ChangeState(new TargetingActorState(), false);
                break;
        }
    }

    public void ProcessAnalogInput(PlayerTurnController context, float x, float y, float deltaTime)
    {
        // 1. Slide the cursor
        SimVector3 pos = context.CurrentCursorPosition;
        pos.x += x * context.CursorSpeed * deltaTime;
        pos.z += y * context.CursorSpeed * deltaTime;
        context.CurrentCursorPosition = pos;

        // Cache cursor position for rewinding
        savedCursorPosition = context.CurrentCursorPosition;

        ValidateCursorPosition(context);
        UpdateCursorVisuals(context);
    }

    private void UpdateCursorVisuals(PlayerTurnController context)
    {
        var ability = context.SelectedAbility;
        // Pass the AoE gemoetric data to the presentation layer
        context.Simulation.Events.Publish(new CursorMovedEvent(context.CurrentCursorPosition, true, isValidPosition, ability.Mode, ability.Radius, ability.Angle));
    }

    private void ValidateCursorPosition(PlayerTurnController context)
    {
        var targetInfo = TargetInfo.ForPosition(context.CurrentCursorPosition, context.SelectedAbility.Mode);
        var activeActor = context.Simulation.Actors.GetActor(context.ActiveActorId.Value);

        SimVector3 originPosition = activeActor.Position;

        // If we moved in PHase 1, calculate range from FUTURE position
        if (context.Builder.Size > 0)
        {
            if (context.Builder.LastStepAdded() is MoveStep moveStep)
            {
                originPosition = moveStep.Destination;
            }
        }

        if (!context.Simulation.RangeSystem.IsInRange(originPosition, activeActor.Radius, context.SelectedAbility, targetInfo))
        {
            isValidPosition = false;
            currentErrorMessage = "Out of Range!";
        }
        else
        {
            isValidPosition = true;
            currentErrorMessage = "";
        }
    }

    public void Exit(PlayerTurnController context)
    {
        context.Simulation.Events.Publish(new CursorMovedEvent(new SimVector3(), false));
        currentContext = null;
    }
}