public class ActorRegisteredEvent : IBattleEvent
{
    public BattleActor Actor { get; }

    public ActorRegisteredEvent(BattleActor actor)
    {
        Actor = actor;
    }
}