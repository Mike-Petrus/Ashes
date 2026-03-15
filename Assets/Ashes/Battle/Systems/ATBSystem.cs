public class ATBSystem
{
    private EventBus events;
    private ActorRegistry actors;

    public ATBSystem(EventBus eventBus, ActorRegistry actorList)
    {
        events = eventBus;
        actors = actorList;

        events.Subscribe<BattleTickEvent>(OnBattleTick);
    }

    void OnBattleTick(BattleTickEvent tick)
    {
        foreach (var actorId in actors.GetAllActorIds())
        {
            var actor = actors.GetActor(actorId);

            if (actor.IsReady)
            {
                continue;
            }

            actor.ATB += actor.Speed * tick.DeltaTime;

            events.Publish(new ATBChangedEvent(actorId, actor.ATB));

            if (actor.ATB >= actor.MaxATB)
            {
                actor.ATB = actor.MaxATB;

                events.Publish(new ActorReadyEvent(actorId));
            }
        }
    }
}