using System.Collections.Generic;

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
}