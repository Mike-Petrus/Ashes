using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewEncounterBlueprint", menuName = "Ashes/Data/Encounter Blueprint")]
public class EncounterBlueprintSO : ScriptableObject
{
    [Header("Identity")]
    public string EncounterId;
    public bool IsRandom = true;
    
    [Header("Spawn Rules")]
    public int MinEnemies = 2;
    public int MaxEnemies = 5;
    
    [Header("Enemy Pool")]
    [Tooltip("The spawner will randomly pick enemies from this list.")]
    public List<EnemyTemplateSO> PossibleEnemies = new List<EnemyTemplateSO>();

    /// <summary>
    /// Generates a pure C# EncounterData instance based on a provided random seed.
    /// </summary>
    public EncounterData ToDomain(System.Random rand)
    {
        EncounterData data = new EncounterData();
        
        int enemyCount = rand.Next(MinEnemies, MaxEnemies + 1);
        for (int i = 0; i < enemyCount; i++)
        {
            if (PossibleEnemies.Count == 0) break;
            
            int randomIndex = rand.Next(0, PossibleEnemies.Count);
            if (PossibleEnemies[randomIndex] != null)
            {
                data.EnemyIds.Add(PossibleEnemies[randomIndex].EnemyId);
            }
        }
        
        return data;
    }
}