using UnityEngine;
using System.Collections.Generic;

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

    [Header("Testing Tools")]
    public BattleScenarioTester ScenarioTester;

    void Start()
    {
        if (ScenarioTester != null && ScenarioTester.IsEnabled)
        {
            RunScenarioTest();
        }
        else
        {
            RunEncounterTest();
        }
    }

    void Update()
    {
        if (simulation != null)
        {
            simulation.Update(Time.deltaTime);
        }
    }

    public void RunScenarioTest()
    {
        eventBus = new BattleEventBus();
        
        // 1. Create Persistent Party & Inventory from Sandbox Config
        PartyManager globalPartyManager = new PartyManager();

        foreach (var memberConfig in ScenarioTester.PartyMembers)
        {
            var attributes = new CoreAttributes
            {
                Strength = memberConfig.Strength,
                Aether = memberConfig.Aether,
                Vitality = memberConfig.Vitality,
                Agility = memberConfig.Agility,
                Speed = memberConfig.Speed,
                MoveDistance = memberConfig.MoveDistance
            };

            var stats = new CharacterStats(attributes);
            // MaxHP/MP are now calculated! Just top off the current pools.
            stats.CurrentHP = stats.MaxHP;
            stats.CurrentMP = stats.MaxMP;

            globalPartyManager.AddMemberToParty(new PartyMemberData(memberConfig.CharacterName + "_ID", memberConfig.CharacterName, stats));
        }

        foreach (var itemConfig in ScenarioTester.InventoryItems)
        {
            globalPartyManager.Inventory.AddItem(itemConfig.ItemType.ToString(), itemConfig.Quantity);
        }

        ILineOfSightChecker losChecker = new UnityLineOfSightAdapter(obstacleLayer);
        simulation = new BattleSimulation(eventBus, globalPartyManager.Inventory, navMeshPathfinder, losChecker);
        debugSystem = new BattleDebugSystem(eventBus, simulation.Actors);

        eventBus.Subscribe<ActorRegisteredEvent>(OnActorRegistered);
        eventBus.Subscribe<BattleEndedEvent>(OnBattleEnded);

        var cursorViewObj = Instantiate(CursorViewPrefab);
        cursorViewObj.name = "View_Cursor";
        cursorViewObj.GetComponent<CursorView>().Initialize(eventBus);

        // 2. Setup Arena
        int totalActors = ScenarioTester.PartyMembers.Count + ScenarioTester.Enemies.Count;
        
        Vector3 unityCenter = ScenarioTester.EncounterCenter != null ? ScenarioTester.EncounterCenter.position : Vector3.zero;
        Vector3 unityForward = ScenarioTester.EncounterCenter != null ? ScenarioTester.EncounterCenter.forward : Vector3.forward;

        SimVector3 simCenter = new SimVector3(unityCenter.x, unityCenter.y, unityCenter.z);
        SimVector3 simForward = new SimVector3(unityForward.x, unityForward.y, unityForward.z);

        IMapValidator mapValidator = new UnityNavMeshValidator();
        BattleArena arena;

        if (ScenarioTester.OverrideArenaRadius)
        {
            arena = new BattleArena(simCenter, simForward, ScenarioTester.CustomArenaRadius, mapValidator);
        }
        else
        {
            arena = new BattleArena(simCenter, simForward, totalActors, mapValidator);
        }
        
        simulation.InitializeBattle(arena);

        // 3. Spawn the Party!
        int nextActorId = 1;
        int[] formationOffsets = { 0, -1, 1, -2, 2 };
        float partySpacing = 1.5f;
        SimVector3 partyBaseLine = arena.GetPartyBaseLine();
        var roster = globalPartyManager.ActiveRoster;

        for (int i = 0; i < roster.Count; i++)
        {
            if (i >= formationOffsets.Length) break;

            SimVector3 targetSpawn = partyBaseLine + (arena.DivisionAxis * formationOffsets[i] * partySpacing);
            targetSpawn = mapValidator.GetNearestValidPosition(targetSpawn, 4f);

            var actor = new BattleActor(new ActorId(nextActorId), roster[i].CharacterName, roster[i].BaseStats, targetSpawn, 1.0f, ActorFaction.Party);
            actor.Stats.CurrentHP = roster[i].CurrentHP;
            actor.Stats.CurrentMP = roster[i].CurrentMP;

            simulation.Actors.RegisterActor(actor);
            nextActorId++;
        }

        // 4. Spawn the Enemies!
        nextActorId = 6;
        IEnemyDatabase mockDB = new MockEnemyDatabase();
        
        // IMPORTANT: Seed the randomizer exactly the same as the Gizmo preview!
        System.Random rand = new System.Random(ScenarioTester.RandomSeed);
        Dictionary<string, int> enemySpawnCounts = new Dictionary<string, int>();

        for (int i = 0; i < ScenarioTester.Enemies.Count; i++)
        {
            var config = ScenarioTester.Enemies[i];
            EnemyTemplate template = mockDB.GetEnemy(config.EnemyId);
            
            if (template == null) continue; 

            // Naming Logic
            if (!enemySpawnCounts.ContainsKey(config.EnemyId)) enemySpawnCounts[config.EnemyId] = 0;
            int currentCount = enemySpawnCounts[config.EnemyId]++;
            
            string actorName = template.DefaultName;
            if (ScenarioTester.Enemies.Count > 1) 
            {
                actorName = $"{template.DefaultName} {(char)('A' + currentCount)}";
            }

            // Determine Position
            SimVector3 spawnPos;
            if (ScenarioTester.UseManualSpawnPoints && i < ScenarioTester.ManualSpawnPoints.Count && ScenarioTester.ManualSpawnPoints[i] != null)
            {
                Vector3 p = ScenarioTester.ManualSpawnPoints[i].position;
                spawnPos = new SimVector3(p.x, p.y, p.z);
            }
            else
            {
                // This will perfectly replicate the sequence of NextDouble() calls from OnDrawGizmos
                float randomForward = (float)rand.NextDouble() * (arena.Radius - 3f) + 1f;
                float randomSide = (float)(rand.NextDouble() * 2.0 - 1.0) * (arena.Radius - 3f);
                spawnPos = arena.Center + (arena.PlayerFacingDir * randomForward) + (arena.DivisionAxis * randomSide);
                spawnPos = mapValidator.GetNearestValidPosition(spawnPos, 4f);
            }

            // Determine Stats cleanly via Attributes
            CharacterStats enemyStats;
            if (config.OverrideStats)
            {
                var customAttributes = new CoreAttributes
                {
                    Strength = template.BaseAttributes.Strength,
                    Agility = template.BaseAttributes.Agility,
                    Aether = config.Aether,
                    Vitality = config.Vitality,
                    Speed = config.Speed,
                    MoveDistance = config.MoveDistance
                };
                enemyStats = new CharacterStats(customAttributes);
            }
            else
            {
                enemyStats = new CharacterStats(template.BaseAttributes);
            }

            enemyStats.CurrentHP = enemyStats.MaxHP;
            enemyStats.CurrentMP = enemyStats.MaxMP;

            // Register
            var enemy = new BattleActor(new ActorId(nextActorId), actorName, enemyStats, spawnPos, template.Radius, ActorFaction.Enemy);
            enemy.Abilities.UnlockAbility(new BasicAttackAbility()); 
            simulation.Actors.RegisterActor(enemy);
            nextActorId++;
        }

        // 5. Initialize Controller & UI
        var playerBuilder = new BattleCommandBuilder();

        controller = new PlayerTurnController(simulation, playerBuilder, globalPartyManager); 

        if (inputManager != null) inputManager.Initialize(controller);
        if (battleMenuUI != null) battleMenuUI.Initialize(controller);
        if (battleFeedbackUI != null) battleFeedbackUI.Initialize(simulation);

        eventBus.Subscribe<ActorReadyEvent>(OnActorReady);
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

                // AoE Test Abilities
                e.Actor.Abilities.UnlockAbility(new HolyNovaAbility());
                e.Actor.Abilities.UnlockAbility(new CometAbility());
                e.Actor.Abilities.UnlockAbility(new DivineCleaveAbility());
                 
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