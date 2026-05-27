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

        context.Events.Subscribe<ATBRequestCompletedEvent>(OnATBRequestCompleted);
        context.Events.Publish(new ATBChangeRequestEvent(actorId, 0.50f, false));
    }

    private void OnATBRequestCompleted(ATBRequestCompletedEvent e)
    {
        if (e.ActorId != actorId)
        {
            return;
        }

        context.Events.Unsubscribe<ATBRequestCompletedEvent>(OnATBRequestCompleted);

        IsFinished = true;
    }
}