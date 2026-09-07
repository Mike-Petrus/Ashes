using System.Collections.Generic;
using UnityEngine;

public class TargetHighlightVFXController : MonoBehaviour
{
    private BattleSimulation simulation;
    private UIThemeSO theme;
    
    // Quick lookup map: ActorId -> ActorView
    private Dictionary<ActorId, ActorView> viewMap = new Dictionary<ActorId, ActorView>();
    
    // Memory to cleanly turn off outlines when the cursor moves off an actor
    private HashSet<ActorId> currentlyHighlightedIds = new HashSet<ActorId>();

    public void Initialize(BattleSimulation sim, UIThemeSO globalTheme)
    {
        simulation = sim;
        theme = globalTheme;

        // We clear this here. We will let the Lazy-Loader populate this dynamically!
        viewMap.Clear();
        currentlyHighlightedIds.Clear();

        // Subscribe to the semantic visualization event
        simulation.Events.Subscribe<TargetingImpactsChangedEvent>(OnImpactsChanged);
    }

    private void OnImpactsChanged(TargetingImpactsChangedEvent e)
    {
        // 1. Identify which actors *should* be highlighted this frame
        HashSet<ActorId> newImpacts = new HashSet<ActorId>();
        if (e.ImpactedActors != null)
        {
            foreach (var impact in e.ImpactedActors)
            {
                newImpacts.Add(impact.ActorId);
            }
        }

        // 2. Clear highlights for actors that were highlighted LAST frame, 
        // but are NO LONGER in the blast zone THIS frame.
        foreach (var oldId in currentlyHighlightedIds)
        {
            if (!newImpacts.Contains(oldId) && TryGetActorView(oldId, out ActorView oldView))
            {
                oldView.ClearHighlight();
            }
        }
        
        // Reset our memory
        currentlyHighlightedIds.Clear();

        // 3. Apply the correct outline color to all actors currently in the blast zone
        if (e.ImpactedActors != null && e.ImpactedActors.Count > 0)
        {
            foreach (var impact in e.ImpactedActors)
            {
                if (TryGetActorView(impact.ActorId, out ActorView view))
                {
                    Color highlightColor = GetThemeColorForOutcome(impact.VisualColorOutcome);
                    
                    // We only apply if it's not the invisible failsafe
                    if (highlightColor != Color.clear)
                    {
                        view.ApplyHighlight(highlightColor);
                        currentlyHighlightedIds.Add(impact.ActorId);
                    }
                }
                else
                {
                    Debug.LogWarning($"[VFX Controller] Could not find ActorView in scene for ID: {impact.ActorId}");
                }
            }
        }
    }

    // --- Auto-Discovery ---
    private bool TryGetActorView(ActorId id, out ActorView view)
    {
        // 1. Check if we already know about it and it hasn't been destroyed by Unity
        if (viewMap.TryGetValue(id, out view) && view != null)
        {
            return true;
        }

        // 2. If missing, sweep the scene. (This happens exactly once per newly spawned actor!)
        ActorView[] allViews = FindObjectsByType<ActorView>(FindObjectsSortMode.None);
        foreach (var v in allViews)
        {
            // Only map it if it has actually been initialized with an ID by the bootstrapper
            if (v.ActorId.Equals(id)) 
            {
                viewMap[id] = v;
                view = v;
                return true;
            }
        }

        view = null;
        return false;
    }

    // Maps the Engine's semantic outcome to the Designer's chosen UI colors
    private Color GetThemeColorForOutcome(OutcomeColor outcome)
    {
        if (theme == null) return Color.magenta;

        switch (outcome)
        {
            case OutcomeColor.IntendedHarm: 
                return theme.HighlightIntendedFoe;
            case OutcomeColor.IntendedHelp: 
                return theme.HighlightIntendedFriend;
            case OutcomeColor.UnintendedHarm: 
                return theme.HighlightUnintendedHarm;
            case OutcomeColor.UnintendedHelp: 
                return theme.HighlightUnintendedHelp;
            default: 
                return Color.clear;
        }
    }

    private void OnDestroy()
    {
        if (simulation != null && simulation.Events != null)
        {
            simulation.Events.Unsubscribe<TargetingImpactsChangedEvent>(OnImpactsChanged);
        }
    }
}