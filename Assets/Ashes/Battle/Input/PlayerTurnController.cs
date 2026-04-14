using System.Collections.Generic;
using System.Linq;

// TODO: Move move/targeting validation to their own functions
// TODO: Create Error events for player feedback

public class PlayerTurnController
{
    private BattleSimulation simulation;
    private BattleCommandBuilder builder;

    public InputState CurrentState { get; private set; } = InputState.Idle;
    public List<InputState> PreviousStates { get; private set; } = new();
    public List<ActorId> PartyActorIds { get; private set; } = new();
    public ActorId? ActiveActorId { get; private set; }

    private SimVector3 currentCursorPosition;
    private float cursorSpeed = 8f;

    private List<ActorId> currentAvailableTargets = new();
    private int currentTargetIndex = 0;

    private bool pursuitEnabled = false;

    private int menuIndex = 0;
    private int subMenuIndex = 0;
    private int selectedActorIndex = 0;
    private Ability selectedAbility;

    private List<string> currentMenuOptions = new();
    private List<Ability> currentSubMenuOptions = new();

    public IReadOnlyList<string> CurrentMenuOptions => currentMenuOptions;
    public IReadOnlyList<Ability> CurrentSubMenuOptions => currentSubMenuOptions;
    public int MenuIndex => menuIndex;
    public int SubMenuIndex => subMenuIndex;


    public PlayerTurnController(BattleSimulation battleSimulation, BattleCommandBuilder commandBuilder, List<BattleActor> Party)
    {
        simulation = battleSimulation;
        builder = commandBuilder;

        // TODO: Implement party system
        // For now we manually pass a list of actors
        foreach (var actor in Party)
        {
            PartyActorIds.Add(actor.Id);
        }
    }

    // Single entry point for all inputs
    public void ProcessInput(InputButton button)
    {
        if (button == InputButton.Pursuit)
        {
            pursuitEnabled = !pursuitEnabled;
            // TODO: create event so UI can toggle Pursuit icon
        }

        switch (CurrentState)
        {
            case InputState.Idle:
                // Nothing. Later confirm to select party menu
                break;

            case InputState.PartySelection:
                HandlePartySelectionInput(button);
                break;

            case InputState.RootMenuPhase1:
                HandleRootMenuPhase1Input(button);
                break;

            case InputState.RootMenuPhase2:
                HandleRootMenuPhase2Input(button);
                break;

            case InputState.AbilitySelectionMenu:
                HandleAbilitySelectionInput(button);
                break;

            // case InputState.ItemSelectionMenu:
            //     HandleItemSelectionInput(button);
            //     break;

            case InputState.TargetingActor:
                HandleTargetingActorInput(button);
                break;

            case InputState.TargetingMove:
                HandleTargetingMoveInput(button);
                break;
        }
    }

    private void HandlePartySelectionInput(InputButton button)
    {
        if (button == InputButton.Up)
        {
            MoveCursor(-1, ref selectedActorIndex, PartyActorIds.Count);
        }
        if (button == InputButton.Down)
        {
            MoveCursor(1, ref selectedActorIndex, PartyActorIds.Count);
        }
        if (button == InputButton.Cancel)
        {
            // Return to idle
            CurrentState = InputState.Idle;
        }
        else if (button == InputButton.Confirm)
        {
            // Get the ActorId the player highlighted with the D-pad
            // For now assume Party Members 0 - 4 always reserve Ids 1-5
            ActiveActorId = PartyActorIds[selectedActorIndex];
            builder = new BattleCommandBuilder();
            builder.BeginCommand(ActiveActorId.Value);

            BuildRootMenu();
            CurrentState = InputState.RootMenuPhase1;
        }
    }

    private void HandleRootMenuPhase1Input(InputButton button)
    {
        if (button == InputButton.Up)
        {
            MoveCursor(-1, ref menuIndex, currentMenuOptions.Count);
        }
        if (button == InputButton.Down)
        {
            MoveCursor(1, ref menuIndex, currentMenuOptions.Count);
        }
        if (button == InputButton.Cancel)
        {
            ActiveActorId = null;
            menuIndex = 0;
            currentMenuOptions.Clear();
            CurrentState = InputState.PartySelection;
        }
        else if (button == InputButton.Confirm)
        {
            string selection = currentMenuOptions[menuIndex];
            PreviousStates.Add(InputState.RootMenuPhase1);

            if (selection == "Attack")
            {
                selectedAbility = new BasicAttackAbility();
                BeginTargetingActor();
            }
            else if (selection == "Move")
            {
                BeginTargetingMove();
            }
            else
            {
                // Otherwise must be Ability Category -- Need submenu
                BuildAbilityMenu(selection);
                CurrentState = InputState.AbilitySelectionMenu;
            }
        }
    }

