using System;
using System.Collections.Generic;
using System.Linq;


public class TargetingActorState : IInputState
{
    private List<ActorId> currentAvailableTargets = new();
    private int currentTargetIndex = 0;

    // Cached Data
    private ActorId? savedTargetId = null;
    
    private bool isTargetingValidActor = true;
    private List<SimVector3> currentPreviewPath = null;
    private string currentErrorMessage = "";

    // Cached context for event listening
    private PlayerTurnController currentContext;

    private float magneticRadius = 0.5f;

    public void Enter(PlayerTurnController context)
    {
        currentContext = context;

        currentAvailableTargets.Clear();
        context.ToggleFreeAim(false);

        var activeActor = context.Simulation.Actors.GetActor(context.ActiveActorId.Value);
        var abilityAlignment = context.SelectedAbility.Alignment;

        List<ActorId> aliveActors = context.Simulation.Actors.GetAliveActorIds().ToList();
        currentAvailableTargets = context.Simulation.TargetingSystem.FilterByAlignment(activeActor.Id, aliveActors, abilityAlignment);

        // Cursor Memory
        if (savedTargetId.HasValue)
        {
            currentTargetIndex = currentAvailableTargets.IndexOf(savedTargetId.Value);
        }
        else
        {
            currentTargetIndex = -1;
        }

        ProcessSnapToTarget(context, true);

        // Listen for actors moving while targeting
        context.Simulation.Events.Subscribe<ActorMovedEvent>(OnActorMoved);
    }

    public void ProcessInput(PlayerTurnController context, InputButton button)
    {
        int listSize = currentAvailableTargets.Count;

        switch (button)
        {
            case InputButton.Right:
                if (listSize > 0)
                {
                    currentTargetIndex++;

                    if (currentTargetIndex >= listSize)
                    {
                        currentTargetIndex = 0;
                    }
                    ProcessSnapToTarget(context);
                }
                break;

            case InputButton.Left:
                if (listSize > 0)
                {
                    currentTargetIndex--;

                    if (currentTargetIndex < 0)
                    {
                        currentTargetIndex = listSize - 1;
                    }
                    ProcessSnapToTarget(context);
                }
                break;

            // Can combine Right/Down, Left/Up if wanted, but for now they do nothing
            case InputButton.Up:
            case InputButton.Down:
                break;

            case InputButton.Confirm:
                // Block confirm if not hybrid/directional
                if (currentTargetIndex < 0 && context.SelectedAbility.Mode != TargetingMode.HybridAoE && context.SelectedAbility.Mode != TargetingMode.Directional)
                {
                    context.Simulation.Events.Publish(new PlayerFeedbackEvent("Select a valid target!"));
                    return;
                }

                if (!isTargetingValidActor)
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
                if (!context.PursuitEnabled && context.SelectedAbility is DummyAbility dummy && dummy.DummyId == "system_follow")
                {
                    DisableTargetVisuals(context);

                    context.Simulation.Events.Publish(new PlayerFeedbackEvent("Switched to Move")); // DEBUG

                    context.ChangeState(new TargetingMoveState(), false);
                    
                    return;
                }

                UpdateTargetVisuals(context);

                break;

            case InputButton.FreeAim:
                bool canTargetFree =  context.SelectedAbility.Mode != TargetingMode.Self && 
                                      context.SelectedAbility.Mode != TargetingMode.SingleTarget && 
                                      context.SelectedAbility.Mode != TargetingMode.ActorAoE;

                if (canTargetFree)
                {
                    context.ToggleFreeAim(true);
                    context.ChangeState(new TargetingFreeAimState(), false);
                }
                else
                {
                    // Play error sound
                    // This event may not be necessary, but for testing it is useful. Consider removing later
                    context.Simulation.Events.Publish(new PlayerFeedbackEvent($"Cannot Free Aim with {context.SelectedAbility.Name}!"));
                }
                break;
        }
    }

