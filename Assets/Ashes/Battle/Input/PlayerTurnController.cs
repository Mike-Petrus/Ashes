using System.Collections.Generic;
using System.Linq;

public class PlayerTurnController
{
    // --- CORE DEPENDENCIES ---
    public BattleSimulation Simulation { get; private set; }
    public BattleCommandBuilder Builder { get; private set; }
    public PartyManager Party { get; private set; }
    
    // --- STATE MANAGEMENT ---
    public IInputState CurrentState { get; private set; }
    public Stack<IInputState> PreviousStates { get; private set; } = new();

    // --- SHARED DATA ---
    public List<ActorId> PartyActorIds { get; private set; } = new();
    public ActorId? ActiveActorId { get; set; }
    
    public SimVector3 CurrentCursorPosition { get; set; }
    public float CursorSpeed { get; set; } = 8f;

    // --- CURSOR MEMORY ---
    public string SelectedPhase1Option { get; set; }
    public string SelectedPhase2Option { get; set; }
    public Ability SelectedAbility { get; set; }
    public string SelectedItemId { get; set; }

    // Global toggle
    public bool PursuitEnabled { get; set; } = false;
    public bool FreeAimEnabled { get; set; } = false;

    public PlayerTurnController(BattleSimulation battleSimulation, BattleCommandBuilder commandBuilder, PartyManager partyManager)
    {
        Simulation = battleSimulation;
        Builder = commandBuilder;
        Party = partyManager;

        // Ask the runtime registry who the party members are
        var partyActors = Simulation.Actors.GetAllActors().Where(a => a.Faction == ActorFaction.Party).ToList();

        foreach (var actor in partyActors)
        {
            PartyActorIds.Add(actor.Id);
        }

        ChangeState(new IdleState());
    }

    // --- STATE MACHINE ---
    public void ChangeState(IInputState newState, bool recordPrevious = true)
    {
        if (recordPrevious && CurrentState != null)
        {
            PreviousStates.Push(CurrentState);    
        }

        CurrentState?.Exit(this);
        CurrentState = newState;
        CurrentState.Enter(this);
    }

    public void RevertToPreviousState()
    {
        if (PreviousStates.Count > 0)
        {
            var previousState = PreviousStates.Pop();

            CurrentState?.Exit(this);
            CurrentState = previousState;
            CurrentState.Enter(this);
        }
        else
        {
            ChangeState(new IdleState(), false);
        }
    }

    // --- INPUT ROUTING ---
    public void ProcessInput(InputButton button)
    {
        CurrentState?.ProcessInput(this, button);
    }

    public void ProcessAnalogLeft(float x, float y, float deltaTime)
    {
        CurrentState?.ProcessAnalogLeft(this, x, y, deltaTime);
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

        // 2. Set the pursuit right before we add to queue and reset all menues
        Builder.SetPursuit(PursuitEnabled);

        // 3. Build and queue
        var command = Builder.Build();
        Simulation.ActionQueue.Enqueue(command);

        if (ActiveActorId.HasValue)
        {
            Simulation.Events.Publish(new PlayerCommandEndedEvent(ActiveActorId.Value));
        }

        ResetController();
    }

    public void TogglePursuit()
    {
        PursuitEnabled = !PursuitEnabled;
        Simulation.Events.Publish(new PursuitToggledEvent(PursuitEnabled));
    }

    public void TogglePursuit(bool enable)
    {
        PursuitEnabled = enable;
        Simulation.Events.Publish(new PursuitToggledEvent(PursuitEnabled));
    }

    public void ToggleFreeAim()
    {
        FreeAimEnabled = !FreeAimEnabled;
        Simulation.Events.Publish(new FreeAimToggledEvent(FreeAimEnabled));
    }

    public void ToggleFreeAim(bool enable)
    {
        FreeAimEnabled = enable;
        Simulation.Events.Publish(new FreeAimToggledEvent(FreeAimEnabled));
    }

    public void UpdateGhostPreview(bool isVisible, SimVector3 position = default)
    {
        if (ActiveActorId.HasValue)
        {
            Simulation.Events.Publish(new UpdateActorGhostEvent(ActiveActorId.Value, isVisible, position));
        }
    }
    
    public void ResetController()
    {
        SelectedAbility = null;
        PreviousStates.Clear();

        ToggleFreeAim(false);
        TogglePursuit(false);
        UpdateGhostPreview(false);
        
        ChangeState(new IdleState(), false);
        
        ActiveActorId = null;
    }
}