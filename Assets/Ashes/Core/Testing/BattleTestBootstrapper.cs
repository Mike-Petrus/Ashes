using UnityEngine;

public class BattleTestBootstrapper : MonoBehaviour
{
    [Header("Presentation Layer")]
    public BattleInputManager inputManager;
    public ActorStatusUI paladinStatusUI;
    public BattleMenuUI battleMenuUI;
    public BattleFeedbackUI battleFeedbackUI;

    [Header("Simulation Adapters")]
    public NavMeshPathfinder navMeshPathfinder;

    public LayerMask obstacleLayer;

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
        
        // 1. CREATE PERSISTENT PARTY FIRST (So we have an inventory to inject!)
        PartyManager globalPartyManager = new PartyManager();
        var cecilStats = new CharacterStats(new CoreAttributes { Strength = 15, Aether = 15, Vitality = 20, Agility = 10, Speed = 10, MoveDistance = 10 });
        globalPartyManager.AddMemberToParty(new PartyMemberData("Paladin_01", "Cecil", cecilStats));
        
        // Give Cecil 5 Potions to test the ItemSelectionState
        globalPartyManager.Inventory.AddItem("Potion", 5);

        // Create losChecker
        ILineOfSightChecker losChecker = new UnityLineOfSightAdapter(obstacleLayer);

        // 2. INITIALIZE SIMULATION (Injecting the inventory!)
        simulation = new BattleSimulation(eventBus, globalPartyManager.Inventory, navMeshPathfinder, losChecker);
        debugSystem = new BattleDebugSystem(eventBus, simulation.Actors);

        // 3. SUBSCRIBE TO REGISTRATION EVENT
        eventBus.Subscribe<ActorRegisteredEvent>(OnActorRegistered);
        eventBus.Subscribe<BattleEndedEvent>(OnBattleEnded);

        var cursorViewObj = Instantiate(CursorViewPrefab);
        cursorViewObj.name = "View_Cursor";
        cursorViewObj.GetComponent<CursorView>().Initialize(eventBus);

        // 4. CREATE ENCOUNTER DATA
        EncounterData testEncounter = new EncounterData();
        testEncounter.EnemyIds.Add("Goblin_01");
        testEncounter.EnemyIds.Add("Goblin_01");
        testEncounter.EnemyIds.Add("Goblin_01");

        // 5. SIMULATE OVERWORLD COLLISION & RUN SPAWNER
        SimVector3 fakePlayerPos = new SimVector3(0, 0, -4f); 
        SimVector3 fakePlayerFacingDir = new SimVector3(0, 0, 1f); 

        IMapValidator mapValidator = new UnityNavMeshValidator();
        IEnemyDatabase mockDB = new MockEnemyDatabase();
        EncounterSpawner spawner = new EncounterSpawner();
        
        spawner.SetupEncounter(fakePlayerPos, fakePlayerFacingDir, globalPartyManager, mockDB, testEncounter, simulation, mapValidator);

        // 6. INITIALIZE CONTROLLER & UI
        var playerBuilder = new BattleCommandBuilder();

        // Controller now takes the PartyManager and handles roster lookups internally
        controller = new PlayerTurnController(simulation, playerBuilder, globalPartyManager); 

        // Dedicated builder and controller for the Enemy AI!
        var enemyBuilder = new BattleCommandBuilder();
        EnemyTurnController enemyAI = new EnemyTurnController(simulation, enemyBuilder);

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

    private void OnBattleEnded(BattleEndedEvent e)
    {
        if (e.BattleWon)
        {
            Debug.Log("VICTORY! All enemies defeated!");

            foreach (var item in e.Loot)
            {
                Debug.Log($"Loot Obtained: {item.Key} x{item.Value}");
                simulation.BattleContext.Inventory.AddItem(item.Key, item.Value);
            }
        }
        else
        {
            Debug.Log("DEFEAT! The party has been wiped out.");
        }

        controller.ChangeState(new IdleState(), false);
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