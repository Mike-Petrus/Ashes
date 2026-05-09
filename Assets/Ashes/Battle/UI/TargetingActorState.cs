using System.Collections.Generic;

public class TargetingActorState : IInputState
{
    private List<ActorId> currentAvailableTargets = new();
    private int currentTargetIndex = 0;

    // Cached Data
    private ActorId? savedTargetId = null;
    private bool isTargetingValidActor = true;
    private string currentErrorMessage = "";

    public void Enter(PlayerTurnController context)
    {
        currentAvailableTargets.Clear();

        // TODO: The RangeSystem should filter this list!
        // E.g. If it's a heal, only populate with allies

        foreach (var actorId in context.Simulation.Actors.GetAliveActorIds())
        {
            currentAvailableTargets.Add(actorId);
        }

        if (currentAvailableTargets.Count == 0)
        {
            return;
        }

        // Cursor Memory
        if (savedTargetId.HasValue)
        {
            currentTargetIndex = currentAvailableTargets.IndexOf(savedTargetId.Value);
        }
        else
        {
            currentTargetIndex = -1;
        }

        // Default to 0 if memory was null or the saved taret is no longer in the list
        if (currentTargetIndex < 0)
        {
            currentTargetIndex  = 0;
        }

        // Cache
        savedTargetId = currentAvailableTargets[currentTargetIndex];

        // Initial Validate/Visuals
        ValidateCurrentTarget(context);
        UpdateCursorVisuals(context);
    }

    public void ProcessInput(PlayerTurnController context, InputButton button)
    {
        int listSize = currentAvailableTargets.Count;
        
        if (listSize == 0)
        {
            return;
        }

        switch (button)
        {
            case InputButton.Right:
                currentTargetIndex++;

                if (currentTargetIndex >= listSize)
                {
                    currentTargetIndex = 0;
                }

                savedTargetId = currentAvailableTargets[currentTargetIndex];
                ValidateCurrentTarget(context);
                UpdateCursorVisuals(context);

                break;

            case InputButton.Left:
                currentTargetIndex--;

                if (currentTargetIndex < 0)
                {
                    currentTargetIndex = listSize - 1;
                }

                savedTargetId = currentAvailableTargets[currentTargetIndex];
                ValidateCurrentTarget(context);
                UpdateCursorVisuals(context);

                break;

            // Can combine Right/Down, Left/Up if wanted, but for now they do nothing
            case InputButton.Up:
            case InputButton.Down:
                break;

            case InputButton.Confirm:

                if (!isTargetingValidActor)
                {
                    context.Simulation.Events.Publish(new PlayerFeedbackEvent(currentErrorMessage));
                    // Play error sound
                    return;
                }

                ActorId selectedTarget = currentAvailableTargets[currentTargetIndex];
                var targetInfo = TargetInfo.ForActor(selectedTarget, context.SelectedAbility.Mode);

                context.Simulation.Events.Publish(new CursorMovedEvent(new SimVector3(), false)); // Hide cursor
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
        }
    }

    public void ProcessAnalogInput(PlayerTurnController context, float x, float y, float deltaTime)
    {
        // TODO: Implement Analog stick target snapping
        // Read the X/Y axes, find the target physically closes in that 2D direction
        // relative to the current target. Snap if within threshold, update context.CurrentTargetIndex
    }

    private void ValidateCurrentTarget(PlayerTurnController context)
    {
        isTargetingValidActor = true;
        currentErrorMessage = "";

        if (currentAvailableTargets.Count == 0)
        {
            return;
        }

        ActorId selectedTarget = currentAvailableTargets[currentTargetIndex];

        // TODO: make handle all targeting modes
        var targetInfo = TargetInfo.ForActor(selectedTarget, context.SelectedAbility.Mode);
        var activeActor = context.Simulation.Actors.GetActor(context.ActiveActorId.Value);

        SimVector3 originPosition = activeActor.Position;

        // If we moved in Phase 1, we must calculate range from the FUTURE position
        if (context.Builder.Size > 0)
        {
            if (context.Builder.LastStepAdded() is MoveStep moveStep)
            {
                originPosition = moveStep.Destination;
            }
        }

        if (!context.Simulation.RangeSystem.IsInRange(originPosition, activeActor.Radius, context.SelectedAbility, targetInfo))
        {
            isTargetingValidActor = false;
            currentErrorMessage = "Out of Range!";
        }
    }

    private void UpdateCursorVisuals(PlayerTurnController context)
    {
        if (currentAvailableTargets.Count == 0)
        {
            return;
        }

        var targetActor = context.Simulation.Actors.GetActor(currentAvailableTargets[currentTargetIndex]);

        // Pass the boolean into the event so the cursor changes color
        context.Simulation.Events.Publish(new CursorMovedEvent(targetActor.Position, true, isTargetingValidActor));
    }

    public void Exit(PlayerTurnController context)
    {
        context.Simulation.Events.Publish(new CursorMovedEvent(new SimVector3(), false));
        // Do NOT clear CurrentAvailableTargets here so that if we come back,
        // the memory index doesn't temporarily throw an out of bounds error
    }
}