using System.Collections.Generic;
using System.Linq;

public class BattleCommand
{
    public ActorId ActorId { get; }

    private List<CommandStep> steps = new();

    public IReadOnlyList<CommandStep> Steps => steps;

    public BattleCommand(ActorId actorId)
    {
        ActorId = actorId;
    }

    public void AddStep(CommandStep step)
    {
        steps.Add(step);
    }

    public void RemoveLastStep()
    {
        if (steps.Count > 0)
        {
            steps.RemoveAt(steps.Count - 1);
        }
    }

    public CommandStep LastStep()
    {
        if (steps.Count > 0)
        {
            return steps.Last();
        }

        return null;
    }
}