using System.Collections.Generic;

public class MoveStep : CommandStep
{
    private ActorId actorId;
    private SimVector3 start;
    private SimVector3 destination;

    public List<SimVector3> CachedPath;
    public SimVector3 Destination => destination;

    public MoveStep(ActorId actorId, SimVector3 destination, List<SimVector3> cachedPath = null)
    {
        this.actorId = actorId;
        this.destination = destination;
        CachedPath = cachedPath != null ? new List<SimVector3>(cachedPath) : new List<SimVector3>();
    }

    public override void Start(BattleContext ctx)
    {
        base.Start(ctx);

        start = ctx.Actors.GetActor(actorId).Position;

        context.Events.Subscribe<MoveCompletedEvent>(OnMoveCompleted);
        context.Events.Publish(new MoveRequestEvent(actorId, start, destination)); // TODO: Add cached path
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