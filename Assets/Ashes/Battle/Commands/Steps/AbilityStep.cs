public class AbilityStep : CommandStep
{
    private EventBus events;
    private ActorId actorId;
    private Ability ability;
    private ActorId targetId;

    public AbilityStep(ActorId actor, Ability ability, ActorId target)
    {
        this.actorId = actor;
        this.ability = ability;
        this.targetId = target;
    }

    public override void Start(BattleContext ctx)
    {
        events = ctx.Events;

        ctx.Events.Subscribe<AbilityCompletedEvent>(OnAbilityCompleted);
        ctx.Events.Publish(new AbilityRequestEvent(actorId, ability, targetId));
    }

    private void OnAbilityCompleted(AbilityCompletedEvent e)
    {
        if (e.ActorId != actorId)
        {
            return;
        }

        events.Unsubscribe<AbilityCompletedEvent>(OnAbilityCompleted);
        IsFinished = true;
    }

    public override void Cancel(BattleContext ctx)
    {
        ctx.Events.Unsubscribe<AbilityCompletedEvent>(OnAbilityCompleted);
        base.Cancel(ctx);
    }
}