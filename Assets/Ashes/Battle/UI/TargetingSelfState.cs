using System.Collections.Generic;

public class TargetingSelfState : IInputState
{
    private TargetInfo targetInfo;
    private bool isValid;
    private string currentErrorMessage = "";

    public void Enter(PlayerTurnController context)
    {
        var activeActor  = context.Simulation.Actors.GetActor(context.ActiveActorId.Value);
        targetInfo = TargetInfo.ForSelf(activeActor.Id);

        // Draw the cursor on the actor's position or future position if they move first
        context.CurrentCursorPosition = TargetingUtility.GetOriginPosition(context);

        UpdateTargetVisuals(context);
    }

    public void ProcessInput(PlayerTurnController context, InputButton button)
    {

        switch (button)
        {
            case InputButton.Confirm:
                if (!isValid)
                {
                    context.Simulation.Events.Publish(new PlayerFeedbackEvent(currentErrorMessage));
                    return;
                }
                
                TryConfirmCommand(context);  
                
                break;

            case InputButton.Cancel:
                DisableTargetVisuals(context);
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

    public void ProcessAnalogLeft(PlayerTurnController context, float x, float y, float deltaTime) { }


    private void UpdateTargetVisuals(PlayerTurnController context)
    {
        // Query RangeSystem. In future could have Null-Magic Zone or spells that can only be self cast on certain terrain (e.g. water)
        isValid = TargetingUtility.IsTargetInRange(context, targetInfo, out currentErrorMessage);

        // TODO: There's no preview path to pass because we disabled Pursuit for self targeting
        // Later we can allow Pursuit to be a quick macro: move then target self
        // Since we're passing null the function will either check the PINNED MoveStep or bypass the FLUID Pursuit branch strait to OFF
       UpdateGhostVisuals(context, null);

        // Unified Hub Call. We pass the active actor as the snapped ID.
        TargetingUtility.UpdateTargetVisuals(context, context.CurrentCursorPosition, isValid, null, context.ActiveActorId);
    }

    private void UpdateGhostVisuals(PlayerTurnController context, List<SimVector3> pursuitPath = null)
    {
        // 1. Check if this is Phase 2 (means we 99% have MoveStep first)
        bool hasMove = false;
        SimVector3 pinnedPosition = default;

        foreach (var step in context.Builder.Steps)
        {
            if (step is MoveStep moveStep)
            {
                hasMove = true;
                pinnedPosition = moveStep.Destination;
                break;
            }
        }

        if (hasMove)
        {
            // PINNED: The move location is already set. Lock the preview there
            context.UpdateGhostPreview(true, pinnedPosition);
        }
        else if (context.PursuitEnabled && pursuitPath != null && pursuitPath.Count > 0)
        {
            // FLUID: Phase 1 Pursuit. Preview is locked to the last valid point on path
            SimVector3 lastValidPoint = pursuitPath[pursuitPath.Count - 1];
            context.UpdateGhostPreview(true, lastValidPoint);
        }
        else
        {
            // GHOST OFF: Phase 1 targeting w/ Pursuit off
            context.UpdateGhostPreview(false);
        }
    }
    
    private void DisableTargetVisuals(PlayerTurnController context)
    {
        context.Simulation.Events.Publish(new CursorMovedEvent(new SimVector3(), false));
        context.Simulation.Events.Publish(new TargetingFocusChangedEvent(null));
        context.Simulation.Events.Publish(new TargetingImpactsChangedEvent(null));
        context.UpdateGhostPreview(false);
    }

    private void TryConfirmCommand(PlayerTurnController context)
    {
        DisableTargetVisuals(context);
                    
        context.Builder.AddStep(new AbilityStep(context.ActiveActorId.Value, context.SelectedAbility, targetInfo));

        // Standard Command sequence flow
        if (context.Builder.Size >= 2)
        {
            context.SubmitCommand();
        }
        else
        {
            context.ChangeState(new RootMenuPhase2State());
        }
    }

    public void Exit(PlayerTurnController context)
    {
        DisableTargetVisuals(context);
    }
}