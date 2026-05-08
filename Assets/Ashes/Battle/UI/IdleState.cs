public class IdleState : IInputState
{
    public void Enter(PlayerTurnController context)
    {
        // Do nothing
    }

    public void ProcessInput(PlayerTurnController context, InputButton button)
    {
        switch (button)
        {
            case InputButton.Confirm:
                context.ChangeState(new PartySelectionState());
                break;

            case InputButton.Pursuit:
                context.PursuitEnabled = !context.PursuitEnabled;
                break;
        }
    }

    public void ProcessAnalogInput(PlayerTurnController context, float x, float y, float deltaTime)
    {
        // TODO: Free-look camera
    }

    public void Exit(PlayerTurnController context)
    {
        
    }
}