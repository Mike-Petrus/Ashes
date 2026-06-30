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
    private string currentErrorMessage = "";

    // Cached context for event listening
    private PlayerTurnController currentContext;

    private float magneticRadius = 0.5f;

    public void Enter(PlayerTurnController context)
    {
        currentContext = context;
        currentAvailableTargets.Clear();

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

        ForceSnapToTarget(context);

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
                    ForceSnapToTarget(context);
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
                    ForceSnapToTarget(context);
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
                context.PursuitEnabled = !context.PursuitEnabled;
                break;

            case InputButton.FreeAim:
                bool canTargetFree =  context.SelectedAbility.Mode != TargetingMode.Self && 
                                      context.SelectedAbility.Mode != TargetingMode.SingleTarget && 
                                      context.SelectedAbility.Mode != TargetingMode.ActorAoE;

                if (canTargetFree)
                {
                    context.FreeAimEnabled = true;
                    context.Simulation.Events.Publish(new PlayerFeedbackEvent($"Switching to Free Aim"));
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

    public void ProcessAnalogInput(PlayerTurnController context, float x, float y, float deltaTime)
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
            savedTargetId = currentAvailableTargets[currentTargetIndex];
        }
        else
        {
            currentTargetIndex = -1;
        }

        ValidateCurrentTarget(context);
        UpdateCursorVisuals(context);
    }

    private void ValidateCurrentTarget(PlayerTurnController context)
    {
        isTargetingValidActor = true;
        currentErrorMessage = "";

        if (currentTargetIndex < 0 || currentAvailableTargets.Count == 0)
        {
            if (context.SelectedAbility.Mode != TargetingMode.HybridAoE && context.SelectedAbility.Mode != TargetingMode.Directional)
            {
                isTargetingValidActor = false;
                currentErrorMessage = "No target selected!";
                return;
            }
        }

        var activeActor = context.Simulation.Actors.GetActor(context.ActiveActorId.Value);

        SimVector3 originPosition = activeActor.Position;

        // If we moved in Phase 1, we must calculate range from the FUTURE position
        if (context.Builder.Size > 0 && context.Builder.LastStepAdded() is MoveStep moveStep)
        {
            originPosition = moveStep.Destination;
        }

        TargetInfo targetInfo;

        // Dynamically validate against the actor OR floor position
        if (currentTargetIndex >= 0 && currentAvailableTargets.Count > 0)
        {
            ActorId selectedTarget = currentAvailableTargets[currentTargetIndex];
            targetInfo = TargetInfo.ForActor(selectedTarget, context.SelectedAbility.Mode);
        }
        else
        {
            targetInfo = TargetInfo.ForPosition(context.CurrentCursorPosition, context.SelectedAbility.Mode);
        }

        if (!context.Simulation.RangeSystem.IsInRange(originPosition, activeActor.Radius, context.SelectedAbility, targetInfo))
        {
            isTargetingValidActor = false;
            currentErrorMessage = "Out of Range!";
        }
    }

    private void UpdateCursorVisuals(PlayerTurnController context)
    {
        var ability = context.SelectedAbility;
        SimVector3 displayPosition = context.CurrentCursorPosition;

        if (currentTargetIndex >= 0 && currentAvailableTargets.Count > 0)
        {
            displayPosition = context.Simulation.Actors.GetActor(currentAvailableTargets[currentTargetIndex]).Position;
        }

        // Pass the boolean into the event so the cursor changes color
        context.Simulation.Events.Publish(new CursorMovedEvent(displayPosition, true, isTargetingValidActor, ability.Mode, ability.Radius, ability.Angle));
    }

    private void ForceSnapToTarget(PlayerTurnController context)
    {
        if (currentAvailableTargets.Count == 0)
        {
            ValidateCurrentTarget(context);
            UpdateCursorVisuals(context);
            return;
        }

        if (currentTargetIndex < 0)
        {
            currentTargetIndex = 0;
        }

        savedTargetId = currentAvailableTargets[currentTargetIndex];
        context.CurrentCursorPosition = context.Simulation.Actors.GetActor(savedTargetId.Value).Position;

        ValidateCurrentTarget(context);
        UpdateCursorVisuals(context);
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

            ValidateCurrentTarget(currentContext);
            UpdateCursorVisuals(currentContext);
        }
    }

    public void Exit(PlayerTurnController context)
    {
        context.Simulation.Events.Unsubscribe<ActorMovedEvent>(OnActorMoved);
        context.Simulation.Events.Publish(new CursorMovedEvent(new SimVector3(), false));
        // Do NOT clear CurrentAvailableTargets here so that if we come back,
        // the memory index doesn't temporarily throw an out of bounds error

        currentContext = null;
    }
}