    private void HandleRootMenuPhase2Input(InputButton button)
    {
        if (button == InputButton.Up)
        {
            MoveCursor(-1, ref menuIndex, currentMenuOptions.Count);
        }
        if (button == InputButton.Down)
        {
            MoveCursor(1, ref menuIndex, currentMenuOptions.Count);
        }
        if (button == InputButton.Cancel)
        {
            builder.UndoLastStep();

            CurrentState = PreviousStates.Last();
            PreviousStates.RemoveAt(PreviousStates.Count - 1);
        }
        else if (button == InputButton.Confirm)
        {
            string selection = currentMenuOptions[menuIndex];
            PreviousStates.Add(InputState.RootMenuPhase2);

            if (selection == "Attack")
            {
                selectedAbility = new BasicAttackAbility();
                BeginTargetingActor();
            }
            else if (selection == "Move")
            {
                BeginTargetingMove();
            }
            else if (selection == "Wait")
            {
                builder.AddStep(new WaitStep(ActiveActorId.Value));
                SubmitCommand();
            }
            else
            {
                // Otherwise must be Ability Category -- Need submenu
                BuildAbilityMenu(selection);
                CurrentState = InputState.AbilitySelectionMenu;
            }
        }       
    }

    private void HandleAbilitySelectionInput(InputButton button)
    {
        if (button == InputButton.Up)
        {
            MoveCursor(-1, ref subMenuIndex, currentSubMenuOptions.Count);
        }
        if (button == InputButton.Down)
        {
            MoveCursor(1, ref subMenuIndex, currentSubMenuOptions.Count);
        }
        if (button == InputButton.Cancel)
        {
            subMenuIndex = 0;
            CurrentState = PreviousStates.Last();
            PreviousStates.RemoveAt(PreviousStates.Count - 1);
        }
        else if (button == InputButton.Confirm)
        {
            Ability selected = currentSubMenuOptions[subMenuIndex];

            // Pre-validation happens here
            // But would need to be moved to BuildAbilityMenu
            // if we want to show gray text when menu is populated
            bool canCast = true;

            foreach (var req in selected.Requirements)
            {
                if (!req.MeetsRequirement(ActiveActorId.Value, simulation.Actors))
                {
                    canCast = false;
                    break;
                }
            }

            if (canCast)
            {
                selectedAbility = selected;
                PreviousStates.Add(CurrentState);
                BeginTargetingActor();
            }
            else
            {
                // Error message/sound
                // can't cast
            }
        }
    }

    private void HandleItemSelectionInput(InputButton button)
    {
        // player chooses item from inventory and then target
    }

    private void HandleTargetingActorInput(InputButton button)
    {
        // TODO: Consolidate D-Pad inputs
        if (button == InputButton.Left)
        {
            currentTargetIndex--;
            if (currentTargetIndex < 0) currentTargetIndex = currentAvailableTargets.Count - 1;

            UpdateActorCursorVisuals();
        }
        else if (button == InputButton.Right)
        {
            currentTargetIndex++;
            if (currentTargetIndex >= currentAvailableTargets.Count) currentTargetIndex = 0;

            UpdateActorCursorVisuals();
        }
        else if (button == InputButton.Cancel)
        {
            // Hide the cursor
            simulation.Events.Publish(new CursorMovedEvent(new SimVector3(), false));

            CurrentState = PreviousStates.Last();
            PreviousStates.RemoveAt(PreviousStates.Count - 1);
        }
        else if (button == InputButton.Confirm)
        {
            if (currentAvailableTargets.Count == 0)
            {
                return;
            }

            ActorId selectedTargetId = currentAvailableTargets[currentTargetIndex];

            var targetInfo = TargetInfo.ForActor(selectedTargetId, selectedAbility.Mode);

            // TODO: Move validation to its own function
            // 1. Determine the origin point
            var activeActor = simulation.Actors.GetActor(ActiveActorId.Value);
            SimVector3 originPosition = activeActor.Position;

            if (builder.Size > 0)
            {
                var previousStep = builder.LastStepAdded();

                if (previousStep is MoveStep moveStep)
                {
                    originPosition = moveStep.Destination;
                }

                if (!simulation.RangeSystem.IsInRange(originPosition, activeActor.Radius, selectedAbility, targetInfo))
                {
                    // Error: Target is out of range
                    return;
                }
            }
            else
            {
                if (!simulation.RangeSystem.IsActorInRange(ActiveActorId.Value, selectedAbility, targetInfo))
                {
                    // Error: Target is out of range
                    return;
                }
            }

            // Validation Passed. Hide the cursor and draft the step
            simulation.Events.Publish(new CursorMovedEvent(new SimVector3(), false));

            builder.AddStep(new AbilityStep(ActiveActorId.Value, selectedAbility, targetInfo));

            // Assume for now that all BattleCommands can only be 2 steps
            if (builder.Size >= 2)
            {
                SubmitCommand();
            }
            else
            {
                menuIndex = 0;
                PreviousStates.Add(CurrentState);
                BuildRootMenuPhase2();
                CurrentState = InputState.RootMenuPhase2;
            }
        }
    }

