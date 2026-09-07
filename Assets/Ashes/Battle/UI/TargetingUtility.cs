using System.Collections.Generic;

public static class TargetingUtility
{
    /// <summary>
    /// Returns the Caster's position, or their future position if a MoveStep is queued.
    /// </summary>
    public static SimVector3 GetOriginPosition(PlayerTurnController context)
    {
        var activeActor = context.Simulation.Actors.GetActor(context.ActiveActorId.Value);
        SimVector3 originPosition = activeActor.Position;

        if (context.Builder.Size > 0 && context.Builder.LastStepAdded() is MoveStep moveStep)
        {
            originPosition = moveStep.Destination;
        }

        return originPosition;
    }

    /// <summary>
    /// STRICT Validation for standard Move targeting. Returns false if unreachable, too far, or occupied.
    /// </summary>
    public static bool TryValidateStandardMove(PlayerTurnController context, SimVector3 destination, out List<SimVector3> path, out string errorMessage)
    {
        var activeActor = context.Simulation.Actors.GetActor(context.ActiveActorId.Value);
        path = context.Simulation.Pathfinder.FindPath(activeActor.Position, destination, activeActor.Radius);

        if (path == null || path.Count == 0)
        {
            errorMessage = "Unreachable!";
            return false;
        }

        float pathDistance = 0f;
        for (int i = 0; i < path.Count - 1; i++)
        {
            pathDistance += SimVector3.Distance(path[i], path[i + 1]);
        }

        if (pathDistance > activeActor.Stats.MoveDistance)
        {
            errorMessage = "Too Far!";
            return false;
        }

        if (context.Simulation.PositionSystem.IsSpaceOccupied(destination, activeActor.Radius, context.ActiveActorId.Value))
        {
            errorMessage = "Space Occupied!";
            return false;
        }

        errorMessage = "";
        return true;
    }

    /// <summary>
    /// FORGIVING Path Generation for Pursuit Mode previews. Leverages the TacticalPathfindingUtility.
    /// </summary>
    public static List<SimVector3> GeneratePursuitPreview(PlayerTurnController context, TargetInfo targetInfo, Ability ability)
    {
        var sourceActor = context.Simulation.Actors.GetActor(context.ActiveActorId.Value);
        SimVector3 finalDestination;

        // Simulate exactly what BattleCommandExecutor will do dynamically
        if (targetInfo.TargetActor.HasValue)
        {
            var targetActor = context.Simulation.Actors.GetActor(targetInfo.TargetActor.Value);
            
            if (TacticalPathfindingUtility.TryCalculateMoveDestination(sourceActor, targetActor, ability, context.Simulation.BattleContext, out SimVector3 validDest))
            {
                finalDestination = validDest;
            }
            else
            {
                List<SimVector3> testPoints = TacticalPathfindingUtility.GetTestPoints(sourceActor, targetActor);
                SimVector3 bestPoint = testPoints.Count > 0 ? testPoints[0] : targetActor.Position;
                finalDestination = TacticalPathfindingUtility.CalculatePartialMove(sourceActor, bestPoint, context.Simulation.BattleContext);
            }
        }
        else
        {
            finalDestination = TacticalPathfindingUtility.CalculatePartialMove(sourceActor, targetInfo.TargetPosition, context.Simulation.BattleContext);
        }

        // Return the path to this dynamically generated point
        return context.Simulation.Pathfinder.FindPath(sourceActor.Position, finalDestination, sourceActor.Radius) ?? new List<SimVector3>();
    }

    /// <summary>
    /// Validates if a target is within the ability's range from the active actor's origin position.
    /// Can be used by TargetingActorState, TargetingFreeAimState, and TargetingSelfState.
    /// </summary>
    public static bool IsTargetInRange(PlayerTurnController context, TargetInfo targetInfo, out string errorMessage)
    {
        var activeActor = context.Simulation.Actors.GetActor(context.ActiveActorId.Value);
        SimVector3 originPosition = GetOriginPosition(context);

        // --- DIRECTIONAL BYPASS ---
        // You can aim a cone in any direction. The cone's radius handles the physical limits.
        // TODO: Find a more elegant solution??? 
        if (context.SelectedAbility.Mode == TargetingMode.Directional)
        {
            errorMessage = "";
            return true; 
        }

        return context.Simulation.RangeSystem.IsInRange(originPosition, activeActor.Radius, context.SelectedAbility, targetInfo, out errorMessage);
    }

