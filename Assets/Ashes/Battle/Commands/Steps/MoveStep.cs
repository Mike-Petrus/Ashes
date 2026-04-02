public class MoveStep : CommandStep
{
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
        base.Start(ctx);

        start = ctx.Actors.GetActor(actorId).Position;

        context.Events.Subscribe<MoveCompletedEvent>(OnMoveCompleted);
        context.Events.Publish(new MoveRequestEvent(actorId, start, destination));
    }

    private void OnMoveCompleted(MoveCompletedEvent e)
    {
        if (e.ActorId != actorId)
        {
            return;
        }

        context.Events.Unsubscribe<MoveCompletedEvent>(OnMoveCompleted);

        IsFinished = true;
    }

    public override void Cancel()
    {
        context.Events.Unsubscribe<MoveCompletedEvent>(OnMoveCompleted);
        base.Cancel();
    }
}