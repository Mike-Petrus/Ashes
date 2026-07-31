public class EffectInteractionEvent : IBattleEvent
{
    public ActorId SourceId { get; }
    public ActorId TargetId { get; }
    public string StatusId { get; }
    public string TriggerAbilityId { get; }

    public EffectInteractionEvent(ActorId sourceId, ActorId targetId, string statusId, string triggerId)
    {
        SourceId = sourceId;
        TargetId = targetId;
        StatusId = statusId;
        TriggerAbilityId = triggerId;
    }
}