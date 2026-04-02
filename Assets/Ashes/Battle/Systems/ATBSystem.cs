using System;

public class ATBSystem
{
    private BattleEventBus events;
    private ActorRegistry actors;

    public ATBSystem(BattleEventBus eventBus, ActorRegistry actorList)
    {
        events = eventBus;
        actors = actorList;

        events.Subscribe<BattleTickEvent>(OnBattleTick);
        events.Subscribe<ATBChangeRequestEvent>(OnATBChangeRequest);
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

    void OnATBChangeRequest(ATBChangeRequestEvent request)
    {
        var modifiedActor = actors.GetActor(request.ActorId);

        if (modifiedActor == null || !modifiedActor.IsAlive)
        {
            return;
        }

        float ATBChangeValue = modifiedActor.MaxATB * request.RequestPercent;

        if (request.IsNegative == true)
        {
            modifiedActor.ATB = Math.Max(0, modifiedActor.ATB - ATBChangeValue);
        }
        else
        {
            modifiedActor.ATB = Math.Min(modifiedActor.MaxATB, modifiedActor.ATB + ATBChangeValue);
        }
    }
}