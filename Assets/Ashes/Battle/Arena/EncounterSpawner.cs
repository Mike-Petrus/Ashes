using System;
using System.Collections.Generic;

public class EncounterSpawner
{
    private readonly int[] formationOffsets = { 0, -1, 1, -2, 2 };
    private readonly float partySpacing = 1.5f;

    // ADDED: IAbilityDatabase abilityDatabase so we can load enemy spells!
    public void SetupEncounter(SimVector3 playerPos, SimVector3 playerDir, PartyManager party, IEnemyDatabase enemyDatabase, IAbilityDatabase abilityDatabase, EncounterData encounter, BattleSimulation simulation, IMapValidator mapValidator)
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

        int nextActorId = 1;

        // 3. Spawn the party
        var roster = party.ActiveRoster;
        for (int i = 0; i < roster.Count; i++)
        {
            if (i >= formationOffsets.Length) break;

            SimVector3 targetSpawn = partyBaseLine +  (arena.DivisionAxis * formationOffsets[i] * partySpacing);
            targetSpawn = mapValidator.GetNearestValidPosition(targetSpawn, 4f);

            var actor = new BattleActor(new ActorId(nextActorId), roster[i].CharacterName, roster[i].BaseStats, targetSpawn, 1.0f, ActorFaction.Party);

            // Everyone gets attack
            actor.Abilities.UnlockAbility(new BasicAttackAbility());

            foreach (string abilityId in roster[i].UnlockedAbilities)
            {
                var abilityTemplate = abilityDatabase.GetAbility(abilityId);

                if (abilityTemplate != null)
                {
                    actor.Abilities.UnlockAbility(new DataDrivenAbility(abilityTemplate));                    
                }
            }

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
            if (!enemyTotals.ContainsKey(eId)) enemyTotals[eId] = 0;
            enemyTotals[eId]++;
        }

        // 4. Spawn the Enemies
        Dictionary<string, int> enemySpawnCounts = new();
        Random rand = new Random();
        float arenaRadius = arena.Radius;

        foreach (var enemyId in encounter.EnemyIds)
        {
            EnemyTemplate template = enemyDatabase.GetEnemy(enemyId);

            // Naming Logic
            if (!enemySpawnCounts.ContainsKey(enemyId)) enemySpawnCounts[enemyId] = 0;
            int currentCount = enemySpawnCounts[enemyId]++;

            string actorName = template.DefaultName;
            if (enemyTotals[enemyId] > 1)
            {
                char suffix = (char)('A' + currentCount);
                actorName = $"{template.DefaultName} {suffix}";
            }

            // PERFECT SEMI-CIRCLE MATH
            // Offset exactly 1.0f from the Division Axis to maintain visual symmetry!
            float padding = 1.0f;
            float randomForward = (float)rand.NextDouble() * (arenaRadius - (padding * 2)) + padding;
            float maxSide = (float)Math.Sqrt(Math.Pow(arenaRadius - padding, 2) - Math.Pow(randomForward, 2));
            float randomSide = (float)(rand.NextDouble() * 2.0 - 1.0) * maxSide;
            
            SimVector3 randomSpawn = arena.Center + (arena.PlayerFacingDir * randomForward) + (arena.DivisionAxis * randomSide);
            randomSpawn = mapValidator.GetNearestValidPosition(randomSpawn, 4f);

            CharacterStats freshStats = new CharacterStats(template.BaseAttributes);
            var enemyActor = new BattleActor(new ActorId(nextActorId), actorName, freshStats, randomSpawn, template.Radius, ActorFaction.Enemy);

            // ALWAYS give enemies a basic attack fallback
            enemyActor.Abilities.UnlockAbility(new BasicAttackAbility());

            // NEW: Read the template and load their specific abilities!
            foreach(var abilityId in template.Abilities)
            {
                var abilityTemplate = abilityDatabase.GetAbility(abilityId);
                if (abilityTemplate != null)
                {
                    enemyActor.Abilities.UnlockAbility(new DataDrivenAbility(abilityTemplate));
                }
            }

            simulation.Actors.RegisterActor(enemyActor);
            nextActorId++;
        }

        // 5. Hand the finalized Arena to the Simulation
        simulation.InitializeBattle(arena);
    }
}