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

        if (!context.Simulation.RangeSystem.IsInRange(originPosition, activeActor.Radius, context.SelectedAbility, targetInfo))
        {
            errorMessage = "Out of Range!";
            return false;
        }

        errorMessage = "";
        return true;
    }

    /// <summary>
    /// Consolidates event publishing so every targeting state safely broadcasts 
    /// the correct display position, origin, path, and AoE geometry to the Presentation Layer.
    /// </summary>
    public static void UpdateCursorVisuals(PlayerTurnController context, SimVector3 displayPosition, bool isValid, List<SimVector3> path = null)
    {
        var ability = context.SelectedAbility;
        SimVector3 originPosition = GetOriginPosition(context);

        TargetingMode mode = ability != null ? ability.Mode : TargetingMode.SingleTarget;
        float radius = ability != null ? ability.Radius : 0f;
        float angle = ability != null ? ability.Angle : 0f;

        context.Simulation.Events.Publish(new CursorMovedEvent(
            displayPosition, 
            true, 
            isValid, 
            mode, 
            radius, 
            angle, 
            path: path,
            staticCenter: originPosition
        ));
    }
}