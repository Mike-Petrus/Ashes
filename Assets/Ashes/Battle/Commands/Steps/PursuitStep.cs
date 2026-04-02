public class PursuitStep : CommandStep
{
    // TODO: Implement lol...
    // PursuitStep is an intent container.
    // It temporarily stores a Navmesh path to build a MoveStep
    // And Ability/Target info to build an AbilityStep

    // When start is called it checks the target's current position and validates the path
    // If the target has moved it asks for a new path to the target or as close to the target as possible
    // It then injects a new MoveStep and AbilityStep (or WaitStep if out of range) into the BattleCommand
    // PursuitStep then marks itself finished and the Executor naturally moves to the new MoveStep
}