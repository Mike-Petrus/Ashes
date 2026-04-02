using System.Collections.Generic;
using System.Linq;

public class BattleSimulation
{
    public BattleEventBus Events { get; }
    public BattleClock Clock { get; }
    public ATBSystem ATB { get; }

    public BattleArena Arena { get; private set;}

    public ActorRegistry Actors { get; }
    public ActorStateSystem ActorStates { get; }

    public BattleActionQueue ActionQueue { get; }
    public BattleCommandExecutor CommandExecutor { get; }

    public MovementSystem MovementSystem { get; }
    public AbilitySystem AbilitySystem { get; }
   
    public CombatSystem CombatSystem { get; }
    public PositionSystem PositionSystem { get; }
    public RangeSystem RangeSystem { get; }
    public TargetingSystem TargetingSystem { get; }

    public BattleContext CommandContext { get; }

    private readonly List<IBattleSystem> systems = new();

    public BattleSimulation(BattleEventBus eventBus)
    {
        Events = eventBus;

        Actors = new ActorRegistry();

        ATB = new ATBSystem(Events, Actors);
        Clock = new BattleClock(Events);

        ActorStates = new ActorStateSystem(Events, Actors);

        ActionQueue = new BattleActionQueue(Events);

        PositionSystem = new PositionSystem(Actors);
        RangeSystem = new RangeSystem(Actors);
        TargetingSystem = new TargetingSystem(Actors, PositionSystem);

        MovementSystem = new MovementSystem(Events, ActorStates, Actors);
        AbilitySystem = new AbilitySystem(Events, ActorStates, TargetingSystem);

        CombatSystem = new CombatSystem(Events, Actors);

        CommandContext = new BattleContext
        {
            Events = Events,
            Actors = Actors,
            ActorStates = ActorStates,
            Movement = MovementSystem,
            Abilities = AbilitySystem,
            Range = RangeSystem,
            Combat = CombatSystem,
            Clock = Clock
        };

        CommandExecutor = new BattleCommandExecutor(CommandContext);

        systems.Add(Clock);
        systems.Add(MovementSystem);
        systems.Add(CommandExecutor);
    }

    public void Update(float deltaTime)
    {
        // Later we can have Update Groups of List<IBattleSystem>
        // PreUpdate: ATB, Clock
        // Simulation: Movement, Abilities, Effects
        // PostUpdate: CommandExecutor
        foreach (var system in systems)
        {
            system.Update(deltaTime);
        }

        Events.ProcessEvents();

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

        // Spawn and position actors in the arena
        // Later this should be handled by using Party formation and some Spawning system
        SetupInitialPositions();
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

    private void SetupInitialPositions()
    {
        // Example logic: Put actors with ID 1 & 2 (Party) on the left, others (Enemies) on the right
        foreach (var actor in Actors.GetAllActors())
        {
            if (actor.Id.Value <= 2) 
            {
                // Place party members on the left side of the arena
                actor.Position = new SimVector3(Arena.Center.x - 5f, 0, Arena.Center.z + (actor.Id.Value * 2f));
            }
            else
            {
                // Place enemies on the right side
                actor.Position = new SimVector3(Arena.Center.x + 5f, 0, Arena.Center.z + (actor.Id.Value * 2f));
            }

            // Immediately reserve their starting space so they don't overlap
            PositionSystem.ReserveSpace(actor.Id, actor.Position);
        }
    }
}