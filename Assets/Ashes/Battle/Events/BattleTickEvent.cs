public class BattleTickEvent : IBattleEvent
{
    public float DeltaTime;

    public BattleTickEvent(float deltaTime)
    {
        DeltaTime = deltaTime;
    }
}