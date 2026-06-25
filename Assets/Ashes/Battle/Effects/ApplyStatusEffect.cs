using System.Collections.Generic;

public class ApplyStatusEffect : Effect
{
    public string StatusName { get; } // Poison, Slow, Haste, etc.
    // public bool IsBuff { get; } // TODO: refactor existing statuses to include
    public float DurationSeconds { get; }
    public float TickIntervalSeconds { get; } // Interval time for periodic effects (e.g. Poison). 0 if passive effect (Haste)
    public List<Effect> TickPayload { get; }    // What happens when this status ticks

    public ApplyStatusEffect(string statusName, float durationSeconds, float tickInterval = 0f, List<Effect> tickPayload = null)
    {
        StatusName = statusName;
        DurationSeconds = durationSeconds;
        TickIntervalSeconds = tickInterval;
        TickPayload = tickPayload ?? new List<Effect>();
    }
}