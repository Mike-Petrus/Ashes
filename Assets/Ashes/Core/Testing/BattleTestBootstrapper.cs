using UnityEngine;

public class BattleTestBootstrapper : MonoBehaviour
{
    BattleEventBus eventBus;
    BattleSimulation simulation;

    BattleDebugSystem debugSystem;
    BattleTestCommandSource commandSource;

    void Start()
    {
        // 1. Create core battle systems
        eventBus = new BattleEventBus();
        simulation = new BattleSimulation(eventBus);

        debugSystem = new BattleDebugSystem(eventBus, simulation.Actors);
        commandSource = new BattleTestCommandSource(eventBus);

        // 2. Register Actors
        SetupBattle();

        // 3. Initialize 
        // Create some random battle spot in range [-40, 40]
        float x = Random.Range(-40.0f, 40.0f);
        float z = Random.Range(-40.0f, 40.0f);
        SimVector3 arenaCenter = new SimVector3(x, 0f, z);

        // Move to Debug system if we have an event triggered
        Debug.Log($"Arena center placed at {x} , {z}");

        simulation.InitializeBattle(arenaCenter);

        // May eventually need some BattleStartedEvent to trigger UI
    }

    void Update()
    {
        simulation.Update(Time.deltaTime);
    }

    void SetupBattle()
    {
        // TODO: In the future this data will be handled by other systems, e.g.
        // 1. GlobalGameState.ActiveParty
        // 2. EncounterSystem.GetCurrentEncouter()

        simulation.Actors.RegisterActor(new BattleActor(new ActorId(1), "Knight", 10, SimVector3.Zero));
        simulation.Actors.RegisterActor(new BattleActor(new ActorId(2), "Mage", 12, SimVector3.Zero));
        simulation.Actors.RegisterActor(new BattleActor(new ActorId(3), "Goblin", 15, SimVector3.Zero));
    }
}