    private void HandleTargetingMoveInput(InputButton button)
    {
        if (button ==InputButton.Cancel)
        {
            simulation.Events.Publish(new CursorMovedEvent(new SimVector3(), false));
            CurrentState = PreviousStates.Last();
            PreviousStates.RemoveAt(PreviousStates.Count - 1);
        }
        else if (button == InputButton.Confirm)
        {
            // TODO: Move validation to it's own function
            // 1. Validate Distance
            var activeActor = simulation.Actors.GetActor(ActiveActorId.Value);
            float moveDistance = SimVector3.Distance(activeActor.Position, currentCursorPosition);

            if (moveDistance > activeActor.Stats.MoveDistance)
            {
                // Error: Target position is too far
                return;
            }

            // 2. Validate Path Availability
            var path = simulation.Pathfinder.FindPath(activeActor.Position, currentCursorPosition, activeActor.Radius);

            if (path == null || path.Count == 0)
            {
                // Error: No path found!
                return;
            }

            // 3. Validate Spatial Collision
            if (simulation.PositionSystem.IsSpaceOccupied(currentCursorPosition, activeActor.Radius, ActiveActorId.Value))
            {
                // Error: Space is occupied
                return;
            }

            // Validation Passed
            simulation.Events.Publish(new CursorMovedEvent(new SimVector3(), false));

            // 4. Draft the step (NO RESERVATION YET)
            builder.AddStep(new MoveStep(ActiveActorId.Value, currentCursorPosition));

            if (builder.Size >= 2)
            {
                SubmitCommand();
            }
            else
            {
                PreviousStates.Add(CurrentState);
                BuildRootMenuPhase2();
                CurrentState = InputState.RootMenuPhase2;
            }
        }
    }

    // MAIN COMMAND MENU
    private void BuildRootMenu()
    {
        currentMenuOptions.Clear();
        currentMenuOptions.Add("Attack");

        // Dynamically grab the actor's ability categories
        var actor = simulation.Actors.GetActor(ActiveActorId.Value);
        currentMenuOptions.AddRange(actor.Abilities.AvailableAbilities.Keys);

        currentMenuOptions.Add("Move");
        // TODO: Implement inventory
        currentMenuOptions.Add("Items");

        menuIndex = 0;
    }

    // SECOND PHASE MENU
    private void BuildRootMenuPhase2()
    {
        currentMenuOptions.Clear();

        // TODO: Should we check the previous state or last step added to the command?
        if (PreviousStates.Last() == InputState.TargetingMove)
        {
            currentMenuOptions.Add("Attack");

            // TODO: create a function to return all the abilities to replace this code
            var actor = simulation.Actors.GetActor(ActiveActorId.Value);
            currentMenuOptions.AddRange(actor.Abilities.AvailableAbilities.Keys);

            currentMenuOptions.Add("Items");
            currentMenuOptions.Add("Wait");
        }
        else
        {
            currentMenuOptions.Add("Move");
            currentMenuOptions.Add("Wait");            
        }

        menuIndex = 0;
    }

    // ABILITY SUB-MENU
    private void BuildAbilityMenu(string category)
    {
        var actor = simulation.Actors.GetActor(ActiveActorId.Value);
        currentSubMenuOptions.AddRange(actor.Abilities.AvailableAbilities[category]);

        subMenuIndex = 0;
    }

