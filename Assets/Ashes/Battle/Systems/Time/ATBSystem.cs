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
        events.Subscribe<CommandStartedEvent>(OnCommandStarted);
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

    private void OnCommandStarted(CommandStartedEvent e)
    {
        var actor = actors.GetActor(e.Command.ActorId);

        if (actor == null || !actor.IsAlive)
        {
            return;
        }

        actor.ATB = 0f;
        events.Publish(new ATBChangedEvent(actor.Id, actor.ATB));
    }

    private void OnATBChangeRequest(ATBChangeRequestEvent request)
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

        // 1. Force the UI to update immediately while the clock is paused!
        events.Publish(new ATBChangedEvent(request.ActorId, modifiedActor.ATB));

        // 2. Tell the WaitStep that the transaction is finished
        events.Publish(new ATBRequestCompletedEvent(request.ActorId, request.RequestPercent, request.IsNegative));
    }
}