    /// <summary>
    // Unified Visualization Hub. This replaces UpdateCursorVisuals.
    // It answers all queries: Caster origin, decel valid, caught targets, and highlight outcomes.
    /// </summary>
    /// projectorCenter // Center of the decal (unsnapped or snapped ID pos)
    /// movePath // Path visualization (if Pursuit tethers)
    /// snappedTargetId // Passes snapped ID for hybrid AoE center calculations
    public static void UpdateTargetVisuals(PlayerTurnController context, SimVector3 projectorCenter, bool projectorIsValid, List<SimVector3> movePath = null, ActorId? snappedTargetId = null)
    {
        BattleSimulation simulation = context.Simulation;
        Ability ability = context.SelectedAbility;

        // 1. Establish Origin (accounts for MoveStep tethers)
        SimVector3 originPosition = GetOriginPosition(context);
        var activeActor = simulation.Actors.GetActor(context.ActiveActorId.Value);       

        // 2. Publish original ground projector event.
        // extractor logic:extract decal mode/radius/angle/valid
        TargetingMode projectorMode = ability != null ? ability.Mode : TargetingMode.SingleTarget;
        float radius = ability != null ? ability.Radius : activeActor.Radius;
        float angle = ability != null ? ability.Angle : 0f;

        simulation.Events.Publish(new CursorMovedEvent(projectorCenter, true, projectorIsValid, projectorMode, radius, angle, path: movePath, staticCenter: originPosition));
        simulation.Events.Publish(new TargetingFocusChangedEvent(snappedTargetId));

        // 3. --- VISUAL IMPACT GATHERING PIPELINE (O(Actors)) ---

        // If it's a movement command (no ability), don't highlight actors
        if (ability == null)
        {
            simulation.Events.Publish(new TargetingImpactsChangedEvent(null));
            return;
        }

        TargetInfo targetInfo;
        // In Hybrid AoE modes, if snapped ON, that actor is the AoE center. If snapped OFF, floor is center.
        if (snappedTargetId.HasValue)
        {
            targetInfo = TargetInfo.ForActor(snappedTargetId.Value, projectorMode);
        }
        else
        {
            targetInfo = TargetInfo.ForPosition(projectorCenter, projectorMode);
        }

        // A. Ask existing Pure C# Targeting System for raw affect targets List<ActorId>
        // Note: For visualization, we always pass alignment everyone/all to calculate collateral!
        // GetAffectedTargets must return *Everyone* physically inside the zone.
        var affectedActorIds = simulation.TargetingSystem.GetAffectedTargets(activeActor.Id, targetInfo, ability, TargetAlignment.Everyone);

        // B. Perform pre-emptive Impact Evaluation and standard Outcome Sorting.
        // We use the NEW Ability SO metadata (Heal/Harm) to perform the math inside C#.
        var visualizationDTOs = SortTargetsByOutcomeColor(context.Simulation, activeActor, ability, affectedActorIds);

        // C. Publish the specific highlights semantic event!
        simulation.Events.Publish(new TargetingImpactsChangedEvent(visualizationDTOs));
    }

    /// <summary>
    // Standardized Outcome-Based Sorting. This logic *defines* what Red, Yellow, Blue, Green mean.
    // Leveraging the NEW Ability ImpactType/ElementType metadata.
    /// </summary>
    private static List<TargetVisualImpact> SortTargetsByOutcomeColor(
        BattleSimulation sim, BattleActor sourceActor, Ability ability, List<ActorId> affectedActorIds)
    {
        List<TargetVisualImpact> visualList = new List<TargetVisualImpact>();

        foreach (var id in affectedActorIds)
        {
            var targetActor = sim.Actors.GetActor(id);
            if (targetActor == null) continue;

            bool isAlly = sourceActor.Faction == targetActor.Faction;
            bool isSelf = sourceActor.Id == targetActor.Id;

            // --- ABILITY ALIGNMENT CHECK ---
            // If the ability mathematically cannot hit this target, they shouldn't be highlighted at all!
            bool isTargetValidForAlignment = false;
            switch (ability.Alignment)
            {
                case TargetAlignment.Everyone: isTargetValidForAlignment = true; break;
                case TargetAlignment.Enemy: isTargetValidForAlignment = !isAlly; break;
                case TargetAlignment.Ally: isTargetValidForAlignment = isAlly; break;
                case TargetAlignment.SelfOnly: isTargetValidForAlignment = isSelf; break;
            }

            if (!isTargetValidForAlignment)
            {
                // Skip highlighting them completely
                continue;
            }

            OutcomeColor visualOutcome = OutcomeColor.None;

            // Simple Phase 10 logic based on the ability's intention.
            // Undead inversion math happens inside this check in later phases!
            bool abilityIsHostileIntention = ability.ImpactType == ImpactType.Damage;

            if (abilityIsHostileIntention)
            {
                // Case A: Damage Ability. Enemy gets hurt (Red), Cecil gets friendly fire (Yellow).
                // Assuming Undead and Absorption aren't present yet for MVP.
                visualOutcome = !isAlly ? OutcomeColor.IntendedHarm : OutcomeColor.UnintendedHarm;
            }
            else
            {
                // Case B: Healing Ability. Cecil gets healed (Green), Enemy waste/heal (Blue).
                visualOutcome = isAlly ? OutcomeColor.IntendedHelp : OutcomeColor.UnintendedHelp;
            }
            
            visualList.Add(new TargetVisualImpact(id, visualOutcome));
        }

        return visualList;
    }
}