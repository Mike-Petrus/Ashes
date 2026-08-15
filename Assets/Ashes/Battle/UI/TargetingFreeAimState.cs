using System;
using System.Collections.Generic;
using System.Linq;

public class TargetingFreeAimState : IInputState
{
    // Cached Data
    private SimVector3? savedCursorPosition = null;

    private bool isValidPosition = true;
    private string currentErrorMessage = "";
    private List<SimVector3> currentPreviewPath = null;

    public void Enter(PlayerTurnController context)
    {
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

                if (context.PursuitEnabled)
                {
                    // 1. Determine the final point of the calculated preview path
                    SimVector3 destination = currentPreviewPath != null && currentPreviewPath.Count > 0 ? currentPreviewPath.Last() : TargetingUtility.GetOriginPosition(context);

                    // 2. Only generate a MoveStep if we actually have to move to reach it
                    if (SimVector3.Distance(TargetingUtility.GetOriginPosition(context), destination) > 0.1f)
                    {
                        context.Builder.AddStep(new MoveStep(context.ActiveActorId.Value, destination, currentPreviewPath));
                    }

                    // 3. Add the Ability and Submit 
                    context.Builder.AddStep(new AbilityStep(context.ActiveActorId.Value, context.SelectedAbility, targetInfo));
                    context.SubmitCommand();
                }
                else
                {
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
                break;

            case InputButton.Cancel:
                context.Simulation.Events.Publish(new CursorMovedEvent(new SimVector3(), false));
                context.RevertToPreviousState();
                break;

            case InputButton.Pursuit:
                // TODO: Should probably be able to toggle here as long as in Phase 1
                context.TogglePursuit();
                ValidateCursorPosition(context);
                UpdateCursorVisuals(context);
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

    public void ProcessAnalogInput(PlayerTurnController context, float x, float y, float deltaTime)
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

        // Cache cursor position for rewinding
        savedCursorPosition = context.CurrentCursorPosition;

        ValidateCursorPosition(context);
        UpdateCursorVisuals(context);
    }

    private void UpdateCursorVisuals(PlayerTurnController context)
    {
        TargetingUtility.UpdateCursorVisuals(context, context.CurrentCursorPosition, isValidPosition, currentPreviewPath);
    }

    private void ValidateCursorPosition(PlayerTurnController context)
    {
        isValidPosition = true;
        currentErrorMessage = "";
        currentPreviewPath = null;

        var targetInfo = TargetInfo.ForPosition(context.CurrentCursorPosition, context.SelectedAbility.Mode);

        // Pursuit Validation & Preview Path
        if (context.PursuitEnabled)
        {
            currentPreviewPath = TargetingUtility.GeneratePursuitPreview(context, targetInfo, context.SelectedAbility);
            isValidPosition = true;
        }
        else
        {
            // Standard Strict Path Validation
            isValidPosition = TargetingUtility.IsTargetInRange(context, targetInfo, out currentErrorMessage);
        }
    }

    public void Exit(PlayerTurnController context)
    {
        context.Simulation.Events.Publish(new CursorMovedEvent(new SimVector3(), false));
    }
}