public class CombatSystem
{
    public EventBus events;
    public ActorRegistry actors;

    public CombatSystem(EventBus eventBus, ActorRegistry actorRegistry)
    {
        events = eventBus;
        actors = actorRegistry;

        events.Subscribe<DamageRequestEvent>(OnDamageRequest);
    }

    private void OnDamageRequest(DamageRequestEvent e)
    {
        var target = actors.GetActor(e.TargetId);

        target.Health -= e.Amount;

        events.Publish(new DamageAppliedEvent(e.SourceId, e.TargetId, e.Amount));
        
        if (target.Health <= 0)
        {
            events.Publish(new ActorDiedEvent(e.TargetId));
        }
    }
}