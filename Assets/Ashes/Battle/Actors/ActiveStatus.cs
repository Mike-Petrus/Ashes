using System.Collections.Generic;

public class ActiveStatus
{
    public string Name;
    public float DurationLeft;

    public float TickInterval;
    public float TimeSinceLastTick;

    public ActorId SourceId;
    public List<Effect> TickPayload;

    public ActiveStatus(ApplyStatusEffect blueprint, ActorId sourceId)
    {
        Name = blueprint.StatusName;
        DurationLeft = blueprint.DurationSeconds;
        TickInterval = blueprint.TickIntervalSeconds;
        TimeSinceLastTick = 0f;
        SourceId = sourceId;
        TickPayload = blueprint.TickPayload;
    }
}