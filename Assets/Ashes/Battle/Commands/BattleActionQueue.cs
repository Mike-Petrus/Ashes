using System.Collections.Generic;

public class BattleActionQueue
{
    private BattleEventBus events;
    private Queue<BattleCommand> queue = new();

    public BattleActionQueue(BattleEventBus eventBus)
    {
        events = eventBus;
        events.Subscribe<CommandBuiltEvent>(OnCommandBuilt);
    }

    private void OnCommandBuilt(CommandBuiltEvent e)
    {
        queue.Enqueue(e.Command);
    }

    public void Enqueue(BattleCommand command)
    {
        queue.Enqueue(command);
    }

    public BattleCommand Dequeue()
    {
        if (queue.Count == 0)
        {
            return null;
        }

        return queue.Dequeue();
    }

    public bool HasCommands()
    {
        return queue.Count > 0;
    }
}