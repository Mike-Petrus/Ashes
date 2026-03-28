public class ActorStateSystem
{
    private BattleEventBus events;
    private ActorRegistry actors;

    public ActorStateSystem(BattleEventBus eventBus, ActorRegistry actorRegistry)
    {
        events = eventBus;
        actors = actorRegistry;
    }

    public void SetState(ActorId actorId, ActorState newState)
    {
        var actor = actors.GetActor(actorId);

        if (actor.State != newState)
        {
            actor.State = newState;

            events.Publish(new ActorStateChangedEvent(actorId, newState));
        }
    }
}