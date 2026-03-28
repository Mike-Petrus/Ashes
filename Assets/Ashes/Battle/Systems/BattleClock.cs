public class BattleClock : IBattleSystem
{
    private BattleEventBus events;

    public bool IsRunning = true;
    public float TimeScale = 1f;

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

        float battleDelta = deltaTime * TimeScale;

        events.Publish(new BattleTickEvent(battleDelta));
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