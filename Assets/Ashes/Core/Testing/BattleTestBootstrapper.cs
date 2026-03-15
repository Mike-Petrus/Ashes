using UnityEngine;

public class BattleTestBootstrapper : MonoBehaviour
{
    EventBus eventBus;
    BattleSimulation simulation;

    BattleDebugSystem debugSystem;
    BattleTestCommandSource commandSource;

    void Start()
    {
        eventBus = new EventBus();
        simulation = new BattleSimulation(eventBus);

        debugSystem = new BattleDebugSystem(eventBus, simulation.Actors);
        commandSource = new BattleTestCommandSource(eventBus);

        SetupBattle();
    }

    void Update()
    {
        simulation.Update(Time.deltaTime);
    }

    void SetupBattle()
    {
        simulation.Actors.RegisterActor(new BattleActor(new ActorId(1), "Knight", 10, SimVector3.Zero));
        simulation.Actors.RegisterActor(new BattleActor(new ActorId(2), "Mage", 12, SimVector3.Zero));
        simulation.Actors.RegisterActor(new BattleActor(new ActorId(3), "Goblin", 15, SimVector3.Zero));
    }
}