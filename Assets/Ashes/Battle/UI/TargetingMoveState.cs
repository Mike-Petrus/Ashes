using System.Collections.Generic;

public class TargetingMoveState : IInputState
{
    // Cached Data
    private SimVector3? savedCursorPosition = null;

    private bool isTargetingValidSpace = true;
    private List<SimVector3> currentPath = new();
    private string currentErrorMessage = "";

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

        UpdateTargetVisuals(context);
    }

    public void ProcessInput(PlayerTurnController context, InputButton button)
    {
        switch (button)
        {
            case InputButton.Confirm:
                if (!isTargetingValidSpace)
                {
                    context.Simulation.Events.Publish(new PlayerFeedbackEvent(currentErrorMessage));
                    // Play error sound
                    return;
                }

                TryConfirmCommand(context);

                break;

            case InputButton.Cancel:
                // Hide cursor and rewind
                DisableTargetVisuals(context);
                context.RevertToPreviousState();

                break;

            case InputButton.Pursuit:
                // TODO: probably shouldn't be able to toggle Pursuit in this state, but if we want to, just uncomment
                // context.TogglePursuit();
                break;
        }
    }

    public void ProcessAnalogLeft(PlayerTurnController context, float x, float y, float deltaTime)
    {
        // 1. Slide the cursor
        SimVector3 pos = context.CurrentCursorPosition;
        pos.x += x * context.CursorSpeed * deltaTime;
        pos.z += y * context.CursorSpeed * deltaTime;


        // 2. HARD CLAMP: Tether to Arena Radius + Actor Move Distance          // TODO: DECIDE IF WE WANT HARD CLAMP
        // if (context.Simulation.Arena != null)
        // {
        //     var activeActor = context.Simulation.Actors.GetActor(context.ActiveActorId.Value);
            
        //     // Movement tether is max move range from the actor's start, NOT the arena center!
        //     // But we must ALSO constrain them to the arena.
        //     float moveDist = activeActor.Stats.MoveDistance;
            
        //     // Constrain to Actor's Move Bubble
        //     float distFromActor = SimVector3.Distance(pos, activeActor.Position);
        //     if (distFromActor > moveDist)
        //     {
        //         SimVector3 dir = (pos - activeActor.Position).Normalized();
        //         pos = activeActor.Position + (dir * moveDist);
        //     }

        //     // Constrain to Arena
        //     float distFromCenter = SimVector3.Distance(pos, context.Simulation.Arena.Center);
        //     if (distFromCenter > context.Simulation.Arena.Radius)
        //     {
        //         SimVector3 dir = (pos - context.Simulation.Arena.Center).Normalized();
        //         pos = context.Simulation.Arena.Center + (dir * context.Simulation.Arena.Radius);
        //     }
        // }



        context.CurrentCursorPosition = pos;
        // Cache cursor position for rewinding
        savedCursorPosition = context.CurrentCursorPosition;

        UpdateTargetVisuals(context);
    }

    private void UpdateTargetVisuals(PlayerTurnController context)
    {
        // Validate standard move (generates the strict path)
        isTargetingValidSpace = TargetingUtility.TryValidateStandardMove(context, context.CurrentCursorPosition, out currentPath, out currentErrorMessage);

        // Unified Hub Call. 
        // Note: Because SelectedAbility is null, the Utility handles this safely and draws a point radius.
        TargetingUtility.UpdateTargetVisuals(context, context.CurrentCursorPosition, isTargetingValidSpace, currentPath, null);
    }

    private void DisableTargetVisuals(PlayerTurnController context)
    {
        context.Simulation.Events.Publish(new CursorMovedEvent(new SimVector3(), false));
        context.Simulation.Events.Publish(new TargetingFocusChangedEvent(null));
        context.Simulation.Events.Publish(new TargetingImpactsChangedEvent(null));
    }

    private void TryConfirmCommand(PlayerTurnController context)
    {
        DisableTargetVisuals(context);

        context.Builder.AddStep(new MoveStep(context.ActiveActorId.Value, context.CurrentCursorPosition, currentPath));

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