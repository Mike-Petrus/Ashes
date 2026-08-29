using System;
using System.Collections.Generic;
using System.Linq;

public class TargetingFreeAimState : IInputState
{
    // Cached Data
    private SimVector3? savedCursorPosition = null;
    
    private bool isTargetingValidPosition = true;
    private List<SimVector3> currentPreviewPath = null;
    private string currentErrorMessage = "";

    public void Enter(PlayerTurnController context)
    {
        context.ToggleFreeAim(true);

        if (!savedCursorPosition.HasValue)
        {
            var activeActor = context.Simulation.Actors.GetActor(context.ActiveActorId.Value);
            context.CurrentCursorPosition = activeActor.Position;
        }
        else
        {
            context.CurrentCursorPosition = savedCursorPosition.Value;
        }

        UpdateTargetVisuals(context);
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

                if (!isValidMode || !isTargetingValidPosition)
                {
                    context.Simulation.Events.Publish(new PlayerFeedbackEvent(currentErrorMessage));
                    // Play error sound
                    return;
                }    

                TryConfirmCommand(context);

                break;

            case InputButton.Cancel:
                DisableTargetVisuals(context);
                context.RevertToPreviousState();

                break;

            case InputButton.Pursuit:
                // TODO: Should probably be able to toggle here as long as in Phase 1
                context.TogglePursuit();
                
                // If they turn Pursuit off while using the follow dummy, go to TargetingMoveState
                if (!context.PursuitEnabled && context.SelectedAbility is DummyAbility dummy && dummy.AbilityId == "system_follow")
                {
                    DisableTargetVisuals(context);

                    context.Simulation.Events.Publish(new PlayerFeedbackEvent("Switched to Move")); // DEBUG

                    context.ChangeState(new TargetingMoveState(), false);
                    
                    return;
                }

                UpdateTargetVisuals(context);
                
                break;

            case InputButton.FreeAim:
                bool canTargetActor = context.SelectedAbility.Mode != TargetingMode.Self && context.SelectedAbility.Mode != TargetingMode.PointAoE;

                if (canTargetActor)
                {
                    context.ToggleFreeAim(false);
                    context.ChangeState(new TargetingActorState(), false);
                }
                else
                {
                    // Play error sound
                    // This event may not be necessary, but for testing it is useful. Consider removing later
                    context.Simulation.Events.Publish(new PlayerFeedbackEvent($"Cannot snap to target with {context.SelectedAbility.Name}!"));
                }
                break;
        }
    }

    public void ProcessAnalogLeft(PlayerTurnController context, float x, float y, float deltaTime)
    {
        float inputMagnitude = (float)Math.Sqrt((x * x) + (y * y));

        // Directional pointing. Snaps directly to the direction the stick is pointing in
        // TODO: Update to make movement smoother. Make sure decal maps to actual mouse positioning and targeting is accurate
        // TODO: Test TargetingActorState and see if this needs to be duplicated
        if (context.SelectedAbility.Mode == TargetingMode.Directional)
        {
            if (inputMagnitude > 0.1f)
            {
                SimVector3 origin = TargetingUtility.GetOriginPosition(context);

                // Normalize the analog input into a raw direction vector
                SimVector3 stickDirection = new SimVector3(x / inputMagnitude, 0 , y / inputMagnitude);

                // Project the cursor out along that vector
                context.CurrentCursorPosition = origin + (stickDirection * (context.SelectedAbility.Radius - 0.5f));
            }
        }
        else
        {
            // 1. Slide the cursor
            SimVector3 pos = context.CurrentCursorPosition;
            pos.x += x * context.CursorSpeed * deltaTime;
            pos.z += y * context.CursorSpeed * deltaTime;

            // 2. HARD CLAMP: Tether to Arena Radius + Ability Radius
            if (context.Simulation.Arena != null)
            {
                float maxDistance = context.Simulation.Arena.Radius + context.SelectedAbility.Radius - 0.05f;

                float dx = pos.x - context.Simulation.Arena.Center.x;
                float dz = pos.z - context.Simulation.Arena.Center.z;
                float dist = (float)Math.Sqrt((dx * dx) + (dz * dz));

                if (dist > maxDistance)
                {
                    float dirX = dx / dist;
                    float dirZ = dz / dist;

                    pos.x = context.Simulation.Arena.Center.x + (dirX * maxDistance);
                    pos.z = context.Simulation.Arena.Center.z + (dirZ * maxDistance);
                }
            }
            context.CurrentCursorPosition = pos;
        }

        // Cache cursor position for rewinding
        savedCursorPosition = context.CurrentCursorPosition;

        UpdateTargetVisuals(context);
    }

    private void UpdateTargetVisuals(PlayerTurnController context)
    {
        TargetInfo targetInfo = TargetInfo.ForPosition(context.CurrentCursorPosition, context.SelectedAbility.Mode);

        if (context.PursuitEnabled)
        {
            currentPreviewPath = TargetingUtility.GeneratePursuitPreview(context, targetInfo, context.SelectedAbility);
            isTargetingValidPosition = true;
            currentErrorMessage = "";
        }
        else
        {
            currentPreviewPath = null;
            isTargetingValidPosition = TargetingUtility.IsTargetInRange(context, targetInfo, out currentErrorMessage);
        }

        UpdateGhostVisuals(context, currentPreviewPath);

        // Call the unified hub. snappedTargetId is NULL because this is free aim
        TargetingUtility.UpdateTargetVisuals(context, context.CurrentCursorPosition, isTargetingValidPosition, currentPreviewPath, null);
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
        TargetInfo targetInfo = TargetInfo.ForPosition(context.CurrentCursorPosition, context.SelectedAbility.Mode);

        DisableTargetVisuals(context);

        if (context.PursuitEnabled)
        {
            // 1. Determine the final point of the calculated preview path
            SimVector3 destination = currentPreviewPath != null && currentPreviewPath.Count > 0 ? currentPreviewPath.Last() : TargetingUtility.GetOriginPosition(context);

            // 2. Only generate a MoveStep if we actually have to move to reach it
            if (SimVector3.Distance(TargetingUtility.GetOriginPosition(context), destination) > 0.1f)
            {
                context.Builder.AddStep(new MoveStep(context.ActiveActorId.Value, destination, currentPreviewPath));
            }
        }

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
    }

    public void Exit(PlayerTurnController context)
    {
        DisableTargetVisuals(context);
    }
}