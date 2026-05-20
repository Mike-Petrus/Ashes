using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BattleTestBootstrapper : MonoBehaviour
{
    [Header("Presentation Layer")]
    public BattleInputManager inputManager;
    public ActorStatusUI paladinStatusUI;
    public BattleMenuUI battleMenuUI;
    public BattleFeedbackUI battleFeedbackUI;

    [Header("Simulation Adapters")]
    public NavMeshPathfinder navMeshPathfinder;

    [Header("Prefabs")]
    public GameObject ActorViewPrefab;
    public GameObject CursorViewPrefab;

    BattleEventBus eventBus;
    BattleSimulation simulation;
    BattleDebugSystem debugSystem;

    PlayerTurnController controller;
    ActorId paladinId;

    void Start()
    {
        RunEncounterTest();
    }

    void Update()
    {
        if (simulation != null)
        {
            simulation.Update(Time.deltaTime);
        }
    }

    public void RunEncounterTest()
    {
        eventBus = new BattleEventBus();
        simulation = new BattleSimulation(eventBus, navMeshPathfinder);
        debugSystem = new BattleDebugSystem(eventBus, simulation.Actors);

        // 1. SUBSCRIBE TO THE REGISTRATION EVENT FIRST!
        eventBus.Subscribe<ActorRegisteredEvent>(OnActorRegistered);

        var cursorViewObj = Instantiate(CursorViewPrefab);
        cursorViewObj.name = "View_Cursor";
        cursorViewObj.GetComponent<CursorView>().Initialize(eventBus);

        // 2. CREATE PERSISTENT PARTY
        PartyManager globalPartyManager = new PartyManager();
        var cecilStats = new CharacterStats(new CoreAttributes { Strength = 15, Aether = 15, Vitality = 20, Agility = 10, Speed = 10, MoveDistance = 10 });
        globalPartyManager.AddMemberToParty(new PartyMemberData("Paladin_01", "Cecil", cecilStats));
        globalPartyManager.Inventory.AddItem("Potion", 5);

        // 3. CREATE ENCOUNTER DATA
        EncounterData testEncounter = new EncounterData();
        testEncounter.EnemyIds.Add("Goblin_01");
        testEncounter.EnemyIds.Add("Goblin_01");
        testEncounter.EnemyIds.Add("Goblin_01");

        // 4. SIMULATE OVERWORLD COLLISION
        SimVector3 fakePlayerPos = new SimVector3(0, 0, -4f); 
        SimVector3 fakePlayerFacingDir = new SimVector3(0, 0, 1f); 

        // 5. RUN SPAWNER (This fires OnActorRegistered for everyone automatically!)
        IMapValidator mapValidator = new UnityNavMeshValidator();
        IEnemyDatabase mockDB = new MockEnemyDatabase();
        EncounterSpawner spawner = new EncounterSpawner();
        
        spawner.SetupEncounter(fakePlayerPos, fakePlayerFacingDir, globalPartyManager, mockDB, testEncounter, simulation, mapValidator);

        // 6. INITIALIZE CONTROLLER & UI
        var builder = new BattleCommandBuilder();
        
        // Grab only the player-controlled actors for the controller!
        List<BattleActor> activePartyActors = simulation.Actors.GetAllActors()
            .Where(a => a.Faction == ActorFaction.Party)
            .ToList();

        controller = new PlayerTurnController(simulation, builder, activePartyActors); 

        if (inputManager != null) inputManager.Initialize(controller);
        if (battleMenuUI != null) battleMenuUI.Initialize(controller);
        if (battleFeedbackUI != null) battleFeedbackUI.Initialize(simulation);

        // 7. SUBSCRIBE TO GAMEPLAY EVENTS
        eventBus.Subscribe<ActorReadyEvent>(OnActorReady);
    }

    private void OnActorReady(ActorReadyEvent e)
    {
        // Is paladin's turn?
        if (paladinId != null && e.ActorId.Value == paladinId.Value)
        {
            controller.ChangeState(new PartySelectionState());
        }
    }

    private void OnActorRegistered(ActorRegisteredEvent e)
    {
        var viewObj = Instantiate(ActorViewPrefab);
        viewObj.name = $"View_{e.Actor.Name}";
        
        // Check the Faction
        if (e.Actor.Faction == ActorFaction.Party)
        {
            viewObj.GetComponentInChildren<Renderer>().material.color = Color.blue;
            
            if (e.Actor.Name == "Cecil")
            {
                paladinId = e.Actor.Id;
                e.Actor.Abilities.UnlockAbility(new SacrificeAbility());
                e.Actor.Abilities.UnlockAbility(new HolyFireAbility()); 
                if (paladinStatusUI != null) paladinStatusUI.Initialize(e.Actor, eventBus);
            }
        }
        else if (e.Actor.Faction == ActorFaction.Enemy)
        {
            viewObj.GetComponentInChildren<Renderer>().material.color = Color.red;
        }

        viewObj.GetComponent<ActorView>().Initialize(eventBus, e.Actor.Id, e.Actor.Position);
    }
}