public class ActorStateSystem
{
    private EventBus events;
    private ActorRegistry actors;

    public ActorStateSystem(EventBus eventBus, ActorRegistry actorRegistry)
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