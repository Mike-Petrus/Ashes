public class MoveStep : CommandStep
{
    private EventBus events;

    private ActorId actorId;
    private SimVector3 start;
    private SimVector3 destination;

    public MoveStep(ActorId actorId, SimVector3 destination)
    {
        this.actorId = actorId;
        this.destination = destination;
    }

    public override void Start(BattleContext ctx)
    {
        events = ctx.Events;

        start = ctx.Actors.GetActor(actorId).Position;

        events.Subscribe<MoveCompletedEvent>(OnMoveCompleted);
        events.Publish(new MoveRequestEvent(actorId, start, destination));
    }

    private void OnMoveCompleted(MoveCompletedEvent e)
    {
        if (e.ActorId != actorId)
        {
            return;
        }

        events.Unsubscribe<MoveCompletedEvent>(OnMoveCompleted);

        IsFinished = true;
    }

    public override void Cancel(BattleContext ctx)
    {
        events?.Unsubscribe<MoveCompletedEvent>(OnMoveCompleted);
        base.Cancel(ctx);
    }
}