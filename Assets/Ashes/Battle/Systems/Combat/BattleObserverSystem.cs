using System.Collections.Generic;
using System.Linq;

public class BattleObserverSystem : IBattleSystem
{
    private BattleContext context;
    private bool isBattleOver = false;

    // TODO: Consider creating WinCondition and LossCondition enum
    // Majority of battles will be decided by killing all enemies/party members
    // But we may want special conditions in the future (e.g. Kill target, survive X turns, etc.)


    // context injected in Bootstrapper after it is created by the BattleSimulation
    public BattleObserverSystem(BattleContext battleContext)
    {
        context = battleContext;
        context.Events.Subscribe<ActorDiedEvent>(OnActorDied);
    }

    private void OnActorDied(ActorDiedEvent e)
    {
        if (isBattleOver)
        {
            return;
        }

        var deadActor = context.Actors.GetActor(e.ActorId);

        if (deadActor.Faction == ActorFaction.Party)
        {
            if (!context.Actors.GetAliveActorsByFaction(ActorFaction.Party).Any())
            {
                isBattleOver = true;
                context.Clock.Pause();
                context.Events.Publish(new BattleEndedEvent(false));
            }
        }

        else if (deadActor.Faction == ActorFaction.Enemy)
        {
            if (!context.Actors.GetAliveActorsByFaction(ActorFaction.Enemy).Any())
            {
                isBattleOver = true;
                context.Clock.Pause();

                // TODO: Loop through dead enemeis and get their specific loot table
                // For now, use dummy loot
                Dictionary<string, int> generatedLoot = new Dictionary<string, int>();
                generatedLoot.Add("Potion", 2);

                context.Events.Publish(new BattleEndedEvent(true, generatedLoot));
            }
        }
    }

    public void Update(float deltaTime) { }
}