public interface IInputState
{
    // Called once when the stte becomes active
    void Enter(PlayerTurnController context);

    // Discrete menu navigation and button taps
    void ProcessInput(PlayerTurnController context, InputButton button);

    // Continuous analog movement
    void ProcessAnalogInput(PlayerTurnController context, float x, float y, float deltaTime);

    // Called right before transitioning to a different state
    void Exit(PlayerTurnController context);
}