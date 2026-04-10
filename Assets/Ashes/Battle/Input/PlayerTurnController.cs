using System.Collections.Generic;
using System.Linq;

public class PlayerTurnController
{
    private BattleSimulation simulation;
    private BattleCommandBuilder builder;

    public InputState CurrentState { get; private set; } = InputState.Idle;
    public List<InputState> PreviousStates { get; private set; } = new();
    public List<ActorId> PartyActorIds { get; private set; } = new();
    public ActorId? ActiveActorId { get; private set; }

    // ONLY FOR TESTING --- REMOVE LATER
    private SimVector3 injectedTestPosition;
    private ActorId injectedTestActor;

    private bool pursuitEnabled = false;

    private int menuIndex = 0;
    private int subMenuIndex = 0;
    private int selectedActorIndex = 0;
    private Ability selectedAbility;

    private List<string> currentMenuOptions = new();
    private List<Ability> currentSubMenuOptions = new();


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
                CurrentState = InputState.TargetingActor;
            }
            else if (selection == "Move")
            {
                CurrentState = InputState.TargetingMove;
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
                CurrentState = InputState.TargetingActor;
            }
            else if (selection == "Move")
            {
                CurrentState = InputState.TargetingMove;
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
                CurrentState = InputState.TargetingActor;
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
        // Use RangeSystem to validate target

        // System doesn't know if targeting was initiated by
        // Ability menu or Item menu, so use the State list

        if (button == InputButton.Cancel)
        {
            CurrentState = PreviousStates.Last();
            PreviousStates.RemoveAt(PreviousStates.Count - 1);
        }
        else if (button == InputButton.Confirm)
        {
            // If target is not valid give error, then break

            // TEMP: We use an target injected by the BattleTestBootstrapper
            var targetInfo = TargetInfo.ForActor(injectedTestActor, selectedAbility.Mode);
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
        // Use PositionSystem to validate move target

        if (button ==InputButton.Cancel)
        {
            CurrentState = PreviousStates.Last();
            PreviousStates.RemoveAt(PreviousStates.Count - 1);
        }
        else if (button == InputButton.Confirm)
        {
            // TEMP: We use a position injected from the BattleTestBootStrapper
            builder.AddStep(new MoveStep(ActiveActorId.Value, injectedTestPosition));

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
        currentSubMenuOptions = actor.Abilities.AvailableAbilities[category];

        subMenuIndex = 0;
    }

    // ITEM SUB-MENU
    private void BuildItemMenu()
    {
        // TODO: Implement inventory system
        // Filter items by type (don't include equipment)
        // and build menu from available items in inventory
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

    private void SubmitCommand()
    {   // Not sure if we need validation here
        // Ideally all steps should be validated during input and again before execution
        // So validating here would probably be redundant unless edge cases pop up later


        // 1. Build and queue
        var command = builder.Build();
        simulation.ActionQueue.Enqueue(command);

        // 2. Clean up controller
        ActiveActorId = null;
        // Builder should be cleaned automatically after build
        selectedAbility = null;

        currentMenuOptions.Clear();
        currentSubMenuOptions.Clear();
        PreviousStates.Clear();

        menuIndex = 0;
        subMenuIndex = 0;

        // 3. Return to Idle
        // TODO: Consider returning to PartySelection and 
        // handling menuIndex check (e.g. is another actor ready?)
        CurrentState = InputState.Idle;
    }

    // TEMP FOR TESTING ---------
    public void InjectTestActor(ActorId testActor)
    {
        injectedTestActor = testActor;
    }

    // TEMP FOR TESTING ---------
    public void InjectTestPosition(SimVector3 testPosition)
    {
        injectedTestPosition = testPosition;
    }

    // TEMP FOR TESTING ---------
    public void BeginPartySelection()
    {
        CurrentState = InputState.PartySelection;
    }
}