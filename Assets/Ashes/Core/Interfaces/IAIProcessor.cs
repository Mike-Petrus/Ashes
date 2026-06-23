public interface IAIProcessor
{
    BattleCommand DetermineAction(ActorId actorId, BattleSimulation simulation, BattleCommandBuilder builder);
}