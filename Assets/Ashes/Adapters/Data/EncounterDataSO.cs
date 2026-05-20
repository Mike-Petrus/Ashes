using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEncounter", menuName = "Ashes/Encounter Data")]
public class EncounterDataSO : ScriptableObject
{
    [Tooltip("List of enemy IDs or Prefab names to spawn in the Enemy Party")]
    public List<string> EnemyIds = new();

    // TODO: 
    // public EncounterType Type; (e.g. Standard, Ambush, BackAttack)
    // public AudioClip BattleMusic;

    // Converts Unity data to pure C#
    public EncounterData ToCoreData()
    {
        return new EncounterData
        {
            EnemyIds = new List<string>(this.EnemyIds)
        };
    }
}