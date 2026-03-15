using System;
using System.Collections.Generic;

public class EventBus
{
    // Maps an Event Type -> List of listeners
    private Dictionary<Type, List<Action<object>>> listeners = new();

    private Dictionary<Delegate, Action<object>> callbackLookup = new();

    // Registers a listener for an Event Type
    // Generic <T> = Event Type e.g. BattleTickEvent
    public void Subscribe<T>(Action<T> callback)
    {
        var type = typeof(T);

        // If no listeners for Event Type exist, create the empty list
        // e.g. BattleTickEvent -> []
        if (!listeners.ContainsKey(type))
        {
            listeners[type] = new List<Action<object>>();
        }

        // Convert a typed callback into an Action<object>
        // e.g. callback = void OnTick(BattleTickEvent tick)
        // but dictionary expects Action<object>, so wrap it: e => callback((T)e)
        Action<object> wrapper = (e) => callback((T)e);
        callbackLookup[callback] = wrapper;

        listeners[type].Add(wrapper);
    }

    public void Unsubscribe<T>(Action<T> callback)
    {
        var type = typeof(T);

        if (!callbackLookup.TryGetValue(callback, out var wrapper))
            return;

        if (listeners.TryGetValue(type, out var list))
        {
            list.Remove(wrapper);
        }

        callbackLookup.Remove(callback);
    }

    // Publishing sends an event e.g. eventBus.Publish(new BattleTickEvent(deltaTime));
    public void Publish<T>(T eventData)
    {
        var type = typeof(T);

        if (!listeners.TryGetValue(type, out var eventListeners))
        {
            return;
        }

        // Create snapshot to avoid modification during iteration
        // We can replace this with Queued Event System, but for now it works
        var snapshot = new List<Action<object>>(eventListeners);

        foreach (var listener in snapshot)
        {
            listener(eventData);
        }
    }

    // TODO: consider Queued Event System
    // prevents bugs when events trigger other events in the same frame
}
