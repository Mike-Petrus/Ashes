using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class AbilityUnlockConfig
{
    [Tooltip("The level at which this ability is unlocked. Use 1 for starting abilities.")]
    public int RequiredLevel = 1;
    
    [Tooltip("Drag the AbilityTemplateSO here!")]
    public AbilityTemplateSO AbilityPreset;
}

[CreateAssetMenu(fileName = "NewClassTemplate", menuName = "Ashes/Data/Class Template")]
public class ClassTemplateSO : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Unique identifier used by the save system (e.g., 'Paladin_01')")]
    public string ClassId;
    public string ClassName;

    [Header("Base Attributes")]
    public int Strength = 10;
    public int Aether = 10;
    public int Vitality = 10;
    public int Agility = 10;
    public int Speed = 10;
    public int MoveDistance = 5;

    [Header("Learned Abilities")]
    [Tooltip("List of abilities this class learns as they level up.")]
    public List<AbilityUnlockConfig> LearnedAbilities = new List<AbilityUnlockConfig>();

    /// <summary>
    /// Converts this Unity asset into a pure C# domain object for the Core Engine.
    /// </summary>
    public ClassTemplate ToDomain()
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

        var domainAbilities = new List<AbilityUnlock>();
        foreach (var config in this.LearnedAbilities)
        {
            if (config.AbilityPreset != null && !string.IsNullOrEmpty(config.AbilityPreset.AbilityId))
            {
                domainAbilities.Add(new AbilityUnlock
                {
                    RequiredLevel = config.RequiredLevel,
                    AbilityId = config.AbilityPreset.AbilityId // Extracts the pure string ID!
                });
            }
        }

        return new ClassTemplate
        {
            ClassId = this.ClassId,
            ClassName = this.ClassName,
            BaseStats = attributes,
            LearnedAbilities = domainAbilities
        };
    }
}