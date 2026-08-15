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
        UpdateCursorVisuals(context);
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

                break;

            case InputButton.Cancel:
                // Hide cursor and rewind
                context.Simulation.Events.Publish(new CursorMovedEvent(new SimVector3(), false));
                context.RevertToPreviousState();

                break;

            case InputButton.Pursuit:
                // TODO: probably shouldn't be able to toggle Pursuit in this state, but if we want to, just uncomment
                // context.TogglePursuit();
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
        UpdateCursorVisuals(context);
    }

    private void ValidateCurrentPosition(PlayerTurnController context)
    {
        isTargetingValidSpace = TargetingUtility.TryValidateStandardMove(context, context.CurrentCursorPosition, out currentPath, out currentErrorMessage);
    }

    private void UpdateCursorVisuals(PlayerTurnController context)
    {
        TargetingUtility.UpdateCursorVisuals(context, context.CurrentCursorPosition, isTargetingValidSpace, currentPath);
    }

    public void Exit(PlayerTurnController context)
    {
        // Safety to ensure cursor turns off
        context.Simulation.Events.Publish(new CursorMovedEvent(new SimVector3(), false));
    }
}