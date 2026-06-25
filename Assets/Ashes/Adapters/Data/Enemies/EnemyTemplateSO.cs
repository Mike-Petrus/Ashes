using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewEnemyTemplate", menuName = "Ashes/Data/Enemy Template")]
public class EnemyTemplateSO : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Unique identifier used by the database and spawner (e.g., 'Goblin_01')")]
    public string EnemyId;
    public string DefaultName;

    [Header("Physical Properties")]
    public float Radius = 1.0f;

    [Header("Base Attributes")]
    public int Strength = 10;
    public int Aether = 10;
    public int Vitality = 10;
    public int Agility = 5;
    public int Speed = 8;
    public int MoveDistance = 5;

    [Header("Abilities")]
    [Tooltip("Drag AbilityTemplateSOs here. The AI will prioritize them top-to-bottom.")]
    public List<AbilityTemplateSO> Abilities = new List<AbilityTemplateSO>();

    /// <summary>
    /// Converts this Unity asset into a pure C# domain object for the Core Game/Battle Simulation.
    /// </summary>
    public EnemyTemplate ToDomain()
    {
        var attributes = new CoreAttributes
        {
            Strength = this.Strength,
            Aether = this.Aether,
            Vitality = this.Vitality,
            Agility = this.Agility,
            Speed = this.Speed,
            MoveDistance = this.MoveDistance
        };

        var domainAbilities = new List<string>();
        foreach(var ability in Abilities)
        {
            if (ability != null && !string.IsNullOrEmpty(ability.AbilityId))
            {
                domainAbilities.Add(ability.AbilityId);
            }
        }

        return new EnemyTemplate
        {
            EnemyId = this.EnemyId,
            DefaultName = this.DefaultName,
            Radius = this.Radius,
            BaseAttributes = attributes,
            Abilities = domainAbilities // Pass the string IDs to the backend!
        };
    }
}