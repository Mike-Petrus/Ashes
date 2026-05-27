using UnityEngine;

public class BattleDebugSystem
{
    private float lastLogTime;
    private ActorRegistry actors;

    public BattleDebugSystem(BattleEventBus events, ActorRegistry actorRegistry)
    {
        actors = actorRegistry;

        events.Subscribe<ActorReadyEvent>(OnActorReady);
        events.Subscribe<CommandStartedEvent>(OnCommandStarted);
        events.Subscribe<CommandStepStartedEvent>(OnStepStarted);
        events.Subscribe<CommandFinishedEvent>(OnCommandFinished);
        events.Subscribe<ActorMovedEvent>(OnActorMoved);
        events.Subscribe<MoveCompletedEvent>(OnMoveCompleted);
        events.Subscribe<AbilityCompletedEvent>(OnAbilityCompleted);
        events.Subscribe<ATBRequestCompletedEvent>(OnATBRequestCompleted);
        events.Subscribe<DamageAppliedEvent>(OnDamageApplied);
        events.Subscribe<HealAppliedEvent>(OnHealApplied);
        events.Subscribe<StatusAppliedEvent>(OnStatusApplied);
        events.Subscribe<StatusExpiredEvent>(OnStatusExpired);
        events.Subscribe<EffectTickRequestEvent>(OnEffectTickRequest);
        events.Subscribe<ResourceConsumedEvent>(OnResourceConsumed);
        events.Subscribe<ActorRegisteredEvent>(OnActorRegistered);
        events.Subscribe<ActorRemovedEvent>(OnActorRemoved);
    }

    private void OnActorReady(ActorReadyEvent e)
    {
        Debug.Log($"[ATB] {actors.GetActor(e.ActorId).Name} ready");
    }

    private void OnCommandStarted(CommandStartedEvent e)
    {
        Debug.Log($"[COMMAND] {actors.GetActor(e.Command.ActorId).Name} started command");
    }

    private void OnStepStarted(CommandStepStartedEvent e)
    {
        Debug.Log($"[STEP] {e.Step.GetType().Name}");
    }

    private void OnCommandFinished(CommandFinishedEvent e)
    {
        Debug.Log($"[COMMAND] {actors.GetActor(e.Command.ActorId).Name} finished command");
    }

    private void OnActorMoved(ActorMovedEvent e)
    {
        if (Time.time - lastLogTime < 0.2f)
        {
            return;
        }

        lastLogTime = Time.time;

        Debug.Log($"[MOVE] {actors.GetActor(e.ActorId).Name} -> {e.Position}");
    }

    private void OnMoveCompleted(MoveCompletedEvent e)
    {
        Debug.Log($"[MOVE] {actors.GetActor(e.ActorId).Name} finished moving (arrived: {actors.GetActor(e.ActorId).Position})");
    }

    private void OnAbilityCompleted(AbilityCompletedEvent e)
    {
        string targetDescription;

        // Check if the ability was targeted at a specific actor
        if (e.TargetInfo.TargetActor.HasValue)
        {
            var targetActor = actors.GetActor(e.TargetInfo.TargetActor.Value);
            // Fallback just in case the actor died and was removed from the registry before completion
            targetDescription = targetActor != null ? targetActor.Name : "Unknown/Dead Actor";
        }
        else
        {
            // It was targeted at a point on the ground!
            targetDescription = $"Position {e.TargetInfo.TargetPosition}";
        }

        Debug.Log($"[ABILITY] {actors.GetActor(e.ActorId).Name} cast {e.Ability.Name} on {targetDescription}");
    }

    private void OnATBRequestCompleted(ATBRequestCompletedEvent e)
    {
        string isNegative = e.IsNegative? "reduced" : "refunded";

        Debug.Log($"[ATB] {actors.GetActor(e.ActorId).Name} ATB {isNegative} {e.RefundPercent}");
    }

    private void OnDamageApplied(DamageAppliedEvent e)
    {
        var source = actors.GetActor(e.SourceId);
        var target = actors.GetActor(e.TargetId);

        Debug.Log($"[DAMAGE] {source.Name} dealt {e.Amount} to {target.Name}");
    }

        private void OnHealApplied(HealAppliedEvent e)
    {
        var source = actors.GetActor(e.SourceId);
        var target = actors.GetActor(e.TargetId);

        Debug.Log($"[Heal] {source.Name} healed {target.Name} for {e.Amount}");
    }
    
    private void OnStatusApplied(StatusAppliedEvent e)
    {
        Debug.Log($"[STATUS] {actors.GetActor(e.TargetId).Name} was afflicted with {e.StatusName}!");
    }

    private void OnStatusExpired(StatusExpiredEvent e)
    {
        Debug.Log($"[STATUS] {e.StatusName} wore off from {actors.GetActor(e.TargetId).Name}.");
    }

    private void OnEffectTickRequest(EffectTickRequestEvent e)
    {
        Debug.Log($"[TICK] {e.StatusName} ticked on {actors.GetActor(e.Context.TargetId).Name}!");
    }

    private void OnResourceConsumed(ResourceConsumedEvent e)
    {
        var actor = actors.GetActor(e.ActorId);
        Debug.Log($"[RESOURCE] {actor.Name} spent {e.Amount} {e.Resource}");
    }

    private void OnActorRegistered(ActorRegisteredEvent e)
    {
        Debug.Log($"[SPAWN] {e.Actor.Name} (ID: {e.Actor.Id.Value}) entered the battle as {e.Actor.Faction}.");
    }

    private void OnActorRemoved(ActorRemovedEvent e)
    {
        Debug.Log($"[DESPAWN] Actor ID {e.ActorId.Value} was removed from the battle.");
    }
}