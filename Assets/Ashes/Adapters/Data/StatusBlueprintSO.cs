using UnityEngine;

public enum StatusTickType { None, Damage, Heal }

[CreateAssetMenu(fileName = "NewStatusBlueprint", menuName = "Ashes/Data/Status Blueprint")]
public class StatusBlueprintSO : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("The exact string ID the engine recognizes (e.g., 'Poison', 'Haste')")]
    public string StatusName;
    public bool IsBuff;

    [Header("Timing")]
    public float DefaultDuration = 15f;
    [Tooltip("How often this effect ticks in seconds. Use 0 for passive buffs like Haste.")]
    public float TickInterval = 3f;

    [Header("Tick Behavior")]
    [Tooltip("What type of payload should be generated when this status ticks?")]
    public StatusTickType TickType = StatusTickType.Damage;

    // TODO: In the future add a list of Stat Modifiers here for things like Haste/Slow!
}