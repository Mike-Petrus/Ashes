public class AbilityStep : CommandStep
{
    private ActorId sourceId;
    private Ability ability;
    private TargetInfo targetInfo;

    public TargetInfo TargetInfo => targetInfo;

    public AbilityStep(ActorId sourceId, Ability ability, TargetInfo targetInfo)
    {
        this.sourceId = sourceId;
        this.ability = ability;
        this.targetInfo = targetInfo;
    }

    public override void Start(BattleContext ctx)
    {
        base.Start(ctx);

        // LATE VALIDATION : occurs at execution rather than command input
        bool isValid = ValidateTarget();

        if (!isValid)
        {
            MutateToWaitStep();
            IsFinished = true;
            return;
        }

        context.Events.Subscribe<AbilityCompletedEvent>(OnAbilityCompleted);
        context.Events.Publish(new AbilityRequestEvent(sourceId, ability, targetInfo));
    }

    private void OnAbilityCompleted(AbilityCompletedEvent e)
    {
        if (e.ActorId != sourceId)
        {
            return;
        }

        context.Events.Unsubscribe<AbilityCompletedEvent>(OnAbilityCompleted);
        IsFinished = true;
    }

    public override void Cancel()
    {
        context.Events.Unsubscribe<AbilityCompletedEvent>(OnAbilityCompleted);
        base.Cancel();
    }

    private bool ValidateTarget()
    {
        if (targetInfo.Mode == TargetingMode.PointAoE)
        {
            return true;
        }

        // 1. If actor-targeted ability then check that actor is still alive
        if (targetInfo.TargetActor.HasValue)
        {
            var targetActor = context.Actors.GetActor(targetInfo.TargetActor.Value);

            // TODO: check ability.CanTargetDead e.g. Resurrection
            if (targetActor == null || !targetActor.IsAlive)
            {
                return false;
            }
        }

        // 2. Ask RangeSystem if target is currently in range
        return context.Range.IsActorInRange(sourceId, ability, targetInfo);
    }

    private void MutateToWaitStep()
    {
        // Give Actor ATB refund
        context.Events.Publish(new ATBChangeRequestEvent(sourceId, ability.RefundPercent, false));

        // TODO: May need to also publish something like AbilityFizzleEvent or CommandMutateEvent
    }
}