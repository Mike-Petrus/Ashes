using UnityEngine;
using System;
using System.Collections.Generic;

// --- CONFIGURATION CLASSES ---

[Serializable]
public class PartyMemberConfig
{
    public string CharacterName = "Cecil";
    
    [Header("Core Attributes")]
    public int Strength = 15;
    public int Aether = 15;
    public int Vitality = 20;
    public int Agility = 10;
    public int Speed = 10;
    public int MoveDistance = 6;
}

public enum SandboxItemType { Potion, Ether, PhoenixDown, Elixir }

[Serializable]
public class ItemConfig
{
    public SandboxItemType ItemType = SandboxItemType.Potion;
    public int Quantity = 5;
}

[Serializable]
public class EnemyScenarioConfig
{
    [Tooltip("The Enemy ID from MockEnemyDatabase (e.g., Goblin_01)")]
    public string EnemyId = "Goblin_01";
    
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

    [Header("Enemy Setup")]
    public List<EnemyScenarioConfig> Enemies = new List<EnemyScenarioConfig>();

    [Header("Spawn Locations")]
    [Tooltip("Seed used to generate random enemy spawns. Change this to generate a new layout.")]
    public int RandomSeed = 12345;
    [Tooltip("Check this box to automatically generate a new random seed.")]
    public bool GenerateNewSeed = false;
    
    public bool UseManualSpawnPoints = false;
    [Tooltip("Drag empty GameObjects here to act as exact spawn coordinates for enemies.")]
    public List<Transform> ManualSpawnPoints = new List<Transform>();

    // Shared Formation Data
    private readonly int[] formationOffsets = { 0, -1, 1, -2, 2 };
    private readonly float partySpacing = 1.5f;

    // Unity Editor callback: Runs whenever a value is changed in the Inspector
    private void OnValidate()
    {
        if (GenerateNewSeed)
        {
            RandomSeed = UnityEngine.Random.Range(1, 999999);
            GenerateNewSeed = false; // Uncheck it immediately like a button
        }
    }

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
        Vector3 right = EncounterCenter.right;

        int totalActors = PartyMembers.Count + Enemies.Count;
        float radius = GetArenaRadius(totalActors);

        // 1. Draw Arena Boundary
        Gizmos.color = Color.yellow;
        DrawWireCircle(center, radius, 36);

        // 2. Draw Division Line
        Vector3 divisionAxis = new Vector3(-forward.z, 0, forward.x).normalized;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(center - divisionAxis * radius, center + divisionAxis * radius);

        // 3. Draw Party Formation Preview
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

        // 4. Draw Enemy Hemisphere (Threat Vector)
        Gizmos.color = Color.red;
        Gizmos.DrawLine(center, center + forward * (radius * 0.5f));
        
        // 5. Draw Random OR Manual Spawns
        System.Random previewRand = new System.Random(RandomSeed);

        for (int i = 0; i < Enemies.Count; i++)
        {
            Vector3 spawnPos;
            if (UseManualSpawnPoints && i < ManualSpawnPoints.Count && ManualSpawnPoints[i] != null)
            {
                spawnPos = ManualSpawnPoints[i].position;
                Gizmos.color = Color.magenta;
            }
            else
            {
                // Calculate exactly how the spawner does!
                float randomForward = (float)previewRand.NextDouble() * (radius - 3f) + 1f;
                float randomSide = (float)(previewRand.NextDouble() * 2.0 - 1.0) * (radius - 3f);
                spawnPos = center + (forward * randomForward) + (divisionAxis * randomSide);
                Gizmos.color = Color.red;
            }

            Gizmos.DrawSphere(spawnPos, 0.5f);
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