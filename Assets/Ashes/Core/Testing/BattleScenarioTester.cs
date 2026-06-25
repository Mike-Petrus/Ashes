using UnityEngine;
using System;
using System.Collections.Generic;

// --- CONFIGURATION CLASSES ---

[Serializable]
public class PartyMemberConfig
{
    public string CharacterName = "Cecil";
    
    [Tooltip("Drag a Class Template SO here")]
    public ClassTemplateSO ClassPreset;
}

[Serializable]
public class ItemConfig
{
    [Tooltip("Drag an Item Template SO here")]
    public ItemTemplateSO ItemPreset;
    public int Quantity = 5;
}

[Serializable]
public class EnemyScenarioConfig
{
    [Tooltip("Drag an Enemy Template SO here")]
    public EnemyTemplateSO EnemyPreset;
    
    [Header("Stat Overrides")]
    public bool OverrideStats = false;
    public int Vitality = 10; // Drives MaxHP
    public int Aether = 10;   // Drives MaxMP
    public int Speed = 10;
    public int MoveDistance = 5;
}

// --- MAIN MONOBEHAVIOUR ---

public class BattleScenarioTester : MonoBehaviour
{
    [Header("Testing Sandbox")]
    public bool IsEnabled = true;

    [Header("Encounter Center & Alignment")]
    [Tooltip("Place an empty GameObject here. Its position is the collision center, and its Z-Forward is the player's approach direction.")]
    public Transform EncounterCenter;

    [Header("Arena Sizing")]
    public bool OverrideArenaRadius = false;
    public float CustomArenaRadius = 15f;

    [Header("Party Setup")]
    public List<PartyMemberConfig> PartyMembers = new List<PartyMemberConfig>();
    public List<ItemConfig> InventoryItems = new List<ItemConfig>();

    [Header("Encounter Blueprint")]
    public bool UseEncounterBlueprint = false;
    [Tooltip("If true, generates a random encounter based on this blueprint instead of the manual enemy list.")]
    public EncounterBlueprintSO EncounterBlueprint;

    [Header("Enemy Setup")]
    [Tooltip("These enemies will only spawn if UseEncounterBlueprint is FALSE.")]
    public List<EnemyScenarioConfig> Enemies = new List<EnemyScenarioConfig>();

    [Header("Spawn Locations & Randomization")]
    [Tooltip("Seed used to generate random enemy spawns. Right-click this script's header and select 'Generate New Random Seed' to shuffle!")]
    public int RandomSeed = 12345;
    [Tooltip("Check this box to automatically generate a new random seed.")]
    public bool GenerateNewSeed = false;
    
    public bool UseManualSpawnPoints = false;
    [Tooltip("Drag empty GameObjects here to act as exact spawn coordinates for enemies.")]
    public List<Transform> ManualSpawnPoints = new List<Transform>();

    // Shared Formation Data
    private readonly int[] formationOffsets = { 0, -1, 1, -2, 2 };
    private readonly float partySpacing = 1.5f;

    // Restored OnValidate so the Inspector checkbox updates the Gizmos!
    private void OnValidate()
    {
        if (GenerateNewSeed)
        {
            RandomSeed = UnityEngine.Random.Range(10000, 999999);
            GenerateNewSeed = false; 
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Generate New Random Seed")]
    private void GenerateNewSeedMenu()
    {
        RandomSeed = UnityEngine.Random.Range(10000, 999999);
        UnityEditor.SceneView.RepaintAll();
    }
#endif

    public float GetArenaRadius(int totalActors)
    {
        if (OverrideArenaRadius) return CustomArenaRadius;
        return Mathf.Clamp(10f + (totalActors * 2.0f), 15f, 30f); 
    }

    private void OnDrawGizmos()
    {
        if (!IsEnabled || EncounterCenter == null) return;

        Vector3 center = EncounterCenter.position;
        Vector3 forward = EncounterCenter.forward;
        Vector3 divisionAxis = new Vector3(-forward.z, 0, forward.x).normalized;

        // 1. Roll Random Seed to determine exact enemy count FIRST
        System.Random previewRand = new System.Random(RandomSeed);
        int previewEnemyCount = 0;

        if (UseEncounterBlueprint && EncounterBlueprint != null)
        {
            EncounterData mockData = EncounterBlueprint.ToDomain(previewRand);
            previewEnemyCount = mockData.EnemyIds.Count;
        }
        else
        {
            previewEnemyCount = Enemies.Count;
        }

        // Cap enemies to the amount of manual spawn points provided
        if (UseManualSpawnPoints)
        {
            previewEnemyCount = Mathf.Min(previewEnemyCount, ManualSpawnPoints.Count);
        }

        // 2. Calculate Exact Arena Radius based on the RNG result
        int totalActors = PartyMembers.Count + previewEnemyCount;
        float radius = GetArenaRadius(totalActors);

        // 3. Draw Arena Boundary
        Gizmos.color = Color.yellow;
        DrawWireCircle(center, radius, 36);

        // 4. Draw Division Line
        Gizmos.color = Color.red;
        Gizmos.DrawLine(center - divisionAxis * radius, center + divisionAxis * radius);

        // 5. Draw Party Formation Preview
        Gizmos.color = Color.blue;
        Vector3 partyBaseLine = center - (forward * 2f); 
        
        for (int i = 0; i < PartyMembers.Count; i++)
        {
            if (i >= formationOffsets.Length) break;
            
            Vector3 spawnPos = partyBaseLine + (divisionAxis * formationOffsets[i] * partySpacing);
            Gizmos.DrawSphere(spawnPos, 0.5f);
            
            Gizmos.color = new Color(0, 0, 1, 0.3f);
            Gizmos.DrawLine(center, spawnPos);
            Gizmos.color = Color.blue;
        }

        // 6. Draw Threat Vector
        Gizmos.color = Color.red;
        Gizmos.DrawLine(center, center + forward * (radius * 0.5f));
        
        // 7. Draw Enemy Spawns (Perfectly isolated RNG!)
        if (UseManualSpawnPoints)
        {
            for (int i = 0; i < ManualSpawnPoints.Count; i++)
            {
                if (ManualSpawnPoints[i] != null)
                {
                    // Draw used points as magenta, unused as gray
                    Gizmos.color = i < previewEnemyCount ? Color.magenta : new Color(0.5f, 0.5f, 0.5f, 0.5f);
                    Gizmos.DrawSphere(ManualSpawnPoints[i].position, 0.5f);
                }
            }
        }
        else
        {
            for (int i = 0; i < previewEnemyCount; i++)
            {
                // Flawless Semicircle Math using EXACTLY 1.0f offset!
                float padding = 1.0f;
                float randomForward = (float)previewRand.NextDouble() * (radius - (padding * 2)) + padding;
                float maxSide = (float)Math.Sqrt(Math.Pow(radius - padding, 2) - Math.Pow(randomForward, 2));
                float randomSide = (float)(previewRand.NextDouble() * 2.0 - 1.0) * maxSide;

                Vector3 spawnPos = center + (forward * randomForward) + (divisionAxis * randomSide);
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(spawnPos, 0.5f);
            }
        }
    }

    private void DrawWireCircle(Vector3 center, float radius, int segments)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(Mathf.Sin(0) * radius, 0, Mathf.Cos(0) * radius);

        for (int i = 1; i <= segments; i++)
        {
            float rad = Mathf.Deg2Rad * (i * angleStep);
            Vector3 newPoint = center + new Vector3(Mathf.Sin(rad) * radius, 0, Mathf.Cos(rad) * radius);
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
}