    // ITEM SUB-MENU
    private void BuildItemMenu()
    {
        // TODO: Implement inventory system
        // Filter items by type (don't include equipment)
        // and build menu from available items in inventory
    }

    private void SubmitCommand()
    {   
        var activeActor = simulation.Actors.GetActor(ActiveActorId.Value);

        // 1. Final Position Validation
        foreach (var step in builder.Steps)
        {
            if (step is MoveStep moveStep)
            {
                if (simulation.PositionSystem.IsSpaceOccupied(moveStep.Destination, activeActor.Radius, ActiveActorId.Value))
                {
                    // Error: Destination taken while deciding
                    return;
                }

                // Reserve the space
                simulation.PositionSystem.ReserveSpace(ActiveActorId.Value, moveStep.Destination);
            }
        }

        // 2. Build and queue
        var command = builder.Build();
        simulation.ActionQueue.Enqueue(command);

        // 3. Clean up controller
        ActiveActorId = null;
        selectedAbility = null;

        currentMenuOptions.Clear();
        currentSubMenuOptions.Clear();
        PreviousStates.Clear();

        menuIndex = 0;
        subMenuIndex = 0;

        // 4. Return to Idle
        // TODO: Consider returning to PartySelection and 
        // handling menuIndex check (e.g. is another actor ready?)
        CurrentState = InputState.Idle;
    }

    private void BeginTargetingActor()
    {
        CurrentState = InputState.TargetingActor;

        // 1. Gather all valid targets
        // In the future TargetingSystem can filter this based on the Ability
        currentAvailableTargets.Clear();

        foreach (var actorId in simulation.Actors.GetAliveActorIds())
        {
            currentAvailableTargets.Add(actorId);
        }

        currentTargetIndex = 0;

        // 2. Snap the cursor to the first target
        UpdateActorCursorVisuals();
    }

    private void BeginTargetingMove()
    {
        CurrentState = InputState.TargetingMove;

        var activeActor = simulation.Actors.GetActor(ActiveActorId.Value);
        currentCursorPosition = activeActor.Position;

        simulation.Events.Publish(new CursorMovedEvent(currentCursorPosition, true, true, null));
    }

    private void MoveCursor(int direction, ref int indexChanged, int listSize)
    {
        // TODO: Refine and allow to handle left/right in two dimensional array layout
        // depending on how final menu looks e.g. 2 columsn, 3 columns, etc.
        if (listSize == 0)
        {
            return;
        }

        indexChanged += direction;

        if (indexChanged < 0)
        {
            indexChanged = listSize - 1;
        }
        if (indexChanged >= listSize)
        {
            indexChanged = 0;
        }
    }

    public void ProcessAnalogInput(float x, float y, float deltaTime)
    {
        if (CurrentState == InputState.TargetingMove)
        {
            // 1. Update the virtual position 
            currentCursorPosition.x += x * cursorSpeed * deltaTime;
            currentCursorPosition.z += y * cursorSpeed * deltaTime;

            var activeActor = simulation.Actors.GetActor(ActiveActorId.Value);

            // 2. Get the path
            var path = simulation.Pathfinder.FindPath(activeActor.Position, currentCursorPosition, activeActor.Radius);

            // 3. Validation
            bool isValid = true;
            float pathDistance = 0f;

            for (int i = 0; i < path.Count - 1; i++)
            {
                pathDistance += SimVector3.Distance(path[i], path[i+1]);
            }

            if (pathDistance > activeActor.Stats.MoveDistance || path.Count == 0)
            {
                isValid = false;
            }
            if (simulation.PositionSystem.IsSpaceOccupied(currentCursorPosition, activeActor.Radius, ActiveActorId.Value))
            {
                isValid = false;
            }

            // 4. Broadcast
            simulation.Events.Publish(new CursorMovedEvent(currentCursorPosition, true, isValid, path));
        }
    }

    private void UpdateActorCursorVisuals()
    {
        if (currentAvailableTargets.Count == 0)
        {
            return;
        }

        var targetActor = simulation.Actors.GetActor(currentAvailableTargets[currentTargetIndex]);

        // Broadcast to Unity (Position, IsVisible, IsValid)
        // For now assume valid, but we can add RangeSystem check later
        // to turn cursor red
        simulation.Events.Publish(new CursorMovedEvent(targetActor.Position, true, true));
    }

    // TEMP FOR TESTING ---------
    public void BeginPartySelection()
    {
        CurrentState = InputState.PartySelection;
    }
}