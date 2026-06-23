using System.Collections.Generic;

public class EnemyTurnController : IBattleSystem
{
    private BattleSimulation simulation;
    private BattleCommandBuilder builder;
    private IAIProcessor aiProcessor;

    private Queue<ActorId> readyQueue = new();

    public EnemyTurnController(BattleSimulation battleSimulation, BattleCommandBuilder commandBuilder, IAIProcessor processor)
    {
        simulation = battleSimulation;
        builder = commandBuilder;
        aiProcessor = processor;

        simulation.Events.Subscribe<ActorReadyEvent>(OnActorReady);
    }

    public void Update(float deltaTime)
    {
        if (readyQueue.Count > 0)
        {
            ActorId nextEnemy = readyQueue.Dequeue();
            ProcessEnemyTurn(nextEnemy);
        }
    }

    private void OnActorReady(ActorReadyEvent e)
    {
        var actor = simulation.Actors.GetActor(e.ActorId);

        if (actor == null || actor.Faction != ActorFaction.Enemy || !actor.IsAlive)
        {
            return;
        }

        readyQueue.Enqueue(e.ActorId);
    }

    private void ProcessEnemyTurn(ActorId actorId)
    {
        // 1. Ask AI Processor what to do
        BattleCommand command = aiProcessor.DetermineAction(actorId, simulation, builder);

        if (command != null && command.Steps.Count > 0)
        {
            foreach (var step in command.Steps)
            {
                if (step is MoveStep moveStep)
                {
                    simulation.PositionSystem.ReserveSpace(actorId, moveStep.Destination);
                }
            }
            simulation.ActionQueue.Enqueue(command);
        }
    }
}