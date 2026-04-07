using System.Collections.Generic;

public class CharacterStats
{
    public CoreAttributes BaseStats { get; private set; }

    public int CurrentHP { get; set; }
    public int CurrentMP { get; set; }

    private Dictionary<string, List<StatModifier>> modifiers = new();
    private Dictionary<ElementType, float> ElementalAffinities = new();     // 1.0f = Normal, 0.5f = 50% resist, 2.0f = 200% weak

    public CharacterStats(CoreAttributes baseStats)
    {
        BaseStats = baseStats;

        ElementalAffinities[ElementType.Physical] = 1.0f;

        CurrentHP = MaxHP;
        CurrentMP = MaxMP;
    }

    // Base Modifier logic
    public float GetEffectiveStat(string statName, float baseValue)
    {
        if (!modifiers.ContainsKey(statName))
        {
            return baseValue;
        }

        float flat = 0f;
        float percent = 0f;

        foreach (var mod in modifiers[statName])
        {
            flat += mod.FlatValue;
            percent += mod.PercentValue;
        }

        return (baseValue + flat) * (1f + percent);
    }

    // Core Atrributes (buffable)
    public int Strength => (int)GetEffectiveStat(nameof(Strength), BaseStats.Strength);
    public int Aether => (int)GetEffectiveStat(nameof(Aether), BaseStats.Aether);
    public int Vitality => (int)GetEffectiveStat(nameof(Vitality), BaseStats.Vitality);
    public int Agility => (int)GetEffectiveStat(nameof(Agility), BaseStats.Agility);
    public float Speed => GetEffectiveStat(nameof(Speed), BaseStats.Speed);
    public float MoveDistance => GetEffectiveStat(nameof(MoveDistance), BaseStats.MoveDistance);

    // Calculated Stats
    public int MaxHP => (int)GetEffectiveStat(nameof(MaxHP), Vitality * 10f);
    public int MaxMP => (int)GetEffectiveStat(nameof(MaxMP), Aether * 5f);

    public int Defense => (int)GetEffectiveStat(nameof(Defense), Vitality);             // Armor mods are added to this string key
    public int MagicResist => (int)GetEffectiveStat(nameof(MagicResist), Aether); 

    // Percentages (Stored as floats, e.g., 0.05f = 5%)
    public float AttackCrit => GetEffectiveStat(nameof(AttackCrit), Agility * 0.01f); 
    public float MagicCrit => GetEffectiveStat(nameof(MagicCrit), 0f);                  // Purely gear/buff driven
    public float DodgeChance => GetEffectiveStat(nameof(DodgeChance), Agility * 0.01f); 
    
    // Shield Mechanics
    public float BlockChance => GetEffectiveStat(nameof(BlockChance), 0f);              // Purely gear/buff driven
    public int BlockValue => (int)GetEffectiveStat(nameof(BlockValue), Strength * 0.5f);

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

        return (int)GetEffectiveStat("AttackPower", baseScalingValue);       // Weapon damage added as flat mod
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

        return (int)GetEffectiveStat("MagicPower", baseScalingValue);             // Wand/Staff damage added as flat mod
    }
}