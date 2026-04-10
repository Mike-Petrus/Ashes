public class BattleClock : IBattleSystem
{
    private BattleEventBus events;

    public bool IsRunning = true;
    public float TimeScale = 1f;
    private float battleDelta;

    public float BattleDelta => battleDelta;

    public BattleClock(BattleEventBus eventBus)
    {
        events = eventBus;
    }

    public void Update(float deltaTime)
    {
        if (!IsRunning)
        {
            return;
        }

        battleDelta = deltaTime * TimeScale;
    }

    public void Pause()
    {
        IsRunning = false;
    }

    public void Resume()
    {
        IsRunning = true;
    }
}