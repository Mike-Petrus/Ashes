using System.Collections.Generic;

public class ActorRegistry
{
    private readonly Dictionary<ActorId, BattleActor> actors = new();
    private BattleEventBus events;

    public int Count => actors.Count;

    public IEnumerable<BattleActor> Actors => actors.Values;

    // TODO: publish events ActorRegisteredEvent, ActorRemoved event
    // For systems like targeting and UI when Actor dies or new Actor joins battle

    public ActorRegistry(BattleEventBus eventBus)
    {
        events = eventBus;
    }

    public void RegisterActor(BattleActor actor)
    {
        if (actors.ContainsKey(actor.Id))
        {
            throw new System.Exception($"Actor with ID {actor.Id} already registered.");
        }
        actors[actor.Id] = actor;

        // Broadcast that a new actor has entered the battle!
        events.Publish(new ActorRegisteredEvent(actor));
    }

    public void RemoveActor(ActorId id)
    {
        if (actors.ContainsKey(id))
        {
            actors.Remove(id);
            
            // Broadcast that an actor was completely removed (e.g. escaped, banished)
            events.Publish(new ActorRemovedEvent(id));
        }
    }

    public BattleActor GetActor(ActorId id)
    {
        if (!actors.TryGetValue(id, out var actor))
        {
            throw new System.Exception($"Actor {id} not found");
        }
        return actor;
    }

    // Safe lookup that avoids exceptions
    public bool TryGetActor(ActorId id, out BattleActor actor)
    {
        return actors.TryGetValue(id, out actor);
    }

    public IEnumerable<BattleActor> GetAllActors()
    {
        return actors.Values;
    }

    public IEnumerable<ActorId> GetAllActorIds()
    {
        return actors.Keys;
    }

    public IEnumerable<BattleActor> GetAliveActors()
    {
        foreach (var actor in actors.Values)
        {
            if (actor.IsAlive)
                yield return actor;
        }
    }

    public IEnumerable<ActorId> GetAliveActorIds()
    {
        foreach (var pair in actors)
        {
            if (pair.Value.IsAlive)
                yield return pair.Key;
        }
    }

    public bool Contains(ActorId id)
    {
        return actors.ContainsKey(id);
    }
}
