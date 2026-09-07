using UnityEngine;
using System.Collections.Generic;

public class BattleWorldUIManager : MonoBehaviour
{
    [Header("Global UI Theme")]
    [Tooltip("Drag your UIThemeSO here. All child panels will pull colors from this!")]
    public UIThemeSO CurrentTheme;

    [Header("World Space Sub-Controllers")]
    public FloatingTextUIController FloatingTextUI;

    [Tooltip("Drag your UnitStatusPanel prefab here")]
    public UnitStatusPanelController unitStatusPanelControllerPrefab;

    private BattleSimulation simulation;
    private Camera mainCamera;
    
    // Dictionary to keep track of spawned panels
    private Dictionary<ActorId, UnitStatusPanelController> activePanels = new Dictionary<ActorId, UnitStatusPanelController>();

    public void Initialize(BattleSimulation simulation, Camera camera)
    {
        this.simulation = simulation;
        this.mainCamera = camera; // Pass the camera so panels know what to look

        if (CurrentTheme == null)
        {
            Debug.LogError("[BattleWorldUIManager] No UI Theme assigned! UI will break.");
            return;
        }

        if (FloatingTextUI != null) FloatingTextUI.Initialize(simulation);

        // Subscribe to the registration event
        this.simulation.Events.Subscribe<ActorRegisteredEvent>(OnActorRegistered);

        // For highlight conditions
        this.simulation.Events.Subscribe<TargetingImpactsChangedEvent>(OnTargetingImpactsChanged);
        
        // Optional: Subscribe to ActorDied/Removed event to clean up panels later
        // this.simulation.Events.Subscribe<ActorDiedEvent>(OnActorDied);
    }

    private void OnActorRegistered(ActorRegisteredEvent e)
    {
        if (unitStatusPanelControllerPrefab == null) return;

        // Instantiate the prefab as a child of this Manager
        UnitStatusPanelController newPanel = Instantiate(unitStatusPanelControllerPrefab, this.transform);
        
        // Name it cleanly for the hierarchy
        newPanel.name = $"StatusPanel_{e.Actor.Name}";

        // Initialize it with the data it needs to survive
        newPanel.Initialize(e.Actor, CurrentTheme, mainCamera);

        // Track it
        activePanels[e.Actor.Id] = newPanel;
    }

    // --- EXTERNAL OVERRIDES ---

    // 1. Called by BattleInputManager when L2 is held down or released
    public void ToggleAllInfoPanels(bool isHeld)
    {
        foreach (var panel in activePanels.Values)
        {
            panel.SetInfoToggleState(isHeld);
        }
    }

    // 2. Called by the Event Bus when the player moves the targeting cursor
    private void OnTargetingImpactsChanged(TargetingImpactsChangedEvent e)
    {
        // First, clear the targeted state on ALL panels
        foreach (var panel in activePanels.Values)
        {
            panel.SetTargetedState(false);
        }

        // If the event passes null or empty, we are done.
        if (e.ImpactedActors == null) return;

        // Otherwise, turn the targeted state ON for affected actors
        foreach (var target in e.ImpactedActors)
        {
            // Assuming your event passes the targeted ActorId!
            if (activePanels.TryGetValue(target.ActorId, out var panel)) 
            {
                panel.SetTargetedState(true);
            }
        }
    }

    private void OnDestroy()
    {
        if (simulation != null && simulation.Events != null)
        {
            simulation.Events.Unsubscribe<ActorRegisteredEvent>(OnActorRegistered);
            simulation.Events.Unsubscribe<TargetingImpactsChangedEvent>(OnTargetingImpactsChanged);
        }
    }
}