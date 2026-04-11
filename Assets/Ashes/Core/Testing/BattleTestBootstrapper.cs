using UnityEngine;
using System.Collections.Generic;

public class BattleTestBootstrapper : MonoBehaviour
{
    [Header("Presentation Layer")]
    public BattleInputManager inputManager;
    
    public ActorStatusUI paladinStatusUI;
    public BattleMenuUI battleMenuUI;

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
        simulation = new BattleSimulation(eventBus);
        debugSystem = new BattleDebugSystem(eventBus, simulation.Actors);

        // 1. Create the Paladin
        var paladinTemplate = new ClassTemplate 
        { 
            ClassName = "Paladin", 
            BaseStats = new CoreAttributes { Strength = 15, Aether = 15, Vitality = 20, Agility = 10, Speed = 10, MoveDistance = 10 } 
        };
        var paladinStats = new CharacterStats(paladinTemplate.BaseStats);
        
        paladinId = new ActorId(1); // Store it to check against later!
        var paladin = new BattleActor(paladinId, "Cecil", paladinStats, new SimVector3(0, 0, 0));
        
        paladin.Abilities.UnlockAbility(new SacrificeAbility());
        paladin.Abilities.UnlockAbility(new HolyFireAbility()); 

        // 2. Create the Goblin (Target)
        var goblinStats = new CharacterStats(new CoreAttributes {  Strength = 10, Aether = 10, Vitality = 10, Agility = 5, Speed = 8, MoveDistance = 10 });
        var goblin = new BattleActor(new ActorId(2), "Goblin", goblinStats, new SimVector3(5, 0, 0));

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
            // Debug.Log("--- EXECUTING SIMULATED D-PAD MACRO ---");

            // Wake the controller up from Idle!
            controller.BeginPartySelection();

            // Execute the inputs instantly
            // controller.ProcessInput(InputButton.Confirm); // Selects Paladin -> RootMenu Phase 1
            
            // controller.ProcessInput(InputButton.Down); // Hovers White Magic
            // controller.ProcessInput(InputButton.Down); // Hovers Wrath
            // controller.ProcessInput(InputButton.Confirm); // Enters Wrath Menu
            
            // controller.ProcessInput(InputButton.Confirm); // Selects Holy Fire -> TargetingActor
            
            controller.InjectTestActor(new ActorId(2)); // Inject Goblin
            // controller.ProcessInput(InputButton.Confirm); // Locks in AbilityStep -> Phase 2 Menu
            
            // controller.ProcessInput(InputButton.Confirm); // Selects Move -> TargetingMove
            
            controller.InjectTestPosition(new SimVector3(2, 0, 0));
            // controller.ProcessInput(InputButton.Confirm); // Locks in MoveStep -> SUBMITS COMMAND!
        }
    }
}