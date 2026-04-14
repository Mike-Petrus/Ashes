using System.Linq;

public class BattleSimulation
{
    public BattleArena Arena { get; private set;}

    public BattleEventBus Events { get; }
    public IPathfinder Pathfinder { get; }
    
    public ActorRegistry Actors { get; }
    public ActorStateSystem ActorStates { get; }

    public BattleClock Clock { get; }
    public ATBSystem ATB { get; }

    public BattleActionQueue ActionQueue { get; }

    public MovementSystem MovementSystem { get; }
    public PositionSystem PositionSystem { get; }

    public RangeSystem RangeSystem { get; }
    public TargetingSystem TargetingSystem { get; }

    public AbilitySystem AbilitySystem { get; }
    public StatusEffectSystem StatusEffectSystem { get; }
    public EffectPipeline EffectPipeline { get; }

    public BattleContext BattleContext { get; }
    public BattleCommandExecutor CommandExecutor { get; }

    public BattleSimulation(BattleEventBus eventBus, IPathfinder pathfinder)
    {
        Events = eventBus;
        Pathfinder = pathfinder;

        Actors = new ActorRegistry();
        ActorStates = new ActorStateSystem(Events, Actors);

        Clock = new BattleClock(Events);
        ATB = new ATBSystem(Events, Actors, Clock);

        ActionQueue = new BattleActionQueue(Events);

        MovementSystem = new MovementSystem(Events, Pathfinder, ActorStates, Actors);
        PositionSystem = new PositionSystem(Actors);

        RangeSystem = new RangeSystem(Actors);
        TargetingSystem = new TargetingSystem(Actors, PositionSystem);

        AbilitySystem = new AbilitySystem(Events, Actors, ActorStates, TargetingSystem);
        StatusEffectSystem = new StatusEffectSystem(Events, Actors, Clock);
        EffectPipeline = new EffectPipeline(Events, Actors);

        BattleContext = new BattleContext
        {
            Events = Events,
            Actors = Actors,
            ActorStates = ActorStates,
            Movement = MovementSystem,
            Abilities = AbilitySystem,
            Range = RangeSystem,
            Effects = EffectPipeline,
            Clock = Clock
        };

        CommandExecutor = new BattleCommandExecutor(BattleContext);
    }

    public void Update(float deltaTime)
    {
        // Phase 1: TIME & CONTINUIOUS SIMULATION
        // These only process if Clock.IsRunning = true inside their own Update methods
        Clock.Update(deltaTime);
        StatusEffectSystem.Update(deltaTime);   // Effect ticks and expiration
        ATB.Update(deltaTime);            // ATB bars fill up

        // Phase 2: PHYSICAL WORLD
        // Actors physicall move
        MovementSystem.Update(deltaTime);
        PositionSystem.Update(deltaTime);

        // Phase 3: COMMAND MANAGEMENT
        // The actieve step updates (e.g. AbilityStep animating)
        CommandExecutor.Update(deltaTime);

        // Phase 4: EVENT RESOLUTION
        // Flush the event queue - Any damage, deaths, or UI request that happened
        // in Phases 1-3 are processed
        Events.ProcessEvents();

        // Phase 5: QUEUE & STATE MANAGEMENT
        // check if we need to start another command in the queue or unpause the clock
        TryStartNextCommand();
        TryResumeClock();
    }

    // Called after actors are registered by boostrapper
    // TODO: Implement a PartySystem and EnemySpawner to register actors
    public void InitializeBattle(SimVector3 centerPoint)
    {
        // Calculate Arena size
        int totalActors = Actors.GetAllActors().Count();
        Arena = new BattleArena(centerPoint, totalActors);
    }

    private void TryStartNextCommand()
    {
        if (CommandExecutor.IsExecuting)
        {
            return;
        }

        // TODO: Reactions, interrupts
        // ex.
        // if (ReactionSystem.HasReaction())
        // {
        //      StartReactionCommand();
        //      return;
        // }

        if(!ActionQueue.HasCommands())
        {
            return;
        }

        var command = ActionQueue.Dequeue();

        Clock.Pause();
        CommandExecutor.StartCommand(command);
    }

    private void TryResumeClock()
    {
        if (CommandExecutor.IsExecuting)
        {
            return;
        }

        if(Clock.IsRunning)
        {
            return;
        }

        Clock.Resume();
    }
}