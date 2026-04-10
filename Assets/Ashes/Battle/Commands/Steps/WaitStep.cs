public class WaitStep : CommandStep
{
    private ActorId actorId;

    public WaitStep(ActorId actorId)
    {
        this.actorId = actorId;
    }

    public override void Start(BattleContext ctx)
    {
        base.Start(ctx);

        // TODO: decide how much ATB to refund
        // To start this is probably a flat value 25-50%
        // could change under certain conditions

        // TODO: publish ATBRefundEvent
        // use context.Events.Publish(new ATBChangeRequestEvent(sourceId, RefundPercent, false))
        // or decide if this has its own unique event
    }
}