using System.Collections.Generic;

public class BattleSimulation
{
    public EventBus Events { get; }
    public BattleClock Clock { get; }
    public ATBSystem ATB { get; }

    public ActorRegistry Actors { get; }
    public ActorStateSystem ActorStates { get; }

    public BattleActionQueue ActionQueue { get; }
    public BattleCommandExecutor CommandExecutor { get; }

    public MovementSystem MovementSystem { get; }
    public AbilitySystem AbilitySystem { get; }
    public CombatSystem CombatSystem { get; }

    public BattleContext CommandContext { get; }

    private readonly List<IBattleSystem> systems = new();

    public BattleSimulation(EventBus eventBus)
    {
        Events = eventBus;

        Actors = new ActorRegistry();

        ATB = new ATBSystem(Events, Actors);
        Clock = new BattleClock(Events);

        ActorStates = new ActorStateSystem(Events, Actors);

        ActionQueue = new BattleActionQueue(Events);

        MovementSystem = new MovementSystem(Events, ActorStates, Actors);
        AbilitySystem = new AbilitySystem(Events, ActorStates);
        CombatSystem = new CombatSystem(Events, Actors);

        CommandContext = new BattleContext
        {
            Events = Events,
            Actors = Actors,
            ActorStates = ActorStates,
            Movement = MovementSystem,
            Abilities = AbilitySystem,
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

        TryStartNextCommand();
        TryResumeClock();
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