using System.Collections.Generic;

// TODO: Move move/targeting validation to their own functions
// TODO: Create Error events for player feedback

public class PlayerTurnController
{
    // --- CORE DEPENDENCIES ---
    public BattleSimulation Simulation { get; private set; }
    public BattleCommandBuilder Builder { get; private set; }
    
    // --- STATE MANAGEMENT ---
    public IInputState CurrentState { get; private set; }
    public Stack<IInputState> PreviousStates { get; private set; } = new();

    // --- SHARED DATA ---
    public List<ActorId> PartyActorIds { get; private set; } = new();
    public ActorId? ActiveActorId { get; set; }
    
    public SimVector3 CurrentCursorPosition { get; set; }
    public float CursorSpeed { get; set; } = 8f;

    public Ability SelectedAbility { get; set; }
    
    // --- MENU DATA ---
    public List<string> CurrentMenuOptions { get; } = new();

    // Global toggle
    public bool PursuitEnabled { get; set; } = false;

    public PlayerTurnController(BattleSimulation battleSimulation, BattleCommandBuilder commandBuilder, List<BattleActor> Party)
    {
        Simulation = battleSimulation;
        Builder = commandBuilder;

        // TODO: Implement party system
        // For now we manually pass a list of actors
        foreach (var actor in Party)
        {
            PartyActorIds.Add(actor.Id);
        }

        ChangeState(new IdleState());
    }

    // --- STATE MACHINE ---
    public void ChangeState(IInputState newState, bool recordPrevious = true)
    {
        if (CurrentState != null)
        {
            CurrentState.Exit(this);
            if (recordPrevious)
            {
                PreviousStates.Push(CurrentState);
            }
        }

        CurrentState = newState;
        CurrentState.Enter(this);
    }

    public void RevertToPreviousState()
    {
        if (PreviousStates.Count > 0)
        {
            CurrentState?.Exit(this);
            CurrentState = PreviousStates.Pop();
            CurrentState.Enter(this);
        }
    }

    // --- INPUT ROUTING ---
    public void ProcessInput(InputButton button)
    {
        CurrentState?.ProcessInput(this, button);
    }

    public void ProcessAnalogInput(float x, float y, float deltaTime)
    {
        CurrentState?.ProcessAnalogInput(this, x, y, deltaTime);
    }

    public void SubmitCommand()
    {   
        var activeActor = Simulation.Actors.GetActor(ActiveActorId.Value);

        // 1. Final Position Validation
        foreach (var step in Builder.Steps)
        {
            if (step is MoveStep moveStep)
            {
                if (Simulation.PositionSystem.IsSpaceOccupied(moveStep.Destination, activeActor.Radius, ActiveActorId.Value))
                {
                    // Error: Destination taken while deciding
                    return;
                }

                // Reserve the space
                Simulation.PositionSystem.ReserveSpace(ActiveActorId.Value, moveStep.Destination);
            }
        }

        // 2. Build and queue
        var command = Builder.Build();
        Simulation.ActionQueue.Enqueue(command);

        ResetController();
    }
    
    public void ResetController()
    {
        ActiveActorId = null;
        SelectedAbility = null;
        CurrentMenuOptions.Clear();
        PreviousStates.Clear();
        
        ChangeState(new IdleState(), false);    
    }
}