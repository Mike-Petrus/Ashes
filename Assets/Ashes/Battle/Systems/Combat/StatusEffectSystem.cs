using System.Collections.Generic;

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
        if (!clock.IsRunning) return;

        foreach (var actor in actors.GetAllActors())
        {
            if (!actor.IsAlive) continue;

            // Iterate backwards when removing from a list!
            for (int i = actor.ActiveStatuses.Count - 1; i >= 0; i--)
            {
                var status = actor.ActiveStatuses[i];
                status.DurationLeft -= clock.BattleDelta;

                // --- TICK LOGIC (Poison, Regen) ---
                if (status.TickInterval > 0)
                {
                    status.TimeSinceLastTick += clock.BattleDelta;
                    
                    if (status.TimeSinceLastTick >= status.TickInterval)
                    {
                        status.TimeSinceLastTick -= status.TickInterval;

                        // Dynamically generate the payload using the Power we saved
                        List<Effect> tickPayload = new List<Effect>();
                        
                        if (status.TickType == StatusTickType.Damage)
                        {
                            tickPayload.Add(new DamageEffect(status.Power));
                        }
                        else if (status.TickType == StatusTickType.Heal)
                        {
                            tickPayload.Add(new HealEffect(status.Power));
                        }

                        if (tickPayload.Count > 0)
                        {
                            var tickContext = new EffectContext(status.SourceId, actor.Id);
                            events.Publish(new EffectTickRequestEvent(tickContext, tickPayload, status.Name));
                        }
                    }
                }
                
                // --- EXPIRATION LOGIC ---
                if (status.DurationLeft <= 0)
                {
                    // Clean up the modifiers (Haste/Slow) dynamically
                    actor.Stats.RemoveModifiersFromSource(status);
                    
                    actor.ActiveStatuses.RemoveAt(i);
                    events.Publish(new StatusExpiredEvent(actor.Id, status.Name));
                }
            }
        }
    }
}