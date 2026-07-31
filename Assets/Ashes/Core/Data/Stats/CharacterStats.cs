using System.Collections.Generic;

public class CharacterStats
{
    public CoreAttributes BaseStats { get; private set; }

    public int CurrentHP { get; set; }
    public int CurrentMP { get; set; }

    private Dictionary<StatType, List<StatModifier>> modifiers = new();
    private Dictionary<ElementType, float> ElementalAffinities = new();     // 1.0f = Normal, 0.5f = 50% resist, 2.0f = 200% weak

    public CharacterStats(CoreAttributes baseStats)
    {
        BaseStats = baseStats;

        ElementalAffinities[ElementType.Physical] = 1.0f;

        CurrentHP = MaxHP;
        CurrentMP = MaxMP;
    }

    // Base Modifier logic
    public float GetEffectiveStat(StatType statType, float baseValue)
    {
        if (!modifiers.ContainsKey(statType))
        {
            return baseValue;
        }

        float flat = 0f;
        float percent = 0f;

        foreach (var mod in modifiers[statType])
        {
            flat += mod.FlatValue;
            percent += mod.PercentValue;
        }

        return (baseValue + flat) * (1f + percent);
    }

    // Core Atrributes (buffable)
    public int Strength => (int)GetEffectiveStat(StatType.Strength, BaseStats.Strength);
    public int Aether => (int)GetEffectiveStat(StatType.Aether, BaseStats.Aether);
    public int Vitality => (int)GetEffectiveStat(StatType.Vitality, BaseStats.Vitality);
    public int Agility => (int)GetEffectiveStat(StatType.Agility, BaseStats.Agility);
    public float Speed => GetEffectiveStat(StatType.Speed, BaseStats.Speed);
    public float MoveDistance => GetEffectiveStat(StatType.MoveDistance, BaseStats.MoveDistance);

    // Calculated Stats
    public int MaxHP => (int)GetEffectiveStat(StatType.MaxHP, Vitality * 10f);
    public int MaxMP => (int)GetEffectiveStat(StatType.MaxMP, Aether * 5f);

    public int Defense => (int)GetEffectiveStat(StatType.Defense, Vitality);            // Armor mods are added to this key
    public int MagicResist => (int)GetEffectiveStat(StatType.MagicResist, Aether); 

    // Percentages (Stored as floats, e.g., 0.05f = 5%)
    public float AttackCrit => GetEffectiveStat(StatType.AttackCrit, Agility * 0.01f); 
    public float MagicCrit => GetEffectiveStat(StatType.MagicCrit, 0f);                 // Purely gear/buff driven
    public float DodgeChance => GetEffectiveStat(StatType.DodgeChance, Agility * 0.01f); 
    
    // Shield Mechanics
    public float BlockChance => GetEffectiveStat(StatType.BlockChance, 0f);             // Purely gear/buff driven
    public int BlockValue => (int)GetEffectiveStat(StatType.BlockValue, Strength * 0.5f);

    // Power Calculation
    public int GetAttackPower(ScalingStat scalingStat = ScalingStat.Strength) 
    {
        int baseScalingValue;

        switch (scalingStat)
        {
            case ScalingStat.Strength:
                baseScalingValue = Strength;
                break;
            case ScalingStat.Agility:
                baseScalingValue = Agility;
                break;
            case ScalingStat.Aether:
                baseScalingValue = Aether;
                break;
            case ScalingStat.Vitality:
                baseScalingValue = Vitality;
                break;
            default:
                baseScalingValue = Strength;
                break;
        }
        return (int)GetEffectiveStat(StatType.AttackPower, baseScalingValue);       // Weapon damage added as flat mod
    }

    public int GetMagicPower(ScalingStat scalingStat = ScalingStat.Aether)
    {
        int baseScalingValue;

        switch (scalingStat)
        {
            case ScalingStat.Aether:
                baseScalingValue = Aether;
                break;
            case ScalingStat.Strength:
                baseScalingValue = Strength;
                break;
            case ScalingStat.Vitality:
                baseScalingValue = Vitality;
                break;
            case ScalingStat.Agility:
                baseScalingValue = Agility;
                break;
            default:
                baseScalingValue = Aether;
                break;
        }
        return (int)GetEffectiveStat(StatType.MagicPower, baseScalingValue);            // Wand/Staff damage added as flat mod
    }

    public void AddModifier(StatType statType, StatModifier modifier)
    {
        if (!modifiers.ContainsKey(statType))
        {
            modifiers[statType] = new List<StatModifier>();
        }
        modifiers[statType].Add(modifier);
    }

    public void RemoveModifiersFromSource(object source)
    {
        // Loop through every stat and remove any modifiers that came from this source
        foreach (var statList in modifiers.Values)
        {
            statList.RemoveAll(mod => mod.Source == source);
        }
    }
}