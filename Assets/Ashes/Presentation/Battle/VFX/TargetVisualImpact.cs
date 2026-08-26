public struct TargetVisualImpact
{
    public ActorId ActorId { get; }
    // The presenter uses this to find theme.ColorYellow, etc.
    public OutcomeColor VisualColorOutcome { get; } 
        
    public TargetVisualImpact(ActorId actorId, OutcomeColor visualColorOutcome)
    {
        ActorId = actorId;
        VisualColorOutcome = visualColorOutcome;
    }
}