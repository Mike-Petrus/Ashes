using System.Collections.Generic;

// The targeting system publishes this whenever the affected area changes!
public class TargetingImpactsChangedEvent : IBattleEvent
{
    // Can be null if targeting cancels or free aims into empty space.
    public List<TargetVisualImpact> ImpactedActors { get; }
        
    public TargetingImpactsChangedEvent(List<TargetVisualImpact> impactedActors)
    {
        ImpactedActors = impactedActors;
    }
}