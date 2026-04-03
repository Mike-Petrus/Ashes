public class StatusEffectSystem : IBattleSystem
{
    private BattleEventBus events;
    private ActorRegistry actors;
    private BattleClock clock;

    public StatusEffectSystem(BattleEventBus eventBus, ActorRegistry actorRegistry, BattleClock battleClock)
    {
        events = eventBus;
        actors = actorRegistry;
        clock = battleClock;
    }

    public void Update(float deltaTime)
    {
        if (!clock.IsRunning)
        {
            return;
        }

        foreach (var actor in actors.GetAllActors())
        {
            if (!actor.IsAlive)
            {
                continue;
            }

            for (int i = actor.ActiveStatuses.Count - 1; i >= 0; i--)
            {
                var status = actor.ActiveStatuses[i];

                status.DurationLeft -= clock.BattleDelta;

                if (status.TickInterval > 0)
                {
                    status.TimeSinceLastTick += clock.BattleDelta;

                    if (status.TimeSinceLastTick >= status.TickInterval)
                    {
                        status.TimeSinceLastTick -= status.TickInterval;

                        var tickContext = new EffectContext(status.SourceId, actor.Id);
                        events.Publish(new EffectTickRequestEvent(tickContext, status.TickPayload, status.Name));
                    }
                }

                if (status.DurationLeft <= 0)
                {
                    actor.ActiveStatuses.RemoveAt(i);
                    events.Publish(new StatusExpiredEvent(actor.Id, status.Name));
                }
            }
        }
    }
}