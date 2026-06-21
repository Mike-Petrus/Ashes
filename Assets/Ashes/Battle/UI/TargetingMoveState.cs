using System.Collections.Generic;

public class TargetingMoveState : IInputState
{
    // Cached Data
    private BattleActor activeActor;
    private SimVector3? savedCursorPosition = null;

    private List<SimVector3> currentPath = new();
    private bool isTargetingValidSpace = true;
    private string currentErrorMessage = "";

    public void Enter(PlayerTurnController context)
    {
        activeActor = context.Simulation.Actors.GetActor(context.ActiveActorId.Value);

        context.CurrentCursorPosition = savedCursorPosition ?? activeActor.Position;

        ValidateCurrentPosition(context);

        // Tell the View to show the cursor
        context.Simulation.Events.Publish(new CursorMovedEvent(context.CurrentCursorPosition, true, isTargetingValidSpace, path: currentPath));
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

                context.Simulation.Events.Publish(new CursorMovedEvent(new SimVector3(), false));
                context.Builder.AddStep(new MoveStep(context.ActiveActorId.Value, context.CurrentCursorPosition));

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
                // Hide cursor and rewind
                context.Simulation.Events.Publish(new CursorMovedEvent(new SimVector3(), false));
                context.RevertToPreviousState();

                break;

            case InputButton.Pursuit:
                // TODO: probably shouldn't be able to toggle Pursuit in this state, but if we want to, just uncomment
                // context.PursuitEnabled = !context.PursuitEnabled;
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

        // 2. Validate new position
        ValidateCurrentPosition(context);

        // 3. Broadcast to Unity View
        context.Simulation.Events.Publish(new CursorMovedEvent(context.CurrentCursorPosition, true, isTargetingValidSpace, path: currentPath));
    }

    public void Exit(PlayerTurnController context)
    {
        // Safety to ensure cursor turns off
        context.Simulation.Events.Publish(new CursorMovedEvent(new SimVector3(), false));
    }

    private void ValidateCurrentPosition(PlayerTurnController context)
    {
        isTargetingValidSpace = true;
        currentErrorMessage = "";

        // TODO: Display correct error message. E.g. An occupied space never displays the error message
        // because it fails the path check first

        // 1. Calculate NavMeshPath
        currentPath = context.Simulation.Pathfinder.FindPath(activeActor.Position, context.CurrentCursorPosition, activeActor.Radius);

        if (currentPath == null || currentPath.Count == 0)
        {
            isTargetingValidSpace = false;
            currentErrorMessage = "Unreachable!";

            return;
        }

        // 2. Validate True Path Distance
        float pathDistance = 0f;

        for (int i = 0; i < currentPath.Count - 1; i++)
        {
            pathDistance += SimVector3.Distance(currentPath[i], currentPath[i + 1]);
        }

        if (pathDistance > activeActor.Stats.MoveDistance)
        {
            isTargetingValidSpace = false;
            currentErrorMessage = "Too Far!";

            return;
        }

        // 3. Validate Collision
        if (context.Simulation.PositionSystem.IsSpaceOccupied(context.CurrentCursorPosition, activeActor.Radius, context.ActiveActorId.Value))
        {
            isTargetingValidSpace = false;
            currentErrorMessage = "Space Occupied!";

            return;
        }
    }
}