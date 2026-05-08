public class PartySelectionState : IInputState
{
    private int currentIndex = 0;

    public void Enter(PlayerTurnController context)
    {
        // Default to the first actor, or whoever we selected previously
        currentIndex = context.PartyActorIds.IndexOf(context.ActiveActorId ?? context.PartyActorIds[0]);
        
        if (currentIndex < 0)
        {
            currentIndex = 0;
        }
    }

    public void ProcessInput(PlayerTurnController context, InputButton button)
    {
        int listSize = context.PartyActorIds.Count;

        if (listSize == 0)
        {
            return;
        }

        switch (button)
        {
            case InputButton.Up:
                currentIndex--;
                
                if (currentIndex < 0 )
                {
                    currentIndex = listSize - 1;
                }
                // tell UI to update highlight
                break;

            case InputButton.Down:
                currentIndex++;

                if (currentIndex >= listSize)
                {
                    currentIndex = 0;
                }
                // tell UI to update highlight
                break;

            case InputButton.Confirm:
                context.ActiveActorId = context.PartyActorIds[currentIndex];
                context.Builder.BeginCommand(context.ActiveActorId.Value);
                context.ChangeState(new RootMenuPhase1State());
                break;

            case InputButton.Cancel:
                context.ChangeState(new IdleState(), false);
                break;

            case InputButton.Pursuit:
                context.PursuitEnabled = !context.PursuitEnabled;   // Safe to toggle
                break;   
        }
    }

    public void ProcessAnalogInput(PlayerTurnController context, float x, float y, float deltaTime) { }
    public void Exit(PlayerTurnController context) { /* close UI */ }
}