using System;
using System.Collections.Generic;

public class EncounterSpawner
{
    // TODO: Add different party formations and adjust based on that
    private readonly int[] formationOffsets = { 0, -1, 1, -2, 2 };
    private readonly float partySpacing = 1.5f;

    public void SetupEncounter(SimVector3 playerPos, SimVector3 playerDir, PartyManager party, IEnemyDatabase enemyDatabase, EncounterData encounter, BattleSimulation simulation, IMapValidator mapValidator)
    {
        int totalActors = party.ActiveRoster.Count + encounter.EnemyIds.Count;

        // 1. Determine raw collision center
        SimVector3 enemyCollisionPos = playerPos + (playerDir * 2f);
        SimVector3 rawCenter = new SimVector3(
            (playerPos.x + enemyCollisionPos.x) / 2f,
            (playerPos.y + enemyCollisionPos.y) / 2f,
            (playerPos.z + enemyCollisionPos.z) / 2f
        );

        // 2. Create the Arena
        BattleArena arena = new BattleArena(rawCenter, playerDir, totalActors, mapValidator);
        SimVector3 partyBaseLine = arena.GetPartyBaseLine();

        // Actor Ids: Reserve 1-5 for party, 6+ for enemies
        int nextActorId = 1;

        // 3. Spawn the party
        var roster = party.ActiveRoster;

        for (int i = 0; i < roster.Count; i++)
        {
            if (i >= formationOffsets.Length)
            {
                break;
            }

            SimVector3 targetSpawn = partyBaseLine +  (arena.DivisionAxis * formationOffsets[i] * partySpacing);
            targetSpawn = mapValidator.GetNearestValidPosition(targetSpawn, 4f);

            var actor = new BattleActor(new ActorId(nextActorId), roster[i].CharacterName, roster[i].BaseStats, targetSpawn, 1.0f);

            actor.Stats.CurrentHP = roster[i].CurrentHP;
            actor.Stats.CurrentMP = roster[i].CurrentMP;

            simulation.Actors.RegisterActor(actor);
            nextActorId++;
        }

        nextActorId = 6;

        // Pre-calculate enemy duplicates
        Dictionary<string, int> enemyTotals = new();
        foreach(var eId in encounter.EnemyIds)
        {
            if (!enemyTotals.ContainsKey(eId))
            {
                enemyTotals[eId] = 0;
            }

            enemyTotals[eId]++;
        }

        // 4. Spawn the Enemies
        Dictionary<string, int> enemySpawnCounts = new();
        Random rand = new Random();

        foreach (var enemyId in encounter.EnemyIds)
        {
            // Lookup true stats from database
            EnemyTemplate template = enemyDatabase.GetEnemy(enemyId);

            // Naming Logic
            if (!enemySpawnCounts.ContainsKey(enemyId))
            {
                enemySpawnCounts[enemyId] = 0;
            }

            int currentCount = enemySpawnCounts[enemyId]++;

            string actorName = template.DefaultName;
            if (enemyTotals[enemyId] > 1)
            {
                char suffix = (char)('A' + currentCount);
                actorName = $"{template.DefaultName} {suffix}";
            }

            // Spawn Math
            float randomForward = (float)rand.NextDouble() * (arena.Radius - 3f) + 1f;
            float randomSide = (float)(rand.NextDouble() * 2.0 - 1.0) * (arena.Radius - 3f);
            
            SimVector3 randomSpawn = arena.Center + (arena.PlayerFacingDir * randomForward) + (arena.DivisionAxis * randomSide);
            randomSpawn = mapValidator.GetNearestValidPosition(randomSpawn, 4f);

            // Create unique CharacterStats using the template's CoreAttributes
            CharacterStats freshStats = new CharacterStats(template.BaseAttributes);

            var enemyActor = new BattleActor(new ActorId(nextActorId), actorName, freshStats, randomSpawn, template.Radius, ActorFaction.Enemy);

            simulation.Actors.RegisterActor(enemyActor);
            nextActorId++;
        }

        // 5. Hand the finalized Arnea to the Simulation
        simulation.InitializeBattle(arena);
    }
}