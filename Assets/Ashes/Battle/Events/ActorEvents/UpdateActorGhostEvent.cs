public class UpdateActorGhostEvent : IBattleEvent
{
    public ActorId ActorId { get; }
    public bool IsVisible { get; }
    public SimVector3 Position { get; }

    public UpdateActorGhostEvent(ActorId actorId, bool isVisible, SimVector3 position)
    {
        ActorId = actorId;
        IsVisible = isVisible;
        Position = position;
    }
}