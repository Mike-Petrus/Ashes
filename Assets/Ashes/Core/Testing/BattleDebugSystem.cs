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

        events.Subscribe<DamageAppliedEvent>(OnDamageApplied);
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

    private void OnDamageApplied(DamageAppliedEvent e)
    {
        var source = actors.GetActor(e.SourceId);
        var target = actors.GetActor(e.TargetId);

        Debug.Log($"[DAMAGE] {source.Name} dealt {e.Amount} to {target.Name}");
    }
}