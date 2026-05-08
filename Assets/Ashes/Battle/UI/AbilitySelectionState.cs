using System;
using System.Collections.Generic;

public class AbilitySelectionState : IInputState, IMenuState
{
    private string category;
    private List<Ability> subMenuOptions = new();

    private int currentIndex = 0;
    private int columns = 3; // Define our grid size. TODO: Decide final size later

    // --- IMenuState ---
    public IReadOnlyList<string> MenuOptions
    {
        get
        {
            List<string> names = new();
            foreach (var ability in subMenuOptions)
            {
                names.Add(ability.Name);
            }
            return names;
        }
    }
    public int CurrentIndex => currentIndex;

    public AbilitySelectionState(string abilityCategory)
    {
        category = abilityCategory;
    }

    public void Enter(PlayerTurnController context)
    {
        var actor = context.Simulation.Actors.GetActor(context.ActiveActorId.Value);
        subMenuOptions.AddRange(actor.Abilities.AvailableAbilities[category]);
        
        currentIndex = subMenuOptions.IndexOf(context.SelectedAbility ?? subMenuOptions[0]);

        if (currentIndex < 0)
        {
            currentIndex = 0;
        }

        // Tell UI to draw the 2D grid
    }

    public void ProcessInput(PlayerTurnController context, InputButton button)
    {
        int listSize = subMenuOptions.Count;

        if (listSize == 0)
        {
            return;
        }

        int minInRow = (currentIndex / columns) * columns;
        int maxInRow = Math.Min(minInRow + columns - 1, listSize - 1);

        switch (button)
        {
            case InputButton.Right:
                currentIndex++;

                if (currentIndex > maxInRow)
                {
                    currentIndex = minInRow; // Wrap to left
                }
                break;

            case InputButton.Left:
                currentIndex--;

                if (currentIndex < minInRow)
                {
                    currentIndex += maxInRow; // Wrap to right
                }
                break;

            case InputButton.Down:
                // Jump down a row
                currentIndex += columns;

                if (currentIndex >= listSize)
                {
                    currentIndex %= columns; // Wrap to top
                }
                break;

            case InputButton.Up:
                // Jump up a row
                currentIndex -= columns;

                if (currentIndex < 0)
                {
                    int col = currentIndex + columns;
                    currentIndex = col;

                    while (currentIndex + columns < listSize)
                    {
                        currentIndex += columns;
                    }
                }
                break;

            case InputButton.Confirm:
                Ability selected = subMenuOptions[currentIndex];

                // Check requirements here
                bool canCast = true;
                foreach (var req in selected.Requirements)
                {
                    if (!req.MeetsRequirement(context.ActiveActorId.Value, context.Simulation.Actors))
                    {
                        canCast = false;
                        break;
                    }
                }

                if (canCast)
                {
                    context.SelectedAbility = selected;
                    context.ChangeState(new TargetingActorState());
                }
                else
                {
                    // error message/sound
                }
                break;

            case InputButton.Cancel:
                context.RevertToPreviousState();
                break;
        }
    }

    public void ProcessAnalogInput(PlayerTurnController context, float x, float y, float deltaTime) { }
    public void Exit(PlayerTurnController context) { /* close UI */ }
}