using System;
using System.Collections.Generic;

public class BattleEventBus
{
    // Maps an Event Type -> List of listeners
    private Dictionary<Type, List<Action<IBattleEvent>>> listeners = new();
    private Dictionary<Delegate, Action<IBattleEvent>> callbackLookup = new();

    private Queue<IBattleEvent> currentQueue = new();
    private Queue<IBattleEvent> nextQueue = new();

    // Deffered modification tracking
    private bool isProcessing = false;
    private List<PendingModifier> pendingModifiers = new(); //??

    private struct PendingModifier
    {
        public Type EventType;
        public Action<IBattleEvent> Wrapper;
        public Delegate OriginalCallback;
        public bool IsAdd;
    }

    // Registers a listener for an Event Type
    // Generic <T> = Event Type e.g. BattleTickEvent
    public void Subscribe<T>(Action<T> callback) where T : IBattleEvent // why?
    {
        var type = typeof(T);

        // Convert a typed callback into an Action<IBattleEvent>
        // e.g. callback = void OnTick(BattleTickEvent tick)
        // but dictionary expects Action<IBattleEvent>, so wrap it: e => callback((T)e)
        Action<IBattleEvent> wrapper = (e) => callback((T)e);

        if (isProcessing)
        {
            pendingModifiers.Add(new PendingModifier { EventType = type, Wrapper = wrapper, OriginalCallback = callback, IsAdd = true });
            return;
        }

        ApplySubscribe(type, wrapper, callback);
    }

    public void Unsubscribe<T>(Action<T> callback) where T : IBattleEvent
    {
        var type = typeof(T);

        if (!callbackLookup.TryGetValue(callback, out var wrapper))
        {
            return;
        }
        
        if (isProcessing)
        {
            pendingModifiers.Add(new PendingModifier { EventType = type, Wrapper = wrapper, OriginalCallback = callback, IsAdd = false });
            return;
        }

        ApplyUnsubscribe(type, wrapper, callback);
    }

    // Publishing sends an event e.g. eventBus.Publish(new BattleTickEvent(deltaTime));
    public void Publish<T>(T eventData) where T : IBattleEvent
    {
        nextQueue.Enqueue(eventData);
    }

    public void ProcessEvents()
    {
        // Swap queues each pass
        // Loop continuously until both queues are drained
        // Ensures cascading events resolve in the same frame

        while (currentQueue.Count > 0 || nextQueue.Count > 0)
        {
            var temp = currentQueue;
            currentQueue = nextQueue;
            nextQueue = temp;

            isProcessing = true;

            while (currentQueue.Count > 0)
            {
                var eventData = currentQueue.Dequeue();
                var type = eventData.GetType();

                if (listeners.TryGetValue(type, out var eventListeners))
                {
                    // No snapshot. Just iterate the existing list
                    foreach (var listener in eventListeners)
                    {
                        listener(eventData);
                    }
                }
            }
        }

        isProcessing = false;

        // Apply any deferred sub/unsub requests before the next cascade loop
        ProcessPendingModifiers();
    }

    private void ProcessPendingModifiers()
    {
        if (pendingModifiers.Count == 0)
        {
            return;
        }

        foreach (var mod in pendingModifiers)
        {
            if (mod.IsAdd)
            {
                ApplySubscribe(mod.EventType, mod.Wrapper, mod.OriginalCallback);
            }
            else
            {
                ApplyUnsubscribe(mod.EventType, mod.Wrapper, mod.OriginalCallback);
            }
        }
        pendingModifiers.Clear();
    }

    private void ApplySubscribe(Type type, Action<IBattleEvent> wrapper, Delegate original)
    {
        if (!listeners.ContainsKey(type))
        {
            listeners[type] = new List<Action<IBattleEvent>>();
        }

        callbackLookup[original] = wrapper;
        listeners[type].Add(wrapper);
    }

    private void ApplyUnsubscribe(Type type, Action<IBattleEvent> wrapper, Delegate original)
    {
        if (listeners.TryGetValue(type, out var list))
        {
            list.Remove(wrapper);
        }

        callbackLookup.Remove(original);
    }
}
