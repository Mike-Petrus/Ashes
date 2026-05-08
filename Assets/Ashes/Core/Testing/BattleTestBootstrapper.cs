using UnityEngine;
using System.Collections.Generic;

public class BattleTestBootstrapper : MonoBehaviour
{
    [Header("Presentation Layer")]
    public BattleInputManager inputManager;
    public ActorStatusUI paladinStatusUI;
    public BattleMenuUI battleMenuUI;

    [Header("Simulation Adapters")]
    public NavMeshPathfinder navMeshPathfinder;

    [Header("Prefabs")]
    public GameObject ActorViewPrefab;
    public GameObject CursorViewPrefab;

    BattleEventBus eventBus;
    BattleSimulation simulation;
    BattleDebugSystem debugSystem;

    // Make these class-level so our event handler can use them!
    PlayerTurnController controller;
    ActorId paladinId;

    void Start()
    {
        RunPaladinTest();
    }

    void Update()
    {
        if (simulation != null)
        {
            simulation.Update(Time.deltaTime);
        }
    }

    public void RunPaladinTest()
    {
        eventBus = new BattleEventBus();
        simulation = new BattleSimulation(eventBus, navMeshPathfinder);
        debugSystem = new BattleDebugSystem(eventBus, simulation.Actors);

        var cursorViewObj = Instantiate(CursorViewPrefab);
        cursorViewObj.name = "View_Cursor";
        cursorViewObj.GetComponent<CursorView>().Initialize(eventBus);

        // 1. Create the Paladin
        var paladinTemplate = new ClassTemplate 
        { 
            ClassName = "Paladin", 
            BaseStats = new CoreAttributes { Strength = 15, Aether = 15, Vitality = 20, Agility = 10, Speed = 10, MoveDistance = 10 } 
        };
        var paladinStats = new CharacterStats(paladinTemplate.BaseStats);
        
        paladinId = new ActorId(1); // Store it to check against later!
        var paladin = new BattleActor(paladinId, "Cecil", paladinStats, new SimVector3(0, 0, 0));

        // Spawn the Cube
        var paladinViewObj = Instantiate(ActorViewPrefab);
        paladinViewObj.name = "View_Cecil";
        paladinViewObj.GetComponentInChildren<Renderer>().material.color = Color.blue;
        paladinViewObj.GetComponent<ActorView>().Initialize(eventBus, paladin.Id, paladin.Position);
        
        paladin.Abilities.UnlockAbility(new SacrificeAbility());
        paladin.Abilities.UnlockAbility(new HolyFireAbility()); 

        // 2. Create the Goblin (Target)
        var goblinStats = new CharacterStats(new CoreAttributes {  Strength = 10, Aether = 10, Vitality = 10, Agility = 5, Speed = 8, MoveDistance = 10 });
        var goblin = new BattleActor(new ActorId(2), "Goblin", goblinStats, new SimVector3(5, 0, 0));

        // Spawn the Cube
        var goblinViewObj = Instantiate(ActorViewPrefab);
        goblinViewObj.name = "View_Goblin";
        goblinViewObj.GetComponentInChildren<Renderer>().material.color = Color.red; // Make him red!
        goblinViewObj.GetComponent<ActorView>().Initialize(eventBus, goblin.Id, goblin.Position);

        // 3. Register Actors and Start Battle
        simulation.Actors.RegisterActor(paladin);
        simulation.Actors.RegisterActor(goblin);
        simulation.InitializeBattle(new SimVector3(0,0,0));

        // 4. Initialize the Input Controller
        var party = new List<BattleActor> { paladin };
        var builder = new BattleCommandBuilder();
        controller = new PlayerTurnController(simulation, builder, party);

        // 5. Wire up the Presentation Layer
        if (inputManager != null) inputManager.Initialize(controller);
        if (paladinStatusUI != null) paladinStatusUI.Initialize(paladin, eventBus);
        if (battleMenuUI != null) battleMenuUI.Initialize(controller);

        // 6. SUBSCRIBE TO THE EVENT!
        eventBus.Subscribe<ActorReadyEvent>(OnActorReady);
    }

    // This fires the moment an ATB bar hits 100!
    private void OnActorReady(ActorReadyEvent e)
    {
        // Is it the Paladin's turn?
        if (e.ActorId.Value == paladinId.Value)
        {
            // Wake the controller up from Idle!
            controller.ChangeState(new PartySelectionState());

            // Execute the inputs instantly
            // controller.ProcessInput(InputButton.Confirm); // Selects Paladin -> RootMenu Phase 1
            
            // controller.ProcessInput(InputButton.Down); // Hovers White Magic
            // controller.ProcessInput(InputButton.Down); // Hovers Wrath
            // controller.ProcessInput(InputButton.Confirm); // Enters Wrath Menu
            
            // controller.ProcessInput(InputButton.Confirm); // Selects Holy Fire -> TargetingActor
            
            //controller.InjectTestActor(new ActorId(2)); // Inject Goblin
            // controller.ProcessInput(InputButton.Confirm); // Locks in AbilityStep -> Phase 2 Menu
            
            // controller.ProcessInput(InputButton.Confirm); // Selects Move -> TargetingMove
            
            //controller.InjectTestPosition(new SimVector3(2, 0, 0));
            // controller.ProcessInput(InputButton.Confirm); // Locks in MoveStep -> SUBMITS COMMAND!
        }
    }
}