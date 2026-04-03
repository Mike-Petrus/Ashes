using System;

public class ATBSystem : IBattleSystem
{
    private BattleEventBus events;
    private ActorRegistry actors;
    private BattleClock clock;

    public ATBSystem(BattleEventBus eventBus, ActorRegistry actorList, BattleClock battleClock)
    {
        events = eventBus;
        actors = actorList;
        clock = battleClock;

        events.Subscribe<ATBChangeRequestEvent>(OnATBChangeRequest);
    }
    
    public void Update(float deltaTime)
    {
        if (!clock.IsRunning)
        {
            return;
        }

        foreach (var actorId in actors.GetAllActorIds())
        {
            var actor = actors.GetActor(actorId);

            if (actor.IsReady || !actor.IsAlive)
            {
                continue;
            }

            actor.ATB += actor.Speed * clock.BattleDelta;
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