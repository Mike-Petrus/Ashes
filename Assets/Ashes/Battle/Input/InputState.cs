public enum InputState
{
    Idle,                   // Watching the battle. No input needed.
    PartySelection,         // ATB is full. D-Pad cycles through ready party members.
    
    // Command Phase 1: The root menu for the first action of the turn
    RootMenuPhase1,        // Choosing: Attack, Magic, Item, Move
    
    // Command Phase 2: The root menu for the second action (filters out what they already did)
    RootMenuPhase2,        // Choosing: Attack, Magic, Wait OR Move, Wait
    
    // Sub-Menus
    // AbilityCategoryMenu,    // Navigating: White Magic, Black Magic, etc.
    AbilitySelectionMenu,   // Navigating specific spells (Pre-validation happens here!)
    ItemSelectionMenu,      // Navigating Inventory
    
    // Targeting
    TargetingActor,        // Selecting an enemy or ally
    TargetingMove          // Selecting a destination vector
}