    public void ProcessAnalogLeft(PlayerTurnController context, float x, float y, float deltaTime)
    {
        float inputMagnitude = (float)Math.Sqrt((x * x) + (y * y));

        if (inputMagnitude < 0.1f)      // low magnitude stays snapped if on a target
        {
            if (currentTargetIndex >= 0 && currentAvailableTargets.Count > 0)
            {
                context.CurrentCursorPosition = context.Simulation.Actors.GetActor(currentAvailableTargets[currentTargetIndex]).Position;
            }
            return;
        }

        // 1. Move the cursor
        SimVector3 pos = context.CurrentCursorPosition;
        pos.x += x * context.CursorSpeed * deltaTime;
        pos.z += y * context.CursorSpeed * deltaTime;
        
        // HARD CLAMP: Tether to Arena Radius + Ability Radius
        if (context.Simulation.Arena != null)
        {
            float maxDistance = context.Simulation.Arena.Radius + context.SelectedAbility.Radius - 0.05f;
            
            float dx = pos.x - context.Simulation.Arena.Center.x;
            float dz = pos.z - context.Simulation.Arena.Center.z;
            float dist = (float)Math.Sqrt((dx * dx) + (dz * dz));

            if (dist > maxDistance)
            {
                // Normalize and project out to the max tether limit
                float dirX = dx / dist;
                float dirZ = dz / dist;

                pos.x = context.Simulation.Arena.Center.x + (dirX * maxDistance);
                pos.z = context.Simulation.Arena.Center.z + (dirZ * maxDistance);
            }
        }

        context.CurrentCursorPosition = pos;

        // 2. Find the closest valid target to the cursor
        int bestIndex = -1;
        float closestDist = float.MaxValue;

        for (int i = 0; i < currentAvailableTargets.Count; i++)
        {
            var candidate = context.Simulation.Actors.GetActor(currentAvailableTargets[i]);
            float dist = SimVector3.Distance(pos, candidate.Position);

            if (dist < closestDist)
            {
                closestDist = dist;
                bestIndex = i;
            }
        }

        // 3. Evaluate magnetism
        if (bestIndex != -1 && closestDist <= magneticRadius)
        {
            currentTargetIndex = bestIndex;
        }
        else
        {
            currentTargetIndex = -1;
        }

        UpdateTargetVisuals(context);
    }

    private void UpdateTargetVisuals(PlayerTurnController context)
    {
        // 1. Maintain the original internal state
        savedTargetId = (currentTargetIndex >= 0 && currentAvailableTargets.Count > 0) 
            ? currentAvailableTargets[currentTargetIndex] 
            : (ActorId?)null;

        SimVector3 projectedCenter = context.CurrentCursorPosition;

        // 2. Build semantic targetInfo based only on snap data
        TargetInfo targetInfo = savedTargetId.HasValue
            ? TargetInfo.ForActor(savedTargetId.Value, context.SelectedAbility.Mode)
            : TargetInfo.ForPosition(projectedCenter, context.SelectedAbility.Mode);

        // 3. Perform Pure C# Validation
        if (context.PursuitEnabled)
        {
            currentPreviewPath = TargetingUtility.GeneratePursuitPreview(context, targetInfo, context.SelectedAbility);
            isTargetingValidActor = true;
            currentErrorMessage = "";
        }
        else
        {
            currentPreviewPath = null;
            isTargetingValidActor = TargetingUtility.IsTargetInRange(context, targetInfo, out currentErrorMessage);
        }

        // Unified Visual Update Call
        TargetingUtility.UpdateTargetVisuals(context, projectedCenter, isTargetingValidActor, currentPreviewPath, savedTargetId);
    }

    private void DisableTargetVisuals(PlayerTurnController context)
    {
        context.Simulation.Events.Publish(new CursorMovedEvent(new SimVector3(), false));
        context.Simulation.Events.Publish(new TargetingFocusChangedEvent(null));
        context.Simulation.Events.Publish(new TargetingImpactsChangedEvent(null));
    }

    private void TryConfirmCommand(PlayerTurnController context)
    {
        TargetInfo targetInfo;

        if (currentTargetIndex >= 0 && currentAvailableTargets.Count > 0)
        {
            ActorId selectedTarget = currentAvailableTargets[currentTargetIndex];
            targetInfo = TargetInfo.ForActor(selectedTarget, context.SelectedAbility.Mode);
        }
        else
        {
            targetInfo = TargetInfo.ForPosition(context.CurrentCursorPosition, context.SelectedAbility.Mode);
        }

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
    }

    private void ProcessSnapToTarget(PlayerTurnController context, bool initialSnap = false)
    {
        if (currentAvailableTargets.Count == 0)
        {
            currentTargetIndex = -1;
            UpdateTargetVisuals(context);
            
            return;
        }

        if (currentTargetIndex < 0 && !initialSnap)
        {
            currentTargetIndex = 0;
        }

        if (currentTargetIndex >= 0)
        {   
            // Snapped ID
            ActorId snapId = currentAvailableTargets[currentTargetIndex];
            context.CurrentCursorPosition = context.Simulation.Actors.GetActor(snapId).Position;
        }

        // centralized focus publish
        UpdateTargetVisuals(context);
    }

    private void OnActorMoved(ActorMovedEvent e)
    {
        if (currentContext == null || !savedTargetId.HasValue)
        {
            return;
        }

        if (e.ActorId == savedTargetId.Value || e.ActorId == currentContext.ActiveActorId.Value)
        {
            if (currentTargetIndex >= 0)
            {
                currentContext.CurrentCursorPosition = currentContext.Simulation.Actors.GetActor(savedTargetId.Value).Position;
            }

            UpdateTargetVisuals(currentContext);
        }
    }

    public void Exit(PlayerTurnController context)
    {
        context.Simulation.Events.Unsubscribe<ActorMovedEvent>(OnActorMoved);

        DisableTargetVisuals(context);
        // Do NOT clear CurrentAvailableTargets here so that if we come back,
        // the memory index doesn't temporarily throw an out of bounds error

        currentContext = null;
    }
}